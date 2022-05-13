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

            // Demo robot
            _ = new ParallaxRobot(room, debug: false);

            for (int i = 0; i < 3; i++)
            {
                _ = new Cube(room, visualInfo: new VisualInfo() { Image = "crate.png" });
            }
        }
    }
}