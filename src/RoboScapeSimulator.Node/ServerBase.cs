using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace RoboScapeSimulator.Node;

public abstract class ServerBase
{
    internal readonly List<Action<SocketBase>> connectionCallbacks = new();

    /// <summary>
    /// Add a callback run when a socket connects
    /// </summary>
    /// <param name="callback"></param>
    public virtual void OnConnection(Action<SocketBase> callback)
    {
        connectionCallbacks.Add(callback);
    }

    public abstract void Start();
    public abstract void Send(string data);

    /// <summary>
    /// Sockets known to this server
    /// </summary>
    public readonly ConcurrentDictionary<string, SocketBase> sockets = new();

    internal enum ReceiveMessageType
    {
        Message, SocketConnected, SocketDisconnected
    }
}

public abstract class SocketBase
{
    public abstract void Emit(string eventName);
    public abstract void Emit(string eventName, string data);
    public abstract void Emit(string eventName, JsonNode data);


    /// <summary>
    /// Add a callback for an event
    /// </summary>
    /// <param name="eventName">Name of event</param>
    /// <param name="callback">Callback to run when event occurs</param>
    public virtual void On(string eventName, Action<SocketBase, JsonNode[]> callback)
    {
        if (callbacks.ContainsKey(eventName))
        {
            callbacks[eventName].Add(callback);
        }
        else
        {
            callbacks.Add(eventName, new List<Action<SocketBase, JsonNode[]>>() { callback });
        }
    }

    /// <summary>
    /// Add a callback for an event
    /// </summary>
    /// <param name="eventName">Name of event</param>
    /// <param name="callback">Callback to run when event occurs</param>
    public virtual void On(string eventName, Action callback)
    {
        On(eventName, (SocketBase sock, JsonNode[] args) => callback());
    }

    /// <summary>
    /// Remove a callback from an event
    /// </summary>
    /// <param name="eventName">Event to remove callback from</param>
    /// <param name="callback">Callback to remove</param>
    public virtual void Off(string eventName, Action<SocketBase, JsonNode[]> callback)
    {
        if (callbacks.ContainsKey(eventName))
        {
            callbacks[eventName].Remove(callback);
        }
    }

    /// <summary>
    /// Setup a callback to run when this Socket disconnects
    /// </summary>
    /// <param name="callback">Callback to run when socket disconnects</param>
    public virtual void OnDisconnect(Action callback)
    {
        onDisconnect.Add(callback);
    }

    /// <summary>
    /// Callbacks for message types
    /// </summary>
    internal readonly Dictionary<string, List<Action<SocketBase, JsonNode[]>>> callbacks = new();

    internal readonly List<Action> onDisconnect = new();

    /// <summary>
    /// ID of this Socket
    /// </summary>
    public string ID;
}