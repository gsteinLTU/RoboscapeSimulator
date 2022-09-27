using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.RobotScape;
using RoboScapeSimulator.IoTScape;

namespace RoboScapeSimulator.Environments.Helpers;

/// <summary>
/// A class representing Waypoints specifically for the GateEnvironment class.
/// Instead of randomly generating new Waypoints upon trigger, the Waypoints are moved to a set location.
/// This can be altered using the 'move' function.
/// </summary>

internal class WaypointForGate
{
    //An empty list to hold checkpoints
    readonly List<VisualOnlyEntity> Markers = new();
    //A Trigger object for the robot to set off
    readonly Trigger waypoint_X;
    //Two rectangles that make the crosses of the X to visualize the Trigger
    readonly VisualOnlyEntity waypoint_X_1;
    readonly VisualOnlyEntity waypoint_X_2;
    //Position of the waypoint Trigger
    Vector3 waypoint = new();
    //When the waypoint is triggered, pressed equals true
    bool pressed = false;

    public WaypointForGate(Room room, Func<List<Vector3>> waypointGenerator, string id, float threshold = 0.2f)
    {
        waypoint = waypointGenerator()[0];

        //Creates the X for a single waypoint
        //#633 is dark red
        waypoint_X = new Trigger(room, waypoint, Quaternion.Identity, threshold, 0.1f, threshold);
        waypoint_X_1 = new VisualOnlyEntity(room, initialPosition: waypoint_X.Position, initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, 45), width: 0.1f, height: 0.05f, depth: 0.5f, visualInfo: new VisualInfo() { Color = "#633" });
        waypoint_X_2 = new VisualOnlyEntity(room, initialPosition: waypoint_X.Position, initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, -45), width: 0.1f, height: 0.05f, depth: 0.5f, visualInfo: new VisualInfo() { Color = "#633" });
        Markers.Add(new VisualOnlyEntity(room, initialPosition: new Vector3(0, -1.0f, 0), initialOrientation: Quaternion.Identity, width: 0.25f, height: 0.1f, depth: 0.25f, visualInfo: new VisualInfo() { Color = "#363" }));

        //When the waypoint is triggered, the green checkpoint will move from its negative postion to the waypoint
        //The waypoint will then move out of sight into the negative position
        waypoint_X.OnTriggerStay += (o, e) =>
        {
            if (e is Robot r)
            {
                if (r.ID == id)
                {
                    var greenSquare = Markers[0];
                    greenSquare.Position = waypoint_X.Position;

                    //the waypoint moves to its 'final' location after being triggered once.
                    //After being triggered twice it moves out of sight.
                    if (waypoint_X.Position == new Vector3(0, 0, 10.0f))
                    {
                        waypoint_X.Position = new Vector3(0, -1.0f, 0);
                        waypoint_X_1.Position = new Vector3(0, -1.0f, 0);
                        waypoint_X_2.Position = new Vector3(0, -1.0f, 0);
                    }
                    else
                    {
                        waypoint_X.Position = new Vector3(0, 0, 10.0f);
                        waypoint_X_1.Position = new Vector3(0, 0, 10.0f);
                        waypoint_X_2.Position = new Vector3(0, 0, 10.0f);
                    }

                    pressed = true;
                }
            }
        };

        //Changes pressed to false 3 seconds after the robot leaves the trigger in order to give time to other functions.
        waypoint_X.OnTriggerExit += async (o, e) =>
        {
            if (e is Robot r)
            {
                if (r.ID == id)
                {
                    await Task.Delay(3000);
                    pressed = false;
                }
            }
        };

        //IoTScape setup 
        IoTScapeServiceDefinition waypointServiceDefinition = new(
            "WaypointList",
            new IoTScapeServiceDescription() { version = "1" },
            "",
            new Dictionary<string, IoTScapeMethodDescription>()
            {
                {"getNextWaypoint", new IoTScapeMethodDescription(){
                    documentation = "Get the next waypoint to navigate to",
                    paramsList = new List<IoTScapeMethodParams>(),
                    returns = new IoTScapeMethodReturns(){type = new List<string>(){
                        "number","number","number"
                    }}
                }}
            },
        new Dictionary<string, IoTScapeEventDescription>());

        IoTScapeObject waypointService = new(waypointServiceDefinition, id);

        waypointService.Methods["getNextWaypoint"] = (string[] args) =>
        {
            return new string[] { waypoint_X.Position.X.ToString(), waypoint_X.Position.Y.ToString(), waypoint_X.Position.Z.ToString() };
        };

        waypointService.Setup(room);

        //When the room is reset, the waypoints are generated anew.
        room.OnReset += (room, _) =>
        {

            waypoint = waypointGenerator()[0];

            waypoint_X.Position = waypoint;
            waypoint_X_1.Position = waypoint_X.Position;
            waypoint_X_2.Position = waypoint_X.Position;

            pressed = false;

            Markers.ForEach(marker =>
            {
                marker.Position = -Vector3.UnitY;
            });
        };
    }

    /// <summary>
    /// When the waypoint is triggered, returns true.
    /// </summary>
    public bool ButtonPressed => pressed;

    /// <summary>
    /// Returns the position of the waypoint.
    /// </summary>
    public Vector3 GetPosition()
    {
        return waypoint_X.Position;
    }

    /// <summary>
    /// Moves the waypoint from its current position to pos.
    /// </summary>
    public void Move(Vector3 pos)
    {
        waypoint_X.Position = pos;
        waypoint_X_1.Position = pos;
        waypoint_X_2.Position = pos;
    }
}