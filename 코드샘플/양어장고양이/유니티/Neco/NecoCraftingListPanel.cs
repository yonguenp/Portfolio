using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CraftData
{
    public items itemData;
    public recipe recipeData;

    public Sprite craftIcon;
    public uint craftCount;
    public uint craftMaxCount;
    public string craftName;
    public string craftDesc;
};

public class NecoCraftingListPanel : CraftingUI
{
    public Text craftLevelText;

    [Header("[Craft Info List]")]
    public GameObject recipeListScrollContainer;
    public GameObject recipeListCloneObject;

    [Header("[Craft List Button]")]
    public GameObject levelupButton;
    public Button matButton;
    public Button toyObjectButton;
    public Color levelupButtonColor;
    public Color originButtonColor;
    public Color dimmedButtonColor;

    public GameObject HelpWindow;

    CraftData curSelectedCraftData = null;
    List<CraftData> craftDataList = new List<CraftData>();

    neco_level necoLevelData;

    bool isFirst = true;
    bool isSelectMat = true;

    uint userMoney;

    private void OnEnable()
    {
        //Refresh();
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
        if (seq == neco_data.PrologueSeq.철판제작가이드퀘스트 || seq == neco_data.PrologueSeq.우드락제작)
        {
            OnClickMatListButton();
        }
        else
        {
            OnClickObjectListButton(); // 설치물 디폴트로 오픈
        }

        if (HelpWindow != null)
            HelpWindow.SetActive(false);
    }

    private void OnDisable()
    {
        isFirst = true;
    }

    public void OnShowCraftingListPanel(bool isMat = false)
    {
        if (isMat)
        {
            OnClickMatListButton();
        }
        else
        {
            OnClickObjectListButton();
        }
    }

    public void OnClickOpenCraftListButton()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_MAKING_LIST_POPUP);
    }

    public void OnClickPopupCloseButton()
    {
        // 프롤로그 체크
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ13"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_401"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_MAKING_LIST_POPUP);
    }

    public void OnClickLevelupButton()
    {
        if (necoLevelData == null) { return; }

        // 프롤로그 체크
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ13"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_401"));
            return;
        }

        if (necoLevelData.GetNecoLevel() < neco_data.CRAFT_MAX_LEVEL)
        {
            NecoCanvas.GetPopupCanvas().OnLevelupPopupShow(SUPPLY_UI_TYPE.WORKBENCH, Refresh);
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_249"));
        }
    }

    public void OnClickMatListButton()
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
            return;
        }

        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_401"));
            return;
        }

        // 이미 해당 탭일 경우 처리
        if (isSelectMat && isFirst == false) { return; }

        matButton.image.color = originButtonColor;
        toyObjectButton.image.color = dimmedButtonColor;

        isFirst = false;
        isSelectMat = true;
        Refresh();
    }

    public void OnClickObjectListButton()
    {
        // 프롤로그 체크
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ13"));
            return;
        }

        // 이미 해당 탭일 경우 처리
        if (isSelectMat == false && isFirst == false) { return; }

        matButton.image.color = dimmedButtonColor;
        toyObjectButton.image.color = originButtonColor;

        isFirst = false;
        isSelectMat = false;
        Refresh();
    }

    public override void CreftingDone()
    {
        //RewardData rewardData = new RewardData();

        //rewardData.sprite = curSelectedCraftData.craftIcon;
        //rewardData.name = curSelectedCraftData.craftName;
        //rewardData.count = curSelectedCraftData.craftMaxCount;

        //NecoCanvas.GetPopupCanvas().OnSingleRewardPopup("제작 완료" ,"제작을 완료하였습니다.", rewardData, Refresh);

        //if (curSelectedCraftData.recipeData.GetRecipeType() == "TOY")
        //{
        //    NecoCanvas.GetPopupCanvas().OnSuccessEffectPopupShow(EFFECT_TYPE.NEW_OBJECT, curSelectedCraftData.recipeData.GetRecipeID(), curSelectedCraftData.craftMaxCount, EffectDone);
        //}
        //else
        //{
        //    NecoCanvas.GetPopupCanvas().OnSuccessEffectPopupShow(EFFECT_TYPE.NEW_RECIPE_RESULT, curSelectedCraftData.recipeData.GetRecipeID(), curSelectedCraftData.craftMaxCount, EffectDone);
        //}

        //Refresh();
    }

    public void EffectDone()
    {
        //if (curSelectedCraftData.recipeData.GetRecipeID() == 28 && neco_data.PrologueSeq.우드락제작 == neco_data.GetPrologueSeq())
        //{
        //    MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
        //    if (mapController != null)
        //    {
        //        ApplyGuideTryCount((int)curSelectedCraftData.craftCount);
        //        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.우드락제작, SendMessageOptions.DontRequireReceiver);
        //    }
        //}

        Refresh();
    }

    public override void Refresh()
    {
        necoLevelData = neco_level.GetNecoLevelData("TOY", neco_data.Instance.GetCraftRecipeLevel());

        SetUserData();
        InitCraftList();
        SetCraftListUI();

        recipeListScrollContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
    }

    public CraftData RefreshByCraftUI(CraftData curCraftData)
    {
        Refresh();

        CraftData newCraftData = new CraftData();

        if (craftDataList != null && craftDataList.Count > 0 && curCraftData != null)
        {
            newCraftData = craftDataList.Find(x => x.recipeData.GetRecipeID() == curCraftData.recipeData.GetRecipeID());
        }

        return newCraftData;
    }

    //public void OnStartQuickCook(CraftData curCraftData)
    //{
    //    curSelectedCraftData = curCraftData;
    //    if (curSelectedCraftData == null) { return; }

    //    OnCrafting(curSelectedCraftData.recipeData, curSelectedCraftData.craftMaxCount);
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

    void InitCraftList()
    {
        ClearData();
        
        // 현재 선택된 탭의 타입 적용
        string listType = isSelectMat ? "T_MATERIAL" : "TOY";

        // 제작대 레벨 데이터 적용
        uint nowRecipeLevel = neco_data.Instance.GetCraftRecipeLevel();
        craftLevelText.text = string.Format("Lv {0}/{1}", nowRecipeLevel, neco_data.CRAFT_MAX_LEVEL); 

        List<game_data> userItemList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);
        List<game_data> recipeList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.RECIPE);

        object obj;
        if (userItemList != null && recipeList != null)
        {
            foreach (game_data craftRecipe in recipeList)
            {
                if (craftRecipe.data.TryGetValue("recipe_type", out obj))
                {
                    if ((string)obj == listType)
                    {
                        recipe recipeInfo = (recipe)craftRecipe;
                        KeyValuePair<uint, uint> outputItem = recipeInfo.GetOutputItem(0);
                        items itemInfo = items.GetItem(outputItem.Key);

                        // 제작 관련 데이터 세팅
                        CraftData craftData = new CraftData();
                        craftData.itemData = itemInfo;
                        craftData.recipeData = recipeInfo;
                        
                        craftData.craftIcon = craftData.itemData.GetItemIcon();
                        craftData.craftName = craftData.itemData.GetItemName();
                        craftData.craftDesc = craftData.itemData.GetItemDesc();
                        craftData.craftCount = 0;
                        craftData.craftMaxCount = 0;

                        // 레시피 최대 제작 갯수 계산 (레벨 충족)
                        if (nowRecipeLevel >= craftData.recipeData.GetRecipeLevel())
                        {
                            craftData.craftMaxCount = CalculateMaxCraftCount(craftData, userItemList);
                        }

                        // 2차재료 / 장난감 최대 제작 횟수 분기 처리
                        if (recipeInfo.GetRecipeType() == "TOY" && craftData.craftMaxCount > 1)
                        {
                            craftData.craftMaxCount = 1;
                        }

                        // 설치물인데 이미 가지고 있는 경우 다시 0개 처리
                        if (recipeInfo.GetRecipeType() == "TOY" && craftData.recipeData.HasRecipeItem())
                        {
                            craftData.craftMaxCount = 0;
                        }

                        // 유저 데이터에서 보유한 재료 정보 조회
                        foreach (game_data data in userItemList)
                        {
                            user_items userItem = (user_items)data;
                            uint userItem_id = userItem.GetItemID();

                            // 보유한 아이템이 있다면 카운트 갱신
                            if (itemInfo.GetItemID() == userItem_id)
                            {
                                craftData.craftCount = userItem.GetAmount();
                            }
                        }

                        craftDataList.Add(craftData);
                    }
                }
            }
        }
    }

    void SetCraftListUI()
    {
        if (craftDataList == null || craftDataList.Count <= 0) { return; }
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

        // 레시피 데이터 정렬
        List<CraftData> sortedCraftDataList = new List<CraftData>();
        List<CraftData> openCraftDataList = new List<CraftData>();
        List<CraftData> closeCraftDataList = new List<CraftData>();

        if (isSelectMat)
        {
            openCraftDataList = craftDataList.FindAll(x => x.recipeData.GetRecipeLevel() <= necoLevelData.GetNecoLevel());
            openCraftDataList = openCraftDataList.OrderByDescending(x => x.craftMaxCount > 0).ThenBy(x => x.recipeData.GetRecipeLevel()).ToList();

            closeCraftDataList = craftDataList.FindAll(x => x.recipeData.GetRecipeLevel() > necoLevelData.GetNecoLevel());
            closeCraftDataList = closeCraftDataList.OrderBy(x => x.recipeData.GetRecipeLevel()).ToList();
        }
        else
        {
            openCraftDataList = craftDataList.FindAll(x => x.recipeData.GetRecipeLevel() <= necoLevelData.GetNecoLevel());
            openCraftDataList = openCraftDataList.OrderByDescending(x => x.recipeData.HasRecipeItem()).ThenByDescending(x => x.craftMaxCount > 0).ThenBy(x => x.recipeData.GetRecipeLevel()).ToList();

            closeCraftDataList = craftDataList.FindAll(x => x.recipeData.GetRecipeLevel() > necoLevelData.GetNecoLevel());
            closeCraftDataList = closeCraftDataList.OrderBy(x => x.recipeData.GetRecipeLevel()).ToList();
        }

        CraftData catnipfarmRecipe = null;
        foreach (CraftData craftData in closeCraftDataList)
        {
            if (craftData.recipeData.GetRecipeID() == 49)
            {
                catnipfarmRecipe = craftData;
                closeCraftDataList.Remove(craftData);
                break;
            }
        }

        List<CraftData> RemoveList = new List<CraftData>();
        foreach(CraftData craftdata in openCraftDataList)
        {
            if(craftdata.recipeData.GetRecipeID() == 47 && user_items.GetUserItemAmount(154) > 0 || craftdata.recipeData.GetRecipeID() == 32 && user_items.GetUserItemAmount(157) > 0)
            {
                RemoveList.Add(craftdata);
            }
        }

        foreach(CraftData craftdata in RemoveList)
        {
            openCraftDataList.Remove(craftdata);
        }

        if(catnipfarmRecipe != null)
        {
            closeCraftDataList.Insert(0, catnipfarmRecipe);
        }

        sortedCraftDataList.AddRange(openCraftDataList);
        sortedCraftDataList.AddRange(closeCraftDataList);
        
        
        // 기본 레시피 데이터 설정
        foreach (CraftData craftData in sortedCraftDataList)
        {
            GameObject craftInfoUI = Instantiate(recipeListCloneObject);
            craftInfoUI.transform.SetParent(recipeListScrollContainer.transform);
            craftInfoUI.transform.localScale = recipeListCloneObject.transform.localScale;
            craftInfoUI.transform.localPosition = recipeListCloneObject.transform.localPosition;

            craftInfoUI.GetComponent<CraftRecipeListInfo>().SetCraftListInfoData(craftData, this);
        }

        recipeListCloneObject.SetActive(false);

        // max 레벨 버튼 체크
        levelupButton.GetComponentInChildren<Image>().color = necoLevelData.GetNecoLevel() >= neco_data.CRAFT_MAX_LEVEL ? dimmedButtonColor : levelupButtonColor;
    }

    uint CalculateMaxCraftCount(CraftData craftData, List<game_data> userItemList)
    {
        float maxRatio = 0.0f;
        float curMaxRatio = 0.0f;

        // 보유 골드 검사
        object obj;
        if (craftData.recipeData.data.TryGetValue("need_gold", out obj))
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
        for (int i = 0; i < craftData.recipeData.GetRecipeCaseCount(); i++)
        {
            List<KeyValuePair<uint, uint>> itemList = craftData.recipeData.GetNeedItems(i);

            foreach (KeyValuePair<uint, uint> input in itemList)
            {
                uint need = input.Value;
                bool hasItem = false;
                uint cur = 0;
                cur = user_items.GetUserItemAmount(input.Key);
                hasItem = cur >= need;

                // 부족한 재료가 1개라도 있을 경우 0개 처리
                if (hasItem == false)
                {
                    return 0;
                }

                switch (input.Key)
                {
                    case 124://이동식캣하우스 예외처리
                        if(neco_spot.GetNecoSpotObjectByItemID(124).GetSpotLevel() < neco_data.OBJECT_MAX_LEVEL)
                        {
                            return 0;
                        }
                        break;

                    case 109://화이트캣하우스 예외처리
                        if (neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() < neco_data.OBJECT_MAX_LEVEL)
                        {
                            return 0;
                        }
                        break;
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
        craftDataList.Clear();
        curSelectedCraftData = null;
    }

    bool CheckPrologue()
    {
        return neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.철판제작가이드퀘스트;
    }

    bool CheckPrologueWithToastAlarm()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
            case neco_data.PrologueSeq.조리대레벨업:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ17"));
                return true;
            case neco_data.PrologueSeq.배스구이강조:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
                return true;
            case neco_data.PrologueSeq.배틀패스강조및대사:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("패틀패스강조"));
                return true;
        }

        return false;
    }

    public void OnCraftingHelp()
    {
        if (HelpWindow == null)
            return;

        HelpWindow.SetActive(!HelpWindow.activeSelf);
    }
}
