namespace Server.Content.Object;

using Protocol;

public class Projectile : GameObject
{
    public Projectile()
    {
        ObjectType = ObjectType.Projectile;
    }

    public Data.Skill? SkillData { get; set; }

    public virtual void Update()
    { }
}
