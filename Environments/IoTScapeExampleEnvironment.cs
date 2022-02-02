using System.Numerics;
using IoTScape;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;

class IoTScapeExampleEnvironment : EnvironmentConfiguration
{
    public IoTScapeExampleEnvironment()
    {
        Name = "IoTScapeExampleEnvironment";
        ID = "iotscape1";
        Description = "The demo environment";
    }
    IoTScapeObject? cubeObject;

    IoTScapeObject? locationSensor;

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



        IoTScapeServiceDefinition locationSensorService = new(
            "TransformSensor",
            new IoTScapeServiceDescription() { version = "1" },
            "",
            new Dictionary<string, IoTScapeMethodDescription>()
            {
                {"getX", new IoTScapeMethodDescription(){
                    documentation = "Get X coordinate",
                    paramsList = new List<IoTScapeMethodParams>(){},
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "number"
                    }}
                }},
                {"getY", new IoTScapeMethodDescription(){
                    documentation = "Get Y coordinate",
                    paramsList = new List<IoTScapeMethodParams>(){},
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "number"
                    }}
                }},
                {"getZ", new IoTScapeMethodDescription(){
                    documentation = "Get Z coordinate",
                    paramsList = new List<IoTScapeMethodParams>(){},
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "number"
                    }}
                }},
                {"getHeading", new IoTScapeMethodDescription(){
                    documentation = "Get heading angle in degrees",
                    paramsList = new List<IoTScapeMethodParams>(){},
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "number"
                    }}
                }}
            },
            new Dictionary<string, IoTScapeEventDescription>());

        locationSensor = new(locationSensorService, "");
        locationSensor.Methods["getX"] = (string[] args) =>
        {
            return new string[] { robot.BodyReference.Pose.Position.X.ToString() };
        };

        locationSensor.Methods["getY"] = (string[] args) =>
        {
            return new string[] { robot.BodyReference.Pose.Position.Y.ToString() };
        };

        locationSensor.Methods["getZ"] = (string[] args) =>
        {
            return new string[] { robot.BodyReference.Pose.Position.Z.ToString() };
        };

        locationSensor.Methods["getHeading"] = (string[] args) =>
        {
            robot.BodyReference.Pose.Orientation.ExtractYawPitchRoll(out var yaw, out var _, out var _);
            return new string[] { (yaw * 180.0f / MathF.PI).ToString() };
        };

        locationSensor.Setup(room);


    }
}