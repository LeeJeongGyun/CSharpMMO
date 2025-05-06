using System.Collections.Generic;

public class CreateAccountReq
{
    public string AccountName;
    public string Password;
}

public class CreateAccountRes
{
    public bool Result;
}

public class LoginAccountReq
{
    public string AccountName;
    public string Password;
}

public class ServerInfo
{
    public string Name;
    public string Ip;
    public int Port;
    public int ServerLoad;
}

public class LoginAccountRes
{
    public bool Result;
    public int AccountDbId;
    public int UserToken;
    public List<ServerInfo> ServerInfos = new List<ServerInfo>();
}
