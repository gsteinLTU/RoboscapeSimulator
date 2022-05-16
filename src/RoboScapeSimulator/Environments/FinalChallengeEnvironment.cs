using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments;

internal class FinalChallengeEnvironment : EnvironmentConfiguration
{
    readonly bool _mini = false;

    public FinalChallengeEnvironment(bool mini = false)
    {
        _mini = mini;
        Name = "Final Challenge" + (_mini ? " (mini)" : "");
        ID = "final1" + (_mini ? "mini" : "");
        Description = "Final activity for SSMV 2022";
    }

    public override object Clone()
    {
        return new FinalChallengeEnvironment(_mini);
    }

    public override void Setup(Room room)
    {
        Trace.WriteLine($"Setting up {Name} environment");

        Random rng = new();

        StopwatchTimer timer = new(room);

        // Ground
        _ = new Ground(room);

        List<Vector3> centerPoints1 = new()
        {
            new Vector3(0f, 3.25f, -0.5f),
            new Vector3(0f, 3.25f, 2f),
            new Vector3(-1f, 3.25f, 3f),
            new Vector3(-1f, 3.25f, 3.25f),
            new Vector3(1.75f, 3.25f, 4.5f),
            new Vector3(1.75f, 3.25f, 5f),
            new Vector3(0f, 3.25f, 6f),
            new Vector3(0f, 3.25f, 6.25f),
        };

        List<Vector3> centerPoints2 = new()
        {
            new Vector3(0f, 3.25f, 7.25f),
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

        var miniScale = new Vector3(0.8f, 1, 0.7f);

        if (_mini)
        {
            centerPoints1 = centerPoints1.Select(p => p * miniScale).ToList();
            centerPoints2 = centerPoints2.Select(p => p * miniScale).ToList();
            otherPath = otherPath.Select(p => p * miniScale).ToList();
            rightWalls = rightWalls.Select(p => p * miniScale).ToList();
            leftWalls = leftWalls.Select(p => p * miniScale).ToList();
            innerWalls = innerWalls.Select(p => p * miniScale).ToList();
        }

        EnvironmentUtils.AddPath(room, centerPoints1, 1, 0.5f, 0.5f, visualInfo: VisualInfo.DefaultCube);
        EnvironmentUtils.AddPath(room, centerPoints2, 1, 0.5f, 0.5f, visualInfo: VisualInfo.DefaultCube);
        EnvironmentUtils.AddPath(room, otherPath, 1, 0.5f, 0.5f, visualInfo: VisualInfo.DefaultCube);

        EnvironmentUtils.AddPath(room, rightWalls, padding: 0.05f);
        EnvironmentUtils.AddPath(room, leftWalls, padding: 0.05f);
        EnvironmentUtils.AddPath(room, innerWalls, padding: 0.05f);

        _ = new Cube(room, 4, 1, 4, new Vector3(0f, 3f, 15f) * (_mini ? miniScale : Vector3.One), Quaternion.Identity, isKinematic: true);

        Vector3 cubePos = rng.PointOnCircle(1f);

        if (cubePos.Z < 0)
        {
            cubePos.Z *= -1;
        }

        cubePos += new Vector3(0f, 3.6f, 14f) * (_mini ? miniScale : Vector3.One);
        var targetCube = new Cube(room, 1, 1, 1, cubePos, Quaternion.Identity, visualInfo: new VisualInfo() { Color = "#363" });
        var targetTrigger = new Trigger(room, targetCube.Position, targetCube.Orientation, 1.1f, 1.1f, 1.1f);


        // Robots
        var highBot = new ParallaxRobot(room, new Vector3(0, 3.75f, 0f), Quaternion.Identity, visualInfo: new() { ModelName = "car1_red.gltf" }, debug: false);
        var lowBot = new ParallaxRobot(room, new Vector3(-1.25f, 0.25f, 0), Quaternion.Identity, visualInfo: new() { ModelName = "car1_green.gltf" }, debug: false);

        var highLidar = new LIDARSensor(highBot) { Offset = new(0, 0.08f, 0.0f), NumRays = 3, StartAngle = MathF.PI / 4, AngleRange = 2 * MathF.PI / 4, MaxDistance = 5 };
        highLidar.Setup(room);

        var lowLidar = new LIDARSensor(lowBot) { Offset = new(0, 0.08f, 0.0f), NumRays = 3, StartAngle = MathF.PI / 4, AngleRange = 2 * MathF.PI / 4, MaxDistance = 5 };
        lowLidar.Setup(room);

        var highPos = new PositionSensor(highBot);
        var lowPos = new PositionSensor(lowBot);
        highPos.Setup(room);
        lowPos.Setup(room);

        // First obstacle
        var firstBlock = new Cube(room, 1, 1, 1, new Vector3(0f, 3.75f, 1f) * (_mini ? miniScale : Vector3.One), Quaternion.Identity, isKinematic: true);
        var secondBlock = new Cube(room, 1.25f, 1, 1.25f, new Vector3(0f, -3.75f, 1f), Quaternion.Identity, isKinematic: true);

        Waypoints lowWaypoints = new(room, () =>
        {
            return new List<Vector3>() {
                (new Vector3(0f, 0f, 1f) * (_mini? miniScale : Vector3.One))+ rng.PointOnCircle(0.5f),
                (new Vector3(0f, 0f, 5.75f) * (_mini? miniScale : Vector3.One)) + rng.PointOnCircle(1f),
                (new Vector3(0f, 0f, 16f) * (_mini? miniScale : Vector3.One)),
            };
        }, lowBot.ID, 3);

        lowWaypoints.OnWaypointActivated += (o, idx) =>
        {
            if (idx == 0)
            {
                firstBlock.Position = new Vector3(0.25f, -3.75f, 1f);
                firstBlock.forceUpdate = true;
            }
            if (idx == 1)
            {
                secondBlock.Position = new Vector3(0f, 3f, 6.75f) * (_mini ? miniScale : Vector3.One);
                secondBlock.BodyReference.Awake = true;
                secondBlock.forceUpdate = true;
            }
        };


        Waypoints highWaypoints = new(room, () =>
        {
            return new List<Vector3>() {
                new Vector3(0.25f, 3.5f, 5.75f) * (_mini? miniScale : Vector3.One),
                new Vector3(0f, 3.5f, 11.4f) * (_mini? miniScale : Vector3.One),
                new Vector3(0f, 3.5f, 13.25f) * (_mini? miniScale : Vector3.One),
            };
        }, highBot.ID, threshold: 0.75f);

        targetTrigger.OnTriggerEnter += (o, e) =>
        {
            if (e is Robot r)
            {
                if (r.ID == lowBot.ID)
                {
                    // Win condition for now
                    timer.Stop();
                }
            }
        };

        room.OnUpdate += (e, dt) =>
        {
            targetTrigger.Position = targetCube.Position;
            targetTrigger.Orientation = targetCube.Orientation;
        };

        room.OnReset += (o, e) =>
        {
            Vector3 cubePos = rng.PointOnCircle(1.5f);

            if (cubePos.Z < 0)
            {
                cubePos.Z *= -1;
            }

            cubePos += new Vector3(0f, 3.6f, 15f) * (_mini ? miniScale : Vector3.One);
            targetCube.Position = cubePos;

            firstBlock.Position = new Vector3(0f, 3.75f, 1f) * (_mini ? miniScale : Vector3.One);
            firstBlock.BodyReference.Awake = true;

            firstBlock.forceUpdate = true;
            secondBlock.Position = new Vector3(0f, -3f, 6.75f);
            secondBlock.BodyReference.Awake = true;
            secondBlock.forceUpdate = true;
        };
    }
}