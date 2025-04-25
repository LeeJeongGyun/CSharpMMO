using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Listener
    {
        private Socket _listenSock = null!;
        private Func<Session> _sessionFactory = null!;

        /// <summary>
        /// Listener를 초기화합니다.
        /// </summary>
        /// <param name="sessionFactory">클라이언트 접속 시 사용할 Session 객체를 생성하는 팩토리 함수입니다.</param>
        public Listener(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <summary>
        /// 서버에서 Listen을 시작하고 지정한 개수만큼 비동기 Accept 등록을 수행합니다.
        /// </summary>
        /// <param name="endPoint">바인딩할 IP 엔드포인트입니다.</param>
        /// <param name="registerCount">동시에 등록할 비동기 Accept 요청 수입니다. 기본값은 1입니다.</param>
        /// <param name="backlog">백로그 큐의 최대 크기입니다. 기본값은 100입니다.</param>
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

        /// <summary>
        /// Accept 완료 시 호출되는 콜백 함수입니다.
        /// </summary>
        /// <param name="sender">이벤트를 발생시킨 객체입니다.</param>
        /// <param name="args">비동기 Accept 작업의 결과를 포함한 이벤트 인자입니다.</param>
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

        /// <summary>
        /// 비동기 Accept를 등록합니다.
        /// </summary>
        /// <param name="args">Accept 요청에 사용할 SocketAsyncEventArgs 객체입니다.</param>
        private void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending = _listenSock.AcceptAsync(args);
            if (!pending)
                OnCompletedAccept(null, args);
        }
    }
}
