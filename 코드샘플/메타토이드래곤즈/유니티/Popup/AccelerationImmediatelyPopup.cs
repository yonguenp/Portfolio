using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AccelerationImmediatelyPopup : Popup<PopupData>
{
    public GameObject clonePrefab = null;
    public Text titleText = null;
    public Text subTitleText = null;

    public Slider timeRemainSlider = null;
    public Text timeRemainText = null;

    public ScrollRect ticketScrollRect = null;

    public Text fullDiaDescText = null;//가속권을 사용 안하는 팝업에서의 노티

    List<AccelerationClone> cloneList = new List<AccelerationClone>();
    List<ItemBaseData> ticketItemDataList = new List<ItemBaseData>();

    TimeObject timeObject = null;

    public delegate void AssetDelegate(Asset jsonData);
    AssetDelegate Callback = null;
    public int RemainEnd { get; private set; } = 0;
    public int FullTime { get; private set; } = 0;
    public int RemainTime { get { return TimeManager.GetTimeCompare(RemainEnd); } }
    public bool CheckAccState { set; get; } = true;
    #region OpenPopup
    public static AccelerationImmediatelyPopup OpenPopup(int remain, int fulltime, AssetDelegate completeAction = null)
    {
        AccelerationImmediatelyPopup popup = PopupManager.GetPopup<AccelerationImmediatelyPopup>();
        popup.SetData(remain, fulltime, completeAction);
        PopupManager.OpenPopup<AccelerationImmediatelyPopup>();
        return popup;
    }

    void SetData(int r, int f, AssetDelegate cb)
    {
        RemainEnd = r;
        FullTime = f;
        Callback = cb;
    }
    #endregion
    public override void InitUI()
    {
        InitPopupLayer();
    }

   
    public void OnClickCloseButton()
    {
        PopupManager.ClosePopup<AccelerationImmediatelyPopup>();
    }

    public override void ClosePopup()
    {
        if (timeObject != null)
            timeObject.Refresh = null;

        base.ClosePopup();
    }

    void InitPopupLayer()
    {
        if (TryGetComponent(out timeObject) == false)
        {
            timeObject = gameObject.AddComponent<TimeObject>();
        }

        ticketItemDataList = ItemBaseData.GetItemListByKind(eItemKind.ACC_TICKET);
        ticketItemDataList = ticketItemDataList.OrderBy(elemet => elemet.VALUE).ToList();

        SetSubTitle();
        SetSliderState();
        SetAccelerateCloneList();

        CheckAccState = true;
    }

    void RefreshUI()
    {
        if (timeObject != null && timeObject.Refresh != null)
            timeObject.Refresh();
    }

    void SetSubTitle()
    {
        if (titleText != null)
            titleText.text = StringData.GetStringByStrKey("가속권사용");

        subTitleText.text = StringData.GetStringByStrKey("필요대기시간");
    }

    void SetSliderState()
    {
        if (timeRemainSlider == null) { return; }

        if (timeObject != null)
        {
            timeObject.Refresh = delegate {

                timeRemainText.text = SBFunc.TimeString(RemainTime);

                timeRemainSlider.value = (FullTime - RemainTime) / (float)FullTime;

                foreach(var clone in cloneList)
                {
                    clone.Refresh();
                }

                CheckAccState = true;
                if (RemainTime <= 0)
                {
                    timeObject.Refresh = null;

                    ClosePopup();

                    //PopupManager.ForceUpdate<MainPopup>();
                    PopupManager.ClosePopup<AccelerationMainPopup>();

                    CheckAccState = false;
                }
            };
        }
    }

    public void OnAcceleration(Asset asset)
    {
        Callback.Invoke(asset);
    }

    void SetAccelerateCloneList()
    {
        if (ticketScrollRect == null) { return; }
        if (ticketItemDataList == null || ticketItemDataList.Count <= 0) { return; }

        // 초기화
        cloneList.Clear();
        SBFunc.RemoveAllChildrens(ticketScrollRect.content);
        clonePrefab.SetActive(true);
        // 첫번째 재화 사용 클론 우선 처리
        GameObject cashClone = Instantiate(clonePrefab);
        cashClone.transform.SetParent(ticketScrollRect.content.transform);
        cashClone.transform.localScale = Vector3.one;

        string cashType = GameConfigTable.GetConfigValue("ACCELERATION_CASH_TYPE");
        eGoodType resultCashType = SBFunc.GetGoodType(cashType);

        AccelerationClone cashCloneComponent = cashClone.GetComponent<AccelerationClone>();
        cashCloneComponent?.Init(this, resultCashType);

        cloneList.Add(cashCloneComponent);

        fullDiaDescText?.gameObject.SetActive(false);

        // 나머지 가속권 클론 처리
        foreach (ItemBaseData itemData in ticketItemDataList)
        {
            GameObject newClone = Instantiate(clonePrefab);
            newClone.transform.SetParent(ticketScrollRect.content.transform);
            newClone.transform.localScale = Vector3.one;

            AccelerationClone component = newClone.GetComponent<AccelerationClone>();
            component?.Init(this, eGoodType.ITEM, itemData);

            cloneList.Add(component);
        }

        clonePrefab.SetActive(false);
    }
}
