using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class AccelerationMainPopup : Popup<AccelerationMainData>
    {
        public GameObject clonePrefab = null;
        public Text titleText = null;
        public Text subTitleText = null;

        public Slider timeRemainSlider = null;
        public Text timeRemainText = null;

        public ScrollRect ticketScrollRect = null;

        public Text fullDiaDescText = null;//가속권을 사용 안하는 팝업에서의 노티

        List<AccelerationMainClone> cloneList = new List<AccelerationMainClone>();
        List<ItemBaseData> ticketItemDataList = new List<ItemBaseData>();

        TimeObject timeObject = null;

        AccelerationMainClone updateCashCloneData = null;   // 실시간으로 갱신할 클론
        AccelerationMainClone updateCloneData = null;   // 실시간으로 갱신할 클론

        public bool CheckAccState { set; get; } = true;
        #region OpenPopup
        public static AccelerationMainPopup OpenPopup(eAccelerationType eType, BuildingPopupData data, VoidDelegate completeAction = null)
        {
            if (data == null || data.BuildInfo == null)
                return null;

            return OpenPopup(eType, data.BuildInfo.Tag, data.LevelData.UPGRADE_TIME, data.BuildInfo.ActiveTime, completeAction);
        }
        public static AccelerationMainPopup OpenPopup(eAccelerationType eType, int tag, int time, int endTime, VoidDelegate completeAction = null, VoidDelegate reduceAction = null)
        {
            if (eType == eAccelerationType.NONE)
                return null;

            return OpenPopup(new AccelerationMainData(eType, tag, time, endTime, completeAction));
        }
        public static AccelerationMainPopup OpenPopup(eAccelerationType eType, int tag, int time, int endTime, int frame, VoidDelegate completeAction = null, VoidDelegate reduceAction=null, bool _isFull = false)
        {
            if (eType == eAccelerationType.NONE)
                return null;

            return OpenPopup(new AccelerationMainData(eType, tag, time, endTime, frame, completeAction, reduceAction, _isFull));
        }
        public static AccelerationMainPopup OpenPopup(AccelerationMainData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<AccelerationMainPopup>(data);
        }
        #endregion
        public override void InitUI()
        {
            InitPopupLayer();
        }

        public override void ForceUpdate(AccelerationMainData data)
        {
            base.DataRefresh(data);
            RefreshUI();
            if(Data != null)
            {
                Data.timeReductAction?.Invoke();
            }
            
        }

        // 다른 클론의 모든 버블 레이어를 닫음
        public void CloseAllBubble()
        {
            cloneList?.ForEach(element => element.CloseBubbleLayer());
            updateCloneData = null;
        }

        public void RegistCloneUpdate(AccelerationMainClone updateClone)
        {
            updateCloneData = updateClone;
        }

        public void OnClickCloseButton()
        {
            PopupManager.ClosePopup<AccelerationMainPopup>();
        }

        public override void ClosePopup()
        {
            if (timeObject != null)
                timeObject.Refresh = null;

            base.ClosePopup();
        }

        void InitPopupLayer()
        {
            if (Data == null) { return; }

            if (TryGetComponent(out timeObject) == false)
            {
                timeObject = gameObject.AddComponent<TimeObject>();
            }

            updateCashCloneData = null;
            updateCloneData = null;

            ticketItemDataList = ItemBaseData.GetItemListByKind(eItemKind.ACC_TICKET);
            ticketItemDataList = ticketItemDataList.OrderBy(elemet => elemet.VALUE).ToList();

            SetSubTitle();
            SetSliderState();
            SetAccelerateCloneList();

            CheckAccState = true;
        }

        void RefreshUI()
        {
            if (Data == null) { return; }

            UpdateEndTime();
            if (timeObject != null && timeObject.Refresh != null)
                timeObject.Refresh();
        }

        void SetSubTitle()
        {
            if (titleText != null)
                titleText.text = StringData.GetStringByStrKey("가속권사용");

            switch (Data.accelerateType)
            {
                case eAccelerationType.CONSTRUCT:
                    subTitleText.text = StringData.GetStringByStrKey("building_construction_progress");
                    break;
                case eAccelerationType.LEVELUP:
                    subTitleText.text = StringData.GetStringByStrKey("town_level_text_02");
                    break;
                case eAccelerationType.JOB:
                    if(Data.isFull)
                    {
                        if (titleText != null)
                            titleText.text = StringData.GetStringByStrKey("전체가속권");

                        string subStr;
                        var buildingOpenData = BuildingOpenData.GetByInstallTag(Data.accMainTag);
                        if (buildingOpenData != null)
                        {
                            var buildingName = StringData.GetStringByStrKey(buildingOpenData.BaseData._NAME);
                            subStr = StringData.GetStringFormatByStrKey("즉시생산완료건물이름", buildingName);
                        }
                        else
                            subStr = StringData.GetStringByStrKey("building_production_progress");

                        subTitleText.text = subStr;
                    }
                    else
                    {
                        subTitleText.text = StringData.GetStringByStrKey("building_production_progress");
                        if (Data.accMainTag == (int)eLandmarkType.Travel)
                            subTitleText.text = StringData.GetStringByStrKey("여행중");
                        else if (Data.accMainTag == (int)eLandmarkType.SUBWAY)
                            subTitleText.text = StringData.GetStringByStrKey("열차가속팝업문구");
                    }
                    break;
                case eAccelerationType.EXCHANGE:
                    subTitleText.text = StringData.GetStringByStrKey("소원나무가속팝업문구");
                    break;
            }
        }

        void SetSliderState()
        {
            if (timeRemainSlider == null) { return; }

            if (timeObject != null)
            {
                timeObject.Refresh = delegate {

                    int remainTime = TimeManager.GetTimeCompare(Data.accMainEndTime);
                    timeRemainText.text = SBFunc.TimeString(remainTime);

                    timeRemainSlider.value = (Data.accMainTime - remainTime) / (float)Data.accMainTime;
                    
                    updateCloneData?.Refresh();
                    updateCashCloneData?.Refresh();

                    CheckAccState = true;
                    if (remainTime <= 0)
                    {
                        timeObject.Refresh = null;
                        updateCloneData = null;
                        updateCashCloneData = null;

                        Data.timeCompleteAction?.Invoke();
                        Data.timeCompleteAction = null;

                        //PopupManager.ForceUpdate<MainPopup>();
                        PopupManager.ClosePopup<AccelerationMainPopup>();

                        CheckAccState = false;
                    }
                };
            }
        }

        void SetAccelerateCloneList()
        {
            if (ticketScrollRect == null) { return; }
            if (ticketItemDataList == null || ticketItemDataList.Count <= 0) { return; }

            // 초기화
            cloneList.Clear();
            SBFunc.RemoveAllChildrens(ticketScrollRect.content);

            // 첫번째 재화 사용 클론 우선 처리
            GameObject cashClone = Instantiate(clonePrefab);
            cashClone.transform.SetParent(ticketScrollRect.content.transform);
            cashClone.transform.localScale = Vector3.one;

            string cashType = GameConfigTable.GetConfigValue("ACCELERATION_CASH_TYPE");
            eGoodType resultCashType = SBFunc.GetGoodType(cashType);

            AccelerationMainClone cashCloneComponent = cashClone.GetComponent<AccelerationMainClone>();
            cashCloneComponent?.Init(this, resultCashType, Data);

            updateCashCloneData = cashCloneComponent;

            cloneList.Add(cashCloneComponent);

            var isFullGemstoneType = Data.accelerateType == eAccelerationType.JOB && Data.isFull;//즉시 구매 타입인가
            fullDiaDescText?.gameObject.SetActive(isFullGemstoneType);
            
            if (isFullGemstoneType)
                return;

            // 나머지 가속권 클론 처리
            foreach (ItemBaseData itemData in ticketItemDataList)
            {
                GameObject newClone = Instantiate(clonePrefab);
                newClone.transform.SetParent(ticketScrollRect.content.transform);
                newClone.transform.localScale = Vector3.one;

                AccelerationMainClone component = newClone.GetComponent<AccelerationMainClone>();
                component?.Init(this, eGoodType.ITEM, Data, itemData);

                cloneList.Add(component);
            }
        }

        // 시간에 대한 변경이 발생할 경우 남은 시간 갱신
        void UpdateEndTime()
        {
            switch (Data.accelerateType)
            {
                case eAccelerationType.CONSTRUCT:

                    BuildInfo constructUpdateData = User.Instance.GetUserBuildingInfoByTag(Data.accMainTag);
                    if (constructUpdateData != null)
                    {
                        Data.accMainEndTime = constructUpdateData.State == eBuildingState.CONSTRUCT_FINISHED ? 0 : constructUpdateData.ActiveTime;
                    }

                    break;
                case eAccelerationType.LEVELUP:

                    if (Data.accMainTag == 1)   // 외형 개별 처리
                    {
                        Data.accMainEndTime = User.Instance.ExteriorData.ExteriorState == eBuildingState.CONSTRUCT_FINISHED ? 0 : User.Instance.ExteriorData.ExteriorTime;
                    }
                    else  // 나머지 건물에 대한 처리
                    {
                        BuildInfo levelUpUpdateData = User.Instance.GetUserBuildingInfoByTag(Data.accMainTag);
                        if (levelUpUpdateData != null)
                        {
                            Data.accMainEndTime = levelUpUpdateData.ActiveTime;
                        }
                    }

                    break;
                case eAccelerationType.JOB:

                    if (Data.accMainTag == (int)eLandmarkType.Travel)
                    {
                        var landmarkData = User.Instance.GetLandmarkData<LandmarkTravel>();
                        if (landmarkData != null)
                        {
                            int remainTime = TimeManager.GetTimeCompare(landmarkData.TravelTime);
                            Data.accMainEndTime = remainTime <= 0 ? 0 : landmarkData.TravelTime;
                        }
                    }
                    else if (Data.accMainTag == (int)eLandmarkType.SUBWAY)
                    {
                        var landmarkData = User.Instance.GetLandmarkData<LandmarkSubway>();
                        if (landmarkData != null)
                        {
                            int remainTime = TimeManager.GetTimeCompare(landmarkData.PlatsData[Data.platform-1].Expire);
                            Data.accMainEndTime = remainTime <= 0 ? 0 : landmarkData.PlatsData[Data.platform-1].Expire;
                        }
                    }
                    else
                    {
                        if(Data.isFull)
                            Data.accMainEndTime = 0;
                        else
                        {
                            // 일반 생산 아이템 가속 처리
                            ProducesBuilding jobUpdataData = User.Instance.GetProduces(Data.accMainTag);
                            if (jobUpdataData != null && jobUpdataData.Items != null && jobUpdataData.Items.Count > 0)
                            {
                                if (Data.frameIndex >= 0)
                                {
                                    Data.accMainEndTime = 0;
                                    var currentProduct = jobUpdataData.Items[Data.frameIndex];
                                    if (currentProduct != null)
                                    {
                                        Data.accMainEndTime = currentProduct.ProductionExp;
                                    }
                                }
                            }
                        }
                    }

                    break;
                case eAccelerationType.EXCHANGE:
                    var data = User.Instance.Exchange.Exchange[Data.accMainTag - 1];
                    if (data != null)
                    {
                        Data.accMainEndTime = TimeManager.GetTimeStamp(data.regist_time);
                    }
                    break;
            }
        }
    }
}