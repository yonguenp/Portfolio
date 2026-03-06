using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaInfoUI : MonoBehaviour
{
    public AreaUI areaUI;
    public StageItem StageItemSample;
    public GameObject StageContainer;    

    public GameObject SubInfoPanel;
    public RewardListItem RewardItemSample;
    public GameObject RewardContainer;

    public GameObject SelectCatButton;
    public GameObject StartAutoModeButton;

    public GameObject AreaClearUI;
    public Image clearGuage;
    public GameObject[] clearStar;
    public Text[] clearStarText;

    public GameObject NeedHungryUI;
    public Image myFullnessGuage;
    public Image needGuage;
    public GameObject fullnessGuageIcon;
    

    public GameObject CanvasCloseButton;

    public GameObject[] Star;
    public Animation[] StarIcon;
    public Text[] StarDesc;

    public Text Title;

    AreaItem curAreaItem;
    bool bAutoMode = false;
    public void Open(AreaItem target)
    {
        gameObject.SetActive(true);
        curAreaItem = target;

        SetAreaStageList();

        CanvasCloseButton.SetActive(false);
        string titleKey = "";
        switch(target.world_type)
        {
            case 1:
                titleKey = "area_foreground";
                break;
            case 2:
                titleKey = "area_warehouse";
                break;
            case 3:
                titleKey = "area_fishfarm";
                break;
            case 4:
                titleKey = "area_ha3house";
                break;
            case 5:
                titleKey = "area_pond";
                break;
        }

        Title.text = LocalizeData.GetText(titleKey);
    }

    public void Close()
    {
        areaUI.OnAreaItemSelected(null);
        gameObject.SetActive(false);
        CanvasCloseButton.SetActive(true);
    }

    public void OnSubInfoOpen(stage stageData)
    {
        SubInfoPanel.SetActive(true);

        foreach (Transform child in RewardContainer.transform)
        {
            if (child.gameObject != RewardItemSample.gameObject)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (Transform child in StageContainer.transform)
        {
            StageItem item = child.GetComponent<StageItem>();
            item.SetCursor(stageData == item.GetData());
        }

        RewardItemSample.gameObject.SetActive(true);
        List<game_data> clip_event = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);
        uint stageID = stageData.GetStageID();
        List<string> conditionDesc = new List<string>();
        foreach (game_data clip in clip_event)
        {
            clip_event evtData = (clip_event)clip;
            if (evtData.GetStageNo() == stageID)
            {
                JObject conData = evtData.GetSuccessConditionData();
                if (conData.ContainsKey("dsc"))
                    conditionDesc.Add(conData["dsc"].Value<string>());

                JArray rewArray = evtData.GetRewardData();
                foreach (JToken token in rewArray)
                {
                    JArray row = (JArray)token;
                    foreach (JToken r in row)
                    {
                        string key = r.ToString();
                        if (key == "item" || key == "card")
                        {
                            GameObject listItem = Instantiate(RewardItemSample.gameObject);
                            listItem.transform.SetParent(RewardContainer.transform);
                            RectTransform rt = listItem.GetComponent<RectTransform>();
                            rt.localPosition = Vector3.zero;
                            rt.localScale = Vector3.one;

                            List<game_data> dataList = key == "item" ? GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ITEMS) 
                                : key == "card" ? GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CARD_DEFINE)
                                : null;

                            if (dataList != null)
                            {
                                string idKey = key == "item" ? "item_id" 
                                : key == "card" ? "card_id"
                                : "";
                                string iconKey = key == "item" ? "icon_img"
                                : key == "card" ? "cover_img"
                                : "";
                                string image = "";

                                JToken noToken = r.Next;
                                string no = noToken.ToString();
                                uint itemNo = System.Convert.ToUInt32(no);
                            
                                object obj;
                                foreach (game_data data in dataList)
                                {
                                    if (data.data.TryGetValue(idKey, out obj))
                                    {
                                        if (itemNo == (uint)obj)
                                        {
                                            if (!string.IsNullOrEmpty(iconKey) && data.data.TryGetValue(iconKey, out obj))
                                            {
                                                image = (string)obj;
                                            }
                                        }
                                    }
                                }

                                RewardListItem item = listItem.GetComponent<RewardListItem>();
                                item.SetRewarditem(image, "");
                            }

                        }
                    }
                }
            }
        }

        uint curStar = stageData.GetCurStar();

        for (int i = 0; i < Star.Length; i++)
        {
            if (i < stageData.GetMaxStar())
            {
                Star[i].SetActive(true);
                if (conditionDesc.Count > i)
                    StarDesc[i].text = conditionDesc[i];

                StarIcon[i].transform.Find("Get_Star").gameObject.SetActive(i < curStar);
            }
            else
            {
                Star[i].SetActive(false);
            }
        }


        RewardItemSample.gameObject.SetActive(false);
    }

    public void ClearUI()
    {
        foreach(Transform child in StageContainer.transform)
        {
            if (child.gameObject != StageItemSample.gameObject)
            {
                Destroy(child.gameObject);
            }            
        }
        foreach (Transform child in RewardContainer.transform)
        {
            if (child.gameObject != RewardItemSample.gameObject)
            {
                Destroy(child.gameObject);
            }
        }


        SubInfoPanel.SetActive(false);
        StageItemSample.gameObject.SetActive(false);
        RewardItemSample.gameObject.SetActive(false);
    }

    public void SetAreaStageList()
    {
        ClearUI();

        List<game_data> stages = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.STAGE);
        bool isAutoEnable = false;
        uint maxStar = 0;
        uint curStar = 0;
        foreach (game_data gd in stages)
        {
            stage stage = (stage)gd;
            if (stage.GetLocationID() == curAreaItem.world_type)
            {
                StageItem item = StageItemSample.CloneItem(StageContainer.transform, stage);
                item.SetButtonCallback((stage stageData) => {
                    OnSubInfoOpen(stageData);
                });

                maxStar += stage.GetMaxStar();
                curStar += stage.GetCurStar();
            }

            if (stage.GetMaxStar() <= stage.GetCurStar())
            {
                isAutoEnable = true;
            }
        }

        bAutoMode = false;
        SelectCatButton.SetActive(true);
        SelectCatButton.GetComponent<Button>().interactable = isAutoEnable;
        StartAutoModeButton.SetActive(false);

        AreaClearUI.SetActive(true);
        NeedHungryUI.SetActive(false);

        clearGuage.fillAmount = float.Parse(curStar.ToString()) / float.Parse(maxStar.ToString());
        float range = 1.0f / clearStar.Length;
        for (int i = 0; i < clearStar.Length; i++)
        {
            clearStar[i].SetActive(range * (i + 1) <= clearGuage.fillAmount);
            clearStarText[i].text = ((int)(range * (i + 1) * float.Parse(maxStar.ToString()))).ToString();

            if(clearStar[i].activeSelf)
            {
                users userData = GameDataManager.Instance.GetUserData();
                Dictionary<uint, List<uint>> location_rew = ((Dictionary<uint, List<uint>>)userData.data["location_rew"]);
                if (location_rew.ContainsKey(curAreaItem.world_type))
                {
                    List<uint> pastRewards = location_rew[curAreaItem.world_type];

                    uint idx = System.Convert.ToUInt32(i + 1);
                    foreach (uint past in pastRewards)
                    {
                        if (past == idx)
                        {
                            clearStar[i].GetComponent<Button>().interactable = false;
                        }
                    }
                }
                else
                {
                    clearStar[i].GetComponent<Button>().interactable = false;
                }
            }
        }
    }

    public void OnClearRewardButton(int index)
    {
        users userData = GameDataManager.Instance.GetUserData();
        Dictionary<uint, List<uint>> location_rew = ((Dictionary<uint, List<uint>>)userData.data["location_rew"]);
        List<uint> pastRewards = location_rew[curAreaItem.world_type];

        uint idx = System.Convert.ToUInt32(index);
        foreach(uint past in pastRewards)
        {
            if (past == idx)
                return;
        }

        pastRewards.Add(idx);
        clearStar[index - 1].GetComponent<Button>().interactable = false;

        WWWForm data = new WWWForm();
        data.AddField("api", "adventure");
        data.AddField("op", 8);
        data.AddField("location", curAreaItem.world_type.ToString());
        data.AddField("type", index);

        NetworkManager.GetInstance().SendApiRequest("adventure", 8, data, (res) => {
            WorldCanvas canvas = GetComponentInParent<WorldCanvas>();
            canvas.ResponseEventData(res);
        });
    }


    public void SetEnableAutoList()
    {
        ClearUI();

        List<game_data> stages = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.STAGE);
        foreach (game_data gd in stages)
        {
            stage stage = (stage)gd;
            if (stage.GetMaxStar() <= stage.GetCurStar())
            {
                if (stage.GetLocationID() == curAreaItem.world_type)
                {
                    bool isAutoEnable = false;
                    StageItem item = StageItemSample.CloneItem(StageContainer.transform, stage);
                    item.SetButtonCallback((stage stageData) => {

                        foreach (Transform child in StageContainer.transform)
                        {
                            StageItem stageItem = child.GetComponent<StageItem>();
                            if (stageData == stageItem.GetData())
                            {
                                stageItem.SetCursor(!stageItem.IsCursor());
                            }

                            if(stageItem.IsCursor())
                                isAutoEnable = true;
                        }

                        StartAutoModeButton.GetComponent<Button>().interactable = isAutoEnable;
                        RefreshFullnessGuageUI();
                    });
                }
            }
        }

        bAutoMode = true;

        SelectCatButton.SetActive(false);
        StartAutoModeButton.SetActive(true);
        StartAutoModeButton.GetComponent<Button>().interactable = false;

        AreaClearUI.SetActive(false);
        NeedHungryUI.SetActive(true);

        RefreshFullnessGuageUI();
    }

    public void RefreshFullnessGuageUI()
    {
        users user = GameDataManager.Instance.GetUserData();
        List<game_data> exp_table = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.EXP);
        uint maxFullness = 0;
        uint curFullness = 0;
        object obj;
        if (user.data.TryGetValue("level", out obj))
        {
            uint level = (uint)obj;
            foreach (game_data exp in exp_table)
            {
                if (exp.data.TryGetValue("level", out obj))
                {
                    if (level == (uint)obj)
                    {
                        if (exp.data.TryGetValue("max_fullness", out obj))
                        {
                            maxFullness = (uint)obj;
                        }
                    }
                }
            }

            if (user.data.TryGetValue("fullness", out obj))
            {
                curFullness = (uint)obj;
            }
        }

        float ratio = float.Parse(curFullness.ToString()) / maxFullness;
        myFullnessGuage.fillAmount = ratio;

        uint needFullness = 0;
        foreach (Transform child in StageContainer.transform)
        {
            StageItem stageItem = child.GetComponent<StageItem>();
            if (stageItem.GetData() != null && stageItem.IsCursor())
            {
                needFullness += stageItem.GetData().GetNeedFullness();
            }
        }
        ratio = float.Parse(needFullness.ToString()) / maxFullness;
        needGuage.fillAmount = myFullnessGuage.fillAmount - ratio;

        //RectTransform rect = myFullnessGuage.GetComponent<RectTransform>();
        //if (rect)
        //{
        //    float width = rect.rect.size.x;
        //    Vector2 localPos = fullnessGuageIcon.transform.localPosition;
        //    localPos.x = width * (needGuage.fillAmount - 0.5f);
        //    fullnessGuageIcon.transform.localPosition = localPos;
        //}

        Animation ani = myFullnessGuage.GetComponent<Animation>();
        if(ani != null)
        {
            if(needFullness < curFullness)
                ani.Play("hungry_gauge");
            else
                ani.Play("hungry_gauge2");
        }
    }

    public void OnAutoSelectMode()
    {
        SetEnableAutoList();
    }

    public void OnStartAutoMode()
    {
        List<stage> listStage = new List<stage>();
        foreach (Transform child in StageContainer.transform)
        {
            StageItem item = child.GetComponent<StageItem>();
            if (item.IsCursor())
            {
                if(item.GetData() != null)
                    listStage.Add(item.GetData());
            }
        }

        areaUI.AutoStart(listStage);
    }
}
