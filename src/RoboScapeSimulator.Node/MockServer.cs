using System.Diagnostics;
using System.Text.Json.Nodes;

namespace RoboScapeSimulator.Node;


class MockServer : ServerBase
{
    public override void Send(string data)
    {
        // Not actually sending anything in MockServer
        Debug.WriteLine("MockServer send " + data);
    }

    public override void Start()
    {
        // Nothing to start for a MockServer
        Debug.WriteLine("MockServer Start");
    }
}

class MockSocket : SocketBase
{
    public override void Emit(string eventName)
    {
        
    }

    public override void Emit(string eventName, string data)
    {
        
    }

    public override void Emit(string eventName, JsonNode data)
    {
        
    }

    public override void Off(string eventName, Action<SocketBase, JsonNode[]> callback)
    {
        
    }

    public override void On(string eventName, Action callback)
    {
        
    }

    public override void On(string eventName, Action<SocketBase, JsonNode[]> callback)
    {
        
    }
}