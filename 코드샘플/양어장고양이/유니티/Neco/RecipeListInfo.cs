using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeListInfo : MonoBehaviour
{
    public Image foodImage;
    public Text foodCountText;
    public Text foodNameText;
    public Text foodDurationText;
    //public Text foodMaxCookCountText;

    [Header("[Dimmed Layer]")]
    public GameObject DimmedObject;
    public Text dimmedLevelText;

    [Header("[Recipe List Button]")]
    public GameObject cookButton;
    public GameObject cookNoButton;
    //public GameObject quickCookButton;
    //public GameObject quickCookNoButton;

    FoodData curFoodData = null;
    NecoCookListPanel parentPanel;

    public void SetFoodListInfoData(FoodData data, NecoCookListPanel cookListPanel)
    {
        curFoodData = data;
        parentPanel = cookListPanel;
        if (curFoodData == null || parentPanel == null) { return; }

        foodImage.sprite = curFoodData.foodIcon;
        foodCountText.text = LocalizeData.GetText("LOCALIZE_46") + curFoodData.foodCount.ToString("n0");
        foodNameText.text = curFoodData.foodName;
        foodDurationText.text = curFoodData.foodDuration;
        //foodMaxCookCountText.text = string.Format("{0}", curFoodData.foodMaxCount);

        SetCookButtonState();
        SetCookLevelDimmedState();

        // 프롤로그 체크
        if (CheckPrologue() && curFoodData.recipeData.GetRecipeID() == 1)
        {
            cookButton.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이강조 && curFoodData.recipeData.GetRecipeID() == 3)
        {
            cookButton.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void OnClickCookButton()
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.조리대레벨업)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ17"));
            return;
        }

        if (CheckPrologue() && curFoodData.recipeData.GetRecipeID() != 1)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_346"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이강조 && curFoodData.recipeData.GetRecipeID() != 3)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnCookUIPopupShow(curFoodData);
    }

    //public void OnClickQuickCookButton()
    //{
    //    if (curFoodData?.foodMaxCount <= 0)
    //    {
    //        NecoCanvas.GetPopupCanvas().OnToastPopupShow("재료가 부족합니다.");
    //    }
    //    else
    //    {
    //        parentPanel.OnStartQuickCook(curFoodData);
    //    }
    //}

    void SetCookButtonState()
    {
        if (cookButton == null || cookNoButton == null) { return; }
        //if (quickCookButton == null || quickCookNoButton == null) { return; }
        
        ClearData();

        cookButton.SetActive(curFoodData.foodMaxCount > 0);
        cookNoButton.SetActive(curFoodData.foodMaxCount <= 0);
        //quickCookButton.SetActive(curFoodData.foodMaxCount > 0);
        //quickCookNoButton.SetActive(curFoodData.foodMaxCount <= 0);
    }

    void SetCookLevelDimmedState()
    {
        if (DimmedObject == null || dimmedLevelText == null) { return; }

        uint userLevel = neco_data.Instance.GetCookRecipeLevel();
        uint recipeLevel = curFoodData.recipeData.GetRecipeLevel();
        DimmedObject.SetActive(userLevel < recipeLevel);
        dimmedLevelText.text = string.Format(LocalizeData.GetText("LOCALIZE_347"), recipeLevel);
    }

    void ClearData()
    {
        cookButton.SetActive(false);
        cookNoButton.SetActive(false);
        //quickCookButton.SetActive(false);
        //quickCookNoButton.SetActive(false);

        cookButton.GetComponent<RectTransform>().localScale = Vector3.one;
        cookButton.GetComponent<RectTransform>().DORewind();
        cookButton.GetComponent<RectTransform>().DOKill();
    }

    bool CheckPrologue()
    {
        return neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.조리대UI등장;
    }
}
