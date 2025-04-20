using System;
using System.Threading;

namespace ServerCore
{
    public class SendBufferHelper
    {
        private static ThreadLocal<SendBuffer> _sendBuffer = new ThreadLocal<SendBuffer>(() => new SendBuffer(ChunkSize));

        private static int ChunkSize => 65535;

        public static ArraySegment<byte> Close(int usedSize) => _sendBuffer.Value!.Close(usedSize);

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (_sendBuffer.Value == null)
                _sendBuffer.Value = new SendBuffer(ChunkSize);

            if (reserveSize > _sendBuffer.Value.FreeSize)
                _sendBuffer.Value = new SendBuffer(ChunkSize);

            return _sendBuffer.Value.Open(reserveSize);
        }

        private class SendBuffer
        {
            private byte[] _buffer;
            private int _writePos;

            public SendBuffer(int bufferSize = 4096)
            {
                _buffer = new byte[bufferSize];
                _writePos = 0;
            }

            public int FreeSize => _buffer.Length - _writePos;

            public ArraySegment<byte> Close(int usedSize)
            {
                if (usedSize > FreeSize)
                    return default;

                var segment = new ArraySegment<byte>(_buffer, _writePos, usedSize);
                _writePos += usedSize;
                return segment;
            }

            public ArraySegment<byte> Open(int reserveSize) => new ArraySegment<byte>(_buffer, _writePos, reserveSize);
        }
    }
}
