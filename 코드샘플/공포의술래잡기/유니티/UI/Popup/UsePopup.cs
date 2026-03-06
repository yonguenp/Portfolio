using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UsePopup : Popup
{
    [SerializeField]
    Text ItemName;
    [SerializeField]
    UIBundleItem bundle;
    
    [SerializeField]
    Slider slider;
    [SerializeField]
    Text countText;

    ItemGameData itemData = null;
    public delegate void UseCallback(int count);
    protected UseCallback useCallback = null;

    public void Init(ItemGameData s, UseCallback cb)
    {
        itemData = s;
        useCallback = cb;
        ItemName.text = itemData.GetName();

        bundle.SetItem(s, 0);

        int maxCount = Managers.UserData.GetMyItemCount(itemData.GetID());
        slider.minValue = maxCount == 0 ? 0 : 1;
        slider.value = maxCount == 0 ? 0 : 1;
        slider.maxValue = maxCount;

        countText.text = ((int)slider.value).ToString() + "/" + ((int)slider.maxValue).ToString();
    }

    public override void Close()
    {
        base.Close();
    }

    public void OnUse()
    {
        if(Managers.UserData.GetMyItemCount(itemData.GetID()) > 0)
            useCallback?.Invoke((int)slider.value);

        CloseForce();
    }


    public void OnPlus()
    {
        slider.value += 1;

        if (slider.value > slider.maxValue)
            slider.value = slider.maxValue;
        OnSlide();
    }

    public void OnMinus()
    {
        slider.value -= 1;

        if (slider.value < slider.minValue)
            slider.value = slider.minValue;

        OnSlide();
    }


    public void OnSlide()
    {
        if (itemData == null)
            return;

        int count = (int)(slider.value);
        countText.text = count.ToString() + "/" + ((int)slider.maxValue).ToString();
    }
}
