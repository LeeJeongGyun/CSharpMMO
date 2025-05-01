namespace Server.Content.Object;

using Protocol;
using Server.Content.Job;
using Server.Content.Room;
using Server.Data;

public class Monster : GameObject
{
    private Player? _target = null;

    private long _nextSearchTick;
    private long _nextMoveTick;
    private long _skillDelayTick;

    private int _searchDistance = 10;
    private int _chaseDistance = 15;

    private int _skillRange = 1;

    private IJob? _lastJob;

    public Monster()
    {
        ObjectType = ObjectType.Monster;
    }

    public int TemplateId { get; private set; }

    public void Init(int templateId)
    {
        TemplateId = templateId;

        DataManager.Monsters.TryGetValue(templateId, out MonsterData? monsterData);
        if (monsterData == null)
            return;

        StatInfo.MergeFrom(monsterData.statInfo);
        StatInfo.Hp = monsterData.statInfo.MaxHp;
        State = ObjectState.Idle;
    }

    // FSM 적용
    public void Update()
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

        // Per 5 프레임
        _lastJob = Room?.PushAfter(Update, 100);
    }

    public void CancelJob()
    {
        if (_lastJob != null)
        {
            _lastJob.Cancel = true;
            _lastJob = null;
        }
    }

    protected virtual void UpdateIdle()
    {
        if (_nextSearchTick > Environment.TickCount64)
            return;

        _nextSearchTick = Environment.TickCount64 + 1000;

        if (Room == null)
            return;

        _target = Room.FindPlayer(p =>
        {
            Vector2Int dir = p.CellPos - CellPos;
            if (dir.cellDistance > _searchDistance)
                return false;

            return true;
        });

        if (_target != null)
            State = ObjectState.Moving;
    }

    protected virtual void UpdateMoving()
    {
        if (_nextMoveTick > Environment.TickCount64)
            return;

        long speedToTick = (long)(1000 / StatInfo.Speed);
        _nextMoveTick = Environment.TickCount64 + speedToTick;

        if (_target == null || _target.Room == null || _target.Room != Room)
        {
            State = ObjectState.Idle;
            SendBroadcastMovePacket();
            return;
        }

        Vector2Int dir = _target.CellPos - CellPos;
        int dist = dir.cellDistance;
        if (dist == 0 || dist > _chaseDistance)
        {
            _target = null;
            State = ObjectState.Idle;
            SendBroadcastMovePacket();
            return;
        }

        // 스킬 사용 가능 체크
        if (dist <= _skillRange && (dir.x == 0 || dir.y == 0))
        {
            _skillDelayTick = 0;
            State = ObjectState.Skill;
            return;
        }

        List<Vector2Int> paths = Room.Map.FindPath(CellPos, _target.CellPos, checkObject: true);
        if (paths.Count < 2 || paths.Count > _chaseDistance)
        {
            _target = null;
            State = ObjectState.Idle;
            SendBroadcastMovePacket();
            return;
        }

        State = ObjectState.Moving;
        Dir = GetDirFromVec(paths[1] - CellPos);
        Room.Map.MoveObject(this, paths[1]);
        SendBroadcastMovePacket();
    }

    protected virtual void UpdateSkill()
    {
        if (_skillDelayTick == 0)
        {
            // 유효 타겟
            if (_target == null || _target.Room != Room || _target.StatInfo.Hp == 0)
            {
                _target = null;
                State = ObjectState.Idle;
                SendBroadcastMovePacket();
                return;
            }

            // 스킬 사용 가능 확인
            Vector2Int dir = _target.CellPos - CellPos;
            bool canUseSkill = (dir.cellDistance <= _skillRange && (dir.x == 0 || dir.y == 0));
            if (canUseSkill == false)
            {
                _target = null;
                State = ObjectState.Idle;
                SendBroadcastMovePacket();
                return;
            }

            // 몬스터가 보는 방향과 타겟의 방향이 다르다면 변경
            MoveDir targetDir = GetDirFromVec(dir);
            if (targetDir != Dir)
            {
                Dir = targetDir;
                SendBroadcastMovePacket();
            }

            // 데미지 판정
            DataManager.Skills.TryGetValue(1, out Skill? skillData);
            _target.OnDamaged(this, skillData!.damage);

            // 스킬 사용 송신
            S2C_Skill skillPacket = new S2C_Skill() { SkillInfo = new SkillInfo() };
            skillPacket.ObjectId = ObjectId;
            skillPacket.SkillInfo.SkillId = skillData.id;
            Room?.BroadcastMessage(CellPos, skillPacket);

            // 스킬 딜레이 타임 갱신
            long skillDelayTick = (int)(1000 * skillData.cooldown);
            _skillDelayTick = Environment.TickCount64 + skillDelayTick;
        }

        if (_skillDelayTick > Environment.TickCount64)
            return;

        _skillDelayTick = 0;
    }

    protected virtual void UpdateDead()
    { }

    protected override void OnDead(GameObject attacker)
    {
        CancelJob();
        base.OnDead(attacker);

        if (attacker.GetOwner() is not Player)
            return;

        RewardData? reward = GetRandomRewardData();
        if (reward == null)
            return;

        DBTransaction.SaveMonsterReward((Player)attacker, Room, reward);
    }

    private RewardData? GetRandomRewardData()
    {
        DataManager.Monsters.TryGetValue(TemplateId, out MonsterData? monsterData);
        if (monsterData == null)
            return null;

        int randNum = new Random().Next(0, 101);
        int sum = 0;

        foreach (var reward in monsterData.rewards)
        {
            sum += reward.probability;
            if (randNum <= sum)
                return reward;
        }

        return null;
    }

    private void SendBroadcastMovePacket()
    {
        S2C_Move movePacket = new S2C_Move();
        movePacket.ObjectId = ObjectId;
        movePacket.PosInfo = PosInfo;
        Room?.BroadcastMessage(CellPos, movePacket);
    }
}
