namespace Server.Content;

/// <summary>
/// Player에 포함되고, GameRoom에서 접근해서 사용할 것이기 때문에 lock 필요 없다.
/// </summary>
public class Inventory
{
    private Dictionary<int, Item> _items = new Dictionary<int, Item>();

    // 아이템 저장 패킷이 DB Thread에 의해 처리되기 전에 요청될 경우 중복 방지
    private HashSet<int> _reserveSlot = new HashSet<int>();

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

    public int? GetEmptySlot()
    {
        for (int slot = 0; slot < 20; ++slot)
        {
            Item? item = _items.Values.Where(item => item.Slot == slot).FirstOrDefault();
            if (item == null && _reserveSlot.TryGetValue(slot, out _) == false)
            {
                _reserveSlot.Add(slot);
                return slot;
            }
        }

        return null;
    }

    public void RemoveReserveSlot(int slot) => _reserveSlot.Remove(slot);
}
