using System;
using System.Threading;

namespace ServerCore
{
    public class SendBufferHelper
    {
        /// <summary>
        /// 각 스레드마다 독립적으로 관리되는 송신 버퍼입니다.
        /// 송신 시, 버퍼 할당 비용을 줄이고 메모리 재사용을 통해 성능을 높이기 위해 사용됩니다.
        /// </summary>
        private static ThreadLocal<SendBuffer> _sendBuffer = new ThreadLocal<SendBuffer>(() => new SendBuffer(ChunkSize));

        private static int ChunkSize => 65535;

        /// <summary>
        /// Open으로 확보한 송신 버퍼 중 실제로 사용한 크기를 기반으로 송신 구간을 반환합니다.
        /// </summary>
        /// <param name="usedSize">실제로 송신에 사용할 바이트 수입니다.</param>
        /// <returns>송신할 데이터의 ArraySegment입니다.</returns>
        public static ArraySegment<byte> Close(int usedSize) => _sendBuffer.Value!.Close(usedSize);

        /// <summary>
        /// 송신을 위해 지정한 크기의 버퍼 공간을 확보하고 그 구간을 반환합니다.
        /// 내부적으로 버퍼가 없거나 공간이 부족할 경우 새로 생성합니다.
        /// </summary>
        /// <param name="reserveSize">필요한 버퍼 크기입니다.</param>
        /// <returns>예약된 송신 버퍼 구간입니다.</returns>
        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (_sendBuffer.Value == null)
                _sendBuffer.Value = new SendBuffer(ChunkSize);

            if (reserveSize > _sendBuffer.Value.FreeSize)
                _sendBuffer.Value = new SendBuffer(ChunkSize);

            return _sendBuffer.Value.Open(reserveSize);
        }

        /// <summary>
        /// 실제 송신 버퍼를 관리하는 내부 클래스입니다.
        /// </summary>
        private class SendBuffer
        {
            private byte[] _buffer;
            private int _writePos;

            /// <summary>
            /// 지정한 크기로 송신 버퍼를 초기화합니다.
            /// </summary>
            /// <param name="bufferSize">버퍼의 총 크기입니다. 기본값은 4096입니다.</param>
            public SendBuffer(int bufferSize = 4096)
            {
                _buffer = new byte[bufferSize];
                _writePos = 0;
            }

            /// <summary>
            /// 현재 버퍼에서 남은 여유 공간 크기를 반환합니다.
            /// </summary>
            public int FreeSize => _buffer.Length - _writePos;

            /// <summary>
            /// 예약된 버퍼 공간 중 실제로 송신할 데이터 구간을 반환합니다.
            /// </summary>
            /// <param name="usedSize">실제로 송신에 사용할 바이트 수입니다.</param>
            /// <returns>송신할 데이터의 ArraySegment입니다. 사용량이 유효하지 않으면 기본값 반환.</returns>
            public ArraySegment<byte> Close(int usedSize)
            {
                if (usedSize > FreeSize)
                    return default;

                var segment = new ArraySegment<byte>(_buffer, _writePos, usedSize);
                _writePos += usedSize;
                return segment;
            }

            /// <summary>
            /// 송신을 위해 지정한 크기의 버퍼 공간을 반환합니다.
            /// </summary>
            /// <param name="reserveSize">예약할 버퍼 크기입니다.</param>
            /// <returns>송신을 위한 버퍼 구간입니다.</returns>
            public ArraySegment<byte> Open(int reserveSize) => new ArraySegment<byte>(_buffer, _writePos, reserveSize);
        }
    }
}
