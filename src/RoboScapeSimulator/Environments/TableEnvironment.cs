using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
namespace RoboScapeSimulator.Environments
{
    class TableEnvironment : EnvironmentConfiguration
    {
        uint _boxes = 2;
        uint _robots = 1;

        public TableEnvironment(uint boxes = 2, uint robots = 1)
        {
            Name = $"Table With {boxes} Boxes and {robots} robots";
            ID = $"table_{boxes}b{robots}r";
            Description = "The table environment";
            _boxes = boxes;
            _robots = robots;
        }

        public override object Clone()
        {
            return new TableEnvironment(_boxes, _robots);
        }

        public override void Setup(Room room)
        {
            Console.WriteLine("Setting up table environment");

            // Ground
            var ground = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Table
            var table = new Cube(room, 10.5f, 1, 10.5f, new Vector3(0, 0.5f, 0), Quaternion.Identity, true, nameOverride: "table");

            // Demo robots
            for (int i = 0; i < _robots; i++)
            {
                var robot = new ParallaxRobot(room, spawnHeight: 1.25f);
            }

            for (int i = 0; i < _boxes; i++)
            {
                var cube = new Cube(room, visualInfo: new VisualInfo() { Color = "#B85" });
            }
        }
    }
}