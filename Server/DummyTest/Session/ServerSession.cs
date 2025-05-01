namespace DummyTest.Session;

using System.Net;
using ServerCore;

public class ServerSession : PacketSession
{
    public int DummyId { get; set; }

    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"Connected({DummyId})");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"Disconnected({DummyId})");
    }

    public override void OnPacketRecv(ArraySegment<byte> buffer)
    {
        ClientPacketManager.Instance.MatchPacketHandler(this, buffer);
    }

    public override void OnSend(int sendBytes)
    {
    }
}
