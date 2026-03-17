using System;
using System.IO;
using System.Security.Cryptography;

/// <summary>
/// SandBox Common Library
/// </summary>
namespace SBCommonLib
{
    /// <summary>
    /// 암호화/복호화 클래스
    /// </summary>
    public class SBCrypt
    {
        /// <summary>
        /// Padding 모드 (기본값(PKCS7)을 사용하지만 추후 변경할 가능성이 있기 때문에 const 값 만들어 둠)
        /// https://docs.microsoft.com/ko-kr/dotnet/api/system.security.cryptography.paddingmode?view=netcore-3.1 참고
        /// None    (1) : 패딩 없이 이루어집니다.
        /// PKCS7   (2) : PKCS #7 패딩 문자열을 바이트 시퀀스로, 추가 된 패딩 바이트의 총 수는 각각이 같은지 이루어져 있습니다.
        /// Zeros   (3) : 안쪽 여백 문자열을 0으로 설정 된 바이트로 이루어져 있습니다.
        /// ANSIX923(4) : ANSIX923 패딩 문자열 길이 0으로 채워진 바이트 시퀀스로 구성 됩니다.
        /// ISO10126(5) : ISO10126 패딩 문자열 길이 임의의 데이터로 이루어져 있습니다.
        /// </summary>
        private const PaddingMode kPaddingMode = PaddingMode.PKCS7;

        /// <summary>
        /// Cipher 모드 (기본값(CBC)을 사용하지만 추후 변경할 가능성이 있기 때문에 const 값 만들어 둠)
        /// 자세한 설명은 https://docs.microsoft.com/ko-kr/dotnet/api/system.security.cryptography.ciphermode?view=netcore-3.1 참고
        /// CipherMode.ECB 는 약한 암호 스트림을 생성하므로 실무에서는 절대 사용하면 안된다고 함.
        /// </summary>
        private const CipherMode kCipherMode = CipherMode.CBC;

        /// <summary>
        /// 키 길이 (MinimumSize : 128bit, MaximumSize : 256bit)
        /// AES 표준인 AES128(128bit == 16byte), AES192(192bit == 24byte), AES256(256bit == 32byte) 중 골라서 사용. (기본값 256bit)
        /// 키 길이가 길어지면 무작위 대입 공격에도 유리하고 암호화에 진행되는 Round도 많아져서 암호화 성능은 좋아지지만 컴퓨팅 파워가 많이 필요함.
        /// 하지만 요즘은 서버 스펙이 좋아져서 256bit를 사용해도 큰 무리가 없어 AES256이 권장되고 있음.
        /// </summary>
        public const int kCryptKeyLen = 32;

        /// <summary>
        /// 초기화 벡터 길이
        /// 초기화 벡터의 길이는 블럭의 크기(AES 표준 Min/MaxSize : 128bit)와 같은 값을 사용해야함. 
        /// </summary>
        public const int kCryptIvLen = 16;

        /// <summary>
        /// 텍스트 암호화 (string -> string)
        /// </summary>
        /// <param name="plainText_">평문 텍스트</param>
        /// <param name="key_">키</param>
        /// <param name="iv_">초기화 벡터</param>
        /// <returns>암호화된 텍스트</returns>
        public static string EncryptStringToString(string plainText_, byte[] key_, byte[] iv_)
        {
            try
            {
                if (null == plainText_ || 0 >= plainText_.Length)
                    throw new ArgumentNullException("PlainText");

                if (null == key_ || 0 >= key_.Length)
                    throw new ArgumentNullException("Key");

                if (null == iv_ || 0 >= iv_.Length)
                    throw new ArgumentNullException("IV");

                using (Rijndael rijAlg = Rijndael.Create()) // RijndaelManaged class를 사용해도 됨. ex: [ using (RijndaelManaged rijAlg = new RijndaelManaged()) ]
                {
                    rijAlg.Key = key_;
                    rijAlg.IV = iv_;
                    rijAlg.Padding = kPaddingMode;
                    rijAlg.Mode = kCipherMode;

                    ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText_);
                            }

                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            catch (Exception e_)
            {
                SBLog.PrintError($"EncryptStringToString - Error(Message: {e_.Message})", calledBy_: typeof(SBCrypt).Name);
                return string.Empty;
            }
        }

        /// <summary>
        /// 텍스트 암호화 (string -> byte[])
        /// </summary>
        /// <param name="plainText_">평문 텍스트</param>
        /// <param name="key_">키</param>
        /// <param name="iv_">초기화 벡터</param>
        /// <returns>암호화된 데이터</returns>
        public static byte[] EncryptStringToBytes(string plainText_, byte[] key_, byte[] iv_)
        {
            try
            {
                if (null == plainText_ || 0 >= plainText_.Length)
                    throw new ArgumentNullException("PlainText");

                if (null == key_ || 0 >= key_.Length)
                    throw new ArgumentNullException("Key");

                if (null == iv_ || 0 >= iv_.Length)
                    throw new ArgumentNullException("IV");

                using (Rijndael rijAlg = Rijndael.Create())
                {
                    rijAlg.Key = key_;
                    rijAlg.IV = iv_;
                    rijAlg.Padding = kPaddingMode;
                    rijAlg.Mode = kCipherMode;

                    ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText_);
                            }

                            return msEncrypt.ToArray();
                        }
                    }
                }
            }
            catch (Exception e_)
            {
                SBLog.PrintError($"EncryptStringToBytes - Error(Message: {e_.Message})", calledBy_: typeof(SBCrypt).Name);
                return null;
            }
        }

        /// <summary>
        /// 데이터 암호화 (byte[] -> string)
        /// </summary>
        /// <param name="plainData_">원본 데이터</param>
        /// <param name="key_">키</param>
        /// <param name="iv_">초기화 벡터</param>
        /// <returns>암호화된 텍스트</returns>
        public static string EncryptBytesToString(byte[] plainData_, byte[] key_, byte[] iv_)
        {
            try
            {
                if (null == plainData_ || 0 >= plainData_.Length)
                    throw new ArgumentNullException("Data");

                if (null == key_ || 0 >= key_.Length)
                    throw new ArgumentNullException("Key");

                if (null == iv_ || 0 >= iv_.Length)
                    throw new ArgumentNullException("IV");

                using (Rijndael rijAlg = Rijndael.Create())
                {
                    rijAlg.Key = key_;
                    rijAlg.IV = iv_;
                    rijAlg.Padding = kPaddingMode;
                    rijAlg.Mode = kCipherMode;

                    ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(plainData_, 0, plainData_.Length);
                            csEncrypt.FlushFinalBlock();

                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            catch (Exception e_)
            {
                SBLog.PrintError($"EncryptBytesToString - Error(Message: {e_.Message})", calledBy_: typeof(SBCrypt).Name);
                return string.Empty;
            }
        }

        /// <summary>
        /// 텍스트 복호화 (string -> string)
        /// </summary>
        /// <param name="cipherText_">암호화된 텍스트</param>
        /// <param name="key_">키</param>
        /// <param name="iv_">초기화 벡터</param>
        /// <returns>복호화된 텍스트</returns>
        public static string DecryptStringFromString(string cipherText_, byte[] key_, byte[] iv_)
        {
            try
            {
                if (null == cipherText_ || 0 >= cipherText_.Length)
                    throw new ArgumentNullException("CipherText");

                if (null == key_ || 0 >= key_.Length)
                    throw new ArgumentNullException("Key");

                if (null == iv_ || 0 >= iv_.Length)
                    throw new ArgumentNullException("IV");

                using (Rijndael rijAlg = Rijndael.Create())
                {
                    rijAlg.Key = key_;
                    rijAlg.IV = iv_;
                    rijAlg.Padding = kPaddingMode;
                    rijAlg.Mode = kCipherMode;

                    ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                    byte[] cipherData = Convert.FromBase64String(cipherText_);
                    using (MemoryStream msDecrypt = new MemoryStream(cipherData))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception e_)
            {
                SBLog.PrintError($"DecryptStringToString - Error(Message: {e_.Message})", calledBy_: typeof(SBCrypt).Name);
                return string.Empty;
            }
        }

        /// <summary>
        /// 데이터 복호화 (byte[] -> string)
        /// </summary>
        /// <param name="cipherData_">암호화된 데이터</param>
        /// <param name="key_">키</param>
        /// <param name="iv_">초기화 벡터</param>
        /// <returns>복호화된 텍스트</returns>
        public static string DecryptStringFromBytes(byte[] cipherData_, byte[] key_, byte[] iv_)
        {
            try
            {
                if (null == cipherData_ || 0 >= cipherData_.Length)
                    throw new ArgumentNullException("CipherText");

                if (null == key_ || 0 >= key_.Length)
                    throw new ArgumentNullException("Key");

                if (null == iv_ || 0 >= iv_.Length)
                    throw new ArgumentNullException("IV");

                using (Rijndael rijAlg = Rijndael.Create())
                {
                    rijAlg.Key = key_;
                    rijAlg.IV = iv_;
                    rijAlg.Padding = kPaddingMode;
                    rijAlg.Mode = kCipherMode;

                    ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(cipherData_))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception e_)
            {
                SBLog.PrintError($"DecryptStringFromBytes - Error(Message: {e_.Message})", calledBy_: typeof(SBCrypt).Name);
                return string.Empty;
            }
        }

        /// <summary>
        /// 데이터 복호화 (string -> byte[])
        /// </summary>
        /// <param name="cipherText_">암호화된 텍스트</param>
        /// <param name="key_">키</param>
        /// <param name="iv_">초기화 벡터</param>
        /// <returns>복호화된 데이터</returns>
        public static byte[] DecryptBytesFromString(string cipherText_, byte[] key_, byte[] iv_)
        {
            try
            {
                if (null == cipherText_ || 0 >= cipherText_.Length)
                    throw new ArgumentNullException("CipherText");

                if (null == key_ || 0 >= key_.Length)
                    throw new ArgumentNullException("Key");

                if (null == iv_ || 0 >= iv_.Length)
                    throw new ArgumentNullException("IV");

                using (Rijndael rijAlg = Rijndael.Create())
                {
                    rijAlg.Key = key_;
                    rijAlg.IV = iv_;
                    rijAlg.Padding = kPaddingMode;
                    rijAlg.Mode = kCipherMode;

                    ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                    byte[] cipherData = Convert.FromBase64String(cipherText_);
                    using (MemoryStream msDecrypt = new MemoryStream(cipherData))
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            csEncrypt.Read(cipherData, 0, cipherData.Length);

                            return msDecrypt.ToArray();
                        }
                    }
                }
            }
            catch (Exception e_)
            {
                //SBDebug.Log($"DecryptBytesFromString - Error(Message: {e_.Message}");
                SBLog.PrintError($"DecryptBytesFromString - Error(Message: {e_.Message})", calledBy_: typeof(SBCrypt).Name);
                return null;
            }
        }
    }
}
