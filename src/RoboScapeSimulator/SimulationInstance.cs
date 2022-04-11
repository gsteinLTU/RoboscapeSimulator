using System.Diagnostics;
using System.Numerics;
using System.Text;
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
        public Dictionary<string, StaticReference> NamedStatics = new();

        /// <summary>
        /// References to moving bodies in the scene
        /// </summary>
        public Dictionary<string, BodyReference> NamedBodies = new();

        public List<Entity> Entities = new();

        public IEnumerable<Robot> Robots => Entities.Where(e => e is Robot).Cast<Robot>();

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
            Simulation = Simulation.Create(BufferPool, new SimulationInstanceCallbacks(this, Properties), new SimulationInstanceIntegratorCallbacks(new Vector3(0, -10, 0)), new SolveDescription(8, 2), new DefaultTimestepper());
        }

        bool disposed;
        public void Dispose()
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
                try
                {
                    entity.Update(dt);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Get a Dictionary of bodies in the simulation
        /// </summary>
        /// <param name="onlyAwake">Should only dynamic, non-sleeping objects be returned?</param>
        /// <returns>Dictionary with entity name as key, BodyInfo as value</returns>
        public Dictionary<string, object> GetBodies(bool onlyAwake = false, bool allData = true)
        {
            var output = new Dictionary<string, object>();

            foreach (var entity in Entities)
            {
                if (!onlyAwake && entity is StaticEntity staticEntity)
                {
                    output.Add(entity.Name, entity.GetBodyInfo(allData));
                }
                else if (entity is DynamicEntity dynamicEntity && (!onlyAwake || dynamicEntity.BodyReference.Awake))
                {
                    output.Add(entity.Name, entity.GetBodyInfo(allData));
                }
                else if (allData)
                {
                    output.Add(entity.Name, entity.GetBodyInfo(allData));
                }
            }

            return output;
        }

    }

    [Serializable]
    public struct BodyInfo
    {
        public string? label;
        public Vec3 pos;
        public Vec3 vel;
        public Quaternion angle;
        public float? anglevel;
        public float? width;
        public float? height;
        public float? depth;
        public VisualInfo? visualInfo;

        public override string ToString()
        {
            StringBuilder builder = new();

            builder.AppendLine("BodyInfo:");
            builder.AppendLine("\tlabel:\t" + label ?? "null");
            builder.AppendLine("\tpos:\t" + pos ?? "null");
            builder.AppendLine("\tvel:\t" + vel ?? "null");
            builder.AppendLine("\tangle:\t" + angle ?? "null");
            builder.AppendLine("\tanglevel:\t" + anglevel ?? "null");
            builder.AppendLine("\twidth:\t" + width ?? "null");
            builder.AppendLine("\theight:\t" + height ?? "null");
            builder.AppendLine("\tdepth:\t" + depth ?? "null");
            builder.AppendLine("\tvisualInfo:\t" + visualInfo ?? "null");

            return builder.ToString();
        }
    }

    [Serializable]
    public struct Vec3
    {
        public static implicit operator Vec3(Vector3 vector3) => new() { x = vector3.X, y = vector3.Y, z = vector3.Z };
        public float x;
        public float y;
        public float z;
    }
}