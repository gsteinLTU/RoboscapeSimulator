using System.Diagnostics;
using System.Numerics;
using System.Text;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.RobotScape;
namespace RoboScapeSimulator.Physics
{
    /// <summary>
    /// Abstract base class providing interface used for interacting with physics engine
    /// </summary>
    public abstract class SimulationInstance : IDisposable
    {
        public List<Entity> Entities = new();

        /// <summary>
        /// Enumerable of Robot-type Entities in the current simulation
        /// </summary>
        public IEnumerable<Robot> Robots => Entities.Where(e => e is Robot).Cast<Robot>();

        /// <summary>
        /// Enumerable of Trigger-type Entities in the current simulation
        /// </summary>
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

        /// <summary>
        /// Add a cubic body to the simulation
        /// </summary>
        /// <param name="name">Name to give this body</param>
        /// <param name="position">Initial position of body</param>
        /// <param name="orientation">Initial orientation of body, or null to use default</param>
        /// <param name="width">Width (X) of cubic volume</param>
        /// <param name="height">Height (Y) of cubic volume</param>
        /// <param name="depth">Depth (Z) of cubic volume</param>
        /// <param name="mass">Mass of body, only needed for non-kinematic bodies</param>
        /// <param name="isKinematic">If the body should have motion physics applied to it</param>
        /// <returns>SimBody containing information about body</returns>
        public abstract SimBody CreateBox(string name, Vector3 position, Quaternion? orientation = null, float width = 1, float height = 1, float depth = 1, float mass = 1, bool isKinematic = false);
        
        /// <summary>
        /// Add a static cubic body to the simulation
        /// </summary>
        /// <param name="name">Name to give this body</param>
        /// <param name="position">Position of body</param>
        /// <param name="orientation">Orientation of body, or null to use default</param>
        /// <param name="width">Width (X) of cubic volume</param>
        /// <param name="height">Height (Y) of cubic volume</param>
        /// <param name="depth">Depth (Z) of cubic volume</param>
        /// <returns>SimStatic containing information about body</returns>
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
                else if (allData || entity.ShouldUpdate || entity.ShouldUpdateVisualInfo)
                {
                    output.Add(entity.Name, entity.GetBodyInfo(allData));
                }
                entity.ShouldUpdateVisualInfo = false;
            }

            return output;
        }
    }

    /// <summary>
    /// Information about a body, sent to the client
    /// </summary>
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

    /// <summary>
    /// Provides access to a body in the simulation. Subclasses for use in specific subtypes of SimulationInstance may provide more direct access to underlying simulation
    /// </summary>
    public abstract class SimBody {
        public abstract Vector3 Position { get; set; }
        public abstract Quaternion Orientation { get; set; }
        public abstract Vector3 LinearVelocity { get; set; }
        public abstract Vector3 AngularVelocity { get; set; }
        public abstract bool Awake { get; set; }
        public abstract float Mass { get; }
        public abstract Vector3 Size { get; }
        public abstract void ApplyForce(Vector3 force);
    }

    /// <summary>
    /// Provides access to a static object in the simulation. Subclasses for use in specific subtypes of SimulationInstance may provide more direct access to underlying simulation
    /// </summary>
    public abstract class SimStatic {
        public abstract Vector3 Position { get; set; }
        public abstract Quaternion Orientation { get; set; }
        public abstract Vector3 Size { get; }
    }

    /// <summary>
    /// Exception thrown when a method/function/environment/etc requires features of a specific physics engine not provided to it
    /// </summary>
    public class SimulationTypeNotSupportedException : Exception {
        public SimulationTypeNotSupportedException() : base("SimulationInstance type not supported") {}
    }
}