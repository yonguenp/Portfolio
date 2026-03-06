using System;

using SBCommonLib;

/// <summary>
/// SandBox Socket Packet Library
/// </summary>
namespace SBSocketPacketLib
{
    public interface ISBPacket
    {
        /// <summary>
        /// 패킷 Id
        /// </summary>
        ushort PacketId { get; set; }

        /// <summary>
        /// 세션 Guid
        /// </summary>
        byte[] SessionId { get; set; }

        /// <summary>
        /// 패킷 데이터
        /// </summary>
        string Data { get; set; }
    }

    /// <summary>
    /// 패킷 클래스
    /// </summary>
    public class SBPacket : ISBPacket
    {
        /// <summary>
        /// 패킷 길이 크기
        /// </summary>
        public static readonly int kPacketLengthSize = sizeof(short);

        /// <summary>
        /// 패킷 Id 사이즈
        /// </summary>
        public static readonly int kPacketIdSize = sizeof(short);

        /// <summary>
        /// 세션 Guid 사이즈
        /// byte[]의 경우 16, string의 경우 32임.
        /// </summary>
        public static readonly int kSessionIdSize = 16;

        public static readonly int kHeaderSize = kPacketLengthSize;

        /// <summary>
        /// 패킷 사이즈
        /// </summary>
        public int Length
        {
            get => (kPacketLengthSize + kPacketIdSize + kSessionIdSize + DataLength);
        }

        public int BodyLength
        {
            get => (kPacketIdSize + kSessionIdSize + DataLength);
        }

        /// <summary>
        /// 패킷 Id
        /// </summary>
        public ushort PacketId { get; set; }

        /// <summary>
        /// 세션 Guid
        /// </summary>
        public byte[] SessionId { get; set; } = Guid.Empty.ToByteArray();

        /// <summary>
        /// 패킷 데이터
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 데이터 사이즈
        /// </summary>
        public int DataLength
        {
            get => SBUtil.kUtf8Encoding.GetByteCount(Data);
        }

        /// <summary>
        /// 데이터 Byte Array
        /// </summary>
        //public ArraySegment<byte> DataBytes
        public byte[] DataBytes
        {
            get => SBUtil.kUtf8Encoding.GetBytes(Data);
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="SBPacket"/> class.
        /// </summary>
        public SBPacket()
        {
            PacketId = 0;
            Data = "";
            SessionId = Guid.Empty.ToByteArray();
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="SBPacket"/> class.
        /// </summary>
        /// <param name="packetId_">Packet Id</param>
        public SBPacket(ushort packetId_) : this()
        {
            PacketId = packetId_;
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="SBPacket"/> class.
        /// </summary>
        /// <param name="packetId_">Packet Id</param>
        /// <param name="data_">Data</param>
        public SBPacket(ushort packetId_, string data_) : this(packetId_)
        {
            Data = data_;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packetId_">Packet Id</param>
        /// <param name="data_">Data</param>
        /// <param name="sessionId_">Session Id</param>
        public SBPacket(ushort packetId_, string data_, byte[] sessionId_) : this(packetId_, data_)
        {
            SessionId = sessionId_;
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="SBPacket"/> class.
        /// </summary>
        /// <param name="segment_"></param>
#if true
        #region SocketAsyncEventArgs(SAEA) 용
        public SBPacket(ArraySegment<byte> segment_)
        {
            //SBDebug.Log($"[SBPacket] Array: {segment_.Array.Length}, Offset: {segment_.Offset}, Count: {segment_.Count}");
            //SBLog.PrintTrace($"[SBPacket] Array: {segment_.Array.Length}, Offset: {segment_.Offset}, Count: {segment_.Count}");
            int offset = 0;
            // skip PacketLength field
            //offset += kPacketLengthSize;
            // read packet id field
            PacketId = DecodeUInt16FromBytes(segment_.Array, segment_.Offset + offset);
            offset += kPacketIdSize;
            // read session id field
            Buffer.BlockCopy(segment_.Array, segment_.Offset + offset, SessionId, 0, kSessionIdSize);
            offset += kSessionIdSize;
            // read data
            Data = DecodeStringFromBytes(new ArraySegment<byte>(segment_.Array, segment_.Offset + offset, segment_.Count - offset));
        }
        #endregion
#else
        #region BeginReceive/BeginSend(BRBS) 용
        #endregion
#endif

        /// <summary>
        /// Initialize a new instance of the <see cref="SBPacket"/> class.
        /// 현재는 미사용.
        /// </summary>
        /// <param name="bytes_">Type</param>
#if false
        #region SocketAsyncEventArgs(SAEA) 용
        public SBPacket(byte[] bytes_)
        {
            int offset = 0;
            // read total packet length
            var length = DecodeUInt16FromBytes(bytes_, offset);
            offset = kPacketLengthSize;
            // read packet id
            PacketId = DecodeUInt16FromBytes(bytes_, offset);
            offset += kPacketIdSize;
            // read session id
            Buffer.BlockCopy(bytes_, offset, SessionId, 0, kSessionIdSize);
            offset += kSessionIdSize;
            // read data
            Data = DecodeStringFromBytes(bytes_, length, offset);
        }
        #endregion
#else
        #region BeginSend/BeginReceive(BSBR) 용
        public SBPacket(byte[] bytes_)
        {
            int offset = 0;
            // read packet id
            PacketId = DecodeUInt16FromBytes(bytes_, offset);
            offset += kPacketIdSize;
            // read session id
            Buffer.BlockCopy(bytes_, offset, SessionId, 0, kSessionIdSize);
            offset += kSessionIdSize;
            // read data
            Data = DecodeStringFromBytes(bytes_, offset);
        }
        #endregion
#endif

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <returns></returns>
#if true
        #region SocketAsyncEventArgs(SAEA) 용
        public ArraySegment<byte> GetBytes()
        {
            ArraySegment<byte> segment = SBSendBufferHelper.Open(Length);
            int count = 0;

            // write total packet size
            //EncodeInt16ToBytes(Length, ref packetBytes);
            Array.Copy(BitConverter.GetBytes(BodyLength), 0, segment.Array, segment.Offset + count, kPacketLengthSize);
            count += kPacketLengthSize;
            // write packet id
            //EncodeInt16ToBytes(PacketId, ref packetBytes, kPacketLengthSize);
            Array.Copy(BitConverter.GetBytes(PacketId), 0, segment.Array, segment.Offset + count, kPacketIdSize);
            count += kPacketIdSize;
            // write session id
            //Buffer.BlockCopy(SessionId, 0, packetBytes, kHeaderSize, SessionId.Length);
            Array.Copy(SessionId, 0, segment.Array, segment.Offset + count, kSessionIdSize);
            count += kSessionIdSize;
            // write data
            //Buffer.BlockCopy(DataBytes, 0, packetBytes, kHeaderSize + kSessionIdSize, DataBytes.Length);
            Array.Copy(DataBytes, 0, segment.Array, segment.Offset + count, DataBytes.Length);
            count += DataLength;

            return SBSendBufferHelper.Close(count);
        }
        #endregion
#else
        #region BeginReceive/BeginSend(BRBS) 용
        #endregion
#endif

        public ArraySegment<byte> GetBytesWithoutSizeField()
        {
            ArraySegment<byte> segment = SBSendBufferHelper.Open(BodyLength);
            int count = 0;

            // write total packet size
            //EncodeInt16ToBytes(Length, ref packetBytes);
            //Array.Copy(BitConverter.GetBytes(Length), 0, segment.Array, segment.Offset + count, kPacketLengthSize);
            //count += kPacketLengthSize;
            // write packet id
            //EncodeInt16ToBytes(PacketId, ref packetBytes, kPacketLengthSize);
            Array.Copy(BitConverter.GetBytes(PacketId), 0, segment.Array, segment.Offset + count, kPacketIdSize);
            count += kPacketIdSize;
            // write session id
            //Buffer.BlockCopy(SessionId, 0, packetBytes, kHeaderSize, SessionId.Length);
            Array.Copy(SessionId, 0, segment.Array, segment.Offset + count, kSessionIdSize);
            count += kSessionIdSize;
            // write data
            //Buffer.BlockCopy(DataBytes, 0, packetBytes, kHeaderSize + kSessionIdSize, DataBytes.Length);
            Array.Copy(DataBytes, 0, segment.Array, segment.Offset + count, DataBytes.Length);
            count += DataLength;

            return SBSendBufferHelper.Close(count);
        }

        #region static function
        /// <summary>
        /// Encodes the short to bytes. Little-Endian
        /// </summary>
        /// <param name="value_">Value</param>
        /// <param name="buffer_">Buffer</param>
        /// <param name="offset_">Offset</param>
        public static void EncodeInt16ToBytes(int value_, ref byte[] buffer_, int offset_ = 0)
        {
            buffer_[offset_] = BitConverter.GetBytes(value_)[0];
            buffer_[offset_ + 1] = BitConverter.GetBytes(value_)[1];
        }

        /// <summary>
        /// Encodes the int to bytes. Little-Endian
        /// </summary>
        /// <param name="value_">Value</param>
        /// <param name="buffer_">Buffer</param>
        /// <param name="offset_">Offset</param>
        public static void EncodeInt32ToBytes(int value_, ref byte[] buffer_, int offset_ = 0)
        {
            buffer_[offset_] = BitConverter.GetBytes(value_)[0];
            buffer_[offset_ + 1] = BitConverter.GetBytes(value_)[1];
            buffer_[offset_ + 2] = BitConverter.GetBytes(value_)[2];
            buffer_[offset_ + 3] = BitConverter.GetBytes(value_)[3];
        }

        /// <summary>
        /// Decodes the short from bytes.
        /// </summary>
        /// <param name="bytes_">Bytes</param>
        /// <param name="offset_">Offset</param>
        /// <returns></returns>
        public static short DecodeInt16FromBytes(byte[] bytes_, int offset_ = 0)
        {
            return BitConverter.ToInt16(bytes_, offset_);
        }

        /// <summary>
        /// Decodes the ushort from bytes.
        /// </summary>
        /// <param name="bytes_">Bytes</param>
        /// <param name="offset_">Offset</param>
        /// <returns></returns>
        public static ushort DecodeUInt16FromBytes(byte[] bytes_, int offset_ = 0)
        {
            return (ushort)DecodeInt16FromBytes(bytes_, offset_); //BitConverter.ToUInt16(bytes_, offset_);
        }

        /// <summary>
        /// Decodes the int from bytes.
        /// </summary>
        /// <param name="bytes_">Bytes</param>
        /// <param name="offset_">Offset</param>
        /// <returns></returns>
        public static int DecodeInt32FromBytes(byte[] bytes_, int offset_ = 0)
        {
            return BitConverter.ToInt32(bytes_, offset_);
        }

        /// <summary>
        /// Decodes the uint from bytes.
        /// </summary>
        /// <param name="bytes_">Bytes</param>
        /// <param name="offset_">Offset</param>
        /// <returns></returns>
        public static uint DecodeUInt32FromBytes(byte[] bytes_, int offset_ = 0)
        {
            return (uint)DecodeInt32FromBytes(bytes_, offset_); //BitConverter.ToUInt16(bytes_, offset_);
        }

        public static string DecodeStringFromBytes(ArraySegment<byte> segment_)
        {
            //SBLog.PrintTrace($"[DecodeStringFromBytes] Array: {segment_.Array.Length}, Offset: {segment_.Offset}, Count: {segment_.Count}");
            return SBUtil.kUtf8Encoding.GetString(segment_.Array, segment_.Offset, segment_.Count);
        }

        /// <summary>
        /// Decodes the string from bytes.
        /// </summary>
        /// <param name="bytes_">Bytes</param>
        /// <param name="offset_">Offset</param>
        /// <returns></returns>
        public static string DecodeStringFromBytes(byte[] bytes_, int offset_ = 0)
        {
            return SBUtil.kUtf8Encoding.GetString(bytes_, offset_, bytes_.Length - offset_);
        }

        /// <summary>
        /// Decodes the string from bytes.
        /// </summary>
        /// <param name="bytes_">Bytes</param>
        /// <param name="length_">Bytes</param>
        /// <param name="offset_">Offset</param>
        /// <returns></returns>
        public static string DecodeStringFromBytes(byte[] bytes_, int length_, int offset_ = 0)
        {
            return SBUtil.kUtf8Encoding.GetString(bytes_, offset_, length_ - offset_);
        }
        #endregion
    }
}
