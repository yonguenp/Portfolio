using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageInfoUI : MonoBehaviour
{
    public AreaUI areaUI;
    public GameObject[] Star;
    public GameObject[] StarIcon;
    public Text[] StarDesc;
    public RewardListItem RewardItemSample;
    public GameObject RewardContainer;
    
    public Image myFullnessGuage;
    public Image needGuage;
    public GameObject fullnessGuageIcon;
    public GameObject CanvasCloseButton;
    stage curStageData;
    
    public void Open(stage target)
    {
        gameObject.SetActive(true);
        curStageData = target;

        CanvasCloseButton.SetActive(false);

        SetUI();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        CanvasCloseButton.SetActive(true);
    }

    public void OnStartStage()
    {
        areaUI.StageStart(curStageData);
    }

    public void SetUI()
    {
        foreach (Transform child in RewardContainer.transform)
        {
            if (child.gameObject != RewardItemSample.gameObject)
            {
                Destroy(child.gameObject);
            }
        }

        if (curStageData != null)
        {
            List<string> conditionDesc = new List<string>();

            List<game_data> clip_event = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);
            uint stageID = curStageData.GetStageID();
            foreach (game_data clip in clip_event)
            {
                clip_event evtData = (clip_event)clip;
                if (evtData.GetStageNo() == stageID)
                {
                    JObject conData = evtData.GetSuccessConditionData();
                    if(conData.ContainsKey("dsc"))
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

            uint curStar = curStageData.GetCurStar();

            for (int i = 0; i < Star.Length; i++)
            {
                if (i < curStageData.GetMaxStar())
                {
                    Star[i].SetActive(true);
                    if (conditionDesc.Count > i)
                        StarDesc[i].text = conditionDesc[i];
                    
                    StarIcon[i].SetActive(i < curStar);
                }
                else
                {
                    Star[i].SetActive(false);
                }
            }
        }

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

        uint needFullness = curStageData.GetNeedFullness();
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
        if (ani != null)
        {
            if (needFullness < curFullness)
                ani.Play("hungry_gauge");
            else
                ani.Play("hungry_gauge2");
        }
    }
}

