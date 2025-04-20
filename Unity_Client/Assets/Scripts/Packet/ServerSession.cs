using ServerCore;
using System;
using System.Net;

internal class ServerSession : PacketSession
{
    public int ObjectId { get; set; }

    public override void OnConnected(EndPoint endPoint)
    {
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
    }

    public override void OnPacketRecv(ArraySegment<byte> buffer)
    {
        ClientPacketManager.Instance.MatchPacketHandler(this, buffer);
    }

    public override void OnSend(int sendBytes)
    {
    }
}
