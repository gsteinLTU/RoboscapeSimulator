using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments
{
    /// <summary>
    /// A class representing an environment composed of a rectangle with a door and a number N of robots and each of their own waypoints (WaypointForGate).
    /// Only if all robots trigger their respective waypoints within a 2 second window will the gate open.
    /// </summary>
    class GateEnvironment : EnvironmentConfiguration
    {
        //Creates a list for N timers
        readonly List<StopwatchLite> timers = new();
        //Creates a list for N waypoints
        readonly List<WaypointForGate> waypoints = new();
        //Stores the type of course
        readonly private string _courseType;
        //Stores the number (N) of robots
        readonly uint _numBots;

        public GateEnvironment(string courseType = Courses.Default, uint numBots = 2)
        {
            Name = $"Gate Environment({courseType}, {numBots} Robots)";
            ID = "Gate" + courseType + " " + numBots;
            Description = "N robots, N waypoints, and a gate";
            _courseType = courseType;
            _numBots = numBots;
        }

        /// <summary>
        /// Courses changes the appearance of the environment.
        /// </summary>
        public struct Courses
        {
            //Default is just the rectangle.
            public const string Default = "Default";
            //Obstacles adds additional walls the robots must maneuver around.
            public const string Obstacles = "Obstacles";
        }

        public override object Clone()
        {
            return new GateEnvironment(_courseType, _numBots);
        }

        public override void Setup(Room room)
        {

            Trace.WriteLine("Setting up Gate Environment");

            //Creates the ground
            _ = new Ground(room);

            //Creates rectangular wall with opening
            EnvironmentUtils.AddPath(room, new()
            {
                new(1.0f, 0, 9.0f),
                new(4.0f, 0, 9.0f),
                new(4.0f, 0, -1.0f),
                new(-4.0f, 0, -1.0f),
                new(-4.0f, 0, 9.0f),
                new(-1.0f, 0, 9.0f),
            });

            //Creates gate to fill in opening
            Cube gate = new Cube(room, 1.8f, 0.25f, 0.15f, new Vector3(0, 0, 9.0f), Quaternion.Identity);
            gate.BodyReference.Awake = true;
            gate.forceUpdate = true;

            Random rng = new();

            //Stores the robots, to be initialized in a loop later on
            ParallaxRobot robot;

            switch (_courseType)
            {
                //An Obstacles course will spawn 3 extra walls, requiring the use of additional sensors
                case Courses.Obstacles:
                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(-2.0f, 0, 2.5f),
                        new(2.0f, 0, 2.5f),
                    });

                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(-4.0f, 0, 5.0f),
                        new(-1.5f, 0, 5.0f),
                    });

                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(1.5f, 0, 5.0f),
                        new(4.0f, 0, 5.0f),
                    });
                    break;
            }

            //Initializes number of robots as set by _numBots
            for (int i = 0; i < _numBots; i++)
            {
                robot = new ParallaxRobot(room, rng.PointOnLine(new(-3.5f, 0.25f, 0), new(3.5f, 0.25f, 0)), Quaternion.Identity);
                //Initializes the sensors for the robots
                PositionSensor positionSensor = new(robot);
                var lidar = new LIDARSensor(robot) { Offset = new(0, 0.08f, 0.0f), NumRays = 3, StartAngle = MathF.PI / 4, AngleRange = 2 * MathF.PI / 4, MaxDistance = 5 };
                lidar.Setup(room);
                positionSensor.Setup(room);

                //Adds N number of Waypoints to the list
                //These change location on every reset, whereas positions of robots only change on the creation of a new room
                waypoints.Add(new WaypointForGate(room, () =>
                {
                    return new List<Vector3>() {
                        rng.PointOnCircle(2.0f, 0, new Vector3(0, 0, 6.0f))
                    };
                }, robot.ID));

                //Adds N number of timers to the list
                timers.Add(new());
            }

            //Ensures that the gate only opens if the robots hit the triggers within 2 seconds of one another
            room.OnUpdate += (e, dt) =>
            {
                bool complete = true;

                //Checks whether any of the timers are running, so it only starts the one that triggers its waypoint first
                for (int i = 0; i < _numBots; i++)
                {
                    bool start = true;
                    for (int j = 0; j < _numBots; j++)
                    {
                        if (timers[j].IsRunning)
                        {
                            start = false;
                        }
                    }
                    if (start && waypoints[i].buttonPressed())
                    {
                        timers[i].Start();
                    }
                }

                //Checks that all the waypoints have been triggered within 2 seconds of the first
                for (int i = 0; i < _numBots; i++)
                {
                    if (!(waypoints[i].buttonPressed() && timers[i].ElapsedSeconds < 2))
                    {
                        complete = false;
                    }
                    //*Complete is false for the second(final) waypoint*
                }

                //Gate is removed and timers are reset, as long as prior conditions are met
                if (complete)
                {
                    gate.Position = new Vector3(0, -1.0f, 0);

                    for (int i = 0; i < _numBots; i++)
                    {
                        timers[i].Reset();
                    }
                }
            };

            //When room is reset, gate will return to its initial position.
            room.OnReset += (o, e) =>
            {
                gate.Position = new Vector3(0, 0, 9.0f);
            };
        }
    }
}