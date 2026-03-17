using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeInfo : MonoBehaviour
{
    public Image iconImage;
    public GameObject selectStrokeObject;

    recipe curRecipeData = null;

    NecoCookPanel parentCookPanelUI;
    NecoMakingPanel parentMakingPanelUI;

    public Button buttonUI;

    private void Awake()
    {
        buttonUI = GetComponent<Button>();
    }

    public void SetCookRecipeData(recipe curData, NecoCookPanel parentPanel)
    {
        curRecipeData = curData;
        parentCookPanelUI = parentPanel;

        iconImage.sprite = Resources.Load<Sprite>(curData.GetRecipeIcon());

        buttonUI.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
            //parentCookPanelUI.SetRecipeListUI(curRecipeData);
        }));

        gameObject.SetActive(true);
    }

    public void SetMakingRecipeData(recipe curData, NecoMakingPanel parentPanel)
    {
        curRecipeData = curData;
        parentMakingPanelUI = parentPanel;

        iconImage.sprite = Resources.Load<Sprite>(curData.GetRecipeIcon());

        buttonUI.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
            //parentMakingPanelUI.SetRecipeListUI(curRecipeData);
        }));

        gameObject.SetActive(true);
    }

    public recipe GetCurrentRecipeData()
    {
        return curRecipeData;
    }

    public void ClearRecipeData()
    {
        curRecipeData = null;

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        selectStrokeObject.SetActive(false);
    }
}
