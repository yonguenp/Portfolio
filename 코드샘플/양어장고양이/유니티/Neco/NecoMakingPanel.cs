using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoMakingPanel : CraftingUI
{
    enum BUTTON_STATE
    {
        NONE,
        ADD,
        MINUS,
        MINUS_10,
        ADD_10,
    }

    public NecoCraftingListPanel craftListPanel;

    [Header("[Mid Recipe Mid_UI Info]")]
    public GameObject countLayer;
    public Image recipeIcon;
    public Text recipeNameText;
    public Text recipeCountText;
    public Text recipeDescText;

    [Header("[Mid Recipe Bot_UI Info]")]
    public GameObject recipeMaterialLayer;
    public GameObject recipeMaterialCloneObject;

    [Header("[BOT_UI Info]")]
    public GameObject craftToyButtonLayer;
    public GameObject craftMaterialtButtonLayer;
    public Text craftCountText;

    [Header("[Button Layer]")]
    public Button craftButton;
    public Button countCraftButton;
    public Color originButtonColor;
    public Color dimmedButtonColor;

    [Header("[Layout List]")]
    public RectTransform layoutRect;

    CraftData curSelectedRecipeData = null;

    private uint craftTryCount = 0;
    private List<RecipeMaterialInfo> curNeedMaterials = new List<RecipeMaterialInfo>();

    uint userMoney;
    uint needMoney;

    neco_spot spot;

    #region UI CONTROL
    public void OnClickCraftButton()
    {
        OnStartCrafting();
    }

    public void OnClickPopupCloseButton()
    {
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.철판제작가이드퀘스트 && curSelectedRecipeData.recipeData.GetRecipeID() == 22)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ13"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기 && curSelectedRecipeData.recipeData.GetRecipeID() == 29)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_MAKING_POPUP);
    }

    public void OnClickMinus10Button(int repeat)
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.철판제작가이드퀘스트 && curSelectedRecipeData.recipeData.GetRecipeID() == 22)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ13"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기 && curSelectedRecipeData.recipeData.GetRecipeID() == 29)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
            return;
        }

        CountButtonChecker(BUTTON_STATE.MINUS_10, repeat);
    }

    public void OnClickPlus10Button(int repeat)
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.철판제작가이드퀘스트 && curSelectedRecipeData.recipeData.GetRecipeID() == 22)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ13"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기 && curSelectedRecipeData.recipeData.GetRecipeID() == 29)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
            return;
        }

        CountButtonChecker(BUTTON_STATE.ADD_10, repeat);
    }

    public void OnClickPlusButton(int repeat)
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.철판제작가이드퀘스트 && curSelectedRecipeData.recipeData.GetRecipeID() == 22)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ13"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기 && curSelectedRecipeData.recipeData.GetRecipeID() == 29)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
            return;
        }

        CountButtonChecker(BUTTON_STATE.ADD, repeat);
    }

    public void OnClickMinusButton(int repeat)
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.철판제작가이드퀘스트 && curSelectedRecipeData.recipeData.GetRecipeID() == 22)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ13"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기 && curSelectedRecipeData.recipeData.GetRecipeID() == 29)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
            return;
        }

        CountButtonChecker(BUTTON_STATE.MINUS, repeat);
    }

    #endregion

    public void SetRecipeListUI(CraftData curSelectedData)
    {
        if (curSelectedData == null)
        {
            return;
        }

        ClearMakingPanel();

        curSelectedRecipeData = curSelectedData;
        ResetInputData();

        // 최대 1개이상 제작 가능하면 기본 제작 갯수 1추가
        if (curSelectedRecipeData.craftMaxCount > 0)
        {
            CountButtonChecker(BUTTON_STATE.ADD);
        }

        SetUserData();
        SetSelectedRecipeInfo();
        SetMaterialListUI();

        ButtonChecker();

        if (gameObject.activeSelf)
        {
            StartCoroutine(RefreshLayout());
        }

            // 프롤로그 세팅
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.철판제작가이드퀘스트 && curSelectedRecipeData.recipeData.GetRecipeID() == 22)
        {
            craftTryCount = 10;
            CountButtonChecker(BUTTON_STATE.NONE);
            
            countCraftButton.gameObject.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기 && curSelectedRecipeData.recipeData.GetRecipeID() == 29)
        {
            craftButton.gameObject.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public override void CreftingDone()
    {
        //RewardData rewardData = new RewardData();

        //rewardData.sprite = curSelectedRecipeData.craftIcon;
        //rewardData.name = curSelectedRecipeData.craftName;
        //rewardData.count = craftTryCount;

        //NecoCanvas.GetPopupCanvas().OnSingleRewardPopup("제작 완료" ,"제작을 완료하였습니다.", rewardData, Refresh);

        if (curSelectedRecipeData.recipeData.GetRecipeType() == "TOY")
        {
            NecoCanvas.GetPopupCanvas().OnSuccessEffectPopupShow(EFFECT_TYPE.NEW_OBJECT, curSelectedRecipeData.recipeData.GetRecipeID(), craftTryCount, Refresh);

        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnSuccessEffectPopupShow(EFFECT_TYPE.NEW_RECIPE_RESULT, curSelectedRecipeData.recipeData.GetRecipeID(), craftTryCount, Refresh);
        }

        //Refresh();
    }

    public override void Refresh()
    {
        if (craftListPanel == null) { return; }

        if (curSelectedRecipeData.recipeData.GetRecipeType() == "TOY")
        {

            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (curSelectedRecipeData.recipeData.GetRecipeID() == 29)//낚시 장난감
            {
                if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기)
                {
                    if (mapController != null)
                    {
                        mapController.SendMessage("낚시장난감만들기완료", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
            if (curSelectedRecipeData.recipeData.GetRecipeID() == 32)//화이트 캣하우스
            {
                if (mapController != null)
                {
                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.화이트캣하우스제작, SendMessageOptions.DontRequireReceiver);
                    mapController.SendMessage("OnNewCat", 5, SendMessageOptions.DontRequireReceiver);
                }
            }
            if (curSelectedRecipeData.recipeData.GetRecipeID() == 30)   // 화장실
            {
                if (mapController != null)
                {
                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.화장실제작, SendMessageOptions.DontRequireReceiver);
                }
            }
            if (curSelectedRecipeData.recipeData.GetRecipeID() == 36)//무스크래쳐
            {
                if (mapController != null)
                {
                    mapController.SendMessage("OnNewCat", 9, SendMessageOptions.DontRequireReceiver);
                }
            }

            if (mapController != null)
            {
                switch (curSelectedRecipeData.recipeData.GetRecipeID())
                {
                    case 35:
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.보온캣하우스제작, SendMessageOptions.DontRequireReceiver);
                        break;
                    case 36:
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.무스크래쳐제작, SendMessageOptions.DontRequireReceiver);
                        break;
                    case 38:
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.원목캣타워제작, SendMessageOptions.DontRequireReceiver);
                        break;
                    case 39:
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.반자동화장실제작, SendMessageOptions.DontRequireReceiver);
                        break;
                    case 41:
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.파이프캣타워제작, SendMessageOptions.DontRequireReceiver);
                        break;
                    case 43:
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.나무위캣하우스제작, SendMessageOptions.DontRequireReceiver);
                        break;
                    case 45:
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.크리스마스캣타워제작, SendMessageOptions.DontRequireReceiver);
                        break;

                    case 49:
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.캣닢급식소제작, SendMessageOptions.DontRequireReceiver);
                        break;
                    case 53:
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.플로랄캣하우스제작, SendMessageOptions.DontRequireReceiver);
                        break;
                    case 52:
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.알록달록이동식캣하우스제작, SendMessageOptions.DontRequireReceiver);
                        break;
                }
            }

            if (spot != null)
            {
                MapObjectController controller = NecoCanvas.GetGameCanvas().GetCurMapController();
                if (controller != null)
                {
                    foreach (MapObjectSpot s in controller.MapObjectSpots)
                    {
                        if (s.GetSpotID() == spot.GetSpotID())
                        {
                            spot.SetUI(s);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("not find maching spot");
            }

            NecoCanvas.GetPopupCanvas().OnPopupClose();

            uint targetMap = 0;
            items item = curSelectedRecipeData.itemData;
            if (item != null)
            {
                List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SPOT);
                if (necoData != null)
                {
                    foreach (neco_spot data in necoData)
                    {
                        if (data != null)
                        {
                            uint curObjectItem = objects.GetSpotItem(data.GetSpotID());
                            if (curObjectItem == item.GetItemID())
                            {
                                neco_map map = data.GetCurMapData();
                                if (map != null)
                                {
                                    targetMap = map.GetMapID();
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (NecoCanvas.GetGameCanvas().curMapID != targetMap)
            {
                NecoCanvas.GetGameCanvas().LoadMap(targetMap);
            }
            //debug
            //if (curRecipe.GetRecipeID() == 29)
            //{
            //    NecoCanvas.GetGameCanvas().TmpCraftingItem();
            //    NecoCanvas.GetPopupCanvas().OnPopupClose();
            //}
        }
        else
        {
            if (curSelectedRecipeData.recipeData.GetRecipeID() == 28 && neco_data.PrologueSeq.우드락제작 == neco_data.GetPrologueSeq())
            {
                MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                if (mapController != null)
                {
                    ApplyGuideTryCount((int)craftTryCount);
                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.우드락제작, SendMessageOptions.DontRequireReceiver);
                }
            }
            if (curSelectedRecipeData.recipeData.GetRecipeID() == 22 && neco_data.PrologueSeq.철판제작가이드퀘스트 == neco_data.GetPrologueSeq())
            {
                MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                if (mapController != null)
                {
                    ApplyGuideTryCount((int)craftTryCount);
                    mapController.SendMessage("철판제작가이드퀘스트완료", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        ResetInputData();
        curSelectedRecipeData = craftListPanel.RefreshByCraftUI(curSelectedRecipeData);
        SetRecipeListUI(curSelectedRecipeData);
        NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);

        if (gameObject.activeSelf)
        {
            StartCoroutine(RefreshLayout());
        }
    }

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

    void SetSelectedRecipeInfo()
    {
        countLayer.SetActive(curSelectedRecipeData.recipeData.GetRecipeType() != "TOY"); // 설치물의 경우 보유량을 표기하지 않음

        recipeIcon.sprite = curSelectedRecipeData.craftIcon;
        recipeNameText.text = curSelectedRecipeData.craftName;
        recipeCountText.text = LocalizeData.GetText("LOCALIZE_46") + curSelectedRecipeData.craftCount.ToString("n0");
        recipeDescText.text = curSelectedRecipeData.craftDesc;
    }

    void SetMaterialListUI()
    {
        if (curSelectedRecipeData == null)
        {
            return;
        }

        curNeedMaterials.Clear();
        recipeMaterialCloneObject.SetActive(true);

        foreach (Transform child in recipeMaterialLayer.transform)
        {
            if (child.gameObject != recipeMaterialCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        if (curSelectedRecipeData.recipeData.GetRecipeCaseCount() == 0)
        {
            recipeMaterialCloneObject.SetActive(false);
            return;
        }

        // 골드 재료 정보 세팅
        object obj;
        if (curSelectedRecipeData.recipeData.data.TryGetValue("need_gold", out obj))
        {
            needMoney = (uint)obj;
            if (needMoney > 0)
            {
                GameObject userItemUI = Instantiate(recipeMaterialCloneObject);
                userItemUI.transform.SetParent(recipeMaterialLayer.transform);
                userItemUI.transform.localScale = recipeMaterialCloneObject.transform.localScale;
                userItemUI.transform.localPosition = recipeMaterialCloneObject.transform.localPosition;

                RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
                recipeMaterial.IsMoneyData = true;
                recipeMaterial.SetMoneyMaterialData(needMoney, userMoney);

                curNeedMaterials.Add(recipeMaterial);
            }
        }

        // 아이템 재료 정보 세팅
        for (int i = 0; i < curSelectedRecipeData.recipeData.GetRecipeCaseCount(); i++)
        {
            List<KeyValuePair<uint, uint>> itemList = curSelectedRecipeData.recipeData.GetNeedItems(i);

            foreach (KeyValuePair<uint, uint> input in itemList)
            {
                GameObject userItemUI = Instantiate(recipeMaterialCloneObject);
                userItemUI.transform.SetParent(recipeMaterialLayer.transform);
                userItemUI.transform.localScale = recipeMaterialCloneObject.transform.localScale;
                userItemUI.transform.localPosition = recipeMaterialCloneObject.transform.localPosition;

                RecipeMaterialInfo recipeMaterial = userItemUI.GetComponent<RecipeMaterialInfo>();
                recipeMaterial.SetMarerialData(input);

                curNeedMaterials.Add(recipeMaterial);
            }
        }

        recipeMaterialCloneObject.SetActive(false);
    }

    void ButtonChecker()
    {
        // 제작 진행 버튼
        if (craftToyButtonLayer == null || craftMaterialtButtonLayer == null) { return; }
        if (craftButton == null) { return; }

        craftButton.image.color = craftTryCount > 0 ? originButtonColor : dimmedButtonColor;
        countCraftButton.image.color = craftTryCount > 0 ? originButtonColor : dimmedButtonColor;

        craftToyButtonLayer.SetActive(curSelectedRecipeData.recipeData.GetRecipeType() == "TOY");
        craftMaterialtButtonLayer.SetActive(curSelectedRecipeData.recipeData.GetRecipeType() == "T_MATERIAL");
    }

    void CountButtonChecker(BUTTON_STATE buttonState, int repeat = 1)
    {
        switch (buttonState)
        {
            case BUTTON_STATE.ADD:
                craftTryCount += (uint)repeat;
                break;
            case BUTTON_STATE.MINUS:
                if (craftTryCount == 0) break;   // uint 예외처리
                if (craftTryCount < (uint)repeat)
                    craftTryCount = 0;
                else
                    craftTryCount -= (uint)repeat;
                break;
            case BUTTON_STATE.ADD_10:
                if (craftTryCount == curSelectedRecipeData.craftMaxCount)
                    return;

                craftTryCount += 10 * (uint)repeat;
                if (craftTryCount >= curSelectedRecipeData.craftMaxCount)
                {
                    uint diff = (craftTryCount - curSelectedRecipeData.craftMaxCount) % 10;
                    if (diff == 0)
                        diff = 10;

                    users user = GameDataManager.Instance.GetUserData();
                    if (user == null)
                        return;

                    string msg = LocalizeData.GetText("재료부족");

                    object obj;
                    uint userGold = 0;
                    if (user.data.TryGetValue("gold", out obj))
                    {
                        userGold = (uint)obj;
                    }


                    if (curSelectedRecipeData.recipeData.data.TryGetValue("need_gold", out obj))
                    {
                        uint perGold = (uint)obj;
                        userGold = userGold - (perGold * curSelectedRecipeData.craftMaxCount);
                        if (userGold < perGold * diff)
                        {
                            uint needGold = (perGold * diff) - userGold;
                            msg += "\n" + LocalizeData.GetText(string.Format(LocalizeData.GetText("재화상세"), LocalizeData.GetText("LOCALIZE_229"), needGold));
                        }
                    }

                    for (int i = 0; i < curSelectedRecipeData.recipeData.GetRecipeCaseCount(); i++)
                    {
                        List<KeyValuePair<uint, uint>> itemList = curSelectedRecipeData.recipeData.GetNeedItems(i);
                        foreach (KeyValuePair<uint, uint> input in itemList)
                        {
                            uint perNeed = input.Value;
                            uint userItemCount = user_items.GetUserItemAmount(input.Key);
                            userItemCount = userItemCount - (perNeed * curSelectedRecipeData.craftMaxCount);
                            if (userItemCount < perNeed * diff)
                            {
                                uint needCount = (perNeed * diff) - userItemCount;
                                msg += "\n" + LocalizeData.GetText(string.Format(LocalizeData.GetText("재료상세"), items.GetItem(input.Key).GetItemName(), needCount));
                            }
                        }
                    }

                    NecoCanvas.GetPopupCanvas().OnToastPopupShow(msg);
                }
                break;
            case BUTTON_STATE.MINUS_10:
                craftTryCount = craftTryCount < 10 * (uint)repeat ? 0 : craftTryCount - (10 * (uint)repeat);
                break;
            case BUTTON_STATE.NONE:
            default:
                break;
        }

        if (craftTryCount >= curSelectedRecipeData.craftMaxCount)
        {
            craftTryCount = curSelectedRecipeData.craftMaxCount;
        }
        else if (craftTryCount <= 0)
        {
            craftTryCount = 0;
        }

        //if (count > maxTryCount)
        //{
        //    cookCountTextField.text = maxTryCount.ToString();
        //    count = maxTryCount;
        //}
        //else if (count <= 0)
        //{
        //    cookCountTextField.text = "0";
        //    count = 0;
        //}
        //else
        //{
        //    cookCountTextField.text = count.ToString();
        //}

        foreach (RecipeMaterialInfo info in curNeedMaterials)
        {
            info.UpdateTryCount(craftTryCount, userMoney, needMoney);
        }

        craftCountText.text = string.Format("{0}", craftTryCount);

        countCraftButton.image.color = craftTryCount > 0 ? originButtonColor : dimmedButtonColor;
    }

    void ClearMakingPanel()
    {
        ResetInputData();

        curSelectedRecipeData = null;

        craftToyButtonLayer.SetActive(false);
        craftMaterialtButtonLayer.SetActive(false);

        craftButton.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        craftButton.gameObject.GetComponent<RectTransform>().DORewind();
        craftButton.gameObject.GetComponent<RectTransform>().DOKill();

        countCraftButton.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        countCraftButton.gameObject.GetComponent<RectTransform>().DORewind();
        countCraftButton.gameObject.GetComponent<RectTransform>().DOKill();
    }

    void ResetInputData()
    {
        craftTryCount = 0;
        craftCountText.text = string.Format("{0}", craftTryCount);
        //cookCountTextField.text = "0";
    }

    public void OnStartCrafting()
    {
        if (curSelectedRecipeData.craftMaxCount <= 0 || craftTryCount <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_295"));
            return;
        }

        OnCrafting(curSelectedRecipeData.recipeData, craftTryCount, OnStartCraftSet);
    }

    public void OnStartCraftSet()
    {
        if (curSelectedRecipeData.recipeData.GetRecipeType() == "TOY")
        {
            items item = curSelectedRecipeData.itemData;
            if (item != null)
            {
                List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SPOT);
                if (necoData != null)
                {
                    foreach (neco_spot data in necoData)
                    {
                        if (data != null)
                        {
                            uint curObjectItem = objects.GetSpotItem(data.GetSpotID());
                            if (curObjectItem == item.GetItemID())
                            {
                                spot = data;                                
                                spot.SetUI(null);
                            }
                        }
                    }
                }
            }
        }
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
}
