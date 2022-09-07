using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using RoboScapeSimulator.Entities;
namespace RoboScapeSimulator.Physics.Bepu
{
    public class BepuSimulationInstance : SimulationInstance
    {
        /// <summary>
        /// References to static bodies in the scene
        /// </summary>
        public Dictionary<string, StaticReference> NamedStatics = new();

        /// <summary>
        /// References to moving bodies in the scene
        /// </summary>
        public Dictionary<string, BodyReference> NamedBodies = new();
        
        public Simulation Simulation;

        /// <summary>
        /// Gets the buffer pool used by the demo's simulation.
        /// </summary>
        public BufferPool BufferPool { get; private set; }

        internal CollidableProperty<BodyCollisionProperties> Properties = new();

        public SimulationInstanceIntegratorCallbacks IntegratorCallbacks;

        public BepuSimulationInstance()
        {
            Properties = new CollidableProperty<BodyCollisionProperties>();
            BufferPool = new BufferPool();
            IntegratorCallbacks = new SimulationInstanceIntegratorCallbacks(new Vector3(0, -9.81f, 0));
            Simulation = Simulation.Create(BufferPool, new BepuSimulationInstanceCallbacks(this, Properties), IntegratorCallbacks, new SolveDescription(3, 10), new DefaultTimestepper());
        }

        bool disposed;
        public override void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Simulation.Dispose();
                BufferPool.Clear();
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Update the simulation
        /// </summary>
        /// <param name="dt">Delta time in s</param>
        public override void Update(float dt)
        {
            if (dt <= 0)
                return;
            Simulation.Timestep(dt);
            base.Update(dt);
        }

        /// <summary>
        /// Get a Dictionary of bodies in the simulation
        /// </summary>
        /// <param name="onlyAwake">Should only dynamic, non-sleeping objects be returned?</param>
        /// <returns>Dictionary with entity name as key, BodyInfo as value</returns>
        public override Dictionary<string, object> GetBodies(bool onlyAwake = false, bool allData = true)
        {
            var output = new Dictionary<string, object>();

            foreach (var entity in Entities)
            {
                if (!onlyAwake && entity is StaticEntity)
                {
                    output.Add(entity.Name, entity.GetBodyInfo(allData));
                }
                else if (entity is DynamicEntity dynamicEntity && (!onlyAwake || dynamicEntity.BodyReference.Awake))
                {
                    output.Add(entity.Name, entity.GetBodyInfo(allData));
                }
                else if (allData || entity.ShouldUpdate)
                {
                    output.Add(entity.Name, entity.GetBodyInfo(allData));
                }
            }

            return output;
        }

        public override SimBody CreateBox(string name, Vector3 position, Quaternion? orientation = null, float width = 1, float height = 1, float depth = 1, float mass = 1, bool isKinematic = false)
        { 
            var box = new Box(width, height, depth);
            var boxInertia = box.ComputeInertia(mass);


            BodyHandle bodyHandle;
            RigidPose pose = new(position, orientation ?? Quaternion.Identity);

            if (isKinematic)
            {
                bodyHandle = Simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, new CollidableDescription(Simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(0.01f)));
            }
            else
            {
                bodyHandle = Simulation.Bodies.Add(BodyDescription.CreateDynamic(pose, boxInertia, new CollidableDescription(Simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(0)));
            }

            SimBodyBepu simBody = new SimBodyBepu();
            simBody.BodyReference = Simulation.Bodies.GetBodyReference(bodyHandle);

            ref var bodyProperties = ref Properties.Allocate(simBody.BodyReference.Handle);
            bodyProperties = new BodyCollisionProperties { Friction = 1f, Filter = new SubgroupCollisionFilter(simBody.BodyReference.Handle.Value, 0) };
 
            NamedBodies.Add(name, simBody.BodyReference);

            return simBody;
        }

        public override SimStatic CreateStaticBox(string name, Vector3 position, Quaternion? orientation = null, float width = 100, float height = 100, float depth = 1)
        {
            var groundHandle = Simulation.Statics.Add(new StaticDescription(position, Simulation.Shapes.Add(new Box(width, height, depth))));
            
            SimStaticBepu simStatic = new SimStaticBepu();
            simStatic.StaticReference = Simulation.Statics.GetStaticReference(groundHandle);

            NamedStatics.Add(name, simStatic.StaticReference);

            return simStatic;
        }
    }
  
    public class SimBodyBepu : SimBody {
        public BodyReference BodyReference;

        public override Vector3 Position { get => BodyReference.Pose.Position; set 
            {
                BodyReference.Pose.Position = value;
                BodyReference.Awake = true;
            }
        }
        public override Quaternion Orientation { get => BodyReference.Pose.Orientation;set 
            {
                BodyReference.Pose.Orientation = value;
                BodyReference.Awake = true;
            }
        }

        public override Vector3 LinearVelocity { get => BodyReference.Velocity.Linear; set => BodyReference.Velocity.Linear = value; }
        public override Vector3 AngularVelocity { get => BodyReference.Velocity.Angular; set => BodyReference.Velocity.Angular = value;  }
        public override bool Awake {  get => BodyReference.Awake; set => BodyReference.Awake = value; }

        public override float Mass => (1.0f / BodyReference.LocalInertia.InverseMass);

        public override void ApplyForce(Vector3 force)
        {
            BodyReference.ApplyLinearImpulse(force);
        }
    }

    public class SimStaticBepu : SimStatic {
        public StaticReference StaticReference;

        public override Vector3 Position { get => StaticReference.Pose.Position; set => StaticReference.Pose.Position = value; }
        public override Quaternion Orientation { get => StaticReference.Pose.Orientation; set => StaticReference.Pose.Orientation = value; }

        public override Vector3 Size => StaticReference.BoundingBox.Max - StaticReference.BoundingBox.Min;
    }
}