using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelupMatData
{
    public items itemData;

    public Sprite matIcon;
    public int matCount;
    public string matName;
}

public class LevelupTextData
{
    public enum LEVELUP_TEXT_STYLE
    {
        ARROW_STYLE,
        COMMON_STYLE,
        CUSTOM_STYLE
    }

    public LEVELUP_TEXT_STYLE textStyle;

    // common style
    public string commonText;

    // arrow style
    public string levelInfoText;
    public string beforeLevelText;
    public string afterLevelText;

    public LevelupTextData(LEVELUP_TEXT_STYLE style, string common)
    {
        textStyle = style;
        commonText = common;
    }

    public LevelupTextData(LEVELUP_TEXT_STYLE style, string levelinfo, string beforelevel, string afterlevel)
    {
        textStyle = style;
        levelInfoText = levelinfo;
        beforeLevelText = beforelevel;
        afterLevelText = afterlevel;
    }
}

public class NecoLevelupUIPanel : MonoBehaviour
{
    [Header("[Object Info Layer]")]
    public GameObject objectInfoLayer;
    public Image objectInfoImage;
    public Text objectInfoNameText;
    public Text objectInfoDescText;
    public Slider objectInfoSlider;
    public Button repairButton;

    [Header("[Levelup Material List]")]
    public GameObject materialListScrollContainer;
    public GameObject materialCloneObject;
    public int centerSortCount = 3;

    [Header("[Levelup Info Layer]")]
    public Text levelupGuideText;
    public GameObject levelupInfoContainer;
    public GameObject levelupInfoCloneObject;

    [Header("[Button Layers]")]
    public GameObject levelupButton;
    public Text levelupButtonText;
    public Color originLevelupButtonColor;
    public Color DimmedLevelupButtonColor;

    [Header("[Layout List]")]
    public RectTransform layoutRect;

    [Header("[Success Popup]")]
    public GameObject successPopupObject;

    SUPPLY_UI_TYPE curLevelupUIType = SUPPLY_UI_TYPE.UNKNOWN;

    neco_level curLevelData;
    neco_level nextLevelData;
    recipe curRecipeData = null;

    List<RecipeMaterialInfo> curNeedMaterials = new List<RecipeMaterialInfo>();
    List<LevelupTextData> levelupTextDataList = new List<LevelupTextData>();

    uint userMoney;
    uint needMoney;
    uint userCatnip;
    uint needCatnip;

    NecoLevelupSuccessPanel.Callback callback = null;

    bool isAvailLevelup = true;
    bool isFixed = false;

    public void OnClickCloseButton()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.LEVELUP_POPUP);
    }

    public void OnClickLevelupButton()
    {
        if (isAvailLevelup)
        {
            OnTryLevelUp();
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_283"));
        }
    }

    public void InitLevelupData(SUPPLY_UI_TYPE levelupUIType, NecoLevelupSuccessPanel.Callback _closeCallback = null, recipe recipeData = null)
    {
        curLevelupUIType = levelupUIType;
        callback = _closeCallback;
        curRecipeData = recipeData;

        levelupGuideText.text = LocalizeData.GetText("LOCALIZE_90");
        levelupButtonText.text = LocalizeData.GetText("LOCALIZE_45");

        SetUserData();
        Refresh();
    }

    public void Refresh()
    {
        ClearData();

        SetUserData();

        // UI TYPE에 맞게 데이터 세팅
        switch (curLevelupUIType)
        {
            case SUPPLY_UI_TYPE.CAT_GIFT:
                curLevelData = neco_level.GetNecoLevelData("GIFT", neco_data.Instance.GetGiftBasketLevel());
                if (neco_data.Instance.GetGiftBasketLevel() < neco_data.GIFT_MAX_LEVEL)
                {
                    nextLevelData = neco_level.GetNecoLevelData("GIFT", neco_data.Instance.GetGiftBasketLevel() + 1);
                }
                SetCatGiftLevelupUIData();
                break;
            case SUPPLY_UI_TYPE.FISH_FARM:
                curLevelData = neco_level.GetNecoLevelData("GOLD", neco_data.Instance.GetFishfarmLevel());
                if (neco_data.Instance.GetFishfarmLevel() < neco_data.FISH_FARM_MAX_LEVEL)
                {
                    nextLevelData = neco_level.GetNecoLevelData("GOLD", neco_data.Instance.GetFishfarmLevel() + 1);
                }
                SetFishfarmLevelupUIData();
                break;
            case SUPPLY_UI_TYPE.FISH_TRAP:
                curLevelData = neco_level.GetNecoLevelData("FISH", neco_data.Instance.GetFishtrapLevel());
                if (neco_data.Instance.GetFishtrapLevel() < neco_data.FISH_TRAP_MAX_LEVEL)
                {
                    nextLevelData = neco_level.GetNecoLevelData("FISH", neco_data.Instance.GetFishtrapLevel() + 1);
                }
                SetFishtrapLevelupUIData();
                break;
            case SUPPLY_UI_TYPE.WORKBENCH:
                curLevelData = neco_level.GetNecoLevelData("TOY", neco_data.Instance.GetCraftRecipeLevel());
                if (neco_data.Instance.GetCraftRecipeLevel() < neco_data.CRAFT_MAX_LEVEL)
                {
                    nextLevelData = neco_level.GetNecoLevelData("TOY", neco_data.Instance.GetCraftRecipeLevel() + 1);
                }
                SetCraftLevelupUIData();
                break;
            case SUPPLY_UI_TYPE.COUNTERTOP:
                curLevelData = neco_level.GetNecoLevelData("FOOD", neco_data.Instance.GetCookRecipeLevel());
                if (neco_data.Instance.GetCookRecipeLevel() < neco_data.COOK_MAX_LEVEL)
                {
                    nextLevelData = neco_level.GetNecoLevelData("FOOD", neco_data.Instance.GetCookRecipeLevel() + 1);
                }
                SetCookLevelupUIData();
                break;
            case SUPPLY_UI_TYPE.OBJECT:
                SetObjectLevelupUIData();
                break;
        }

        NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);

        UpdateButtonColor();

        StartCoroutine(RefreshLayout());
    }

    IEnumerator RefreshLayout()
    {
        // 원인 불명.. 2프레임에 걸쳐 최소 2회 갱신해야 정상 작동함

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }
    }

    void SetCatGiftLevelupUIData()
    {
        foreach (Transform child in materialListScrollContainer.transform)
        {
            if (child.gameObject != materialCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        materialCloneObject.SetActive(true);

        List<KeyValuePair<uint, uint>> matList = curLevelData.GetMaterialList();

        // 골드 재료 정보 세팅
        needMoney = curLevelData.GetNecoNeedGold();
        if (needMoney > 0)
        {
            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.IsMoneyData = true;
            recipeMaterial.SetMoneyMaterialData(needMoney, userMoney);

            curNeedMaterials.Add(recipeMaterial);
        }

        // 일반 재료 세팅
        foreach (KeyValuePair<uint, uint> mat in matList)
        {
            if (mat.Key == 0) continue;

            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.SetMarerialData(mat);

            curNeedMaterials.Add(recipeMaterial);
        }

        materialCloneObject.SetActive(false);

        // 재료 부족 검사
        CheckAvailLevelup(matList);

        // 보은 수급량 계산
        List<neco_user_cat> userCatList = neco_user_cat.GetGainUserCatList();
        uint catCount = (uint)userCatList.Count;
        uint totalCatCount = (uint)neco_cat.GetTotalNecoCatCount();

        uint currentGiftAmountResult = curLevelData.GetNecoLevelValue1() * catCount;
        uint nextGiftAmountResult = nextLevelData.GetNecoLevelValue1() * catCount;

        // 텍스트 세팅
        LevelupTextData textData1 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_284"), curLevelData.GetNecoLevel().ToString(), nextLevelData.GetNecoLevel().ToString());
        //LevelupTextData textData2 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_285"), currentGiftAmountResult.ToString("n0"), nextGiftAmountResult.ToString("n0"));
        LevelupTextData textData3 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_286"), curLevelData.GetNecoLevelValue2().ToString("n0"), nextLevelData.GetNecoLevelValue2().ToString("n0"));
        List<uint> itemlist = neco_gift_basket.GetLevelUPNewItems(curLevelData.GetNecoLevel() + 1);
        List<string> itemNames = new List<string>();
        foreach(uint itemNo in itemlist)
        {
            items item = items.GetItem(itemNo);
            if (item != null)
                itemNames.Add(item.GetItemName());
        }
        LevelupTextData textData4 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.CUSTOM_STYLE, string.Format(LocalizeData.GetText("LOCALIZE_458"), string.Join(",", itemNames)));

        levelupTextDataList.Add(textData1);
        //levelupTextDataList.Add(textData2);
        levelupTextDataList.Add(textData3);
        levelupTextDataList.Add(textData4);

        CreateInfoTextLayer();
    }

    void SetFishfarmLevelupUIData()
    {
        foreach (Transform child in materialListScrollContainer.transform)
        {
            if (child.gameObject != materialCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        materialCloneObject.SetActive(true);

        List<KeyValuePair<uint, uint>> matList = curLevelData.GetMaterialList();

        // 골드 재료 정보 세팅
        needMoney = curLevelData.GetNecoNeedGold();
        if (needMoney > 0)
        {
            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.IsMoneyData = true;
            recipeMaterial.SetMoneyMaterialData(needMoney, userMoney);

            curNeedMaterials.Add(recipeMaterial);
        }

        // 일반 재료 세팅
        foreach (KeyValuePair<uint, uint> mat in matList)
        {
            if (mat.Key == 0) continue;

            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.SetMarerialData(mat);

            curNeedMaterials.Add(recipeMaterial);
        }

        materialCloneObject.SetActive(false);

        // 재료 부족 검사
        CheckAvailLevelup(matList);

        // 텍스트 세팅
        LevelupTextData textData1 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_284"), curLevelData.GetNecoLevel().ToString(), nextLevelData.GetNecoLevel().ToString());
        LevelupTextData textData2 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_287"), curLevelData.GetNecoLevelValue1().ToString("n0"), nextLevelData.GetNecoLevelValue1().ToString("n0"));
        LevelupTextData textData3 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_288"), curLevelData.GetNecoLevelValue2().ToString("n0"), nextLevelData.GetNecoLevelValue2().ToString("n0"));

        levelupTextDataList.Add(textData1);
        levelupTextDataList.Add(textData2);
        levelupTextDataList.Add(textData3);

        CreateInfoTextLayer();
    }

    void SetFishtrapLevelupUIData()
    {
        foreach (Transform child in materialListScrollContainer.transform)
        {
            if (child.gameObject != materialCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        materialCloneObject.SetActive(true);

        List<KeyValuePair<uint, uint>> matList = curLevelData.GetMaterialList();

        // 골드 재료 정보 세팅
        needMoney = curLevelData.GetNecoNeedGold();
        if (needMoney > 0)
        {
            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.IsMoneyData = true;
            recipeMaterial.SetMoneyMaterialData(needMoney, userMoney);

            curNeedMaterials.Add(recipeMaterial);
        }

        // 일반 재료 세팅
        foreach (KeyValuePair<uint, uint> mat in matList)
        {
            if (mat.Key == 0) continue;

            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.SetMarerialData(mat);

            curNeedMaterials.Add(recipeMaterial);
        }

        materialCloneObject.SetActive(false);

        // 재료 부족 검사
        CheckAvailLevelup(matList);

        // 텍스트 세팅
        LevelupTextData textData1 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_284"), curLevelData.GetNecoLevel().ToString(), nextLevelData.GetNecoLevel().ToString());
        LevelupTextData textData2 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_289"), curLevelData.GetNecoLevelValue2().ToString("n0"), nextLevelData.GetNecoLevelValue2().ToString("n0"));

        levelupTextDataList.Add(textData1);
        levelupTextDataList.Add(textData2);

        List<neco_fish_trap_rate> newFishList = neco_fish_trap_rate.GetNewFishListByLevel(curLevelData.GetNecoLevel());
        if (newFishList != null && newFishList.Count > 0)
        {
            foreach (neco_fish_trap_rate fishinfo in newFishList)
            {
                items itemData = items.GetItem(fishinfo.GetNecoFishID());
                string resultStr = string.Format(LocalizeData.GetText("LOCALIZE_290"), itemData.GetItemName());

                LevelupTextData textData3 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.COMMON_STYLE, resultStr);
                levelupTextDataList.Add(textData3);
            }
        }

        CreateInfoTextLayer();
    }

    void SetCookLevelupUIData()
    {
        foreach (Transform child in materialListScrollContainer.transform)
        {
            if (child.gameObject != materialCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        materialCloneObject.SetActive(true);

        List<KeyValuePair<uint, uint>> matList = curLevelData.GetMaterialList();

        // 골드 재료 정보 세팅
        needMoney = curLevelData.GetNecoNeedGold();
        if (needMoney > 0)
        {
            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.IsMoneyData = true;
            recipeMaterial.SetMoneyMaterialData(needMoney, userMoney);

            curNeedMaterials.Add(recipeMaterial);
        }

        // 일반 재료 세팅
        foreach (KeyValuePair<uint, uint> mat in matList)
        {
            if (mat.Key == 0) continue;

            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.SetMarerialData(mat);

            curNeedMaterials.Add(recipeMaterial);
        }

        materialCloneObject.SetActive(false);

        // 재료 부족 검사
        CheckAvailLevelup(matList);

        // 텍스트 세팅
        LevelupTextData textData1 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_284"), curLevelData.GetNecoLevel().ToString(), nextLevelData.GetNecoLevel().ToString());

        levelupTextDataList.Add(textData1);

        List<recipe> newRecipeList = recipe.GetRecipeListByLevel("FOOD", nextLevelData.GetNecoLevel());
        if (newRecipeList != null && newRecipeList.Count > 0)
        {
            foreach (recipe recipeData in newRecipeList)
            {
                string resultStr = string.Format(LocalizeData.GetText("LOCALIZE_291"), recipeData.GetRecipeName());

                LevelupTextData textData2 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.COMMON_STYLE, resultStr);
                levelupTextDataList.Add(textData2);
            }
        }

        CreateInfoTextLayer();

        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.조리대레벨업)
        {
            // 골드 수급 강조 연출 적용
            levelupButton.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }

    void SetCraftLevelupUIData()
    {
        foreach (Transform child in materialListScrollContainer.transform)
        {
            if (child.gameObject != materialCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        materialCloneObject.SetActive(true);

        List<KeyValuePair<uint, uint>> matList = curLevelData.GetMaterialList();

        // 골드 재료 정보 세팅
        needMoney = curLevelData.GetNecoNeedGold();
        if (needMoney > 0)
        {
            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.IsMoneyData = true;
            recipeMaterial.SetMoneyMaterialData(needMoney, userMoney);

            curNeedMaterials.Add(recipeMaterial);
        }

        // 일반 재료 세팅
        foreach (KeyValuePair<uint, uint> mat in matList)
        {
            if (mat.Key == 0) continue;

            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.SetMarerialData(mat);

            curNeedMaterials.Add(recipeMaterial);
        }

        materialCloneObject.SetActive(false);

        // 재료 부족 검사
        CheckAvailLevelup(matList);

        // 텍스트 세팅
        LevelupTextData textData1 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_284"), curLevelData.GetNecoLevel().ToString(), nextLevelData.GetNecoLevel().ToString());

        levelupTextDataList.Add(textData1);

        List<recipe> newRecipeList = recipe.GetRecipeListByLevel("TOY", nextLevelData.GetNecoLevel());
        if (newRecipeList != null && newRecipeList.Count > 0)
        {
            foreach (recipe recipeData in newRecipeList)
            {
                string resultStr = string.Format(LocalizeData.GetText("LOCALIZE_292"), recipeData.GetRecipeName());

                LevelupTextData textData2 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.COMMON_STYLE, resultStr);
                levelupTextDataList.Add(textData2);
            }
        }

        CreateInfoTextLayer();
    }

    void SetObjectLevelupUIData()
    {
        if (curRecipeData == null) { return; }

        foreach (Transform child in materialListScrollContainer.transform)
        {
            if (child.gameObject != materialCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        materialCloneObject.SetActive(true);

        // 기본 오브젝트 데이터 세팅
        
        neco_spot curLevelSpotObjectData = neco_spot.GetNecoSpotObjectByItemID(curRecipeData.GetOutputItem(0).Key);
        neco_object_durability objectDurabilityData = neco_object_durability.GetObjectDurability(curLevelSpotObjectData.GetSpotID());
        curLevelData = neco_level.GetNecoLevelDataByObjectID(curLevelSpotObjectData.GetSpotID(), curLevelSpotObjectData.GetSpotLevel());

        List<KeyValuePair<uint, uint>> matList = curLevelData.GetMaterialList();

        // 골드 재료 정보 세팅
        needMoney = curLevelData.GetNecoNeedGold();
        if (needMoney > 0)
        {
            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.IsMoneyData = true;
            recipeMaterial.SetMoneyMaterialData(needMoney, userMoney);

            curNeedMaterials.Add(recipeMaterial);
        }

        needCatnip = curLevelData.GetNecoNeedCatnip();
        if (needCatnip > 0)
        {
            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.IsMoneyData = true;
            recipeMaterial.SetCatnipMaterialData(needCatnip, userCatnip);

            curNeedMaterials.Add(recipeMaterial);
        }

        // 일반 재료 세팅
        foreach (KeyValuePair<uint, uint> mat in matList)
        {
            if (mat.Key == 0) continue;

            GameObject userItemUI = Instantiate(materialCloneObject);
            userItemUI.transform.SetParent(materialListScrollContainer.transform);
            userItemUI.transform.localScale = materialCloneObject.transform.localScale;
            userItemUI.transform.localPosition = materialCloneObject.transform.localPosition;

            RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
            recipeMaterial.SetMarerialData(mat);

            curNeedMaterials.Add(recipeMaterial);
        }

        materialCloneObject.SetActive(false);

        // 상단 오브젝트 레이어 세팅
        objectInfoLayer.SetActive(true);
        objectInfoImage.sprite = Resources.Load<Sprite>(curRecipeData.GetRecipeIcon());
        objectInfoNameText.text = curRecipeData.GetRecipeName();
        objectInfoDescText.text = curRecipeData.GetRecipeDesc();

        // 슬라이더 세팅
        float durabilityRate = 0.0f;
        if (curLevelSpotObjectData != null)
        {
            uint nowDurability = curLevelSpotObjectData.GetSpotItemDurability();
            uint maxDurability = objectDurabilityData.GetNecoDurabilityByLevel(curLevelSpotObjectData.GetSpotLevel());

            durabilityRate = (float)nowDurability / (float)maxDurability;
        }
        objectInfoSlider.value = durabilityRate;
        repairButton.GetComponent<Image>().color = user_items.GetUserItemAmount(148) > 0 ? originLevelupButtonColor : DimmedLevelupButtonColor;

        // 재료 부족 검사
        CheckAvailLevelup(matList);

        // 텍스트 세팅
        uint nowLevel = curLevelSpotObjectData.GetSpotLevel();
        uint nextLevel = nowLevel + 1;
        if(nextLevel > neco_data.OBJECT_MAX_LEVEL)
        {
            nextLevel = neco_data.OBJECT_MAX_LEVEL;
        }

        levelupTextDataList.Clear();

        // max 레벨 (수리 상태 일 때 처리)
        if (nowLevel >= neco_data.OBJECT_MAX_LEVEL)
        {
            levelupGuideText.text = LocalizeData.GetText("수리");
            levelupButtonText.text = LocalizeData.GetText("수리");

            LevelupTextData textData2 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_65"), curLevelSpotObjectData.GetSpotItemDurability().ToString("n0"), objectDurabilityData.GetNecoDurabilityByLevel(nextLevel).ToString("n0"));
            levelupTextDataList.Add(textData2);
        }

        if (nowLevel < neco_data.OBJECT_MAX_LEVEL)
        {
            LevelupTextData textData1 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_284"), nowLevel.ToString(), nextLevel.ToString());
            levelupTextDataList.Add(textData1);
            LevelupTextData textData2 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("LOCALIZE_293"), objectDurabilityData.GetNecoDurabilityByLevel(nowLevel).ToString("n0"), objectDurabilityData.GetNecoDurabilityByLevel(nextLevel).ToString("n0"));
            levelupTextDataList.Add(textData2);

            if (user_items.GetUserItemAmount(147) > 0)
            {
                LevelupTextData textData3 = null;
                uint cur = 0;

                uint catnipfarmLevel = neco_spot.GetNecoSpotObjectByItemID(147).GetSpotLevel();
                uint targetID = 0;
                uint targetLevel = 0;
                if (user_items.GetUserItemAmount(154) > 0)
                {
                    targetID = 154;
                    targetLevel = neco_spot.GetNecoSpotObjectByItemID(154).GetSpotLevel();
                }
                else if (user_items.GetUserItemAmount(124) > 0)
                {
                    targetID = 124;
                    targetLevel = neco_spot.GetNecoSpotObjectByItemID(124).GetSpotLevel();
                }

                catnip_farm farmdata = catnip_farm.GetCatnipFarmData(catnipfarmLevel, targetID, targetLevel);
                if (farmdata != null)
                    cur = farmdata.GetFarmValue();

                switch (curRecipeData.GetRecipeID())
                {
                    case 47://이동식 캣하우스                    
                        farmdata = catnip_farm.GetCatnipFarmData(catnipfarmLevel, targetID, targetLevel + 1);
                        if (farmdata != null)
                        {
                            uint next = farmdata.GetFarmValue();
                            textData3 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("캣닢급식소증가량"), cur.ToString("n0"), next.ToString("n0"));
                        }
                        break;
                    case 49://캣닢급식소
                        farmdata = catnip_farm.GetCatnipFarmData(catnipfarmLevel + 1, targetID, targetLevel);
                        if (farmdata != null)
                        {
                            uint next = farmdata.GetFarmValue();
                            textData3 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("캣닢급식소증가량"), cur.ToString("n0"), next.ToString("n0"));
                        }
                        break;
                    case 52://알록달록 이동식 캣하우스
                        farmdata = catnip_farm.GetCatnipFarmData(catnipfarmLevel, targetID, targetLevel + 1);
                        if (farmdata != null)
                        {
                            uint next = farmdata.GetFarmValue();
                            textData3 = new LevelupTextData(LevelupTextData.LEVELUP_TEXT_STYLE.ARROW_STYLE, LocalizeData.GetText("캣닢급식소증가량"), cur.ToString("n0"), next.ToString("n0"));
                        }
                        break;
                }

                if (textData3 != null)
                    levelupTextDataList.Add(textData3);
            }
        }

        CreateInfoTextLayer();

        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드)
        {
            levelupButton.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }

    void CheckAvailLevelup(List<KeyValuePair<uint, uint>> matList)
    {
        // 보유 골드 검사
        uint needGold = curLevelData.GetNecoNeedGold();
        if (needGold > 0)
        {
            // 골드 부족시 0개 처리
            if (needGold > userMoney)
            {
                isAvailLevelup = false;
                return;
            }
        }

        uint needCatnip = curLevelData.GetNecoNeedCatnip();
        if (needCatnip > 0)
        {
            // 골드 부족시 0개 처리
            if (needCatnip > userCatnip)
            {
                isAvailLevelup = false;
                return;
            }
        }

        List<game_data> userItemList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);
        // 재료 갯수 검사
        foreach (KeyValuePair<uint, uint> input in matList)
        {
            if (input.Key == 0) continue;

            uint need = input.Value;
            bool hasItem = false;
            uint cur = 0;
            foreach (game_data user_item in userItemList)
            {
                if ((((user_items)user_item).GetItemID() == input.Key))
                {
                    cur = ((user_items)user_item).GetAmount();
                    hasItem = cur >= need;

                    break;
                }
            }

            // 부족한 재료가 1개라도 있을 경우 0개 처리
            if (hasItem == false)
            {
                isAvailLevelup = false;
            }
        }
    }

    void AlignScrollView(int dicCount)
    {
        if (materialListScrollContainer == null) return;

        RectTransform scrollRect = materialListScrollContainer.GetComponent<RectTransform>();

        if (dicCount > centerSortCount)
        {
            scrollRect.pivot = new Vector2(0, 0.5f);
            scrollRect.anchoredPosition = Vector3.zero;
        }
        else
        {
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = Vector3.zero;
        }
    }

    void SetUserData()
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;

        object obj;
        uint money = 0;
        uint catnip = 0;
        if (user.data.TryGetValue("gold", out obj))
        {
            money = (uint)obj;
        }
        if (user.data.TryGetValue("catnip", out obj))
        {
            catnip = (uint)obj;
        }
        userMoney = money;
        userCatnip = catnip;
    }

    void OnTryLevelUp()
    {
        int whatType = (int)curLevelupUIType;
        if (curLevelupUIType == SUPPLY_UI_TYPE.OBJECT && curLevelData != null)
        {
            whatType = (int)curLevelData.GetNecoLevelObjectID();
        }

        WWWForm data = new WWWForm();
        data.AddField("api", "upgrade");
        data.AddField("op", 1);

        data.AddField("what", whatType);

        isFixed = false;
        neco_spot spotObjectData = neco_spot.GetNecoSpot((uint)whatType);
        if(spotObjectData != null)
        {
            isFixed = spotObjectData.GetSpotLevel() >= neco_data.OBJECT_MAX_LEVEL;
        }
        NetworkManager.GetInstance().SendApiRequest("upgrade", 1, data, (response) => {


            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "upgrade")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            //if (isFixed)
                            //{
                            //    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_469"), LocalizeData.GetText("수리완료"), ()=> { callback?.Invoke(); });
                            //    return;
                            //}
                            //Invoke("Refresh", 0.1f);  // 레벨업 후에도 팝업 유지 시 다시 활성화
                            Invoke("UpdateSucessPopup", 0.1f);

                            if (curLevelupUIType == SUPPLY_UI_TYPE.FISH_FARM && neco_data.Instance.GetFishfarmLevel() >= 4)
                            {
                                MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                                if (mapController != null)
                                    mapController.SendMessage("OnNewCat", 9, SendMessageOptions.DontRequireReceiver);
                            }

                            // 구글 플레이 업적
                            Invoke("UpdateGooglePlayAchievement", 0.1f);
                        }
                        else
                        {
                            string msg = LocalizeData.GetText("LOCALIZE_294");
                            switch (rs)
                            {
                                case 2: msg = LocalizeData.GetText("매력도최대상태"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(msg);
                        }
                    }
                }
            }
        });
    }

    void UpdateSucessPopup()
    {
        OnSuccessPopupShow(levelupTextDataList, callback);
    }

    void OnSuccessPopupShow(List<LevelupTextData> textList, NecoLevelupSuccessPanel.Callback levelupCloseCallback = null)
    {
        if (successPopupObject == null) { return; }
        if (textList == null && textList.Count <= 0) { return; }

        successPopupObject.SetActive(true);
        successPopupObject.GetComponent<NecoLevelupSuccessPanel>().SetLevelupSuccessPopupMsg(curLevelupUIType, textList, isFixed, () => {
            levelupCloseCallback?.Invoke();
            NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
        });

        
    }

    void CreateInfoTextLayer()
    {
        if (levelupInfoContainer == null || levelupInfoCloneObject == null) { return; }
        if (levelupTextDataList == null || levelupTextDataList.Count <= 0) { return; }

        foreach (Transform child in levelupInfoContainer.transform)
        {
            if (child.gameObject != levelupInfoCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        levelupInfoCloneObject.SetActive(true);

        foreach (LevelupTextData textData in levelupTextDataList)
        {
            GameObject InfoText = Instantiate(levelupInfoCloneObject);
            InfoText.transform.SetParent(levelupInfoContainer.transform);
            InfoText.transform.localScale = levelupInfoCloneObject.transform.localScale;
            InfoText.transform.localPosition = levelupInfoCloneObject.transform.localPosition;

            InfoText.GetComponent<LevelupTextInfo>().SetLevelTextData(textData);
        }

        levelupInfoCloneObject.SetActive(false);
    }

    void UpdateGooglePlayAchievement()
    {
        // 구글 플레이 업적
        switch (curLevelupUIType)
        {
            case SUPPLY_UI_TYPE.FISH_FARM:
                if (neco_data.Instance.GetFishfarmLevel() == 5)
                {
                    GameServiceManager.GetInstance().TryAchievementProgress("achievement_skilled_fishfarmer", 100.0f, null);
                }
                else if (neco_data.Instance.GetFishfarmLevel() == 10)
                {
                    GameServiceManager.GetInstance().TryAchievementProgress("achievement_master_of_fishfarm", 100.0f, null);
                }
                break;
            case SUPPLY_UI_TYPE.FISH_TRAP:
                if (neco_data.Instance.GetFishtrapLevel() == 5)
                {
                    GameServiceManager.GetInstance().TryAchievementProgress("achievement_skilled_angler", 100.0f, null);
                }
                else if (neco_data.Instance.GetFishtrapLevel() == 10)
                {
                    GameServiceManager.GetInstance().TryAchievementProgress("achievement_master_of_fishing", 100.0f, null);
                }
                break;
            case SUPPLY_UI_TYPE.CAT_GIFT:
                if (neco_data.Instance.GetGiftBasketLevel() == 5)
                {
                    GameServiceManager.GetInstance().TryAchievementProgress("achievement_cat_lover", 100.0f, null);
                }
                else if (neco_data.Instance.GetGiftBasketLevel() == 10)
                {
                    GameServiceManager.GetInstance().TryAchievementProgress("achievement_cat_holic", 100.0f, null);
                }
                break;
            case SUPPLY_UI_TYPE.WORKBENCH:
                if (neco_data.Instance.GetCraftRecipeLevel() == 5)
                {
                    GameServiceManager.GetInstance().TryAchievementProgress("achievement_skilled_maker", 100.0f, null);
                }
                else if (neco_data.Instance.GetCraftRecipeLevel() == 10)
                {
                    GameServiceManager.GetInstance().TryAchievementProgress("achievement_master_of_maker", 100.0f, null);
                }
                break;
            case SUPPLY_UI_TYPE.COUNTERTOP:
                if (neco_data.Instance.GetCookRecipeLevel() == 5)
                {
                    GameServiceManager.GetInstance().TryAchievementProgress("achievement_skilled_cooker", 100.0f, null);
                }
                else if (neco_data.Instance.GetCookRecipeLevel() == 10)
                {
                    GameServiceManager.GetInstance().TryAchievementProgress("achievement_master_of_cook", 100.0f, null);
                }
                break;
        }
    }

    void UpdateButtonColor()
    {
        if (levelupButton == null) { return; }

        levelupButton.GetComponent<Image>().color = isAvailLevelup ? originLevelupButtonColor : DimmedLevelupButtonColor;
    }

    void ClearData()
    {
        isAvailLevelup = true;

        levelupTextDataList.Clear();

        objectInfoLayer.SetActive(false);

        levelupGuideText.text = LocalizeData.GetText("LOCALIZE_90");
        levelupButtonText.text = LocalizeData.GetText("LOCALIZE_45");

        levelupButton.GetComponent<RectTransform>().localScale = Vector3.one;
        levelupButton.GetComponent<RectTransform>().DORewind();
        levelupButton.GetComponent<RectTransform>().DOKill();
    }

    bool CheckPrologueWithToastAlarm()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
            case neco_data.PrologueSeq.조리대레벨업:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ17"));
                return true;
            case neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_401"));
                return true;
        }

        return false;
    }

    public void OnRepairTicket()
    {
        uint ticket_amount = user_items.GetUserItemAmount(148);
        if (ticket_amount <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("수리권부족"));
            return;
        }

        ConfirmPopupData popupData = new ConfirmPopupData();
        popupData.titleText = LocalizeData.GetText("LOCALIZE_85");

        popupData.titleMessageText = LocalizeData.GetText("수리권사용버튼");

        popupData.messageText_1 = LocalizeData.GetText("수리권사용메시지");
        popupData.messageText_2 = string.Format(LocalizeData.GetText("수리권보유량"), ticket_amount);
        popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_repairticket");
        popupData.amountText = "1"; // todo bt - 추후 데이터 연동 필요


        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, () =>
        {
            int whatType = (int)curLevelData.GetNecoLevelObjectID();

            WWWForm data = new WWWForm();
            data.AddField("api", "upgrade");
            data.AddField("op", 2);
            data.AddField("what", whatType);


            NetworkManager.GetInstance().SendApiRequest("upgrade", 2, data, (response) =>
            {
                JObject root = JObject.Parse(response);
                JToken apiToken = root["api"];
                if (null == apiToken || apiToken.Type != JTokenType.Array
                    || !apiToken.HasValues)
                {
                    return;
                }

                JArray apiArr = (JArray)apiToken;
                foreach (JObject row in apiArr)
                {
                    string uri = row.GetValue("uri").ToString();
                    if (uri == "upgrade")
                    {
                        JToken resultCode = row["rs"];
                        if (resultCode != null && resultCode.Type == JTokenType.Integer)
                        {
                            int rs = resultCode.Value<int>();
                            if (rs == 0)
                            {
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), LocalizeData.GetText("수리권사용완료"));
                                Invoke("OnRepairTicketSuccess", 0.1f);
                            }
                            else
                            {
                                string msg = LocalizeData.GetText("LOCALIZE_294");
                                switch (rs)
                                {
                                    case 2: msg = LocalizeData.GetText("매력도최대상태"); break;
                                }

                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(msg);
                            }
                        }
                    }
                }
            });
        });
    }

    void OnRepairTicketSuccess()
    {
        SetObjectLevelupUIData();
        callback?.Invoke();
    }
}
