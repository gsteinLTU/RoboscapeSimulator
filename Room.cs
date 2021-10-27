using System;
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
}