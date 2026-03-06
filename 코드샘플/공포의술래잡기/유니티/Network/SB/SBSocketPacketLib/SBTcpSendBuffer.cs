using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using SBCommonLib;

namespace SBSocketPacketLib
{
    public class SBSendBufferHelper
    {
        public static ThreadLocal<SBTcpSendBuffer> CurrentBuffer = new ThreadLocal<SBTcpSendBuffer>(() => { return null; });
        public static int SendBufferSize { get; set; } = 4096;
        public static int SendBufferCount { get; set; } = 100;
        public static int ChunkSize { get; private set; } = SendBufferSize * SendBufferCount;
        public static int ChunkCount { get; private set; } = 10;
        public static int MaxChunkSize { get; private set; } = ChunkSize * ChunkCount;

        public static ArraySegment<byte> Open(int reserveSize_)
        {
            if (null == CurrentBuffer.Value)
                CurrentBuffer.Value = new SBTcpSendBuffer(ChunkSize);

            if (CurrentBuffer.Value.FreeSize < reserveSize_)
                CurrentBuffer.Value = new SBTcpSendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize_);
        }

        public static ArraySegment<byte> Close(int usedSize_)
        {
            return CurrentBuffer.Value.Close(usedSize_);
        }
    }

    public class SBTcpSendBuffer
    {
        // [u][][][][][][][][][]
        byte[] _buffer;
        int _usedSize = 0;

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public SBTcpSendBuffer(int chunkSize_)
        {
            _buffer = new byte[chunkSize_];
        }

        public ArraySegment<byte> Open(int reserveSize_)
        {
            //if (reserveSize_ > FreeSize)
            //    return null;

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize_);
        }

        public ArraySegment<byte> Close(int usedSize_)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize_);
            _usedSize += usedSize_;
            return segment;
        }
    }
}
