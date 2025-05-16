using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerCore
{
    /// <summary>
    /// 패킷 단위로 데이터를 처리하는 세션 추상 클래스입니다.
    /// 수신 버퍼에서 유효한 패킷을 분리하여 처리합니다.
    /// </summary>
    public abstract class PacketSession : Session
    {
        /// <summary>
        /// 패킷 단위로 수신된 데이터를 처리하는 콜백 함수입니다.
        /// </summary>
        /// <param name="buffer">하나의 완성된 패킷 데이터를 포함하는 버퍼입니다.</param>
        public abstract void OnPacketRecv(ArraySegment<byte> buffer);

        /// <summary>
        /// 수신 버퍼에서 유효한 패킷들을 파싱하고, 각 패킷마다 <see cref="OnPacketRecv"/> 콜백 함수를 호출합니다.
        /// 처리한 총 바이트 수를 반환하여 이후 버퍼에서 제거할 수 있도록 합니다.
        /// </summary>
        /// <param name="buffer">수신된 원시 데이터 버퍼입니다.</param>
        /// <returns>처리한 총 바이트 수입니다.</returns>
        public override sealed int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;
            while (true)
            {
                if (buffer.Count < processLen + 4)
                    break;

                ushort size = BitConverter.ToUInt16(buffer.Array!, buffer.Offset + processLen);
                if (buffer.Count < processLen + size)
                    break;

                // Performance Log
                //Interlocked.Increment(ref PerformanceProfiler.Instance.prevRecvCount);
                Interlocked.Add(ref PerformanceProfiler.Instance.prevRecvCount, size);

                OnPacketRecv(buffer.Slice(processLen, size));
                processLen += size;
            }

            return processLen;
        }
    }

    /// <summary>
    /// 네트워크 통신을 위한 기본 세션 추상 클래스입니다.
    /// 수신, 송신, 연결 종료 등의 이벤트에 대한 콜백 함수를 정의합니다.
    /// </summary>
    public abstract class Session
    {
        private long _disconnect = 0;
        private object _lockObj = new object();
        private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        private RecvBuffer _recvBuffer = new RecvBuffer(65535);
        private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        private bool _sendFlag = true;
        private Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        private Socket _socket = null!;

        #region Content Overloading

        /// <summary>
        /// 연결이 성공적으로 완료되었을 때 호출됩니다.
        /// </summary>
        /// <param name="endPoint">연결된 상대방의 <see cref="EndPoint"/> 정보입니다.</param>
        public abstract void OnConnected(EndPoint? endPoint);

        /// <summary>
        /// 연결이 끊어졌을 때 호출됩니다.
        /// </summary>
        /// <param name="endPoint">끊어진 상대방의 <see cref="EndPoint"/> 정보입니다.</param>
        public abstract void OnDisconnected(EndPoint? endPoint);

        /// <summary>
        /// 데이터를 수신했을 때 호출됩니다.
        /// </summary>
        /// <param name="buffer">수신된 데이터를 담고 있는 <see cref="ArraySegment{T}"/>입니다.</param>
        /// <returns>처리한 바이트 수를 반환합니다.</returns>
        public abstract int OnRecv(ArraySegment<byte> buffer);

        /// <summary>
        /// 데이터를 성공적으로 송신했을 때 호출됩니다.
        /// </summary>
        /// <param name="sendBytes">송신한 바이트 수입니다.</param>
        public abstract void OnSend(int sendBytes);

        #endregion Content Overloading

        /// <summary>
        /// 데이터를 리스트 형태로 Send 큐에 등록합니다.
        /// 송신이 진행 중이지 않은 경우,
        /// <see cref="RegisterSend"/> 함수를 호출하여 송신을 시작합니다.
        /// </summary>
        /// <param name="dataList">송신할 데이터의 리스트입니다.</param>
        public void Send(List<ArraySegment<byte>> dataList)
        {
            if (dataList.Count == 0)
                return;

            bool sendFlag = false;
            lock (_lockObj)
            {
                foreach (var data in dataList)
                    _sendQueue.Enqueue(data);

                if (_sendFlag == true)
                    sendFlag = true;

                _sendFlag = false;
            }

            if (sendFlag)
                RegisterSend();
        }

        /// <summary>
        /// 단일 데이터를 송신 큐에 등록합니다.
        /// 송신이 진행 중이지 않은 경우,
        /// <see cref="RegisterSend"/> 함수를 호출하여 송신을 시작합니다.
        /// </summary>
        /// <param name="data">송신할 데이터입니다.</param>
        public void Send(ArraySegment<byte> data)
        {
            if (data.Count == 0)
            {
                GlobalLogger.WriteLog?.Invoke("Send Data Size Zero");
                return;
            }

            bool sendFlag = false;
            lock (_lockObj)
            {
                _sendQueue.Enqueue(data);
                if (_sendFlag == true)
                    sendFlag = true;

                _sendFlag = false;
            }

            if (sendFlag)
                RegisterSend();
        }

        /// <summary>
        /// 소켓을 등록하고 수신 및 송신 이벤트 핸들러를 설정한 후,
        /// <see cref="RegisterRecv"/> 함수를 호출하여 수신을 등록합니다.
        /// </summary>
        /// <param name="socket">통신에 사용할 <see cref="Socket"/>입니다.</param>
        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += OnCompletedRecv;
            _sendArgs.Completed += OnCompletedSend;

            RegisterRecv();
        }

        /// <summary>
        /// 연결을 종료하고 OnDisconnected 콜백 함수를 호출한 후 소켓을 닫습니다.
        /// 중복 호출을 방지하기 위해 Interlocked.Exchange를 사용합니다.
        /// </summary>
        protected void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnect, 1) == 0)
            {
                OnDisconnected(_recvArgs.RemoteEndPoint);
                _socket.Close();
            }
        }

        /// <summary>
        /// 수신 작업 완료 시 호출되는 콜백 함수입니다.
        /// 수신 데이터를 처리하고, 예외 발생 시 연결을 종료합니다.
        /// </summary>
        /// <param name="sender">이벤트 발신자입니다.</param>
        /// <param name="args">수신에 사용된 <see cref="SocketAsyncEventArgs"/>입니다.</param>
        private void OnCompletedRecv(object? sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                        throw new InvalidOperationException("RecvBuffer OnWrite Error");

                    int processLen = OnRecv(_recvBuffer.ReadSegment);

                    if (_recvBuffer.OnRead(processLen) == false)
                        throw new InvalidOperationException("RecvBuffer OnRead Error");

                    RegisterRecv();
                }
                catch (Exception e) when (true)
                {
                    GlobalLogger.WriteLog?.Invoke($"OnCompletedRecv Exception: {e.Message}");
                    Disconnect();
                }
            }
            else
            {
                Disconnect();
            }
        }

        /// <summary>
        /// 송신 작업 완료 시 호출되는 콜백 함수입니다.
        /// 송신 완료 후 대기 중인 데이터가 있으면 RegisterSend를 재호출합니다.
        /// </summary>
        /// <param name="sender">이벤트 발신자입니다.</param>
        /// <param name="args">송신에 사용된 <see cref="SocketAsyncEventArgs"/>입니다.</param>
        private void OnCompletedSend(object? sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                OnSend(args.BytesTransferred);

                bool sendFlag = false;
                lock (_lockObj)
                {
                    if (_sendQueue.Count > 0)
                        sendFlag = true;
                    else
                        _sendFlag = true;
                }

                if (sendFlag)
                    RegisterSend();
            }
            else
            {
                Disconnect();
            }
        }

        /// <summary>
        /// 비동기 수신을 등록합니다.
        /// 수신 요청이 즉시 완료될 경우 OnCompletedRecv를 직접 호출합니다.
        /// </summary>
        private void RegisterRecv()
        {
            _recvArgs.SetBuffer(_recvBuffer.WriteSegment);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (!pending)
                    OnCompletedRecv(null, _recvArgs);
            }
            catch (Exception e)
            {
                GlobalLogger.WriteLog?.Invoke($"RegisterRecv Exception: {e.Message}");
                Disconnect();
            }
        }

        /// <summary>
        /// Send 큐에서 데이터를 꺼내 BufferList로 설정한 뒤 비동기 송신을 등록합니다.
        /// 송신 요청이 즉시 완료될 경우 OnCompletedSend를 직접 호출합니다.
        /// </summary>
        private void RegisterSend()
        {
            List<ArraySegment<byte>> sendBufferList = new List<ArraySegment<byte>>();
            lock (_lockObj)
            {
                while (_sendQueue.Count > 0)
                {
                    // Performance Log
                    ArraySegment<byte> _sendBuffer = _sendQueue.Dequeue();
                    PerformanceProfiler.Instance.prevSendCount += _sendBuffer.Count;
                    sendBufferList.Add(_sendBuffer);
                }
            }

            _sendArgs.BufferList = sendBufferList;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (!pending)
                    OnCompletedSend(null, _sendArgs);
            }
            catch (Exception e)
            {
                GlobalLogger.WriteLog?.Invoke($"RegisterSend Exception: {e.Message}");
                Disconnect();
            }
        }
    }
}
