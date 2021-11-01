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
        SimInstance = new SimulationInstance();
        Name = "Room";
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public static List<Dictionary<string, object>> ListEnvironments()
    {
        return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "name", "default" } } };
    }
}