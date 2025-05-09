using System.Collections;
using System.Collections.Generic;
using Protocol;
using UnityEngine;
using UnityEngine.UI;

public class UI_SelectServerPopup_Item : UI_Base
{
    private ServerInfo _serverInfo;

    private enum Buttons
    {
        ServerButton
    }

    private enum Texts
    {
        ServerButtonText
    }

    public override void Init()
    {
    }

    public void SetServerInfo(ServerInfo serverInfo)
    {
        _serverInfo = serverInfo;

        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));

        Get<Text>((int)Texts.ServerButtonText).text = _serverInfo.Name;
        Get<Button>((int)Buttons.ServerButton).gameObject.BindEvent(_ =>
        {
            Managers.Network.ConnectToGameServer(_serverInfo.Ip, _serverInfo.Port);
            Managers.Scene.LoadScene(Define.Scene.Game);
            Managers.UI.ClosePopupUI();
        });
    }
}
