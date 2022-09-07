using System.Diagnostics;
using System.Numerics;
using System.Text;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
namespace RoboScapeSimulator
{
    public abstract class SimulationInstance : IDisposable
    {
        public List<Entity> Entities = new();

        public IEnumerable<Robot> Robots => Entities.Where(e => e is Robot).Cast<Robot>();

        public IEnumerable<Trigger> Triggers => Entities.Where(e => e is Trigger).Cast<Trigger>();

        /// <summary>
        /// Time elapsed in this simulation
        /// </summary>
        public float Time = 0;

        public SimulationInstance() { }

        bool disposed;
        public virtual void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }


        /// <summary>
        /// Resets all resettable objects in the environment
        /// </summary>
        public virtual void Reset()
        {
            ((List<IResettable>)Entities.Where(e => e is IResettable)).ForEach(e => e.Reset());
        }

        /// <summary>
        /// Update the simulation
        /// </summary>
        /// <param name="dt">Delta time in s</param>
        public virtual void Update(float dt)
        {
            if (dt <= 0)
                return;
            Time += dt;
            foreach (var entity in Entities.ToList())
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

        public abstract SimBody CreateBox(string name, Vector3 position, Quaternion? orientation = null, float width = 1, float height = 1, float depth = 1, float mass = 1, bool isKinematic = false);
        public abstract SimStatic CreateStaticBox(string name, Vector3 position, Quaternion? orientation = null, float width = 100, float height = 100, float depth = 1);

        /// <summary>
        /// Get a Dictionary of bodies in the simulation
        /// </summary>
        /// <param name="onlyAwake">Should only dynamic, non-sleeping objects be returned?</param>
        /// <returns>Dictionary with entity name as key, BodyInfo as value</returns>
        public virtual Dictionary<string, object> GetBodies(bool onlyAwake = false, bool allData = true)
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

    [Serializable]
    public struct BodyInfo
    {
        public string? label;
        public Vec3 pos;
        public Vec3? vel;
        public Quaternion angle;
        public float? anglevel;
        public float? width;
        public float? height;
        public float? depth;
        public VisualInfo? visualInfo;
        public string? claimedBy;

        public bool? claimable;

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
            builder.AppendLine("\tclaimedBy:\t" + claimedBy ?? "null");

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

    public abstract class SimBody {}

    public abstract class SimStatic {}
}