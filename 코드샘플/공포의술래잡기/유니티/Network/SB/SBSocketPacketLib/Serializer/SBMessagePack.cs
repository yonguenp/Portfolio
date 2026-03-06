using System;

using MessagePack;

using SBCommonLib;

namespace SBSocketPacketLib.Serializer
{
    /// <summary>
    /// MessagePack을 사용한 TCP 패킷 Serialize 클래스
    /// </summary>
    public class SBMessagePack
    {
        /// <summary>
        /// Serialize
        /// </summary>
        /// <typeparam name="TZFObject">객체 타입</typeparam>
        /// <param name="key_">Key</param>
        /// <param name="seed_">Seed</param>
        /// <param name="object_">TCP 패킷</param>
        /// <param name="isCrypt_">암호화 여부</param>
        /// <returns></returns>
        public static string Serialize<TZFObject>(byte[] key_, byte[] seed_, TZFObject object_, bool isCrypt_ = true)
        {
            try
            {
                var buffer = MessagePackSerializer.Serialize(object_);

                if (isCrypt_)
                {
                    return SBCrypt.EncryptBytesToString(buffer, key_, seed_);
                }
                else
                {
                    return Convert.ToBase64String(buffer);
                }
            }
            catch (Exception e_)
            {
                SBLog.PrintError($"Serialize - Error", e_, typeof(SBMessagePack).Name);
                return string.Empty;
            }
        }

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <typeparam name="T">변환할 객체 타입</typeparam>
        /// <param name="key_">Key</param>
        /// <param name="seed_">Seed</param>
        /// <param name="text_">Text</param>
        /// <param name="isCrypt_">암호화 여부</param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] key_, byte[] seed_, string text_, bool isCrypt_ = true)
        {
            try
            {
                if (isCrypt_)
                {
                    return MessagePackSerializer.Deserialize<T>(SBCrypt.DecryptBytesFromString(text_, key_, seed_));
                }
                else
                {
                    return MessagePackSerializer.Deserialize<T>(Convert.FromBase64String(text_));
                }
            }
            catch (Exception e_)
            {
                SBLog.PrintError($"Deserialize - Error", e_, typeof(SBMessagePack).Name);
                return default(T);
            }
        }
    }
}
