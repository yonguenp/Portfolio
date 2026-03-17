using Coffee.UIEffects;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class LoadingManager : MonoBehaviour
    {

        private static LoadingManager instance = null;
        public static LoadingManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = FindObjectOfType<LoadingManager>();
                    if (obj != null)
                    {
                        instance = obj;
                    }
                }
                return instance;
            }
        }

        [SerializeField]
        private GameObject whiteBg;
        [SerializeField]
        private Image BlackBG;
        [SerializeField]
        private GameObject loadingBg;
        [SerializeField]
        private Image progressBar = null;
        [SerializeField]
        private CanvasGroup sceneLoaderCanvasGroup = null;
        [SerializeField]
        private float blinkTimer = 0.6f;
        [SerializeField]
        private Animator sceneLoadAnim = null;
        [SerializeField]
        private GameObject loadIndicatorObj = null;
        [SerializeField]
        private Image indicatorImg = null;

        public VoidDelegate SceneCallback { get; set; } = null;
        private string loadSceneName = "";
        public string currentScene
        {
            get
            {
                return loadSceneName;
            }
        }
        private Stack<string> SceneStack = new Stack<string>(); //씬 뒤로 가기 적용을 위한 스텍
        private eUIType UiType;
        private bool IsRefreshUI = false;
        private bool isLoadingEnd = false;
        public bool IsStartLoadDone { get; private set; }
        public bool IsLoadingEnd { get { return isLoadingEnd && IsStartLoadDone; } }
        private IEnumerator coroutine = null;
        public void OnPostStartLoading() { IsStartLoadDone = true; }

        public void LoadStartScene()
        {
            // To Do :: SceneManager가 아닌 EffectiveSceneLoad를 사용하여 씬 이동 효과를 유지해야 함.
            SceneManager.LoadScene("Start");
        }

        public void LoadLastestScene(eSceneEffectType eEffect = eSceneEffectType.None, params IEnumerator[] loadingCompleateCO)
        {
            if (SceneStack.Count < 1) 
                return;

            switch (SceneStack.Peek())
            {
                case "WorldBossLobby":                    
                    //eEffect = eSceneEffectType.BlackBackground;
                    break;
                case "WorldBossBattle":
                    if (WorldBossStage.Instance != null && !WorldBossStage.Instance.IsBattleState())
                        return;
                    eEffect = eSceneEffectType.BlackBackground;
                    break;
            }

            var sceneName = SceneStack.Pop();

            if (sceneName == loadSceneName) sceneName = SceneStack.Pop();
            if (sceneName != SceneManager.GetActiveScene().name)
            {   
                EffectiveSceneLoad(sceneName, eEffect, loadingCompleateCO);
            }
        }

        public void Clear()
        {
            InitState();
            sceneLoadAnim.gameObject.SetActive(false);
            LoadSceneEnd(SceneManager.GetActiveScene(), LoadSceneMode.Single);

            loadIndicatorObj.SetActive(false);
        }
        public void EffectiveSceneLoad(string sceneName, eSceneEffectType eEffect = eSceneEffectType.CloudAnimation, params IEnumerator[] loadingCompleateCO) // 여기 코루틴은 구름 가린 후 호출되는 코루틴
        {
            InitState();
            loadSceneName = sceneName;
            SceneManager.sceneLoaded += LoadSceneEnd;
            switch (eEffect)
            {
                case eSceneEffectType.CloudAnimation:
                    //연출에 따라 고정할지 선택
                    SoundManager.Instance.PlaySFX("FX_SCENE_CHANGE_2");
                    coroutine = FadeSceneMoveCoroutine(sceneName, loadingCompleateCO);
                    StartCoroutine(coroutine);
                    break;
                case eSceneEffectType.BlackBackground:
                    //연출에 따라 고정할지 선택
                    SoundManager.Instance.PlaySFX("FX_SCENE_CHANGE_2");
                    coroutine = BlackBackGroundCoroutine(sceneName, loadingCompleateCO);
                    StartCoroutine(coroutine);
                    break;
                    //case eEffectType.Gage:
                    //    StartCoroutine(LoadingGageCoroutine(sceneName));
                    //    break;
                    //case eEffectType.BlockDoor:
                    //    blockDoorBg.SetActive(true);
                    //    blockDoorBg.GetComponent<Animation>().Play();
                    //    break;
            }
        }

        void InitState()
        {
            isLoadingEnd = false;
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            whiteBg.SetActive(false);
            if(BlackBG != null)
                BlackBG.gameObject.SetActive(false);
            SceneManager.sceneLoaded -= LoadSceneEnd;
        }

        private IEnumerator LoadingGageCoroutine(string sceneName)
        {
            instance.loadingBg.SetActive(true);
            progressBar.fillAmount = 0f;
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            float timer = 0.0f;
            while (!op.isDone)
            {
                yield return null;
                timer += Time.unscaledDeltaTime;

                if (op.progress < 0.9f)
                {
                    progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress, timer);
                    if (progressBar.fillAmount >= op.progress)
                    {
                        timer = 0f;
                    }
                }
                else
                {
                    progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer);

                    if (progressBar.fillAmount == 1.0f)
                    {
                        op.allowSceneActivation = true;
                        LoadingManager.instance.loadingBg.SetActive(false);
                        yield break;
                    }
                }
            }
        }

        public void initSceneStack()
        {
            SceneStack.Clear();
            isLoadingEnd = true;
            loadSceneName = "Town";
            SceneStack.Push("Town");
        }

        private void LoadSceneEnd(Scene scene, LoadSceneMode loadSceneMode)
        {
            string[] path = scene.name.Split("/");
            path = path[path.Length - 1].Split(".");
           
            if (path[0] == loadSceneName)
            {
                instance?.loadingBg?.SetActive(false);
                isLoadingEnd = true;
                SceneCallback?.Invoke();
                SceneCallback = null;
                if (IsRefreshUI)
                {
                    UIManager.Instance.RefreshUI(UiType);
                }

                // 씬 뒤로가기 처리를 위한 push 과정
                if (loadSceneName == "Town")
                {
                    initSceneStack();
                }
                else
                {
                    if (SceneStack.Contains(loadSceneName))
                    { //타운이 아닌 A <-> B 씬 왔다 갔다 예외 처리
                        for (int i = 0, count = SceneStack.Count; i < count; ++i)
                        {
                            var popVal = SceneStack.Pop();
                            if (popVal == loadSceneName)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        SceneStack.Push(loadSceneName);
                    }
                }
            }
            
        }

        private IEnumerator FadeSceneMoveCoroutine(string sceneName, params IEnumerator[] coroutine)
        {
            yield return CloudAnimationCoroutine(blinkTimer, blinkTimer, null, MoveScene(sceneName, coroutine));
        }

        private IEnumerator MoveScene(string sceneName, params IEnumerator[] coroutines)
        {
            var temp = Time.time;

            SetIndicator();

            AsyncOperation sceneLoaded = null;
            //씬 이동
            if (AssetBundleManager.IsBundle(eResourcePath.BundleScenePath))
            {
                sceneLoaded = SceneManager.LoadSceneAsync(ResourceManager.LoadSceneFromAssetBundle("BundleScenes/" + sceneName));
            }
            else
            {
                sceneLoaded = SceneManager.LoadSceneAsync(sceneName);
            }

            while (false == sceneLoaded.isDone)
            {
                yield return SBDefine.GetWaitForEndOfFrame();
            }

            yield return CleanUpAssets();

            if (coroutines != null)
            {
                for (int i = 0, count = coroutines.Length; i < count; ++i)
                {
                    var coroutine = coroutines[i];
                    if (coroutine == null)
                        continue;

                    yield return coroutine;
                }
            }

            EndIndicator();
            //씬로딩 확인
            yield return new WaitUntil(() => isLoadingEnd);
        }

        public void CloudAnimation(float fadeInTime, float fadeOutTime, VoidDelegate cb = null)
        {
            StartCoroutine(CloudAnimationCoroutine(fadeInTime, fadeOutTime, cb));
        }
        private IEnumerator CloudAnimationCoroutine(float fadeInTime, float fadeOutTime, VoidDelegate cb = null, IEnumerator coroutine = null)
        {
            yield return CloudInCoroutine(fadeInTime);
            PopupManager.AllClosePopup();
           
            yield return new WaitForSeconds(0.1f);
            cb?.Invoke();
            if (coroutine != null)
                yield return coroutine;
           
            yield return new WaitForSeconds(0.3f);
           
            yield return CloudOutCoroutine(fadeOutTime);
        }

        void SetIndicator()
        {
            if (loadIndicatorObj != null)
            {
                loadIndicatorObj.SetActive(true);
                loadIndicatorObj.GetComponent<Animator>().Play("effect", 0);
            }
        }
        
        void EndIndicator()
        {
            if (loadIndicatorObj != null)
                loadIndicatorObj.SetActive(false);
        }


        private IEnumerator BlackBackGroundCoroutine(string sceneName, params IEnumerator[] coroutine)
        {
            if (BlackBG != null)
            {
                BlackBG.gameObject.SetActive(true);
                BlackBG.color = Color.clear;


                Color BgColor = new Color(0.05f, 0.0f, 0.0f, 1.0f);
                while (BlackBG.color.a < 1.0f)
                {
                    BlackBG.color += (BgColor * Time.deltaTime) * 1.3f;
                    yield return new WaitForEndOfFrame();
                }
                BlackBG.color = BgColor;

                PopupManager.AllClosePopup();
                yield return MoveScene(sceneName, coroutine);

                while (BlackBG.color.a > 0.0f)
                {
                    BlackBG.color -= (BgColor * Time.deltaTime) * 1.5f;
                    yield return new WaitForEndOfFrame();
                }

                BlackBG.color = Color.clear;
                BlackBG.gameObject.SetActive(false);
            }
        }

        private IEnumerator CleanUpAssets()
        {
            System.GC.Collect();
            var async = Resources.UnloadUnusedAssets();
            while (false == async.isDone)
            {
                yield return SBDefine.GetWaitForEndOfFrame();
            }
            System.GC.Collect();
            //                yield return Resources.UnloadUnusedAssets();

#if UNITY_EDITOR
            //            long max;
            //            long current;

            //#if UNITY_5_6_OR_NEWER
            //            max = Profiler.GetTotalReservedMemoryLong();
            //            current = Profiler.GetTotalAllocatedMemoryLong();
            //#else
            //            max = Profiler.GetTotalReservedMemory();
            //            current = Profiler.GetTotalAllocatedMemory();
            //#endif

            //            var maxMb = (max >> 10);
            //            maxMb /= 1024; // On new line to fix il2cpp

            //            var currentMb = (current >> 10);
            //            currentMb /= 1024;

            //            Debug.Log("=-=-=-=- MEMORY : " + currentMb.ToString() + " / " + maxMb.ToString() + "MB");
#endif
        }

        private IEnumerator CloudInCoroutine(float fadeInTime)
        {
            float timer = 0f;
            sceneLoadAnim.gameObject.SetActive(true);
            sceneLoadAnim.Play("Cloud_in", 0);

            whiteBg.SetActive(true);
            sceneLoaderCanvasGroup.alpha = 0.0f;
            //하얀색 빠르게
            while (timer < fadeInTime)
            {
                yield return null;
                timer += Time.fixedDeltaTime;
                //sceneLoaderCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeInTime);
            }

            //sceneLoaderCanvasGroup.alpha = 1.0f;
        }

        private IEnumerator CloudOutCoroutine(float fadeOutTime)
        {
            float timer = 0f;
            sceneLoadAnim.Play("Cloud_out", 0);
            sceneLoaderCanvasGroup.alpha = 0.0f;

            while (timer < fadeOutTime)
            {
                yield return null;
                timer += Time.fixedDeltaTime;
                //sceneLoaderCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeOutTime);
            }

            //sceneLoaderCanvasGroup.alpha = 0.0f;
            whiteBg.SetActive(false);
            sceneLoadAnim.gameObject.SetActive(false);
        }

        public string GetSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }
    }
}