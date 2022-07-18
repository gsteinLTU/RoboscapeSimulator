using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments;

class WaypointChase : EnvironmentConfiguration
{
    readonly uint _waittime = 0;

    public WaypointChase(uint waittime = 0)
    {
        _waittime = waittime;
        Name = "Waypoint Chase" + (_waittime > 0 ? $" (wait for {_waittime} s)" : "");
        ID = "Waypoint Chase" + _waittime;
        Description = $"Robot must find waypoints and stay at them for {_waittime} seconds";
    }

    public override object Clone()
    {
        return new WaypointChase(_waittime);
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
        ParallaxRobot robot1 = new ParallaxRobot(room, new Vector3(0, 0.25f, 1.0f), Quaternion.Identity);
        ParallaxRobot robot2 = new ParallaxRobot(room, new Vector3(1.0f, 0.25f, 0), Quaternion.Identity);
        ParallaxRobot robot3 = new ParallaxRobot(room, new Vector3(-1.0f, 0.25f, 0), Quaternion.Identity);


        Waypoints waypointHelper = new(room, () =>
         {
             return new List<Vector3>()
             {
                rng.PointOnLine(new(-3f, 0, 2), new(2, 0, 2)),
                rng.PointOnLine(new(-2, 0, 4), new(3f, 0, 4)),
                rng.PointOnLine(new(-2.5f, 0, 6), new(2.5f, 0, 6)),
                rng.PointOnLine(new(-3f, 0, 2), new(2, 0, -2)),
                rng.PointOnLine(new(-2, 0, 4), new(3f, 0, -4)),
                rng.PointOnLine(new(-2.5f, 0, 6), new(2.5f, 0, -6))
             };
         }, robot1.ID, _waittime);

        PositionSensor positionSensor1 = new(robot1);
        positionSensor1.Setup(room);

        PositionSensor positionSensor2 = new(robot2);
        positionSensor2.Setup(room);

        PositionSensor positionSensor3 = new(robot3);
        positionSensor3.Setup(room);
    }
}