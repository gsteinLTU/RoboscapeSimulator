using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
namespace RoboScapeSimulator.Environments
{
    class WallEnvironment : EnvironmentConfiguration
    {
        public WallEnvironment()
        {
            Name = "Wall";
            ID = "wall";
            Description = "Wall and one robot";
        }

        public override object Clone()
        {
            return new WallEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up wall environment");

            // Ground
            _ = new Ground(room);

            // Walls
            float wallsize = 15;
            _ = new Cube(room, wallsize, 1, 0.1f, new Vector3(0, 0.5f, -wallsize / 2), Quaternion.Identity, true, nameOverride: "wall1", visualInfo: new VisualInfo() { Image = "bricks.png" });

            // Demo robots
            _ = new ParallaxRobot(room);
        }
    }
}