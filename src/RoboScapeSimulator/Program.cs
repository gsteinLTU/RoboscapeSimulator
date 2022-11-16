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
    const int simFPS = 40;

    /// <summary>
    /// Mapping of room IDs to Room objects
    /// </summary>
    public static ConcurrentDictionary<string, Room> Rooms { get; private set; } = new();

    public static IoTScapeManager IoTScapeManager { get; private set; } = new();

    public static readonly long StartTicks = Environment.TickCount64;

    public static readonly DateTime StartDateTime = DateTime.Now;

    public static void Main()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        Trace.WriteLine("Starting RoboScapeSimulator...");

        APIServer.CreateWebServer().RunAsync();

        using Server server = new();

        // Socket.io setup
        server.OnConnection((SocketBase socket) =>
        {
            string? socketRoom = "";

            Trace.WriteLine("Client connected!");

            // Cleanup a bit on disconnect
            socket.OnDisconnect(() =>
            {
                Trace.WriteLine("Client disconnected!");
                
                if (!string.IsNullOrEmpty(socketRoom) && Rooms.TryGetValue(socketRoom, out var room))
                {
                    room.RemoveSocket(socket);
                }

                socketRoom = null;
            });

            // Cleanup a bit on disconnect
            socket.On("leaveRoom", () =>
            {
                if (!string.IsNullOrEmpty(socketRoom) && Rooms.TryGetValue(socketRoom, out var room))
                {
                    Trace.WriteLine("Client left room!");
                    room.RemoveSocket(socket);
                }
                socketRoom = null;
            });

            // Send room info
            socket.On("getRooms", (SocketBase s, JsonNode[] args) =>
            {
                if (args.Length == 0) return;
                var user = args[0]?.ToString() ?? "";
                Messages.SendUserRooms(socket, user);
            });

            socket.On("joinRoom", (SocketBase s, JsonNode[] args) =>
            {
                Messages.HandleJoinRoom(args, socket, ref socketRoom);
            });
        });

        server.Start();

        // Client update loops
        var clientUpdateTimer = Timers.CreateClientUpdateTimer(updateFPS);
        var cleanDeadRoomsTimer = Timers.CreateCleanDeadRoomsTimer();
        var apiAnnounceTimer = Timers.CreateMainAPIServerAnnounceTimer();
        var roomUpdateTimer = Timers.CreateRoomUpdateTimer(simFPS);

        Trace.WriteLine("Server started");

        while (true)
        {
            Thread.Sleep(100);
        }
    }
};