using System.Collections;
using Protocol;
using UnityEngine;

public class PlayerController : CreatureController
{
    public override void OnDead()
    {
        base.OnDead();
        Managers.Object.Remove(Id);
    }

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();
    }

    protected override void CanKeepMoving()
    {
    }
}
