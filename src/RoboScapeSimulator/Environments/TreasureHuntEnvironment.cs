using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape;
using RoboScapeSimulator.IoTScape.Devices;
namespace RoboScapeSimulator.Environments
{
    internal class TreasureHuntEnvironment : EnvironmentConfiguration
    {
        public TreasureHuntEnvironment()
        {
            Name = "Treasure Hunt";
            ID = "treasurehunt";
            Description = "treasureSensor Demo Environment";
        }

        private IoTScapeObject? treasureSensor;
        private PositionSensor? locationSensor;

        public override object Clone()
        {
            return new TreasureHuntEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine($"Setting up {Name} environment");

            // Ground
            Ground ground = new(room);

            // Walls
            EnvironmentUtils.MakeWalls(room);

            // Demo robot 
            ParallaxRobot robot = new(room, new Vector3(0, 0.25f, 0), Quaternion.Identity);

            Cube chest = new(room, 0.5f, 0.5f, 0.5f, initialPosition: new Vector3(1, -1.5f, 1), initialOrientation: Quaternion.Identity, visualInfo: new VisualInfo() { ModelName = "chest_opt.glb", ModelScale = 0.4f }, isKinematic: true);
            Trigger chestTrigger = new(room, chest.Position + new Vector3(0, 2f, 0), Quaternion.Identity, 0.5f, 1f, 0.5f, oneTime: true);

            chestTrigger.OnTriggerEnter += (o, e) =>
            {
                if (e == robot)
                {
                    chest.Position += new Vector3(0, 1.5f, 0);
                }
            };

            locationSensor = new(robot);
            locationSensor.Setup(room);

            IoTScapeServiceDefinition treasureSensorDefinition = new(
                "MetalDetector",
                new IoTScapeServiceDescription() { version = "1" },
                "",
                new Dictionary<string, IoTScapeMethodDescription>()
                {
                {"getIntensity", new IoTScapeMethodDescription(){
                    documentation = "Get metal detector reading at current location",
                    paramsList = new List<IoTScapeMethodParams>(){},
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "number"
                    }}
                }}
                },
                new Dictionary<string, IoTScapeEventDescription>());

            var sensorBody = robot.BodyReference;
            var targetBody = chest.BodyReference;
            int targetIntensity = 100;

            treasureSensor = new(treasureSensorDefinition, robot.ID);
            treasureSensor.Methods["getIntensity"] = (string[] args) =>
            {
                float intensity = 0;
                float distance = (sensorBody.Pose.Position - targetBody.Pose.Position).LengthSquared();
                intensity = targetIntensity / distance;

                return new string[] { intensity.ToString() };
            };

            treasureSensor.Setup(room);

            room.OnReset += (r, e) =>
            {
                chestTrigger.Reset();
            };
        }
    }
}