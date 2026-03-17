using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class AccelerationTutorialPopup : Popup<AccelerationMainData>
    {

        public Text titleText = null;
        public Text subTitleText = null;

        public Slider timeRemainSlider = null;
        public Text timeRemainText = null;

        public ScrollRect ticketScrollRect = null;

        public Text fullDiaDescText = null;//가속권을 사용 안하는 팝업에서의 노티

        [SerializeField]
        List<AccelerationMainClone> cloneList = new List<AccelerationMainClone>();
        List<ItemBaseData> ticketItemDataList = new List<ItemBaseData>();

        TimeObject timeObject = null;

        int curAccelItemKey = 0;
        int recipeItemKey = 0;

        Vector2Int buildingPos = Vector2Int.zero;
        int buildingTag = 0;


        public override void InitUI()
        {
            InitPopupLayer();
        }
        public void SetRecipeId(int itemID)
        {
            recipeItemKey = itemID;
        }

        public void SetBuildingInfo(int tag, Vector2Int pos)
        {
            buildingTag = tag;
            buildingPos = pos;
        }

        private void OnEnable()
        {
            PopupTopUIRefreshEvent.Hide();
        }
        private void OnDisable()
        {
            PopupTopUIRefreshEvent.Show();
            if (timeObject != null)
                timeObject.Refresh = null;
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
                    if (Data.isFull)
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

            timeRemainText.text = SBFunc.TimeString(Data.accMainTime);
            timeRemainSlider.value = 0 / (float)Data.accMainTime;
        }

        void SetAccelerateCloneList()
        {
            if (ticketScrollRect == null) { return; }
            if (ticketItemDataList == null || ticketItemDataList.Count <= 0) { return; }

            // 첫번째 재화 사용 클론 우선 처리

            string cashType = GameConfigTable.GetConfigValue("ACCELERATION_CASH_TYPE");
            eGoodType resultCashType = SBFunc.GetGoodType(cashType);

            cloneList[0]?.Init(null, resultCashType, Data);
            var isFullGemstoneType = Data.accelerateType == eAccelerationType.JOB && Data.isFull;//즉시 구매 타입인가
            fullDiaDescText?.gameObject.SetActive(isFullGemstoneType);

            if (isFullGemstoneType)
                return;

            // 가속권 클론 처리
            if (ticketItemDataList.Count > 0)
            {
                ItemBaseData itemData = ticketItemDataList[0];
                curAccelItemKey = itemData.KEY;
                cloneList[1]?.Init(null, eGoodType.ITEM, Data, itemData);
            }

        }

        public void OnClickAccel()
        {
            if (Data.accelerateType == eAccelerationType.JOB)
            {
                WWWForm param = new WWWForm();
                param.AddField("tag", Data.accMainTag);
                param.AddField("recipe", recipeItemKey);
                param.AddField("all", 0);
                NetworkManager.Send("produce/enqueue", param, (jsonData) =>
                {
                    //PopupManager.ForceUpdate<MainPopup>();
                    if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                    {
                        switch (jsonData["rs"].ToObject<int>())
                        {
                            case (int)eApiResCode.OK:
                                SendAccel();
                                break;
                        }
                    }
                });
            }
            else if (Data.accelerateType == eAccelerationType.CONSTRUCT)
            {
                WWWForm paramData = new WWWForm();
                paramData.AddField("tag", buildingTag);
                paramData.AddField("x", buildingPos.x);
                paramData.AddField("y", buildingPos.y);
                NetworkManager.Send("building/construct", paramData, ((jsonData) =>
                {
                    if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                    {
                        switch (jsonData["rs"].ToObject<int>())
                        {
                            case (int)eApiResCode.OK:
                                SendAccel();
                                break;
                        }
                    }
                }));
            }
        }

        void SendAccel()
        {
            WWWForm paramData = new WWWForm();
            paramData.AddField("type", (int)Data.accelerateType);
            paramData.AddField("tag", Data.accMainTag);
            paramData.AddField("item", curAccelItemKey);
            paramData.AddField("count", 1);
            NetworkManager.Send("building/haste", paramData, (jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].ToObject<int>())
                    {
                        case (int)eApiResCode.OK:
                            if(Data.accelerateType == eAccelerationType.CONSTRUCT)
                            {
                                Town.Instance.RefreshMap();
                                Town.Instance.SetConstructModeState((bool)false);
                                UIManager.Instance.RefreshCurrentUI();
                            }
                            Data.timeCompleteAction?.Invoke();
                            TutorialManager.tutorialManagement.NextTutorialStart();
                            ClosePopup();
                            break;
                    }
                }
            });
        }
    }
}