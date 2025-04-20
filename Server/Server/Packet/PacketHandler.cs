using Google.Protobuf;
using Protocol;
using Server;
using Server.Content;
using Server.Content.Object;
using ServerCore;
using System;

internal class PacketHandler
{
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
}
