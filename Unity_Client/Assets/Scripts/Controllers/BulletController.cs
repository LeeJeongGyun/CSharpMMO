using System.Collections;
using Protocol;
using UnityEngine;

public class BulletController : BaseController
{
    protected override void Init()
    {
        base.Init();

        switch (Dir)
        {
        case MoveDir.Up:
            transform.rotation = Quaternion.Euler(0, 0, -90);
            break;

        case MoveDir.Down:
            transform.rotation = Quaternion.Euler(0, 0, 90);
            break;

        case MoveDir.Left:
            transform.rotation = Quaternion.Euler(0, 0, 0);
            break;

        case MoveDir.Right:
            transform.rotation = Quaternion.Euler(0, 0, 180);
            break;
        }

        Data.Skill skillData;
        if (Managers.Data.Skills.TryGetValue(2, out skillData))
        {
            StatInfo.Speed = skillData.projectile.speed;
        }
    }

    protected override void CanKeepMoving()
    {
    }
}
