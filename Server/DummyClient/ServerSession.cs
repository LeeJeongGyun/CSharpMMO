using ServerCore;
using System.Net;

namespace DummyClient;

internal class ServerSession : PacketSession
{
    public static int connectCount = 0;
    public static int disConnectCount = 0;
    public static int recvCount = 0;

    public override void OnConnected(EndPoint? endPoint)
    {
        Interlocked.Increment(ref connectCount);
        Console.WriteLine($"[CLIENT] Connected: {endPoint}");

        ServerSessionManager.Instance.AddSession(this);
    }

    public override void OnDisconnected(EndPoint? endPoint)
    {
        Interlocked.Increment(ref disConnectCount);
        Console.WriteLine($"[CLIENT] Disconnected: {endPoint}");

        ServerSessionManager.Instance.RemoveSession(this);
    }

    public override void OnPacketRecv(ArraySegment<byte> buffer)
    {
        Interlocked.Increment(ref recvCount);
        ClientPacketManager.Instance.MatchPacketHandler(this, buffer);
    }

    public override void OnSend(int sendBytes)
    { }
}