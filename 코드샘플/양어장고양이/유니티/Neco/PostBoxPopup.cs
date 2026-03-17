using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostDataManager
{
    public delegate void Callback();
    public delegate void RewardCallback(List<RewardData> data);
    public class PostData
    {
        public PostData(JObject data)
        {
            uid = data["uid"].Value<uint>();
            title = data["title"].Value<string>();
            body = data["body"].Value<string>();
            state = data["state"].Value<uint>();
            exp = data["exp"].Value<uint>();
            at_type = data["at_type"].Value<uint>();
            at_id = data["at_id"].Value<uint>();
            at_cnt = data["at_cnt"].Value<uint>();
        }

        public uint uid;
        public string title;
        public string body;
        public uint state;
        public uint exp;
        public uint at_type;
        public uint at_id;
        public uint at_cnt;
    };

    private List<PostData> PostList = new List<PostData>();

    public List<PostData> GetPostList() { return PostList; }

    public void RequestPostList(Callback cb)
    {
        PostList.Clear();

        WWWForm data = new WWWForm();
        data.AddField("api", "post");
        data.AddField("op", 1);

        NetworkManager.GetInstance().SendApiRequest("post", 1, data, (response) =>
        {
            PostList.Clear();

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
                if (uri == "post")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            JArray list = (JArray)row["list"];
                            foreach (JObject post in list)
                            {
                                PostList.Add(new PostData(post));
                            }

                            cb?.Invoke();

                            // 우편물 레드닷 갱신
                            neco_data.Instance.SetOpenPostTimestamp(NecoCanvas.GetCurTime());
                            NecoCanvas.GetUICanvas().RefreshTopMenuRedDot();
                        }
                        else
                        {
                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_338"), LocalizeData.GetText("LOCALIZE_339"));
                        }
                    }
                }
            }
        });
    }

    public void TryReciveOnePost(uint post_id, RewardCallback cb)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "post");
        data.AddField("op", 2);
        data.AddField("uid", post_id.ToString());

        NetworkManager.GetInstance().SendApiRequest("post", 2, data, (response) =>
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
                if (uri == "post")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            List<RewardData> ret = new List<RewardData>();
                            if (row.ContainsKey("rew") && row["rew"].HasValues)
                            {
                                JObject income = (JObject)row["rew"];
                                if (income.ContainsKey("gold"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.gold = income["gold"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("catnip"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.catnip = income["catnip"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("point"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.point = income["point"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("item"))
                                {
                                    JArray item = (JArray)income["item"];
                                    foreach (JObject rw in item)
                                    {
                                        RewardData reward = new RewardData();
                                        reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                        reward.count = rw["amount"].Value<uint>();
                                        ret.Add(reward);
                                    }
                                }

                                if (income.ContainsKey("memory"))
                                {
                                    JArray memory = (JArray)income["memory"];
                                    foreach (JArray rw in memory)
                                    {
                                        RewardData reward = new RewardData();
                                        reward.memoryData = neco_cat_memory.GetNecoMemory(rw[0].Value<uint>());
                                        reward.point = rw[1].Value<uint>();
                                        ret.Add(reward);
                                    }
                                }
                            }

                            cb?.Invoke(ret);
                            return;
                        }
                        else
                        {
                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_338"), LocalizeData.GetText("LOCALIZE_339"));
                        }
                    }
                }
            }
        }, (err)=> {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_338"), LocalizeData.GetText("LOCALIZE_339"));
        });
    }

    public void TryReciveAllPost(RewardCallback cb)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "post");
        data.AddField("op", 3);

        NetworkManager.GetInstance().SendApiRequest("post", 3, data, (response) =>
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
                if (uri == "post")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            List<RewardData> ret = new List<RewardData>();
                            if (row.ContainsKey("rew"))
                            {
                                JObject income = (JObject)row["rew"];
                                if (income.ContainsKey("gold"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.gold = income["gold"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("catnip"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.catnip = income["catnip"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("point"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.point = income["point"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("item"))
                                {
                                    JArray item = (JArray)income["item"];
                                    foreach (JObject rw in item)
                                    {
                                        RewardData reward = new RewardData();
                                        reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                        reward.count = rw["amount"].Value<uint>();
                                        ret.Add(reward);
                                    }
                                }

                                if (income.ContainsKey("memory"))
                                {
                                    JArray memory = (JArray)income["memory"];

                                    Dictionary<string, uint> memoryDic = new Dictionary<string, uint>();
                                    
                                    foreach (JArray rw in memory)
                                    {
                                        neco_cat_memory catMemory = neco_cat_memory.GetNecoMemory(rw[0].Value<uint>());
                                        if (catMemory == null) continue;

                                        if (rw[1].Value<uint>() > 0)
                                        {
                                            if(!memoryDic.ContainsKey("point"))
                                                memoryDic.Add("point", 0);
                                            memoryDic["point"] += rw[1].Value<uint>();  // 포인트로 합산
                                        }
                                        else if (memoryDic.ContainsKey(catMemory.GetNecoMemoryID().ToString()) == false)
                                        {
                                            memoryDic.Add(catMemory.GetNecoMemoryID().ToString(), 1);
                                        }
                                    }

                                    foreach (var memoryPair in memoryDic)
                                    {
                                        RewardData reward = new RewardData();
                                        
                                        if (memoryPair.Key == "point")
                                        {
                                            reward.point = memoryPair.Value;
                                        }
                                        else
                                        {
                                            reward.memoryData = neco_cat_memory.GetNecoMemory(uint.Parse(memoryPair.Key));
                                        }

                                        ret.Add(reward);
                                    }
                                }
                            }
                            cb?.Invoke(ret);
                            return;
                        }
                        else
                        {
                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_338"), LocalizeData.GetText("LOCALIZE_339"));
                        }
                    }
                }
            }
        }, (err) => {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_338"), LocalizeData.GetText("LOCALIZE_339"));
        });
    }
}

public class PostBoxPopup : MonoBehaviour
{
    private PostDataManager postData = new PostDataManager();

    public GameObject MailInfoPopup;

    public GameObject ListContainer;
    public GameObject ListSampleObject;

    [Header("[Button Layers]")]
    public Color originButtonColor;
    public Color dimmedButtonColor;

    private void OnEnable()
    {
        ClearPostUI();
    }

    public void ClearPostUI()
    {
        CloseInfoPopup();
        ClearList();

        postData.RequestPostList(OnListRefresh);
    }

    public void ClearList()
    {
        foreach(Transform item in ListContainer.transform)
        {
            if (item != ListSampleObject.transform)
                Destroy(item.gameObject);
        }

        Button reciveAllBtn = transform.Find("Panel").transform.Find("BottomUI_Panel").Find("ButtonLayer").Find("GetGiftButton").GetComponent<Button>();
        reciveAllBtn.interactable = false;
    }

    public void OnListRefresh()
    {
        ClearList();

        bool recivable = false;
        ListSampleObject.SetActive(true);
        List<PostDataManager.PostData> list = postData.GetPostList();
        foreach(PostDataManager.PostData post in list)
        {
            GameObject mailRow = Instantiate(ListSampleObject);
            mailRow.transform.SetParent(ListContainer.transform);
            mailRow.transform.localScale = ListSampleObject.transform.localScale;
            mailRow.transform.localPosition = ListSampleObject.transform.localPosition;

            Image itemIcon = mailRow.transform.Find("ItemLayer").Find("Icon").GetComponent<Image>();
            Text itemCount = mailRow.transform.Find("ItemLayer").Find("Text").GetComponent<Text>();
            Text TitleText = mailRow.transform.Find("TitleText").GetComponent<Text>();
            Text DescText = mailRow.transform.Find("ContentText").GetComponent<Text>();
            Button button = mailRow.transform.Find("GetButton").GetComponent<Button>();
            string itemname = "";

            TitleText.text = post.title;
            DescText.text = post.body;
            if (post.at_cnt > 1)
                itemCount.text = post.at_cnt.ToString("n0");
            else
                itemCount.text = "";

            switch (post.at_type)
            {
                case 1:
                    {
                        switch(post.at_id)
                        {
                            case 0:
                                itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                                itemname = LocalizeData.GetText("LOCALIZE_229");
                                break;
                            case 1:
                                itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_catleaf");
                                itemname = LocalizeData.GetText("LOCALIZE_348");
                                break;
                            case 2:
                                itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_point");
                                itemname = LocalizeData.GetText("LOCALIZE_335");
                                break;
                        }
                        recivable = true;
                    }
                    break;
                case 2:
                    {
                        itemIcon.sprite = items.GetItem(post.at_id).GetItemIcon();
                        itemname = items.GetItem(post.at_id).GetItemName();
                        recivable = true;
                    }
                    break;
                case 3:
                    {
                        switch (post.at_id)
                        {
                            case 130:
                                itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Box_01");
                                itemname = LocalizeData.GetText("items:name_kr:130");
                                break;
                            case 131:
                                itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Box_02");
                                itemname = LocalizeData.GetText("items:name_kr:131");
                                break;
                            case 132:
                                itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Box_03");
                                itemname = LocalizeData.GetText("items:name_kr:132");
                                break;
                            case 133:
                                itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Box_04");
                                break;
                            case 134:
                                itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Box_04");
                                break;
                        }
                    }
                    break;
                case 4:
                    {
                        neco_cat_memory memory = neco_cat_memory.GetNecoMemory(post.at_id);
                        if(memory != null)
                            itemIcon.sprite = Resources.Load<Sprite>(memory.GetNecoMemoryThumbnail());
                        itemname = LocalizeData.GetText("LOCALIZE_29");

                        recivable = true;
                    }
                    break;
            }

            if (post.body.Length < 1)
            {
                DescText.text = string.Format(LocalizeData.GetText("재료상세"), itemname, itemCount.text);
            }
                

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(()=> { ShowMailInfoPopup(post); });
        }

        ListSampleObject.SetActive(false);
        Button reciveAllBtn = transform.Find("Panel").transform.Find("BottomUI_Panel").Find("ButtonLayer").Find("GetGiftButton").GetComponent<Button>();
        reciveAllBtn.interactable = recivable;
        reciveAllBtn.GetComponent<Image>().color = recivable ? originButtonColor : dimmedButtonColor;

        transform.Find("Empty_Text").gameObject.SetActive(list.Count == 0);
    }

    public void ShowMailInfoPopup(PostDataManager.PostData post)
    {
        MailInfoPopup.SetActive(true);
        MailInfoPopup.transform.Find("Close_Bt").GetComponent<Button>().interactable = true;

        Transform background = MailInfoPopup.transform.Find("BackgroundPanel");
        background.GetComponent<Button>().onClick.RemoveAllListeners();

        Transform NormalObject = MailInfoPopup.transform.Find("Background");
        Transform BoxObject = MailInfoPopup.transform.Find("BoxSpineObject");
        
        NormalObject.gameObject.SetActive(post.at_type != 3);
        BoxObject.gameObject.SetActive(post.at_type == 3);

        if(post.at_type == 3)
        {
            background.GetComponent<Button>().onClick.AddListener(() => { OnBoxSelect(post.uid); });
            BoxObject.Find("ItemImage").gameObject.SetActive(false);

            Spine.Unity.SkeletonGraphic spine = BoxObject.GetComponent<Spine.Unity.SkeletonGraphic>();
            switch (post.at_id)
            {
                case 130:
                    spine.initialSkinName = "Box1";
                    break;
                case 131:
                    spine.initialSkinName = "Box2";
                    break;
                case 132:
                    spine.initialSkinName = "Box3";
                    break;
                case 133:
                    spine.initialSkinName = "Box4";
                    break;
                case 134:
                    spine.initialSkinName = "Box4";
                    break;
            }

            spine.startingAnimation = "wait";
            spine.AnimationState.TimeScale = 1.0f;

            spine.startingLoop = true;
            spine.Initialize(true);

            MailInfoPopup.transform.Find("BoxSpineObject").Find("WarningTextLayer").gameObject.SetActive(true);
            MailInfoPopup.transform.Find("BoxSpineObject").Find("Touch_Text").gameObject.SetActive(true);

            Text touchText = MailInfoPopup.transform.Find("BoxSpineObject").Find("Touch_Text").GetComponent<Text>();
            touchText.DOKill();
            touchText.color = Color.white;
            touchText.DOColor(new Color(1, 1, 1, 0.7f), 0.3f).SetLoops(-1, LoopType.Yoyo);

            return;
        }

        Image itemIcon = NormalObject.Find("MidUI_Panel").Find("CommonItemInfoLayer").Find("ItemIcon").GetComponent<Image>();
        Text itemCount = NormalObject.Find("MidUI_Panel").Find("CommonItemInfoLayer").Find("ItemAmounText").GetComponent<Text>();
        Text itemName = NormalObject.Find("MidUI_Panel").Find("CommonItemInfoLayer").Find("ItemNameText").GetComponent<Text>();
        Text TitleText = NormalObject.Find("TopUI_Panel").Find("TitleBg").Find("TitleText").GetComponent<Text>();
        Text DescText = NormalObject.Find("MidUI_Panel").Find("ContentLayer").Find("Bg").Find("Text").GetComponent<Text>();
        Button button = NormalObject.Find("BottomUI_Panel").Find("PurchaseButtonLayer").Find("PurchaseBg").GetComponent<Button>();

        GameObject contentLayer = NormalObject.Find("MidUI_Panel").Find("ContentLayer")?.gameObject;
        string itemname = "";

        items item = items.GetItem(post.at_id);

        TitleText.text = post.title;
        DescText.text = post.body;

        if (post.at_cnt > 1)
            itemCount.text = post.at_cnt.ToString();
        else
            itemCount.text = "";

        switch (post.at_type)
        {
            case 1:
                {
                    switch (post.at_id)
                    {
                        case 0:
                            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                            itemName.text = LocalizeData.GetText("LOCALIZE_229");
                            itemname = LocalizeData.GetText("LOCALIZE_229");
                            break;
                        case 1:
                            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_catleaf");
                            itemName.text = LocalizeData.GetText("LOCALIZE_348");
                            itemname = LocalizeData.GetText("LOCALIZE_348");
                            break;
                        case 2:
                            itemname = LocalizeData.GetText("LOCALIZE_335");
                            break;
                    }
                }
                break;
            case 2:
                {
                    itemIcon.sprite = items.GetItem(post.at_id).GetItemIcon();
                    itemName.text = item.GetItemName();
                    itemname = items.GetItem(post.at_id).GetItemName();
                }
                break;
            case 3:
                {
                    switch (post.at_id)
                    {
                        case 130:
                            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Box_01");
                            itemName.text = item.GetItemName();
                            itemname = LocalizeData.GetText("items:name_kr:130");
                            break;
                        case 131:
                            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Box_02");
                            itemName.text = item.GetItemName();
                            itemname = LocalizeData.GetText("items:name_kr:131");
                            break;
                        case 132:
                            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Box_03");
                            itemName.text = item.GetItemName();
                            itemname = LocalizeData.GetText("items:name_kr:132");
                            break;
                        case 133:
                            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Box_04");
                            itemName.text = item.GetItemName();
                            break;
                        case 134:
                            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Box_04");
                            itemName.text = item.GetItemName();
                            break;
                    }
                }
                break;
            case 4:
                {
                    neco_cat_memory memory = neco_cat_memory.GetNecoMemory(post.at_id);
                    if (memory != null)
                    {
                        itemIcon.sprite = Resources.Load<Sprite>(memory.GetNecoMemoryThumbnail());
                        itemName.text = "";//memory.GetNecoMemoryTitle(); 빼기로함
                    }
                    itemname = LocalizeData.GetText("LOCALIZE_29");
                }
                break;
        }

        if (post.body.Length < 1)
            DescText.text = string.Format(LocalizeData.GetText("재료상세"), itemname, itemCount.text);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { postData.TryReciveOnePost(post.uid, OnReciveItem); });
    }

    public void OnReciveAll()
    {
        List<PostDataManager.PostData> list = postData.GetPostList();
        foreach (PostDataManager.PostData post in list)
        {
            if(post.at_type != 3)
            {
                postData.TryReciveAllPost(OnReciveItem);
                return;
            }
        }

        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_338"), LocalizeData.GetText("LOCALIZE_340"));
    }

    public void CloseInfoPopup()
    {
        MailInfoPopup.SetActive(false);
    }

    public void OnMailButton()
    {
        IAPManager.GetInstance().CheckPendingProducts();
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.MAIL_BOX_POPUP);
    }

    public void OnCloseMailBox()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.MAIL_BOX_POPUP);
    }

    public void OnReciveItem(List<RewardData> reward)
    {
        ClearPostUI();

        if(reward.Count == 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_341"));
        }
        else if (reward.Count == 1)
        {
            NecoCanvas.GetPopupCanvas().OnSingleRewardPopup(LocalizeData.GetText("LOCALIZE_342"), LocalizeData.GetText("LOCALIZE_343"), reward[0]);
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnRewardListPopup(LocalizeData.GetText("LOCALIZE_342"), LocalizeData.GetText("LOCALIZE_343"), reward);
        }
    }

    public void OnBoxSelect(uint uid)
    {
        Transform background = MailInfoPopup.transform.Find("BackgroundPanel");
        background.GetComponent<Button>().onClick.RemoveAllListeners();

        postData.TryReciveOnePost(uid, OnBoxOpenAction);

        MailInfoPopup.transform.Find("BoxSpineObject").Find("WarningTextLayer").gameObject.SetActive(false);
        MailInfoPopup.transform.Find("BoxSpineObject").Find("Touch_Text").gameObject.SetActive(false);
        
        MailInfoPopup.transform.Find("Close_Bt").GetComponent<Button>().interactable = false;
    }

    public void OnBoxOpenAction(List<RewardData> reward)
    {
        if (reward.Count == 0)
        {
            ClearPostUI();
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_344"));
            return;
        }

        if(reward[0].memoryData != null)
        {
            ClearPostUI();
            neco_cat_memory memory = reward[0].memoryData;
            NecoCanvas.GetPopupCanvas().OnShowGetCatPhotoBoxPopup(neco_cat.GetNecoCat(memory.GetNecoMemoryCatID()), memory, ()=> {
                if (reward[0].point > 0)
                {
                    reward[0].memoryData = null;
                    NecoCanvas.GetPopupCanvas().OnSingleRewardPopup(LocalizeData.GetText("LOCALIZE_342"), LocalizeData.GetText("LOCALIZE_345"), reward[0]);
                }
            });
            return;
        }

        Transform BoxObject = MailInfoPopup.transform.Find("BoxSpineObject");
        Spine.Unity.SkeletonGraphic spine = BoxObject.GetComponent<Spine.Unity.SkeletonGraphic>();

        spine.startingAnimation = "open";
        spine.startingLoop = false;
        spine.Initialize(true);

        Image itemIcon = BoxObject.Find("ItemImage").GetComponent<Image>();
        Text Amount = itemIcon.transform.Find("Amount").GetComponent<Text>();
        RewardData data = reward[0];
        if(data.gold > 0)
        {
            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
            Amount.text = data.gold.ToString("n0");
        }
        else if (data.catnip > 0)
        {
            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_catleaf");
            Amount.text = data.catnip.ToString("n0");
        }
        else
        {
            itemIcon.sprite = data.itemData.GetItemIcon();
            Amount.text = data.count.ToString("n0");
        }

        float speedRaito = 3.0f;
        Spine.Animation animationObject = spine.skeletonDataAsset.GetSkeletonData(false).FindAnimation("open");
        spine.AnimationState.TimeScale = speedRaito;

        itemIcon.gameObject.SetActive(true);
        StartCoroutine(DelayCallFunc((animationObject.Duration / speedRaito) + 1, () => { OnReciveItem(reward); }));
    }

    IEnumerator DelayCallFunc(float seconds, Action act)
    {
        yield return new WaitForSeconds(seconds);
        act();
    }

    [ContextMenu("상자모두받기")]
    public void ReciveAllBox()
    {
        List<PostDataManager.PostData> list = postData.GetPostList();
        foreach (PostDataManager.PostData post in list)
        {
            if (post.at_type == 3)
            {
                postData.TryReciveOnePost(post.uid, (data)=> { Invoke("ReciveAllBox", 0.1f); });
                return;
            }
        }

        Invoke("ClearPostUI", 0.3f);
    }
}
