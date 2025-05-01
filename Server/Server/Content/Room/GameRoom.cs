using Server.Content.Object;

namespace Server.Content;

using System.Numerics;
using System.Threading;
using Google.Protobuf;
using Protocol;
using Server.Content.Job;
using Server.Content.Room;
using Server.Data;
using Server.DB;

public partial class GameRoom : JobSerializer
{
    public const int visionCells = 5;
    private Dictionary<int, Player> _players = new Dictionary<int, Player>();
    private Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
    private Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
    private Map _map;

    public GameRoom(int zoneCells)
    {
        // TODO 추후 맵이 여러개 된다면 이쪽 수정 필요.
        // 지금은 맵 데이터 1개 밖에 없음으로 하드코딩
        _map = new Map();
        _map.LoadMap(1);

        // Zone 생성
        int zoneSizeX = (_map.SizeX + zoneCells - 1) / zoneCells;
        int zoneSizeY = (_map.SizeY + zoneCells - 1) / zoneCells;
        ZoneCells = zoneCells;
        Zones = new Zone[zoneSizeY, zoneSizeX];

        for (int y = 0; y < zoneSizeY; ++y)
        {
            for (int x = 0; x < zoneSizeX; ++x)
            {
                Zones[y, x] = new Zone() { IndexY = y, IndexX = x };
            }
        }

        // Temp
        GenerateMonsterAI(10);
    }

    public Zone[,] Zones { get; private set; }
    public int ZoneCells { get; private set; }
    public int Id { get; init; }

    public Map Map => _map;

    public Zone? GetZone(Vector2Int cellPos)
    {
        int zoneX = (cellPos.x - Map._minX) / ZoneCells;
        int zoneY = (Map._maxY - 1 - cellPos.y) / ZoneCells;

        if (zoneX < 0 || zoneX >= Zones.GetLength(1))
            return null;

        if (zoneY < 0 || zoneY >= Zones.GetLength(0))
            return null;

        return Zones[zoneY, zoneX];
    }

    // 주기적으로 호출 필요
    public void Update() => Flush();

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

            // 내 스텟 최신화
            player.RefreshAdditionalStat();

            // Map에 배치 진행
            _map.InitObjectPosition(gameObject);

            // Player Zone에 배치
            GetZone(player.CellPos)!.Players.Add(player);

            // 1. 입장 패킷 송신
            var enterRoomPacket = new S2C_EnterRoom();
            enterRoomPacket.ObjectInfo = gameObject.Info;
            gameObject.Session.Send(enterRoomPacket);

            // Zone에 있는 Objct 정보 player에게 송신
            List<Zone> zones = GetAdjacentZone(player.CellPos);

            S2C_Spawn spawnPacket = new S2C_Spawn();
            foreach (Player p in zones.SelectMany(zone => zone.Players))
            {
                spawnPacket.ObjectInfos.Add(p.Info);
                player.Vision._prevObjects.Add(p);
            }

            foreach (Monster m in zones.SelectMany(zone => zone.Monsters))
            {
                spawnPacket.ObjectInfos.Add(m.Info);
                player.Vision._prevObjects.Add(m);
            }

            foreach (Projectile pj in zones.SelectMany(zone => zone.Projectiles))
            {
                spawnPacket.ObjectInfos.Add(pj.Info);
                player.Vision._prevObjects.Add(pj);
            }

            if (spawnPacket.ObjectInfos.Count > 0)
                player.Session.Send(spawnPacket);

            player.Vision.Update();
        }
        else if (gameObject.ObjectType == ObjectType.Monster)
        {
            Monster monster = gameObject as Monster;
            monster!.Room = this;

            _monsters.Add(objectId, monster);
            _map.InitObjectPosition(gameObject);

            // Monster Zone에 배치
            Zone myZone = GetZone(monster.CellPos);
            myZone.Monsters.Add(monster);

            // 몬스터 AI 등록
            monster.Update();
        }
        else if (gameObject.ObjectType == ObjectType.Projectile)
        {
            Projectile projectile = gameObject as Projectile;
            projectile!.Room = this;

            _projectiles.Add(objectId, gameObject as Projectile);

            // Projectile Zone에 배치
            Zone myZone = GetZone(projectile.CellPos);
            myZone.Projectiles.Add(projectile);

            // Projectile AI 등록
            projectile.Update();
        }

        // 상대방에게 내 정보 송신
        {
            var spawnPacket = new S2C_Spawn();
            spawnPacket.ObjectInfos.Add(gameObject.Info);
            if (gameObject.ObjectType == ObjectType.Player)
                BroadcastMessage(gameObject.CellPos, spawnPacket, excludeId: objectId);
            else
                BroadcastMessage(gameObject.CellPos, spawnPacket);
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

        Vector2Int? curCellPos = null;
        if (type == ObjectType.Player)
        {
            Player? player = null;
            _players.TryGetValue(objectId, out player);

            if (player == null)
                return;

            // Broadcasting을 위한 현재 cellPos 설정
            curCellPos = player.CellPos;

            // 1. 나에게 퇴장 정보 송신
            var leavePacket = new S2C_LeaveRoom();
            player.Session.Send(leavePacket);

            // Player 관련 정보 제거
            player.OnLeaveRoom(); // DB 정보 갱신
            player.Room = null;
            _players.Remove(objectId);

            // Zone에서 제거
            Zone myZone = GetZone(player.CellPos);
            myZone.Players.Remove(player);

            // 맵에서 제거
            _map.RemoveObject(player);
        }
        else if (type == ObjectType.Monster)
        {
            Monster? monster = null;
            _monsters.TryGetValue(objectId, out monster);
            if (monster == null)
                return;

            // Broadcasting을 위한 현재 cellPos 설정
            curCellPos = monster.CellPos;

            monster.Room = null;
            _monsters.Remove(objectId);

            // Zone에서 제거
            Zone myZone = GetZone(monster.CellPos);
            myZone.Monsters.Remove(monster);

            // 맵에서 제거
            _map.RemoveObject(monster);
        }
        else if (type == ObjectType.Projectile)
        {
            if (_projectiles.TryGetValue(objectId, out Projectile? projectile))
            {
                // Broadcasting을 위한 현재 cellPos 설정
                curCellPos = projectile.CellPos;

                _projectiles.Remove(objectId);

                // Zone에서 제거
                Zone myZone = GetZone(projectile.CellPos);
                myZone.Projectiles.Remove(projectile);
            }
        }

        // 2. 상대방에게 내 퇴장 정보 전달
        var despawnPacket = new S2C_Despawn();
        despawnPacket.ObjectIds.Add(objectId);
        BroadcastMessage(curCellPos!.Value, despawnPacket);
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

    public void BroadcastMessage(Vector2Int cellPos, IMessage message, int excludeId = -1)
    {
        List<Zone> adjacentZones = GetAdjacentZone(cellPos);
        foreach (Player player in adjacentZones.SelectMany(zone => zone.Players))
        {
            if (player.ObjectId == excludeId)
                continue;

            int dx = player.CellPos.x - cellPos.x;
            int dy = player.CellPos.y - cellPos.y;

            // 나의 시야각 처리
            if (Math.Abs(dx) > GameRoom.visionCells)
                continue;

            if (Math.Abs(dy) > GameRoom.visionCells)
                continue;

            player.Session.Send(message);
        }
    }

    public List<Zone> GetAdjacentZone(Vector2Int cellPos, int cellRange = GameRoom.visionCells)
    {
        // 중복 제거
        HashSet<Zone> adjacentZone = new HashSet<Zone>();
        int[] delta = { -cellRange, +cellRange };
        foreach (int dy in delta)
        {
            foreach (int dx in delta)
            {
                int y = cellPos.y + dy;
                int x = cellPos.x + dx;

                Zone? zone = GetZone(new Vector2Int(x, y));
                if (zone == null)
                    continue;

                adjacentZone.Add(zone);
            }
        }

        return adjacentZone.ToList();
    }

    private void GenerateMonsterAI(int monsterCount)
    {
        for (int i = 0; i < monsterCount; ++i)
        {
            Monster monster = ObjectManager.Instance.AddObject<Monster>();
            monster.Init(1);
            monster.Room = this;
            EnterRoom(monster);
        }
    }
}
