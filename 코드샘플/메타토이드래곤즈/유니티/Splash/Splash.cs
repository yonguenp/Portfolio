using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_IOS
// Include the IosSupport namespace if running on iOS:
using Unity.Advertisement.IosSupport;
#endif
namespace SandboxNetwork
{
    public class Splash : MonoBehaviour
    {
        [SerializeField]
        Canvas SplashCanvas = null;
        [SerializeField]
        private Animator splash = null;
		[SerializeField]
		private AudioClip splashSound = null;

        EasyMobile.EEARegionStatus EEARegionStatus = EasyMobile.EEARegionStatus.Unknown;
        void Start()
        {
            AppsFlyerSDK.AppsFlyer.initSDK("3ydXcgfSNi2pXu9x8PHUFS", "6748107724", this);

            LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());

            if (splash != null)
            {
                splash.Play("Splash");
                //StartCoroutine(splashAnim());
            }

            //#if DEBUG && UNITY_EDITOR_WIN
            //    SplashEnd();
            //    Destroy(this.gameObject);
            //#endif

            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });

            AppsFlyerSDK.AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(GameConfigTable.ATTTimeOutInterval);

            EEARegionStatus = (EasyMobile.EEARegionStatus)PlayerPrefs.GetInt("EEA_USER", (int)EasyMobile.EEARegionStatus.Unknown);
            if (EEARegionStatus == EasyMobile.EEARegionStatus.Unknown)
            {
                EasyMobile.EEARegionValidator.ValidateEEARegionStatus((EasyMobile.EEARegionStatus eea) =>
                {
                    EEARegionStatus = eea;                    
                });
            }
        }

        IEnumerator splashAnim()
        {
            yield return SBDefine.GetWaitForSeconds(1f); //오늘까지 마감인데 프레임 드랍 가능한 줄이기 위한 노력2
            splash.Play("Splash");
        }


        private void SplashSound()
        {
            PlaySound(splashSound, Vector3.zero);
        }
        public virtual AudioSource PlaySound(AudioClip sfx, Vector3 location, bool loop = false, float power = 1f)
        {
            float MasterVolume = PlayerPrefs.GetFloat("masterVolume", 1f);
            float EffectVolume = PlayerPrefs.GetFloat("effectVolume", 1f);
            float SFXVolume = MasterVolume * EffectVolume;

            if (!loop && (sfx == null || SFXVolume <= 0f))
                return null;
            // we create a temporary game object to host our audio source
            GameObject temporaryAudioHost = new GameObject(SBFunc.StrBuilder("Temp_", sfx.name));

            // we set the temp audio's position
            temporaryAudioHost.transform.position = location;
            // we add an audio source to that host
            AudioSource audioSource = temporaryAudioHost.AddComponent<AudioSource>();
            // we set that audio source clip to the one in paramaters
            audioSource.clip = sfx;
            // we set the audio source volume to the one in parameters
            audioSource.volume = SFXVolume;
            // we set our loop setting
            audioSource.loop = loop;
            // we start playing the sound
            audioSource.Play();

            DontDestroyOnLoad(temporaryAudioHost);
            Destroy(temporaryAudioHost, sfx.length);

            // we return the audiosource reference
            return audioSource;
        }

        private void SplashEnd() //씬 이동 가즈아
        {
            StartCoroutine(GameStartLoad());
        }

        public IEnumerator GameStartLoad()
        {
            float time = 3.0f;
#if UNITY_IOS
            // Check the user's consent status.
            // If the status is undetermined, display the request request:
            if(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED) {
                yield return new WaitForSeconds(0.5f); 
                
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                time = 5.0f;
                while (time > 0 && (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED))
                {
                    time -= 0.1f;
                    yield return new WaitForSeconds(0.1f);
                }
            }
#endif


            time = 3.0f;
            while (time > 0 && (EEARegionStatus == EasyMobile.EEARegionStatus.Unknown))
            {
                time -= 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            PlayerPrefs.SetInt("EEA_USER", (int)EEARegionStatus);

            try
            {
                //알수없을땐 강제로 아닌걸로
                if (EEARegionStatus != EasyMobile.EEARegionStatus.InEEA)
                {
                    AppsFlyerSDK.AppsFlyerConsent consent = AppsFlyerSDK.AppsFlyerConsent.ForNonGDPRUser();
                    AppsFlyerSDK.AppsFlyer.setConsentData(consent);

                    AppsFlyerSDK.AppsFlyer.startSDK();
                }
            }
            catch
            {
                Debug.LogError("AppsFlyer start Failed");
            }

            SceneManager.LoadScene("Start");
        }
    }
}