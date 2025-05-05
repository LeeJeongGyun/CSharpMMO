using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoginScene : UI_Popup
{
    private InputField _idField;
    private InputField _passwordField;

    private enum GameObjects
    {
        IdField,
        PasswordField,
    }

    private enum Buttons
    {
        CreateButton,
        LoginButton,
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));

        _idField = Get<GameObject>((int)GameObjects.IdField).gameObject.GetComponent<InputField>();
        _passwordField = Get<GameObject>((int)GameObjects.PasswordField).gameObject.GetComponent<InputField>();

        GetButton((int)Buttons.CreateButton).onClick.AddListener(OnCreateButtonClicked);
        GetButton((int)Buttons.LoginButton).onClick.AddListener(OnLoginButtonClicked);
    }

    private void OnCreateButtonClicked()
    {
        CreateAccountReq createAccountReq = new CreateAccountReq();
        createAccountReq.AccountName = _idField.text;
        createAccountReq.Password = _passwordField.text;
        Managers.Web.SendWebReqPacket<CreateAccountRes>("create", createAccountReq, res =>
        {
            Debug.Log($"Create Account: {res.Result}");
            _idField.text = "";
            _passwordField.text = "";
        });
    }

    private void OnLoginButtonClicked()
    {
        LoginAccountReq loginAccountReq = new LoginAccountReq();
        loginAccountReq.AccountName = _idField.text;
        loginAccountReq.Password = _passwordField.text;
        Managers.Web.SendWebReqPacket<LoginAccountRes>("login", loginAccountReq, res =>
        {
            Debug.Log($"Login Account: {res.Result}");
            _idField.text = "";
            _passwordField.text = "";

            if (res.Result)
            {
                Managers.Scene.LoadScene(Define.Scene.Game);
                Managers.Network.ConnectToGameServer();
            }
        });
    }
}
