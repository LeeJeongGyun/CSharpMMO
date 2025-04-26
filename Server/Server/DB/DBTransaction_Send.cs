namespace Server;

using Microsoft.EntityFrameworkCore;
using Protocol;
using Server.Content;
using Server.Content.Job;
using Server.Content.Object;
using Server.Data;
using Server.DB;
using Server.Utils;

public partial class DBTransaction : JobSerializer
{
    public static void SaveEquipItem(Player player, Item item)
    {
        if (player == null || item == null)
            return;

        ItemDb itemDb = new ItemDb()
        {
            ItemDbId = item.ItemDbId,
            OwnerDbId = player.PlayerDbId,
            Equiped = item.Equiped
        };

        Instance.Push(() =>
        {
            // DB 비동기 저장
            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(itemDb).State = EntityState.Unchanged;
                db.Entry(itemDb).Property(nameof(ItemDb.Equiped)).IsModified = true;
                bool success = db.SaveChangesEx();
                if (!success)
                    Console.WriteLine("Item Equiped Fail");
            }
        });
    }
}
