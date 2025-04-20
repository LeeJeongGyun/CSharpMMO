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
    public Dictionary<int, Data.Stat> Stats { get; private set; } = new Dictionary<int, Data.Stat>();
    public Dictionary<int, Data.Skill> Skills { get; private set; } = new Dictionary<int, Data.Skill>();

    public void Init()
    {
        Stats = LoadJson<Data.StatData, int, Data.Stat>("StatData").MakeDict();
        Skills = LoadJson<Data.SkillData, int, Data.Skill>("SkillData").MakeDict();
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
        // JsonUnity Enum 스트링 파싱 불가
        // Newtonsoft로 변경
        //return JsonUtility.FromJson<Loader>(textAsset.text);
    }
}
