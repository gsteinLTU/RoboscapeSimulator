using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments;

class WaypointNavigationEnvironment : EnvironmentConfiguration
{
    readonly uint _waittime = 0;

    public WaypointNavigationEnvironment(uint waittime = 0)
    {
        _waittime = waittime;
        Name = "Waypoint Navigation" + (_waittime > 0 ? $" (wait for {_waittime} s)" : "");
        ID = "WaypointNavigation_" + _waittime;
        Description = $"Robot must find waypoints and stay at them for {_waittime} seconds";
    }

    public override object Clone()
    {
        return new WaypointNavigationEnvironment(_waittime);
    }

    public override void Setup(Room room)
    {
        Trace.WriteLine($"Setting up {Name} environment");
        Random rng = new();

        // Ground
        _ = new Ground(room);

        // robot
        var robot = new ParallaxRobot(room, new Vector3(0, 0.25f, 0), Quaternion.Identity, debug: false);

        // Waypoints

        Waypoints waypointHelper = new(room, () =>
        {
            return new List<Vector3>()
            {
                rng.PointOnLine(new(-3f, 0, 2), new(2, 0, 2)),
                rng.PointOnLine(new(-2, 0, 4), new(3f, 0, 4)),
                rng.PointOnLine(new(-2.5f, 0, 6), new(2.5f, 0, 6))
            };
        }, robot.ID, _waittime);

        PositionSensor positionSensor = new(robot);
        positionSensor.Setup(room);
    }
}