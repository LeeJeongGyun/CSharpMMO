namespace Server;

using Microsoft.EntityFrameworkCore;
using Protocol;
using Server.Content;
using Server.Content.Job;
using Server.Content.Object;
using Server.DB;
using Server.Utils;

public class DBTransaction : JobSerializer
{
    public static DBTransaction Instance { get; } = new DBTransaction();

    public static void SavePlayerStatInfo(Player player, GameRoom? room)
    {
        if (player == null || room == null)
            return;

        PlayerDb playerDb = new PlayerDb()
        {
            PlayerDbId = player.PlayerDbId,
            StatInfo = player.StatInfo,
        };

        // 비동기로 DB 처리 요청
        Instance.Push(() =>
        {
            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(playerDb).State = EntityState.Unchanged;
                db.Entry(playerDb.StatInfo).State = EntityState.Unchanged;
                db.Entry(playerDb.StatInfo).Property(nameof(StatInfo.Hp)).IsModified = true;
                bool success = db.SaveChangesEx();
                if (success)
                {
                    // 비동기로 다시 알려주기
                    room.Push(() =>
                    {
                        // 작업이 완료되었을 때 처리해야 될 일감
                        Console.WriteLine($"Saved Hp: {playerDb.StatInfo.Hp}");
                    });
                }
            }
        });
    }
}
