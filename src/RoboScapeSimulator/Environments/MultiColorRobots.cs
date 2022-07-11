using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments
{
    /// <summary>
    /// A configurable environment with multiple robots on a plane
    /// </summary>
    class MultiColorRobotsEnvironment : EnvironmentConfiguration
    {
        public const uint MaxRobots = 4;

        uint _robots = 4;
        bool _walls = false;
        bool _positionSensor = false;
        bool _LIDAR = false;
        public MultiColorRobotsEnvironment(uint numRobots = 4, bool walls = false, bool positionSensor = false, bool LIDAR = false)
        {
            if (numRobots > MaxRobots)
            {
                throw new Exception($"MultiColorRobotsEnvironment supports a maximum of {MaxRobots} robots");
            }

            if (numRobots == 0)
            {
                throw new Exception($"MultiColorRobotsEnvironment requires at least one robot");
            }

            var colors = new string[]{
                "Zero", "One", "Two", "Three", "Four"
            };

            Name = $"{colors[numRobots]} Robot{(numRobots > 1 ? "s" : "")}{(positionSensor ? " with PositionSensor" : "")}{(LIDAR ? " with LIDAR" : "")}{(walls ? " with Walls" : "")}";
            ID = $"{numRobots}color{(walls ? "_walls" : "")}{(positionSensor ? "_pos" : "")}{(LIDAR ? "_lidar" : "")}";
            Description = "Environment with four differently colored robots";
            Category = "Multi-Color Robots";

            _robots = numRobots;
            _walls = walls;
            _positionSensor = positionSensor;
            _LIDAR = LIDAR;
        }

        public override object Clone()
        {
            return new MultiColorRobotsEnvironment(_robots, _walls, _positionSensor, _LIDAR);
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up four color robots environment");

            // Ground
            _ = new Ground(room);

            // Walls
            if (_walls)
            {
                EnvironmentUtils.MakeWalls(room);
            }

            // Robots
            List<Robot> robots = new()
            {
                new ParallaxRobot(room, new Vector3(0, 0.25f, 1.25f), Quaternion.Identity, visualInfo: new() { ModelName = "car1_red.gltf" }, debug: false)
            };

            if (_robots > 1)
                robots.Add(new ParallaxRobot(room, new Vector3(1.25f, 0.25f, 0), Quaternion.Identity, visualInfo: new() { ModelName = "car1_green.gltf" }, debug: false));
            if (_robots > 2)
                robots.Add(new ParallaxRobot(room, new Vector3(0, 0.25f, -1.25f), Quaternion.Identity, visualInfo: new() { ModelName = "car1_blue.gltf" }, debug: false));
            if (_robots > 3)
                robots.Add(new ParallaxRobot(room, new Vector3(-1.25f, 0.25f, 0), Quaternion.Identity, visualInfo: new() { ModelName = "car1_purple.gltf" }, debug: false));

            // Sensors
            if (_positionSensor)
            {
                robots.ForEach(robot =>
                {
                    var positionSensor = new PositionSensor(robot);
                    positionSensor.Setup(room);
                });
            }

            if (_LIDAR)
            {
                robots.ForEach(robot =>
                {
                    var lidarSensor = new LIDARSensor(robot) { Offset = new(0, 0.08f, 0.0f), NumRays = 3, StartAngle = MathF.PI / 4, AngleRange = 2 * MathF.PI / 4, MaxDistance = 5 };
                    lidarSensor.Setup(room);
                });
            }
        }
    }
}