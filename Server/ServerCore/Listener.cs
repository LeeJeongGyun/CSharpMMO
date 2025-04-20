using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Listener
    {
        private Socket _listenSock = null!;
        private Func<Session> _sessionFactory = null!;

        public Listener(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public void Start(IPEndPoint endPoint, int registerCount = 1, int backlog = 100)
        {
            _listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSock.Bind(endPoint);
            _listenSock.Listen(backlog);

            for (int i = 0; i < registerCount; ++i)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnCompletedAccept;
                RegisterAccept(args);
            }
        }

        private void OnCompletedAccept(object? sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session clientSession = _sessionFactory();
                clientSession.Start(args.AcceptSocket!);
                clientSession.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                GlobalLogger.WriteLog?.Invoke($"OnCompletedAccept Error: {args.SocketError}");
            }

            RegisterAccept(args);
        }

        private void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending = _listenSock.AcceptAsync(args);
            if (!pending)
                OnCompletedAccept(null, args);
        }
    }
}
