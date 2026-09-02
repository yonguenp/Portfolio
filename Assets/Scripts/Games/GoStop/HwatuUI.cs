using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIEffects;

/// <summary>
/// 화투 카드 GameObject를 그리는 공용 헬퍼. <see cref="GoStop3PGame"/>
/// (2~4인 전부)이 쓴다.
/// </summary>
public static class HwatuUI
{
    static TMP_FontAsset Font => HwatuTheme.Font;

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
    /// <br/>2026-09-01: "디자인이 다르다, new GameObject 대신 클론을 만들어라"
    /// 요청 — 카드 프레임을 코드로 직접 조립하는 대신, 씬의 SampleCard를
    /// 그대로 저장해 둔 프리팹(Assets/Resources/Prefabs/GoStop/Cards/
    /// CardFront.prefab)을 복제한다. 사용자가 SampleCard를 에디터에서
    /// 계속 손볼 수 있고, 그 결과가 이 프리팹에 반영되기만 하면 코드를
    /// 다시 안 고쳐도 자동으로 실제 게임에 따라온다. 프레임의 룩(색·
    /// 테두리·그림자 유무 등)은 전부 프리팹이 갖고 있으므로 여기서는
    /// 크기·위치·클릭 가능 여부·실제 카드 그림·하이라이트 on/off만 주입한다.
    /// <br/>하이라이트도 별도 오브젝트를 새로 만들지 않는다 — CardFront
    /// 프리팹 안에 이미 "Highlight" 자식(카드보다 사방 8px 큰 골드 링,
    /// 기본 비활성)이 있어서 여기선 SetActive만 토글한다. 카드 크기가
    /// 호출부마다 달라도(w,h) Highlight가 스트레치 앵커라 자동으로 같이
    /// 늘어나므로 예전처럼 크기를 따로 넘겨줄 필요가 없다.
    /// <br/>2026-08-20: UIEffect(mob-sakai) 도입 — 카드 본체엔 은은한 드롭섀도를 상시 걸어
    /// 평평한 이미지가 살짝 뜬 것처럼 보이게 하고(<see cref="GoStopFX.ApplyCardShadow"/>),
    /// 하이라이트 링엔 자동 반복 샤이니 스윕을 건다(<see cref="GoStopFX.ApplyShinyEdge"/>)
    /// — 코루틴 없이 <c>edgeShinyAutoPlaySpeed</c> 하나로 계속 훑고 지나간다.</summary>
    public static GameObject MakeCard(HwatuCard card, Transform parent, Vector2 pos, float w, float h,
                                      System.Action onClick, bool highlight)
    {
        var prefab = Resources.Load<GameObject>("Prefabs/GoStop/Cards/CardFront");
        var go = Object.Instantiate(prefab, parent, false);
        go.name = card.spriteName;
        var rt = go.GetComponent<RectTransform>();
        // SampleCard 자체는 디자인 미리보기용으로 중앙 고정 앵커를 쓰지만,
        // 실제 게임의 모든 호출부는 top-pivot 기준 anchoredPosition으로
        // 좌표를 계산한다 — 복제 직후 앵커/피벗을 게임 규약에 맞게 덮어쓴다.
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = pos;

        var frame = go.GetComponent<Image>();
        frame.raycastTarget = false; // 클릭 서피스는 Art가 담당(아래) — frame은 장식용 프레임일 뿐
        GoStopFX.ApplyCardShadow(frame);

        var artImg = go.transform.Find("Art").GetComponent<Image>();
        artImg.sprite = Resources.Load<Sprite>("Hwatu/" + card.spriteName);
        artImg.raycastTarget = onClick != null;

        var highlightGo = go.transform.Find("Highlight").gameObject;
        highlightGo.SetActive(highlight);
        if (highlight)
        {
            var hImg = highlightGo.GetComponent<Image>();
            hImg.sprite = HwatuShapes.RoundedRect(64, 12);
            hImg.type = Image.Type.Sliced;
            GoStopFX.ApplyShinyEdge(hImg);
        }

        if (onClick != null)
        {
            // 2026-09-01: "Hand 카드 버튼이 안 눌린다 — frame 이미지가 꺼져
            // 있기 때문" — frame이 이제 raycastTarget=false(장식)라 Button의
            // targetGraphic도 실제로 클릭을 받는 Art를 봐야 한다. 이 프로젝트
            // 공통 함정(raycastTarget=false 그래픽 위에 얹은 버튼은 클릭이
            // 조상으로 샌다)과 정확히 같은 원인이다.
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = artImg;
            btn.onClick.AddListener(() => onClick());
        }
        return go;
    }

    /// <summary>카드 뒷면 — 2026-09-01: "디자인이 다르다, 클론을 만들어라"
    /// 요청으로 코드 조립 대신 씬의 SampleCardBack을 저장해 둔 프리팹
    /// (Assets/Resources/Prefabs/GoStop/Cards/CardBack.prefab)을 복제한다.
    /// 룩(짙은 적갈색+금테 프레임, 안쪽 Mask+패턴 구조)은 전부 프리팹이
    /// 갖고 있어 여기서는 크기·위치만 주입한다 — 사용자가 SampleCardBack을
    /// 에디터에서 계속 다듬으면 이 프리팹만 다시 구우면 그대로 반영된다.
    /// RectTransform을 돌려줘서 호출자가 회전(좌/우 좌석을 "누워있는"
    /// 모습으로 눕히는 등)을 걸 수 있게 한다.</summary>
    public static RectTransform MakeCardBack(Transform parent, Vector2 pos, float w, float h, bool miniback = false)
    {
        var prefab = Resources.Load<GameObject>(miniback ? "Prefabs/GoStop/Cards/CardBackMini" : "Prefabs/GoStop/Cards/CardBack");
        var go = Object.Instantiate(prefab, parent, false);
        go.name = "Back";
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = pos;

        var frame = go.GetComponent<Image>();
        GoStopFX.ApplyCardShadow(frame);
        return rt;
    }

    /// <summary>기존 컨테이너(RectTransform) 자체에 배경 이미지를 얹는다 —
    /// 별도 자식을 새로 만드는 대신, 카드가
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

    /// <summary>테두리가 있는 버전 — 오리엔탈 목업의 "panel_dark"(짙은 초록
    /// 채움 + 더 짙은 테두리)를 그대로 옮겨온 것. Field처럼 "틀에 담긴
    /// 패널"이라는 인상이 필요한 큰 영역에 쓴다 — Cap 존(<see
    /// cref="AddZoneBackground"/>, 테두리 없는 플랫 색)과 시각적으로
    /// 구분하기 위해 프레임 유무로 나눴다(색만으로는 다시 헷갈릴 수 있어서
    /// — 예전에 "Cap이 필드와 헷갈린다" 신고로 Cap에 배경을 준 적이 있다).</summary>
    public static void AddFramedZoneBackground(RectTransform rt, Color fill, Color border, int borderWidth = 4)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = HwatuShapes.RoundedRectBordered(96, 20, borderWidth, fill, border);
        img.type = Image.Type.Sliced;
        img.color = Color.white; // 색은 이미 스프라이트에 구워져 있다 — 틴트하면 프레임/채움 대비가 죽는다
        img.raycastTarget = false;
    }


    /// <summary><c>Assets/Resources/Prefabs/GoStop/Popups/</c>의 팝업 프리팹을
    /// 불러와 canvasRoot 밑에 인스턴스화하고 그 컴포넌트를 돌려준다. 2인/4인이
    /// 같은 프리팹을 공유하는 경우(흔들기·9월열끗·필드선택·점수상세)가 많아
    /// 공용 헬퍼로 뽑았다 — 반드시 canvasRoot(Canvas 바로 밑, Overlay와 같은
    /// 층)에 붙여야 한다(다른 게임 오버레이에 가려지지 않도록).</summary>
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

    /// <summary>좌석 정보 박스 등 팝업·이펙트가 아닌 나머지 UI 프리팹 —
    /// <see cref="InstantiatePopup{T}"/>/<see cref="InstantiateEffect{T}"/>와
    /// 같은 패턴, 별도 폴더(UI)만 다르다.</summary>
    public static T InstantiateUIPrefab<T>(string prefabName, Transform parent) where T : Component
    {
        var prefab = Resources.Load<GameObject>("Prefabs/GoStop/UI/" + prefabName);
        if (prefab == null) { Debug.LogError($"[HwatuUI] UI 프리팹 없음: {prefabName}"); return null; }
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
