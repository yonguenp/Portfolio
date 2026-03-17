using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftRecipeListInfo : MonoBehaviour
{
    [Header("[Origin Layer]")]
    public GameObject originObject;
    public GameObject countLayer;
    public Image craftImage;
    public Text craftCountText;
    public Text craftNameText;
    public Text craftDescText;
    public GameObject IAPHelpButon;
    //public Text craftMaxCookCountText;

    [Header("[Complete Layer]")]
    public GameObject completeObject;
    public GameObject levelupButtonObject;
    public Text levelupButtonText;
    public Image completeCraftImage;
    public Text completeCraftNameText;
    public Text completeCraftDescText;
    public Slider completeSlider;
    public Color originButtonColor;
    public Color dimmedButtonColor;
    public Text completeCraftLevelText;

    [Header("[Dimmed Layer]")]
    public GameObject dimmedObject;
    public Text dimmedLevelText;

    //[Header("[Second Material Craft Button]")]
    //public GameObject craftButtonLayer;
    //public GameObject craftButton;
    //public GameObject craftNoButton;
    //public GameObject quickcraftButton;
    //public GameObject quickcraftNoButton;

    [Header("[Craft Button]")]
    //public GameObject craftButtonLayer;
    public GameObject craftButton;
    public GameObject craftNoButton;
    public GameObject buyableButtonsLayer;

    CraftData curCraftData = null;
    NecoCraftingListPanel parentPanel;

    neco_level necoLevelData;

    public void SetCraftListInfoData(CraftData data, NecoCraftingListPanel craftListPanel)
    {
        curCraftData = data;
        parentPanel = craftListPanel;
        if (curCraftData == null || parentPanel == null) { return; }

        ClearData();

        SetCraftInfo(curCraftData.recipeData.GetRecipeType());

        if (completeObject.activeSelf == false)
        {
            SetCraftButtonState();
            SetCraftDimmedState();
        }

        // 프롤로그 세팅
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.철판제작가이드퀘스트 && curCraftData.recipeData.GetRecipeID() == 22)
        {
            craftButton.gameObject.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기 && curCraftData.recipeData.GetRecipeID() == 29)
        {
            craftButton.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }

        // 프롤로그 세팅
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드 && curCraftData.recipeData.GetRecipeID() == 29)
        {
            levelupButtonObject.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }

        if (IAPHelpButon != null)
        {
            IAPHelpButon.SetActive(curCraftData.recipeData.GetRecipeID() == 47 || curCraftData.recipeData.GetRecipeID() == 49 || curCraftData.recipeData.GetRecipeID() == 52);
        }
    }

    public void OnClickCraftButton()
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.철판제작가이드퀘스트 && curCraftData.recipeData.GetRecipeID() != 22)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ13"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기 && curCraftData.recipeData.GetRecipeID() != 29)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드 && curCraftData.recipeData.GetRecipeID() != 29)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_401"));
            return;
        }
        
        if (curCraftData.recipeData.GetRecipeID() == 47)
        {
            if(user_items.GetUserItemAmount(124) <= 0)
            {
                OnShopOpen();
                return;
            }
        }

        if (curCraftData.recipeData.GetRecipeID() == 49)
        {
            if (neco_data.Instance.GetCraftRecipeLevel() <= 10)
            {
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(string.Format(LocalizeData.GetText("LOCALIZE_210"), 11));
                return;
            }
        }

        if (curCraftData.recipeData.GetRecipeID() == 53)
        {
            if (neco_data.Instance.GetCraftRecipeLevel() <= 11)
            {
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(string.Format(LocalizeData.GetText("LOCALIZE_210"), 12));
                return;
            }
        }

        NecoCanvas.GetPopupCanvas().OnCraftUIPopupShow(curCraftData);
    }

    public void OnShopOpen()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose();
        NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.PACKAGE);
    }

    //public void OnClickQuickCookButton()
    //{
    //    if (curCraftData?.craftMaxCount <= 0)
    //    {
    //        NecoCanvas.GetPopupCanvas().OnToastPopupShow("재료가 부족합니다.");
    //    }
    //    else
    //    {
    //        parentPanel.OnStartQuickCook(curCraftData);
    //    }
    //}

    public void OnClickObjectLevelupButton()
    {
        if (necoLevelData == null) { return; }

        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드 && curCraftData.recipeData.GetRecipeID() != 29)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_401"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnLevelupPopupShow(SUPPLY_UI_TYPE.OBJECT, Refresh, curCraftData.recipeData);
    }

    void SetCraftInfo(string type)
    {
        // 완성된 설치물 데이터 세팅
        if (curCraftData.recipeData.GetRecipeType() == "TOY")
        {
            bool hasItem = curCraftData.recipeData.HasRecipeItem();
            if (hasItem)
            {
                originObject.SetActive(false);
                completeObject.SetActive(true);

                completeCraftImage.sprite = curCraftData.craftIcon;
                completeCraftNameText.text = curCraftData.craftName;
                completeCraftDescText.text = curCraftData.craftDesc;
                

                neco_spot spotObjectData = neco_spot.GetNecoSpotObjectByItemID(curCraftData.itemData.GetItemID());
                completeCraftLevelText.text = "Lv." + spotObjectData.GetSpotLevel();
                float durabilityRate = 0.0f;
                uint objectLevel = 0;
                if (spotObjectData != null)
                {
                    objectLevel = spotObjectData.GetSpotLevel();
                    necoLevelData = neco_level.GetNecoLevelDataByObjectID(spotObjectData.GetSpotID(), objectLevel);
                    neco_object_durability objectData = neco_object_durability.GetObjectDurability(spotObjectData.GetSpotID());
                    uint nowDurability = spotObjectData.GetSpotItemDurability();
                    uint maxDurability = objectData.GetNecoDurabilityByLevel(objectLevel);

                    durabilityRate = (float)nowDurability / (float)maxDurability;
                }

                completeSlider.value = durabilityRate;

                // 레벨업 버튼 max 체크
                levelupButtonObject.GetComponentInChildren<Image>().color = originButtonColor;
                if (levelupButtonText != null)
                {
                    if (objectLevel < neco_data.OBJECT_MAX_LEVEL)
                    {
                        levelupButtonText.text = LocalizeData.GetText("LOCALIZE_45");
                    }
                    else
                    {
                        levelupButtonText.text = LocalizeData.GetText("수리");
                    }
                }
                // 프롤로그 세팅
                if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드 && curCraftData.recipeData.GetRecipeID() == 29)
                {
                    levelupButtonObject.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
                }

                return;
            }
        }

        // 완성된 설치물이 아닐경우 일반 데이터 세팅
        originObject.SetActive(true);
        completeObject.SetActive(false);
        countLayer.SetActive(curCraftData.recipeData.GetRecipeType() != "TOY"); // 설치물의 경우 보유량을 표기하지 않음

        craftImage.sprite = curCraftData.craftIcon;
        craftCountText.text = LocalizeData.GetText("LOCALIZE_46") + curCraftData.craftCount.ToString("n0");
        craftNameText.text = curCraftData.craftName;
        craftDescText.text = curCraftData.craftDesc;
        //craftMaxCookCountText.text = string.Format("{0}", curCraftData.craftMaxCount);
    }

    void SetCraftButtonState()
    {
        if (/*craftButtonLayer == null ||*/ craftButton == null || craftNoButton == null) { return; }
        //if (craftButtonLayer == null || craftButton == null || craftNoButton == null) { return; }
        //if (quickcraftButton == null || quickcraftNoButton == null) { return; }

        //craftButtonLayer.SetActive(curCraftData.recipeData.GetRecipeType() == "T_MATERIAL");
        //craftButtonLayer.SetActive(curCraftData.recipeData.GetRecipeType() == "TOY");

        //if (craftButtonLayer.activeSelf)
        //{
        //    craftButton.SetActive(curCraftData.craftMaxCount > 0);
        //    craftNoButton.SetActive(curCraftData.craftMaxCount <= 0);
        //    quickcraftButton.SetActive(curCraftData.craftMaxCount > 0);
        //    quickcraftNoButton.SetActive(curCraftData.craftMaxCount <= 0);
        //}

        //if (craftButtonLayer.activeSelf)
        //{
        //    craftButton.SetActive(curCraftData.craftMaxCount > 0);
        //    craftNoButton.SetActive(curCraftData.craftMaxCount <= 0);
        //}

        bool enable_craft = curCraftData.recipeData.GetRecipeID() == 47 || curCraftData.craftMaxCount > 0;

        if (curCraftData.recipeData.GetRecipeID() == 49)
        {
            craftButton.SetActive(false);
            craftNoButton.SetActive(false);

            buyableButtonsLayer.SetActive(true);
            craftButton = buyableButtonsLayer.transform.Find("CraftButton").gameObject;
            craftNoButton = buyableButtonsLayer.transform.Find("CraftNo_Button").gameObject;

            GameObject moreLvButton = buyableButtonsLayer.transform.Find("NeedMoreLevel").gameObject;
            if (neco_data.Instance.GetCraftRecipeLevel() <= 10)
            {
                craftButton.SetActive(false);
                craftNoButton.SetActive(false);
                moreLvButton.SetActive(true);
            }
            else
            {
                moreLvButton.SetActive(false);
                craftButton.SetActive(enable_craft);
                craftNoButton.SetActive(!enable_craft);
            }

            return;
        }

        buyableButtonsLayer.SetActive(false);

        craftButton.SetActive(enable_craft);
        craftNoButton.SetActive(!enable_craft);

        if (enable_craft)
        {
            Transform CookButtonTextTransform = craftButton.transform.Find("CookButtonText");
            if (CookButtonTextTransform)
            {
                Text CookButtonText = CookButtonTextTransform.GetComponent<Text>();
                TextLocalize localize = CookButtonTextTransform.GetComponent<TextLocalize>();
                if(localize)
                    Destroy(localize);

                if(curCraftData.recipeData.GetRecipeID() == 47)
                    CookButtonText.text = LocalizeData.GetText("LOCALIZE_163");
                else
                    CookButtonText.text = LocalizeData.GetText("LOCALIZE_47");
            }
        }
    }

    void SetCraftDimmedState()
    {
        if (dimmedObject == null || dimmedLevelText == null) { return; }

        // 보유한 장난감이 아니라면 레벨 딤드 레이어 처리 적용
        uint userLevel = neco_data.Instance.GetCraftRecipeLevel();
        uint recipeLevel = curCraftData.recipeData.GetRecipeLevel();

        bool isDimmed = curCraftData.recipeData.GetRecipeID() != 49 && userLevel < recipeLevel;
        dimmedObject.SetActive(isDimmed);
        dimmedLevelText.text = string.Format(LocalizeData.GetText("LOCALIZE_210"), recipeLevel);
    }

    void Refresh()
    {
        ClearData();

        SetCraftInfo(curCraftData.recipeData.GetRecipeType());

        if (completeObject.activeSelf == false)
        {
            SetCraftButtonState();
            SetCraftDimmedState();
        }

        if (parentPanel != null)
        {
            if (curCraftData.recipeData.GetRecipeType() == "TOY")
            {
                if(neco_spot.GetNecoSpotObjectByItemID(curCraftData.recipeData.GetOutputItem(0).Key).GetSpotLevel() >= neco_data.OBJECT_MAX_LEVEL)                
                    parentPanel.Refresh();
            }
        }
    }

    void ClearData()
    {
        //craftButtonLayer.SetActive(false);
        //craftButtonLayer.SetActive(false);

        craftButton.SetActive(false);
        craftNoButton.SetActive(false);

        //craftButton.SetActive(false);
        //craftNoButton.SetActive(false);
        //quickcraftButton.SetActive(false);
        //quickcraftNoButton.SetActive(false);

        originObject.SetActive(false);
        completeObject.SetActive(false);

        levelupButtonObject.GetComponent<RectTransform>().DORewind();
        levelupButtonObject.GetComponent<RectTransform>().DOKill();
        levelupButtonObject.GetComponent<RectTransform>().localScale = Vector3.one;
    }

    public void OnIAPHelpButton()
    {
        switch(curCraftData.recipeData.GetRecipeID())
        {
            case 47:
                NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.IAP_OBJECT_HELP_POPUP].GetComponent<IAPObjectHelpPopup>().SetUIType(0);
                NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.IAP_OBJECT_HELP_POPUP);
                break;
            case 49:
                NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.IAP_OBJECT_HELP_POPUP].GetComponent<IAPObjectHelpPopup>().SetUIType(1);
                NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.IAP_OBJECT_HELP_POPUP);
                break;
            case 52:
                NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.IAP_OBJECT_HELP_POPUP].GetComponent<IAPObjectHelpPopup>().SetUIType(0);
                NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.IAP_OBJECT_HELP_POPUP);
                break;
        }
    }
}
