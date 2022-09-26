using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments
{
    class RobotInterceptEnvironment : EnvironmentConfiguration
    {
        private readonly string _difficulty;

        public struct Difficulties
        {
            public const string Easy = "Easy";
            public const string Hard = "Hard";
        }

        public RobotInterceptEnvironment(string difficulty = Difficulties.Easy)
        {
            _difficulty = difficulty;
            Name = $"Robot Intercept ({difficulty})";
            ID = $"robotintercept{difficulty}";
            Description = "One robot has to intercept another";
            //Category = "§_Testing";
        }

        public override object Clone()
        {
            return new RobotInterceptEnvironment(_difficulty);
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine($"Setting up {Name} environment");

            // Ground
            _ = new Ground(room);

            // Main robot
            var robot = new ParallaxRobot(room, new(0, 0.25f, 0), Quaternion.Identity, debug: false);

            // Target robot
            float targetSpeed = _difficulty == Difficulties.Hard ? 90 : 50;

            StopwatchTimer timer = new(room);

            var rng = new Random();

            Vector3 initialPosition, robotTarget;
            (initialPosition, robotTarget) = robotGen(rng);

            var targetRobot = new ParallaxRobot(room, initialPosition, Quaternion.Identity, debug: false, udpClientOverride: null);
            var startMarker = new VisualOnlyEntity(room, initialPosition: initialPosition, initialOrientation: Quaternion.Identity, width: 0.1f, height: 0.025f, depth: 0.1f, visualInfo: new VisualInfo() { Color = "#363" });
            var endMarker = new VisualOnlyEntity(room, initialPosition: robotTarget, initialOrientation: Quaternion.Identity, width: 0.1f, height: 0.025f, depth: 0.1f, visualInfo: new VisualInfo() { Color = "#633" });

            var trigger = new Trigger(room, initialPosition, Quaternion.Identity, 0.15f, 0.15f, 0.15f);

            trigger.OnTriggerEnter += (trigger, ent) =>
            {
                if (ent is Robot r)
                {
                    if (r.ID == robot.ID)
                    {
                        targetRobot.ResetSpeed();
                    }
                }
            };

            PositionSensor s1 = new(robot);
            PositionSensor s2 = new(targetRobot);

            s1.Setup(room);
            s2.Setup(room);

            bool resetting = false;

            room.OnUpdate += (room, dt) =>
            {
                if (resetting)
                {
                    resetting = false;
                    return;
                }

                // Stop if at destination
                if (targetRobot.Speed.Left != 0)
                {
                    trigger.Position = targetRobot.Position;

                    // Update heading
                    targetRobot.Orientation.ExtractYawPitchRoll(out var yaw, out var _, out var _);
                    if (MathF.Abs(yaw - getHeading(targetRobot.Position, robotTarget)) > MathF.PI / 10f)
                    {
                        targetRobot.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, getHeading(targetRobot.Position, robotTarget));
                    }

                    // Stop on target
                    var distSqr = (targetRobot.Position - robotTarget).LengthSquared();
                    if (distSqr < 0.0025f)
                    {
                        targetRobot.ResetSpeed();
                    }
                }
            };

            room.OnReset += (room, _) =>
            {
                // New start and finish
                (initialPosition, robotTarget) = robotGen(rng);

                targetRobot._initialPosition = initialPosition;
                targetRobot._initialOrientation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, getHeading(initialPosition, robotTarget));
                startMarker.Position = initialPosition;
                endMarker.Position = robotTarget;

                targetRobot.Reset();

                targetRobot.SetSpeed(targetSpeed, targetSpeed);
                resetting = true;
            };

            static (Vector3, Vector3) robotGen(Random rng)
            {
                Vector3 initialPosition = rng.PointOnLine(new(1.5f, 0.075f, 2.5f), new(3, 0.075f, 2));
                Vector3 robotTarget = rng.PointOnLine(new(2, 0.05f, -4), new(4, 0.05f, -4));
                return (initialPosition, robotTarget);
            }

            static float getHeading(Vector3 p1, Vector3 p2)
            {
                return MathF.Atan2(p2.X - p1.X, p2.Z - p1.Z);
            }
        }
    }
}