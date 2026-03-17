using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfigUI : MonoBehaviour
{
    public Color FillColor;
    public Color EmptyColor;

    public Image[] EffectVolumeImage;
    public Image[] BackgroundVolumeImage;

    public Text VersionText;
    public Text UserNickName;
    public Text VibrateState;

    public Toggle EffectSoundToggle;
    public Toggle BackgroundSoundToggle;
    public Toggle VibrateToggle;

    public GameCanvas gameCanvas;
    public GameMain gameMain;

    public AudioSource BackgroundAudioSource;
    public AudioSource BackgroundChangedSource;

    private bool bAwaked = true;

    public void OnShowConfigUI()
    {
        CancelInvoke("OnCompleteTweenAnimation");
        gameObject.SetActive(true);
        foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
        {
            dotween.DOPlayForward();
        }
        return;

        CancelInvoke("OnCompleteTweenAnimation");
        gameObject.SetActive(true);

        gameCanvas.OnHideUI();
        gameCanvas.RunBackgroundFadeOut();
        foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
        {
            dotween.DOPlayForward();
        }
    }

    public void OnHideConfigUI()
    {
        foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
        {
            dotween.DOPlayBackwards();
        }
        Invoke("OnCompleteTweenAnimation", 1.0f);
        return;

        gameMain.SetState(GameMain.HahahaState.HAHAHA_GAME);
        gameCanvas.OnShowUI();
        
        foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
        {
            dotween.DOPlayBackwards();
        }

        Invoke("OnCompleteTweenAnimation", 1.0f);
        gameObject.SetActive(false);
    }

    public void OnCompleteTweenAnimation()
    {
        gameObject.SetActive(false);
    }

    private void Awake()
    {
        bAwaked = false;

        VersionText.text = GameDataManager.Instance.GetVersionWithFlag();

        EffectSoundToggle.isOn = PlayerPrefs.GetInt("Config_ES", 1) == 0;
        BackgroundSoundToggle.isOn = PlayerPrefs.GetInt("Config_BS", 1) == 0;
        VibrateToggle.isOn = PlayerPrefs.GetInt("Config_VB", 1) == 1;

        OnEffectVolumeButton(PlayerPrefs.GetInt("Config_EV", 9));
        OnBackgroundVolumeButton(PlayerPrefs.GetInt("Config_BV", 9));
        OnVibrateToggle();

        bAwaked = true;
    }

    public void OnEnable()
    {
        UserNickName.text = SamandaLauncher.GetAccountNickName();
    }


    public void OnVibrateToggle()
    {
        bool on = VibrateToggle.isOn;
        string text = on ? "ON" : "OFF";
        VibrateState.text = text;
        PlayerPrefs.SetInt("Config_VB", VibrateToggle.isOn ? 1 : 0);

        if (on && bAwaked)
        {
            RDG.Vibration.Vibrate(500, 60, true);
        }
    }

    public void OnEffectSoundToggle()
    {
        bool mute = EffectSoundToggle.isOn;
        PlayerPrefs.SetInt("Config_ES", mute ? 0 : 1);

        int volume = PlayerPrefs.GetInt("Config_EV", 9);
        if (mute)
            volume = -1;
        for (int i = 0; i < EffectVolumeImage.Length; i++)
        {
            Image image = EffectVolumeImage[i];
            if (i <= volume)
            {
                image.color = FillColor;
            }
            else
            {
                image.color = EmptyColor;
            }
        }

        Invoke("ApplyEffectSoundConfig", 0.01f);
    }

    public void OnEffectVolumeButton(int index)
    {
        bool mute = EffectSoundToggle.isOn;
        if(mute)
        {
            EffectSoundToggle.isOn = false;
        }

        for (int i = 0; i < EffectVolumeImage.Length; i++)
        {
            Image image = EffectVolumeImage[i];
            if(i <= index)
            {
                image.color = FillColor;
            }
            else
            {
                image.color = EmptyColor;
            }
        }

        PlayerPrefs.SetInt("Config_EV", index);

        Invoke("ApplyEffectSoundConfig", 0.01f);
    }

    public void OnBackgroundSoundToggle()
    {
        bool mute = BackgroundSoundToggle.isOn;
        PlayerPrefs.SetInt("Config_BS", mute ? 0 : 1);

        int volume = PlayerPrefs.GetInt("Config_BV", 9);
        if (mute)
            volume = -1;
        for (int i = 0; i < BackgroundVolumeImage.Length; i++)
        {
            Image image = BackgroundVolumeImage[i];
            if (i <= volume)
            {
                image.color = FillColor;
            }
            else
            {
                image.color = EmptyColor;
            }
        }

        Invoke("ApplyBackgroundSoundConfig", 0.01f);
    }
    public void OnBackgroundVolumeButton(int index)
    {
        bool mute = BackgroundSoundToggle.isOn;
        if (mute)
        {
            BackgroundSoundToggle.isOn = false;
        }

        for (int i = 0; i < BackgroundVolumeImage.Length; i++)
        {
            Image image = BackgroundVolumeImage[i];
            if (i <= index)
            {
                image.color = FillColor;
            }
            else
            {
                image.color = EmptyColor;
            }
        }

        PlayerPrefs.SetInt("Config_BV", index);

        Invoke("ApplyBackgroundSoundConfig", 0.01f);

        if (bAwaked)
        {
            BackgroundChangedSource.Stop();
            BackgroundChangedSource.volume = (float)index / (BackgroundVolumeImage.Length - 1);
            BackgroundChangedSource.Play();
        }
    }

    public void ApplyEffectSoundConfig()
    {
        CancelInvoke("ApplySoundConfig");

        bool mute = PlayerPrefs.GetInt("Config_ES", 1) == 0;
        float volume = (float)PlayerPrefs.GetInt("Config_EV", 9) / (EffectVolumeImage.Length - 1);
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (root.GetComponent<Canvas>() != null)
            {
                AudioSource[] audios = root.GetComponentsInChildren<AudioSource>(true);
                foreach (AudioSource audio in audios)
                {
                    audio.mute = mute;
                    audio.volume = volume;
                }
            }
        }
    }

    public void ApplyBackgroundSoundConfig()
    {
        if (BackgroundAudioSource == null)
            return;

        bool mute = PlayerPrefs.GetInt("Config_BS", 1) == 0;
        BackgroundAudioSource.mute = mute;
        float volume = (float)PlayerPrefs.GetInt("Config_BV", 9) / (BackgroundVolumeImage.Length - 1);
        BackgroundAudioSource.volume = volume;
    }

    public void OnSamandaButton()
    {
        SamandaLauncher.SetOnHideCallback(() => {
            gameMain.curState = GameMain.HahahaState.HAHAHA_GAME;
            SamandaLauncher.SetOnHideCallback(null);
        });
        gameMain.OnSamandaButton();
    }

    public void OnCustomerServiceButton()
    {
        SamandaLauncher.OpenCustomerSupportPage();
    }
}
