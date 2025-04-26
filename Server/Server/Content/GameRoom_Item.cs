using Server.Content.Object;

namespace Server.Content;

using System.Numerics;
using Google.Protobuf;
using Protocol;
using Server.Content.Job;
using Server.Data;

public partial class GameRoom : JobSerializer
{
    public void ApplyEquipItem(Player player, int itemDbId, bool equiped)
    {
        if (player == null)
            return;

        Item? item = player.Inven.GetItem(itemDbId);
        if (item == null)
            return;

        // 메모리 선 적용
        item.Equiped = equiped;

        // DB 비동기 요청
        DBTransaction.SaveEquipItem(player, item);

        S2C_EquipItem equipItemPacket = new S2C_EquipItem();
        equipItemPacket.ItemDbId = itemDbId;
        equipItemPacket.Equiped = equiped;
        player.Session.Send(equipItemPacket);
    }
}
