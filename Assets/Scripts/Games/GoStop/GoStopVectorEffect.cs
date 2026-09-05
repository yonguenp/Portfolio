using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 족보 완성(고도리/홍단/초단/청단/광 4단계) 전용 풀스크린 카드 이펙트.
///
/// 기존 <see cref="GoStopEffectPopup"/>(래스터 PNG, UGUI Canvas, 필드 중앙에
/// 작게 뜸)과는 완전히 별개의 렌더 파이프라인 — UGUI Canvas가 아니라
/// UI Toolkit(UIDocument+PanelSettings)으로 그린다. Assets/Art/hwatu_svg의
/// 원본 SVG는 이미 Unity 6.3 내장 SVGImporter가 VectorImage(svgType=3)로
/// 임포트해 뒀고(<c>Assets/Resources/Hwatu_SVG/</c>에 이번에 필요한 17장만
/// 역할 이름으로 복사 — Kenney 때와 같은 "원본은 Art, 실제 쓰는 것만
/// Resources" 원칙), 벡터라서 화면 전체 크기로 확대해도 안 깨진다는 이점을
/// 실제로 살릴 수 있는 유일한 자리라 이 효과에만 도입했다.
///
/// UI Toolkit ScreenSpaceOverlay 패널은 카메라 렌더링이 끝난 뒤 별도
/// 오버레이 패스로 그려져서 UGUI Screen Space Overlay Canvas와 같은 층에서
/// 경합하지 않는다 — "이펙트는 어차피 최상단에 잠깐 뜨는 것뿐이라 순서
/// 문제는 없다"는 사용자 판단을 그대로 따랐다. sortingOrder를 크게 잡아
/// 다른 UI Toolkit 패널(지금은 이거 하나뿐)보다도 항상 위가 되게 했다.
///
/// GoStop3PGame이 이 씬의 CanvasScaler를 가로(1920×1080)+Expand로 강제
/// 덮어쓰는 것과 똑같이(2인/3인/4인 전부 이 씬 하나를 공유한다),
/// PanelSettings도 같은 referenceResolution+Expand로 맞춰서 좌표 감각이
/// UGUI 쪽과 어긋나지 않게 했다.
/// </summary>
public class GoStopVectorEffect : MonoBehaviour
{
    public static GoStopVectorEffect Instance;

    const string RES_PREFIX = "Hwatu_SVG/";

    UIDocument doc;
    VisualElement root;
    VisualElement dim;
    VisualElement cardRow;
    Label titleLabel;
    Coroutine playing;

    // 2026-09-05 — 비상/실패는 완성과 완전히 별개 트리(자기 전용 row+label)를
    // 쓴다. 화면 상단 스트립에만 그려서 "필드는 가리면 안 됨" 요구를
    // 지키고, 완성 이펙트와 동시에 떠도(같은 순간 다른 좌석이 완성+다른
    // 좌석이 비상을 함께 겪는 극히 드문 경우) 서로 트리를 안 건드린다.
    VisualElement altRow;
    Label altTitleLabel;
    VisualElement sliceLine; // 실패 전용 — 대각선 슬래시
    Coroutine playingAlt;

    public static GoStopVectorEffect Ensure()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("GoStopVectorEffect");

        // UIDocument.OnEnable은 컴포넌트가 "활성 GameObject에 붙는 그 순간"
        // 동기로 실행되고, 그 안에서 panelSettings를 바탕으로 rootVisualElement를
        // 만든다 — 그래서 AddComponent 직후 panelSettings를 아직 안 채운
        // 상태로 한 번 OnEnable이 돌면 "No Theme Style Sheet" 경고가 뜬다.
        // GameObject를 비활성으로 만들어 둔 채 UIDocument를 붙이고 panelSettings까지
        // 다 채운 뒤에야 SetActive(true)로 OnEnable을 발생시키면 경고 없이
        // 한 번에 올바르게 초기화된다 — 단, rootVisualElement는 OnEnable이
        // 돌기 전(비활성 상태)엔 null이므로, 트리를 짓는 BuildTree()는
        // 반드시 SetActive(true) 다음에 불러야 한다(비활성 상태에서 불렀다가
        // NullReferenceException으로 Ensure() 전체가 깨진 적이 있었다).
        go.SetActive(false);
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<GoStopVectorEffect>();
        Instance.doc = go.AddComponent<UIDocument>();
        Instance.doc.panelSettings = Resources.Load<PanelSettings>("Prefabs/GoStop/Effects/GoStopVectorEffectPanel");
        Instance.doc.sortingOrder = 1000f;
        go.SetActive(true);
        Instance.BuildTree();
        return Instance;
    }

    void BuildTree()
    {
        root = doc.rootVisualElement;
        root.style.position = Position.Absolute;
        root.style.left = 0; root.style.top = 0; root.style.right = 0; root.style.bottom = 0;
        root.pickingMode = PickingMode.Ignore;

        dim = new VisualElement();
        dim.style.position = Position.Absolute;
        dim.style.left = 0; dim.style.top = 0; dim.style.right = 0; dim.style.bottom = 0;
        dim.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
        dim.pickingMode = PickingMode.Ignore;
        root.Add(dim);

        cardRow = new VisualElement();
        cardRow.style.position = Position.Absolute;
        cardRow.style.left = 0; cardRow.style.right = 0;
        cardRow.style.top = 0; cardRow.style.bottom = 170; // 아래 타이틀 자리를 남긴다
        cardRow.style.alignItems = Align.Center;
        cardRow.style.justifyContent = Justify.Center;
        cardRow.style.flexDirection = FlexDirection.Row;
        cardRow.pickingMode = PickingMode.Ignore;
        root.Add(cardRow);

        titleLabel = new Label();
        titleLabel.style.position = Position.Absolute;
        titleLabel.style.left = 0; titleLabel.style.right = 0;
        titleLabel.style.bottom = 70;
        titleLabel.style.height = 90;
        titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        titleLabel.style.fontSize = 52;
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        titleLabel.style.opacity = 0f;
        titleLabel.pickingMode = PickingMode.Ignore;
        root.Add(titleLabel);

        // 비상/실패 전용 — 화면 "최상단" 스트립에만 배치해서 필드(게임판)를
        // 절대 가리지 않는다. dim(전체 화면 어둡게)은 안 쓴다 — 다른
        // 플레이어들이 계속 필드를 봐야 하는 상황이라 화면을 가리면 안 된다.
        altRow = new VisualElement();
        altRow.style.position = Position.Absolute;
        altRow.style.left = 0; altRow.style.right = 0;
        altRow.style.top = 70; // 타이틀 라벨 자리를 위에 남기고 그 아래부터
        altRow.style.height = 220;
        altRow.style.alignItems = Align.Center;
        altRow.style.justifyContent = Justify.Center;
        altRow.style.flexDirection = FlexDirection.Row;
        altRow.pickingMode = PickingMode.Ignore;
        root.Add(altRow);

        altTitleLabel = new Label();
        altTitleLabel.style.position = Position.Absolute;
        altTitleLabel.style.left = 0; altTitleLabel.style.right = 0;
        altTitleLabel.style.top = 10;
        altTitleLabel.style.height = 60;
        altTitleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        altTitleLabel.style.fontSize = 44;
        altTitleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        altTitleLabel.style.opacity = 0f;
        altTitleLabel.pickingMode = PickingMode.Ignore;
        root.Add(altTitleLabel);

        sliceLine = new VisualElement();
        sliceLine.style.position = Position.Absolute;
        sliceLine.style.left = 0; sliceLine.style.right = 0;
        sliceLine.style.top = 70; sliceLine.style.height = 220;
        sliceLine.style.backgroundColor = new Color(1f, 1f, 1f, 0f);
        sliceLine.pickingMode = PickingMode.Ignore;
        root.Add(sliceLine);
    }

    /// <summary>족보를 완성한 순간 부른다. <paramref name="cards"/>는 그 세트를
    /// 실제로 구성한 카드들(예: 고도리면 2·4·8월 열끗, 광이면 그 좌석이
    /// 실제로 가진 광 카드 전부) — 하드코딩된 고정 목록이 아니라 호출부가
    /// 그 순간의 <c>captured</c>에서 걸러 넘긴다. 광처럼 3~5장으로 장수가
    /// 갈리는 경우도 이 하나의 함수가 자동으로 대응한다.</summary>
    public void Play(string title, Color accent, IEnumerable<HwatuCard> cards)
    {
        var list = cards?.Where(c => c != null).ToList() ?? new List<HwatuCard>();
        if (list.Count == 0) return;
        if (playing != null) StopCoroutine(playing);
        playing = StartCoroutine(PlaySeq(title, accent, list));
    }

    IEnumerator PlaySeq(string title, Color accent, List<HwatuCard> cards)
    {
        cardRow.Clear();
        dim.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
        titleLabel.text = title;
        titleLabel.style.color = accent;
        titleLabel.style.opacity = 0f;

        int n = cards.Count;
        float cardH = n <= 3 ? 520f : (n == 4 ? 440f : 380f);
        float cardW = cardH * 0.62f;
        const float gap = 22f;

        var wraps = new List<VisualElement>(n);
        foreach (var card in cards)
        {
            var wrap = new VisualElement();
            wrap.style.width = cardW;
            wrap.style.height = cardH;
            wrap.style.marginLeft = gap * 0.5f;
            wrap.style.marginRight = gap * 0.5f;
            wrap.style.opacity = 0f;

            var img = new Image();
            img.vectorImage = Resources.Load<VectorImage>(RES_PREFIX + card.spriteName);
            img.scaleMode = ScaleMode.ScaleToFit;
            img.style.width = Length.Percent(100);
            img.style.height = Length.Percent(100);
            wrap.Add(img);

            cardRow.Add(wrap);
            wraps.Add(wrap);
        }

        // 1) 딤 페이드인
        yield return Fade(a => dim.style.backgroundColor = new Color(0f, 0f, 0f, a * 0.62f), 0.12f);

        // 2) 카드 스태거 슬램인(작게 시작 → 오버슈트 → 정착 + 페이드인) —
        //    한 장씩 순서대로 "빡" 박히는 느낌을 주려고 카드마다 시작을
        //    조금씩 늦춘다.
        var slams = new List<Coroutine>(n);
        for (int i = 0; i < wraps.Count; i++)
            slams.Add(StartCoroutine(SlamCard(wraps[i], i * 0.08f)));
        foreach (var c in slams) yield return c;

        // 3) 타이틀 라벨 페이드인
        yield return Fade(a => titleLabel.style.opacity = a, 0.18f);

        // 4) 홀드
        yield return new WaitForSeconds(0.9f);

        // 5) 전체 페이드아웃
        float t = 0f;
        const float outDur = 0.45f;
        while (t < outDur)
        {
            t += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(t / outDur);
            foreach (var wrap in wraps) wrap.style.opacity = a;
            titleLabel.style.opacity = a;
            dim.style.backgroundColor = new Color(0f, 0f, 0f, 0.62f * a);
            yield return null;
        }

        cardRow.Clear();
        dim.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
        titleLabel.style.opacity = 0f;
        playing = null;
    }

    IEnumerator SlamCard(VisualElement wrap, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float t = 0f;
        const float dur = 0.22f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;
            float s = p < 0.65f ? Mathf.Lerp(0.25f, 1.16f, p / 0.65f) : Mathf.Lerp(1.16f, 1f, (p - 0.65f) / 0.35f);
            wrap.style.scale = new StyleScale(new Scale(new Vector3(s, s, 1f)));
            wrap.style.opacity = Mathf.Clamp01(p / 0.5f);
            yield return null;
        }
        wrap.style.scale = new StyleScale(new Scale(Vector3.one));
        wrap.style.opacity = 1f;
    }

    IEnumerator Fade(System.Action<float> apply, float dur)
    {
        float t = 0f;
        while (t < dur) { t += Time.deltaTime; apply(Mathf.Clamp01(t / dur)); yield return null; }
        apply(1f);
    }

    // ── 비상(Emergency) — 화면 최상단, 위→아래 슬라이드, 붉은 블링크 ──
    // 2026-09-05 사용자 확인 요청: "화면 최상단에 족보 패들 위에서 아래로
    // 스윽 등장하며(필드는 가리면 안 됨) 붉은색으로 블링크, 패들 위쪽으로
    // 텍스트로 '[족보이름] 비상' 뜨고 페이드아웃, 전체 2초 내외". 예전엔
    // GoStopEffectPopup(래스터, 작은 텍스트 팝업)을 썼는데, 완성 이펙트와
    // 같은 벡터 카드 자산을 재사용해 "이 카드들이 위험하다"를 훨씬
    // 분명하게 보여준다.
    public void PlayEmergency(string title, IEnumerable<HwatuCard> cards)
    {
        var list = cards?.Where(c => c != null).ToList() ?? new List<HwatuCard>();
        if (list.Count == 0) return;
        if (playingAlt != null) StopCoroutine(playingAlt);
        playingAlt = StartCoroutine(PlayEmergencySeq(title, list));
    }

    IEnumerator PlayEmergencySeq(string title, List<HwatuCard> cards)
    {
        altRow.Clear();
        altTitleLabel.text = title;
        altTitleLabel.style.color = Color.white;
        altTitleLabel.style.opacity = 0f;

        const float cardH = 190f;
        float cardW = cardH * 0.62f;
        const float gap = 14f;

        var wraps = new List<VisualElement>(cards.Count);
        var imgs = new List<Image>(cards.Count);
        foreach (var card in cards)
        {
            var wrap = new VisualElement();
            wrap.style.width = cardW; wrap.style.height = cardH;
            wrap.style.marginLeft = gap * 0.5f; wrap.style.marginRight = gap * 0.5f;
            wrap.style.opacity = 0f;
            wrap.style.translate = new StyleTranslate(new Translate(0, -320, 0));

            var img = new Image();
            img.vectorImage = Resources.Load<VectorImage>(RES_PREFIX + card.spriteName);
            img.scaleMode = ScaleMode.ScaleToFit;
            img.style.width = Length.Percent(100);
            img.style.height = Length.Percent(100);
            wrap.Add(img);

            altRow.Add(wrap);
            wraps.Add(wrap);
            imgs.Add(img);
        }

        // 1) 위에서 아래로 스윽 슬라이드
        float t = 0f;
        const float slideDur = 0.30f;
        while (t < slideDur)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / slideDur);
            float ease = 1f - Mathf.Pow(1f - p, 3f); // ease-out cubic
            float y = Mathf.Lerp(-320f, 0f, ease);
            foreach (var w in wraps) { w.style.translate = new StyleTranslate(new Translate(0, y, 0)); w.style.opacity = p; }
            yield return null;
        }
        foreach (var w in wraps) { w.style.translate = new StyleTranslate(new Translate(0, 0, 0)); w.style.opacity = 1f; }

        // 2) 타이틀("[족보이름] 비상") 페이드인
        yield return Fade(a => altTitleLabel.style.opacity = a, 0.15f);

        // 3) 붉은색 블링크 3회
        var red = new Color(1f, 0.25f, 0.25f);
        for (int i = 0; i < 3; i++)
        {
            foreach (var img in imgs) img.tintColor = red;
            yield return new WaitForSeconds(0.12f);
            foreach (var img in imgs) img.tintColor = Color.white;
            yield return new WaitForSeconds(0.12f);
        }

        // 4) 짧은 홀드 후 페이드아웃 — 전체 합쳐 2초 내외
        yield return new WaitForSeconds(0.25f);
        float ft = 0f;
        const float outDur = 0.35f;
        while (ft < outDur)
        {
            ft += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(ft / outDur);
            foreach (var w in wraps) w.style.opacity = a;
            altTitleLabel.style.opacity = a;
            yield return null;
        }

        altRow.Clear();
        altTitleLabel.style.opacity = 0f;
        playingAlt = null;
    }

    // ── 실패(Blocked) — 카드 등장 후 대각선으로 "잘리는" 연출 ─────────
    // 2026-09-05: "족보 패들 스윽 등장 후 후르츠닌자처럼 칼로 좌상단에서
    // 우하단으로 잘리는 느낌으로 잘려서 패들 위로 '[족보이름] 실패'".
    // UI Toolkit엔 실제 메시 절단(두 조각으로 갈라져 물리적으로 떨어지는
    // 연출)을 만들 셰이더/지오메트리 도구가 없어서, 정확히 같은 픽셀
    // 단위 절단은 이번 범위 밖으로 남기고 근사로 구현했다 — 카드들이
    // 슬램인한 뒤, 좌상단→우하단 대각선 흰 슬래시 라인이 화면을 가로질러
    // 훑고 지나가는 동시에 카드가 살짝 아래로 처지며(중력에 끊긴 느낌)
    // 붉게 물들고 페이드아웃된다. 사용자에게 이 단순화를 알릴 것.
    public void PlayBlocked(string title, IEnumerable<HwatuCard> cards)
    {
        var list = cards?.Where(c => c != null).ToList() ?? new List<HwatuCard>();
        if (list.Count == 0) return;
        if (playingAlt != null) StopCoroutine(playingAlt);
        playingAlt = StartCoroutine(PlayBlockedSeq(title, list));
    }

    IEnumerator PlayBlockedSeq(string title, List<HwatuCard> cards)
    {
        altRow.Clear();
        altTitleLabel.text = title;
        altTitleLabel.style.color = Color.white;
        altTitleLabel.style.opacity = 0f;
        sliceLine.style.rotate = new StyleRotate(new Rotate(0));
        sliceLine.style.backgroundColor = new Color(1f, 1f, 1f, 0f);

        const float cardH = 190f;
        float cardW = cardH * 0.62f;
        const float gap = 14f;

        var wraps = new List<VisualElement>(cards.Count);
        var imgs = new List<Image>(cards.Count);
        foreach (var card in cards)
        {
            var wrap = new VisualElement();
            wrap.style.width = cardW; wrap.style.height = cardH;
            wrap.style.marginLeft = gap * 0.5f; wrap.style.marginRight = gap * 0.5f;
            wrap.style.opacity = 0f;

            var img = new Image();
            img.vectorImage = Resources.Load<VectorImage>(RES_PREFIX + card.spriteName);
            img.scaleMode = ScaleMode.ScaleToFit;
            img.style.width = Length.Percent(100);
            img.style.height = Length.Percent(100);
            wrap.Add(img);

            altRow.Add(wrap);
            wraps.Add(wrap);
            imgs.Add(img);
        }

        // 1) 카드 스태거 슬램인(완성 이펙트와 같은 방식 — 작게 시작→오버슈트→정착)
        var slams = new List<Coroutine>(wraps.Count);
        for (int i = 0; i < wraps.Count; i++)
            slams.Add(StartCoroutine(SlamCard(wraps[i], i * 0.06f)));
        foreach (var c in slams) yield return c;

        yield return Fade(a => altTitleLabel.style.opacity = a, 0.12f);
        yield return new WaitForSeconds(0.15f);

        // 2) 좌상단→우하단 대각선 슬래시가 화면을 훑고 지나간다("스윽 잘림")
        //    — 실제 지오메트리 절단 대신, 45도로 기울인 얇고 긴 흰 띠를
        //    화면 왼쪽 밖에서 오른쪽 밖까지 빠르게 이동시켜 "칼날이
        //    지나간다"는 인상을 준다.
        sliceLine.style.rotate = new StyleRotate(new Rotate(38f));
        sliceLine.style.backgroundColor = new Color(1f, 1f, 1f, 0.9f);
        sliceLine.style.height = 6f;
        sliceLine.style.top = 165f; // altRow 세로 중앙 근처
        sliceLine.style.left = -400f;
        sliceLine.style.right = new StyleLength(StyleKeyword.Auto);
        sliceLine.style.width = 1900f; // 대각선으로 눕혀도 화면 폭을 다 덮도록 넉넉히
        float st = 0f;
        const float sliceDur = 0.16f;
        while (st < sliceDur)
        {
            st += Time.deltaTime;
            float p = Mathf.Clamp01(st / sliceDur);
            sliceLine.style.left = Mathf.Lerp(-1200f, 1900f, p);
            yield return null;
        }
        sliceLine.style.backgroundColor = new Color(1f, 1f, 1f, 0f);

        // 3) 잘린 순간 카드가 붉게 물들며 아래로 살짝 처진다(중력에 끊긴 느낌)
        var red = new Color(1f, 0.3f, 0.25f);
        foreach (var img in imgs) img.tintColor = red;
        float dt = 0f;
        const float dropDur = 0.22f;
        while (dt < dropDur)
        {
            dt += Time.deltaTime;
            float p = Mathf.Clamp01(dt / dropDur);
            foreach (var w in wraps) w.style.translate = new StyleTranslate(new Translate(0, 40f * p, 0));
            yield return null;
        }

        // 4) 짧은 홀드 후 페이드아웃
        yield return new WaitForSeconds(0.2f);
        float ft = 0f;
        const float outDur = 0.35f;
        while (ft < outDur)
        {
            ft += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(ft / outDur);
            foreach (var w in wraps) w.style.opacity = a;
            altTitleLabel.style.opacity = a;
            yield return null;
        }

        altRow.Clear();
        altTitleLabel.style.opacity = 0f;
        playingAlt = null;
    }
}
