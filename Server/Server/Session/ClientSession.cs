namespace Server;

using System;
using System.Net;
using Google.Protobuf;
using Protocol;
using Server.Content;
using Server.Content.Object;
using Server.Data;
using Server.Session;
using ServerCore;

public partial class ClientSession : PacketSession
{
    public static int connectCount = 0;
    public static int disConnectCount = 0;
    public static int recvCount = 0;
    public static int sessionId = 1;
    private object _lock = new object();
    public PlayerServerState PlayerServerState { get; private set; } = PlayerServerState.PlayerStateLogin;
    public int SessionId { get; set; }
    public int ObjectId { get; private set; } = 0;
    public List<ArraySegment<byte>> _pendingList { get; private set; } = new List<ArraySegment<byte>>();

    /// <summary>
    /// 얘약만 하고 실제 송신은 SendThread가 FlushSend 함수를 호출하여 진행한다.
    /// </summary>
    /// <param name="packet">송신을 예약할 패킷</param>
    public void Send(IMessage packet)
    {
        // 4바이트 헤더 공간 확보
        int dataSize = packet.CalculateSize();
        byte[] packetBuffer = new byte[dataSize + 4];

        string messageName = packet.Descriptor.Name;
        PacketID packetId = (PacketID)Enum.Parse(typeof(PacketID), messageName.Replace("_", string.Empty));
        int value = (int)packetId;

        Span<byte> span = new Span<byte>(packetBuffer);
        BitConverter.TryWriteBytes(span, (ushort)(dataSize + 4));
        BitConverter.TryWriteBytes(span.Slice(sizeof(ushort)), (ushort)packetId);
        Array.Copy(packet.ToByteArray(), 0, packetBuffer, 4, dataSize);

        // Send(packetBuffer);
        lock (_lock)
            _pendingList.Add(packetBuffer);
    }

    public override void OnConnected(EndPoint? endPoint)
    {
        Interlocked.Increment(ref connectCount);
        Console.WriteLine($"[SERVER] ClientSession Connected: {endPoint}");

        S2C_Connected connectedPacket = new S2C_Connected();
        Send(connectedPacket);
    }

    public override void OnDisconnected(EndPoint? endPoint)
    {
        Interlocked.Increment(ref disConnectCount);
        Console.WriteLine($"[SERVER] ClientSession Disconnected: {endPoint}");

        Player? player = ObjectManager.Instance.FindPlayer(ObjectId);
        if (player != null)
        {
            // 1. 현재 있는 방에서 퇴장
            // TODO Command 패턴으로 바뀌면서 내부에서 Null Crash 발생 가능성 존재.
            player.Room?.Push(player.Room.LeaveRoom, player.ObjectType, ObjectId);

            // 2. PlayerManager 삭제
            ObjectManager.Instance.RemovePlayer(ObjectId);
            ObjectId = 0;

            // 3. SessionManager 삭제
            SessionManager.Instance.Remove(SessionId);
        }
    }

    public override void OnPacketRecv(ArraySegment<byte> buffer)
    {
        Interlocked.Increment(ref recvCount);
        ServerPacketManager.Instance.MatchPacketHandler(this, buffer);
    }

    public override void OnSend(int sendBytes)
    { }

    /// <summary>
    /// Send 함수에 의해서 예약된 패킷을 송신
    /// </summary>
    public void FlushSend()
    {
        if (_pendingList.Count == 0)
            return;

        List<ArraySegment<byte>> pendingListRef;
        lock (_lock)
        {
            pendingListRef = _pendingList;
            _pendingList = new List<ArraySegment<byte>>();
        }

        Send(pendingListRef);
    }
}
