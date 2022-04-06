using System.Diagnostics;
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
            Trace.WriteLine("Setting up PositionSensor Demo environment");

            // Ground
            _ = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Walls
            float wallsize = 15;
            _ = new Cube(room, wallsize, 1, 1, new Vector3(0, 0.5f, -wallsize / 2), Quaternion.Identity, true, nameOverride: "wall1");
            _ = new Cube(room, wallsize, 1, 1, new Vector3(0, 0.5f, wallsize / 2), Quaternion.Identity, true, nameOverride: "wall2");
            _ = new Cube(room, 1, 1, wallsize + 1, new Vector3(-wallsize / 2, 0.5f, 0), Quaternion.Identity, true, nameOverride: "wall3");
            _ = new Cube(room, 1, 1, wallsize + 1, new Vector3(wallsize / 2, 0.5f, 0), Quaternion.Identity, true, nameOverride: "wall4");

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