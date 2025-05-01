namespace DummyTest.Packet;

using DummyTest.Session;
using Google.Protobuf;
using Protocol;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using ServerCore;

public class PacketHandler
{
    public static void S2C_EnterRoomHandler(PacketSession session, IMessage message)
    {
        S2C_EnterRoom enterRoomPacket = message as S2C_EnterRoom;
    }

    public static void S2C_LeaveRoomHandler(PacketSession session, IMessage message)
    {
        S2C_LeaveRoom leaveRoomPacket = message as S2C_LeaveRoom;
    }

    public static void S2C_SpawnHandler(PacketSession session, IMessage message)
    {
        S2C_Spawn spawnPacket = message as S2C_Spawn;
    }

    public static void S2C_DespawnHandler(PacketSession session, IMessage message)
    {
        S2C_Despawn despawnPacket = message as S2C_Despawn;
    }

    public static void S2C_MoveHandler(PacketSession session, IMessage message)
    {
        S2C_Move movePacket = message as S2C_Move;
    }

    public static void S2C_SkillHandler(PacketSession session, IMessage message)
    {
        S2C_Skill skillPacket = message as S2C_Skill;
    }

    public static void S2C_UpdateHpHandler(PacketSession session, IMessage message)
    {
        S2C_UpdateHp updateHpPacket = message as S2C_UpdateHp;
    }

    public static void S2C_DieHandler(PacketSession session, IMessage message)
    {
        S2C_Die updateHpPacket = message as S2C_Die;
    }

    public static void S2C_ConnectedHandler(PacketSession session, IMessage message)
    {
    }

    public static void S2C_LoginHandler(PacketSession session, IMessage message)
    {
        S2C_Login loginPacket = message as S2C_Login;
    }

    public static void S2C_CreatePlayerHandler(PacketSession session, IMessage message)
    {
        S2C_CreatePlayer createPlayer = message as S2C_CreatePlayer;
    }

    public static void S2C_ItemListHandler(PacketSession session, IMessage message)
    {
        S2C_ItemList itemListPacket = message as S2C_ItemList;
    }

    public static void S2C_UpdateItemHandler(PacketSession session, IMessage message)
    {
        S2C_UpdateItem updateItemPacket = message as S2C_UpdateItem;
    }

    public static void S2C_EquipItemHandler(PacketSession session, IMessage message)
    {
        S2C_EquipItem equipItemPacket = message as S2C_EquipItem;
    }

    public static void S2C_PingHandler(PacketSession session, IMessage message)
    {
    }
}
