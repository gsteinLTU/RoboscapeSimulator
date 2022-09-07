using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
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
            Simulation = Simulation.Create(BufferPool, new SimulationInstanceCallbacks(this, Properties), IntegratorCallbacks, new SolveDescription(3, 10), new DefaultTimestepper());
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

    }
}