namespace Server.Content.Room;

using Server.Content.Object;

public class Zone
{
    public HashSet<Player> Players { get; } = new HashSet<Player>();
    public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
    public HashSet<Projectile> Projectiles { get; } = new HashSet<Projectile>();

    public int IndexY { get; init; }
    public int IndexX { get; init; }

    public Player? FindPlayer(Func<Player, bool> condition)
    {
        foreach (Player player in Players)
        {
            if (condition.Invoke(player))
                return player;
        }

        return null;
    }

    public List<Player> FindPlayerList(Func<Player, bool> condition)
    {
        List<Player> retList = new List<Player>();
        foreach (Player player in Players)
        {
            if (condition.Invoke(player))
                retList.Add(player);
        }

        return retList;
    }
}
