using Coffee.UIEffects;
using SandboxNetwork;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BuildingPopup : Popup<BuildingPopupData>
{

}


public abstract class BuildingZoomPopup : BuildingPopup, IPointerDownHandler, IPointerUpHandler, EventListener<BuildingUIEvent>
{
    [SerializeField]
    protected GameObject bgMid = null;

    public List<int> BuildingTags { get; protected set; } = null;

    private int curTabIndex = 0;

    private Vector2 touchPoint = Vector2.negativeInfinity;
    private float swipeValue = Screen.width * 0.25f;

    protected List<SkeletonGraphic> buildingSkeletons = null;
    protected abstract void SetBuildingTags();

    protected virtual int CurIndex()
    {
        return curTabIndex;
    }
    protected virtual void SetIndex(int index)
    {
        curTabIndex = index;
    }
    private void OnEnable()
    {
        EventManager.AddListener(this);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener(this);
    }
    public override void InitUI()
    {
        UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);

        Town.Instance.ZoomBuilding(Data.BuildingTag,null, Data.BuildingKey != "exp_battery");

        UICanvas.Instance.StartBackgroundBlurEffect();

        SetBuildingTags();
    }

    public bool OnChangeTargetBulding(int tag)
    {
        if (Town.Instance.GetBuilding(tag) != null)
        {
            Town.Instance.ZoomBuilding(tag, () =>
            {
                ForceUpdate(new BuildingPopupData(tag));
            }, Data.BuildingKey != "exp_battery");

            return true;
        }

        return false;
    }

    public override void ClosePopup()
    {
        base.ClosePopup();

        UICanvas.Instance.EndBackgroundBlurEffect();
        
        Town.Instance?.ZoomBackToLastestView(0.3f);
        
        UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);

        // 레드닷 갱신처리 - 액션마다 처리하면 오히려 너무 잦아지므로 일단은 팝업 닫힐 때 갱신처리
        UIManager.Instance.MainPopupUI.RequestUpdateProductReddot();
    }

    public override void ForceUpdate(BuildingPopupData data)
    {
        base.DataRefresh(data);
        Refresh();
    }

    public virtual void OnClickMoveTab(int move)
    {
        if (move == 0)
            return;

        if (BuildingTags == null)
        {
            Debug.LogError("need to BuildingTags!");
            return;
        }

        int count = BuildingTags.Count;
        if (count > 1)
        {
            curTabIndex = (curTabIndex + move + count) % count;
            int tag = BuildingTags[curTabIndex];
            
            if (!OnChangeTargetBulding(tag))
            {
                OnClickMoveTab(1);
            }
        }
    }

    public virtual void Refresh()
    {
        RefreshSpineBg();
    }

    public void RefreshSpineBg()
    {
        if (Data == null)
            return;

        var buildingObj = Town.Instance.GetBuilding(Data.BuildingTag);
        if (buildingObj != null && bgMid != null)
            bgMid.SetActive(buildingObj.Cell > TownMap.X && buildingObj.Cell < (TownMap.Width - 1));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        touchPoint = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (touchPoint != Vector2.negativeInfinity)
        {
            float diff = eventData.position.x - touchPoint.x;
            if (diff < swipeValue * -1.0f)
            {
                OnClickMoveTab(1);
            }
            else if (diff > swipeValue)
            {
                OnClickMoveTab(-1);
            }

            touchPoint = Vector2.negativeInfinity;
        }
    }
    protected virtual void SetCurrentBuildingSpine(int tabIndex){}
    protected virtual void SetCurrentBuildingSpine(int tag, int curTapNubmer){}
    protected virtual void RefreshCurrentBuildingSpine(bool animStart = false) { }

    protected SkeletonGraphic GetSpine(int _index)
    {
        if (buildingSkeletons == null || buildingSkeletons.Count <= 0 || buildingSkeletons.Count <= _index)
            return null;

        return buildingSkeletons[_index];
    }

    protected virtual Building GetTownBuilding(int _buildingTag)
    {
        switch (_buildingTag)
        {
            case (int)eLandmarkType.Dozer:
                return Town.Instance.dozer;
            case (int)eLandmarkType.Travel:
                return Town.Instance.travel;
            default:
                return Town.Instance.GetBuilding(_buildingTag);
        }
    }
    public virtual void OnEvent(BuildingUIEvent eventType)
    {
        switch (eventType.Event)
        {
            case BuildingUIEvent.eBuildingUIEventEnum.RefreshSpine:
                RefreshCurrentBuildingSpine();
                break;
            default:
                break;
        }
    }
}