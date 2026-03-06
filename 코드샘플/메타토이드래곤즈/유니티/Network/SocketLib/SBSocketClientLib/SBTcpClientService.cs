using System;
using System.Collections.Generic;
using System.Threading;
using SBPacketLib;

namespace SBSocketClientLib
{
    public class PacketData
    {
        public Int16 DataSize;
        public Int16 PacketID;
        public SByte Type;
        public byte[]? BodyData;
    }

    public class SBTcpClientService
    {
        public SBTcpConnector? Connector { get; private set; } = null;


        private bool IsNetworkThreadRunning = false;

        private Thread? NetworkReadThread = null;
        private Thread? NetworkSendThread = null;

        private SBPacketManager PacketManager = new SBPacketManager();

        private Queue<PacketData> RecvPacketQueue = new Queue<PacketData>();

        //개선작업 큐가아닌 배열로 해서 한번에 보낼수 있도록 수정이 필요
        private Queue<byte[]> SendPacketQueue = new Queue<byte[]>();
        private int SendPacketSize = 0;

        public SBTcpClientService()
        {
            Connector = new SBTcpConnector();

            //스레드 생성

            PacketManager.Init((8096 * 10), PacketDef.PACKET_HEADER_SIZE, 4096);

            IsNetworkThreadRunning = true;
            NetworkReadThread = new Thread(new ThreadStart(NetworkReadProcess));
            NetworkReadThread.Start();
            NetworkSendThread = new Thread(new ThreadStart(NetworkSendProcess));
            NetworkSendThread.Start();
        }

        public bool IsConnect()
        {
            return Connector != null ? Connector.IsConnected() : false;
        }

        public void Connect(string ip_, int port_, int tryConnet_)
        {
            //if (Connector == null)
            //    return;

            if (Connector == null)
            {
                Connector = new SBTcpConnector();
                Connector.Connect(ip_, port_, tryConnet_);
            }
            else
                Connector.ReConnect(ip_, port_, tryConnet_);
        }

        public void DisConnect()
        {
            if (Connector != null)
            {
                SendPacketQueue.Clear();

                Connector.Close();
                Connector = null;
            }
        }

        public void ReConnent(string ip_, int port_, int tryConnet_)
        {
            //if (Connector == null)
            //    return;

            DisConnect();

            if (Connector == null)
            {
                Connector = new SBTcpConnector();
            }

            Connector.ReConnect(ip_, port_, tryConnet_);
        }


        public List<PacketData> GetPacket()
        {
            List<PacketData>? temp = new List<PacketData>();

            lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
            {
                int over = 0;
                if (SandboxNetwork.ChatManager.CHAT_QUEUE_MAX_SIZE < RecvPacketQueue.Count)
                    over = RecvPacketQueue.Count - SandboxNetwork.ChatManager.CHAT_QUEUE_MAX_SIZE;

                while (0 < RecvPacketQueue.Count)
                {
                    PacketData packet = RecvPacketQueue.Dequeue();

                    if (over > 0)
                        over--;
                    else
                        temp.Add(packet);
                }

            }

            return temp;
        }

        public void Send(byte[] sendData_)
        {
            lock (((System.Collections.ICollection)SendPacketQueue).SyncRoot)
            {
                SendPacketQueue.Enqueue(sendData_);
                SendPacketSize += sendData_.Length;
            }
        }


        //
        void NetworkReadProcess()
        {
            const Int16 PacketHeaderSize = PacketDef.PACKET_HEADER_SIZE;

            while (IsNetworkThreadRunning)
            {
                if (Connector == null || Connector.IsConnected() == false)
                {
                    Thread.Sleep(300);
                    continue;
                }

                var recvData = Connector.Receive();
                if (recvData != null)
                {
                    PacketManager.Write(recvData.Item2, 0, recvData.Item1);

                    while (true)
                    {
                        var data = PacketManager.Read();
                        if (data.Count < 1)
                        {
                            break;
                        }

                        var packet = new PacketData();
                        packet.DataSize = (short)(data.Count - PacketHeaderSize);
                        packet.PacketID = BitConverter.ToInt16(data.Array, data.Offset + 2);
                        packet.Type = (SByte)data.Array[(data.Offset + 4)];
                        packet.BodyData = new byte[packet.DataSize];
                        Buffer.BlockCopy(data.Array, (data.Offset + PacketHeaderSize), packet.BodyData, 0, (data.Count - PacketHeaderSize));
                        lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                        {
                            RecvPacketQueue.Enqueue(packet);
                        }
                    }
                    //DevLog.Write($"받은 데이터: {recvData.Item2}", LOG_LEVEL.INFO);
                }
                else
                {
#if DEBUG
                    UnityEngine.Debug.Log("##Chat 여기로 들어오면 커넥터가 이상함 => 서버 접속종료");
#endif
                    DisConnect();
                    //DevLog.Write("서버와 접속 종료 !!!", LOG_LEVEL.INFO);
                }

                Thread.Sleep(300);
            }
        }

        void NetworkSendProcess()
        {
            while (IsNetworkThreadRunning)
            {
                //딜레이를 줘서 모아서 패킷을 보내도록 구현
                if (Connector == null || Connector.IsConnected() == false)
                {
                    Thread.Sleep(300);
                    continue;
                }

                if (SendPacketQueue.Count > 0)
                {
                    lock (((System.Collections.ICollection)SendPacketQueue).SyncRoot)
                    {
                        //모아서 보내준다.
                        var packet = new byte[SendPacketSize];
                        int size = 0;

                        while (SendPacketQueue.TryDequeue(out var outPacket))
                        {
                            Buffer.BlockCopy(outPacket, 0, packet, size, outPacket.Length);
                            size += outPacket.Length;
                        }

                        Connector.Send(packet);
                        SendPacketSize = 0;
                    }
                }

                Thread.Sleep(30);
            }
        }
    }
}
