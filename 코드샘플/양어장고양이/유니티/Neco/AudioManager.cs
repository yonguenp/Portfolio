using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Video;

public class AudioManager : MonoBehaviour
{
    static private AudioManager instance = null;
    
    public AudioSource backgroundAudioPlayer;
    public AudioSource effectAudioPlayer;

    public GameObject nyangAudioPlayerObject;
    private List<AudioSource> nyang = null;

    public AudioClip DefaultBGMClip = null;

    public AudioMixer mixer;

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;        
    }

    static public AudioManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        InitSoundSetting();
    }

    public void InitSoundSetting()
    {
        //bool isBGMOn = PlayerPrefs.GetInt("Setting_BGM", 1) == 1;

        //if (isBGMOn)
        //{
        //    ResumeBackgroundAudio();
        //}
        //else
        //{
        //    PauseBackgroundAudio();
        //}

        // SFX
        //bool isSFXOn = PlayerPrefs.GetInt("Setting_SFX", 1) == 1;

        //GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        //foreach (GameObject root in roots)
        //{
        //    if (root.GetComponent<Canvas>() != null)
        //    {
        //        AudioSource[] audios = root.GetComponentsInChildren<AudioSource>(true);
        //        foreach (AudioSource audio in audios)
        //        {
        //            audio.mute = !isSFXOn;
        //        }
        //    }
        //}

        SetBGMMixerControl(PlayerPrefs.GetInt("Setting_BGM", 1) == 1);
        SetSFXMixerControl(PlayerPrefs.GetInt("Setting_SFX", 1) == 1);
    }

    public void PlayBackgroundAudio(AudioClip clip = null)
    {
        if (clip == null)
            clip = DefaultBGMClip;

        float volume = PlayerPrefs.GetInt("Setting_BGM", 1);

        backgroundAudioPlayer.volume = volume;
        //backgroundAudioPlayer.mute = volume <= 0;

        backgroundAudioPlayer.clip = clip;
        backgroundAudioPlayer.loop = true;

        backgroundAudioPlayer.Play();
    }

    public void ResumeBackgroundAudio()
    {
        bool isBGMon = PlayerPrefs.GetInt("Setting_BGM", 1) == 1;

        if (isBGMon)
        {
            backgroundAudioPlayer.mute = false;
            backgroundAudioPlayer.UnPause();
        }

    }
    public void PauseBackgroundAudio()
    {
        backgroundAudioPlayer.mute = true;
        backgroundAudioPlayer.Pause();
    }

    public void PlayEffectAudio(AudioClip clip)
    {
        if (PlayerPrefs.GetInt("Setting_SFX", 1) == 0)
        {
            return;
        }

        float volume = PlayerPrefs.GetInt("Setting_SFX", 1);

        effectAudioPlayer.volume = volume;

        effectAudioPlayer.clip = clip;
        effectAudioPlayer.loop = false;

        effectAudioPlayer.Play();
    }

    public void PlayNyangSound(float volume = 1.0f)
    {
        if(PlayerPrefs.GetInt("Setting_SFX", 1) == 0)
        {
            return;
        }

        if (nyang == null)
        {
            nyang = new List<AudioSource>(nyangAudioPlayerObject.GetComponents<AudioSource>());            
        }

        List<AudioSource> ableAC = new List<AudioSource>();
        foreach(AudioSource ac in nyang)
        {
            if (!ac.isPlaying)
                ableAC.Add(ac);
        }

        if (ableAC.Count > 0)
        {
            AudioSource target = nyang[Random.Range(0, ableAC.Count)];
            target.volume = volume;
            target.Play();
        }
    }

    public void SetBGMMixerControl(bool mute = false)
    {
        mixer.SetFloat("BGM", mute ? 0f : -80f);
    }

    public void SetSFXMixerControl(bool mute = false)
    {
        mixer.SetFloat("SFX", mute ? 0f : -80f);
    }
}
