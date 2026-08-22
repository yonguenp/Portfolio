using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class Game2048 : MonoBehaviour
{
    [SerializeField] Transform boardContainer;
    [SerializeField] GameUIManager ui;

    const int SIZE = 4;
    const string BestKey = "Best2048";

    readonly int[,] board = new int[SIZE, SIZE];
    Image[,] tileBg;
    TextMeshProUGUI[,] tileLabel;
    int score;
    bool won, over;
    Vector2 swipeStart;
    bool pressing;

    static readonly Color[] TileColors =
    {
        new Color(.17f,.19f,.32f),
        new Color(.93f,.89f,.85f),
        new Color(.93f,.87f,.78f),
        new Color(.95f,.69f,.47f),
        new Color(.96f,.58f,.39f),
        new Color(.96f,.49f,.37f),
        new Color(.96f,.37f,.23f),
        new Color(.93f,.82f,.45f),
        new Color(.93f,.80f,.38f),
        new Color(.93f,.78f,.31f),
        new Color(.93f,.76f,.24f),
        new Color(.93f,.73f,.18f),
        new Color(.14f,.12f,.28f),
    };

    void Start()
    {
        // 공용 UI(GameUI 프리팹)는 씬 스크립트를 직렬화 참조할 수 없으므로
        // 버튼 동작은 런타임에 등록한다. Back은 기본 동작(TitleScene)을 그대로 쓴다.
        ui?.SetNewGameAction(OnNewGame);

        BuildBoard();
        NewGame();
    }

    void BuildBoard()
    {
        tileBg    = new Image[SIZE, SIZE];
        tileLabel = new TextMeshProUGUI[SIZE, SIZE];
        var fnt = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");

        for (int r = 0; r < SIZE; r++)
        for (int c = 0; c < SIZE; c++)
        {
            var go = new GameObject($"T{r}{c}");
            go.transform.SetParent(boardContainer, false);
            tileBg[r, c] = UISkin.Apply(go.AddComponent<Image>(), UISkin.Panel);
            tileBg[r, c].color = TileColors[0];

            var tg = new GameObject("L");
            tg.transform.SetParent(go.transform, false);
            var tr = tg.AddComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.sizeDelta = Vector2.zero;
            tileLabel[r, c] = tg.AddComponent<TextMeshProUGUI>();
            tileLabel[r, c].alignment = TextAlignmentOptions.Center;
            tileLabel[r, c].fontStyle = FontStyles.Bold;
            tileLabel[r, c].color     = new Color(.1f, .1f, .1f);
            if (fnt) tileLabel[r, c].font = fnt;
        }
    }

    void NewGame()
    {
        score = 0; won = false; over = false;
        for (int r = 0; r < SIZE; r++) for (int c = 0; c < SIZE; c++) board[r, c] = 0;
        ui?.HideOverlay();
        ui?.SetTitle("2048");
        SpawnTile(); SpawnTile(); RefreshUI();
    }

    void Update()
    {
        if (over) return;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.upArrowKey.wasPressedThisFrame    || kb.wKey.wasPressedThisFrame) TryMove(0);
            if (kb.downArrowKey.wasPressedThisFrame  || kb.sKey.wasPressedThisFrame) TryMove(1);
            if (kb.leftArrowKey.wasPressedThisFrame  || kb.aKey.wasPressedThisFrame) TryMove(2);
            if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) TryMove(3);
        }

        var ms = Mouse.current;
        if (ms != null)
        {
            if (ms.leftButton.wasPressedThisFrame)  { swipeStart = ms.position.ReadValue(); pressing = true; }
            if (ms.leftButton.wasReleasedThisFrame && pressing) { pressing = false; ProcessSwipe(ms.position.ReadValue() - swipeStart); }
        }

        var ts = Touchscreen.current;
        if (ts != null)
        {
            if (ts.primaryTouch.press.wasPressedThisFrame)  { swipeStart = ts.primaryTouch.position.ReadValue(); pressing = true; }
            if (ts.primaryTouch.press.wasReleasedThisFrame && pressing) { pressing = false; ProcessSwipe(ts.primaryTouch.position.ReadValue() - swipeStart); }
        }
    }

    void ProcessSwipe(Vector2 delta)
    {
        if (delta.magnitude < 30) return;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            TryMove(delta.x > 0 ? 3 : 2);
        else
            TryMove(delta.y > 0 ? 0 : 1);
    }

    void TryMove(int dir)
    {
        if (over || won) return;
        bool moved = false;

        if (dir == 2 || dir == 3)
        {
            for (int r = 0; r < SIZE; r++)
            {
                var row = GetRow(r);
                if (dir == 3) System.Array.Reverse(row);
                if (SlideLeft(row, out var result)) moved = true;
                if (dir == 3) System.Array.Reverse(result);
                SetRow(r, result);
            }
        }
        else
        {
            for (int c = 0; c < SIZE; c++)
            {
                var col = GetCol(c);
                if (dir == 1) System.Array.Reverse(col);
                if (SlideLeft(col, out var result)) moved = true;
                if (dir == 1) System.Array.Reverse(result);
                SetCol(c, result);
            }
        }

        if (moved) { SpawnTile(); RefreshUI(); if (IsOver()) ShowOver(); }
    }

    bool SlideLeft(int[] line, out int[] result)
    {
        result = new int[SIZE];
        var nums = new List<int>();
        foreach (int v in line) if (v > 0) nums.Add(v);

        bool changed = nums.Count != line.Count(v => v > 0);
        for (int i = 0; i < nums.Count - 1; i++)
        {
            if (nums[i] == nums[i + 1])
            {
                nums[i] *= 2;
                score += nums[i];
                if (nums[i] == 2048 && !won)
                {
                    won = true;
                    ui?.ShowOverlay(new Color(.93f,.73f,.18f), "2048!",
                        score.ToString(), "도달했습니다!",
                        "계속", OnContinue,
                        "새 게임", OnNewGame);
                }
                nums.RemoveAt(i + 1);
                changed = true;
            }
        }
        for (int i = 0; i < nums.Count; i++) result[i] = nums[i];
        for (int i = 0; i < SIZE; i++) if (result[i] != line[i]) changed = true;
        return changed;
    }

    void SpawnTile()
    {
        var empty = new List<(int, int)>();
        for (int r = 0; r < SIZE; r++) for (int c = 0; c < SIZE; c++) if (board[r, c] == 0) empty.Add((r, c));
        if (empty.Count == 0) return;
        var pos = empty[Random.Range(0, empty.Count)];
        board[pos.Item1, pos.Item2] = Random.value < 0.9f ? 2 : 4;
    }

    bool IsOver()
    {
        for (int r = 0; r < SIZE; r++) for (int c = 0; c < SIZE; c++)
        {
            if (board[r, c] == 0) return false;
            if (r + 1 < SIZE && board[r, c] == board[r + 1, c]) return false;
            if (c + 1 < SIZE && board[r, c] == board[r, c + 1]) return false;
        }
        return true;
    }

    void ShowOver()
    {
        over = true;
        ui?.ShowOverlay(new Color(.93f,.25f,.30f), "게임 오버",
            score.ToString(), null,
            "다시 시작", OnNewGame,
            "타이틀", OnBack);
    }

    void RefreshUI()
    {
        int best = Mathf.Max(PlayerPrefs.GetInt(BestKey, 0), score);
        PlayerPrefs.SetInt(BestKey, best); PlayerPrefs.Save();
        ui?.SetScore(score);
        ui?.SetBest(best);

        for (int r = 0; r < SIZE; r++) for (int c = 0; c < SIZE; c++)
        {
            int v = board[r, c];
            tileBg[r, c].color = TileColor(v);
            tileLabel[r, c].text = v > 0 ? v.ToString() : "";
            tileLabel[r, c].fontSize = v >= 1024 ? 28f : v >= 128 ? 36f : 44f;
            tileLabel[r, c].color = v <= 4 ? new Color(.1f,.1f,.1f) : Color.white;
        }
    }

    int[] GetRow(int r) { var a = new int[SIZE]; for (int c = 0; c < SIZE; c++) a[c] = board[r, c]; return a; }
    void  SetRow(int r, int[] a) { for (int c = 0; c < SIZE; c++) board[r, c] = a[c]; }
    int[] GetCol(int c) { var a = new int[SIZE]; for (int r = 0; r < SIZE; r++) a[r] = board[r, c]; return a; }
    void  SetCol(int c, int[] a) { for (int r = 0; r < SIZE; r++) board[r, c] = a[r]; }

    static Color TileColor(int v)
    {
        int idx = v == 0 ? 0 : Mathf.Clamp(Mathf.RoundToInt(Mathf.Log(v, 2)), 1, TileColors.Length - 1);
        return TileColors[idx];
    }

    public void OnNewGame()  => NewGame();
    public void OnContinue() { won = false; ui?.HideOverlay(); }
    public void OnBack()     => SceneManager.LoadScene("TitleScene");
}
