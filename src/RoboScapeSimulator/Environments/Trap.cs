using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments;

class Trap : EnvironmentConfiguration
{
    readonly uint _waittime = 0;

    public Trap(uint waittime = 0)
    {
        _waittime = waittime;
        Name = "Trap";
        ID = "Trap";
        Description = $"Robot must find waypoints and stay at them for {_waittime} seconds";
    }

    public override object Clone()
    {
        return new Trap(_waittime);
    }

    public override void Setup(Room room)
    {
        Trace.WriteLine($"Setting up {Name} environment");
        Random rng = new();

        //Creates the ground
        _ = new Ground(room);

        //Creates walls
        EnvironmentUtils.MakeWalls(room);

        //Creates the robots
        ParallaxRobot rat = new ParallaxRobot(room, rng.PointOnLine(new(-5.0f, 0.25f, 0), new(5.0f, 0.25f, 0)), Quaternion.Identity);

        ParallaxRobot robot1 = new ParallaxRobot(room, new Vector3(-6.5f, 0.25f, 5.5f), Quaternion.Identity);
        ParallaxRobot robot2 = new ParallaxRobot(room, new Vector3(6.5f, 0.25f, -5.5f), Quaternion.Identity);

        //Is it possible to program a robot's movements here instead of in NetsBlox? (ie create moving components)
        PositionSensor positionSensor = new(rat);
        positionSensor.Setup(room);
        //Initializes one sensor in the front and one in the back, allows for back and forth motion of the rat
        var lidar = new LIDARSensor(rat) { Offset = new(0, 0.08f, 0.0f), NumRays = 2, StartAngle = MathF.PI, AngleRange = MathF.PI, MaxDistance = 5 };
        lidar.Setup(room);

        PositionSensor positionSensor1 = new(robot1);
        positionSensor1.Setup(room);
        var lidar1 = new LIDARSensor(robot1) { Offset = new(0, 0.08f, 0.0f), NumRays = 3, StartAngle = MathF.PI / 4, AngleRange = 2 * MathF.PI / 4, MaxDistance = 5 };
        lidar1.Setup(room);

        PositionSensor positionSensor2 = new(robot2);
        positionSensor2.Setup(room);
        var lidar2 = new LIDARSensor(robot2) { Offset = new(0, 0.08f, 0.0f), NumRays = 3, StartAngle = MathF.PI / 4, AngleRange = 2 * MathF.PI / 4, MaxDistance = 5 };
        lidar2.Setup(room);
    }
}