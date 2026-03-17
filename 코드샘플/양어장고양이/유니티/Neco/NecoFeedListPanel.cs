using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NecoFeedListPanel : NecoAnimatePopup
{
    [Header("[Food Info]")]
    public Image foodIcon;
    public Text foodCountText;
    public Text foodNameText;
    public GameObject NoFoodUI;

    [Header("[Food Info List]")]
    public GameObject feedScrollContainer;
    public GameObject feedCloneObject;

    [Header("[Button Layers]")]
    public Button FeedButton;
    public Color originFeedButtonColor;
    public Color DimmedFeedButtonColor;

    public Toggle autoDispenserToggle;

    [Header("[Contents Info]")]
    public GameObject contentsInfoPopup;
    public Text contentsInfoText;

    neco_spot curSpotData = null;
    FoodData curSelectedFoodData = null;
    List<FoodData> foodDataList = new List<FoodData>();

    public override void OnAnimateDone()
    {
        
    }

    private void OnEnable()
    {
        base.OnEnable();
        GetComponent<AudioSource>().enabled = PlayerPrefs.GetInt("Setting_SFX", 1) == 1;
    }

    public void OnClickCloseButton()
    {
        // 프롤로그 체크
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_254"));
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이완료후밥그릇강조)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_400"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose();
    }

    public void OnClickFeedButton()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        if (curSelectedFoodData == null && foodDataList.Count > 0) 
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_255"));
            return; 
        }
        else if (curSelectedFoodData == null && foodDataList.Count <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_256"));
            return;
        }



        NecoCanvas.GetPopupCanvas().OnFeedConfirmPopupShow(curSelectedFoodData, curSpotData);
    }

    public void OnClickContentsInfoButton()
    {
        if (contentsInfoPopup == null) { return; }

        contentsInfoPopup.SetActive(!contentsInfoPopup.activeSelf);

        if (contentsInfoPopup.activeSelf)
        {
            if (contentsInfoText != null)
            {
#if UNITY_IOS
                contentsInfoText.text = LocalizeData.GetText("밥그릇설명문_FORIOS");
#else
                contentsInfoText.text = LocalizeData.GetText("밥그릇설명문");
#endif
            }
        }
    }

    public void SetFeedDataUI(neco_spot spotData)
    {
        if (spotData == null) { return; }

        curSpotData = spotData;                         // 현재 맵에 배치된 spot data

        InitFeedData();
        SetFeedUI();

        feedScrollContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
    }

    public void RefreshFeedData()
    {
        InitFeedData();
        SetFeedUI();
    }

    public void UpdateCurrentSelectedFood(FoodData curFoodData)
    {
        curSelectedFoodData = curFoodData;

        foreach (Transform child in feedScrollContainer.transform)
        {
            child.gameObject.GetComponent<FoodInfo>().selectStokeObject.SetActive(false);
        }
    }

    public FoodData GetCurrentSelectedFood()
    {
        return curSelectedFoodData;
    }

    void InitFeedData()
    {
        foodDataList.Clear();

        List<game_data> userItemList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);

        foodDataList = new List<FoodData>();
        
        object obj;
        if (userItemList != null)
        {
            foreach (user_items userItem in userItemList)
            {
                if (userItem.GetAmount() > 0)
                {
                    items item = items.GetItem(userItem.GetItemID());
                    if (item != null && item.GetItemType() == "FOOD")
                    {
                        // 요리 지속 시간 계산
                        uint foodDuration = neco_food.GetFoodDuration(userItem.GetItemID());
                        uint minute = foodDuration / 60;
                        uint second = foodDuration % 60;

                        // 밥그릇 관련 데이터 세팅
                        FoodData foodData = new FoodData();
                        foodData.itemData = item;
                        foodData.recipeData = null;
                        foodData.foodIcon = foodData.itemData.GetItemIcon();
                        foodData.foodName = foodData.itemData.GetItemName();
                        foodData.foodDesc = foodData.itemData.GetItemDesc();
                        foodData.foodCount = 0;

                        if (0 < second)
                        {
                            foodData.foodDuration = string.Format(LocalizeData.GetText("LOCALIZE_247"), minute, second);
                        }
                        else
                        {
                            foodData.foodDuration = string.Format(LocalizeData.GetText("LOCALIZE_257"), minute);
                        }

                        foodData.foodCount = userItem.GetAmount();

                        foodDataList.Add(foodData);
                    }
                }
            }
        }

        if (user_items.GetUserItemAmount(129) > 0)
        {
            autoDispenserToggle.gameObject.SetActive(true);

            autoDispenserToggle.interactable = false;
            autoDispenserToggle.isOn = PlayerPrefs.GetInt("AUTO_DISPENSER", 0) > 0;
            SetAutoDispenser();
            autoDispenserToggle.interactable = true;
        }
        else
        {
            autoDispenserToggle.gameObject.SetActive(false);
        }
    }

    void SetFeedUI()
    {
        // 보유한 먹이가 없을 경우 설정
        NoFoodUI.SetActive(foodDataList.Count == 0);
        FeedButton.image.color = foodDataList.Count > 0 ? originFeedButtonColor : DimmedFeedButtonColor;

        feedCloneObject.SetActive(false);

        if (feedScrollContainer == null || feedCloneObject == null) { return; }

        foreach (Transform child in feedScrollContainer.transform)
        {
            if (child.gameObject != feedCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        if (foodDataList == null || foodDataList.Count <= 0) { return; }

        feedCloneObject.SetActive(true);

        List<GameObject> foodUI = new List<GameObject>();

        // 밥그릇 리스트 정렬
        foodDataList = foodDataList.OrderBy(x => x.itemData.GetItemID()).ToList();
        
        foreach (FoodData foodData in foodDataList)
        {
            GameObject foodInfoUI = Instantiate(feedCloneObject);
            foodInfoUI.transform.SetParent(feedScrollContainer.transform);
            foodInfoUI.transform.localScale = feedCloneObject.transform.localScale;
            foodInfoUI.transform.localPosition = feedCloneObject.transform.localPosition;

            foodInfoUI.GetComponent<FoodInfo>().SetFoodInfoData(foodData, this);
            foodUI.Add(foodInfoUI);
        }

        feedCloneObject.SetActive(false);

        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
        if (seq >= neco_data.PrologueSeq.첫밥그릇등장 && seq <= neco_data.PrologueSeq.첫밥주기완료)
        {
            foreach(GameObject ui in foodUI)
            {
                Sequence mySequence = DOTween.Sequence();
                
                mySequence.Append(ui.transform.DOLocalRotate(new Vector3(0, 0, -5), 0.1f, RotateMode.Fast));
                mySequence.Append(ui.transform.DOLocalRotate(new Vector3(0, 0, 5), 0.1f, RotateMode.Fast));
                mySequence.Append(ui.transform.DOLocalRotate(new Vector3(0, 0, -5), 0.1f, RotateMode.Fast));
                mySequence.Append(ui.transform.DOLocalRotate(new Vector3(0, 0, 5), 0.1f, RotateMode.Fast));
                mySequence.Append(ui.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.1f, RotateMode.Fast));
                mySequence.Append(ui.transform.DOLocalRotate(new Vector3(0, 0, 0), 2.0f, RotateMode.Fast));
                mySequence.SetLoops(-1);
            }
        }
    }

    private void OnDisable()
    {
        foodDataList.Clear();
        curSelectedFoodData = null;

        contentsInfoPopup.SetActive(false);
    }

    bool CheckPrologue()
    {
        return neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.첫밥그릇등장;
    }

    public void SetAutoDispenser(bool message = false)
    {
        if(message)
        {
            message = autoDispenserToggle.interactable;
        }

        if(curSpotData == null)
        {
            return;
        }

        neco_map map = curSpotData.GetCurMapData();
        if (map == null)
        {
            return;
        }

        string reason = LocalizeData.GetText("LOCALIZE_258");
        if (user_items.GetUserItemAmount(129) > 0)
        {
            if (autoDispenserToggle.isOn)
            {
                if (message)
                {
                    NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_259"));
                }
                PlayerPrefs.SetInt("AUTO_DISPENSER", 1);
                NecoCanvas.GetGameCanvas().GetCurMapController()?.RefreshFoodTruck();
                autoDispenserToggle.transform.Find("Button").GetComponent<Image>().color = new Color(0.7568628f, 0.9411765f, 0.3607843f, 1.0f);
                return;
            }
        }
        else
        {
            reason = LocalizeData.GetText("LOCALIZE_261");
        }

        if (message)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(reason);
        }

        autoDispenserToggle.interactable = false;
        autoDispenserToggle.isOn = false;
        PlayerPrefs.SetInt("AUTO_DISPENSER", 0);
        autoDispenserToggle.interactable = true;

        autoDispenserToggle.transform.Find("Button").GetComponent<Image>().color = new Color(0.6f, 0.6352941f, 0.6509804f, 1.0f);
    }

    bool CheckPrologueWithToastAlarm()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
            case neco_data.PrologueSeq.통발UI등장:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_274"));
                return true;
            case neco_data.PrologueSeq.고양이10번터치가이드퀘스트:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_214"));
                return true;
            case neco_data.PrologueSeq.배틀패스강조및대사:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("패틀패스강조"));
                return true;
        }

        return false;
    }
}
