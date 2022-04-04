using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments
{
    class TableEnvironment : EnvironmentConfiguration
    {
        uint _boxes = 2;
        uint _robots = 1;
        bool _lidar = false;

        public TableEnvironment(uint boxes = 2, uint robots = 1, bool lidar = false)
        {
            Name = $"Table With {boxes} Boxes and {robots} robots {(lidar ? "with lidar" : "")}";
            ID = $"table_{boxes}b{robots}r{(lidar ? "_lidar" : "")}";
            Description = "The table environment";
            _boxes = boxes;
            _robots = robots;
            _lidar = lidar;
        }

        public override object Clone()
        {
            return new TableEnvironment(_boxes, _robots, _lidar);
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine($"Setting up {Name} environment");

            // Ground
            var ground = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Table
            var table = new Cube(room, 5.5f, 1, 5.5f, new Vector3(0, 0.5f, 0), Quaternion.Identity, true, nameOverride: "table");

            // Demo robots
            Random rng = new Random();
            for (int i = 0; i < _robots; i++)
            {
                var robot = new ParallaxRobot(room, rng.PointOnCircle(1, 1.75f));

                PositionSensor locationSensor = new(robot);
                locationSensor.Setup(room);

                if (_lidar)
                {
                    var lidar = new LIDARSensor(robot) { Offset = new(0, 0.25f, 0.07f), NumRays = 15, MinAngle = MathF.PI / 2, MaxAngle = 3 * MathF.PI / 2, MaxDistance = 5 };
                    lidar.Setup(room);
                }
            }

            for (int i = 0; i < _boxes; i++)
            {
                var cube = new Cube(room, 0.5f, 0.5f, 0.5f, initialPosition: rng.PointOnCircle(2, 0.75f), visualInfo: new VisualInfo() { Color = "#B85" });
            }
        }
    }
}