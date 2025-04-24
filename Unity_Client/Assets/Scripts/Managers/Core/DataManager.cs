using System;
using System.Collections;
using System.Collections.Generic;
using Protocol;
using UnityEngine;
using Newtonsoft.Json;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Data.Skill> Skills { get; private set; } = new Dictionary<int, Data.Skill>();

    public Dictionary<int, Data.ItemData> Items { get; private set; } = new Dictionary<int, Data.ItemData>();

    public void Init()
    {
        Skills = LoadJson<Data.SkillData, int, Data.Skill>("SkillData").MakeDict();
        Items = LoadJson<Data.ItemLoader, int, Data.ItemData>("ItemData").MakeDict();
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
