using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 화투 카드 그림을 그리는 데 쓰는 기본 도형 스프라이트.
///
/// 화투 이미지를 인터넷에서 받아 쓰지 않는다 — 실제 화투는 특정 제조사가 그린
/// 저작권 있는 도안이라, 광고 붙는 앱에 그대로 넣으면 법적 위험이 있다.
/// 대신 소나무·학·보름달 같은 전통 소재(수백 년 된 공용 모티프, 저작권 없음)를
/// 여기 도형들로 직접 조합해 그린다.
///
/// UISkin은 버튼용 9-slice 프레임이라 카드 그림에는 안 맞는다(베벨이 구워져
/// 있어서 평평한 카드처럼 안 보인다). 여기는 베벨 없는 평평한 도형만 만든다.
/// </summary>
public static class HwatuShapes
{
    static readonly Dictionary<int, Sprite> circleCache = new();
    static readonly Dictionary<long, Sprite> triangleCache = new();
    static readonly Dictionary<long, Sprite> roundedCache = new();
    static Sprite dotGridCache;
    static Sprite coinCache;

    /// <summary>속이 찬 원. 해·달·머리 등에 쓴다.</summary>
    public static Sprite Circle(int size = 64)
    {
        if (circleCache.TryGetValue(size, out var cached)) return cached;

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size * 0.5f;
        var cen = new Vector2(r - 0.5f, r - 0.5f);
        var px = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), cen);
            float a = Mathf.Clamp01(r - d);
            px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
        }
        tex.SetPixels32(px); tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;

        var sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        sp.hideFlags = HideFlags.HideAndDontSave;
        circleCache[size] = sp;
        return sp;
    }

    /// <summary>
    /// 위 꼭짓점 삼각형. 피벗은 <b>아래쪽 중앙</b> — 소나무처럼 여러 개를
    /// 쌓아 올릴 때 밑변 기준으로 위치를 잡기 편하다.
    /// </summary>
    public static Sprite Triangle(int w = 64, int h = 64)
    {
        long key = ((long)w << 32) | (uint)h;
        if (triangleCache.TryGetValue(key, out var cached)) return cached;

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px = new Color32[w * h];
        float cx = w * 0.5f;
        for (int y = 0; y < h; y++)
        {
            float t = (float)y / Mathf.Max(h - 1, 1);      // 0 아래, 1 위
            float halfWidth = (1f - t) * cx;                // 위로 갈수록 좁아진다
            for (int x = 0; x < w; x++)
            {
                float d = Mathf.Abs(x - cx);
                float a = Mathf.Clamp01(halfWidth - d + 1f); // 1px 안티에일리어싱
                px[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }
        tex.SetPixels32(px); tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;

        var sp = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f));
        sp.hideFlags = HideFlags.HideAndDontSave;
        triangleCache[key] = sp;
        return sp;
    }

    /// <summary>모서리 둥근 사각형(베벨 없음). 카드 배경·테두리용.</summary>
    public static Sprite RoundedRect(int size = 64, int radius = 14)
    {
        long key = ((long)size << 32) | (uint)radius;
        if (roundedCache.TryGetValue(key, out var cached)) return cached;

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = Mathf.Max(radius - x - 0.5f, 0f, x + 0.5f - (size - radius));
            float dy = Mathf.Max(radius - y - 0.5f, 0f, y + 0.5f - (size - radius));
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.Clamp01(radius - d);
            px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
        }
        tex.SetPixels32(px); tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;

        var sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                               100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        sp.hideFlags = HideFlags.HideAndDontSave;
        roundedCache[key] = sp;
        return sp;
    }

    /// <summary>
    /// 짙은 빨강 바탕에 작은 돌기(엠보싱) 점이 촘촘히 반복되는 무늬. 화투 뒷면 특유의
    /// 오돌토돌한 질감을 직접 그린 것 — 특정 제조사 사진을 쓰지 않고, 흔한
    /// 기하학적 반복 무늬(돌출된 점 격자)라는 일반적인 스타일만 참고해 새로 그렸다.
    /// 각 점에 좌상단 하이라이트 + 우하단 그림자를 줘서 평평한 원이 아니라
    /// 도드라져 보이는 돌기처럼 보이게 한다.
    /// </summary>
    public static Sprite DotGridPattern(int size = 128)
    {
        if (dotGridCache != null) return dotGridCache;

        var bg = new Color32(126, 18, 22, 255); // 짙은 적색 바탕

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color32[size * size];
        for (int i = 0; i < px.Length; i++) px[i] = bg;

        // 촘촘한 격자, 한 줄씩 어긋나게(대각선 느낌) 찍는다 — 실제 화투 뒷면의
        // 조밀한 돌기 배열과 같은 인상을 준다.
        float spacing = size / 11f;
        float r = spacing * 0.34f;
        var hi = new Color32(214, 96, 96, 255);   // 돌기 좌상단 하이라이트
        var sh = new Color32(70, 8, 12, 255);     // 돌기 우하단 그림자
        var lightDir = new Vector2(-0.6f, 0.6f).normalized; // 좌상단에서 빛

        for (int gy = -1; gy <= 12; gy++)
        for (int gx = -1; gx <= 12; gx++)
        {
            float offset = (gy % 2 == 0) ? 0f : spacing * 0.5f;
            float cx = gx * spacing + offset;
            float cy = gy * spacing;
            int minX = Mathf.Max(0, Mathf.FloorToInt(cx - r - 1));
            int maxX = Mathf.Min(size - 1, Mathf.CeilToInt(cx + r + 1));
            int minY = Mathf.Max(0, Mathf.FloorToInt(cy - r - 1));
            int maxY = Mathf.Min(size - 1, Mathf.CeilToInt(cy + r + 1));
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                var off = new Vector2(x - cx, y - cy);
                float d = off.magnitude;
                float a = Mathf.Clamp01(r - d + 0.6f); // 돌기 원판(안티에일리어싱 포함)
                if (a <= 0f) continue;

                // 빛 방향으로의 투영값(-1~1)으로 하이라이트/그림자 쪽을 가른다 —
                // 돌기 중심에서 가장자리로 갈수록 뚜렷해지는 엠보싱 느낌.
                float lit = d > 0.01f ? Vector2.Dot(off / d, lightDir) : 0f;
                float edge = Mathf.Clamp01(d / r); // 0=중심(가장 밝음) 1=가장자리
                Color32 bump = lit >= 0f
                    ? Color32.Lerp(bg, hi, edge * lit)
                    : Color32.Lerp(bg, sh, edge * -lit);

                int idx = y * size + x;
                px[idx] = Color32.Lerp(px[idx], bump, a);
            }
        }

        tex.SetPixels32(px); tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.hideFlags = HideFlags.HideAndDontSave;

        dotGridCache = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        dotGridCache.hideFlags = HideFlags.HideAndDontSave;
        return dotGridCache;
    }

    /// <summary>
    /// 판돈 표시용 동전 아이콘. 글자(₩ 등)는 안 넣는다 — 폰트에 없는 글리프가
    /// □로 깨지는 이 프로젝트 공통 함정을 아예 피한다. 금색 원판 + 살짝 어두운
    /// 테두리 링 + 위쪽 하이라이트만으로 "동전"이라는 인상을 준다.
    /// </summary>
    public static Sprite CoinIcon(int size = 48)
    {
        if (coinCache != null) return coinCache;

        var rim  = new Color32(168, 122, 20, 255);   // 짙은 금색 테두리
        var face = new Color32(237, 186, 46, 255);   // #EDBA2E — 프로젝트 강조색
        var hi   = new Color32(255, 224, 140, 255);  // 위쪽 하이라이트

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size * 0.5f;
        var cen = new Vector2(r - 0.5f, r - 0.5f);
        float innerR = r - size * 0.09f; // 테두리 두께
        var px = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            var p = new Vector2(x, y);
            float d = Vector2.Distance(p, cen);
            float a = Mathf.Clamp01(r - d);
            if (a <= 0f) { px[y * size + x] = default; continue; }

            Color32 c = d > innerR ? rim : face;
            // 위쪽(광원 방향)일수록 밝게 — 평평한 원반이 아니라 도드라진
            // 동전처럼 보이게 하는 최소한의 음영.
            if (d <= innerR)
            {
                float upness = Mathf.Clamp01((cen.y - p.y) / innerR); // 위=1, 중심=0, 아래는 0으로 클램프
                c = Color32.Lerp(face, hi, upness * 0.5f);
            }
            c.a = (byte)(a * 255f);
            px[y * size + x] = c;
        }
        tex.SetPixels32(px); tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;

        coinCache = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        coinCache.hideFlags = HideFlags.HideAndDontSave;
        return coinCache;
    }
}
