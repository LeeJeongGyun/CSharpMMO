using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Google.Protobuf;
using Protocol;
using ServerCore;
using UnityEngine;

public class NetworkManager
{
    private ServerSession _session = new ServerSession();

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
        _session.Send(packetBuffer);
    }

    public void ConnectToGameServer()
    {
        ClientPacketManager.Instance.CustomHandler += (id, message) => PacketQueue.Instance.Enqueue(id, message);
        // 소켓 생성
        string hostName = Dns.GetHostName();
        IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

        Connector connector = new Connector(() => _session);
        connector.Connect(endPoint);
    }

    public void Update()
    {
        List<PacketMessage> packetList = PacketQueue.Instance.DequeueAll();
        foreach (PacketMessage packet in packetList)
        {
            var handler = ClientPacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(_session, packet.Message);
        }
    }
}
