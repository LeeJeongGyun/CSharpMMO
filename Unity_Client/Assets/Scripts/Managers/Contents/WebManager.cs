using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WebManager
{
    private string BaseUrl { get; set; } = "https://localhost:7264/api/Account";

    public void SendWebReqPacket<T>(string url, object data, Action<T> callback) => Managers.Instance.StartCoroutine(CoSendWebPacket<T>(url, "post", data, callback));

    private IEnumerator CoSendWebPacket<T>(string url, string method, object data, Action<T> callback)
    {
        string fullUrl = $"{BaseUrl}/{url}";

        byte[] jsonBytes = null;
        if (data != null)
        {
            string jsonData = JsonUtility.ToJson(data);
            jsonBytes = Encoding.UTF8.GetBytes(jsonData);
        }

        using (UnityWebRequest webRequest = new UnityWebRequest(fullUrl, method))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                Debug.Log($"WebPacket Success");
                if (callback != null)
                {
                    T resData = JsonUtility.FromJson<T>(webRequest.downloadHandler.text);
                    callback.Invoke(resData);
                }
            }
        }
    }
}
