namespace Server;

using Microsoft.EntityFrameworkCore;
using Protocol;
using Server.Content;
using Server.Content.Job;
using Server.Content.Object;
using Server.Data;
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

    public static void SaveMonsterReward(Player player, GameRoom? room, RewardData reward)
    {
        if (player == null || room == null || reward == null)
            return;

        int? emptySlot = player.Inven.GetEmptySlot();
        if (emptySlot == null)
        {
            Console.WriteLine("보상을 받을 Slot이 존재하지 않습니다.");
            return;
        }

        ItemDb itemDb = new ItemDb()
        {
            TemplateId = reward.itemId,
            Count = reward.count,
            Slot = emptySlot.Value,
            OwnerDbId = player.PlayerDbId
        };

        Instance.Push(() =>
        {
            using (AppDbContext db = new AppDbContext())
            {
                db.Items.Add(itemDb);
                bool success = db.SaveChangesEx();
                if (success)
                {
                    room.Push(() =>
                    {
                        // 임시 Log
                        Console.WriteLine($"Item Db Add Success");
                        Item? item = Item.MakeItem(itemDb);
                        if (item == null)
                            return;

                        player.Inven.Add(item);

                        S2C_UpdateItem updateItemPacket = new S2C_UpdateItem() { ItemInfo = new ItemInfo() };
                        updateItemPacket.ItemInfo.MergeFrom(item.Info);
                        player.Session.Send(updateItemPacket);
                    });
                }
            }
        });
    }
}
