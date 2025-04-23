namespace Server.Content.Object;

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Protocol;
using Server.Content;
using Server.DB;

public class Player : GameObject
{
    public Player()
    {
        ObjectType = ObjectType.Player;
    }

    public Inventory Inven { get; private set; } = new Inventory();

    public int PlayerDbId { get; set; }

    public override void OnDamaged(GameObject attacker, int damaged)
    {
        base.OnDamaged(attacker, damaged);
    }

    public void OnLeaveRoom()
    {
        // DB 연동
        // 비동기 호출
        DBTransaction.SavePlayerStatInfo(this, Room);
    }
}
