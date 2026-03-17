using System;
using System.Collections.Generic;

using SBCommonLib;

/// <summary>
/// SandBox Socket Client Library
/// </summary>
namespace SBSocketClientLib
{
    /// <summary>
    /// TCP 클라이언트 서비스 클래스
    /// </summary>
    public class SBTcpClientService
    {
        /// <summary>
        /// 서비스 Running 여부(서비스 Running 관리)
        /// </summary>
        private SBAtomicInt _isRunning = new SBAtomicInt();
        public bool IsRunning
        {
            get => _isRunning.IsOn();
        }

        /// <summary>
        /// TCP 클라이언트 설정 정보
        /// </summary>
        public SBTcpClientConfig Config { get; private set; }

        /// <summary>
        /// Connector 리스트
        /// </summary>
        private List<SBTcpConnector> _connectors = new List<SBTcpConnector>();

        public SBTcpClientService(SBTcpClientConfig config_)
        {
            this.Config = config_;
        }

        /// <summary>
        /// Connector 가져오기
        /// </summary>
        /// <param name="name_">Connector 이름</param>
        /// <returns></returns>
        public SBTcpConnector GetConnector(string name_)
        {
            foreach (SBTcpConnector connector in _connectors)
            {
                if (connector.Name == name_)
                {
                    return connector;
                }
            }

            return null;
        }

        /// <summary>
        /// Connector 추가
        /// </summary>
        /// <param name="connector_">Connector</param>
        /// <returns></returns>
        public bool AddConnector(SBTcpConnector connector_)
        {
            //SBTcpClientConfig.ConnectorConfig connectorConfig;
            //bool isFind = false;

            if (null != Config.ConnectorConfigs)
            {
                foreach (SBTcpClientConfig.ConnectorConfig config in Config.ConnectorConfigs)
                {
                    if (string.Equals(connector_.Name, config.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        //connector_.SetConnectorInfo(this, config.Ip, config.Port, config.MaxConnectionTryConut);
                        connector_.SetConnectorInfo(config.Ip, config.Port, config.MaxConnectionTryConut);
                        this._connectors.Add(connector_);
                        return true;
                        //isFind = true;
                        //connectorConfig = config;
                        //break;
                    }
                }
            }

            //if (isFind)
            //{
            //    this._connectors.Add(connector_);
            //    return true;
            //}

            return false;
        }

        /// <summary>
        /// TCP 클라이언트 설정 정보 Setup
        /// </summary>
        /// <param name="config_">TCP 클라이언트 설정</param>
        public void Setup(SBTcpClientConfig config_)
        {
            this.Config = config_;
        }

        /// <summary>
        /// 서비스 시작
        /// </summary>
        /// <returns>true/false</returns>
        public bool Run(Func<Session> sessionFactory_)
        {
            if (false == _isRunning.CasOn())
            {
                return false;
            }

            foreach (SBTcpConnector connector in _connectors)
            {
                connector.Connect(sessionFactory_);
            }

            return true;
        }

        /// <summary>
        /// 서비스 중지
        /// </summary>
        public void Stop()
        {
            _isRunning.CasOff();
        }
    }
}
