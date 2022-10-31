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