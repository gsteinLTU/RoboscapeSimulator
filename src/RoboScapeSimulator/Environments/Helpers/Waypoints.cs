using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.IoTScape;

namespace RoboScapeSimulator.Environments.Helpers;

internal class Waypoints
{
    readonly List<VisualOnlyEntity> Markers = new();

    readonly Trigger waypoint;
    readonly VisualOnlyEntity waypoint_X_1;
    readonly VisualOnlyEntity waypoint_X_2;

    int waypoint_idx = 0;
    readonly StopwatchLite waypointTimer = new();
    readonly uint _waittime;

    List<Vector3> waypoints = new();

    public EventHandler<int>? OnWaypointActivated;

    public Waypoints(Room room, Func<List<Vector3>> waypointGenerator, string id, uint waitTime = 0, bool robotsOnly = true, float threshold = 0.2f, bool debug = false)
    {
        waypoints = waypointGenerator();

        _waittime = waitTime;

        waypoints.ForEach((waypoint) =>
        {
            Markers.Add(new VisualOnlyEntity(room, initialPosition: waypoint - new Vector3(0, 100, 0), initialOrientation: Quaternion.Identity, width: 0.25f, height: 0.1f, depth: 0.25f, visualInfo: new VisualInfo() { Color = "#363" }));
        });

        if (debug)
        {
            waypoints.ForEach((waypoint) =>
            {
                _ = new VisualOnlyEntity(room, initialPosition: waypoint, initialOrientation: Quaternion.Identity, width: 0.2f, height: 0.1f, depth: 0.2f, visualInfo: new VisualInfo() { Color = "#333" });
            });
        }

        // Waypoint trigger
        waypoint = new Trigger(room, waypoints[0], Quaternion.Identity, threshold, 0.1f, threshold);
        waypoint_X_1 = new VisualOnlyEntity(room, initialPosition: waypoint.Position, initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, 45), width: 0.1f, height: 0.05f, depth: 0.5f, visualInfo: new VisualInfo() { Color = "#633" });
        waypoint_X_2 = new VisualOnlyEntity(room, initialPosition: waypoint.Position, initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, -45), width: 0.1f, height: 0.05f, depth: 0.5f, visualInfo: new VisualInfo() { Color = "#633" });

        waypoint.OnTriggerEnter += (o, e) =>
        {
            if (!robotsOnly || e is Robot)
            {
                waypointTimer.Start();
            }
        };

        waypoint.OnTriggerStay += (o, e) =>
        {
            if (!robotsOnly || e is Robot)
            {
                if (waypointTimer.IsRunning && waypointTimer.ElapsedSeconds > _waittime)
                {
                    // Move waypoint
                    if (waypoint_idx <= waypoints.Count)
                    {
                        if (Markers.Count <= waypoint_idx)
                        {
                            Markers.Add(new VisualOnlyEntity(room, initialPosition: waypoint.Position, initialOrientation: Quaternion.Identity, width: 0.25f, height: 0.1f, depth: 0.25f, visualInfo: new VisualInfo() { Color = "#363" }));
                        }
                        else
                        {
                            Markers[waypoint_idx].Position = waypoint.Position;
                        }

                        OnWaypointActivated?.Invoke(this, waypoint_idx);

                        if (waypoint_idx < waypoints.Count - 1)
                        {
                            waypoint_idx++;
                            waypoint.Position = waypoints[waypoint_idx];
                            waypoint_X_1.Position = waypoints[waypoint_idx];
                            waypoint_X_2.Position = waypoints[waypoint_idx];
                        }
                        else
                        {
                            waypoint_idx++;
                        }

                    }

                    waypointTimer.Reset();
                }
            }
        };

        waypoint.OnTriggerExit += (o, e) =>
        {
            if (!robotsOnly || e is Robot)
            {
                waypointTimer.Reset();
            }
        };

        // IoTScape setup 
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
            return new string[] { waypoint.Position.X.ToString(), waypoint.Position.Y.ToString(), waypoint.Position.Z.ToString() };
        };

        waypointService.Setup(room);

        room.OnReset += (room, _) =>
        {
            waypoints.Clear();

            waypoints = waypointGenerator();

            waypoint_idx = 0;
            waypoint.Position = waypoints[waypoint_idx];
            waypoint_X_1.Position = waypoint.Position;
            waypoint_X_2.Position = waypoint.Position;

            Markers.ForEach(marker =>
            {
                marker.Position = -Vector3.UnitY;
            });

            waypointTimer.Reset();
        };
    }
}