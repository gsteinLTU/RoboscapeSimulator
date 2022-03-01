using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.IoTScape.Devices;
namespace RoboScapeSimulator.Environments
{
    class PositionSensorDemo : EnvironmentConfiguration
    {

        uint _robots = 1;

        public PositionSensorDemo(uint robots = 1)
        {

            Name = $"PositionSensor Demo With {robots} robot(s)";
            ID = $"possensor_{robots}r";
            Description = "PositionSensor environment";
            _robots = robots;
        }

        public override object Clone()
        {
            return new PositionSensorDemo(_robots);
        }

        public override void Setup(Room room)
        {
            Console.WriteLine("Setting up PositionSensor Demo environment");

            // Ground
            var ground = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Walls
            float wallsize = 15;
            var wall1 = new Cube(room, wallsize, 1, 1, new Vector3(0, 0.5f, -wallsize / 2), Quaternion.Identity, true, nameOverride: "wall1");
            var wall2 = new Cube(room, wallsize, 1, 1, new Vector3(0, 0.5f, wallsize / 2), Quaternion.Identity, true, nameOverride: "wall2");
            var wall3 = new Cube(room, 1, 1, wallsize + 1, new Vector3(-wallsize / 2, 0.5f, 0), Quaternion.Identity, true, nameOverride: "wall3");
            var wall4 = new Cube(room, 1, 1, wallsize + 1, new Vector3(wallsize / 2, 0.5f, 0), Quaternion.Identity, true, nameOverride: "wall4");

            // Demo robots
            for (int i = 0; i < _robots; i++)
            {
                var robot = new ParallaxRobot(room);

                PositionSensor locationSensor = new(robot);
                locationSensor.Setup(room);
            }
        }
    }
}