using System;
using System.Collections.Concurrent;
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

ConcurrentDictionary<string, Room> rooms = new();

/// <summary>
/// Send the available rooms and environments to a socket
/// </summary>
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

/// <summary>
/// Helper function to print a JToken
/// </summary>
void printJSON(JToken token)
{
    using (var writer = new StringWriter())
    {
        serializer.Serialize(writer, token);
        Console.WriteLine(writer.ToString());
    }
}

/// <summary>
/// Helper function to print a JToken
/// </summary>
void printJSONArray(JToken[] tokens)
{
    Array.ForEach(tokens, printJSON);
}

using (SocketIOServer server = new(new SocketIOServerOption(9001)))
{
    // Socket.io setup
    server.OnConnection((SocketIOSocket socket) =>
    {
        String socketRoom = null;

        Console.WriteLine("Client connected!");

        // Cleanup a bit on disconnect
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
            // Remove from existing room
            if (socketRoom != null)
            {
                rooms[socketRoom].activeSockets.Remove(socket);
            }

            // Create room if requested
            if ((string)args[0]["roomID"] == "create")
            {
                Room newRoom = new();

                if ((string)args[0]["namespace"] != null)
                {
                    newRoom.Name += "@" + (string)args[0]["namespace"];
                }

                rooms[newRoom.Name] = newRoom;

                socketRoom = newRoom.Name;
            }
            else
            {
                // TODO: validation
                socketRoom = (string)args[0]["roomID"];
            }

            // Setup updates for socket in new room
            rooms[socketRoom].activeSockets.Add(socket);

            using (var writer = new JTokenWriter())
            {
                serializer.Serialize(writer, socketRoom);
                socket.Emit("roomJoined", writer.Token);
                printJSON(writer.Token);
            }

            using (var writer = new JTokenWriter())
            {
                serializer.Serialize(writer, rooms[socketRoom].GetInfo());
                socket.Emit("roomInfo", writer.Token);
            }

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
