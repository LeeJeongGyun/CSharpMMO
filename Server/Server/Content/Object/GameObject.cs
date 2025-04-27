namespace Server.Content.Object;

using Protocol;

public class GameObject
{
    public GameObject()
    {
        Info = new ObjectInfo();
        PosInfo = new PositionInfo();
        StatInfo = new StatInfo();

        Info.PosInfo = PosInfo;
        Info.StatInfo = StatInfo;
    }

    public int ObjectId => Info.ObjectId;

    public ObjectType ObjectType
    {
        get => Info.ObjectType;

        protected set
        {
            Info.ObjectType = value;
        }
    }

    public ObjectInfo Info { get; set; }

    public PositionInfo PosInfo { get; set; }

    public GameRoom? Room { get; set; }

    public ClientSession Session { get; set; }

    public StatInfo StatInfo { get; private set; }

    public Vector2Int CellPos
    {
        get => new Vector2Int(PosInfo.PosX, PosInfo.PosY);
        set
        {
            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
        }
    }

    public ObjectState State
    {
        get => PosInfo.State;
        set => PosInfo.State = value;
    }

    public MoveDir Dir
    {
        get => PosInfo.Dir;
        set => PosInfo.Dir = value;
    }

    public string Name
    {
        get => Info.Name;
        set => Info.Name = value;
    }

    public virtual int TotalDamage => StatInfo.Attack;

    public virtual int TotalDefence => 0;

    // Bullet와 같은 Projectile은 Map에 등록을 하지 않아 충돌되지 않음
    public virtual void OnDamaged(GameObject attacker, int damaged)
    {
        int totalDamaged = (TotalDamage + damaged) - TotalDefence;
        totalDamaged = Math.Max(totalDamaged, 0);
        StatInfo.Hp = Math.Max(StatInfo.Hp - totalDamaged, 0);

        S2C_UpdateHp updateHpPacket = new S2C_UpdateHp();
        updateHpPacket.ObjectId = ObjectId;
        updateHpPacket.Hp = StatInfo.Hp;
        Room?.BroadcastMessage(updateHpPacket);

        if (StatInfo.Hp == 0)
        {
            OnDead(attacker);
            return;
        }

        Console.WriteLine($"{ObjectId} is Damaged, Hp: {StatInfo.Hp}");
    }

    public Vector2Int GetFrontCellPos() => Dir switch
    {
        MoveDir.Up => CellPos + Vector2Int.Up,
        MoveDir.Down => CellPos + Vector2Int.Down,
        MoveDir.Left => CellPos + Vector2Int.Left,
        MoveDir.Right => CellPos + Vector2Int.Right,
        _ => CellPos
    };

    public MoveDir GetDirFromVec(Vector2Int cellPos)
    {
        if (cellPos.y > 0)
            return MoveDir.Up;
        else if (cellPos.y < 0)
            return MoveDir.Down;
        else if (cellPos.x < 0)
            return MoveDir.Left;
        else
            return MoveDir.Right;
    }

    public virtual GameObject GetOwner()
    {
        return this;
    }

    protected virtual void OnDead(GameObject attacker)
    {
        S2C_Die diePacket = new S2C_Die();
        diePacket.ObjectId = ObjectId;
        GameRoom? room = Room;
        if (room != null)
        {
            room.BroadcastMessage(diePacket);
            room.LeaveRoom(ObjectType, ObjectId);
            ResetPlayerInfo();
            room.EnterRoom(this);
        }

        void ResetPlayerInfo()
        {
            StatInfo.Hp = StatInfo.MaxHp;
            State = ObjectState.Idle;
            Dir = MoveDir.Down;
            PosInfo.PosX = 0;
            PosInfo.PosY = 0;
        }
    }
}
