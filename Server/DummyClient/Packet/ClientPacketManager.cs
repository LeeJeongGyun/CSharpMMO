using ServerCore;

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

    private Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _makePacketMap = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    private Dictionary<ushort, Action<PacketSession, IPacket>> _packetHandlerMap = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public ClientPacketManager() => Register();

    public void MatchPacketHandler(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort packetId = BitConverter.ToUInt16(buffer.Array!, buffer.Offset + 2);

        Action<PacketSession, ArraySegment<byte>>? action;
        if (_makePacketMap.TryGetValue(packetId, out action))
            action.Invoke(session, buffer);
    }

    public void Register()
    {
        _makePacketMap.Add((ushort)PacketID.S2C_PlayerInfoRes, MakePacket<S2C_PlayerInfoRes>);
		_packetHandlerMap.Add((ushort)PacketID.S2C_PlayerInfoRes, PacketHandler.S2C_PlayerInfoResHandler);
		_makePacketMap.Add((ushort)PacketID.S2C_Chat, MakePacket<S2C_Chat>);
		_packetHandlerMap.Add((ushort)PacketID.S2C_Chat, PacketHandler.S2C_ChatHandler);
		
    }

    private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        IPacket packet = new T();
        packet.Deserialize(buffer);

        Action<PacketSession, IPacket>? packetHandler;
        if (_packetHandlerMap.TryGetValue(packet.PacketId, out packetHandler))
            packetHandler.Invoke(session, packet);
    }
}