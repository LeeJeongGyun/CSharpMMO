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
        ServerSession serverSession = (ServerSession)session;

        C2S_Login loginPacket = new C2S_Login();
        loginPacket.UniqueId = $"Dummy_{serverSession.DummyId}";
        serverSession.Send(loginPacket);
    }

    public static void S2C_LoginHandler(PacketSession session, IMessage message)
    {
        ServerSession serverSession = (ServerSession)session;
        S2C_Login loginPacket = message as S2C_Login;

        if (loginPacket.PlayerInfos.Count == 0)
        {
            C2S_CreatePlayer createPlayerPacket = new C2S_CreatePlayer();
            createPlayerPacket.Name = $"Dummy_{serverSession.DummyId.ToString("0000")}";
            serverSession.Send(createPlayerPacket);
        }
        else
        {
            C2S_EnterRoom enterRoomPacket = new C2S_EnterRoom();
            enterRoomPacket.Name = loginPacket.PlayerInfos[0].Name;
            serverSession.Send(enterRoomPacket);
        }
    }

    public static void S2C_CreatePlayerHandler(PacketSession session, IMessage message)
    {
        ServerSession serverSession = (ServerSession)session;
        S2C_CreatePlayer createPlayer = message as S2C_CreatePlayer;

        C2S_EnterRoom enterRoomPacket = new C2S_EnterRoom();
        enterRoomPacket.Name = createPlayer.PlayerInfo.Name;
        serverSession.Send(enterRoomPacket);
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
        ServerSession serverSession = (ServerSession)session;
        C2S_Pong pongPacket = new C2S_Pong();
        serverSession.Send(pongPacket);
    }
}
