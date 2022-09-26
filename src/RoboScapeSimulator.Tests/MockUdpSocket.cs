using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace RoboScapeSimulator.Tests;

internal class MockUdpSocket : IUdpSocket
{
    internal List<byte> buffer = new();
    
    public int Available => buffer.Count;

    public int Receive(byte[] incoming)
    {
        return 0;
    }

    public int SendTo(byte[] bytes, SocketFlags none, EndPoint hostEndPoint)
    {
        return bytes.Length;
    }
}