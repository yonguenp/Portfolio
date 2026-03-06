using Coffee.UIEffects;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class TownManagePopup : Popup<PopupData>
    {
        [SerializeField]
        private Animator constructingAnim = null;

        [Space(20)]
        [Header("Left")]
        [SerializeField] 
        private GameObject inProgressLayerObject = null;
        [SerializeField]
        private Text inProgressTimeText = null;
        [SerializeField]
        private Text TownLvText = null;
        [SerializeField]
        private Text TownNameText = null;
        [SerializeField] 
        private RawImage buildingTexture;
        [SerializeField]
        private TimeObject upgradeTimeObject = null;
        [SerializeField]
        private Text upgradeTimeText = null;
        [SerializeField]
        private Button upgradeAccelButton = null;
        [SerializeField]
        private Button upgradeButton = null;
        [SerializeField]
        private Text upgradeCostLabel = null;
        [SerializeField] 
        private DOTweenAnimation animObj = null;

        [SerializeField]
        private Button floorBuyButton = null;
        [SerializeField]
        private Button finishButton = null;
        [SerializeField]
        private Text finishButtonLabel = null;

        [Space(20)]
        [Header("Right")]
        [SerializeField]
        private ScrollRect needItemListScrollRect = null;
        [SerializeField]
        private Slider TownMissionProgressSlider = null;
        [SerializeField]
        private Text TownMissionProgressText = null;
        [SerializeField]
        private ScrollRect missionListScrollRect = null;
        [SerializeField]
        private GameObject missionObj = null;
        [SerializeField]
        private GameObject checkLabel = null;

        TownExteriorData curExteriorData = null;
        AreaLevelData curAreaLevelData = null;
        int floorMaxLevel = 0;
        int exteriorMaxLevel = 0;
        bool isAvailableLevelup = false;
        int missionClearCount = 0;

        Quest currentQuest = null;
        List<QuestConditionData> missionConditionList = new List<QuestConditionData>();
        List<GameObject> missionObjList = new();
        List<ItemFrame> needItemList = new List<ItemFrame>();
        List<GameObject> needItemObjList = new();

        private eBuildingState lastestBuildingState = eBuildingState.NONE;

        private bool isNetworkState = false;

        public override void InitUI()
        {
            if (Town.Instance.townViewTexture == null) return;
            if (buildingTexture.texture == null)
            {
                buildingTexture.texture = Town.Instance.townViewTexture;
            }
            isNetworkState = false;
            Town.Instance.SetSubCamState(true);
            UICanvas.Instance.StartBackgroundBlurEffect();
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);
            
            exteriorMaxLevel = AreaLevelData.GetMaxLevel();
            floorMaxLevel = AreaExpansionData.GetMaxFloorLevel();
            lastestBuildingState = eBuildingState.NONE;

            RefreshUI();
        }


        public void RefreshUI()
        {
            curExteriorData = User.Instance.ExteriorData;
            if (curExteriorData == null) { return; }
            curAreaLevelData = AreaLevelData.GetByLevel(curExteriorData.ExteriorLevel);
            if (curAreaLevelData == null) { return; }
            isAvailableLevelup = false;

            TownLvText.text = string.Format("Lv.{0} ", curExteriorData.ExteriorLevel);
            TownNameText.text = StringData.GetStringByIndex(100002038);

            switch (curExteriorData.ExteriorState)
            {
                case eBuildingState.NOT_BUILT:          // 미건설 (외형 탭에서는 별도의 처리 없이 NORMAL과 동일하게 처리
                case eBuildingState.NORMAL:             // 정상 상태 (건설완료 이후)
                    upgradeTimeObject.Refresh = null;

                    RefreshMissionScrollView();//미션 스크롤 갱신
                    RefreshMissionProgress();//미션 프로그래스 바 갱신
                    RefreshNeedItemList();//필요 아이템 리스트 갱신
                    upgradeCostLabel.text = curAreaLevelData.NEED_GOLD.ToString();
                    isAvailableLevelup = IsSufficientNeedItem() && IsSufficientGold() && IsTownMissionComplete(); // 재료 & 골드 & 미션

                    if (curExteriorData.ExteriorLevel == exteriorMaxLevel)
                    {
                        SetConstructingAnimState(true, true);
                    }
                    else 
                    {
                        SetConstructingAnimState(false, lastestBuildingState != eBuildingState.CONSTRUCT_FINISHED);
                    }
                    break;
                case eBuildingState.CONSTRUCTING:       // 건설 중
                    SetConstructingAnimState(true, lastestBuildingState != eBuildingState.NORMAL);
                    RefreshTimeObject();
                    break;
                case eBuildingState.CONSTRUCT_FINISHED: // 건설 완료 후 대기상태
                    if(lastestBuildingState != curExteriorData.ExteriorState)
                    {
                        SetConstructingAnimState(true, true);
                    }
                    
                    upgradeTimeObject.Refresh = null;
                    break;
            }
            if( lastestBuildingState != curExteriorData.ExteriorState)
            {
                lastestBuildingState = curExteriorData.ExteriorState;
            }
            RefreshButtonByState();
        }

        void SetConstructingAnimState(bool isConstructing, bool isAnimStop)
        {
            if (isAnimStop)
            {
                constructingAnim.SetBool("isConstruct", isConstructing);
                constructingAnim.Play(isConstructing ? "FinishState" : "NormalState");
            }
            else
            {
                constructingAnim.Play(isConstructing ? "TownToCenter" : "TownToCenterReverse");
            }
        }

        void RefreshTimeObject()
        {
            upgradeTimeObject.Refresh = () =>
            {
                int time = TimeManager.GetTimeCompare(curExteriorData.ExteriorTime);
                inProgressTimeText.text = SBFunc.TimeString(time);
                if (time <= 0)
                {
                    User.Instance.ExteriorData.SetExteriorState(eBuildingState.CONSTRUCT_FINISHED);
                    RefreshUI();
                }
            };

            upgradeTimeObject?.Refresh?.Invoke();
        }

        void RefreshButtonByState()
        {
            bool btnOffCondition = curExteriorData.ExteriorState == eBuildingState.CONSTRUCTING || curExteriorData.ExteriorState == eBuildingState.CONSTRUCT_FINISHED;

            SetBubbleNodeEffect(isAvailableLevelup);
            if (upgradeCostLabel != null)
                upgradeCostLabel.color = User.Instance.GOLD >= curAreaLevelData.NEED_GOLD ? Color.white : Color.red;

            upgradeButton.SetButtonSpriteState(isAvailableLevelup);
            floorBuyButton.gameObject.SetActive(curExteriorData.ExteriorState == eBuildingState.NORMAL);
            floorBuyButton.SetButtonSpriteState(IsAvailableNextFloor());

            inProgressLayerObject.SetActive(curExteriorData.ExteriorState == eBuildingState.CONSTRUCTING);
            finishButton.gameObject.SetActive(btnOffCondition);
            finishButton.SetButtonSpriteState(curExteriorData.ExteriorState == eBuildingState.CONSTRUCT_FINISHED);
            finishButtonLabel.text = curExteriorData.ExteriorState == eBuildingState.CONSTRUCT_FINISHED ? StringData.GetStringByIndex(100000074) : StringData.GetStringByIndex(100000108);

            bool isExteriorMaxLevel = curExteriorData.ExteriorLevel >= exteriorMaxLevel;
            if (isExteriorMaxLevel)
            {
                finishButtonLabel.text = StringData.GetStringByIndex(100000329);
            }
        }
        protected override IEnumerator OpenAnimation()
        {
            dimClose = false;
            InitUI();
            dimClose = true;
            yield break;
        }
        protected override IEnumerator CloseAnimation()
        {
            SetActive(false);

            RunExitCallback();

            yield break;
        }
        // Start is called before the first frame update
        public override void ClosePopup()
        {
            UICanvas.Instance.EndBackgroundBlurEffect();
            Town.Instance.SetSubCamState(false);
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            base.ClosePopup();
        }
        public void OnClickLevelUpButton()//재료 & 골드 & 미션 상태 선체크 해서 막기
        {
            //User.Instance.GOLD >= curAreaLevelData.NEED_GOLD && IsTownMissionComplete()
            if (!isAvailableLevelup)//타운 조건 불충족 (미션 & 재료 & 돈)
            {
                if(!IsSufficientGold() || !IsTownMissionComplete())
                {
                    ToastManager.On(100002524);
                }
                else
                {
                    if (!IsSufficientNeedItem())//재료 부족?
                    {
                        var needItemList = SBFunc.GetNeedItemList(curAreaLevelData.NEED_ITEM);
                        if (needItemList.Count <= 0)
                        {
                            ToastManager.On(100002524);
                            return;
                        }

                        ProductsBuyNowPopup.OpenPopup(needItemList, () => {
                            RefreshUI();
                        });
                    }
                }
                return;
            }

            var popup = PopupManager.OpenPopup<TownUpgradePopup>();
            popup.SetConstructCompleteCallback(()=> {
                if (currentQuest != null)//현재 들고 있는 미션 퀘스트 완료처리
                    QuestManager.Instance.SetQuestComplete(currentQuest.ID);
                SuccessResponseCallback();
            });
        }

        void SuccessResponseCallback()
        {
            Town.Instance.RefreshMap();
            Town.Instance.RefreshSubCamProjection();
            RefreshUI();
        }

        void RefreshMissionScrollView()
        {
            AllOffMissionObj();
            missionConditionList.Clear();

            var townQuestList = QuestManager.Instance.GetProceedQuestDataByType(eQuestType.TOWN);
            if (townQuestList != null && townQuestList.Count >= 1)
                currentQuest = townQuestList[0];
            
            if(currentQuest != null)
            {
                SetQuestConditionItem();
            }
        }

        void SetQuestConditionItem()
        {
            missionClearCount = 0;
            var conditionDic = currentQuest.Conditions;
            int objCheckIndex = 0;
            foreach (KeyValuePair<int, QuestConditionData> items in conditionDic)
            {
                var data = items.Value;
                if (data == null)
                    continue;

                if (missionObjList.Count <= objCheckIndex)
                {
                    GameObject go;
                    go = Instantiate(missionObj, missionListScrollRect.content.transform);
                    missionObjList.Add(go);
                }

                missionObjList[objCheckIndex].SetActive(true);
                missionObjList[objCheckIndex].GetComponent<TownMission>().Init(data);
                missionConditionList.Add(data);
                
                var isMissionClear = data.IsQuestClear();
                if (isMissionClear)
                    missionClearCount++;

                objCheckIndex++;
            }
        }

        void AllOffMissionObj()
        {
            if(missionObjList == null)
            {
                missionObjList = new List<GameObject>();
            }
            foreach(GameObject obj in missionObjList)
            {
                obj.SetActive(false);
            }
        }

        void AllOffNeedItemsObj()
        {
            if(needItemObjList == null)
            {
                needItemObjList = new List<GameObject>();
            }
            foreach (GameObject obj in needItemObjList)
            {
                obj.SetActive(false);
            }
        }

        void RefreshMissionProgress()
        {
            if (TownMissionProgressSlider == null || missionConditionList == null)
                return;

            if (checkLabel != null)
                checkLabel.gameObject.SetActive(missionConditionList.Count <= 0);

            if (missionConditionList.Count > 0)
            {
                TownMissionProgressSlider.value = (float)missionClearCount / (float)missionConditionList.Count;
                SetProgressLabel();
            }
            else
            {
                TownMissionProgressSlider.value = 0;
                if (TownMissionProgressText != null)
                    TownMissionProgressText.text = string.Format(StringData.GetStringByStrKey("달성도") + " {0}/{1}", 0, 0);
            }
        }
        void SetProgressLabel()
        {
            if (TownMissionProgressText != null)
                TownMissionProgressText.text = string.Format(StringData.GetStringByStrKey("달성도") + " {0}/{1}", missionClearCount, missionConditionList.Count);
        }

        void RefreshNeedItemList()// 외형 레벨업 필요아이템 업데이트
        {
            if (needItemListScrollRect == null || curAreaLevelData == null) { return; }

            AllOffNeedItemsObj();
            needItemList.Clear();

            // 요구 재료가 없는 경우 체크
            if (curAreaLevelData.NEED_ITEM.Count <= 0) { return; }
            if (curAreaLevelData != null)
            {
                for(int i =0; i <curAreaLevelData.NEED_ITEM.Count;++i)
                {
                    if(needItemObjList.Count <= i) { 
                        GameObject itemObject = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "ItemPrefab"), needItemListScrollRect.content);
                        itemObject.transform.localScale = Vector3.one;
                        needItemObjList.Add(itemObject);
                    }
                    needItemObjList[i].SetActive(true);
                    ItemFrame frameComponent = needItemObjList[i].GetComponent<ItemFrame>();
                    if (frameComponent != null)
                    {
                        frameComponent.setFrameRecipeInfo(curAreaLevelData.NEED_ITEM[i].ItemNo, curAreaLevelData.NEED_ITEM[i].Amount);
                        needItemList.Add(frameComponent);
                    }
                }
            }
        }

        bool IsTownMissionComplete()//현재 타운 미션이 완료가 되었는지 체크 - 확인 후 리턴 풀기
        {
            var currentTownMission = QuestManager.Instance.GetProceedQuestDataByType(eQuestType.TOWN);
            if (currentTownMission == null || currentTownMission.Count <= 0)
                return false;

            var questSize = currentTownMission.Count;
            if(questSize == 1)
            {
                return currentTownMission[0].IsQuestClear();
            }
            else//다중 퀘스트가 들어올 경우 는 없겠지만(지금 타운미션은 1개의 퀘스트와 하위 여러개의 컨디션으로 나뉨) 일단 대응
            {
                int isQuestCompleteCount = 0;
                foreach(var questData in currentTownMission)
                {
                    if (questData == null)
                        continue;

                    if (questData.IsQuestClear())
                        isQuestCompleteCount++;
                }
                return isQuestCompleteCount == currentTownMission.Count;
            }
        }
        bool IsSufficientGold()
        {
            return User.Instance.GOLD >= curAreaLevelData.NEED_GOLD;
        }

        bool IsSufficientNeedItem()
        {
            if (needItemList == null || needItemList.Count <= 0)
                return true;

            int resultCount = 0;
            foreach (ItemFrame item in needItemList)
            {
                if (item != null)
                {
                    resultCount += item.IsSufficientAmount ? 1 : 0;
                }
            }

            return resultCount == curAreaLevelData.NEED_ITEM.Count;
        }
        public void OnClickLevelUpAccelerateButton()
        {
            AccelerationMainPopup.OpenPopup(eAccelerationType.LEVELUP, 1, curAreaLevelData.UPGRADE_TIME, curExteriorData.ExteriorTime, RefreshUI);
        }

        public void OnClickLevelUpFinishButton()
        {
            if (isNetworkState)
            {
                return;
            }
            if (curExteriorData.ExteriorLevel >= exteriorMaxLevel)
            {
                ToastManager.On(100002076);
                return;
            }

            if(curExteriorData.ExteriorState == eBuildingState.CONSTRUCTING)
            {
                ToastManager.On(100002523);
                return;
            }

            WWWForm paramData = new WWWForm();
            paramData.AddField("tag", 1);
            
            isNetworkState = true;
            NetworkManager.Send("building/complete", paramData, (jsonData) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            BuildingCompletePopup.OpenPopup(null, ()=> {
                                ReviewPopupManager.Instance.TryShowTownLevelCondition();//리뷰 팝업 
                            });
                            SuccessResponseCallback();

                            // 레드닷 갱신처리
                            UIManager.Instance.MainPopupUI.RequestUpdateTownReddot();
                            //광산아이콘 잠금 상태 갱신 처리
                            UIManager.Instance.MainPopupUI.UpdateMiningLockState();
                            break;
                    }
                }
            }, (string arg) =>
            {
                isNetworkState = false;
            });
        }

        bool IsAvailableNextFloor()
        {
            int nextFloorLevel = curExteriorData.ExteriorFloor + 1 > exteriorMaxLevel ? exteriorMaxLevel : curExteriorData.ExteriorFloor + 1;
            AreaExpansionData nextFloorData = AreaExpansionData.GetFloorData(nextFloorLevel);

            if (nextFloorData == null)
                return false;

            if(nextFloorData.OPEN_LEVEL > curExteriorData.ExteriorLevel && curExteriorData.ExteriorFloor < floorMaxLevel)
            {
                return false;
            }
            return true;
        }

        AreaExpansionData GetNextFloorData()
        {
            int nextFloorLevel = curExteriorData.ExteriorFloor + 1 > exteriorMaxLevel ? exteriorMaxLevel : curExteriorData.ExteriorFloor + 1;
            return AreaExpansionData.GetFloorData(nextFloorLevel);
        }

        public void OnClickFloorExtensionButton()// 충 업그레이드 가능 여부 예외처리
        {
            if (!IsAvailableNextFloor())
            {
                ToastManager.On(100000727);
                return;
            }

            string popupTitle = StringData.GetStringByIndex(100000260);
            string popupSubTitle = string.Format(StringData.GetStringByIndex(100000101), curExteriorData.ExteriorFloor + 1);
            string contentGuide = StringData.GetStringByIndex(100000100);
            ePriceDataFlag priceFlag = ePriceDataFlag.CloseBtn | ePriceDataFlag.Gold | ePriceDataFlag.ContentBG | ePriceDataFlag.SubTitleLayer;

            var nextFloorData = GetNextFloorData();
            PricePopup.OpenPopup(popupTitle, popupSubTitle, contentGuide, nextFloorData.COST_NUM, priceFlag, SendFloorUpgradeAPI);
        }
        void SendFloorUpgradeAPI()
        {
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("building/floor", null, (JObject jsonData) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            BuildingCompletePopup.OpenPopup();
                            PopupManager.ClosePopup<PricePopup>();
                            SuccessResponseCallback();
                            break;
                    }
                }
            }, (string arg) =>
            {
                isNetworkState = false;
            });
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


        public void OnClickTownEdit()
        {
            ClosePopup();
            Invoke("TownEditAfterPointerUp", 0.1f);
        }
        void TownEditAfterPointerUp()
        {
            UIManager.Instance.UIEditTown.OnTownEdit();
        }

    }
}
