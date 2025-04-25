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

        /// <summary>
        /// Connector를 초기화합니다.
        /// </summary>
        /// <param name="sessionFactory">서버와 연결 성공 시 사용할 Session 객체를 생성하는 팩토리 함수입니다.</param>
        public Connector(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <summary>
        /// 지정한 수만큼 서버에 비동기 연결 요청을 수행합니다.
        /// </summary>
        /// <param name="endPoint">연결할 서버의 IP 엔드포인트입니다.</param>
        /// <param name="connectCount">생성할 연결 요청의 수입니다. 기본값은 1입니다.</param>
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

        /// <summary>
        /// 연결 요청 완료 시 호출되는 콜백 함수입니다.
        /// </summary>
        /// <param name="sender">이벤트를 발생시킨 객체입니다.</param>
        /// <param name="args">연결 요청 작업의 결과를 포함한 이벤트 인자입니다.</param>
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

        /// <summary>
        /// 비동기 연결 요청을 등록합니다.
        /// </summary>
        /// <param name="args">연결 요청에 사용할 SocketAsyncEventArgs 객체입니다.</param>
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
