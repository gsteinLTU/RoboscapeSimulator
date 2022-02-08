using System.Numerics;
using BepuPhysics;
using BepuUtilities.Memory;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
namespace RoboScapeSimulator
{
    public class SimulationInstance : IDisposable
    {
        /// <summary>
        /// References to static bodies in the scene
        /// </summary>
        internal Dictionary<string, StaticReference> NamedStatics = new();

        /// <summary>
        /// References to moving bodies in the scene
        /// </summary>
        internal Dictionary<string, BodyReference> NamedBodies = new();

        internal List<Entity> Entities = new();

        internal IEnumerable<Robot> Robots => Entities.Where(e => e is Robot).Cast<Robot>();

        public Simulation Simulation;

        /// <summary>
        /// Gets the buffer pool used by the demo's simulation.
        /// </summary>
        public BufferPool BufferPool { get; private set; }

        internal CollidableProperty<BodyCollisionProperties> Properties = new();

        /// <summary>
        /// Time elapsed in this simulation
        /// </summary>
        public float Time = 0;

        public SimulationInstance()
        {
            Properties = new CollidableProperty<BodyCollisionProperties>();
            BufferPool = new BufferPool();
            Simulation = Simulation.Create(BufferPool, new SimulationInstanceCallbacks() { Properties = Properties }, new SimulationInstanceIntegratorCallbacks(new Vector3(0, -10, 0)), new PositionFirstTimestepper());
        }

        bool disposed;
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Simulation.Dispose();
                BufferPool.Clear();
            }
        }

        /// <summary>
        /// Resets all resettable objects in the environment
        /// </summary>
        public void Reset()
        {
            ((List<IResettable>)Entities.Where(e => e is IResettable)).ForEach(e => e.Reset());
        }

        /// <summary>
        /// Update the simulation
        /// </summary>
        /// <param name="dt">Delta time in s</param>
        public void Update(float dt)
        {
            Time += dt;
            Simulation.Timestep(dt);
            foreach (var entity in Entities)
            {
                entity.Update(dt);
            }
        }

        /// <summary>
        /// Get a Dictionary of bodies in the simulation
        /// </summary>
        /// <param name="onlyAwake">Should only dynamic, non-sleeping objects be returned?</param>
        /// <returns>Dictionary with entity name as key, BodyInfo as value</returns>
        public Dictionary<string, object> GetBodies(bool onlyAwake = false)
        {
            var output = new Dictionary<string, object>();

            foreach (var entity in Entities)
            {
                if (!onlyAwake && entity is StaticEntity staticEntity)
                {
                    output.Add(entity.Name, new BodyInfo
                    {
                        label = entity.Name,
                        pos = {
                        x = staticEntity.StaticReference.Pose.Position.X,
                        y = staticEntity.StaticReference.Pose.Position.Y,
                        z = staticEntity.StaticReference.Pose.Position.Z
                    },
                        angle = staticEntity.StaticReference.Pose.Orientation,
                        width = staticEntity.StaticReference.BoundingBox.Max.X - staticEntity.StaticReference.BoundingBox.Min.X,
                        height = staticEntity.StaticReference.BoundingBox.Max.Y - staticEntity.StaticReference.BoundingBox.Min.Y,
                        depth = staticEntity.StaticReference.BoundingBox.Max.Z - staticEntity.StaticReference.BoundingBox.Min.Z,
                        visualInfo = staticEntity.VisualInfo
                    });
                }
                else if (entity is DynamicEntity dynamicEntity && (!onlyAwake || dynamicEntity.BodyReference.Awake))
                {
                    output.Add(entity.Name, new BodyInfo
                    {
                        label = entity.Name,
                        pos = {
                        x = dynamicEntity.BodyReference.Pose.Position.X,
                        y = dynamicEntity.BodyReference.Pose.Position.Y,
                        z = dynamicEntity.BodyReference.Pose.Position.Z
                    },
                        angle = dynamicEntity.BodyReference.Pose.Orientation,
                        width = dynamicEntity.Width,
                        height = dynamicEntity.Height,
                        depth = dynamicEntity.Depth,
                        visualInfo = entity.VisualInfo,
                        vel = dynamicEntity.BodyReference.Velocity.Linear
                    });
                }
            }

            return output;
        }

    }

    [Serializable]
    public struct BodyInfo
    {
        public string label;
        public Vec3 pos;
        public Vec3 vel;
        public Quaternion angle;
        public float anglevel;
        public float width;
        public float height;
        public float depth;
        public VisualInfo? visualInfo;
    }

    [Serializable]
    public struct Vec3
    {
        public static implicit operator Vec3(Vector3 vector3) => new Vec3() { x = vector3.X, y = vector3.Y, z = vector3.Z };
        public float x;
        public float y;
        public float z;
    }
}