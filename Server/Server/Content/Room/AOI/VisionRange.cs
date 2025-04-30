namespace Server.Content.Room;

using Protocol;
using Server.Content.Job;
using Server.Content.Object;

// Area Of Interest (관심 영역(시야각) 처리)
/// <summary>
/// 캐릭터가 죽었을 경우 재 할당하도록 설계
/// 기존에 _prevObject에 할당되어있는 객체들 때문에 죽고 다시 살아날 때
/// 주위에 있는 객체들 안보이는 문제 발생
/// </summary>
public class VisionRange
{
    private IJob _lastJob;

    // TODO
    // 이게 켜져 있는 애들만 확인하도록 최적화 Player에 있어야되나?
    // 필요한가??
    public int DirtyFlag { get; set; }

    public Player Owner { get; set; }
    public HashSet<GameObject> _prevObjects { get; private set; } = new HashSet<GameObject>();
    public HashSet<GameObject> _curObjects { get; private set; }

    public HashSet<GameObject> GatherObjects()
    {
        if (Owner == null || Owner.Room == null)
            return null;

        HashSet<GameObject> objects = new HashSet<GameObject>();
        Vector2Int cellPos = Owner.CellPos;

        List<Zone> zones = Owner.Room.GetAdjacentZone(cellPos);
        foreach (Player player in zones.SelectMany(zone => zone.Players))
        {
            int dx = player.CellPos.x - cellPos.x;
            int dy = player.CellPos.y - cellPos.y;

            // 나의 시야각 처리
            if (Math.Abs(dx) > GameRoom.visionCells)
                continue;

            if (Math.Abs(dy) > GameRoom.visionCells)
                continue;

            objects.Add(player);
        }

        foreach (Monster monster in zones.SelectMany(zone => zone.Monsters))
        {
            int dx = monster.CellPos.x - cellPos.x;
            int dy = monster.CellPos.y - cellPos.y;

            // 나의 시야각 처리
            if (Math.Abs(dx) > GameRoom.visionCells)
                continue;

            if (Math.Abs(dy) > GameRoom.visionCells)
                continue;

            objects.Add(monster);
        }

        foreach (Projectile projectile in zones.SelectMany(zone => zone.Projectiles))
        {
            int dx = projectile.CellPos.x - cellPos.x;
            int dy = projectile.CellPos.y - cellPos.y;

            // 나의 시야각 처리
            if (Math.Abs(dx) > GameRoom.visionCells)
                continue;

            if (Math.Abs(dy) > GameRoom.visionCells)
                continue;

            objects.Add(projectile);
        }

        return objects;
    }

    public void Update()
    {
        if (Owner == null || Owner.Room == null)
            return;

        // 현재 GmaeObjects 갱신
        _curObjects = GatherObjects();

        // 새로 생긴 애들
        List<GameObject> addedList = _curObjects.Except(_prevObjects).ToList();
        if (addedList.Count() > 0)
        {
            S2C_Spawn spawnPacket = new S2C_Spawn();
            foreach (GameObject gameObject in addedList)
            {
                ObjectInfo objInfo = new ObjectInfo();
                objInfo.MergeFrom(gameObject.Info);
                spawnPacket.ObjectInfos.Add(objInfo);
            }

            Owner.Session.Send(spawnPacket);
        }

        // 삭제된 애들
        List<GameObject> removedList = _prevObjects.Except(_curObjects).ToList();
        if (removedList.Count() > 0)
        {
            S2C_Despawn despawnPacket = new S2C_Despawn();
            foreach (GameObject gameObject in removedList)
                despawnPacket.ObjectIds.Add(gameObject.ObjectId);

            Owner.Session.Send(despawnPacket);
        }

        // 이전 GmaeObjects 갱신
        _prevObjects = _curObjects;

        // 캐릭터가 죽었을 경우 재 생성
        _lastJob = Owner.Room.PushAfter(Update, 500);
    }

    public void CancelJob()
    {
        if (_lastJob != null)
        {
            _lastJob.Cancel = true;
            _lastJob = null;

            _prevObjects.Clear();
            _curObjects.Clear();
        }
    }
}
