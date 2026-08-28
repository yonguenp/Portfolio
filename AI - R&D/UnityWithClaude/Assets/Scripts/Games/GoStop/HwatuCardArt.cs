using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 화투 카드 그림. 전통 소재(소나무·학·보름달 등)를 <see cref="HwatuShapes"/>의
/// 기본 도형으로 직접 조합해 그린다 — 실제 화투 이미지를 스캔/다운로드하지 않는
/// 이유는 <see cref="HwatuShapes"/> 클래스 설명 참고.
///
/// 지금은 1월 광(송학 — 소나무 위 학, 붉은 해) 샘플 하나만 만들어서 퀄리티를
/// 확인하는 단계다. 통과하면 나머지 11개월 47장을 같은 방식으로 만든다.
/// </summary>
public static class HwatuCardArt
{
    // 화투 실물 비율(약 55:85)에 맞춘 카드 크기.
    public const float CARD_W = 220f;
    public const float CARD_H = 340f;

    static RectTransform MakeRT(string name, Transform parent, Vector2 size, Vector2 pos, float pivotX = 0.5f, float pivotY = 0.5f)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(pivotX, pivotY);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return rt;
    }

    static Image AddImg(RectTransform rt, Sprite sp, Color col, bool sliced = false)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sp; img.color = col; img.raycastTarget = false;
        if (sliced) img.type = Image.Type.Sliced;
        return img;
    }

    /// <summary>카드 한 장의 빈 배경(테두리+바탕)을 만들고, 그 안쪽 RectTransform을 돌려준다.</summary>
    static RectTransform BuildBase(Transform parent, string name)
    {
        var root = MakeRT(name, parent, new Vector2(CARD_W, CARD_H), Vector2.zero);

        // 바깥 테두리(진한 적갈색) 위에 안쪽 배경(아이보리)을 얹어 액자 느낌을 낸다.
        var border = MakeRT("Border", root, new Vector2(CARD_W, CARD_H), Vector2.zero);
        AddImg(border, HwatuShapes.RoundedRect(64, 10), new Color(0.29f, 0.10f, 0.08f), true);

        var bg = MakeRT("BG", root, new Vector2(CARD_W - 14f, CARD_H - 14f), Vector2.zero);
        AddImg(bg, HwatuShapes.RoundedRect(64, 8), new Color(0.93f, 0.87f, 0.74f), true);

        return bg;
    }

    /// <summary>정삼각형 3단을 쌓아 소나무 실루엣을 만든다. anchor는 밑동 좌표.</summary>
    static void Pine(Transform parent, Vector2 baseAnchor)
    {
        var trunkRT = MakeRT("Trunk", parent, new Vector2(12f, 46f), baseAnchor + new Vector2(0f, 0f), 0.5f, 0f);
        AddImg(trunkRT, HwatuShapes.RoundedRect(16, 3), new Color(0.36f, 0.22f, 0.12f), true);

        var green = new Color(0.16f, 0.36f, 0.20f);
        float[] widths  = { 92f, 74f, 54f };
        float[] heights = { 46f, 42f, 38f };
        float y = baseAnchor.y + 40f;
        for (int i = 0; i < 3; i++)
        {
            var t = MakeRT("Foliage" + i, parent, new Vector2(widths[i], heights[i]),
                           new Vector2(baseAnchor.x, y), 0.5f, 0f);
            AddImg(t, HwatuShapes.Triangle(64, 64), green);
            y += heights[i] * 0.62f;   // 다음 단이 앞단과 겹치며 소나무 특유의 뭉친 실루엣을 만든다
        }
    }

    /// <summary>학. 몸통(타원)+목(가늘고 긴 사각형)+머리(원)+부리(삼각형)+다리(선) 조합.</summary>
    static void Crane(Transform parent, Vector2 pos)
    {
        var group = MakeRT("Crane", parent, Vector2.zero, pos);
        var white = new Color(0.97f, 0.97f, 0.95f);
        var black = new Color(0.12f, 0.12f, 0.12f);

        // 몸통 — 원을 눕혀서 늘리면 타원이 된다.
        var body = MakeRT("Body", group, new Vector2(58f, 34f), new Vector2(0f, 0f));
        body.localRotation = Quaternion.Euler(0, 0, -12f);
        AddImg(body, HwatuShapes.Circle(48), white);

        // 목 — 몸통에서 위로 뻗어 살짝 꺾인 목을 사각형 회전으로 표현한다.
        var neck = MakeRT("Neck", group, new Vector2(10f, 46f), new Vector2(20f, 28f), 0.5f, 0f);
        neck.localRotation = Quaternion.Euler(0, 0, -28f);
        AddImg(neck, HwatuShapes.RoundedRect(32, 5), white, true);

        var head = MakeRT("Head", group, new Vector2(16f, 16f), new Vector2(38f, 50f));
        AddImg(head, HwatuShapes.Circle(32), white);

        var beak = MakeRT("Beak", group, new Vector2(8f, 16f), new Vector2(47f, 50f), 0.5f, 0f);
        beak.localRotation = Quaternion.Euler(0, 0, -70f);
        AddImg(beak, HwatuShapes.Triangle(32, 32), new Color(0.85f, 0.35f, 0.12f));

        // 다리 — 가는 검은 사각형 두 개, 아래로 벌어지게.
        for (int i = -1; i <= 1; i += 2)
        {
            var leg = MakeRT("Leg" + i, group, new Vector2(4f, 30f), new Vector2(i * 8f, -14f), 0.5f, 1f);
            leg.localRotation = Quaternion.Euler(0, 0, i * 10f);
            AddImg(leg, HwatuShapes.RoundedRect(8, 1), black, true);
        }
    }

    static TextMeshProUGUI AddLabel(Transform parent, string text, float size, Color col, Vector2 pos, Vector2 boxSize)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = boxSize;
        rt.anchoredPosition = pos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
        tmp.text = text; tmp.fontSize = size; tmp.color = col;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    /// <summary>1월 광 — 송학(松鶴). 소나무 위로 해가 뜨고 학이 난다.</summary>
    public static GameObject BuildJanuaryGwang(Transform parent)
    {
        var bg = BuildBase(parent, "Card_1_Gwang");

        // 해 — 우상단.
        var sun = MakeRT("Sun", bg, new Vector2(46f, 46f), new Vector2(60f, 118f));
        AddImg(sun, HwatuShapes.Circle(48), new Color(0.80f, 0.18f, 0.16f));

        // 소나무 — 좌하단에서 자라 올라온다.
        Pine(bg, new Vector2(-38f, -150f));

        // 학 — 소나무 위, 해 아래를 지나가는 자리.
        Crane(bg, new Vector2(10f, 30f));

        // 월 표시(하단) + 광 표(우하단 금색 배지).
        AddLabel(bg, "1월 · 소나무", 15f, new Color(0.28f, 0.16f, 0.10f), new Vector2(0f, -142f), new Vector2(180f, 26f));

        var badge = MakeRT("GwangBadge", bg, new Vector2(38f, 38f), new Vector2(70f, -128f));
        AddImg(badge, HwatuShapes.Circle(48), new Color(0.80f, 0.62f, 0.18f));
        AddLabel(badge, "光", 20f, new Color(0.30f, 0.16f, 0.02f), Vector2.zero, new Vector2(38f, 38f));

        return bg.parent.gameObject; // Card_1_Gwang 루트
    }
}
