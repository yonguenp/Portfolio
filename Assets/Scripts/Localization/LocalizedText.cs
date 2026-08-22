using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] string key;

    TMP_Text label;

    void Awake() => label = GetComponent<TMP_Text>();

    void OnEnable()
    {
        if (LocalizationManager.Instance)
            LocalizationManager.Instance.OnLanguageChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance)
            LocalizationManager.Instance.OnLanguageChanged -= Refresh;
    }

    public void SetKey(string k) { key = k; Refresh(); }

    void Refresh()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (LocalizationManager.Instance != null)
            label.text = LocalizationManager.Instance.Get(key);
    }
}
