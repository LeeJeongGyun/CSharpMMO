using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Item : UI_Base
{
    // Editor에서 연결
    [SerializeField]
    private Image _icon;

    public override void Init()
    {
    }

    public void SetItem(int templateId, int itemCount)
    {
        Managers.Data.Items.TryGetValue(templateId, out var itemData);
        if (itemData == null)
            return;

        _icon.sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);
    }
}
