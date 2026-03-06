using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using static PopupControl;
using System;

public class RewardListUI : MonoBehaviour
{
    public bool isShow = false;

    public GameCanvas GameCanvas = null;
    public GameMain GameManager;
    public GameObject PanelBackground;
    public GameObject RewardInfoPanel;
    public GameObject ItemListPanel;
    public GameObject ItemListContainer;
    public GameObject ItemListPrefab;
    //public float FullSizeHeight;
    //public float WithOutItemListSizeHeight;
    //public float WithOutRewardInfoSizeHeight;

    public Text ExpPoint;
    public Text GoldPoint;

    public GameObject ExpEffectPrefab;
    public GameObject GoldEffectPrefab;

    public Transform ExpGoal;
    public Transform GoldGoal;

    public Text RewardTitle;
    public Text RewardDesc;

    public GameObject StarPanel;
    public GameObject[] Star;
    public Animation[] StarAnimation;
    public Text[] StarDesc;

    public string LevelUPTitle;
    public string LevelUPDesc;

    public string TouchTitle;
    public string TouchDesc;

    public string CollectionTitle;
    public string CollectionDesc;

    public string IdleTitle;
    public string IdleDesc;

    public string FeedTitle;
    public string FeedDesc;

    public string CookTitle;
    public string CookDesc;

    public string FishingTitle;
    public string FishingDesc;

    private Coroutine CoroutineBackGroundAction = null;

    private void Awake()
    {
        StarPanel.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnEnable()
    {
        StarPanel.SetActive(false);
    }

    public void OnDisable()
    {
        StarPanel.SetActive(false);
    }

    //public void OnRewardButton()
    //{
    //    CloseRewardList();
    //}

    public void ShowRewardList()
    {
        isShow = true;
        gameObject.SetActive(true);
        StarPanel.SetActive(false);

        gameObject.GetComponent<DOTweenAnimation>().DOPlayForward();
        PanelBackground.SetActive(true);
        PanelBackground.transform.SetSiblingIndex(transform.GetSiblingIndex() - 1);

        if (CoroutineBackGroundAction != null)
            StopCoroutine(CoroutineBackGroundAction);

        CoroutineBackGroundAction = StartCoroutine(RunAlphaAction(PanelBackground.GetComponent<Image>(), 0, 0.5f));
        //PanelBackground.GetComponent<DOTweenAnimation>().DOPlayForward();
        CancelInvoke("OnCompleteTweenAnimation");
    }

    public IEnumerator RunAlphaAction(Image target, float start, float end)
    {
        float startAlpha = start;
        float maxAlpha = end;
        float curTime = 0.0f;
        float actionTime = 0.5f;
        Color color = target.color;
        color.a = startAlpha;
        target.color = color;

        while (curTime < actionTime)
        {
            float delta = Time.deltaTime;
            curTime += delta;
            color.a += delta * actionTime * maxAlpha;

            yield return new WaitForEndOfFrame();
        }

        color.a = maxAlpha;
        target.color = color;
    }


    public bool ShowIdleRewardList(string data)
    {
        //not used
        RewardTitle.text = LocalizeData.GetText(IdleTitle);
        RewardDesc.text = LocalizeData.GetText(IdleDesc);

        JObject root = JObject.Parse(data);

        JToken apiToken = root["api"];
        if (null == apiToken || apiToken.Type != JTokenType.Array
            || !apiToken.HasValues)
        {
            return false;
        }

        JObject income = null;
        JArray apiArr = (JArray)apiToken;
        foreach (JObject row in apiArr)
        {
            string uri = row.GetValue("uri").ToString();
            if(uri == "inter")
            {
                JToken opCode = row["op"];
                if (opCode != null && opCode.Type == JTokenType.Integer)
                {
                    int op = opCode.Value<int>();
                    if(op == 4)
                    {
                        JToken rew = row.GetValue("rew");
                        if (rew != null)
                            income = (JObject)rew;
                    }
                }
            }
        }

        if (income == null)
            return false;

        uint gold = 0;
        JToken obj;
        if (income.TryGetValue("gold", out obj))
            gold = obj.Value<uint>();

        uint exp = 0;
        if (income.TryGetValue("exp", out obj))
            exp = obj.Value<uint>();

        bool bRewardItemList = false;
        if (income.ContainsKey("touch"))
            bRewardItemList = true;
        if (income.ContainsKey("play"))
            bRewardItemList = true;
        if (income.ContainsKey("card"))
            bRewardItemList = true;
        if (income.ContainsKey("item"))
            bRewardItemList = true;
        if (income.ContainsKey("food"))
            bRewardItemList = true;

        if (income.ContainsKey("unlock"))
        {
            GameCanvas.GameManager.PopupControl.OnShowUnlockPopup();
        }

        GoldPoint.text = "+" + gold.ToString();
        ExpPoint.text = "+" + exp.ToString();
        
        if(bRewardItemList)
        {
            OnShowItemListMode(income);
        }
        else
        {
            OnWithOutItemListMode();
        }
        ShowRewardList();
        return true;
    }

    public bool ShowCookRewardList(string data)
    {
        RewardTitle.text = LocalizeData.GetText(CookTitle);
        RewardDesc.text = LocalizeData.GetText(CookDesc);

        return ShowRewardList(data);
    }
    public bool ShowTouchRewardList(string data)
    {
        RewardTitle.text = LocalizeData.GetText(TouchTitle);
        RewardDesc.text = LocalizeData.GetText(TouchDesc);

        return ShowRewardList(data);
    }

    public bool ShowRewardList(string data)
    {        
        bool bRewardItemList = false;

        JObject root = JObject.Parse(data);

        JToken apiToken = root["api"];
        if (null == apiToken || apiToken.Type != JTokenType.Array
            || !apiToken.HasValues)
        {
            return false;
        }

        GoldPoint.text = "0";
        ExpPoint.text = "0";
        RewardInfoPanel.SetActive(true);

        JObject income = null;
        JArray apiArr = (JArray)apiToken;
        foreach (JObject row in apiArr)
        {
            income = (JObject)row.GetValue("rew");
            if (income != null)
            {
                if (income.ContainsKey("gold"))
                {
                    GoldPoint.text = "+" + income["gold"].ToString();
                }
                if (income.ContainsKey("exp"))
                {
                    ExpPoint.text = "+" + income["exp"].ToString();
                }
                if (income.ContainsKey("touch"))
                    bRewardItemList = true;
                if (income.ContainsKey("play"))
                    bRewardItemList = true;
                if (income.ContainsKey("card"))
                    bRewardItemList = true;
                if (income.ContainsKey("item"))
                    bRewardItemList = true;
                if (income.ContainsKey("food"))
                    bRewardItemList = true;

                if (income.ContainsKey("unlock"))
                {
                    GameCanvas.GameManager.PopupControl.OnShowUnlockPopup();
                }

                break;
            }
        }

        if (bRewardItemList)
            OnShowItemListMode(income);
        else
            OnWithOutItemListMode();

        ShowRewardList();
        return true;
    }

    public bool ShowTouchRewardList(JObject income, WorldCanvas.STATE_WORLD state = WorldCanvas.STATE_WORLD.WORLD_NONE, stage stageData = null)
    {
        switch (state)
        {
            case WorldCanvas.STATE_WORLD.WORLD_FEED:
                RewardTitle.text = LocalizeData.GetText(FeedTitle);
                RewardDesc.text = LocalizeData.GetText(FeedDesc);
                break;
            case WorldCanvas.STATE_WORLD.WORLD_FISHING:
                RewardTitle.text = LocalizeData.GetText(FishingTitle);
                RewardDesc.text = LocalizeData.GetText(FishingDesc);
                break; 
            default:
                RewardTitle.text = LocalizeData.GetText(TouchTitle);
                RewardDesc.text = LocalizeData.GetText(TouchDesc);
                break;
        }

        bool bRewardItemList = false;

        GoldPoint.text = "0";
        ExpPoint.text = "0";
        RewardInfoPanel.SetActive(true);


        if (income != null)
        {
            if (income.ContainsKey("gold"))
            {
                GoldPoint.text = "+" + income["gold"].ToString();
            }
            if (income.ContainsKey("exp"))
            {
                ExpPoint.text = "+" + income["exp"].ToString();
            }
            if (income.ContainsKey("touch"))
                bRewardItemList = true;
            if (income.ContainsKey("play"))
                bRewardItemList = true;
            if (income.ContainsKey("card"))
                bRewardItemList = true;
            if (income.ContainsKey("item"))
                bRewardItemList = true;
            if (income.ContainsKey("food"))
                bRewardItemList = true;

            if (income.ContainsKey("unlock"))
            {
                GameCanvas.GameManager.PopupControl.OnShowUnlockPopup();
            }
        }

        if (bRewardItemList)
        {
            OnShowItemListMode(income);
            OnShowFullMode();
        }
        else
            OnWithOutItemListMode();

        ShowRewardList();
        
        if(stageData != null)
        {
            List<string> conditionDesc = new List<string>();

            List<game_data> clip_event = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);
            uint stageID = stageData.GetStageID();
            foreach (game_data clip in clip_event)
            {
                clip_event evtData = (clip_event)clip;
                if (evtData.GetStageNo() == stageID)
                {
                    JObject conData = evtData.GetSuccessConditionData();
                    if (conData.ContainsKey("dsc"))
                        conditionDesc.Add(conData["dsc"].Value<string>());
                }

            }

            uint curStar = stageData.GetCurStar();
            StarPanel.SetActive(true);
            
            for(int i = 0; i < Star.Length; i++)
            {
                StopCoroutine("starAnimation");
            }

            for(int i = 0; i < Star.Length; i++)
            {
                if (i < stageData.GetMaxStar())
                {
                    Star[i].SetActive(true);
                    if(conditionDesc.Count > i)
                        StarDesc[i].text = conditionDesc[i];

                    StarAnimation[i].transform.Find("Get_Star").gameObject.SetActive(false);
                    
                    if (i < curStar)
                    {
                        StartCoroutine(starAnimation(StarAnimation[i], 1.0f + (0.6f * i)));
                    }
                }
                else
                {
                    Star[i].SetActive(false);
                }
            }
        }

        return true;
    }

    public IEnumerator starAnimation(Animation anim, float delay)
    {
        anim.Stop();

        yield return new WaitForSeconds(delay);

        anim.Play("star_get");
    }

    public bool ShowCollectionRewardList(uint collectionID)
    {
        RewardTitle.text = LocalizeData.GetText(CollectionTitle);
        RewardDesc.text = LocalizeData.GetText(CollectionDesc);

        foreach (Transform child in ItemListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        List<KeyValuePair<string, uint>> curList = null;

        if (!GameDataManager.Instance.GetUserData().data.ContainsKey("collection_rew"))
            return false;

        List<KeyValuePair<uint, List<KeyValuePair<string, uint>>>> rew = (List<KeyValuePair<uint, List<KeyValuePair<string, uint>>>>)GameDataManager.Instance.GetUserData().data["collection_rew"];
        if (rew != null)
        {
            foreach (KeyValuePair<uint, List<KeyValuePair<string, uint>>> pair in rew)
            {
                if (pair.Key == collectionID)
                {
                    curList = pair.Value;
                }
            }
        }

        if (curList == null)
            return false;

        bool bShowPanel = false;
        bool bShowList = false;

        List<string> iconImage = new List<string>();
        List<string> desc = new List<string>();
        object obj;
        foreach (KeyValuePair<string, uint> item in curList)
        {
            switch (item.Key)
            {
                case "gold":
                    {
                        GoldPoint.text = "+" + item.Value.ToString();
                        bShowPanel = true;
                    }
                    break;

                case "exp":
                    {
                        ExpPoint.text = "+" + item.Value.ToString();
                        bShowPanel = true;
                    }
                    break;

                case "touch":
                    {
                        bShowList = true;
                        List<game_data> touch_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.INTER_TOUCH);
                        KeyValuePair<string, string> value = GetIconImageAndNameInData(touch_list, item.Value, "touch_id", "icon_img", "");
                        iconImage.Add(value.Key);
                        desc.Add(string.IsNullOrEmpty(value.Value) ? "만지기 클립" : value.Value);
                    }
                    break;

                case "play":
                    {
                        bShowList = true;
                        List<game_data> play_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.INTER_PLAY);
                        KeyValuePair<string, string> value = GetIconImageAndNameInData(play_list, item.Value, "play_id", "icon_img", "");
                        iconImage.Add(value.Key);
                        desc.Add(string.IsNullOrEmpty(value.Value) ? "놀기 클립" : value.Value);
                    }
                    break;

                case "card":
                    {
                        bShowList = true;
                        iconImage.Add("card:" + item.Value);
                        desc.Add(LocalizeData.GetText("card"));
                    }
                    break;
                case "item":
                    {
                        bShowList = true;
                        List<game_data> item_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ITEMS);
                        KeyValuePair<string, string> value = GetIconImageAndNameInData(item_list, item.Value, "item_id", "icon_img", "name_kr");
                        iconImage.Add(value.Key);
                        desc.Add(string.IsNullOrEmpty(value.Value) ? LocalizeData.GetText("item") : value.Value);                        
                    }
                    break;
                case "food":
                    {
                        bShowList = true;
                        List<game_data> food_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.FOOD);
                        KeyValuePair<string, string> value = GetIconImageAndNameInData(food_list, item.Value, "food_id", "icon_img", "food_name_kr");
                        iconImage.Add(value.Key);
                        desc.Add(string.IsNullOrEmpty(value.Value) ? LocalizeData.GetText("food") : value.Value);                        
                    }
                    break;
                case "unlock":
                    GameCanvas.GameManager.PopupControl.OnShowUnlockPopup();
                    break;
            }
        }

        for (int i = 0; i < iconImage.Count; i++)
        {
            GameObject listItem = Instantiate(ItemListPrefab);
            listItem.transform.SetParent(ItemListContainer.transform);
            RectTransform rt = listItem.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;

            RewardListItem component = listItem.GetComponent<RewardListItem>();
            component.SetRewarditem(iconImage[i], desc[i]);
        }

        ItemListContainer.GetComponentInParent<ScrollRect>().DOVerticalNormalizedPos(0, 0.0f);

        if (bShowPanel && bShowList)
        {
            OnShowFullMode();
        }
        else if (bShowList)
        {
            OnShowItemListMode();
        }
        else if (bShowPanel)
        {
            OnWithOutItemListMode();
        }
        else
        {
            return false;
        }

        ShowRewardList();
        return true;
    }

    public void ReadyLevelUpReward()
    {
        CancelInvoke("OnCompleteTweenAnimation");

        isShow = true;
        gameObject.SetActive(true);
        PanelBackground.SetActive(true);
        PanelBackground.transform.SetSiblingIndex(transform.GetSiblingIndex() - 1);
        Image img = PanelBackground.GetComponent<Image>();

        if (CoroutineBackGroundAction != null)
            StopCoroutine(CoroutineBackGroundAction);

        CoroutineBackGroundAction = StartCoroutine(RunAlphaAction(img, img.color.a, 0.0f));
    }

    public bool ShowLevelUpRewardList(uint level)
    {
        RewardTitle.text = LocalizeData.GetText(LevelUPTitle);
        RewardDesc.text = LocalizeData.GetText(LevelUPDesc);

        foreach (Transform child in ItemListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        List<KeyValuePair<string, uint>> curList = null;

        if (!GameDataManager.Instance.GetUserData().data.ContainsKey("rew"))
            return false;

        List<KeyValuePair<uint, List<KeyValuePair<string, uint>>>> rew = (List<KeyValuePair<uint, List<KeyValuePair<string, uint>>>>)GameDataManager.Instance.GetUserData().data["rew"];
        if (rew != null)
        {
            foreach (KeyValuePair<uint, List<KeyValuePair<string, uint>>> pair in rew)
            {
                if (pair.Key == level)
                {
                    curList = pair.Value;
                    rew.Remove(pair);
                    break;
                }
            }
        }

        if (curList == null)
            return false;

        bool bShowPanel = false;
        bool bShowList = false;

        List<string> iconImage = new List<string>();
        List<string> desc = new List<string>();
        List<int> count = new List<int>();
        object obj;
        GoldPoint.text = "0";
        ExpPoint.text = "0";
        foreach(KeyValuePair<string, uint> item in curList)
        {
            switch(item.Key)
            {
                case "gold":
                    {
                        GoldPoint.text = "+" + item.Value.ToString();                        
                        bShowPanel = true;
                    }
                    break;

                case "exp":
                    {
                        ExpPoint.text = "+" + item.Value.ToString();
                        bShowPanel = true;
                    }
                    break;

                case "touch":
                    {
                        bShowList = true;
                        List<game_data> touch_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.INTER_TOUCH);
                        KeyValuePair<string, string> value = GetIconImageAndNameInData(touch_list, item.Value, "touch_id", "icon_img", "");
                        iconImage.Add(value.Key);
                        desc.Add(string.IsNullOrEmpty(value.Value) ? "만지기 클립" : value.Value);
                        count.Add(1);
                    }
                    break;

                case "play":
                    {
                        bShowList = true;
                        List<game_data> play_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.INTER_PLAY);
                        KeyValuePair<string, string> value = GetIconImageAndNameInData(play_list, item.Value, "play_id", "icon_img", "");
                        iconImage.Add(value.Key);
                        desc.Add(string.IsNullOrEmpty(value.Value) ? "놀기 클립" : value.Value);
                        count.Add(1);
                    }
                    break;

                case "card":
                    {
                        bShowList = true;
                        
                        iconImage.Add("card:" + item.Value);
                        desc.Add(LocalizeData.GetText("card"));
                        count.Add(1);
                    }
                    break;
                case "item":
                    {
                        bShowList = true;
                        List<game_data> item_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ITEMS);
                        KeyValuePair<string, string> value = GetIconImageAndNameInData(item_list, item.Value, "item_id", "icon_img", "name_kr");
                        if (iconImage.Contains(value.Key))
                        {
                            count[iconImage.IndexOf(value.Key)] = count[iconImage.IndexOf(value.Key)] + 1;
                        }
                        else
                        {
                            iconImage.Add(value.Key);
                            desc.Add(string.IsNullOrEmpty(value.Value) ? LocalizeData.GetText("item") : value.Value);
                            count.Add(1);
                        }
                    }
                    break;
                case "food":
                    {
                        bShowList = true;
                        List<game_data> food_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.FOOD);
                        KeyValuePair<string, string> value = GetIconImageAndNameInData(food_list, item.Value, "food_id", "icon_img", "food_name_kr");
                        if (iconImage.Contains(value.Key))
                        {
                            count[iconImage.IndexOf(value.Key)] = count[iconImage.IndexOf(value.Key)] + 1;
                        }
                        else
                        {
                            iconImage.Add(value.Key);
                            desc.Add(string.IsNullOrEmpty(value.Value) ? LocalizeData.GetText("food") : value.Value);
                            count.Add(1);
                        }
                    }break;
                case "unlock":
                    GameCanvas.GameManager.PopupControl.OnShowUnlockPopup();
                    break;
            }
        }

        for (int i = 0; i < iconImage.Count; i++)
        {
            GameObject listItem = Instantiate(ItemListPrefab);
            listItem.transform.SetParent(ItemListContainer.transform);
            RectTransform rt = listItem.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;

            RewardListItem component = listItem.GetComponent<RewardListItem>();
            component.SetRewarditem(iconImage[i], desc[i], count[i]);
        }

        ItemListContainer.GetComponentInParent<ScrollRect>().DOVerticalNormalizedPos(0, 0.0f);

        if (bShowPanel && bShowList)
        {
            OnShowFullMode();
        }
        else if(bShowList)
        {
            OnShowItemListMode();
        }
        else if(bShowPanel)
        {
            OnWithOutItemListMode();
        }
        else
        {
            return false;
        }

        ShowRewardList();
        return true;
    }

    public bool CloseRewardList()
    {
        //PanelBackground.GetComponent<DOTweenAnimation>().DOPlayBackwards();

        if (CoroutineBackGroundAction != null)
            StopCoroutine(CoroutineBackGroundAction);

        CoroutineBackGroundAction = StartCoroutine(RunAlphaAction(PanelBackground.GetComponent<Image>(), 0.5f, 0.0f));
        bool bNeedRewardEffect = RewardInfoPanel.activeSelf;
        isShow = false;
        gameObject.SetActive(true);
        gameObject.GetComponent<DOTweenAnimation>().DOPlayBackwards();

        //Camera UICam = null;
        //Camera[] cams = Camera.allCameras;
        //foreach (Camera cam in cams)
        //{
        //    if (cam.transform.name == "UICamera")
        //        UICam = cam;
        //}
        if (bNeedRewardEffect)
        {
            if (ExpGoal.gameObject.activeInHierarchy)
            {
                if (Camera.main != null && int.Parse(ExpPoint.text) > 0)
                {
                    Vector3 localpos = ExpPoint.transform.position;
                    GameObject ExpEffect = Instantiate(ExpEffectPrefab);
                    ExpEffect.transform.SetParent(Camera.main.transform);
                    localpos.z = Camera.main.transform.position.z * -1;
                    ExpEffect.transform.position = localpos;
                    Vector3 goalPos = ExpGoal.position;
                    goalPos.z = Camera.main.transform.position.z * -1;
                    ExpEffect.transform.DOMove(goalPos, 1.0f);
                    Destroy(ExpEffect, 1.5f);
                }
            }
            if (GoldGoal.gameObject.activeInHierarchy)
            {
                if (Camera.main != null && int.Parse(GoldPoint.text) > 0)
                {
                    Vector3 localpos = GoldPoint.transform.position;
                    GameObject GoldEffect = Instantiate(GoldEffectPrefab);
                    GoldEffect.transform.SetParent(Camera.main.transform);
                    localpos.z = Camera.main.transform.position.z * -1;
                    GoldEffect.transform.position = localpos;
                    Vector3 goalPos = GoldGoal.position;
                    goalPos.z = Camera.main.transform.position.z * -1;
                    GoldEffect.transform.DOLocalMove(goalPos, 1.0f);
                    Destroy(GoldEffect, 1.5f);
                }
            }
        }
        CancelInvoke("OnCompleteTweenAnimation");
        Invoke("OnCompleteTweenAnimation", 0.8f);

        return bNeedRewardEffect;
    }

    public void OnCompleteTweenAnimation()
    {
        gameObject.SetActive(isShow);
        PanelBackground.SetActive(isShow);

        if (IsLevelUpReward())
        {
            ReadyLevelUpReward();
        }

        //((GameCanvas)GameManager.GameCanvas).RefreshProfileUI();
        GameManager.FarmCanvas.FarmUIPanel.Refresh();
        if(isShow == false)
            PanelBackground.transform.SetSiblingIndex(0);
    }

    public bool IsLevelUpReward()
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return false;

        uint realLevel = 0;
        object obj;
        if (user.data.TryGetValue("level", out obj))
        {
            realLevel = (uint)obj;
        }

        GameCanvas gameCanvas = (GameCanvas)GameManager.GameCanvas;
        uint displayLevel = gameCanvas.GetCurDisplayLevel();

        List<uint> levels = new List<uint>();
        for(uint i = displayLevel; i <= realLevel; i++)
        {
            levels.Add(i);
        }

        if (levels.Count == 0)
            return false;
        if (!GameDataManager.Instance.GetUserData().data.ContainsKey("rew"))
            return false;

        List<KeyValuePair<uint, List<KeyValuePair<string, uint>>>> rew = (List<KeyValuePair<uint, List<KeyValuePair<string, uint>>>>)GameDataManager.Instance.GetUserData().data["rew"];
        if (rew != null)
        {
            foreach (KeyValuePair<uint, List<KeyValuePair<string, uint>>> pair in rew)
            {
                foreach (uint level in levels)
                {
                    if (pair.Key == level)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void OnShowItemListMode(JObject income)
    {
        OnShowItemListMode();

        foreach (Transform child in ItemListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        List<string> iconImage = new List<string>();
        List<string> desc = new List<string>();
        List<int> count = new List<int>();
        object obj;

        if (income != null)
        {
            if (income.ContainsKey("touch"))
            {
                foreach (JObject val in income["touch"])
                {
                    uint touchID = val.Value<uint>();
                    List<game_data> touch_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.INTER_TOUCH);
                    KeyValuePair<string, string> value = GetIconImageAndNameInData(touch_list, touchID, "touch_id", "icon_img", "");
                    iconImage.Add(value.Key);
                    desc.Add(string.IsNullOrEmpty(value.Value) ? "만지기 클립" : value.Value);
                    count.Add(1);
                }
            }
            if (income.ContainsKey("play"))
            {
                foreach (JObject val in income["play"])
                {
                    uint playID = val.Value<uint>();
                    List<game_data> play_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.INTER_PLAY);
                    KeyValuePair<string, string> value = GetIconImageAndNameInData(play_list, playID, "play_id", "icon_img", "");
                    iconImage.Add(value.Key);
                    desc.Add(string.IsNullOrEmpty(value.Value) ? "놀기 클립" : value.Value);
                    count.Add(1);
                }
            }
            if (income.ContainsKey("card"))
            {
                JArray cards = (JArray)income["card"];
                foreach (JToken card in cards)
                {
                    uint cardID = card.Value<uint>();
                    iconImage.Add("card:" + cardID);
                    desc.Add(LocalizeData.GetText("card"));
                    count.Add(1);
                }
            }
            if (income.ContainsKey("item"))
            {
                List<game_data> item_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ITEMS);
                if (item_list != null)
                {
                    JArray items = (JArray)income["item"];
                    foreach (JObject item in items)
                    {
                        uint itemID = item["id"].Value<uint>();
                        KeyValuePair<string, string> value = GetIconImageAndNameInData(item_list, itemID, "item_id", "icon_img", "name_kr");
                        iconImage.Add(value.Key);
                        desc.Add(string.IsNullOrEmpty(value.Value) ? LocalizeData.GetText("item") : value.Value);
                        count.Add(Convert.ToInt32(item["amount"].Value<uint>()));
                    }
                }
            }
            if (income.ContainsKey("food"))
            {
                List<game_data> food_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.FOOD);
                if (food_list != null)
                {
                    JArray foods = (JArray)income["food"];
                    foreach (JObject food in foods)
                    {
                        uint foodID = food["id"].Value<uint>();
                        KeyValuePair<string, string> value = GetIconImageAndNameInData(food_list, foodID, "food_id", "icon_img", "food_name_kr");
                        iconImage.Add(value.Key);
                        desc.Add(string.IsNullOrEmpty(value.Value) ? LocalizeData.GetText("food") : value.Value);
                        count.Add(Convert.ToInt32(food["amount"].Value<uint>()));
                    }
                }
            }

            if (income.ContainsKey("unlock"))
            {
                GameCanvas.GameManager.PopupControl.OnShowUnlockPopup();
            }
            
            for (int i = 0; i < iconImage.Count; i++)
            {
                GameObject listItem = Instantiate(ItemListPrefab);
                listItem.transform.SetParent(ItemListContainer.transform);
                RectTransform rt = listItem.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;

                RewardListItem component = listItem.GetComponent<RewardListItem>();
                component.SetRewarditem(iconImage[i], desc[i], count[i]);
            }

            ItemListContainer.GetComponentInParent<ScrollRect>().DOVerticalNormalizedPos(0, 0.0f);
        }
    }

    public KeyValuePair<string, string> GetIconImageAndNameInData(List<game_data> dataList, uint findID, string idKey, string iconKey, string nameKey)
    {
        string image = "";
        string name = "";

        if (dataList != null)
        {
            object obj;
            foreach (game_data data in dataList)
            {
                if (data.data.TryGetValue(idKey, out obj))
                {
                    if (findID == (uint)obj)
                    {
                        if (!string.IsNullOrEmpty(iconKey) && data.data.TryGetValue(iconKey, out obj))
                        {
                            image = (string)obj;
                        }
                        if (!string.IsNullOrEmpty(nameKey) && data.data.TryGetValue(nameKey, out obj))
                        {
                            name = (string)obj;
                        }
                    }
                }
            }
        }

        return new KeyValuePair<string, string>(image, name);
    }

    public void OnWithOutItemListMode()
    {
        //GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, WithOutItemListSizeHeight);
        ItemListPanel.SetActive(false);
        RewardInfoPanel.SetActive(true);
    }

    public void OnShowItemListMode()
    {
        //GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, WithOutRewardInfoSizeHeight);
        ItemListPanel.SetActive(true);
        RewardInfoPanel.SetActive(false);
    }

    public void OnShowFullMode()
    {
        //GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, FullSizeHeight);
        ItemListPanel.SetActive(true);
        RewardInfoPanel.SetActive(true);
    }

    public void OnCloseButton()
    {
        CloseRewardList();

        if (!GameCanvas.CookListUI.gameObject.activeSelf && !GameCanvas.FoodListUI.gameObject.activeSelf)
        {
            Invoke("OnCloseActionDone", 0.5f);            
        }
    }

    public void OnCloseActionDone()
    {
        GameCanvas.OnShowUI();
    }
}
