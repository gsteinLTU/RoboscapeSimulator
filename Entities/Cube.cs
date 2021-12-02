using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;

class Cube
{
    BodyReference bodyReference;

    public Cube(SimulationInstance simulationInstance, float size = 1)
    {
        var rng = new Random();
        var box = new BepuPhysics.Collidables.Box(size, size, size);
        box.ComputeInertia(2, out var boxInertia);
        var bodyHandle = simulationInstance.Simulation.Bodies.Add(BodyDescription.CreateDynamic(new Vector3(rng.Next(-5, 5), rng.Next(3, 5), rng.Next(-5, 5)), boxInertia, new CollidableDescription(simulationInstance.Simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(0.01f)));
        bodyReference = simulationInstance.Simulation.Bodies.GetBodyReference(bodyHandle);

        bodyReference.Pose.Orientation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)rng.NextDouble() * MathF.PI);

        ref var bodyProperties = ref simulationInstance.Properties.Allocate(bodyReference.Handle);
        bodyProperties = new BodyCollisionProperties { Friction = 1f, Filter = new SubgroupCollisionFilter(bodyReference.Handle.Value, 0) };

    }

    public BodyReference GetMainBodyReference()
    {
        return bodyReference;
    }
}