using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;

/// <summary>
/// A static planar surface
/// </summary>
class Ground
{
    /// <summary>
    /// The reference to this object in the simulation
    /// </summary>
    public StaticReference StaticReference;

    public Ground(Room room, float xsize = 200, float zsize = 100, Vector3? position = null, float thickness = 0.1f)
    {
        var simulationInstance = room.SimInstance;
        var groundHandle = simulationInstance.Simulation.Statics.Add(new StaticDescription(position ?? new Vector3(0, 0, 0), new CollidableDescription(simulationInstance.Simulation.Shapes.Add(new Box(xsize, thickness, zsize)), 0.1f)));
        StaticReference = simulationInstance.Simulation.Statics.GetStaticReference(groundHandle);
    }
}