using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;

namespace RoboScapeSimulator.Entities;

/// <summary>
/// A box-shaped object
/// </summary>
class Cube : DynamicEntity, IResettable
{
    private static uint ID = 0;

    /// <summary>
    /// When a scene reset is requested by the user, should this Cube reset
    /// </summary>
    public bool AllowReset = true;

    private Vector3 initialPosition = new();
    private Quaternion initialOrientation = Quaternion.Identity;

    /// <summary>
    /// Create a new Cube
    /// </summary>
    /// <param name="room">Room to put this Cube into</param>
    /// <param name="width">width (x-axis)</param>
    /// <param name="height">height (y-axis)</param>
    /// <param name="depth">depth (z-axis)</param>
    /// <param name="initialPosition">Initial location of the Cube, or null for a random location</param>
    /// <param name="initialOrientation">Initial orientation of the Cube, or null for a random yaw</param>
    /// <param name="isKinematic">Whether this object should be movable</param>
    /// <param name="visualInfo">Visual description string for the Cube</param>
    public Cube(Room room, float width = 1, float height = 1, float depth = 1, in Vector3? initialPosition = null, in Quaternion? initialOrientation = null, bool isKinematic = false, in VisualInfo visualInfo = default, string nameOverride = "cube", float mass = 2, bool allowReset = true)
    {
        Name = $"{nameOverride}_{ID++}";

        VisualInfo = visualInfo;

        Width = width;
        Height = height;
        Depth = depth;

        AllowReset = allowReset;

        var rng = new Random();
        Vector3 position = initialPosition ?? new Vector3(rng.Next(-5, 5), rng.Next(3, 5), rng.Next(-5, 5));
        Quaternion orientation = initialOrientation ?? Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)rng.NextDouble() * MathF.PI);
        this.initialPosition = position;
        this.initialOrientation = Orientation;

        room.SimInstance.CreateBox(Name, position, orientation, width, height, depth, mass, isKinematic);
        room.SimInstance.Entities.Add(this);
    }

    public event EventHandler? OnReset;

    public void Reset()
    {
        if (AllowReset)
        {
            Position = initialPosition;
            Orientation = initialOrientation;
            BodyReference.LinearVelocity = new Vector3();
            BodyReference.AngularVelocity = new Vector3();
            OnReset?.Invoke(this, EventArgs.Empty);
        }
    }
}
