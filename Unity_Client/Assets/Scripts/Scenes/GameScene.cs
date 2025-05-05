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
    }
}
