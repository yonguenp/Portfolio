using UnityEngine;

/// <summary>
/// 뻑/뻑먹기/쪽/따닥/쓸 센터스크린 아이콘(2026-09-05) — 사용자는 "웹에서
/// SVG를 찾아서" 요청했지만, 이 프로젝트는 처음부터 커스텀 아트(동전 아이콘,
/// 카드 뒷면 무늬, 화투 파티클 모티프 등)를 전부 코드로 직접 그려왔다 —
/// 외부 이미지를 받아오면 출처·라이선스를 확인할 방법이 없고(이 샌드박스는
/// 임의 URL을 신뢰성 있게 받아오지 못한다), 이 프로젝트가 화투 카드조차
/// CC BY-SA 원본을 신중히 골라 쓴 전례가 있어 그 원칙을 지켰다 — 그래서
/// 같은 SDF(signed-distance function) 페인팅 방식(GoStopMotifAtlas와 같은
/// 기법이지만 그쪽은 private이라 재사용 대신 이 파일에 축소판으로 다시
/// 구현했다)으로 poop·휴지·입술·핑거스냅·빗자루 5개 아이콘을 직접 그린다.
/// 정교한 사실화가 아니라 이 프로젝트 전역의 "단순 실루엣" 톤(화투 파티클
/// 모티프와 같은 수준)에 맞춘 것이다.
/// </summary>
public static class GoStopComboIcons
{
    const int Size = 128;
    static Sprite poopSprite, tissueSprite, lipsSprite, snapSprite, broomSprite;

    public static Sprite Poop   => poopSprite   != null ? poopSprite   : (poopSprite   = Build(DrawPoop));
    public static Sprite Tissue => tissueSprite != null ? tissueSprite : (tissueSprite = Build(DrawTissue));
    public static Sprite Lips   => lipsSprite   != null ? lipsSprite   : (lipsSprite   = Build(DrawLips));
    public static Sprite Snap   => snapSprite   != null ? snapSprite   : (snapSprite   = Build(DrawSnap));
    public static Sprite Broom  => broomSprite  != null ? broomSprite  : (broomSprite  = Build(DrawBroom));

    static Sprite Build(System.Action<Color32[]> draw)
    {
        var px = new Color32[Size * Size]; // 전부 투명(0,0,0,0)에서 시작
        draw(px);
        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels32(px);
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;
        return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
    }

    static void Set(Color32[] px, int x, int y, Color c, float a)
    {
        if (x < 0 || y < 0 || x >= Size || y >= Size) return;
        int idx = y * Size + x;
        float newA = Mathf.Clamp01(a);
        var existing = px[idx];
        // 함정 — 여기 원래 "<="였다(GoStopMotifAtlas.Set과 같은 규칙, 겹치지
        // 않는 불투명 도형들끼리는 안전하다). 그런데 이 파일은 "흰 몸통
        // 위에 진한 튜브 구멍을 나중에 그린다"처럼 **의도적으로 겹치는
        // 불투명 전경 디테일**이 많다 — 두 도형이 같은 픽셀에서 둘 다
        // 알파 1.0(완전 불투명)로 끝나면 부동소수점이 정확히 같은 1.0f로
        // 맞아떨어져 "<="에 걸려 나중 도형이 통째로 무시됐다(휴지 튜브
        // 구멍·입술 라인/광택·스냅 중심 하이라이트가 전부 안 보이는 걸
        // PNG로 직접 저장해 확인하고 나서야 발견했다). "<"로 좁혀서
        // 알파가 같을 땐 나중 그림이 이기게(=의도한 레이어 순서) 했다 —
        // 알파가 실제로 더 낮을 때만(안티에일리어싱 경계) 기존 걸 지킨다.
        if (newA < existing.a / 255f) return;
        px[idx] = new Color32((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), (byte)(newA * 255));
    }

    /// <paramref name="sdf"/>가 음수면 안쪽, 0이 경계 — feather 폭으로 경계를 부드럽게 깎는다.
    static void Paint(Color32[] px, System.Func<float, float, float> sdf, Color color, float feather = 0.035f)
    {
        for (int y = 0; y < Size; y++)
        {
            float v = (y + 0.5f) / Size * 2f - 1f;
            for (int x = 0; x < Size; x++)
            {
                float u = (x + 0.5f) / Size * 2f - 1f;
                float d = sdf(u, v);
                float a = Mathf.Clamp01(0.5f - d / feather);
                if (a > 0f) Set(px, x, y, color, a);
            }
        }
    }

    static float EllipseD(float u, float v, float rx, float ry)
    {
        float du = u / rx, dv = v / ry;
        return Mathf.Sqrt(du * du + dv * dv) - 1f;
    }

    static float BoxD(float u, float v, float hw, float hh)
    {
        float dx = Mathf.Abs(u) - hw, dy = Mathf.Abs(v) - hh;
        float ax = Mathf.Max(dx, 0f), ay = Mathf.Max(dy, 0f);
        return Mathf.Sqrt(ax * ax + ay * ay) + Mathf.Min(Mathf.Max(dx, dy), 0f);
    }

    // ── 뻑(위기감) — 똥 모양 3단 스월 + 만화식 눈 두 개 ──────────────
    static void DrawPoop(Color32[] px)
    {
        var brown = new Color(0.40f, 0.27f, 0.15f);
        var brownLight = new Color(0.52f, 0.37f, 0.20f);
        Paint(px, (u, v) => EllipseD(u + 0.04f, v + 0.56f, 0.60f, 0.32f), brown);   // 밑단(가장 큼)
        Paint(px, (u, v) => EllipseD(u - 0.05f, v + 0.14f, 0.46f, 0.30f), brown);   // 중단
        Paint(px, (u, v) => EllipseD(u + 0.06f, v - 0.28f, 0.32f, 0.26f), brown);   // 상단
        Paint(px, (u, v) => EllipseD(u - 0.01f, v - 0.58f, 0.15f, 0.15f), brown);   // 꼭대기 소용돌이 끝
        Paint(px, (u, v) => EllipseD(u - 0.15f, v - 0.62f, 0.06f, 0.06f), brownLight, 0.05f); // 하이라이트
        // 만화식 눈
        Paint(px, (u, v) => EllipseD(u + 0.15f, v - 0.02f, 0.055f, 0.075f), Color.white, 0.03f);
        Paint(px, (u, v) => EllipseD(u - 0.19f, v - 0.02f, 0.055f, 0.075f), Color.white, 0.03f);
        Paint(px, (u, v) => EllipseD(u + 0.15f, v, 0.025f, 0.035f), Color.black, 0.02f);
        Paint(px, (u, v) => EllipseD(u - 0.19f, v, 0.025f, 0.035f), Color.black, 0.02f);
    }

    // ── 뻑먹기(상쾌함) — 두루마리 화장지 ────────────────────────────
    static void DrawTissue(Color32[] px)
    {
        var white = new Color(0.97f, 0.97f, 0.94f);
        var tube = new Color(0.55f, 0.50f, 0.42f);
        var shade = new Color(0.85f, 0.85f, 0.80f);
        // 몸통 — 세로 캡슐(위아래 반원 + 중간 직선 구간을 하나의 SDF로 근사)
        Paint(px, (u, v) =>
        {
            float vv = Mathf.Clamp(v, -0.32f, 0.32f);
            return EllipseD(u, v - vv, 0.40f, 0.40f);
        }, white, 0.04f);
        Paint(px, (u, v) => EllipseD(u, v - 0.60f, 0.19f, 0.085f), tube, 0.03f); // 위쪽 튜브 구멍
        Paint(px, (u, v) => BoxD(u - 0.10f, v, 0.010f, 0.50f), shade, 0.02f);    // 롤 결 음영선
        Paint(px, (u, v) => BoxD(u + 0.16f, v, 0.010f, 0.50f), shade, 0.02f);
        Paint(px, (u, v) => BoxD(u - 0.52f, v + 0.70f, 0.15f, 0.20f), white, 0.03f); // 늘어진 화장지 조각
    }

    // ── 쪽 — 입술(키스마크) ─────────────────────────────────────────
    static void DrawLips(Color32[] px)
    {
        var red = new Color(0.85f, 0.16f, 0.35f);
        var redDark = new Color(0.60f, 0.07f, 0.20f);
        var shine = new Color(1f, 0.75f, 0.80f);
        Paint(px, (u, v) => EllipseD(u + 0.19f, v - 0.08f, 0.27f, 0.19f), red, 0.03f); // 윗입술 좌
        Paint(px, (u, v) => EllipseD(u - 0.19f, v - 0.08f, 0.27f, 0.19f), red, 0.03f); // 윗입술 우
        Paint(px, (u, v) => EllipseD(u, v + 0.26f, 0.44f, 0.28f), red, 0.03f);          // 아랫입술
        Paint(px, (u, v) => BoxD(u, v - 0.02f, 0.40f, 0.014f), redDark, 0.01f);         // 입술 라인
        Paint(px, (u, v) => EllipseD(u - 0.12f, v + 0.20f, 0.08f, 0.05f), shine, 0.03f); // 광택
    }

    // ── 따닥 — 핑거스냅(스파크) ──────────────────────────────────────
    static void DrawSnap(Color32[] px)
    {
        var yellow = new Color(1f, 0.85f, 0.20f);
        Paint(px, (u, v) =>
        {
            float r = Mathf.Sqrt(u * u + v * v);
            float ang = Mathf.Atan2(v, u);
            const int points = 6;
            float seg = Mathf.PI / points;
            float folded = Mathf.Repeat(ang, 2f * seg) - seg;
            float t = Mathf.Abs(folded) / seg;
            float outerR = Mathf.Lerp(0.86f, 0.22f, t);
            return r / outerR - 1f;
        }, yellow, 0.04f);
        Paint(px, (u, v) => EllipseD(u, v, 0.17f, 0.17f), Color.white, 0.05f); // 중심 하이라이트
    }

    // ── 쓸 — 빗자루 ──────────────────────────────────────────────────
    static void DrawBroom(Color32[] px)
    {
        var handle = new Color(0.55f, 0.36f, 0.18f);
        var bristle = new Color(0.82f, 0.66f, 0.30f);
        var bristleDark = new Color(0.70f, 0.54f, 0.22f);
        const float ang = -18f * Mathf.Deg2Rad;
        float cosA = Mathf.Cos(ang), sinA = Mathf.Sin(ang);
        Paint(px, (u, v) =>
        {
            float ru = u * cosA - v * sinA;
            float rv = u * sinA + v * cosA;
            return BoxD(ru, rv - 0.12f, 0.05f, 0.52f);
        }, handle, 0.03f); // 손잡이(대각선 막대)
        for (int i = -2; i <= 2; i++)
        {
            float offset = i * 0.10f;
            var col = (i % 2 == 0) ? bristle : bristleDark;
            Paint(px, (u, v) => BoxD(u - offset * 1.3f, v + 0.55f, 0.045f, 0.30f), col, 0.03f);
        }
        Paint(px, (u, v) => BoxD(u, v + 0.30f, 0.34f, 0.055f), handle, 0.02f); // 솔 밑동 띠
    }
}
