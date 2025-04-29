using Server.Content.Object;

namespace Server.Content;

using System.Numerics;
using Google.Protobuf;
using Protocol;
using Server.Content.Job;
using Server.Data;
using Server.DB;

public partial class GameRoom : JobSerializer
{
    public void ApplyEquipItem(Player player, C2S_EquipItem equipItemPacket)
    {
        if (player == null)
            return;

        player.HandleEquipItemPacket(equipItemPacket);
    }
}
