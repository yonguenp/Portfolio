using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SandboxNetwork
{
    /*
	 * 설정 데이터 통합
	 * 사운드 (BGM, SFX)
	 * 언어 설정
	 * 게임 속도
	 * 
	 */

    public struct VolumeData
    {
        public float volume;
        public bool isOn;

        public void Init()
        {
            volume = 1f;
            isOn = true;
        }
    }

    public class GamePreference
    {
        public static GamePreference Instance
        {
            get { return SBGameManager.Instance.GamePrefData; }
        }

        private CultureInfo culture = null;
        public CultureInfo Culture 
        { 
            get 
            { 
                if (culture == null)
                {
                    switch (GamePreference.Instance.GameLanguage)
                    {
                        case SystemLanguage.Korean:
                            culture = new CultureInfo("ko-KR", false);
                            break;
                        case SystemLanguage.English:
                            culture = new CultureInfo("en-US", false);
                            break;
                        case SystemLanguage.Japanese:
                            culture = new CultureInfo("ja-JP", false);
                            break;
                        case SystemLanguage.Portuguese:
                            culture = new CultureInfo("pt-PT", false);
                            break;
                        default:
                            culture = new CultureInfo("en-US", false);
                            break;
                    }
                }

                return culture;
            } 
        }
        private SystemLanguage gameLanguage = (SystemLanguage)PlayerPrefs.GetInt("Setting_Language", (int)Application.systemLanguage);
        public SystemLanguage GameLanguage
        {
            get
            {
                LanguageData data = LanguageData.Get(gameLanguage);
                if(data == null)
                {
                    return SystemLanguage.English;
                }

                return gameLanguage;
            }

            set
            {
                culture = null;
                PlayerPrefs.SetInt("Setting_Language", (int)value);
                gameLanguage = value;
            }
        }

        VolumeData BGMData = new VolumeData();
        VolumeData SFXData = new VolumeData();

        public bool OPTION_VIBRATION
        {
            get
            {
                return PlayerPrefs.GetInt("is_vibration", 1) > 0;
            }
            set
            {
                PlayerPrefs.SetInt("is_vibration", value ? 1 : 0);
            }
        }

        public string VERSION { get; private set; } = "unknown";
        public int BUILD_CODE { get; private set; } = -1;
        public string BUILD_BRANCH { get; private set; } = "unknown";
        public int RESOURCE_BUNDLE_VERSION { get; private set; } = -1;
        public GamePreference()
        {
            InitGamePrefData();
        }

        #region Volume Setting
        public VolumeData GetBGMData()
        {
            return BGMData;
        }
        public VolumeData GetSFXData()
        {
            return SFXData;
        }

        public void InitGamePrefData()//초기화할 데이터
        {
            LoadVersionInfo();
            SetBgm();// 사운드 세팅
            SetSfx();
        }

        public void LoadVersionInfo()
        {
            TextAsset ver = Resources.Load("Version") as TextAsset;
            if (ver)
            {
                string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
                var lines = Regex.Split(ver.text, LINE_SPLIT_RE);

                if (lines.Length > 0)
                {
                    string strCode = lines[0].Trim('\n');
                    strCode = strCode.Trim('\r');

                    if (string.IsNullOrEmpty(strCode))
                        BUILD_CODE = 0;
                    else
                        BUILD_CODE = int.Parse(strCode);
                }

                if (lines.Length > 1)
                {
#if UNITY_EDITOR
                    BUILD_BRANCH = "EDITOR";
#else
                BUILD_BRANCH = lines[1].Trim('\n');
                BUILD_BRANCH = BUILD_BRANCH.Trim('\r');
                string[] tmp = BUILD_BRANCH.Split('/');
                BUILD_BRANCH = tmp[tmp.Length - 1];
#endif
                }

                VERSION = Application.version + "." + BUILD_CODE.ToString();
            }
        }

        public void SetBundleVersion(int version)
        {
            RESOURCE_BUNDLE_VERSION = version;
        }

        void SetBgm()
        {
            BGMData.Init();
            if (PlayerPrefs.HasKey("Setting_BGM"))
            {
                var volumeJobjectData = PlayerPrefs.GetString("Setting_BGM");
                var data = JsonConvert.DeserializeObject<JObject>(volumeJobjectData);

                if (SBFunc.IsJTokenCheck(data["volume"])) BGMData.volume = data["volume"].Value<float>();// 순서 중요 -> 슬라이더먼저
                if (SBFunc.IsJTokenCheck(data["isOn"])) BGMData.isOn = data["isOn"].Value<bool>();
            }
            SetBgmVolume();
        }

        public void SetBgm(float _volume, bool _isOn)
        {
            BGMData.volume = _volume;
            BGMData.isOn = _isOn;
            SetBgmVolume();
        }

        public void SetSfx(float _volume, bool _isOn)
        {
            SFXData.volume = _volume;
            SFXData.isOn = _isOn;
            SetSfxVolume();
        }

        void SetSfx()
        {
            SFXData.Init();
            if (PlayerPrefs.HasKey("Setting_SFX"))
            {
                var volumeJobjectData = PlayerPrefs.GetString("Setting_SFX");
                var data = JsonConvert.DeserializeObject<JObject>(volumeJobjectData);

                if (SBFunc.IsJTokenCheck(data["volume"])) SFXData.volume = data["volume"].Value<float>();// 순서 중요 -> 슬라이더먼저
                if (SBFunc.IsJTokenCheck(data["isOn"])) SFXData.isOn = data["isOn"].Value<bool>();
            }
            SetSfxVolume();
        }

        void SetBgmVolume()
        {
            var Volume = BGMData.isOn ? BGMData.volume : 0f;
            SoundManager.Instance.SetBgmVolume(Volume);
        }

        public float GetBgmVolume()
        {
            return BGMData.isOn? BGMData.volume: 0f;
        }

        void SetSfxVolume()
        {
            var Volume = SFXData.isOn ? SFXData.volume : 0f;
            SoundManager.Instance.SetEffectVolume(Volume);
        }
        public float GetSfxVolume()
        {
            return SFXData.isOn ? SFXData.volume : 0f;
        }
        #endregion
    }
}
