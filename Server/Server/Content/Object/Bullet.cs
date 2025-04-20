namespace Server.Content.Object;

using Protocol;
using Server.Data;

public class Bullet : Projectile
{
    private long _moveTick = 0;

    public GameObject Owner { get; set; }

    public float Speed { get; init; }

    public override void Update()
    {
        if (_moveTick > Environment.TickCount64)
            return;

        if (Room == null || Owner == null || SkillData == null || SkillData.projectile == null)
            return;

        CellPos = GetFrontCellPos();
        if (Room.Map.FindCollision(CellPos))
        {
            Room.Push(Room.LeaveRoom, ObjectType.Projectile, ObjectId);
            return;
        }

        GameObject? target = Room.Map.FindObject(CellPos);
        if (target != null)
        {
            target.OnDamaged(Owner, SkillData.damage);
            Room.Push(Room.LeaveRoom, ObjectType.Projectile, ObjectId);
            return;
        }

        S2C_Move movePacket = new S2C_Move();
        movePacket.ObjectId = ObjectId;
        movePacket.PosInfo = PosInfo;
        Room.BroadcastMessage(movePacket);

        long speedToTick = (long)(1000 / SkillData.projectile.speed);
        _moveTick = Environment.TickCount64 + speedToTick;
    }
}
