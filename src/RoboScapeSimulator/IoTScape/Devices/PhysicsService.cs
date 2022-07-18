using System.Numerics;
using BepuPhysics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;

namespace RoboScapeSimulator.IoTScape.Devices
{
    /// <summary>
    /// An IoTScape service that enables some physics controls
    /// </summary>
    class PhysicsService : IoTScapeObject
    {
        static readonly IoTScapeServiceDefinition definition = definition = new IoTScapeServiceDefinition(
            "PhysicsService",
            new IoTScapeServiceDescription() { version = "1" },
            "",
            new Dictionary<string, IoTScapeMethodDescription>()
            {
                {"getPosition", new IoTScapeMethodDescription(){
                    documentation = "Get X, Y, and Z coordinates as a list",
                    paramsList = new List<IoTScapeMethodParams>(){},
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "number",
                        "number",
                        "number"
                    }}
                }},
                {"setPosition", new IoTScapeMethodDescription(){
                    documentation = "Move the object",
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
                }},
                {"applyForce", new IoTScapeMethodDescription(){
                    documentation = "Apply a force on the object",
                    paramsList = new List<IoTScapeMethodParams>(){
                        new IoTScapeMethodParams() {
                            documentation = "X component",
                            name = "x",
                            optional = false,
                            type = "number"
                        },
                        new IoTScapeMethodParams() {
                            documentation = "Y component",
                            name = "y",
                            optional = false,
                            type = "number"
                        },
                        new IoTScapeMethodParams() {
                            documentation = "Z component",
                            name = "z",
                            optional = false,
                            type = "number"
                        },
                    },
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "void"
                    }}
                }},
                {"setVelocity", new IoTScapeMethodDescription(){
                    documentation = "Set velocity of the object",
                    paramsList = new List<IoTScapeMethodParams>(){
                        new IoTScapeMethodParams() {
                            documentation = "X component",
                            name = "x",
                            optional = false,
                            type = "number"
                        },
                        new IoTScapeMethodParams() {
                            documentation = "Y component",
                            name = "y",
                            optional = false,
                            type = "number"
                        },
                        new IoTScapeMethodParams() {
                            documentation = "Z component",
                            name = "z",
                            optional = false,
                            type = "number"
                        },
                    },
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "void"
                    }}
                }},
                {"getMass", new IoTScapeMethodDescription(){
                    documentation = "Get mass of object",
                    paramsList = new List<IoTScapeMethodParams>(){},
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "number"
                    }}
                }},
                {"getOrientation", new IoTScapeMethodDescription(){
                    documentation = "Get pitch/yaw/roll in degrees",
                    paramsList = new List<IoTScapeMethodParams>(){},
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "number",
                        "number",
                        "number"
                    }}
                }}
            },
            new Dictionary<string, IoTScapeEventDescription>());


        /// <summary>
        /// Create PositionSensor attached to a Robot
        /// </summary>
        /// <param name="robot">Robot to attach sensor to</param>
        public PhysicsService(Robot robot) : this(robot, robot.ID) { }

        /// <summary>
        /// Create a PositionSensor tracking a BodyReference
        /// </summary>
        /// <param name="trackedBody">Body to track position/heading of</param>
        /// <param name="id">ID to assign sensor</param>
        public PhysicsService(BodyReference trackedBody, string? id = null) : base(definition, id)
        {
            Methods["getPosition"] = (string[] args) =>
            {
                return new string[] { trackedBody.Pose.Position.X.ToString(), trackedBody.Pose.Position.Y.ToString(), trackedBody.Pose.Position.Z.ToString() };
            };

            Methods["getOrientation"] = (string[] args) =>
            {
                trackedBody.Pose.Orientation.ExtractYawPitchRoll(out var yaw, out var pitch, out var roll);
                return new string[] { (pitch * 180.0f / MathF.PI).ToString(), (yaw * 180.0f / MathF.PI).ToString(), (roll * 180.0f / MathF.PI).ToString() };
            };

            Methods["getMass"] = (string[] args) =>
            {
                return new string[] { (1.0f / trackedBody.LocalInertia.InverseMass).ToString() };
            };

            Methods["applyForce"] = (string[] args) =>
            {
                trackedBody.Awake = true;
                trackedBody.ApplyLinearImpulse(new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2])));
                return Array.Empty<string>();
            };

            Methods["setVelocity"] = (string[] args) =>
            {
                trackedBody.Awake = true;
                trackedBody.Velocity.Linear = new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
                return Array.Empty<string>();
            };
        }

        /// <summary>
        /// Create a PositionSensor tracking an Entity
        /// </summary>
        /// <param name="trackedBody">Body to track position/heading of</param>
        /// <param name="id">ID to assign sensor</param>
        public PhysicsService(DynamicEntity trackedBody, string? id = null) : this(trackedBody.BodyReference, id) { }
    }
}