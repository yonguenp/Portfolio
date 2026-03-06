using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{

    protected static SceneLoader instance;

    public static SceneLoader Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<SceneLoader>();

                if (obj != null)
                {
                    instance = obj;
                }
                else
                {
                    instance = Create();
                }
            }

            return instance;
        }

        private set
        {
            instance = value;
        }
    }

    [SerializeField]
    private UITransitionEffect background;
    [SerializeField]
    private UITransitionEffect[] loadingImage;
    [SerializeField]
    private UITransitionEffect loadingText;
    private string loadSceneName;
    private int curLoadingImageIndex = 0;
    private bool cardResourcesLoaded = false;

    public static SceneLoader Create()
    {
        var SceneLoaderPrefab = Resources.Load<SceneLoader>("Prefabs/LoadingSceneCanvas");
        return Instantiate(SceneLoaderPrefab);
    }

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }


    public void LoadScene(string sceneName)
    {
        gameObject.SetActive(true);
        SceneManager.sceneLoaded += LoadSceneEnd;
        loadSceneName = sceneName;
        StartCoroutine(Load(sceneName));
    }

    private IEnumerator Load(string sceneName)
    {
        foreach (UITransitionEffect effect in loadingImage)
        {
            effect.gameObject.SetActive(true);
            effect.effectFactor = 0.0f;
        }

        curLoadingImageIndex = Random.Range(0, loadingImage.Length);
        
        yield return StartCoroutine(Fade(true));

        //StartCoroutine(LoadCardResourceAsync());
        StartCoroutine(LoadVideoResourceAsync());
        neco_map.TmpNecoMapData();

        cardResourcesLoaded = true;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        int curIndex = 0;

        while (true)
        {
            float delta = Time.deltaTime * 60.0f;
            for (int i = 0; i < loadingImage.Length; i++)
            {
                if (i != curIndex)
                {
                    float f = loadingImage[i].effectFactor;
                    f = f - 0.03f * delta;
                    loadingImage[i].effectFactor = f;
                }
            }

            float factor = loadingImage[curIndex].effectFactor;
            factor = factor + 0.12f * delta;

            loadingImage[curIndex].effectFactor = factor;

            if (factor >= 1.0f)
            {
                curIndex++;

                if (curIndex == loadingImage.Length)
                {
                    curIndex = 0;
                }
            }

            yield return new WaitForEndOfFrame();

            if (op.progress >= 0.9f && cardResourcesLoaded && curIndex == 0)
            {
                op.allowSceneActivation = true;
            }
        }
    }

    private struct CardResourceLoadStruct
    {
        public ResourceRequest req;
        public user_card target;
    };

    private IEnumerator LoadCardResourceAsync()
    {
        cardResourcesLoaded = false;
        List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
        if (data_list != null)
        {
            List<string> loadPath = new List<string>();
            List<CardResourceLoadStruct> loadInfo = new List<CardResourceLoadStruct>();
            foreach (game_data data in data_list)
            {
                user_card userCard = ((user_card)data);
                if (userCard.IsOriginLoaded() == false)
                {
                    CardResourceLoadStruct info = new CardResourceLoadStruct();
                    string path = userCard.GetCardData().GetResourcePath();
                    if (!loadPath.Contains(path))
                    {
                        loadPath.Add(path);
                        info.req = Resources.LoadAsync<Sprite>(path);
                        info.target = userCard;
                        loadInfo.Add(info);
                    }
                    yield return new WaitForEndOfFrame();
                }
            }

            while (loadInfo.Count > 0)
            {
                bool loadDone = true;
                
                foreach (CardResourceLoadStruct info in loadInfo)
                {
                    if (!info.req.isDone)
                    {
                        info.target.SetOriginSprite((Sprite)info.req.asset);
                        loadDone = false;
                        loadInfo.Remove(info);
                        break;
                    }
                }

                if(loadDone)
                {
                    break;
                }

                yield return new WaitForEndOfFrame();
            }
            
        }

        cardResourcesLoaded = true;
    }


    private IEnumerator LoadVideoResourceAsync()
    {
        List<string> clipPath = new List<string>();

        clipPath.Add("beta_clips/2.gilm/pk_meet_01");
        clipPath.Add("beta_clips/2.gilm/pk_run");
        clipPath.Add("beta_clips/2.gilm/mixedfishfried");
        clipPath.Add("beta_clips/2.gilm/nc_fishing");

        foreach (string path in clipPath)
        {
            Resources.LoadAsync<UnityEngine.Video.VideoClip>(path);

            yield return new WaitForEndOfFrame();
        }
        
    }
    private void LoadSceneEnd(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == loadSceneName)
        {
            SceneManager.sceneLoaded -= LoadSceneEnd;

            StopCoroutine("Fade");
            StartCoroutine(Fade(false));
        }
    }

    private IEnumerator Fade(bool isFadeIn)
    {
        float fromValue = (isFadeIn ? 0.0f : 1.0f);
        float fullActionTime = 1.0f * 1.5f;
        float firstActionTime = 0.5f * 1.5f;
        float scondActionTime = fullActionTime - (firstActionTime * 0.8f);

        background.effectFactor = fromValue;
        background.effectPlayer.duration = firstActionTime;

        loadingText.effectFactor = fromValue;
        loadingText.effectPlayer.duration = fullActionTime;
        
        if (isFadeIn)
        {
            loadingText.Show();
            background.Show();
        }
        else
        {
            loadingText.Hide();
        }

        float time = firstActionTime * 0.8f;
        while(time > 0.0f)
        {
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;            
        }
        
        if (isFadeIn)
        {
            
        }
        else
        {
            background.Hide();
        }
        
        time = scondActionTime;
        while (time > 0.0f)
        {
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;

            foreach (UITransitionEffect image in loadingImage)
            {
                float factor = image.effectFactor;
                factor -= Time.deltaTime;
                image.effectFactor = factor;
            }
        }

        foreach (UITransitionEffect image in loadingImage)
        {
            image.effectFactor = 0;
        }

        background.effectFactor = 1.0f - fromValue;
        loadingText.effectFactor = 1.0f - fromValue;    

        if(isFadeIn == false)
        {
            gameObject.SetActive(false);
        }
    }

}
