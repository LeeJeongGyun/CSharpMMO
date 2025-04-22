using System;
using System.Collections.Generic;
using Google.Protobuf;
using Protocol;
using ServerCore;

internal class ServerPacketManager
{
    #region Singleton

    private static ServerPacketManager _instance = null!;

    public static ServerPacketManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ServerPacketManager();

            return _instance;
        }
    }

    #endregion Singleton

    private Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _makePacketMap = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
    private Dictionary<ushort, Action<PacketSession, IMessage>> _packetHandlerMap = new Dictionary<ushort, Action<PacketSession, IMessage>>();

    public ServerPacketManager() => Register();

    // Unity에서만 사용
    public Action<ushort, IMessage> CustomHandler { get; set; }

    public void MatchPacketHandler(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort packetId = BitConverter.ToUInt16(buffer.Array!, buffer.Offset + 2);

        Action<PacketSession, ArraySegment<byte>, ushort>? action;
        if (_makePacketMap.TryGetValue(packetId, out action))
            action.Invoke(session, buffer, packetId);
    }

    public void Register()
    {
        _makePacketMap.Add((ushort)PacketID.C2SMove, MakePacket<C2S_Move>);
		_packetHandlerMap.Add((ushort)PacketID.C2SMove, PacketHandler.C2S_MoveHandler);_makePacketMap.Add((ushort)PacketID.C2SSkill, MakePacket<C2S_Skill>);
		_packetHandlerMap.Add((ushort)PacketID.C2SSkill, PacketHandler.C2S_SkillHandler);_makePacketMap.Add((ushort)PacketID.C2SLogin, MakePacket<C2S_Login>);
		_packetHandlerMap.Add((ushort)PacketID.C2SLogin, PacketHandler.C2S_LoginHandler);
    }

    // Unity에서만 사용
    public Action<PacketSession, IMessage> GetPacketHandler(ushort packetId)
    {
        Action<PacketSession, IMessage>? packetHandler;
        _packetHandlerMap.TryGetValue(packetId, out packetHandler);
        return packetHandler;
    }

    private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort packetId) where T : IMessage, new()
    {
        IMessage packet = new T();
        packet.MergeFrom(buffer.Slice(4));

        if (CustomHandler != null)
            CustomHandler.Invoke(packetId, packet);
        else
        {
            Action<PacketSession, IMessage>? packetHandler;
            if (_packetHandlerMap.TryGetValue(packetId, out packetHandler))
                packetHandler.Invoke(session, packet);
        }
    }
}