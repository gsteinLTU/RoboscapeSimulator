using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
namespace RoboScapeSimulator.Environments
{
    class FourColorRobotsEnvironment : EnvironmentConfiguration
    {
        public FourColorRobotsEnvironment()
        {
            Name = "Four Color Robots";
            ID = "4color";
            Description = "Environment with four differently colored robots";
        }

        public override object Clone()
        {
            return new FourColorRobotsEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up four color robots environment");

            // Ground
            var ground = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Robots
            var robot1 = new ParallaxRobot(room, new Vector3(0, 0.25f, 1.25f), Quaternion.Identity, visualInfo: new() { ModelName = "car1_red.gltf" }, debug: false);
            var robot2 = new ParallaxRobot(room, new Vector3(1.25f, 0.25f, 0), Quaternion.Identity, visualInfo: new() { ModelName = "car1_green.gltf" }, debug: false);
            var robot3 = new ParallaxRobot(room, new Vector3(0, 0.25f, -1.25f), Quaternion.Identity, visualInfo: new() { ModelName = "car1_blue.gltf" }, debug: false);
            var robot4 = new ParallaxRobot(room, new Vector3(-1.25f, 0.25f, 0), Quaternion.Identity, visualInfo: new() { ModelName = "car1_purple.gltf" }, debug: false);
        }
    }
}