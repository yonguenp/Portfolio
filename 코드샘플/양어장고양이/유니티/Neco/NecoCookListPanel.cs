using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FoodData
{
    public items itemData;
    public recipe recipeData;

    //public neco_food necoFoodData;

    public Sprite foodIcon;
    public uint foodCount;
    public uint foodMaxCount;
    public string foodName;
    public string foodDesc;
    public string foodDuration;
};

public class NecoCookListPanel : CraftingUI
{
    public Text cookLevelText;

    [Header("[Food Info List]")]
    public GameObject recipeListScrollContainer;
    public GameObject recipeListCloneObject;

    [Header("[Button Layers]")]
    public GameObject levelupButton;
    public Color levelupButtonColor;
    public Color originButtonColor;
    public Color DimmedButtonColor;

    FoodData curSelectedFoodData = null;
    List<FoodData> foodDataList = new List<FoodData>();

    neco_level necoLevelData;

    uint userMoney;

    private void OnEnable()
    {
        Refresh();

        //neco_data.ClientDEBUG_Seq seq = neco_data.GetDebugSeq();
        //if (neco_data.ClientDEBUG_Seq.COOK_BUTTON_OPEN == seq)
        //{
        //    neco_data.SetDebugSeq(neco_data.ClientDEBUG_Seq.COOK_DONE);
        //}
    }

    public void OnClickOpenCookListButton()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_COOK_LIST_POPUP);
    }

    public void OnClickPopupCloseButton()
    {
        // 프롤로그 체크
        if (CheckPrologue()) 
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_245"));
            return; 
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.조리대레벨업)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ17"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이강조)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_COOK_LIST_POPUP);
    }

    public void OnClickLevelupButton()
    {
        // 프롤로그 체크
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_245"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이강조)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
            return;
        }

        if (necoLevelData == null) { return; }

        if (necoLevelData.GetNecoLevel() < neco_data.COOK_MAX_LEVEL)
        {
            NecoCanvas.GetPopupCanvas().OnLevelupPopupShow(SUPPLY_UI_TYPE.COUNTERTOP, Refresh);
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_246"));
        }
    }

    public override void CreftingDone()
    {
        // 이전에 사용되던 리워드팝업 소스코드
        //RewardData rewardData = new RewardData();

        //rewardData.sprite = curSelectedFoodData.foodIcon;
        //rewardData.name = curSelectedFoodData.foodName;
        //rewardData.count = curSelectedFoodData.foodMaxCount;

        //NecoCanvas.GetPopupCanvas().OnSingleRewardPopup("요리 완료", "요리를 완료하였습니다.", rewardData, Refresh);

        //NecoCanvas.GetPopupCanvas().OnSuccessEffectPopupShow(EFFECT_TYPE.NEW_RECIPE_RESULT, curSelectedFoodData.recipeData.GetRecipeID(), curSelectedFoodData.foodMaxCount, EffectDone);

        //neco_data.ClientDEBUG_Seq seq = neco_data.GetDebugSeq();
        //if (neco_data.ClientDEBUG_Seq.COOK_DONE == seq)
        //{
        //    neco_data.SetDebugSeq(neco_data.ClientDEBUG_Seq.FOOD_TOUCH_2ND);
        //}

        //Refresh();        
    }

    //public void EffectDone()
    //{
    //    MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
    //    if (mapController != null)
    //    {
    //        mapController.SendMessage("OnCookResult", SendMessageOptions.DontRequireReceiver);

    //        if (curSelectedFoodData.recipeData.GetRecipeID() == 2)
    //        {
    //            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.전갱이구이제작, SendMessageOptions.DontRequireReceiver);
    //        }
    //        if (curSelectedFoodData.recipeData.GetRecipeID() == 3)
    //        {
    //            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.배스구이제작, SendMessageOptions.DontRequireReceiver);
    //        }
    //    }

    //    Refresh();
    //}

    public override void Refresh()
    {
        necoLevelData = neco_level.GetNecoLevelData("FOOD", neco_data.Instance.GetCookRecipeLevel());

        SetUserData();
        InitCookList();
        SetCookListUI();

        recipeListScrollContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
    }

    public FoodData RefreshByCookUI(FoodData curFoodData)
    {
        Refresh();

        FoodData newFoodData = new FoodData();

        if (foodDataList != null && foodDataList.Count > 0 && curFoodData != null)
        {
            newFoodData = foodDataList.Find(x => x.recipeData.GetRecipeID() == curFoodData.recipeData.GetRecipeID());
        }

        return newFoodData;
    }

    //public void OnStartQuickCook(FoodData curFoodData)
    //{
    //    curSelectedFoodData = curFoodData;
    //    if (curSelectedFoodData == null) { return; }

    //    OnCrafting(curSelectedFoodData.recipeData, curSelectedFoodData.foodMaxCount);
    //}

    void SetUserData()
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;

        object obj;
        uint money = 0;
        if (user.data.TryGetValue("gold", out obj))
        {
            money = (uint)obj;
        }

        userMoney = money;
    }

    void InitCookList()
    {
        ClearData();

        // 조리대 레벨 데이터 적용
        cookLevelText.text = string.Format("Lv {0}/{1}", neco_data.Instance.GetCookRecipeLevel(), neco_data.COOK_MAX_LEVEL);

        List<game_data> userItemList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);
        List<game_data> recipeList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.RECIPE);

        object obj;
        if (userItemList != null && recipeList != null)
        {
            foreach (game_data foodRecipe in recipeList)
            {
                if (foodRecipe.data.TryGetValue("recipe_type", out obj))
                {
                    if ((string)obj != "FOOD") continue;

                    recipe recipeInfo = (recipe)foodRecipe;
                    KeyValuePair<uint, uint> outputItem = recipeInfo.GetOutputItem(0);
                    items itemInfo = items.GetItem(outputItem.Key);

                    // 요리 지속 시간 계산
                    uint foodDuration = neco_food.GetFoodDuration(outputItem.Key);
                    uint minute = foodDuration / 60;
                    uint second = foodDuration % 60;  

                    // 요리 관련 데이터 세팅
                    FoodData foodData = new FoodData();
                    foodData.itemData = itemInfo;
                    foodData.recipeData = recipeInfo;
                    foodData.foodIcon = foodData.itemData.GetItemIcon();
                    foodData.foodName = foodData.itemData.GetItemName();
                    foodData.foodDesc = foodData.itemData.GetItemDesc();
                    foodData.foodCount = 0;
                    foodData.foodDuration = string.Format(LocalizeData.GetText("LOCALIZE_247"), minute, second);

                    // 레시피 최대 제작 갯수 계산
                    foodData.foodMaxCount = CalculateMaxCookCount(foodData, userItemList);

                    // 유저 데이터에서 보유한 먹이 정보 조회
                    foreach (game_data data in userItemList)
                    {
                        user_items userItem = (user_items)data;
                        uint userItem_id = userItem.GetItemID();

                        // 보유한 아이템이 있다면 카운트 갱신
                        if (itemInfo.GetItemID() == userItem_id)
                        {
                            foodData.foodCount = userItem.GetAmount();
                        }
                    }

                    foodDataList.Add(foodData);
                }
            }
        } 
    }

    void SetCookListUI()
    {
        if (foodDataList == null || foodDataList.Count <= 0) { return; }
        if (recipeListScrollContainer == null || recipeListCloneObject == null) { return; }
        if (necoLevelData == null) { return; }

        foreach (Transform child in recipeListScrollContainer.transform)
        {
            if (child.gameObject != recipeListCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        recipeListCloneObject.SetActive(true);

        // 조리 리스트 정렬
        List<FoodData> sortedFoodDataList = new List<FoodData>();

        List<FoodData> openFoodDataList = foodDataList.FindAll(x => x.recipeData.GetRecipeLevel() <= necoLevelData.GetNecoLevel());
        openFoodDataList = openFoodDataList.OrderByDescending(x => x.foodMaxCount > 0).ThenBy(x => x.recipeData.GetRecipeLevel()).ToList();

        List<FoodData> closeFoodDataList = foodDataList.FindAll(x => x.recipeData.GetRecipeLevel() > necoLevelData.GetNecoLevel());
        closeFoodDataList = closeFoodDataList.OrderBy(x => x.recipeData.GetRecipeLevel()).ToList();

        sortedFoodDataList.AddRange(openFoodDataList);
        sortedFoodDataList.AddRange(closeFoodDataList);

        // 기본 레시피 데이터 설정
        foreach (FoodData foodData in sortedFoodDataList)
        {
            GameObject foodInfoUI = Instantiate(recipeListCloneObject);
            foodInfoUI.transform.SetParent(recipeListScrollContainer.transform);
            foodInfoUI.transform.localScale = recipeListCloneObject.transform.localScale;
            foodInfoUI.transform.localPosition = recipeListCloneObject.transform.localPosition;

            foodInfoUI.GetComponent<RecipeListInfo>().SetFoodListInfoData(foodData, this);
        }

        recipeListCloneObject.SetActive(false);

        // max 레벨 버튼 체크
        levelupButton.GetComponentInChildren<Image>().color = necoLevelData.GetNecoLevel() >= neco_data.COOK_MAX_LEVEL ? DimmedButtonColor : levelupButtonColor;

        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.조리대레벨업)
        {
            levelupButton.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }

    uint CalculateMaxCookCount(FoodData foodData, List<game_data> userItemList)
    {
        float maxRatio = 0.0f;
        float curMaxRatio = 0.0f;

        // 보유 골드 검사
        object obj;
        if (foodData.recipeData.data.TryGetValue("need_gold", out obj))
        {
            uint needGold = (uint)obj;
            if (needGold > 0)
            {
                // 골드 부족시 0개 처리
                if (needGold > userMoney)
                {
                    return 0;
                }

                maxRatio = (float)userMoney / needGold;
            }
        }

        // 재료 갯수 검사
        for (int i = 0; i < foodData.recipeData.GetRecipeCaseCount(); i++)
        {
            List<KeyValuePair<uint, uint>> itemList = foodData.recipeData.GetNeedItems(i);

            foreach (KeyValuePair<uint, uint> input in itemList)
            { 
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
                    return 0;
                }

                // 재료가 있을 경우 처리
                curMaxRatio = (float)cur / need;
                maxRatio = curMaxRatio < maxRatio ? curMaxRatio : maxRatio;
            }
        }

        return (uint)maxRatio;
    }

    void ClearData()
    {
        foodDataList.Clear();
        curSelectedFoodData = null;

        levelupButton.GetComponent<RectTransform>().localScale = Vector3.one;
        levelupButton.GetComponent<RectTransform>().DORewind();
        levelupButton.GetComponent<RectTransform>().DOKill();
    }

    bool CheckPrologue()
    {
        return neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.조리대UI등장;
    }

    bool CheckPrologueWithToastAlarm()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
            case neco_data.PrologueSeq.철판제작가이드퀘스트:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ13"));
                return true;
            case neco_data.PrologueSeq.낚시장난감만들기:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
                return true;
            case neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_401"));
                return true;
            case neco_data.PrologueSeq.배틀패스강조및대사:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("패틀패스강조"));
                return true;
        }

        return false;
    }
}
