using System.Collections.Concurrent;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoboScapeSimulator;
using RoboScapeSimulator.IoTScape;

Trace.Listeners.Add(new ConsoleTraceListener());
Trace.WriteLine("Starting RoboScapeSimulator...");

/// <summary>
/// Frequency to send update messages to users
/// </summary>
const int updateFPS = 9;

/// <summary>
/// Frequency to run simulation at
/// </summary> 
const int simFPS = 45;

JsonSerializer serializer = new();
serializer.NullValueHandling = NullValueHandling.Ignore;

/// <summary>
/// Mapping of room IDs to Room objects
/// </summary>
ConcurrentDictionary<string, Room> rooms = new();

IoTScapeManager ioTScapeManager = new IoTScapeManager();

using (RoboScapeSimulator.Node.Server server = new())
{
    // Socket.io setup
    server.OnConnection((RoboScapeSimulator.Node.Socket socket) =>
    {
        string? socketRoom = "";

        Trace.WriteLine("Client connected!");

        // Cleanup a bit on disconnect
        socket.OnDisconnect(() =>
        {
            Trace.WriteLine("Client disconnected!");
            if (!string.IsNullOrEmpty(socketRoom))
            {
                rooms[socketRoom].RemoveSocket(socket);
            }
        });

        // Cleanup a bit on disconnect
        socket.On("leaveRoom", () =>
        {
            if (!string.IsNullOrEmpty(socketRoom))
            {
                Trace.WriteLine("Client left room!");
                rooms[socketRoom].RemoveSocket(socket);
                socketRoom = null;
            }
        });

        // Send room info
        socket.On("getRooms", (JToken[] args) =>
        {
            if (args.Length == 0 || args[0].Type != JTokenType.String) return;
            var user = (string)args[0];
            Messages.SendUserRooms(socket, user, rooms);
        });

        socket.On("joinRoom", (JToken[] args) =>
        {
            Messages.HandleJoinRoom(args, socket, rooms, ref socketRoom);
        });
    });

    server.Start();

    Stopwatch stopwatch = new();
    stopwatch.Start();

    Trace.WriteLine("Server started");

    // Client update loops
    var clientUpdateTimer = Timers.CreateClientUpdateTimer(updateFPS, rooms);
    var clientFullUpdateTimer = Timers.CreateClientFullUpdateTimer(rooms, serializer);
    var cleanDeadRoomsTimer = Timers.CreateCleanDeadRoomsTimer(rooms);

    var fpsSpan = TimeSpan.FromSeconds(1d / simFPS);
    Thread.Sleep(Math.Max(0, (int)fpsSpan.Subtract(stopwatch.Elapsed).TotalMilliseconds));

    var updateTimer = new Timer((e) =>
    {
        lock (rooms)
        {
            foreach (Room room in rooms.Values)
            {
                room.Update((float)stopwatch.Elapsed.TotalSeconds);
            }
        }

        ioTScapeManager.Update((float)stopwatch.Elapsed.TotalSeconds);
        stopwatch.Restart();
    });
    updateTimer.Change(fpsSpan, fpsSpan);

    while (true)
    {
        Thread.Sleep(100);
    }
}