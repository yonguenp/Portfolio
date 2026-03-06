using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class FarmCanvas : CanvasControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public delegate void Callback();
    public enum FARM_STATE
    {
        FARM_NORMAL,
        FARM_MOVIE,
        FARM_ANIMATION,
    };

    public InteractionUIControl interactionUIControl;
    public SearchAttachPanel SearchObject;

    public ScrollRect MapScroller;
    public VideoPlayer FarmVideoPlayer;
    public AudioSource audioSource;
    public AudioSource BackgroundAudioSource;
    public RawImage movieTexture;        
    public GameObject InteractionTitlePanel;
    public Text InteractionTitle;
    public GameObject ParticlePrefab;
    public FarmUIPanel FarmUIPanel;
    public WorldCatManager WorldCatManager;
    public CatInfoUI CatInfoUI;
    public RewardListUI RewardListUI;

    private uint curEventID = 0;
    private uint score = 0;
    private Coroutine VideoAnimation = null;
    private Coroutine VideoLoadCoroutine = null;
    private Coroutine VolumeControlCoroutine = null;
    private Coroutine VibrateControlCoroutine = null;
    private GameObject CurTouchEffect = null;
    private FARM_STATE curFarmState = FARM_STATE.FARM_NORMAL;
    private List<JObject> rewardHistory = new List<JObject>();

    public void Awake()
    {
        gameObject.SetActive(false);
        CloseCatInfoUI();        
    }

    private void OnEnable()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (FARM_STATE.FARM_MOVIE != curFarmState)
            return;
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
        if (FARM_STATE.FARM_MOVIE != curFarmState)
            return;
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
        audioSource.loop = false;
        ClearInteractionObjects();

        if (VolumeControlCoroutine != null)
            StopCoroutine(VolumeControlCoroutine);

        VolumeControlCoroutine = StartCoroutine(VolumeControl(0.0f));

        if (FARM_STATE.FARM_MOVIE != curFarmState)
            return;
        if (!interactionUIControl.IsParticleInteraction())
            return;

        if (PlayerPrefs.GetInt("Config_VB", 1) > 0)
            RDG.Vibration.Vibrate(500, 10, true);
    }

    public override void SetCanvasState(GameMain.HahahaState state)
    {
        gameObject.SetActive(true);

        if(state == GameMain.HahahaState.HAHAHA_FARM)
            SetFarmState(FARM_STATE.FARM_NORMAL);
    }

    public void SetFarmState(FARM_STATE state)
    {
        CloseCatInfoUI();

        switch (state)
        {
            case FARM_STATE.FARM_NORMAL:
                ClearVideo();
                SearchObject.SearchDone();
                InteractionTitlePanel.SetActive(false);
                FarmUIPanel.Show();
                ShowRewardPopup();
                break;
            case FARM_STATE.FARM_MOVIE:
                FarmUIPanel.Hide();
                break;
            case FARM_STATE.FARM_ANIMATION:
                FarmUIPanel.Hide();
                break;
        }

        curFarmState = state;
    }

    public void ClearVideo()
    {
        interactionUIControl.gameObject.SetActive(false);

        RenderTexture target = FarmVideoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        FarmVideoPlayer.clip = null;
        CancelInvoke("OnFarmInteractionDone");
    }

    public IEnumerator VideoLoad(clip_event eventData, string invokeMethodName)
    {
        FarmVideoPlayer.Stop();
        VideoClip preClip = FarmVideoPlayer.clip;
        Resources.UnloadAsset(preClip);

        RenderTexture target = FarmVideoPlayer.targetTexture;
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
            SetFarmState(FARM_STATE.FARM_NORMAL);
            yield return null;
        }

        FarmVideoPlayer.playOnAwake = false;

        FarmVideoPlayer.gameObject.SetActive(true);
        FarmVideoPlayer.clip = clip;
        FarmVideoPlayer.isLooping = false;
        FarmVideoPlayer.time = 0;

        FarmVideoPlayer.Prepare();
        while (!FarmVideoPlayer.isPrepared)
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

        curEventID = eventData.GetEventID();

        if (clip.audioTrackCount > 0)
        {
            BackgroundAudioSource.DOFade(0.0f, 0.3f);

            float volume = PlayerPrefs.GetInt("Config_BV", 9) / 9;
            if (PlayerPrefs.GetInt("Config_BS", 1) <= 0)
            {
                volume = 0.0f;
            }

            for (ushort i = 0; i < FarmVideoPlayer.audioTrackCount; i++)
            {
                FarmVideoPlayer.SetDirectAudioVolume(i, volume);
            }
        }

        FarmVideoPlayer.Play();

        interactionUIControl.SetClipData(eventData);

        if (VideoAnimation != null)
            StopCoroutine(VideoAnimation);
        VideoAnimation = StartCoroutine(OnVideoFadeIn());

        Invoke(invokeMethodName, (float)FarmVideoPlayer.length);
    }

    public void OnFarmInteractionDone()
    {
        CancelInvoke("OnFarmInteractionDone");

        BackgroundAudioSource.DOFade((float)PlayerPrefs.GetInt("Config_BV", 9) / 9, 0.3f);
        bool mute = PlayerPrefs.GetInt("Config_BS", 1) == 0;
        BackgroundAudioSource.mute = mute;

        if (FARM_STATE.FARM_MOVIE != curFarmState)
            return;

        ClearInteractionObjects();

        if (VideoAnimation != null)
            StopCoroutine(VideoAnimation);

        VideoAnimation = StartCoroutine(OnVideoFadeOut(SendNextStep));
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

    public void SendNextStep()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "cat");
        data.AddField("op", 2);

        NetworkManager.GetInstance().SendApiRequest("cat", 2, data, ResponseEventData);

        score = 0;
    }

    public void ResponseEventData(string response)
    {
        JObject root = JObject.Parse(response);

        JToken apiToken = root["api"];
        if (null == apiToken || apiToken.Type != JTokenType.Array
            || !apiToken.HasValues)
        {
            SetFarmState(FARM_STATE.FARM_NORMAL);
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
                    rewardHistory.Add(income);
                }
            }
        }

        SetFarmState(FARM_STATE.FARM_NORMAL);
    }

    public void OnFarmSearchState(clip_event eventData, int prefabIndex)
    {
        SetFarmState(FARM_STATE.FARM_ANIMATION);
        SearchObject.OnSearch(eventData, prefabIndex);
    }

    public void OnInteractionEffect(clip_event eventData)
    {
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

    public void OnInteractionState(clip_event eventData)
    {
        SetFarmState(FARM_STATE.FARM_MOVIE);

        if (VideoLoadCoroutine != null)
            StopCoroutine(VideoLoadCoroutine);

        VideoLoadCoroutine = StartCoroutine(VideoLoad(eventData, "OnFarmInteractionDone"));

        FarmVideoPlayer.aspectRatio = VideoAspectRatio.FitInside;
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

            if (targetVolume == 0.0f)
            {
                audioSource.Stop();
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
                                foreach(JToken cardID in res)
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

        List<game_data> user_card = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
        foreach(user_card card in user_card)
        {
            if(card.GetCardUniqueID() == cardUniqueID)
                NecoCanvas.GetPopupCanvas().ShowPhotoResult(card);
        }

    }

    public void OnCatInfoUI(CatSkeletonGraphic cat)
    {
        CatInfoUI.OnUIOpen(cat);        
    }

    public void CloseCatInfoUI()
    {
        CatInfoUI.gameObject.SetActive(false);
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

            RewardListUI.ShowTouchRewardList(obj);
        }

        rewardHistory.Clear();
    }

    public void SendTryInteraction(clip_event.CLIP_EVENT_TYPE type)
    {
        
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

                        if (!string.IsNullOrEmpty(msg))
                            CatInfoUI.FarmCanvas.GameManager.PopupControl.OnPopupMessage(msg);

                        return false;
                    }
                }
            }
        }

        return true;
    }
}
