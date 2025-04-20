using ServerCore;
using System;
using System.Collections.Generic;

internal class ClientPacketManager
{
    #region Singleton

    private static ClientPacketManager _instance;

    public static ClientPacketManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ClientPacketManager();

            return _instance;
        }
    }

    #endregion Singleton

    private Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makePacketMap = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    private Dictionary<ushort, Action<PacketSession, IPacket>> _packetHandlerMap = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public ClientPacketManager() => Register();

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
        _makePacketMap.Add((ushort)PacketID.S2C_PlayerInfoRes, MakePacket<S2C_PlayerInfoRes>);
		_packetHandlerMap.Add((ushort)PacketID.S2C_PlayerInfoRes, PacketHandler.S2C_PlayerInfoResHandler);
		_makePacketMap.Add((ushort)PacketID.S2C_Chat, MakePacket<S2C_Chat>);
		_packetHandlerMap.Add((ushort)PacketID.S2C_Chat, PacketHandler.S2C_ChatHandler);
		
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