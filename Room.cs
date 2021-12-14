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

    /// <summary>
    /// Time (in seconds) without interaction this room will stay alive for, default 15 minutes
    /// </summary>
    public float Timeout = 60 * 15;

    /// <summary>
    /// Previous time this Room was interacted with
    /// </summary>
    private DateTime lastInteractionTime;

    /// <summary>
    /// When going inactive, how long should this room be kept in "suspended animation" until deletion
    /// </summary>
    public float MaxHibernateTime = 60 * 60 * 24;

    /// <summary>
    /// Is the Room currently suspended
    /// </summary>
    public bool Hibernating = false;

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

        LastInteractionTime = DateTime.Now;

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
            {"background", ""},
            {"time", SimInstance.Time}
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

    /// <summary>
    /// Adds a socket to active list and sets up message listeners
    /// </summary>
    /// <param name="socket">Socket to add to active sockets list</param>
    internal void AddSocket(SocketIOSocket socket)
    {
        LastInteractionTime = DateTime.Now;
        activeSockets.Add(socket);
        socket.On("resetRobot", handleResetRobot);
        socket.On("robotButton", handleRobotButton);
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

    private void handleRobotButton(JToken[] args)
    {
        string robotID = args[0].ToString();

        Robot? robot = SimInstance.Robots.Find(r => r.ID == robotID);
        if (robot is ParallaxRobot parallaxRobot)
        {
            parallaxRobot.OnButtonPress((bool)args[1]);
        }
    }

    /// <summary>
    /// Removes a socket from active list and removes message listeners
    /// </summary>
    /// <param name="socket">Socket to remove from active sockets list</param>
    internal void RemoveSocket(SocketIOSocket socket)
    {
        socket.Off("resetRobot", handleResetRobot);
        socket.Off("robotButton", handleRobotButton);
        socket.Emit("roomLeft");
        activeSockets.Remove(socket);
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
        new DefaultEnvironment(),
        new DemoEnvironment()
    };

    public DateTime LastInteractionTime
    {
        get => lastInteractionTime;
        set
        {
            lastInteractionTime = value;

            // Wake up if sleeping
            Hibernating = false;
        }
    }

    /// <summary>
    /// Update the state of the Room and its simulation
    /// </summary>
    /// <param name="dt">Delta time between updates</param>
    internal void Update(float dt)
    {
        if (Hibernating) return;

        // Check if too much time has passed
        if ((DateTime.Now - LastInteractionTime).TotalSeconds > Timeout)
        {
            // Go to sleep
            Hibernating = true;

            // Kick all users
            while (activeSockets.Count > 0)
            {
                RemoveSocket(activeSockets[0]);
            }

            Console.WriteLine($"Room {Name} is now hibernating");
            return;
        }

        SimInstance.Update(dt);
    }
}