using MessagePack; //https://github.com/neuecc/MessagePack-CSharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBPacketLib
{

    public class PacketDef
    {
        public const Int16 PACKET_HEADER_SIZE = 5;
    }

    public class PacketToBytes
    {
        public static byte[] Make<T>(ChatServer_PacketID packetID, T packet_)
        {

            var bodyData = MessagePackSerializer.Serialize(packet_);

            byte type = 0;
            var pktID = (Int16)packetID;
            Int16 bodyDataSize = 0;
            if (bodyData != null)
            {
                bodyDataSize = (Int16)bodyData.Length;
            }
            var packetSize = (Int16)(bodyDataSize + PacketDef.PACKET_HEADER_SIZE);

            var dataSource = new byte[packetSize];
            Buffer.BlockCopy(BitConverter.GetBytes(packetSize), 0, dataSource, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(pktID), 0, dataSource, 2, 2);
            dataSource[4] = type;

            if (bodyData != null)
            {
                Buffer.BlockCopy(bodyData, 0, dataSource, 5, bodyDataSize);
            }

            return dataSource;
        }

        public static Tuple<int, byte[]> ClientReceiveData(int recvLength, byte[] recvData)
        {
            var packetSize = BitConverter.ToInt16(recvData, 0);
            var packetID = BitConverter.ToInt16(recvData, 2);
            var bodySize = packetSize - PacketDef.PACKET_HEADER_SIZE;

            var packetBody = new byte[bodySize];
            Buffer.BlockCopy(recvData, PacketDef.PACKET_HEADER_SIZE, packetBody, 0, bodySize);

            return new Tuple<int, byte[]>(packetID, packetBody);
        }

    }

}
