using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RoboScapeSimulator.Tests;

class MockUdpClient : IUdpClient
{
    public List<byte> buffer = new();

    public int Available => buffer.Count;

    public void Close()
    {
        
    }

    public void Connect(string host, int port)
    {
        
    }

    public byte[] Receive(ref IPEndPoint? remoteEP)
    {
        return System.Array.Empty<byte>();
    }

    public Task<int> SendAsync(byte[] datagram, int bytes)
    {
        return new Task<int>(() => {return bytes;});
    }
}