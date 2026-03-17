using System.Collections.Generic;
using SBSocketPacketLib;

public class PacketQueue : SBSingleton<PacketQueue>
{
    Queue<ISBPacket> _packetQueue = new Queue<ISBPacket>();
    object _lock = new object();

    public void Push(ISBPacket packet_)
    {
        lock (_lock)
        {
            _packetQueue.Enqueue(packet_);
        }
    }

    public ISBPacket Pop()
    {
        lock (_lock)
        {
            if (0 == _packetQueue.Count)
                return null;

            return _packetQueue.Dequeue();
        }
    }

    public List<ISBPacket> PopAll()
    {
        List<ISBPacket> list = new List<ISBPacket>();

        if (_packetQueue.Count > 0)
        {
            lock (_lock)
            {
                while (_packetQueue.Count > 0)
                {
                    list.Add(_packetQueue.Dequeue());
                }
            }
        }

        return list;
    }
}
