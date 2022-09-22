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

    public abstract void On(string eventName, Action callback);
    public abstract void On(string eventName, Action<SocketBase, JsonNode[]> callback);

    public abstract void Off(string eventName, Action<SocketBase, JsonNode[]> callback);

    public abstract void OnDisconnect(Action callback);

    /// <summary>
    /// Callbacks for message types
    /// </summary>
    internal readonly Dictionary<string, List<Action<SocketBase, JsonNode[]>>> callbacks = new();

    internal readonly List<Action> onDisconnect = new();
}