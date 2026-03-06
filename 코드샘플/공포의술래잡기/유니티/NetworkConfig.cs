using System;
using System.Text;
using System.ComponentModel;
using System.Configuration;

using SBCommonLib;
using SBSocketClientLib;
using UnityEngine;
using System.IO;

#if false
namespace DummyClient
{
    public static class AppSettings
    {
        public static bool IsExist(string key_)
        {
            return ConfigurationManager.AppSettings[key_] != null;
        }

        public static string Get(string key_)
        {
            return ConfigurationManager.AppSettings[key_];
        }

        public static T Get<T>(string key_)
        {
            var value = ConfigurationManager.AppSettings[key_];
            if (string.IsNullOrWhiteSpace(value))
                throw new SettingsPropertyNotFoundException();

            var convert = TypeDescriptor.GetConverter(typeof(T));
            return (T)(convert.ConvertFromInvariantString(value));
            //return default(T);
        }

        public static void Refresh()
        {
            ConfigurationManager.RefreshSection("appSettings");
        }
    }

    public class Config : SBSingleton<Config>
    {
        public SBTcpClientConfig Load()
        {
            SBTcpClientConfig config = new SBTcpClientConfig();

            // Default: 4K
            config.ReceiveBufferSize = AppSettings.IsExist("receiveBufferSize") ? AppSettings.Get<int>("receiveBufferSize") : 4096; // int.Parse(AppSettings.Get("receiveBufferSize"));

            // Default: 4K
            config.SendBufferSize = AppSettings.IsExist("sendBufferSize") ? AppSettings.Get<int>("sendBufferSize") : 4096; // int.Parse(AppSettings.Get("sendBufferSize"));
            config.SendBufferCount = AppSettings.IsExist("sendBufferCount") ? AppSettings.Get<int>("sendBufferCount") : 100; // int.Parse(AppSettings.Get("sendBufferCount"));

            // Default: 4K
            config.DefaultPacketSize = AppSettings.IsExist("defaultPacketSize") ? AppSettings.Get<int>("defaultPacketSize") : 4096; // int.Parse(AppSettings.Get("defaultPacketSize"));
            // Default: 4M
            config.MaxPacketSize = AppSettings.IsExist("maxPacketSize") ? AppSettings.Get<int>("maxPacketSize") : 4194304; // int.Parse(AppSettings.Get("maxPacketSize"));

            config.SessionUpdateIntervalTime = AppSettings.IsExist("sessionUpdateIntervalTime") ? AppSettings.Get<int>("sessionUpdateIntervalTime") : 50; // int.Parse(AppSettings.Get("sessionUpdateIntervalTime"));
            config.SessionReceiveTimeout = AppSettings.IsExist("sessionReceiveTimeout") ? AppSettings.Get<int>("sessionReceiveTimeout") : 30000; // int.Parse(AppSettings.Get("sessionReceiveTimeout"));

            config.CryptKey = Encoding.ASCII.GetBytes(AppSettings.Get("cryptKey"));
            config.CryptIV = Encoding.ASCII.GetBytes(AppSettings.Get("cryptIV"));

            config.ConnectorConfigs = new SBTcpClientConfig.ConnectorConfig[1];
            config.ConnectorConfigs[0] = new SBTcpClientConfig.ConnectorConfig(
                "devServerConnector",
                AppSettings.Get("devServerConnectorIP"),
                AppSettings.Get<int>("devServerConnectorPort"), // int.Parse(AppSettings.Get("devServerConnectorPort")),
                AppSettings.Get<int>("devServerConnectorMaxConnectionTryCount") // int.Parse(AppSettings.Get("devServerConnectorMaxConnectionTryCount"))
                );

            return config;
        }
    }
}
#endif

//public static class AppSettings
//{
//    public static bool IsExist(string key_)
//    {
//        return ConfigurationManager.AppSettings[key_] != null;
//    }

//    public static string Get(string key_)
//    {
//        return ConfigurationManager.AppSettings[key_];
//    }

//    public static T Get<T>(string key_)
//    {
//        var value = ConfigurationManager.AppSettings[key_];
//        if (string.IsNullOrWhiteSpace(value))
//            throw new Exception("Setting property not found."); // SettingsPropertyNotFoundException();

//        var convert = TypeDescriptor.GetConverter(typeof(T));
//        return (T)(convert.ConvertFromInvariantString(value));
//        //return default(T);
//    }

//    public static void Refresh()
//    {
//        ConfigurationManager.RefreshSection("appSettings");
//    }
//}

public class NetworkConfig
{
    public int SendBufferSize { get; private set; } = 4096; // 4k
    public int SendBufferCount { get; private set; } = 16;
    public int ReceiveBufferSize { get; private set; } = 4096 * 16; // SendBufferSize * SendBufferCount // 65k
    public int DefaultPacketSize { get; private set; } = 4096; // 4k
    public int MaxPacketSize { get; private set; } = 4096 * 16 * 16; // ReceiveBufferSize * 16 // 1M
    public int SessionUpdateIntervalTime { get; private set; } = 50;
    public int SessionReceiveTimeout { get; private set; } = 30000;
    public byte[] CryptKey { get; private set; } = Encoding.ASCII.GetBytes("*wpwkrrhdntmzpd!"/*AppSettings.Get("cryptKey")*/);
    public byte[] CryptIV { get; private set; } = Encoding.ASCII.GetBytes("@rhtosemqkrtmrh!"/*AppSettings.Get("cryptIV")*/);

    public SBTcpClientConfig Load(string ip, short port)
    {
        SBDebug.Log($"{ip} / {port}");

        //#if !UNITY_ANDROID && !UNITY_IOS
        //        string path = Application.dataPath + "/config.txt";
        //        FileInfo fileInfo = new FileInfo(path);
        //        if (fileInfo.Exists)
        //        {
        //            var textValue = System.IO.File.ReadAllLines(path);
        //            SBDebug.Log(textValue);

        //            ip = textValue[0];
        //            port = int.Parse(textValue[1]);
        //        }
        //        else
        //        {
        //            using (StreamWriter outputFile = new StreamWriter(path))
        //            {
        //                outputFile.WriteLine(ip);
        //                outputFile.WriteLine(port.ToString()) ;
        //                outputFile.Close();
        //            }
        //        }
        //        SBDebug.Log($"{ip} / {port}");
        //#endif
        SBTcpClientConfig config = new SBTcpClientConfig();

        // Default: 4K
        config.ReceiveBufferSize = ReceiveBufferSize; // AppSettings.IsExist("receiveBufferSize") ? AppSettings.Get<int>("receiveBufferSize") : 4096; // int.Parse(AppSettings.Get("receiveBufferSize"));

        // Default: 4K
        config.SendBufferSize = SendBufferSize; // AppSettings.IsExist("sendBufferSize") ? AppSettings.Get<int>("sendBufferSize") : 4096; // int.Parse(AppSettings.Get("sendBufferSize"));
        config.SendBufferCount = SendBufferCount; // AppSettings.IsExist("sendBufferCount") ? AppSettings.Get<int>("sendBufferCount") : 100; // int.Parse(AppSettings.Get("sendBufferCount"));

        // Default: 4K
        config.DefaultPacketSize = DefaultPacketSize; // AppSettings.IsExist("defaultPacketSize") ? AppSettings.Get<int>("defaultPacketSize") : 4096; // int.Parse(AppSettings.Get("defaultPacketSize"));
                                                      // Default: 4M
        config.MaxPacketSize = MaxPacketSize; // AppSettings.IsExist("maxPacketSize") ? AppSettings.Get<int>("maxPacketSize") : 4194304; // int.Parse(AppSettings.Get("maxPacketSize"));

        config.SessionUpdateIntervalTime = SessionUpdateIntervalTime; // AppSettings.IsExist("sessionUpdateIntervalTime") ? AppSettings.Get<int>("sessionUpdateIntervalTime") : 50; // int.Parse(AppSettings.Get("sessionUpdateIntervalTime"));
        config.SessionReceiveTimeout = SessionReceiveTimeout; // AppSettings.IsExist("sessionReceiveTimeout") ? AppSettings.Get<int>("sessionReceiveTimeout") : 30000; // int.Parse(AppSettings.Get("sessionReceiveTimeout"));

        config.CryptKey = CryptKey; // Encoding.ASCII.GetBytes("*wpwkrrhdntmzpd!"/*AppSettings.Get("cryptKey")*/);
        config.CryptIV = CryptIV; // Encoding.ASCII.GetBytes("@rhtosemqkrtmrh!"/*AppSettings.Get("cryptIV")*/);

        config.ConnectorConfigs = new SBTcpClientConfig.ConnectorConfig[1];
        config.ConnectorConfigs[0] = new SBTcpClientConfig.ConnectorConfig(
            "devServerConnector",
            //"13.125.13.208"
            //"127.0.0.1",
            //"192.168.1.61", /*AppSettings.Get("devServerConnectorIP"),*/
            //"192.168.1.72",
            //"sandbox-gs.mynetgear.com",
            ip,
            port,
            //4317, /*AppSettings.Get<int>("devServerConnectorPort"),*/ // int.Parse(AppSettings.Get("devServerConnectorPort")),
            10 /*AppSettings.Get<int>("devServerConnectorMaxConnectionTryCount")*/ // int.Parse(AppSettings.Get("devServerConnectorMaxConnectionTryCount"))
            );

        return config;
    }
}
