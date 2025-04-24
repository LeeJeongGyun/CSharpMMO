using System;
using System.Collections;
using System.Collections.Generic;
using Protocol;
using UnityEngine;

namespace Data
{
    #region Skill

    [Serializable]
    public class Skill
    {
        public int id;
        public string name;
        public float cooldown;
        public int damage;
        public SkillType skillType;
        public ProjectileInfo projectile;
    }

    [Serializable]
    public class ProjectileInfo
    {
        public string name;
        public float speed;
        public int range;
    }

    [Serializable]
    public class SkillData : ILoader<int, Skill>
    {
        public List<Skill> skills = new List<Skill>();

        public Dictionary<int, Skill> MakeDict()
        {
            Dictionary<int, Skill> ret = new Dictionary<int, Skill>();
            foreach (Skill skill in skills)
                ret.Add(skill.id, skill);

            return ret;
        }
    }

    #endregion Skill

    #region Item

    [Serializable]
    public class ItemData
    {
        public int id;
        public string name;
        public ItemType itemType;
        public string iconPath;
    }

    [Serializable]
    public class WeaponData : ItemData
    {
        public WeaponType weaponType;
        public int damage;
    }

    [Serializable]
    public class ArmorData : ItemData
    {
        public ArmorType armorType;
        public int defence;
    }

    [Serializable]
    public class ConsumableData : ItemData
    {
        public ConsumableType consumableType;
        public int maxCount;
    }

    [Serializable]
    public class ItemLoader : ILoader<int, ItemData>
    {
        public List<WeaponData> weapons = new List<WeaponData>();
        public List<ArmorData> armors = new List<ArmorData>();
        public List<ConsumableData> consumables = new List<ConsumableData>();

        public Dictionary<int, ItemData> MakeDict()
        {
            Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
            foreach (ItemData item in weapons)
            {
                item.itemType = ItemType.Weapon;
                dict.Add(item.id, item);
            }

            foreach (ItemData item in armors)
            {
                item.itemType = ItemType.Armor;
                dict.Add(item.id, item);
            }

            foreach (ItemData item in consumables)
            {
                item.itemType = ItemType.Consumable;
                dict.Add(item.id, item);
            };

            return dict;
        }
    }

    #endregion Item
}
