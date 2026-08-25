using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ColorSortGame : MonoBehaviour
{
    const int CAP = LevelDatabase.CAPACITY;

    [SerializeField] RectTransform tubeContainer;
    [SerializeField] TextMeshProUGUI moveText;
    [SerializeField] GameObject hintHighlight;
    [SerializeField] GameUIManager ui;   // 공용 UI (GameUI 프리팹)

    bool cleared;   // 승리 오버레이 표시 중

    List<List<int>> tubes;
    int selected = -1;
    int moves;
    int level;
    readonly Stack<UndoState> undoStack = new Stack<UndoState>();

    Image[][] blockSlots;
    Image[] tubeBg;
    RectTransform[] tubeRects;
    float[] tubeOrigY;

    void Start()
    {
        // 공용 HUD에서 이 게임이 쓰지 않는 요소는 숨긴다 (점수/최고점/NEW 없음)
        ui?.SetScoreVisible(false);
        ui?.SetBestVisible(false);
        ui?.SetNewGameAction(null);

        level = PlayerPrefs.GetInt("CurrentLevel", 0);
        level = Mathf.Clamp(level, 0, LevelDatabase.Levels.Length - 1);
        LoadLevel(level);
    }

    void LoadLevel(int idx)
    {
        selected = -1;
        moves = 0;
        undoStack.Clear();
        level = Mathf.Clamp(idx, 0, LevelDatabase.Levels.Length - 1);

        var template = LevelDatabase.Levels[level];
        tubes = new List<List<int>>(template.Length);
        foreach (var t in template)
            tubes.Add(new List<int>(t));

        ui?.SetTitle($"LEVEL  {level + 1}");
        UpdateMoveText();
        cleared = false;
        ui?.HideOverlay();
        if (hintHighlight) hintHighlight.SetActive(false);

        BuildTubeUI();
    }

    void BuildTubeUI()
    {
        foreach (Transform child in tubeContainer)
            Destroy(child.gameObject);

        int n = tubes.Count;
        blockSlots = new Image[n][];
        tubeBg     = new Image[n];
        tubeRects  = new RectTransform[n];
        tubeOrigY  = new float[n];

        // Layout calculation
        float areaW  = 1000f; // tubeContainer reference width (safe margin on 1080)
        float tubeW  = Mathf.Clamp(Mathf.Floor((areaW - (n - 1) * 10f) / n), 80f, 140f);
        float tubeH  = tubeW * 3.8f;
        float blockH = (tubeH - 14f) / CAP;
        float totalW = n * tubeW + (n - 1) * 10f;
        float startX = -totalW / 2f + tubeW / 2f;

        for (int i = 0; i < n; i++)
        {
            int idx = i;

            var go = new GameObject($"Tube{i}");
            go.transform.SetParent(tubeContainer, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(tubeW, tubeH);
            float origY = 0f;
            rt.anchoredPosition = new Vector2(startX + i * (tubeW + 10f), origY);
            tubeRects[i] = rt;
            tubeOrigY[i] = origY;

            // Drop shadow
            var sh = new GameObject("Shadow");
            sh.transform.SetParent(go.transform, false);
            var shr = sh.AddComponent<RectTransform>();
            shr.anchorMin = Vector2.zero; shr.anchorMax = Vector2.one;
            shr.sizeDelta = new Vector2(6, 6);
            shr.anchoredPosition = new Vector2(3, -3);
            sh.AddComponent<Image>().color = new Color(0, 0, 0, 0.45f);

            // Body — 2026-08-25 Kenney 밝은 Depth 스킨 통일(Phase 2). 튜브
            // 자체는 기본/선택 2상태뿐이라(액체 색은 blockSlots가 따로
            // 담당) Depth 스프라이트로 완전히 전환한다.
            tubeBg[i] = UISkin.Apply(go.AddComponent<Image>(), UISkin.DepthButton(UISkin.Accent.Grey));
            tubeBg[i].color = Color.white;

            // Block slots (j=0 bottom)
            blockSlots[i] = new Image[CAP];
            for (int j = 0; j < CAP; j++)
            {
                var slot = new GameObject($"Block{j}");
                slot.transform.SetParent(go.transform, false);
                var sr = slot.AddComponent<RectTransform>();
                sr.anchorMin = new Vector2(0.05f, 0f);
                sr.anchorMax = new Vector2(0.95f, 0f);
                sr.pivot     = new Vector2(0.5f, 0f);
                sr.sizeDelta = new Vector2(0, blockH - 3f);
                sr.anchoredPosition = new Vector2(0, 7f + j * blockH);
                blockSlots[i][j] = UISkin.Apply(slot.AddComponent<Image>(), UISkin.Panel);
            }

            // Glass left highlight
            var gl = new GameObject("Glass");
            gl.transform.SetParent(go.transform, false);
            var glr = gl.AddComponent<RectTransform>();
            glr.anchorMin = new Vector2(0f, .02f);
            glr.anchorMax = new Vector2(0f, .98f);
            glr.pivot     = new Vector2(0f, .5f);
            glr.sizeDelta = new Vector2(tubeW * .09f, 0);
            glr.anchoredPosition = new Vector2(tubeW * .07f, 0);
            gl.AddComponent<Image>().color = new Color(1, 1, 1, .15f);

            // Top open edge
            var edge = new GameObject("Edge");
            edge.transform.SetParent(go.transform, false);
            var er = edge.AddComponent<RectTransform>();
            er.anchorMin = new Vector2(0, 1); er.anchorMax = new Vector2(1, 1);
            er.pivot     = new Vector2(.5f, 1);
            er.sizeDelta = new Vector2(0, 3);
            er.anchoredPosition = Vector2.zero;
            edge.AddComponent<Image>().color = new Color(1, 1, 1, .5f);

            // Button on go
            var btn = go.AddComponent<Button>();
            var cb  = new Button.ButtonClickedEvent();
            cb.AddListener(() => OnTubeTapped(idx));
            btn.onClick = cb;
        }

        RefreshAll();
    }

    void OnTubeTapped(int idx)
    {
        if (cleared) return;

        if (selected == -1)
        {
            if (tubes[idx].Count == 0) return;
            selected = idx;
        }
        else if (selected == idx)
        {
            selected = -1;
        }
        else
        {
            if (CanMove(selected, idx))
            {
                SaveUndo();
                StartCoroutine(DoMoveAnimated(selected, idx));
                return;
            }
            else
            {
                selected = tubes[idx].Count > 0 ? idx : -1;
            }
        }

        UpdateSelectionVisuals();
    }

    IEnumerator DoMoveAnimated(int from, int to)
    {
        DoMove(from, to);
        moves++;
        UpdateMoveText();
        selected = -1;
        RefreshAll();

        // Quick pop scale on destination tube blocks
        if (tubeContainer)
        {
            var destTube = tubeContainer.GetChild(to);
            for (int j = 0; j < CAP; j++)
            {
                if (tubes[to].Count > j)
                    yield return ScalePop(blockSlots[to][j].transform);
            }
        }

        if (CheckWin())
            ShowWin();
    }

    IEnumerator ScalePop(Transform t)
    {
        Vector3 orig = t.localScale;
        t.localScale = orig * 0.85f;
        float dur = 0.12f;
        for (float elapsed = 0; elapsed < dur; elapsed += Time.deltaTime)
        {
            t.localScale = Vector3.Lerp(orig * 0.85f, orig * 1.05f, elapsed / dur);
            yield return null;
        }
        t.localScale = orig;
    }

    bool CanMove(int from, int to)
    {
        if (tubes[from].Count == 0) return false;
        if (tubes[to].Count >= CAP) return false;
        if (tubes[to].Count == 0) return true;
        return TopColor(from) == TopColor(to);
    }

    void DoMove(int from, int to)
    {
        int color = TopColor(from);
        while (tubes[from].Count > 0 && TopColor(from) == color && tubes[to].Count < CAP)
        {
            tubes[to].Add(color);
            tubes[from].RemoveAt(tubes[from].Count - 1);
        }
    }

    int TopColor(int tubeIdx) =>
        tubes[tubeIdx].Count > 0 ? tubes[tubeIdx][tubes[tubeIdx].Count - 1] : 0;

    bool CheckWin()
    {
        foreach (var tube in tubes)
        {
            if (tube.Count == 0) continue;
            if (tube.Count != CAP) return false;
            int first = tube[0];
            foreach (int c in tube)
                if (c != first) return false;
        }
        return true;
    }

    void ShowWin()
    {
        int next = Mathf.Min(level + 1, LevelDatabase.Levels.Length - 1);
        PlayerPrefs.SetInt("CurrentLevel", next);
        PlayerPrefs.Save();
        SaveManager.WriteSave("GameScene");
        cleared = true;
        ui?.ShowOverlay(
            new Color(0.18f, 0.80f, 0.44f),
            "CLEAR!",
            null,
            $"이동 횟수  {moves}",
            "다음 레벨", OnNextLevel,
            "다시하기", OnRestart,
            "타이틀",   OnBack);
    }

    void RefreshAll()
    {
        for (int i = 0; i < tubes.Count; i++)
            RefreshTube(i);
        UpdateSelectionVisuals();
    }

    void RefreshTube(int i)
    {
        for (int j = 0; j < CAP; j++)
        {
            int colorIdx = j < tubes[i].Count ? tubes[i][j] : 0;
            blockSlots[i][j].color = LevelDatabase.Palette[colorIdx];
        }
    }

    void UpdateSelectionVisuals()
    {
        for (int i = 0; i < tubeRects.Length; i++)
        {
            if (tubeRects[i] == null) continue;
            bool sel = i == selected;

            // 선택된 튜브만 위로 30px 들어올리기
            var pos = tubeRects[i].anchoredPosition;
            pos.y = tubeOrigY[i] + (sel ? 30f : 0f);
            tubeRects[i].anchoredPosition = pos;

            // 튜브 배경 스프라이트로 선택 상태 표시 (블록을 가리지 않음).
            // Depth 스프라이트는 색이 구워져 있어 틴트 대신 스프라이트를
            // 상태별로 교체한다(선택=파랑, 기본=회색).
            if (tubeBg[i] != null)
                UISkin.Apply(tubeBg[i], UISkin.DepthButton(sel ? UISkin.Accent.Blue : UISkin.Accent.Grey));
        }
    }

    void UpdateMoveText()
    {
        if (moveText) moveText.text = moves.ToString();
    }

    void SaveUndo()
    {
        var copy = new List<List<int>>(tubes.Count);
        foreach (var t in tubes) copy.Add(new List<int>(t));
        undoStack.Push(new UndoState(copy, moves));
    }

    // ── Public button handlers ──────────────────────────

    public void OnUndo()
    {
        if (undoStack.Count == 0) return;
        var s = undoStack.Pop();
        tubes   = s.tubes;
        moves   = s.moves;
        selected = -1;
        UpdateMoveText();
        RefreshAll();
    }

    public void OnRestart() => LoadLevel(level);

    public void OnNextLevel() => LoadLevel(level + 1);

    public void OnBack() => SceneManager.LoadScene("TitleScene");

    public void OnHint()
    {
        // Show first valid move hint
        for (int f = 0; f < tubes.Count; f++)
        {
            if (tubes[f].Count == 0) continue;
            for (int t = 0; t < tubes.Count; t++)
            {
                if (t == f) continue;
                if (CanMove(f, t))
                {
                    selected = f;
                    UpdateSelectionVisuals();
                    return;
                }
            }
        }
    }

    class UndoState
    {
        public readonly List<List<int>> tubes;
        public readonly int moves;
        public UndoState(List<List<int>> t, int m) { tubes = t; moves = m; }
    }
}
