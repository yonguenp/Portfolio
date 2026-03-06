using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HahahaOpening : MonoBehaviour
{
    public SamandaStarter samandaStarter;
    public UITransitionEffect OpeningImage;
    public UITransitionEffect OpeningText;

    public AudioSource effectAudioPlayer;

    void Awake()
    {
        Text text = OpeningText.GetComponent<Text>();
        if (text != null)
        {
            LANGUAGE_TYPE defaultLanguage = LANGUAGE_TYPE.LANGUAGE_KOR;
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Korean:
                    defaultLanguage = LANGUAGE_TYPE.LANGUAGE_KOR;
                    break;
                case SystemLanguage.English:
                default:
                    defaultLanguage = LANGUAGE_TYPE.LANGUAGE_ENG;
                    break;
            }
            LANGUAGE_TYPE langType = (LANGUAGE_TYPE)PlayerPrefs.GetInt("Setting_Language", (int)defaultLanguage);

            switch (langType)
            {
                case LANGUAGE_TYPE.LANGUAGE_KOR:
                    text.text = "‘크리에이터 haha ha의 마당냥들이 전격 출현!\n공개되지 않은 고양이 사진과 영상들을 만날 수 있습니다.";
                    break;

                default:
                    text.text = "Creator haha ​​ha's wildcats have appeared!\nYou can find unpublished cat photos and videos.";
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnHahahaOpening()
    {
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        float actionTime = 2.0f;
        OpeningImage.effectFactor = 0.0f;
        OpeningImage.effectPlayer.duration = actionTime;
        OpeningText.effectFactor = 0.0f;
        
        yield return new WaitForEndOfFrame();

        OpeningImage.Show();
        
        float time = actionTime;
        while (time > 0.0f)
        {
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;
        }

        if (effectAudioPlayer != null)
        {
            bool isSFXOn = PlayerPrefs.GetInt("Setting_SFX", 1) == 1;
            if (isSFXOn)
            {
                effectAudioPlayer.Play();
            }
        }

        OpeningText.effectPlayer.duration = actionTime * 0.5f;
        OpeningText.Show();

        OpeningImage.effectFactor = 1.0f;        

        actionTime = 2.0f;
        time = actionTime;
        while (time > 0.0f)
        {
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;
        }

        OpeningText.effectFactor = 1.0f;

        actionTime = 2.0f;

        OpeningImage.effectPlayer.duration = actionTime;
        OpeningText.effectPlayer.duration = actionTime;

        OpeningImage.Hide();
        OpeningText.Hide();
                
        time = actionTime;
        while (time > 0.0f)
        {
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;
        }

        OpeningImage.effectFactor = 0.0f;
        OpeningText.effectFactor = 0.0f;

        SceneManager.LoadScene("Intro");
    }
}
