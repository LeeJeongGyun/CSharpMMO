using System.Collections;
using System.Collections.Generic;
using Protocol;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Item : UI_Base
{
    // Editor에서 연결
    [SerializeField]
    private Image _icon;

    [SerializeField]
    private GameObject _equiped;

    private Item _item;

    public override void Init()
    {
        if (_icon != null)
        {
            _icon.gameObject.BindEvent(_ =>
            {
                if (_item == null)
                    return;

                if (_item.ItemType == ItemType.Consumable)
                {
                    Debug.Log($"{name}은 소비 아이템 입니다.");
                    return;
                }

                C2S_EquipItem equipItemPacket = new C2S_EquipItem();
                equipItemPacket.ItemDbId = _item.ItemDbId;
                equipItemPacket.Equiped = !_item.Equiped;
                Managers.Network.Send(equipItemPacket);
            });
        }
    }

    public void SetItem(Item item)
    {
        Managers.Data.Items.TryGetValue(item.TemplateId, out var itemData);
        if (itemData == null)
            return;

        _icon.sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _item = item;
        _equiped.SetActive(item.Equiped);
    }
}
