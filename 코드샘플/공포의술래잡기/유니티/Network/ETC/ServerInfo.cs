using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerInfo : Singleton<ServerInfo>
{
    public string IP{get;set;}
    public short PORT {get;set;}

    public ServerInfo()
    {    
        IP = "cmmmo.sandbox-gs.com";
        PORT = 4317;
    }
}
