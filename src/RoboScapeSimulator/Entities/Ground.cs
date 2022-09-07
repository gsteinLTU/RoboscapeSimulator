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
    private static VisualInfo GroundDefault = new()
    {
        Image = "grid.png",
        uScale = 1,
        vScale = 1
    };

    public Ground(Room room, float xsize = 200, float zsize = 100, in Vector3? position = null, float thickness = 0.1f, in VisualInfo? visualInfo = null)
    {
        Name = $"ground_{ID++}";

        if (visualInfo == null)
        {
            VisualInfo = GroundDefault;
            VisualInfo.uScale = zsize;
            VisualInfo.vScale = xsize;
        }
        else
        {
            VisualInfo = visualInfo.Value;
        }

        StaticReference = room.SimInstance.CreateStaticBox(Name, position ?? new Vector3(0, -thickness / 2, 0), null, xsize, thickness, zsize);
        room.SimInstance.Entities.Add(this);
    }
}