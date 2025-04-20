using System;

namespace ServerCore
{
    internal class RecvBuffer
    {
        private byte[] _buffer;
        private int _readPos;
        private int _writePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new byte[bufferSize];
            _readPos = _writePos = 0;
        }

        public int DataSize => _writePos - _readPos;

        public int FreeSize => _buffer.Length - _writePos;

        public ArraySegment<byte> ReadSegment => new ArraySegment<byte>(_buffer, _readPos, DataSize);
        public ArraySegment<byte> WriteSegment => new ArraySegment<byte>(_buffer, _writePos, FreeSize);

        public bool OnRead(int readSize)
        {
            if (readSize > DataSize)
                return false;

            _readPos += readSize;
            Clean();
            return true;
        }

        public bool OnWrite(int writeSize)
        {
            if (writeSize > FreeSize)
                return false;

            _writePos += writeSize;
            return true;
        }

        private void Clean()
        {
            if (_readPos == _writePos)
            {
                _readPos = _writePos = 0;
            }
            else
            {
                Array.Copy(_buffer, _readPos, _buffer, 0, DataSize);
                _writePos = DataSize;
                _readPos = 0;
            }
        }
    }
}
