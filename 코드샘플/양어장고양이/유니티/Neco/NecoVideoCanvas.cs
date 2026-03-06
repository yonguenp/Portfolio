using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class NecoVideoCanvas : NecoCanvas, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public delegate void Callback();

    public InteractionUIControl interactionUIControl;
    public SearchAttachPanel SearchObject;

    //public VideoPlayer VideoPlayer;
    //public AudioSource BackgroundAudioSource;
    public AudioSource EffectAudioSource;
    public RenderTexture videoRenderTexture;
    //public AudioSource VideoAudioSource;
    public RawImage movieTexture;
    public GameObject InteractionTitlePanel;
    public Text InteractionTitle;
    public GameObject ParticlePrefab;

    public Font HoonminFont;
    public Font JapFont;

    private uint score = 0;
    private Coroutine VideoAnimation = null;
    private Coroutine VideoLoadCoroutine = null;
    private Coroutine VolumeControlCoroutine = null;
    private Coroutine VibrateControlCoroutine = null;
    private GameObject CurTouchEffect = null;
    private clip_event curClipData = null;
    private Callback VideoDoneCallback = null;

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

    bool displayControl = false;
    protected override void Awake()
    {
        base.Awake();
        gameObject.SetActive(false);
        Application.targetFrameRate = 30;
    }


    void SetDisplayControl(bool use)
    {
        displayControl = use;

        movieTexture.transform.localScale = Vector3.one;
        if(displayControl)
        {
            movieTexture.transform.localScale = Vector3.zero;
            PictureFixPosCheck();
        }
    }

    private void figureDelta()
    {
        // when/if two touches, keep track of the distance between them
        distance = Vector2.Distance(posA, posB);
    }
    private void _processPinch()
    {
        Vector2 canvasSize = (NecoCanvas.GetVideoCanvas().transform as RectTransform).sizeDelta;

        RectTransform rt = movieTexture.transform as RectTransform;
        Vector2 videoSize = rt.sizeDelta;

        float ratio = movieTexture.transform.localScale.x + (pinchDelta / videoSize.x);
        movieTexture.transform.localScale = Vector3.one * ratio;
    }

    public void ResizedMovieRawImage()
    {
        if (displayControl)
        {
            movieTexture.transform.localScale = Vector3.zero;
            PictureFixPosCheck();
        }
    }

    private void PictureFixPosCheck()
    {
        Vector2 canvasSize = (NecoCanvas.GetVideoCanvas().transform as RectTransform).sizeDelta;

        RectTransform rt = movieTexture.transform as RectTransform;
        Vector2 videoSize = rt.sizeDelta;

        float minRatio = Mathf.Min(canvasSize.x / videoSize.x, canvasSize.y / videoSize.y);
        if(movieTexture.transform.localScale.x < minRatio)
        {
            movieTexture.transform.localScale = Vector3.one * minRatio;
        }
        float maxRatio = 1.0f;
        if (movieTexture.transform.localScale.x > maxRatio)
        {
            movieTexture.transform.localScale = Vector3.one * maxRatio;
        }
    }

    public void OnVideoPlayWithDisplayControl(uint clipID, Callback cb = null)
    {
        List<game_data> clip_event = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);
        if (clip_event != null)
        {
            foreach (clip_event clip in clip_event)
            {
                if (clip != null)
                {
                    if (clip.GetEventID() == clipID)
                    {
                        OnVideoPlay(clip, cb);
                        SetDisplayControl(true);
                        return;
                    }
                }
            }
        }
    }

    public void OnVideoPlay(uint clipID, Callback cb = null)
    {
        List<game_data> clip_event = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);
        if (clip_event != null)
        {
            foreach (clip_event clip in clip_event)
            {
                if (clip != null)
                {
                    if (clip.GetEventID() == clipID)
                    {
                        OnVideoPlay(clip, cb);
                        return;
                    }
                }
            }
        }
    }

    public void OnVideoPlay(clip_event clip, Callback cb = null)
    {
        //neco_data.ClientDEBUG_Seq seq = neco_data.GetDebugSeq();

        //if (neco_data.ClientDEBUG_Seq.NONE == seq)
        //{
        //    neco_data.SetDebugSeq(neco_data.ClientDEBUG_Seq.MOTHER_FOOD);
        //}

        SetDisplayControl(false);
        VideoDoneCallback = cb;
        gameObject.SetActive(true);

        if (clip.GetEventID() == 0)
        {
            ExitVideo();
            return;
        }

        string path = clip.GetClipPath();
        string[] typeCheck = path.Split(':');
        if (typeCheck.Length == 2)
        {
            if (typeCheck[0] == "prefab")
            {
                OnSearchState(clip, int.Parse(typeCheck[1]));
                return;
            }
        }

        curClipData = clip;
        OnInteractionEffect(clip);
    }

    public void OnTestVideoPlay()
    {
        List<game_data> clip_event = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);
        if (clip_event != null)
        {
            foreach (clip_event clip in clip_event)
            {
                if (clip != null)
                {
                    OnInteractionState(clip);
                    clip_event.Remove(clip);
                    return;
                }
            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if(displayControl)
        {
            if (currentMainFinger == -1)
            {
                currentMainFinger = eventData.pointerId;
                prevPoint = eventData.position;

                posA = eventData.position;

                return;
            }

            if (currentSecondFinger == -1)
            {
                currentSecondFinger = eventData.pointerId;
                posB = eventData.position;

                figureDelta();
                previousDistance = distance;

                return;
            }

            Debug.Log("third+ finger! (ignore)");
            return;
        }

        if (!interactionUIControl.IsParticleInteraction())
            return;

        ClearInteractionObjects();

        CurTouchEffect = Instantiate(ParticlePrefab);
        CurTouchEffect.transform.SetParent(Camera.main.transform);
        //CurTouchEffect.transform.localScale = Vector3.one;
        Vector3 localpos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(eventData.position));
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

        if (PlayerPrefs.GetInt("Setting_Vibration", 1) > 0)
            RDG.Vibration.Vibrate(500, 10, true);

        //movieTexture.transform.DORewind();
        //movieTexture.transform.DOKill();
        //movieTexture.transform.DOShakePosition(10.0f, 10.0f).SetLoops(-1, LoopType.Yoyo);

        score++;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (displayControl)
        {
            if (currentMainFinger == eventData.pointerId)
            {
                newPoint = eventData.position;
                screenTravel = newPoint - prevPoint;
                prevPoint = newPoint;

                if (currentSecondFinger == -1)
                {
                    //_processSwipe();
                }
                else
                {

                }

                posA = eventData.position;
            }

            if (currentSecondFinger == -1) return;

            if (currentMainFinger == eventData.pointerId) posA = eventData.position;
            if (currentSecondFinger == eventData.pointerId) posB = eventData.position;

            figureDelta();
            pinchDelta = distance - previousDistance;
            previousDistance = distance;

            _processPinch();

            return;
        }

        if (!interactionUIControl.IsParticleInteraction())
            return;

        if (CurTouchEffect != null)
        {
            Vector3 localpos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(eventData.position));
            localpos.z = Camera.main.transform.position.z * -1;
            CurTouchEffect.transform.localPosition = localpos;
        }

        if (VibrateControlCoroutine == null)
        {
            if (PlayerPrefs.GetInt("Setting_Vibration", 1) > 0)
                VibrateControlCoroutine = StartCoroutine(ViberateControl());
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.길막이만지기돌발발생)
        {
            Transform TouchTutorial_popup = transform.Find("TouchTutorial_popup");
            if (TouchTutorial_popup != null)
            {
                TouchTutorial_popup.GetComponent<TouchTutorial>().OnTouchThisObject();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (displayControl)
        {
            if (currentMainFinger == eventData.pointerId)
            {
                currentMainFinger = -1;
            }
            if (currentSecondFinger == eventData.pointerId)
            {
                currentSecondFinger = -1;
            }

            PictureFixPosCheck();

            return;
        }
        EffectAudioSource.loop = false;
        ClearInteractionObjects();

        if (VolumeControlCoroutine != null)
            StopCoroutine(VolumeControlCoroutine);

        VolumeControlCoroutine = StartCoroutine(VolumeControl(0.0f));

        if (!interactionUIControl.IsParticleInteraction())
            return;

        if (PlayerPrefs.GetInt("Setting_Vibration", 1) > 0)
            RDG.Vibration.Vibrate(500, 10, true);

        movieTexture.transform.DORewind();
        movieTexture.transform.DOKill();
    }

    public void ClearVideo()
    {
        SearchObject.SearchDone();
        interactionUIControl.gameObject.SetActive(false);

        CancelInvoke("OnInteractionDone");
    }

    public IEnumerator VideoLoad(clip_event eventData, string invokeMethodName)
    {
        VideoManager.GetInstance().StopVideo(false);
        interactionUIControl.gameObject.SetActive(false);

        //ResourceRequest req = Resources.LoadAsync<VideoClip>(eventData.GetClipPath());
        //while (!req.isDone)
        //{
        //    yield return new WaitForEndOfFrame();
        //}

        //VideoClip clip = (VideoClip)req.asset;
        //if (clip == null)
        //{
        //    yield return null;
        //}

        VideoClip clip = Resources.Load<VideoClip>(eventData.GetClipPath());

        yield return VideoManager.GetInstance().SetVideoCoroutine(clip, videoRenderTexture);

        //if (GameCanvasVideoPlayer.gameObject.activeSelf)
        //{
        //    if (GameCanvasVideoPlayer.clip != null)
        //        GameCanvasVideoPlayer.Stop();

        //    RenderTexture gct = GameCanvasVideoPlayer.targetTexture;
        //    if (gct)
        //    {
        //        RenderTexture rt = RenderTexture.active;
        //        RenderTexture.active = gct;
        //        GL.Clear(true, true, Color.clear);
        //        RenderTexture.active = rt;
        //    }
        //}

        curClipData = eventData;

        interactionUIControl.SetClipData(eventData);

        if (VideoAnimation != null)
            StopCoroutine(VideoAnimation);
        VideoAnimation = StartCoroutine(OnVideoFadeIn());

        yield return VideoAnimation;

        Invoke(invokeMethodName, (float)clip.length);

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.길막이만지기돌발발생 && CurTouchEffect == null)
        {
            CancelInvoke(invokeMethodName);
            GameObject popup = Instantiate(Resources.Load<GameObject>("Prefabs/Neco/TouchTutorial_popup"), transform);
            popup.name = "TouchTutorial_popup";
            popup.GetComponent<TouchTutorial>().SetCallback(() => {
                Invoke(invokeMethodName, (float)clip.length);
            });
            VideoManager.GetInstance().PauseVideo();
        }
    }

    public void OnInteractionDone()
    {
        CancelInvoke("OnInteractionDone");
        ClearInteractionObjects();

        if (VideoAnimation != null)
            StopCoroutine(VideoAnimation);

        if (GetNextVideoClip() != null)
        {
            VideoAnimation = StartCoroutine(OnVideoFadeOut(OnNextVideo));
        }
        else
        {
            List<clip_event> clipList = curClipData.GetNextClipEvent("touch");

            if (clipList != null)
            {
                if (clipList.Count > 0)
                {
                    return;
                }
            }
            
            ExitVideo();
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
            EffectAudioSource.volume = 0.0f;
            EffectAudioSource.Stop();

            if (VibrateControlCoroutine != null)
            {
                StopCoroutine(VibrateControlCoroutine);
                VibrateControlCoroutine = null;
            }
        }
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
            //BlurTexture.color = color;
            yield return new WaitForSeconds(0.05f);
        }

        color.r = 1.0f;
        color.g = 1.0f;
        color.b = 1.0f;
        renderImage.color = color;
        //BlurTexture.color = color;
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
            //BlurTexture.color = color;
            yield return new WaitForSeconds(0.05f);
        }

        color.r = 0.0f;
        color.g = 0.0f;
        color.b = 0.0f;
        renderImage.color = color;
        //BlurTexture.color = color;
        cb.Invoke();
    }

    public clip_event GetNextVideoClip()
    { 
        if(curClipData == null)
            return null;

        List<clip_event> clipList = curClipData.GetNextClipEvent("finish");

        if (clipList == null)
            return null;
        if (clipList.Count == 0)
            return null;

        return clipList[0];
    }
    public void OnNextVideo()
    {
        clip_event next = GetNextVideoClip();
        if (next == null)
        {
            ExitVideo();
            return;
        }

        if(string.IsNullOrEmpty(next.GetClipPath()))
        {
            ExitVideo();
            return;
        }

        OnVideoPlay(next, VideoDoneCallback);
    }

    public void ResponseEventData(string response)
    {
        JObject root = JObject.Parse(response);

        JToken apiToken = root["api"];
        if (null == apiToken || apiToken.Type != JTokenType.Array
            || !apiToken.HasValues)
        {
            //error popup
            return;
        }

        JArray apiArr = (JArray)apiToken;
        foreach (JObject row in apiArr)
        {
            string uri = row.GetValue("uri").ToString();
            if (uri == "cat")
            {
                JToken rsToken = row["rs"];
                JToken opCode = row["op"];

                if (rsToken != null && rsToken.Type == JTokenType.Integer)
                {

                }

                if (opCode != null && opCode.Type == JTokenType.Integer)
                {

                }
                JObject income = (JObject)row.GetValue("rew");
                if (income != null)
                {
                    //rewardHistory.Add(income);
                }
            }
        }
    }

    public void OnSearchState(clip_event eventData, int prefabIndex)
    {
        ClearVideo();

        //string audioPath = eventData.GetAudioPath();
        //if (!string.IsNullOrEmpty(audioPath))
        //{
        //    AudioClip audio = Resources.Load<AudioClip>(audioPath);
        //    VideoAudioSource.clip = audio;

        //    BackgroundAudioSource.Stop();
        //    VideoAudioSource.Play();
        //}

        curClipData = eventData;
        SearchObject.OnSearch(eventData, prefabIndex);
    }

    public void OnInteractionEffect(clip_event eventData)
    {
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
            InteractionTitle.font = LocalizeData.instance.CurLanguage() == SystemLanguage.Japanese ? JapFont : HoonminFont;

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

        OnInteractionState(eventData);
        yield return new WaitForSeconds(0.1f);

        InteractionTitlePanel.SetActive(false);        
    }

    public void OnInteractionState(clip_event eventData)
    {
        SearchObject.SearchDone();

        if (VideoLoadCoroutine != null)
            StopCoroutine(VideoLoadCoroutine);

        VideoLoadCoroutine = StartCoroutine(VideoLoad(eventData, "OnInteractionDone"));
    }

    public IEnumerator VolumeControl(float targetVolume)
    {
        float curVolume = EffectAudioSource.volume;
        EffectAudioSource.loop = true;

        if (targetVolume == 0.0f && curVolume == 0.0f)
        {
            EffectAudioSource.Stop();
        }
        else
        {
            if (!EffectAudioSource.isPlaying)
                EffectAudioSource.Play();

            if (curVolume > targetVolume)
            {
                while (curVolume > targetVolume)
                {
                    curVolume = curVolume - (1.0f * Time.deltaTime);
                    EffectAudioSource.volume = curVolume;
                    yield return new WaitForEndOfFrame();
                }

                EffectAudioSource.volume = targetVolume;
            }
            else if (curVolume < targetVolume)
            {
                while (curVolume < targetVolume)
                {
                    curVolume = curVolume + (1.0f * Time.deltaTime);
                    EffectAudioSource.volume = curVolume;
                    yield return new WaitForEndOfFrame();
                }

                EffectAudioSource.volume = targetVolume;
            }

            if (targetVolume == 0.0f)
            {
                EffectAudioSource.Stop();
            }
        }
    }

    public IEnumerator ViberateControl()
    {
        long playTime = 500;
        int[] amp = { 20, 40, 60, 40, 20, 10 };
        
        int index = 0;
        while (true)
        {
            RDG.Vibration.Vibrate(playTime, amp[index], true);
            
            index = (index + 1) % amp.Length;
            
            yield return new WaitForSeconds(playTime * 0.001f);
        }
    }

    public void OnCatInteraction(cat_action_def action)
    {
        if (action == null)
            return;

        WWWForm data = new WWWForm();
        if (action.GetActionType() == cat_action_def.INTERACTION_TYPE.CAMERA)
        {
            data.AddField("api", "cat");
            data.AddField("op", 6);
            data.AddField("cat", action.GetTargetCat().GetCatID().ToString());
            //data.AddField("act", action.GetCatActionID().ToString());

            NetworkManager.GetInstance().SendApiRequest("cat", 6, data, (response) =>
            {
                JObject root = JObject.Parse(response);
                JToken apiToken = root["api"];
                if (null == apiToken || apiToken.Type != JTokenType.Array
                    || !apiToken.HasValues)
                {
                    return;
                }

                JArray apiArr = (JArray)apiToken;
                foreach (JObject row in apiArr)
                {
                    string uri = row.GetValue("uri").ToString();
                    if (uri == "cat")
                    {
                        JToken resultCode = row["rs"];
                        if (resultCode != null && resultCode.Type == JTokenType.Integer)
                        {
                            int rs = resultCode.Value<int>();
                            if (rs == 0)
                            {
                                JArray res = (JArray)row["res"];
                                foreach (JToken cardID in res)
                                {
                                    uint cardUniqueID = cardID.Value<uint>();

                                    StartCoroutine(ShowPhotoResult(cardUniqueID));
                                }

                                return;
                            }
                            else
                            {
                                string msg = rs.ToString();
                                switch (rs)
                                {
                                    case 1: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                    case 2: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                    case 3: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                    case 4: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                    case 5: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                    case 6: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                    case 7: msg = LocalizeData.GetText("LOCALIZE_370"); break;
                                    case 8: msg = LocalizeData.GetText("LOCALIZE_314"); break;
                                    case 9: msg = LocalizeData.GetText("LOCALIZE_311"); break;
                                    case 10: msg = LocalizeData.GetText("LOCALIZE_506"); break;
                                    case 11: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                    case 12: msg = LocalizeData.GetText("LOCALIZE_314"); break;
                                    case 13: msg = LocalizeData.GetText("LOCALIZE_79"); break;
                                }

                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_360"), msg);
                            }
                        }
                    }
                }
            });
            return;
        }


        data.AddField("api", "cat");
        data.AddField("op", 1);
        data.AddField("cat", action.GetTargetCat().GetCatID().ToString());
        data.AddField("act", action.GetCatActionID().ToString());

        NetworkManager.GetInstance().SendApiRequest("cat", 1, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "cat")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            OnInteractionState(action.GetActionClip());
                            return;
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 5: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 6: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 7: msg = LocalizeData.GetText("LOCALIZE_370"); break;
                                case 8: msg = LocalizeData.GetText("LOCALIZE_314"); break;
                                case 9: msg = LocalizeData.GetText("LOCALIZE_311"); break;
                                case 10: msg = LocalizeData.GetText("LOCALIZE_506"); break;
                                case 11: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 12: msg = LocalizeData.GetText("LOCALIZE_314"); break;
                                case 13: msg = LocalizeData.GetText("LOCALIZE_79"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_360"), msg);
                        }
                    }
                }
            }
        });

    }

    public IEnumerator ShowPhotoResult(uint cardUniqueID)
    {
        yield return new WaitForEndOfFrame();
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

        //foreach (JObject income in rewardHistory)
        //{
        //    if (income.ContainsKey("gold"))
        //    {
        //        bRewardGet = true;
        //        obj["gold"] = obj["gold"].Value<uint>() + income["gold"].Value<uint>();
        //    }
        //    if (income.ContainsKey("exp"))
        //    {
        //        bRewardGet = true;
        //        obj["exp"] = obj["exp"].Value<uint>() + income["exp"].Value<uint>();
        //    }
        //    if (income.ContainsKey("touch"))
        //    {
        //        foreach (JObject val in income["touch"])
        //        {
        //            bRewardGet = true;
        //            ((JArray)obj["touch"]).Add(val);
        //        }
        //    }
        //    if (income.ContainsKey("play"))
        //    {
        //        foreach (JObject val in income["play"])
        //        {
        //            bRewardGet = true;
        //            ((JArray)obj["play"]).Add(val);
        //        }
        //    }
        //    if (income.ContainsKey("card"))
        //    {
        //        JArray arr = (JArray)income["card"];
        //        foreach (JToken val in arr)
        //        {
        //            bRewardGet = true;
        //            ((JArray)obj["card"]).Add(val);
        //        }
        //    }
        //    if (income.ContainsKey("item"))
        //    {
        //        foreach (JObject val in income["item"])
        //        {
        //            bRewardGet = true;
        //            ((JArray)obj["item"]).Add(val);
        //        }
        //    }
        //    if (income.ContainsKey("food"))
        //    {
        //        foreach (JObject val in income["food"])
        //        {
        //            bRewardGet = true;
        //            ((JArray)obj["food"]).Add(val);
        //        }
        //    }
        //}

        //if (bRewardGet)
        //{
        //    if (((JArray)obj["touch"]).Count <= 0)
        //    {
        //        obj.Remove("touch");
        //    }
        //    if (((JArray)obj["play"]).Count <= 0)
        //    {
        //        obj.Remove("play");
        //    }
        //    if (((JArray)obj["card"]).Count <= 0)
        //    {
        //        obj.Remove("card");
        //    }
        //    if (((JArray)obj["item"]).Count <= 0)
        //    {
        //        obj.Remove("item");
        //    }
        //    if (((JArray)obj["food"]).Count <= 0)
        //    {
        //        obj.Remove("food");
        //    }

        //    RewardListUI.ShowTouchRewardList(obj);
        //}

        //rewardHistory.Clear();
    }

    public void SendTryInteraction(clip_event.CLIP_EVENT_TYPE type)
    {
        string key = "";
        switch (type)
        {
            case clip_event.CLIP_EVENT_TYPE.TOUCH:
                key = "touch";
                break;
            case clip_event.CLIP_EVENT_TYPE.PLAY:
                key = "play";
                break;
            case clip_event.CLIP_EVENT_TYPE.FEED:
                key = "feed";
                break;
            case clip_event.CLIP_EVENT_TYPE.FISH:
                key = "fish";
                break;
            default:
                return;
        }

        List<clip_event> clipList = curClipData.GetNextClipEvent(key);

        if (clipList == null)
            return;
        if (clipList.Count == 0)
            return;

        OnVideoPlay(clipList[0], VideoDoneCallback);
    }

    public bool CheckErrorResponse(string response)
    {
        JObject root = JObject.Parse(response);
        JToken apiToken = root["api"];
        if (null == apiToken || apiToken.Type != JTokenType.Array
            || !apiToken.HasValues)
        {
            return false;
        }

        JArray apiArr = (JArray)apiToken;
        foreach (JObject row in apiArr)
        {
            string uri = row.GetValue("uri").ToString();
            if (uri == "cat")
            {
                JToken resultCode = row["rs"];
                if (resultCode != null && resultCode.Type == JTokenType.Integer)
                {
                    int rs = resultCode.Value<int>();
                    if (rs != 0)
                    {
                        string msg = "";
                        switch (rs)
                        {
                            case 1:
                                msg = LocalizeData.GetText("LOCALIZE_303");
                                break;
                            case 2:
                                msg = LocalizeData.GetText("LOCALIZE_304");
                                break;
                            case 3:
                                msg = LocalizeData.GetText("LOCALIZE_305");
                                break;
                            case 4:
                                msg = LocalizeData.GetText("LOCALIZE_306");
                                break;
                            case 5:
                                msg = LocalizeData.GetText("LOCALIZE_307");
                                break;
                            case 6:
                                msg = LocalizeData.GetText("LOCALIZE_308");
                                break;
                            case 7:
                                msg = LocalizeData.GetText("LOCALIZE_309");
                                break;
                            case 8:
                                msg = LocalizeData.GetText("LOCALIZE_310");
                                break;
                            case 9:
                                msg = LocalizeData.GetText("LOCALIZE_311");
                                break;
                            case 10:
                                msg = LocalizeData.GetText("LOCALIZE_312");
                                break;
                            case 11:
                                msg = LocalizeData.GetText("LOCALIZE_313");
                                break;
                            case 12:
                                msg = LocalizeData.GetText("LOCALIZE_314");
                                break;
                            case 13:
                                msg = LocalizeData.GetText("LOCALIZE_315");
                                break;
                        }

                        //if (!string.IsNullOrEmpty(msg))
                        //    CatInfoUI.FarmCanvas.GameManager.PopupControl.OnPopupMessage(msg);

                        return false;
                    }
                }
            }
        }

        return true;
    }

    public void ExitVideo()
    {
        CancelInvoke("OnInteractionDone");
        ClearInteractionObjects();
        SearchObject.SearchDone();

        VideoManager.GetInstance().StopVideo(true);


        clip_event eventClip = curClipData;

        while(eventClip != null)
        {
            eventClip = GetNextVideoClip();
            if (eventClip != null)
                curClipData = eventClip;
        }

        //JArray rewArray = curClipData.GetRewardData();
        
        //foreach (JToken token in rewArray)
        //{
        //    JArray row = (JArray)token;
        //    if (row[0].ToString() == "gold")
        //    {
        //        string amount = row[3].ToString();

        //        WWWForm data = new WWWForm();
        //        data.AddField("api", "chore");
        //        data.AddField("op", 1);
        //        data.AddField("gold", amount);

        //        NetworkManager.GetInstance().SendApiRequest("chore", 1, data, (response) =>
        //        {
        //            RewardData rewardData = new RewardData();

        //            rewardData.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
        //            rewardData.gold = uint.Parse(amount);

        //            GetPopupCanvas().OnSingleRewardPopup("고양이와의 즐거운 한때였습니다.", rewardData);

        //            gameObject.SetActive(false);
                    
        //            VideoDoneCallback?.Invoke();
        //        });

        //        return;
        //    }
        //}

        gameObject.SetActive(false);

        VideoDoneCallback?.Invoke();
        VideoDoneCallback = null;

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.길막이만지기돌발발생)
        {
            Transform TouchTutorial_popup = transform.Find("TouchTutorial_popup");
            if(TouchTutorial_popup != null)
            {
                Destroy(TouchTutorial_popup.gameObject);
            }
        }
    }

    private void OnEnable()
    {
        NecoGameCanvas gameCanvas = GetGameCanvas();
        if (gameCanvas != null)
            gameCanvas.OnUICurtain(true);

        Application.targetFrameRate = 60;
    }

    private void OnDisable()
    {
        NecoGameCanvas gameCanvas = GetGameCanvas();
        if(gameCanvas != null)
            gameCanvas.OnUICurtain(false);

        Application.targetFrameRate = 30;
    }
}
