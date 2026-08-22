using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIEffects;

/// <summary>
/// 화투 카드 GameObject를 그리는 공용 헬퍼. 원래 <see cref="GoStopGame"/>(2인 맞고)
/// 안에 인스턴스 메서드로만 있던 걸 3인 고스톱(<see cref="GoStop3PGame"/>)과
/// 공유하려고 뽑아냈다 — 2인 파일은 이미 검증이 끝난 코드라 손대지 않고
/// 그대로 뒀고(회귀 위험 없음), 새 코드만 여기를 쓴다.
/// </summary>
public static class HwatuUI
{
    static TMP_FontAsset font;
    static TMP_FontAsset Font => font ??= Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");

    public static RectTransform MakeRect(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return rt;
    }

    public static TextMeshProUGUI MakeLabel(Transform parent, Vector2 pos, Vector2 size, float fontSize, Color col)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = Font;
        tmp.fontSize = fontSize;
        tmp.color = col;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }

    /// <summary>카드 한 장. onClick이 null이면 클릭 불가(레이캐스트도 끔 — 겹쳤을 때 다른 카드 입력을 안 가로챈다).
    /// <paramref name="highlightSize"/>/<paramref name="highlightOffset"/>을 생략하면 기존 공식(카드보다
    /// 사방 8px 큰 링, 카드와 같은 위치)을 쓴다 — 손패처럼 카드 비율이 특수해 기본 공식이 어긋나는
    /// 곳에서만 직접 지정한다.
    /// <br/>2026-08-20: UIEffect(mob-sakai) 도입 — 카드 본체엔 은은한 드롭섀도를 상시 걸어
    /// 평평한 이미지가 살짝 뜬 것처럼 보이게 하고(<see cref="GoStopFX.ApplyCardShadow"/>),
    /// 하이라이트 링엔 자동 반복 샤이니 스윕을 건다(<see cref="GoStopFX.ApplyShinyEdge"/>)
    /// — 코루틴 없이 <c>edgeShinyAutoPlaySpeed</c> 하나로 계속 훑고 지나간다.</summary>
    public static GameObject MakeCard(HwatuCard card, Transform parent, Vector2 pos, float w, float h,
                                      System.Action onClick, bool highlight,
                                      Vector2? highlightSize = null, Vector2? highlightOffset = null)
    {
        if (highlight)
        {
            var ring = new GameObject("Highlight", typeof(RectTransform));
            ring.transform.SetParent(parent, false);
            var ringRT = ring.GetComponent<RectTransform>();
            ringRT.anchorMin = ringRT.anchorMax = new Vector2(0.5f, 1f);
            ringRT.pivot = new Vector2(0.5f, 1f);
            ringRT.sizeDelta = highlightSize ?? new Vector2(w + 16f, h + 16f);
            ringRT.anchoredPosition = pos + (highlightOffset ?? Vector2.zero);
            var ringImg = ring.AddComponent<Image>();
            ringImg.sprite = HwatuShapes.RoundedRect(64, 12);
            ringImg.type = Image.Type.Sliced;
            ringImg.color = new Color(1f, 0.82f, 0.25f, 0.9f);
            ringImg.raycastTarget = false;
            GoStopFX.ApplyShinyEdge(ringImg);
        }

        var go = new GameObject(card.spriteName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = pos;

        var img = go.AddComponent<Image>();
        img.sprite = Resources.Load<Sprite>("Hwatu/" + card.spriteName);
        img.preserveAspect = true;
        img.raycastTarget = onClick != null;
        GoStopFX.ApplyCardShadow(img);

        if (onClick != null)
        {
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick());
        }
        return go;
    }

    /// <summary>카드 뒷면 — 금색 테두리 프레임 + 안쪽 점무늬 필드(HwatuShapes.DotGridPattern).
    /// RectTransform을 돌려줘서 호출자가 회전(좌/우 좌석을 "누워있는" 모습으로 눕히는 등)을
    /// 걸 수 있게 한다.</summary>
    public static RectTransform MakeCardBack(Transform parent, Vector2 pos, float w, float h)
    {
        var go = new GameObject("Back", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = pos;

        var frame = go.AddComponent<Image>();
        frame.sprite = HwatuShapes.RoundedRect(64, 6);
        frame.type = Image.Type.Sliced;
        frame.color = new Color(0.541f, 0.129f, 0.129f, 1f); // #8A2122 — 카드 뒷면 통일 색(사용자 확인)
        frame.raycastTarget = false;
        GoStopFX.ApplyCardShadow(frame);

        var fieldGo = new GameObject("PatternField", typeof(RectTransform));
        fieldGo.transform.SetParent(go.transform, false);
        var fieldRT = fieldGo.GetComponent<RectTransform>();
        fieldRT.anchorMin = fieldRT.anchorMax = new Vector2(0.5f, 0.5f);
        fieldRT.sizeDelta = new Vector2(w - 4f, h - 4f);
        fieldRT.anchoredPosition = Vector2.zero;
        var field = fieldGo.AddComponent<Image>();
        field.sprite = HwatuShapes.DotGridPattern();
        field.raycastTarget = false;
        return rt;
    }

    public static void MakeConfirmButton(Transform parent, Vector2 pos, string label, Color color, UnityEngine.Events.UnityAction onClick)
    {
        var rt = MakeRect("Btn", parent, new Vector2(280f, 72f), pos);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = HwatuShapes.RoundedRect(64, 10);
        img.type = Image.Type.Sliced;
        img.color = color;
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);
        var lbl = MakeLabel(rt, Vector2.zero, new Vector2(280f, 72f), 22f, Color.white);
        lbl.text = label;
    }

    public static void MakeRowBg(Transform parent, Vector2 pos, Vector2 size)
    {
        var rt = MakeRect("StatusBg", parent, size, pos);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = HwatuShapes.RoundedRect(64, 10);
        img.type = Image.Type.Sliced;
        img.color = new Color(0f, 0f, 0f, 0.22f);
        img.raycastTarget = false;
    }

    /// <summary>이름·머니 같은 상태 텍스트 뒤에 두르는 카드형 박스 — "텍스트만
    /// 덜렁 떠 있다"는 지적(4인 고스톱 좌석 정보)으로 추가했다. <see cref="MakeRowBg"/>
    /// (거의 투명한 어두운 바)보다 훨씬 진하게(표면색 #1B2244, alpha 0.88) 잡아서
    /// 실제 카드처럼 도드라져 보인다. <paramref name="textTopPos"/>/<paramref name="textHeight"/>는
    /// 그 자리에 놓일 라벨의 anchoredPosition/sizeDelta.y와 같은 값을 넘긴다
    /// (라벨과 같은 top-center pivot 기준) — 텍스트보다 상하 7px씩 여유를 두고
    /// 감싼다. 반드시 텍스트보다 먼저(sibling 순서상 먼저) 만들어야 뒤에 깔린다.</summary>
    /// <summary>반환값(Image)은 2026-08-20에 추가 — 호출부가 "지금 이 좌석
    /// 차례" 강조를 위해 나중에 색을 바꿀 수 있어야 해서(<see cref="GoStop3PGame"/>의
    /// FillSlot 참고) 생성한 배경 Image를 그대로 넘겨준다.</summary>
    public static Image MakeStatusBox(Transform parent, Vector2 textTopPos, float textHeight, float boxWidth)
    {
        var size = new Vector2(boxWidth, textHeight + 14f);
        var pos = new Vector2(textTopPos.x, textTopPos.y + 7f);
        var rt = MakeRect("StatusBox", parent, size, pos);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = HwatuShapes.RoundedRect(64, 12);
        img.type = Image.Type.Sliced;
        img.color = new Color(0.106f, 0.133f, 0.267f, 0.88f); // 표면색(#1B2244) — B안 디자인 시스템과 통일
        img.raycastTarget = false;
        return img;
    }

    /// <summary>기존 컨테이너(RectTransform) 자체에 배경 이미지를 얹는다 —
    /// <see cref="MakeStatusBox"/>처럼 별도 자식을 새로 만드는 대신, 카드가
    /// 나중에 자식으로 채워지는 존(획득패 영역 등)에 바로 붙인다. Image가
    /// 같은 GameObject에 먼저 올라가 있으므로 이후 추가되는 카드 자식들이
    /// 자동으로 그 위에 그려진다(부모 그래픽 → 자식 그래픽 순서). 회전된
    /// 컨테이너(좌/우 획득패 등)에 붙여도 배경이 같이 돌아가 자연스럽다.
    /// 2026-08-18: "Cap 영역이 필드와 헷갈린다"는 신고로 획득패 존을
    /// 필드와 다른 색(#2E3F29)으로 구분하는 데 처음 썼다.</summary>
    public static void AddZoneBackground(RectTransform rt, Color color)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = HwatuShapes.RoundedRect(64, 14);
        img.type = Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = false;
    }

    /// <summary>동전 아이콘 + 숫자 한 줄.</summary>
    public static TextMeshProUGUI BuildMoneyChip(Transform parent, Vector2 pos, float width = 150f, float iconSize = 20f, float fontSize = 16f)
    {
        float h = Mathf.Max(iconSize, fontSize + 10f);
        var chip = MakeRect("MoneyChip", parent, new Vector2(width, h), pos);

        var icon = new GameObject("Coin", typeof(RectTransform));
        icon.transform.SetParent(chip, false);
        var iconRT = icon.GetComponent<RectTransform>();
        iconRT.anchorMin = iconRT.anchorMax = new Vector2(0f, 1f);
        iconRT.pivot = new Vector2(0f, 1f);
        iconRT.sizeDelta = new Vector2(iconSize, iconSize);
        iconRT.anchoredPosition = new Vector2(0f, -(h - iconSize) * 0.5f);
        var iconImg = icon.AddComponent<Image>();
        // 2026-08-18: "Kenney board-game-icons 팩의 매칭되는 아이콘으로
        // 교체" 요청 — 절차적 동전 그림보다 실제 아트(dollar.png)를
        // 우선한다. 못 찾으면(리소스 미배치 등) 기존 절차적 동전으로 폴백.
        var dollarSprite = Resources.Load<Sprite>("UI/KenneyBoard/dollar");
        iconImg.sprite = dollarSprite != null ? dollarSprite : HwatuShapes.CoinIcon();
        iconImg.raycastTarget = false;

        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(chip, false);
        var labelRT = labelGo.GetComponent<RectTransform>();
        labelRT.anchorMin = labelRT.anchorMax = new Vector2(0f, 1f);
        labelRT.pivot = new Vector2(0f, 1f);
        labelRT.sizeDelta = new Vector2(width - iconSize - 8f, h);
        labelRT.anchoredPosition = new Vector2(iconSize + 8f, 0f);
        var label = labelGo.AddComponent<TextMeshProUGUI>();
        label.font = Font;
        label.fontSize = fontSize;
        label.color = new Color(1f, 1f, 1f, 0.95f);
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    // ── 공용 팝업(딤+패널) ───────────────────────────────
    /// <summary>전체화면 딤(반투명 검정 배경) — 모든 팝업의 공용 바탕. 반드시
    /// <paramref name="canvasRoot"/>(Canvas 바로 밑, GameUI 프리팹의 Overlay와
    /// 같은 층)에 붙여야 한다. ContentArea 밑에 붙이면 게임오버 Overlay가
    /// Canvas 자식 중 나중 순번이라 항상 그 위를 덮어버린다("점수 상세가
    /// 오버레이 뒤에 가려지는" 부류의 버그 — 이 함수로 통일해서 구조적으로
    /// 막는다). 뒤 화면 클릭을 막기 위해 raycastTarget은 항상 켜져 있고,
    /// 기본은 숨김 상태로 만들어진다.</summary>
    public static RectTransform MakeModalDim(RectTransform canvasRoot, string name, float alpha = 0.6f)
    {
        var go = new GameObject(name + "Dim", typeof(RectTransform));
        go.transform.SetParent(canvasRoot, false);
        var dim = go.GetComponent<RectTransform>();
        // 캔버스 전체(1080×1920)를 덮어야 한다 — ContentArea 높이(964)로 고정
        // 크기를 줬던 예전 버전은 딤이 화면 위쪽 절반만 덮고 아래는 안 가려져
        // "어정쩡하게 뜬다"는 신고를 받았다. stretch 앵커로 부모(canvasRoot)
        // 크기에 자동으로 맞춘다 — 하드코딩된 해상도 값에 안 기댄다.
        dim.anchorMin = Vector2.zero;
        dim.anchorMax = Vector2.one;
        dim.offsetMin = Vector2.zero;
        dim.offsetMax = Vector2.zero;
        var dimImg = go.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, alpha);
        dimImg.raycastTarget = true;
        go.SetActive(false);
        return dim;
    }

    /// <summary>딤 위에 얹는 둥근 사각 패널 — "가운데 뜨는 대화상자" 팝업(흔들기
    /// 확인·9월 열끗 선택·참가 선언·점수 상세 등)에서 쓴다. 화투장을 늘어놓고
    /// 보여주기만 하는 연출용 팝업(선 뽑기·광판다 결과)은 패널 없이
    /// <see cref="MakeModalDim"/> 위에 바로 내용을 올린다.</summary>
    public static RectTransform MakeModalPanel(RectTransform dim, string name, Vector2 size, Vector2 pos)
    {
        var panel = MakeRect(name, dim, size, pos);
        var panelImg = panel.gameObject.AddComponent<Image>();
        panelImg.sprite = HwatuShapes.RoundedRect(64, 16);
        panelImg.type = Image.Type.Sliced;
        panelImg.color = new Color(0.13f, 0.16f, 0.30f, 0.98f);
        return panel;
    }

    /// <summary><c>Assets/Resources/Prefabs/GoStop/Popups/</c>의 팝업 프리팹을
    /// 불러와 canvasRoot 밑에 인스턴스화하고 그 컴포넌트를 돌려준다. 2인/4인이
    /// 같은 프리팹을 공유하는 경우(흔들기·9월열끗·필드선택·점수상세)가 많아
    /// 공용 헬퍼로 뽑았다 — 반드시 canvasRoot(Canvas 바로 밑, Overlay와 같은
    /// 층)에 붙여야 한다(<see cref="MakeModalDim"/>과 같은 이유).</summary>
    public static T InstantiatePopup<T>(string prefabName, Transform canvasRoot) where T : Component
    {
        var prefab = Resources.Load<GameObject>("Prefabs/GoStop/Popups/" + prefabName);
        if (prefab == null) { Debug.LogError($"[HwatuUI] 팝업 프리팹 없음: {prefabName}"); return null; }
        var go = Object.Instantiate(prefab, canvasRoot, false);
        return go.GetComponent<T>();
    }

    /// <summary>쪽/쓸/뻑처럼 순간 떴다 사라지는 이펙트 프리팹 로더 —
    /// <see cref="InstantiatePopup{T}"/>와 같은 패턴이지만 별도 폴더
    /// (Effects)를 쓴다. 팝업은 상태를 갖고 열렸다 닫혔다 하지만, 이펙트는
    /// 매번 새로 Instantiate했다가 애니메이션이 끝나면 자기 자신을 파괴한다
    /// (재사용 안 함 — 동시에 여러 개가 겹쳐 뜰 수 있어서 풀링보다 단순한
    /// 즉석 생성/파괴가 맞다).</summary>
    public static T InstantiateEffect<T>(string prefabName, Transform parent) where T : Component
    {
        var prefab = Resources.Load<GameObject>("Prefabs/GoStop/Effects/" + prefabName);
        if (prefab == null) { Debug.LogError($"[HwatuUI] 이펙트 프리팹 없음: {prefabName}"); return null; }
        var go = Object.Instantiate(prefab, parent, false);
        return go.GetComponent<T>();
    }

    public static void ClearChildren(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
            Object.Destroy(t.GetChild(i).gameObject);
    }

    /// <summary>카드 리스트를 줄(row) 단위로 묶는다. <paramref name="weighted"/>가
    /// true면 장수가 아니라 카드 값(피의 쌍피=2/홑피=1 등, <see cref="HwatuCard.EffectivePiValue"/>)의
    /// 합으로 <paramref name="maxPerRow"/>를 채운다 — "5장씩"이 아니라 "5피씩"
    /// 쌓여야 하는 피 존에 쓴다(예: 쌍피1+홑피4 → 1줄에 쌍피1+홑피3(=5피), 다음
    /// 줄에 홑피1). 광/열끗/띠처럼 장당 가치가 늘 1인 존은 weighted=false로
    /// 두면 기존과 동일하게 장수 기준으로 묶인다.</summary>
    public static List<List<HwatuCard>> GroupIntoRows(List<HwatuCard> cards, int maxPerRow, bool weighted)
    {
        var rows = new List<List<HwatuCard>>();
        var cur = new List<HwatuCard>();
        int weight = 0;
        foreach (var c in cards)
        {
            int w = weighted ? c.EffectivePiValue : 1;
            if (cur.Count > 0 && weight + w > maxPerRow)
            {
                rows.Add(cur);
                cur = new List<HwatuCard>();
                weight = 0;
            }
            cur.Add(c);
            weight += w;
        }
        if (cur.Count > 0) rows.Add(cur);
        return rows;
    }

    /// <summary>세로 스크롤 가능한 콘텐츠 영역을 만든다 — 점수 상세처럼 항목 수가
    /// 게임마다 달라져 고정 높이로는 넘칠 수 있는 팝업 본문에 쓴다. 반환하는
    /// <c>content</c>에 자식을 위에서 아래로(anchoredPosition.y가 음수 방향으로)
    /// 쌓고, 마지막에 <paramref name="setContentHeight"/>로 실제 높이를 알려주면
    /// 그만큼만 스크롤된다(ContentSizeFitter 대신 호출자가 직접 재는 이유 —
    /// 카드 썸네일이 섞인 줄은 텍스트 레이아웃만으로 높이를 자동 추정하기 어렵다).</summary>
    public static (RectTransform viewport, RectTransform content) MakeScrollBody(RectTransform parent, Vector2 size, Vector2 pos)
    {
        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        var scrollRT = scrollGo.GetComponent<RectTransform>();
        scrollRT.SetParent(parent, false);
        scrollRT.anchorMin = scrollRT.anchorMax = new Vector2(0.5f, 1f);
        scrollRT.pivot = new Vector2(0.5f, 1f);
        scrollRT.sizeDelta = size;
        scrollRT.anchoredPosition = pos;
        var maskImg = scrollGo.GetComponent<Image>();
        maskImg.color = new Color(1f, 1f, 1f, 0.01f); // Mask는 알파를 요구한다 — 거의 투명하되 0은 아니게
        scrollGo.GetComponent<Mask>().showMaskGraphic = false;

        var content = MakeRect("Content", scrollRT, new Vector2(size.x, size.y), Vector2.zero);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.sizeDelta = new Vector2(0f, size.y);

        var scrollRect = scrollGo.GetComponent<ScrollRect>();
        scrollRect.content = content;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 24f;

        return (scrollRT, content);
    }
}
