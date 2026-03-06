using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxInfoPopup : MonoBehaviour
{
    [SerializeField] Transform[] pivot;
    [SerializeField] PerItem perItem;
    public void Init()
    {
        Clear();

        var tableDatas = Managers.Data.GetData(GameDataManager.DATA_TYPE.event_box);
        perItem.gameObject.SetActive(true);

        Dictionary<int, List<EventBoxData>> weight_dic = new Dictionary<int, List<EventBoxData>>();
        foreach (EventBoxData boxData in tableDatas)
        {
            if (weight_dic.ContainsKey(boxData.item_no))
                weight_dic[boxData.item_no].Add(boxData);
            else weight_dic.Add(boxData.item_no, new List<EventBoxData> { boxData });
        }

        int idx = 0;
        foreach (var key in weight_dic.Keys)
        {
            foreach (var list in weight_dic[key])
            {
                PerItem obj = GameObject.Instantiate(perItem, pivot[idx]) as PerItem;
                var reward = ShopPackageGameData.GetRewardDataList(list.goods_id);
                if (reward.Count > 0)
                {
                    obj.icon.SetReward(reward[0]);
                    string str = "";

                    str = reward[0].GetName();
                    if (reward.Count == 1)
                    {
                        switch ((ASSET_TYPE)reward[0].goods_type)
                        {
                            case ASSET_TYPE.GOLD:
                                str = StringManager.GetString("gold_name");
                                break;
                            case ASSET_TYPE.DIA:
                                str = StringManager.GetString("dia_name");
                                break;
                            case ASSET_TYPE.MILEAGE:
                                str = StringManager.GetString("mileage_name");
                                break;
                            case ASSET_TYPE.ITEM:
                            case ASSET_TYPE.EQUIPMENT:
                            case ASSET_TYPE.BUFF_ITEM:
                                if(reward[0].targetItem != null)                                    
                                    str = reward[0].targetItem.GetName();
                                break;
                            case ASSET_TYPE.CHARACTER:
                                if (reward[0].targetCharacter != null)
                                    str = reward[0].targetCharacter.GetName();
                                break;
                            default:
                                str = reward[0].GetName();
                                break;
                        }

                        if(reward[0].goods_amount > 1)
                            str += " " + StringManager.GetString("ui_count", reward[0].goods_amount);
                    }

                    obj.name.text = str;

                    float totalWeight = 0;
                    foreach (var item in weight_dic[key])
                    {
                        totalWeight += item.rates;
                    }

                    obj.perCnt.text = (((float)list.rates / totalWeight) * 100).ToString("F2") + "%";
                }
                else
                {
                    SBDebug.LogError($"보상 데이터 오류 ::{reward}");
                }
            }
            idx++;
        }

        perItem.gameObject.SetActive(false);

        LayoutRebuilder.ForceRebuildLayoutImmediate(pivot[0].parent.GetComponent<RectTransform>());
    }
    void Clear()
    {
        foreach (Transform tr in pivot)
        {
            foreach (Transform per in tr)
            {
                if (per == perItem.transform)
                    continue;
                Destroy(per.gameObject);
            }
        }
    }

    private void Update()
    {
        float maxSize = 0.0f;
        foreach(var p in pivot)
        {
            maxSize = Mathf.Max((p as RectTransform).sizeDelta.y, maxSize);
        }
        RectTransform container = pivot[0].parent as RectTransform;
        container.sizeDelta = new Vector2(container.sizeDelta.x, maxSize);
    }
}

public class EventBoxData : GameData
{
    public int uid { get; private set; }
    public int item_no { get; private set; }
    public int rates { get; private set; }

    public int goods_id { get; private set; }
    public override void SetValue(Dictionary<string, string> tmp)
    {
        base.SetValue(tmp);
        uid = Int(tmp["uid"]);
        item_no = Int(tmp["item_no"]);
        rates = Int(tmp["rates"]);
        goods_id = Int(tmp["goods_id"]);
    }

    static public List<EventBoxData> GetItemGroupList(int item_no)
    {
        List<EventBoxData> ret = new List<EventBoxData>();

        foreach (EventBoxData item in Managers.Data.GetData(GameDataManager.DATA_TYPE.event_box))
        {
            if (item.item_no == item_no)
                ret.Add(item);
        }

        return ret; 
    }
}
