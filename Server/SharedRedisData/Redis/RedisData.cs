namespace SharedRedisData.Redis;

public class RedisUserToken
{
    public int AccountDbId { get; set; }
    public int Token { get; set; }
}

public class RedisServerInfo
{
    public string Name { get; set; }
    public string Ip { get; set; }
    public int Port { get; set; }
    public int ServerLoad { get; set; }
}
