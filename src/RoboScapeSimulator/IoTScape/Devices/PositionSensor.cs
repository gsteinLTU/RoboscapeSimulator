using BepuPhysics;

namespace RoboScapeSimulator.IoTScape.Devices
{
    class PositionSensor : IoTScapeObject
    {
        static readonly IoTScapeServiceDefinition definition = definition = new IoTScapeServiceDefinition(
            "PositionSensor",
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
                {"getPosition", new IoTScapeMethodDescription(){
                    documentation = "Get X, Y, and Z coordinates as a list",
                    paramsList = new List<IoTScapeMethodParams>(){},
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "number",
                        "number",
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

        public PositionSensor(BodyReference trackedBody, string? id = null) : base(definition, id)
        {
            Methods["getX"] = (string[] args) =>
            {
                return new string[] { trackedBody.Pose.Position.X.ToString() };
            };

            Methods["getY"] = (string[] args) =>
            {
                return new string[] { trackedBody.Pose.Position.Y.ToString() };
            };

            Methods["getZ"] = (string[] args) =>
            {
                return new string[] { trackedBody.Pose.Position.Z.ToString() };
            };

            Methods["getPosition"] = (string[] args) =>
            {
                return new string[] { trackedBody.Pose.Position.X.ToString(), trackedBody.Pose.Position.Y.ToString(), trackedBody.Pose.Position.Z.ToString() };
            };

            Methods["getHeading"] = (string[] args) =>
            {
                trackedBody.Pose.Orientation.ExtractYawPitchRoll(out var yaw, out var _, out var _);
                return new string[] { (yaw * 180.0f / MathF.PI).ToString() };
            };
        }
    }
}