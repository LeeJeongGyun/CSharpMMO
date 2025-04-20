using System.Collections;
using System.Collections.Generic;
using Protocol;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    private bool _inputKey = true;
    private bool _skillInput = false;

    public override void OnDead()
    {
        base.OnDead();

        // 내가 죽었다면 서버에서 모든 정보 다시 보내주기 때문에 초기화
        Managers.Object.Clear();
    }

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        base.UpdateController();

        switch (State)
        {
        case ObjectState.Idle:
            InputKey();
            InputSkillKey();
            break;

        case ObjectState.Moving:
            InputKey();
            break;

        case ObjectState.Skill:
            break;

        case ObjectState.Dead:
            break;
        }
    }

    protected override void UpdateIdle()
    {
        if (_inputKey)
            State = ObjectState.Moving;
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();

        switch (SkillType)
        {
        case SkillType.Scythe:
            if (_coScytheSkill == null)
                _coScytheSkill = StartCoroutine("CoScytheSkill");

            break;

        case SkillType.Bullet:
            if (_coGunSkill == null)
                _coGunSkill = StartCoroutine("CoGunSkill");

            break;
        }
    }

    protected override void CanKeepMoving()
    {
        if (_inputKey == false)
        {
            State = ObjectState.Idle;
            SendMovePacket();
            return;
        }

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
            return;

        // 몬스터 or 플레이어 충돌 처리
        GameObject go = Managers.Object.FindObject(dstCellPos);
        if (go != null)
            return;

        CellPos = dstCellPos;
        SendMovePacket();
    }

    private void SendMovePacket()
    {
        C2S_Move movePacket = new C2S_Move();
        movePacket.PosInfo = new PositionInfo();
        movePacket.PosInfo = PosInfo;
        Managers.Network.Send(movePacket);
    }

    private IEnumerator CoScytheSkill()
    {
        yield return new WaitForSeconds(1.0f);
        if (State == ObjectState.Skill)
            State = ObjectState.Idle;

        _skillInput = false;
        _coScytheSkill = null;

        // TODO 임시처리, 추후 서버에서 상태 변경하도록 수정
        SendMovePacket();
    }

    private IEnumerator CoGunSkill()
    {
        yield return new WaitForSeconds(0.3f);
        if (State == ObjectState.Skill)
            State = ObjectState.Idle;

        _skillInput = false;
        _coGunSkill = null;

        // TODO 임시처리, 추후 서버에서 상태 변경하도록 수정
        SendMovePacket();
    }

    private void InputKey()
    {
        _inputKey = true;
        if (Input.GetKey(KeyCode.W))
            Dir = MoveDir.Up;
        else if (Input.GetKey(KeyCode.S))
            Dir = MoveDir.Down;
        else if (Input.GetKey(KeyCode.A))
            Dir = MoveDir.Left;
        else if (Input.GetKey(KeyCode.D))
            Dir = MoveDir.Right;
        else
            _inputKey = false;
    }

    private void InputSkillKey()
    {
        if (_skillInput)
            return;

        _skillInput = true;
        if (Input.GetKey(KeyCode.Space))
        {
            C2S_Skill skill = new C2S_Skill();
            skill.SkillInfo = new SkillInfo();
            skill.SkillInfo.SkillId = 1;
            Managers.Network.Send(skill);
        }
        else if (Input.GetKey(KeyCode.Z))
        {
            C2S_Skill skill = new C2S_Skill();
            skill.SkillInfo = new SkillInfo();
            skill.SkillInfo.SkillId = 2;
            Managers.Network.Send(skill);
        }
        else
        {
            _skillInput = false;
        }
    }

    private void LateUpdate() => Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
}
