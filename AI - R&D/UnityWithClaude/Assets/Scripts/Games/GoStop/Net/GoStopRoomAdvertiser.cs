using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// 호스트 쪽 — 대기실이 열려 있는 동안 로컬 네트워크 전체에 "방 열림"을
/// 주기적으로 브로드캐스트한다. 게스트는 IP를 몰라도
/// <see cref="GoStopRoomScanner"/>로 이 신호를 듣고 방을 자동으로 찾는다.
///
/// UDP 브로드캐스트를 쓰는 이유 — 같은 서브넷 안의 모든 기기가 자기
/// IP를 모른 채로도 받을 수 있는 유일한 방법이다(멀티캐스트/mDNS도
/// 대안이지만, 이 프로젝트 규모에 비해 플랫폼별 설정이 더 까다롭다).
///
/// <b>표준 브로드캐스트 주소(255.255.255.255)만 쏘면 안 된다.</b> 인터페이스가
/// 여러 개 동시에 활성화된 기기(맥의 Wi-Fi+개인용 핫스팟+VPN 등 — 실제로
/// 개발 환경에서 재현: 활성 인터페이스가 10개 넘게 잡혀 있었다)에서는
/// "어느 인터페이스로 내보낼지" 라우팅 테이블이 모호해서 <c>SocketException:
/// No route to host</c>로 그냥 실패한다. 그래서 활성 IPv4 인터페이스마다
/// 그 서브넷의 방향성 브로드캐스트 주소(예: 192.168.45.255)를 계산해서
/// 각각에 직접 쏜다 — 목적지가 명확해 라우팅이 모호할 수 없다. 인터페이스
/// 열거 자체가 실패하는 드문 경우에만 예전 방식(255.255.255.255)으로 폴백한다.
/// </summary>
public class GoStopRoomAdvertiser : MonoBehaviour
{
    public const int DiscoveryPort = 47776;
    const float BroadcastIntervalSec = 1.0f;

    UdpClient udp;
    Coroutine loop;
    string hostName;
    int tcpPort;

    public int CurrentPlayerCount { get; set; } = 1;
    public int MaxPlayers { get; set; } = 4;

    /// <summary>브로드캐스트를 시작한다. <paramref name="tcpGamePort"/>는
    /// 실제 게임 연결(<see cref="TcpGoStopHostTransport"/>)이 열려 있는
    /// 포트 — 게스트가 이 값을 받아 곧바로 그 포트로 TCP 접속한다.</summary>
    public void StartAdvertising(string displayHostName, int tcpGamePort)
    {
        StopAdvertising();
        hostName = displayHostName;
        tcpPort = tcpGamePort;

        udp = new UdpClient();
        udp.EnableBroadcast = true;
        loop = StartCoroutine(BroadcastLoop());
    }

    public void StopAdvertising()
    {
        if (loop != null) { StopCoroutine(loop); loop = null; }
        udp?.Close();
        udp = null;
    }

    IEnumerator BroadcastLoop()
    {
        var wait = new WaitForSeconds(BroadcastIntervalSec);
        while (true)
        {
            var msg = new GoStopRoomAnnounce
            {
                hostName = hostName,
                tcpPort = tcpPort,
                playerCount = CurrentPlayerCount,
                maxPlayers = MaxPlayers,
            };
            string json = JsonUtility.ToJson(msg);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            foreach (var target in BroadcastTargets())
            {
                try { udp.Send(bytes, bytes.Length, new IPEndPoint(target, DiscoveryPort)); }
                catch { /* 이 인터페이스로는 못 나갔어도 나머지로는 나갈 수 있다 — 개별 실패는 무시 */ }
            }
            yield return wait;
        }
    }

    /// <summary>활성 IPv4 인터페이스마다 그 서브넷의 방향성 브로드캐스트
    /// 주소를 계산해서 돌려준다. 인터페이스를 하나도 못 찾으면(매우 드문
    /// 경우) 예전 방식(255.255.255.255)으로 폴백한다.</summary>
    static IEnumerable<IPAddress> BroadcastTargets()
    {
        bool any = false;
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

            IPInterfaceProperties props;
            try { props = ni.GetIPProperties(); } catch { continue; }

            foreach (var ua in props.UnicastAddresses)
            {
                if (ua.Address.AddressFamily != AddressFamily.InterNetwork) continue; // IPv4만
                if (ua.IPv4Mask == null) continue;
                var bcast = SubnetBroadcast(ua.Address, ua.IPv4Mask);
                if (bcast == null) continue;
                any = true;
                yield return bcast;
            }
        }
        if (!any) yield return IPAddress.Broadcast;
    }

    static IPAddress SubnetBroadcast(IPAddress address, IPAddress mask)
    {
        byte[] ip = address.GetAddressBytes();
        byte[] m = mask.GetAddressBytes();
        if (ip.Length != m.Length) return null;
        byte[] result = new byte[ip.Length];
        for (int i = 0; i < ip.Length; i++) result[i] = (byte)(ip[i] | (byte)~m[i]);
        return new IPAddress(result);
    }

    void OnDestroy() => StopAdvertising();
}
