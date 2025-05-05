namespace Server.Content;

using Server.Content.Job;

/// <summary>
/// 해당 클래스의 함수들은 전부 JobQueue를 통해 예약되는 방식으로 동작
/// JobQueue를 통해 작업이 예약되기 때문에 lock 존재하지 않음.
/// GameLogicThread 스레드에 의해 실행
/// </summary>
internal class GameLogic : JobSerializer
{
    #region 싱글톤

    public static GameLogic Instance { get; } = new GameLogic();

    #endregion 싱글톤

    private Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
    private int _roomId = 1;

    public int JobCount => GetJobCount();

    public GameRoom AddRoom()
    {
        GameRoom room = new GameRoom(10) { Id = _roomId++ };
        _rooms.Add(room.Id, room);
        return room;
    }

    public void RemoveRoom(int roomId) => _rooms.Remove(roomId);

    public GameRoom? FindRoom(int roomId)
    {
        GameRoom? room = null;
        _rooms.TryGetValue(roomId, out room);

        return room;
    }

    public new void Flush()
    {
        base.Flush();

        foreach (GameRoom room in _rooms.Values)
        {
            room.Flush();
        }
    }
}
