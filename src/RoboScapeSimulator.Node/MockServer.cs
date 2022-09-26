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

    public string CreateMockSocket(){
        var id = Guid.NewGuid().ToString();
        sockets[id] = new MockSocket();
        return id;
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
}