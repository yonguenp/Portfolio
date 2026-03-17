using System;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

using UnityEngine;

using SBSocketClientLib;
using SBSocketSharedLib;

public class TcpConnector : SBTcpConnector
{
    public long StartTime { get; set; }
    public long EndTime { get; set; }
    public System.Diagnostics.Stopwatch SystemPerformanceWatch { get; set; }
    public bool IsConnected { get => ((Session != null) ? Session.IsConnected : false); }

    Action actionConnected = null;

    public void SetActionConneced(Action action)
    {
        actionConnected = action;
    }

    public TcpConnector(string name_) : base(name_)
    {
        StartTime = DateTime.Now.Ticks;
    }

    protected override void OnConnected(Session session_)
    {
        SBDebug.Log($"[TcpConnector.OnConnected]");
        if(actionConnected != null)
            actionConnected.Invoke();
    }
}
