using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;

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
            Trace.WriteLine($"Setting up {Name} environment");

            StopwatchTimer sw = new(room);

            // Ground
            _ = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Walls
            float wallX = 4f;
            float wallZ = 7f;

            // Outer walls
            _ = new Cube(room, wallX, 0.7f, 0.1f, new Vector3(0, 0.35f, -wallZ / 2), Quaternion.Identity, true, nameOverride: "outer_wall1", visualInfo: new VisualInfo() { Image = "bricks.png" });
            _ = new Cube(room, wallX, 0.7f, 0.1f, new Vector3(0, 0.35f, wallZ / 2), Quaternion.Identity, true, nameOverride: "outer_wall2", visualInfo: new VisualInfo() { Image = "bricks.png" });
            _ = new Cube(room, wallZ, 0.7f, 0.1f, new Vector3(-wallX / 2, 0.35f, 0), Quaternion.CreateFromYawPitchRoll(MathF.PI / 2, 0, 0), true, nameOverride: "outer_wall3", visualInfo: new VisualInfo() { Image = "bricks.png" });
            _ = new Cube(room, wallZ, 0.7f, 0.1f, new Vector3(wallX / 2, 0.35f, 0), Quaternion.CreateFromYawPitchRoll(MathF.PI / 2, 0, 0), true, nameOverride: "outer_wall4", visualInfo: new VisualInfo() { Image = "bricks.png" });

            // Start and end areas
            _ = new Cube(room, wallX, 0.01f, 1, new(0, 0.005f, -wallZ / 2 + 0.5f), Quaternion.CreateFromYawPitchRoll(0, 0.05f, 0), isKinematic: true, visualInfo: new VisualInfo() { Color = "#D22" });
            var end = new Cube(room, wallX, 0.01f, 1, new(0, 0.005f, wallZ / 2 - 0.5f), Quaternion.CreateFromYawPitchRoll(0, -0.05f, 0), isKinematic: true, visualInfo: new VisualInfo() { Color = "#2D2" });
            var endTrigger = new Trigger(room, end.Position + new Vector3(0, 0, 0.3f), end.Orientation, end.Width, 2, end.Depth);

            endTrigger.OnTriggerEnter += (trigger, ent) =>
            {
                if (ent is Robot r)
                {
                    sw.Stop();
                }
            };

            // Inner walls
            AddObstacleWall(room, wallX, wallZ, 1.5f, 1f, 1.5f);
            AddObstacleWall(room, wallX, wallZ, 2.5f, 1f, 2.5f);
            AddObstacleWall(room, wallX, wallZ, 0.25f, 1f, 3.75f);
            AddObstacleWall(room, wallX, wallZ, wallX / 2 - 0.2f, 0.4f, 5f);

            // Block
            _ = new Cube(room, 0.5f, 0.5f, 0.5f, new(0, 0.55f, -wallZ / 2 + 4.7f), Quaternion.Identity);

            // Robot
            _ = new ParallaxRobot(room, new(0, 0.15f, -wallZ / 2 + 0.5f), Quaternion.Identity);

            static void AddObstacleWall(Room room, float wallX, float wallZ, float leftSize, float gapSize, float zPos)
            {
                _ = new Cube(room, leftSize, 0.5f, 0.1f, new Vector3(-wallX / 2 + leftSize / 2, 0.25f, -wallZ / 2 + zPos), Quaternion.Identity, true, nameOverride: "walll", visualInfo: new VisualInfo() { Color = "#633" });
                _ = new Cube(room, wallX - leftSize - gapSize, 0.5f, 0.1f, new Vector3(wallX / 2 - (wallX - leftSize - gapSize) / 2, 0.25f, -wallZ / 2 + zPos), Quaternion.Identity, true, nameOverride: "wallr", visualInfo: new VisualInfo() { Color = "#633" });
            }

            sw.Start();
        }
    }
}