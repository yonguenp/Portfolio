using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishInfo : MonoBehaviour
{
    public GameObject DimmedObject;
    public GameObject selectStokeObject;

    public Image foodImage;
    public Text foodCountText;
    public Text foodNameText;
    public Text foodDurationText;

    public Button buttonUI;

    items curFishData = null;
    neco_fish_trade trade_data = null;

    public void SetFishInfoData(neco_fish_trade data, NecoFeedListPanel parentPanel)
    {
        trade_data = data;

        if (curFishData == null || parentPanel == null) { return; }

        //foodImage.sprite = curFoodData.foodIcon;
        //foodCountText.text = string.Format("{0}", curFoodData.foodCount.ToString("n0"));
        //foodNameText.text = curFoodData.foodName;
        //foodDurationText.text = curFoodData.foodDuration;

        //DimmedObject?.SetActive(curFoodData.foodCount == 0);
        //selectStokeObject?.SetActive(parentPanel.GetCurrentSelectedFood() == curFoodData);

        //buttonUI.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
            //if (DimmedObject.activeSelf == false)
            {
                //parentPanel.UpdateCurrentSelectedFood(curFoodData);
                //selectStokeObject?.SetActive(parentPanel.GetCurrentSelectedFood() == curFoodData);
            }
        //}));
    }
}
