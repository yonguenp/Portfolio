using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public struct DragonHideEvent
    {
        private static DragonHideEvent obj;

        public static void Send()
        {
            EventManager.TriggerEvent(obj);
        }
    }

    public struct DragonShowEvent
    {
        private static DragonShowEvent obj;

        public static void Send()
        {
            EventManager.TriggerEvent(obj);
        }
    }

    public struct TravelDepartureEvent
    {
        private static TravelDepartureEvent obj;

        public static void Send()
        {
            EventManager.TriggerEvent(obj);
        }
    }
    public class TravelLayer : BuildingLayer, EventListener<DragonShowEvent>, EventListener<TravelDepartureEvent>
    {
        [SerializeField] Image mapBGSprite = null;
        [SerializeField] Sprite[] worldSpriteList = null;
        [SerializeField] GameObject worldMapAlerter = null;
        [SerializeField] Transform[] FlipBoardUpTrans = null;
        [SerializeField] Transform[] FlipBoardLowerTrans = null;
        [SerializeField] GameObject BtnUpgrade = null;
        [SerializeField] GameObject BtnRestrictedHardArea = null;
        [SerializeField] GameObject BtnRestrictedHellArea = null;

        [Header("Layer")]
        [SerializeField] GameObject NormalObject;
        [SerializeField] RestrictedAreaLayer RestrictedAreaLayer;

        [Header("LayerObject")]
        [SerializeField] Text mapTitleLabel = null;
        [SerializeField] GameObject dragonListSlotNode = null;
        [SerializeField] Text dragonListSlotLabel = null;
        [SerializeField] Text constructLayerTimerLabel = null;
        [SerializeField] ScrollRect mainRewardScroll = null;
        [SerializeField] GameObject detailRewardObject = null;
        [SerializeField] TableViewGrid addRewardTableView = null;
        [SerializeField] RectTransform addRewardContent = null;
        [SerializeField] ScrollRect detailRewardScroll = null;
        [SerializeField] BattleDragonListView DragonList = null;
        [SerializeField] Text DragonListCountLabel = null;
        [SerializeField] GameObject dragonPortraitFrame = null;
        [SerializeField] WorldTravelWorldSelectLayer worldSelectLayer = null;
        [SerializeField] Text needEnegyLabel = null;
        [SerializeField] GameObject onTravelNode = null;
        [SerializeField] GameObject characterBlockNode = null;
        [SerializeField] GameObject rewardBlockNode = null;
        [SerializeField] GameObject travelStartButtonNode = null;
        [SerializeField] GameObject travelStartOffButtonNode = null;
        
        [SerializeField] GameObject NormalRewardPanel = null;
        [SerializeField] GameObject RestrictedRewardPanel = null;
        [SerializeField] RectTransform restrictedRewardContent = null;

        [Header("Start Construct")]
        [Space(10)]
        [SerializeField] GameObject normalBody = null;

        [Header("Start Construct")]
        [Space(10)]
        [SerializeField] GameObject startConstructLayer = null;
        [SerializeField] Text startConstructGuideLabel = null;

        [Header("Progressing Construct")]
        [Space(10)]
        [SerializeField] GameObject progressingConstructLayer = null;
        [SerializeField] Text progressingConstructTimberLabel = null;

        [Header("Finish Construct")]
        [Space(10)]
        [SerializeField] GameObject finishConstructLayer = null;

        [SerializeField] LandMarkPopup landMarkTempPopup = null;

        //testUI - 시안용
        [SerializeField] TravelWorldSelect testWorldSelectUI = null;
        //testUI
        [SerializeField] DOTweenAnimation animObj = null;

        int curWorld = 1;
        int uiWorld = -1;
        int MAX_SELECT_COUNT = 5;

        bool isTableViewFirstInit = false;

        List<Dictionary<string, GameObject>> characterSlots = null;
        List<UserDragon> pickDragons = null;// 실제 선택된 드래곤
        List<int> prePickedDragons = null;// 드래곤 선택창에서 선택된 드래곤
        LandmarkTravel travel = null;
        LandmarkTravel Travel
        {
            get
            {
                if (travel == null)
                    travel = User.Instance.GetLandmarkData<LandmarkTravel>();
                return travel;
            }
        }


        protected void OnDisable()
        {
            PopupManager.Instance.Top.SetStaminaUI(false);
            EventManager.RemoveListener<TravelDepartureEvent>(this);
            EventManager.RemoveListener<DragonShowEvent>(this);
        }

        private void OnEnable()
        {
            PopupManager.Instance.Top.SetStaminaUI(true);
            EventManager.AddListener<TravelDepartureEvent>(this);
            EventManager.AddListener<DragonShowEvent>(this);
        }

        public override bool IsCloseAble()
        {
            if (NormalObject.activeInHierarchy)
                return true;

            return false;
        }

        public override void CloseAction()
        {
            if(!RestrictedAreaLayer.IsCloseAble())
            {
                return;
            }
            var worldData = TravelData.GetByWorldID(curWorld);
            SetRewardPopup(worldData);

            NormalObject.SetActive(true);
            RestrictedAreaLayer.Hide();
        }
        protected override void Clear()
        {
            DragonList.OnHideList();
        }

        protected override void Init()
        {
            NormalObject.SetActive(true);
            RestrictedAreaLayer.Hide();

            eLandmarkType = eLandmarkType.Travel;
            if (characterSlots == null)
            {
                characterSlots = new List<Dictionary<string, GameObject>>();
            }
            characterSlots.Clear();


            if (pickDragons == null)
            {
                pickDragons = new List<UserDragon>();
            }
            pickDragons.Clear();


            if (prePickedDragons == null)
            {
                prePickedDragons = new List<int>();
            }
            prePickedDragons.Clear();

            if (Travel != null)
            {
                if (Travel.TravelState == eTravelState.Travel)
                {
                    var travelDragonList = Travel.TravelDragon;

                    for (var i = 0; i < travelDragonList.Count; ++i)
                    {
                        if (travelDragonList[i] == null) { return; }

                        pickDragons.Add(travelDragonList[i]);
                        prePickedDragons.Add(pickDragons[i].Tag);
                    }
                }
            }
            curWorld = Travel.TravelWorld <= 0 ? 1 : Travel.TravelWorld;
            if (dragonListSlotNode != null)
            {
                for (var i = 1; i <= 5; i++)
                {
                    var curSlot = SBFunc.GetChildrensByName(dragonListSlotNode.transform, new string[] { i.ToString() }).gameObject;
                    if (curSlot != null)
                    {
                        var select = SBFunc.GetChildrensByName(curSlot.transform, new string[] { "Select" }).gameObject;
                        var dimd = SBFunc.GetChildrensByName(curSlot.transform, new string[] { "dimd" }).gameObject;
                        var plus = SBFunc.GetChildrensByName(curSlot.transform, new string[] { "plus" }).gameObject;

                        if (select != null)
                        {
                            select.SetActive(false);
                            var container = SBFunc.GetChildrensByName(select.transform, new string[] { "character" }).gameObject;

                            if (container != null && dimd != null)
                            {
                                Dictionary<string, GameObject> tempDic = new Dictionary<string, GameObject>
                                {
                                    { "slot", curSlot },
                                    { "target", select },
                                    { "container", container },
                                    { "dimd", dimd },
                                    { "plus", plus }
                                };

                                characterSlots.Add(tempDic);
                            }
                        }
                    }
                }
            }

            if (mainRewardScroll != null)
                SBFunc.RemoveAllChildrens(mainRewardScroll.content.transform);

            if (detailRewardScroll != null)
                SBFunc.RemoveAllChildrens(detailRewardScroll.content.transform);

            timeObject = GetComponent<TimeObject>();

            if (addRewardTableView != null && !isTableViewFirstInit)
            {
                addRewardTableView.OnStart();
                isTableViewFirstInit = true;
            }
            RefreshUI();

            if (TutorialManager.tutorialManagement.IsFinishedTutorial(TutorialDefine.Travel) == false)
            {
                TutorialManager.tutorialManagement.StartTutorial((int)TutorialDefine.Travel);
            }
        }

        void ClearLayer()
        {
            progressingConstructLayer.SetActive(false);
            finishConstructLayer.SetActive(false);
            onTravelNode.SetActive(false);
            characterBlockNode.SetActive(false);
            rewardBlockNode.SetActive(false);
            DragonList.OnHideList();
        }

        void SetLayerState()
        {
            switch (Data.BuildInfo.State)
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
                default:
                    SetNormalState();
                    break;
            }
        }

        void TravelStartRefresh()
        {
            ClearLayer();
            travelStartButtonNode.SetActive(true);
            travelStartOffButtonNode.SetActive(false);
            SetDragon();
            normalBody.SetActive(Data.BuildInfo.State == eBuildingState.NORMAL);
            SetLayerState();
            travelStartButtonNode.GetComponent<Button>()?.SetButtonSpriteState(false);
            SetBubbleNodeEffect(false);
            worldSelectLayer.gameObject.SetActive(false);
        }

        protected override void Refresh()
        {

            ClearLayer();

            var worldData = TravelData.GetByWorldID(curWorld);
            if (worldData != null)
            {
                if (needEnegyLabel != null)
                {
                    needEnegyLabel.text = SBFunc.StrBuilder("-", worldData.COST_STAMINA);
                }

                MAX_SELECT_COUNT = worldData.CHAR_NUM;

                if (uiWorld != curWorld)
                {
                    if (mainRewardScroll != null)
                        SetWorldDefaultRewardScroll(worldData, mainRewardScroll);
                    
                    SetRewardPopup(worldData);
                }
            }

            // 맵 데이터 세팅
            mapTitleLabel.text = StringData.GetStringByStrKey(worldData._NAME);
            SetMapBGSprite(curWorld);

            var curCount = pickDragons.Count;

            if (dragonListSlotLabel != null)
            {
                dragonListSlotLabel.text = SBFunc.StrBuilder(curCount, "/", MAX_SELECT_COUNT);
            }

            SetDragon();

            travelStartButtonNode.SetActive(true);
            travelStartOffButtonNode.SetActive(false);
            normalBody.SetActive(Data.BuildInfo.State == eBuildingState.NORMAL);

            SetLayerState();

            // 여행 가능 버튼 상태 갱신
            if (travelStartButtonNode != null)
            {
                bool disableTravel = GetPickDragonTagList().Count < MAX_SELECT_COUNT || User.Instance.UserData.Energy < worldData.COST_STAMINA || !IsAvailableWorld();
                travelStartButtonNode.GetComponent<Button>()?.SetButtonSpriteState(!disableTravel);
                SetBubbleNodeEffect(!disableTravel);
            }

            worldSelectLayer.gameObject.SetActive(false);
        }

        public void SetRewardPopup(TravelData worldData)
        {
            NormalRewardPanel.SetActive(true);
            RestrictedRewardPanel.SetActive(false);

            if (detailRewardScroll != null)
                SetWorldDefaultRewardScroll(worldData, detailRewardScroll);

            SBFunc.RemoveAllChildrens(addRewardContent);

            if (worldData.REWARD_BONUS > 0)
            {
                var itemGroup = ItemGroupData.Get(worldData.REWARD_BONUS.ToString());
                //테이블 뷰 방식이 아닌 코드
                foreach (var item in itemGroup)
                {
                    if (itemGroup != null)
                    {
                        var newItem = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), addRewardContent);
                        var frame = newItem.GetComponent<ItemFrame>();
                        var reward = item.Reward;
                        switch (reward.GoodType)
                        {
                            case eGoodType.GOLD:
                            {
                                frame.setFrameCashInfo((int)eGoodType.GOLD, reward.Amount);
                            }
                            break;
                            case eGoodType.ARENA_TICKET:
                            {
                                frame.setFrameCashInfo((int)eGoodType.ARENA_TICKET, reward.Amount);
                            }
                            break;
                            case eGoodType.ENERGY:
                            {
                                frame.setFrameEnergyInfo(reward.Amount);
                            }
                            break;
                            case eGoodType.ITEM:
                            case eGoodType.EQUIPMENT:
                            {
                                frame.SetFrameItemInfo(reward.ItemNo, reward.Amount);
                            }
                            break;
                        }
                    }
                }
            }
        }

        public void SetRestrictedRewardPopup(Dictionary<eGoodType, Dictionary<int, List<int>>> attached)
        {
            NormalRewardPanel.SetActive(false);
            RestrictedRewardPanel.SetActive(true);

            SBFunc.RemoveAllChildrens(restrictedRewardContent);

            //테이블 뷰 방식이 아닌 코드            
            foreach (var key in attached.Keys)
            {
                foreach (var no in attached[key].Keys)
                {
                    int min = attached[key][no].Min();
                    int max = attached[key][no].Max();

                    if (min == 0 && max == 0)
                        continue;

                    var r = new Asset(key, no, max);
                    var newItem = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), restrictedRewardContent);
                    if (newItem != null)
                    {
                        var frame = newItem.GetComponent<ItemFrame>();
                        if (frame != null)
                        {
                            frame.SetFrameItem(r);
                            if (min != max)
                            {
                                frame.SetMinMaxText(min, max, true);
                            }
                        }
                    }
                }
            }
        }
        bool IsAvailableWorld()
        {
            var selectWorldStages = StageManager.Instance.AdventureProgressData.GetWorldStages(curWorld);
            return StageManager.Instance.AdventureProgressData.IsClearedStage(curWorld, selectWorldStages.Count);
        }

        void SetWorldDefaultRewardScroll(TravelData _worldData, ScrollRect _targetRect)
        {
            SBFunc.RemoveAllChildrens(_targetRect.content.transform);

            if (_worldData.REWARD_GOLD > 0)
            {
                MakeItemPrefab(true, _worldData.REWARD_GOLD, 0, _targetRect);
            }
            if (_worldData.REWARD_ACCOUNT_EXP > 0)
            {
                MakeItemPrefab(false, _worldData.REWARD_ACCOUNT_EXP, 10000003, _targetRect);
            }
            if (_worldData.REWARD_CHAR_EXP > 0)
            {
                MakeItemPrefab(false, (int)(_worldData.REWARD_CHAR_EXP * ServerOptionData.GetFloat("dragon_exp", 1.0f)), 10000004, _targetRect);
            }
        }

        void MakeItemPrefab(bool _isGoldType, int _amount, int _itemNo, ScrollRect _targetRect)
        {
            var newItem = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), _targetRect.content);
            if (newItem != null)
            {
                var frame = newItem.GetComponent<ItemFrame>();
                if (frame != null)
                {
                    if (_isGoldType)
                        frame.setFrameCashInfo((int)eGoodType.GOLD, _amount);
                    else
                        frame.SetFrameItem(_itemNo, _amount);
                }
            }
        }

        public void OnClickShowDetailReward()
        {
            if (detailRewardObject != null)
                detailRewardObject.SetActive(true);
        }

        public void OnClickHideDetailReward()
        {
            if (detailRewardObject != null)
                detailRewardObject.SetActive(false);
        }

        protected override void SetNormalState()
        {
            if (Travel.BuildInfo.Level <= 1)
            {
                BtnUpgrade.SetActive(true);
                BtnRestrictedHardArea.SetActive(false);
                BtnRestrictedHellArea.SetActive(false);
            }
            else
            {
                BtnUpgrade.SetActive(false);
                BtnRestrictedHardArea.SetActive(true);
                BtnRestrictedHellArea.SetActive(true);
            }

            if (Travel != null)
            {
                switch (Travel.TravelState)
                {
                    case eTravelState.Normal:
                    {
                        onTravelNode.SetActive(false);
                        characterBlockNode.SetActive(false);
                        rewardBlockNode.SetActive(false);
                        travelStartButtonNode.SetActive(true);
                        travelStartOffButtonNode.SetActive(false);
                        if (constructLayerTimerLabel != null)
                        {
                            constructLayerTimerLabel.text = SBFunc.TimeString(-1);
                            var worldData = TravelData.GetByWorldID(curWorld);
                            if (worldData != null)
                            {
                                constructLayerTimerLabel.text = SBFunc.TimeString(worldData.TIME);
                            }
                        }
                    }
                    break;
                    case eTravelState.Complete:
                    {
                        onTravelNode.SetActive(true);
                        characterBlockNode.SetActive(true);
                        rewardBlockNode.SetActive(true);
                        travelStartButtonNode.SetActive(false);
                        travelStartOffButtonNode.SetActive(true);

                        if (gameObject.activeInHierarchy)
                            OnFinish();
                    }
                    break;
                    case eTravelState.Travel:
                    {
                        onTravelNode.SetActive(true);
                        characterBlockNode.SetActive(true);
                        rewardBlockNode.SetActive(true);
                        travelStartButtonNode.SetActive(false);
                        travelStartOffButtonNode.SetActive(true);

                        if (timeObject != null && timeObject.Refresh == null)
                        {
                            timeObject.Refresh = () =>
                            {
                                if (Travel != null)
                                {
                                    var time = TimeManager.GetTimeCompare(Travel.TravelTime);
                                    if (time < 0)
                                    {
                                        timeObject.Refresh = null;
                                        Travel.SetTravelState(eTravelState.Complete);
                                        RefreshUI();
                                    }
                                    else
                                    {
                                        if (constructLayerTimerLabel != null)
                                        {
                                            constructLayerTimerLabel.text = SBFunc.TimeString(time);
                                        }
                                    }
                                }
                            };
                        }
                    }
                    break;
                }
            }
        }

        protected override void SetNotBuiltState()
        {
            startConstructLayer.SetActive(true);
            startConstructGuideLabel.gameObject.SetActive(true);
            BtnUpgrade.SetActive(false);
            BtnRestrictedHardArea.SetActive(false);
            BtnRestrictedHellArea.SetActive(false);
            SetUpgradeBtnState(eUpgradeButtonState.ConstructAble);
        }

        protected override void SetLockState()
        {
            startConstructLayer.SetActive(true);
            startConstructGuideLabel.gameObject.SetActive(true);
            startConstructGuideLabel.text = StringData.GetStringByIndex(100001242);
            BtnUpgrade.SetActive(false);
            BtnRestrictedHardArea.SetActive(false);
            BtnRestrictedHellArea.SetActive(false);
            SetUpgradeBtnState(eUpgradeButtonState.LOCK);
        }

        protected override void SetConstructingState()
        {
            BtnUpgrade.SetActive(false);
            BtnRestrictedHardArea.SetActive(false);
            BtnRestrictedHellArea.SetActive(false);

            if (TimeManager.GetTimeCompare(Data.BuildInfo.ActiveTime) > 0 && Data.BuildInfo.State != eBuildingState.CONSTRUCT_FINISHED)
            {
                SetUpgradeBtnState(eUpgradeButtonState.Constructing);
                timeObject = GetComponent<TimeObject>();
                timeObject.Time = TimeManager.GetTime();
                timeObject.Refresh = () =>
                {
                    var time = TimeManager.GetTimeCompare(Data.BuildInfo.ActiveTime);
                    progressingConstructTimberLabel.text = SBFunc.TimeString(time);
                    if (time <= 0)
                    {
                        SetUpgradeBtnState(eUpgradeButtonState.ConstructFinish);
                        timeObject.Refresh = null;
                        progressingConstructLayer.SetActive(false);
                        finishConstructLayer.SetActive(false);
                        landMarkTempPopup.ClosePopup();
                    }
                };
                timeObject.Refresh();
                progressingConstructLayer.SetActive(true);
            }
            //else
            //{
            //    SetUpgradeBtnState(eUpgradeButtonState.ConstructFinish);
            //    OnClickConstructFinish();
            //}

            travelStartButtonNode.SetActive(false);
            travelStartOffButtonNode.SetActive(false);
        }

        void SetDragon()
        {
            if (pickDragons == null)
            {
                return;
            }

            for (var i = 0; i < 5; ++i)
            {
                SetSlot(i, null);
            }
            for (var i = 0; i < pickDragons.Count; i++)
            {
                SetSlot(i, pickDragons[i]);
            }


        }

        void RefreshDragonListLabel()
        {
            if (DragonListCountLabel != null)
            {
                var currentCount = prePickedDragons.Count;
                var maxSize = MAX_SELECT_COUNT;

                DragonListCountLabel.text = SBFunc.StrBuilder(currentCount, "/", maxSize);
            }

            if (dragonListSlotLabel != null)
            {
                var currentCount = GetPickDragonTagList().Count;
                var maxSize = MAX_SELECT_COUNT;

                dragonListSlotLabel.text = SBFunc.StrBuilder(currentCount, "/", maxSize);
            }
        }

        List<int> GetPickDragonTagList()
        {
            var dragonTagList = new List<int>();
            if (pickDragons == null || pickDragons.Count <= 0)
            {
                return dragonTagList;
            }

            for (var i = 0; i < pickDragons.Count; ++i)
            {
                dragonTagList.Add(pickDragons[i].Tag);
            }

            return dragonTagList;
        }

        void RegistDragonListCallback()
        {
            DragonList.SetRegistCallBack((param) =>
            {

                //SBLog('regist param : ' + param);
                var tag = int.Parse(param);
                OnClickRegistTeam(tag);

                RefreshDragonListLabel();
            });
            DragonList.SetReleaseCallback((param) =>
            {

                //SBLog('release param : ' + param);
                var tag = int.Parse(param);
                onClickReleaseTeam(tag);

                RefreshDragonListLabel();
            });
        }

        //현재 여행 팀원 추가
        void OnClickRegistTeam(int dragonTag)
        {
            if (prePickedDragons.Count >= MAX_SELECT_COUNT)
            {
                return;
            }

            prePickedDragons.Add(dragonTag);
            DragonList.RefreshList(prePickedDragons.ToArray());
            DragonList.RefreshDragonCountLabel(prePickedDragons.Count, MAX_SELECT_COUNT);

            RefreshDragonListLabel();
        }

        //현재 여행 팀원 해제
        void onClickReleaseTeam(int dragonTag)
        {
            if (prePickedDragons.Count <= 0)
            {
                return;
            }

            var dragonIndex = prePickedDragons.IndexOf(dragonTag);
            if (dragonIndex > -1)
            {
                prePickedDragons.RemoveAt(dragonIndex);
            }
            DragonList.RefreshList(prePickedDragons.ToArray());
            DragonList.RefreshDragonCountLabel(prePickedDragons.Count, MAX_SELECT_COUNT);

            RefreshDragonListLabel();
        }

        public void OnClickHaste()
        {
            var worldData = TravelData.GetByWorldID(Travel.TravelWorld);

            if (Travel == null || worldData == null)
            {
                return;
            }

            AccelerationMainPopup.OpenPopup(eAccelerationType.JOB, (int)eLandmarkType.Travel, worldData.TIME, Travel.TravelTime, OnAcceleration);
        }

        private void OnAcceleration()
        {
            if (Travel != null)
            {
                var time = TimeManager.GetTimeCompare(Travel.TravelTime);
                if (time < 0)
                {
                    timeObject.Refresh = null;
                    Travel.SetTravelState(eTravelState.Complete);
                    SetNormalState();
                    landMarkTempPopup.SetBuildingSpine("play");
                }
            }
        }

        public override void OnClickConstruct()
        {
            // 건설 가능상태 예외처리
            if (StageManager.Instance.AdventureProgressData.GetLatestWorld() < 2)
            {
                ToastManager.On(100001242);
                return;
            }

            // 타운 외형 레벨 예외처리
            if (User.Instance.ExteriorData.ExteriorLevel < Data.OpenData.OPEN_LEVEL)
            {
                ToastManager.On(100000059, Data.OpenData.OPEN_LEVEL);
                return;
            }

            PopupManager.OpenPopup<BuildingConstructPopup>(Data);
        }

        public override void OnClickConstructFinish()
        {
            WWWForm param = new WWWForm();
            param.AddField("tag", ((int)eLandmarkType.Travel).ToString());

            NetworkManager.Send("building/complete", param, (jsonData) =>
            {
                PopupManager.ForceUpdate<LandMarkPopup>();

                BuildingCompletePopup.OpenPopup(false);
                Town.Instance.RefreshMap();
            });
        }

        public override void OnClickUpgrade()
        {
            var worldInfo = StageManager.Instance.AdventureProgressData.GetWorldInfoData(1, (int)StageDifficult.HARD);
            if (worldInfo == null)
            {
                ToastManager.On(StringData.GetStringByStrKey("제한구역제한안내"));
                return;
            }

            if (!StageManager.Instance.AdventureProgressData.IsClearedStage(1, worldInfo.Stages.Count, (int)StageDifficult.HARD))
            {
                ToastManager.On(StringData.GetStringByStrKey("제한구역제한안내"));
                return;
            }

            if (Data.LevelData.NEED_AREA_LEVEL > User.Instance.TownInfo.AreaLevel)
            {
                ToastManager.On(StringData.GetStringByStrKey("제한구역타운레벨제한안내"));
            }

            var popup = PopupManager.OpenPopup<BuildingUpgradePopup>(new BuildingPopupData(Data.BuildingTag));
            popup.SetUpgradeCallBack(()=> { landMarkTempPopup.ClosePopup(); });
        }

        public void OnClickRestrictedHardArea()
        {
            var worldInfo = StageManager.Instance.AdventureProgressData.GetWorldInfoData(1, (int)StageDifficult.HARD);
            if (worldInfo == null)
            {
                ToastManager.On(StringData.GetStringByStrKey("제한구역제한안내"));
                return;
            }

            if (!StageManager.Instance.AdventureProgressData.IsClearedStage(1, worldInfo.Stages.Count, (int)StageDifficult.HARD))
            {
                ToastManager.On(StringData.GetStringByStrKey("제한구역제한안내"));
                return;
            }

            NormalObject.SetActive(false);
            RestrictedAreaLayer.Show(StageDifficult.HARD);
        }

        public void OnClickRestrictedHellArea()
        {
            var worldInfo = StageManager.Instance.AdventureProgressData.GetWorldInfoData(1, (int)StageDifficult.HELL);
            if (worldInfo == null)
            {
                ToastManager.On(StringData.GetStringByStrKey("제한구역제한안내"));
                return;
            }

            if (!StageManager.Instance.AdventureProgressData.IsClearedStage(1, worldInfo.Stages.Count, (int)StageDifficult.HELL))
            {
                ToastManager.On(StringData.GetStringByStrKey("제한구역제한안내"));
                return;
            }

            NormalObject.SetActive(false);
            RestrictedAreaLayer.Show(StageDifficult.HELL);
        }

        public void OnRetrunToRestrictedArea(int world)
        {
            NormalObject.SetActive(false);
            RestrictedAreaLayer.Show(StageDifficult.NONE, world);
        }

        public void OnClickPick()
        {
            //현재 소유한 드래곤이 없으면 팀설정 못하게 막음
            if (User.Instance.DragonData.GetAllUserDragons().Count == 0)
            {
                //var text = TableManager.GetTable(StringTable.Name).Get(100000393).TEXT;
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000623));
                return;
            }

            if (DragonList != null)
            {
                prePickedDragons = GetPickDragonTagList();

                DragonList.OnShowList();
                RegistDragonListCallback();
                DragonList.Init(prePickedDragons.ToArray());
                RefreshDragonListLabel();
                DragonList.RefreshDragonCountLabel(prePickedDragons.Count, MAX_SELECT_COUNT);

            }
        }

        public void OnClickAuto()
        {
            //현재 소유한 드래곤이 없으면 팀설정 못하게 막음
            if (User.Instance.DragonData.GetAllUserDragons().Count == 0)
            {
                //var text = TableManager.GetTable(StringTable.Name).Get(100000393).TEXT;
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000623));
                return;
            }

            var dragonData = User.Instance.DragonData;
            if (dragonData != null)
            {
                var travelDragonCount = MAX_SELECT_COUNT;
                var allDragons = dragonData.GetAllUserDragons();

                allDragons.Sort(ConditionSortDragonList());//드래곤 조건에 따른 소팅

                DelAllSlot();

                for (var i = 0; i < allDragons.Count; i++)
                {
                    if (travelDragonCount <= i)
                        break;

                    if (allDragons[i] != null)
                    {
                        pickDragons.Add(allDragons[i]);
                    }
                }

                if (pickDragons != null && pickDragons.Count > 0)
                {
                    for (var k = 0; k < pickDragons.Count; k++)
                    {
                        SetSlot(k, pickDragons[k]);
                    }
                    landMarkTempPopup.SetTravelWaitingDragons(pickDragons);
                }
            }

            RefreshUI();
        }

        public void OnClickWorld()
        {
            //WJ - testUI(여행사 테스트 시안)
            if (testWorldSelectUI != null)
            {
                testWorldSelectUI.Init(curWorld);
                testWorldSelectUI.gameObject.SetActive(true);
                return;
            }
            //testUI

            if (worldSelectLayer == null) { return; }

            worldSelectLayer.Init(curWorld);

            worldSelectLayer.gameObject.SetActive(true);
        }

        public void OnClickCompleteDragonSelect()
        {
            pickDragons.Clear();

            for (var i = 0; i < prePickedDragons.Count; i++)
            {
                pickDragons.Add(User.Instance.DragonData.GetDragon(prePickedDragons[i]));
            }

            DragonList.RefreshList(GetPickDragonTagList().ToArray());

            SetDragon();

            DragonList?.OnHideList();
            landMarkTempPopup.SetTravelWaitingDragons(pickDragons);
            RefreshUI();
        }

        //WJ - test UI 확인 버튼
        public void OnClickTESTUIWorldSelect()
        {
            if (testWorldSelectUI == null) { return; }

            var worldData = TravelData.GetByWorldID(testWorldSelectUI.GetCurrentSelectedWorld());
            if (worldData != null)
            {
                curWorld = testWorldSelectUI.GetCurrentSelectedWorld();
                constructLayerTimerLabel.text = SBFunc.TimeString(worldData.TIME);
                MAX_SELECT_COUNT = worldData.CHAR_NUM;
                mapTitleLabel.text = StringData.GetStringByStrKey(worldData._NAME);

                DelAllSlot();
                RefreshUI();
                FlipBoardRoll();
            }

            testWorldSelectUI.gameObject.SetActive(false);
        }
        //TEST UI


        public void OnClickWorldSelect()
        {
            if (worldSelectLayer == null) { return; }

            var worldData = TravelData.GetByWorldID(worldSelectLayer.GetCurrentSelectedWorld());
            if (worldData != null)
            {
                curWorld = worldSelectLayer.GetCurrentSelectedWorld();
                constructLayerTimerLabel.text = SBFunc.TimeString(worldData.TIME);
                MAX_SELECT_COUNT = worldData.CHAR_NUM;
                mapTitleLabel.text = StringData.GetStringByStrKey(worldData._NAME);

                DelAllSlot();
                RefreshUI();

            }

            worldSelectLayer.gameObject.SetActive(false);
        }

        public void OnClickStart()
        {
            List<int> dragons = new List<int>();
            if (pickDragons != null && pickDragons.Count > 0)
            {
                for (var i = 0; i < pickDragons.Count; i++)
                {
                    var pDragonData = pickDragons[i];
                    if (pDragonData == null)
                    {
                        continue;
                    }

                    if (pDragonData.Tag > 0)
                    {
                        dragons.Add(pDragonData.Tag);
                    }
                }
            }

            // 선택 드래곤이 부족하면 출발 블럭
            if (dragons.Count < MAX_SELECT_COUNT)
            {
                //SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000135));
                ToastManager.On(100000135);//토스트로 교체
                return;
            }

            // 스태미나가 부족하면 출발 블럭
            var worldData = TravelData.GetByWorldID(curWorld);
            if (User.Instance.UserData.Energy < worldData.COST_STAMINA)
            {
                //SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000134));
                ToastManager.On(100000134);//토스트로 교체
                return;
            }

            if (!IsAvailableWorld())//현재 클리어한 월드(스테이지)가 아니면 리턴
            {
                ToastManager.On(StringData.GetStringFormatByStrKey("여행불가월드클리어오류", curWorld));
                return;
            }

            // 인벤토리 여유 공간 체크
            if (User.Instance.CheckInventoryForContentsEnter(eInvenSlotCheckContentType.Travel, curWorld, 0) == false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                    () =>
                    {
                        PopupManager.OpenPopup<InventoryPopup>();
                    }
                );
                return;
            }

            WWWForm netMsg = new WWWForm();
            netMsg.AddField("world", curWorld);
            netMsg.AddField("dragon", JsonConvert.SerializeObject(dragons.ToArray()));

            NetworkManager.Send("travel/start", netMsg, (jsonData) => { OnStartResponse(jsonData); });
        }

        void OnStartResponse(JObject jsonData)
        {
            if (jsonData.ContainsKey("rs"))
            {
                switch ((eApiResCode)jsonData["rs"].Value<int>())
                {
                    case eApiResCode.OK:
                    {
                        if (pickDragons != null && pickDragons.Count > 0)
                        {
                            for (var i = 0; i < pickDragons.Count; i++)
                            {
                                if (pickDragons[i] == null)
                                    continue;

                                pickDragons[i].SetDragonState((eDragonState)eTravelState.Travel);
                            }
                            landMarkTempPopup.DragonGoTravel();
                        }

                        TravelStartRefresh();

                        DragonHideEvent.Send();
                    }
                    break;
                    case eApiResCode.PARAM_ERROR:
                    {// Dragon or World Param 오류

                    }
                    break;
                    case eApiResCode.BUILDING_NOT_EXISTS:
                    {// 여행사가 없음.

                    }
                    break;
                    case eApiResCode.TRAVEL_ADVENTURE_NOT_CLEAR:
                    {// 여행 출발할 수 있는 상태 아님

                    }
                    break;
                    case eApiResCode.TRAVEL_CANNOT_START_NOW:
                    {// 여행 출발할 수 있는 상태 아님(여행사가 대기 상태가 아님)

                    }
                    break;
                    case eApiResCode.TRAVEL_INVALID_WORLD:
                    {// 대상지 파라미터 오류 or 대상지 데이터 오류

                    }
                    break;
                    case eApiResCode.TRAVEL_NOT_ENOUGH_DRAGONS:
                    {// 드래곤 부족

                    }
                    break;
                    case eApiResCode.TRAVEL_DRAGON_NOT_EXISTS:
                    {// 존재하지 않는 드래곤

                    }
                    break;
                    case eApiResCode.TRAVEL_NOT_ENOUGH_ENERGY:
                    {// 에너지 부족
                        ToastManager.On(100000134);
                    }
                    break;
                    case eApiResCode.TRAVEL_NOT_FINISHED:
                    {// 여행이 아직 끝나지 않음

                    }
                    break;
                    default:
                        break;
                }
            }
        }

        void OnFinish()
        {
            timeObject.Refresh = null;
            Travel.GetReward();
        }
        public void OnEvent(DragonShowEvent eventType)
        {
            for (var i = 0; i < pickDragons.Count; i++)
            {
                if (pickDragons[i] == null)
                    continue;

                pickDragons[i].SetDragonState((eDragonState)eTravelState.Travel);
            }

            DelAllSlot();

            RefreshUI();
            // 팝업 관련 처리
            PopupManager.ClosePopup<AccelerationMainPopup>();
        }

        void SetMapBGSprite(int selectedWorld)
        {
            int checkIndex = selectedWorld - 1;

            if (checkIndex >= 0 && checkIndex < worldSpriteList.Length)
            {
                mapBGSprite.sprite = worldSpriteList[checkIndex];
            }
        }

        void SetSlot(int slot, UserDragon data)
        {
            if (characterSlots == null)
            {
                return;
            }
            var slotCount = characterSlots.Count;
            var curSlot = characterSlots[slot];

            if (curSlot.ContainsKey("dimd") && curSlot["dimd"] != null)
            {
                curSlot["dimd"].SetActive(slot >= MAX_SELECT_COUNT);

                if (curSlot.ContainsKey("plus") && curSlot["plus"] != null)
                {
                    curSlot["plus"].SetActive(slot < MAX_SELECT_COUNT);
                }
            }

            if (slot < 0 || slot >= slotCount)
            {
                return;
            }

            if (curSlot == null)
            {
                return;
            }

            if (data == null)
            {
                if (curSlot.ContainsKey("target"))
                {
                    var targetGo = curSlot["target"];
                    targetGo.SetActive(false);
                }

                if (curSlot.ContainsKey("container"))
                {
                    var containerGo = curSlot["container"];
                    SBFunc.RemoveAllChildrens(containerGo.transform);
                }
                return;
            }

            if (curSlot.ContainsKey("target"))
            {
                var targetGo = curSlot["target"];
                targetGo.SetActive(true);
            }

            if (curSlot.ContainsKey("container"))
            {
                var containerGo = curSlot["container"];
                SBFunc.RemoveAllChildrens(containerGo.transform);

                //var clone = Instantiate(User.Instance.DragonData.GetNameDragonSpine(data.Image(), eSpineType.UI), containerGo.transform);
                //var dragonSpine = clone.GetComponent<UIDragonSpine>();
                //if (dragonSpine != null)
                //{
                //    dragonSpine.Data = data.BaseData;
                //}
                var clone = Instantiate(dragonPortraitFrame, containerGo.transform);//드래곤 스파인이 아닌, 드래곤 포트레이트로 기획 변경
                clone.transform.localPosition = Vector3.zero;
                clone.gameObject.SetActive(true);
                var frame = clone.GetComponent<DragonPortraitFrame>();
                if (frame == null)
                {
                    return;
                }
                frame.SetDragonPortraitFrame(data, false, false);
            }
        }

        void DelAllSlot()
        {
            for (var i = 0; i < 5; ++i)
            {
                SetSlot(i, null);
            }
            landMarkTempPopup.ClearTravelWaitingDragons();
            pickDragons.Clear();
        }

        private Comparison<UserDragon> ConditionSortDragonList()//여행보내기 드래곤 결정 조건 추가 (등급순 -> 레벨순-> 전투력순)
        {
            return SortCondition;
        }
        private int SortCondition(UserDragon a, UserDragon b)
        {
            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortGradeDescend(a, b);
                if (checker == 0)
                {
                    checker = SortLevelDescend(a, b);
                    if (checker == 0)
                    {
                        checker = SortBattlePointDescend(a, b);
                    }
                }
            }

            return checker;
        }
        //즐겨찾기
        private int SortFavorite(UserDragon a, UserDragon b)
        {
            return User.Instance.DragonData.IsFavorite(b.Tag).CompareTo(User.Instance.DragonData.IsFavorite(a.Tag));
        }
        //등급 내림차순
        private int SortGradeDescend(UserDragon a, UserDragon b)
        {
            var aGrade = a.Grade();
            var bGrade = b.Grade();
            return bGrade - aGrade;
        }

        //레벨 내림차순
        private int SortLevelDescend(UserDragon a, UserDragon b)
        {
            var aLevel = a.Level;
            var bLevel = b.Level;
            return bLevel - aLevel;
        }

        //전투력 내림차순
        private int SortBattlePointDescend(UserDragon a, UserDragon b)
        {
            var aInf = a.Status.GetTotalINFFloat();
            var bInf = b.Status.GetTotalINFFloat();

            if (aInf < bInf)
                return 1;
            else if (aInf == bInf)
                return 0;
            else
                return -1;
        }

        void FlipBoardRoll()
        {
            if (worldMapAlerter == null) return;
            worldMapAlerter.SetActive(true);
            int FlipBoardCount = FlipBoardUpTrans.Length;
            for (int i = 0; i < FlipBoardUpTrans.Length; ++i)
            {
                FlipBoardUpTrans[i].localEulerAngles = Vector3.zero;
                FlipBoardUpTrans[i].SetAsLastSibling();
                FlipBoardUpTrans[i].gameObject.SetActive(true);
                FlipBoardLowerTrans[i].localEulerAngles = new Vector3(90, 0, 0);
                FlipBoardLowerTrans[i].SetAsFirstSibling();
                FlipBoardLowerTrans[i].gameObject.SetActive(true);
            }

            FlipBoardLowerTrans[0].SetAsLastSibling();
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(0.3f);
            bool isFirstRotate = true;
            for (int i = FlipBoardCount - 1; i > -1; --i)
            {
                if (isFirstRotate)
                {
                    isFirstRotate = false;
                    FlipBoardLowerTrans[i].localEulerAngles = Vector3.zero;
                }
                sequence.Append(FlipBoardUpTrans[i].DORotate(new Vector3(90, 0, 0), 0.05f));
                sequence.AppendInterval(0.05f);
                if (i > 0)
                {
                    sequence.Append(FlipBoardLowerTrans[i - 1].DORotate(new Vector3(0, 0, 0), 0.05f));
                }
            }
            sequence.AppendCallback(() => { worldMapAlerter.SetActive(false); });
        }
        void SetBubbleNodeEffect(bool _isNormal)
        {
            if (animObj != null)
            {
                if (_isNormal)
                    animObj.DOPlay();
                else
                    animObj.DOPause();
            }
        }
        public void OnEvent(TravelDepartureEvent eventType)
        {

        }
    }
}



