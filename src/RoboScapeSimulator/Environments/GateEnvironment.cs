using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments
{
    class GateEnvironment : EnvironmentConfiguration
    {
        //Creates a list for N timers
        readonly List<StopwatchLite> timers = new();
        //Creates a list for N waypoints
        readonly List<WaypointTest> waypoints = new();
        //Stores the type of course
        readonly private string _courseType;
        //Stores the number (N) of robots
        readonly uint _numBots;
        public GateEnvironment(string courseType = Courses.Default, uint numBots = 2)
        {
            Name = $"Gate Environment({courseType}, {numBots} Robots)";
            ID = "Gate" + courseType + " " + numBots;
            Description = "Two robots, two waypoints, and a gate";
            _courseType = courseType;
            _numBots = numBots;
        }

        public struct Courses
        {
            public const string Default = "Default";
            public const string Obstacles = "Obstacles";
        }

        public override object Clone()
        {
            return new GateEnvironment(_courseType, _numBots);
        }

        public override void Setup(Room room)
        {

            /* 
            GateEnvironment
            N number of robots and N number of waypoints will be initalized in a 8 by 9 rectangle with a
            gate at the end. Only if all robots trigger their respective waypoints within a 2 second window
            will the gate open.

            FIXME : All waypoints move to a set end location (in WaypointTest.cs) meant to act as the final checkpoint
            by going through the gate. Because any triggered waypoints move to that hardcoded location, the green squares
            are overlayed on the red Xs. When removing entities is possible, fix this. (Also as of now it seems that a 
            waypoint can only be assigned with one robot, but this approach should bypass that). 7-11-22
            */

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

                    //case Courses.Random:
                    //Robots are spawned in random locations within the enclosure

                    /* correct behaviour, but creates duplicates so there are four */
                    /*robot1 = new ParallaxRobot(room, rng.PointOnCircle(2.75f, 0.25f, new Vector3(0, 0, 2.0f)), Quaternion.Identity);
                    robot2 = new ParallaxRobot(room, rng.PointOnCircle(2.75f, 0.25f, new Vector3(0, 0, 2.0f)), Quaternion.Identity);

                    robot3 = new ParallaxRobot(room, new Vector3(0, 0.25f, 0), Quaternion.Identity);*/

                    /* robots freak out and bounce everywhere */
                    // robot1.Position = rng.PointOnCircle(2.75f, 0f, new Vector3(0, 0.25f, 2.0f));
                    // robot1._initialPosition = rng.PointOnCircle(2.75f, 0f, new Vector3(0, 0.25f, 2.0f));
                    // robot1._initialOrientation = Quaternion.Identity;
                    // robot2.Position = rng.PointOnCircle(2.75f, 0f, new Vector3(0, 0.25f, 2.0f));
                    // robot2._initialPosition = rng.PointOnCircle(2.75f, 0f, new Vector3(0, 0.25f, 2.0f));
                    //robot2._initialOrientation = Quaternion.Identity;
                    //break;

                    //case Courses.Default:
                    //default:

                    /*robot1 = new ParallaxRobot(room, new Vector3(1.0f, 0.25f, 0), Quaternion.Identity);
                    robot2 = new ParallaxRobot(room, new Vector3(-1.0f, 0.25f, 0), Quaternion.Identity);
                    robot3 = new ParallaxRobot(room, new Vector3(0, 0.25f, 0), Quaternion.Identity);*/

                    //break;

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
                //Note : These change location on every reset, whereas positions of robots only change on the creation of a new room
                waypoints.Add(new WaypointTest(room, () =>
                {
                    return new List<Vector3>() {
                        rng.PointOnCircle(2.25f, 0, new Vector3(0, 0, 6.0f))
                    };
                }, robot.ID));

                //Adds N number of timers to the list
                timers.Add(new());
            }

            //Creates two waypoints, one for each robot
            // WaypointTest waypoint1 = new WaypointTest(room, () =>
            // {
            //     return new List<Vector3>() {
            //         rng.PointOnLine(new(-3.5f, 0, 6.5f), new(-1.5f, 0, 6.5f)),
            //         rng.PointOnLine(new(-3.5f, 0, 7.0f), new(1.5f, 0, 7.0f)),
            //         rng.PointOnLine(new(-3.5f, 0, 7.75f), new(0, 0, 7.75f))

            //     };
            // }, /*() =>
            // {
            //     return new List<Vector3>() {
            //         rng.PointOnCircle(0, 0, new(0, 0, 10.0f)),
            //     };
            // },*/ robot2.ID);

            // WaypointTest waypoint2 = new WaypointTest(room, () =>
            // {
            //     return new List<Vector3>() {
            //         rng.PointOnLine(new(-1.5f, 0, 6.0f), new(3.5f, 0, 6.0f)),
            //         rng.PointOnLine(new(1.5f, 0, 7.25f), new(3.5f, 0, 7.25f)),
            //         rng.PointOnLine(new(0, 0, 8.0f), new(3.5f, 0, 8.0f))

            //     };
            // }, /*() =>
            // {
            //     return new List<Vector3>() {
            //         rng.PointOnCircle(0, 0, new(0, 0, 10.0f)),
            //     };
            // },*/ robot1.ID);

            // WaypointTest waypoint3 = new WaypointTest(room, () =>
            // {
            //     return new List<Vector3>() {
            //         rng.PointOnLine(new(-1.0f, 0, 8.0f), new(1.0f, 0, 8.0f))

            //     };
            // }, robot3.ID);

            //Final waypoint test
            /* WaypointTest final1 = new WaypointTest(room, () =>
            {
                return new List<Vector3>() {
                    rng.PointOnLine(new(0, 0, 10.0f), new(0, 0, 10.0f))

                };
            }, robot1.ID, robot2.ID); */

            //Initializes position and LIDAR sensors for the robots
            /*PositionSensor positionSensor1 = new(robot1);
            var lidar1 = new LIDARSensor(robot1) { Offset = new(0, 0.08f, 0.0f), NumRays = 3, StartAngle = MathF.PI / 4, AngleRange = 2 * MathF.PI / 4, MaxDistance = 5 };
            lidar1.Setup(room);
            positionSensor1.Setup(room);

            PositionSensor positionSensor2 = new(robot2);
            var lidar2 = new LIDARSensor(robot2) { Offset = new(0, 0.08f, 0.0f), NumRays = 3, StartAngle = MathF.PI / 4, AngleRange = 2 * MathF.PI / 4, MaxDistance = 5 };
            lidar2.Setup(room);
            positionSensor2.Setup(room);

            PositionSensor positionSensor3 = new(robot3);
            var lidar3 = new LIDARSensor(robot3) { Offset = new(0, 0.08f, 0.0f), NumRays = 3, StartAngle = MathF.PI / 4, AngleRange = 2 * MathF.PI / 4, MaxDistance = 5 };
            lidar3.Setup(room);
            positionSensor3.Setup(room);*/


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
                }

                /*if (timer1.IsRunning == false && timer2.IsRunning == false)
                {
                    if (waypoint1.buttonPressed())
                    {
                        timer1.Start();
                    }
                    if (waypoint2.buttonPressed())
                    {
                        timer2.Start();
                    }
                }*/

                //Gate is removed and timers are reset, as long as prior conditions are met
                if (complete)
                {
                    gate.Position = new Vector3(0, -1.0f, 0);

                    for (int i = 0; i < _numBots; i++)
                    {
                        timers[i].Reset();
                    }
                }

                /*if (waypoint1.buttonPressed() && waypoint2.buttonPressed() && timer1.ElapsedSeconds < 2 && timer2.ElapsedSeconds < 2)
                {
                    gate.Position = new Vector3(0, -1.0f, 0);
                    timer1.Reset();
                    timer2.Reset();
                }*/
            };

            //When room is reset, gate will return to its initial position.
            room.OnReset += (o, e) =>
            {
                gate.Position = new Vector3(0, 0, 9.0f);
            };
        }
    }
}