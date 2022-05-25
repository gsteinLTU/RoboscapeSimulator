using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json.Nodes;
using RoboScapeSimulator.API;
using RoboScapeSimulator.IoTScape;
using RoboScapeSimulator.Node;

namespace RoboScapeSimulator;

public static class Program
{
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
    public static ConcurrentDictionary<string, Room> Rooms { get; private set; } = new();

    public static IoTScapeManager IoTScapeManager { get; private set; } = new();

    public static void Main()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        Trace.WriteLine("Starting RoboScapeSimulator...");

        APIServer.CreateWebServer().RunAsync();

        using Server server = new();

        // Socket.io setup
        server.OnConnection((Socket socket) =>
        {
            string? socketRoom = "";

            Trace.WriteLine("Client connected!");

            // Cleanup a bit on disconnect
            socket.OnDisconnect(() =>
            {
                Trace.WriteLine("Client disconnected!");
                if (!string.IsNullOrEmpty(socketRoom) && Rooms.ContainsKey(socketRoom))
                {
                    Rooms[socketRoom].RemoveSocket(socket);
                }

                socketRoom = null;
            });

            // Cleanup a bit on disconnect
            socket.On("leaveRoom", () =>
            {
                if (!string.IsNullOrEmpty(socketRoom) && Rooms.ContainsKey(socketRoom))
                {
                    Trace.WriteLine("Client left room!");
                    Rooms[socketRoom].RemoveSocket(socket);
                }
                socketRoom = null;
            });

            // Send room info
            socket.On("getRooms", (Socket s, JsonNode[] args) =>
            {
                if (args.Length == 0) return;
                var user = args[0]?.ToString() ?? "";
                Messages.SendUserRooms(socket, user, Rooms);
            });

            socket.On("joinRoom", (Socket s, JsonNode[] args) =>
            {
                Messages.HandleJoinRoom(args, socket, Rooms, ref socketRoom);
            });
        });

        server.Start();

        Stopwatch stopwatch = new();
        stopwatch.Start();

        Trace.WriteLine("Server started");

        // Client update loops
        var clientUpdateTimer = Timers.CreateClientUpdateTimer(updateFPS, Rooms);
        var clientFullUpdateTimer = Timers.CreateClientFullUpdateTimer(Rooms);
        var cleanDeadRoomsTimer = Timers.CreateCleanDeadRoomsTimer(Rooms);

        var fpsSpan = TimeSpan.FromSeconds(1d / simFPS);
        Thread.Sleep(Math.Max(0, (int)fpsSpan.Subtract(stopwatch.Elapsed).TotalMilliseconds));

        var updateTimer = new Timer((e) =>
        {
            lock (Rooms)
            {
                foreach (Room room in Rooms.Values)
                {
                    room.Update((float)stopwatch.Elapsed.TotalSeconds);
                }
            }

            IoTScapeManager.Update((float)stopwatch.Elapsed.TotalSeconds);
            stopwatch.Restart();
        });
        updateTimer.Change(fpsSpan, fpsSpan);

        while (true)
        {
            Thread.Sleep(100);
        }
    }
};