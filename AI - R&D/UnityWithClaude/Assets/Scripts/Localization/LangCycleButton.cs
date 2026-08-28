using UnityEngine;
using TMPro;

public class LangCycleButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;

    static readonly string[] Codes = { "KO", "EN", "JA" };

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

    void Refresh()
    {
        if (label && LocalizationManager.Instance)
            label.text = Codes[(int)LocalizationManager.Instance.CurrentLanguage];
    }
}
