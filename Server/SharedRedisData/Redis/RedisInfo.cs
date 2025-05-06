namespace SharedRedisData.Redis;

using CloudStructures;

public static class RedisInfo
{
    public static string RedisAddress { get; set; } = "localhost:6379";
    public static RedisConnection Connection { get; } = new RedisConnection(new RedisConfig("CommonData", RedisAddress));
}
