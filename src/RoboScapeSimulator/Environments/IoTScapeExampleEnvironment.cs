using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape;
using RoboScapeSimulator.IoTScape.Devices;
using RoboScapeSimulator.Physics;
using RoboScapeSimulator.Physics.Bepu;

namespace RoboScapeSimulator.Environments
{
    class IoTScapeExampleEnvironment : EnvironmentConfiguration
    {
        public IoTScapeExampleEnvironment()
        {
            Name = "IoTScapeExampleEnvironment";
            ID = "iotscape1";
            Description = "The demo environment";
        }
        IoTScapeObject? cubeObject;

        PositionSensor? locationSensor;

        public override object Clone()
        {
            return new IoTScapeExampleEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up IoTScape Example environment");

            if(room.SimInstance is BepuSimulationInstance bSim){ 
                bSim.IntegratorCallbacks.LinearDamping = 0;
            }

            // Ground
            var ground = new Ground(room);

            // Walls
            EnvironmentUtils.MakeWalls(room);

            // Demo robot
            var robot = new ParallaxRobot(room, debug: false);

            var cube = new Cube(room, visualInfo: new VisualInfo() { Color = "#B85" }, isKinematic: true);

            IoTScapeServiceDefinition exampleService = new(
                "MoveCube",
                new IoTScapeServiceDescription() { version = "1" },
                "",
                new Dictionary<string, IoTScapeMethodDescription>()
                {
                {"move", new IoTScapeMethodDescription(){
                    documentation = "Move the cube",
                    paramsList = new List<IoTScapeMethodParams>(){
                        new IoTScapeMethodParams() {
                            documentation = "X coordinate",
                            name = "x",
                            optional = false,
                            type = "number"
                        },
                        new IoTScapeMethodParams() {
                            documentation = "Y coordinate",
                            name = "y",
                            optional = false,
                            type = "number"
                        },
                        new IoTScapeMethodParams() {
                            documentation = "Z coordinate",
                            name = "z",
                            optional = false,
                            type = "number"
                        },
                    },
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "void"
                    }}
                }}
                },
                new Dictionary<string, IoTScapeEventDescription>());

            cubeObject = new(exampleService, "");
            cubeObject.Methods["move"] = (string[] args) =>
            {
                float x = 0, y = 0, z = 0;
                if (args.Length > 0)
                {
                    x = float.Parse(args[0]);
                }

                if (args.Length > 1)
                {
                    y = float.Parse(args[1]);
                }

                if (args.Length > 2)
                {
                    z = float.Parse(args[2]);
                }

                Debug.WriteLine($"Moving to {x}, {y}, {z}");
                cube.Position = new Vector3(x, y, z);

                return Array.Empty<string>();
            };

            cubeObject.Setup(room);

            locationSensor = new(robot.BodyReference, "")
            {
                IDOverride = robot.ID
            };
            locationSensor.Setup(room);
        }
    }
}