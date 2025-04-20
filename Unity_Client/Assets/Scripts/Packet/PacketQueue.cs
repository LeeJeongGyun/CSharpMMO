using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using ServerCore;
using UnityEngine;

public struct PacketMessage
{
    public ushort Id;

    public IMessage Message;
}

public class PacketQueue
{
    #region 싱글톤

    public static PacketQueue Instance { get; } = new PacketQueue();

    #endregion 싱글톤

    private Queue<PacketMessage> _packetQueue = new Queue<PacketMessage>();
    private object _lock = new object();

    public void Enqueue(ushort id, IMessage message)
    {
        lock (_lock)
        {
            _packetQueue.Enqueue(new PacketMessage() { Id = id, Message = message });
        }
    }

    public List<PacketMessage> DequeueAll()
    {
        List<PacketMessage> list = new List<PacketMessage>();
        lock (_lock)
        {
            while (_packetQueue.Count > 0)
                list.Add(_packetQueue.Dequeue());
        }

        return list;
    }
}
