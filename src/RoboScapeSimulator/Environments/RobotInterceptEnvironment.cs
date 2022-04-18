using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
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
        }

        public override object Clone()
        {
            return new RobotInterceptEnvironment(_difficulty);
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine($"Setting up {Name} environment");

            // Ground
            _ = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Main robot
            var robot = new ParallaxRobot(room, new(0, 0.25f, 0), Quaternion.Identity, debug: false);

            // Target robot
            float targetSpeed = _difficulty == Difficulties.Hard ? 100 : 50;

            var rng = new Random();
            var targetRobot = new ParallaxRobot(room, rng.PointOnCircle(1.5f, 0.25f), Quaternion.Identity, debug: false, internalUse: true);

            PositionSensor s1 = new(robot);
            PositionSensor s2 = new(targetRobot);

            s1.Setup(room);
            s2.Setup(room);

            room.OnUpdate += (room, dt) =>
            {

            };
        }
    }
}