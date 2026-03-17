using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class SettingLanguageLayer : MonoBehaviour
    {
        // 언어 관련
        [Space(10)]
        [Header("Language")]
        [SerializeField] GameObject korSelectedObject = null;
        [SerializeField] GameObject engSelectedObject = null;
        [SerializeField] GameObject japSelectedObject = null;

        private void Awake()
        {
            RefreshToggleUI();
        }
        public void Init()
        {
            RefreshToggleUI();
        }

        void RefreshToggleUI()
        {
            ClearButtonState();

            korSelectedObject?.SetActive(GamePreference.Instance.GameLanguage == SystemLanguage.Korean);
            engSelectedObject?.SetActive(GamePreference.Instance.GameLanguage == SystemLanguage.English);
            japSelectedObject?.SetActive(GamePreference.Instance.GameLanguage == SystemLanguage.Japanese);
        }

        public void OnChangeLanguageState(int selectLanguage) 
        {
            ClearButtonState();

            korSelectedObject?.SetActive((SystemLanguage)selectLanguage == SystemLanguage.Korean);
            engSelectedObject?.SetActive((SystemLanguage)selectLanguage == SystemLanguage.English);
            japSelectedObject?.SetActive((SystemLanguage)selectLanguage == SystemLanguage.Japanese);

            GamePreference.Instance.GameLanguage = (SystemLanguage)selectLanguage;

            RefreshToggleUI();

            SettingEvent.RefreshString();
        }

        public void ClearButtonState()
        {
            korSelectedObject?.SetActive(false);
            engSelectedObject?.SetActive(false);
            japSelectedObject?.SetActive(false);
        }
    }
}

