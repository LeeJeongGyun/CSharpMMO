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

        // TODO 보안 체크 필요..

        Console.WriteLine($"UniqueId: {loginPacket.UniqueId}");

        // TODO 문제가 존재
        using (AppDbContext db = new AppDbContext())
        {
            AccountDb? account = db.Accounts.Where(account => account.AccountName == loginPacket.UniqueId).FirstOrDefault();
            if (account == null)
            {
                db.Accounts.Add(new AccountDb() { AccountName = loginPacket.UniqueId });
                db.SaveChanges();
            }
        }

        S2C_Login loginPacketRes = new S2C_Login();
        loginPacketRes.LoginOk = 1;
        (session as ClientSession)?.Send(loginPacketRes);
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
}
