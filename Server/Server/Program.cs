using System.Net;
using System.Net.Sockets;
using System.Timers;
using Server.Content;
using Server.Data;
using Server.DB;
using ServerCore;

namespace Server;

internal class Program
{
    private static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();

    /// <summary>
    /// GameRoom에서 주기적으로 호출될 함수 등록
    /// </summary>
    /// <param name="room">주기적으로 호출될 함수를 등록할 GameRoom</param>
    /// <param name="periodicTick">주기적으로 호출될 시간</param>
    private static void TickRoom(GameRoom room, int periodicTick = 100)
    {
        if (room == null)
            return;

        System.Timers.Timer timer = new System.Timers.Timer();
        timer.Elapsed += (s, e) => { room.Update(); };
        timer.Interval = periodicTick;
        timer.AutoReset = true;
        timer.Enabled = true;

        _timers.Add(timer);
    }

    private static void Main(string[] args)
    {
        // Config 파일 Load
        ConfigManager.LoadConfig();
        DataManager.LoadData();

        // Logger 등록
        GlobalLogger.WriteLog += log => Console.WriteLine(log);

        // 1번 GameRoom 생성
        GameRoom room = RoomManager.Instance.AddRoom();
        TickRoom(room, 50);

        PerformanceProfiler.Instance.prevRecvCount = 0;
        string hostName = Dns.GetHostName();
        IPHostEntry entrys = Dns.GetHostEntry(hostName, AddressFamily.InterNetwork);

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7777);

        Listener listener = new Listener(() => new ClientSession());
        listener.Start(endPoint, 1);

        Thread.Sleep(1000);
        Console.WriteLine("Listening...");
        while (true)
        {
            //room.Flush();
            //room.Push(room.Update);
            Thread.Sleep(1000);
        }
    }
}
