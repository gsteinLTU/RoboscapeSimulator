using System.Numerics;
using RoboScapeSimulator.Entities;

namespace RoboScapeSimulator.Environments.Helpers;

public static class EnvironmentUtils
{
    public static void AddPath(Room room, List<Vector3> points, float thickness = 0.1f, float height = 0.5f, VisualInfo? visualInfo = null)
    {
        Vector3 previous = points[0];
        foreach (var point in points)
        {
            AddWall(room, previous, point, thickness, height, visualInfo: visualInfo);
            previous = point;
        }
    }

    internal static Cube? AddWall(Room room, Vector3 p1, Vector3 p2, float thickness = 0.1f, float height = 0.5f, float padding = 0.05f, VisualInfo? visualInfo = null)
    {
        var length = (p2 - p1).Length();

        if (length < 0.0001)
        {
            return null;
        }

        var center = p1 + ((p2 - p1) * 0.5f);
        var angle = MathF.Atan2(p2.Z - p1.Z, p2.X - p1.X);
        return new Cube(room, length + padding, height, thickness, center, Quaternion.CreateFromAxisAngle(Vector3.UnitY, -angle), true, visualInfo: visualInfo ?? new VisualInfo() { Color = "#633" });
    }
}