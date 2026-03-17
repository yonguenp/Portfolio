using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatInfoStatus : MonoBehaviour
{
    public Text CatName;
    public Text CatLevel;
    public Text HungerStatus;
    public Image CurFullness;
    public Image FeedFullness;

    private cat_def curCatData = null;

    public void Refresh()
    {
        Refresh(curCatData);
    }

    public void Refresh(cat_def cat)
    {
        curCatData = cat;
        FeedFullness.fillAmount = 0.0f;

        user_cats info = curCatData.GetUserCatInfo();
        uint curLevel = info.GetCatLevel();
        CatName.text = curCatData.GetCatName();
        CatLevel.text = "레벨 " + curLevel.ToString();

        cat_level_def levelInfo = curCatData.GetLevelInfo(curLevel);
        float max = System.Convert.ToSingle(levelInfo.GetCatMaxHunger());
        float cur = System.Convert.ToSingle(info.Getfullness());
        
        CurFullness.fillAmount = cur / max;

        HungerStatus.text = System.Convert.ToInt32(cur).ToString() + "/" + System.Convert.ToInt32(max).ToString();
    }

    public void OnFoodSelect(user_items item)
    {
        FeedFullness.fillAmount = 0.0f;
        if (item == null)
            return;

        uint itemID = item.GetItemID();

        food_fullness target = null;
        object obj;
        List<game_data> fullness = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.FOOD_FULLNESS);
        foreach (food_fullness food_fullness in fullness)
        { 
            if(food_fullness.data.TryGetValue("item_id", out obj))
            {
                if(itemID == (uint)obj)
                {
                    target = food_fullness;
                }
            }
        }

        if (target == null)
            return;

        if(target.data.TryGetValue("fullness", out obj))
        {
            uint value = (uint)obj;

            user_cats info = curCatData.GetUserCatInfo();
            uint curLevel = info.GetCatLevel();
            cat_level_def levelInfo = curCatData.GetLevelInfo(curLevel);
            float max = System.Convert.ToSingle(levelInfo.GetCatMaxHunger());
            float feed = System.Convert.ToSingle(value);

            FeedFullness.fillAmount = Mathf.Min(CurFullness.fillAmount + (feed / max), 1.0f);
        }
        else
        {
            return;
        }
    }
}
