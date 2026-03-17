using System;
using System.Collections.Generic;
using System.Text;

namespace SBSocketPacketLib
{
    public class SBTcpRecvBuffer
    {
        // Read/Write Position 변화 예. 10byte array
        // [rw][][][][][][][][][] // init 상태
        // [r][][][][][w][][][][] // 클라로부터 5byte를 받았다.
        // 분기 1) 실제 패킷 크기가 5byte일 때
        // [][][][][][rw][][][][] // 콘텐츠 로직에서 메시지를 읽어서 처리하게 됨.
        // 분기 2) 실제 패킷 크기가 8byte일 때
        // [r][][][][][w][][][][] // 현 상태에서 다음 패킷(3byte)을 기다림.
        // [r][][][][][][][][w][] // 추가 3byte를 받음.
        // [][][][][][][][][rw][] // 콘텐츠 로직에서 메시지를 읽어서 처리하게 됨.
        // [rw][][][][][][][][][] // 중간 중간 정리가 되면서 position이 앞으로 당겨짐.
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public SBTcpRecvBuffer(int bufferSize_ = 4096)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize_], 0, bufferSize_);
        }

        public int DataSize { get { return _writePos - _readPos; } }
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        /// <summary>
        /// 현재까지 받은 데이터의 유효범위가 어디부터 어디까지인가.
        /// </summary>
        public ArraySegment<byte> ReadSegment // or DataSegment
        {
#if true
            // ArraySegment(배열, 시작위치, 크기)
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
#else
            get
            {
                SBCommonLib.SBLog.PrintTrace($"[ReadSegment] offset: {_buffer.Offset}, readPos: {_readPos}, dataSize: {DataSize}");
                return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize);
            }
#endif
        }

        /// <summary>
        /// Receive를 할 때 어디부터 어디까지가 유효범위인가.
        /// </summary>
        public ArraySegment<byte> WriteSegment // or RecvSegment
        {
#if true
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
#else
            // for Debug
            get
            {
                SBCommonLib.SBLog.PrintTrace($"[WriteSegment] offset: {_buffer.Offset}, writePos: {_writePos}, freeSize: {FreeSize}");
                return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize);
            }
#endif
        }

        public void Clean()
        {
            int dataSize = DataSize;
            // 남은 데이터가 없으면 복사하지 않고 위치만 리셋
            if (0 == dataSize)
                _readPos = _writePos = 0;
            else
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes_)
        {
            //SBCommonLib.SBLog.PrintTrace($"[OnRead] readBytes: {numOfBytes_}");
            if (numOfBytes_ > DataSize)
                return false;

            _readPos += numOfBytes_;
            return true;
        }

        public bool OnWrite(int numOfBytes_)
        {
            //SBCommonLib.SBLog.PrintTrace($"[OnWrite] writeBytes: {numOfBytes_}");
            if (numOfBytes_ > FreeSize)
                return false;

            _writePos += numOfBytes_;
            return true;
        }
    }
}
