using Server.Content.Object;

namespace Server.Content;

using Protocol;
using Server.Content.Job;
using Server.Content.Room;
using Server.Data;

public partial class GameRoom : JobSerializer
{
    public void ApplyMove(Player player, PositionInfo posInfo)
    {
        // State가 Idle일 경우 현재 좌표로 보내기 때문에 예외 처리에 넣어준다.
        if (false == _map.MoveObject(player, new Vector2Int(posInfo.PosX, posInfo.PosY))
             && posInfo.State != ObjectState.Idle)
            return;

        player.State = posInfo.State;
        player.Dir = posInfo.Dir;

        // 이동 패킷 전송
        var movePacket = new S2C_Move();
        movePacket.ObjectId = player.ObjectId;
        movePacket.PosInfo = posInfo;
        BroadcastMessage(player.CellPos, movePacket, excludeId: player.ObjectId);
    }

    public void ApplySkill(Player player, SkillInfo skillInfo)
    {
        if (player.State != ObjectState.Idle)
            return;

        Data.Skill? skillData = null;
        if (false == DataManager.Skills.TryGetValue(skillInfo.SkillId, out skillData))
            return;

        switch (skillData.skillType)
        {
        case SkillType.Scythe:
            {
                GameObject? target = _map.FindObject(player.GetFrontCellPos());
                if (target != null)
                    target.OnDamaged(player, skillData.damage);
            }
            break;

        case SkillType.Bullet:
            {
                Bullet bullet = ObjectManager.Instance.AddObject<Bullet>();
                bullet.State = ObjectState.Moving;
                bullet.Dir = player.Dir;
                bullet.CellPos = player.CellPos;

                bullet.Owner = player;
                bullet.Room = this;
                bullet.SkillData = skillData;
                EnterRoom(bullet);
            }
            break;
        }

        player.State = ObjectState.Skill;

        // 스킬 사용 패킷 방에 있는 유저들에게 전송
        var skillPacket = new S2C_Skill();
        skillPacket.ObjectId = player.ObjectId;
        skillPacket.SkillInfo = skillInfo;
        BroadcastMessage(player.CellPos, skillPacket);
    }
}
