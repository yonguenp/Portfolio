using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSettingPopup : MonoBehaviour
{
    [Header("[Skin Info]")]
    //public GameObject NoSkinItemGuideLayer;
    public Transform SkinContainer;
    public GameObject SkinInfoCloneObject;
    
    List<items> skinItemList = new List<items>();

    private void OnEnable()
    {
        ClearUI();
        SetUI();
    }

    private void OnDisable()
    {
        ClearUI();

        foreach(SkinController skinController in NecoCanvas.GetGameCanvas().GetComponentsInChildren<SkinController>())
        {
            skinController.Refresh();
        }
    }

    public void ClearUI()
    {
        // 스킨 리스트 초기화
        foreach (Transform child in SkinContainer)
        {
            if (child.gameObject != SkinInfoCloneObject)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void SetUI()
    {
        // 스킨 리스트 세팅
        SkinInfoCloneObject.SetActive(true);

        skinItemList.Clear();
        List<game_data> skinList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ITEMS);
        foreach (items item in skinList)
        {
            if (item.GetItemType() == "SKIN" || item.GetItemType() == "THEME" || item.GetItemType() == "XMAS_SKIN1" || item.GetItemType() == "XMAS_SKIN2" || item.GetItemType() == "XMAS_SKIN3")
            {
                skinItemList.Add(item);
            }
        }

        foreach (items skin in skinItemList)
        {
            GameObject skinUI = Instantiate(SkinInfoCloneObject);
            skinUI.transform.SetParent(SkinContainer.transform);
            skinUI.transform.localScale = SkinInfoCloneObject.transform.localScale;
            skinUI.transform.localPosition = SkinInfoCloneObject.transform.localPosition;

            skinUI.GetComponent<SkinInfo>().SetSkinInfoData(this, skin);
        }

        SkinInfoCloneObject.SetActive(false);

        Invoke("ResetScrollRectContentHeight", 0.1f);

        //NoSkinItemGuideLayer.SetActive(skinItemList.Count == 0);
    }

    void ResetScrollRectContentHeight()
    {
        GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f;
    }

    public void OnShowSkinSetting()
    {
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.SKIN_SETTING);
    }

    public void OnCloseSkinSetting()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SKIN_SETTING);
    }

    public void SetEquipSkin(uint itemID, bool equip)
    {
        if (equip)
        {
            items target = null;
            foreach (items item in skinItemList)
            {
                if (item.GetItemID() == itemID)
                {
                    target = item;
                }
            }

            string type = target.GetItemType();
            foreach(Transform child in SkinContainer)
            {
                SkinInfo info = child.GetComponent<SkinInfo>();
                if(info != null)
                {
                    items item = info.GetItem();
                    if (item != null && item != target)
                    {
                        if (item.GetItemType() == type)
                        {
                            PlayerPrefs.SetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_" + item.GetItemID(), 0);
                        }
                    }

                    info.RefreshUI();
                }
            }
        }
    }
}
