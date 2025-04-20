namespace Server.Content;

using Protocol;
using Server.Content.Object;

internal class ObjectManager
{
    #region 싱글톤

    public static ObjectManager Instance { get; } = new ObjectManager();

    #endregion 싱글톤

    private Dictionary<int, Player> _players = new Dictionary<int, Player>();
    private object _lock = new object();
    private int _objectId = 1;

    public T AddObject<T>() where T : GameObject, new()
    {
        lock (_lock)
        {
            T obj = new T();
            obj.Info.ObjectId = _objectId++;

            if (obj.ObjectType == ObjectType.Player)
                _players.Add(obj.ObjectId, obj as Player);

            return obj;
        }
    }

    public void RemovePlayer(int playerId)
    {
        lock (_lock)
        {
            _players.TryGetValue(playerId, out _);
        }
    }

    public Player? FindPlayer(int playerId)
    {
        lock (_lock)
        {
            Player? player = null;
            _players.TryGetValue(playerId, out player);
            return player;
        }
    }
}
