using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public enum Language { Korean, English, Japanese }

    public Language CurrentLanguage { get; private set; } = Language.Korean;
    public event Action OnLanguageChanged;

    static readonly string[] ColumnNames = { "ko", "en", "ja" };

    Dictionary<string, string[]> table = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadCSV();
        var saved = PlayerPrefs.GetString("Language", "Korean");
        if (Enum.TryParse<Language>(saved, out var lang)) CurrentLanguage = lang;
    }

    void LoadCSV()
    {
        var asset = Resources.Load<TextAsset>("Localization/strings");
        if (asset == null) { Debug.LogError("[Loc] strings.csv not found"); return; }

        var lines = asset.text.Split('\n');
        if (lines.Length < 2) return;

        var headers = lines[0].Trim().Split(',');
        int koIdx = Array.IndexOf(headers, "ko");
        int enIdx = Array.IndexOf(headers, "en");
        int jaIdx = Array.IndexOf(headers, "ja");

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            var cols = line.Split(',');
            if (cols.Length == 0) continue;
            var key = cols[0].Trim();
            table[key] = new[]
            {
                GetCol(cols, koIdx),
                GetCol(cols, enIdx),
                GetCol(cols, jaIdx),
            };
        }
    }

    static string GetCol(string[] cols, int idx) =>
        idx >= 0 && idx < cols.Length ? cols[idx].Trim() : "";

    public string Get(string key)
    {
        if (!table.TryGetValue(key, out var vals)) return key;
        int langIdx = (int)CurrentLanguage;
        if (langIdx < vals.Length && !string.IsNullOrEmpty(vals[langIdx]))
            return vals[langIdx];
        return vals.Length > 0 ? vals[0] : key;
    }

    /// <summary>
    /// 키가 없으면 fallback을 쓴다.
    /// <see cref="Get"/>는 키가 없을 때 **키 문자열을 그대로 돌려주므로**
    /// `Get(k) ?? "기본값"` 은 절대 안 걸리고 화면에 키가 그대로 찍힌다.
    /// </summary>
    public string GetOr(string key, string fallback)
    {
        var v = Get(key);
        return string.IsNullOrEmpty(v) || v == key ? fallback : v;
    }

    public void SetLanguage(Language lang)
    {
        if (CurrentLanguage == lang) return;
        CurrentLanguage = lang;
        PlayerPrefs.SetString("Language", lang.ToString());
        OnLanguageChanged?.Invoke();
    }

    public void CycleLanguage()
    {
        int next = ((int)CurrentLanguage + 1) % 3;
        SetLanguage((Language)next);
    }
}
