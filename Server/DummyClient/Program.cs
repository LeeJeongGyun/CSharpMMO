using System.Net.Sockets;
using System.Net;
using System.Text;
using ServerCore;

namespace DummyClient;

internal class Program
{
    private static void Main(string[] args)
    {
        PerformanceProfiler.Instance.StopPrintPerformanceData();

        string hostName = Dns.GetHostName();
        IPHostEntry entrys = Dns.GetHostEntry(hostName, AddressFamily.InterNetwork);
        IPEndPoint endPoint = new IPEndPoint(entrys.AddressList[0], 7777);

        Connector connector = new Connector(() => new ServerSession());
        connector.Connect(endPoint, 100);
        Thread.Sleep(1000);
        connector.Connect(endPoint, 100);
        Thread.Sleep(1000);
        connector.Connect(endPoint, 100);
        Thread.Sleep(1000);

        while (true)
        {
            Thread.Sleep(250);
            ServerSessionManager.Instance.SendChatPacket();
        }

        //Console.WriteLine($"ConnectFailCount: {Connector.ConnectFailCount}, ConnectCount: {ServerSession.connectCount}, DisconnectCount: {ServerSession.disConnectCount}, RecvCount: {ServerSession.recvCount}");
    }
}