using SBCommonLib;

/// <summary>
/// SandBox Socket Client Library
/// </summary>
namespace SBSocketClientLib
{
    /// <summary>
    /// TCP 클라이언트 설정 클래스
    /// </summary>
    public class SBTcpClientConfig
    {
        /// <summary>
        /// 패킷 받을 버퍼의 size
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 4096 * 16; // SendBufferSize * SendBufferCount // 65k

        /// <summary>
        /// 패킷 보낼 버퍼의 size
        /// </summary>
        public int SendBufferSize { get; set; } = 4096; // 4k

        /// <summary>
        /// 세션당 보낼 수 있는 버퍼의 count
        /// </summary>
        public int SendBufferCount { get; set; } = 16;

        /// <summary>
        /// 패킷 사이즈 기본값
        /// </summary>
        public int DefaultPacketSize { get; set; } = 4096; // 4k

        /// <summary>
        /// 패킷 사이즈 최대값
        /// </summary>
        public int MaxPacketSize { get; set; } = 4096 * 16 * 16; // ReceiveBufferSize * 16 // 1M

        /// <summary>
        /// 세션 Update 간격시간 (ms)
        /// </summary>
        public int SessionUpdateIntervalTime { get; set; }

        /// <summary>
        /// 세션 Receive Timeout (ms)
        /// </summary>
        public int SessionReceiveTimeout { get; set; }

        /// <summary>
        /// Crypt Key
        /// </summary>
        public byte[] CryptKey { get; set; } = new byte[SBCrypt.kCryptKeyLen];

        /// <summary>
        /// Crypt IV
        /// </summary>
        public byte[] CryptIV { get; set; } = new byte[SBCrypt.kCryptIvLen];

        /// <summary>
        /// Connector Config
        /// </summary>
        public struct ConnectorConfig
        {
            /// <summary>
            /// Name
            /// </summary>
            public string Name;

            /// <summary>
            /// 연결 IP
            /// </summary>
            public string Ip;

            /// <summary>
            /// 연결 포트
            /// </summary>
            public int Port;

            /// <summary>
            /// Max 연결 시도 횟수
            /// </summary>
            public int MaxConnectionTryConut;

            public ConnectorConfig(string name_, string ip_, int port_, int maxCount_)
            {
                this.Name = name_;
                this.Ip = ip_;
                this.Port = port_;
                this.MaxConnectionTryConut = maxCount_;
            }
        }

        /// <summary>
        /// connectors
        /// </summary>
        [System.Xml.Serialization.XmlArrayItemAttribute("Connector", IsNullable = false)]
        public ConnectorConfig[] ConnectorConfigs;
    }
}
