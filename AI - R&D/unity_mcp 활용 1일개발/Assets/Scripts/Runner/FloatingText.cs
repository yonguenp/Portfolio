using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 월드 공간에 떠오르는 점수/콤보 텍스트 (오브젝트 풀링).
/// </summary>
public class FloatingText : MonoBehaviour
{
    private TextMeshPro _tmp;
    private float       _elapsed;
    private float       _lifetime = 1.1f;
    private Vector3     _vel;
    private Color       _col;

    private static readonly List<FloatingText> Pool = new List<FloatingText>();

    // ── Public ───────────────────────────────────────────
    public static void Show(string text, Vector3 worldPos, Color color, float size = 1.8f)
    {
        var ft = Get();
        ft.transform.position = worldPos + Vector3.up * 0.3f;
        ft._tmp.text      = text;
        ft._tmp.fontSize  = size;
        ft._tmp.color     = color;
        ft._col           = color;
        ft._elapsed       = 0f;
        ft._vel           = new Vector3(Random.Range(-0.4f, 0.4f), 3.5f, 0f);
        ft._tmp.sortingOrder = 20;
        ft.gameObject.SetActive(true);
    }

    // ── Pool ─────────────────────────────────────────────
    static FloatingText Get()
    {
        Pool.RemoveAll(ft => ft == null);
        foreach (var ft in Pool)
            if (ft != null && !ft.gameObject.activeSelf) return ft;

        var go  = new GameObject("FT");
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.alignment  = TextAlignmentOptions.Center;
        tmp.fontStyle  = FontStyles.Bold;
        var ft2 = go.AddComponent<FloatingText>();
        ft2._tmp = tmp;
        Pool.Add(ft2);
        return ft2;
    }

    void Awake() { _tmp = GetComponent<TextMeshPro>(); }

    void Update()
    {
        _elapsed += Time.deltaTime;
        if (_elapsed >= _lifetime) { gameObject.SetActive(false); return; }

        // 중력 감속
        _vel.y -= 4f * Time.deltaTime;
        transform.position += _vel * Time.deltaTime;

        // 알파: 0→0.2s 페이드인, 0.2s→1.1s 페이드아웃
        float t = _elapsed / _lifetime;
        float a = t < 0.18f ? t / 0.18f : 1f - ((t - 0.18f) / 0.82f);
        _tmp.color = new Color(_col.r, _col.g, _col.b, Mathf.Clamp01(a));

        // 약간 팝 스케일
        float sc = 1f + Mathf.Sin(t * Mathf.PI) * 0.25f;
        transform.localScale = Vector3.one * sc;
    }
}
