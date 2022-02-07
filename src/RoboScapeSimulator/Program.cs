using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using IoTScape;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;

Console.WriteLine("Starting RoboScapeSimulator...");

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
        string socketRoom = "";

        Console.WriteLine("Client connected!");

        // Cleanup a bit on disconnect
        socket.On(SocketIOEvent.DISCONNECT, () =>
        {
            Console.WriteLine("Client disconnected!");
            if (socketRoom != null)
            {
                rooms[socketRoom].RemoveSocket(socket);
            }
        });

        // Send room info
        socket.On("getRooms", (JToken[] args) =>
        {
            var user = (string)args[0];
            Console.WriteLine("init " + user);
            Messages.SendUserRooms(socket, user, rooms);
            Console.WriteLine("init2 " + user);
        });

        socket.On("joinRoom", (JToken[] args) =>
        {
            Messages.HandleJoinRoom(args, socket, rooms, ref socketRoom);
        });
    });

    server.Start();

    Stopwatch stopwatch = new();
    stopwatch.Start();

    Console.WriteLine("Server started");

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
