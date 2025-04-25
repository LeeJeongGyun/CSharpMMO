using Data;
using Google.Protobuf;
using Protocol;
using ServerCore;
using UnityEngine;

public class PacketHandler
{
    public static void S2C_EnterRoomHandler(PacketSession session, IMessage message)
    {
        S2C_EnterRoom enterRoomPacket = message as S2C_EnterRoom;
        ServerSession serverSession = session as ServerSession;

        serverSession.ObjectId = enterRoomPacket.ObjectInfo.ObjectId;
        Managers.Object.Add(enterRoomPacket.ObjectInfo, myPlayer: true);
    }

    public static void S2C_LeaveRoomHandler(PacketSession session, IMessage message)
    {
        S2C_LeaveRoom leaveRoomPacket = message as S2C_LeaveRoom;
        ServerSession serverSession = session as ServerSession;
        Managers.Object.Remove(serverSession.ObjectId);
    }

    public static void S2C_SpawnHandler(PacketSession session, IMessage message)
    {
        S2C_Spawn spawnPacket = message as S2C_Spawn;

        foreach (var objectInfo in spawnPacket.ObjectInfos)
            Managers.Object.Add(objectInfo, myPlayer: false);
    }

    public static void S2C_DespawnHandler(PacketSession session, IMessage message)
    {
        S2C_Despawn despawnPacket = message as S2C_Despawn;
        Managers.Object.Remove(despawnPacket.ObjectId);
    }

    public static void S2C_MoveHandler(PacketSession session, IMessage message)
    {
        S2C_Move movePacket = message as S2C_Move;

        GameObject go = Managers.Object.FindObject(movePacket.ObjectId);
        if (go != null)
        {
            BaseController bc = go.GetComponent<BaseController>();
            if (bc != null)
                bc.PosInfo = movePacket.PosInfo;
        }
    }

    public static void S2C_SkillHandler(PacketSession session, IMessage message)
    {
        S2C_Skill skillPacket = message as S2C_Skill;

        GameObject go = Managers.Object.FindObject(skillPacket.ObjectId);
        if (go != null)
        {
            CreatureController cc = go.GetComponent<CreatureController>();
            if (cc != null)
            {
                Skill skill;
                Managers.Data.Skills.TryGetValue(skillPacket.SkillInfo.SkillId, out skill);
                cc.SkillType = skill.skillType;
                cc.State = ObjectState.Skill;
            }
        }
    }

    public static void S2C_UpdateHpHandler(PacketSession session, IMessage message)
    {
        S2C_UpdateHp updateHpPacket = message as S2C_UpdateHp;

        GameObject go = Managers.Object.FindObject(updateHpPacket.ObjectId);
        if (go != null)
        {
            CreatureController cc = go.GetComponent<CreatureController>();
            if (cc != null)
                cc.Hp = updateHpPacket.Hp;
        }
    }

    public static void S2C_DieHandler(PacketSession session, IMessage message)
    {
        S2C_Die updateHpPacket = message as S2C_Die;

        GameObject go = Managers.Object.FindObject(updateHpPacket.ObjectId);
        if (go != null)
        {
            CreatureController cc = go.GetComponent<CreatureController>();
            if (cc != null)
                cc.OnDead();
        }
    }

    public static void S2C_ConnectedHandler(PacketSession session, IMessage message)
    {
        Debug.Log($"S2C_ConntectedHandler");

        C2S_Login loginPacket = new C2S_Login();
        loginPacket.UniqueId = SystemInfo.deviceUniqueIdentifier;
        Managers.Network.Send(loginPacket);
    }

    public static void S2C_LoginHandler(PacketSession session, IMessage message)
    {
        S2C_Login loginPacket = message as S2C_Login;
        if (loginPacket == null)
            return;

        // TODO: 로비 UI에서 캐릭터 선택할 수 있도록 유도

        Debug.Log($"S2C_LoginHandler");
        Debug.Log($"Login Response: {loginPacket.LoginOk}");
        if (loginPacket.PlayerInfos.Count == 0)
        {
            // 플레이어 생성
            C2S_CreatePlayer createPlayerPacket = new C2S_CreatePlayer();
            createPlayerPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";
            Managers.Network.Send(createPlayerPacket);
        }
        else
        {
            // 게임 입장
            C2S_EnterRoom enterRoomPacket = new C2S_EnterRoom();
            enterRoomPacket.Name = loginPacket.PlayerInfos[0].Name;
            Managers.Network.Send(enterRoomPacket);
        }
    }

    public static void S2C_CreatePlayerHandler(PacketSession session, IMessage message)
    {
        S2C_CreatePlayer createPlayer = message as S2C_CreatePlayer;
        LobbyPlayerInfo playerInfo = createPlayer.PlayerInfo;
        if (playerInfo == null)
        {
            Debug.Log("S2C_CreatePlayerHandler PlayerInfo Null");
            return;
        }

        C2S_EnterRoom enterRoomPacket = new C2S_EnterRoom();
        enterRoomPacket.Name = playerInfo.Name;
        Managers.Network.Send(enterRoomPacket);
    }

    public static void S2C_ItemListHandler(PacketSession session, IMessage message)
    {
        S2C_ItemList itemListPacket = message as S2C_ItemList;
        if (itemListPacket == null)
            return;

        Managers.Inven.Clear();

        // 메모리에 아이템 적용
        foreach (var itemInfo in itemListPacket.ItemInfos)
        {
            Item item = Item.MakeItem(itemInfo);
            Managers.Inven.Add(item);
        }
    }

    public static void S2C_UpdateItemHandler(PacketSession session, IMessage message)
    {
        S2C_UpdateItem updateItemPacket = message as S2C_UpdateItem;

        // 1) 메모리 갱신
        ItemInfo itemInfo = updateItemPacket.ItemInfo;
        Managers.Inven.Add(Item.MakeItem(itemInfo));

        // 2) UI 갱신
        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        gameScene.InvenUI.RefreshUI();

        Debug.Log($"Item을 획득했습니다.");
    }
}
