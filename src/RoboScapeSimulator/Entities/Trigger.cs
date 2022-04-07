using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using RoboScapeSimulator;
using RoboScapeSimulator.Entities;

class Trigger : DynamicEntity, IResettable
{
    private static uint ID = 0;

    public Trigger(Room room, in Vector3 initialPosition, in Quaternion initialOrientation, float width = 1, float height = 1, float depth = 1, bool debug = false)
    {
        if (debug)
        {
            VisualInfo = VisualInfo.None;
        }

        Name = $"trigger_{ID++}";

        Width = width;
        Height = height;
        Depth = depth;

        var simulationInstance = room.SimInstance;

        var box = new Box(width, height, depth);

        RigidPose pose = new(initialPosition, initialOrientation);

        // Body created which never sleeps, so trigger can repeatedly know bodies inside it
        BodyHandle bodyHandle = simulationInstance.Simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, new CollidableDescription(simulationInstance.Simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(-1)));
        BodyReference = simulationInstance.Simulation.Bodies.GetBodyReference(bodyHandle);

        ref var bodyProperties = ref simulationInstance.Properties.Allocate(BodyReference.Handle);
        bodyProperties = new BodyCollisionProperties { Friction = 1f, Filter = new SubgroupCollisionFilter(BodyReference.Handle.Value, 0), IsTrigger = true };

        room.SimInstance.NamedBodies.Add(Name, BodyReference);
        room.SimInstance.Entities.Add(this);
    }

    public void Reset()
    {

    }
}