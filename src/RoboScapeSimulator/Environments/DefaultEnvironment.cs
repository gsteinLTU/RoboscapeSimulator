using System.Diagnostics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
namespace RoboScapeSimulator.Environments
{
    class DefaultEnvironment : EnvironmentConfiguration
    {
        public DefaultEnvironment()
        {
            Name = "Default";
            ID = "default";
            Description = "The default environment";
        }

        public override object Clone()
        {
            return new DefaultEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up default environment");

            // Ground
            _ = new Ground(room);

            Random rng = new();

            // Demo robot
            _ = new ParallaxRobot(room, rng.PointOnCircle(1, 0.25f), debug: false);

            for (int i = 0; i < 3; i++)
            {
                _ = new Cube(room, initialPosition: rng.PointOnCircle(1.5f + 0.5f * i, 0.5f), visualInfo: new VisualInfo() { Image = "crate.png" });
            }
        }
    }
}