using Protocol;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    protected Animator _animator;
    protected SpriteRenderer _spriteRenderer;
    protected Vector3 _fixedOffset = new Vector3(0.5f, 0.6f, 0);

    private ObjectInfo _info = new ObjectInfo();

    public ObjectInfo Info
    {
        get => _info;
        set
        {
            if (_info.Equals(value))
                return;

            _info = value;
            UpdateAnimation();
        }
    }

    public PositionInfo PosInfo
    {
        get => _info.PosInfo;
        set
        {
            if (_info.PosInfo.Equals(value))
                return;

            _info.PosInfo = value;
            UpdateAnimation();
        }
    }

    public StatInfo StatInfo
    {
        get => Info.StatInfo;
        set => Info.StatInfo = value;
    }

    public MoveDir Dir
    {
        get => PosInfo.Dir;
        set
        {
            if (PosInfo.Dir == value)
                return;

            PosInfo.Dir = value;
            UpdateAnimation();
        }
    }

    public ObjectState State
    {
        get => PosInfo.State;
        set
        {
            if (PosInfo.State == value)
                return;

            PosInfo.State = value;
            UpdateAnimation();
        }
    }

    public Vector3Int CellPos
    {
        get => new Vector3Int(PosInfo.PosX, PosInfo.PosY);
        set
        {
            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
        }
    }

    public int Id => Info.ObjectId;

    protected virtual void Init()
    {
        transform.position = Managers.Map.CurrentGrid.CellToWorld(CellPos) + _fixedOffset;
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
        case ObjectState.Idle:
            UpdateIdle();
            break;

        case ObjectState.Moving:
            UpdateMoving();
            break;

        case ObjectState.Skill:
            UpdateSkill();
            break;

        case ObjectState.Dead:
            UpdateDead();
            break;
        }
    }

    protected virtual void UpdateIdle()
    {
    }

    protected virtual void UpdateSkill()
    {
    }

    protected virtual void UpdateDead()
    {
    }

    protected virtual void CanKeepMoving()
    {
        Vector3Int dstCellPos = CellPos;
        switch (Dir)
        {
        case MoveDir.Up:
            dstCellPos += Vector3Int.up;
            break;

        case MoveDir.Down:
            dstCellPos += Vector3Int.down;
            break;

        case MoveDir.Left:
            dstCellPos += Vector3Int.left;
            break;

        case MoveDir.Right:
            dstCellPos += Vector3Int.right;
            break;
        }

        // 맵 충돌 처리
        if (Managers.Map.FindCollision(dstCellPos))
        {
            State = ObjectState.Idle;
            return;
        }

        // 몬스터 or 플레이어 충돌 처리
        GameObject go = Managers.Object.FindObject(dstCellPos);
        if (go != null)
        {
            State = ObjectState.Idle;
            return;
        }

        CellPos = dstCellPos;
    }

    protected virtual void UpdateAnimation()
    {
    }

    protected virtual void UpdateMoving()
    {
        Vector3 dstWorldPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + _fixedOffset;
        Vector3 dirVec = dstWorldPos - transform.position;
        float moveDist = StatInfo.Speed * Time.deltaTime;

        if (dirVec.magnitude < moveDist)
        {
            transform.position = dstWorldPos;
            CanKeepMoving();
        }
        else
            transform.position += dirVec.normalized * StatInfo.Speed * Time.deltaTime;
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        UpdateController();
    }
}
