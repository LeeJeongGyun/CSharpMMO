namespace DummyTest.Session;

public class SessionManager
{
    private HashSet<ServerSession> _sessions = new HashSet<ServerSession>();
    private object _lock = new object();
    private int _dummyId = 1;
    public static SessionManager Instacne { get; } = new SessionManager();

    public ServerSession Add()
    {
        lock (_lock)
        {
            ServerSession session = new ServerSession() { DummyId = _dummyId++ };

            _sessions.Add(session);
            Console.WriteLine($"Conntected {_sessions.Count}");
            return session;
        }
    }

    public void Remove(ServerSession session)
    {
        lock (_lock)
        {
            _sessions.Remove(session);
            Console.WriteLine($"Conntected {_sessions.Count}");
        }
    }
}
