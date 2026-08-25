using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Game1to50 : MonoBehaviour
{
    [SerializeField] Transform gridContainer;
    [SerializeField] TextMeshProUGUI nextText;
    [SerializeField] GameUIManager ui;

    const string BestKey = "Best1to50";

    readonly Image[] cellByNum = new Image[51];
    int nextTarget;
    float elapsed;
    bool running;

    void Start()
    {
        BuildGrid();
        StartGame();
    }

    void BuildGrid()
    {
        foreach (Transform c in gridContainer) Destroy(c.gameObject);

        // 2026-08-25 — Kenney 밝은 Depth 스킨 통일(Phase 2). 이 게임은 셀
        // 색상이 데이터(숫자)를 안 나타내고 상태(기본/정답)만 나타내므로
        // Depth 스프라이트로 완전히 전환할 수 있었다(2048/1010과 다른 점).
        var frameGo = new GameObject("Frame");
        frameGo.transform.SetParent(gridContainer, false);
        var frameRT = frameGo.AddComponent<RectTransform>();
        frameRT.anchorMin = Vector2.zero; frameRT.anchorMax = Vector2.one;
        frameRT.offsetMin = Vector2.zero; frameRT.offsetMax = Vector2.zero;
        UISkin.Apply(frameGo.AddComponent<Image>(), UISkin.PanelBody);

        var nums = Enumerable.Range(1, 50).ToList();
        for (int i = 49; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (nums[i], nums[j]) = (nums[j], nums[i]);
        }

        var fnt = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
        foreach (int n in nums)
        {
            int num = n;
            var go = new GameObject($"N{n}");
            go.transform.SetParent(gridContainer, false);

            cellByNum[n] = UISkin.Apply(go.AddComponent<Image>(), UISkin.DepthButton(UISkin.Accent.Grey));
            cellByNum[n].color = Color.white;

            go.AddComponent<Button>().onClick.AddListener(() => OnTap(num));

            var tg = new GameObject("T");
            tg.transform.SetParent(go.transform, false);
            var tr = tg.AddComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.sizeDelta = Vector2.zero;
            var tmp = tg.AddComponent<TextMeshProUGUI>();
            tmp.text = n.ToString();
            tmp.fontSize = 32;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            if (fnt) tmp.font = fnt;
        }
    }

    void StartGame()
    {
        nextTarget = 1; elapsed = 0; running = true;
        ui?.HideOverlay();
        ui?.SetTitle("1 to 50");
        ui?.SetScore("0.0s");
        float best = PlayerPrefs.GetFloat(BestKey, float.MaxValue);
        ui?.SetBest(best < float.MaxValue ? $"최고 {best:F1}s" : "-");
        UpdateHint();
        for (int i = 1; i <= 50; i++)
            if (cellByNum[i]) UISkin.Apply(cellByNum[i], UISkin.DepthButton(UISkin.Accent.Grey));
    }

    void OnTap(int number)
    {
        if (!running || number != nextTarget) return;
        UISkin.Apply(cellByNum[number], UISkin.DepthButton(UISkin.Accent.Green));
        nextTarget++;
        if (nextTarget > 50) { running = false; ShowResult(); }
        else UpdateHint();
    }

    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        ui?.SetScore($"{elapsed:F1}s");
    }

    void UpdateHint()
    {
        if (nextText) nextText.text = $"다음  →  {nextTarget}";
    }

    void ShowResult()
    {
        float best = PlayerPrefs.GetFloat(BestKey, float.MaxValue);
        if (elapsed < best) { best = elapsed; PlayerPrefs.SetFloat(BestKey, best); PlayerPrefs.Save(); }
        ui?.SetBest($"최고 {best:F1}s");
        ui?.ShowOverlay(
            new Color(.98f, .82f, .10f), "완료!",
            $"{elapsed:F2}초", $"최고  {best:F2}초",
            "다시 시작", OnRestart,
            "타이틀", OnBack);
    }

    public void OnRestart() => StartGame();
    public void OnBack()    => SceneManager.LoadScene("TitleScene");
}
