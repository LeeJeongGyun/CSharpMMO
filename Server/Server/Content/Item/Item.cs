namespace Server.Content;

using Protocol;
using Server.Data;
using Server.DB;
using static System.Net.Mime.MediaTypeNames;

public class Item
{
    public Item(ItemType type) => ItemType = type;

    public ItemInfo Info { get; } = new ItemInfo();

    public int ItemDbId
    {
        get => Info.ItemDbId;
        set => Info.ItemDbId = value;
    }

    public int TemplateId
    {
        get => Info.TemplateId;
        set => Info.TemplateId = value;
    }

    public int Count
    {
        get => Info.Count;
        set => Info.Count = value;
    }

    public string Name { get; protected set; }

    public ItemType ItemType { get; private set; }

    // 겹쳐지냐..
    public bool Stackable { get; protected set; }

    public static Item? MakeItem(ItemDb itemDb)
    {
        DataManager.Items.TryGetValue(itemDb.TemplateId, out var item);
        if (item == null)
            return null;

        Item? newItem = null;
        switch (item.itemType)
        {
        case ItemType.Weapon:
            newItem = new Weapon(itemDb.TemplateId);
            break;

        case ItemType.Armor:
            newItem = new Armor(itemDb.TemplateId);
            break;

        case ItemType.Consumable:
            newItem = new Consumable(itemDb.TemplateId);
            break;

        default:
            return null;
        }

        newItem.ItemDbId = itemDb.ItemDbId;
        return newItem;
    }
}

public class Weapon : Item
{
    public Weapon(int templateId) : base(ItemType.Weapon)
    {
        Init(templateId);
    }

    public WeaponType WeaponType { get; private set; }

    public int Damage { get; private set; }

    private void Init(int templateId)
    {
        DataManager.Items.TryGetValue(templateId, out var itemData);
        if (itemData != null)
        {
            WeaponData weapon = itemData as WeaponData;
            if (weapon != null)
            {
                TemplateId = weapon.id;
                Count = 1;
                Name = weapon.name;
                WeaponType = weapon.weaponType;
                Damage = weapon.damage;
                Stackable = false;
            }
        }
    }
}

public class Armor : Item
{
    public Armor(int templateId) : base(ItemType.Armor)
    {
        Init(templateId);
    }

    public ArmorType ArmorType { get; private set; }
    public int Defence { get; private set; }

    private void Init(int templateId)
    {
        DataManager.Items.TryGetValue(templateId, out var itemData);
        if (itemData != null)
        {
            ArmorData armor = itemData as ArmorData;
            if (armor != null)
            {
                TemplateId = armor.id;
                Count = 1;
                Name = armor.name;
                ArmorType = armor.armorType;
                Defence = armor.defence;
                Stackable = false;
            }
        }
    }
}

public class Consumable : Item
{
    public Consumable(int templateId) : base(ItemType.Consumable)
    {
        Init(templateId);
    }

    public ConsumableType ConsumableType { get; private set; }
    public int MaxCount { get; private set; }

    private void Init(int templateId)
    {
        DataManager.Items.TryGetValue(templateId, out var itemData);
        if (itemData != null)
        {
            ConsumableData consumable = itemData as ConsumableData;
            if (consumable != null)
            {
                TemplateId = consumable.id;
                Count = 1;
                MaxCount = consumable.maxCount;
                Name = consumable.name;
                ConsumableType = consumable.consumableType;
                Stackable = (MaxCount > 1) ? true : false;
            }
        }
    }
}
