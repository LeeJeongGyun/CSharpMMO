using System;
using System.Collections;
using Protocol;
using UnityEngine;

public class CreatureController : BaseController
{
    protected HpBar _hpBar = null;
    protected Coroutine _coScytheSkill = null;
    protected Coroutine _coGunSkill = null;
    public SkillType SkillType { get; set; } = SkillType.None;

    public int Hp
    {
        get => StatInfo.Hp;
        set
        {
            StatInfo.Hp = Math.Max(value, 0);
            UpdateHpBar();
        }
    }

    public int MaxHp => StatInfo.MaxHp;

    public void UpdateHpBar()
    {
        if (_hpBar == null)
            return;

        float ratio = MaxHp > 0 ? (float)Hp / MaxHp : 0;
        _hpBar.SetHpBar(ratio);
    }

    public void AddHpBar()
    {
        GameObject hpBar = Managers.Resource.Instantiate("UI/HpBar", transform);
        if (hpBar != null)
        {
            hpBar.name = "HpBar";
            hpBar.transform.localPosition = new Vector3(0, 0.4f, 0);
            _hpBar = hpBar.GetComponent<HpBar>();

            UpdateHpBar();
        }
    }

    public void RemoveHpBar()
    {
        if (_hpBar != null)
            Managers.Resource.Destroy(_hpBar.gameObject);

        _hpBar = null;
    }

    public Vector3Int GetFrontCellPos() => Dir switch
    {
        MoveDir.Up => CellPos + Vector3Int.up,
        MoveDir.Down => CellPos + Vector3Int.down,
        MoveDir.Left => CellPos + Vector3Int.left,
        MoveDir.Right => CellPos + Vector3Int.right,
        _ => CellPos
    };

    public virtual void OnDead()
    {
        State = ObjectState.Dead;

        // DieEffect 생성
        GameObject dieEffect = Managers.Resource.Instantiate("Misc/DieEffect");
        dieEffect.transform.position = transform.position;
        GameObject.Destroy(dieEffect, 0.1f);
    }

    protected override void Init()
    {
        base.Init();
        AddHpBar();
    }

    protected override void UpdateController()
    {
        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
    }

    protected override void CanKeepMoving()
    {
        Vector3Int dstCellPos = CellPos;
        switch (Dir)
        {
        case MoveDir.Up:
            dstCellPos += Vector3Int.up;
            break;

        case MoveDir.Down:
            dstCellPos += Vector3Int.down;
            break;

        case MoveDir.Left:
            dstCellPos += Vector3Int.left;
            break;

        case MoveDir.Right:
            dstCellPos += Vector3Int.right;
            break;
        }

        // 맵 충돌 처리
        if (Managers.Map.FindCollision(dstCellPos))
        {
            State = ObjectState.Idle;
            return;
        }

        // 몬스터 or 플레이어 충돌 처리
        GameObject go = Managers.Object.FindObject(dstCellPos);
        if (go != null)
        {
            State = ObjectState.Idle;
            return;
        }

        // TODO 방향 바뀌거나 or 멈출 때만 보내도록 수정
        C2S_Move movePacket = new C2S_Move();
        movePacket.PosInfo = new PositionInfo();

        Vector3Int nextCellPos = GetFrontCellPos();
        movePacket.PosInfo.Dir = Dir;
        movePacket.PosInfo.PosX = nextCellPos.x;
        movePacket.PosInfo.PosY = nextCellPos.y;
        Managers.Network.Send(movePacket);

        CellPos = dstCellPos;
    }

    protected override void UpdateAnimation()
    {
        switch (State)
        {
        case ObjectState.Idle:
            if (Dir == MoveDir.Up)
                _animator.Play("IDLE_UP");
            else if (Dir == MoveDir.Down)
                _animator.Play("IDLE_DOWN");
            else if (Dir == MoveDir.Left)
            {
                _spriteRenderer.flipX = false;
                _animator.Play("IDLE_LEFT");
            }
            else
            {
                _spriteRenderer.flipX = true;
                _animator.Play("IDLE_LEFT");
            }
            break;

        case ObjectState.Moving:
            if (Dir == MoveDir.Up)
                _animator.Play("WALK_UP");
            else if (Dir == MoveDir.Down)
                _animator.Play("WALK_DOWN");
            else if (Dir == MoveDir.Left)
            {
                _spriteRenderer.flipX = false;
                _animator.Play("WALK_LEFT");
            }
            else
            {
                _spriteRenderer.flipX = true;
                _animator.Play("WALK_LEFT");
            }
            break;

        case ObjectState.Skill:
            if (Dir == MoveDir.Up)
                _animator.Play(SkillType == SkillType.Scythe ? "SCYTHE_ATTACK_UP" : "GUN_ATTACK_UP");
            else if (Dir == MoveDir.Down)
                _animator.Play(SkillType == SkillType.Scythe ? "SCYTHE_ATTACK_DOWN" : "GUN_ATTACK_DOWN");
            else if (Dir == MoveDir.Left)
            {
                _spriteRenderer.flipX = false;
                if (SkillType == SkillType.Scythe)
                    ChangeSkillSpriteRenderderFlipX("Scythe_Attack_Left", false);
                else
                    ChangeSkillSpriteRenderderFlipX("Gun_Attack_Left", false);

                _animator.Play(SkillType == SkillType.Scythe ? "SCYTHE_ATTACK_LEFT" : "GUN_ATTACK_LEFT");
            }
            else
            {
                _spriteRenderer.flipX = true;
                if (SkillType == SkillType.Scythe)
                    ChangeSkillSpriteRenderderFlipX("Scythe_Attack_Left", true);
                else
                    ChangeSkillSpriteRenderderFlipX("Gun_Attack_Left", true);

                _animator.Play(SkillType == SkillType.Scythe ? "SCYTHE_ATTACK_LEFT" : "GUN_ATTACK_LEFT");
            }
            break;

        case ObjectState.Dead:
            break;
        }
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();
    }

    protected override void UpdateDead()
    {
    }

    /// <summary>
    /// Sprite 좌우 대칭
    /// </summary>
    /// <param name="spriteName">좌우 대칭할 Sprite의 부모 Name</param>
    /// <param name="flipFlag">true는 좌우 대칭 o, false는 좌우 대칭 x</param>
    private void ChangeSkillSpriteRenderderFlipX(string spriteName, bool flipFlag)
    {
        GameObject go = Util.FindChild(gameObject, spriteName);
        if (go != null)
        {
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                GameObject child = go.transform.GetChild(i).gameObject;
                child.GetComponent<SpriteRenderer>().flipX = flipFlag;

                // 자식 객체의 상대 위치 x좌표 좌우 반전
                Vector3 localPos = child.transform.localPosition;
                localPos.x = Mathf.Abs(localPos.x) * (flipFlag ? 1 : -1);
                child.transform.localPosition = localPos;
            }
        }
    }
}
