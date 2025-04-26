using System;
using System.Collections.Generic;
using Google.Protobuf;
using Protocol;
using ServerCore;

internal class ClientPacketManager
{
    #region Singleton

    private static ClientPacketManager _instance = null!;

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

    private Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _makePacketMap = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
    private Dictionary<ushort, Action<PacketSession, IMessage>> _packetHandlerMap = new Dictionary<ushort, Action<PacketSession, IMessage>>();

    public ClientPacketManager() => Register();

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
        _makePacketMap.Add((ushort)PacketID.S2CEnterRoom, MakePacket<S2C_EnterRoom>);
		_packetHandlerMap.Add((ushort)PacketID.S2CEnterRoom, PacketHandler.S2C_EnterRoomHandler);_makePacketMap.Add((ushort)PacketID.S2CLeaveRoom, MakePacket<S2C_LeaveRoom>);
		_packetHandlerMap.Add((ushort)PacketID.S2CLeaveRoom, PacketHandler.S2C_LeaveRoomHandler);_makePacketMap.Add((ushort)PacketID.S2CSpawn, MakePacket<S2C_Spawn>);
		_packetHandlerMap.Add((ushort)PacketID.S2CSpawn, PacketHandler.S2C_SpawnHandler);_makePacketMap.Add((ushort)PacketID.S2CDespawn, MakePacket<S2C_Despawn>);
		_packetHandlerMap.Add((ushort)PacketID.S2CDespawn, PacketHandler.S2C_DespawnHandler);_makePacketMap.Add((ushort)PacketID.S2CMove, MakePacket<S2C_Move>);
		_packetHandlerMap.Add((ushort)PacketID.S2CMove, PacketHandler.S2C_MoveHandler);_makePacketMap.Add((ushort)PacketID.S2CSkill, MakePacket<S2C_Skill>);
		_packetHandlerMap.Add((ushort)PacketID.S2CSkill, PacketHandler.S2C_SkillHandler);_makePacketMap.Add((ushort)PacketID.S2CUpdateHp, MakePacket<S2C_UpdateHp>);
		_packetHandlerMap.Add((ushort)PacketID.S2CUpdateHp, PacketHandler.S2C_UpdateHpHandler);_makePacketMap.Add((ushort)PacketID.S2CDie, MakePacket<S2C_Die>);
		_packetHandlerMap.Add((ushort)PacketID.S2CDie, PacketHandler.S2C_DieHandler);_makePacketMap.Add((ushort)PacketID.S2CConnected, MakePacket<S2C_Connected>);
		_packetHandlerMap.Add((ushort)PacketID.S2CConnected, PacketHandler.S2C_ConnectedHandler);_makePacketMap.Add((ushort)PacketID.S2CLogin, MakePacket<S2C_Login>);
		_packetHandlerMap.Add((ushort)PacketID.S2CLogin, PacketHandler.S2C_LoginHandler);_makePacketMap.Add((ushort)PacketID.S2CCreatePlayer, MakePacket<S2C_CreatePlayer>);
		_packetHandlerMap.Add((ushort)PacketID.S2CCreatePlayer, PacketHandler.S2C_CreatePlayerHandler);_makePacketMap.Add((ushort)PacketID.S2CItemList, MakePacket<S2C_ItemList>);
		_packetHandlerMap.Add((ushort)PacketID.S2CItemList, PacketHandler.S2C_ItemListHandler);_makePacketMap.Add((ushort)PacketID.S2CUpdateItem, MakePacket<S2C_UpdateItem>);
		_packetHandlerMap.Add((ushort)PacketID.S2CUpdateItem, PacketHandler.S2C_UpdateItemHandler);_makePacketMap.Add((ushort)PacketID.S2CEquipItem, MakePacket<S2C_EquipItem>);
		_packetHandlerMap.Add((ushort)PacketID.S2CEquipItem, PacketHandler.S2C_EquipItemHandler);
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