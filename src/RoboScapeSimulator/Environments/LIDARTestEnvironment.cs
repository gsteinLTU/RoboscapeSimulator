using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments
{
    class LIDARTestEnvironment : EnvironmentConfiguration
    {
        public LIDARTestEnvironment()
        {
            Name = "LIDAR test";
            ID = "lidartest";
            Description = "Robot with LIDAR";
        }

        public override object Clone()
        {
            return new LIDARTestEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up lidar test environment");

            // Ground
            var ground = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Wall
            float wallsize = 15;
            var wall = new Cube(room, wallsize, 1, 0.1f, new Vector3(0, 0.5f, 2f), Quaternion.Identity, true, nameOverride: "wall1", visualInfo: new VisualInfo() { Image = "bricks.png" });

            // Demo robot
            var robot = new ParallaxRobot(room, new(1, 0.2f, 1), Quaternion.Identity);
            var lidar = new LIDARSensor(robot) { Offset = new(0, 0.1f, 0.07f), NumRays = 16, MinAngle = MathF.PI / 4, MaxAngle = 3f * MathF.PI / 4 };
            lidar.Setup(room);
        }
    }
}