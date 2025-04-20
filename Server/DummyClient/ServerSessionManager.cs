namespace DummyClient;

internal class ServerSessionManager
{
    private static ServerSessionManager _instance;
    private object _lock = new object();

    private List<ServerSession> _sessions = new List<ServerSession>();

    public static ServerSessionManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ServerSessionManager();

            return _instance;
        }
    }

    public int ServerSessionCount => _sessions.Count;

    public void AddSession(ServerSession session)
    {
        lock (_lock)
            _sessions.Add(session);
    }

    public void RemoveSession(ServerSession session)
    {
        lock (_lock)
            _sessions.Remove(session);
    }

    public void SendChatPacket()
    {
        C2S_Chat chatPkt = new C2S_Chat();
        chatPkt.chat = "Hello Server";

        ArraySegment<byte> sendData = chatPkt.Serialize();
        lock (_lock)
        {
            foreach (var session in _sessions)
                session.Send(sendData);
        }
    }
}