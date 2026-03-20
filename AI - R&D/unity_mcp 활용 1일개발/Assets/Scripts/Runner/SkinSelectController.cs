using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 캐릭터 선택 스크롤 — Kenney 스프라이트 캐릭터 4종.
/// Spine 의존성 없음. SpriteAnimator 기반.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class SkinSelectController : MonoBehaviour
{
    [Header("레이아웃")]
    public float cellHeight = 210f;
    public float cellPad    = 6f;
    public int   poolSize   = 6;

    // ── 캐릭터 정의 ──────────────────────────────────────────
    public struct CharDef
    {
        public string folder;   // Resources/Characters/{folder}/
        public string display;  // 한국어 이름
        public CharDef(string f, string d) { folder = f; display = d; }
    }

    public static readonly CharDef[] CHARACTERS = new CharDef[]
    {
        new CharDef("Adventurer", "모험가"),
        new CharDef("Zombie",     "좀비"),
    };

    // ── 내부 ─────────────────────────────────────────────────
    private ScrollRect    _sr;
    private RectTransform _content;
    private TMP_FontAsset _font;
    private List<SkinCell> _pool = new List<SkinCell>();
    private int _lastFirstIdx = -9999;

    void Awake()
    {
        _sr      = GetComponent<ScrollRect>();
        _content = _sr.content;
        _font    = Resources.Load<TMP_FontAsset>("Font/ONE Mobile POP SDF");
        // 폴백: TMP 기본 폰트 (한글 없어도 레이아웃은 유지)
        if (_font == null) _font = TMP_Settings.defaultFontAsset;

        // LayoutGroup / SizeFitter 제거 (수동 배치)
        var vlg = _content.GetComponent<VerticalLayoutGroup>();
        if (vlg) Destroy(vlg);
        var csf = _content.GetComponent<ContentSizeFitter>();
        if (csf) Destroy(csf);

        float totalH = CHARACTERS.Length * (cellHeight + cellPad) + cellPad + 20f;
        _content.sizeDelta = new Vector2(0f, totalH);

        int count = Mathf.Min(poolSize, CHARACTERS.Length);
        for (int i = 0; i < count; i++)
        {
            var go   = new GameObject("SkinCell_" + i);
            go.transform.SetParent(_content, false);
            var cell = go.AddComponent<SkinCell>();
            cell.Build(_font);
            cell.gameObject.SetActive(false);
            _pool.Add(cell);
        }
    }

    void Start()
    {
        _lastFirstIdx = -9999;
        UpdateCells();
    }

    void Update() => UpdateCells();

    void UpdateCells()
    {
        if (_content == null) return;

        float scrollY  = Mathf.Max(0f, _content.anchoredPosition.y);
        float stride   = cellHeight + cellPad;
        int   firstIdx = Mathf.Max(0, Mathf.FloorToInt(scrollY / stride) - 1);

        if (firstIdx == _lastFirstIdx) return;
        _lastFirstIdx = firstIdx;

        for (int i = 0; i < _pool.Count; i++)
        {
            int ci   = firstIdx + i;
            var cell = _pool[i];

            if (ci < 0 || ci >= CHARACTERS.Length)
            {
                cell.gameObject.SetActive(false);
                continue;
            }

            cell.gameObject.SetActive(true);
            var ch = CHARACTERS[ci];
            cell.Setup(ch.folder, ch.display, ci, cellHeight, cellPad, this);
        }
    }

    public void ResetScroll()
    {
        if (_content) _content.anchoredPosition = Vector2.zero;
        _lastFirstIdx = -9999;
        UpdateCells();
    }

    /// <summary>SkinCell 버튼 클릭 → folderName(예: "Adventurer") 으로 게임 시작.</summary>
    public void SelectSkin(string folderName)
    {
        AudioManager.Instance?.PlayButton();
        RunnerGameManager.StartGameWithSkin(folderName);
    }
}
