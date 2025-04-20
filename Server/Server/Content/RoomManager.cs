namespace Server.Content;

internal class RoomManager
{
    #region 싱글톤

    public static RoomManager Instance { get; } = new RoomManager();

    #endregion 싱글톤

    private Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
    private object _lock = new object();
    private int _roomId = 1;

    public GameRoom AddRoom()
    {
        lock (_lock)
        {
            GameRoom room = new GameRoom() { Id = _roomId++ };
            _rooms.Add(room.Id, room);
            return room;
        }
    }

    public void RemoveRoom(int roomId)
    {
        lock (_lock)
        {
            _rooms.Remove(roomId);
        }
    }

    public GameRoom? FindRoom(int roomId)
    {
        GameRoom? room = null;
        lock (_lock)
        {
            _rooms.TryGetValue(roomId, out room);
        }

        return room;
    }
}
