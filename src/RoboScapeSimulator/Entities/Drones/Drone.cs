using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.IoTScape;

namespace RoboScapeSimulator.Entities.Drones;

class Drone : DynamicEntity, IResettable
{
    /// <summary>
    /// Reference to the room this drone is inside of
    /// </summary>
    internal Room room;

    /// <summary>
    /// Position where drone was created
    /// </summary>
    internal Vector3 _initialPosition;

    /// <summary>
    /// Orientation where drone was created
    /// </summary>
    internal Quaternion _initialOrientation;

    static internal int id = 0;

    public Drone(Room room, in Vector3? position = null, in Quaternion? rotation = null, in VisualInfo? visualInfo = null, float spawnHeight = 0.2f)
    {
        this.room = room;
        var rng = new Random();

        VisualInfo = visualInfo ?? new VisualInfo() {};

        Name = "drone" + id++;

        BodyReference = room.SimInstance.CreateBox(Name,
            position ?? new Vector3(rng.Next(-5, 5), spawnHeight, rng.Next(-5, 5)),
            rotation ?? Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)rng.NextDouble() * MathF.PI),
            0.1f, 0.1f, 0.1f, 1);

        _initialPosition = Position;
        _initialOrientation = Orientation;

        room.SimInstance.Entities.Add(this);
    }

    public readonly int NumMotors = 4;

    public float[] MotorSpeeds = { 0, 0, 0, 0 };

    public event EventHandler? OnReset;

    public void Reset()
    {
        // Reset position and speed
        BodyReference.Position = _initialPosition;
        BodyReference.Orientation = _initialOrientation;
        BodyReference.AngularVelocity = new Vector3();
        BodyReference.LinearVelocity = new Vector3();

        // Set all speeds to 0
        Array.Fill(MotorSpeeds, 0);
    }

    class DroneService : IoTScapeObject
    {
        static readonly IoTScapeServiceDefinition definition = new("Drone",
            new IoTScapeServiceDescription()
            {
                description = "A service to control a UAV",
                version = "1"
            },
            "",
            new Dictionary<string, IoTScapeMethodDescription>(){
                {"setMotorSpeeds", new IoTScapeMethodDescription(){
                    documentation = "Set the target speeds of the motors directly",
                    paramsList = new List<IoTScapeMethodParams>(){
                        new IoTScapeMethodParams(){
                            name = "m1",
                            type = "number",
                            documentation = "Speed for motor 1"
                        },
                        new IoTScapeMethodParams(){
                            name = "m2",
                            type = "number",
                            documentation = "Speed for motor 2"
                        },
                        new IoTScapeMethodParams(){
                            name = "m3",
                            type = "number",
                            documentation = "Speed for motor 3"
                        },
                        new IoTScapeMethodParams(){
                            name = "m4",
                            type = "number",
                            documentation = "Speed for motor 4"
                        },
                    },
                    returns = new IoTScapeMethodReturns(){
                        type = new List<string>(){"void"}
                    }
                }}
            },
            new Dictionary<string, IoTScapeEventDescription>());

        public DroneService(Drone drone) : base(definition, drone.Name)
        {
            Methods["setMotorSpeeds"] = (string[] args) =>
            {
                if (args.Length != drone.NumMotors)
                {
                    return Array.Empty<string>();
                }

                try
                {
                    drone.MotorSpeeds = args.Select(arg => float.Parse(arg)).ToArray();
                }
                catch (Exception)
                {
                    Trace.WriteLine("Error parsing motor speeds");
                }
                return Array.Empty<string>();
            };
        }
    }
}