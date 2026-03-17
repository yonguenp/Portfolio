using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionItem : MonoBehaviour
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

    [SerializeField] GameObject clearCheckImage;
    [SerializeField] Image reward;
    [SerializeField] Button clearBTN;

    [SerializeField] UIBundleItem characterItem;

    [SerializeField] Text Title;
    [SerializeField] Text Buff_name;
    [SerializeField] Text Buff_amount;

    [SerializeField] GameObject clear_condition;

    CollectionPopup parent = null;
    ClearType clearType = ClearType.NONE;


    bool isReward = false;
    public void Init(QuestData d, CollectionPopup p, int index)
    {
        data = d;
        parent = p;
        
        isReward = Managers.UserData.IsContainClearQuest(data.GetID());

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
        ShopPackageGameData rwd = data.rewards[0];
        reward.sprite = rwd.GetIcon();

        questType = data.quest_type;

        Title.text = data.GetName();//d.GetValue("achievement_point") + StringManager.GetString((d.GetValue("content")));

        string buff_name = "";
        switch (rwd.goods_type)
        {
            case 1:
                buff_name = StringManager.GetString("gold_name");
                break;
            case 2:
                buff_name = StringManager.GetString("dia_name");
                break;
            case 3:
                buff_name = rwd.targetItem.GetName();
                break;
            case 4:
                buff_name = rwd.targetCharacter.GetName();
                break;
            default:
                break;
        }

        string buff_amount = rwd.goods_amount.ToString();

        if (rwd.targetItem != null)
        {
            CollectionBuff buff = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.collection_buff, rwd.targetItem.GetID()) as CollectionBuff;
            if(buff != null)
            {
                buff_name = buff.GetName();
                buff_amount = buff.GetValueString(rwd.goods_amount);                
            }            
        }        

        Buff_name.text = buff_name;
        Buff_amount.text = "+" + buff_amount;

        clearCheckImage.SetActive(isReward);
        if (isReward)
            clearBTN.interactable = !isReward;

        ClearItem();

        clear_condition.gameObject.SetActive(true);
        foreach (CollectionCharGroupData item in CollectionCharGroupData.GetGroupData(data.param))
        {
            UIBundleItem obj = GameObject.Instantiate(characterItem, characterItem.transform.parent);
            UserCharacterData ud = Managers.UserData.GetMyCharacterInfo(item.char_uid);

            obj.SetCharacterInfo(item.char_uid, ud != null && item.level_value <= ud.lv && item.skill_value <= ud.skillLv);

            GameObject name = Instantiate(clear_condition, clear_condition.transform.parent);
            Text ds = name.transform.Find("Text").GetComponent<Text>();

            bool clear = false;//임시            
            if(item.level_value == 1 && item.skill_value == 1)
            {
                ds.text = StringManager.GetString("collection_goal_gain", CharacterGameData.GetCharacterData(item.char_uid).GetName());
                clear = ud != null;
            }
            else if (item.skill_value == 1)
            {
                ds.text = StringManager.GetString("collection_goal_level", CharacterGameData.GetCharacterData(item.char_uid).GetName(), item.level_value);
                clear = ud != null && (Managers.UserData.GetMyCharacterInfo(item.char_uid).lv >= item.level_value);
            }
            else
            {
                ds.text = StringManager.GetString("collection_goal_skill", CharacterGameData.GetCharacterData(item.char_uid).GetName(), item.skill_value);
                clear = ud != null && (Managers.UserData.GetMyCharacterInfo(item.char_uid).skillLv >= item.skill_value);
            }

            if (clear)
            {
                Color color = new Color(0.9686275f, 0.9538653f, 0.4352942f);
                ds.color = color;                
            }
            else
            {
                ds.color = Color.white;
            }
            name.transform.Find("check").Find("complete").gameObject.SetActive(clear);

        }
        characterItem.gameObject.SetActive(false);
        clear_condition.gameObject.SetActive(false);

        RefreshItem();
    }

    public void RefreshItem()
    {
        if (data == null || (Managers.UserData.userQuestDic.Count == 0 && Managers.UserData.userRewardedQuest.Count == 0))
            return;

        if (parent != null || data != null)
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

            isReward = Managers.UserData.IsContainClearQuest(data.GetID());

            if (!isReward && value >= data.clear_count)
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
        foreach (Transform item in characterItem.transform.parent)
        {
            if (item == characterItem.transform)
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
