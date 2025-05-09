using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_SelectServerPopup : UI_Popup
{
    private List<ServerInfo> _serverInfos;

    public override void Init()
    {
        base.Init();
    }

    public void SetServerInfos(List<ServerInfo> serverInfos)
    {
        if (_serverInfos != null)
            _serverInfos.Clear();

        _serverInfos = serverInfos;

        GameObject grid = Util.FindChild(gameObject, "Grid", true);
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        foreach (var serverInfo in _serverInfos)
        {
            GameObject popupItem = Managers.Resource.Instantiate("UI/Popup/UI_SelectServerPopup_Item", grid.transform);
            UI_SelectServerPopup_Item component = popupItem.GetOrAddComponent<UI_SelectServerPopup_Item>();
            component.SetServerInfo(serverInfo);
        }
    }
}
