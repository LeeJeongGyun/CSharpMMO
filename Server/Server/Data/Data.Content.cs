namespace Server.Data;

using Protocol;

#region Stat

public class StatData : ILoader<int, StatInfo>
{
    public List<StatInfo> stats = new List<StatInfo>();

    public Dictionary<int, StatInfo> MakeDictionary()
    {
        Dictionary<int, StatInfo> ret = new Dictionary<int, StatInfo>();
        foreach (StatInfo stat in stats)
        {
            stat.Hp = stat.MaxHp;
            ret.Add(stat.Level, stat);
        }

        return ret;
    }
}

#endregion Stat

#region Skill

public class Skill
{
    public int id;
    public string name;
    public float cooldown;
    public int damage;
    public SkillType skillType;
    public ProjectileInfo projectile;
}

public class ProjectileInfo
{
    public string name;
    public float speed;
    public int range;
}

public class SkillData : ILoader<int, Skill>
{
    public List<Skill> skills = new List<Skill>();

    public Dictionary<int, Skill> MakeDictionary()
    {
        Dictionary<int, Skill> ret = new Dictionary<int, Skill>();
        foreach (Skill skill in skills)
            ret.Add(skill.id, skill);

        return ret;
    }
}

#endregion Skill
