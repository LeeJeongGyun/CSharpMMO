using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using Server.Content;
using Server.Data;
using Server.DB;
using Server.Session;
using ServerCore;

namespace Server;

internal class Program
{
    private static List<Thread> _threads = new List<Thread>();

    private static void Main(string[] args)
    {
        // Config 파일 Load
        ConfigManager.LoadConfig();
        DataManager.LoadData();

        // Logger 등록
        GlobalLogger.WriteLog += log => Console.WriteLine(log);

        // 1번 GameRoom 생성
        GameLogic.Instance.Push(() => GameLogic.Instance.AddRoom());
        //GameRoom room = GameLogic.Instance.AddRoom();

        PerformanceProfiler.Instance.prevRecvCount = 0;
        string hostName = Dns.GetHostName();
        IPHostEntry entrys = Dns.GetHostEntry(hostName, AddressFamily.InterNetwork);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7777);

        Listener listener = new Listener(() => SessionManager.Instance.Add());
        listener.Start(endPoint, 1);
        Console.WriteLine("Listening...");

        _threads.Add(new Thread(GameLogicThread) { Name = "GameLogicThread" });
        _threads.Add(new Thread(DbThread) { Name = "DbThread" });
        _threads.Add(new Thread(SendThread) { Name = "SendThread" });
        Thread.CurrentThread.Name = "MainThread";

        foreach (var thread in _threads)
            thread.Start();

        while (true)
        {
            Console.WriteLine($"SessionCount: {SessionManager.Instance.GetSessionCount()}");
            Thread.Sleep(1000);
        }

        foreach (var thread in _threads)
            thread.Join();
    }

    /// <summary>
    /// GameLogic 처리를 담당할 스레드
    /// 모든 GameRoom의 JobQueue를 순회하며 실행
    /// </summary>
    /// <param name="_"></param>
    /// <remarks>
    /// CPU가 튀는 것을 방지하기 위해서 Thread.Sleep(0) 삽입
    /// </remarks>
    private static void GameLogicThread(object? _)
    {
        Console.WriteLine("GameLogicThread Start");
        while (true)
        {
            GameLogic.Instance.Flush();
            Thread.Sleep(0);
        }
    }

    /// <summary>
    /// Db 처리를 담당할 스레드
    /// </summary>
    /// <param name="_"></param>
    /// <remarks>
    /// CPU가 튀는 것을 방지하기 위해서 Thread.Sleep(0) 삽입
    /// </remarks>
    private static void DbThread(object? _)
    {
        Console.WriteLine("DbThread Start");
        while (true)
        {
            DBTransaction.Instance.Flush();
            Thread.Sleep(0);
        }
    }

    /// <summary>
    /// 송신 처리를 담당할 스레드
    /// </summary>
    /// <param name="_"></param>
    /// <remarks>
    /// CPU가 튀는 것을 방지하기 위해서 Thread.Sleep(0) 삽입
    /// </remarks>
    private static void SendThread(object? _)
    {
        Console.WriteLine("SendThread Start");
        while (true)
        {
            foreach (ClientSession session in SessionManager.Instance.GetSessions())
                session.FlushSend();

            Thread.Sleep(0);
        }
    }
}
