using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager
{
    public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();

    public void Add(Item item)
    {
        Items.Add(item.ItemDbId, item);
    }

    public Item GetItem(int itemDbId)
    {
        Items.TryGetValue(itemDbId, out Item item);
        return item;
    }

    public Item FindItem(Func<Item, bool> condition)
    {
        foreach (var item in Items.Values)
        {
            if (condition.Invoke(item))
                return item;
        }

        return null;
    }

    public IReadOnlyList<Item> GetEquipedItemList() => Items.Values.Where(item => item.Equiped).ToList();

    public void Clear()
    {
        Items.Clear();
    }
}
