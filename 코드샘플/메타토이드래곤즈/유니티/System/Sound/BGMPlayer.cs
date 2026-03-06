using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    [System.Serializable]
    public class BGMData
    {
        public string soundKey = "";
        public bool isSmoothChange = true;
        private AudioSource bgmAudioSource;
        public AudioSource BGMAudioSource
        {
            get { return bgmAudioSource; }
            set { bgmAudioSource = value; }
        }
    }

    public class BGMPlayer : MonoBehaviour
    {
        public bool isAuto = true;
        public BGMData bgmData = new BGMData();

        private bool isPlayed = false;
        private void Awake()
        {
            bgmData.BGMAudioSource = GetComponentInChildren<AudioSource>();
        }

        private void OnEnable()
        {
            if(isAuto)
                Play();
        }

        public void Play()
        {
            if (isPlayed)
                return;

            if (string.IsNullOrWhiteSpace(bgmData.soundKey))
            {
                SoundManager.Instance.PushBGM(bgmData);
            }
            else
            {
                SoundManager.Instance.PushBGM(bgmData.soundKey, bgmData.isSmoothChange);
            }

            isPlayed = true;
        }
    }
}