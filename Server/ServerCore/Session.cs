using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public abstract void OnPacketRecv(ArraySegment<byte> buffer);

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
                Interlocked.Increment(ref PerformanceProfiler.Instance.prevRecvCount);

                OnPacketRecv(buffer.Slice(processLen, size));
                processLen += size;
            }

            return processLen;
        }
    }

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

        public abstract void OnConnected(EndPoint? endPoint);

        public abstract void OnDisconnected(EndPoint? endPoint);

        public abstract int OnRecv(ArraySegment<byte> buffer);

        public abstract void OnSend(int sendBytes);

        #endregion Content Overloading

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

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += OnCompletedRecv;
            _sendArgs.Completed += OnCompletedSend;

            RegisterRecv();
        }

        protected void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnect, 1) == 0)
            {
                OnDisconnected(_recvArgs.RemoteEndPoint);
                _socket.Close();
            }
        }

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

        private void RegisterSend()
        {
            List<ArraySegment<byte>> sendBufferList = new List<ArraySegment<byte>>();
            lock (_lockObj)
            {
                PerformanceProfiler.Instance.prevSendCount += _sendQueue.Count;
                while (_sendQueue.Count > 0)
                    sendBufferList.Add(_sendQueue.Dequeue());
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
