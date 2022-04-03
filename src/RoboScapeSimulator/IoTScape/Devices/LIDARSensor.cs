using System.Numerics;
using BepuPhysics;
using BepuUtilities.Memory;
using RoboScapeSimulator.Entities.Robots;

namespace RoboScapeSimulator.IoTScape.Devices
{
    /// <summary>
    /// An IoTScape sensor that performs multiple range tests centered on a body
    /// </summary>
    class LIDARSensor : IoTScapeObject
    {
        /// <summary>
        /// Offset from center of body to perform tests at
        /// </summary>
        public Vector3 Offset = new(0, 0.1f, 0);

        /// <summary>
        /// Furthest distance from the center point to 
        /// </summary>
        public float MaxDistance = 3;

        /// <summary>
        /// To prevent self-collisions, a minimum range for each ray
        /// </summary>
        public float MinDistance = 0.05f;

        /// <summary>
        /// A set of constants for converting output to different units easily
        /// </summary>
        public struct Conversions
        {
            public const float TO_CENTIMETERS = 100f;
            public const float TO_METERS = 1f;
            public const float TO_FEET = 3.28084f;
            public const float TO_INCHES = 39.37008f;
        }

        /// <summary>
        /// Amount to multiply distance by to change units from meters
        /// </summary>
        public float OutputMulitplier = Conversions.TO_CENTIMETERS;

        /// <summary>
        /// Number of rays this LIDARSensor "emits"
        /// </summary>
        public uint NumRays = 1;

        static readonly IoTScapeServiceDefinition definition = definition = new IoTScapeServiceDefinition(
            "LIDARSensor",
            new IoTScapeServiceDescription() { version = "1" },
            "",
            new Dictionary<string, IoTScapeMethodDescription>()
            {
                {"getRange", new IoTScapeMethodDescription(){
                    documentation = "Get list of distances around the sensor",
                    paramsList = new List<IoTScapeMethodParams>(){},
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "number",
                        "number"
                    }}
                }}
            },
            new Dictionary<string, IoTScapeEventDescription>());


        /// <summary>
        /// Create LIDARSensor attached to a Robot
        /// </summary>
        /// <param name="robot">Robot to attach sensor to</param>
        public LIDARSensor(Robot robot) : this(robot.BodyReference, robot.simulation, robot.ID) { }

        /// <summary>
        /// Create a LIDARSensor tracking a BodyReference
        /// </summary>
        /// <param name="trackedBody">Body to track position/heading of</param>
        /// <param name="id">ID to assign sensor</param>
        public LIDARSensor(BodyReference trackedBody, Simulation simulation, string? id = null) : base(definition, id)
        {
            unsafe
            {
                Methods["getRange"] = (string[] args) =>
                {
                    List<float> ranges = new();

                    simulation.BufferPool.Take(1, out Buffer<RayHit> results);

                    int intersectionCount = 0;
                    HitHandler hitHandler = new()
                    {
                        Hits = results,
                        IntersectionCount = &intersectionCount
                    };

                    float distance;

                    Vector3 axis = Vector3.Transform(Vector3.UnitY, trackedBody.Pose.Orientation);

                    for (int i = 0; i < NumRays; i++)
                    {
                        distance = MaxDistance;

                        Vector3 direction = Vector3.Transform(Vector3.Transform(Vector3.UnitZ, Quaternion.CreateFromAxisAngle(axis, 2f * MathF.PI / NumRays * i)), trackedBody.Pose.Orientation);
                        simulation.RayCast(trackedBody.Pose.Position + Vector3.Transform(Offset, trackedBody.Pose.Orientation) + direction * MinDistance,
                                           direction, MaxDistance, ref hitHandler);

                        if (intersectionCount > 0)
                        {
                            distance = Math.Min(hitHandler.Hits[0].T + MinDistance, MaxDistance);
                        }

                        ranges.Add(distance * OutputMulitplier);
                    }


                    return ranges.Select(range => range.ToString()).ToArray();
                };
            }
        }
    }
}