using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.IoTScape;

namespace RoboScapeSimulator.Environments.Helpers;

/*
Same as Waypoints except it does not generate additional Waypoints when the first one has been triggered.
Instead, the red X is moved into a negative postion out of sight and is replaced by a green square checkpoint.
*/

internal class WaypointTest
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
    //Vector3 final = new();
    //When the waypoint is triggered, pressed equals true
    bool pressed = false;

    public WaypointTest(Room room, Func<List<Vector3>> waypointGenerator, /*Func<List<Vector3>> finalGenerator,*/ string id, string id2 = "", bool robotsOnly = true, float threshold = 0.2f, bool debug = false)
    {
        waypoint = waypointGenerator()[0];
        //final = finalGenerator()[0];

        //Creates the X for a single waypoint
        //#633 is dark red
        waypoint_X = new Trigger(room, waypoint, Quaternion.Identity, threshold, 0.1f, threshold);
        waypoint_X_1 = new VisualOnlyEntity(room, initialPosition: waypoint_X.Position, initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, 45), width: 0.1f, height: 0.05f, depth: 0.5f, visualInfo: new VisualInfo() { Color = "#633" });
        waypoint_X_2 = new VisualOnlyEntity(room, initialPosition: waypoint_X.Position, initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, -45), width: 0.1f, height: 0.05f, depth: 0.5f, visualInfo: new VisualInfo() { Color = "#633" });
        Markers.Add(new VisualOnlyEntity(room, initialPosition: new Vector3(0, -1.0f, 0), initialOrientation: Quaternion.Identity, width: 0.25f, height: 0.1f, depth: 0.25f, visualInfo: new VisualInfo() { Color = "#363" }));

        /* waypoint_X.OnTriggerEnter += (o, e) =>
        {
            //Doesn't Markers.Add just add to the list? How come we don't do this for the X's?
            //Why is this the only line that seems to work here?
            //Markers.Add(new VisualOnlyEntity(room, initialPosition: waypoint_X.Position, initialOrientation: Quaternion.Identity, width: 0.25f, height: 0.1f, depth: 0.25f, visualInfo: new VisualInfo() { Color = "#363" }));

            //Green square shows up out of nowhere and doesn't leave, even on reset
            //VisualOnlyEntity greenSquare = new VisualOnlyEntity(room, initialPosition: waypoint_X.Position, initialOrientation: Quaternion.Identity, width: 0.25f, height: 0.1f, depth: 0.25f, visualInfo: new VisualInfo() { Color = "#363" });

            //Why doesn't the X become invisible?
            waypoint_X_1.VisualInfo = VisualInfo.None;
            waypoint_X_2.VisualInfo = VisualInfo.None;

            pressed = true;
        }; */

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
                    waypoint_X.Position = new Vector3(0, 0, 10.0f);
                    waypoint_X_1.Position = new Vector3(0, 0, 10.0f);
                    waypoint_X_2.Position = new Vector3(0, 0, 10.0f);

                    /* The waypoints are moved into this position (0, 0, 10) specifically
                    for the GateEnvironment.Another way is to have them move into a negative Y position,
                    which effectively puts them out of view. */

                    pressed = true;
                }
            }
        };

        /* waypoint_X.OnTriggerEnter += (o, e) =>
        {
            if (!robotsOnly || e is Robot)
            {
                //waypointTimer.Start();
            }
        }; */

        /* waypoint_X.OnTriggerStay += (o, e) =>
        {
            if (!robotsOnly || e is Robot)
            {
                if (waypointTimer.IsRunning && waypointTimer.ElapsedSeconds > _waittime)
                {
                    // Move waypoint
                    //if (waypoint_idx <= waypoint.Count)
                    if (waypoint_idx <= 1)
                    {
                        if (Markers.Count <= waypoint_idx)
                        {
                            Markers.Add(new VisualOnlyEntity(room, initialPosition: waypoint_X.Position, initialOrientation: Quaternion.Identity, width: 0.25f, height: 0.1f, depth: 0.25f, visualInfo: new VisualInfo() { Color = "#363" }));
                            //readonly, so error, but I want to make them invisible so the X disappears when the green squares show up
                            //waypoint_X_1 = new VisualOnlyEntity(room, initialPosition: waypoint_X.Position, initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, 45), width: 0.1f, height: 0.05f, depth: 0.5f, visualInfo: VisualInfo.None);
                            waypoint_X_1.VisualInfo = VisualInfo.None;
                            //waypoint_X_2 = new VisualOnlyEntity(room, initialPosition: waypoint_X.Position, initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, -45), width: 0.1f, height: 0.05f, depth: 0.5f, visualInfo: VisualInfo.None);
                            waypoint_X_2.VisualInfo = VisualInfo.None;
                        }
                        else
                        {
                            Markers[waypoint_idx].Position = waypoint_X.Position;
                        }

                        //OnWaypointActivated?.Invoke(this, waypoint_idx);

                        if (waypoint_idx < waypoint.Count - 1)
                        {
                            waypoint_idx++;
                            waypoint_X.Position = waypoint[waypoint_idx];
                            waypoint_X_1.Position = waypoint[waypoint_idx];
                            waypoint_X_2.Position = waypoint[waypoint_idx];
                        }
                        else
                        {
                            waypoint_idx++;
                        }

                    }
                }
            }
        }; */

        /* waypoint_X.OnTriggerExit += (o, e) =>
        {
            if (!robotsOnly || e is Robot)
            {
                //waypointTimer.Reset();
            }
        }; */

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

        room.OnReset += (room, _) =>
        {
            //waypoint.Clear();

            waypoint = waypointGenerator()[0];

            //waypoint_idx = 0;
            //waypoint_X.Position = waypoint[waypoint_idx];
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

    public bool buttonPressed()
    {
        return pressed;
    }
}