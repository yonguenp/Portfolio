using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardData
{
    public items itemData;
    public neco_cat_memory memoryData;
    
    public uint gold = 0;
    public uint catnip = 0;
    public uint point = 0;
    public uint count = 0;
}

public class NecoRewardPopup : MonoBehaviour
{
    public delegate void Callback();

    public Text TitleText;
    public Text MessageText;
    public Button button;

    [Header("[Reward List]")]
    public GameObject rewardListScrollContainer;
    public GameObject rewardCloneObject;
    public int centerSortCount = 3;

    Animator animator;
    Callback closeCallback = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (animator != null)
        {
            animator.Play("popup_open");
        }
    }

    public void OnClickCloseButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.REWARD_POPUP);
        closeCallback?.Invoke();
    }

    public bool SetRewardPopupData(string title, string msg, string uriParam, JArray rewardArray, Callback _closeCallback = null)
    {
        ClearRewardData();

        closeCallback = _closeCallback;

        TitleText.text = title;
        MessageText.text = msg;

        List<RewardData> rewardList = new List<RewardData>();

        foreach (JObject row in rewardArray)
        {
            string uri = row.GetValue("uri").ToString();
            if (uri == uriParam)
            {
                JToken resultCode = row["rs"];
                if (resultCode != null && resultCode.Type == JTokenType.Integer)
                {
                    int rs = resultCode.Value<int>();
                    if (rs == 0)
                    {
                        if (row.ContainsKey("rew"))
                        {
                            if (row["rew"].HasValues)
                            {
                                JObject income = (JObject)row["rew"];
                                if (income.ContainsKey("gold"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.gold = income["gold"].Value<uint>();
                                    rewardList.Add(reward);
                                }

                                if (income.ContainsKey("catnip"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.catnip = income["catnip"].Value<uint>();
                                    rewardList.Add(reward);
                                }

                                if (income.ContainsKey("point"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.point = income["point"].Value<uint>();
                                    rewardList.Add(reward);
                                }

                                if (income.ContainsKey("item"))
                                {
                                    JArray item = (JArray)income["item"];
                                    foreach (JObject rw in item)
                                    {
                                        RewardData reward = new RewardData();
                                        reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                        reward.count = rw["amount"].Value<uint>();
                                        rewardList.Add(reward);
                                    }
                                }

                                if (income.ContainsKey("memory"))
                                {
                                    JArray memory = (JArray)income["memory"];

                                    Dictionary<string, uint> memoryDic = new Dictionary<string, uint>();
                                    memoryDic.Add("point", 0);
                                    foreach (JArray rw in memory)
                                    {
                                        neco_cat_memory catMemory = neco_cat_memory.GetNecoMemory(rw[0].Value<uint>());
                                        if (catMemory == null) continue;

                                        if (rw[1].Value<uint>() > 0)
                                        {
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
                                            if (reward.point > 0)
                                            {
                                                rewardList.Add(reward);
                                            }
                                        }
                                        else
                                        {
                                            reward.memoryData = neco_cat_memory.GetNecoMemory(uint.Parse(memoryPair.Key));
                                            rewardList.Add(reward);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), LocalizeData.GetText("LOCALIZE_317"));
                        return false;
                    }
                }
            }
        }

        // 추억 단일 보상일 경우 처리
        if (rewardList.Count == 1 && rewardList.Exists(x => x.memoryData != null))
        {
            neco_cat catData = neco_cat.GetNecoCat(rewardList[0].memoryData.GetNecoMemoryCatID());
            NecoCanvas.GetPopupCanvas().OnShowGetCatPhotoPopup(catData, rewardList[0].memoryData, rewardList[0].point <= 0, new CatPhotoPopup.Callback(closeCallback));
            return true;
        }

        rewardCloneObject.SetActive(true);

        AlignScrollView(rewardList.Count);

        foreach (RewardData data in rewardList)
        {
            GameObject rewardInfoUI = Instantiate(rewardCloneObject);
            rewardInfoUI.transform.SetParent(rewardListScrollContainer.transform);
            rewardInfoUI.transform.localScale = rewardCloneObject.transform.localScale;
            rewardInfoUI.transform.localPosition = rewardCloneObject.transform.localPosition;

            rewardInfoUI.GetComponent<RewardInfo>().SetRewardInfoData(data);
        }

        button.onClick.AddListener(() => {
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.REWARD_POPUP);

            NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
        });

        rewardCloneObject.SetActive(false);

        return false;
    }

    public void SetSingleRewardPopup(string title, string msg, RewardData rewardData, Callback _closeCallback = null)
    {
        if (rewardData == null)
        {
            return;
        }

        RewardData curReward = rewardData;

        ClearRewardData();

        closeCallback = _closeCallback;

        rewardCloneObject.SetActive(true);

        AlignScrollView(1);

        TitleText.text = title;
        MessageText.text = msg;

        GameObject rewardInfoUI = Instantiate(rewardCloneObject);
        rewardInfoUI.transform.SetParent(rewardListScrollContainer.transform);
        rewardInfoUI.transform.localScale = rewardCloneObject.transform.localScale;
        rewardInfoUI.transform.localPosition = rewardCloneObject.transform.localPosition;

        rewardInfoUI.GetComponent<RewardInfo>().SetRewardInfoData(curReward);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.REWARD_POPUP);

            NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
        });

        rewardCloneObject.SetActive(false);

        if (neco_data.GetPrologueSeq() < neco_data.PrologueSeq.프리플레이)
        {
            NecoCanvas.GetPopupCanvas().OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);
        }
    }

    public void SetRewardListPopup(string title, string msg, List<RewardData> rewardList, Callback _closeCallback = null)
    {
        if (rewardList == null || rewardList.Count < 1)
        {
            return;
        }


        ClearRewardData();

        closeCallback = _closeCallback;

        rewardCloneObject.SetActive(true);

        AlignScrollView(rewardList.Count);

        TitleText.text = title;
        MessageText.text = msg;

        foreach (RewardData data in rewardList)
        {
            GameObject rewardInfoUI = Instantiate(rewardCloneObject);
            rewardInfoUI.transform.SetParent(rewardListScrollContainer.transform);
            rewardInfoUI.transform.localScale = rewardCloneObject.transform.localScale;
            rewardInfoUI.transform.localPosition = rewardCloneObject.transform.localPosition;

            rewardInfoUI.GetComponent<RewardInfo>().SetRewardInfoData(data);
        }

        button.onClick.AddListener(() => {
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.REWARD_POPUP);

            NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
        });

        rewardCloneObject.SetActive(false);

        if (neco_data.GetPrologueSeq() < neco_data.PrologueSeq.프리플레이)
        {
            NecoCanvas.GetPopupCanvas().OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);
        }
    }

    public void SetRewardListPopup(string title, string msg, Dictionary<string, RewardData> rewardDic, Callback _closeCallback = null)
    {
        if (rewardDic == null || rewardDic.Count < 1)
        {
            return;
        }

        Dictionary<string, RewardData> curRewardDic = new Dictionary<string, RewardData>(rewardDic);

        ClearRewardData();

        closeCallback = _closeCallback;

        rewardCloneObject.SetActive(true);

        AlignScrollView(rewardDic.Count);

        TitleText.text = title;
        MessageText.text = msg;

        foreach (KeyValuePair<string, RewardData> data in curRewardDic)
        {
            GameObject rewardInfoUI = Instantiate(rewardCloneObject);
            rewardInfoUI.transform.SetParent(rewardListScrollContainer.transform);
            rewardInfoUI.transform.localScale = rewardCloneObject.transform.localScale;
            rewardInfoUI.transform.localPosition = rewardCloneObject.transform.localPosition;

            rewardInfoUI.GetComponent<RewardInfo>().SetRewardInfoData(data.Value);
        }

        button.onClick.AddListener(() => {
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.REWARD_POPUP);

            NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
        });

        rewardCloneObject.SetActive(false);

        if (neco_data.GetPrologueSeq() < neco_data.PrologueSeq.프리플레이)
        {
            NecoCanvas.GetPopupCanvas().OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);
        }
    }

    void AlignScrollView(int dicCount)
    {
        if (rewardListScrollContainer == null) return;

        RectTransform scrollRect = rewardListScrollContainer.GetComponent<RectTransform>();

        if (dicCount > centerSortCount)
        {
            scrollRect.pivot = new Vector2(0, 0.5f);
            scrollRect.anchoredPosition = Vector3.zero;
        }
        else
        {
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = Vector3.zero;
        }
    }

    void ClearRewardData()
    {
        foreach (Transform child in rewardListScrollContainer.transform)
        {
            if (child.gameObject != rewardCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        closeCallback = null;
    }
}
