using System.Numerics;
using System.Text.Json.Serialization;
using RoboScapeSimulator.Physics;

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
    /// The position of this Entity
    /// </summary>
    public Vector3 Position
    {
        get;
    }

    /// <summary>
    /// The orientation of this Entity
    /// </summary>
    public Quaternion Orientation
    {
        get;
    }

    /// <summary>
    /// The name of this Entity
    /// </summary>
    public string Name = "entity";

    /// <summary>
    /// VisualInfo describing this Entity's appearance
    /// </summary>
    internal VisualInfo visualInfo = VisualInfo.DefaultCube;

    public bool ShouldUpdateVisualInfo = false;

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

    /// <summary>
    /// Create a BodyInfo for this Entity's current state
    /// </summary>
    /// <param name="allData">Should fields unlikely to change be included</param>
    /// <returns>BodyInfo for this Entity</returns>
    public abstract BodyInfo GetBodyInfo(bool allData);

    public abstract bool ShouldUpdate { get; }
    public VisualInfo VisualInfo { get => visualInfo; set {
        visualInfo = value;
        ShouldUpdateVisualInfo = true;
     }
    }

    /// <summary>
    /// A user ID "claiming" the robot
    /// </summary>
    public string? claimedByUser;

    /// <summary>
    /// A socket ID "claiming" the robot
    /// </summary>
    public string? claimedBySocket;

    /// <summary>
    /// Can this Entity be claimed by a user?
    /// </summary>
    public bool claimable;
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
    [JsonPropertyName("model")]
    public string ModelName = "";

    /// <summary>
    /// Uniform scale to apply to model
    /// </summary>
    [JsonPropertyName("modelScale")]
    public float ModelScale = 1;

    /// <summary>
    /// Color to apply to cube mesh if no model is used
    /// </summary>
    [JsonPropertyName("color")]
    public string Color = "#fff";

    /// <summary>
    /// Texture to apply to cube mesh if no model is used
    /// </summary>
    [JsonPropertyName("image")]
    public string Image = "";

    /// <summary>
    /// U scale to apply to image
    /// </summary>
    public float? uScale = null;

    /// <summary>
    /// V scale to apply to image
    /// </summary>
    public float? vScale = null;

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
    public SimStatic StaticReference;

    public new Vector3 Position
    {
        get => StaticReference.Position;
    }

    public new Quaternion Orientation
    {
        get => StaticReference.Orientation;
    }

    public override BodyInfo GetBodyInfo(bool allData)
    {
        return new BodyInfo
        {
            label = allData ? Name : null,
            pos = {
                            x = StaticReference.Position.X,
                            y = StaticReference.Position.Y,
                            z = StaticReference.Position.Z
                        },
            angle = StaticReference.Orientation,
            width = (allData || ShouldUpdateVisualInfo) ? StaticReference.Size.X : null,
            height = (allData || ShouldUpdateVisualInfo) ? StaticReference.Size.Y : null,
            depth = (allData || ShouldUpdateVisualInfo) ? StaticReference.Size.Z : null,
            visualInfo = (allData || ShouldUpdateVisualInfo) ? VisualInfo : null,
            claimable = allData ? claimable : null,
            claimedBy = allData ? claimedByUser : null
        };
    }

    public override bool ShouldUpdate => false;
}


/// <summary>
/// Base class for Entity classes with non-static bodies
/// </summary>
public abstract class DynamicEntity : Entity
{
    /// <summary>
    /// The reference to this object's body in the simulation
    /// </summary>
    public SimBody BodyReference;

    public new Vector3 Position
    {
        get => BodyReference.Position;
        set
        {
            BodyReference.Position = value;
            forceUpdate = true;
        }
    }

    public new Quaternion Orientation
    {
        get => BodyReference.Orientation;
        set
        {
            BodyReference.Orientation = value;
            forceUpdate = true;
        }
    }

    public float Width = 1;

    public float Height = 1;

    public float Depth = 1;

    public override BodyInfo GetBodyInfo(bool allData)
    {
        return new BodyInfo
        {
            label = allData ? Name : null,
            pos = {
                x = Position.X,
                y = Position.Y,
                z = Position.Z
            },
            angle = Orientation,
            width = (allData || ShouldUpdateVisualInfo) ? Width : null,
            height = (allData || ShouldUpdateVisualInfo) ? Height : null,
            depth = (allData || ShouldUpdateVisualInfo) ? Depth : null,
            visualInfo = (allData || ShouldUpdateVisualInfo) ? VisualInfo : null,
            vel = BodyReference.LinearVelocity,
            claimable = allData ? claimable : null,
            claimedBy = allData ? claimedByUser : null
        };
    }

    internal bool forceUpdate = false;

    public override bool ShouldUpdate
    {
        get
        {
            if (forceUpdate)
            {
                forceUpdate = false;
                return true;
            }
            return BodyReference.Awake;
        }
    }
}
