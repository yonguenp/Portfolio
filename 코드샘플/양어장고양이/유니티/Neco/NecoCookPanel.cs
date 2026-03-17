using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoCookPanel : CraftingUI
{
    enum BUTTON_STATE
    {
        NONE,
        ADD,
        MINUS,
        MINUS_10,
        ADD_10,
    }

    public NecoCookListPanel cookListPanel;

    [Header("[Mid Recipe Mid_UI Info]")]
    public Image recipeIcon;
    public Text recipeNameText;
    public Text recipeCountText;
    public Text recipeDurationText;

    [Header("[Mid Recipe Bot_UI Info]")]
    public GameObject recipeMaterialLayer;
    public GameObject recipeMaterialCloneObject;
    //public InputField cookCountTextField;

    [Header("[BOT_UI Info]")]
    public GameObject craftingStartButton;
    public GameObject craftingStartButton_Dimmed;
    public Text cookCountText;

    [Header("[Button Layer]")]
    public Button cookButton;
    public Color originButtonColor;
    public Color dimmedButtonColor;

    [Header("[Layout List]")]
    public RectTransform layoutRect;

    FoodData curSelectedFoodData = null;

    private uint cookTryCount = 0;
    private List<RecipeMaterialInfo> curNeedMaterials = new List<RecipeMaterialInfo>();

    uint userMoney;
    uint needMoney;

    #region UI CONTROL
    public void OnClickCookButton()
    {
        // 프롤로그 체크
        if (CheckPrologue() && curSelectedFoodData.recipeData.GetRecipeID() != 1)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_245"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이강조 && curSelectedFoodData.recipeData.GetRecipeID() != 3)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
            return;
        }

        OnStartCook();
    }

    public void OnClickPopupCloseButton()
    {
        // 프롤로그 체크
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_245"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이강조 && curSelectedFoodData.recipeData.GetRecipeID() == 3)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_COOK_POPUP);
    }

    public void OnClickMinus10Button(int repeat)
    {
        // 프롤로그 체크
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_245"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이강조 && curSelectedFoodData.recipeData.GetRecipeID() == 3)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
            return;
        }

        CountButtonChecker(BUTTON_STATE.MINUS_10, repeat);
        ButtonChecker();
    }

    public void OnClickPlus10Button(int repeat)
    {
        // 프롤로그 체크
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_245"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이강조 && curSelectedFoodData.recipeData.GetRecipeID() == 3)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
            return;
        }

        CountButtonChecker(BUTTON_STATE.ADD_10, repeat);
        ButtonChecker();
    }

    public void OnClickPlusButton(int repeat)
    {
        // 프롤로그 체크
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_245"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이강조 && curSelectedFoodData.recipeData.GetRecipeID() == 3)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
            return;
        }

        CountButtonChecker(BUTTON_STATE.ADD, repeat);
        ButtonChecker();
    }

    public void OnClickMinusButton(int repeat)
    {
        // 프롤로그 체크
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_245"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이강조 && curSelectedFoodData.recipeData.GetRecipeID() == 3)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
            return;
        }

        CountButtonChecker(BUTTON_STATE.MINUS, repeat);
        ButtonChecker();
    }
    #endregion

    public void SetRecipeListUI(FoodData curSelectedData)
    {
        if (curSelectedData == null)
        {
            return;
        }

        ClearCookPanel();

        curSelectedFoodData = curSelectedData;
        ResetInputData();

        // 최대 1개이상 제작 가능하면 기본 제작 갯수 1추가
        if (curSelectedFoodData.foodMaxCount > 0)
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

        cookButton.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;

        // 프롤로그 체크
        if (CheckPrologue() && curSelectedFoodData.recipeData.GetRecipeID() == 1)
        {
            cookButton.gameObject.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이강조 && curSelectedFoodData.recipeData.GetRecipeID() == 3)
        {
            cookButton.gameObject.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public override void CreftingDone()
    {
        //RewardData rewardData = new RewardData();

        //rewardData.sprite = curSelectedFoodData.foodIcon;
        //rewardData.name = curSelectedFoodData.foodName;
        //rewardData.count = cookTryCount;

        //NecoCanvas.GetPopupCanvas().OnSingleRewardPopup("요리 완료" ,"요리를 완료하였습니다.", rewardData, Refresh);

        NecoCanvas.GetPopupCanvas().OnSuccessEffectPopupShow(EFFECT_TYPE.NEW_RECIPE_RESULT, curSelectedFoodData.recipeData.GetRecipeID(), cookTryCount, EffectDone);

        //Refresh();        
    }

    public void EffectDone()
    {
        MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
        if (mapController != null)
        {
            //mapController.SendMessage("OnCookResult", SendMessageOptions.DontRequireReceiver);

            if (curSelectedFoodData.recipeData.GetRecipeID() == 2)
            {
                mapController.SendMessage("OnCookPompano", SendMessageOptions.DontRequireReceiver);
            }
            
            if (curSelectedFoodData.recipeData.GetRecipeID() == 9)
            {
                mapController.SendMessage("OnNewCat", 7, SendMessageOptions.DontRequireReceiver);
                //mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.배스구이제작, SendMessageOptions.DontRequireReceiver);
            }
        }

        Refresh();
    }

    public override void Refresh()
    {
        if (cookListPanel == null) { return; }

        //ClearCookPanel();

        ResetInputData();
        curSelectedFoodData = cookListPanel.RefreshByCookUI(curSelectedFoodData);
        SetRecipeListUI(curSelectedFoodData);
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
        recipeIcon.sprite = curSelectedFoodData.foodIcon;
        recipeNameText.text = curSelectedFoodData.foodName;
        recipeCountText.text = LocalizeData.GetText("LOCALIZE_46") + curSelectedFoodData.foodCount.ToString("n0");
        recipeDurationText.text = curSelectedFoodData.foodDuration;
    }

    void SetMaterialListUI()
    {
        if (curSelectedFoodData == null)
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

        if (curSelectedFoodData.recipeData.GetRecipeCaseCount() == 0)
        {
            recipeMaterialCloneObject.SetActive(false);
            return;
        }

        // 골드 재료 정보 세팅
        object obj;
        if (curSelectedFoodData.recipeData.data.TryGetValue("need_gold", out obj))
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

        // 일반 재료 세팅
        for (int i = 0; i < curSelectedFoodData.recipeData.GetRecipeCaseCount(); i++)
        {
            List<KeyValuePair<uint, uint>> itemList = curSelectedFoodData.recipeData.GetNeedItems(i);

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
        if (cookButton == null) { return; }

        // 제작 진행 버튼
        cookButton.image.color = cookTryCount > 0 ? originButtonColor : dimmedButtonColor;
    }

    void CountButtonChecker(BUTTON_STATE buttonState, int repeat = 1)
    {
        //uint count = uint.Parse(cookCountTextField.text);

        switch (buttonState)
        {
            case BUTTON_STATE.ADD:
                cookTryCount += (uint)repeat;
                break;
            case BUTTON_STATE.MINUS:
                if (cookTryCount == 0) break;   // uint 예외처리
                if (cookTryCount < (uint)repeat)
                    cookTryCount = 0;
                else
                    cookTryCount -= (uint)repeat;
                break;
            case BUTTON_STATE.ADD_10:
                if (cookTryCount == curSelectedFoodData.foodMaxCount)
                    return;

                cookTryCount += 10 * (uint)repeat;
                if (cookTryCount >= curSelectedFoodData.foodMaxCount)
                {
                    uint diff = (cookTryCount - curSelectedFoodData.foodMaxCount) % 10;
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

                    
                    if (curSelectedFoodData.recipeData.data.TryGetValue("need_gold", out obj))
                    {
                        uint perGold = (uint)obj;
                        userGold = userGold - (perGold * curSelectedFoodData.foodMaxCount);
                        if (userGold < perGold * diff)
                        {
                            uint needGold = (perGold * diff) - userGold;
                            msg += "\n" + LocalizeData.GetText(string.Format(LocalizeData.GetText("재화상세"), LocalizeData.GetText("LOCALIZE_229"), needGold));
                        }
                    }

                    for (int i = 0; i < curSelectedFoodData.recipeData.GetRecipeCaseCount(); i++)
                    {
                        List<KeyValuePair<uint, uint>> itemList = curSelectedFoodData.recipeData.GetNeedItems(i);
                        foreach (KeyValuePair<uint, uint> input in itemList)
                        {
                            uint perNeed = input.Value;                                                        
                            uint userItemCount = user_items.GetUserItemAmount(input.Key);
                            userItemCount = userItemCount - (perNeed * curSelectedFoodData.foodMaxCount);
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
                cookTryCount = cookTryCount < 10 * (uint)repeat ? 0 : cookTryCount - (10 * (uint)repeat);
                break;
            case BUTTON_STATE.NONE:
            default:
                break;
        }

        if (cookTryCount >= curSelectedFoodData.foodMaxCount)
        {               
            cookTryCount = curSelectedFoodData.foodMaxCount;
        }
        else if (cookTryCount <= 0)
        {
            cookTryCount = 0;
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
            info.UpdateTryCount(cookTryCount, userMoney, needMoney);
        }

        cookCountText.text = string.Format("{0}", cookTryCount);
    }

    void ClearCookPanel()
    {
        ResetInputData();

        curSelectedFoodData = null;

        cookButton.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        cookButton.gameObject.GetComponent<RectTransform>().DORewind();
        cookButton.gameObject.GetComponent<RectTransform>().DOKill();
    }

    void ResetInputData()
    {
        cookTryCount = 0;
        cookCountText.text = string.Format("{0}", cookTryCount);
        //cookCountTextField.text = "0";
    }

    public void OnStartCook()
    {
        if (curSelectedFoodData.foodMaxCount <= 0 || cookTryCount <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_248"));
            return;
        }

        OnCrafting(curSelectedFoodData.recipeData, cookTryCount);
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

    bool CheckPrologue()
    {
        return neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.조리대UI등장;
    }
}
