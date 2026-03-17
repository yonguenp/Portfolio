using EasyMobile;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class SettingGameLayer : MonoBehaviour
    {
        [SerializeField] Text versionText = null;

        [Header("[Language]")]
        [SerializeField] Button LanguageButtonSample = null;

        [Header("[Sound]")]
        [SerializeField] Slider bgmSlider = null;
        [SerializeField] Slider sfxSlider = null;

        [Header("[Graphic]")]
        [SerializeField] Slider townDragonSlider = null;

        [Header("[Push]")]
        [SerializeField] Toggle pushToggle = null;
        [Header("[DAPP]")]
        [SerializeField] GameObject dappButton = null;
        public void Init()
        {
            versionText.text = "ClientVer. " + GamePreference.Instance.VERSION + "/" + ClientConstants.CurrentPlatform + "-" + NetworkManager.ServerName;

            // 사운드 관련
            var bgmData = GamePreference.Instance.GetBGMData();
            bgmSlider.value = bgmData.volume;

            var sfxData = GamePreference.Instance.GetSFXData();
            sfxSlider.value = sfxData.volume;       // 순서 중요 -> 슬라이더먼저

            // 언어 관련
            OnClickLanguageButton((int)GamePreference.Instance.GameLanguage);
            pushToggle.isOn = Notifications.DataPrivacyConsent == ConsentStatus.Granted;
            // 타운 드래곤 조절 관련
            if (PlayerPrefs.HasKey("Setting_Toy"))
            {
                var toyData = JsonConvert.DeserializeObject<JObject>(PlayerPrefs.GetString("Setting_Toy"));
                if (toyData != null)
                {
                    if(SBFunc.IsJTokenCheck(toyData["value"])) townDragonSlider.value = toyData["value"].Value<float>();
                }
                else
                {
                    townDragonSlider.value = 1.0f;
                }
            }
            else
            {
                townDragonSlider.value = 1.0f;
            }

//            prtLanguageButton.gameObject.SetActive(false);
//#if DEBUG
//            prtLanguageButton.gameObject.SetActive(true);
//#endif

            if(dappButton != null)
            {
                dappButton.SetActive(/*User.Instance.ENABLE_P2E*/ false);
            }
        }

        public void OnClickLanguageButton(int LanguageType)
        {
//#if !UNITY_EDITOR
            if (GamePreference.Instance.GameLanguage != (SystemLanguage)LanguageType)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("언어변경"), StringData.GetStringByStrKey("언어변경메시지"), () =>
                {
                    GamePreference.Instance.GameLanguage = (SystemLanguage)LanguageType;
                    UIManager.Instance.InitUI(eUIType.None);
                    LoadingManager.Instance.LoadStartScene();
                    PopupManager.AllClosePopup();
                    return;
                }, () => {

                });

                return;
            }
//#endif

            LanguageButtonSample.gameObject.SetActive(true);
            foreach(Transform child in LanguageButtonSample.transform.parent)
            {
                if (child == LanguageButtonSample.transform)
                    continue;

                Destroy(child.gameObject);
            }

            foreach (var data in TableManager.GetTable<LanguageTable>().GetAllList())
            {
                GameObject langButton = Instantiate(LanguageButtonSample.gameObject, LanguageButtonSample.transform.parent);
                langButton.GetComponent<Button>().SetButtonSpriteState(data.Lang == (SystemLanguage)LanguageType);
                langButton.GetComponent<Button>().onClick.AddListener(() => {
                    OnClickLanguageButton((int)data.Lang);
                });

                langButton.GetComponentInChildren<Text>().text = data.NAME;
            }
            LanguageButtonSample.gameObject.SetActive(false);

            if (GamePreference.Instance.GameLanguage != (SystemLanguage)LanguageType)
            {
                GamePreference.Instance.GameLanguage = (SystemLanguage)LanguageType;
                SettingEvent.RefreshString();
            }
        }

        public void OnClickGameGuideLink()
        {
            Application.OpenURL(GameConfigTable.GetGameGuideURL());
        }
        
        public void OnClickDAppLink()
        {
            if(User.Instance.ENABLE_P2E == false)
            {
                ToastManager.On(StringData.GetStringByIndex(100000636));
                return;
            }
            
            DAppManager.Instance.OpenDAppWebView();
        }

        public void OnClickPushToggle()
        {
            bool push = pushToggle.isOn;
            if (push)
            {
                if (Notifications.DataPrivacyConsent != ConsentStatus.Granted)
                {
                    Notifications.GrantDataPrivacyConsent();
                    Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
                }
            }
            else
            {
                if (Notifications.DataPrivacyConsent == ConsentStatus.Granted)
                {
                    Notifications.RevokeDataPrivacyConsent();
                    Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = false;
                }
            }
        }

        public void OnBGMValueChanged()
        {
            if (bgmSlider == null) return;

            SoundManager.Instance.SetBgmVolume(bgmSlider.value);

            SaveBGMLocalData();
        }

        public void OnSFXValueChanged()
        {
            if (sfxSlider == null) return;

            SoundManager.Instance.SetEffectVolume(sfxSlider.value);

            SaveSFXLocalData();
        }

        public void OnTownDragonValueChanged()
        {
            if (townDragonSlider == null) return;

            SaveTownDragonLocalData();
        }

        void SaveBGMLocalData()
        {
            if (bgmSlider == null) { return; }
            JObject soundData = new JObject();
            soundData.Add("volume", bgmSlider.value);

            var soundDataString = JsonConvert.SerializeObject(soundData);
            PlayerPrefs.SetString("Setting_BGM", soundDataString);

            GamePreference.Instance.SetBgm(bgmSlider.value, true);
        }

        void SaveSFXLocalData()
        {
            if (sfxSlider == null) { return; }
            JObject soundData = new JObject();
            soundData.Add("volume", sfxSlider.value);

            var soundDataString = JsonConvert.SerializeObject(soundData);
            PlayerPrefs.SetString("Setting_SFX", soundDataString);

            GamePreference.Instance.SetSfx(sfxSlider.value, true);
        }

        void SaveTownDragonLocalData()
        {
            if (townDragonSlider == null) { return; }

            JObject toyData = new JObject();
            toyData.Add("value", townDragonSlider.value);

            var DataString = JsonConvert.SerializeObject(toyData);
            PlayerPrefs.SetString("Setting_Toy", DataString);

            CancelInvoke("ApplyTown");
            Invoke("ApplyTown", 1.0f);
        }

        void ApplyTown()
        {
            if(Town.Instance != null)
                Town.Instance.SettingDragonVisible();
        }
    }
}
