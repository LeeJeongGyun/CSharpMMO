using Server.Content.Object;

namespace Server.Content;

using System.Numerics;
using Google.Protobuf;
using Protocol;
using Server.Content.Job;
using Server.Data;

public class GameRoom : JobSerializer
{
    private Dictionary<int, Player> _players = new Dictionary<int, Player>();
    private Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
    private Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
    private object _lock = new object();
    private Map _map;

    public GameRoom()
    {
        // TODO 추후 맵이 여러개 된다면 이쪽 수정 필요.
        // 지금은 맵 데이터 1개 밖에 없음으로 하드코딩
        _map = new Map();
        _map.LoadMap(1);

        GenerateMonsterAI(1);
    }

    public int Id { get; init; }

    public Map Map => _map;

    // 주기적으로 호출 필요
    public void Update()
    {
        foreach (var projectile in _projectiles.Values)
            projectile.Update();

        foreach (var monster in _monsters.Values)
            monster.Update();

        Flush();
    }

    public void EnterRoom(GameObject gameObject)
    {
        int objectId = gameObject.ObjectId;

        if (gameObject.ObjectType == ObjectType.Default)
        {
            // TODO FileLog
            Console.WriteLine("ObejctType이 없음");
            return;
        }

        if (gameObject.ObjectType == ObjectType.Player)
        {
            Player player = gameObject as Player;
            player!.Room = this;

            // 방에 입장 등록
            _players.Add(objectId, player);

            // Map에 배치 진행
            _map.InitObjectPosition(gameObject);

            // 1. 입장 패킷 송신
            var enterRoomPacket = new S2C_EnterRoom();
            enterRoomPacket.ObjectInfo = gameObject.Info;
            gameObject.Session.Send(enterRoomPacket);

            // 2. 나에게 상대방 정보 송신
            var spawnPacket = new S2C_Spawn();
            foreach (var other in _players.Values)
            {
                if (other.ObjectId != gameObject.ObjectId)
                    spawnPacket.ObjectInfos.Add(other.Info);
            }

            foreach (var monster in _monsters.Values)
                spawnPacket.ObjectInfos.Add(monster.Info);

            foreach (var projectile in _projectiles.Values)
                spawnPacket.ObjectInfos.Add(projectile.Info);

            if (spawnPacket.ObjectInfos.Count > 0)
                gameObject.Session.Send(spawnPacket);
        }
        else if (gameObject.ObjectType == ObjectType.Monster)
        {
            Monster monster = gameObject as Monster;
            monster!.Room = this;

            _monsters.Add(objectId, monster);
            _map.InitObjectPosition(gameObject);
        }
        else if (gameObject.ObjectType == ObjectType.Projectile)
        {
            Projectile projectile = gameObject as Projectile;
            projectile!.Room = this;

            _projectiles.Add(objectId, gameObject as Projectile);
        }

        // 상대방에게 내 정보 송신
        {
            var spawnPacket = new S2C_Spawn();
            spawnPacket.ObjectInfos.Add(gameObject.Info);
            if (gameObject.ObjectType == ObjectType.Player)
                BroadcastMessage(spawnPacket, excludeId: objectId);
            else
                BroadcastMessage(spawnPacket);
        }
    }

    public void LeaveRoom(ObjectType type, int objectId)
    {
        if (type == ObjectType.Default)
        {
            // TODO FileLog
            Console.WriteLine("ObjectType이 없음");
            return;
        }

        if (type == ObjectType.Player)
        {
            Player? player = null;
            _players.TryGetValue(objectId, out player);

            if (player == null)
                return;

            // 1. 나에게 퇴장 정보 송신
            var leavePacket = new S2C_LeaveRoom();
            player.Session.Send(leavePacket);

            // Player 관련 정보 제거
            player.Room = null;
            _players.Remove(objectId);
            _map.RemoveObject(player);
        }
        else if (type == ObjectType.Monster)
        {
            Monster? monster = null;
            _monsters.TryGetValue(objectId, out monster);
            if (monster == null)
                return;

            monster.Room = null;
            _monsters.Remove(objectId);
            _map.RemoveObject(monster);
        }
        else if (type == ObjectType.Projectile)
        {
            _projectiles.Remove(objectId);
        }

        // 2. 상대방에게 내 퇴장 정보 전달
        var despawnPacket = new S2C_Despawn();
        despawnPacket.ObjectId = objectId;
        BroadcastMessage(despawnPacket);
    }

    public void ApplyMove(Player player, PositionInfo posInfo)
    {
        // State가 Idle일 경우 현재 좌표로 보내기 때문에 예외 처리에 넣어준다.
        if (false == _map.MoveObject(player, new Vector2Int(posInfo.PosX, posInfo.PosY))
             && posInfo.State != ObjectState.Idle)
            return;

        player.State = posInfo.State;
        player.Dir = posInfo.Dir;

        // 이동 패킷 전송
        var movePacket = new S2C_Move();
        movePacket.ObjectId = player.ObjectId;
        movePacket.PosInfo = posInfo;
        BroadcastMessage(movePacket, excludeId: player.ObjectId);
    }

    public void ApplySkill(Player player, SkillInfo skillInfo)
    {
        if (player.State != ObjectState.Idle)
            return;

        Data.Skill? skillData = null;
        if (false == DataManager.Skills.TryGetValue(skillInfo.SkillId, out skillData))
            return;

        switch (skillData.skillType)
        {
        case SkillType.Scythe:
            {
                GameObject? target = _map.FindObject(player.GetFrontCellPos());
                if (target != null)
                    target.OnDamaged(player, skillData.damage);
            }
            break;

        case SkillType.Bullet:
            {
                Bullet bullet = ObjectManager.Instance.AddObject<Bullet>();
                bullet.State = ObjectState.Moving;
                bullet.Dir = player.Dir;
                bullet.CellPos = player.CellPos;

                bullet.Owner = player;
                bullet.Room = this;
                bullet.SkillData = skillData;
                EnterRoom(bullet);
            }
            break;
        }

        player.State = ObjectState.Skill;

        // 스킬 사용 패킷 방에 있는 유저들에게 전송
        var skillPacket = new S2C_Skill();
        skillPacket.ObjectId = player.ObjectId;
        skillPacket.SkillInfo = skillInfo;
        BroadcastMessage(skillPacket);
    }

    public Player? FindPlayer(int playerId)
    {
        _players.TryGetValue(playerId, out Player? player);
        return player;
    }

    public Player? FindPlayer(Func<GameObject, bool> condition)
    {
        foreach (var player in _players.Values)
        {
            if (condition.Invoke(player))
                return player;
        }

        return null;
    }

    public void Clear()
    {
        _players.Clear();
        _monsters.Clear();
        _projectiles.Clear();
    }

    public void BroadcastMessage(IMessage message, int excludeId = -1)
    {
        foreach (var player in _players.Values)
        {
            if (player.ObjectId == excludeId)
                continue;

            player.Session.Send(message);
        }
    }

    private void GenerateMonsterAI(int monsterCount)
    {
        // TODO 몬스터 임시 세팅 수정
        for (int i = 0; i < monsterCount; ++i)
        {
            Monster monster = ObjectManager.Instance.AddObject<Monster>();
            monster.State = ObjectState.Idle;
            monster.Dir = MoveDir.Down;
            monster.CellPos = new Vector2Int(0, 0);
            monster.Room = this;

            DataManager.Stats.TryGetValue(1, out StatInfo? statData);
            if (statData != null)
                monster.StatInfo.MergeFrom(statData);

            EnterRoom(monster);
        }
    }
}
