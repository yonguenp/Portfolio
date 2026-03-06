using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum eSortType
    {
        HighGrade = 0,
        LowGrade,
        Normal,
        Rare,
        SuperRare,
        UltraRare,
        Legend
    }
    public class UserDragonSelectCardSlot : ITableData
    {
        public List<UserDragonCard> list = new List<UserDragonCard>();
        public string GetKey(){ return ""; } 
        public void Init(){}

        public UserDragonSelectCardSlot(List<UserDragonCard> _list)
        {
            list = _list.ToList();
        }
    }
    public class UserCardBundle : UserDragonCard
    {
        public UserDragonCard card { get { if (cards.Count > 0) return cards[0]; else return null; } }
        public List<UserDragonCard> cards;
        public int amount { get { return cards.Count; } }

        public UserCardBundle(UserDragonCard c)
        {
            cards = new List<UserDragonCard>();
            cards.Add(c);

            SetData(card.CardTag, card.DragonTag);
        }

        public void AddBundle(UserDragonCard c)
        {
            if (cards.Contains(c))
                return;

            cards.Add(c);
        }

        public void DelBundle(UserDragonCard c)
        {
            if (!cards.Contains(c))
                return;

            cards.Remove(c);

            if (card != null)
                SetData(card.CardTag, card.DragonTag);
        }
    }
    public class DragonMultiCompoundLayer : SubLayer
    {
        const int UNIQUE_GACHA_TICKET_ITEM_NO = 30000009;
        const int LEGENDARY_GACHA_TICKET_ITEM_NO = 30000010;


        const int MaxCompoundSlotCount = 10;//스크롤 뷰 최대 슬롯 갯수
        const string COMPOUND_CONSTRAINT_GOAL_TIME = "dragon_compound_time";//오늘 하루 안보기를 누르면 켜야될 시간을 미리 계산해서 넘김
        int MERGE_SUCCESS_COUNT_UR = -1;//드래곤 합성 시 천장 기준 값(UR 등급 합성 카운트 20회를 만족하면 이후 시도 시 100% 성공)
        int MERGE_SUCCESS_COUNT_SR = -1;//드래곤 합성 시 천장 기준 값(UR 등급 합성 카운트 20회를 만족하면 이후 시도 시 100% 성공)

        [Header("Count")]
        [SerializeField]
        private Text sr_ceilingLabel = null;
        [SerializeField]
        private DOTweenAnimation sr_ticket_anim = null;

        [SerializeField]
        private Text ur_ceilingLabel = null;
        [SerializeField]
        private DOTweenAnimation ur_ticket_anim = null;

        [Header("Scroll")]
        [SerializeField]
        protected Text emptyCardLabel = null;
        [SerializeField]
        protected TableViewGrid cardTableView = null;
        [SerializeField]
        protected TableView compoundSlotTableView = null;

        [Space(10)]
        [Header("Buttons")]
        [SerializeField]
        protected Button eater_button = null;
        [SerializeField]
        protected Text countLabel = null;
        [SerializeField]
        protected Button resultBtn = null;
        [SerializeField]
        protected Button addBtn = null;
        [SerializeField]
        Button resetBtn = null;
        [SerializeField]
        Button backBtn = null;

        [Space(10)]
        [Header("DropDown")]
        [SerializeField]
        private DropDownUIController dropdownController = null;//각 eDropDownType 별로 1개씩 있다고 픽스

        [Header("CeilingSlider")]
        [SerializeField]
        private Slider rareSlider = null;
        [SerializeField]
        private Slider uniqueSlider = null;


        [Header("ExplainLabel")]
        [SerializeField]
        private Text explainLabel = null;


        protected List<List<UserDragonCard>> selectCardsList = new List<List<UserDragonCard>>();//합성하기 위한 2차 배열 (10,4) 픽스
        protected bool selectDirty = true;

        protected List<UserCardBundle> viewCards = null;//현재 유저가 보유한 카드들
        protected bool viewDirty = true;
        protected bool isNetwork = false;
        protected Sequence tween = null;

        bool tableViewCardResetFlag = false;
        bool tableViewSlotResetFlag = false;

        bool isCompoundButtonClicked = false;
        bool isRewardTicketRequestClick = false;

        public delegate void func();

        private bool isTableInit = false;

        private bool isResetPopupOpen = false;
        //Init
        public override void Init()
        {
            if (cardTableView != null && compoundSlotTableView != null && !isTableInit)
            {
                cardTableView.OnStart();
                compoundSlotTableView.OnStart();
                isTableInit = true;
            }

            if(MERGE_SUCCESS_COUNT_UR < 0)
                MERGE_SUCCESS_COUNT_UR = GameConfigTable.GetMergeURSuccessCount();
            if(MERGE_SUCCESS_COUNT_SR < 0)
                MERGE_SUCCESS_COUNT_SR = GameConfigTable.GetMergeSRSuccessCount();

            isRewardTicketRequestClick = false;
            isCompoundButtonClicked = false;

            InitCards();
            InitSelects();
            InitCustomSort();
            SetProductionProcess();
        }

        void SetProductionProcess()
        {
            SetTween();
            SetVisibleEaterButton(false);
        }

        void SetTween()
        {
            if (tween != null)
            {
                tween.Kill();
            }
            tween = null;
        }

        void SetVisibleEaterButton(bool isVisible)
        {
            if (eater_button != null)
            {
                eater_button.gameObject.SetActive(isVisible);
            }
        }

        private void InitCustomSort()
        {
            viewDirty = true;
            tableViewCardResetFlag = true;
            ForceUpdate();
            dropdownController.InitDropDown();//일단 임시로 끄기
        }

        private void InitCards()
        {
            var dic = new Dictionary<int, UserCardBundle>();
            foreach (var card in GetUserDragonCompoundList())
            {
                if (IsContainCard(card))
                    continue;

                if (dic.ContainsKey(card.DragonTag))
                    dic[card.DragonTag].AddBundle(card);
                else
                    dic.Add(card.DragonTag, new UserCardBundle(card));
            }

            viewCards = new List<UserCardBundle>(dic.Values);
            viewDirty = true;
        }

        private void InitSelects()
        {
            selectCardsList.Clear();

            for(var i = 0; i < MaxCompoundSlotCount; i++)
                selectCardsList.Add(new List<UserDragonCard>());

            selectDirty = true;
            tableViewSlotResetFlag = true;
        }
        //Sort
        //최신 획득 내림 차순
        protected int SortObtainTimeDescend(UserDragonCard param_a, UserDragonCard param_b)
        {
            return param_b.CardTag - param_a.CardTag;
        }
        //최신 획득 오름 차순
        protected int SortObtainTimeAscend(UserDragonCard param_a, UserDragonCard param_b)
        {
            return param_a.CardTag - param_b.CardTag;
        }

        //Grade 내림 차순
        protected int SortHighGrade(UserDragonCard param_a, UserDragonCard param_b)
        {
            var value = GetCardByGrade(param_b) - GetCardByGrade(param_a);
            if (value == 0)
            {
                return SortTagAscend(param_a, param_b);
            }
            else
            {
                return value;
            }
        }

        //Grade 오름 차순
        protected int SortLowGrade(UserDragonCard param_a, UserDragonCard param_b)
        {
            var value = GetCardByGrade(param_a) - GetCardByGrade(param_b);
            if (value == 0)
            {
                return SortTagAscend(param_a, param_b);
            }
            else
            {
                return value;
            }
        }

        //드래곤 tag값 (key값) 내림 차순
        protected int SortTagDescend(UserDragonCard param_a, UserDragonCard param_b)
        {
            var value = param_b.DragonTag - param_a.DragonTag;
            if (value == 0)
                return SortCardTagDescend(param_a, param_b);
            else
                return value;
        }

        //드래곤 tag값 (key값) 오름 차순
        protected int SortTagAscend(UserDragonCard param_a, UserDragonCard param_b)
        {
            var value = param_a.DragonTag - param_b.DragonTag;
            if (value == 0)
                return SortCardTagAscend(param_a, param_b);
            else
                return value;
        }

        //카드 tag값 (key값) 내림 차순
        protected int SortCardTagDescend(UserDragonCard param_a, UserDragonCard param_b)
        {
            return param_b.CardTag - param_a.CardTag;
        }

        //카드 tag값 (key값) 오름 차순
        protected int SortCardTagAscend(UserDragonCard param_a, UserDragonCard param_b)
        {
            return param_a.CardTag - param_b.CardTag;
        }

        protected List<UserDragonCard> GetUserDragonCompoundList()
        {
            List<UserDragonCard> totalList = User.Instance.DragonCards.GetAllList();
            return totalList.ToList();
        }

        protected List<UserCardBundle> GetListCustomSort(int sortIndex)
        {
            var allDragondic = new Dictionary<int, UserCardBundle>();
            foreach (var card in GetUserDragonCompoundList())
            {
                if (IsContainCard(card))
                    continue;

                if (allDragondic.ContainsKey(card.DragonTag))
                    allDragondic[card.DragonTag].AddBundle(card);
                else
                    allDragondic.Add(card.DragonTag, new UserCardBundle(card));
            }
            var allDragonList = new List<UserCardBundle>(allDragondic.Values);
            if (dropdownController.GetDropdownIndex(eDropDownType.DEFAULT) != sortIndex)
            {
                viewDirty = true;
            }

            switch ((eSortType)sortIndex)
            {
                case eSortType.HighGrade:
                {
                    allDragonList.Sort(SortHighGrade);
                }
                break;
                case eSortType.LowGrade:
                {
                    allDragonList.Sort(SortLowGrade);
                }
                break;
                case eSortType.Normal:
                {
                    allDragonList = allDragonList.FindAll(element => (eDragonGrade)GetCardByGrade(element) == eDragonGrade.Normal);
                    allDragonList.Sort(SortTagAscend);
                }
                break;
                case eSortType.Rare:
                {
                    allDragonList = allDragonList.FindAll(element => (eDragonGrade)GetCardByGrade(element) == eDragonGrade.Uncommon);
                    allDragonList.Sort(SortTagAscend);
                }
                break;
                case eSortType.SuperRare:
                {
                    allDragonList = allDragonList.FindAll(element => (eDragonGrade)GetCardByGrade(element) == eDragonGrade.Rare);
                    allDragonList.Sort(SortTagAscend);
                }
                break;
                case eSortType.UltraRare:
                {
                    allDragonList = allDragonList.FindAll(element => (eDragonGrade)GetCardByGrade(element) == eDragonGrade.Unique);
                    allDragonList.Sort(SortTagAscend);
                }
                break;
                case eSortType.Legend:
                {
                    allDragonList = allDragonList.FindAll(element => (eDragonGrade)GetCardByGrade(element) == eDragonGrade.Legend);
                    allDragonList.Sort(SortTagAscend);
                }
                break;
                default: { } break;
            }

            return allDragonList;
        }

        private int GetCardByGrade(UserDragonCard card)
        {
            return card.CardGrade;
        }

        private bool IsCompoundCondition()//합성이 가능한지에 대한 컨디션 체크
        {
            if (selectCardsList == null)
                return false;

            int allClearCount = 0;
            foreach (var listData in selectCardsList)
            {
                if (listData == null)
                    continue;

                if (IsEmptySlot(listData))//아예 빈 덱
                {
                    allClearCount++;
                    continue;
                }

                if (!IsSlotFull(listData))//풀상태가 아님
                    return false;

                if (!IsEqualGradeList(listData))//등급 전부 같은지
                    return false;
            }
            
            return !(allClearCount == MaxCompoundSlotCount);
        }

        int GetAvailableSlotIndex(UserDragonCard _cardData)//현재 슬롯이 가득 차서 들어갈 수 있는 최적의 슬롯 넘버 찾기 -1리턴시 못찾음(가득 참)
        {
            if (selectCardsList == null || selectCardsList.Count <= 0)
                return -1;

            for(int i = 0; i< selectCardsList.Count; i++)
            {
                var list = selectCardsList[i];
                if (list == null)
                    continue;

                if (IsEqualCardGrade(list, _cardData) && !IsSlotFull(i))//현재 들어갈 등급이 같고, 가득 차지 않았다면 삽입 가능
                    return i;
            }

            return -1;
        }

        bool IsAllEmpty()
        {
            var emptyCount = 0;
            foreach (var dataList in selectCardsList)
            {
                if (dataList == null)
                    continue;
                if (IsEmptySlot(dataList))
                    emptyCount++;
            }

            return emptyCount == selectCardsList.Count;
        }

        bool IsEmptySlot(int _slotIndex)
        {
            var list = selectCardsList[_slotIndex];
            if (list != null && list.Count <= 0)
                return true;

            return false;
        }
        bool IsEmptySlot(List<UserDragonCard> _list)
        {
            return _list.Count == 0;
        }

        bool IsSlotFull(int _slotIndex)
        {
            if (selectCardsList == null || selectCardsList.Count <= 0 || selectCardsList.Count <= _slotIndex)
                return false;

            var list = selectCardsList[_slotIndex];
            if (list.Count == 0)
                return false;

            var mergeInfo = CharMergeBaseData.GetMergeDataByGrade(list[0].CardGrade);
            if (mergeInfo == null)
                return false;

            if (list == null || list.Count < mergeInfo.NEED_COUNT)
                return false;

            return true;
        }

        bool IsSlotFull(List<UserDragonCard> _list)
        {
            if (_list == null || _list.Count <= 0)
                return false;

            var mergeInfo = CharMergeBaseData.GetMergeDataByGrade(_list[0].CardGrade);
            if (mergeInfo == null)
                return false;

            if (_list.Count < mergeInfo.NEED_COUNT)
                return false;

            return true;
        }

        bool IsContainCard(UserDragonCard _cardData)
        {
            if (selectCardsList == null || selectCardsList.Count <= 0)
                return false;

            for (int i = 0; i < selectCardsList.Count; i++)
            {
                var list = selectCardsList[i];
                if (list == null)
                    continue;

                var isContain = list.Contains(_cardData);
                if (isContain)
                    return true;
            }
            return false;
        }

        void RemoveCardInList(UserDragonCard _cardData)
        {
            if (selectCardsList == null || selectCardsList.Count <= 0)
                return;
            
            for (int i = 0; i < selectCardsList.Count; i++)
            {
                var list = selectCardsList[i];
                if (list == null)
                    continue;

                if (list.Remove(_cardData))
                    break;
            }
        }

        void AddCardInList(UserDragonCard _cardData)
        {
            if (IsContainCard(_cardData))
            {
                //Debug.LogError("이미 존재하는 드래곤 카드 데이터 : " + _cardData);
                //ToastManager.On("이미 존재하는 카드 데이터 확인 필요 dragonTag : " + _cardData.DragonTag + "  카드태그 : " + _cardData.CardTag);
                ToastManager.On(100002546);//합성이 불가능한 드래곤카드 입니다.
                return;
            }

            var addIndex = GetAvailableSlotIndex(_cardData);
            if(addIndex >= 0 && addIndex < selectCardsList.Count)
                selectCardsList[addIndex].Add(_cardData);
            else
                ToastManager.On(StringData.GetStringByStrKey("합성추가오류"));
        }

        bool IsEqualCardGrade(List<UserDragonCard> _list, UserDragonCard _inputData)//이미 카드가 등록된 상태에서 신규 카드 넣을 때 해당 슬롯의 등급이 신규와 같은지
        {
            if (_list.Count <= 0)
                return true;

            var inputGrade = _inputData.CardGrade;
            var listCount = _list.Count;
            var checkList = _list.FindAll(element => element.CardGrade == inputGrade);

            return checkList.Count == listCount;
        }

        bool IsEqualGradeList(List<UserDragonCard> _list)//리스트내부의 등급이 전부 동일한지
        {
            if (_list.Count <= 0)//들어가지 않았다면 넘기기
                return true;

            var mergeInfo = CharMergeBaseData.GetMergeDataByGrade(_list[0].CardGrade);
            if (mergeInfo == null)
                return false;

            if (_list.Count != mergeInfo.NEED_COUNT)
                return false;

            var listCount = _list.Select(x => x.CardGrade).Distinct().ToList();//등급이 모두 같다는 가정하에, 중복제거를 하면 size 1
            return listCount.Count == 1;
        }

        public void OnFrameClick(DragonCardFrame frame, UserDragonCard cardData)//카드 리스트에서 등록 및 해제
        {
            if (frame == null || cardData == null || selectCardsList == null)
            {
                return;
            }

            var find = IsContainCard(cardData);
            if (find)
            {
                RemoveCardInList(cardData);
                //frame.SetSelect(false);
                UserCardBundle bundle = null;
                foreach (var card in viewCards)
                {
                    if(card.DragonTag == cardData.DragonTag)
                    {
                        card.AddBundle(card);
                        bundle = card;
                        break;
                    }
                }
                if (bundle == null)
                {
                    bundle = new UserCardBundle(cardData);
                    viewCards.Add(bundle);                    
                }

                frame.InitCardFrame(bundle.card, false, false, bundle.amount);
            }
            else
            {
                var availableIndex = GetAvailableSlotIndex(cardData);
                if(availableIndex >= 0)
                {
                    AddCardInList(cardData);
                    //frame.SetSelect(true);
                    UserCardBundle bundle = null;
                    foreach (var card in viewCards)
                    {
                        if (card.DragonTag == cardData.DragonTag)
                        {
                            bundle = card;
                            break;
                        }
                    }
                    if (bundle != null)
                    {
                        bundle.DelBundle(cardData);

                        if (bundle.amount <= 0)
                        {
                            viewCards.Remove(bundle);
                            viewDirty = true;
                            tableViewCardResetFlag = false;
                            RefreshCardList();
                        }
                        else
                        {
                            frame.InitCardFrame(bundle.card, false, true, bundle.amount);
                        }
                    }
                }
                else
                {
                    ToastManager.On(StringData.GetStringByStrKey("합성추가오류"));
                    return;
                }
            }

            selectDirty = true;
            tableViewCardResetFlag = false;
            tableViewSlotResetFlag = false;
            ForceUpdate();
        }

        public void OnSelectClick(DragonCardFrame frame, UserDragonCard cardData)//등록된 슬롯에서 드래곤 프레임 클릭할 때
        {
            if (frame == null || cardData == null || selectCardsList == null)
                return;

            RemoveCardInList(cardData);
            viewDirty = true;
            selectDirty = true;
            tableViewCardResetFlag = false;
            tableViewSlotResetFlag = false;
            ForceUpdate();
        }

        void RefreshCount()
        {
            if (countLabel == null)
            {
                return;
            }

            var text = StringData.GetStringByIndex(100001252);
            countLabel.text = string.Format(text, GetUserDragonCompoundList().Count);
        }

        void RefreshUI()
        {
            if (resultBtn == null)
                return;

            resultBtn.SetButtonSpriteState(IsCompoundCondition());
            resetBtn.SetButtonSpriteState(!IsAllEmpty());
            addBtn.SetButtonSpriteState(IsAutoCompoundCondition());

            RefreshCeilingAnimation();
            RefreshCeilingLabel();
            RefreshCeilingSlider();
            RefreshExplainLabel();
        }

        void RefreshCeilingAnimation()
        {
            var srAvail = User.Instance.PrefData.DragonSRCeilingCount >= MERGE_SUCCESS_COUNT_SR;
            if (sr_ticket_anim != null)
            {
                if (srAvail)
                    sr_ticket_anim.DOPlay();
                else
                    sr_ticket_anim.DOPause();
            }

            var urAvail = User.Instance.PrefData.DragonURCeilingCount >= MERGE_SUCCESS_COUNT_UR;
            if (ur_ticket_anim != null)
            {
                if (urAvail)
                    ur_ticket_anim.DOPlay();
                else
                    ur_ticket_anim.DOPause();
            }
        }

        void RefreshCeilingLabel()//천장 카운트 체크
        {
            var upperColorHexStr = "<color=#FFE100>";
            var underHexColor = "<color=#FF0000>";

            if (ur_ceilingLabel != null)
            {
                var prefix = User.Instance.PrefData.DragonURCeilingCount < MERGE_SUCCESS_COUNT_UR ? underHexColor : upperColorHexStr;
                ur_ceilingLabel.text = string.Format("{0}/{1}", prefix + User.Instance.PrefData.DragonURCeilingCount + "</color>", MERGE_SUCCESS_COUNT_UR);
            }
                
            if (sr_ceilingLabel != null)
            {
                var prefix = User.Instance.PrefData.DragonSRCeilingCount < MERGE_SUCCESS_COUNT_SR ? underHexColor : upperColorHexStr;
                sr_ceilingLabel.text = string.Format("{0}/{1}", prefix + User.Instance.PrefData.DragonSRCeilingCount + "</color>", MERGE_SUCCESS_COUNT_SR);
            }
        }

        void RefreshCardList()
        {
            if (!viewDirty || viewCards == null)
            {
                return;
            }

            if (cardTableView == null)
            {
                return;
            }

            emptyCardLabel.gameObject.SetActive(!(viewCards.Count > 0));

            List<ITableData> tableViewItemList = new List<ITableData>();
            if (viewCards != null && viewCards.Count > 0)
            {
                foreach(var card in viewCards)
                {
                    if (card == null)
                        continue;

                    tableViewItemList.Add(card);
                }
            }

            cardTableView.SetDelegate(new TableViewDelegate(tableViewItemList, SetTableElementData));

            cardTableView.ReLoad(tableViewCardResetFlag);
            viewDirty = false;
            tableViewCardResetFlag = true;
        }

        void SetTableElementData(GameObject node, ITableData item)
        {
            if (node == null)
            {
                return;
            }
            var frame = node.GetComponent<DragonCardFrame>();
            if (frame == null)
            {
                return;
            }

            var bundle = (UserCardBundle)item;
            var card = bundle.card;

            var isSelect = IsContainCard(card);
            frame.InitCardFrame(card, isSelect, true, bundle.amount);
            //frame.SetSelect(isSelect);

            if (frame.ClickCallBack == null)
            {
                frame.ClickCallBack = OnFrameClick;
            }
        }

        protected void RefreshSelect()//selectCardsList 기반으로 tableView 세팅하는 곳
        {
            if (!selectDirty || selectCardsList == null || cardTableView == null)
            {
                return;
            }

            List<ITableData> tableViewItemList = new List<ITableData>();
            tableViewItemList.Clear();
            if (selectCardsList != null && selectCardsList.Count > 0)
            {
                for (var i = 0; i < selectCardsList.Count; i++)
                {
                    var data = selectCardsList[i];
                    if (data == null)
                        continue;

                    tableViewItemList.Add(new UserDragonSelectCardSlot(data));
                }
            }

            compoundSlotTableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
                if (node == null)
                    return;
                
                var frame = node.GetComponent<DragonCompoundSlot>();
                if (frame == null)
                {
                    return;
                }

                var cardList = (UserDragonSelectCardSlot)item;//선택된 드래곤 카드 데이터
                frame.Init(cardList, (frame, cardData) =>
                {
                    OnSelectClick(frame, cardData);
                });

            }));

            compoundSlotTableView.ReLoad(tableViewSlotResetFlag);
            selectDirty = false;
            tableViewSlotResetFlag = true;
        }

        public override void ForceUpdate()
        {
            SortCards();
            RefreshCardList();
            RefreshSelect();
            RefreshCount();
            RefreshUI();
        }
        /*
         * 자동 삽입 기본 로직
         * 일반 -> 고급 -> 희귀 -> 영웅 순으로 등록
         * 동일 등급 내 드래곤 KEY값 순으로 순차적 등록
         * 동일 드래곤인 경우 획득 시간이 빠른 순으로 등록
         * 
         * 기획 변경 (05-25 WJ)
         * 기존 : 가장 등급 낮은 것부터 오름차순 최대 삽입
         * 변경 : 가장 등급 낮은 것부터 등급 같은 것만 삽입
         * ex) N등급 4장 R등급 12장 -> N등급만 4장 삽입
         */
        /*
         * 기획 변경 (07-20 WJ)
         * 기존 : 최대 유니크 까지만 표시
         * 변경 : 레전더리 추가
         */

        void AutoSetdragonCompoundData()
        {
            var dragonSortIndex = dropdownController.GetDropdownIndex(eDropDownType.DEFAULT);
            var elementSortIndex = dropdownController.GetDropdownIndex(eDropDownType.ELEMENT);

            var list = GetUserDragonCompoundList();
            if (list == null)
                return;

            list.Sort(SortLowGrade);
            var originSortList = list.ToList();

            var successGrade = GetAutoCompoundAvailableGrade();//조합 가능한 등급을 선체크 - 없으면 -1 리턴

            bool isRarityIndex = IsSelectRarityDropDownIndex();//등급 필터링을 선택 했을 때 해당 등급만 합성리스트에 올리기
            int rarityIndex = -1;
            if (isRarityIndex)
                rarityIndex = GetSelectRarityIndex();//eDragonGrade 보정

            for (int k = 0; k < MaxCompoundSlotCount; k++)
            {
                var currentCardList = selectCardsList[k];
                var currentCardCount = currentCardList.Count;
                list = originSortList.ToList();
                var success = false;
                if (currentCardCount <= 0)
                {
                    for (var i = eDragonGrade.Normal; i <= eDragonGrade.Legend; ++i)
                    {
                        if (rarityIndex > 0 && (int)i != rarityIndex)
                            continue;

                        var countlist = list.FindAll(element => (eDragonGrade)GetCardByGrade(element) == i && !IsContainCard(element));
                        
                        var mergeInfo = CharMergeBaseData.GetMergeDataByGrade((int)i);
                        if (mergeInfo == null)
                            continue;

                        if ((countlist.Count + currentCardCount) >= mergeInfo.NEED_COUNT)
                        {
                            list = countlist;
                            success = true;
                            break;
                        }
                    }
                }
                else
                {
                    var grade = GetCardByGrade(currentCardList[0]);
                    if (grade < (int)eDragonGrade.Normal || grade > (int)eDragonGrade.Legend)
                    {
                        ToastManager.On(100002546);
                    }
                    else
                    {
                        list = list.FindAll(element => GetCardByGrade(element) == grade && !IsContainCard(element));
                        if (list.Count > 0)
                        {
                            var mergeInfo = CharMergeBaseData.GetMergeDataByGrade(list[0].CardGrade);
                            if (mergeInfo != null)
                            {
                                success = (list.Count + currentCardCount) >= mergeInfo.NEED_COUNT;
                            }
                        }
                    }
                }

                bool isNonGradeCondition = successGrade > 0 && list.Count > 0 &&  successGrade == list[0]?.CardGrade;
                if (isRarityIndex)
                    isNonGradeCondition = true;

                if (success && list.Count > 0 && isNonGradeCondition)
                {
                    var cardCount = currentCardList.Count;

                    for (var g = eDragonGrade.Normal; g <= eDragonGrade.Legend; g++)
                    {
                        var mergeInfo = CharMergeBaseData.GetMergeDataByGrade((int)g);
                        var remainCount = mergeInfo.NEED_COUNT - cardCount;

                        for (var i = 0; i < remainCount; i++)
                        {
                            if (i >= list.Count)
                                continue;
                            var listData = list[i];
                            if (listData == null)
                                continue;
                            if (listData.CardGrade != (int)g)
                                continue;

                            if (!IsContainCard(listData))
                                AddCardInList(listData);
                        }
                    }
                }
            }

            viewDirty = true;
            selectDirty = true;
        }

        bool IsSelectRarityDropDownIndex()//드롭다운 옵션에서 등급만 선택했는지 
        {
            var currentIndex = dropdownController.GetDropdownIndex(eDropDownType.DEFAULT);
            return currentIndex > 1 && currentIndex <= 6;
        }

        int GetSelectRarityIndex()//현재 선택한 드롭다운 인덱스를 등급으로 변환
        {
            return dropdownController.GetDropdownIndex(eDropDownType.DEFAULT) - 1;
        }    

        int GetAutoCompoundCount()//현재 유저 보유 카드 기준으로 조합이 완성 될 수 있는 갯수를 미리 체크 - 등록 슬롯은 고정이라는 전제
        {
            var totalAvailableCompoundCount = 0;
            var userTotalCardList = GetUserDragonCompoundList().ToList();
            for (var i = eDragonGrade.Legend; i >= eDragonGrade.Normal; --i)
            {
                var countlist = userTotalCardList.FindAll(element => (eDragonGrade)GetCardByGrade(element) == i);//등급
                var mergeInfo = CharMergeBaseData.GetMergeDataByGrade((int)i);
                totalAvailableCompoundCount += countlist.Count / mergeInfo.NEED_COUNT;
            }

            return totalAvailableCompoundCount;
        }
        int GetCountCompoundSpecificGrade(int _grade)//특정 등급에 합성 가능 갯수 구하기
        {
            var userTotalCardList = GetUserDragonCompoundList().ToList();
            var countlist = userTotalCardList.FindAll(element => GetCardByGrade(element) == _grade);//등급
            var mergeInfo = CharMergeBaseData.GetMergeDataByGrade(_grade);
            return countlist.Count / mergeInfo.NEED_COUNT;
        }
        int GetAutoCompoundAvailableGrade()//현재 유저 보유 카드 기준으로 조합이 완성 될 수 있는 카드 등급 체크
        {
            var userTotalCardList = GetUserDragonCompoundList().ToList();
            for (var i = eDragonGrade.Normal; i <= eDragonGrade.Legend; i++)
            {
                var countlist = userTotalCardList.FindAll(element => (eDragonGrade)GetCardByGrade(element) == i);//등급
                var mergeInfo = CharMergeBaseData.GetMergeDataByGrade((int)i);
                if (countlist.Count / mergeInfo.NEED_COUNT > 0)
                    return (int)i;
            }

            return -1;
        }

        int GetCurrentFullSlotCount()//전부 채워진 슬롯 갯수체크
        {
            var listSize = selectCardsList.Count;
            if (MaxCompoundSlotCount != listSize)
            {
                Debug.LogWarning("합성 슬롯 데이터 리스트 오류 확인할 것");
                return -1;
            }

            int fullSlotCount = 0;
            for(int i = 0; i< listSize; i++)
            {
                var cardList = selectCardsList[i];

                if (IsSlotFull(cardList))
                    fullSlotCount++;
            }

            return fullSlotCount;
        }


        bool IsAutoCompoundCondition(bool _isToastOn = false)
        {
            if (selectCardsList == null)
                return false;

            var currentFullSlotCount = GetCurrentFullSlotCount();//현재 채워진 슬롯 카운트
            var availableCompoundCount = GetAutoCompoundCount();//합성 가능한 최대 갯수
            if (availableCompoundCount >= MaxCompoundSlotCount)//슬롯 최대치보다 더 많은 조합 가능하면 
                availableCompoundCount = MaxCompoundSlotCount;

            if (IsSelectRarityDropDownIndex())//등급 선택소팅을 한 상태
            {
                var currentGrade = GetSelectRarityIndex();
                var compoundCount = GetCountCompoundSpecificGrade(currentGrade);
                if (compoundCount <= 0)
                {
                    if (_isToastOn)
                        ToastManager.On(StringData.GetStringByStrKey("합성부족오류"));
                    return false;
                }
                else
                {
                    if (currentFullSlotCount == compoundCount)
                    {
                        if (_isToastOn)
                            ToastManager.On(StringData.GetStringByStrKey("합성등록오류"));
                        return false;
                    }
                }
            }
            
            if (availableCompoundCount <= 0)
            {
                if(_isToastOn)
                    ToastManager.On(StringData.GetStringByStrKey("합성부족오류"));
                return false;
            }

            if (currentFullSlotCount == availableCompoundCount)//현재 채워진슬롯 == 최대 조합 카운트면 더이상 못함
            {
                if (_isToastOn)
                    ToastManager.On(StringData.GetStringByStrKey("합성등록오류"));
                return false;
            }

            return true;
        }

        //OnClick
        //현재 full 상태는 아니지만, 등록할 카드가 더 이상 없으면 - full 상태 아닌 슬롯찾아서 현재 등록안된 카드 잔여 갯수 확인
        public void OnClickAuto()//현재 등록된 것을 전부 밀고 다시 세팅하는지 확인
        {
            if (!IsAutoCompoundCondition(true))
                return;

            AutoSetdragonCompoundData();

            tableViewSlotResetFlag = false;
            tableViewCardResetFlag = false;
            ForceUpdate();
        }

        public void OnClickReset()
        {
            if (isCompoundButtonClicked)
                return;

            if (IsAllEmpty())
            {
                ToastManager.On(100001169);
                return;
            }

            isResetPopupOpen = true;
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("등록카드초기화"),
                () => {
                    //ok
                    DataAndUIReset();
                    isResetPopupOpen = false;
                },
                () => {
                    //cancel
                    isResetPopupOpen = false;
                },
                () => {
                    //x
                    isResetPopupOpen = false;
                });
        }

        void DataAndUIReset()
        {
            if (selectCardsList == null)
                return;

            int allClearCount = 0;
            foreach (var listData in selectCardsList)
            {
                if (listData == null)
                    continue;

                if (IsEmptySlot(listData))//아예 빈 덱
                {
                    allClearCount++;
                }
            }

            if (allClearCount == MaxCompoundSlotCount)
                return;

            InitCards();//초기화 - 안쪽에 테이블 위치 초기화 플래그 들어가 있는데 어색하면 플래그 세팅 안하게 수정
            InitSelects();
            InitCustomSort();
        }

        public void OnClickMerge()
        {
            if (isCompoundButtonClicked)
            {
                return;
            }

            if (selectCardsList == null || selectCardsList.Count <= 0)
            {
                return;
            }

            if(!IsCompoundCondition())
            {
                ToastManager.On(StringData.GetStringByStrKey("합성부족오류"));
                return;
            }

            var constraintCheck = isAlertCondition();
            if (constraintCheck)
            {
                ShowAlertPopup(COMPOUND_CONSTRAINT_GOAL_TIME, RequestDragonMerge, () => {
                    resultBtn.SetButtonSpriteState(true);
                    isCompoundButtonClicked = false;
                });
            }
            else
            {
                RequestDragonMerge();
            }
        }

        void RequestDragonMerge()
        {
            if (isResetPopupOpen)
                return;

            if (isNetwork || selectCardsList == null)
                return;

            var compoundCost = GetCompoundTotalCost();//조합요청시 드는 비용
            if(compoundCost > User.Instance.GOLD)
            {
                ToastManager.On(100000104);//골드가 부족합니다.
                return;
            }

            const int MaxDragonCompoundCount = 4;
            //list serialize 안되면 이걸로 확인 -> List<List<int>> 형태는 JsonConvert.SerializeObject 동작 안해서 강제 2중 배열로 변경
            var currentFullSlotCount = GetCurrentFullSlotCount();//현재 채워진 슬롯 카운트
            int[,] cardListArr = new int[currentFullSlotCount, MaxDragonCompoundCount];
            int availableIndex = 0;
            for (int i = 0; i < selectCardsList.Count; i++)
            {
                var cardSlotListData = selectCardsList[i];
                if (IsEmptySlot(cardSlotListData) || !IsSlotFull(cardSlotListData))//빈 슬롯이거나, 풀이 아니면 
                    continue;

                if (!IsEqualGradeList(cardSlotListData))//등급이 모두 같은지
                    continue;

                var cidArr = cardSlotListData.Select(x => x.CardTag).ToArray();
                for (int k = 0; k < cidArr.Length; k++)
                {
                    cardListArr[availableIndex, k] = cidArr[k];
                }
                availableIndex++;
            }

            if (currentFullSlotCount <= 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("합성부족오류"));
                return;
            }
            
            var data = new WWWForm();
            data.AddField("dcid_list", JsonConvert.SerializeObject(cardListArr));

            isCompoundButtonClicked = true;
            isNetwork = true;
            NetworkManager.Send("dragon/merge", data, ResponseMerge, ResponseFail);
        }

        /**
         * @param param 소트 타입 으로 제공
         * 0 : all
         * 1 : fire
         * 2 : water
         * 3 : soil
         * 4 : wind
         * 5 : light
         * 6 : dark
         */
        public void OnClickElementSort(string customEventData)
        {
            var checker = int.Parse(customEventData);
            if (dropdownController.GetDropdownIndex(eDropDownType.ELEMENT) == checker)
            {
                dropdownController.SetDropDownVisible(eDropDownType.ELEMENT, false);
                return;
            }

            viewDirty = true;
            dropdownController.SetDropdownIndex(eDropDownType.ELEMENT, checker);
            dropdownController.InitDropDown();//일단 임시로 끄기
            tableViewCardResetFlag = true;
            ForceUpdate();
        }
        /**
         * 
         * @param param 
         * @param customEventData 
         * 
         * 정렬 타입
         * 0 : 최신 획득 내림 차순
         * 1 : 최신 획득 오름 차순
         */
        public void OnClickCustomSort(string customEventData)
        {
            var checker = int.Parse(customEventData);
            if (dropdownController.GetDropdownIndex(eDropDownType.DEFAULT) == checker)
            {
                dropdownController.SetDropDownVisible(eDropDownType.DEFAULT, false);
                return;
            }

            viewDirty = true;
            dropdownController.SetDropdownIndex(eDropDownType.DEFAULT, checker);
            dropdownController.InitDropDown();//일단 임시로 끄기
            tableViewCardResetFlag = true;
            ForceUpdate();
        }

        protected void SortCards()
        {
            dropdownController.RefreshAllFilterLabel();

            var dragonSortIndex = dropdownController.GetDropdownIndex(eDropDownType.DEFAULT);
            var elementSortIndex = dropdownController.GetDropdownIndex(eDropDownType.ELEMENT);

            viewCards = GetListCustomSort(dragonSortIndex);

            if (elementSortIndex > 0 && (eElementType)elementSortIndex < eElementType.MAX)
            {
                viewCards = viewCards.FindAll((element) => {
                    var charData = CharBaseData.Get(element.DragonTag.ToString());
                    if (charData == null)
                    {
                        return false;
                    }
                    return charData.ELEMENT == elementSortIndex;
                });
                viewDirty = true;
            }
        }

        //Network
        protected void ResponseMerge(JObject jsonData)
        {
            if (jsonData == null)
            {
                isCompoundButtonClicked = false;
                isNetwork = false;
                return;
            }

            if (jsonData.ContainsKey("rs") && (eApiResCode)jsonData["rs"].Value<int>() != eApiResCode.OK)
            {
                isNetwork = false;
                ResponseMessage(jsonData["rs"].Value<int>());
                SetVisibleEaterButton(false);
                isCompoundButtonClicked = false;
                RefreshUI();
                return;
            }

            SetVisibleEaterButton(true);

            if (tween != null)
                tween.Kill();

            SetCeilingCount(jsonData);

            tween = DOTween.Sequence();
            tween.AppendCallback(() =>
                {
                    ShowResultPopup(jsonData);
                    SetProductionProcess();
                });

            tween.Play();

            isCompoundButtonClicked = false;
        }

        void SetCeilingCount(JObject jsonData)
        {
            if (jsonData.ContainsKey("ur_d_merge_cnt"))
                User.Instance.PrefData.SetDragonCompoundCeilingCount("ur_d_merge_cnt", jsonData["ur_d_merge_cnt"].Value<int>());
            if (jsonData.ContainsKey("sr_d_merge_cnt"))
                User.Instance.PrefData.SetDragonCompoundCeilingCount("sr_d_merge_cnt", jsonData["sr_d_merge_cnt"].Value<int>());
        }

        void ShowResultPopup(JObject jsonData)
        {
            if (!jsonData.ContainsKey("result"))
            {
                Debug.LogWarning("result Field 없음 확인 필수");
                return;
            }

            List<DragonCompoundInfoData> compoundList = new List<DragonCompoundInfoData>();

            JArray current = (JArray)jsonData["result"];
            foreach (JToken token in current)
            {
                JObject datas = (JObject)token;

                DragonCompoundInfoData info = null;
                if (datas.ContainsKey("isNew") && datas.ContainsKey("upgrade") && datas.ContainsKey("did"))
                {
                    var isNew = datas["isNew"].Value<int>() == 1 ? true : false;
                    var isSuccess = datas["upgrade"].Value<int>() == 1 ? true : false;
                    var dragonID = datas["did"].Value<int>();

                    info = new DragonCompoundInfoData(dragonID,isSuccess,isNew);
                }

                if (info != null)
                    compoundList.Add(info);
            }

            if (compoundList.Count <= 0)
                return;
            
            DragonCompoundResultPopupData newPopupData = new DragonCompoundResultPopupData(compoundList, SuccessResponseRefreshUI);
            PopupManager.OpenPopup<DragonCompoundResultPopup>(newPopupData);
        }

        void SuccessResponseRefreshUI()
        {
            viewDirty = true;
            isNetwork = false;
            InitSelects();
            var dic = new Dictionary<int, UserCardBundle>();
            foreach (var card in GetUserDragonCompoundList())
            {
                if (IsContainCard(card))
                    continue;

                if (dic.ContainsKey(card.DragonTag))
                    dic[card.DragonTag].AddBundle(card);
                else
                    dic.Add(card.DragonTag, new UserCardBundle(card));
            }

            viewCards = new List<UserCardBundle>(dic.Values);
            ForceUpdate();
        }

        protected void ResponseMessage(int resCode)
        {
            switch ((eApiResCode)resCode)
            {
                // 드래곤 보유하지 않음
                case eApiResCode.DRA_NO_SUCH_DRAGON:
                {
                    ToastManager.On(100002548);
                }
                break;
                // 합성 재료로 요청한 카드 미보유
                case eApiResCode.DRA_MERGE_NO_SUCH_CARD:
                {
                    ToastManager.On(100002547);
                }
                break;
                // 합성 재료로 요청한 카드 조합 불가
                case eApiResCode.DRA_MERGE_INVALID_CARDS:
                {
                    ToastManager.On(100002544);
                }
                break;
            }
        }

        protected void ResponseFail(string data = "")
        {
            isNetwork = false;
            SetVisibleEaterButton(false);
            isCompoundButtonClicked = false;
            RefreshUI();
        }

        bool isAlertCondition()//등급 중에서 하나라도 넘어서는 것이 있다면
        {
            var constraintGrade = int.Parse(GameConfigTable.GetConfigValue("DRAGON_MERGE_GRADE_NOTIE_STANDARD"));
            foreach (var listData in selectCardsList)
            {
                if (listData == null)
                    continue;

                foreach(var cardData in listData)
                {
                    if (cardData == null)
                        continue;
                    if (constraintGrade <= cardData.CardGrade)
                        return true;
                }
            }
            return false;
        }

        void ShowAlertPopup(string checkFlag, func ok_cb = null, func cancel_cb = null)//합성 재료 중에 드래곤 grade 제한에 걸리는 상황일 때 뜨는 팝업
        {
            var valueCheck = SBFunc.HasTimeValue(checkFlag);
            if (valueCheck)
            {
                if (ok_cb != null)
                {
                    ok_cb();
                }
                return;//하루동안 보이지 않기 on
            }

            var popup = PopupManager.OpenPopup<DragonCompoundConstraintPopup>();
            popup.setCallback(() => 
            {
                //예 - 토글 상태 (하루동안 보이지않기) 체크는 예 일때만 판단 (기획)
                //쿠키 세팅
                var checkValue = popup.toggle.isOn;
                if(checkValue)
                {
                    SBFunc.SetTimeValue(checkFlag);
                }

                if (ok_cb != null)
                {
                    ok_cb();
                }

                popup.ClosePopup();
            },() => 
            {
                if(cancel_cb != null)
                {
                    cancel_cb();
                }
                popup.ClosePopup();
            },() => 
            {
                if(cancel_cb != null)
                {
                    cancel_cb();
                }
                popup.ClosePopup();
            });
        }

        int GetCompoundTotalCost()//현재 선택된 드래곤 슬롯(풀상태) 일 때의 드는 비용
        {
            if (selectCardsList == null)
                return 0;

            int totalCompoundCost = 0;
            foreach (var listData in selectCardsList)
            {
                if (listData == null)
                    continue;

                if (IsEmptySlot(listData))//아예 빈 덱
                    continue;

                if (!IsSlotFull(listData))//풀상태가 아님
                    continue;

                if (!IsEqualGradeList(listData))//등급 전부 같은지
                    continue;

                var mergeData = CharMergeBaseData.GetMergeDataByGrade((int)listData[0].CardGrade);
                if (mergeData != null)
                    totalCompoundCost += mergeData.COST_VALUE;
            }

            return totalCompoundCost;
        }

        void RefreshCeilingSlider()//유니크와 레어 프로그레스 갱신
        {
            //rare
            rareSlider.maxValue = MERGE_SUCCESS_COUNT_SR;
            rareSlider.value = User.Instance.PrefData.DragonSRCeilingCount;
            //unique
            uniqueSlider.maxValue = MERGE_SUCCESS_COUNT_UR;
            uniqueSlider.value = User.Instance.PrefData.DragonURCeilingCount;
        }

        void RefreshExplainLabel()
        {
            if (explainLabel == null)
                return;

            var defaultStringData = StringData.GetStringByStrKey("드래곤합성안내");
            var legendaryStringData = StringData.GetStringByStrKey("드래곤합성안내2");
            if (selectCardsList == null || selectCardsList.Count <= 0)
            {
                explainLabel.text = defaultStringData;
                return;
            }

            bool isLegendaryDeck = false;
            foreach(var deckDataList in selectCardsList)
            {
                if (deckDataList == null)
                    continue;

                if (deckDataList.Count <= 0)
                    continue;

                var card = deckDataList[0];
                if (card == null)
                    continue;

                var grade = card.CardGrade;
                if (grade != (int)eDragonGrade.Legend)
                    continue;

                var deckCount = deckDataList.Count;
                var mergeInfo = CharMergeBaseData.GetMergeDataByGrade(grade);
                if (mergeInfo != null)
                {
                    var maxcount = mergeInfo.NEED_COUNT;
                    if(maxcount <= deckCount)
                    {
                        isLegendaryDeck = true;
                        break;
                    }
                }
                else
                    continue;
            }

            explainLabel.text = isLegendaryDeck ? legendaryStringData : defaultStringData;
        }

        public void OnClickGetTicketCount(string _param)//SR : 3 // UR : 4
        {
            bool isFullFillCount = true;
            var expectTicketCount = 0;
            var itemNo = 0;
            switch (_param)
            {
                case "3":
                    isFullFillCount = MERGE_SUCCESS_COUNT_SR <= User.Instance.PrefData.DragonSRCeilingCount;
                    expectTicketCount = User.Instance.PrefData.DragonSRCeilingCount / MERGE_SUCCESS_COUNT_SR;
                    itemNo = UNIQUE_GACHA_TICKET_ITEM_NO;
                    break;
                case "4":
                    isFullFillCount = MERGE_SUCCESS_COUNT_UR <= User.Instance.PrefData.DragonURCeilingCount;
                    expectTicketCount = User.Instance.PrefData.DragonURCeilingCount / MERGE_SUCCESS_COUNT_UR;
                    itemNo = LEGENDARY_GACHA_TICKET_ITEM_NO;
                    break;
            }    


            if(!isFullFillCount)
            {
                ToastManager.On(100000106);
                return;
            }

            if (expectTicketCount <= 0 || itemNo <= 0)
                return;

            //가방 빈공간 체크
            var tempAssetList = new List<Asset>() { new Asset(itemNo, expectTicketCount) };
            if (User.Instance.CheckInventoryGetItem(tempAssetList))
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                    () => {
                        PopupManager.AllClosePopup();
                        PopupManager.OpenPopup<InventoryPopup>();
                    }
                );
                return;
            }

            if (isRewardTicketRequestClick)
            {
                return;
            }

            isRewardTicketRequestClick = true;

            //합성 요청
            var param = new WWWForm();
            param.AddField("grade", _param);

            NetworkManager.Send("dragon/pity", param, (jsonObj) =>
            {
                var data = jsonObj;
                var isSuccess = (data["err"].Value<int>() == 0);
                if(data.ContainsKey("rs"))
                {
                    var rs = (eApiResCode)data["rs"].Value<int>();
                    switch (rs)
                    {
                        case eApiResCode.OK:
                        {
                            if (isSuccess && data.ContainsKey("rewards"))
                            {
                                SetCeilingCount(data);//서버 데이터 기준 값 갱신
                                RefreshUI();//UI갱신

                                SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(data["rewards"])));
                            }
                        }
                        break;
                    }
                }
                
                isRewardTicketRequestClick = false;
            });
        }


        public override bool backBtnCall()
        {
            if (backBtn != null)
            {
                backBtn.onClick.Invoke();
                return true;
            }
            return false;
        }
    }
}

