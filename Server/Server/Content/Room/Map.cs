namespace Server.Content.Room;

using System.Diagnostics;
using Protocol;
using Server.Content.Object;
using ServerCore;

public struct Vector2Int
{
    public Vector2Int(int x, int y) => (this.x, this.y) = (x, y);

    public static Vector2Int Up => new Vector2Int(0, 1);
    public static Vector2Int Down => new Vector2Int(0, -1);
    public static Vector2Int Left => new Vector2Int(-1, 0);
    public static Vector2Int Right => new Vector2Int(1, 0);
    public int x { get; set; }
    public int y { get; set; }

    public int sqrMagnitude => x * x + y * y;

    public int magnitude => (int)Math.Sqrt(sqrMagnitude);

    // 거리 비교에 드는 연산 부하 감소
    public int cellDistance => Math.Abs(x) + Math.Abs(y);

    public static Vector2Int operator +(Vector2Int lhs, Vector2Int rhs) => new Vector2Int(lhs.x + rhs.x, lhs.y + rhs.y);

    public static Vector2Int operator -(Vector2Int lhs, Vector2Int rhs) => new Vector2Int(lhs.x - rhs.x, lhs.y - rhs.y);
}

// 룸 안에서 동작하기 때문에 lock 걸고 들어와서 Lock 없음.
// 외부에서 직접 접근 시 락 필요.
public class Map
{
    public int _minX, _maxX;
    public int _minY, _maxY;
    private bool[,] _collision;
    private GameObject[,] _objects;
    public int SizeX { get; private set; }

    public int SizeY { get; private set; }

    public void LoadMap(int mapId)
    {
        string mapName = $"Map_{mapId.ToString("000")}";

        string mapText = File.ReadAllText($"../../../../../Common/MapData/{mapName}.txt");
        var sr = new StringReader(mapText);

        _maxX = int.Parse(sr.ReadLine());
        _minX = int.Parse(sr.ReadLine());
        _maxY = int.Parse(sr.ReadLine());
        _minY = int.Parse(sr.ReadLine());
        SizeX = _maxX - _minX;
        SizeY = _maxY - _minY;

        _collision = new bool[SizeY, SizeX];
        _objects = new GameObject[SizeY, SizeX];
        for (int y = 0; y < SizeY; ++y)
        {
            string line = sr.ReadLine();
            for (int x = 0; x < line.Length; ++x)
                _collision[y, x] = line[x] == '1' ? true : false;
        }
    }

    public void DestroyMap()
    {
        _collision = null;
    }

    // 제일 처음 Room에 입장했을 때 위치 좌표가 겹친다면 조정
    // 제일 처음 룸에 입장했을 때만 호출
    public bool InitObjectPosition(GameObject gameObject)
    {
        bool initSuccessFlag = true;
        if (FindObject(gameObject.CellPos) != null)
        {
            initSuccessFlag = false;
            for (int i = 0; i < 100; ++i)
            {
                int randX = Random.Shared.Next(_minX, _maxX - 1);
                int randY = Random.Shared.Next(_minY, _maxY - 1);
                var newCellPos = new Vector2Int(randX, randY);

                if (FindObject(newCellPos) == null && FindCollision(newCellPos) == false)
                {
                    gameObject.CellPos = newCellPos;
                    initSuccessFlag = true;
                    break;
                }
            }
        }

        if (initSuccessFlag == false)
        {
            // TODO FileLog
            // 끊고 재 연결 요청
            Console.WriteLine($"Init Fail!! {gameObject.ObjectId}");
            return false;
        }

        Pos pos = Cell2Pos(gameObject.CellPos);
        _objects[pos.Y, pos.X] = gameObject;
        return true;
    }

    public bool MoveObject(GameObject gameObject, Vector2Int dstCellPos)
    {
        if (FindCollision(dstCellPos))
            return false;

        if (FindObject(dstCellPos) != null)
            return false;

        if (false == RemoveObject(gameObject))
        {
            // Log
            Console.WriteLine($"PlayerId: {gameObject.ObjectId} 이동 삭제 실패");
            return false;
        }

        // Zone 정보 변경
        Zone prevZone = gameObject.Room.GetZone(gameObject.CellPos);
        Zone moveZone = gameObject.Room.GetZone(dstCellPos);

        if (prevZone != moveZone)
        {
            if (gameObject.ObjectType == ObjectType.Player)
            {
                Player player = (Player)gameObject;
                prevZone.Players.Remove(player);
                moveZone.Players.Add(player);
            }
            else if (gameObject.ObjectType == ObjectType.Monster)
            {
                Monster monster = (Monster)gameObject;
                prevZone.Monsters.Remove(monster);
                moveZone.Monsters.Add(monster);
            }
        }

        // 내 현재 좌표 변경
        gameObject.CellPos = dstCellPos;

        int convertedX = dstCellPos.x - _minX;
        int convertedY = _maxY - 1 - dstCellPos.y;
        _objects[convertedY, convertedX] = gameObject;
        return true;
    }

    public bool RemoveObject(GameObject gameObject)
    {
        GameObject? me = FindObject(gameObject.CellPos);
        if (me == null)
        {
            Console.WriteLine("내가 없음");
            return false;
        }

        if (me != gameObject)
        {
            Console.WriteLine("내가 아님");
            return false;
        }

        int convertedX = gameObject.CellPos.x - _minX;
        int convertedY = _maxY - 1 - gameObject.CellPos.y;

        _objects[convertedY, convertedX] = null;
        return true;
    }

    public bool FindCollision(Vector2Int cellPos)
    {
        if (IsInMapRange(cellPos) == false)
            return true;

        int convertedX = cellPos.x - _minX;
        int convertedY = _maxY - 1 - cellPos.y;
        return _collision[convertedY, convertedX];
    }

    public GameObject? FindObject(Vector2Int cellPos)
    {
        if (IsInMapRange(cellPos) == false)
            return null;

        int convertedX = cellPos.x - _minX;
        int convertedY = _maxY - 1 - cellPos.y;
        return _objects[convertedY, convertedX];
    }

    private bool IsInMapRange(Vector2Int cellPos)
    {
        if (cellPos.x < _minX || cellPos.x >= _maxX || cellPos.y < _minY || cellPos.y >= _maxY)
            return false;
        return true;
    }

    #region AStar 길찾기

    private int[] _deltaY = new int[] { 1, -1, 0, 0 };

    private int[] _deltaX = new int[] { 0, 0, -1, 1 };
    private int[] _cost = new int[] { 10, 10, 10, 10 };

    public List<Vector2Int> FindPath(Vector2Int startCellPos, Vector2Int destCellPos, bool checkObject = false, int maxDist = 10)
    {
        // 점수 매기기
        // F = G + H
        // F = 최종 점수 (작을 수록 좋음, 경로에 따라 달라짐)
        // G = 시작점에서 해당 좌표까지 이동하는데 드는 비용 (작을 수록 좋음, 경로에 따라 달라짐)
        // H = 목적지에서 얼마나 가까운지 (작을 수록 좋음, 고정)

        // (y, x) 이미 방문했는지 여부 (방문 = closed 상태)
        var closedList = new HashSet<Pos>(); // CloseList

        // (y, x) 가는 길을 한 번이라도 발견했는지
        // 발견X => MaxValue
        // 발견O => F = G + H
        var openList = new Dictionary<Pos, int>(); // OpenList

        var parent = new Dictionary<Pos, Pos>();

        // 오픈리스트에 있는 정보들 중에서, 가장 좋은 후보를 빠르게 뽑아오기 위한 도구
        var pq = new PriorityQ<PQNode>();

        // CellPos -> ArrayPos
        Pos pos = Cell2Pos(startCellPos);
        Pos dest = Cell2Pos(destCellPos);

        // 시작점 발견 (예약 진행)
        openList.Add(pos, 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)));
        pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
        parent.Add(pos, pos);

        while (pq.Count > 0)
        {
            // 제일 좋은 후보를 찾는다
            PQNode node = pq.Pop();
            var curPos = new Pos(node.Y, node.X);
            // 동일한 좌표를 여러 경로로 찾아서, 더 빠른 경로로 인해서 이미 방문(closed)된 경우 스킵
            if (closedList.Contains(curPos))
                continue;

            // 방문한다
            closedList.Add(curPos);

            // 목적지 도착했으면 바로 종료
            if (curPos == dest)
                break;

            // 상하좌우 등 이동할 수 있는 좌표인지 확인해서 예약(open)한다
            for (int i = 0; i < _deltaY.Length; i++)
            {
                var next = new Pos(node.Y + _deltaY[i], node.X + _deltaX[i]);

                // 너무 멀면 Skip
                if (Math.Abs(pos.Y - next.Y) + Math.Abs(pos.X - next.X) > maxDist)
                    continue;

                // 유효 범위를 벗어났으면 스킵
                // 벽으로 막혀서 갈 수 없으면 스킵
                if (next.Y != dest.Y || next.X != dest.X)
                {
                    if (FindCollision(Pos2Cell(next))) // CellPos
                        continue;

                    if (checkObject && FindObject(Pos2Cell(next)) != null)
                        continue;
                }

                // 이미 방문한 곳이면 스킵
                if (closedList.Contains(next))
                    continue;

                // 비용 계산
                int g = node.G + _cost[i];
                int h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));

                // 다른 경로에서 더 빠른 길 이미 찾았으면 스킵
                if (openList.TryGetValue(next, out int prevF) && prevF < g + h)
                    continue;

                // 예약 진행
                if (openList.TryAdd(next, g + h) == false)
                    openList[next] = g + h;

                pq.Push(new PQNode() { F = g + h, G = g, Y = next.Y, X = next.X });
                if (parent.TryAdd(next, curPos) == false)
                    parent[next] = curPos;
            }
        }

        return CalcCellPathFromParent(parent, dest);
    }

    private List<Vector2Int> CalcCellPathFromParent(Dictionary<Pos, Pos> parent, Pos dest)
    {
        var cells = new List<Vector2Int>();

        // 길을 못찾을 경우 예외 처리
        if (parent.ContainsKey(dest) == false)
        {
            Pos bestPos = new Pos();
            int bestDist = int.MaxValue;

            foreach (Pos posKey in parent.Keys)
            {
                int dist = Math.Abs(dest.Y - posKey.Y) + Math.Abs(dest.X - posKey.X);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestPos = posKey;
                }
            }

            dest = bestPos;
        }

        Pos pos = dest;
        while (parent[pos] != pos)
        {
            cells.Add(Pos2Cell(pos));
            pos = parent[pos];
        }

        cells.Add(Pos2Cell(pos));
        cells.Reverse();
        return cells;
    }

    private Pos Cell2Pos(Vector2Int cell)
    {
        // CellPos -> ArrayPos
        return new Pos(_maxY - 1 - cell.y, cell.x - _minX);
    }

    private Vector2Int Pos2Cell(Pos pos)
    {
        // ArrayPos -> CellPos
        return new Vector2Int(pos.X + _minX, _maxY - 1 - pos.Y);
    }

    public struct Pos
    {
        public int Y;

        public int X;

        public Pos(int y, int x) => (Y, X) = (y, x);

        public static bool operator ==(Pos lhs, Pos rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y;

        public static bool operator !=(Pos lhs, Pos rhs) => !(lhs == rhs);

        public override int GetHashCode()
        {
            long hashValue = Y << 32 | X;
            return hashValue.GetHashCode();
        }

        public override bool Equals(object obj) => this == (Pos)obj;
    }

    public struct PQNode : IComparable<PQNode>
    {
        public int F;
        public int G;
        public int Y;
        public int X;

        public int CompareTo(PQNode other)
        {
            if (F == other.F)
                return 0;
            return F < other.F ? 1 : -1;
        }
    }

    #endregion AStar 길찾기
}
