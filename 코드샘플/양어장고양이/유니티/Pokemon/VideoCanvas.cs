using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoCanvas : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public MainVideoCanvas mainVideoCanvas;
    public delegate void Callback();
    
    public InteractionUIControl interactionUIControl;
    public SearchAttachPanel SearchObject;

    public VideoPlayer VideoPlayer;
    public AudioSource BackgroundAudioSource;
    public AudioSource EffectAudioSource;
    public AudioSource VideoAudioSource;
    public RawImage movieTexture;
    public GameObject InteractionTitlePanel;
    public Text InteractionTitle;
    public GameObject ParticlePrefab;    

    private uint score = 0;
    private Coroutine VideoAnimation = null;
    private Coroutine VideoLoadCoroutine = null;
    private Coroutine VolumeControlCoroutine = null;
    private Coroutine VibrateControlCoroutine = null;
    private GameObject CurTouchEffect = null;
    private clip_event curClipData = null;

    public void Awake()
    {
        
    }

    private void OnEnable()
    {

    }

    private void Start()
    {
        
    }

    public void OnVideoPlay(uint clipID)
    {
        List<game_data> clip_event = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);
        if (clip_event != null)
        {
            foreach (clip_event clip in clip_event)
            {
                if(clip.GetEventID() == clipID)
                {
                    OnVideoPlay(clip);
                    return;
                }
            }
        }
    }

    public void OnVideoPlay(clip_event clip)
    {
        if (clip.GetEventID() == 0)
        {
            //mainVideoCanvas.OnReturnMain();
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

        OnInteractionState(clip);
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

        if (PlayerPrefs.GetInt("Config_VB", 1) > 0)
            RDG.Vibration.Vibrate(500, 10, true);

        score++;
    }

    public void OnDrag(PointerEventData eventData)
    {
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
            if (PlayerPrefs.GetInt("Config_VB", 1) > 0)
                VibrateControlCoroutine = StartCoroutine(ViberateControl());
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        EffectAudioSource.loop = false;
        ClearInteractionObjects();

        if (VolumeControlCoroutine != null)
            StopCoroutine(VolumeControlCoroutine);

        VolumeControlCoroutine = StartCoroutine(VolumeControl(0.0f));

        if (!interactionUIControl.IsParticleInteraction())
            return;

        if (PlayerPrefs.GetInt("Config_VB", 1) > 0)
            RDG.Vibration.Vibrate(500, 10, true);
    }

    public void ClearVideo()
    {
        BackgroundAudioSource.Play();
        VideoAudioSource.Stop();

        SearchObject.SearchDone();
        interactionUIControl.gameObject.SetActive(false);

        RenderTexture target = VideoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        VideoPlayer.clip = null;
        CancelInvoke("OnInteractionDone");
    }

    public IEnumerator VideoLoad(clip_event eventData, string invokeMethodName)
    {
        VideoPlayer.Stop();
        VideoClip preClip = VideoPlayer.clip;
        Resources.UnloadAsset(preClip);

        RenderTexture target = VideoPlayer.targetTexture;
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
            yield return null;
        }

        VideoPlayer.playOnAwake = false;

        VideoPlayer.gameObject.SetActive(true);
        VideoPlayer.clip = clip;
        VideoPlayer.isLooping = false;
        VideoPlayer.time = 0;

        BackgroundAudioSource.Stop();
        VideoAudioSource.clip = null;

        string audioPath = eventData.GetAudioPath();
        if (string.IsNullOrEmpty(audioPath))
        {
            VideoPlayer.SetTargetAudioSource(0, VideoAudioSource);
        }
        else
        {
            AudioClip audio = Resources.Load<AudioClip>(audioPath);
            VideoAudioSource.clip = audio;
        }

        VideoAudioSource.Play();

        VideoPlayer.Prepare();
        while (!VideoPlayer.isPrepared)
        {
            yield return new WaitForEndOfFrame();
        }

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

        VideoPlayer.Play();

        interactionUIControl.SetClipData(eventData);

        if (VideoAnimation != null)
            StopCoroutine(VideoAnimation);
        VideoAnimation = StartCoroutine(OnVideoFadeIn());
                
        Invoke(invokeMethodName, (float)clip.length);
    }

    public void OnInteractionDone()
    {
        VideoAudioSource.Stop();
        BackgroundAudioSource.Play();

        CancelInvoke("OnInteractionDone");

        ClearInteractionObjects();

        if (VideoAnimation != null)
            StopCoroutine(VideoAnimation);

        if (GetNextVideoClip() != null)
            VideoAnimation = StartCoroutine(OnVideoFadeOut(OnNextVideo));
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
        List<clip_event> clipList = curClipData.GetNextClipEvent("finish");
        
        if (clipList == null)
            return null;
        if (clipList.Count == 0)
            return null;

        return clipList[0];
    }
    public void OnNextVideo()
    {
        List<clip_event> clipList = curClipData.GetNextClipEvent("finish");

        if (clipList == null || clipList.Count == 0)
        {
            Debug.LogError("!!!");
            OnTestVideoPlay();
            return;
        }

        clip_event next = clipList[0];
        if (next == null)
        {
            Debug.LogError("!!!");
            OnTestVideoPlay();
            return;
        }

        OnVideoPlay(next);
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

        string audioPath = eventData.GetAudioPath();
        if (!string.IsNullOrEmpty(audioPath))        
        {
            AudioClip audio = Resources.Load<AudioClip>(audioPath);
            VideoAudioSource.clip = audio;

            BackgroundAudioSource.Stop();
            VideoAudioSource.Play();
        }

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

    public void OnInteractionState(clip_event eventData)
    {
        SearchObject.SearchDone();

        if (VideoLoadCoroutine != null)
            StopCoroutine(VideoLoadCoroutine);

        VideoLoadCoroutine = StartCoroutine(VideoLoad(eventData, "OnInteractionDone"));

        VideoPlayer.aspectRatio = VideoAspectRatio.FitInside;
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
        switch(type)
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

        OnVideoPlay(clipList[0]);
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
                                msg = "고양이 정보 오류";
                                break;
                            case 2:
                                msg = "고양이 행동 정보 오류";
                                break;
                            case 3:
                                msg = "고양이가 하기 싫어해요";
                                break;
                            case 4:
                                msg = "고양이가 배가 고파요";
                                break;
                            case 5:
                                msg = "NOT ON ACTION";
                                break;
                            case 6:
                                msg = "NOT MORE ACTION";
                                break;
                            case 7:
                                msg = "금전적으로 힘이 듭니다.";
                                break;
                            case 8:
                                msg = "NO MORE LEVEL";
                                break;
                            case 9:
                                msg = "아이템을 찾을 수 없어요";
                                break;
                            case 10:
                                msg = "배가 부릅니다.";
                                break;
                            case 11:
                                msg = "음식이 이상합니다.";
                                break;
                            case 12:
                                msg = "레벨이 부족해요.";
                                break;
                            case 13:
                                msg = "카드가 너무 많아요.";
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
}
