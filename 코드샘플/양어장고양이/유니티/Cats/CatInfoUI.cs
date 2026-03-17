using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatInfoUI : MonoBehaviour
{
    public enum CAT_SUB_UI
    { 
        CAT_UNKNOWN = -1,
        CAT_FEEDING,
        CAT_ACTIONS,
    };

    private CAT_SUB_UI curSubUI = CAT_SUB_UI.CAT_UNKNOWN;
    private cat_def curCatData = null;
    private CatSkeletonGraphic curSkeleton = null;

    public FarmCanvas FarmCanvas;
    public CatInfoStatus CatInfoStatus;
    public CatInfoSub_Actions CatInfoSub_Actions;
    public CatInfoSub_Feeding CatInfoSub_Feeding;
    public Toggle ToggleActions;
    public Toggle ToggleFeeding;

    private void Awake()
    {
        ToggleActions.onValueChanged.AddListener((bool enabled) => { if (enabled) OnSubUIOpen(CAT_SUB_UI.CAT_ACTIONS); });
        ToggleFeeding.onValueChanged.AddListener((bool enabled) => { if (enabled) OnSubUIOpen(CAT_SUB_UI.CAT_FEEDING); });
    }

    private void OnEnable()
    {
        FarmCanvas.FarmUIPanel.SetUI(FarmUIPanel.UI_STATE.UI_POPUP_OPENING);
        FarmCanvas.MapScroller.enabled = false;
    }

    private void OnDisable()
    {
        FarmCanvas.FarmUIPanel.SetUI(FarmUIPanel.UI_STATE.UI_NORMAL);
        FarmCanvas.MapScroller.enabled = true;
        FarmCanvas.MapScroller.transform.localPosition = Vector2.zero;
        (FarmCanvas.MapScroller.transform as RectTransform).offsetMin = Vector2.zero;
        (FarmCanvas.MapScroller.transform as RectTransform).offsetMax = Vector2.zero;
        FarmCanvas.WorldCatManager.transform.localScale = Vector2.one;        
    }

    public void OnUIOpen(CatSkeletonGraphic catSkeleton, CAT_SUB_UI subUI = CAT_SUB_UI.CAT_ACTIONS)
    {
        curCatData = catSkeleton.curCatData;
        curSkeleton = catSkeleton;

        gameObject.SetActive(true);
        OnSubUIOpen(subUI);
    }

    private void Update()
    {
        if (curSkeleton == null)
            return;

        Vector2 localPos = curSkeleton.cursor.transform.localPosition;        
        localPos.y += (((FarmCanvas.MapScroller.transform as RectTransform).rect.size.y / 2) - ((curSkeleton.cursor.transform as RectTransform).sizeDelta.y * 2.0f * (2.5f * 0.5f)));
        
        FarmCanvas.MapScroller.gameObject.transform.localPosition = (localPos * -1.0f);
        FarmCanvas.WorldCatManager.transform.localScale = Vector2.one * 2.5f;
    }

    public void OnSubUIOpen(CAT_SUB_UI subUI)
    {
        curSubUI = subUI;

        CatInfoStatus.Refresh(curCatData);

        CatInfoSub_Actions.gameObject.SetActive(curSubUI == CAT_SUB_UI.CAT_ACTIONS);
        CatInfoSub_Feeding.gameObject.SetActive(curSubUI == CAT_SUB_UI.CAT_FEEDING);

        ToggleActions.isOn = curSubUI == CAT_SUB_UI.CAT_ACTIONS;
        ToggleActions.interactable = !ToggleActions.isOn;
        ToggleFeeding.isOn = curSubUI == CAT_SUB_UI.CAT_FEEDING;
        ToggleFeeding.interactable = !ToggleFeeding.isOn;

        switch (curSubUI)
        {
            case CAT_SUB_UI.CAT_ACTIONS:
                CatInfoSub_Actions.InitActionList(curCatData, this);
                break;
            case CAT_SUB_UI.CAT_FEEDING:
                CatInfoSub_Feeding.InitFeedList(curCatData);
                break;
        }
    }

    public void OnFoodSelect(user_items item)
    {
        CatInfoStatus.OnFoodSelect(item);
    }

    public void OnFeed(user_items item)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "cat");
        data.AddField("op", 5);
        data.AddField("cat", curCatData.GetCatID().ToString());
        data.AddField("food", item.GetItemID().ToString());

        NetworkManager.GetInstance().SendApiRequest("cat", 5, data, (response) =>
        {
            if (!FarmCanvas.CheckErrorResponse(response))
                return;

            FarmCanvas.CloseCatInfoUI();
            curSkeleton.OnStartEatAnimation(10.0f);            
        });
    }
}
