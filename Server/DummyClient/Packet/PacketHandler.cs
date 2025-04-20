using ServerCore;

internal class PacketHandler
{
    public static void S2C_ChatHandler(PacketSession session, IPacket packet)
    {
        S2C_Chat? pkt = packet as S2C_Chat;
        if (pkt == null)
            throw new InvalidOperationException("PacketHandler 오류");

        //Console.WriteLine($"[CLIENT] RecvData: Id: {pkt.playerId}, Chat: {pkt.chat}");
    }

    public static void S2C_PlayerInfoResHandler(PacketSession session, IPacket packet)
    {
        S2C_PlayerInfoRes? res = packet as S2C_PlayerInfoRes;
        if (res == null)
            return;

        Console.WriteLine($"Result: {res.result}, Id: {res.playerId}, Time: {res.time}, Name: {res.name}");
    }
}