using UnityEngine;

/// <summary>
/// 배경 바람 파티클(<see cref="GoStopWindParticles"/>)이 쓰는 4×3(12칸)
/// 텍스처 아틀라스 — 화투 12개월의 대표 식물/자연 모티프(소나무 솔잎·매화·
/// 벚꽃잎·등나무 꽃·붓꽃·모란·싸리·억새·국화·단풍·오동·빗방울)를 각 셀에
/// 하나씩 절차적으로 그린다.
///
/// 화투 카드 아트(Assets/Art/hwatu_svg)는 카드 한 장 전체 구도라 잎/꽃
/// 하나만 오려 쓰기 어렵다 — 배경에서 작고 흐릿하게 떠다니는 용도는
/// 디테일보다 "실루엣만으로 그 계절 식물처럼 읽히는지"가 기준이라, 이
/// 프로젝트가 오디오·아이콘을 전부 코드로 합성해 온 것과 같은 원칙으로
/// 절차적 도형을 골랐다. Signed-distance 근사값(경계에서 0이 되는 값,
/// 안쪽이 음수)에 <see cref="Paint"/>가 feather 폭만큼 부드럽게 알파를
/// 깎아서 계단 현상 없이 그린다 — HwatuShapes의 RoundedRect류가 쓰는
/// 것과 같은 안티에일리어싱 원리다.
/// </summary>
public static class GoStopMotifAtlas
{
    public const int Cols = 4, Rows = 3;
    public const int CellPx = 48;

    static Texture2D cached;
    public static Texture2D Texture => cached != null ? cached : (cached = Build());

    static Texture2D Build()
    {
        int w = Cols * CellPx, h = Rows * CellPx;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        var px = new Color32[w * h];
        // 전부 투명으로 시작 — Set()이 알파 0에서만 덮어써서 셀 경계 밖(투명)이
        // 이웃 셀을 침범하지 않는다.

        // 텍스처 좌표는 아래에서 위로(row 0 = 텍스처 하단)라, ParticleSystem
        // TextureSheetAnimation의 grid index(왼쪽 위부터 0,1,2... 행 우선)와
        // 어긋난다 — 하지만 이 파티클은 "12개 중 아무거나 랜덤"이 목적이라
        // 정확한 인덱스-모티프 매핑 순서는 중요하지 않다(어차피 매번 랜덤).
        void Cell(int col, int row, System.Action<Color32[], int, int, int> draw) => draw(px, w, col * CellPx, row * CellPx);

        Cell(0, 0, (p, W, ox, oy) => Needle(p, W, ox, oy, new Color(0.24f, 0.44f, 0.22f)));                 // 소나무 솔잎
        Cell(1, 0, (p, W, ox, oy) => FlowerBlob(p, W, ox, oy, 5, 0.28f, 0.22f, new Color(0.96f, 0.75f, 0.80f))); // 매화
        Cell(2, 0, (p, W, ox, oy) => NotchedPetal(p, W, ox, oy, new Color(0.98f, 0.80f, 0.86f)));           // 벚꽃잎
        Cell(3, 0, (p, W, ox, oy) => Raceme(p, W, ox, oy, new Color(0.62f, 0.48f, 0.78f)));                 // 등나무 꽃

        Cell(0, 1, (p, W, ox, oy) => NarrowPetal(p, W, ox, oy, new Color(0.42f, 0.38f, 0.72f)));            // 붓꽃
        Cell(1, 1, (p, W, ox, oy) => FlowerBlob(p, W, ox, oy, 6, 0.32f, 0.26f, new Color(0.86f, 0.32f, 0.42f))); // 모란
        Cell(2, 1, (p, W, ox, oy) => LeafPair(p, W, ox, oy, new Color(0.74f, 0.42f, 0.62f)));               // 싸리
        Cell(3, 1, (p, W, ox, oy) => Blade(p, W, ox, oy, new Color(0.86f, 0.82f, 0.66f)));                  // 억새

        Cell(0, 2, (p, W, ox, oy) => Star(p, W, ox, oy, 11, 0.58f, new Color(0.95f, 0.80f, 0.24f)));        // 국화
        Cell(1, 2, (p, W, ox, oy) => Star(p, W, ox, oy, 5, 0.32f, new Color(0.82f, 0.38f, 0.16f)));         // 단풍
        Cell(2, 2, (p, W, ox, oy) => HeartLeaf(p, W, ox, oy, new Color(0.50f, 0.40f, 0.52f)));              // 오동
        Cell(3, 2, (p, W, ox, oy) => Teardrop(p, W, ox, oy, new Color(0.55f, 0.62f, 0.74f)));               // 빗방울

        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    // ── 픽셀 페인팅 유틸 ──────────────────────────────────────────────

    static void Set(Color32[] px, int texW, int x, int y, Color c, float a)
    {
        if (x < 0 || y < 0 || x >= texW) return;
        int idx = y * texW + x;
        if (idx < 0 || idx >= px.Length) return;
        float newA = Mathf.Clamp01(a);
        var existing = px[idx];
        if (newA <= existing.a / 255f) return; // 이미 더 진한 알파가 있으면 덮어쓰지 않는다(도형끼리 겹칠 때 안전)
        px[idx] = new Color32((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), (byte)(newA * 255));
    }

    /// <paramref name="sdf"/>가 음수면 안쪽, 0이 경계 — feather 폭으로
    /// 경계를 부드럽게 깎는다.
    static void Paint(Color32[] px, int texW, int ox, int oy, System.Func<float, float, float> sdf, Color color, float feather = 0.06f)
    {
        for (int y = 0; y < CellPx; y++)
        {
            float v = (y + 0.5f) / CellPx * 2f - 1f;
            for (int x = 0; x < CellPx; x++)
            {
                float u = (x + 0.5f) / CellPx * 2f - 1f;
                float d = sdf(u, v);
                float a = Mathf.Clamp01(0.5f - d / feather);
                if (a > 0f) Set(px, texW, ox + x, oy + y, color, a);
            }
        }
    }

    static float EllipseD(float u, float v, float rx, float ry)
    {
        float du = u / rx, dv = v / ry;
        return Mathf.Sqrt(du * du + dv * dv) - 1f;
    }

    // ── 도형 원형(archetype)들 ────────────────────────────────────────

    /// <summary>소나무 솔잎 — 한 점에서 부채꼴로 퍼지는 가늘고 긴 바늘 3가닥.</summary>
    static void Needle(Color32[] px, int texW, int ox, int oy, Color color)
    {
        float[] angles = { -22f, 0f, 22f };
        foreach (var deg in angles)
        {
            float rad = deg * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
            Paint(px, texW, ox, oy, (u, v) =>
            {
                // 바늘 밑동을 화면 아래쪽(v=-0.85)에 두고 위로 뻗는다
                float ru = u * cos - (v + 0.85f) * sin;
                float rv = u * sin + (v + 0.85f) * cos;
                return EllipseD(ru, rv - 0.75f, 0.045f, 0.85f);
            }, color, 0.05f);
        }
    }

    /// <summary>벚꽃잎 — 타원 꽃잎, 넓은 쪽(꽃잎 끝) 가운데에 V자 노치.</summary>
    static void NotchedPetal(Color32[] px, int texW, int ox, int oy, Color color)
    {
        Paint(px, texW, ox, oy, (u, v) =>
        {
            float d = EllipseD(u, v - 0.05f, 0.46f, 0.62f);
            if (v > 0.30f)
            {
                float notchHalfW = Mathf.Lerp(0.0f, 0.15f, Mathf.Clamp01((v - 0.30f) / 0.35f));
                if (Mathf.Abs(u) < notchHalfW) d = Mathf.Max(d, 0.2f); // 노치 부분은 강제로 바깥 취급
            }
            return d;
        }, color);
    }

    /// <summary>등나무 꽃 — 위에서 아래로 갈수록 작아지는 3연 타원(총상꽃차례).</summary>
    static void Raceme(Color32[] px, int texW, int ox, int oy, Color color)
    {
        (float y, float r)[] beads = { (0.5f, 0.30f), (0.05f, 0.26f), (-0.42f, 0.20f) };
        foreach (var (y, r) in beads)
            Paint(px, texW, ox, oy, (u, v) => EllipseD(u, v - y, r, r * 0.9f), color, 0.05f);
    }

    /// <summary>붓꽃 — 위아래로 뾰족한 가늘고 긴 꽃잎 한 장.</summary>
    static void NarrowPetal(Color32[] px, int texW, int ox, int oy, Color color) =>
        Paint(px, texW, ox, oy, (u, v) => EllipseD(u, v, 0.24f, 0.82f), color);

    /// <summary>매화/모란 — 중심 둘레에 <paramref name="n"/>개 꽃잎(원)을 배치한
    /// 작은 꽃 실루엣. 매화는 꽃잎이 작고 촘촘(5장), 모란은 크고 겹쳐서
    /// 풍성해(3장) 보이게 반지름·개수를 다르게 쓴다.</summary>
    static void FlowerBlob(Color32[] px, int texW, int ox, int oy, int n, float ringR, float petalR, Color color)
    {
        for (int i = 0; i < n; i++)
        {
            float a = (2f * Mathf.PI / n) * i;
            float cx = ringR * Mathf.Cos(a), cy = ringR * Mathf.Sin(a);
            Paint(px, texW, ox, oy, (u, v) => EllipseD(u - cx, v - cy, petalR, petalR), color, 0.05f);
        }
        if (ringR > 0.01f) // 매화류만 중심 꽃술 점을 찍는다(모란은 ringR=0이라 이미 중심이 꽉 참)
            Paint(px, texW, ox, oy, (u, v) => EllipseD(u, v, petalR * 0.35f, petalR * 0.35f), color * 0.85f, 0.05f);
    }

    /// <summary>싸리 — 나란히 붙은 작은 타원 잎 두 장.</summary>
    static void LeafPair(Color32[] px, int texW, int ox, int oy, Color color)
    {
        Paint(px, texW, ox, oy, (u, v) => EllipseD(u + 0.26f, v, 0.28f, 0.42f), color);
        Paint(px, texW, ox, oy, (u, v) => EllipseD(u - 0.26f, v, 0.28f, 0.42f), color);
    }

    /// <summary>억새 — 완만하게 휜 가늘고 긴 잎(칼날형).</summary>
    static void Blade(Color32[] px, int texW, int ox, int oy, Color color) =>
        Paint(px, texW, ox, oy, (u, v) =>
        {
            float bentU = u - 0.30f * v * v; // v(세로) 제곱에 비례해 옆으로 휘어지는 느낌
            return EllipseD(bentU, v, 0.10f, 0.9f);
        }, color, 0.05f);

    /// <summary>국화(다각 별, 촘촘한 톱니)와 단풍(뾰족한 5각 별)을 겸하는
    /// 별 실루엣 — 각도별로 바깥(뾰족한 끝)과 안쪽(innerRatio) 반지름을
    /// 선형보간해서 각진 별을 만든다.</summary>
    static void Star(Color32[] px, int texW, int ox, int oy, int points, float innerRatio, Color color) =>
        Paint(px, texW, ox, oy, (u, v) =>
        {
            float r = Mathf.Sqrt(u * u + v * v);
            float ang = Mathf.Atan2(v, u);
            float seg = Mathf.PI / points;
            float folded = Mathf.Repeat(ang, 2f * seg) - seg;
            float t = Mathf.Abs(folded) / seg; // 0=뾰족한 끝, 1=골(valley)
            float outerR = Mathf.Lerp(0.92f, innerRatio, t);
            return r / outerR - 1f;
        }, color, 0.05f);

    /// <summary>오동 — 위쪽 두 개의 둥근 잎몸(하트 상단)이 아래로 갈수록
    /// 한 점으로 좁아지는 넓은 잎 실루엣.</summary>
    static void HeartLeaf(Color32[] px, int texW, int ox, int oy, Color color) =>
        Paint(px, texW, ox, oy, (u, v) =>
        {
            float dTopL = EllipseD(u + 0.26f, v - 0.28f, 0.40f, 0.40f);
            float dTopR = EllipseD(u - 0.26f, v - 0.28f, 0.40f, 0.40f);
            float widthAtV = Mathf.Lerp(0.62f, 0.02f, Mathf.Clamp01((v + 0.85f) / 1.1f));
            float dBottom = Mathf.Abs(u) - widthAtV; // v가 낮을수록(아래로 갈수록) 좁아지는 삼각형 몸통
            return Mathf.Min(dTopL, dTopR, dBottom);
        }, color, 0.06f);

    /// <summary>빗방울 — 아래는 둥글고 위로 갈수록 뾰족해지는 물방울형.</summary>
    static void Teardrop(Color32[] px, int texW, int ox, int oy, Color color) =>
        Paint(px, texW, ox, oy, (u, v) =>
        {
            float widthScale = v > -0.1f ? Mathf.Lerp(1f, 0.05f, Mathf.Clamp01((v + 0.1f) / 0.95f)) : 1f;
            return EllipseD(u / widthScale, v + 0.15f, 0.5f, 0.62f);
        }, color, 0.05f);
}
