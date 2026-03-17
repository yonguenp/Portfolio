using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{

    public class SubwayLayer : BuildingLayer
    {
        [SerializeField] GameObject constructParentLayer = null;  // 지하철 최상단 잠금 상태 최상단 레이어

        [SerializeField] SubwaySlot[] subwaySlots = null;

        [Header("construct condition layer")]
        [Space(10)]
        [SerializeField] GameObject constructLockLayer = null;   // 지하철 최상단 건설 조건 미충족 레이어
        [SerializeField] Text constructConditionLabel = null; // 건설 조건 표시 텍스트

        [Header("construct able layer")]
        [Space(10)]
        [SerializeField] GameObject constructAbleLayer = null;  //지하철 최상단 건설 가능 레이어
        [Header("construct finish layer")]
        [Space(10)]
        [SerializeField] GameObject constructFinishLayer = null; // 지하철 최상단 건설 완료 레이어

        [Header("constructing layer")]
        [Space(10)]
        [SerializeField] GameObject constructingLayer = null; // 지하철 최상단 건설 중 레이어
        [SerializeField] Text constructingTimerLabel = null; // 건설 중 나타나는 타이머 
        [SerializeField] TimeObject constructingTimeObj = null; // 건설 중 타이머 타임 오브젝트

        BuildInfo subwayBuildInfo = null;

        //private bool buildAvaiable = false;

        //private void OnEnable()
        //{
        //    // 팝업 오픈 시에는 지하철 동작 멈춤
        //    var subway = User.Instance.GetLandmarkData(eLandmarkType.SUBWAY) as LandmarkSubway;
        //    if (subway != null)
        //    {
        //        subway.TrainActionState = eTrainActionState.POPUP_HOLD;
        //    }
        //}

        //private void OnDisable()
        //{
        //    var subway = User.Instance.GetLandmarkData(eLandmarkType.SUBWAY) as LandmarkSubway;
        //    if (subway != null)
        //    {
        //        subway.TrainActionState = eTrainActionState.NONE;
        //    }
        //}

        public void RefreshAllSlot()
        {
            foreach (var slot in subwaySlots)
            {
                if (slot.LastState == LandmarkSubwayPlantState.READY)
                {
                    slot.RefreshItemSlot();
                }
            }
        }

        protected override void Init()
        {
            int TownLvToSubwayOpen = 0;
            var buildingData = BuildingOpenData.GetByInstallTag((int)eLandmarkType.SUBWAY);

            if (buildingData != null)
                TownLvToSubwayOpen = buildingData.OPEN_LEVEL;
            //buildAvaiable = User.Instance.ExteriorData.ExteriorLevel >= TownLvToSubwayOpen;
            constructConditionLabel.text = string.Format(StringData.GetStringByIndex(100000059), TownLvToSubwayOpen);

            if (subwaySlots != null)
            {
                foreach (var subwaySlot in subwaySlots)
                {
                    subwaySlot.AllLayerOff();
                }
            }

            Refresh();
        }

        protected override void Refresh()
        {
            if (constructParentLayer == null || constructLockLayer == null || constructAbleLayer == null || constructingLayer == null) return;

            subwayBuildInfo = User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.SUBWAY);

            if (subwayBuildInfo == null) return;

            switch (subwayBuildInfo.State)
            {
                case eBuildingState.LOCKED:
                    SetLockState();
                    break;
                case eBuildingState.NOT_BUILT:
                    SetNotBuiltState();
                    break;
                case eBuildingState.CONSTRUCTING:
                case eBuildingState.CONSTRUCT_FINISHED:
                    SetConstructingState();
                    break;
                case eBuildingState.NORMAL:
                    SetNormalState();
                    break;
            }

            constructParentLayer.SetActive(subwayBuildInfo.State != eBuildingState.NORMAL);
            constructLockLayer.SetActive(subwayBuildInfo.State == eBuildingState.LOCKED);
            constructAbleLayer.SetActive(subwayBuildInfo.State == eBuildingState.NOT_BUILT);
            constructingLayer.SetActive(subwayBuildInfo.State == eBuildingState.CONSTRUCTING);
            constructFinishLayer.SetActive(subwayBuildInfo.State == eBuildingState.CONSTRUCT_FINISHED);
        }

        protected override void Clear()
        {

        }

        protected override void SetLockState()
        {

        }

        protected override void SetNotBuiltState()
        {

        }

        protected override void SetConstructingState()
        {
            if (subwayBuildInfo.State == eBuildingState.CONSTRUCTING)
            {
                if (TimeManager.GetTimeCompare(subwayBuildInfo.ActiveTime) > 0) //건설중 타이머
                {
                    constructingTimeObj.Refresh = delegate
                    {
                        constructingTimerLabel.text = TimeManager.GetTimeCompareString(subwayBuildInfo.ActiveTime);
                        if (TimeManager.GetTimeCompare(subwayBuildInfo.ActiveTime) <= 0)  //건설 중 타이머 끝
                        {
                            constructingTimeObj.Refresh = null;
                            constructingLayer.SetActive(false);
                            constructFinishLayer.SetActive(true);
                        }
                    };
                }
                else
                {
                    constructingTimeObj.Refresh = null;
                    constructingLayer.SetActive(false);
                    constructFinishLayer.SetActive(true);
                }
            }
        }

        protected override void SetNormalState()
        {
            constructParentLayer.SetActive(false);

            if (subwaySlots != null)
            {
                for (int i = 0; i < subwaySlots.Length; ++i)
                {
                    if (subwaySlots[i] != null)
                    {
                        subwaySlots[i].Init(i);
                    }
                }
            }
        }

        public override void OnClickConstruct()
        {
            PopupManager.OpenPopup<BuildingConstructPopup>(Data);
        }

        public override void OnClickConstructFinish()
        {
            WWWForm param = new WWWForm();
            param.AddField("tag", (int)eLandmarkType.SUBWAY);
            NetworkManager.Send("building/complete", param, (JObject jsonData) =>
            {
                Refresh();
                Town.Instance.RefreshMap();
            });
        }

        public override void OnClickUpgrade()
        {

        }

        public void SetDeliveryButton(bool isOn)
        {
            if (subwaySlots != null)
            {
                foreach (var subwaySlot in subwaySlots)
                {
                    subwaySlot.SetButtonState(isOn);
                }
            }
        }

        public void OnClickLevelUpAccelerateButton()
        {
            var buidlingInstance = User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.SUBWAY);
            var buildlingInfo = BuildingOpenData.GetByInstallTag((int)eLandmarkType.SUBWAY);

            var buildingLevelInfo = BuildingLevelData.GetDataByGroupAndLevel(buildlingInfo.BUILDING, User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.SUBWAY).Level);
            AccelerationMainPopup.OpenPopup(eAccelerationType.CONSTRUCT, (int)eLandmarkType.SUBWAY, buildingLevelInfo.UPGRADE_TIME, buidlingInstance.ActiveTime, Refresh);
        }
    }

}