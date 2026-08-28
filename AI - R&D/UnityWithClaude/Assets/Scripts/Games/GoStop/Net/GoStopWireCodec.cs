using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// <see cref="GoStopNetMessage"/>를 TCP 스트림에 실어 보내고 받는 저수준
/// 프레이밍 — 호스트/클라이언트 코드가 이 로직을 각자 중복해서 짜지
/// 않도록 한 곳에 모았다. 새 네트워크 패키지를 안 쓰고 순수
/// <c>System.Net.Sockets</c>만으로 돌아간다(이 프로젝트가 오디오·이펙트를
/// 전부 직접 짜온 것과 같은 방향 — LAN 안에서 턴마다 작은 메시지 몇 개
/// 주고받는 정도라 풀 네트워킹 프레임워크는 오버킬이라고 판단했다).
///
/// <b>프레임 형식: [4바이트 길이(리틀엔디안 int32)] + [UTF8 JSON 본문]</b>.
/// TCP는 스트림이라 <c>NetworkStream.Read</c> 한 번이 요청한 바이트 수를
/// 다 채워준다는 보장이 없다 — 그래서 <see cref="ReadExact"/>로 정확히
/// N바이트가 모일 때까지 루프를 돈다. 이걸 빼먹으면 "가끔 메시지가
/// 깨진다"는 재현하기 어려운 버그가 된다.
/// </summary>
public static class GoStopWireCodec
{
    /// <summary>메시지 하나를 스트림에 써서 즉시 보낸다. 여러 스레드가
    /// 같은 스트림에 동시에 쓰면 프레임이 섞일 수 있으므로, 호출부가
    /// 스트림 하나당 락을 쥐고 불러야 한다(호스트/클라이언트 구현체가
    /// <c>lock (writeLock)</c>으로 감싼다).</summary>
    public static void Write(NetworkStream stream, GoStopNetMessage msg)
    {
        string json = JsonUtility.ToJson(msg);
        byte[] body = Encoding.UTF8.GetBytes(json);
        byte[] header = BitConverter.GetBytes(body.Length);
        stream.Write(header, 0, header.Length);
        stream.Write(body, 0, body.Length);
        stream.Flush();
    }

    /// <summary>메시지 하나를 블로킹으로 읽는다. 연결이 끊기면(상대가
    /// 소켓을 닫으면) <see cref="IOException"/> 계열 예외가 나거나
    /// 길이 0으로 읽혀 <c>null</c>을 돌려준다 — 호출부(읽기 스레드)가
    /// 이 경우를 "연결 종료"로 처리해야 한다.</summary>
    public static GoStopNetMessage Read(NetworkStream stream)
    {
        byte[] header = new byte[4];
        if (!ReadExact(stream, header, 4)) return null;
        int len = BitConverter.ToInt32(header, 0);
        if (len <= 0 || len > 1024 * 1024) return null; // 비정상 길이 방어 — 1MB 넘는 메시지는 이 게임에 있을 수 없다

        byte[] body = new byte[len];
        if (!ReadExact(stream, body, len)) return null;

        string json = Encoding.UTF8.GetString(body);
        try { return JsonUtility.FromJson<GoStopNetMessage>(json); }
        catch { return null; } // 파싱 실패한 메시지 한 개 때문에 연결 전체를 끊지 않는다
    }

    static bool ReadExact(NetworkStream stream, byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int n = stream.Read(buffer, offset, count - offset);
            if (n <= 0) return false; // 상대가 연결을 닫음
            offset += n;
        }
        return true;
    }
}
