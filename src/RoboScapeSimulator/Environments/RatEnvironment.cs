using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments;

/// <summary>
/// A class representing an environment composed of a square with three robots.
/// The red center-most robot, labeled rat, has a set behaviour in NetsBlox moving back and forth along an axis.
/// The goal of the other two robots is to detect it using LIDAR and intercept the rat's path.
/// </summary>
class RatEnvironment : EnvironmentConfiguration
{
    public RatEnvironment()
    {
        Name = "RatEnvironment";
        ID = "RatEnvironment";
        Description = $"Robots must trap the red robot";
        Category = "§_Testing";
    }

    public override object Clone()
    {
        return new RatEnvironment();
    }

    public override void Setup(Room room)
    {
        Trace.WriteLine($"Setting up {Name} environment");
        Random rng = new();

        //Creates the ground
        _ = new Ground(room);

        //Creates a closed 6 by 6 rectangle
        EnvironmentUtils.AddPath(room, new()
        {
            new(6.0f, 0, 0),
            new(6.0f, 0, 6.0f),
            new(0, 0, 6.0f),
            new(0, 0, 0),
            new(6.0f, 0, 0)
        });

        //Creates the robots
        Vector3 position = rng.PointOnLine(new(1.0f, 0.25f, 3.0f), new(5.0f, 0.25f, 3.0f));
        //This function ensures that the rat is placed on an axis
        position.X = MathF.Round(position.X);
        ParallaxRobot rat = new(room, position, Quaternion.Identity, visualInfo: new() { ModelName = "car1_red.gltf" });

        ParallaxRobot robot1 = new(room, new Vector3(0.5f, 0.25f, 5.0f), Quaternion.Identity);
        ParallaxRobot robot2 = new(room, new Vector3(5.5f, 0.25f, 1.0f), Quaternion.Identity);

        //Is it possible to program a robot's movements here instead of in NetsBlox? (ie create moving components)

        //Initializes the sensors
        PositionSensor positionSensor = new(rat);
        positionSensor.Setup(room);
        //Initializes one sensor in the front and one in the back, allows for back and forth motion of the rat
        var lidar = new LIDARSensor(rat) { Offset = new(0, 0.07f, 0.08f), NumRays = 2, StartAngle = MathF.PI, AngleRange = MathF.PI, MaxDistance = 5 };
        lidar.Setup(room);

        PositionSensor positionSensor1 = new(robot1);
        positionSensor1.Setup(room);
        var lidar1 = new LIDARSensor(robot1) { Offset = new(0, 0.0f, 0.08f), NumRays = 7, StartAngle = MathF.PI / 2, AngleRange = MathF.PI, MaxDistance = 5, };
        lidar1.Setup(room);

        PositionSensor positionSensor2 = new(robot2);
        positionSensor2.Setup(room);
        var lidar2 = new LIDARSensor(robot2) { Offset = new(0, 0.0f, 0.08f), NumRays = 7, StartAngle = MathF.PI / 2, AngleRange = MathF.PI, MaxDistance = 5, };
        lidar2.Setup(room);
    }
}