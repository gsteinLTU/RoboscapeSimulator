using System.Collections.Concurrent;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoboScapeSimulator;
using RoboScapeSimulator.IoTScape;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;

Trace.Listeners.Add(new ConsoleTraceListener());
Trace.WriteLine("Starting RoboScapeSimulator...");

RoboScapeSimulator.Node.Server server = new();

server.Start();


JsonSerializer serializer = new();

serializer.NullValueHandling = NullValueHandling.Ignore;

server.OnConnection((socket) =>
{
    Trace.WriteLine("New socket connected: " + socket.ID);

    socket.On("getRooms", (args) =>
    {
        Trace.WriteLine("Socket " + socket.ID + " requesting rooms for " + args[0]);

        using (var writer = new JTokenWriter())
        {
            serializer.Serialize(writer, new Dictionary<string, object>(){{"availableRooms",new List<String>()}});
            socket.Emit("availableRooms", writer.Token);
        }

        using (var writer = new JTokenWriter())
        {
            serializer.Serialize(writer, Room.ListEnvironments());
            socket.Emit("availableEnvironments", writer.Token);
        }
    });
});

while (true)
{

}

/*
/// <summary>
/// Frequency to send update messages to users
/// </summary>
const int updateFPS = 10;

/// <summary>
/// Frequency to run simulation at
/// </summary>
const int simFPS = 60;

JsonSerializer serializer = new();
serializer.NullValueHandling = NullValueHandling.Ignore;

/// <summary>
/// Mapping of room IDs to Room objects
/// </summary>
ConcurrentDictionary<string, Room> rooms = new();

IoTScapeManager ioTScapeManager = new IoTScapeManager();

using (SocketIOServer server = new(new SocketIOServerOption(9001)))
{
    // Socket.io setup
    server.OnConnection((SocketIOSocket socket) =>
    {
        string? socketRoom = "";

        Trace.WriteLine("Client connected!");

        // Cleanup a bit on disconnect
        socket.On(SocketIOEvent.DISCONNECT, () =>
        {
            Trace.WriteLine("Client disconnected!");
            if (socketRoom != null)
            {
                rooms[socketRoom].RemoveSocket(socket);
            }
        });

        // Cleanup a bit on disconnect
        socket.On("leaveRoom", () =>
        {
            if (socketRoom != null)
            {
                Trace.WriteLine("Client left room!");
                rooms[socketRoom].RemoveSocket(socket);
                socketRoom = null;
            }
        });

        // Send room info
        socket.On("getRooms", (JToken[] args) =>
        {
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

    // Simulation update loop
    while (true)
    {
        foreach (Room room in rooms.Values)
        {
            room.Update((float)stopwatch.Elapsed.TotalSeconds);
        }
        ioTScapeManager.Update((float)stopwatch.Elapsed.TotalSeconds);
        stopwatch.Restart();
        Thread.Sleep(Math.Max(0, (int)fpsSpan.Subtract(stopwatch.Elapsed).TotalMilliseconds));
    }
}
*/