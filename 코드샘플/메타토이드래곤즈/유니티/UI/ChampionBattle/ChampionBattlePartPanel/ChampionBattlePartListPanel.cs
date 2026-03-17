using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


//각 컨텐츠(정보, 강화, 분해, 합성)에 따라서 리스트를 공용으로 쓰려고 함.
//DragonPartLayer 에서 PartListViewType type을 넘겨주면, 해당 상태에 따라 리스트를 새로 만듦.
namespace SandboxNetwork
{
    public class ChampionBattlePartListPanel : MonoBehaviour, EventListener<DragonPartEvent>
    {
        const int PartGradeLimit = 5;//합성 버튼이 4 이하 일때만 적용(레전더리 이상일때는 합성 비활성화)

        [Header("DropDown")]
        [SerializeField]
        private DropDownUIController dropdownController = null;

        [SerializeField]
        TableViewGrid tableView = null;
        
        [SerializeField]
        GameObject nodeInvenContent = null;

        [SerializeField]
        Text invenCheckLabel = null;

        [SerializeField]
        ChampionBattlePartButtonController buttonController = null;


        ChampionBattlePartLayer partLayer = null;

        List<ChampionBattlePartInfo> partInfoList = new List<ChampionBattlePartInfo>(); //실제 테이블 뷰 데이터 리스트
        List<UserPart> currentPartList = new List<UserPart>();

        private bool isTableInit = false;

        private ChampionBattlePartListViewType currentType = ChampionBattlePartListViewType.DEFAULT;

        FilterPopupData compoundFilterData = null;
        FilterPopupData decomposeFilterData = null;

        bool isFirstViewReloadCheck = false;

        public void OnEnable()
        {
            EventManager.AddListener(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }
        public void OnEvent(DragonPartEvent eventType)
        {
            switch (eventType.Event)
            {
                case DragonPartEvent.DragonPartEventEnum.SetListType://리스트 타입 세팅 (모든 서브 팝업 열기 전 항상 세팅)
                {
                    var eventIndex = (ChampionBattlePartListViewType)eventType.listTypeIndex;
                    if (eventIndex != currentType)
                        isFirstViewReloadCheck = true;
                    else
                        isFirstViewReloadCheck = false;

                    currentType = eventIndex;

                    if (partLayer != null)
                        partLayer.CurrentViewType = currentType;

                    //RefreshButtonVisible();//현재 타입 기반으로 세팅
                }
                break;
            }
        }

        void RefreshButtonVisible()//현재 타입 기반으로 버튼 상태 on/off
        {
            //if (buttonController != null)
            //    buttonController.SetVisibleButtonVisibleByType(currentType);
        }

        public void Init(ChampionBattlePartLayer _partLayer)//현재 타입에 따라서 리스트 방식 계산 달리해야함
        {
            if (_partLayer != null)
                partLayer = _partLayer;

            InitTableView();
            InitDropDown();
            InitCustomSort();
        }

        void InitTableView()
        {
            if (tableView != null && !isTableInit)
            {
                tableView.OnStart();
                isTableInit = true;
            }
        }
        void InitDropDown()
        {
            if (dropdownController != null)
                dropdownController.InitDropDown();
        }

        public void InitCustomSort()
        {
            OnClickCustomSort("0");
        }

        public void SetData(ChampionBattlePartLayer _parent)
        {
            partLayer = _parent;
        }

        public void RefreshList(bool reloadPos)
        {
            var currentSortIndex = dropdownController.GetDropdownIndex(eDropDownType.DEFAULT);
            
            dropdownController.SetDropdownIndex(eDropDownType.DEFAULT, currentSortIndex);
            dropdownController.RefreshAllFilterLabel();
            dropdownController.InitDropDown();//일단 임시로 끄기

            if(compoundFilterData == null)//필터 데이터 미리 생성
            {
                compoundFilterData = new FilterPopupData();
                compoundFilterData.Init();
            }
                
            if(decomposeFilterData == null)
            {
                decomposeFilterData = new FilterPopupData();
                decomposeFilterData.Init();
            }

            SetList(currentSortIndex, isFirstViewReloadCheck);
        }

        public void OnClickCustomSort(string index)
        {
            var checker = int.Parse(index);

            dropdownController.SetDropdownIndex(eDropDownType.DEFAULT, checker);
            dropdownController.RefreshAllFilterLabel();
            dropdownController.InitDropDown();//일단 임시로 끄기

            SetList(checker, isFirstViewReloadCheck);
        }

        void SetList(int _sortIndex, bool reloadPos = true)
        {
            List<ChampionPart> list = new List<ChampionPart>();
            switch(currentType)
            {
                case ChampionBattlePartListViewType.DEFAULT:
                case ChampionBattlePartListViewType.REINFORCE:
                case ChampionBattlePartListViewType.INFO:
                {
                    list = GetDefaultList(_sortIndex);
                }
                break;
                case ChampionBattlePartListViewType.COMPOUND://합성 - 필터 체크
                {
                }
                break;
                case ChampionBattlePartListViewType.DECOMPOSE:
                {
                }
                break;
            }

            invenCheckLabel.gameObject.SetActive(false);
            DrawList(list, reloadPos);

            if (list == null || list.Count <= 0)
            {
                invenCheckLabel.gameObject.SetActive(true);
                switch (currentType)
                {
                    case ChampionBattlePartListViewType.DEFAULT:
                    case ChampionBattlePartListViewType.REINFORCE:
                    case ChampionBattlePartListViewType.INFO:
                        invenCheckLabel.text = StringData.GetStringByIndex(100001237);//기본 인벤에 장비가 없을 때 처리
                        break;
                    case ChampionBattlePartListViewType.COMPOUND:
                        break;
                    case ChampionBattlePartListViewType.DECOMPOSE:
                        break;
                }
            }
        }

        List<ChampionPart> GetDefaultList(int _sortIndex)
        {
            var partList = ChampionManager.GetSelectableParts();
            if (partList == null || partList.Count <= 0)
                return null;

            List<ChampionPart> totalList = new List<ChampionPart>();
            //유저리스트, 빈 장비 리스트 분리 - 기본 빈 장비가 우선
            List<ChampionPart> dragonPartList = new List<ChampionPart>();
            List<ChampionPart> emptyPartList = new List<ChampionPart>();

            partList.ForEach((Element) => {
                if (Element == null)
                    return;

                totalList.Add(new ChampionPart(Element.KEY));
            });

            return totalList;
        }

        public void DrawList(List<ChampionPart> partList, bool reloadPos = true)
        {
            partInfoList.Clear();
            currentPartList.Clear();

            if (partList != null && partList.Count > 0)
            {
                foreach (ChampionPart part in partList)
                {
                    if (part == null)
                        continue;

                    ChampionBattlePartInfo partInfo = new ChampionBattlePartInfo();
                    partInfo.userPart = part;

                    partInfoList.Add(partInfo);
                    currentPartList.Add(part);
                }
            }

            var isEmpty = partInfoList.Count <= 0;
            invenCheckLabel.gameObject.SetActive(isEmpty);

            if (isEmpty)
            {
                switch (currentType)
                {
                    case ChampionBattlePartListViewType.DEFAULT:
                    case ChampionBattlePartListViewType.REINFORCE:
                    case ChampionBattlePartListViewType.INFO:
                        invenCheckLabel.text = StringData.GetStringByIndex(100001237);//기본 인벤에 장비가 없을 때 처리
                        break;
                    case ChampionBattlePartListViewType.COMPOUND:
                        break;
                    case ChampionBattlePartListViewType.DECOMPOSE:
                        break;
                }
            }

            DrawTableView(reloadPos);
        }

        List<UserPart> GetConditionPartList(int tag = -1, bool _isCompound = true)//자신 포함 귀속드래곤만 제외
        {
            List<UserPart> emptyPartList = new List<UserPart>();
            emptyPartList.Clear();

            var partList = ChampionManager.GetSelectableParts();
            if (partList == null || partList.Count <= 0)
                return emptyPartList;

            ChampionDragon dragon = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
            var specificGrade = -1;
            if(tag > 0)
            {
                var partData = dragon.GetPart(tag);
                if (partData == null)
                    return emptyPartList;

                specificGrade = partData.Grade();
            }

            partList.ForEach((Element) => {
                if (Element == null)
                    return;

                if(_isCompound)
                {
                    var isUnderGrade = (Element.GRADE >= PartGradeLimit);
                    if (isUnderGrade)
                        return;

                    if (specificGrade > 0)
                    {
                        var isSameGrade = (Element.GRADE == specificGrade);
                        if (!isSameGrade)
                            return;
                    }
                }

                emptyPartList.Add(new ChampionPart(Element.KEY));
            });

            return emptyPartList;
        }

        public void DrawTableView(bool reloadPos = true)
        {
            if (tableView != null)
            {
                List<ITableData> tableViewItemList = new List<ITableData>();
                tableViewItemList.Clear();
                if (partInfoList != null && partInfoList.Count > 0)
                {
                    for (var i = 0; i < partInfoList.Count; i++)
                    {
                        var data = partInfoList[i];
                        if (data == null)
                            continue;

                        tableViewItemList.Add(data);
                    }
                }

                tableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
                    var partComp = node.GetComponent<PartSlotFrame>();
                    if (partComp == null)
                        return;

                    var partData = (ChampionBattlePartInfo)item;
                    if (item == null)
                        return;

                    partComp.SetPartSlotFrame(partData.userPart);
                    partComp.SetVisibleSelectedNode(false);//버튼 UI 켜기

                    switch (currentType)//강화, 분해 제외 클릭 노드 켜기
                    {
                        case ChampionBattlePartListViewType.DEFAULT:
                        case ChampionBattlePartListViewType.REINFORCE:
                        case ChampionBattlePartListViewType.INFO:
                            partComp.SetVisibleSelectedNode(false);
                            partComp.SetVisibleClickNode(false);
                            //partComp.SetVisibleClickNode(partData.userPart.Tag == partLayer.PartTag);
                            break;
                        case ChampionBattlePartListViewType.COMPOUND://합성 선택 노드 켜기
                        {
                        }
                        break;
                        case ChampionBattlePartListViewType.DECOMPOSE:
                        {
                        }
                        break;
                    }
                    
                    partComp.SetCallback((param) => {
                        switch (currentType)
                        {
                            case ChampionBattlePartListViewType.DEFAULT:
                            case ChampionBattlePartListViewType.INFO:
                            case ChampionBattlePartListViewType.REINFORCE:
                                InitPartslotClickNode();//클릭한것 전체 일단 초기화
                                partLayer.PartTag = int.Parse(param);
                                partComp.SetVisibleClickNode(true);//해당 버튼만 활성화

                                if (currentType == ChampionBattlePartListViewType.REINFORCE)
                                    DragonPartEvent.ReinforcePartSlotSelect(partLayer.PartTag);
                                else
                                    DragonPartEvent.RefreshInfoPanel(int.Parse(param), false, null);//정보창 열기 콜백
                                break;
                            case ChampionBattlePartListViewType.DECOMPOSE:
                            {
                            }
                            break;
                            case ChampionBattlePartListViewType.COMPOUND:
                            {
                            }
                            break;
                        }
                    });
                }));
                tableView.ReLoad(reloadPos);
                isFirstViewReloadCheck = false;
            }
        }


        #region 장비 강화 표시
        public void InitPartslotClickNode()
        {
            //partLayer.PartTag = -1;
            if (nodeInvenContent == null)
                return;

            var children = SBFunc.GetChildren(nodeInvenContent.transform);
            if (children == null || children.Length <= 0)
                return;

            for (var i = 0; i < children.Length; i++)
            {
                var node = children[i];
                if (node == null)
                    continue;

                var partComp = node.GetComponent<PartSlotFrame>();
                if (partComp == null)
                    continue;

                partComp.SetVisibleClickNode(false);
            }
        }
        public void SuccessReinforcePartData(int _partTag)
        {
            //강화는 있을수없다.
            DrawTableView(false);
        }

        public void DeletePartDataInTableData(int parttag)
        {
            if (parttag <= 0 || partInfoList == null || partInfoList.Count <= 0)
                return;

            //삭제할 태그데이터 정보 날리기
            partInfoList = partInfoList.FindAll(element =>
            {
                if (element.userPart == null || element.userPart.Tag <= 0) { return false; }
                return (element.userPart.Tag != parttag);
            });
            //SBLog(partInfoList);
            DrawTableView(false);
        }
        #endregion

        #region 정렬 규칙
        /**
         * sortIndex 
         * 0 : 등급 > 강화 > 최신 - 내림차순
         * 1 : 등급 > 강화 > 최신 - 오름차순
         * 2 : 강화 > 최신 > 등급 - 내림차순
         * 3 : 강화 > 최신 > 등급 - 오름차순
         * 4 : 등급 > 강화 - 내림차순 (분해)
         * 5 : 등급 > 강화 - 오름차순 (분해)
         * 6 : 신규 추가 형태 - 합성전용 소팅 (현재 선택한 메인 재료 등급과 동급 - 선처리 및 강화 순서 오름차순)
         * 7 : 신규 추가 형태 - 합성전용 소팅 (현재 선택한 메인 재료 등급과 동급 - 선처리 및 강화 순서 내림차순)
         */
        List<UserPart> MakeListBySortCondition(List<UserPart> partList, int sortIndex)
        {
            var sortFunc = Sort(sortIndex);
            if (sortFunc == null)
            {
                return null;
            }

            partList.Sort(sortFunc);

            return partList;
        }
        private Comparison<UserPart> Sort(int index)
        {
            switch (index)
            {
                case 0: return Sort0;
                case 1: return Sort1;
                case 2: return Sort2;
                case 3: return Sort3;
                case 4: return Sort4;
                case 5: return Sort5;
                case 6: return Sort6;
                case 7: return Sort7;
            }

            return null;
        }
        private int Sort0(UserPart a, UserPart b)
        {
            var checker = SortGradeDescend(a, b);

            if (checker == 0)
            {
                checker = SortLevelDescend(a, b);
                if (checker == 0)
                {
                    checker = SortIDDescend(a, b);
                    if (checker == 0)
                        return SortTagDescend(a, b);
                }
            }

            return checker;
        }

        private int Sort1(UserPart a, UserPart b)
        {
            var checker = SortGradeAscend(a, b);

            if (checker == 0)
            {
                checker = SortLevelAscend(a, b);
                if (checker == 0)
                {
                    checker = SortIDAscend(a, b);
                    if (checker == 0)
                        return SortTagAscend(a, b);
                }
            }

            return checker;
        }

        private int Sort2(UserPart a, UserPart b)
        {
            var checker = SortLevelDescend(a, b);
            if (checker == 0)
            {
                checker = SortGradeDescend(a, b);
                if (checker == 0)
                {
                    checker = SortIDDescend(a, b);
                    if (checker == 0)
                        return SortTagDescend(a, b);
                }
            }

            return checker;
        }

        private int Sort3(UserPart a, UserPart b)
        {
            var checker = SortLevelAscend(a, b);

            if (checker == 0)
            {
                checker = SortGradeAscend(a, b);
                if (checker == 0)
                {
                    checker = SortIDAscend(a, b);
                    if (checker == 0)
                        return SortTagAscend(a, b);
                }
            }

            return checker;
        }

        private int Sort4(UserPart a, UserPart b)
        {
            var checker = SortGradeDescend(a, b);
            if (checker == 0)
            {
                checker = SortLevelDescend(a, b);
                if (checker == 0)
                {
                    checker = SortItemTypeAscend(a, b);
                    if (checker == 0)
                    {
                        return SortObtainTimeDescend(a, b);
                    }
                }
            }

            return checker;
        }

        private int Sort5(UserPart a, UserPart b)
        {
            var checker = SortGradeAscend(a, b);

            if (checker == 0)
            {
                checker = SortLevelAscend(a, b);
                if (checker == 0)
                {
                    checker = SortItemTypeAscend(a, b);
                    if (checker == 0)
                    {
                        return SortObtainTimeAscend(a, b);
                    }
                }
            }

            return checker;
        }

        private int Sort6(UserPart a, UserPart b)
        {
            return SortLevelAscend(a, b);
        }

        private int Sort7(UserPart a, UserPart b)
        {
            return SortLevelDescend(a, b); ;
        }

        //등급 내림차순
        private int SortGradeDescend(UserPart a, UserPart b)
        {
            var aGrade = a.Grade();
            var bGrade = b.Grade();
            return bGrade - aGrade;
        }
        //등급 오름차순
        private int SortGradeAscend(UserPart a, UserPart b)
        {
            var aGrade = a.Grade();
            var bGrade = b.Grade();
            return aGrade - bGrade;
        }
        //강화레벨 내림차순
        private int SortLevelDescend(UserPart a, UserPart b)
        {
            var aLevel = a.Reinforce;
            var bLevel = b.Reinforce;
            return bLevel - aLevel;
        }
        //강화레벨 오름차순
        private int SortLevelAscend(UserPart a, UserPart b)
        {
            var aLevel = a.Reinforce;
            var bLevel = b.Reinforce;
            return aLevel - bLevel;
        }
        //최신 획득 내림 차순
        private int SortObtainTimeDescend(UserPart a, UserPart b)
        {
            var aObtainTime = a.Obtain;
            var bObtaionTime = b.Obtain;
            return bObtaionTime - aObtainTime;
        }
        //최신 획득 오름 차순
        private int SortObtainTimeAscend(UserPart a, UserPart b)
        {
            var aObtainTime = a.Obtain;
            var bObtaionTime = b.Obtain;
            return aObtainTime - bObtaionTime;
        }
        //아이템 ID 획득 내림 차순
        private int SortIDDescend(UserPart a, UserPart b)
        {
            var aTag = a.ID;
            var bTag = b.ID;
            return bTag - aTag;
        }
        //아이템키 ID 획득 오름 차순
        private int SortIDAscend(UserPart a, UserPart b)
        {
            var aTag = a.ID;
            var bTag = b.ID;
            return aTag - bTag;
        }
        //아이템 고유키 획득 내림 차순
        private int SortTagDescend(UserPart a, UserPart b)
        {
            var aTag = a.Tag;
            var bTag = b.Tag;
            return bTag - aTag;
        }
        //아이템키 고유키 획득 오름 차순
        private int SortTagAscend(UserPart a, UserPart b)
        {
            var aTag = a.Tag;
            var bTag = b.Tag;
            return aTag - bTag;
        }


        //아이템 타입 (per / value 타입 정리)
        private int SortItemTypeAscend(UserPart a, UserPart b)
        {
            var aValue_type = a.GetPartDesignData().VALUE_TYPE;
            var bValue_type = b.GetPartDesignData().VALUE_TYPE;

            var aIndex = GetNumberType(aValue_type);
            var bIndex = GetNumberType(bValue_type);

            return aIndex - bIndex;
        }

        int GetNumberType(string valuetype)
        {
            var checkIndex = 0;
            switch (valuetype)
            {
                case "PERCENT":
                    checkIndex = 1;
                    break;
                case "VALUE":
                    checkIndex = 2;
                    break;
            }
            return checkIndex;
        }
        #endregion

        #region 필터 데이터
        public void OnClickFilterPopup()//현재 컴포넌트 같이 넘기기
        {
            var filterData = currentType == ChampionBattlePartListViewType.COMPOUND ? compoundFilterData : decomposeFilterData;
            var popup = PopupManager.OpenPopup<PartFilterPopup>(filterData);
            popup.ApplyCallback = SetFilter;
        }

        public void SetFilter(FilterPopupData data)//필터 체크 후 받아와야하는 값
        {
            if (data == null)
            {
                Debug.Log("필터 데이터 생성 누락");
                return;
            }

            //리스트 갱신
            DragonPartEvent.RefreshList();

        }
        List<UserPart> MakeListByFilter(int gradeFilter = -1, int typeFilter = -1, int levelFilter = -1, List<UserPart> originList = null)//이벤트로 넘기기
        {
            if(originList == null)
                originList = GetConditionPartList();

            List<UserPart> gradeList = new List<UserPart>();
            List<UserPart> typeList = new List<UserPart>();
            List<UserPart> levelList = new List<UserPart>();

            List<UserPart> tempArr = new List<UserPart>();
            if (gradeFilter > 0)
            {
                tempArr = GetDecomposeGradeFilter(originList, gradeFilter);
                if (tempArr != null && tempArr.Count >= 0)
                {
                    gradeList = tempArr.ToList();
                }
            }

            tempArr.Clear();
            if (typeFilter > 0)
            {
                tempArr = GetDecomposeTypeFilter(originList, typeFilter);
                if (tempArr != null && tempArr.Count >= 0)
                {
                    typeList = tempArr.ToList();
                }
            }

            tempArr.Clear();
            if (levelFilter > 0)
            {
                tempArr = GetDecomposeLevelFilter(originList, levelFilter);
                if (tempArr != null && tempArr.Count >= 0)
                {
                    levelList = tempArr.ToList();
                }
            }

            var GtoTArr = ArrayIntersection(gradeList.ToArray(), typeList.ToArray());
            var TtoLArr = ArrayIntersection(typeList.ToArray(), levelList.ToArray());
            var resultArr = ArrayIntersection(GtoTArr, TtoLArr);

            return resultArr.ToList();
        }

        UserPart[] ArrayIntersection(UserPart[] array1, UserPart[] array2)
        {
            var intersect = array1.Intersect(array2);
            List<UserPart> tempList = new List<UserPart>();
            tempList.Clear();

            foreach (UserPart item in intersect)
            {
                tempList.Add(item);
            }

            return tempList.ToArray();
        }
        List<UserPart> GetDecomposeGradeFilter(List<UserPart> userPartList, int filter)
        {
            List<UserPart> userPartFilter = new List<UserPart>();
            userPartFilter.Clear();

            for (var i = 0; i < (int)eDcomposeCount.Grade; i++)
            {
                var pow = (int)Math.Pow(2, i);
                if ((filter & pow) != 0)
                {
                    var tempFilter = userPartList.FindAll(element => element.Grade() == (i + 1));
                    userPartFilter = userPartFilter.Concat(tempFilter).ToList();
                }
            }

            return userPartFilter;
        }

        List<UserPart> GetDecomposeTypeFilter(List<UserPart> userPartList, int filter)
        {
            List<UserPart> userPartFilter = new List<UserPart>();
            userPartFilter.Clear();

            for (var i = 0; i < (int)eDcomposeCount.Type; i++)
            {
                var pow = (int)Math.Pow(2, i);
                if ((filter & pow) != 0)
                {
                    var tempFilter = userPartList.FindAll(element => element.GetPartDesignData().STAT_TYPE == ((eTypeFilter)pow).ToString());
                    userPartFilter = userPartFilter.Concat(tempFilter).ToList();
                }
            }

            return userPartFilter;
        }

        List<UserPart> GetDecomposeLevelFilter(List<UserPart> userPartList, int filter)
        {
            List<UserPart> userPartFilter = new List<UserPart>();
            userPartFilter.Clear();

            for (var i = 0; i < (int)eDcomposeCount.Level; i++)
            {
                var pow = (int)Math.Pow(2, i);
                if ((filter & pow) != 0)
                {
                    switch ((eReinforceLevelFilter)pow)
                    {
                        case eReinforceLevelFilter.Zero:
                        {
                            var tempFilter = userPartList.FindAll(element => element.Reinforce == 0);
                            userPartFilter = userPartFilter.Concat(tempFilter).ToList();
                        }
                        break;
                        case eReinforceLevelFilter.OneToSix:
                        {
                            var tempFilter = userPartList.FindAll(element => element.Reinforce >= 1 && element.Reinforce <= 6);
                            userPartFilter = userPartFilter.Concat(tempFilter).ToList();
                        }
                        break;
                        case eReinforceLevelFilter.SevenToNine:
                        {
                            var tempFilter = userPartList.FindAll(element => element.Reinforce >= 7 && element.Reinforce <= 9);
                            userPartFilter = userPartFilter.Concat(tempFilter).ToList();
                        }
                        break;
                        case eReinforceLevelFilter.TenToTwelve:
                        {
                            var tempFilter = userPartList.FindAll(element => element.Reinforce >= 10 && element.Reinforce <= 12);
                            userPartFilter = userPartFilter.Concat(tempFilter).ToList();
                        }
                        break;
                    }
                }
            }

            return userPartFilter;
        }
        #endregion
    }
}
