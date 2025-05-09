using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LoginScene : BaseScene
{
    public GameObject UiLoginScene { get; private set; }

    public override void Clear() => Managers.Resource.Destroy(UiLoginScene);

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Login;

        UiLoginScene = GameObject.Find("UI_LoginScene");
        if (UiLoginScene == null)
            UiLoginScene = Managers.UI.ShowPopupUI<UI_LoginScene>().gameObject;
    }
}
