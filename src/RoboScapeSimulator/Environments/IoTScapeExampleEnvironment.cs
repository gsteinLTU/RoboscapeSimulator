using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.IoTScape;
using RoboScapeSimulator.IoTScape.Devices;

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
            Console.WriteLine("Setting up IoTScape Example environment");

            // Ground
            var ground = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Walls
            float wallsize = 15;
            var wall1 = new Cube(room, wallsize, 1, 1, new Vector3(0, 0.5f, -wallsize / 2), Quaternion.Identity, true, nameOverride: "wall1");
            var wall2 = new Cube(room, wallsize, 1, 1, new Vector3(0, 0.5f, wallsize / 2), Quaternion.Identity, true, nameOverride: "wall2");
            var wall3 = new Cube(room, 1, 1, wallsize + 1, new Vector3(-wallsize / 2, 0.5f, 0), Quaternion.Identity, true, nameOverride: "wall3");
            var wall4 = new Cube(room, 1, 1, wallsize + 1, new Vector3(wallsize / 2, 0.5f, 0), Quaternion.Identity, true, nameOverride: "wall4");

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

                Console.WriteLine($"Moving to {x}, {y}, {z}");
                cube.BodyReference.Pose.Position = new Vector3(x, y, z);
                cube.BodyReference.Awake = true;

                return Array.Empty<string>();
            };

            cubeObject.Setup(room);

            locationSensor = new(robot.BodyReference, "");
            locationSensor.IDOverride = robot.ID;
            locationSensor.Setup(room);
        }
    }
}