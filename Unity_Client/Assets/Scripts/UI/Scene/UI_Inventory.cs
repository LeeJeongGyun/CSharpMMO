using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Inventory : UI_Base
{
    public List<UI_Inventory_Item> Items { get; } = new List<UI_Inventory_Item>();

    public override void Init()
    {
        Items.Clear();

        GameObject itemGrid = transform.Find("ItemGrid").gameObject;
        foreach (Transform child in itemGrid.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < 12; ++i)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Inventory_Item", itemGrid.transform);
            UI_Inventory_Item invenItem = go.GetOrAddComponent<UI_Inventory_Item>();
            invenItem.transform.Find("Equip").gameObject.SetActive(false);
            Items.Add(invenItem);
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (Items.Count == 0)
            return;

        List<Item> itemList = Managers.Inven.Items.Values.ToList();
        itemList.Sort((lhs, rhs) => lhs.Slot - rhs.Slot);

        foreach (var item in itemList)
        {
            if (item.Slot < 0 || item.Slot >= 12)
                continue;

            Items[item.Slot].SetItem(item);
        }
    }
}
