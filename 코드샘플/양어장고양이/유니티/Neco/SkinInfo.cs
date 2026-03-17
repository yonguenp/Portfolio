using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinInfo : MonoBehaviour
{
    public Image skinImage;    
    public Text skinName;
    public Text skinDesc;

    public Button buttonUI;
    public Image buttonImage;
    public Text buttonText;

    public GameObject usedLayerObject;
    public GameObject DimmLayer;

    [Header("[Colors]")]
    public Color originButtonColor;
    public Color dimmedButtonColor;

    items curItemData = null;

    public void SetSkinInfoData(SkinSettingPopup root, items data)
    {
        curItemData = data;
        if (curItemData == null) { return; }

        skinImage.sprite = curItemData.GetItemIcon();        
        skinName.text = curItemData.GetItemName();
        skinDesc.text = curItemData.GetItemDesc();

        RefreshUI();

        buttonUI.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                if (user_items.GetUserItemAmount(curItemData.GetItemID()) > 0)
                {
                    bool isEquiped = PlayerPrefs.GetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_" + curItemData.GetItemID(), 0) > 0;
                    PlayerPrefs.SetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_" + curItemData.GetItemID(), isEquiped ? 0 : 1);

                    root.SetEquipSkin(curItemData.GetItemID(), !isEquiped);
                    RefreshUI();
                }
                else
                {
                    NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("스킨교체_아이템없음"));
                }
            }));
    }

    public void RefreshUI()
    {
        bool isEquiped = curItemData != null && PlayerPrefs.GetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_" + curItemData.GetItemID(), 0) > 0;
        bool hasItem = curItemData != null && user_items.GetUserItemAmount(curItemData.GetItemID()) > 0;
        if (isEquiped)
        {
            if (!hasItem)
            {
                PlayerPrefs.SetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_" + curItemData.GetItemID(), 0);
                isEquiped = false;
            }                
        }

        //DimmLayer.SetActive(!hasItem);
        usedLayerObject.SetActive(isEquiped);

        buttonImage.color = hasItem ? originButtonColor : dimmedButtonColor;

        if (hasItem)
        {
            buttonText.text = isEquiped ? LocalizeData.GetText("사용중") : LocalizeData.GetText("사용하기");
        }
        else
        {
            buttonText.text = LocalizeData.GetText("미보유");
        }
    }

    public items GetItem()
    {
        return curItemData;
    }
}
