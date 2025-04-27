namespace Server.Content.Object;

using System.Diagnostics;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Protocol;
using Server.Content;
using Server.Data;
using Server.DB;

public class Player : GameObject
{
    public Player()
    {
        ObjectType = ObjectType.Player;
    }

    public Inventory Inven { get; private set; } = new Inventory();

    public int PlayerDbId { get; set; }

    public int AdditionalWeaponDamage { get; private set; }

    public int AdditionalArmorDefence { get; private set; }

    public override int TotalDamage => StatInfo.Attack + AdditionalWeaponDamage;

    public override int TotalDefence => AdditionalArmorDefence;

    public override void OnDamaged(GameObject attacker, int damaged)
    {
        base.OnDamaged(attacker, damaged);
    }

    public void OnLeaveRoom()
    {
        // DB 연동
        // 비동기 호출
        DBTransaction.SavePlayerStatInfo(this, Room);
    }

    public void HandleEquipItemPacket(C2S_EquipItem equipItemPacket)
    {
        Item? item = Inven.GetItem(equipItemPacket.ItemDbId);
        if (item == null)
            return;

        // 소비 아이템이면 장착 불가능
        if (item.ItemType == ItemType.Consumable)
            return;

        // 동일한 부위 착용 중이면 해제
        if (equipItemPacket.Equiped)
        {
            Item? unEquipedItem = null;
            // WeaponType 장착 중이면 장착 변경
            if (item.ItemType == ItemType.Weapon)
            {
                unEquipedItem = Inven.FindItem(invenItem =>
                                invenItem.Equiped && invenItem.ItemType == ItemType.Weapon);
            }
            else if (item.ItemType == ItemType.Armor)
            {
                ArmorType armorType = ((Armor)item).ArmorType;
                unEquipedItem = Inven.FindItem(invenItem =>
                        invenItem.Equiped && invenItem.ItemType == ItemType.Armor && ((Armor)invenItem).ArmorType == armorType);
            }

            if (unEquipedItem != null)
            {
                // 메모리 선 적용
                unEquipedItem.Equiped = false;
                DBTransaction.SaveEquipItem(this, unEquipedItem);

                S2C_EquipItem equipItemPacketRes = new S2C_EquipItem();
                equipItemPacketRes.ItemDbId = unEquipedItem.ItemDbId;
                equipItemPacketRes.Equiped = unEquipedItem.Equiped;
                Session.Send(equipItemPacketRes);
            }
        }

        {
            // 메모리 선 적용
            item.Equiped = equipItemPacket.Equiped;

            // DB 비동기 요청
            DBTransaction.SaveEquipItem(this, item);

            S2C_EquipItem equipItemPacketRes = new S2C_EquipItem();
            equipItemPacketRes.ItemDbId = item.ItemDbId;
            equipItemPacketRes.Equiped = item.Equiped;
            Session.Send(equipItemPacketRes);
        }

        RefreshAdditionalStat();
    }

    public void RefreshAdditionalStat()
    {
        int additionalWaeaponDamage = 0;
        int additionalArmorDefence = 0;
        foreach (var item in Inven.GetEquipedItemList())
        {
            switch (item.ItemType)
            {
            case ItemType.Weapon:
                additionalWaeaponDamage += ((Weapon)item).Damage;
                break;

            case ItemType.Armor:
                additionalArmorDefence += ((Armor)item).Defence;
                break;
            }
        }

        AdditionalWeaponDamage = additionalWaeaponDamage;
        AdditionalArmorDefence = additionalArmorDefence;
    }
}
