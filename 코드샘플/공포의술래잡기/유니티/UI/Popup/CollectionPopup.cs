using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionPopup : Popup
{
    [SerializeField] CollectionItem collectionItem;

    [SerializeField] ScrollRect scrollRect;

    [SerializeField] Slider progressSlider;
    [SerializeField] Text progressText;
    [SerializeField] GameObject curBuffItem;

    public List<CollectionItem> questItemList = new List<CollectionItem>();

    public override void Open(CloseCallback cb = null)
    {
        Clear();
        SBWeb.GetMyQuestDB((res) =>
        {
            Managers.UserData.SetMyQuestDBData(res);
            RefreshUI();
        });

        base.Open(cb);
    }

    public override void Close()
    {
        Clear();
        base.Close();
    }

    public void RefreshItemUI()
    {
        scrollRect.verticalNormalizedPosition = 1.0f;

        foreach (var item in questItemList)
        {
            item.QueestSort();
        }
    }

    void Clear()
    {
        foreach (Transform item in collectionItem.transform.parent)
        {
            if (item == collectionItem.transform)
                continue;

            Destroy(item.gameObject);
        }
    }

    public override void RefreshUI()
    {
        Clear();
        questItemList.Clear();

        int totalCollectionQuestCount = 0;
        List<QuestData> rewardedQuests = new List<QuestData>();
        foreach (QuestData data in QuestData.GetQuests())
        {
            if ((int)data.quest_type != 5)
                continue;

            totalCollectionQuestCount++;

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

            var obj = GameObject.Instantiate(collectionItem, collectionItem.transform.parent);
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

            var obj = GameObject.Instantiate(collectionItem, collectionItem.transform.parent);
            obj.transform.localScale = Vector3.one;
            obj.Init(data, this, questItemList.Count);

            obj.gameObject.SetActive(true);
            questItemList.Add(obj);
        }

        collectionItem.gameObject.SetActive(false);
        progressSlider.value = (float)rewardedQuests.Count / (totalCollectionQuestCount);
        progressText.text = $"{rewardedQuests.Count}/{(totalCollectionQuestCount)}";
        RefreshMyBuff();
        RefreshItemUI();
    }

    public void RefreshMyBuff()
    {
        foreach(Transform child in curBuffItem.transform.parent)
        {
            if (child == curBuffItem.transform)
                continue;

            Destroy(child.gameObject);
        }

        curBuffItem.SetActive(true);

        var mybuff_hp = GameObject.Instantiate(curBuffItem, curBuffItem.transform.parent);
        mybuff_hp.transform.localScale = Vector3.one;
        mybuff_hp.transform.Find("name").GetComponent<Text>().text = StringManager.GetString("buff_hp");
        mybuff_hp.transform.Find("amount").GetComponent<Text>().text = "+" + (Managers.UserData.buff_hp * 0.001f).ToString("N0");

        var mybuff_atk = GameObject.Instantiate(curBuffItem, curBuffItem.transform.parent);
        mybuff_atk.transform.localScale = Vector3.one;
        mybuff_atk.transform.Find("name").GetComponent<Text>().text = StringManager.GetString("buff_atk");
        mybuff_atk.transform.Find("amount").GetComponent<Text>().text = "+" + (Managers.UserData.buff_atk * 0.001f).ToString("N0");

        var mybuff_gold = GameObject.Instantiate(curBuffItem, curBuffItem.transform.parent);
        mybuff_gold.transform.localScale = Vector3.one;
        mybuff_gold.transform.Find("name").GetComponent<Text>().text = StringManager.GetString("buff_gold");
        mybuff_gold.transform.Find("amount").GetComponent<Text>().text = "+" + (Managers.UserData.buff_gold * 0.1f).ToString() + "%";

        var mybuff_item = GameObject.Instantiate(curBuffItem, curBuffItem.transform.parent);
        mybuff_item.transform.localScale = Vector3.one;
        mybuff_item.transform.Find("name").GetComponent<Text>().text = StringManager.GetString("buff_item");
        mybuff_item.transform.Find("amount").GetComponent<Text>().text = "+" + (Managers.UserData.buff_item * 0.1f).ToString() + "%";
        
        curBuffItem.SetActive(false);
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
            });
        });
    }
}
