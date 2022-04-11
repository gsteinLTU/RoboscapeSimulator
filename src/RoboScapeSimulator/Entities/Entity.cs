using BepuPhysics;
using Newtonsoft.Json;

namespace RoboScapeSimulator.Entities;

/// <summary>
/// Base class for objects within the simulation
/// </summary>
public abstract class Entity : IDisposable
{
    /// <summary>
    /// Run the behavior of this Entity
    /// </summary>
    /// <param name="dt">Time delta in seconds</param>
    public virtual void Update(float dt) { }

    /// <summary>
    /// The name of this Entity
    /// </summary>
    public string Name = "entity";

    public VisualInfo VisualInfo = VisualInfo.DefaultCube;

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Entity()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public abstract BodyInfo GetBodyInfo(bool allData);
}

/// <summary>
/// Stores information used to display an Entity
/// </summary>
[Serializable]
public struct VisualInfo
{
    public VisualInfo() { }

    /// <summary>
    /// Name of the model to be displayed, if any
    /// </summary>
    [JsonProperty("model")]
    public string ModelName = "";

    /// <summary>
    /// Uniform scale to apply to model
    /// </summary>
    [JsonProperty("modelScale")]
    public float ModelScale = 1;

    /// <summary>
    /// Color to apply to cube mesh if no model is used
    /// </summary>
    [JsonProperty("color")]
    public string Color = "#fff";

    /// <summary>
    /// Texture to apply to cube mesh if no model is used
    /// </summary>
    [JsonProperty("image")]
    public string Image = "";

    /// <summary>
    /// Empty VisualInfo to display a default white cube in the client
    /// </summary>
    public static readonly VisualInfo DefaultCube = new() { };

    /// <summary>
    /// Used to indicate that an Entity does not have any visual representation (e.g. <see cref="Trigger"/> with debug disabled)
    /// </summary>
    public static readonly VisualInfo None = new() { ModelScale = -1 };
}

/// <summary>
/// Interface for Entities with the ability to be reset
/// </summary>
public interface IResettable
{
    /// <summary>
    /// Restore this Entity to its original state
    /// </summary>
    public void Reset();

    /// <summary>
    /// Event to fire when resetting
    /// </summary>
    public event EventHandler? OnReset;
}

/// <summary>
/// Base class for Entity classes with static bodies
/// </summary>
public abstract class StaticEntity : Entity
{
    /// <summary>
    /// The reference to this object in the simulation
    /// </summary>
    public StaticReference StaticReference;

    public override BodyInfo GetBodyInfo(bool allData)
    {
        return new BodyInfo
        {
            label = allData ? Name : null,
            pos = {
                            x = StaticReference.Pose.Position.X,
                            y = StaticReference.Pose.Position.Y,
                            z = StaticReference.Pose.Position.Z
                        },
            angle = StaticReference.Pose.Orientation,
            width = allData ? StaticReference.BoundingBox.Max.X - StaticReference.BoundingBox.Min.X : null,
            height = allData ? StaticReference.BoundingBox.Max.Y - StaticReference.BoundingBox.Min.Y : null,
            depth = allData ? StaticReference.BoundingBox.Max.Z - StaticReference.BoundingBox.Min.Z : null,
            visualInfo = allData ? VisualInfo : null
        };
    }
}


/// <summary>
/// Base class for Entity classes with non-static bodies
/// </summary>
public abstract class DynamicEntity : Entity
{
    /// <summary>
    /// The reference to this object's body in the simulation
    /// </summary>
    public BodyReference BodyReference;

    public float Width = 1;

    public float Height = 1;

    public float Depth = 1;

    public override BodyInfo GetBodyInfo(bool allData)
    {
        return new BodyInfo
        {
            label = allData ? Name : null,
            pos = {
                x = BodyReference.Pose.Position.X,
                y = BodyReference.Pose.Position.Y,
                z = BodyReference.Pose.Position.Z
            },
            angle = BodyReference.Pose.Orientation,
            width = allData ? Width : null,
            height = allData ? Height : null,
            depth = allData ? Depth : null,
            visualInfo = allData ? VisualInfo : null,
            vel = BodyReference.Velocity.Linear
        };
    }
}
