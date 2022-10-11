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
            0.175f, 0.025f, 0.175f, 0.5f);

        _initialPosition = Position;
        _initialOrientation = Orientation;

        room.SimInstance.Entities.Add(this);
        DroneService droneService = new(this);
        droneService.Setup(room);
    }

    public readonly int NumMotors = 4;

    public float[] MotorSpeeds = { 0, 0, 0, 0 };
    public float[] MotorSpeedTargets = { 0, 0, 0, 0 };

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
        Array.Fill(MotorSpeedTargets, 0);
    }

    float k_M = 1.5e-9f;
    float k_F = 6.11e-8f;
    float k_m = 20;

    Matrix4x4 I = Utils.MakeMatrix3x3(  2.32e-3f, 0, 0,
                                        0, 4e-3f, 0,
                                        0, 0, 2.32e-3f);

    public override void Update(float dt)
    {
        base.Update(dt);

        // Update motor speeds
        for (int i = 0; i < MotorSpeeds.Length; i++)
        {
            MotorSpeeds[i] += dt * k_m * (MotorSpeedTargets[i] - MotorSpeeds[i]);
        }

        float motorForce = MotorSpeeds.Sum(speed => speed * speed * k_F);
        Vector3 updateLinearForce = (1.0f / BodyReference.Mass) * Vector3.Transform(new Vector3(0, motorForce, 0),BodyReference.Orientation);

        Vector3 updateAngularForce = new();

        BodyReference.LinearVelocity += dt * updateLinearForce;
        BodyReference.AngularVelocity += dt * updateAngularForce;
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
                    drone.MotorSpeedTargets = args.Select(arg => float.Parse(arg)).ToArray();
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