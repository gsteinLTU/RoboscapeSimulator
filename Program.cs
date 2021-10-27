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




using (SocketIOServer server = new(new SocketIOServerOption(9001)))
{
    Dictionary<string, Room> rooms = new();

    Room newRoom = new();

    rooms[newRoom.Name] = newRoom;

    JsonSerializer serializer = new();
    serializer.NullValueHandling = NullValueHandling.Ignore;

    // Socket.io setup
    server.OnConnection((SocketIOSocket socket) =>
    {
        String socketRoom = "Room";
        rooms[socketRoom].activeSockets.Add(socket);
        Console.WriteLine("Client connected!");

        // Send room info
        socket.Emit("availableRooms", new String[] { });

        using (var writer = new JTokenWriter())
        {
            serializer.Serialize(writer, rooms[socketRoom].SimInstance.GetBodies());
            socket.Emit("fullUpdate", writer.Token);
        }

        socket.On("input", (data) =>
        {
            foreach (JToken token in data)
            {
                Console.Write(token + " ");
            }

            Console.WriteLine();
            socket.Emit("echo", data);
        });

        socket.On(SocketIOEvent.DISCONNECT, () =>
        {
            Console.WriteLine("Client disconnected!");
            rooms[socketRoom].activeSockets.Remove(socket);
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
