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
        readonly StopwatchLite timer1 = new();
        readonly StopwatchLite timer2 = new();
        readonly private string _courseType;
        public GateEnvironment(string courseType = Courses.Default)
        {
            Name = $"Gate Environment({courseType})";
            ID = "Gate" + courseType;
            Description = "Two robots, two waypoints, and a gate";
            _courseType = courseType;
        }

        public struct Courses
        {
            public const string Default = "Default";
            public const string Random = "Random";
            public const string Obstacles = "Obstacles";
        }

        public override object Clone()
        {
            return new GateEnvironment(_courseType);
        }

        public override void Setup(Room room)
        {

            /*
            - Two robots
            - Two waypoints/buttons/triggers
            - One final waypoint
            - One walled in rectangled with an opening for a movable/vanishing gate

            Goal is for each robot to go to one of the waypoints. When one robot gets near but not touches, it will send
            a 'Ready!' signal to the other. Only when both robots are ready can they press the buttons to open the gate,
            allowing them to pass through and reach the final waypoint. 
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
            var gate = new Cube(room, 1.8f, 0.25f, 0.15f, new Vector3(0, 0, 9.0f), Quaternion.Identity);
            gate.BodyReference.Awake = true;
            gate.forceUpdate = true;

            Random rng = new();

            //Spawns two robots at set locations within the walls
            var robot1 = new ParallaxRobot(room, new Vector3(1.0f, 0.25f, 0), Quaternion.Identity);
            var robot2 = new ParallaxRobot(room, new Vector3(-1.0f, 0.25f, 0), Quaternion.Identity);

            switch (_courseType)
            {
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

                case Courses.Random:
                    //Robots are spawned in random locations within the enclosure

                    /* correct behaviour, but creates duplicates so there are four */
                    //robot1 = new ParallaxRobot(room, rng.PointOnCircle(2.75f, 0.25f, new Vector3(0, 0, 2.0f)), Quaternion.Identity);
                    //robot2 = new ParallaxRobot(room, rng.PointOnCircle(2.75f, 0.25f, new Vector3(0, 0, 2.0f)), Quaternion.Identity);

                    /* robots freak out and bounce everywhere */
                    robot1._initialPosition = rng.PointOnCircle(2.75f, 0.25f, new Vector3(0, 0.25f, 2.0f));
                    //robot1._initialOrientation = Quaternion.Identity;
                    robot2._initialPosition = rng.PointOnCircle(2.75f, 0.25f, new Vector3(0, 0.25f, 2.0f));
                    //robot2._initialOrientation = Quaternion.Identity;
                    break;

                case Courses.Default:
                default:
                    //Should spawn two robots at set locations within the walls
                    //robot1 = new ParallaxRobot(room, new Vector3(1.0f, 0.25f, 0), Quaternion.Identity);
                    //robot1._initialPosition = new Vector3(1.0f, 0.25f, 0);
                    //robot1._initialOrientation = Quaternion.Identity;
                    //robot2 = new ParallaxRobot(room, new Vector3(-1.0f, 0.25f, 0), Quaternion.Identity);
                    //robot2._initialPosition = new Vector3(-1.0f, 0.25f, 0);
                    //robot2._initialOrientation = Quaternion.Identity;
                    break;

            }

            //Creates two waypoints, one for each robot
            WaypointTest waypoint1 = new WaypointTest(room, () =>
            {
                return new List<Vector3>() {
                    rng.PointOnLine(new(-3.5f, 0, 6.5f), new(-1.5f, 0, 6.5f)),
                    rng.PointOnLine(new(-3.5f, 0, 7.0f), new(1.5f, 0, 7.0f)),
                    rng.PointOnLine(new(-3.5f, 0, 7.75f), new(0, 0, 7.75f))

                };
            }, /*() =>
            {
                return new List<Vector3>() {
                    rng.PointOnCircle(0, 0, new(0, 0, 10.0f)),
                };
            },*/ robot2.ID);

            WaypointTest waypoint2 = new WaypointTest(room, () =>
            {
                return new List<Vector3>() {
                    rng.PointOnLine(new(-1.5f, 0, 6.0f), new(3.5f, 0, 6.0f)),
                    rng.PointOnLine(new(1.5f, 0, 7.25f), new(3.5f, 0, 7.25f)),
                    rng.PointOnLine(new(-0, 0, 8.0f), new(3.5f, 0, 8.0f))

                };
            }, /*() =>
            {
                return new List<Vector3>() {
                    rng.PointOnCircle(0, 0, new(0, 0, 10.0f)),
                };
            },*/ robot1.ID);

            //Final waypoint test
            WaypointTest final1 = new WaypointTest(room, () =>
            {
                return new List<Vector3>() {
                    rng.PointOnLine(new(0, 0, 10.0f), new(0, 0, 10.0f))

                };
            }, robot1.ID, robot2.ID);

            /*WaypointTest final2 = new WaypointTest(room, () =>
            {
                return new List<Vector3>() {
                    rng.PointOnLine(new(0, 0, 10.0f), new(0, 0, 10.0f))

                };
            }, robot2.ID);*/

            //Initializes position and LIDAR sensors for the robots
            PositionSensor positionSensor1 = new(robot1);
            var lidar1 = new LIDARSensor(robot1) { Offset = new(0, 0.08f, 0.0f), NumRays = 3, StartAngle = MathF.PI / 4, AngleRange = 2 * MathF.PI / 4, MaxDistance = 5 };
            lidar1.Setup(room);
            positionSensor1.Setup(room);

            PositionSensor positionSensor2 = new(robot2);
            var lidar2 = new LIDARSensor(robot2) { Offset = new(0, 0.08f, 0.0f), NumRays = 3, StartAngle = MathF.PI / 4, AngleRange = 2 * MathF.PI / 4, MaxDistance = 5 };
            lidar2.Setup(room);
            positionSensor2.Setup(room);

            //Ensures that the gate only opens if the robots hit the triggers within two seconds of one another
            room.OnUpdate += (e, dt) =>
            {
                if (timer1.IsRunning == false && timer2.IsRunning == false)
                {
                    if (waypoint1.buttonPressed())
                    {
                        timer1.Start();
                    }
                    if (waypoint2.buttonPressed())
                    {
                        timer2.Start();
                    }
                }
                if (waypoint1.buttonPressed() && waypoint2.buttonPressed() && timer1.ElapsedSeconds < 2 && timer2.ElapsedSeconds < 2)
                {
                    gate.Position = new Vector3(0, -1.0f, 0);
                    timer1.Reset();
                    timer2.Reset();
                }
            };

            room.OnReset += (o, e) =>
            {
                //var gate = new Cube(room, 1.8f, 0.25f, 0.15f, new Vector3(0, 0, 9.0f), Quaternion.Identity);
                gate.Position = new Vector3(0, 0, 9.0f);
                //gate.BodyReference.Awake = true;
                //gate.forceUpdate = true;
            };
        }
    }
}