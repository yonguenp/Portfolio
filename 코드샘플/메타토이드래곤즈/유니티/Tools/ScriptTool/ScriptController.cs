using System;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SandboxNetwork;

public class ScriptController : MonoBehaviour
{
    [SerializeField] ScriptObjectController[] ObjectController = new ScriptObjectController[(int)ScriptGroupData.OBJECT_POS.MAX];
    [SerializeField] Image Background;
    [SerializeField] Text ScriptText = null;
    [SerializeField] CanvasGroup MonologBackground = null;
    [SerializeField] Text MonologueText = null;
    [SerializeField] GameObject Arrow = null;
    [SerializeField] GameObject SkipButton = null;

    [SerializeField] GameObject ScriptPanel = null;
    [SerializeField] GameObject MonologuePanel = null;

    Image dim = null;
    Coroutine ScriptCoroutine = null;

    /// <summary>
    /// 종료 시점에 연출 이전에 호출되는 콜백
    /// </summary>
    Action exitFirstCallBack = null; 
    /// <summary>
    /// 종료 시점에 연출 이후에 호출되는 콜백
    /// </summary>
    Action exitCallback = null;


    List<ScriptGroupData> Scripts = null;
    Sequence tween = null;
    int seq = 0;
    ScriptGroupData.UI_TYPE curUIType = ScriptGroupData.UI_TYPE.UNKNOWN;

    string prevBG = "";
    string prevBGM = "";
    AudioClip prevBGMClip = null;
    BGMData bgmData = new BGMData();

    const float FadeTime = 1.5f;

    public bool IsPlaying { get; private set; } = false;
    bool isSkip = false; // 스킵여부 체크

    public void Close()
    {
        if (tween != null)
            tween.Kill();
        IsPlaying = false;
        ScriptText.text = "";
        gameObject.SetActive(false);
        ScriptPanel.SetActive(false);
        MonologuePanel.SetActive(false);
        isSkip = false;

        if (Background != null)
        {
            Background.gameObject.SetActive(false);

            //foreach (Transform c in Background.transform)
            //{
            //   Destroy(c.gameObject);
            //}
        }

        exitCallback = null;
        exitFirstCallBack = null;
        
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.RefreshNotifications();

        prevBG = "";
        prevBGM = "";
        
        if (dim != null)
            dim.color = Color.clear;
    }
    public void SetData(ScriptTriggerData trigger, Action exitCallBack = null, Action exitFirstCallBack = null)
    {
        if (trigger == null)
        {
            Close();
            return;
        }
        IsPlaying = true;
        prevBGMClip = null;
        var prevBGMData = SoundManager.Instance.PlayBGM();
        if (prevBGMData != null)
        {
            if(prevBGMData.BGMAudioSource.clip != null) 
            { 
                prevBGMClip = prevBGMData.BGMAudioSource.clip;
            }
        }
#if UNITY_EDITOR
        Debug.LogFormat("Start Script : {0}",trigger.KEY);
#endif
        isSkip = false;
        gameObject.SetActive(true);

        if (dim == null)
        {
            dim = GetComponent<Image>();
            if (dim != null)
                dim.color = Color.clear;
        }

        if (dim != null)
            dim.DOColor(new Color(0.0f,0.0f,0.0f,0.6f), 1.0f);

        exitCallback = exitCallBack;
        this.exitFirstCallBack = exitFirstCallBack;
        seq = 0;
        Scripts = new List<ScriptGroupData>(trigger.Child);
        OnScript(seq);

        SkipButton.SetActive(false);
        CancelInvoke("ShowSkipButton");
        Invoke("ShowSkipButton", 1.0f);
    }
#if UNITY_EDITOR
    public void StopScript()
    {
        CancelInvoke("OnNext");
    }
    public void Next()
    {
        OnNext();
    }
    public void ExitScript()
    {
        Close();
    }
#endif
    void OnScript(int index)
    {
        CancelInvoke("OnNext");

        if (tween != null)
        {
            tween.Kill();
        }
        tween = DOTween.Sequence();

        if (Scripts.Count <= index)
        {
            OnExit();
            return;
        }
        if (isSkip) // 스킵버튼을 눌렀으면 동작하면 안됨
        {
            return;
        }

        var Script = Scripts[index];

        curUIType = Script.UI;

        if (prevBG != Script.BG_resource)
        {
            prevBG = Script.BG_resource;

            GameObject bg = null;
            if (!string.IsNullOrEmpty(Script.BG_resource) && Script.BG_resource != "NONE" && Script.BG_resource != "0")
            {                
                bg = ResourceManager.GetResource<GameObject>(eResourcePath.ScriptBGPath, Script.BG_resource);
            }

            if (Background != null)
            {
                Background.gameObject.SetActive(bg != null);
                if (bg != null)
                {
                    foreach (Transform c in Background.transform)
                    {
                        Destroy(c.gameObject);
                    }

                    GameObject child = Instantiate(bg) as GameObject;
                    child.transform.SetParent(Background.transform);

                    CutSceneBackground cutscene = child.GetComponent<CutSceneBackground>();
                    if (cutscene != null)
                    {
                        cutscene.Init(Background.transform as RectTransform);
                    }
                }
            }
        }
        if (Script.BG_resource == string.Empty) // 튜토리얼 도중 탈출하고 호출되면 prevBG 와 Script.BG_resource 둘다 empty 상태가 되기도 함
            Background.gameObject.SetActive(false);


        if (!string.IsNullOrEmpty(Script.BGM_resource) && Script.BGM_resource != "NONE" && Script.BGM_resource != "0" && Script.BGM_resource != "")
        {
            if (Script.BGM_resource != prevBGM)
            {
                //prevBGMData = SoundManager.Instance.StopBGM();
                prevBGM = Script.BGM_resource;
                SoundManager.Instance.PushBGM(Script.BGM_resource, true);
            }
        }

        ScriptPanel.SetActive(false);
        MonologuePanel.SetActive(false);
        for (int i = 0; i < (int)ScriptGroupData.OBJECT_POS.MAX; i++)
        {
            ObjectController[i].Clear();
        }

        switch (curUIType)
        {
            case ScriptGroupData.UI_TYPE.NORMAL:
                ScriptPanel.SetActive(true);
                var scriptBackImg = ScriptPanel.GetComponent<Image>();
                
                MonologuePanel.SetActive(false);
                for (int i = 0; i < (int)ScriptGroupData.OBJECT_POS.MAX; i++)
                {
                    ObjectController[i].SetData(Script.OBJECTS[i], Script.STATES[i]);
                }

                ScriptText.text = "";

                Arrow.SetActive(false);
                if(index == 0)
                {
                    scriptBackImg.color = new Color(1, 1, 1, 0);
                    tween.Append(scriptBackImg.DOColor(new Color(1, 1, 1, 1), 0.8f));
                }
                tween.Append(ScriptText.DOText(Script.TEXT, Script.DURATION)).AppendCallback(() => {
                    Arrow.SetActive(true);
                });
                break;
            case ScriptGroupData.UI_TYPE.MONOLOGUE:
                ScriptPanel.SetActive(false);
                MonologuePanel.SetActive(true);
                
                if(UICanvas.Instance != null)
                    (MonologuePanel.transform as RectTransform).sizeDelta = (UICanvas.Instance.transform as RectTransform).sizeDelta;
                
                for (int i = 0; i < (int)ScriptGroupData.OBJECT_POS.MAX; i++)
                {
                    ObjectController[i].Clear();
                }

                MonologueText.text = Script.TEXT;

                MonologueText.color = new Color(1, 1, 1, 0);
                tween.Append(MonologueText.DOColor(Color.white, 1f));
                Invoke("OnNext", Script.DURATION);
                break;
            case ScriptGroupData.UI_TYPE.TUTORIAL:                
                TutorialManager.tutorialManagement.StartTutorial(Script.GROUP_ID,1);
                break;
        }
    }

    void ShowSkipButton()
    {
        CancelInvoke("ShowSkipButton");
        SkipButton.SetActive(true);
    }

    
    public void OnNext()
    {
        CancelInvoke("OnNext");

        if(tween != null && tween.IsActive())
        {
            tween.Complete();
            return;
        }

        OnScript(++seq);
    }

    public void OnExit()
    {
        SkipButton.SetActive(false);

        if (isSkip)
            return;

        isSkip = true;

        // 현재 사운드 매니저 구조 및 사운드 리소스 테이블에서도 사용하지 않는 사운드 사용으로 
        // 스크립트 연출 전 사운드로 돌아가기 위한 코드
        if (prevBGMClip != null)
        {
            var source =GetComponent<AudioSource>();
            if(source == null) 
                source = gameObject.AddComponent<AudioSource>();
            source.clip = prevBGMClip;
            source.volume = SoundManager.Instance.BGMVolume;

            bgmData.BGMAudioSource = source;
            SoundManager.Instance.PushBGM(bgmData, true); 
        }

        bool isFadeOutDelay = false;
        exitFirstCallBack?.Invoke();

        if (Background.gameObject.activeSelf) { 
            foreach(var child in Background.transform.GetComponentsInChildren<CutSceneBackground>())
            {
                if(child != null)
                {
                    isFadeOutDelay = true;
                    CancelInvoke("OnNext");
                    child.SetFadeOut(FadeTime, () =>
                    {
                        ScriptText.text = "";
                        ScriptPanel.SetActive(false);
                        MonologuePanel.SetActive(false);

                        var cb = exitCallback;
                        Close();
                        cb?.Invoke();
                    });
                }
            }
        }

        if (Scripts != null && Scripts.Count > 0)
            ScriptEndEvent.Event(Scripts[0].GROUP_ID);
        
        if (LoadingManager.Instance != null && LoadingManager.Instance.GetSceneName() == "Town")
            QuestEvent.Event(QuestEvent.eEvent.TUTORIAL_QUEST_CHECK);

        if(isFadeOutDelay == false)
        {
            var cb = exitCallback;
            Close();
            cb?.Invoke();
        }
    }
}
