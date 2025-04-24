using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameScene : UI_Scene
{
    public UI_Stat StatUI { get; private set; }

    public UI_Inventory InvenUI { get; private set; }

    public override void Init()
    {
        base.Init();

        StatUI = Util.FindChild<UI_Stat>(gameObject, recursive: true);
        StatUI.gameObject.SetActive(false);

        InvenUI = Util.FindChild<UI_Inventory>(gameObject, recursive: true);
        InvenUI.gameObject.SetActive(false);
    }
}
