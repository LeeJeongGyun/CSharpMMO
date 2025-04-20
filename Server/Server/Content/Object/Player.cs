namespace Server.Content.Object;

using System.Diagnostics;
using Protocol;
using Server.Content;

public class Player : GameObject
{
    public Player()
    {
        ObjectType = ObjectType.Player;
    }
}
