using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerCore
{
    public class Connector
    {
        public static int ConnectFailCount = 0;
        private Func<Session> _sessionFactory = null!;

        public Connector(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public void Connect(IPEndPoint endPoint, int connectCount = 1)
        {
            for (int i = 0; i < connectCount; ++i)
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.UserToken = socket;
                args.Completed += OnCompletedConnect;
                args.RemoteEndPoint = endPoint;

                RegisterConnect(args);
            }
        }

        private void OnCompletedConnect(object? sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory();
                session.Start(args.ConnectSocket!);
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                GlobalLogger.WriteLog?.Invoke($"OnCompletedConnect Error: {args.SocketError}");
                Interlocked.Increment(ref ConnectFailCount);
            }
        }

        private void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket? clientSocket = args.UserToken as Socket;
            if (clientSocket != null)
            {
                bool pending = clientSocket.ConnectAsync(args);
                if (!pending)
                    OnCompletedConnect(null, args);
            }
        }
    }
}
