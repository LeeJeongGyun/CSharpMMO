using ServerCore;
using System;
using System.Collections.Generic;

internal class ServerPacketManager
{
    #region Singleton

    private static ServerPacketManager _instance;

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

    private Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makePacketMap = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    private Dictionary<ushort, Action<PacketSession, IPacket>> _packetHandlerMap = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public ServerPacketManager() => Register();

    public void MatchPacketHandler(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> action = null)
    {
        ushort packetId = BitConverter.ToUInt16(buffer.Array!, buffer.Offset + 2);

        Func<PacketSession, ArraySegment<byte>, IPacket> func;
        if (_makePacketMap.TryGetValue(packetId, out func))
        {
            IPacket pkt = func.Invoke(session, buffer);
            if (action != null)
                action.Invoke(session, pkt);
            else
                HandlePacket(session, pkt);
        }
    }

    public void Register()
    {
        _makePacketMap.Add((ushort)PacketID.C2S_PlayerInfoReq, MakePacket<C2S_PlayerInfoReq>);
		_packetHandlerMap.Add((ushort)PacketID.C2S_PlayerInfoReq, PacketHandler.C2S_PlayerInfoReqHandler);
		_makePacketMap.Add((ushort)PacketID.C2S_Chat, MakePacket<C2S_Chat>);
		_packetHandlerMap.Add((ushort)PacketID.C2S_Chat, PacketHandler.C2S_ChatHandler);
		
    }

    public void HandlePacket(PacketSession session, IPacket packet)
    {
        Action<PacketSession, IPacket> packetHandler;
        if (_packetHandlerMap.TryGetValue(packet.PacketId, out packetHandler))
            packetHandler.Invoke(session, packet);
    }

    private T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        var packet = new T();
        packet.Deserialize(buffer);
        return packet;
    }
}