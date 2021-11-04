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
    sendAsJSON(socket, "availableRooms", new Dictionary<string, object> { { "availableRooms", rooms.Keys }, { "canCreate", rooms.Count < SettingsManager.MaxRooms } });
    sendAsJSON(socket, "availableEnvironments", Room.ListEnvironments());
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

void sendAsJSON<T>(SocketIOSocket socket, string eventName, T data)
{
    using (var writer = new JTokenWriter())
    {
        serializer.Serialize(writer, data);
        socket.Emit(eventName, writer.Token);
    }
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
            string roomID = (string)args[0]["roomID"];
            if (roomID == "create")
            {
                Room newRoom = new("", (string)args[0]["password"] ?? "");

                if ((string)args[0]["namespace"] != null)
                {
                    newRoom.Name += "@" + (string)args[0]["namespace"];
                }

                rooms[newRoom.Name] = newRoom;

                socketRoom = newRoom.Name;
            }
            else
            {
                // Joining existing room, make sure it exists first
                if (rooms.ContainsKey(roomID))
                {
                    if (rooms[roomID].Password == "" || rooms[roomID].Password == (string)args[0]["password"])
                    {
                        socketRoom = (string)args[0]["roomID"];
                    }
                }
            }

            if (socketRoom != null)
            {
                // Setup updates for socket in new room
                rooms[socketRoom].activeSockets.Add(socket);
                sendAsJSON(socket, "roomJoined", socketRoom);
                sendAsJSON(socket, "roomInfo", rooms[socketRoom].GetInfo());
                sendAsJSON(socket, "fullUpdate", rooms[socketRoom].SimInstance.GetBodies());
            }
            else
            {
                // Join failed
                sendAsJSON(socket, "roomJoined", false);
                Console.WriteLine("Failed attempt to join room " + roomID);
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
                serializer.Serialize(writer, room.SimInstance.GetBodies(true));

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
