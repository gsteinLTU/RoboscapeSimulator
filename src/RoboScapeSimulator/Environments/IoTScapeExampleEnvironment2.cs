using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape;
using RoboScapeSimulator.IoTScape.Devices;
namespace RoboScapeSimulator.Environments
{
    class IoTScapeExampleEnvironment2 : EnvironmentConfiguration
    {
        public IoTScapeExampleEnvironment2()
        {
            Name = "IoTScapeExampleEnvironment2";
            ID = "iotscape2";
            Description = "RadiationSensor Demo Environment";
        }

        IoTScapeObject? radiationSensor;

        PositionSensor? locationSensor;

        public override object Clone()
        {
            return new IoTScapeExampleEnvironment2();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up IoTScape Example 2 environment");

            // Ground
            var ground = new Ground(room);

            // Walls
            EnvironmentUtils.MakeWalls(room);

            // Demo robot 
            var robot = new ParallaxRobot(room, debug: false);

            var barrel = new Cube(room, initialPosition: new Vector3(3, 0, 3), initialOrientation: Quaternion.Identity, visualInfo: new VisualInfo() { ModelName = "barrel.gltf", ModelScale = 0.4f }, isKinematic: true);

            locationSensor = new(robot);
            locationSensor.Setup(room);

            IoTScapeServiceDefinition radiationSensorDefinition = new(
                "RadiationSensor",
                new IoTScapeServiceDescription() { version = "1" },
                "",
                new Dictionary<string, IoTScapeMethodDescription>()
                {
                {"getIntensity", new IoTScapeMethodDescription(){
                    documentation = "Get radiation reading at current location",
                    paramsList = new List<IoTScapeMethodParams>(){},
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "number"
                    }}
                }}
                },
                new Dictionary<string, IoTScapeEventDescription>());

            var sensorBody = robot.BodyReference;
            var targetBody = barrel.BodyReference;
            var targetIntensity = 100;

            radiationSensor = new(radiationSensorDefinition, robot.ID);
            radiationSensor.Methods["getIntensity"] = (string[] args) =>
            {
                float intensity = 0;
                float distance = (sensorBody.Pose.Position - targetBody.Pose.Position).LengthSquared();
                intensity = targetIntensity / distance;

                return new string[] { intensity.ToString() };
            };

            radiationSensor.Setup(room);

        }
    }
}