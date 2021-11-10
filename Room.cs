using System;
using System.Collections.Generic;
using System.Reflection;
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

    public Room(string name = "", string password = "", string environment = "default")
    {
        SimInstance = new SimulationInstance();

        var env = Environments.Find((env) => (string)env.GetField("ID", BindingFlags.Public | BindingFlags.Static).GetValue(null) == environment) ?? Environments[0];
        env.GetMethod("Setup", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object?[] { SimInstance });

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
        return Environments.Select(
            (environmentType) => new Dictionary<string, object> {
                { "Name", environmentType.GetField("Name", BindingFlags.Public | BindingFlags.Static).GetValue(null) },
                { "ID", environmentType.GetField("ID", BindingFlags.Public | BindingFlags.Static).GetValue(null) }
        }).ToList();
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

    public bool SkipNextUpdate = false;

    internal static List<Type> Environments = new()
    {
        typeof(DefaultEnvironment)
    };
}