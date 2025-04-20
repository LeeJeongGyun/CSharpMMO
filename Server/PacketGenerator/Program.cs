namespace PacketGenerator;

internal class Program
{
    private static void Main(string[] args)
    {
        string protoFile = @"..\..\..\Common\protoc-30.0-win64\bin\Protocol.proto";
        if (args.Length >= 1)
            protoFile = args[0];

        string clientRegister = "";
        string serverRegister = "";
        bool startParsing = false;
        foreach (string line in File.ReadLines(protoFile))
        {
            if (line.Contains("enum PacketID"))
            {
                startParsing = true;
                continue;
            }

            if (!startParsing)
                continue;

            if (line.Contains('}'))
                break;

            string[] names = line.Split("=");
            if (names.Length == 0)
                continue;

            string name = names[0].Trim();
            string[] words = name.Split('_');

            string msgName = words[0];
            for (int i = 1; i < words.Length; ++i)
                msgName += FirstCharToUpper(words[i]);

            if (name.StartsWith("C2S_"))
            {
                string pktName = $"C2S_{msgName.Substring(3)}";
                serverRegister += string.Format(PacketFormat.managerRegisterFormat, msgName, pktName);
            }
            else if (name.StartsWith("S2C_"))
            {
                string pktName = $"S2C_{msgName.Substring(3)}";
                clientRegister += string.Format(PacketFormat.managerRegisterFormat, msgName, pktName);
            }
        }

        string serverManagerText = string.Format(PacketFormat.managerFormat, "ServerPacketManager", serverRegister.Replace("\n", "\n\t\t"));
        string clientManagerText = string.Format(PacketFormat.managerFormat, "ClientPacketManager", clientRegister.Replace("\n", "\n\t\t"));

        File.WriteAllText("ServerPacketManager.cs", serverManagerText);
        File.WriteAllText("ClientPacketManager.cs", clientManagerText);

        static string FirstCharToUpper(string word)
        {
            return word[0].ToString().ToUpper() + word.Substring(1).ToLower();
        }
    }
}
