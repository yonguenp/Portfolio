using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SandboxNetwork
{


    public class GemDungeonPopup : Popup<PopupData>
    {
        [SerializeField] RectTransform bodyRect;
        [Space(10)]
        [Header("Top")]
        [SerializeField] GameObject leftArrObj;
        [SerializeField] GameObject rightArrObj;

        [Space(10)]
        [Header("Predict Reward")]
        [SerializeField] GemGraphObj[] gemGraphObjs;
        [SerializeField] Button boostBtn;
        [SerializeField] Text boostAmountText;
        [SerializeField] TimeObject boostTimeObj;

        [Space(10)]
        [Header("Current Reward")]
        //[SerializeField] GemRewardObj[] gemRewardObjs;
        [SerializeField] GameObject rewardMark;
        [SerializeField] Button getItemBtn;
        [SerializeField] TimeObject timeObj;
        [SerializeField] Text timeText;
        [SerializeField] Image boostImage;
        [SerializeField] Sprite defaultBoostSprite;

        [Space(10)]
        [Header("Dragon Layer")]
        [SerializeField] GemDragonSlot[] gemDragonSlots;
        [SerializeField] Text battlePointText = null;
        [SerializeField] Text boosterPointText = null;
        [SerializeField] GameObject battlePointIcon = null;
        [SerializeField] GameObject boosterPointIcon = null;

        [Space(10)]
        [Header("Sub Popup")]
        [SerializeField] GemDungeonDragonListView DragonSelectPopup; // 여기서만 쓰니깐 이 팝업에 종속시켜둠
        [SerializeField] GemDungeonHealDragonListView DragonHealShowPopup;
        [SerializeField] Text healItemCountText;
        [SerializeField] Button healBtn;
        [SerializeField] GameObject infoPopupObj;
        [SerializeField] Text expectedUseHealItemCountText;
        [SerializeField] RectTransform infoRect;
        [SerializeField] GemBlackSmith blackSmith;

        private const int RefreshStandardTime = 60; // 현재 시간과 이 값만큼 차이나면 서버에 데이터 요청
        private int maxGemDungeonIndex = 0;
        private int curGemDungeonIndex = 0;
        LandmarkGemDungeon GemDungeonData = null;
        LandmarkGemDungeonFloor FloorData { get; set; } = null;

        List<UserDragon> AvailAbleRegistDragons = new List<UserDragon>(); // 등록 가능한 드래곤 처리
        List<int> zeroFatigueDragons = new List<int>();  // 피로도 0인 드래곤은 리스트에서 어둡게 표시해줘야 되서 저장
        List<int> currentRegistedDragon = new List<int>();
        List<int> lastRegistedDragon = new List<int>();
        List<int> currentHealExpectedDragon = new List<int>();
        private bool isDragonListViewInit = false;
        private int expectedUseHealItemCount = 0;
        private int curMaxSlot = 0;
        private bool isFullReward = false;
        private Coroutine boosterCoroutine = null;
        public int CurrentFloor { get { return -curGemDungeonIndex + SBDefine.GemDungeonDefaultFloor; } }
        private List<Tween> tweens = null;

        private bool IsBattle { get => eGemDungeonState.BATTLE == FloorData.State; }
        private bool IsFirstOpen { get => PlayerPrefs.GetInt("GemDungeonPopupFirst", 0) == 0; }
        static int refreshTimeStamp = 0;
        public override void InitUI()
        {
            bodyRect.localPosition = new Vector2(bodyRect.localPosition.x,-Screen.safeArea.position.y); // 아이폰 해상도 대응
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);
            GemDungeonData = LandmarkGemDungeon.Get();
            FloorData = GemDungeonData.GetFloorData(CurrentFloor);
            maxGemDungeonIndex = GemDungeonData.FloorDatas.Count;
            bool isAnotherLayerExist = (maxGemDungeonIndex > 1);
            leftArrObj.SetActive(isAnotherLayerExist);
            rightArrObj.SetActive(isAnotherLayerExist);
            DragonSelectPopup.gameObject.SetActive(false);
            DragonHealShowPopup.gameObject.SetActive(false);
            infoPopupObj.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(infoRect);
            Refresh();

            if (TutorialManager.tutorialManagement.IsFinishedTutorial(TutorialDefine.GemDungeon) == false)
            {
                TutorialManager.tutorialManagement.StartTutorial((int)TutorialDefine.GemDungeon);
            }
            else
            {
                /** 튜토리얼 끝나고 가장 처음 팝업 켤 때 도움말 켜기 */
                if (IsFirstOpen)
                {
                    PlayerPrefs.SetInt("GemDungeonPopupFirst", 1);
                    OnClickInfoBtn();
                }
            }
        }


        public void Refresh()
        {
            blackSmith.Refresh();

            RefreshUI();
            if (TimeManager.GetTimeCompareFromNow(refreshTimeStamp) >= RefreshStandardTime)
            {
                refreshTimeStamp = TimeManager.GetTime();
                NetworkManager.Send("gemdungeon/state", null, (JObject jsonData) =>    // 서버 응답 받고 UI 뿌리기
                {
                    if(SBFunc.IsJTokenCheck(jsonData["rs"]) && jsonData["rs"].ToObject<int>()== (int)eApiResCode.OK)
                    {
                        RefreshUI();
                    }
                    
                });
            }
        }
        void RefreshUI()
        {
            isFullReward = FloorData.IsFullReward;
            SetAvailAbleRegistDragon();
            SetDragonListLayer();
            SetDragonSelectList();
            SetRewardList();
            SetPredictRewardLayer();
            SetProgressTimeObj();
            SetBoostState();
        }


        #region 팝업 관련
        public override void OnClickDimd()
        {
            ClosePopup();
        }
        public void OnClickLeftArr()
        {
            curGemDungeonIndex += maxGemDungeonIndex - 1;
            curGemDungeonIndex %= maxGemDungeonIndex;
            Refresh();
        }
        public void OnClickRightArr()
        {
            curGemDungeonIndex += maxGemDungeonIndex + 1;
            curGemDungeonIndex %= maxGemDungeonIndex;
            Refresh();
        }
        public override void ClosePopup()
        {

            if (DragonSelectPopup.gameObject.activeSelf)
            {
                OnClickTeamSetOff();
                return;
            }
            if (DragonHealShowPopup.gameObject.activeSelf)
            {
                OnClickDragonHealOff();
                return;
            }
            if (infoPopupObj.activeSelf)
            {
                infoPopupObj.SetActive(false);
                return;
            }
                    

            if (boosterCoroutine != null)
            {
                StopCoroutine(boosterCoroutine);
                boosterCoroutine = null;
            }
            InitializeAnim(true);

            base.ClosePopup();
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
        }

        #endregion

        #region 드래곤 레이어
        void SetDragonSelectList()
        {
            DragonSelectPopup.RefreshDragonCountLabel(currentRegistedDragon.Count, curMaxSlot);
            if (isDragonListViewInit == false)
            {
                DragonSelectPopup.Init(currentRegistedDragon.ToArray());
                DragonSelectPopup.RefreshDragonCountLabel(currentRegistedDragon.Count, curMaxSlot);
                DragonSelectPopup.SetRegistCallBack((param) =>
                {
                    if(currentRegistedDragon.Count < curMaxSlot)
                    {
                        int tag = int.Parse(param);
                        currentRegistedDragon.Add(tag);
                        DragonSelectPopup.RefreshList(currentRegistedDragon.ToArray());
                        DragonSelectPopup.RefreshDragonCountLabel(currentRegistedDragon.Count, curMaxSlot);
                    }
                });
                DragonSelectPopup.SetReleaseCallback((param) =>
                {
                    int tag = int.Parse(param);
                    currentRegistedDragon.Remove(tag);
                    DragonSelectPopup.RefreshList(currentRegistedDragon.ToArray());
                    DragonSelectPopup.RefreshDragonCountLabel(currentRegistedDragon.Count, curMaxSlot);
                });
                isDragonListViewInit = true;
            }
            DragonSelectPopup.RefreshList(currentRegistedDragon.ToArray());
        }
        
        void SetAvailAbleRegistDragon()
        {
            AvailAbleRegistDragons = User.Instance.DragonData.GetAllUserDragons();
            zeroFatigueDragons = new List<int>();
            foreach (var dragon in GemDungeonData.DragonDatas.Values)
            {
                if (dragon.Floor > 0 && dragon.Floor != CurrentFloor) // 이미 다른 곳에 참여 중인 드래곤 제외
                {
                    AvailAbleRegistDragons.Remove(User.Instance.DragonData.GetDragon(dragon.DragonNo));
                    continue;
                }
                int curFatigue = dragon.ExpectedFatigue;
                if (curFatigue <= 0) // 피로도 0 드래곤 제외
                {
                    AvailAbleRegistDragons.Remove(User.Instance.DragonData.GetDragon(dragon.DragonNo));
                    zeroFatigueDragons.Add(dragon.DragonNo);
                    continue;
                }
            }
        }
        
        public void OnClickTeamSet()
        {
            if (isFullReward)
            {
                FullRewardProcess();
                return;
            }
            DragonSelectPopup.Init(currentRegistedDragon.ToArray());
            DragonSelectPopup.RefreshList(currentRegistedDragon.ToArray());
            DragonSelectPopup.OnShowList();
            DragonSelectPopup.RefreshSort();
            //DragonSelectPopup.gameObject.SetActive(true);
        }
        
        public void OnClickSuggest()
        {
            if (isFullReward)
            {
                FullRewardProcess();
                return;
            }
            var popup = PopupManager.OpenPopup<GemDungeonTeamRecommendPopup>();
            popup.SetCallBack(SuggestionOkProcess);
        }
        
        void SuggestionOkProcess()
        {
            var dragons = AvailAbleRegistDragons.OrderByDescending(dragon => dragon.Status.GetTotalINF());
            var selectDragon = dragons.Take(curMaxSlot).ToList(); // 피로도 남아 있고 다른 층에서 안 싸우고 있으는 드래곤들을 전투력 순으로 해서 슬롯 수만큼 가져옴
            List<int> temp = new List<int>();
            foreach (var dragon in selectDragon)
            {
                temp.Add(dragon.Tag);
            }
            bool isSame = true;
            if(temp.Count == currentRegistedDragon.Count)
            {
                for(int i =0; i < temp.Count; ++i)
                {
                    if (currentRegistedDragon.Contains(temp[i])==false)
                    {
                        isSame = false;
                        break;
                    }
                }
            }
            else
            {
                isSame = false;
            }
            if (isSame == false)
            {
                currentRegistedDragon = temp;
                OnClickSaveDragon();
            }
            PopupManager.ClosePopup<GemDungeonTeamRecommendPopup>();
            //currentRegistedDragon
        }
        
        void SetDragonListLayer()
        {
            int index = 0;
            curMaxSlot = FloorData.Slot;
            int maxSlotLimit = BuildingBaseData.Get("gemdungeon").MAX_SLOT;
            currentRegistedDragon.Clear();
            
            for(int i = 0, count = FloorData.Dragons.Count; i < count; ++i)
            {
                var dragonData = GemDungeonData.GetDragonData(FloorData.Dragons[i]);
                if (null == dragonData)
                    continue;

                currentRegistedDragon.Add(dragonData.DragonNo);
                gemDragonSlots[index].Init(FloorData, eGemDungeonSlotState.DragonExist, dragonData);
                index++;
            }
            if (curMaxSlot > index)
            {
                for (; index < curMaxSlot; ++index)
                {
                    gemDragonSlots[index].Init(FloorData, eGemDungeonSlotState.Empty, null);
                }
            }
            if (curMaxSlot < maxSlotLimit)
            {
                gemDragonSlots[curMaxSlot].Init(FloorData, eGemDungeonSlotState.AddSlot, null, curMaxSlot);
                if (curMaxSlot + 1 < maxSlotLimit)
                {
                    for (index = curMaxSlot + 1; index < maxSlotLimit; ++index)
                    {
                        gemDragonSlots[index].Init(FloorData, eGemDungeonSlotState.Lock, null, curMaxSlot);
                    }
                }
            }

            battlePointText.text = FloorData.TotalBattlePoint.ToString();
            lastRegistedDragon.Clear();
            foreach (var item in currentRegistedDragon)
            {
                lastRegistedDragon.Add(item);
            }
        }
        
        public void OnClickSaveDragon()
        {
            bool isSame = true;
            if (lastRegistedDragon.Count == currentRegistedDragon.Count)
            {
                for (int i = 0; i < lastRegistedDragon.Count; ++i)
                {
                    if (currentRegistedDragon.Contains(lastRegistedDragon[i]) == false)
                    {
                        isSame = false;
                        break;
                    }
                }
            }
            else
            {
                isSame = false;
            }
            if(isSame)
            {
                DragonSelectPopup.OnHideList();
            }
            else
            {
                SaveProcess();
            }
        }
        
        void SaveProcess()
        {
            DragonSelectPopup.OnHideList();
            WWWForm form = new();
            string deckString = string.Format("[{0}]", string.Join(",", currentRegistedDragon));
            form.AddField("floor", CurrentFloor);
            form.AddField("deck", deckString);
            NetworkManager.Send("gemdungeon/setdragon", form, (JObject jsonData) =>
            {
                foreach(var towns in Town.TownDragonsDic.Keys)
                {

                }
                Refresh();
            });
        }

        public void OnClickTeamSetOff()
        {
            currentRegistedDragon.Clear();
            foreach (var dragonData in GemDungeonData.DragonDatas.Values)
            {
                if (dragonData.Floor == CurrentFloor)
                {
                    currentRegistedDragon.Add(dragonData.DragonNo);
                }
            } 
            DragonSelectPopup.OnHideList();
            //currentRegistedDragon
        }
        
        public void OnClickDragonHealOff()
        {
            DragonHealShowPopup.OnHideList();
            currentHealExpectedDragon.Clear();
        }

        public void OnClickDragonHealState()
        {
            expectedUseHealItemCount = 0;
            DragonSelectPopup.OnHideList();
            currentHealExpectedDragon.Clear();
            DragonHealShowPopup.Init();
            DragonHealShowPopup.OnShowList();
            DragonHealShowPopup.RefreshList(currentHealExpectedDragon.ToArray());
            var healItem = ItemBaseData.GetItemListByKind(eItemKind.GEM_FATIGUE_RECOVERY);
            int healItemCount = 0;
            int itemKey = 0;
            foreach (var item in healItem)
            {
                itemKey = item.KEY;
                int itemCount = User.Instance.GetItemCount(itemKey);
                
                healItemCount += itemCount;
            }
            healItemCountText.text = (healItemCount).ToString();
            healBtn.SetButtonSpriteState(false);
            expectedUseHealItemCountText.text = string.Format("{0} : {1}", StringData.GetStringByStrKey("드링크사용개수"), expectedUseHealItemCount);
            DragonHealShowPopup.SetRegistCallBack((param) =>
            {
                if (currentHealExpectedDragon.Count < healItemCount)
                {
                    int tag = int.Parse(param);
                    currentHealExpectedDragon.Add(tag);
                    DragonHealShowPopup.RefreshList(currentHealExpectedDragon.ToArray());
                    //  DragonHealShowPopup.RefreshDragonCountLabel(currentHealExpectedDragon.Count, healItemCount);
                    healBtn.SetButtonSpriteState(true);
                    ++expectedUseHealItemCount;
                    //healItemCountText.text = (healItemCount - expectedUseHealItemCount).ToString();
                    expectedUseHealItemCountText.text = string.Format("{0} : {1}", StringData.GetStringByStrKey("드링크사용개수"), expectedUseHealItemCount);
                }
                else
                {
                    ToastManager.On(StringData.GetStringByStrKey("보유아이템없음"));
                }
            });
            DragonHealShowPopup.SetReleaseCallback((param) =>
            {
                int tag = int.Parse(param);
                currentHealExpectedDragon.Remove(tag);
                DragonHealShowPopup.RefreshList(currentHealExpectedDragon.ToArray());
                //DragonHealShowPopup.RefreshDragonCountLabel(currentHealExpectedDragon.Count, healItemCount);
                --expectedUseHealItemCount;
                if(expectedUseHealItemCount == 0)
                    healBtn.SetButtonSpriteState(false);
                //healItemCountText.text = (healItemCount - expectedUseHealItemCount).ToString();
                expectedUseHealItemCountText.text = string.Format("{0} : {1}", StringData.GetStringByStrKey("드링크사용개수"), expectedUseHealItemCount);
            });
            DragonHealShowPopup.OnShowList();
        }

        public void OnClickHealFatigue()
        {
            if (currentHealExpectedDragon == null || currentHealExpectedDragon.Count == 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("회복드래곤선택없음"));
                return;
            }
                
            var healItem = ItemBaseData.GetItemListByKind(eItemKind.GEM_FATIGUE_RECOVERY);
            int healItemCount = 0;
            int healItemKey = 0;
            foreach (var item in healItem)
            {
                healItemCount += User.Instance.GetItemCount(item.KEY);
                healItemKey = item.KEY;
            }
            if (healItemCount <= 0 || healItemCount < currentHealExpectedDragon.Count)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("아이템구매팝업제목"), StringData.GetStringByStrKey("회복제구매팝업내용"), () => PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData()));
                PopupManager.GetPopup<SystemPopup>().SetButtonState(true, true, true);
                return;
            }
            JArray healList = new JArray();
            foreach (var dragonNo in currentHealExpectedDragon)
            {
                healList.Add(dragonNo);
            }
            WWWForm param = new WWWForm();
            param.AddField("item_no", healItemKey);
            param.AddField("item_count", currentHealExpectedDragon.Count);
            param.AddField("dragon_tags", healList.ToString(Newtonsoft.Json.Formatting.None));
            NetworkManager.Send("gemdungeon/recovery", param, (JObject json) =>
            {
                if (SBFunc.IsJTokenCheck(json["rs"])&& (int)json["rs"] == (int)eApiResCode.OK)
                {
                    ToastManager.On(StringData.GetStringByStrKey("스태미나회복완료"));
                    DragonHealShowPopup.SetFatigueHealEffect(currentHealExpectedDragon, OnClickDragonHealState);
                    Refresh();
                }
            });
        }

        public void OnClickHealItemMakeBtn()
        {
            var healItem = ItemBaseData.GetItemListByKind(eItemKind.GEM_FATIGUE_RECOVERY);
            if (healItem != null && healItem.Count > 0)
            {
                int itemKey = healItem[0].KEY;
                int key = 0;
                int.TryParse(RecipeBaseData.GetRecipeData(itemKey).GetKey(), out key);
                var popup = PopupManager.OpenPopup<ItemMakePerfectPopup>(new ItemMakePopupData(key, StringData.GetStringByStrKey("드링크제작")));
                popup.SetRefreshCallBack(OnClickDragonHealState);
            }
        }
        
        #endregion

        #region 리워드 얻는 레이어

        void SetRewardList()
        {
            bool rewardAble = false;
            for (int i = 0, count = FloorData.Rewards.Count; i < count; ++i)
            {
                var rewardCount = Mathf.FloorToInt((FloorData.GetReward(i) + FloorData.GetClientReward(i)) * SBDefine.CONVERT_MILLION);
                if (rewardCount > 0)
                {
                    rewardAble = true;
                    break;
                }
            }

            rewardMark.SetActive(rewardAble);
            getItemBtn.SetButtonSpriteState(rewardAble);
        }

        void SetProgressTimeObj()
        {
            if (isFullReward)
            {
                timeText.color = Color.red;
                timeObj.Refresh = null;
                timeText.text = SBFunc.TimeString(0);
            }
            else
            {
                timeText.color = Color.white;
                if (FloorData.State == eGemDungeonState.IDLE)
                    timeText.text = SBFunc.TimeString(0);
                else
                    timeObj.Refresh = RefreshGainTime;
            }
        }

        void RefreshGainTime()
        {
            if (null == timeText)
                return;

            timeText.text = SBFunc.TimeString(TimeManager.GetTimeCompareFromNow(FloorData.GainTimeStamp));
            SetRewardList();
        }

        public void OnClickRewardBtn()
        {
            if (false == FloorData.IsReward)
            {
                if (FloorData.Dragons ==null ||FloorData.Dragons.Count == 0)
                    ToastManager.On(StringData.GetStringByStrKey("드래곤등록알림"));
                else
                    ToastManager.On(StringData.GetStringByStrKey("보상획득불가"));
                return;
            }

            FloorData.OnHarvest(Refresh);
        }

        #endregion



        #region 보상예측 및 부스트 레이어
        void SetPredictRewardLayer()
        {
            for (int i =0, count = gemGraphObjs.Length; i<count;++i)
            {
                gemGraphObjs[i].Init(i, FloorData);
            }
        }
        void SetBoostState()
        {
            if (FloorData.IsBuffState) // 부스트 중
            {
                boostBtn.SetButtonSpriteState(IsBattle);
                boostTimeObj.Refresh = () =>
                {
                    if (FloorData.IsBuffState)
                    {
                        boostAmountText.text = SBFunc.TimeString(FloorData.BuffTimeLimit);
                    }
                    else
                    {
                        Refresh();
                    }
                };

                if (IsBattle)
                    BoosterStart();
                else
                    BoosterClear();
            }
            else // 부스터 사용 중이 아닐 때
            {
                var list = ItemBaseData.GetItemListByKind(eItemKind.GEM_BOOSTER);
                int boostCnt = 0;
                foreach (var item in list)
                {
                    boostCnt += User.Instance.GetItemCount(item.KEY);
                }
                boostBtn.SetButtonSpriteState(boostCnt > 0 && IsBattle);
                boostAmountText.text = string.Format("x{0}",boostCnt);
                boostTimeObj.Refresh = null;

                BoosterClear();
            }
        }
        public void OnClickBoost()
        {
            if (isFullReward)
            {
                FullRewardProcess();
                return;
            }
            if (IsBattle) { 
                if (FloorData.IsBuffState)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("부스터알림팝업내용"), () => BoostCheckUseProcess());
                    PopupManager.GetPopup<SystemPopup>().SetButtonState(true, true, true);
                }
                else
                {
                    BoostCheckUseProcess();
                }
            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("부스터사용불가토스트"));
                //현재 전투중이 아닙니다.
                //Toa
            }
        }

        void BoostCheckUseProcess()
        {
            var list = ItemBaseData.GetItemListByKind(eItemKind.GEM_BOOSTER);
            int boostCnt = 0;
            foreach (var item in list)
            {
                boostCnt += User.Instance.GetItemCount(item.KEY);
            }
            if (boostCnt == 0) 
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("아이템구매팝업제목"), StringData.GetStringByStrKey("부스터구매팝업내용"), () => PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData(6)));
                PopupManager.GetPopup<SystemPopup>().SetButtonState(true, true, true);
            }
            else
            {
                PopupManager.OpenPopup<GemDungeonBoostPopup>(new GemDungeonBoosterPopupData(CurrentFloor));
            }
        }
        #endregion


        public void OnClickInfoBtn()
        {

            infoPopupObj.SetActive(true);
        }

        public void OnClickInfoClose()
        {
            infoPopupObj.SetActive(false);
        }

        void FullRewardProcess()
        {
            ToastManager.On(StringData.GetStringByStrKey("잼던전보상수령경고"));
        }
        #region 부스터 연출
        private void InitializeAnim(bool isClose = false)
        {
            if (null == tweens)
                tweens = new();

            for (int i = 0, count = tweens.Count; i < count; ++i)
            {
                if (null == tweens[i])
                    continue;
                tweens[i].Kill();
            }
            tweens.Clear();

            var isBuff = FloorData.IsBuffState;

            if (null != battlePointIcon)
                battlePointIcon.SetActive(!isBuff);

            if (null != boosterPointIcon)
                boosterPointIcon.SetActive(isBuff);

            if (null != battlePointText)
                battlePointText.color = IsBattle ? (isBuff ? Color.yellow : Color.white) : Color.red;

            SetBoosterInfo();

            if (null != gemGraphObjs)
            {
                for (int i = 0, count = gemGraphObjs.Length; i < count; ++i)
                {
                    if (null == gemGraphObjs[i])
                        continue;

                    gemGraphObjs[i].SetEffect(!isClose && IsBattle);
                }
            }
        }

        void SetBoosterInfo()
        {
            var isBuff = FloorData.IsBuffState;
            var item = ItemBaseData.Get(FloorData.BuffItemNo);
            if (null != boosterPointText)
            {
                boosterPointText.color = Color.yellow;
                boosterPointText.gameObject.SetActive(isBuff);
                if (isBuff)
                {
                    if (item != null)
                        boosterPointText.text = SBFunc.StrBuilder("+", item.VALUE, StringData.GetStringByStrKey("skill_effect_percent"));
                }
            }

            if (boostImage != null)
                boostImage.sprite = (isBuff && item != null) ? item.ICON_SPRITE : defaultBoostSprite;
        }

        private void BoosterStart()
        {
            InitializeAnim();
            if (null == boosterCoroutine)
            {
                boosterCoroutine = StartCoroutine(BoosterCoroutine());
            }
            else
            {
                SetBoosterInfo();
            }
        }
        private void BoosterClear()
        {
            if (null != boosterCoroutine)
            {
                StopCoroutine(boosterCoroutine);
                boosterCoroutine = null;
            }
            InitializeAnim();
        }
        IEnumerator BoosterCoroutine()
        {
            BoosterTextAnim(battlePointText);
            BoosterTextAnim(boosterPointText);

            var wait = SBDefine.GetWaitForSecondsRealtime(0.5f);
            while (IsBattle && FloorData.IsBuffState)
            {
                yield return wait;
            }
            InitializeAnim();
            yield break;
        }
        private void BoosterTextAnim(Text target)
        {
            if (null == target)
                return;

            var sequnce = DOTween.Sequence();
            sequnce.SetDelay(0.125f);
            sequnce.Append(target.DOColor(new Color(1f, 0.6f, 0.016f, 1f), 2f).SetEase(Ease.InQuad));
            sequnce.SetDelay(0.125f);
            sequnce.SetLoops(-1, LoopType.Yoyo);
            tweens.Add(sequnce.Play());
        }
        #endregion

    }
}