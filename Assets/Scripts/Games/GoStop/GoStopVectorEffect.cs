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
///
/// 2026-09-06 — 정적 트리(Dim/CardRow/TitleLabel/AltRow/AltTitleLabel/
/// SliceLine)를 코드로 직접 짓던 것을 프리팹+UXML로 옮겼다
/// (<c>Assets/Resources/Prefabs/GoStop/Effects/GoStopVectorEffect.prefab</c>
/// + <c>.uxml</c>) — UI Builder(Window > UI Toolkit > UI Builder)로 그
/// UXML을 직접 열어 위치·크기·색·폰트를 편집할 수 있다. 매 판마다 장수가
/// 달라지는 카드(Image)만 여전히 <see cref="Ensure"/> 이후 코드가
/// 런타임에 채워 넣는다 — 이 부분은 UXML로 옮길 수 없다(정적 데이터가
/// 아니라서).
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

    /// <summary>2026-09-06(사용자 요청) — "한 턴에 청단비상+홍단비상처럼
    /// 이펙트가 여러 개 뜰 때는 큐에 쌓아 순차 재생하고, 대신 다음 유저의
    /// 이펙트가 등록되면 이전 유저 큐는 지우고 새 유저 것부터 우선
    /// 재생". 같은 좌석(owner)의 요청은 큐에 쌓이고, 다른 좌석의 요청이
    /// 들어오면 큐를 비우고 지금 재생 중인 것도 즉시 중단한다.
    ///
    /// `altGeneration`은 "지금 재생 권한이 있는 세대"를 가리키는 카운터다
    /// — 다른 좌석이 끼어들면 값을 올리고, `AltDispatchLoop`이 매 반복마다
    /// 자기 세대가 여전히 최신인지 확인해서 스스로 멈춘다. `StopCoroutine`
    /// 이 `yield return StartCoroutine(inner)`로 중첩된 안쪽 코루틴까지
    /// 확실히 멈춰준다는 보장이 이 프로젝트/버전에서 검증되지 않았으므로,
    /// 코루틴 정지에 기대는 대신 이 세대 비교로 명시적으로 제어한다.</summary>
    readonly struct AltEffectRequest
    {
        public readonly int owner;
        public readonly bool blocked; // false=비상, true=실패
        public readonly string title;
        public readonly List<HwatuCard> cards;
        public AltEffectRequest(int owner, bool blocked, string title, List<HwatuCard> cards)
        { this.owner = owner; this.blocked = blocked; this.title = title; this.cards = cards; }
    }
    int altQueueOwner = int.MinValue;
    int altGeneration;
    readonly Queue<AltEffectRequest> altQueue = new Queue<AltEffectRequest>();

    void EnqueueAlt(int owner, bool blocked, string title, IEnumerable<HwatuCard> cards)
    {
        var list = cards?.Where(c => c != null).ToList() ?? new List<HwatuCard>();
        if (list.Count == 0) return;

        if (owner != altQueueOwner)
        {
            // 다른 유저의 이펙트 — 이전 유저 큐를 비우고, 지금 뭔가 재생
            // 중이었다면 다음 프레임부터 스스로 멈추게 세대를 올린 뒤
            // 화면 잔여물을 바로 지운다(마스크 등 중간 상태가 남아있을
            // 수 있으므로 altRow.Clear()로 확실히 정리).
            altQueue.Clear();
            altQueueOwner = owner;
            altGeneration++;
            altRow.Clear();
            altTitleLabel.style.opacity = 0f;
        }
        altQueue.Enqueue(new AltEffectRequest(owner, blocked, title, list));
        if (playingAlt == null) playingAlt = StartCoroutine(AltDispatchLoop(altGeneration));
    }

    IEnumerator AltDispatchLoop(int myGeneration)
    {
        while (altGeneration == myGeneration && altQueue.Count > 0)
        {
            var req = altQueue.Dequeue();
            if (req.blocked) yield return StartCoroutine(PlayBlockedSeq(req.title, req.cards, myGeneration));
            else yield return StartCoroutine(PlayEmergencySeq(req.title, req.cards, myGeneration));
        }
        if (altGeneration == myGeneration) playingAlt = null;
    }

    public static GoStopVectorEffect Ensure()
    {
        if (Instance != null) return Instance;

        // 2026-09-06 — 코드로 매번 새로 짓던 트리를 프리팹+UXML로 옮겼다
        // ("비상,실패 이펙트들은 프리펩으로 로드하는건가? 기존 프리펩을
        // ui doc이 포함된 상태로 수정해줘 내가 후반작업을 좀 하고싶어"
        // 요청). `Assets/Resources/Prefabs/GoStop/Effects/
        // GoStopVectorEffect.prefab`가 UIDocument를 이미 완전히 구성된
        // 채로(panelSettings+visualTreeAsset+sortingOrder 전부 프리팹
        // 저장 시점에 구워짐) 들고 있어서, 예전에 필요했던 "GameObject를
        // 비활성으로 만들어 두고 설정을 다 채운 뒤에야 활성화한다"는
        // OnEnable 순서 방어 코드가 더 이상 필요 없다 — Instantiate 시점에
        // 이미 완전한 설정을 갖고 있으므로 OnEnable이 한 번에 정상
        // 초기화된다. 트리 자체(Dim/CardRow/TitleLabel/AltRow/
        // AltTitleLabel/SliceLine)는 이제 UXML이 정의하고, 여기선
        // 이름으로 찾아 참조만 잡는다 — UI Builder로 그 UXML을 열면
        // 위치·크기·색을 직접 편집할 수 있다.
        var prefab = Resources.Load<GameObject>("Prefabs/GoStop/Effects/GoStopVectorEffect");
        var go = Instantiate(prefab);
        go.name = "GoStopVectorEffect";
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<GoStopVectorEffect>();
        Instance.doc = go.GetComponent<UIDocument>();
        Instance.BuildTree();
        return Instance;
    }

    void BuildTree()
    {
        root = doc.rootVisualElement;
        dim = root.Q<VisualElement>("Dim");
        cardRow = root.Q<VisualElement>("CardRow");
        titleLabel = root.Q<Label>("TitleLabel");
        altRow = root.Q<VisualElement>("AltRow");
        altTitleLabel = root.Q<Label>("AltTitleLabel");
        sliceLine = root.Q<VisualElement>("SliceLine");
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
    public void PlayEmergency(int owner, string title, IEnumerable<HwatuCard> cards) =>
        EnqueueAlt(owner, false, title, cards);

    // 2026-09-06 — 사용자 확인: "텍스트 색이 잘안보여 기존 UI와 어우러지되
    // 잘보이게 색 조정해줘". 카드 바로 위에(z방향 앞으로) 겹쳐 그리게
    // 되면서 카드 배경색이 매번 달라(밝은 광 카드부터 어두운 카드까지)
    // 흰 글자만으로는 대비가 들쭉날쭉했다 — UXML에 진한 아웃라인
    // (-unity-text-outline-*)을 구워둬서 어떤 배경 위에서도 최소한의
    // 대비는 보장하고, 채우기 색은 "붉은 블링크"와 같은 계열의 danger
    // 톤(비상)/이 프로젝트의 단일 강조색 HwatuTheme.Gold(#D5A43A, 실패)로
    // 나눠서 두 이펙트가 서로 구분되면서도 기존 팔레트와 어우러지게 했다.
    static readonly Color EmergencyTitleColor = new Color(1f, 0.42f, 0.30f); // danger red-orange
    IEnumerator PlayEmergencySeq(string title, List<HwatuCard> cards, int myGeneration)
    {
        if (myGeneration != altGeneration) yield break; // 시작 전에 이미 다른 유저에게 밀림
        altRow.Clear();
        altTitleLabel.text = title;
        altTitleLabel.style.color = EmergencyTitleColor;
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
            if (myGeneration != altGeneration) yield break; // 다른 유저에게 밀림 — 화면 정리는 그쪽이 이미 했다
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
        if (myGeneration != altGeneration) yield break;

        // 3) 붉은색 블링크 3회
        var red = new Color(1f, 0.25f, 0.25f);
        for (int i = 0; i < 3; i++)
        {
            if (myGeneration != altGeneration) yield break;
            foreach (var img in imgs) img.tintColor = red;
            yield return new WaitForSeconds(0.12f);
            foreach (var img in imgs) img.tintColor = Color.white;
            yield return new WaitForSeconds(0.12f);
        }

        // 4) 짧은 홀드 후 페이드아웃 — 전체 합쳐 2초 내외
        yield return new WaitForSeconds(0.25f);
        if (myGeneration != altGeneration) yield break;
        float ft = 0f;
        const float outDur = 0.35f;
        while (ft < outDur)
        {
            if (myGeneration != altGeneration) yield break;
            ft += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(ft / outDur);
            foreach (var w in wraps) w.style.opacity = a;
            altTitleLabel.style.opacity = a;
            yield return null;
        }

        altRow.Clear();
        altTitleLabel.style.opacity = 0f;
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
    public void PlayBlocked(int owner, string title, IEnumerable<HwatuCard> cards) =>
        EnqueueAlt(owner, true, title, cards);

    /// <summary>카드 한 장을 위/아래 절반으로 "잘라 보이게" 하는 도구 —
    /// 실제 지오메트리 절단 대신, 같은 이미지를 두 번 그리고 각각 절반만
    /// 남기고 나머지는 `overflow:Hidden`으로 잘라내는(마스킹) 방식이다.
    /// 두 조각이 겹쳐진 원래 위치에 있으면 완전한 카드 한 장처럼 보이고,
    /// 이후 각자 다른 방향으로 움직이면 "위아래로 쪼개져 떨어진다"는
    /// 인상을 준다. UI Toolkit엔 진짜 메시 절단 도구가 없어서 이 마스킹
    /// 트릭이 이 프로젝트가 낼 수 있는 가장 근접한 근사다.</summary>
    readonly struct CutPiece
    {
        public readonly VisualElement mask; // overflow:Hidden 컨테이너 — 이걸 움직인다
        public readonly Image img;          // 그 안의 카드 전체 이미지(절반만 보임)
        public CutPiece(VisualElement mask, Image img) { this.mask = mask; this.img = img; }
    }

    static CutPiece MakeHalf(VisualElement parent, VectorImage vector, float cardW, float cardH, bool top)
    {
        var mask = new VisualElement();
        mask.style.position = Position.Absolute;
        mask.style.left = 0; mask.style.width = cardW;
        mask.style.height = cardH * 0.5f;
        mask.style.top = top ? 0 : cardH * 0.5f;
        mask.style.overflow = Overflow.Hidden;
        mask.pickingMode = PickingMode.Ignore;

        var img = new Image();
        img.vectorImage = vector;
        img.scaleMode = ScaleMode.ScaleToFit;
        img.style.position = Position.Absolute;
        img.style.left = 0; img.style.width = cardW; img.style.height = cardH;
        img.style.top = top ? 0 : -cardH * 0.5f; // 아래쪽 조각은 이미지를 위로 반 칸 올려서, 잘린 창 안에 "아래 절반"이 보이게 한다
        mask.Add(img);
        parent.Add(mask);
        return new CutPiece(mask, img);
    }

    IEnumerator PlayBlockedSeq(string title, List<HwatuCard> cards, int myGeneration)
    {
        if (myGeneration != altGeneration) yield break;
        altRow.Clear();
        altTitleLabel.text = title;
        altTitleLabel.style.color = HwatuTheme.Gold; // 위 EmergencyTitleColor 주석 참고
        altTitleLabel.style.opacity = 0f;
        sliceLine.style.backgroundColor = new Color(1f, 1f, 1f, 0f);

        const float cardH = 190f;
        float cardW = cardH * 0.62f;
        const float gap = 14f;

        // 2026-09-06(사용자 확인) — "닌자 후르츠처럼 카드를 겹쳐서 렌더링하다가
        // 상단 마스킹·하단 마스킹을 별도로 해서 위아래로 잘리는 연출"을
        // 원했다 — 예전(대각선 슬래시 라인 하나가 화면을 스쳐 지나가는 것)은
        // "가운데 선만 나오는" 것으로 보였다는 지적. 카드마다 처음엔 온전한
        // 이미지 하나(`wrap`/`img`, 슬램인 애니메이션용)로 보이다가, 슬램인이
        // 끝나면 그 자리에 정확히 겹치는 위/아래 절반 마스크로 바꿔치기하고
        // (겹쳐 있을 땐 시각적으로 구분이 안 감), "잘리는" 순간 위 조각은
        // 위로, 아래 조각은 아래로 벌어지며 반대 방향으로 살짝 회전+페이드
        // 되게 했다.
        var wraps = new List<VisualElement>(cards.Count);
        var imgs = new List<Image>(cards.Count);
        var tops = new List<CutPiece>(cards.Count);
        var bottoms = new List<CutPiece>(cards.Count);
        foreach (var card in cards)
        {
            var wrap = new VisualElement();
            wrap.style.width = cardW; wrap.style.height = cardH;
            wrap.style.marginLeft = gap * 0.5f; wrap.style.marginRight = gap * 0.5f;
            wrap.style.opacity = 0f;

            var vector = Resources.Load<VectorImage>(RES_PREFIX + card.spriteName);
            var img = new Image();
            img.vectorImage = vector;
            img.scaleMode = ScaleMode.ScaleToFit;
            img.style.width = Length.Percent(100);
            img.style.height = Length.Percent(100);
            wrap.Add(img);

            var top = MakeHalf(wrap, vector, cardW, cardH, top: true);
            var bottom = MakeHalf(wrap, vector, cardW, cardH, top: false);
            top.mask.style.display = DisplayStyle.None;
            bottom.mask.style.display = DisplayStyle.None;

            altRow.Add(wrap);
            wraps.Add(wrap);
            imgs.Add(img);
            tops.Add(top);
            bottoms.Add(bottom);
        }

        // 1) 카드 스태거 슬램인(완성 이펙트와 같은 방식 — 작게 시작→오버슈트→정착)
        var slams = new List<Coroutine>(wraps.Count);
        for (int i = 0; i < wraps.Count; i++)
            slams.Add(StartCoroutine(SlamCard(wraps[i], i * 0.06f)));
        foreach (var c in slams) yield return c;
        if (myGeneration != altGeneration) yield break;

        yield return Fade(a => altTitleLabel.style.opacity = a, 0.12f);
        yield return new WaitForSeconds(0.15f);
        if (myGeneration != altGeneration) yield break;

        // 2) "잘리는" 순간 — 짧은 흰 플래시(sliceLine, 스윕 없이 그 자리에서
        // 번쩍) + 카드가 붉게 물들며, 하나였던 이미지를 위/아래 마스크로
        // 바꿔치기한다. 마스크는 아직 원래 자리 그대로라 시각적으로
        // 끊김이 없다.
        sliceLine.style.rotate = new StyleRotate(new Rotate(0));
        sliceLine.style.top = 0; sliceLine.style.height = 220f;
        sliceLine.style.left = 0; sliceLine.style.right = 0;
        sliceLine.style.width = new StyleLength(StyleKeyword.Auto);
        sliceLine.style.backgroundColor = new Color(1f, 1f, 1f, 0.85f);
        yield return null;
        sliceLine.style.backgroundColor = new Color(1f, 1f, 1f, 0f);

        var red = new Color(1f, 0.35f, 0.3f);
        for (int i = 0; i < wraps.Count; i++)
        {
            imgs[i].style.display = DisplayStyle.None;
            tops[i].mask.style.display = DisplayStyle.Flex;
            bottoms[i].mask.style.display = DisplayStyle.Flex;
            tops[i].img.tintColor = red;
            bottoms[i].img.tintColor = red;
        }

        // 3) 위 조각은 위로+반시계 회전, 아래 조각은 아래로+시계 회전하며
        // 벌어진다(중력에 끊긴 느낌) — 동시에 페이드아웃.
        float dt = 0f;
        const float dropDur = 0.32f;
        while (dt < dropDur)
        {
            if (myGeneration != altGeneration) yield break;
            dt += Time.deltaTime;
            float p = Mathf.Clamp01(dt / dropDur);
            float ease = p * p; // ease-in — 처음엔 천천히, 갈수록 빠르게 벌어짐
            float sep = 55f * ease;
            float rot = 14f * ease;
            float a = 1f - Mathf.Clamp01((p - 0.4f) / 0.6f); // 후반부에만 페이드
            for (int i = 0; i < wraps.Count; i++)
            {
                tops[i].mask.style.translate = new StyleTranslate(new Translate(0, -sep, 0));
                tops[i].mask.style.rotate = new StyleRotate(new Rotate(-rot));
                tops[i].img.style.opacity = a;
                bottoms[i].mask.style.translate = new StyleTranslate(new Translate(0, sep, 0));
                bottoms[i].mask.style.rotate = new StyleRotate(new Rotate(rot));
                bottoms[i].img.style.opacity = a;
            }
            yield return null;
        }

        // 4) 짧은 홀드 후 남은 요소(타이틀 등) 페이드아웃
        yield return new WaitForSeconds(0.15f);
        if (myGeneration != altGeneration) yield break;
        float ft = 0f;
        const float outDur = 0.25f;
        while (ft < outDur)
        {
            if (myGeneration != altGeneration) yield break;
            ft += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(ft / outDur);
            altTitleLabel.style.opacity = a;
            yield return null;
        }

        altRow.Clear();
        altTitleLabel.style.opacity = 0f;
    }
}
