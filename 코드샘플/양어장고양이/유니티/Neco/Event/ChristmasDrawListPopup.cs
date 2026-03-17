using Crosstales;
using DG.Tweening.Plugins;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChristmasDrawListPopup : MonoBehaviour
{
    const uint MAX_LEVEL = 13;
    List<KeyValuePair<string, List<uint>>> DRAW_ID;
    class DrawListInfo : ChristmasDrawLayer.DrawInfo
    {
        public enum ANIMATE_TYPE { MATERIAL_1 = 101, MATERIAL_2, MATERIAL_3, FOOD_1 = 111, FOOD_2, FOOD_3, BOX = 121, STATIC }
        public ANIMATE_TYPE aType;
        public uint aIndex;
        public uint weight;
        public DrawListInfo(ANIMATE_TYPE _atype, string _type = "", uint _id = 0, uint _amount = 0, uint _weight = 0) : base(_type, _id, _amount)
        {
            weight = _weight;
            aType = _atype;
            switch (aType)
            {
                case ANIMATE_TYPE.MATERIAL_1:
                case ANIMATE_TYPE.MATERIAL_2:
                case ANIMATE_TYPE.MATERIAL_3:
                case ANIMATE_TYPE.FOOD_1:
                case ANIMATE_TYPE.FOOD_2:
                case ANIMATE_TYPE.FOOD_3:
                case ANIMATE_TYPE.BOX:
                    type = ItemType.ITEM;
                    break;

                case ANIMATE_TYPE.STATIC:
                    break;
            }
            aIndex = 0;
        }
    }
    IEnumerator refresher = null;
    List<DrawListInfo> drawLists = null;
    List<RewardInfo> drawCloneList = null;
    public List<GameObject> contentList = null;
    

    public ScrollRect masterScroll;
    public GameObject itemClone;

    public void Init()
    {
        if (drawLists == null)
            drawLists = new List<DrawListInfo>();
        if (contentList == null)
            contentList = new List<GameObject>();
        if (drawCloneList == null)
            drawCloneList = new List<RewardInfo>();

        if (refresher != null)
        {
            StopCoroutine(refresher);
            refresher = null;
        }
        refresher = Refresh();

        if (drawCloneList.Count != 0)
            return;

        List<neco_event_xmas_gacha> gachaList = neco_event_xmas_gacha.GetNecoGachaList();
        DRAW_ID = new List<KeyValuePair<string, List<uint>>>();
        DRAW_ID.Add(new KeyValuePair<string, List<uint>>(((int)DrawListInfo.ANIMATE_TYPE.MATERIAL_1).ToString(),new List<uint>()));
        DRAW_ID.Add(new KeyValuePair<string, List<uint>>(((int)DrawListInfo.ANIMATE_TYPE.MATERIAL_2).ToString(), new List<uint>()));
        DRAW_ID.Add(new KeyValuePair<string, List<uint>>(((int)DrawListInfo.ANIMATE_TYPE.MATERIAL_3).ToString(), new List<uint>()));
        DRAW_ID.Add(new KeyValuePair<string, List<uint>>(((int)DrawListInfo.ANIMATE_TYPE.FOOD_1).ToString(), new List<uint>()));
        DRAW_ID.Add(new KeyValuePair<string, List<uint>>(((int)DrawListInfo.ANIMATE_TYPE.FOOD_2).ToString(), new List<uint>()));
        DRAW_ID.Add(new KeyValuePair<string, List<uint>>(((int)DrawListInfo.ANIMATE_TYPE.FOOD_3).ToString(), new List<uint>()));
        DRAW_ID.Add(new KeyValuePair<string, List<uint>>(((int)DrawListInfo.ANIMATE_TYPE.BOX).ToString(), new List<uint>()));

        foreach (neco_event_xmas_gacha info in gachaList)
        {
            if(info.GetNecoEventGroup_ID() > MAX_LEVEL)
            {
                DRAW_ID.Find((keyvalue) => keyvalue.Key == info.GetNecoEventGroup_ID().ToString()).Value.Add(info.GetNecoEventRewID());
            }
        }

        {
            //level 1

            for(int i = 0; i < MAX_LEVEL; i++)
            {
                for(int j = 0; j < gachaList.Count; j++)
                {
                    if (gachaList[j].GetNecoEventGroup_ID() > MAX_LEVEL)
                        continue;

                    if(gachaList[j].GetNecoEventGroup_ID()-1 == i)
                    {
                        DrawListInfo.ANIMATE_TYPE _atype;
                        string _type = "";
                        uint _id = 0;

                        switch (gachaList[j].GetNecoEventRew_type())
                        {
                            case "group":
                                _atype = (DrawListInfo.ANIMATE_TYPE)gachaList[j].GetNecoEventRewID();
                                break;

                            default:
                                _atype = DrawListInfo.ANIMATE_TYPE.STATIC;
                                _type = gachaList[j].GetNecoEventRew_type();

                                if (gachaList[j].GetNecoEventRew_type() == "point" || gachaList[j].GetNecoEventRew_type() == "dia" || gachaList[j].GetNecoEventRew_type() == "gold")
                                {
                                    if (gachaList[j].GetNecoEventRew_type() == "gold")
                                    {
                                        _id = 0;
                                    }
                                    else if (gachaList[j].GetNecoEventRew_type() == "dia")
                                    {
                                        _type = "catnip";
                                        _id = 1;
                                    }
                                    else
                                    {
                                        _id = 2;
                                    }
                                }
                                else
                                {
                                    _id = gachaList[j].GetNecoEventRewID();
                                }
                                break;
                        }

                        drawLists.Add(new DrawListInfo(_atype, _type, _id, gachaList[j].GetNecoEventRewCount(), gachaList[j].GetNecoEventWeight()));

                        GameObject obj = Instantiate(itemClone, contentList[i].GetComponent<ScrollRect>().content.transform);
                        obj.SetActive(true);
                        drawCloneList.Add(obj.GetComponent<RewardInfo>());
                    }
                }
            }
        }

        foreach(GameObject obj in contentList)
        {
            int index = contentList.IndexOf(obj);
            obj.GetComponent<ScrollRect>().content.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
            obj.CTFind("LevelText").GetComponent<Text>().text = string.Format(LocalizeData.GetText("Xmasbox_ChanceInfo_level"), index+1);
        }
    }

    private void OnEnable()
    {
        Init();
        StartCoroutine(refresher);
    }

    IEnumerator Refresh()
    {
        foreach (DrawListInfo info in drawLists)
        {
            if (info.aType == DrawListInfo.ANIMATE_TYPE.STATIC)
            {
                int index = drawLists.IndexOf(info);
                drawCloneList[index].SetRewardInfoData(info.ToRewardData());
                drawCloneList[index].CTFind("itemWeight").GetComponent<Text>().text = info.weight.ToString()+"%";
            }
        }

        while (true)
        {
            foreach(DrawListInfo info in drawLists)
            {
                if(info.aType <= DrawListInfo.ANIMATE_TYPE.BOX)
                {
                    int index = drawLists.IndexOf(info);
                    ++info.aIndex;
                    if (DRAW_ID.Find((keyvalue)=> keyvalue.Key == ((int)info.aType).ToString()).Value.Count <= info.aIndex)
                    {
                        info.aIndex = 0;
                    }
                    info.id = DRAW_ID.Find((keyvalue) => keyvalue.Key == ((int)info.aType).ToString()).Value[(int)info.aIndex];
                    drawCloneList[index].SetRewardInfoData(info.ToRewardData());
                    drawCloneList[index].CTFind("itemWeight").GetComponent<Text>().text = info.weight.ToString() + "%";
                }
            }
            yield return new WaitForSeconds(0.8f);
        }

        yield return null;
    }

    private void OnDisable()
    {
        StopCoroutine(refresher);
        refresher = null;
    }
}
