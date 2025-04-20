using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapManager
{
    private GameObject _map;

    private bool[,] _collision;

    private int _minX, _maxX;

    private int _minY, _maxY;

    public int SizeX { get; private set; }

    public int SizeY { get; private set; }

    public Grid CurrentGrid { get; set; }

    public void LoadMap(int mapId)
    {
        string mapName = $"Map_{mapId.ToString("000")}";
        GameObject map = Managers.Resource.Instantiate($"Map/{mapName}");
        map.name = "Map";
        _map = map;

        GameObject collision = Util.FindChild(map, "Tilemap_Collision");
        if (collision != null)
            collision.SetActive(false);

        Managers.Map.CurrentGrid = map.GetComponent<Grid>();

        TextAsset mapText = Managers.Resource.Load<TextAsset>($"Map/{mapName}");
        StringReader sr = new StringReader(mapText.text);

        _maxX = int.Parse(sr.ReadLine());
        _minX = int.Parse(sr.ReadLine());
        _maxY = int.Parse(sr.ReadLine());
        _minY = int.Parse(sr.ReadLine());
        SizeX = _maxX - _minX;
        SizeY = _maxY - _minY;

        _collision = new bool[SizeY, SizeX];
        for (int y = 0; y < SizeY; ++y)
        {
            string line = sr.ReadLine();
            for (int x = 0; x < line.Length; ++x)
                _collision[y, x] = line[x] == '1' ? true : false;
        }
    }

    public void DestroyMap()
    {
        if (_map != null)
            Managers.Resource.Destroy(_map);

        CurrentGrid = null;
    }

    public bool FindCollision(Vector3Int cellPos)
    {
        if (cellPos.x < _minX || cellPos.x >= _maxX || cellPos.y < _minY || cellPos.y >= _maxY)
            return true;

        int convertedX = cellPos.x - _minX;
        int convertedY = (_maxY - 1) - cellPos.y;
        return _collision[convertedY, convertedX];
    }

    #region AStar 길찾기

    private int[] _deltaY = new int[] { 1, -1, 0, 0 };

    private int[] _deltaX = new int[] { 0, 0, -1, 1 };
    private int[] _cost = new int[] { 10, 10, 10, 10 };

    public List<Vector3Int> FindPath(Vector3Int startCellPos, Vector3Int destCellPos, bool ignoreDestCollision = false)
    {
        // 점수 매기기
        // F = G + H
        // F = 최종 점수 (작을 수록 좋음, 경로에 따라 달라짐)
        // G = 시작점에서 해당 좌표까지 이동하는데 드는 비용 (작을 수록 좋음, 경로에 따라 달라짐)
        // H = 목적지에서 얼마나 가까운지 (작을 수록 좋음, 고정)

        // (y, x) 이미 방문했는지 여부 (방문 = closed 상태)
        bool[,] closed = new bool[SizeY, SizeX]; // CloseList

        // (y, x) 가는 길을 한 번이라도 발견했는지
        // 발견X => MaxValue
        // 발견O => F = G + H
        int[,] open = new int[SizeY, SizeX]; // OpenList
        for (int y = 0; y < SizeY; y++)
            for (int x = 0; x < SizeX; x++)
                open[y, x] = Int32.MaxValue;

        Pos[,] parent = new Pos[SizeY, SizeX];

        // 오픈리스트에 있는 정보들 중에서, 가장 좋은 후보를 빠르게 뽑아오기 위한 도구
        PriorityQ<PQNode> pq = new PriorityQ<PQNode>();

        // CellPos -> ArrayPos
        Pos pos = Cell2Pos(startCellPos);
        Pos dest = Cell2Pos(destCellPos);

        // 시작점 발견 (예약 진행)
        open[pos.Y, pos.X] = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X));
        pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
        parent[pos.Y, pos.X] = new Pos(pos.Y, pos.X);

        while (pq.Count > 0)
        {
            // 제일 좋은 후보를 찾는다
            PQNode node = pq.Pop();
            // 동일한 좌표를 여러 경로로 찾아서, 더 빠른 경로로 인해서 이미 방문(closed)된 경우 스킵
            if (closed[node.Y, node.X])
                continue;

            // 방문한다
            closed[node.Y, node.X] = true;
            // 목적지 도착했으면 바로 종료
            if (node.Y == dest.Y && node.X == dest.X)
                break;

            // 상하좌우 등 이동할 수 있는 좌표인지 확인해서 예약(open)한다
            for (int i = 0; i < _deltaY.Length; i++)
            {
                Pos next = new Pos(node.Y + _deltaY[i], node.X + _deltaX[i]);

                // 유효 범위를 벗어났으면 스킵
                // 벽으로 막혀서 갈 수 없으면 스킵
                if (!ignoreDestCollision || next.Y != dest.Y || next.X != dest.X)
                {
                    if (FindCollision(Pos2Cell(next))) // CellPos
                        continue;
                }

                // 이미 방문한 곳이면 스킵
                if (closed[next.Y, next.X])
                    continue;

                // 비용 계산
                int g = 0;// node.G + _cost[i];
                int h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));
                // 다른 경로에서 더 빠른 길 이미 찾았으면 스킵
                if (open[next.Y, next.X] < g + h)
                    continue;

                // 예약 진행
                open[dest.Y, dest.X] = g + h;
                pq.Push(new PQNode() { F = g + h, G = g, Y = next.Y, X = next.X });
                parent[next.Y, next.X] = new Pos(node.Y, node.X);
            }
        }

        return CalcCellPathFromParent(parent, dest);
    }

    private List<Vector3Int> CalcCellPathFromParent(Pos[,] parent, Pos dest)
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        int y = dest.Y;
        int x = dest.X;
        while (parent[y, x].Y != y || parent[y, x].X != x)
        {
            cells.Add(Pos2Cell(new Pos(y, x)));
            Pos pos = parent[y, x];
            y = pos.Y;
            x = pos.X;
        }
        cells.Add(Pos2Cell(new Pos(y, x)));
        cells.Reverse();

        return cells;
    }

    private Pos Cell2Pos(Vector3Int cell)
    {
        // CellPos -> ArrayPos
        return new Pos((_maxY - 1) - cell.y, cell.x - _minX);
    }

    private Vector3Int Pos2Cell(Pos pos)
    {
        // ArrayPos -> CellPos
        return new Vector3Int(pos.X + _minX, (_maxY - 1) - pos.Y, 0);
    }

    public struct Pos
    {
        public int Y;

        public int X;

        public Pos(int y, int x)
        { Y = y; X = x; }
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
