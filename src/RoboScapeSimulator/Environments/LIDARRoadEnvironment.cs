using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments
{
    class LIDARRoadEnvironment : EnvironmentConfiguration
    {
        readonly private string _courseType;

        public LIDARRoadEnvironment(string courseType = Courses.Easy)
        {
            Name = $"LIDAR Road ({courseType})";
            ID = "lidarroad" + courseType;
            Description = $"Robot with LIDAR drives down a {courseType} road";
            _courseType = courseType;
        }

        public struct Courses
        {
            public const string Easy = "Easy";
            public const string Hard = "Hard";
            public const string VeryHard = "VeryHard";
        }

        public override object Clone()
        {
            return new LIDARRoadEnvironment(_courseType);
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine($"Setting up {Name} environment");

            // Ground
            _ = new Ground(room);

            float endPosZ;
            float endPosX;

            switch (_courseType)
            {
                case Courses.Hard:
                    // left
                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(-0.5f, 0, 0),
                        new(-0.5f, 0, 1.75f),
                        new(1.5f, 0, 2.25f),
                        new(1.5f, 0, 2.75f),
                        new(-2.5f, 0, 3.5f),
                        new(-2.5f, 0, 5.25f),
                        new(1.5f, 0, 6.25f),
                        new(1.5f, 0, 8f),
                    });

                    // right
                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(0.5f, 0, 0),
                        new(0.5f, 0, 0.75f),
                        new(2.5f, 0, 1.5f),
                        new(2.5f, 0, 3.5f),
                        new(-1.5f, 0, 4.25f),
                        new(-1.5f, 0, 4.5f),
                        new(2.5f, 0, 5.5f),
                        new(2.5f, 0, 8f),
                    });

                    // start area
                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(-0.5f, 0, 0),
                        new(-0.5f, 0, -0.5f),
                        new(0.5f, 0, -0.5f),
                        new(0.5f, 0, 0f),
                    });

                    endPosZ = 8;
                    endPosX = 2;

                    break;
                case Courses.VeryHard:
                    // left
                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(-0.5f, 0, 0),
                        new(-0.5f, 0, 1f),
                        new(0f, 0, 2f),
                        new(0f, 0, 2.25f),
                        new(1.25f, 0, 3f),
                        new(2.5f, 0, 2.75f),
                        new(3.5f, 0, 2.75f),
                        new(3.5f, 0, 1f),
                        new(4f, 0, 0f),
                        new(3.75f, 0, -1f),
                        new(3.25f, 0, -2f),
                        new(1.5f, 0, -2.25f),
                        new(-1.25f, 0, -2.25f),
                        new(-2.25f, 0, -1f),
                        new(-2.5f, 0, 0.25f),
                        new(-2.5f, 0, 1f),
                    });

                    // right
                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(0.5f, 0, 0),
                        new(0.5f, 0, 1f),
                        new(1.5f, 0, 2f),
                        new(1.5f, 0, 2.25f),
                        new(2.5f, 0, 2f),
                        new(2.5f, 0, 1f),
                        new(3f, 0, 0f),
                        new(2.75f, 0, -1f),
                        new(1.5f, 0, -1.25f),
                        new(-1.25f, 0, -1.25f),
                        new(-1.5f, 0, -0.75f),
                        new(-1.5f, 0, 1f),
                    });

                    // Spikes
                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(0.5f, 0f, -1.25f),
                        new(0f, 0f, -1.75f),
                        new(-0.5f, 0f, -1.25f),
                    });

                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(0.5f, 0f, -2.25f),
                        new(1f, 0f, -1.75f),
                        new(1.5f, 0f, -2.25f),
                    });

                    // start area
                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(-0.5f, 0, 0),
                        new(-0.5f, 0, -0.5f),
                        new(0.5f, 0, -0.5f),
                        new(0.5f, 0, 0f),
                    });

                    endPosZ = 0.5f;
                    endPosX = -2f;

                    break;
                case Courses.Easy:
                default:

                    // left
                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(-0.5f, 0, 0),
                        new(-0.5f, 0, 1),
                        new(-1.25f, 0, 2.25f),
                        new(-1f, 0, 2.75f),
                        new(0.25f, 0, 4.75f),
                        new(0.25f, 0, 5.25f),
                        new(-0.5f, 0, 7f),
                        new(-0.5f, 0, 7.5f),
                    });

                    // right
                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(0.5f, 0, 0),
                        new(0.5f, 0, 1),
                        new(-0.25f, 0, 2.25f),
                        new(0f, 0, 2.75f),
                        new(1.25f, 0, 4.75f),
                        new(1.25f, 0, 5.25f),
                        new(0.5f, 0, 7f),
                        new(0.5f, 0, 7.5f),
                    });

                    // start area
                    EnvironmentUtils.AddPath(room, new()
                    {
                        new(-0.5f, 0, 0),
                        new(-0.5f, 0, -0.5f),
                        new(0.5f, 0, -0.5f),
                        new(0.5f, 0, 0f),
                    });

                    endPosZ = 8;
                    endPosX = 0;

                    break;
            }

            // Start and end areas 
            _ = new Cube(room, 1, 0.01f, 1.1f, new(0, 0.0025f, 0f), Quaternion.CreateFromYawPitchRoll(0, 0.005f, 0), isKinematic: true, visualInfo: new VisualInfo() { Color = "#D22" });
            _ = new Cube(room, 1, 0.01f, 1.1f, new(endPosX, 0.0025f, endPosZ), Quaternion.CreateFromYawPitchRoll(0, -0.005f, 0), isKinematic: true, visualInfo: new VisualInfo() { Color = "#2D2" });

            // Demo robot
            var robot = new ParallaxRobot(room, new(0, 0.25f, 0), Quaternion.Identity);
            var lidar = new LIDARSensor(robot) { Offset = new(0, 0.08f, 0.0f), NumRays = 3, StartAngle = MathF.PI / 4, AngleRange = 2 * MathF.PI / 4, MaxDistance = 5 };
            lidar.Setup(room);
        }
    }
}