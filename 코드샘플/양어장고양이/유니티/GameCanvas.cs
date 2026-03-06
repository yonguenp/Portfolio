using DG.Tweening;
using Newtonsoft.Json.Linq;
using SuperBlur;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using static UnityEngine.UI.Button;

public class GameCanvas : CanvasControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Vector2 prevPoint;
    private Vector2 newPoint;
    private Vector2 screenTravel;
    private int currentMainFinger = -1;
    private int currentSecondFinger = -1;
    private Vector2 posA;
    private Vector2 posB;
    private float previousDistance = -1f;
    private float distance;
    private float pinchDelta = 0f;

    public RectTransform mainLayout;
    public GameObject ParticlePrefab;
    public GameObject RewardEffectSample;
    private GameObject CurTouchEffect;
    
    public RawImage BackgroundImage;
    public VideoPlayer mainPlayer;

    public GameObject Chat_Panel;
    public HahahaChat hahahaChat;

    public ProgressControl ProgressBar;

    public Image ExpBackground;
    public Image ExpBar;
    int videoIndex = 1;

    bool interactionMode = false;
    uint interactionParam = 0;
    int interactionModeType = 0; 

    float nextDeckVideoTime = -1;
    int gainScore = 0;

    public GameObject InteractionGroup;
    public GameObject TopUIGroup;
    public GameObject EmptyUI;
    public GameObject StatusUIGroup;
    public Text Heart;
    public Image HungerGauge;
    public GameObject HungerIcon;
    private Coroutine HeartAnimation = null;

    public SuperBlurFast superBlur;

    public TouchListUI TouchListUI;
    public PlayListUI PlayListUI;
    public FoodListUI FoodListUI;
    public CookListUI CookListUI;
    public RewardListUI RewardListUI;

    public Text UI_NickName;
    public Text UI_Level;
    public Text UI_Gold;
    public Text UI_EXP;
    public Text UI_Dia;
    public Text UI_GoldPerSec;

    public Image IdleRewardProgress;
    public GameObject IdleRewardButton;
    public GameObject IdleRewardEffect;
    public GameObject IdleRewardGetEffect;
    public GameObject ToggleButtonGroup;
    public Animation LevelUpAnimation;

    public GameObject ChatOffToggleIcon;
    public GameObject ChatOnToggleIcon;

    public FriendsPanel FriendsUI;
    public GameObject ReturnToOpenChatButton;
    public GameObject FriendAlarm;

    public AppearPanel CatAppearPanel;

    private string roomID = "";
    private bool isOpenChat { get { return string.IsNullOrEmpty(roomID); } }
    public string ChatRoomID { set { roomID = value; OnRoomChange(); } get { return roomID; } }

    private uint curLevel;
    private uint curExp;
    private Vector2 CONTENT_SIZE;
    public enum BackgroundType { NONE = -1, VIDEO, IMAGE };
    private BackgroundType bgType = BackgroundType.NONE;
    private BackgroundType curBackgroundType {
        get { return bgType; }
        set { 
            bgType = value;
            mainPlayer.gameObject.SetActive(bgType == BackgroundType.VIDEO);
            BackgroundImage.gameObject.SetActive(bgType == BackgroundType.IMAGE || bgType == BackgroundType.VIDEO);
        }
    }
    private Coroutine ExpAnimation = null;
    private Coroutine MoneyAnimation = null;
    private Coroutine BlurAnimation = null;
    private Coroutine videoFadeAnimation = null;
    private Coroutine chatRefreshCoroutine = null;
    private Coroutine persnalChatRefresh = null;
    private string SendChatMessage = "";
    void Start()
    {
        IdleRewardButton.SetActive(false);

        InitProfileUI();

        ProgressBar.SetNormalMode();
        videoIndex = 1;
        SetBackground();

        interactionMode = false;
        gainScore = 0;

        OnShowUI();
        ClearListUI();

        if(ContentLocker.GetCurContentSeq() == 0)
            OnHideUI();

        OnRoomChange();

        //CanvasRenderer[] canvasRenders = GetComponentsInChildren<CanvasRenderer>();
        //foreach (CanvasRenderer canvasRender in canvasRenders)
        //{
        //    UIMover mover = canvasRender.gameObject.AddComponent<UIMover>();
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (curBackgroundType == BackgroundType.VIDEO)
        {
            if (nextDeckVideoTime > 0)
            {
                nextDeckVideoTime -= Time.deltaTime;
                if (nextDeckVideoTime < 0)
                {
                    SetBackground();
                }
            }
        }

        ProgressBar.UpdateProgressBar();

        game_data user = GameDataManager.Instance.GetUserData();
        if(user != null)
        {
            uint last_reward = 0;
            //double rate = 0;
            //double tick_cycle = 0;
            uint next_tick = 0;

            object obj;
            if (user.data.TryGetValue("is_active", out obj))
            {
                if((uint)obj <= 0)
                {
                    IdleRewardProgress.gameObject.transform.parent.parent.gameObject.SetActive(false);
                    return;
                }
                else
                    IdleRewardProgress.gameObject.transform.parent.parent.gameObject.SetActive(true);
            }
            
            if (user.data.TryGetValue("last_reward_time", out obj))
            {
                last_reward = (uint)obj;
            }
            if (user.data.TryGetValue("rate", out obj))
            {
                UI_GoldPerSec.text = string.Format("{0:0.0}/초", (double)obj);
            }
            //if (user.data.TryGetValue("tick_cycle", out obj))
            //{
            //    tick_cycle = (double)obj;
            //}
            if (user.data.TryGetValue("next_tick", out obj))
            {
                next_tick = (uint)obj;
            }

            uint cur = (uint)GameManager.GetCurTime();

            bool enableReward = next_tick < cur;

            if (enableReward)
            {
                if (IdleRewardButton.activeSelf == false)
                {
                    IdleRewardButton.SetActive(true);
                }
            }
            else
                IdleRewardButton.SetActive(false);


            if (enableReward == false)
            {
                if (IdleRewardEffect.activeSelf == true)
                    IdleRewardEffect.SetActive(false);

                if (!DOTween.IsTweening(IdleRewardProgress.GetComponent<RectTransform>()))
                {
                    double diff = 1.0f - ((double)(next_tick - cur)) / (next_tick - last_reward);
                    IdleRewardProgress.GetComponent<RectTransform>().DOSizeDelta(new Vector2((float)diff * 150.0f, IdleRewardProgress.GetComponent<RectTransform>().sizeDelta.y), 1.0f, false).SetEase(Ease.Linear);
                }
            }
            else
            {
                if(IdleRewardEffect.activeSelf == false)
                    IdleRewardEffect.SetActive(true);

                IdleRewardProgress.GetComponent<RectTransform>().DOSizeDelta(new Vector2(150.0f, IdleRewardProgress.GetComponent<RectTransform>().sizeDelta.y), 1.0f, false).SetEase(Ease.Linear);
            }
        }
    }

    public override void SetCanvasState(GameMain.HahahaState state)
    {
        if (state == GameMain.HahahaState.HAHAHA_FARM)
        {
            curBackgroundType = BackgroundType.NONE;
            gameObject.SetActive(false);
            
            return;
        }

        bool visible = state == GameMain.HahahaState.HAHAHA_GAME;

        if (!EmptyUI.activeSelf == visible)
            return;

        if(visible)
        {
            SetBackground();
            gameObject.SetActive(true);
            OnShowUI();
            CancelInvoke("OnCompleteTweenAnimation");            
        }
        else
        {
            gameObject.SetActive(true);
            OnHideUI();

            if (gameObject.activeSelf)
            {
                if (BlurAnimation != null)
                    StopCoroutine(BlurAnimation);
                BlurAnimation = StartCoroutine(OnBlur());
            }

            Invoke("OnCompleteTweenAnimation", 0.5f);            
        }
    }

    public void RunBackgroundFadeIn()
    {
        if (videoFadeAnimation != null)
            StopCoroutine(videoFadeAnimation);
        videoFadeAnimation = StartCoroutine(OnBackgroundFadeIn());
    }

    public void RunBackgroundFadeOut()
    {
        if (videoFadeAnimation != null)
            StopCoroutine(videoFadeAnimation);
        videoFadeAnimation = StartCoroutine(OnBackgroundFadeOut());
    }

    public IEnumerator OnBackgroundFadeIn()
    {
        if (BlurAnimation != null)
            StopCoroutine(BlurAnimation);

        BlurAnimation = StartCoroutine(OffBlur());

        yield return BlurAnimation;
    }

    public IEnumerator OnBackgroundFadeOut()
    {
        if (BlurAnimation != null)
            StopCoroutine(BlurAnimation);

        BlurAnimation = StartCoroutine(OnBlur());

        yield return BlurAnimation;
    }

    public void OnCompleteTweenAnimation()
    {
        if (videoFadeAnimation != null)
            StopCoroutine(videoFadeAnimation);

        //gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (currentMainFinger == -1)
        {
            currentMainFinger = data.pointerId;
            prevPoint = data.position;

            posA = data.position;

            return;
        }

        if (currentSecondFinger == -1)
        {
            currentSecondFinger = data.pointerId;
            posB = data.position;

            figureDelta();
            previousDistance = distance;

            return;
        }

        Debug.Log("third+ finger! (ignore)");
    }

    public void OnDrag(PointerEventData data)
    {
        if (currentMainFinger == data.pointerId)
        {
            newPoint = data.position;
            screenTravel = newPoint - prevPoint;
            prevPoint = newPoint;

            if (currentSecondFinger == -1)
            {
                _processSwipe();
            }
            else
            {
            }

            posA = data.position;
        }

        if (currentSecondFinger == -1) return;

        if (currentMainFinger == data.pointerId) posA = data.position;
        if (currentSecondFinger == data.pointerId) posB = data.position;

        figureDelta();
        pinchDelta = distance - previousDistance;
        previousDistance = distance;

        _processPinch();
    }

    private void figureDelta()
    {
        // when/if two touches, keep track of the distance between them
        distance = Vector2.Distance(posA, posB);
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (currentMainFinger == data.pointerId)
        {
            currentMainFinger = -1;            
        }
        if (currentSecondFinger == data.pointerId)
        {
            currentSecondFinger = -1;
        }

        RectTransform rt = BackgroundImage.GetComponent<RectTransform>();
        RectTransform containerRT = BackgroundImage.transform.parent.GetComponent<RectTransform>();
         
        Vector2 MAX_SIZE = containerRT.rect.size;
        
        float curRatio = rt.sizeDelta.x / CONTENT_SIZE.x;
        float minRatio = MAX_SIZE.x / CONTENT_SIZE.x;
        float maxRatio = MAX_SIZE.y / CONTENT_SIZE.y;

        rt.sizeDelta = CONTENT_SIZE * maxRatio;

        Vector2 pos = rt.localPosition;
        float limit = Mathf.Abs((rt.sizeDelta.x / 2) - (MAX_SIZE.x / 2));
        if (pos.x >= limit)
        {
            pos.x = limit;
        }
        if (pos.x <= limit * -1.0f)
        {
            pos.x = limit * -1.0f;
        }

        if (curRatio <= minRatio)
        {
            pos.y = 0;
        }
        else
        {
            limit = Mathf.Abs((rt.sizeDelta.y / 2) - (MAX_SIZE.y / 2));
            if (pos.y > limit)
            {
                pos.y = limit;
            }
            if (pos.y < limit * -1.0f)
            {
                pos.y = limit * -1.0f;
            }
        }

        rt.localPosition = pos;
    }

    private void _processSwipe()
    {
        RectTransform canvasRT = GetComponent<RectTransform>();
        RectTransform rt = BackgroundImage.GetComponent<RectTransform>();
        Vector2 pos = (Vector2)rt.localPosition + (screenTravel * 0.5f);

        RectTransform containerRT = BackgroundImage.transform.parent.GetComponent<RectTransform>();
        Vector2 MAX_SIZE = containerRT.rect.size;

        float limit = Mathf.Abs((rt.sizeDelta.x / 2) - (MAX_SIZE.x / 2));
        
        float curRatio = rt.sizeDelta.x / CONTENT_SIZE.x;
        float minRatio = MAX_SIZE.x / CONTENT_SIZE.x;

        rt.localPosition = pos;

        if (pos.x >= limit)
        {
            pos.x = limit;
        }

        if (pos.x <= limit * -1.0f)
        {
            pos.x = limit * -1.0f;
        }

        if (curRatio <= minRatio)
        {
            pos.y = 0;
        }
        else
        {
            limit = Mathf.Abs((rt.sizeDelta.y / 2) - (MAX_SIZE.y / 2));
            if (pos.y > limit)
            {
                pos.y = limit;
            }
            if (pos.y < limit * -1.0f)
            {
                pos.y = limit * -1.0f;
            }
        }

        rt.localPosition = pos;

        PlayerPrefs.SetFloat("Background_X", pos.x);
    }

    private void _processPinch()
    {
        RectTransform rt = BackgroundImage.GetComponent<RectTransform>();
        RectTransform containerRT = BackgroundImage.transform.parent.GetComponent<RectTransform>();

        Vector2 MAX_SIZE = containerRT.rect.size;

        //float minRatio = MAX_SIZE.x / CONTENT_SIZE.x;
        float maxRatio = MAX_SIZE.y / CONTENT_SIZE.y;
        //float curRatio = Mathf.Max(((rt.sizeDelta.x / CONTENT_SIZE.x) + (pinchDelta / CONTENT_SIZE.x)), minRatio);
        //curRatio = Mathf.Min(curRatio, maxRatio);
        rt.sizeDelta = CONTENT_SIZE * maxRatio;
    }

    public void OnTouchItemSelect(inter_touch data)
    {
        ClearListUI();
        string clipName = "";
        interactionParam = 0;
        interactionModeType = 1;

        object obj;
        if (data.data.TryGetValue("clip_file", out obj))
        {
            clipName = (string)obj;
        }

        if (data.data.TryGetValue("touch_id", out obj))
        {
            interactionParam = (uint)obj;
        }

        uint playTIme = 1;
        if (data.data.TryGetValue("loop_on", out obj))
        {
            if ((uint)obj > 0)
            {
                if (data.data.TryGetValue("run_time", out obj))
                {
                    if ((uint)obj > 0)
                        playTIme = (uint)obj;
                }
            }
        }

        if (string.IsNullOrEmpty(clipName) || interactionParam == 0)
            return;

        VideoClip clip = Resources.Load<VideoClip>(clipName);
        if (clip == null)
        {
            return;
        }

        curBackgroundType = BackgroundType.VIDEO;
        mainPlayer.clip = clip;
        mainPlayer.isLooping = playTIme > 1;
        mainPlayer.audioOutputMode = VideoAudioOutputMode.None;

        nextDeckVideoTime = (float)clip.length * playTIme;

        ProgressBar.SetInteractionMode(nextDeckVideoTime, OnInteractionDone);
        interactionMode = true;
        gainScore = 0;

        OnStartInteraction();
    }

    public void OnPlayItemSelect(inter_play data)
    {
        ClearListUI();
        string clipName = "";
        interactionParam = 0;
        interactionModeType = 2;

        object obj;
        if (data.data.TryGetValue("clip_file", out obj))
        {
            clipName = (string)obj;
        }

        if (data.data.TryGetValue("play_id", out obj))
        {
            interactionParam = (uint)obj;
        }

        uint playTIme = 1;
        if (data.data.TryGetValue("loop_on", out obj))
        {
            if((uint)obj > 0)
            {
                if (data.data.TryGetValue("run_time", out obj))
                {
                    if ((uint)obj > 0)
                        playTIme = (uint)obj;
                }
            }
        }

        if (string.IsNullOrEmpty(clipName) || interactionParam == 0)
            return;

        VideoClip clip = Resources.Load<VideoClip>(clipName);
        if (clip == null)
        {
            return;
        }

        curBackgroundType = BackgroundType.VIDEO;
        mainPlayer.clip = clip;
        mainPlayer.isLooping = playTIme > 1;

        nextDeckVideoTime = (float)clip.length * playTIme;

        ProgressBar.SetInteractionMode(nextDeckVideoTime, OnInteractionDone);
        interactionMode = true;
        gainScore = 0;

        OnStartInteraction();
    }

    public void OnStartInteraction()
    {
        foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
        {
            dotween.DOPlayBackwards();
        }

        Button button = EmptyUI.GetComponent<Button>();
        if(button)
            button.enabled = false;
    }

    public void OnInteractionDone()
    {
        foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
        {
            dotween.DOPlayForward();
        }

        Button button = EmptyUI.GetComponent<Button>();
        if (button)
            button.enabled = true;

        CurTouchEffect = null;
        for (int i = 0; i < Camera.main.transform.childCount; i++)
        {
            GameObject effect = Camera.main.transform.GetChild(i).gameObject;
            var particle = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particle)
            {
                var em = ps.emission;
                em.enabled = false;
            }
            Destroy(effect, 3.0f); 
        }

        EmptyUI.SetActive(true);
        if (BlurAnimation != null)
            StopCoroutine(BlurAnimation);
        BlurAnimation = StartCoroutine(OnBlur());

        WWWForm data = new WWWForm();
        data.AddField("api", "inter");
        data.AddField("id", interactionParam.ToString());
        NetworkManager.GetInstance().SendApiRequest("inter", interactionModeType, data, (response) => {
            if (!RewardListUI.ShowTouchRewardList(response))
            {
                EmptyUI.SetActive(false);
            }
        });

        interactionMode = false;
        SetBackground();
        interactionParam = 0;
        interactionModeType = 0;
    }

    public void ShowScoreGainEffect()
    {
        GameObject effect = Instantiate(RewardEffectSample);
        effect.transform.SetParent(Camera.main.transform);

        Destroy(effect, 2);
    }

    public void SetBackground()
    {
        object obj;
        uint cardID = 0;
        user_card card = null;
        if (GameDataManager.Instance.GetUserData() != null && GameDataManager.Instance.GetUserData().data.TryGetValue("background_cardid", out obj))
        {
            try
            {
                cardID = (uint)obj;
            }
            catch
            {
                cardID = 0;
            }
        }

        if (cardID == 0)
        {
            if (GameDataManager.Instance.GetUserData() != null)
                GameDataManager.Instance.GetUserData().data["background_cardid"] = (uint)0;
        }
        else
        {
            List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
            if (data_list != null)
            {
                foreach (game_data data in data_list)
                {
                    if(((user_card)data).GetCardUniqueID() == cardID)
                    {
                        card = (user_card)data;
                    }
                }
            }
        }

        string param = "";
        card_define cardData = null;
        if (card != null)
            cardData = card.GetCardData();

        if (card != null && cardData != null)
        {            
            uint type = cardData.GetResourceType();
            switch (type)
            {
                case 1:
                    curBackgroundType = BackgroundType.VIDEO;
                    break;
                case 0:
                default:
                    curBackgroundType = BackgroundType.IMAGE;
                    break;
            }

            param = cardData.GetResourcePath();

            if (curBackgroundType == BackgroundType.VIDEO)
            {
                PlayVideo(param);
            }
            if (curBackgroundType == BackgroundType.IMAGE)
            {
                VideoClip preClip = mainPlayer.clip;
                Resources.UnloadAsset(preClip);
                mainPlayer.clip = null;

                BackgroundImage.texture = card.GetSprite().texture;
                BackgroundImage.uvRect = card.GetUVRect();
                BackgroundImage.GetComponent<RectTransform>().sizeDelta = card.GetRect().size;
                BackgroundImage.rectTransform.localPosition = new Vector2(PlayerPrefs.GetFloat("Background_X", 0), 0);
                CONTENT_SIZE = card.GetRect().size;
                _processPinch();
            }
        }
        else
        {
            VideoClip preClip = mainPlayer.clip;
            Resources.UnloadAsset(preClip);
            mainPlayer.clip = null;

            curBackgroundType = BackgroundType.IMAGE;
            param = "CARDS/origin/default_bg";
            Sprite sprite = Resources.Load<Sprite>(param);
            BackgroundImage.texture = sprite.texture;
            BackgroundImage.GetComponent<RectTransform>().sizeDelta = sprite.rect.size;
            CONTENT_SIZE = sprite.rect.size;
            _processPinch();
        }

        PointerEventData dummy = new PointerEventData(null);
        dummy.pointerId = -1;
        OnPointerUp(dummy);
    }

    public void PlayVideo(string videoName)
    {
        if(curBackgroundType != BackgroundType.VIDEO)
        {
            return;
        }

        if(mainPlayer.clip != null)
        {
            string path = mainPlayer.clip.originalPath;
            string[] fold = path.Split('/');
            string[] name = fold[fold.Length - 1].Split('.');

            path = name[0];
            fold = videoName.Split('/');
            name = fold[fold.Length - 1].Split('.');

            if(path == name[0])
            {
                return;
            }
        }

        mainPlayer.Stop();
        VideoClip preClip = mainPlayer.clip;
        Resources.UnloadAsset(preClip);

        RenderTexture target = mainPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        VideoClip clip = Resources.Load<VideoClip>(videoName);
        if(clip == null)
        {
            return;
        }

        mainPlayer.clip = clip;
        mainPlayer.Play();
        nextDeckVideoTime = (float)clip.length;
        BackgroundImage.texture = mainPlayer.targetTexture;
        BackgroundImage.uvRect = new Rect(Vector2.zero, Vector2.one);
        CONTENT_SIZE = new Vector2(BackgroundImage.texture.width, BackgroundImage.texture.height);
    }

    public void OnRewardBtn()
    {
        EmptyUI.SetActive(true);
        if (BlurAnimation != null)
            StopCoroutine(BlurAnimation);
        BlurAnimation = StartCoroutine(OnBlur());

        WWWForm data = new WWWForm();
        data.AddField("api", "inter");
        NetworkManager.GetInstance().SendApiRequest("inter", 4, data, (response) => { 
            if(!RewardListUI.ShowIdleRewardList(response))
            {
                IdleRewardGetEffect.SetActive(true);
                Invoke("IdleRewardGetEffectDone", 1.0f);
                EmptyUI.SetActive(false);
            }
        });
    }

    public void IdleRewardGetEffectDone()
    {
        IdleRewardGetEffect.SetActive(false);
    }

    public void OnToggleUI()
    {
        if (EmptyUI.activeSelf)
        {
            OnShowUI();
        }
        else
        {
            OnHideUI();
        }
    }

    public void OnHideUI()
    {
        StopChat();

        foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
        {
            dotween.DOPlayBackwards();
        }

        if (GameManager.curState == GameMain.HahahaState.HAHAHA_WORLD)
        {
            foreach (DOTweenAnimation dotween in InteractionGroup.GetComponentsInChildren<DOTweenAnimation>())
            {
                dotween.DOPlayForward();
            }
            foreach (DOTweenAnimation dotween in StatusUIGroup.GetComponentsInChildren<DOTweenAnimation>())
            {
                dotween.DOPlayForward();
            }
        }

        EmptyUI.SetActive(true);

        ClearListUI();

        if (persnalChatRefresh != null)
        {
            StopCoroutine(persnalChatRefresh);
            persnalChatRefresh = null;
        }
    }

    public void OnShowUI()
    {
        if(!FriendAlarm.activeInHierarchy)
        {
            WWWForm data = new WWWForm();
            data.AddField("api", "friend");
            data.AddField("op", 12);

            NetworkManager.GetInstance().SendApiRequest("friend", 12, data, (response) =>
            {
                Invoke("OnFriendAlarmCheck", 0.1f);
            }, null, false);
        }

        if (GameManager.curState != GameMain.HahahaState.HAHAHA_GAME)
        {
            OnHideUI();
            return;
        }

        StartChat();

        foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
        {
            dotween.DOPlayForward();
        }

        if(!IsChatUIActive())
        {
            OnHideChatUI();
        }
        else if(ContentLocker.GetCurContentSeq() < ContentLocker.ContentGuideDoneSeq - 1)
        {
            OnHideChatUI();
        }
        

        EmptyUI.SetActive(false);

        ClearListUI();

        if(persnalChatRefresh == null)
        {
            persnalChatRefresh = StartCoroutine(PersnalChatRefresh());
        }
    }

    public void StartChat()
    {
        if (hahahaChat)
        {
            hahahaChat.ClearMsgList();
        }

        if (chatRefreshCoroutine == null)
        {
            chatRefreshCoroutine = StartCoroutine(ChatRefreshPull());
        }
        CancelInvoke("InactiveChat");

        Chat_Panel.SetActive(true);
    }

    public void StopChat()
    {
        //SamandaLauncher.OnSamandaShortcut(SamandaLauncher.Samanda_Shorcut.PAGE_HOME, false);
        if(chatRefreshCoroutine != null)
        {
            StopCoroutine(chatRefreshCoroutine);
            chatRefreshCoroutine = null;
        }
        Invoke("InactiveChat", 1.0f);
    }

    public void InactiveChat()
    {
        Chat_Panel.SetActive(false);
    }

    public void OnShowChatUI()
    {
        StartChat();
    }

    public void OnHideChatUI()
    {
        StopChat();

        if (Chat_Panel)
        {
            foreach (DOTweenAnimation dotween in Chat_Panel.GetComponentsInChildren<DOTweenAnimation>())
            {
                dotween.DOPlayBackwards();
            }
        }
    }

    public void ClearListUI()
    {
        if (TouchListUI.isShow)
            TouchListUI.CloseTouchList();
        else
            TouchListUI.gameObject.SetActive(false);

        if (PlayListUI.isShow)
            PlayListUI.ClosePlayList();
        else
            PlayListUI.gameObject.SetActive(false);

        if (FoodListUI.isShow)
            FoodListUI.CloseFoodList();
        else
            FoodListUI.gameObject.SetActive(false);

        if (CookListUI.isShow)
            CookListUI.CloseCookList();
        else
            CookListUI.gameObject.SetActive(false);

        if (RewardListUI.isShow)
        {
            if (RewardListUI.CloseRewardList())
            {
                //if (gameObject.activeSelf)
                //    Invoke("RefreshProfileUI", 1.5f);
                //else
                //{
                //    users user = GameDataManager.Instance.GetUserData();
                //    if (user == null)
                //        return;

                //    object obj;
                //    if (user.data.TryGetValue("level", out obj))
                //    {
                //        if((uint)obj > curLevel)
                //        {
                //            curLevel++;
                //            RewardListUI.ShowLevelUpRewardList(curLevel);
                //            InitProfileUI();
                //        }
                //    }
                //}
            }
        }
        else
            RewardListUI.gameObject.SetActive(false);

        if (gameObject.activeSelf && GameManager.curState == GameMain.HahahaState.HAHAHA_GAME)
        {
            if (BlurAnimation != null)
                StopCoroutine(BlurAnimation);
            BlurAnimation = StartCoroutine(OffBlur());
        }
    }

    public void OnEnable()
    {
        hahahaChat.ClearMsgList();        
        PlayListUI.gameObject.SetActive(false);
        SetBackground();
    }

    public void OnTouchButton()
    {
        EmptyUI.SetActive(true);
        if (BlurAnimation != null)
            StopCoroutine(BlurAnimation);
        BlurAnimation = StartCoroutine(OnBlur());
        TouchListUI.ShowTouchList(this);
    }

    public void OnPlayButton()
    {
        EmptyUI.SetActive(true);
        if (BlurAnimation != null)
            StopCoroutine(BlurAnimation);
        BlurAnimation = StartCoroutine(OnBlur());
        PlayListUI.ShowPlayList(this);
    }

    public void OnFoodButton()
    {
        EmptyUI.SetActive(true);
        if (BlurAnimation != null)
            StopCoroutine(BlurAnimation);
        BlurAnimation = StartCoroutine(OnBlur());
        FoodListUI.ShowFoodList();
    }

    public void OnCookButton()
    {
        EmptyUI.SetActive(true);
        if (BlurAnimation != null)
            StopCoroutine(BlurAnimation);
        BlurAnimation = StartCoroutine(OnBlur());
        CookListUI.ShowCookList();
    }

    public void InitProfileUI()
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;

        UI_NickName.text = "-";
        UI_Level.text = "-";
        UI_EXP.text = "0%";
        curLevel = 1;
        curExp = 0;

        object obj;
        if (user.data.TryGetValue("nick", out obj))
        {
            UI_NickName.text = obj.ToString();
        }
        if (user.data.TryGetValue("level", out obj))
        {
            curLevel = (uint)obj;
        }
        if (user.data.TryGetValue("exp", out obj))
        {
            curExp = (uint)obj;
        }
        uint money = 0;
        if(user.data.TryGetValue("gold", out obj))
        {
            money = (uint)obj;
        }
        uint dia = 0;

        RefreshExpColor();
        ExpBar.fillAmount = 0.0f;
        List <game_data> exp_table = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.EXP);
        int curLevelIndex = (int)curLevel - 1;
        int nextLevelIndex = (int)curLevel;
        if(exp_table.Count > curLevelIndex && exp_table.Count > nextLevelIndex)
        {
            object cur;
            object next;
            if (exp_table[curLevelIndex].data.TryGetValue("exp", out cur) && exp_table[nextLevelIndex].data.TryGetValue("exp", out next))
            {
                uint curExpRange = (uint)next - (uint)cur;
                uint curLevelExpGain = curExp - (uint)cur;
                
                float curRatio = (float)((double)curLevelExpGain / curExpRange);
                if (curRatio > 100.0f)
                    curRatio = 100.0f;

                ExpBar.fillAmount = curRatio;
                UI_EXP.text = string.Format("{0:0.0}%", curRatio * 100.0f);
            }
        }

        UI_Gold.text = money.ToString();
        Heart.text = money.ToString();

        HungerGauge.DOKill();
        HungerGauge.fillAmount = 0.0f;
        if (user.data.TryGetValue("level", out obj))
        {
            uint level = (uint)obj;
            uint fullnessMax = 0;

            foreach (game_data exp in exp_table)
            {
                if (exp.data.TryGetValue("level", out obj))
                {
                    if (level == (uint)obj)
                    {
                        if (exp.data.TryGetValue("max_fullness", out obj))
                        {
                            fullnessMax = (uint)obj;
                        }
                    }
                }
            }

            if (user.data.TryGetValue("fullness", out obj) && fullnessMax > 0)
            {
                uint curFullness = (uint)obj;
                float ratio = (float)curFullness / fullnessMax;
                HungerGauge.fillAmount = ratio;
            }
        }

        //RectTransform rect = HungerGauge.GetComponent<RectTransform>();
        //if(rect)
        //{
        //    float width = rect.rect.size.x;
        //    Vector2 localPos = HungerIcon.transform.localPosition;
        //    localPos.x = width * (HungerGauge.fillAmount - 0.5f);
        //    HungerIcon.transform.localPosition = localPos;
        //}

        UI_Dia.text = dia.ToString();
        UI_Level.text = curLevel.ToString();
        if(ExpAnimation != null)
            StopCoroutine(ExpAnimation);
        ExpAnimation = null;

        if (MoneyAnimation != null)
            StopCoroutine(MoneyAnimation);
        MoneyAnimation = null;
    }
    public void RefreshProfileUI()
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;

        object obj;
        if(user.data.TryGetValue("nick", out obj))
        {
            UI_NickName.text = obj.ToString();
        }

        if (user.data.TryGetValue("exp", out obj))
        {
            if(curExp != (uint)obj)
            {
                if (ExpAnimation != null)
                    StopCoroutine(ExpAnimation);

                ExpAnimation = StartCoroutine(PlayExpAnimation((uint)obj));
            }
        }

        UI_Level.text = curLevel.ToString();

        uint money = uint.Parse(Heart.text);
        if (user.data.TryGetValue("gold", out obj))
        {
            if (money != (uint)obj)
            {
                if (HeartAnimation != null)
                    StopCoroutine(HeartAnimation);

                HeartAnimation = StartCoroutine(PlayHeartAnimation((uint)obj));
            }
        }
        else
        {
            Heart.text = "-";
        }

        //HungerIcon.transform.DOKill();

        if (user.data.TryGetValue("level", out obj))
        {
            uint level = (uint)obj;
            uint fullnessMax = 0;

            List<game_data> exp_table = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.EXP);
            foreach (game_data exp in exp_table)
            {
                if (exp.data.TryGetValue("level", out obj))
                {
                    if (level == (uint)obj)
                    {
                        if (exp.data.TryGetValue("max_fullness", out obj))
                        {
                            fullnessMax = (uint)obj;
                        }
                    }
                }
            }

            if (user.data.TryGetValue("fullness", out obj) && fullnessMax > 0)
            {
                uint curFullness = (uint)obj;
                float ratio = (float)curFullness / fullnessMax;
                HungerGauge.DOFillAmount(ratio, ratio);
                
                RectTransform rect = HungerGauge.GetComponent<RectTransform>();
                if (rect)
                {
                    float width = rect.rect.size.x;
                    float localX = width * (ratio - 0.5f);
                    //if (HungerIcon.transform.localPosition.x != localX)
                    //{
                    //    HungerIcon.transform.DOLocalMoveX(localX, ratio).OnComplete(() => {
                            Animation anim = HungerIcon.GetComponent<Animation>();
                            if (anim)
                                anim.Play();
                    //    });
                    //}
                }
            }
            else
            {
                HungerGauge.fillAmount = 0.0f;
                //HungerIcon.transform.localPosition = Vector2.zero;
            }
        }
        else
        {
            HungerGauge.fillAmount = 0.0f;
            //HungerIcon.transform.localPosition = Vector2.zero;
        }
    }

    public IEnumerator PlayHeartAnimation(uint targetMoney)
    {
        uint money = uint.Parse(Heart.text);

        float perRatio = (targetMoney - money) * 0.1f;
        if (Mathf.Abs(perRatio) < 1.0f)
        {
            money = targetMoney;
        }

        while (money != targetMoney)
        {
            money = (uint)(money + perRatio);

            if (money > targetMoney && perRatio > 0)
            {
                money = targetMoney;
            }

            if (money < targetMoney && perRatio < 0)
            {
                money = targetMoney;
            }

            Heart.text = money.ToString();
            yield return new WaitForSeconds(0.1f);
        }

        Heart.text = money.ToString();

        RefreshProfileUI();
    }

    public uint GetCurDisplayLevel()
    {
        return curLevel;
    }

    public IEnumerator PlayExpAnimation(uint targetExp)
    {
        uint goalExp = targetExp;
        float targetRatio = ExpBar.fillAmount;

        List<game_data> exp_table = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.EXP);
        int curLevelIndex = Convert.ToInt32(curLevel) - 1;
        int nextLevelIndex = Convert.ToInt32(curLevel);
        if (exp_table.Count > curLevelIndex && exp_table.Count > nextLevelIndex)
        {
            object cur;
            object next;
            if (exp_table[curLevelIndex].data.TryGetValue("exp", out cur) && exp_table[nextLevelIndex].data.TryGetValue("exp", out next))
            {
                int curExpRange = Convert.ToInt32(next) - Convert.ToInt32(cur);
                int curLevelExpGain = Convert.ToInt32(targetExp) - Convert.ToInt32(cur);
                float curRatio = ((float)curLevelExpGain / curExpRange);
                targetRatio = curRatio;
                if (targetRatio >= 1.0f)//levelup?
                {
                    targetRatio = 1.0f;
                    targetExp = (uint)next;
                }

                float displayStartRatio = ((float)Convert.ToInt32(curExp) - Convert.ToInt32(cur)) / curExpRange;
                UI_EXP.text = string.Format("{0:0.0}%", displayStartRatio * 100.0f);
            }
        }

        RefreshExpColor();

        float perRatio = (targetRatio - ExpBar.fillAmount) * 0.1f;
        uint perExpVal = Convert.ToUInt32((targetExp - curExp) * 0.1f);
        while (ExpBar.fillAmount < targetRatio)
        {
            ExpBar.fillAmount += perRatio;
            curExp += perExpVal;

            if (ExpBar.fillAmount > targetRatio)
            {
                ExpBar.fillAmount = targetRatio;
                curExp = targetExp;
            }
            UI_EXP.text = string.Format("{0:0.0}%", ExpBar.fillAmount * 100.0f);
            yield return new WaitForSeconds(0.1f);
        }

        ExpBar.fillAmount = targetRatio;
        curExp = targetExp;
        UI_EXP.text = string.Format("{0:0.0}%", targetRatio * 100.0f);

        if (targetRatio >= 1.0f)
        {
            curLevel += 1;
            
            if(LevelUpAnimation.gameObject.activeInHierarchy)
                LevelUpAnimation.Play();

            yield return new WaitForSecondsRealtime(LevelUpAnimation.clip.length);

            //yield return CatAppearPanel.PlayAppearMovie(curLevel);
            
            if (RewardListUI.ShowLevelUpRewardList(curLevel))
            {
                while (RewardListUI.isShow)
                {
                    yield return new WaitForSeconds(0.01f);
                }
            }

            ExpBar.fillAmount = 0.0f;
            UI_EXP.text = "0.0%";

            yield return StartCoroutine(PlayExpAnimation(goalExp));
        }

        RefreshProfileUI();
    }

    public IEnumerator PlayMoneyAnimation(uint targetMoney)
    {
        uint money = uint.Parse(UI_Gold.text);

        float perRatio = (targetMoney - money) * 0.1f;
        if(Mathf.Abs(perRatio) < 1.0f)
        {
            money = targetMoney;
        }

        while (money != targetMoney)
        {
            money = (uint)(money + perRatio);
            
            if(money > targetMoney && perRatio > 0)
            {
                money = targetMoney;
            }

            if (money < targetMoney && perRatio < 0)
            {
                money = targetMoney;
            }

            UI_Gold.text = money.ToString();
            yield return new WaitForSeconds(0.1f);
        }

        UI_Gold.text = money.ToString();

        RefreshProfileUI();
    }

    public IEnumerator OnBlur()
    {
        superBlur.enabled = true;

        if (superBlur.downsample >= 2 && superBlur.iterations >= 5 && superBlur.interpolation >= 0.5f)
            yield return null;

        while (superBlur.iterations < 5)
        {
            superBlur.iterations += 1;
            superBlur.downsample = superBlur.iterations / 2;
            superBlur.interpolation = (float)superBlur.downsample / 4;
            yield return new WaitForSeconds(0.075f);
        }

        superBlur.downsample = 2;
        superBlur.iterations = 5;
        superBlur.interpolation = 0.5f;

        BlurAnimation = null;
    }

    public IEnumerator OffBlur()
    {
        if (superBlur.downsample == 0 && superBlur.iterations == 1 && superBlur.interpolation == 0.0f)
            yield return null;

        while (superBlur.iterations > 1)
        {
            superBlur.iterations -= 1;
            superBlur.downsample = superBlur.iterations / 2;
            superBlur.interpolation = (float)superBlur.downsample / 4;
            yield return new WaitForSeconds(0.075f);
        }

        superBlur.downsample = 0;
        superBlur.iterations = 1;
        superBlur.interpolation = 0.0f;

        superBlur.enabled = false;

        BlurAnimation = null;
    }

    public void OnToggleButtons()
    {
        ToggleButtonGroup.SetActive(!ToggleButtonGroup.activeSelf);
    }

    public void OnEmptyUITouch()
    {
        OnShowUI();
    }

    public void OnToggleChat()
    {
        bool bUseChat = ChatOffToggleIcon.activeSelf;
        if (bUseChat)
            OnHideChatUI();
        else
            OnShowChatUI();

        ChatOnToggleIcon.SetActive(bUseChat);
        ChatOffToggleIcon.SetActive(!bUseChat);
    }

    public bool IsChatUIActive()
    {
        return ChatOffToggleIcon.activeSelf;
    }

    static Vector3[] colorTable = {
            new Vector3(200,200,200),
            new Vector3(145,216,252),
            new Vector3(144,228,126),
            new Vector3(252,168,78),
            new Vector3(254,124,142),
            new Vector3(208,124,254),
        };
    static public Vector3 GetLevelColorTable(uint level)
    {
        int index = 0;
        if (level <= 3)
        {
            index = 0;
        }
        else if (level < 8)
        {
            index = 1;
        }
        else if (level < 13)
        {
            index = 2;
        }
        else if (level < 18)
        {
            index = 3;
        }
        else if (level < 24)
        {
            index = 4;
        }
        else
        {
            index = 5;
        }

        return colorTable[index];
    }
    public void RefreshExpColor()
    {
        Vector3 color = GetLevelColorTable(curLevel);

        ExpBar.color = new Color(color.x / 255.0f, color.y / 255.0f, color.z / 255.0f);
    }

    public void OnSendMessageChat()
    {
        SendChatMessage = hahahaChat.InputFeild.text;
        hahahaChat.InputFeild.text = "";
    }

    public IEnumerator ChatRefreshPull()
    {
        uint level = 0;
        long myUserNo = NetworkManager.GetInstance().UserNo;
        string nick = SamandaLauncher.GetAccountNickName();
        users user = GameDataManager.Instance.GetUserData();
        string queryURL = "https://sandbox-gs.mynetgear.com/openchat";
        if(!isOpenChat)
        {
            queryURL = "https://sandbox-gs.mynetgear.com/personalchat";
        }

        if (user != null)
        {            
            object obj;
            if (user.data.TryGetValue("level", out obj))
            {
                level = (uint)obj;
            }
        }

        int tailSeq = 0;
        while (true)
        {
            JObject data = new JObject();
            
            if (string.IsNullOrEmpty(SendChatMessage))
            {
                data.Add("OpCode", 2);
            }
            else
            {
                data.Add("OpCode", 1);
                JObject content = new JObject();
                content.Add("SenderAccountNo", NetworkManager.GetInstance().UserNo.ToString());                
                content.Add("Sender", "[e960cdb67f2cb7488f16347705580180" + level + "e960cdb67f2cb7488f16347705580180]" + nick);
                content.Add("Message", SendChatMessage);
                content.Add("ProfileUrl", "");
                data.Add("Content", content);

                SendChatMessage = "";

                hahahaChat.ScrollRect.verticalNormalizedPosition = 0.0f;
            }

            if (!isOpenChat)
            {
                data.Add("FromUserNo", myUserNo);
                data.Add("ToUserNo", ChatRoomID);
                data.Add("Uri", "personalchat");
            }
            else
            {
                data.Add("Uri", "openchat");
            }

            data.Add("Rs", 0);
            data.Add("PId", SamandaLauncher.GetPID());
            data.Add("ReferenceSeq", tailSeq);
           
            string sendstring = data.ToString(Newtonsoft.Json.Formatting.None);

            UnityWebRequest req = UnityWebRequest.Put(queryURL, System.Text.Encoding.UTF8.GetBytes(sendstring));
            req.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");

            req.timeout = 10;
            yield return req.SendWebRequest();

            if (!req.isNetworkError && !req.isHttpError)
            {   
                string response = req.downloadHandler.text;
                JObject root = JObject.Parse(response);

                if (root.ContainsKey("Contents"))
                {
                    JArray contents = (JArray)root["Contents"];
                    //int count = contents.Count;
                    foreach (JToken chat in contents)
                    {
                        hahahaChat.OnChatMessage(chat["SenderAccountNo"].Value<string>(), chat["Sender"].Value<string>(), chat["Message"].Value<string>());
                        int seq = chat["Sequence"].Value<int>();
                        if (seq > tailSeq)
                        {
                            tailSeq = seq;
                        }

                        //count--;
                        //if (count < 5)
                        //    yield return new WaitForSeconds(0.01f);
                    }
                }
            }

            if (Chat_Panel)
            {
                ChatOffToggleIcon.SetActive(true);
                ChatOnToggleIcon.SetActive(false);
                foreach (DOTweenAnimation dotween in Chat_Panel.GetComponentsInChildren<DOTweenAnimation>())
                {
                    dotween.DOPlayForward();
                }
            }

            float time = 3.0f;
            while(time > 0 && string.IsNullOrEmpty(SendChatMessage))
            {
                yield return new WaitForEndOfFrame();
                time -= Time.deltaTime;
            }
        }
    }

    public IEnumerator PersnalChatRefresh()
    {
        long myUserNo = NetworkManager.GetInstance().UserNo;
        string queryURL = "https://sandbox-gs.mynetgear.com/personalchat";
        uint refrenceTime = 0;

        while (true)
        {
            JObject data = new JObject();
            data.Add("Uri", "personalchat");
            data.Add("OpCode", 4);
            data.Add("FromUserNo", myUserNo);
            data.Add("ReferenceTime", refrenceTime);
            data.Add("Rs", 0);
            data.Add("PId", SamandaLauncher.GetPID());

            string sendstring = data.ToString(Newtonsoft.Json.Formatting.None);

            UnityWebRequest req = UnityWebRequest.Put(queryURL, System.Text.Encoding.UTF8.GetBytes(sendstring));
            req.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");

            req.timeout = 10;
            yield return req.SendWebRequest();

            if (!req.isNetworkError && !req.isHttpError)
            {
                string response = req.downloadHandler.text;
                JObject root = JObject.Parse(response);

                if (root.ContainsKey("Contents"))
                {
                    List<UserProfile> myFriends = FriendsManager.Instance.GetFriendList();
                    JObject contents = (JObject)root["Contents"];

                    uint chatID = string.IsNullOrEmpty(ChatRoomID) ? 0 : Convert.ToUInt32(ChatRoomID);
                    foreach (JProperty contentProp in contents.Properties())
                    {
                        uint uno = Convert.ToUInt32(contentProp.Name);
                        foreach(UserProfile user in myFriends)
                        {
                            if(user.uno == uno)
                            {
                                JObject content = (JObject)contents[contentProp.Name];
                                
                                if(user.last_update < content["SendTime"].Value<uint>())
                                {
                                    user.lastMessage = content["Message"].Value<string>();
                                    user.last_update = content["SendTime"].Value<uint>();

                                    if (!FriendAlarm.activeInHierarchy)                                        
                                    {
                                        if (user.uno != chatID)
                                        {
                                            FriendAlarm.SetActive(true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                refrenceTime = Convert.ToUInt32(GameManager.GetCurTime());
            }

            float time = 3.0f;
            while (time > 0 && string.IsNullOrEmpty(SendChatMessage))
            {
                yield return new WaitForEndOfFrame();
                time -= Time.deltaTime;
            }
        }
    }

    public void OnFriendsButton()
    {
        FriendAlarm.SetActive(false);
        FriendsUI.gameObject.SetActive(true);
    }

    public void OnAllChatButton()
    {
        uint chatID = string.IsNullOrEmpty(ChatRoomID) ? 0 : Convert.ToUInt32(ChatRoomID);
        

        if (chatID != 0)
        {
            List<UserProfile> myFriends = FriendsManager.Instance.GetFriendList();
            foreach (UserProfile user in myFriends)
            {
                if (user.uno != chatID)
                {
                    user.UIShown(Convert.ToUInt32(GameManager.GetCurTime()));
                }
            }
        }

        ChatRoomID = "";
    }

    public void OnSendCardInfo(string cardInfo)
    {
        SendChatMessage = "[e960cdb67f2cb7488f16347705580180" + cardInfo + "e960cdb67f2cb7488f16347705580180]";
    }

    private void OnRoomChange()
    {
        ReturnToOpenChatButton.SetActive(!isOpenChat);
        StopChat();
        StartChat();
    }

    public void OnFriendAlarmCheck()
    {
        if (!FriendAlarm.activeInHierarchy)
        {
            if(FriendsManager.Instance.GetNewFriendCount() > 0 ||  FriendsManager.Instance.GetNewRecivedCount() > 0)
                FriendAlarm.SetActive(true);
        }
    }
}
