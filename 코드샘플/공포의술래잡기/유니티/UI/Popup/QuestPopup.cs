using Newtonsoft.Json.Linq;
using SBCommonLib;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestPopup : Popup
{
    [SerializeField] QuestItem questItem;
    [SerializeField] Image tabBtnImage;
    [SerializeField] GameObject eventTermObj;
    [SerializeField] GameObject dailyGuidObj;
    [SerializeField] Text timeGuidText;
    [SerializeField] Transform tab;
    [SerializeField] Text questTerm;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform[] staticTabs;

    public List<GameObject> RedDot = new List<GameObject>();
    public List<int> ableRewardQeust = new List<int>();
    public List<QuestItem> questItemList = new List<QuestItem>();
    public Color curColor;

    int curtabType = 0;
    public bool HasReward { get { return ableRewardQeust.Count > 0; } }
    public override void Open(CloseCallback cb = null)
    {
        Clear();
        SBWeb.GetMyQuestDB((res) =>
        {
            Managers.UserData.SetMyQuestDBData(res);
            RefreshUI();
        });

        foreach (var item in RedDot)
        {
            if (item == null)
                continue;
            item.SetActive(false);
        }

        RefreshMenu();
        TabButton(1, true);

        base.Open(cb);
    }
    public override void Close()
    {
        Clear();
        ClearEventTabList();
        base.Close();
    }

    public override void RefreshUI()
    {
        Clear();
        questItemList.Clear();

        List<QuestData> rewardedQuests = new List<QuestData>();
        foreach (QuestData data in QuestData.GetQuests())
        {
            if ((int)data.quest_type != curtabType)
                continue;

            bool enableQuest = true;
            foreach (int prev in QuestData.GetPrevQuests(data.GetID()))
            {
                enableQuest = Managers.UserData.IsContainClearQuest(prev);

                if (!enableQuest)
                    break;
            }

            if (!enableQuest)
                continue;

            if (Managers.UserData.IsContainClearQuest(data.GetID()))
            {
                rewardedQuests.Add(data);
                continue;
            }

            var obj = GameObject.Instantiate(questItem, questItem.transform.parent);
            obj.transform.localScale = Vector3.one;
            obj.Init(data, this, questItemList.Count);

            obj.gameObject.SetActive(true);
            questItemList.Add(obj);
        }

        List<QuestData> passViewQuest = new List<QuestData>();
        foreach (QuestData data in rewardedQuests)
        {
            foreach (int prev in QuestData.GetPrevQuests(data.GetID()))
            {
                passViewQuest.Add(QuestData.GetQuestData(prev));
            }
        }

        foreach (QuestData data in rewardedQuests)
        {
            if (passViewQuest.Contains(data))
                continue;

            var obj = GameObject.Instantiate(questItem, questItem.transform.parent);
            obj.transform.localScale = Vector3.one;
            obj.Init(data, this, questItemList.Count);

            obj.gameObject.SetActive(true);
            questItemList.Add(obj);
        }

        questItem.gameObject.SetActive(false);

        eventTermObj.SetActive(false);
        dailyGuidObj.SetActive(false);

        if (curtabType >= 6)
            eventTermObj.SetActive(true);

        if (curtabType == 1)
        {
            dailyGuidObj.SetActive(true);
            timeGuidText.text = StringManager.GetString("ui_quest_reset");
        }
        else if (curtabType == 2)
        {
            dailyGuidObj.SetActive(true);
            timeGuidText.text = StringManager.GetString("ui_wquest_reset");
        }

        RefreshItemUI();
        RedDotFlag();
    }

    public void RefreshItemUI()
    {
        scrollRect.verticalNormalizedPosition = 1.0f;
        foreach (var item in questItemList)
        {
            item.QueestSort();
        }
        //foreach (Transform item in questItem.transform.parent)
        //{
        //    item.GetComponent<QuestItem>().RefreshItem();
        //}
    }

    void Clear()
    {
        foreach (Transform item in questItem.transform.parent)
        {
            if (item == questItem.transform)
                continue;

            Destroy(item.gameObject);
        }
    }
    public void OnTryGetAllReward()
    {
        OnTryReward(ableRewardQeust);

    }

    public void OnTryReward(List<int> pakage_id)
    {
        List<int> types = new List<int>();
        types = pakage_id;
        if (types.Count == 0)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_no_reward"));
            return;
        }

        SBWeb.GetQuestReward(types, (res) =>
        {
            SBWeb.GetMyQuestDB((res) =>
            {
                Managers.UserData.SetMyQuestDBData(res);
                RefreshUI();

                var lobby = Managers.Scene.CurrentScene as LobbyScene;
                if (lobby != null)
                    lobby.CheckQuestRedDot(res);
            });
        });
    }

    public void TabButton(int type, bool init = false)
    {
        if (type == 0)
            return;

        if (curtabType == type)
            return;

        if (tabBtnImage != null)
            tabBtnImage.color = Color.white;

        if (init)
            tabBtnImage = RedDot[1].transform.parent.GetComponent<Image>();
        else
            tabBtnImage = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Image>();
        tabBtnImage.color = curColor;

        curtabType = type;
        RefreshUI();
    }

    public void RedDotFlag()
    {
        ableRewardQeust.Clear();

        foreach (var item in RedDot)
        {
            if (item != null)
                item.SetActive(false);
        }

        foreach (var item in Managers.UserData.userQuestDic)
        {
            QuestData qd = QuestData.GetQuestData(item.Key);
            if (qd.clear_count <= item.Value)
            {
                bool enableQuest = true;

                foreach (int prev in QuestData.GetPrevQuests(item.Key))
                {
                    enableQuest = Managers.UserData.IsContainClearQuest(prev);

                    if (!enableQuest)
                        break;
                }

                if (!enableQuest)
                    continue;


                if (qd.quest_type == 5)//캐릭터컬렉션 분리
                    continue;

                if (qd.quest_type >= 6)      //임시 처리
                {
                    if (!LimitedQuestTimeCheck(qd))
                        continue;
                    else
                    {
                        if (RedDot[(int)(qd.quest_type)] != null)
                        {
                            RedDot[(int)(qd.quest_type)].SetActive(true);
                            ableRewardQeust.Add(qd.GetID());
                        }
                    }
                }
                else if (RedDot.Count > qd.quest_type && RedDot[qd.quest_type] != null)
                {
                    RedDot[qd.quest_type].SetActive(true);
                    ableRewardQeust.Add(qd.GetID());
                }
            }
        }
    }

    public bool LimitedQuestTimeCheck(QuestData data)
    {
        var limitedList = Managers.Data.GetData(GameDataManager.DATA_TYPE.limited_quest_info);
        foreach (LimitedQuestInfoData list in limitedList)
        {
            if (list.quest_group_uid == data.quest_type)
            {
                if (SBUtil.KoreanTime <= DateTime.Parse(list.start_day) || SBUtil.KoreanTime > DateTime.Parse(list.end_day))
                    return false;
                else
                    return true;

            }
        }
        return true;
    }

    public void RefreshMenu()
    {
        ClearEventTabList();

        var eventTab = tab.Find("Event").gameObject;
        var datas = Managers.Data.GetData(GameDataManager.DATA_TYPE.limited_quest_info);

        List<LimitedQuestInfoData> event_quests = new List<LimitedQuestInfoData>();
        List<Transform> event_tabs = new List<Transform>();

        int maxTabCount = staticTabs.Length;

        foreach (LimitedQuestInfoData item in datas)
        {
            var start = DateTime.Parse(item.GetValue("start_day"));
            var end = DateTime.Parse(item.GetValue("end_day"));

            if (start > SBUtil.KoreanTime || end < SBUtil.KoreanTime)
                continue;

            bool hasQuest = false;
            foreach (QuestData data in QuestData.GetQuests())
            {
                if ((int)data.group_uid != item.GetID())
                    continue;

                hasQuest = true;
                break;
            }

            if (!hasQuest)
                continue;

            var obj = GameObject.Instantiate(eventTab, tab);
            obj.name = "Event_" + item.GetDesc();
            obj.transform.Find("Text").GetComponent<Text>().text = item.GetDesc();
            //obj.transform.SetAsFirstSibling();

            event_tabs.Add(obj.transform.Find("Dot"));
            event_quests.Add(item);

            maxTabCount = Mathf.Max(maxTabCount, item.quest_group_uid);
        }

        RedDot.Clear();

        for (int i = 0; i < maxTabCount + 1; i++)
        {
            RedDot.Add(null);
        }

        eventTab.SetActive(false);
        int tabIndex = 1;
        foreach (Transform item in tab)
        {
            if (item == eventTab)
                continue;

            bool isStaticTab = false;
            foreach (Transform tabs in staticTabs)
            {
                if (tabs == item)
                {
                    isStaticTab = true;
                    break;
                }
            }
            if (isStaticTab)
                RedDot[tabIndex++] = (item.Find("Dot").gameObject);
        }

        for (int i = 0; i < event_tabs.Count; i++)
        {
            RedDot[event_quests[i].quest_group_uid] = (event_tabs[i].gameObject);
        }

        foreach (var item in RedDot)
        {
            if (item == null)
                continue;
            if (item.transform.parent.GetComponent<Button>().onClick != null)
            {
                item.transform.parent.GetComponent<Button>().onClick.AddListener(() =>
                {
                    TabButton(RedDot.IndexOf(item));
                });
            }
        }
    }
    public void ClearEventTabList()
    {
        foreach (Transform item in tab)
        {
            if (item.name.Contains("Event"))
            {
                if (item.name.Equals("Event"))
                {
                    item.gameObject.SetActive(true);
                    continue;
                }
                DestroyImmediate(item.gameObject);
            }
        }
        RedDot.Clear();
    }

    public void SetEventDayText(DateTime start, DateTime end)
    {
        string remainText;
        var diff = end - SBUtil.KoreanTime;
        if (diff.Days >= 1.0f)
        {
            remainText = StringManager.GetString("ui_day", diff.Days.ToString()) + " " + StringManager.GetString("ui_hour", diff.Hours.ToString());
        }
        else if (diff.Hours >= 1.0f)
        {
            remainText = StringManager.GetString("ui_hour", diff.Hours.ToString()) + " " + StringManager.GetString("ui_min", diff.Minutes.ToString());
        }
        else
        {
            remainText = StringManager.GetString("ui_min", diff.Minutes.ToString());
        }

        if(diff.TotalSeconds < float.MaxValue)
        {
            Invoke("Close", (float)diff.TotalSeconds);
        }

        questTerm.text = StringManager.GetString("ui_left_time", remainText);
    }
}

