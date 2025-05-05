using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    public UI_GameScene SceneUI { get; private set; }

    public override void Clear()
    {
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;
        Managers.Map.LoadMap(1);

        Screen.SetResolution(800, 600, false);

        SceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();

        // Web Packet Test
        CreateAccountReq createAccountReq = new CreateAccountReq();
        createAccountReq.AccountName = "LEEJK";
        createAccountReq.Password = "1234";
        Managers.Web.SendWebReqPacket<CreateAccountRes>("create", createAccountReq, _ =>
        {
            Debug.Log($"CreateAccount Success");
        });

        LoginAccountReq loginAccountReq = new LoginAccountReq();
        loginAccountReq.AccountName = "LEEJK";
        loginAccountReq.Password = "1234";
        Managers.Web.SendWebReqPacket<LoginAccountRes>("login", loginAccountReq, _ =>
        {
            Debug.Log($"LoginAccount Success");
        });
    }
}
