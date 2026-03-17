using DG.Tweening;
using Newtonsoft.Json.Linq;
using SuperBlur;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class WorldCanvas : CanvasControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public delegate void Callback();

    public enum STATE_WORLD { 
        WORLD_NONE,
        WORLD_FISHING,
        WORLD_COOK,
        WORLD_FEED,

        WORLD_MAP,
        WORLD_SEARCH,
        WORLD_INTERACTION,        
        WORLD_STAGE,
    };

    private STATE_WORLD curWorldState = STATE_WORLD.WORLD_NONE;
    private STATE_WORLD startedWorldState = STATE_WORLD.WORLD_NONE;
    public STATE_WORLD curWorldUI = STATE_WORLD.WORLD_MAP;

    private int curWorldIndex = -1;
    private GameObject CurTouchEffect = null;
    private Coroutine VideoAnimation = null;
    private Coroutine VideoLoadCoroutine = null;
    private Coroutine VolumeControlCoroutine = null;
    private Coroutine VibrateControlCoroutine = null;

    private uint curEventID = 0;
    private long ViberateTime = 0;
    private int ViberateAmplitude = 0;

    public VideoPlayer WorldVideoPlayer;
    public VideoPlayer GameCanvasVideoPlayer;

    public WorldMapUI WorldMapPanel;
    public Text DebugStateText;
    public RewardListUI RewardListUI;
    public GameObject ParticlePrefab;
    public SuperBlurFast superBlur;
    public GameObject CloseButtonObject;
    public GameObject InteractionTitlePanel;
    public Text InteractionTitle;

    public Canvas VideoBlurCanvas;
    public SearchAttachPanel SearchObject;
    public InteractionUIControl interactionUIControl;
    public GameObject AutoIcon;
    public GameObject gameCanvasBackgroundImage;
    public RawImage movieTexture;
    public RawImage BlurTexture;
    public GameObject SkipButton;
    public AudioSource audioSource;
    public AudioSource BackgroundAudioSource;

    List<JObject> rewardHistory = new List<JObject>();
    uint score = 0;
    stage curStageData = null;
    // Start is called before the first frame update
    void Start()
    {
        SkipButton.SetActive(false);
#if UNITY_EDITOR
        SkipButton.SetActive(true);
#endif
        InteractionTitlePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void SetCanvasState(GameMain.HahahaState state)
    {
        bool visible = state == GameMain.HahahaState.HAHAHA_WORLD;
        if (visible == gameObject.activeSelf)
            return;

        gameObject.SetActive(true);
        if (visible)
        {
            foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
            {
                dotween.DOPlayForward();
            }
            CancelInvoke("OnCompleteTweenAnimation");
        }
        else
        {
            foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
            {
                dotween.DOPlayBackwards();
            }
            Invoke("OnCompleteTweenAnimation", 0.5f);
        }
    }

    public void OnCompleteTweenAnimation()
    {
        gameObject.SetActive(false);
    }

    public void OnCloseButton()
    {
        switch (curWorldState)
        {
            case STATE_WORLD.WORLD_MAP:
            case STATE_WORLD.WORLD_STAGE:
                {
                    WorldMapPanel.OnClose();
                }
                break;
            case STATE_WORLD.WORLD_INTERACTION:
            case STATE_WORLD.WORLD_SEARCH:
                OnWorldMapState();
                break;                            
        }
    }
    public void OnWorldMapState()
    {
        Invoke("ShowRewardPopup", 0.1f);
        SearchObject.SearchDone();
        SetWorldState(curWorldUI);
        startedWorldState = curWorldUI;

        AutoIcon.SetActive(false);
    }

    public void OnFeedState(uint itemID)
    {
        curWorldIndex = -1;
        SetWorldState(STATE_WORLD.WORLD_FEED);

        WWWForm data = new WWWForm();
        data.AddField("api", "walk");
        data.AddField("area", itemID.ToString());
        data.AddField("auto", 0);
        data.AddField("atype", 3);

        NetworkManager.GetInstance().SendApiRequest("walk", 1, data, ResponseEventData);

        startedWorldState = STATE_WORLD.WORLD_FEED;
    }

    public void OnWorldMapSelect(uint index)
    {
        game_data targetWorld = null;
        object obj;
        List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.WALK_AREA);
        foreach (game_data data in data_list)
        {
            if (data.data.TryGetValue("walk_id", out obj))
            {
                if (index == (uint)obj)
                    targetWorld = data;
            }
        }

        if (targetWorld != null)
        {
            curWorldIndex = (int)index;

            WWWForm data = new WWWForm();
            data.AddField("api", "walk");
            data.AddField("area", curWorldIndex);
            data.AddField("auto", 0);
            NetworkManager.GetInstance().SendApiRequest("walk", 1, data, ResponseEventData);
        }
        else
        {
            Debug.LogError("알수없는 월드 정보입니다.");
            OnWorldMapState();
        }
    }

    public void OnStageSelected(stage stage)
    {
        if (stage == null)
            return;

        curStageData = stage;
        AutoIcon.SetActive(false);

        WWWForm data = new WWWForm();
        data.AddField("api", "adventure");
        data.AddField("stage", stage.GetStageID().ToString());
        NetworkManager.GetInstance().SendApiRequest("adventure", 1, data, ResponseEventData);
        score = 0;
    }

    public void OnAutoStageSelected(List<uint> listStageID)
    {
        curStageData = null;
        AutoIcon.SetActive(true);
        
        WWWForm data = new WWWForm();
        data.AddField("api", "adventure");
        data.AddField("stages", string.Join(",", listStageID));
        NetworkManager.GetInstance().SendApiRequest("adventure", 2, data, ResponseEventData);
        score = 0;
    }

    public void ClearVideo()
    {
        interactionUIControl.gameObject.SetActive(false);

        RenderTexture target = WorldVideoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        WorldVideoPlayer.clip = null;
        CancelInvoke("OnWorldInteractionDone");
    }

    public IEnumerator VideoLoad(clip_event eventData, string invokeMethodName)
    {
        WorldVideoPlayer.Stop();
        VideoClip preClip = WorldVideoPlayer.clip;
        Resources.UnloadAsset(preClip);

        RenderTexture target = WorldVideoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        ResourceRequest req = Resources.LoadAsync<VideoClip>(eventData.GetClipPath());
        while (!req.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        VideoClip clip = (VideoClip)req.asset;
        if (clip == null)
        {
            OnWorldMapState();
            yield return null;
        }

        WorldVideoPlayer.playOnAwake = false;

        WorldVideoPlayer.gameObject.SetActive(true);
        WorldVideoPlayer.clip = clip;
        WorldVideoPlayer.isLooping = false;
        WorldVideoPlayer.time = 0;

        WorldVideoPlayer.Prepare();
        while (!WorldVideoPlayer.isPrepared)
        {
            yield return new WaitForEndOfFrame();
        }

        if (GameCanvasVideoPlayer.gameObject.activeSelf)
        {
            if (GameCanvasVideoPlayer.clip != null)
                GameCanvasVideoPlayer.Stop();

            RenderTexture gct = GameCanvasVideoPlayer.targetTexture;
            if (gct)
            {
                RenderTexture rt = RenderTexture.active;
                RenderTexture.active = gct;
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = rt;
            }
        }


        curEventID = eventData.GetEventID();

        if (clip.audioTrackCount > 0)
        {
            BackgroundAudioSource.DOFade(0.0f, 0.3f);

            float volume = PlayerPrefs.GetInt("Config_BV", 9) / 9;
            if(PlayerPrefs.GetInt("Config_BS", 1) <= 0)
            {
                volume = 0.0f;
            }

            for (ushort i = 0; i < WorldVideoPlayer.audioTrackCount; i++)
            {
                WorldVideoPlayer.SetDirectAudioVolume(i, volume);
            }
        }

        WorldVideoPlayer.Play();

        interactionUIControl.SetClipData(eventData);

        if (VideoAnimation != null)
            StopCoroutine(VideoAnimation);
        VideoAnimation = StartCoroutine(OnVideoFadeIn());

        Invoke(invokeMethodName, (float)WorldVideoPlayer.length);
    }

    public void OnWorldSearchState(clip_event eventData, int prefabIndex)
    {
        SetWorldState(STATE_WORLD.WORLD_SEARCH);
        SearchObject.OnSearch(eventData, prefabIndex);
        startedWorldState = curWorldUI;
    }

    public void OnInteractionState(clip_event eventData)
    {
        SetWorldState(STATE_WORLD.WORLD_INTERACTION);

        if (VideoLoadCoroutine != null)
            StopCoroutine(VideoLoadCoroutine);

        VideoLoadCoroutine = StartCoroutine(VideoLoad(eventData, "OnWorldInteractionDone"));

        WorldVideoPlayer.aspectRatio = VideoAspectRatio.FitInside;
    }

    public void OnWorldSearchDone()
    {
        CancelInvoke("OnWorldSearchDone");
        if (STATE_WORLD.WORLD_SEARCH != curWorldState)
            return;

        if (VideoAnimation != null)
            StopCoroutine(VideoAnimation);

        ReqNextStep();
    }

    void ReqNextStep()
    {
        if (curWorldUI == STATE_WORLD.WORLD_MAP)
        {
            WWWForm data = new WWWForm();
            data.AddField("api", "walk");
            data.AddField("area", curWorldIndex);
            data.AddField("auto", 0);
            NetworkManager.GetInstance().SendApiRequest("walk", 2, data, ResponseEventData);
        }
        else
        {
            WWWForm data = new WWWForm();
            data.AddField("api", "adventure");
            data.AddField("op", 4);
            NetworkManager.GetInstance().SendApiRequest("adventure", 4, data, ResponseEventData);
        }
    }

    public void OnInteractionEffect(clip_event eventData)
    {
        CloseButtonObject.SetActive(false);
        SearchObject.SearchDone();
        ClearVideo();
        StartCoroutine(InteractionInitEffect(eventData));
    }

    public IEnumerator InteractionInitEffect(clip_event eventData)
    {
        InteractionTitle.text = "";
        if (!string.IsNullOrEmpty(eventData.GetClipTitle()))
        {
            InteractionTitlePanel.SetActive(true);
            InteractionTitle.text = eventData.GetClipTitle();

            Color color = InteractionTitle.color;
            color.a = 0.0f;
            while (color.a < 1.0f)
            {
                color.a += 0.1f;
                InteractionTitle.color = color;
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.5f);

            while (color.a > 0.0f)
            {
                color.a -= 0.2f;
                InteractionTitle.color = color;
                yield return new WaitForSeconds(0.1f);
            }
        }

        InteractionTitlePanel.SetActive(false);
        OnInteractionState(eventData);
    }

    public void OnWorldInteractionDone()
    {
        CancelInvoke("OnWorldInteractionDone");

        if (GameCanvasVideoPlayer.gameObject.activeSelf)
        {
            if(GameCanvasVideoPlayer.clip != null)
                GameCanvasVideoPlayer.Play();
        }

        BackgroundAudioSource.DOFade((float)PlayerPrefs.GetInt("Config_BV", 9) / 9, 0.3f);
        bool mute = PlayerPrefs.GetInt("Config_BS", 1) == 0;
        BackgroundAudioSource.mute = mute;

        if (STATE_WORLD.WORLD_INTERACTION != curWorldState)
            return;

        ClearInteractionObjects();
        
        if (VideoAnimation != null)
            StopCoroutine(VideoAnimation);

        VideoAnimation = StartCoroutine(OnVideoFadeOut(() => {
            if(curEventID == 103 && ContentLocker.GetCurContentSeq() < 28)
            {                
                SendTryInteraction(clip_event.CLIP_EVENT_TYPE.TOUCH);
                return;
            }

            SendNextStep();
        }));
    }

    public void SendNextStep()
    {
        if(curWorldUI == STATE_WORLD.WORLD_MAP)
        {
            WWWForm data = new WWWForm();
            data.AddField("api", "walk");
            data.AddField("area", curWorldIndex);
            data.AddField("state", 2);
            data.AddField("auto", 0);
            NetworkManager.GetInstance().SendApiRequest("walk", 2, data, ResponseEventData);
        }
        else
        {
            WWWForm data = new WWWForm();
            data.AddField("api", "adventure");            
            data.AddField("op", 4);
            data.AddField("score", score.ToString());            
            NetworkManager.GetInstance().SendApiRequest("adventure", 4, data, ResponseEventData);
        }

        score = 0;
    }

    public void SetWorldState(STATE_WORLD state)
    {
        RenderTexture target = WorldVideoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        if (curWorldState == state)
            return;

        curWorldState = state;
        bool showUI = false;
        bool worldMapUI = false;
        switch (curWorldState)
        {
            case STATE_WORLD.WORLD_FEED:
                gameCanvasBackgroundImage.SetActive(true);
                WorldVideoPlayer.gameObject.SetActive(false);
                WorldMapPanel.gameObject.SetActive(true);
                WorldMapPanel.GetComponent<DOTweenAnimation>().DOPlayForward();
                CloseButtonObject.SetActive(false);
                DebugStateText.text = "밥주기 선택";
                showUI = true;
                break;
            case STATE_WORLD.WORLD_COOK:
                gameCanvasBackgroundImage.SetActive(true);
                WorldVideoPlayer.gameObject.SetActive(false);
                WorldMapPanel.gameObject.SetActive(true);
                WorldMapPanel.GetComponent<DOTweenAnimation>().DOPlayForward();
                CloseButtonObject.SetActive(false);
                DebugStateText.text = "밥주기 선택";
                showUI = true;
                break;
            case STATE_WORLD.WORLD_MAP:
            case STATE_WORLD.WORLD_STAGE:
                gameCanvasBackgroundImage.SetActive(true);
                WorldVideoPlayer.gameObject.SetActive(false);
                WorldMapPanel.gameObject.SetActive(true);
                WorldMapPanel.GetComponent<DOTweenAnimation>().DOPlayForward();
                CloseButtonObject.SetActive(false);                
                DebugStateText.text = "월드맵 선택";
                showUI = true;
                worldMapUI = curWorldState == STATE_WORLD.WORLD_MAP;
                break;
            case STATE_WORLD.WORLD_SEARCH:
                gameCanvasBackgroundImage.SetActive(false);
                WorldVideoPlayer.gameObject.SetActive(false);
                WorldMapPanel.GetComponent<DOTweenAnimation>().DOPlayBackwards();

                CloseButtonObject.SetActive(ContentLocker.GetCurContentSeq() >= 36);
                DOTweenAnimation anim = CloseButtonObject.GetComponent<DOTweenAnimation>();
                if (anim)
                {
                    anim.DORestart();
                    anim.DOPlayForward();
                }
                DebugStateText.text = "탐색 중";
                break;
            case STATE_WORLD.WORLD_INTERACTION:
                gameCanvasBackgroundImage.SetActive(false);
                WorldVideoPlayer.gameObject.SetActive(true);
                WorldMapPanel.GetComponent<DOTweenAnimation>().DOPlayBackwards();
                CloseButtonObject.SetActive(false);                
                DebugStateText.text = "인터랙션 (활동 없음)";
                break;
        }
        
        WorldMapPanel.OnUIRefresh(worldMapUI);

        ((GameCanvas)GameManager.GameCanvas).StatusUIGroup.SetActive(showUI);
        ((GameCanvas)GameManager.GameCanvas).InteractionGroup.SetActive(showUI);
        //VideoBlurCanvas.gameObject.SetActive(curWorldState == STATE_WORLD.WORLD_INTERACTION);
        VideoBlurCanvas.gameObject.SetActive(false);
        interactionUIControl.gameObject.SetActive(false);
        
        superBlur.enabled = showUI;
        if (showUI)
        {
            superBlur.iterations = 8;
            superBlur.downsample = 1;
        }
        else
        {
            superBlur.iterations = 1;
            superBlur.downsample = 0;
        }
        
        CancelInvoke("OnWorldSearchDone");
        CancelInvoke("OnWorldInteractionDone");
        ClearInteractionObjects();
    }

    public IEnumerator OnVideoFadeIn()
    {
        RawImage renderImage = movieTexture;
        Color color = renderImage.color;

        while (color.r < 1.0f)
        {
            float next = color.r + 0.05f;
            if (next > 1.0f)
                next = 1.0f;
            
            color.r = next;
            color.g = next;
            color.b = next;
            renderImage.color = color;
            BlurTexture.color = color;
            yield return new WaitForSeconds(0.05f);
        }

        color.r = 1.0f;
        color.g = 1.0f;
        color.b = 1.0f;
        renderImage.color = color;
        BlurTexture.color = color;
    }

    public IEnumerator OnVideoFadeOut(Callback cb)
    {
		interactionUIControl.OnVideoFadeOut();
        
        RawImage renderImage = movieTexture;
        Color color = renderImage.color;

        while (color.r > 0.0f)
        {
            float next = color.r - 0.05f;
            if (next < 0.0f)
                next = 0.0f;
            color.r = next;
            color.g = next;
            color.b = next;
            renderImage.color = color;
            BlurTexture.color = color;
            yield return new WaitForSeconds(0.05f);
        }

        color.r = 0.0f;
        color.g = 0.0f;
        color.b = 0.0f;
        renderImage.color = color;
        BlurTexture.color = color;
        cb.Invoke();
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (STATE_WORLD.WORLD_INTERACTION != curWorldState)
            return;
        if (!interactionUIControl.IsParticleInteraction())
            return;

        DebugStateText.text = "인터랙션 (활동)";

        ClearInteractionObjects();

        CurTouchEffect = Instantiate(ParticlePrefab);
        CurTouchEffect.transform.SetParent(Camera.main.transform);
        //CurTouchEffect.transform.localScale = Vector3.one;
        Vector3 localpos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(data.position));
        localpos.z = Camera.main.transform.position.z * -1;
        CurTouchEffect.transform.localPosition = localpos;

        if (VolumeControlCoroutine != null)
            StopCoroutine(VolumeControlCoroutine);

        VolumeControlCoroutine = StartCoroutine(VolumeControl(PlayerPrefs.GetInt("Config_EV", 9) / 9));

        if (VibrateControlCoroutine != null)
        {
            StopCoroutine(VibrateControlCoroutine);
            VibrateControlCoroutine = null;
        }

        if(PlayerPrefs.GetInt("Config_VB", 1) > 0)
            RDG.Vibration.Vibrate(500, 10, true);

        score++;
    }

    public void OnDrag(PointerEventData data)
    {
        if (STATE_WORLD.WORLD_INTERACTION != curWorldState)
            return;
        if (!interactionUIControl.IsParticleInteraction())
            return;

        if (CurTouchEffect != null)
        {
            Vector3 localpos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(data.position));
            localpos.z = Camera.main.transform.position.z * -1;
            CurTouchEffect.transform.localPosition = localpos;
        }

        if (VibrateControlCoroutine == null)
        {
            if (PlayerPrefs.GetInt("Config_VB", 1) > 0)
                VibrateControlCoroutine = StartCoroutine(ViberateControl());
        }        
    }

    public void OnPointerUp(PointerEventData data)
    {
        audioSource.loop = false;
        ClearInteractionObjects();

        if (VolumeControlCoroutine != null)
            StopCoroutine(VolumeControlCoroutine);

        VolumeControlCoroutine = StartCoroutine(VolumeControl(0.0f));

        if (STATE_WORLD.WORLD_INTERACTION != curWorldState)
        {   
            return;
        }
        if (!interactionUIControl.IsParticleInteraction())
        {
            return;
        }

        if (PlayerPrefs.GetInt("Config_VB", 1) > 0)
            RDG.Vibration.Vibrate(500, 10, true);
    }

    public IEnumerator ViberateControl()
    {
        long playTime = 500;
        int[] amp = { 20, 40, 60, 40, 20, 10 };
        int index = 0;
        while(true)
        {
            RDG.Vibration.Vibrate(playTime, amp[index], true);
            index = (index + 1) % amp.Length;

            yield return new WaitForSeconds(playTime * 0.001f);
        }
    }

    public IEnumerator VolumeControl(float targetVolume)
    {
        float curVolume = audioSource.volume;
        audioSource.loop = true;

        if (targetVolume == 0.0f && curVolume == 0.0f)
        {
            audioSource.Stop();
        }
        else
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            if (curVolume > targetVolume)
            {
                while (curVolume > targetVolume)
                {
                    curVolume = curVolume - (1.0f * Time.deltaTime);
                    audioSource.volume = curVolume;
                    yield return new WaitForEndOfFrame();
                }

                audioSource.volume = targetVolume;
            }
            else if (curVolume < targetVolume)
            {
                while (curVolume < targetVolume)
                {
                    curVolume = curVolume + (1.0f * Time.deltaTime);
                    audioSource.volume = curVolume;
                    yield return new WaitForEndOfFrame();
                }

                audioSource.volume = targetVolume;
            }

            if(targetVolume == 0.0f)
            {
                audioSource.Stop();
            }
        }
    }


    public void ClearInteractionObjects()
    {
        if (CurTouchEffect != null)
        {
            var particle = CurTouchEffect.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particle)
            {
                var em = ps.emission;
                em.enabled = false;
            }

            Destroy(CurTouchEffect, 3.0f);
            CurTouchEffect = null;
        }

        if (VolumeControlCoroutine != null)
            StopCoroutine(VolumeControlCoroutine);

        //if (gameObject.activeInHierarchy)
        //{
        //    VolumeControlCoroutine = StartCoroutine(VolumeControl(0.0f));
        //}
        //else
        {
            VolumeControlCoroutine = null;
            audioSource.volume = 0.0f;
            audioSource.Stop();

            if (VibrateControlCoroutine != null)
            {
                StopCoroutine(VibrateControlCoroutine);
                VibrateControlCoroutine = null;
            }
        }
    }

    public void ResponseEventData(string response)
    {
        JObject root = JObject.Parse(response);

        JToken apiToken = root["api"];
        if (null == apiToken || apiToken.Type != JTokenType.Array
            || !apiToken.HasValues)
        {
            OnWorldMapState();
            return;
        }

        uint result = 0;
        uint eventNo = 0;
        JArray apiArr = (JArray)apiToken;
        foreach (JObject row in apiArr)
        {
            string uri = row.GetValue("uri").ToString();
            if (uri == "walk" || uri == "adventure")
            {                
                JToken rsToken = row["rs"];
                JToken opCode = row["op"];

                if (rsToken != null && rsToken.Type == JTokenType.Integer)
                {
                    int rs = rsToken.Value<int>();
                    if (rs != 0)
                    { 
                        if((uri == "walk" && rs == 2) || (uri == "adventure" && rs == 4))
                        {
                            JToken expToken = row["exp"];
                            if(expToken != null && expToken.Type == JTokenType.Integer)
                            {
                                int exp = expToken.Value<int>();                                
                                Invoke("ReqNextStep", exp - GameManager.GetCurTime());
                            }
                        }

                        if(uri == "adventure")
                        {
                            if (rs == 1)
                            {
                                int op = opCode.Value<int>();
                                if (op == 1)
                                {
                                    GameManager.PopupControl.OnPopupToast("배가 고픈 고양이는 만날 수 없어요. 밥주기를 해볼까요?");
                                }
                            }
                            else if (rs != 0)
                            {
                                int op = opCode.Value<int>();
                                if (op == 1)
                                {
                                    GameManager.PopupControl.OnPopupToast("데이터 오류가 발생했습니다. (" +rs + ")");
                                }
                            }
                        }
                    }
                }
                
                if (opCode != null && opCode.Type == JTokenType.Integer)
                {
                    int op = opCode.Value<int>();
                    if (op == 101)
                    {
                        JToken stateVal = row.GetValue("state");
                        if (stateVal != null)
                        {
                            result = stateVal.Value<uint>();
                            if (result == 2 || result == 1)
                            {
                                JToken evtVal = row.GetValue(uri == "walk" ? "event" : "clip");
                                if (evtVal != null)
                                    eventNo = evtVal.Value<uint>();
                            }
                        }
                    }
                    else if (uri == "adventure")
                    {
                        if (op == 7)
                        {
                            result = 7;
                        }
                        if (op == 8)
                        {
                            result = 8;
                        }
                    }
                }
                JObject income = (JObject)row.GetValue("rew");
                if (income != null)
                {
                    rewardHistory.Add(income);
                }
            }
        }

        if (result == 1 || result == 2)
        {
            List<game_data> list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);
            clip_event eventData = null;
            if (list != null)
            {
                foreach(game_data data in list)
                {
                    if (eventNo == ((clip_event)data).GetEventID())
                    {
                        eventData = (clip_event)data;
                    }
                }
            }

            if (eventData == null)
            {
                OnWorldMapState();
                return;
            }

            string path = eventData.GetClipPath();
            string[] typeCheck = path.Split(':');
            if (typeCheck.Length == 2)
            {
                if (typeCheck[0] == "prefab")
                {
                    OnWorldSearchState(eventData, int.Parse(typeCheck[1]));
                    return;
                }
            }

            OnInteractionEffect(eventData);
        }
        else if (result == 3)
        {
            if (curWorldIndex == 2)
            {
                if(rewardHistory.Count == 0)
                {
                    GameManager.PopupControl.OnPopupToast("낚시에 실패하였습니다.");
                }

                OnWorldMapState();
            }
            else
            {
                switch (startedWorldState)
                {
                    case STATE_WORLD.WORLD_MAP:
                    case STATE_WORLD.WORLD_STAGE:
                        OnWorldMapState();
                        break;
                    
                    case STATE_WORLD.WORLD_FEED:
                        Invoke("ShowRewardPopup", 0.1f);
                        SetWorldState(WorldCanvas.STATE_WORLD.WORLD_FEED);//for tutorial
                        WorldMapPanel.foodListUI.ShowFoodList();
                        break;
                }
            }
        }
        else if (result == 7 || result == 8)
        {
            Invoke("ShowRewardPopup", 0.1f);
        }
    }

    public void SendTryInteraction(clip_event.CLIP_EVENT_TYPE type)
    {
        string api = "";
        WWWForm data = new WWWForm();
        if (curWorldUI == STATE_WORLD.WORLD_MAP)
            api = "walk";
        else
            api = "adventure";

        data.AddField("api", api);
        data.AddField("type", (int)type);
        data.AddField("auto", 0);
        NetworkManager.GetInstance().SendApiRequest(api, 3, data, ResponseEventData);
    }

    public void ShowRewardPopup()
    {
        bool bRewardGet = false;
        JObject obj = new JObject();
        obj.Add("gold", 0);
        obj.Add("exp", 0);
        obj.Add("touch", new JArray());
        obj.Add("play", new JArray());
        obj.Add("card", new JArray());
        obj.Add("item", new JArray());
        obj.Add("food", new JArray());

        foreach (JObject income in rewardHistory)
        {
            if (income.ContainsKey("gold"))
            {
                bRewardGet = true;
                obj["gold"] = obj["gold"].Value<uint>() + income["gold"].Value<uint>();
            }
            if (income.ContainsKey("exp"))
            {
                bRewardGet = true;
                obj["exp"] = obj["exp"].Value<uint>() + income["exp"].Value<uint>();
            }
            if (income.ContainsKey("touch"))
            {
                foreach (JObject val in income["touch"])
                {
                    bRewardGet = true;
                    ((JArray)obj["touch"]).Add(val);
                }
            }
            if (income.ContainsKey("play"))
            {
                foreach (JObject val in income["play"])
                {
                    bRewardGet = true;
                    ((JArray)obj["play"]).Add(val);
                }
            }
            if (income.ContainsKey("card"))
            {
                JArray arr = (JArray)income["card"];
                foreach (JToken val in arr)
                {
                    bRewardGet = true;
                    ((JArray)obj["card"]).Add(val);
                }
            }
            if (income.ContainsKey("item"))
            {
                foreach (JObject val in income["item"])
                {
                    bRewardGet = true;
                    ((JArray)obj["item"]).Add(val);
                }
            }
            if (income.ContainsKey("food"))
            {
                foreach (JObject val in income["food"])
                {
                    bRewardGet = true;
                    ((JArray)obj["food"]).Add(val);
                }
            }
        }

        if (bRewardGet)
        {
            if (((JArray)obj["touch"]).Count <= 0)
            {
                obj.Remove("touch");
            }
            if (((JArray)obj["play"]).Count <= 0)
            {
                obj.Remove("play");
            }
            if (((JArray)obj["card"]).Count <= 0)
            {
                obj.Remove("card");
            }
            if (((JArray)obj["item"]).Count <= 0)
            {
                obj.Remove("item");
            }
            if (((JArray)obj["food"]).Count <= 0)
            {
                obj.Remove("food");
            }

            STATE_WORLD state = startedWorldState;
            if (curWorldIndex == 2)
                state = STATE_WORLD.WORLD_FISHING;

            RewardListUI.ShowTouchRewardList(obj, state, curStageData);
            curStageData = null;
        }

        rewardHistory.Clear();
    }


    void OnDisable()
    {
        if (GameManager != null)
        {
            ((GameCanvas)GameManager.GameCanvas).StatusUIGroup.SetActive(true);
            ((GameCanvas)GameManager.GameCanvas).InteractionGroup.SetActive(true);
        }

        WorldMapPanel.curSelected = 0;        
    }

    public void OnSkip()
    {
        OnWorldInteractionDone();
    }
}
