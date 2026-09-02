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
}
