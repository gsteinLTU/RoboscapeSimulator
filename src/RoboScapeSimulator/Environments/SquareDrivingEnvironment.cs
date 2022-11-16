using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.RobotScape;
namespace RoboScapeSimulator.Environments
{
    class SquareDrivingEnvironment : EnvironmentConfiguration
    {
        public SquareDrivingEnvironment()
        {
            Name = "Square Driving";
            ID = "squaredriving";
            Description = "The Square Driving environment";
        }

        public override object Clone()
        {
            return new SquareDrivingEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up square driving environment");

            // Ground
            _ = new Ground(room);

            // Demo robot
            _ = new ParallaxRobot(room, new Vector3(0, 0.25f, 0), Quaternion.Identity, debug: false);

            // Cube
            float size = 1.5f;
            _ = new Cube(room, size, size / 2.0f, size, new Vector3(-size / 2f - 0.25f, size / 4f + 0.25f, size / 2f), Quaternion.Identity);
        }
    }
}