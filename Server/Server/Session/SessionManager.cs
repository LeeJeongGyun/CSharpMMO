namespace Server.Session;

public class SessionManager
{
    #region 싱글톤

    public static SessionManager Instance { get; } = new SessionManager();

    #endregion 싱글톤

    private object _lock = new object();
    private int _sessionId = 0;
    private Dictionary<int, ClientSession> _sessions { get; } = new Dictionary<int, ClientSession>();

    public List<ClientSession> GetSessions()
    {
        List<ClientSession> sessions = new List<ClientSession>();
        lock (_lock)
            sessions = _sessions.Values.ToList();

        return sessions;
    }

    public ClientSession Add()
    {
        lock (_lock)
        {
            ClientSession session = new ClientSession() { SessionId = ++_sessionId };
            _sessions.Add(session.SessionId, session);
            return session;
        }
    }

    public void Remove(int sessionId)
    {
        lock (_lock)
            _sessions.Remove(sessionId);
    }

    public ClientSession? Find(int sessionId)
    {
        lock (_lock)
        {
            if (_sessions.TryGetValue(sessionId, out ClientSession? session))
                return session;
        }

        return null;
    }
}
