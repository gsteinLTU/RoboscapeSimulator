using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments
{
    class LIDARRoadEnvironment : EnvironmentConfiguration
    {
        public LIDARRoadEnvironment()
        {
            Name = "LIDAR Road";
            ID = "lidarroad";
            Description = "Robot with LIDAR drives down a road";
        }

        public override object Clone()
        {
            return new LIDARRoadEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up lidar test environment");

            // Ground
            var ground = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Demo robot
            var robot = new ParallaxRobot(room, new(1, 0.2f, 1), Quaternion.Identity);
            var lidar = new LIDARSensor(robot) { Offset = new(0, 0.1f, 0.07f), NumRays = 16, MinAngle = MathF.PI / 4, MaxAngle = 3f * MathF.PI / 4 };
            lidar.Setup(room);

            // Start and end areas
            var start = new Cube(room, 5, 0.01f, 1, new(0, 0.005f, -5), Quaternion.CreateFromYawPitchRoll(0, 0.05f, 0), isKinematic: true, visualInfo: new VisualInfo() { Color = "#D22" });
            var end = new Cube(room, 5, 0.01f, 1, new(0, 0.005f, 5), Quaternion.CreateFromYawPitchRoll(0, -0.05f, 0), isKinematic: true, visualInfo: new VisualInfo() { Color = "#2D2" });

            AddPath(room, new()
            {
                new(0, 0, 0),
                new(0, 0, 5),
                new(5, 0, 5),
                new(5, 0, 0),
                new(0, 0, 0),
            });

            static void AddPath(Room room, List<Vector3> points, float thickness = 0.1f, float height = 0.5f)
            {
                Vector3 previous = points[0];
                foreach (var point in points)
                {
                    AddWall(room, previous, point, thickness, height);
                    previous = point;
                }
            }

            static void AddWall(Room room, Vector3 p1, Vector3 p2, float thickness = 0.1f, float height = 0.5f)
            {
                var length = (p2 - p1).Length();

                if (length < 0.0001)
                {
                    return;
                }

                var center = p1 + ((p2 - p1) * 0.5f);
                var angle = MathF.Atan2(p2.Z - p1.Z, p2.X - p1.X);
                var wall1 = new Cube(room, length, height, thickness, center, Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle), true, visualInfo: new VisualInfo() { Color = "#633" });
            }

        }
    }
}