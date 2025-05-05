namespace DummyTest.Session;

using System.Net;
using Google.Protobuf;
using Protocol;
using ServerCore;

public class ServerSession : PacketSession
{
    public int DummyId { get; set; }

    public void Send(IMessage packet)
    {
        // 4바이트 헤더 공간 확보
        int dataSize = packet.CalculateSize();
        byte[] packetBuffer = new byte[dataSize + 4];

        string messageName = packet.Descriptor.Name;
        PacketID packetId = (PacketID)Enum.Parse(typeof(PacketID), messageName.Replace("_", string.Empty));

        Span<byte> span = new Span<byte>(packetBuffer);
        BitConverter.TryWriteBytes(span, (ushort)(dataSize + 4));
        BitConverter.TryWriteBytes(span.Slice(sizeof(ushort)), (ushort)packetId);
        Array.Copy(packet.ToByteArray(), 0, packetBuffer, 4, dataSize);
        Send(packetBuffer);
    }

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
