namespace DummyTest;

using System.Net;
using DummyTest.Session;
using ServerCore;

internal class Program
{
    private static int _dummyCount = 250;

    private static void Main(string[] args)
    {
        Thread.Sleep(3000);

        // 소켓 생성
        string hostName = Dns.GetHostName();
        IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

        Connector connector = new Connector(() => SessionManager.Instacne.Add());

        for (int i = 0; i < 3; ++i)
        {
            connector.Connect(endPoint, _dummyCount);
            Thread.Sleep(2000);
        }

        while (true)
        {
            Thread.Sleep(10000);
        }
    }
}
