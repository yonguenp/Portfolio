using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SandboxNetwork 
{
    public class SFXPlayer : MonoBehaviour, IPointerDownHandler
    {
        public bool isPlayOnEnable = true;
        public string soundKey = "";
        public bool isButton = false;
        public bool isToggle = false;
        bool isBeforeLoading = false;

        AudioSource source = null;


        private void Start()
        {
            checkBeforeLoad();
            //if (isInit == false)
            //{
            //    if (isToggle)
            //    {
            //        GetComponent<Toggle>()?.onValueChanged.AddListener((bool toggleOn) =>
            //        {
            //            if (toggleOn) onClickBtn();
            //        });
            //    }
            //    isInit = true;
            //}
        }
        
        private void checkBeforeLoad()
        {
            if (SBGameManager.Instance.ContainManager(typeof(SoundManager)))
            {
                isBeforeLoading = false;
            }
            else
            {
                isBeforeLoading = true;
            }
        }

        private void OnEnable()
        {
            source = null;

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Start") return;  // 최초에셋번들 로드시 예외처리
            if (isPlayOnEnable == false) return;

            source = SoundManager.Instance?.PlaySFX(soundKey);
        }

        private void OnDisable()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Start") return;  // 최초에셋번들 로드시 예외처리
            if (isPlayOnEnable == false) return;
            
            if (source == null || SoundManager.Instance == null)
                return;

            if(source.clip == SoundManager.Instance.GetClip(soundKey))
                source.Stop();

            source = null;
        }

        private void onClickBtn()
        {
            SoundManager.Instance?.PlaySFX(soundKey);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isBeforeLoading) // 사운드 데이터 로드전 사운드 처리
            {
                var audio = GetComponent<AudioSource>();
                if(audio != null) {
                    audio.volume = SBGameManager.Instance.GamePrefData.GetSfxVolume();
                    audio.Play();
                }
                checkBeforeLoad();
                return;
            }
            if (isButton || isToggle)
            {
                SoundManager.Instance?.PlaySFX(soundKey);
            }
        }
    }
}
