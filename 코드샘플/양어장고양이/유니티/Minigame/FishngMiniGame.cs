using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using UnityEngine.Video;

public class FishngMiniGame : MonoBehaviour
{
    public bool TestMode = false;

    public SkeletonGraphic BackGround;
    public SkeletonGraphic Game;
    public SkeletonGraphic Wave;

    public RectTransform FishLayer;
    public GameObject FishSample;
    public BoneFollowerGraphic FishingHook;

    public GameObject UIGameOver_fast;
    public GameObject UIGameOver_late;
    public GameObject UIGameClear;    

    public RectTransform[] BaitItem;
    public GameObject UIBaitSelect;
    public GameObject HelpPopup;

    public Sprite[] BaitSprites;

    public AudioClip BGMAudio;
    public AudioClip HitAudio;
    public AudioClip GetAudio;

    public RawImage VideoIntro;
    public VideoClip IntroClip;

    public GameObject UITouchGuide;

    uint fish_type = 0;
    uint fish_scale = 0;
    uint fish_len = 0;

    bool HitFish = false;
    bool ReadyFish = false;
    bool GamePlaying = false;
    bool IsTouched = false;

    Coroutine UICoroutine = null;
    Coroutine VibrateControlCoroutine = null;

    Vector3 hookPos = Vector3.zero;

    public uint[] RankUserNo = new uint[3];

    private void OnEnable()
    {
        UIClear();

        BackGround.gameObject.SetActive(false);
        Game.gameObject.SetActive(false);

        transform.Find("sky").gameObject.SetActive(false);

        VideoIntro.gameObject.SetActive(true);
        if (VideoManager.GetInstance() != null)
        {
            VideoManager.GetInstance().PlayVideo(IntroClip, (RenderTexture)VideoIntro.texture, false, true, () => {
                Invoke("IntroDone", (float)IntroClip.length);
            });
        }
        else
        {
            IntroDone();
        }
    }

    private void OnDisable()
    {
        SetBGM(false);
        StopAllCoroutines();
        VibrateControlCoroutine = null;
        UICoroutine = null;

        HitFish = false;
        ReadyFish = false;
        GamePlaying = false;
    }

    void IntroDone()
    {
        HelpPopup.transform.Find("Background").Find("ContentsInfoText").GetComponent<Text>().text =
#if UNITY_IOS
                LocalizeData.GetText("낚시도움말_FORIOS");
#else
                LocalizeData.GetText("낚시도움말");
#endif
        BackGround.gameObject.SetActive(true);
        transform.Find("sky").gameObject.SetActive(true);

        VideoIntro.gameObject.SetActive(false);
        SetBGM(true);
        BaitSet();
    }

    void SetBGM(bool gameBGM)
    {
        if(AudioManager.GetInstance() != null)
            AudioManager.GetInstance().PlayBackgroundAudio(gameBGM ? BGMAudio : null);
    }

    public void BaitSet()
    {
        fish_type = 0;
        fish_scale = 0;
        fish_len = 0;
        IsTouched = false;

        UIClear();

        if(TestMode)
        {
            OnBaitInfo();
            return;
        }

        // 낚시 정보 조회 API
        WWWForm data = new WWWForm();
        data.AddField("api", "fish");
        data.AddField("op", 1);

        NetworkManager.GetInstance().SendApiRequest("fish", 1, data, (response) =>
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
                if (uri == "fish")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            if(row.ContainsKey("baits"))
                                neco_data.Instance.FishingData.Baits = row["baits"].Value<uint>();

                            OnBaitInfo();
                            return;
                        }
                    }
                }
            }

            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.FISHING_POPUP);
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("error"), LocalizeData.GetText("낚시정보동기화오류"));
        });
    }

    private void GameSet()
    {
        //UICoroutine = StartCoroutine(OnUI(UIGameStart, GameStart));
        GameStart();
    }

    void UIClear()
    {
        if (VibrateControlCoroutine != null)
        {
            StopCoroutine(VibrateControlCoroutine);
            VibrateControlCoroutine = null;
        }

        if (UICoroutine != null)
        {
            StopCoroutine(UICoroutine);
            UICoroutine = null;
        }

        UIGameOver_fast.SetActive(false);
        UIGameOver_late.SetActive(false);
        UIGameClear.SetActive(false);
        UIBaitSelect.SetActive(false);
        UITouchGuide.SetActive(false);
    }

    void GameInit()
    {
        Game.gameObject.SetActive(true);
        Game.startingAnimation = "start";
        Game.startingLoop = false;
        Game.Initialize(true);

        Game.AnimationState.AddAnimation(0, "wait", true, 0);

        FollowHook();
    }

    void GameStart()
    {
        GameInit();

        HitFish = false;
        ReadyFish = false;
        GamePlaying = true;

        FishSample.transform.DOKill();
        FishSample.transform.SetParent(FishLayer.transform);
        FishSample.transform.localEulerAngles = Vector3.zero;

        float min = 1.0f;
        float max = 1.0f;
        switch (fish_scale)
        {
            case 1:
                min = 0.5f;
                max = 0.7f;
                break;
            case 2:
                min = 1.0f;
                max = 1.2f;
                break;
            case 3:
                min = 1.5f;
                max = 1.7f;
                break;
            case 4:
                min = 2.0f;
                max = 2.2f;
                break;
        }
        FishSample.transform.localScale = Vector3.one * (Random.Range(min, max));
        
        SkeletonGraphic anim = FishSample.GetComponent<SkeletonGraphic>();
        anim.startingAnimation = "fish1";

        foreach (Transform fish in FishLayer.transform)
        {
            if (fish != FishSample.transform && fish != FishingHook.transform)
            {
                Destroy(fish.gameObject);
            }
        }

        float min_x = FishLayer.sizeDelta.x * 0.5f * -1.0f;
        float max_x = FishLayer.sizeDelta.x * 0.5f;
        float max_y = FishLayer.sizeDelta.y * 0.5f;

        for (int i = 0; i < Random.Range(5, 6 + 1); i++)
        {
            GameObject newFish = Instantiate(FishSample);
            newFish.transform.SetParent(FishLayer);

            SkeletonGraphic fishImage = newFish.GetComponent<SkeletonGraphic>();
            fishImage.color = new Color(1.0f, 1.0f, 1.0f, 0.2f + ((0.8f / 10.0f) * i));
            fishImage.startingAnimation = "fish1";
            fishImage.Initialize(true);
            int ftype = Random.Range(1, 4);

            min = 1.0f;
            max = 1.0f;
            switch (ftype)
            {
                case 1:
                    min = 0.5f;
                    max = 0.7f;
                    break;
                case 2:
                    min = 1.0f;
                    max = 1.2f;
                    break;
                case 3:
                    min = 1.5f;
                    max = 1.7f;
                    break;
                case 4:
                    min = 2.0f;
                    max = 2.2f;
                    break;
            }
            newFish.transform.localScale = Vector3.one * (Random.Range(min, max));

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(Random.value * 5.0f);
            bool leftMove = Random.value > 0.5;

            Ease easeType = Ease.Linear;
            switch(Random.Range(0, 4))
            {
                case 0:
                    easeType = Ease.InQuad;
                    break;
                case 1:
                    easeType = Ease.OutQuad;
                    break;
                case 3:
                    easeType = Ease.InOutQuad;
                    break;
            }
            if (leftMove)
            {
                float start_x = (max_x + (newFish.transform as RectTransform).sizeDelta.x) * max;
                float end_x = (min_x - (newFish.transform as RectTransform).sizeDelta.x) * max;

                Vector3 scale = newFish.transform.localScale;
                scale.x *= 1.0f;
                newFish.transform.localScale = scale;

                newFish.transform.localPosition = new Vector3(start_x, max_y * (Random.value - 0.5f), 0);
                seq.Append(newFish.transform.DOLocalMoveX(end_x, (Random.value * 10.0f) + 5.0f).SetEase(easeType));
            }
            else
            {
                float start_x = (min_x - (newFish.transform as RectTransform).sizeDelta.x) * max;
                float end_x = (max_x + (newFish.transform as RectTransform).sizeDelta.x) * max;

                Vector3 scale = newFish.transform.localScale;
                scale.x *= -1.0f;
                newFish.transform.localScale = scale;

                newFish.transform.localPosition = new Vector3(start_x, max_y * (Random.value - 0.5f), 0);
                seq.Append(newFish.transform.DOLocalMoveX(end_x, (Random.value * 10.0f) + 5.0f).SetEase(easeType));
            }
            
            seq.SetLoops(-1, LoopType.Restart);
        }

        if(hookPos == Vector3.zero)
            hookPos = FishLayer.InverseTransformPoint(FishingHook.transform.position);

        if (Random.value > 0.5)
        {
            //to left            
            float start_x = max_x + ((FishSample.transform as RectTransform).sizeDelta.x * FishSample.transform.localScale.x);
            
            FishSample.transform.localPosition = new Vector3(start_x, max_y * (Random.value - 0.5f), 0);
            FishSample.transform.DOLocalMove(hookPos, (Random.value * 10.0f) + 5.0f).SetDelay((Random.value * 5.0f) + 5.0f).OnComplete(OnApproach);
        }
        else
        {
            //to right
            float start_x = min_x - ((FishSample.transform as RectTransform).sizeDelta.x * FishSample.transform.localScale.x);
            
            Vector3 scale = FishSample.transform.localScale;
            scale.x *= -1.0f;
            FishSample.transform.localScale = scale;

            FishSample.transform.localPosition = new Vector3(start_x, max_y * (Random.value - 0.5f), 0);
            FishSample.transform.DOLocalMove(hookPos, (Random.value * 10.0f) + 5.0f).SetDelay((Random.value * 5.0f) + 5.0f).OnComplete(OnApproach);
        }

        if(fish_type > 1)
            anim.startingAnimation = "fish_sparkle";
        else
            anim.startingAnimation = "fish1";

        anim.Initialize(true);
    }

    public void OnApproach()
    {
        if (!GamePlaying)
            return;

        FishSample.transform.DOKill();
        HitFish = false;
        ReadyFish = true;

        if (hookPos == Vector3.zero)
            hookPos = FishLayer.InverseTransformPoint(FishingHook.transform.position);

        if (Random.value < 0.7)
        {
            //간보기
            Sequence seq = DOTween.Sequence();
            
            if (FishSample.transform.localScale.x > 0.0f)
            {
                seq.Append(FishSample.transform.DOLocalMove(hookPos, 1.0f));
                seq.Append(FishSample.transform.DOLocalMove(hookPos + new Vector3(50.0f + (Random.value * 100.0f), 0, 0), 1.0f));
            }
            else
            {
                seq.Append(FishSample.transform.DOLocalMove(hookPos, 1.0f));
                seq.Append(FishSample.transform.DOLocalMove(hookPos + new Vector3((50.0f + (Random.value * 100.0f)) * -1.0f, 0, 0), 1.0f));
            }
            seq.OnComplete(OnApproach);
            seq.Restart();

            Game.startingAnimation = "yamyam";
            Game.startingLoop = false;
            Game.Initialize(true);

            Game.AnimationState.AddAnimation(0, "wait", true, 0);

            FollowHook();

            if (PlayerPrefs.GetInt("Setting_Vibration", 1) > 0)
                RDG.Vibration.Vibrate(300, 10, true);
        }
        else
        {
            AudioManager.GetInstance().PlayEffectAudio(HitAudio);

            //레알
            Invoke("OnFail", (Random.value * 2.0f) + 1.0f);

            Game.startingAnimation = "catch";
            Game.startingLoop = false;
            Game.Initialize(true);

            Game.AnimationState.AddAnimation(0, "fight", true, 0);

            FishSample.transform.SetParent(FishingHook.transform);
            FishSample.transform.localPosition = Vector3.zero;

            FollowHook();

            HitFish = true;
            FishingHook.GetComponent<Image>().enabled = false;

            if (VibrateControlCoroutine != null)
            {
                StopCoroutine(VibrateControlCoroutine);
                VibrateControlCoroutine = null;
            }

            if (PlayerPrefs.GetInt("Setting_Vibration", 1) > 0)
                VibrateControlCoroutine = StartCoroutine(ViberateControl());

            UITouchGuide.SetActive(true);
        }
    }

    void OnFail()
    {
        CancelInvoke("OnFail");
        FishingHook.GetComponent<Image>().enabled = false;
        OnGameOver(true);
    }

    public void OnTouch()
    {
        if (IsTouched)
            return;

        if (UICoroutine != null)
        {
            return;
        }
        
        if (UIBaitSelect.activeInHierarchy)
            return;

        if (!ReadyFish)
            return;


        if (VibrateControlCoroutine != null)
        {
            StopCoroutine(VibrateControlCoroutine);
            VibrateControlCoroutine = null;
        }

        fish_len = 0;
        FishSample.transform.DOKill();

        if (HitFish)
        {
            CancelInvoke("OnFail");

            Game.startingAnimation = "finish";
            Game.startingLoop = false;
            Game.Initialize(true);
            IsTouched = true;

            FollowHook();

            FishSample.transform.SetParent(FishingHook.transform);
            FishSample.transform.localPosition = Vector3.zero;
            FishSample.transform.localEulerAngles = Vector3.zero;
            if (FishSample.transform.localScale.x < 0)
            {
                Vector3 scale = FishSample.transform.localScale;
                scale.x *= 1.0f;
                FishSample.transform.localScale = scale;
            }

            AudioManager.GetInstance().PlayEffectAudio(GetAudio);

            UIClear();

            if (TestMode)
            {   
                StartCoroutine(delayAction(() =>
                {
                    Game.gameObject.SetActive(false);
                    OnGameResult(null);
                }, 1.0f));
            }
            else
            {
                //낚시 결과 api
                WWWForm data = new WWWForm();
                data.AddField("api", "fish");
                data.AddField("op", 3);
                data.AddField("success", 1);

                NetworkManager.GetInstance().SendApiRequest("fish", 3, data, (response) =>
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
                        if (uri == "fish")
                        {
                            JToken resultCode = row["rs"];

                            if (resultCode != null && resultCode.Type == JTokenType.Integer)
                            {
                                int rs = resultCode.Value<int>();
                                if (rs == 0)
                                {
                                    UIClear();

                                    RewardData rewards = null;

                                    if (row.ContainsKey("rew"))
                                    {
                                        if (row["rew"].HasValues)
                                        {
                                            JObject income = (JObject)row["rew"];
                                            if (income.ContainsKey("gold"))
                                            {
                                                RewardData reward = new RewardData();
                                                reward.gold = income["gold"].Value<uint>();
                                                rewards = reward;
                                            }

                                            if (income.ContainsKey("catnip"))
                                            {
                                                RewardData reward = new RewardData();
                                                reward.catnip = income["catnip"].Value<uint>();
                                                rewards = reward;
                                            }

                                            if (income.ContainsKey("item"))
                                            {
                                                JArray item = (JArray)income["item"];
                                                foreach (JObject rw in item)
                                                {
                                                    RewardData reward = new RewardData();
                                                    reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                                    reward.count = rw["amount"].Value<uint>();
                                                    rewards = reward;
                                                }
                                            }
                                        }
                                    }
                                    
                                    if (row.ContainsKey("len"))
                                    {
                                        fish_len = row["len"].Value<uint>();
                                    }


                                    StartCoroutine(delayAction(() =>
                                    {
                                        Game.gameObject.SetActive(false);
                                        OnGameResult(rewards);
                                    }, 1.0f));
                                }
                                else
                                {
                                    NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.FISHING_POPUP);
                                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("error"), LocalizeData.GetText("낚시정보동기화오류"));
                                }
                            }
                        }
                    }
                });
            }
        }
        else
        {
            OnGameOver(false);
        }

        HitFish = false;
        ReadyFish = false;
    }

    void OnGameOver(bool bLate)
    {
        GamePlaying = false;

        CancelInvoke("OnFail");

        UIClear();

        FishingHook.GetComponent<Image>().enabled = false;

        FishSample.transform.DOKill();
        FishSample.transform.SetParent(FishLayer.transform);
        FishSample.transform.localEulerAngles = Vector3.zero;
        FishSample.transform.DOLocalMove(new Vector3(FishLayer.sizeDelta.x * (FishLayer.transform.localScale.x < 0.0f ? -1.0f : 1.0f), 0.0f, 0), 1.0f).OnComplete(() =>
        {
            FishSample.transform.localPosition = new Vector3(FishLayer.sizeDelta.x * (FishLayer.transform.localScale.x < 0.0f ? -1.0f : 1.0f), 0.0f, 0);
        });

        Game.startingAnimation = "fail";
        Game.startingLoop = false;
        Game.Initialize(true);

        UICoroutine = StartCoroutine(delayAction(() => {
            (bLate ? UIGameOver_fast : UIGameOver_fast).SetActive(true);
        }, 1.0f)); 
    }

    IEnumerator OnUI(GameObject UI, System.Action cb, float delay = 0.0f)
    {
        if(delay > 0.0f)
        {
            yield return new WaitForSeconds(delay);
        }

        UI.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        UI.SetActive(false);

        UICoroutine = null;
        cb?.Invoke();
    }

    void OnBaitInfo()
    {
        UIBaitSelect.SetActive(true);

        foreach(RectTransform item in BaitItem)
        {
            item.Find("Select_Image").gameObject.SetActive(false);
        }

        UIBaitSelect.transform.Find("Background").Find("MiddleUI_Panel").Find("Bait").Find("BaitIcon").Find("BaitInfo").Find("MessageText_1").GetComponent<Text>().text = LocalizeData.GetText("미끼선택오류");
        BaitItem[0].Find("ItemCount_Text").GetComponent<Text>().text = neco_data.Instance.FishingData.Baits.ToString();
        BaitItem[1].Find("ItemCount_Text").GetComponent<Text>().text = user_items.GetUserItemAmount(149).ToString();
        BaitItem[2].Find("ItemCount_Text").GetComponent<Text>().text = user_items.GetUserItemAmount(150).ToString();

        SetRankingUI(0);

        Color disableColor = new Color(0.6f, 0.6352941f, 0.6509804f, 1.0f);
        UIBaitSelect.transform.Find("Background").Find("BottomUI_Panel").Find("OkButtonLayer").Find("OkButton").GetComponent<Image>().color = disableColor;
    }

    public void SetRankingUI(int index)
    {
        HelpPopup.SetActive(false);

        Transform Ranking = UIBaitSelect.transform.Find("Background").Find("MiddleUI_Panel").Find("Ranking");

        Transform Tabs = Ranking.Find("Background").Find("TabButton");
        for(int i = 1; i <= 7; i++)
        {
            Transform tab = Tabs.Find(i.ToString() + "Button");
            tab.Find("Button_ON").gameObject.SetActive(i - 1 == index);
            tab.Find("Button_Off").gameObject.SetActive(i - 1 != index);
        }

        Transform RankingGroup = Ranking.Find("Background").Find("RankingGroup");
        Transform infoPage = RankingGroup.Find("Normal_object");
        Transform rankPage = RankingGroup.Find("Ranking_object");
        infoPage.gameObject.SetActive(index == 0);
        rankPage.gameObject.SetActive(index != 0);

        RankUserNo[0] = 0;
        RankUserNo[1] = 0;
        RankUserNo[2] = 0;

        if (index > 0)
        {
            int fishType = 0;
            switch (index)
            {
                case 1:
                    fishType = 63;
                    break;
                case 2:
                    fishType = 70;
                    break;
                case 3:
                    fishType = 71;
                    break;
                case 4:
                    fishType = 76;
                    break;
                case 5:
                    fishType = 75;
                    break;
                case 6:
                    fishType = 74;
                    break;
            }

            rankPage.transform.Find("Subtitle").Find("Text").GetComponent<Text>().text = items.GetItem((uint)fishType).GetItemName();

            Text[] nick = new Text[3];
            Text[] len = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                Transform tmp = rankPage.Find((i + 1).ToString());
                nick[i] = tmp.Find("Text").GetComponent<Text>();
                len[i] = tmp.Find("Score_1").Find("Text").GetComponent<Text>();

                nick[i].text = "";
                len[i].text = "";
            }
            Text my_len = rankPage.Find("Myscore").Find("Score_Text").GetComponent<Text>();
            my_len.text = "";

            Image[] reward_item = new Image[3];
            Text[] reward_count = new Text[3];
            Text[] reward_name = new Text[3];

            Transform rewardList = rankPage.Find("Reward").Find("List");
            for (int i = 0; i < 3; i++)
            {
                Transform tmp = rewardList.Find((i + 1).ToString()).Find("RecipeMaterialObject");
                reward_count[i] = tmp.Find("MaterialCountText").GetComponent<Text>();
                reward_name[i] = tmp.Find("MaterialName_Text").GetComponent<Text>();
                reward_item[i] = tmp.Find("MaterialIcon").GetComponent<Image>();

                reward_name[i].text = "";
                reward_count[i].text = "";
                reward_item[i].sprite = null;
            }

            //낚시 랭킹 api
            WWWForm data = new WWWForm();
            data.AddField("api", "fish");
            data.AddField("op", 4);
            data.AddField("fish", fishType);

            NetworkManager.GetInstance().SendApiRequest("fish", 4, data, (response) =>
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
                    if (uri == "fish")
                    {
                        JToken resultCode = row["rs"];

                        if (resultCode != null && resultCode.Type == JTokenType.Integer)
                        {
                            int rs = resultCode.Value<int>();
                            if (rs == 0)
                            {
                                JArray ranks = (JArray)row["list"];
                                
                                foreach(JObject rank in ranks)
                                {
                                    uint idx = rank["rank"].Value<uint>() - 1;
                                    if (idx < nick.Length && idx < len.Length)
                                    {
                                        nick[idx].text = rank["nick"].Value<string>();
                                        len[idx].text = (rank["len"].Value<uint>() / 10.0f).ToString("n1") + "cm";
                                    }

                                    if(idx < RankUserNo.Length)
                                        RankUserNo[idx] = rank["uno"].Value<uint>();
                                }

                                if(row.ContainsKey("me"))
                                {
                                    JObject me = (JObject)row["me"];
                                    my_len.text = (me["len"].Value<uint>() / 10.0f).ToString("n1") + "cm";
                                }

                                if(row.ContainsKey("week"))
                                {
                                    uint week = row["week"].Value<uint>();
                                    fishing_reward[] ret = fishing_reward.GetFishingReward(week, (uint)fishType);

                                    for (int i = 0; i < 3; i++)
                                    {
                                        if (ret[i] != null)
                                        {
                                            reward_name[i].text = ret[i].GetRewardItem().GetItemName();
                                            reward_count[i].text = ret[i].GetRewardCount().ToString();
                                            reward_item[i].sprite = ret[i].GetRewardItem().GetItemIcon();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.FISHING_POPUP);
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("error"), LocalizeData.GetText("낚시정보동기화오류"));
                            }
                        }
                    }
                }
            });
        }
    }

    public void OnBaitSelected(int type)
    {
        foreach (RectTransform item in BaitItem)
        {
            item.Find("Select_Image").gameObject.SetActive(false);
        }

        Color enableColor = new Color(0.5254902f, 0.8431373f, 0.9921569f, 1.0f);
        Color disableColor = new Color(0.6f, 0.6352941f, 0.6509804f, 1.0f);
        Image buttonImage = UIBaitSelect.transform.Find("Background").Find("BottomUI_Panel").Find("OkButtonLayer").Find("OkButton").GetComponent<Image>();

        buttonImage.color = disableColor;

        string desc = "";
        int baitCount = 0;
        switch (type)
        {
            case 0://일반
                baitCount = (int)neco_data.Instance.FishingData.Baits;
                desc = LocalizeData.GetText("미끼설명_지렁이");
                break;
            case 1://조은거
                baitCount = (int)user_items.GetUserItemAmount(149);
                desc = LocalizeData.GetText("미끼설명_새우");
                break;
            case 2://더 조은거
                baitCount = (int)user_items.GetUserItemAmount(150);
                desc = LocalizeData.GetText("미끼설명_고등어");
                break;
        }
        BaitItem[type].Find("Select_Image").gameObject.SetActive(true);
        UIBaitSelect.transform.Find("Background").Find("MiddleUI_Panel").Find("Bait").Find("BaitIcon").Find("BaitInfo").Find("MessageText_1").GetComponent<Text>().text = desc;

        if (baitCount > 0)
        {
            FishingHook.GetComponent<Image>().enabled = true;
            FishingHook.GetComponent<Image>().sprite = BaitSprites[type];
            FishingHook.GetComponent<Image>().SetNativeSize();

            buttonImage.color = enableColor;
        }
    }
    public void OnStartGame()
    {
        int type = -1;
        for(int i = 0; i < BaitItem.Length; i++)
        {
            if (BaitItem[i].Find("Select_Image").gameObject.activeSelf)
                type = i;
        }

        int baitCount = 0;
        switch (type)
        {
            case 0://일반
                baitCount = (int)neco_data.Instance.FishingData.Baits;
                break;
            case 1://조은거
                baitCount = (int)user_items.GetUserItemAmount(149);
                break;
            case 2://더 조은거
                baitCount = (int)user_items.GetUserItemAmount(150);
                break;
        }

        if (TestMode)
        {
            UIBaitSelect.SetActive(false);
            GameSet();
            return;
        }

        if (type < 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("미끼선택오류"));
            return;
        }
        else if(baitCount <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("미끼수량부족"));
            return;
        }

        UIBaitSelect.SetActive(false);


        // 낚시 미끼선택 API
        WWWForm data = new WWWForm();
        data.AddField("api", "fish");
        data.AddField("op", 2);
        switch(type)
        {
            case 1:
                data.AddField("bait", 149);
                break;
            case 2:
                data.AddField("bait", 150);
                break;
        }
        

        NetworkManager.GetInstance().SendApiRequest("fish", 2, data, (response) =>
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
                if (uri == "fish")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            neco_data.Instance.FishingData.Baits = row["baits"].Value<uint>();
                            fish_type = row["type"].Value<uint>();
                            fish_scale = row["scale"].Value<uint>();
                            GameSet();
                        }
                        else
                        {
                            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.FISHING_POPUP);
                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("error"), LocalizeData.GetText("낚시정보동기화오류"));
                        }
                    }
                }
            }
        });
    }

    IEnumerator ViberateControl()
    {
        long playTime = 100;
        int[] amp = { 20, 40, 80, 40, 20, 10 };

        int index = 0;
        while (true)
        {
            RDG.Vibration.Vibrate(playTime, amp[index], true);

            index = (index + 1) % amp.Length;

            yield return new WaitForSeconds(playTime * 0.001f);
        }
    }

    public void InitFishingUI()
    {
        //서버에서 필요한 Req 받고 init
        gameObject.SetActive(true);

    }

    public void onClickCloseButton()
    {
        if(!GamePlaying)
        {
            NecoCanvas.GetUICanvas().OnFishingAlarm();
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.FISHING_POPUP);
        }
        else
        {
            ConfirmPopupData param = new ConfirmPopupData();

            param.titleText = LocalizeData.GetText("LOCALIZE_250");
            param.titleMessageText = LocalizeData.GetText("낚시중지");

            param.messageText_1 = LocalizeData.GetText("낚시중지메시지");
            NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(param, CONFIRM_POPUP_TYPE.COMMON, () => {
                GamePlaying = false;
                onClickCloseButton();
            });
        }
    }

    void FollowHook()
    {
        FishingHook.skeletonGraphic = Game;
        FishingHook.SetBone("pivot");
        FishingHook.followBoneRotation = true;
        FishingHook.followXYPosition = true;
        FishingHook.followZPosition = false;
        FishingHook.followLocalScale = false;
        FishingHook.followSkeletonFlip = false;
    }

    void OnGameResult(RewardData reward)
    {
        UIGameClear.SetActive(true);
        GameObject bg = UIGameClear.transform.Find("background").gameObject;
        bg.SetActive(false);
        GamePlaying = false;

        StartCoroutine(delayAction(()=> {
            bg.SetActive(true);            
        }, 1.0f));

        if (reward == null)
            return;

        Transform layer = UIGameClear.transform.Find("fishingSuccess").Find("fishInfoLayer").Find("IconLayer");
        if (reward.gold > 0)
        {
            layer.Find("RecipeIcon").GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
            layer.Find("Count_Text").GetComponent<Text>().text = reward.gold > 1 ? "x" + reward.gold.ToString() : "";
            layer.Find("RecipeNameText").GetComponent<Text>().text = LocalizeData.GetText("LOCALIZE_229");            
        }
        if (reward.catnip > 0)
        {
            layer.Find("RecipeIcon").GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_catleaf");
            layer.Find("Count_Text").GetComponent<Text>().text = reward.catnip > 1 ? "x" + reward.catnip.ToString() : "";
            layer.Find("RecipeNameText").GetComponent<Text>().text = LocalizeData.GetText("LOCALIZE_348");
        }
        if (reward.itemData != null)
        {
            layer.Find("RecipeIcon").GetComponent<Image>().sprite = reward.itemData.GetItemIcon();
            layer.Find("Count_Text").GetComponent<Text>().text = reward.count > 1 ? "x" + reward.count.ToString() : "";

            string itemName = reward.itemData.GetItemName();
            if (fish_len > 0)
                itemName = (fish_len / 10.0f).ToString("n1") + "cm " + itemName;
            layer.Find("RecipeNameText").GetComponent<Text>().text = itemName;
        }
    }

    IEnumerator delayAction(System.Action cb, float delay = 0.0f)
    {
        yield return new WaitForSeconds(delay);
        cb?.Invoke();
    }

    public void OnHelpPopup()
    {
        HelpPopup.SetActive(!HelpPopup.activeSelf);
    }

    [ContextMenu("Test")]
    public void TEST()
    {
        FishSample.transform.localPosition = hookPos;
    }

    public void OnSelectUser(int rank)
    {
        if (rank - 1 < RankUserNo.Length)
        {
            uint accountNo = RankUserNo[rank - 1];
            if (accountNo != 0)
            {
                if (accountNo == NetworkManager.GetInstance().UserNo)
                {
                    return;
                }
                else
                {
                    List<UserProfile> friends = FriendsManager.Instance.GetFriendList();
                    foreach (UserProfile friend in friends)
                    {
                        if (friend.uno == accountNo)
                        {
                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_FRIEND"));
                            return;
                        }
                    }

                    List<UserProfile> sents = FriendsManager.Instance.GetSentList();
                    foreach (UserProfile sent in sents)
                    {
                        if (sent.uno == accountNo)
                        {
                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_SENT"));
                            return;
                        }
                    }

                    WWWForm data = new WWWForm();
                    data.AddField("api", "friend");
                    data.AddField("op", 5);
                    data.AddField("uno", accountNo.ToString());

                    NetworkManager.GetInstance().SendApiRequest("friend", 5, data, (response) =>
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
                            if (uri == "friend")
                            {
                                JToken resultCode = row["rs"];
                                if (resultCode != null && resultCode.Type == JTokenType.Integer)
                                {
                                    int rs = resultCode.Value<int>();
                                    switch (rs)
                                    {
                                        case 0:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("친구요청메시지"));
                                            break;
                                        case 1:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NO_SUCH_USER"));
                                            break;
                                        case 2:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_FRIEND"));
                                            break;
                                        case 3:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_SENT"));
                                            break;
                                        case 4:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NO_REQUEST_SENT"));
                                            break;
                                        case 5:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NO_REQUEST_TAKEN"));
                                            break;
                                        case 6:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("FRIEND_LIST_FULL"));
                                            break;
                                        case 7:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NOT_A_FRIEND"));
                                            break;
                                        case 8:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_RECEIVED"));
                                            break;
                                        case 9:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("CANNOT_SEND_YET"));
                                            break;
                                        case 10:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NOTHING_TO_RECEIVE"));
                                            break;
                                        case 11:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GIFT_DAILY_LIMITED"));
                                            break;
                                        default:
                                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_199"));
                                            break; 
                                    }
                                }
                            }
                        }
                        
                    }, null, false);
                }
            }
        }
    }
}
