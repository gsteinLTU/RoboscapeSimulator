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
    internal static List<Cube> AddPath(Room room, List<Vector3> points, float thickness = 0.1f, float height = 0.5f, float padding = 0.05f, VisualInfo? visualInfo = null)
    {
        List<Cube> path = new();
        Vector3 previous = points[0];
        foreach (var point in points)
        {
            var wall = AddWall(room, previous, point, thickness, height, padding, visualInfo);
            if (wall != null)
            {
                path.Add(wall);
            }
            previous = point;
        }

        return path;
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

    /// <summary>
    /// Create a rectangular walled area
    /// </summary>
    /// <param name="room">Room to add walls to</param>
    /// <param name="xSize">Length of walls</param>
    /// <param name="zSize">Depth of walls</param>
    /// <param name="thickness">Thickness of walls</param>
    /// <param name="height">Height of walls</param>
    /// <param name="offset">Offset to center of walled-in area</param>
    internal static void MakeWalls(Room room, float xSize = 15, float zSize = 15, float thickness = 1, float height = 1, Vector3? offset = null)
    {
        offset ??= new Vector3(0, height / 2f, 0);
        _ = new Cube(room, xSize, height, thickness, new Vector3(0, 0, -zSize / 2) + offset, Quaternion.Identity, true, nameOverride: "wall1");
        _ = new Cube(room, xSize, height, thickness, new Vector3(0, 0, zSize / 2) + offset, Quaternion.Identity, true, nameOverride: "wall2");
        _ = new Cube(room, thickness, height, zSize + thickness, new Vector3(-xSize / 2, 0, 0) + offset, Quaternion.Identity, true, nameOverride: "wall3");
        _ = new Cube(room, thickness, height, zSize + thickness, new Vector3(xSize / 2, 0, 0) + offset, Quaternion.Identity, true, nameOverride: "wall4");
    }
}