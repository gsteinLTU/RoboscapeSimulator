using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
namespace RoboScapeSimulator.Environments
{
    class ObstacleCourseEnvironment : EnvironmentConfiguration
    {
        public ObstacleCourseEnvironment()
        {
            Name = "Obstacle Course";
            ID = "obstaclecourse1";
            Description = "Obstacle course and one robot";
        }

        public override object Clone()
        {
            return new ObstacleCourseEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine($"Setting up {this.Name} environment");

            // Ground
            var ground = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Walls
            float wallX = 7.5f;
            float wallZ = 15;

            // Outer walls
            var wall1 = new Cube(room, wallX, 1, 0.1f, new Vector3(0, 0.5f, -wallZ / 2), Quaternion.Identity, true, nameOverride: "wall1", visualInfo: new VisualInfo() { Image = "bricks.png" });
            var wall2 = new Cube(room, wallX, 1, 0.1f, new Vector3(0, 0.5f, wallZ / 2), Quaternion.Identity, true, nameOverride: "wall2", visualInfo: new VisualInfo() { Image = "bricks.png" });
            var wall3 = new Cube(room, wallZ, 1, 0.1f, new Vector3(-wallX / 2, 0.5f, 0), Quaternion.CreateFromYawPitchRoll(MathF.PI / 2, 0, 0), true, nameOverride: "wall3", visualInfo: new VisualInfo() { Image = "bricks.png" });
            var wall4 = new Cube(room, wallZ, 1, 0.1f, new Vector3(wallX / 2, 0.5f, 0), Quaternion.CreateFromYawPitchRoll(MathF.PI / 2, 0, 0), true, nameOverride: "wall4", visualInfo: new VisualInfo() { Image = "bricks.png" });

            // Robot
            var robot = new ParallaxRobot(room, new(0, 0.15f, -wallZ / 2 + 1));
        }
    }
}