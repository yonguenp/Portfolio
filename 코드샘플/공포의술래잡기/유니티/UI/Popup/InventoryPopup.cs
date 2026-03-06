using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPopup : InventoryPanel, EventListener<NotifyEvent>
{
    [SerializeField] GameObject window;
    [SerializeField] GameObject SampleItem;
    [SerializeField] Transform Container;
    [SerializeField] GameObject EmptyItem;
    [SerializeField] UIPostersPage PosterUI;

    public delegate void SelectedCallback(UIBundleItem item);
    SelectedCallback itemSelectedCallback = null;

    List<ItemGameData.ITEM_TYPE> showTypes = new List<ItemGameData.ITEM_TYPE>();
    Dictionary<ItemGameData, int> itemsInfo = new Dictionary<ItemGameData, int>();
    List<ItemGameData> newItems = new List<ItemGameData>();


    public void OnEvent(NotifyEvent eventType)
    {
        switch (eventType.Message)
        {
            case NotifyEvent.NotifyEventMessage.ON_ITEM_INFO:
            case NotifyEvent.NotifyEventMessage.ON_ITEM_UPDATE:
                {
                    RefreshUI();
                }
                break;
        }
    }

    //private void OnEnable()
    //{
    //    var curScene = Managers.Scene.CurrentScene as LobbyScene;
    //    if (curScene != null)
    //        curScene.OnRedDot(curScene.lobbyBtns[7].transform, false);

    //    this.EventStartListening();
    //}

    //private void OnDisable()
    //{
    //    this.EventStopListening();
    //}

    public void InventoryPanelClose()
    {
        var curScene = Managers.Scene.CurrentScene as LobbyScene;
        if (curScene != null)
            curScene.OnRedDot(curScene.lobbyBtns[7].transform, false);

        this.EventStopListening();
    }

    public void SetCallback(SelectedCallback cb)
    {
        itemSelectedCallback = cb;
    }

    public void SetFilter(Dictionary<ItemGameData, int> items)
    {
        itemsInfo = items;

        showTypes.Clear();
    }

    public void CheckNewItems(Dictionary<ItemGameData, int> prevItems)
    {
        if (prevItems == null)
            return;

        foreach (var item in Managers.UserData.GetAllMyItemData())
        {
            if (item.Key == null)
                continue;

            if (item.Key.type == ItemGameData.ITEM_TYPE.EMOTICON)
                continue;

            if (item.Key.type == ItemGameData.ITEM_TYPE.BUFF_ITEM)
                continue;

            if (newItems.Contains(item.Key))
                continue;

            if (!prevItems.ContainsKey(item.Key))
            {
                newItems.Add(item.Key);
            }
            else if (prevItems[item.Key] < item.Value)
            {
                newItems.Add(item.Key);
            }
        }
    }

    void Clear()
    {
        foreach (Transform child in Container)
        {
            if (child == SampleItem.transform)
                continue;

            Destroy(child.gameObject);
        }

        if (PosterUI != null)
        {
            PosterUI.gameObject.SetActive(false);
            PosterUI.Init();
        }

        window.SetActive(true);
    }

    public override void RefreshUI()
    {
        var curScene = Managers.Scene.CurrentScene as LobbyScene;
        if (curScene != null)
            curScene.OnRedDot(curScene.lobbyBtns[7].transform, false);

        this.EventStartListening();

        Clear();

        bool itemEmpty = true;
        SampleItem.SetActive(true);

        List<ItemGameData> keys = new List<ItemGameData>();
        foreach (var itemInfo in itemsInfo)
        {
            keys.Add(itemInfo.Key);
        }

        keys.Sort((x, y) =>
        {
            if (x.type == y.type)
            {
                switch (x.type)
                {
                    case ItemGameData.ITEM_TYPE.CHAR_PIECE:
                        return y.grade.CompareTo(x.grade);
                    case ItemGameData.ITEM_TYPE.ENCHANT_ITEM:
                        return y.grade.CompareTo(x.grade);
                }
            }
            else
            {
                if (x.type == ItemGameData.ITEM_TYPE.RANDOM_BOX)
                    return -1;
                else if (y.type == ItemGameData.ITEM_TYPE.RANDOM_BOX)
                    return 1;

                if (x.type == ItemGameData.ITEM_TYPE.CHAR_PIECE)
                    return 1;
                else if (y.type == ItemGameData.ITEM_TYPE.CHAR_PIECE)
                    return -1;

                if (x.type == ItemGameData.ITEM_TYPE.ENCHANT_ITEM)
                    return 1;
                else if (y.type == ItemGameData.ITEM_TYPE.ENCHANT_ITEM)
                    return -1;

                return y.type.CompareTo(x.type);
            }

            return 0;
        });

        foreach (var key in keys)
        {
            int amount = itemsInfo[key];
            if (amount <= 0)
                continue;

            var itemData = key;
            if (itemData == null)
                continue;

            switch (key.type)
            {
                case ItemGameData.ITEM_TYPE.EVENT_ITEM:
                    ShopMenuGameData shopmenu = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu, key.value) as ShopMenuGameData;
                    if (shopmenu != null)
                    {
                        if (shopmenu.end_event > SBCommonLib.SBUtil.KoreanTime)
                            break;
                    }
                    continue;
            }

            var obj = GameObject.Instantiate(SampleItem, Container);
            var bundleItem = obj.GetComponent<UIBundleItem>();
            bundleItem.SetItem(itemData, amount);
            bundleItem.SetChecker(newItems.Contains(itemData));
            bundleItem.SetTouchCallback(() => {
                if (itemSelectedCallback != null)
                {
                    itemSelectedCallback.Invoke(bundleItem);
                    return;
                }

                switch (key.type)
                {
                    case ItemGameData.ITEM_TYPE.CHAR_TICKET:
                        foreach (CharacterGameData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character))
                        {
                            if (!data.use)
                                continue;

                            if (data.is_limited > 0)
                                continue;

                            if (!Managers.UserData.MyCharacters.ContainsKey(data.GetID()))
                            {
                                //뽑기에서 얻을수 있는 캐릭터 모두 모았을 때만 편의기능 사용가능
                                PopupCanvas.Instance.ShowGachaPopup(3);
                                return;
                            }
                        }
                        RunOpen(key);
                        break;
                    case ItemGameData.ITEM_TYPE.RANDOM_BOX:
                        RunOpen(key);
                        break;
                    case ItemGameData.ITEM_TYPE.NICK_CHANGE:
                        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.NICKCHANGE_POPUP);
                        break;
                    case ItemGameData.ITEM_TYPE.SELECTABLE_ITEM:
                        PopupCanvas.Instance.ShowCharacterSelectPopup(itemData, this);
                        break;
                }
            });

            itemEmpty = false;
        }

        SampleItem.SetActive(false);

        EmptyItem.SetActive(itemEmpty);

        newItems.Clear();
    }

    void RunOpen(ItemGameData key)
    {
        var itemData = key;
        if (key.grade == 0)
        {
            PopupCanvas.Instance.ShowItemUsePopup(itemData, (cnt) =>
            {
                SBWeb.UseRandomBox(itemData.GetID(), cnt, (response) =>
                {
                    SetFilter(CombineInventoryPopup.GetItemList(InventoryTabBtn.TabType.Item));
                    RefreshUI();

                    bool rewarded = ((JObject)response).ContainsKey("rewards");
                    if (!rewarded)
                    {
                        PopupCanvas.Instance.ShowFadeText("보상없음");
                        return;
                    }

                    JArray rew = (JArray)response["rewards"];
                    if (rew != null && rew.Count != 0)
                    {
                        JArray arr = (JArray)rew;
                        List<SBWeb.ResponseReward> rewards = new List<SBWeb.ResponseReward>();
                        foreach (JToken reward_array in arr)
                        {
                            JArray rewardData = (JArray)reward_array;
                            rewards.Add(new SBWeb.ResponseReward(int.Parse(rewardData[0].ToString()), int.Parse(rewardData[1].ToString()), int.Parse(rewardData[2].ToString())));
                        }

                        if (key.grade > 0)
                        {
                            StartCoroutine(ShowGachaReward(rewards));
                        }
                        else
                        {
                            PopupCanvas.Instance.ShowRewardResult(rewards);
                        }
                    }
                });
            });
        }
        else
        {
            PopupCanvas.Instance.ShowConfirmPopup("ui_item_use", () =>
            {
                SBWeb.UseRandomBox(itemData.GetID(), 1, (response) =>
                {
                    SetFilter(CombineInventoryPopup.GetItemList(InventoryTabBtn.TabType.Item));
                    RefreshUI();

                    bool rewarded = ((JObject)response).ContainsKey("rewards");
                    if (!rewarded)
                    {
                        PopupCanvas.Instance.ShowFadeText("보상없음");
                        return;
                    }

                    JArray rew = (JArray)response["rewards"];
                    if (rew != null && rew.Count != 0)
                    {
                        JArray arr = (JArray)rew;
                        List<SBWeb.ResponseReward> rewards = new List<SBWeb.ResponseReward>();
                        foreach (JToken reward_array in arr)
                        {
                            JArray rewardData = (JArray)reward_array;

                            rewards.Add(new SBWeb.ResponseReward(int.Parse(rewardData[0].ToString()), int.Parse(rewardData[1].ToString()), int.Parse(rewardData[2].ToString())));
                        }

                        if (key.grade > 0)
                        {
                            StartCoroutine(ShowGachaReward(rewards));
                        }
                        else
                        {
                            PopupCanvas.Instance.ShowRewardResult(rewards);
                        }
                    }
                });
            });
        }
    }

    public void OnChoiceCharacter(int item_no, string result)
    {
        PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.CHARACTERSELECT_POPUP);

        SBWeb.UseChoiceBox(item_no, result, (response) =>
        {
            SetFilter(CombineInventoryPopup.GetItemList(InventoryTabBtn.TabType.Item));
            RefreshUI();

            bool rewarded = ((JObject)response).ContainsKey("rewards");
            if (!rewarded)
            {
                PopupCanvas.Instance.ShowFadeText("보상없음");
                return;
            }

            JArray rew = (JArray)response["rewards"];
            if (rew != null && rew.Count != 0)
            {
                JArray arr = (JArray)rew;
                List<SBWeb.ResponseReward> rewards = new List<SBWeb.ResponseReward>();
                foreach (JToken reward_array in arr)
                {
                    JArray rewardData = (JArray)reward_array;
                    rewards.Add(new SBWeb.ResponseReward(int.Parse(rewardData[0].ToString()), int.Parse(rewardData[1].ToString()), int.Parse(rewardData[2].ToString())));
                }

                StartCoroutine(ShowGachaReward(rewards));
            }
        });
    }

    IEnumerator ShowGachaReward(List<SBWeb.ResponseReward> rewards)
    {
        if (PosterUI != null)
        {
            PosterUI.gameObject.SetActive(true);
            if (UnityEngine.Random.value > 0.5f)
                PosterUI.play01();
            else
                PosterUI.play02();

            yield return new WaitForSeconds(0.9f);
        }

        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP);
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP) as GachaResultPopup;
        Popup invenPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.COMBINEINVENTORY_POPUP);
        popup.transform.SetSiblingIndex(invenPopup.transform.GetSiblingIndex() - 1);
        popup.Init(rewards, null, null);

        window.SetActive(false);

        if (PosterUI != null)
        {
            yield return new WaitForSeconds(0.5f);
            PosterUI.gameObject.SetActive(false);
        }

        popup.transform.SetSiblingIndex(invenPopup.transform.GetSiblingIndex() + 1);

        window.SetActive(true);
    }

    
}
