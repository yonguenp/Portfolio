using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoodItem : MonoBehaviour
{
    public Image icon;
    public Text foodName;
    public Text effectDesc;
    public Text count;
    public GameObject cursor;
    public Button button;

    private user_items user_items;
    private items items;
    private CatInfoSub_Feeding parentFoodList;
    public void SetFoodItem(user_items _user_items, items _items, CatInfoSub_Feeding parent)
    {
        user_items = _user_items;
        items = _items;
        parentFoodList = parent;
        icon.sprite = items.GetItemIcon();
        foodName.text = items.GetItemName();
        
        count.text = user_items.GetAmount().ToString();

        List<game_data> fullness = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.FOOD_FULLNESS);
        object obj;
        if (items.data.TryGetValue("item_id", out obj))
        {
            uint id = (uint)obj;
            foreach (game_data f in fullness)
            {
                if (f.data.TryGetValue("item_id", out obj))
                {
                    if (id == (uint)obj)
                    {
                        if (f.data.TryGetValue("fullness", out obj))
                            effectDesc.text = LocalizeData.GetText("fullness") + "+" + ((uint)obj).ToString();
                    }
                }
            }

            if (id == 14)
            {
                if (ContentLocker.GetCurContentSeq() >= 16 && ContentLocker.GetCurContentSeq() <= 20)
                {
                    ContentLocker locker = gameObject.AddComponent<ContentLocker>();
                    locker.ContentID = 20;
                    locker.guidePrefab = Resources.Load<GameObject>("Prefabs/ContentGuide/ContentGuide_Canvas");
                    locker.targetUIRectTransform = GetComponent<RectTransform>();
                    locker.targetSprite = GetComponent<Image>().sprite;
                    //string[] scirpt = { LocalizeData.GetText("CS20-00") };
                    //locker.GuideText = scirpt;

                    locker.RefreshUnlockStatus();
                }
            }
        }

        button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => { parentFoodList.SetCursor(user_items); }));
    }

    public bool SetCursor(user_items item)
    {
        bool isCursor = item == user_items;
        cursor.SetActive(isCursor);
        return isCursor;
    }
}
