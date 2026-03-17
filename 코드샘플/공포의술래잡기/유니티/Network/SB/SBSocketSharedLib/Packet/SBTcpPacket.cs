using System;
using SBSocketPacketLib;
using SBSocketPacketLib.Serializer;

namespace SBSocketSharedLib
{
    public class SBTcpPacket
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="key_"></param>
        /// <param name="seed_"></param>
        /// <param name="packetId_"></param>
        /// <param name="Object_"></param>
        /// <returns></returns>
        public static SBPacket MakePacket<TObject>(byte[] key_, byte[] seed_, PacketId packetId_, TObject Object_)
        {
            SBPacket packet = new SBPacket((ushort)packetId_);

            try
            {
                packet.Data = SBMessagePack.Serialize(key_, seed_, Object_);
            }
            catch
            {
                packet.Data = "";
                throw new Exception();
            }

            return packet;
        }
    }
}
