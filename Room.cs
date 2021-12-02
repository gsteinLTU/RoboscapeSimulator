using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Server.Client;

/// <summary>
/// A room shared between a set of users
/// </summary>
public class Room : IDisposable
{
    /// <summary>
    /// List of sockets for users connected to this room
    /// </summary>
    public List<SocketIOSocket> activeSockets = new();

    /// <summary>
    /// Visible string used to identify this room
    /// </summary>
    public string Name;

    /// <summary>
    /// Extra string required to join the room, not intended for cryptographic security.
    /// </summary>
    public string Password;

    /// <summary>
    /// The simulated environment this Room represents
    /// </summary>
    public SimulationInstance SimInstance;

    /// <summary>
    /// Username/ID of this Room's creator
    /// </summary>
    public string? Creator;

    public Room(string name = "", string password = "", string environment = "default")
    {
        Console.WriteLine($"Setting up room {name} with environment {environment}");
        SimInstance = new SimulationInstance();

        var env = Environments.Find((env) => env.ID == environment) ?? Environments[0];
        env.Setup(this);

        // Give randomized default name
        if (name == "")
        {
            Random random = new();
            Name = "Room" + random.Next(0, 1000000).ToString("X4");
        }
        else
        {
            Name = name;
        }

        Password = password;

        Console.WriteLine("Room " + Name + " created.");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Collect information about the room's metadata in a format sendable to the client as JSON
    /// </summary>
    /// <returns>Structure containing room metadata</returns>
    public Dictionary<string, object> GetInfo()
    {
        return new Dictionary<string, object>()
        {
            {"background", ""}
        };
    }


    /// <summary>
    /// Do not send the next update for this room (if needed for optimization purposes)
    /// </summary>
    public bool SkipNextUpdate = false;

    public void SendToClients(string eventName, params object[] args)
    {
        foreach (var socket in activeSockets)
        {
            Utils.sendAsJSON(socket, eventName, args);
        }
    }

    internal void AddSocket(SocketIOSocket socket)
    {
        activeSockets.Add(socket);
        socket.On("resetRobot", handleResetRobot);
    }

    private void handleResetRobot(JToken[] args)
    {
        string robotID = args[0].ToString();

        Robot? robot = SimInstance.Robots.Find(r => r.ID == robotID);
        if (robot != null)
        {
            robot.Reset();
        }
    }

    internal void RemoveSocket(SocketIOSocket socket)
    {
        activeSockets.Remove(socket);
        socket.Off("resetRobot", handleResetRobot);
    }

    /// <summary>
    /// Lists the available environment types in a format sendable to the client as JSON
    /// </summary>
    /// <returns>Structure representing information about avaialble environment types</returns>
    public static List<Dictionary<string, object>> ListEnvironments()
    {
        return Environments.Select(
            (environmentType) => new Dictionary<string, object> {
                { "Name", environmentType.Name },
                { "ID", environmentType.ID }
        }).ToList();
    }

    /// <summary>
    /// Available environment types
    /// </summary>
    internal static List<EnvironmentConfiguration> Environments = new()
    {
        new DefaultEnvironment()
    };
}