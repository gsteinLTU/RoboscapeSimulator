using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
namespace RoboScapeSimulator.Environments
{
    class DemoEnvironment : EnvironmentConfiguration
    {
        public DemoEnvironment()
        {
            Name = "Demo 2021";
            ID = "demo";
            Description = "The demo environment";
        }

        public override object Clone()
        {
            return new DemoEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up demo 2021 environment");

            // Ground
            _ = new Ground(room);

            // Walls
            float wallsize = 15;
            _ = new Cube(room, wallsize, 1, 1, new Vector3(0, 0.5f, -wallsize / 2), Quaternion.Identity, true, nameOverride: "wall1");
            _ = new Cube(room, wallsize, 1, 1, new Vector3(0, 0.5f, wallsize / 2), Quaternion.Identity, true, nameOverride: "wall2");
            _ = new Cube(room, 1, 1, wallsize + 1, new Vector3(-wallsize / 2, 0.5f, 0), Quaternion.Identity, true, nameOverride: "wall3");
            _ = new Cube(room, 1, 1, wallsize + 1, new Vector3(wallsize / 2, 0.5f, 0), Quaternion.Identity, true, nameOverride: "wall4");

            // Demo robots
            _ = new ParallaxRobot(room);
            _ = new ParallaxRobot(room);

            for (int i = 0; i < 3; i++)
            {
                _ = new Cube(room, visualInfo: new VisualInfo() { Color = "#B85" });
            }
        }
    }
}