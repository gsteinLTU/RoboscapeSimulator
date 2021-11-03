using System;
using System.Collections.Generic;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;

public class Room : IDisposable
{
    public List<SocketIOSocket> activeSockets = new();

    public String Name;

    public SimulationInstance SimInstance;

    public Room()
    {
        Random random = new Random();
        SimInstance = new SimulationInstance();
        Name = "Room" + random.Next(0, 1000000).ToString("X4");

        Console.WriteLine("Room " + Name + " created.");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public static List<Dictionary<string, object>> ListEnvironments()
    {
        return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "name", "default" } } };
    }

    public Dictionary<string, object> GetInfo()
    {
        return new Dictionary<string, object>()
        {
            {"background", ""}
        };
    }
}