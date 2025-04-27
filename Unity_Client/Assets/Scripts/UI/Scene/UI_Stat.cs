using System.Collections;
using System.Collections.Generic;
using Protocol;
using UnityEngine;
using UnityEngine.UI;

public class UI_Stat : UI_Base
{
    private bool _init = false;

    public enum Images
    {
        Helmet_Icon,
        Armor_Icon,
        Weapon_Icon,
    }

    public enum Texts
    {
        NameText,
        AttackValueText,
        DefenceValueText,
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));

        _init = true;
    }

    public void RefreshUI()
    {
        if (_init == false)
            return;

        MyPlayerController mpc = Managers.Object.MyPlayerController;
        GetText((int)Texts.NameText).text = mpc.name;

        // Stat 최신화
        GetText((int)Texts.AttackValueText).text =
            $"{mpc.StatInfo.Attack + mpc.AdditionalWeaponDamage} ({mpc.StatInfo.Attack} + {mpc.AdditionalWeaponDamage})";

        GetText((int)Texts.DefenceValueText).text =
            $"{mpc.AdditionalArmorDefence} (0 + {mpc.AdditionalArmorDefence})";

        // 장비 없다면 노출안되도록 active false 처리
        for (int idx = 0; idx <= (int)Images.Weapon_Icon; ++idx)
            GetImage(idx).enabled = false;

        // 장비 최신화
        foreach (var item in Managers.Inven.GetEquipedItemList())
        {
            Managers.Data.Items.TryGetValue(item.TemplateId, out var itemData);
            if (itemData == null)
                return;

            switch (item.ItemType)
            {
            case ItemType.Weapon:
                GetImage((int)Images.Weapon_Icon).enabled = true;
                GetImage((int)Images.Weapon_Icon).sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);
                break;

            case ItemType.Armor:
                ArmorType armorType = ((Armor)item).ArmorType;
                if (armorType == ArmorType.Helmet)
                {
                    GetImage((int)Images.Helmet_Icon).enabled = true;
                    GetImage((int)Images.Helmet_Icon).sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);
                }
                else if (armorType == ArmorType.Armor)
                {
                    GetImage((int)Images.Armor_Icon).enabled = true;
                    GetImage((int)Images.Armor_Icon).sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);
                }

                break;
            }
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }
}
