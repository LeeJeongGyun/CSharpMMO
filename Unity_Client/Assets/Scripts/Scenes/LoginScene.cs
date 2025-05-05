using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LoginScene : BaseScene
{
    public override void Clear()
    {
    }

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Login;

        GameObject loginMenu = GameObject.Find("UI_LoginScene");
        if (loginMenu == null)
            Managers.UI.ShowPopupUI<UI_LoginScene>();
    }
}
