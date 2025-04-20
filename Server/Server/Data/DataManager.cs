namespace Server.Data;

using Newtonsoft.Json;
using Protocol;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDictionary();
}

public static class DataManager
{
    public static Dictionary<int, StatInfo> Stats { get; private set; }
    public static Dictionary<int, Skill> Skills { get; private set; }

    public static void LoadData()
    {
        Stats = LoadJson<StatData, int, StatInfo>("StatData").MakeDictionary();
        Skills = LoadJson<SkillData, int, Skill>("SkillData").MakeDictionary();
    }

    public static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/{path}.json");
        return JsonConvert.DeserializeObject<Loader>(text);
    }
}
