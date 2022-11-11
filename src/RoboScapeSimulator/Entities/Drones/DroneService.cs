using System.Diagnostics;
using RoboScapeSimulator.IoTScape;

namespace RoboScapeSimulator.Entities.Drones;

/// <summary>
/// Service for controlling Drone entities
/// </summary>
internal class DroneService : IoTScapeObject
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
                }},
                {"setTargetAngles", new IoTScapeMethodDescription(){
                    documentation = "Set the target angles for the drone",
                    paramsList = new List<IoTScapeMethodParams>(){
                        new IoTScapeMethodParams(){
                            name = "pitch",
                            type = "number",
                            documentation = "Pitch (x-axis)"
                        },
                        new IoTScapeMethodParams(){
                            name = "yaw",
                            type = "number",
                            documentation = "Yaw (Y-axis)"
                        },
                        new IoTScapeMethodParams(){
                            name = "roll",
                            type = "number",
                            documentation = "Roll (z-axis)"
                        },
                    },
                    returns = new IoTScapeMethodReturns(){
                        type = new List<string>(){"void"}
                    }
                }},
                {"setTargetSpeed", new IoTScapeMethodDescription(){
                    documentation = "Set the target speed for the drone",
                    paramsList = new List<IoTScapeMethodParams>(){
                        new IoTScapeMethodParams(){
                            name = "speed",
                            type = "number",
                            documentation = "Desired speed (m/s)"
                        },
                    },
                    returns = new IoTScapeMethodReturns(){
                        type = new List<string>(){"void"}
                    }
                }},
                {"setTargetVelocity", new IoTScapeMethodDescription(){
                    documentation = "Set the target speeds for the drone",
                    paramsList = new List<IoTScapeMethodParams>(){
                        new IoTScapeMethodParams(){
                            name = "x",
                            type = "number",
                            documentation = "X speed"
                        },
                        new IoTScapeMethodParams(){
                            name = "y",
                            type = "number",
                            documentation = "Y speed"
                        },
                        new IoTScapeMethodParams(){
                            name = "z",
                            type = "number",
                            documentation = "Z speed"
                        },
                    },
                    returns = new IoTScapeMethodReturns(){
                        type = new List<string>(){"void"}
                    }
                }},
                {"setTargetHeight", new IoTScapeMethodDescription(){
                    documentation = "Set the target height for the drone",
                    paramsList = new List<IoTScapeMethodParams>(){
                        new IoTScapeMethodParams(){
                            name = "height",
                            type = "number",
                            documentation = "Desired height"
                        },
                    },
                    returns = new IoTScapeMethodReturns(){
                        type = new List<string>(){"void"}
                    }
                }},
                {"setSpeedXZ", new IoTScapeMethodDescription(){
                    documentation = "Set the target X and Z speeds for the drone",
                    paramsList = new List<IoTScapeMethodParams>(){
                        new IoTScapeMethodParams(){
                            name = "x",
                            type = "number",
                            documentation = "X speed"
                        },
                        new IoTScapeMethodParams(){
                            name = "z",
                            type = "number",
                            documentation = "Z speed"
                        },
                    },
                    returns = new IoTScapeMethodReturns(){
                        type = new List<string>(){"void"}
                    }
                }},
                {"goTo", new IoTScapeMethodDescription(){
                    documentation = "Set the target position for the drone",
                    paramsList = new List<IoTScapeMethodParams>(){
                        new IoTScapeMethodParams(){
                            name = "x",
                            type = "number",
                            documentation = "X position"
                        },
                        new IoTScapeMethodParams(){
                            name = "y",
                            type = "number",
                            documentation = "Y position"
                        },
                        new IoTScapeMethodParams(){
                            name = "z",
                            type = "number",
                            documentation = "Z position"
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

            drone.DriveState = Drone.DroneDriveState.SetMotorSpeed;

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

        Methods["setTargetAngles"] = (string[] args) =>
        {
            drone.DriveState = Drone.DroneDriveState.SetAnglesAndSpeed;

            return Array.Empty<string>();
        };

        Methods["setTargetSpeed"] = (string[] args) =>
        {
            drone.DriveState = Drone.DroneDriveState.SetAnglesAndSpeed;

            return Array.Empty<string>();
        };

        Methods["setTargetVelocity"] = (string[] args) =>
        {
            drone.DriveState = Drone.DroneDriveState.SetTargetVelocity;

            return Array.Empty<string>();
        };

        Methods["setTargetHeight"] = (string[] args) =>
        {
            drone.DriveState = Drone.DroneDriveState.SetTargetVelocityXZ;

            return Array.Empty<string>();
        };

        Methods["setSpeedXZ"] = (string[] args) =>
        {
            drone.DriveState = Drone.DroneDriveState.SetTargetVelocityXZ;

            return Array.Empty<string>();
        };

        Methods["goTo"] = (string[] args) =>
        {
            drone.DriveState = Drone.DroneDriveState.GoToCoords;

            return Array.Empty<string>();
        };
    }
}