using System.Numerics;

namespace RoboScapeSimulator.Entities;

/// <summary>
/// An Entity that has no physics representation, appearing only as a visual on the client
/// </summary>
class VisualOnlyEntity : Entity
{
    private Vector3 position;
    public Vector3 Position
    {
        get => position;
        set
        {
            _moved = true;
            position = value;
        }
    }

    private Quaternion orientation;
    public Quaternion Orientation
    {
        get => orientation;
        set
        {
            _moved = true;
            orientation = value;
        }
    }

    public float Width;
    public float Height;
    public float Depth;

    private static uint ID = 1;

    /// <summary>
    /// Create a new VisualOnlyEntity
    /// </summary>
    /// <param name="room">Room to put this Cube into</param>
    /// <param name="initialPosition">Initial location of the Cube, or null for a random location</param>
    /// <param name="initialOrientation">Initial orientation of the Cube, or null for a random yaw</param>
    /// <param name="visualInfo">Visual description string for the Cube</param>
    /// <param name="nameOverride">Name to use for this entity</param>
    public VisualOnlyEntity(Room room, in Vector3 initialPosition, in Quaternion initialOrientation, in VisualInfo visualInfo = default, float width = 1, float height = 1, float depth = 1, string nameOverride = "prop")
    {
        Name = $"{nameOverride}_{ID++}";

        VisualInfo = visualInfo;
        Width = width;
        Height = height;
        Depth = depth;
        Position = initialPosition;
        Orientation = initialOrientation;

        room.SimInstance.Entities.Add(this);
    }

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
            width = allData ? Width : null,
            height = allData ? Height : null,
            depth = allData ? Depth : null,
            visualInfo = allData ? VisualInfo : null,
            vel = Vector3.Zero
        };
    }

    private bool _moved = true;

    public override bool ShouldUpdate
    {
        get
        {
            if (_moved)
            {
                _moved = false;
                return true;
            }
            return false;
        }
    }
}