using System;

namespace ServerCore
{
    internal class RecvBuffer
    {
        private byte[] _buffer;
        private int _readPos;
        private int _writePos;

        /// <summary>
        /// 지정한 크기의 수신 버퍼를 초기화합니다.
        /// </summary>
        /// <param name="bufferSize">버퍼의 총 크기입니다.</param>
        public RecvBuffer(int bufferSize)
        {
            _buffer = new byte[bufferSize];
            _readPos = _writePos = 0;
        }

        /// <summary>
        /// 현재 버퍼에 저장된 데이터의 크기를 반환합니다.
        /// </summary>
        public int DataSize => _writePos - _readPos;

        /// <summary>
        /// 버퍼에 남은 여유 공간 크기를 반환합니다.
        /// </summary>
        public int FreeSize => _buffer.Length - _writePos;

        /// <summary>
        /// 현재 읽을 수 있는 데이터 구간을 반환합니다.
        /// </summary>
        public ArraySegment<byte> ReadSegment => new ArraySegment<byte>(_buffer, _readPos, DataSize);

        /// <summary>
        /// 데이터를 수신할 수 있는 버퍼의 쓰기 구간을 반환합니다.
        /// </summary>
        public ArraySegment<byte> WriteSegment => new ArraySegment<byte>(_buffer, _writePos, FreeSize);

        /// <summary>
        /// 인자로 들어온 크기만큼 데이터를 읽었음을 버퍼에 반영합니다.
        /// </summary>
        /// <param name="readSize">읽은 데이터 크기입니다.</param>
        /// <returns>읽기 성공 여부를 반환합니다.</returns>
        public bool OnRead(int readSize)
        {
            if (readSize > DataSize)
                return false;

            _readPos += readSize;
            Clean();
            return true;
        }

        /// <summary>
        /// 인자로 들어온 크기만큼 데이터를 수신했음을 버퍼에 반영합니다.
        /// </summary>
        /// <param name="writeSize">수신된 데이터 크기입니다.</param>
        /// <returns>쓰기 성공 여부를 반환합니다.</returns>
        public bool OnWrite(int writeSize)
        {
            if (writeSize > FreeSize)
                return false;

            _writePos += writeSize;
            return true;
        }

        /// <summary>
        /// 모든 데이터를 처리했다면 위치만 변경하고, 아직 데이터가 남았다면 버퍼의 데이터를 앞으로 복사합니다.
        /// </summary>
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
