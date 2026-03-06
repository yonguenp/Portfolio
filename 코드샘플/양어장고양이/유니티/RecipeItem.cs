using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeItem : MonoBehaviour
{
    public Image iconUI;
    public Text itemName;
    public Button buttonUI;

    private recipe curData;
    private CookUI parentCookUI;
    public void SetRecipeData(recipe data, CookUI cookUI)
    {
        curData = data;
        parentCookUI = cookUI;

        iconUI.sprite = Resources.Load<Sprite>(curData.GetRecipeIcon());
        itemName.text = curData.GetRecipeName();

        buttonUI.onClick.AddListener(new UnityEngine.Events.UnityAction(() => { parentCookUI.SetRecipeInfoLoad(curData); }));
    }

    public recipe GetData()
    {
        return curData;
    }
}
