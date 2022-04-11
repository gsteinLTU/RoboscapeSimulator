using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;

namespace RoboScapeSimulator.Entities;

/// <summary>
/// A static planar surface
/// </summary>
class Ground : StaticEntity
{
    private static uint ID = 0;

    public Ground(Room room, float xsize = 200, float zsize = 100, in Vector3? position = null, float thickness = 0.1f, in VisualInfo visualInfo = default)
    {
        Name = $"ground_{ID++}";
        VisualInfo = visualInfo;
        var simulationInstance = room.SimInstance;
        var groundHandle = simulationInstance.Simulation.Statics.Add(new StaticDescription(position ?? new Vector3(0, -thickness / 2, 0), simulationInstance.Simulation.Shapes.Add(new Box(xsize, thickness, zsize))));
        StaticReference = simulationInstance.Simulation.Statics.GetStaticReference(groundHandle);

        room.SimInstance.NamedStatics.Add(Name, StaticReference);
        room.SimInstance.Entities.Add(this);
    }
}