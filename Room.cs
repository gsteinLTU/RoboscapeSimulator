using System;
using System.Collections.Generic;
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

    public Room(string name = "", string password = "")
    {
        SimInstance = new SimulationInstance();

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
    /// Lists the available environment types in a format sendable to the client as JSON
    /// </summary>
    /// <returns>Structure representing information about avaialble environment types</returns>
    public static List<Dictionary<string, object>> ListEnvironments()
    {
        return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "name", "default" } } };
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
}