using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Game1010 : MonoBehaviour
{
    [SerializeField] Transform boardContainer;
    [SerializeField] Transform pieceContainer;
    [SerializeField] GameUIManager ui;

    const int BOARD = 10;
    const int PIECE_SLOTS = 3;
    const string BestKey = "Best1010";

    static readonly int[][][] Shapes =
    {
        new[]{ new[]{0,0} },
        new[]{ new[]{0,0}, new[]{0,1} },
        new[]{ new[]{0,0}, new[]{0,1}, new[]{0,2} },
        new[]{ new[]{0,0}, new[]{0,1}, new[]{0,2}, new[]{0,3} },
        new[]{ new[]{0,0}, new[]{0,1}, new[]{0,2}, new[]{0,3}, new[]{0,4} },
        new[]{ new[]{0,0}, new[]{1,0} },
        new[]{ new[]{0,0}, new[]{1,0}, new[]{2,0} },
        new[]{ new[]{0,0}, new[]{1,0}, new[]{2,0}, new[]{3,0} },
        new[]{ new[]{0,0}, new[]{1,0}, new[]{2,0}, new[]{3,0}, new[]{4,0} },
        new[]{ new[]{0,0}, new[]{0,1}, new[]{1,0}, new[]{1,1} },
        new[]{ new[]{0,0}, new[]{0,1}, new[]{0,2}, new[]{1,0}, new[]{1,1}, new[]{1,2}, new[]{2,0}, new[]{2,1}, new[]{2,2} },
        new[]{ new[]{0,0}, new[]{1,0}, new[]{1,1} },
        new[]{ new[]{0,1}, new[]{1,0}, new[]{1,1} },
        new[]{ new[]{0,0}, new[]{0,1}, new[]{1,0} },
        new[]{ new[]{0,0}, new[]{0,1}, new[]{1,1} },
        new[]{ new[]{0,0}, new[]{0,1}, new[]{0,2}, new[]{1,2} },
        new[]{ new[]{0,0}, new[]{0,1}, new[]{0,2}, new[]{1,0} },
    };

    static readonly Color[] PieceColors =
    {
        new Color(.93f,.25f,.30f), new Color(.20f,.50f,.95f), new Color(.18f,.80f,.38f),
        new Color(.98f,.82f,.10f), new Color(.70f,.20f,.92f), new Color(.97f,.55f,.12f),
        new Color(.20f,.85f,.85f),
    };

    readonly bool[,] occupied  = new bool[BOARD, BOARD];
    readonly Color[,] cellColor = new Color[BOARD, BOARD];
    Image[,] boardCells;
    int[][][] currentShapes = new int[PIECE_SLOTS][][];
    Color[] currentColors   = new Color[PIECE_SLOTS];
    bool[]  pieceUsed       = new bool[PIECE_SLOTS];
    Image[][] piecePreviewCells;
    Image[]   slotBg;
    int selectedPiece = -1;
    int score, bestScore;

    void Start()
    {
        BuildBoard();
        BuildPieceSlots();
        NewGame();
    }

    void BuildBoard()
    {
        boardCells = new Image[BOARD, BOARD];
        for (int r = 0; r < BOARD; r++)
        for (int c = 0; c < BOARD; c++)
        {
            int row = r, col = c;
            var go = new GameObject($"C{r}{c}");
            go.transform.SetParent(boardContainer, false);
            boardCells[r, c] = UISkin.Apply(go.AddComponent<Image>(), UISkin.Panel);
            boardCells[r, c].color = new Color(.12f, .16f, .34f);
            go.AddComponent<Button>().onClick.AddListener(() => OnBoardTap(row, col));
        }
    }

    void BuildPieceSlots()
    {
        slotBg = new Image[PIECE_SLOTS];
        piecePreviewCells = new Image[PIECE_SLOTS][];

        // 세로 스택: 슬롯 3개를 위에서 아래로 배치
        float slotH = 240f, slotW = 400f, slotGap = 30f;
        float startY = (PIECE_SLOTS - 1) * (slotH + slotGap) / 2f;

        for (int i = 0; i < PIECE_SLOTS; i++)
        {
            int idx = i;
            var slotGo = new GameObject($"Slot{i}");
            slotGo.transform.SetParent(pieceContainer, false);
            var slotRT = slotGo.AddComponent<RectTransform>();
            slotRT.anchorMin = slotRT.anchorMax = new Vector2(.5f, .5f);
            slotRT.sizeDelta = new Vector2(slotW, slotH);
            slotRT.anchoredPosition = new Vector2(0, startY - i * (slotH + slotGap));
            slotBg[i] = UISkin.Apply(slotGo.AddComponent<Image>(), UISkin.Panel);
            slotBg[i].color = new Color(.10f, .14f, .30f);
            slotGo.AddComponent<Button>().onClick.AddListener(() => OnPieceTap(idx));

            var pvGo = new GameObject("Preview");
            pvGo.transform.SetParent(slotGo.transform, false);
            var pvRT = pvGo.AddComponent<RectTransform>();
            pvRT.anchorMin = pvRT.anchorMax = new Vector2(.5f, .5f);
            pvRT.sizeDelta = new Vector2(200, 200);
            pvRT.anchoredPosition = Vector2.zero;

            var glg = pvGo.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(36, 36);
            glg.spacing  = new Vector2(4, 4);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 5;
            glg.childAlignment = TextAnchor.MiddleCenter;

            piecePreviewCells[i] = new Image[25];
            for (int j = 0; j < 25; j++)
            {
                var cell = new GameObject($"P{j}");
                cell.transform.SetParent(pvGo.transform, false);
                piecePreviewCells[i][j] = UISkin.Apply(cell.AddComponent<Image>(), UISkin.Panel);
                piecePreviewCells[i][j].color = new Color(0, 0, 0, 0);
            }
        }
    }

    void NewGame()
    {
        score = 0;
        bestScore = PlayerPrefs.GetInt(BestKey, 0);
        selectedPiece = -1;
        for (int r = 0; r < BOARD; r++) for (int c = 0; c < BOARD; c++) { occupied[r,c] = false; cellColor[r,c] = Color.clear; }
        ui?.HideOverlay();
        ui?.SetTitle("1010!");
        UpdateScore();
        SpawnPieces();
        RefreshBoard();
    }

    void SpawnPieces()
    {
        for (int i = 0; i < PIECE_SLOTS; i++)
        {
            currentShapes[i] = Shapes[Random.Range(0, Shapes.Length)];
            currentColors[i] = PieceColors[Random.Range(0, PieceColors.Length)];
            pieceUsed[i] = false;
        }
        selectedPiece = -1;
        RefreshPieceSlots();
    }

    void OnPieceTap(int idx)
    {
        if (pieceUsed[idx]) return;
        selectedPiece = selectedPiece == idx ? -1 : idx;
        RefreshPieceSlots();
    }

    void OnBoardTap(int row, int col)
    {
        if (selectedPiece < 0 || pieceUsed[selectedPiece]) return;
        if (!CanPlace(currentShapes[selectedPiece], row, col)) return;

        Place(currentShapes[selectedPiece], row, col, currentColors[selectedPiece]);
        score += currentShapes[selectedPiece].Length;
        pieceUsed[selectedPiece] = true;
        selectedPiece = -1;
        StartCoroutine(ClearLinesAndUpdate());
    }

    IEnumerator ClearLinesAndUpdate()
    {
        RefreshBoard();
        RefreshPieceSlots();

        var clearRows = new List<int>();
        var clearCols = new List<int>();
        for (int r = 0; r < BOARD; r++) { bool full = true; for (int c = 0; c < BOARD; c++) if (!occupied[r,c]) { full = false; break; } if (full) clearRows.Add(r); }
        for (int c = 0; c < BOARD; c++) { bool full = true; for (int r = 0; r < BOARD; r++) if (!occupied[r,c]) { full = false; break; } if (full) clearCols.Add(c); }

        if (clearRows.Count + clearCols.Count > 0)
        {
            foreach (int r in clearRows) for (int c = 0; c < BOARD; c++) boardCells[r,c].color = Color.white;
            foreach (int c in clearCols) for (int r = 0; r < BOARD; r++) boardCells[r,c].color = Color.white;
            yield return new WaitForSeconds(0.18f);

            foreach (int r in clearRows) for (int c = 0; c < BOARD; c++) { occupied[r,c] = false; cellColor[r,c] = Color.clear; }
            foreach (int c in clearCols) for (int r = 0; r < BOARD; r++) { occupied[r,c] = false; cellColor[r,c] = Color.clear; }

            int lines = clearRows.Count + clearCols.Count;
            score += lines * 10 * lines;
        }

        UpdateScore();
        RefreshBoard();

        if (pieceUsed[0] && pieceUsed[1] && pieceUsed[2]) SpawnPieces();

        if (IsGameOver())
            ui?.ShowOverlay(new Color(.93f,.25f,.30f), "게임 오버",
                score.ToString(), null,
                "다시 시작", OnRestart,
                "타이틀", OnBack);
    }

    bool CanPlace(int[][] shape, int row, int col)
    {
        foreach (var off in shape)
        {
            int r = row + off[0], c = col + off[1];
            if (r < 0 || r >= BOARD || c < 0 || c >= BOARD || occupied[r, c]) return false;
        }
        return true;
    }

    void Place(int[][] shape, int row, int col, Color color)
    {
        foreach (var off in shape) { int r = row+off[0], c = col+off[1]; occupied[r,c] = true; cellColor[r,c] = color; }
    }

    bool IsGameOver()
    {
        for (int i = 0; i < PIECE_SLOTS; i++)
        {
            if (pieceUsed[i]) continue;
            for (int r = 0; r < BOARD; r++) for (int c = 0; c < BOARD; c++) if (CanPlace(currentShapes[i], r, c)) return false;
        }
        return true;
    }

    void RefreshBoard()
    {
        for (int r = 0; r < BOARD; r++) for (int c = 0; c < BOARD; c++)
            boardCells[r,c].color = occupied[r,c] ? cellColor[r,c] : new Color(.12f,.16f,.34f);
    }

    void RefreshPieceSlots()
    {
        for (int i = 0; i < PIECE_SLOTS; i++)
        {
            bool sel = i == selectedPiece, used = pieceUsed[i];
            slotBg[i].color = used ? new Color(.08f,.10f,.22f) : sel ? new Color(.20f,.28f,.55f) : new Color(.10f,.14f,.30f);
            for (int j = 0; j < 25; j++) piecePreviewCells[i][j].color = new Color(0,0,0,0);
            if (used) continue;
            foreach (var off in currentShapes[i])
            {
                int r = off[0], c = off[1];
                if (r < 5 && c < 5) piecePreviewCells[i][r * 5 + c].color = currentColors[i];
            }
        }
    }

    void UpdateScore()
    {
        if (score > bestScore) { bestScore = score; PlayerPrefs.SetInt(BestKey, bestScore); PlayerPrefs.Save(); }
        ui?.SetScore(score);
        ui?.SetBest(bestScore);
    }

    public void OnRestart() => NewGame();
    public void OnBack()    => SceneManager.LoadScene("TitleScene");
}
