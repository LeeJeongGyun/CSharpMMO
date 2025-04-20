namespace Server.Data;

using Newtonsoft.Json;

public class ServerConfig
{
    public string dataPath;
    public string dbConnectionString;
}

public class ConfigManager
{
    public static ServerConfig Config { get; private set; }

    public static void LoadConfig()
    {
        string configText = File.ReadAllText("config.json");
        Config = JsonConvert.DeserializeObject<ServerConfig>(configText);
    }
}
