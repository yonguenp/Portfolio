using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatInfoSub_Feeding : MonoBehaviour
{
    public GameObject FoodContainer;
    public GameObject FoodCloneObject;
    public Button FeedButton;
    public CatInfoUI CatInfoUI;

    private user_items curSelectedItem = null;
    
    private void OnEnable()
    {
        
    }

    public void InitFeedList(cat_def cat)
    {
        SetFoodList();
    }

    public void SetFoodList()
    {
        foreach (Transform foodUI in FoodContainer.transform)
        {
            if (foodUI.gameObject != FoodCloneObject)
                Destroy(foodUI.gameObject);
        }

        FoodCloneObject.SetActive(true);

        List<game_data> user_items = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);
        List<game_data> items = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ITEMS);
        object obj;

        if (user_items != null && items != null)
        {
            foreach (game_data data in user_items)
            {
                user_items userItem = (user_items)data;
                if (userItem.GetAmount() <= 0)
                    continue;

                uint item_id = userItem.GetItemID();

                foreach (game_data it in items)
                {
                    if (it.data.TryGetValue("item_id", out obj))
                    {
                        if (item_id == (uint)obj)
                        {
                            if (it.data.TryGetValue("item_type", out obj))
                            {
                                if ((string)obj == "FOOD")
                                {
                                    GameObject listItem = Instantiate(FoodCloneObject);
                                    listItem.transform.SetParent(FoodContainer.transform);
                                    RectTransform rt = listItem.GetComponent<RectTransform>();
                                    rt.localScale = Vector3.one;
                                    rt.localPosition = Vector3.zero;
                                    FoodItem item = listItem.GetComponent<FoodItem>();

                                    item.SetFoodItem(userItem, (items)it, this);
                                }
                            }
                        }
                    }
                }
            }
        }

        FoodCloneObject.SetActive(false);
        SetCursor(null);
    }

    public void SetCursor(user_items item)
    {
        curSelectedItem = item;
        bool bCursor = false;
        FoodItem[] uiItem = FoodContainer.GetComponentsInChildren<FoodItem>();
        foreach (FoodItem food in uiItem)
        {
            if (food.SetCursor(item))
                bCursor = true;
        }

        if (!bCursor)
        {
            curSelectedItem = null;
        }

        FeedButton.interactable = curSelectedItem != null;

        CatInfoUI.OnFoodSelect(item);
    }

    public void OnFeed()
    {
        if (curSelectedItem == null)
            return;

        CatInfoUI.OnFeed(curSelectedItem);        
    }
}
