using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 고스톱 상태 아이콘(흔들기·폭탄·굳은자·피박·광박·멍박·선·뻑) — 전부 절차적으로
/// 만든다. TMP는 이모지를 못 그리므로(전 게임 공통 함정) 종·폭탄·똥 같은
/// 그림은 <see cref="HwatuShapes"/>와 같은 방식(Texture2D 픽셀 직접 채색)으로
/// 직접 그리고, 글자가 들어가는 아이콘(피/光/멍/先)은 원형 배경 + TMP 라벨
/// 조합(GameObject)으로 만든다 — 폰트 글리프는 정상 렌더링되므로 굳이
/// 텍스처에 구워 넣을 필요가 없다.
/// </summary>
public static class GoStopIcons
{
    static Sprite bellCache, bombCache, dungCache;

    // 2026-08-18: "Kenney board-game-icons 팩을 추가했으니 매칭되는 아이콘을
    // 교체해달라" 요청 — 절차적으로 그린 것보다 실제 아트가 있으면 그쪽을
    // 우선한다. 있으면 그 스프라이트, 없으면(팩에 매칭이 없는 흔들기·뻑 등)
    // 기존 절차적 도형으로 자동 폴백한다 — 호출부는 폴백 여부를 몰라도 된다.
    static readonly Dictionary<string, Sprite> kenneyCache = new();
    static Sprite KenneyBoard(string name)
    {
        if (kenneyCache.TryGetValue(name, out var cached)) return cached;
        var sp = Resources.Load<Sprite>("UI/KenneyBoard/" + name);
        kenneyCache[name] = sp; // null도 캐싱 — 매번 Resources.Load 재시도 안 함
        return sp;
    }

    /// <summary>흔들기 — 종 모양. 위가 좁고 아래로 벌어지는 돔 + 바닥 테두리 +
    /// 손잡이 고리 + 추(clapper).</summary>
    public static Sprite Bell(int size = 64)
    {
        if (bellCache != null) return bellCache;
        var body = new Color32(237, 186, 46, 255);   // #EDBA2E 프로젝트 강조색
        var dark = new Color32(168, 122, 20, 255);

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color32[size * size];
        float cx = size * 0.5f;
        float domeTopY = size * 0.16f, domeBottomY = size * 0.68f;
        float domeTopR = size * 0.10f, domeBottomR = size * 0.34f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float fx = x + 0.5f, fy = y + 0.5f;
            float a = 0f;
            Color32 c = body;

            // 종 몸통(돔) — y에 따라 반지름이 선형으로 벌어지는 종 모양
            if (fy >= domeTopY && fy <= domeBottomY)
            {
                float t = (fy - domeTopY) / (domeBottomY - domeTopY);
                float r = Mathf.Lerp(domeTopR, domeBottomR, t);
                float d = Mathf.Abs(fx - cx);
                if (d <= r) { a = Mathf.Clamp01(r - d + 1f); if (d > r - size * 0.05f) c = dark; }
            }
            // 바닥 테두리(살짝 넓은 띠)
            if (fy > domeBottomY && fy <= domeBottomY + size * 0.06f)
            {
                float d = Mathf.Abs(fx - cx);
                if (d <= domeBottomR + size * 0.03f) { a = 1f; c = dark; }
            }
            // 추(clapper) — 종 아래 작은 원
            float clapperY = domeBottomY + size * 0.16f;
            float dc = Vector2.Distance(new Vector2(fx, fy), new Vector2(cx, clapperY));
            if (dc <= size * 0.06f) { a = Mathf.Clamp01(size * 0.06f - dc + 1f); c = dark; }
            // 손잡이 고리 — 종 위 작은 원(테두리만)
            float ringY = domeTopY - size * 0.05f;
            float dr = Vector2.Distance(new Vector2(fx, fy), new Vector2(cx, ringY));
            if (dr <= size * 0.07f && dr >= size * 0.04f) { a = 1f; c = dark; }

            px[y * size + x] = new Color32(c.r, c.g, c.b, (byte)(a * 255f));
        }
        tex.SetPixels32(px); tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;
        bellCache = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        bellCache.hideFlags = HideFlags.HideAndDontSave;
        return bellCache;
    }

    /// <summary>폭탄 — 둥근 검은 철구 + 위로 뻗은 곡선 심지 + 심지 끝 불꽃.</summary>
    public static Sprite Bomb(int size = 64)
    {
        var kenney = KenneyBoard("exploding_6"); // 실제 아트가 있으면 절차적 도형보다 우선
        if (kenney != null) return kenney;
        if (bombCache != null) return bombCache;
        var shell = new Color32(30, 30, 34, 255);
        var hi = new Color32(90, 90, 98, 255);
        var fuse = new Color32(120, 84, 50, 255);
        var flameOuter = new Color32(237, 106, 30, 255);
        var flameInner = new Color32(255, 210, 60, 255);

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color32[size * size];
        var ballCenter = new Vector2(size * 0.5f, size * 0.60f);
        float ballR = size * 0.34f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float fx = x + 0.5f, fy = y + 0.5f;
            var p = new Vector2(fx, fy);
            float a = 0f;
            Color32 c = shell;

            float d = Vector2.Distance(p, ballCenter);
            if (d <= ballR)
            {
                a = Mathf.Clamp01(ballR - d + 1f);
                // 좌상단 하이라이트
                float upLeft = Mathf.Clamp01(((ballCenter.x - fx) + (ballCenter.y - fy)) / (ballR * 1.6f));
                c = Color32.Lerp(shell, hi, upLeft * 0.6f);
            }

            // 심지 — 철구 위쪽에서 오른쪽 위로 살짝 곡선을 그리며 뻗는다
            float fuseT = -1f;
            for (float t = 0f; t <= 1f; t += 0.02f)
            {
                float sx = ballCenter.x + size * 0.06f + t * size * 0.20f;
                float sy = ballCenter.y - ballR - t * size * 0.28f + Mathf.Sin(t * 3.14f) * size * 0.05f;
                if (Vector2.Distance(p, new Vector2(sx, sy)) <= size * 0.035f) { fuseT = t; break; }
            }
            if (fuseT >= 0f && a <= 0f) { a = 1f; c = fuse; }

            // 불꽃 — 심지 끝
            var flameTip = new Vector2(ballCenter.x + size * 0.06f + size * 0.20f, ballCenter.y - ballR - size * 0.28f);
            float df = Vector2.Distance(p, flameTip);
            if (df <= size * 0.11f && a <= 0f)
            {
                a = Mathf.Clamp01(size * 0.11f - df + 1f);
                c = df <= size * 0.05f ? flameInner : flameOuter;
            }

            px[y * size + x] = new Color32(c.r, c.g, c.b, (byte)(a * 255f));
        }
        tex.SetPixels32(px); tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;
        bombCache = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        bombCache.hideFlags = HideFlags.HideAndDontSave;
        return bombCache;
    }

    /// <summary>뻑 — 똥 모양(위로 갈수록 좁아지는 3단 스웰).</summary>
    public static Sprite Dung(int size = 64)
    {
        if (dungCache != null) return dungCache;
        var body = new Color32(120, 84, 50, 255);
        var hi = new Color32(158, 116, 72, 255);

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color32[size * size];
        // 아래→위로 갈수록 작아지는 원 3개를 겹쳐서 소용돌이 실루엣을 낸다
        var lobes = new (Vector2 c, float r)[]
        {
            (new Vector2(size * 0.5f, size * 0.78f), size * 0.30f),
            (new Vector2(size * 0.5f, size * 0.54f), size * 0.24f),
            (new Vector2(size * 0.5f, size * 0.32f), size * 0.16f),
        };
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float fx = x + 0.5f, fy = y + 0.5f;
            var p = new Vector2(fx, fy);
            float best = -1f;
            foreach (var (c, r) in lobes)
            {
                float d = Vector2.Distance(p, c);
                float a = r - d;
                if (a > best) best = a;
            }
            if (best <= 0f) continue;
            float upness = Mathf.Clamp01((size * 0.78f - fy) / (size * 0.6f));
            var col = Color32.Lerp(body, hi, upness * 0.5f);
            px[y * size + x] = new Color32(col.r, col.g, col.b, (byte)(Mathf.Clamp01(best + 1f) * 255f));
        }
        tex.SetPixels32(px); tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;
        dungCache = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        dungCache.hideFlags = HideFlags.HideAndDontSave;
        return dungCache;
    }

    /// <summary>글자 배지(피/光/멍/先/!) — 원형 배경 + 가운데 정렬 TMP 라벨.
    /// 폰트에 있는 일반 글리프라 텍스처에 구울 필요 없이 그냥 TMP로 그린다
    /// (이모지만 못 그릴 뿐, 한글·한자·기본 특수문자는 정상 렌더링된다).</summary>
    public static RectTransform MakeTextIcon(Transform parent, Vector2 pos, float size, string label, Color bg, Color fg)
    {
        var rt = HwatuUI.MakeRect("Icon_" + label, parent, new Vector2(size, size), pos);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = HwatuShapes.Circle(48);
        img.color = bg;
        img.raycastTarget = false;

        var txt = HwatuUI.MakeLabel(rt, Vector2.zero, new Vector2(size, size), size * 0.5f, fg);
        txt.text = label;
        txt.font = HwatuTheme.FontBold; // 합성 볼드 대신 실제 Bold 웨이트 폰트(목업 원칙)
        txt.alignment = TextAlignmentOptions.Center;
        txt.rectTransform.anchorMin = Vector2.zero;
        txt.rectTransform.anchorMax = Vector2.one;
        txt.rectTransform.anchoredPosition = Vector2.zero;
        txt.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        return rt;
    }

    /// <summary>흔들기/뻑 횟수 배지 — 사각 라벨("[흔듬]"/"[뻑]") + 그 뒤에
    /// 작은 원 2개. 원은 기본 회색이고, <paramref name="count"/>만큼 왼쪽부터
    /// <paramref name="dotColor"/>로 채워진다(최대 <paramref name="maxCount"/>,
    /// 뻑은 3회째 즉시 승리라 2개까지만 있으면 충분하다는 사용자 확인 규칙).
    /// 2026-08-19: "뻑 배지 마지막에 뭔지 모를 숫자만 떠 있다"는 신고로
    /// 기존 원형 아이콘+구석 숫자 방식을 이걸로 교체했다.</summary>
    public static RectTransform MakeCountBadge(Transform parent, Vector2 pos, string label, Color dotColor, int count, int maxCount = 2)
    {
        const float labelW = 52f, dotSize = 13f, dotGap = 4f, pad = 4f;
        float totalW = labelW + pad + maxCount * dotSize + (maxCount - 1) * dotGap;
        var rt = HwatuUI.MakeRect("CountBadge_" + label, parent, new Vector2(totalW, 34f), pos);

        var bg = rt.gameObject.AddComponent<Image>();
        bg.sprite = HwatuShapes.RoundedRect(64, 10);
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.106f, 0.133f, 0.267f, 0.95f); // #1B2244 — 표면색
        bg.raycastTarget = false;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(rt, false);
        var labelRT = labelGo.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0f, 0.5f); labelRT.anchorMax = new Vector2(0f, 0.5f);
        labelRT.pivot = new Vector2(0f, 0.5f);
        labelRT.sizeDelta = new Vector2(labelW, 34f);
        labelRT.anchoredPosition = new Vector2(2f, 0f);
        var labelTxt = labelGo.GetComponent<TextMeshProUGUI>();
        labelTxt.text = $"[{label}]";
        labelTxt.fontSize = 15f;
        labelTxt.color = new Color(1f, 1f, 1f, 0.9f);
        labelTxt.font = HwatuTheme.FontBold;
        labelTxt.alignment = TextAlignmentOptions.Center;

        for (int i = 0; i < maxCount; i++)
        {
            var dot = new GameObject("Dot" + i, typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(rt, false);
            var dotRT = dot.GetComponent<RectTransform>();
            dotRT.anchorMin = new Vector2(0f, 0.5f); dotRT.anchorMax = new Vector2(0f, 0.5f);
            dotRT.pivot = new Vector2(0f, 0.5f);
            dotRT.sizeDelta = new Vector2(dotSize, dotSize);
            dotRT.anchoredPosition = new Vector2(labelW + pad + i * (dotSize + dotGap), 0f);
            var dotImg = dot.GetComponent<Image>();
            dotImg.sprite = HwatuShapes.Circle(24);
            dotImg.color = i < count ? dotColor : new Color(1f, 1f, 1f, 0.25f);
            dotImg.raycastTarget = false;
        }

        return rt;
    }

    /// <summary>도형 스프라이트(흔들기/폭탄/뻑) 배지 — 원형 배경 위에 아이콘
    /// 스프라이트를 얹는다. <see cref="MakeTextIcon"/>과 같은 크기 규약을
    /// 쓰므로 상태줄에서 섞어 나열해도 정렬이 맞는다.</summary>
    public static RectTransform MakeShapeIcon(Transform parent, Vector2 pos, float size, Sprite iconSprite, Color bg)
    {
        var rt = HwatuUI.MakeRect("ShapeIcon", parent, new Vector2(size, size), pos);
        var bgImg = rt.gameObject.AddComponent<Image>();
        bgImg.sprite = HwatuShapes.Circle(48);
        bgImg.color = bg;
        bgImg.raycastTarget = false;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(rt, false);
        var iconRT = iconGo.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.18f, 0.18f);
        iconRT.anchorMax = new Vector2(0.82f, 0.82f);
        iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
        var iconImg = iconGo.GetComponent<Image>();
        iconImg.sprite = iconSprite;
        iconImg.raycastTarget = false;

        return rt;
    }

    /// <summary>쪽/뻑/싹쓸이/폭탄 같은 캡처 이벤트에 곁들이는 UI 파티클
    /// 버스트 — 작은 원 여러 개가 방사형으로 튀어나가며 줄어들다 사라진다.
    /// <see cref="GoStopEffectPopup"/>(텍스트 팝업)과 같은 자리에서 같이
    /// 터뜨려 손맛을 더한다(2026-08-19, "파티클 이펙트로 좀 더 역동적으로"
    /// 요청). 진짜 <c>ParticleSystem</c> 대신 UI Image를 쓴 이유: Screen
    /// Space Overlay 캔버스는 월드스페이스 파티클을 그대로 못 그린다(별도
    /// 카메라/렌더 세팅이 필요해진다) — 이 프로젝트가 이미 전역적으로
    /// 쓰는 "Image" 패턴이 훨씬 가볍고 기존 텍스트 팝업과 레이어가 자연히
    /// 맞는다.
    /// <br/>
    /// <b>함정 — 처음엔 DOTween(DOAnchorPos/DOScale/DOFade)으로 움직였다가
    /// "Object has been destroyed but you are still trying to access it"
    /// 예외로 그 프레임의 게임 코루틴 전체가 멈추는 치명적 버그가 됐다</b>
    /// (actionBusy가 영원히 true로 남아 플레이어 턴이 다시는 안 도는 것으로
    /// 나타났다). 부모를 안 지워지는 컨테이너로 옮겨도 재현됐다 — 이
    /// 프로젝트는 `DOTween.Init`을 어디서도 명시적으로 안 불러서 SafeMode가
    /// 꺼진 기본값으로 돈다, 즉 "대상이 사라지면 자동으로 트윈을 죽여준다"는
    /// 이 프로젝트의 기존 가정 자체가 이 환경에서는 보장되지 않았다.
    /// <see cref="GoStopEffectPopup"/>이 DOTween을 문제없이 쓰는 건 그
    /// 오브젝트를 오직 자기 자신의 OnComplete만 파괴하기 때문(경쟁 상황이
    /// 없다)이고, 이 파티클처럼 부모나 다른 시스템이 예고 없이 지울 수
    /// 있는 대상은 안전하지 않다. 그래서 이 프로젝트가 이미 검증해 둔
    /// "코루틴 + 매 프레임 null 체크"(FlashAndDestroy와 같은 패턴)로 바꿨다
    /// — 대상이 사라지면 다음 프레임에 조용히 멈출 뿐 예외를 던지지 않는다.</summary>
    public static void SpawnBurst(RectTransform parent, Vector2 localPos, Color color, int count = 12)
    {
        if (parent == null) return;

        // 2026-09-03 — "필드 이펙트가 터질 때 바람 파티클(모티프)도 같이
        // 터지는 연출" 요청. 이 함수를 8개 필드 이펙트 호출부가 전부
        // 공유하므로, 여기 한 줄만 얹으면 전부 자동으로 같이 터진다.
        GoStopWindParticles.Instance?.Burst(localPos);

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Particle", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            float size = Random.Range(8f, 16f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = localPos;
            rt.localScale = Vector3.one;

            var img = go.AddComponent<Image>();
            img.sprite = HwatuShapes.Circle(32);
            img.color = color;
            img.raycastTarget = false;

            float angle = (360f / count) * i + Random.Range(-14f, 14f);
            float dist = Random.Range(70f, 150f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 target = localPos + dir * dist;
            float dur = Random.Range(0.42f, 0.62f);

            var runner = go.AddComponent<GoStopParticle>();
            runner.Animate(rt, img, localPos, target, dur, size);
        }
    }
}

/// <summary><see cref="GoStopIcons.SpawnBurst"/> 파티클 한 알을 움직이는
/// 자기 완결형 컴포넌트 — 자기 자신의 GameObject에 붙어서 스스로
/// <c>StartCoroutine</c>을 돌린다(외부 MonoBehaviour를 안 받아도 된다).
/// 매 프레임 대상이 아직 살아있는지 확인하고, 파괴됐으면 예외 없이 조용히
/// 멈춘다 — DOTween을 썼다가 "파괴된 오브젝트에 계속 접근하려 한다"는
/// 예외로 게임 코루틴 전체가 멈추는 버그를 겪은 뒤 이 패턴으로 바꿨다
/// (자세한 경위는 <see cref="GoStopIcons.SpawnBurst"/> 문서 참고).</summary>
public class GoStopParticle : MonoBehaviour
{
    public void Animate(RectTransform rt, Image img, Vector2 from, Vector2 to, float dur, float startSize)
    {
        StartCoroutine(Run(rt, img, from, to, dur, startSize));
    }

    IEnumerator Run(RectTransform rt, Image img, Vector2 from, Vector2 to, float dur, float startSize)
    {
        float t = 0f;
        Color startColor = img.color;
        while (t < dur)
        {
            if (rt == null || img == null) yield break; // 도중에 부모가 지워졌으면 조용히 멈춘다
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / dur);
            float ease = 1f - (1f - p) * (1f - p) * (1f - p); // OutCubic — 밖으로 튀는 감속
            rt.anchoredPosition = Vector2.Lerp(from, to, ease);
            float scaleP = p * p; // InQuad — 점점 빠르게 줄어든다
            rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.15f, scaleP);
            float fadeP = Mathf.Clamp01((p - 0.3f) / 0.7f); // 앞 30%는 유지, 이후 페이드
            img.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * (1f - fadeP));
            yield return null;
        }
        if (gameObject != null) Destroy(gameObject);
    }
}
