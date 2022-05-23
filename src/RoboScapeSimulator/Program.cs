using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json.Nodes;
using RoboScapeSimulator;
using RoboScapeSimulator.API;
using RoboScapeSimulator.IoTScape;
using RoboScapeSimulator.Node;

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

/// <summary>
/// Mapping of room IDs to Room objects
/// </summary>
ConcurrentDictionary<string, Room> rooms = new();

IoTScapeManager ioTScapeManager = new();

APIServer.CreateWebServer(8000).RunAsync();

using (Server server = new())
{
    // Socket.io setup
    server.OnConnection((Socket socket) =>
    {
        string? socketRoom = "";

        Trace.WriteLine("Client connected!");

        // Cleanup a bit on disconnect
        socket.OnDisconnect(() =>
        {
            Trace.WriteLine("Client disconnected!");
            if (!string.IsNullOrEmpty(socketRoom) && rooms.ContainsKey(socketRoom))
            {
                rooms[socketRoom].RemoveSocket(socket);
            }

            socketRoom = null;
        });

        // Cleanup a bit on disconnect
        socket.On("leaveRoom", () =>
        {
            if (!string.IsNullOrEmpty(socketRoom) && rooms.ContainsKey(socketRoom))
            {
                Trace.WriteLine("Client left room!");
                rooms[socketRoom].RemoveSocket(socket);
            }
            socketRoom = null;
        });

        // Send room info
        socket.On("getRooms", (Socket s, JsonNode[] args) =>
        {
            if (args.Length == 0) return;
            var user = args[0]?.ToString() ?? "";
            Messages.SendUserRooms(socket, user, rooms);
        });

        socket.On("joinRoom", (Socket s, JsonNode[] args) =>
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
    var clientFullUpdateTimer = Timers.CreateClientFullUpdateTimer(rooms);
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