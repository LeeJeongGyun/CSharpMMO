using Google.Protobuf;
using Protocol;
using Server;
using Server.Content;
using Server.Content.Object;
using Server.DB;
using ServerCore;
using System;

internal class PacketHandler
{
    public static void C2S_LoginHandler(PacketSession session, IMessage packet)
    {
        C2S_Login loginPacket = packet as C2S_Login;
        if (loginPacket == null)
            return;

        ClientSession clientSession = session as ClientSession;
        if (clientSession == null)
            return;

        clientSession.HandleLogin(loginPacket);
    }

    public static void C2S_EnterRoomHandler(PacketSession session, IMessage packet)
    {
        C2S_EnterRoom enterPacket = packet as C2S_EnterRoom;
        if (enterPacket == null)
            return;

        ClientSession clientSession = session as ClientSession;
        if (clientSession == null)
            return;

        clientSession.HandleEnterRoom(enterPacket);
    }

    public static void C2S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        C2S_CreatePlayer createPlayerPacket = packet as C2S_CreatePlayer;
        if (createPlayerPacket == null)
            return;

        ClientSession clientSession = session as ClientSession;
        if (clientSession == null)
            return;

        clientSession.HandleCreatePlayer(createPlayerPacket);
    }

    public static void C2S_MoveHandler(PacketSession session, IMessage packet)
    {
        C2S_Move? movePacket = packet as C2S_Move;
        if (movePacket == null)
            return;

        ClientSession? clientSession = session as ClientSession;
        if (clientSession == null)
            return;

        Player? player = ObjectManager.Instance.FindPlayer(clientSession.ObjectId);
        if (player == null)
            return;

        GameRoom? room = player.Room;
        if (room == null)
            return;

        room.Push(room.ApplyMove, player, movePacket.PosInfo);
    }

    public static void C2S_SkillHandler(PacketSession session, IMessage packet)
    {
        C2S_Skill? skillPacket = packet as C2S_Skill;
        if (skillPacket == null)
            return;

        ClientSession? clientSession = session as ClientSession;
        if (clientSession == null)
            return;

        Player? player = ObjectManager.Instance.FindPlayer(clientSession.ObjectId);
        if (player == null)
            return;

        GameRoom? room = player.Room;
        if (room == null)
            return;

        room.Push(room.ApplySkill, player, skillPacket.SkillInfo);
    }

    public static void C2S_EquipItemHandler(PacketSession session, IMessage packet)
    {
        C2S_EquipItem? equipItemPacket = packet as C2S_EquipItem;
        if (equipItemPacket == null)
            return;

        ClientSession? clientSession = session as ClientSession;
        if (clientSession == null)
            return;

        Player? player = ObjectManager.Instance.FindPlayer(clientSession.ObjectId);
        if (player == null)
            return;

        GameRoom? room = player.Room;
        if (room == null)
            return;

        room.Push(room.ApplyEquipItem, player, equipItemPacket);
    }
}
