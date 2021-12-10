using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;

/// <summary>
/// A box-shaped object
/// </summary>
class Cube
{
    /// <summary>
    /// The reference to this object's body in the simulation
    /// </summary>
    BodyReference bodyReference;

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
    public Cube(Room room, float width = 1, float height = 1, float depth = 1, Vector3? initialPosition = null, Quaternion? initialOrientation = null, bool isKinematic = false)
    {
        var simulationInstance = room.SimInstance;
        var rng = new Random();
        var box = new Box(width, height, depth);
        box.ComputeInertia(2, out var boxInertia);

        BodyHandle bodyHandle;
        if (isKinematic)
        {
            bodyHandle = simulationInstance.Simulation.Bodies.Add(BodyDescription.CreateKinematic(initialPosition ?? new Vector3(rng.Next(-5, 5), rng.Next(3, 5), rng.Next(-5, 5)), new CollidableDescription(simulationInstance.Simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(0.01f)));
        }
        else
        {
            bodyHandle = simulationInstance.Simulation.Bodies.Add(BodyDescription.CreateDynamic(initialPosition ?? new Vector3(rng.Next(-5, 5), rng.Next(3, 5), rng.Next(-5, 5)), boxInertia, new CollidableDescription(simulationInstance.Simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(0.01f)));
        }

        bodyReference = simulationInstance.Simulation.Bodies.GetBodyReference(bodyHandle);
        bodyReference.Pose.Orientation = initialOrientation ?? Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)rng.NextDouble() * MathF.PI);

        ref var bodyProperties = ref simulationInstance.Properties.Allocate(bodyReference.Handle);
        bodyProperties = new BodyCollisionProperties { Friction = 1f, Filter = new SubgroupCollisionFilter(bodyReference.Handle.Value, 0) };

    }

    public BodyReference GetMainBodyReference()
    {
        return bodyReference;
    }
}