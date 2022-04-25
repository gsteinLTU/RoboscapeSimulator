using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments;

internal class FinalChallengeEnvironment : EnvironmentConfiguration
{
    public FinalChallengeEnvironment()
    {
        Name = "Final Challenge";
        ID = "final1";
        Description = "Final activity for SSMV 2022";
    }

    public override object Clone()
    {
        return new FinalChallengeEnvironment();
    }

    public override void Setup(Room room)
    {
        Trace.WriteLine($"Setting up {Name} environment");

        Random rng = new();

        StopwatchTimer timer = new StopwatchTimer(room);

        // Ground
        _ = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

        List<Vector3> centerPoints = new()
        {
            new Vector3(0f, 3.25f, -0.5f),
            new Vector3(0f, 3.25f, 2f),
            new Vector3(-1f, 3.25f, 3f),
            new Vector3(-1f, 3.25f, 3.25f),
            new Vector3(1.75f, 3.25f, 4.5f),
            new Vector3(1.75f, 3.25f, 5f),
            new Vector3(0f, 3.25f, 6f),
            new Vector3(0f, 3.25f, 7.5f),
            new Vector3(-2f, 3.25f, 9f),
            new Vector3(-2f, 3.25f, 9.5f),
            new Vector3(0f, 3.25f, 11.5f),
            new Vector3(0f, 3.25f, 13f),
        };

        List<Vector3> otherPath = new()
        {
            new Vector3(0f, 3.25f, 7.5f),
            new Vector3(2f, 3.25f, 9f),
            new Vector3(2f, 3.25f, 9.5f),
            new Vector3(0f, 3.25f, 11.5f),
        };

        List<Vector3> leftWalls = new()
        {
            new Vector3(0.5f, 3.75f, -0.75f),
            new Vector3(-0.5f, 3.75f, -0.75f),
            new Vector3(-0.5f, 3.75f, 1.75f),
            new Vector3(-1.5f, 3.75f, 2.75f),
            new Vector3(-1.5f, 3.75f, 3.5f),
            new Vector3(1.25f, 3.75f, 4.75f),
            new Vector3(-0.5f, 3.75f, 5.75f),
            new Vector3(-0.5f, 3.75f, 7.25f),
            new Vector3(-2.5f, 3.75f, 8.75f),
            new Vector3(-2.5f, 3.75f, 9.75f),
            new Vector3(-0.5f, 3.75f, 11.75f),
            new Vector3(-0.5f, 3.75f, 13f),
        };

        List<Vector3> rightWalls = new()
        {
            new Vector3(0.5f, 3.75f, -0.75f),
            new Vector3(0.5f, 3.75f, 2.25f),
            new Vector3(-0.25f, 3.75f, 3f),
            new Vector3(2.25f, 3.75f, 4f),
            new Vector3(2.25f, 3.75f, 5.25f),
            new Vector3(0.5f, 3.75f, 6.25f),
            new Vector3(0.5f, 3.75f, 7.25f),
            new Vector3(2.5f, 3.75f, 8.75f),
            new Vector3(2.5f, 3.75f, 9.75f),
            new Vector3(0.5f, 3.75f, 11.75f),
            new Vector3(0.5f, 3.75f, 13f),
        };

        List<Vector3> innerWalls = new()
        {
            new Vector3(0f, 3.75f, 8f),
            new Vector3(-1.5f, 3.75f, 9.25f),
            new Vector3(0f, 3.75f, 10.75f),
            new Vector3(1.5f, 3.75f, 9.25f),
            new Vector3(0f, 3.75f, 8f),
        };

        EnvironmentUtils.AddPath(room, centerPoints, 1, 0.5f, 0.5f, visualInfo: VisualInfo.DefaultCube);
        EnvironmentUtils.AddPath(room, otherPath, 1, 0.5f, 0.5f, visualInfo: VisualInfo.DefaultCube);

        EnvironmentUtils.AddPath(room, rightWalls, padding: 0.05f);
        EnvironmentUtils.AddPath(room, leftWalls, padding: 0.05f);
        EnvironmentUtils.AddPath(room, innerWalls, padding: 0.05f);




        _ = new Cube(room, 5, 1, 4, new Vector3(0f, 3f, 15f), Quaternion.Identity, isKinematic: true);

        Vector3 cubePos = rng.PointOnCircle(1.5f);

        if (cubePos.Z < 0)
        {
            cubePos.Z *= -1;
        }

        cubePos += new Vector3(0f, 3.6f, 15f);
        var targetCube = new Cube(room, 1, 1, 1, cubePos, Quaternion.Identity, visualInfo: new VisualInfo() { Color = "#363" });

        // Robots
        var highBot = new ParallaxRobot(room, new Vector3(0, 3.75f, 0f), Quaternion.Identity, visualInfo: new() { ModelName = "car1_red.gltf" }, debug: false);
        var lowBot = new ParallaxRobot(room, new Vector3(-1.25f, 0.25f, 0), Quaternion.Identity, visualInfo: new() { ModelName = "car1_green.gltf" }, debug: false);

        var highLidar = new LIDARSensor(highBot);
        highLidar.Setup(room);

        var lowLidar = new LIDARSensor(lowBot);
        lowLidar.Setup(room);


        // First obstacle
        var firstBlock = new Cube(room, 1, 1, 1, new Vector3(0f, 3.75f, 1f), Quaternion.Identity, isKinematic: true);

        Waypoints lowWaypoints = new(room, () =>
        {
            return new List<Vector3>() {
                new Vector3(0f, 0f, 1f)
            };
        }, lowBot.ID, 3);

        lowWaypoints.OnWaypointActivated += (o, idx) =>
        {
            if (idx == 0)
            {
                firstBlock.Position = new Vector3(0f, -3.75f, 1f);
            }
        };


        Waypoints highWaypoints = new(room, () =>
        {
            return new List<Vector3>() {
                new Vector3(0f, 3.5f, 5.75f),
                new Vector3(0f, 3.5f, 11.75f),
            };
        }, highBot.ID);

        room.OnReset += (o, e) =>
        {
            Vector3 cubePos = rng.PointOnCircle(1.5f);

            if (cubePos.Z < 0)
            {
                cubePos.Z *= -1;
            }

            cubePos += new Vector3(0f, 3.6f, 15f);
            targetCube.Position = cubePos;

            firstBlock.Position = new Vector3(0f, 3.75f, 1f);
        };
    }
}