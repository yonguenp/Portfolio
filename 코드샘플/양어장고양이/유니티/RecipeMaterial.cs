using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeMaterial : MonoBehaviour
{
    public Image iconUI;    
    public Text itemCount;
    public Button buttonUI;
    public Image cursor;

    private recipe curData;
    private KeyValuePair<uint, uint> input;
    private CookUI parentCookUI;
    private int recipeIndex = 0;

    private uint TryMaxCount = 0;
    public void SetMarerialData(int index, KeyValuePair<uint, uint> _input, recipe data, CookUI cookUI)
    {
        input = _input;
        curData = data;
        parentCookUI = cookUI;
        recipeIndex = index;

        List<game_data> user_items = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);
        List<game_data> items = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ITEMS);
        object obj;

        float maxRatio = 0.0f;
        foreach (game_data item in items)
        {
            if (item.data.TryGetValue("item_id", out obj))
            {
                if (input.Key == (uint)obj)
                {
                    uint need = input.Value;
                    bool hasItem = false;
                    uint cur = 0;
                    foreach (game_data user_item in user_items)
                    {
                        if ((((user_items)user_item).GetItemID() == input.Key))
                        {
                            cur = ((user_items)user_item).GetAmount();                            
                            hasItem = cur >= need;                            
                        }
                    }

                    itemCount.text = cur + "/" + need;

                    if (hasItem == false)
                    {                        
                        itemCount.color = Color.red;
                        maxRatio = 0;
                    }
                    else
                    {
                        itemCount.color = new Color((float)80 / 255, (float)100 / 255, (float)196 / 255);
                        maxRatio = Mathf.Min(99, ((float)cur / need));
                    }

                    if (item.data.TryGetValue("icon_img", out obj))
                    {
                        iconUI.sprite = Resources.Load<Sprite>((string)obj);                        
                    }

                    buttonUI.onClick.AddListener(new UnityEngine.Events.UnityAction(() => { parentCookUI.SetMaterialCursor(input, curData); }));
                }
            }
        }

        TryMaxCount = (uint)maxRatio;
    }

    public void SetCursor(bool on)
    {
        cursor.gameObject.SetActive(on);
    }
    public uint GetTryMaxCount()
    {
        return TryMaxCount;
    }

    public KeyValuePair<uint, uint> GetInputInfo()
    {
        return input;
    }
    public recipe GetData()
    {
        return curData;
    }

    public int GetIndex()
    {
        return recipeIndex;
    }

    public bool isFocusCursor()
    {
        return cursor.gameObject.activeInHierarchy;
    }
}
