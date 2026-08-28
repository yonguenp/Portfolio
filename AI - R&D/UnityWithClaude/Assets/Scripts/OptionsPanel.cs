using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsPanel : MonoBehaviour
{
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] TextMeshProUGUI languageLabel;

    const string BgmKey = "Vol_BGM";
    const string SfxKey = "Vol_SFX";

    void Awake()
    {
        bgmSlider?.onValueChanged.AddListener(v => PlayerPrefs.SetFloat(BgmKey, v));
        sfxSlider?.onValueChanged.AddListener(v => PlayerPrefs.SetFloat(SfxKey, v));
    }

    void OnEnable()
    {
        if (bgmSlider) bgmSlider.value = PlayerPrefs.GetFloat(BgmKey, 1f);
        if (sfxSlider) sfxSlider.value = PlayerPrefs.GetFloat(SfxKey, 1f);
        RefreshLanguageLabel();
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += RefreshLanguageLabel;
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= RefreshLanguageLabel;
    }

    public void OnCycleLanguage()
    {
        LocalizationManager.Instance?.CycleLanguage();
    }

    void RefreshLanguageLabel()
    {
        if (!languageLabel || LocalizationManager.Instance == null) return;
        var lang = LocalizationManager.Instance.CurrentLanguage;
        var key = lang == LocalizationManager.Language.Korean ? "lang_korean"
                : lang == LocalizationManager.Language.English ? "lang_english"
                : "lang_japanese";
        languageLabel.text = LocalizationManager.Instance.Get(key);
    }

    public void OnClose()
    {
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }
}
