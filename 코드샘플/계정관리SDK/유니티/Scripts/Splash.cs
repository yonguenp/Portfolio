using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Splash : MonoBehaviour
{
    public SamandaStarter samandaStarter;
    public Image SplashImage;
    public AudioSource SplashAudio;
    public Camera cam;
    public GameObject Notice;
    public Text NoticeText;
    public Text RetryText;
    public Text ExitText;
    public HahahaOpening opening;

    bool SamandaLoadDone = false;
    private void Awake()
    {
        samandaStarter.SetSamandaStartCallback(()=> {
            SamandaLoadDone = true;
        });
    }

    private void OnEnable()
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
                NoticeText.text = "네트워크가 원활하지 않습니다.\n인터넷 연결을 확인하세요.";
                RetryText.text = "재시도";
                ExitText.text = "종료";
                break;
            default:
                NoticeText.text = "Network error occurred.\nPlease check your internet connection.";
                RetryText.text = "Retry";
                ExitText.text = "Exit";
                break;
        }
        

        StopAllCoroutines();
        StartCoroutine(RunSplash());
    }

    public IEnumerator RunSplash()
    {
        SplashImage.transform.localScale = Vector2.one * 0.3888888888888889f;

        List<Sprite> sprites = new List<Sprite>();
        for(int i = 0; i < 42; i++)
        {
            sprites.Add(Resources.Load<Sprite>("Sprites/Splash/" + i.ToString("00")));
        }

        bool isSFXOn = PlayerPrefs.GetInt("Setting_SFX", 1) == 1;
        if (isSFXOn)
        {
            SplashAudio.Play();
        }
        yield return new WaitForSecondsRealtime(0.1f);        

        int seq = 0;
        while(seq < sprites.Count)
        {
            SplashImage.sprite = sprites[seq++];
            yield return new WaitForSecondsRealtime(0.0333333333333333f);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        Color bgcolor = cam.backgroundColor;
        Color logocolor = SplashImage.color;

        float time = 1.0f;
        while (time > 0)
        {
            float delta = Time.deltaTime;
            time -= delta;

            logocolor.a -= delta;
            //logocolor.r -= delta;
            //logocolor.g -= delta;
            //logocolor.b -= delta;
            SplashImage.color = logocolor;

            //bgcolor.r -= delta;
            //bgcolor.g -= delta;
            //bgcolor.b -= delta;
            //cam.backgroundColor = bgcolor;

            yield return new WaitForEndOfFrame();
        }

        OpeningOn();
    }

    public void RetryGameStart()
    {
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            Destroy(root);
        }

        SceneManager.LoadScene("Splash");        
    }
    public void ExitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }

    public void OpeningOn()
    {
        opening.OnHahahaOpening();
    }
}
