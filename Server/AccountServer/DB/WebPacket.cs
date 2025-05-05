namespace AccountServer.DB;

public class CreateAccountReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class CreateAccountRes
{
    public bool Result { get; set; }
}

public class LoginAccountReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class ServerInfo
{
    public string Name { get; set; }
    public string Ip { get; set; }
}

public class LoginAccountRes
{
    public bool Result { get; set; }
    public List<ServerInfo> ServerInfos { get; set; } = new List<ServerInfo>();
}
