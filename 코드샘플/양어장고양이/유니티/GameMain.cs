using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMain : MonoBehaviour
{
    public enum HahahaState
    {
        HAHAHA_INIT = -1,
        HAHAHA_GAME,
        HAHAHA_WORLD,
        HAHAHA_SAMANDA,
        HAHAHA_CHAT,
        HAHAHA_CARD,
        HAHAHA_PHOTO,
        HAHAHA_FARM,
    };

    public HahahaState curState = HahahaState.HAHAHA_INIT;
    public HahahaState preState = HahahaState.HAHAHA_INIT;

    public CanvasControl GameCanvas;
    public CanvasControl WorldCanvas;
    public CanvasControl CardCanvas;
    public CanvasControl SamandaCanvas;
    public CanvasControl ShopCanvas;
    public FarmCanvas FarmCanvas; 

    public PopupControl PopupControl;
    public ConfigUI ConfigUI;

    float TimeChecker = 0.0f;
    int TimeOffset = 0;

    private void Awake()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 1);

        NetworkManager.GetInstance().SendApiRequest("friend", 1, data, null, null, false);

        GameObject canvas = GameObject.Find("SAMANDA_CANVAS");
        if (canvas)
        {
            canvas.GetComponent<SamandaStarter>().SetBackKeyCallback(() => {
                if(ConfigUI.gameObject.activeInHierarchy)
                {
                    ConfigUI.OnHideConfigUI();
                    return;
                }

                if(PopupControl.TryCancel())
                {
                    return;
                }

                PopupControl.OnPopupMessageYN("게임을 종료하시겠습니까?", () => {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit(); // 어플리케이션 종료
#endif
                });
            });
        }
    }
    void Start()
    {
        bool mute = PlayerPrefs.GetInt("Config_ES", 1) == 0;
        float volume = (float)PlayerPrefs.GetInt("Config_EV", 9) / 9;
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

#if UNITY_EDITOR
        GameObject canvas = GameObject.Find("SAMANDA_CANVAS");
        if (canvas == null)
        {
            int targetSeq = 0;
            //PlayerPrefs.SetInt("UnlockEffect", targetSeq);
            GameDataManager.Instance.LoadGameData();
            GameDataManager.Instance.GetUserData().data["contents"] = (uint)targetSeq;
        }
#endif
        //PlayerPrefs.SetInt("UnlockEffect", 0);
        HahahaState startTargetState = HahahaState.HAHAHA_FARM;

        uint curSeq = ContentLocker.GetCurContentSeq();
        uint pivotSeq = ContentLocker.GetCurPivotSeq(curSeq);
        if (pivotSeq != curSeq)
        {
            ContentLocker.SendContentSeq((int)pivotSeq);
        }
        
        NetworkManager.GetInstance().TimerReset = ResetTimer;

        float wr = (float)Screen.width / 720;
        float hr = (float)Screen.height / 1280;

        SamandaLauncher.ResizeWebViewRect(new Rect(0 * wr, 0 * hr, 0 * wr, 0 * hr));

        curState = HahahaState.HAHAHA_INIT;
        SetState(startTargetState);

        switch (pivotSeq)
        {
            case 6:
            case 9:
            case 18:
            case 25:
            case 28:
                OnWorldMapButton();
                break;

            case 43:
            case 45:
                OnShopButton();
                break;

            case 0:
            case 1:
            case 29:
            case 40:                        
            default:
                break;
        }

        PopupControl.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        TimeChecker += Time.deltaTime;
        if(TimeChecker > 1.0f)
        {
            TimeChecker -= 1.0f;
            TimeOffset += 1;
        }
    }

    public void ResetTimer()
    {
        TimeChecker = 0.0f;
        TimeOffset = 0;
    }

    public int GetCurTime()
    {
        return NetworkManager.GetInstance().ServerTime + TimeOffset;
    }

    public void SetState(HahahaState state)
    {
        if (curState == state)
            return;

        preState = curState;
        curState = state;

        GameCanvas.SetCanvasState(curState);
        CardCanvas.SetCanvasState(curState);
        SamandaCanvas.SetCanvasState(curState);
        ShopCanvas.SetCanvasState(curState);
        WorldCanvas.SetCanvasState(curState);
        FarmCanvas.SetCanvasState(curState);
    }

    public void OnSamandaButton()
    {
        SetState(HahahaState.HAHAHA_SAMANDA);
    }

    public void OnChatButton()
    {
        SetState(HahahaState.HAHAHA_CHAT);
    }

    public void OnCardButton()
    {
        SetState(HahahaState.HAHAHA_CARD);
    }

    public void OnGameButton()
    {
        SetState(HahahaState.HAHAHA_GAME);
    }

    public void OnShopButton()
    {
        SetState(HahahaState.HAHAHA_PHOTO);
    }

    public void OnWorldMapButton()
    {
        ((WorldCanvas)WorldCanvas).OnWorldMapState();
        SetState(GameMain.HahahaState.HAHAHA_WORLD);
    }

    public void OnFeedButton(uint itemID)
    {
        ((WorldCanvas)WorldCanvas).OnFeedState(itemID);
        SetState(GameMain.HahahaState.HAHAHA_WORLD);

    }
    public void OnCloseCardButton()
    {
        if(curState == HahahaState.HAHAHA_CARD)
            SetState(preState);
    }

    public void OnPrevState()
    {
        SetState(preState);
    }
}
