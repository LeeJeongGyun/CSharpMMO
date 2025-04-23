namespace Server.Content;

/// <summary>
/// Player에 포함되고, GameRoom에서 접근해서 사용할 것이기 때문에 lock 필요 없다.
/// </summary>
public class Inventory
{
    private Dictionary<int, Item> _items = new Dictionary<int, Item>();

    public void Add(Item item)
    {
        _items.Add(item.ItemDbId, item);
    }

    public Item? GetItem(int itemDbId)
    {
        _items.TryGetValue(itemDbId, out Item? item);
        return item;
    }

    public Item? FindItem(Func<Item, bool> condition)
    {
        foreach (var item in _items.Values)
        {
            if (condition.Invoke(item))
                return item;
        }

        return null;
    }
}
