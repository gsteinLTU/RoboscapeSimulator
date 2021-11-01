using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using EngineIOSharp.Common.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;


Console.WriteLine("Starting RoboScapeSimulator...");

const int updateFPS = 15;
const int simFPS = 90;


JsonSerializer serializer = new();
serializer.NullValueHandling = NullValueHandling.Ignore;

Dictionary<string, Room> rooms = new();

void sendAvailableRooms(SocketIOSocket socket)
{
    using (var writer = new JTokenWriter())
    {
        serializer.Serialize(writer, new Dictionary<string, object> { { "availableRooms", rooms.Keys }, { "canCreate", rooms.Count < SettingsManager.MaxRooms } });
        socket.Emit("availableRooms", writer.Token);
    }
    using (var writer = new JTokenWriter())
    {
        serializer.Serialize(writer, Room.ListEnvironments());
        socket.Emit("availableEnvironments", writer.Token);
    }
}

using (SocketIOServer server = new(new SocketIOServerOption(9001)))
{

    Room newRoom = new();

    rooms[newRoom.Name] = newRoom;


    // Socket.io setup
    server.OnConnection((SocketIOSocket socket) =>
    {

        String socketRoom = null;
        Console.WriteLine("Client connected!");

        socket.On(SocketIOEvent.DISCONNECT, () =>
        {
            Console.WriteLine("Client disconnected!");
            if (socketRoom != null)
            {
                rooms[socketRoom].activeSockets.Remove(socket);
            }
        });

        // Send room info
        sendAvailableRooms(socket);

        socket.On("joinRoom", (JToken[] args) =>
        {
            socketRoom = "Room";
            Console.WriteLine(args);
            rooms[socketRoom].activeSockets.Add(socket);

            using (var writer = new JTokenWriter())
            {
                serializer.Serialize(writer, rooms[socketRoom].SimInstance.GetBodies());
                socket.Emit("fullUpdate", writer.Token);
            }
        });
    });

    server.Start();

    Stopwatch stopwatch = new();
    stopwatch.Start();

    Console.WriteLine("Server started");

    //Console.WriteLine("Input /exit to exit program.");

    // Client update loops
    var clientUpdateTimer = new System.Timers.Timer(1000d / updateFPS);

    clientUpdateTimer.Elapsed += (source, e) =>
    {
        foreach (Room room in rooms.Values)
        {
            using (var writer = new JTokenWriter())
            {
                serializer.Serialize(writer, room.SimInstance.GetBodies());

                foreach (var socket in room.activeSockets)
                {
                    socket.Emit("update", writer.Token);
                }
            }
        }

    };

    clientUpdateTimer.Start();


    var clientFullUpdateTimer = new System.Timers.Timer(500d);

    clientFullUpdateTimer.Elapsed += (source, e) =>
    {
        foreach (Room room in rooms.Values)
        {
            using (var writer = new JTokenWriter())
            {
                serializer.Serialize(writer, room.SimInstance.GetBodies());
                foreach (var socket in room.activeSockets)
                {
                    socket.Emit("fullUpdate", writer.Token);
                }
            }
        }
    };

    clientFullUpdateTimer.Start();


    // string line;
    //
    // while (!(line = Console.ReadLine())?.Trim()?.ToLower()?.Equals("/exit") ?? false)
    // {
    //     server.Emit("echo", line);
    // }

    var fpsSpan = TimeSpan.FromSeconds(1d / simFPS);
    Thread.Sleep(Math.Max(0, (int)fpsSpan.Subtract(stopwatch.Elapsed).TotalMilliseconds));

    // Simulation update loop
    while (true)
    {
        foreach (Room room in rooms.Values)
        {
            room.SimInstance.Update((float)stopwatch.Elapsed.TotalSeconds);
        }
        stopwatch.Restart();
        Thread.Sleep(Math.Max(0, (int)fpsSpan.Subtract(stopwatch.Elapsed).TotalMilliseconds));
    }
}
