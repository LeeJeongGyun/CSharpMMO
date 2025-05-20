namespace Server.Content.Object;

using Protocol;
using Server.Content.Room;

public class Bullet : Projectile
{
    public GameObject Owner { get; set; }

    public float Speed { get; init; }

    public override void Update()
    {
        if (Room == null || Owner == null || SkillData == null || SkillData.projectile == null)
            return;

        Vector2Int curCellPos = CellPos;
        Vector2Int frontCellPos = GetFrontCellPos();

        if (Room.Map.FindCollision(frontCellPos))
        {
            Room.Push(Room.LeaveRoom, ObjectType.Projectile, ObjectId);
            return;
        }

        GameObject? target = Room.Map.FindObject(frontCellPos);
        if (target != null)
        {
            target.OnDamaged(Owner, SkillData.damage);
            Room.Push(Room.LeaveRoom, ObjectType.Projectile, ObjectId);
            return;
        }

        // 좌표 갱신
        CellPos = frontCellPos;

        // Zone 처리
        Zone curZone = Room.GetZone(curCellPos);
        Zone moveZone = Room.GetZone(CellPos);
        if (curZone != moveZone)
        {
            curZone.Projectiles.Remove(this);
            moveZone.Projectiles.Add(this);
        }

        S2C_Move movePacket = new S2C_Move();
        movePacket.ObjectId = ObjectId;
        movePacket.PosInfo = PosInfo;
        Room.BroadcastMessage(CellPos, movePacket);

        // Bullet이 사라졌음에도 계속 Update 호출되는 문제로 인하여 위치 이동
        int speedToTick = (int)(1000 / SkillData.projectile.speed);
        Room.PushAfter(Update, speedToTick);
    }

    public override GameObject GetOwner()
    {
        return Owner;
    }
}
