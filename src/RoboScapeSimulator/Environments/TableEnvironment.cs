using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments
{
    class TableEnvironment : EnvironmentConfiguration
    {
        readonly uint _boxes = 2;
        readonly uint _robots = 1;
        readonly bool _lidar = false;

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
            _ = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Table
            _ = new Cube(room, 5.5f, 1, 5.5f, new Vector3(0, 0.5f, 0), Quaternion.Identity, true, nameOverride: "table");

            // Demo robots
            Random rng = new();
            for (int i = 0; i < _robots; i++)
            {
                var robot = new ParallaxRobot(room, rng.PointOnCircle(1, 1.75f));

                PositionSensor locationSensor = new(robot);
                locationSensor.Setup(room);

                if (_lidar)
                {
                    var lidar = new LIDARSensor(robot) { Offset = new(0, 0.25f, 0.07f), NumRays = 15, StartAngle = MathF.PI / 2, AngleRange = MathF.PI, MaxDistance = 5 };
                    lidar.Setup(room);
                }
            }

            // Cubes
            for (int i = 0; i < _boxes; i++)
            {
                _ = new Cube(room, 0.5f, 0.5f, 0.5f, initialPosition: rng.PointOnCircle(2, 0.75f), visualInfo: new VisualInfo() { Color = "#B85" });
            }
        }
    }
}