using System.Numerics;
using RoboScapeSimulator.Entities;

namespace RoboScapeSimulator.Environments.Helpers;

/// <summary>
/// Various utility functions for environments
/// </summary>
public static class EnvironmentUtils
{
    /// <summary>
    /// Create a path along the XZ plane from a set of points
    /// </summary>
    /// <param name="room">Room to add path to</param>
    /// <param name="points">List of points in path</param>
    /// <param name="thickness">Thickness of path</param>
    /// <param name="height">Height of path</param>
    /// <param name="visualInfo">VisualInfo for path blocks, or null for default red</param>
    public static void AddPath(Room room, List<Vector3> points, float thickness = 0.1f, float height = 0.5f, VisualInfo? visualInfo = null)
    {
        Vector3 previous = points[0];
        foreach (var point in points)
        {
            AddWall(room, previous, point, thickness, height, 0.05f, visualInfo);
            previous = point;
        }
    }

    /// <summary>
    /// Create a wall segment on the XZ plane
    /// </summary>
    /// <param name="room">Room to add wall to</param>
    /// <param name="p1">First point of wall</param>
    /// <param name="p2">End point of wall</param>
    /// <param name="thickness">Thickness of wall</param>
    /// <param name="height">Height of wall</param>
    /// <param name="padding">Extra length to add to make angled paths look better</param> 
    /// <param name="visualInfo">VisualInfo for wall, or null for default red</param>
    /// <returns>Wall segment, or null if length was too short to create one</returns>
    internal static Cube? AddWall(Room room, Vector3 p1, Vector3 p2, float thickness = 0.1f, float height = 0.5f, float padding = 0f, VisualInfo? visualInfo = null)
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