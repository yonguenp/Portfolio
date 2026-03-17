using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestItem : MonoBehaviour
{
    enum ClearType
    {
        NONE,
        PROCEEDING,
        COMPLETE,
        CLEAR,
    }

    [SerializeField] QuestData data = null;
    [SerializeField] int questType = 0;

    [SerializeField] Text valueText = null;
    [SerializeField] Slider valueImage = null;
    [SerializeField] GameObject clearCheckImage;
    [SerializeField] UIBundleItem reward;
    [SerializeField] Button clearBTN;
    [SerializeField] GameObject typeA;
    [SerializeField] GameObject typeB;

    [SerializeField] UIBundleItem questitem;

    QuestPopup parent = null;
    ClearType clearType = ClearType.NONE;

    int clear_count;
    bool isReward = false;
    public void Init(QuestData d, QuestPopup p, int index)
    {
        data = d;
        parent = p;

        typeA.SetActive(false);
        typeB.SetActive(false);

        int value = 0;
        if (!Managers.UserData.userQuestDic.ContainsKey(data.GetID()))
        {
            if (Managers.UserData.IsContainClearQuest(data.GetID()))
            {
                value = data.clear_count;
            }
            else
            {
                SBDebug.LogError("이 퀘스트 키는 존재하지 않음");
            }
        }
        else
            value = Managers.UserData.userQuestDic[data.GetID()];

        if (!isReward && value >= data.clear_count)
        {
            clearType = ClearType.COMPLETE;
        }
        else if (isReward)
        {
            clearType = ClearType.CLEAR;
        }
        else
        {
            clearType = ClearType.PROCEEDING;
        }

        if (index < 10 || clearType == ClearType.COMPLETE)
        {
            Init();
        }
        else
        {
            Invoke("Init", 0.05f * (index - 10));
        }
    }

    void Init()
    {
        CancelInvoke("Init");
        reward.SetRewards(data.rewards);

        questType = data.quest_type;

        if (data.quest_clear_type != 28)//28 == 콜랙션퀘스트
            typeA.SetActive(true);
        else
            typeB.SetActive(true);

        Transform mainContent = typeA.activeSelf == true ? typeA.transform : typeB.transform;


        mainContent.Find("QuestTitle").GetComponent<Text>().text = data.GetName();//d.GetValue("achievement_point") + StringManager.GetString((d.GetValue("content")));
        mainContent.Find("Des").GetComponent<Text>().text = data.GetDesc();

        valueImage.maxValue = 1;

        clear_count = data.clear_count;
        valueText.text = $"0 / {clear_count}";

        clearCheckImage.SetActive(isReward);
        if (isReward)
            clearBTN.interactable = !isReward;

        ClearItem();

        if (typeB.activeSelf)
        {
            typeB.transform.Find("SubDes").GetComponent<Text>().text = string.Empty;

            List<string> nameList = new List<string>();
            nameList.Clear();

            foreach (CollectionCharGroupData item in CollectionCharGroupData.GetGroupData(data.param))
            {
                UIBundleItem obj = GameObject.Instantiate(questitem, questitem.transform.parent);
                obj.SetCharacterInfo(item.char_uid, Managers.UserData.GetMyCharacterInfo(item.char_uid) != null);

                nameList.Add(CharacterGameData.GetCharacterData(item.char_uid).GetName());
            }
            questitem.gameObject.SetActive(false);

            if (typeB.activeSelf)
            {
                for (int i = 0; i < nameList.Count; i++)
                {
                    typeB.transform.Find("SubDes").GetComponent<Text>().text += nameList[i];
                    if (i != nameList.Count - 1)
                        typeB.transform.Find("SubDes").GetComponent<Text>().text += " / ";
                }
            }
        }
        else if (questType == 4)
        {
            questitem.SetCharacterInfo(data.param);
        }
        else if (questType >= 6)
        {
            var limitData = Managers.Data.GetData(GameDataManager.DATA_TYPE.limited_quest_info);
            foreach (LimitedQuestInfoData item in limitData)
            {
                if (item.quest_group_uid == data.quest_type)
                {
                    parent.SetEventDayText(DateTime.Parse(item.start_day), DateTime.Parse(item.end_day));
                }
            }

            switch(data.quest_clear_type)
            {
                case 24:
                case 25:
                case 26:
                case 27:
                    questitem.SetCharacterInfo(data.param);
                    break;
                default:
                    questitem.SetQuestIcon(data);
                    break;
            }
        }
        else
        {
            questitem.SetQuestIcon(data);
        }
        RefreshItem();
    }

    public void RefreshItem()
    {
        if (data == null || (Managers.UserData.userQuestDic.Count == 0 && Managers.UserData.userRewardedQuest.Count == 0))
            return;

        int type = data.quest_clear_type;
        int value = 0;
        if (!Managers.UserData.userQuestDic.ContainsKey(data.GetID()))
        {
            if (Managers.UserData.IsContainClearQuest(data.GetID()))
            {
                value = clear_count;
            }
            else
            {
                SBDebug.LogError("이 퀘스트 키는 존재하지 않음");
                return;
            }
        }
        else
            value = Managers.UserData.userQuestDic[data.GetID()];

        valueImage.value = (float)value / (float)clear_count;
        valueText.text = $"{Mathf.Min(value, clear_count)} / {clear_count}";

        if (parent != null || data != null)
        {
            isReward = Managers.UserData.IsContainClearQuest(data.GetID());

            if (!isReward && value >= clear_count)
            {
                clearBTN.interactable = true;
                clearBTN.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/UI/quest/btn_sub_violet_02");
                clearType = ClearType.COMPLETE;                
            }
            else if (isReward)
            {
                clearCheckImage.SetActive(true);
                clearType = ClearType.CLEAR;
            }
            else
            {
                clearBTN.interactable = false;
                clearBTN.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/UI/quest/btn_sub_gray_01");
                clearType = ClearType.PROCEEDING;
            }
        }
    }
    public void GetQuestRewardButton()
    {
        List<int> i_list = new List<int>();
        i_list.Add(data.GetID());

        parent.OnTryReward(i_list);
    }

    public void ClearItem()
    {
        foreach (Transform item in questitem.transform.parent)
        {
            if (item == questitem.transform)
                continue;
            Destroy(item);
        }
    }

    public void QueestSort()
    {
        if (clearType == ClearType.NONE)
        {
            int value = 0;
            if (!Managers.UserData.userQuestDic.ContainsKey(data.GetID()))
            {
                if (Managers.UserData.IsContainClearQuest(data.GetID()))
                {
                    value = data.clear_count;
                }
                else
                {
                    SBDebug.LogError("이 퀘스트 키는 존재하지 않음");
                    return;
                }
            }
            else
                value = Managers.UserData.userQuestDic[data.GetID()];

            if (!isReward && value >= data.clear_count)
            {
                clearType = ClearType.COMPLETE;
            }
            else if (isReward)
            {
                clearType = ClearType.CLEAR;
            }
            else
            {
                clearType = ClearType.PROCEEDING;
            }
        }

        switch (clearType)
        {
            case ClearType.PROCEEDING:
                break;
            case ClearType.COMPLETE:
                transform.SetAsFirstSibling();
                break;
            case ClearType.CLEAR:
                transform.SetAsLastSibling();
                break;
            default:
                break;
        }
    }

}
