using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonListLayer : SubLayer
    {
        [SerializeField]
        private DragonTabLayer DragonTap = null;
        [SerializeField]
        private Text titleLabel = null;

        [Header("Scroll")]
        [SerializeField]
        protected TableViewGrid tableViewGrid = null;

        [Space(10)]
        [Header("DropDown")]
        [SerializeField]
        private DropDownUIController dropdownController = null;

        [Space(10)]
        [Header("collection reddot")]
        [SerializeField] GameObject reddot = null;

        [Space(10)]
        [Header("Art Block")]
        [SerializeField] Button artBlockBtn = null;
        [SerializeField] Sprite artBlockAbleImg = null;
        [SerializeField] Sprite artBlockDisableImg = null;

        [SerializeField]
        private Text invenCheckLabel = null;

        [SerializeField]
        private GameObject CompoundEventIcon = null;


        private List<UserDragon> userDragons = null;
        private List<UserDragon> viewDragons = null;
        private List<UserDragon> lockDragons = null;//미획득 드래곤 리스트

        private bool viewDirty = true;

        private bool isTableInit = false;
        private bool initScrollPos = true;

        FilterPopupData filterData = null;

        bool isTutorialing = false;
        public override void Init()
        {
            userDragons = User.Instance.DragonData.GetAllUserDragons();
            if (tableViewGrid != null && !isTableInit)
            {
                tableViewGrid.OnStart();
                isTableInit = true;
            }

            if (filterData == null)//필터 데이터 미리 생성
            {
                filterData = new FilterPopupData();
                filterData.Init();
            }

            if (CompoundEventIcon != null)
            {
                CompoundEventIcon.SetActive(false);
                foreach (var data in CharMergeBaseData.GetAll())
                {
                    if (data != null)
                    {
                        int defualt = 0;
                        switch (data.GRADE)
                        {
                            case 1:
                                defualt = GameConfigTable.GetConfigIntValue("DEFUALT_MERGE_SUCCESS_RATE_C", 400000);
                                break;
                            case 2:
                                defualt = GameConfigTable.GetConfigIntValue("DEFUALT_MERGE_SUCCESS_RATE_UC", 300000);
                                break;
                            case 3:
                                defualt = GameConfigTable.GetConfigIntValue("DEFUALT_MERGE_SUCCESS_RATE_R", 150000);
                                break;
                            case 4:
                                defualt = GameConfigTable.GetConfigIntValue("DEFUALT_MERGE_SUCCESS_RATE_U", 100000);
                                break;
                            case 5:
                                defualt = GameConfigTable.GetConfigIntValue("DEFUALT_MERGE_SUCCESS_RATE_L", 1000000);
                                break;
                        }

                        if (data.MERGE_SUCCESS_RATE > defualt)
                        {
                            CompoundEventIcon.SetActive(true);
                            break;
                        }
                    }
                }
            }
            isTutorialing = TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.DragonManage);
            InitCustomSort();
            RefreshCollectionReddot();
            SetArtBlockState();
        }

        void InitCustomSort()
        {
            dropdownController.RefreshAllFilterLabel();
            dropdownController.InitDropDown();
            SetAllFilterSort();
            ForceUpdate();

            //NetworkManager.Send("collection/refreshcollection", null, (jsonData) => {
            //});
        }

        void SetLockDragons()//미획득 드래곤 세팅하기(Init 탈때만 세팅)
        {
            if (lockDragons == null)
                lockDragons = new List<UserDragon>();
            lockDragons.Clear();

            var tableData = CharBaseData.GetAllList();

            tableData = tableData.FindAll(element => element.IS_USE).ToList();//1은 표시 0은 미표시
            foreach (var dragonData in tableData)
            {
                SetEmptyDragonData(dragonData.KEY);
            }
        }

        void SetEmptyDragonData(int _tag)
        {
            if (_tag < 0)
                return;

            if (viewDragons != null && viewDragons.Count > 0)
            {
                var isIndex = viewDragons.FindIndex(element => element.Tag == _tag);
                if (isIndex < 0)
                    AddLockDragonsList(_tag);
            }
            else
                AddLockDragonsList(_tag);
        }

        void AddLockDragonsList(int _tag)
        {
            var newTempDragon = new UserDragon();
            newTempDragon.Init();
            newTempDragon.SetBaseData(_tag, eDragonState.Normal, -1, -1);
            lockDragons.Add(newTempDragon);
        }

        public override void ForceUpdate()
        {
            DrawScrollView();
        }

        public void DrawScrollView()
        {
            if (!viewDirty || tableViewGrid == null || viewDragons == null)
                return;

            var isEmpty = viewDragons.Count <= 0 && lockDragons.Count <= 0;
            if(invenCheckLabel != null)
                invenCheckLabel.gameObject.SetActive(isEmpty);

            var tempAllDragonViewList = new List<UserDragon>();
            tempAllDragonViewList.AddRange(viewDragons);
            tempAllDragonViewList.AddRange(lockDragons);

            List<ITableData> tableViewItemList = new List<ITableData>();
            tableViewItemList.Clear();
            if (tempAllDragonViewList != null && tempAllDragonViewList.Count > 0)
            {
                for (var i = 0; i < tempAllDragonViewList.Count; i++)
                {
                    var data = tempAllDragonViewList[i];
                    if (data == null)
                        continue;

                    if (!data.BaseData.IS_USE)
                        continue;

                    tableViewItemList.Add(data);
                }
            }

            //List<int> adventure = new List<int>();
            //foreach (var line in User.Instance.PrefData.AdventureFormationData.TeamFormation)
            //{
            //    foreach (var tag in line)
            //    {
            //        adventure.Add(tag);
            //    }
            //}
            //List<int> arenaatk = new List<int>();
            //foreach (var line in User.Instance.PrefData.ArenaFormationData.TeamFormationATK)
            //{
            //    foreach (var tag in line)
            //    {
            //        arenaatk.Add(tag);
            //    }
            //}
            //List<int> arenadef = new List<int>();
            //foreach (var line in User.Instance.PrefData.ArenaFormationData.TeamFormationDEF)
            //{
            //    foreach (var tag in line)
            //    {
            //        arenadef.Add(tag);
            //    }
            //}
            bool isTutorialDragonSet = isTutorialing;

            tableViewGrid.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) =>
            {
                if (node == null)
                    return;

                var frame = node.GetComponent<DragonPortraitFrame>();
                if (frame == null)
                    return;

                var dragonData = (UserDragon)item;
                if (dragonData.Level < 0)//깡통 상태 드래곤은 레벨 -1 처리
                    frame.SetLockedDragonPortraitFrame(dragonData);
                else
                {
                    frame.SetDragonPortraitFrame(dragonData);
                    if (User.Instance.DragonData.IsFavorite(dragonData.Tag))
                    {
                        frame.SetFrameColor(new Color(0.0f, 0.8f, 0.0f));
                    }
                }
                frame.setCallback(OnSelectDragon);

                if (isTutorialDragonSet)
                {
                    TutorialManager.Instance.SetRecordObject(600101,frame.GetComponent<RectTransform>());
                    isTutorialDragonSet = false;
                }

                frame.ClearStatusUI();

                //if (adventure.Contains(dragonData.Tag))
                //    frame.SetStatusAdventure();
                //if (arenaatk.Contains(dragonData.Tag))
                //    frame.SetStatusArenaATK();
                //if (arenadef.Contains(dragonData.Tag))
                //    frame.SetStatusArenaDEF();
                //if (dragonData.State == eDragonState.Travel)
                //    frame.SetStatusTravel();
            }));

            tableViewGrid.ReLoad(initScrollPos);
            viewDirty = false;
            initScrollPos = false;
        }

        void SetEmptyCountListLabelByViewDragons()
        {
            if (viewDragons.Count <= 0)
            {
                invenCheckLabel.gameObject.SetActive(true);
                invenCheckLabel.text = StringData.GetStringByIndex(-1);//기본 인벤에 장비가 없을 때 처리
            }
            else
                invenCheckLabel.gameObject.SetActive(false);
        }

        public void OnSelectDragon(string customEventData)//획득 / 미획득 상관없이 정보탭으로 던짐
        {
            if (int.TryParse(customEventData, out int customValue) == false)
                return;


            //WJ - 2023 12월 업데이트 내용으로 돌림.
            PopupManager.GetPopup<DragonManagePopup>().CurDragonTag = customValue;
            DragonTap.moveLayer(1);
            return;

            //WJ - 20231024 드래곤 관리 UI 개편으로 인해 무조건 정보 탭으로 들어가야함.
            //var index = viewDragons.FindIndex(element => element.Tag == customValue);
            //if (index >= 0)
            //{
            //    PopupManager.GetPopup<DragonManagePopup>().DragonTagInfo = customValue;
            //    DragonTap.moveLayer(1);
            //}
            //else//미획득 드래곤은 가챠 씬으로 보내기
            //{
            //    if (lockDragons != null)
            //    {
            //        index = lockDragons.FindIndex(element => element.Tag == customValue);
            //        if (lockDragons.Count > index && lockDragons[index].BaseData != null)
            //        {
            //            if (lockDragons[index].BaseData.IS_SCENARIO)
            //            {
            //                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("미보유시나리오드래곤"));
            //                return;
            //            }

            //            if (lockDragons[index].BaseData.IS_CASH)
            //            {
            //                var popup = PopupManager.OpenPopup<ConditionalBuyPopup>(new ConditionBuyData(80019));
            //                popup.SetRewardCallBack(() =>
            //                {
            //                    PopupManager.GetPopup<ShopPopup>().RefreshCurrentMenu();
            //                    popup.ClosePopup();
            //                });
            //                return;
            //            }

            //            if (lockDragons[index].Grade() >= (int)eDragonGrade.Legend)
            //            {
            //                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("미보유드래곤_합성"), () =>
            //                {
            //                    OnClickDragonCompound();
            //                },
            //                () =>
            //                {
            //                    //나가기
            //                },
            //                () =>
            //                {
            //                    //나가기
            //                }
            //            );
            //                return;
            //            }
            //        }

            //        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("미보유드래곤"),
            //                () =>
            //                {
            //                    PopupManager.AllClosePopup();//가챠 씬이동
            //                    SBFunc.MoveGachaScene();
            //                },
            //                () =>
            //                {
            //                    //나가기
            //                },
            //                () =>
            //                {
            //                    //나가기
            //                }
            //            );
            //    }
            //}
        }

        public void OnClickReset()//모든 필터 및 정렬 초기화
        {
            dropdownController.ResetAllDropDown();
            dropdownController.InitDropDown();
            SetAllFilterSort();
            ForceUpdate();
        }
        /**
         * @param param 소트 타입 으로 제공 - 팀배치
         * 0 : all
         * 1 : 탐험
         * 2 : 요일던전
         * 3 : 아레나 공격
         * 4 : 아레나 방어
         */
        public void OnClickTeamFomationSort(string customEventData)
        {
            var checker = int.Parse(customEventData);
            if (dropdownController.GetDropdownIndex(eDropDownType.TEAM_FORMATION) == checker)
            {
                dropdownController.SetDropDownVisible(eDropDownType.TEAM_FORMATION, false);
                return;
            }

            dropdownController.SetDropdownIndex(eDropDownType.TEAM_FORMATION, checker);
            dropdownController.RefreshSpecificFilterLabel(eDropDownType.TEAM_FORMATION);
            dropdownController.InitDropDown();
            SetAllFilterSort();
            ForceUpdate();
        }
        /**
         * @param param 소트 타입 으로 제공
         * 0 : all
         * 1 : TANKER
         * 2 : WARRIOR
         * 3 : ASSASSIN
         * 4 : CANNON
         * 5 : ARCHER
         * 6 : SUPPORTER
         */
        public void OnClickClassSort(string customEventData)
        {
            var checker = int.Parse(customEventData);
            if (dropdownController.GetDropdownIndex(eDropDownType.CLASS) == checker)
            {
                dropdownController.SetDropDownVisible(eDropDownType.CLASS, false);
                return;
            }

            dropdownController.SetDropdownIndex(eDropDownType.CLASS, checker);
            dropdownController.RefreshSpecificFilterLabel(eDropDownType.CLASS);
            dropdownController.InitDropDown();
            SetAllFilterSort();
            ForceUpdate();
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
                //SetDropDownVisible(eDropDownType.ELEMENT, false);
                return;
            }

            dropdownController.SetDropdownIndex(eDropDownType.ELEMENT, checker);
            dropdownController.RefreshSpecificFilterLabel(eDropDownType.ELEMENT);
            dropdownController.InitDropDown();
            SetAllFilterSort();
            ForceUpdate();
        }

        /**
         * 
         * @param param 
         * @param customEventData 
         * 
         * 정렬 타입
         * 0 : 등급 내림차순 (default)
         * 1 : 등급 오름차순
         * 2 : 레벨 내림차순
         * 3 : 레벨 오름차순
         * 4 : 전투력 내림차순
         * 5 : 전투력 오름차순
         * 6 : 최신 획득 내림 차순
         * 7 : 최신 획득 오름 차순
         */
        public void OnClickCustomSort(string customEventData)
        {
            var checker = int.Parse(customEventData);
            if (dropdownController.GetDropdownIndex(eDropDownType.DEFAULT) == checker)
            {
                dropdownController.SetDropDownVisible(eDropDownType.DEFAULT, false);
                return;
            }

            initScrollPos = true;
            dropdownController.SetDropdownIndex(eDropDownType.DEFAULT, checker);
            dropdownController.RefreshAllFilterLabel();
            dropdownController.InitDropDown();//일단 임시로 끄기
            SetAllFilterSort();
            ForceUpdate();
        }

        void SetAllFilterSort()//드래곤 기본 정렬 > 원소 정렬 > 클래스 정렬 -> 요소 바뀔때마다 전체 소팅
        {
            SetListCustomSort();//정렬
            //SetCustomElementList();
            //SetCustomClassList();
            //SetCustomTeamFomationList();
            SaveCurList();
        }

        void FilteringDragon()//필터 버튼 (속성, 클래스, 팀배치) - 일단 보유 미보유 둘다 적용으로
        {
            var gradeFilter = filterData.gradeFilter;
            var elementFilter = filterData.elementFilter;
            var jobFilter = filterData.jobFilter;
            var formationFilter = filterData.formationFilter;

            if (gradeFilter == eGradeFilter.ALL && elementFilter == eElementFilter.ALL && jobFilter == eJobFilter.ALL && formationFilter == eJoinedContentFilter.ALL)//기본이면 연산 안함
                return;
            else//필터가 하나라도 off 됐다면, 미획득 드래곤은 미표시
            {
                if (gradeFilter != eGradeFilter.ALL)
                {
                    viewDragons.RemoveAll(dragon =>
                    {
                        var grade = SBFunc.GetGradeFilterType(dragon.Grade());
                        return gradeFilter.HasFlag(grade) == false;
                    });
                }
                if (elementFilter != eElementFilter.ALL)
                {
                    viewDragons.RemoveAll(dragon =>
                    {
                        var elem = SBFunc.GetElemFilterType(dragon.Element());
                        return elementFilter.HasFlag(elem) == false;
                    });
                }
                if (jobFilter != eJobFilter.ALL)
                {
                    viewDragons.RemoveAll(dragon =>
                    {
                        var job = SBFunc.GetJobFilterType(dragon.JOB());
                        return jobFilter.HasFlag(job) == false;
                    });
                }
                if (formationFilter != eJoinedContentFilter.ALL) // 참여 컨텐츠에서 무언가 하나를 체크를 풀었을 것이여, 그럼 아무것도 참여 안하는 드래곤은 그려주면 안되는 것이여
                {
                    viewDragons.RemoveAll(dragon =>
                    {
                        var joinContent = dragon.JoinedContent();
                        return (joinContent & formationFilter) == 0;
                        //return formationFilter.HasFlag(joinContent) == false || joinContent == eJoinedContentFilter.None; 
                    });
                }

                lockDragons.Clear(); // 미획득 드래곤은 미표시
            }
        }

        void SetListCustomSort()
        {
            if (userDragons == null)
                userDragons = User.Instance.DragonData.GetAllUserDragons();

            //소팅 구성 데이터 map 세팅 - 소팅 하기전 기본 map 형태 // init에서 맵 구성 완료.
            var sortFunc = Sort(dropdownController.GetDropdownIndex(eDropDownType.DEFAULT));
            if (sortFunc == null)
                return;

            if (viewDragons == null)
                viewDragons = new List<UserDragon>();

            viewDragons.Clear();
            viewDragons = userDragons.ToList();

            SetLockDragons();

            FilteringDragon();

            viewDragons.Sort(sortFunc);
            lockDragons.Sort(sortFunc);//잠금 드래곤 갱신

            viewDirty = true;
        }
        void SetCustomElementList()//필터로 이동 - 원소 정렬 안씀
        {
            //var index = dropdownController.GetDropdownIndex(eDropDownType.ELEMENT);
            //if (index != 0 && index >= 0)
            //{
            //    viewDragons = viewDragons.FindAll(Element => Element.Element() == index);
            //    lockDragons = lockDragons.FindAll(Element => Element.Element() == index);
            //}
        }
        void SetCustomClassList()//직업 정렬 빠진다고 함.
        {
            //var index = dropdownController.GetDropdownIndex(eDropDownType.CLASS);
            //if (index != 0 && index >= 0)
            //{
            //    viewDragons = viewDragons.FindAll(Element => Element.JOB() == index);
            //    lockDragons = lockDragons.FindAll(Element => Element.JOB() == index);
            //}
        }
        /**
         * 정렬 타입
         * 0 : "전체";
         * 1 : "탐험";
         * 2 : "요일던전";
         * 3 : "아레나 공격";
         * 4 : "아레나 방어";
         */
        void SetCustomTeamFomationList()//포메이션 정렬 안씀 - 필터로 빠짐
        {
            //var index = dropdownController.GetDropdownIndex(eDropDownType.TEAM_FORMATION);
            //if (index != 0 && index >= 0)
            //{
            //    var indexList = GetTeamFormationByIndex(index);

            //    viewDragons = viewDragons.FindAll(Element => indexList.Contains(Element.Tag));
            //}
        }
        List<int> GetTeamFormationByIndex(int _index)
        {
            switch(_index)
            {
                case 1:
                    return User.Instance.PrefData.GetAdventureFormation().ToList();
                case 2://데일리지만 0이아닌 WorldIndex로 읽어와야함.
                    return User.Instance.PrefData.GetDailyFormation(0).ToList();
                case 3:
                    return User.Instance.PrefData.GetArenaFomation().ToList();
                case 4:
                    return User.Instance.PrefData.GetArenaFomation(false).ToList();
            }
            return new List<int>();
        }

        void SaveCurList()
        {
            //현재 드래곤 리스트상태 값 저장 (드래곤 레벨업, 스킬, 장비 넘기기 시 해당 기준 소팅 리스트로 사용)
            PopupManager.GetPopup<DragonManagePopup>()?.ClearDragonInfoList();

            var tempAllDragonViewList = new List<UserDragon>();
            tempAllDragonViewList.AddRange(viewDragons);
            tempAllDragonViewList.AddRange(lockDragons);

            if (tempAllDragonViewList != null && tempAllDragonViewList.Count > 0)
            {
                var length = tempAllDragonViewList.Count;
                for (var i = 0; i < length; i++)
                {
                    var dragonData = tempAllDragonViewList[i];
                    if (dragonData == null)
                        continue;

                    if (dragonData.Tag <= 0)
                        continue;

                    PopupManager.GetPopup<DragonManagePopup>()?.DragonInfoList?.Add(dragonData.Tag);
                }
            }
        }

        void RefreshCollectionReddot()
        {
            if (reddot != null)
            {
                var isShow = CollectionAchievementManager.Instance.IsShowCollectionReddot();
                reddot.SetActive(isShow);
            }
        }

        #region 정렬 규칙

        private Comparison<UserDragon> Sort(int index)
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

        private int Sort0(UserDragon a, UserDragon b)
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
                        if (checker == 0)
                        {
                            checker = SortObtainTimeDescend(a, b);
                            if (checker == 0)
                                return SortTagDescend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort1(UserDragon a, UserDragon b)
        {
            var checker = SortFavorite(a, b);

            if (checker == 0)
            {
                checker = SortGradeAscend(a, b);
                if (checker == 0)
                {
                    checker = SortLevelAscend(a, b);
                    if (checker == 0)
                    {
                        checker = SortBattlePointAscend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeAscend(a, b);
                            if (checker == 0)
                                return SortTagAscend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort2(UserDragon a, UserDragon b)
        {
            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortLevelDescend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeDescend(a, b);
                    if (checker == 0)
                    {
                        checker = SortBattlePointDescend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeDescend(a, b);
                            if (checker == 0)
                                return SortTagDescend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort3(UserDragon a, UserDragon b)
        {
            var checker = SortFavorite(a, b);

            if (checker == 0)
            {
                checker = SortLevelAscend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeAscend(a, b);
                    if (checker == 0)
                    {
                        checker = SortBattlePointAscend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeAscend(a, b);
                            if (checker == 0)
                                return SortTagAscend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort4(UserDragon a, UserDragon b)
        {
            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortBattlePointDescend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeDescend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelDescend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeDescend(a, b);
                            if (checker == 0)
                                return SortTagDescend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort5(UserDragon a, UserDragon b)
        {
            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortBattlePointAscend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeAscend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelAscend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeAscend(a, b);
                            if (checker == 0)
                                return SortTagAscend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort6(UserDragon a, UserDragon b)
        {
            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortObtainTimeDescend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeDescend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelDescend(a, b);
                        if (checker == 0)
                        {
                            checker = SortBattlePointDescend(a, b);
                            if (checker == 0)
                                return SortTagDescend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort7(UserDragon a, UserDragon b)
        {
            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortObtainTimeAscend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeAscend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelAscend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeAscend(a, b);
                            if (checker == 0)
                                return SortTagAscend(a, b);
                        }
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
            return b.Grade().CompareTo(a.Grade());
        }
        //등급 오름차순
        private int SortGradeAscend(UserDragon a, UserDragon b)
        {
            return a.Grade().CompareTo(b.Grade());
        }
        //레벨 내림차순
        private int SortLevelDescend(UserDragon a, UserDragon b)
        {
            return b.Level.CompareTo(a.Level);
        }
        //레벨 오름차순
        private int SortLevelAscend(UserDragon a, UserDragon b)
        {
            return a.Level.CompareTo(b.Level);
        }
        //전투력 내림차순
        private int SortBattlePointDescend(UserDragon a, UserDragon b)
        {
            return b.GetTotalINF().CompareTo(a.GetTotalINF());
        }
        //전투력 오름차순
        private int SortBattlePointAscend(UserDragon a, UserDragon b)
        {
            return a.GetTotalINF().CompareTo(b.GetTotalINF());
        }
        //최신 획득 내림 차순
        private int SortObtainTimeDescend(UserDragon a, UserDragon b)
        {
            return b.Obtain.CompareTo(a.Obtain);
        }
        //최신 획득 오름 차순
        private int SortObtainTimeAscend(UserDragon a, UserDragon b)
        {
            return a.Obtain.CompareTo(b.Obtain);
        }
        //태그(키 값) 내림 차순
        private int SortTagDescend(UserDragon a, UserDragon b)
        {
            return b.Tag.CompareTo(a.Tag);
        }
        //태그(키 값) 오름 차순
        private int SortTagAscend(UserDragon a, UserDragon b)
        {
            return a.Tag.CompareTo(b.Tag);
        }
        #endregion
        public void OnClickDragonCompound()
        {
            DragonTap.moveLayer(3);
        }

        public void OnClickCollectionButton()
        {
            PopupManager.GetPopup<DragonManagePopup>()?.OpenCollectionForceClose();

            var popup = PopupManager.OpenPopup<CollectionAchievementPopup>();
            if (popup != null)
            {
                //popup.SetCloseCallback(()=> {
                //    SetVisibleTitleLabel(true);
                //});
            }
            //SetVisibleTitleLabel(false);
        }
        void SetVisibleTitleLabel(bool _isVisible)
        {
            if (titleLabel != null)
                titleLabel.gameObject.SetActive(_isVisible);
        }

        #region 필터 데이터
        public void OnClickFilterPopup()//현재 컴포넌트 같이 넘기기
        {
            var popup = PopupManager.OpenPopup<DragonListFilterPopup>(filterData);
            popup.ApplyCallback = SetFilter;
        }

        public void SetFilter(FilterPopupData data)//필터 체크 후 받아와야하는 값
        {
            if (data == null)
            {
                Debug.Log("필터 데이터 생성 누락");
                return;
            }
            if (filterData != null)
                filterData.SetFilter(data);
            SetAllFilterSort();
            ForceUpdate();
        }
        //List<UserDragon> MakeListByFilter(int elementFilter = -1, int jobFilter = -1, int formationFilter = -1, List<UserDragon> originList = null)//이벤트로 넘기기
        //{
        //    if (originList == null)
        //        originList = User.Instance.DragonData.GetAllUserDragons();

        //    List<UserDragon> elementList = new List<UserDragon>();
        //    List<UserDragon> jobList = new List<UserDragon>();
        //    List<UserDragon> formationList = new List<UserDragon>();

        //    List<UserDragon> tempArr = new List<UserDragon>();
        //    if (elementFilter > 0)
        //    {
        //        tempArr = GetElementFilter(originList, elementFilter);
        //        if (tempArr != null && tempArr.Count >= 0)
        //        {
        //            elementList = tempArr.ToList();
        //        }
        //    }

        //    tempArr.Clear();
        //    if (jobFilter > 0)
        //    {
        //        tempArr = GetJobFilter(originList, jobFilter);
        //        if (tempArr != null && tempArr.Count >= 0)
        //        {
        //            jobList = tempArr.ToList();
        //        }
        //    }

        //    tempArr.Clear();
        //    if (formationFilter > 0 && formationFilter != (int)eJoinedContentFilter.ALL)
        //    {
        //        tempArr = GetFormationFilter(originList, formationFilter);
        //        if (tempArr != null && tempArr.Count >= 0)
        //        {
        //            formationList = tempArr.ToList();
        //        }
        //    }
        //    else
        //    {
        //        formationList = originList.ToList();
        //    }

        //    var etoJArr = ArrayIntersection(elementList.ToArray(), jobList.ToArray());
        //    var jtoFArr = ArrayIntersection(jobList.ToArray(), formationList.ToArray());
        //    var resultArr = ArrayIntersection(etoJArr, jtoFArr);

        //    return resultArr.ToList();
        //}

        //UserDragon[] ArrayIntersection(UserDragon[] array1, UserDragon[] array2)
        //{
        //    var intersect = array1.Intersect(array2);
        //    List<UserDragon> tempList = new List<UserDragon>();
        //    tempList.Clear();

        //    foreach (UserDragon item in intersect)
        //    {
        //        tempList.Add(item);
        //    }

        //    return tempList.ToArray();
        //}
        //List<UserDragon> GetElementFilter(List<UserDragon> userPartList, int filter)
        //{
        //    List<UserDragon> userDragonFilter = new List<UserDragon>();
        //    userDragonFilter.Clear();

        //    for (var i = 0; i < (int)eDragonListFilterCount.Element; i++)
        //    {
        //        var pow = (int)Math.Pow(2, i);
        //        if ((filter & pow) != 0)
        //        {
        //            var tempFilter = userPartList.FindAll(element => element.Element() == (i + 1));
        //            userDragonFilter = userDragonFilter.Concat(tempFilter).ToList();
        //        }
        //    }

        //    return userDragonFilter;
        //}

        //List<UserDragon> GetJobFilter(List<UserDragon> userJobList, int filter)
        //{
        //    List<UserDragon> userJobFilter = new List<UserDragon>();
        //    userJobFilter.Clear();

        //    for (var i = 0; i < (int)eDragonListFilterCount.Job; i++)
        //    {
        //        var pow = (int)Math.Pow(2, i);
        //        if ((filter & pow) != 0)
        //        {
        //            var tempFilter = userJobList.FindAll(element => element.JOB() == (i + 1));
        //            userJobFilter = userJobFilter.Concat(tempFilter).ToList();
        //        }
        //    }

        //    return userJobFilter;
        //}

        //List<UserDragon> GetFormationFilter(List<UserDragon> userFormationList, int filter)
        //{
        //    List<UserDragon> userFormationFilter = new List<UserDragon>();
        //    userFormationFilter.Clear();

        //    List<UserDragon> tempRow = new List<UserDragon>();
        //    for (var i = 0; i < (int)eDragonListFilterCount.TeamFormation; i++)
        //    {
        //        tempRow.Clear();
                
        //        var pow = (int)Math.Pow(2, i);
        //        if ((filter & pow) != 0)
        //        {
        //            switch ((eJoinedContentFilter)pow)
        //            {
        //                case eJoinedContentFilter.Adventure:
        //                {
        //                    tempRow = GetDragonListByTagList(User.Instance.PrefData.GetAdventureFormation());
        //                }
        //                break;
        //                case eJoinedContentFilter.Arena_Atk:
        //                {
        //                    tempRow = GetDragonListByTagList(User.Instance.PrefData.GetArenaFomation());
        //                }
        //                break;
        //                case eJoinedContentFilter.Arena_Def:
        //                {
        //                    tempRow = GetDragonListByTagList(User.Instance.PrefData.GetArenaFomation(false));
        //                }
        //                break;
        //                case eJoinedContentFilter.Daily_Dungeon_Mon:
        //                {
        //                    tempRow = GetDragonListByTagList(User.Instance.PrefData.GetDailyFormation(eDailyType.Mon));
        //                }
        //                break;
        //                case eJoinedContentFilter.Daily_Dungeon_Tue:
        //                {
        //                    tempRow = GetDragonListByTagList(User.Instance.PrefData.GetDailyFormation(eDailyType.Tue));
        //                }
        //                break;
        //                case eJoinedContentFilter.Daily_Dungeon_Wed:
        //                {
        //                    tempRow = GetDragonListByTagList(User.Instance.PrefData.GetDailyFormation(eDailyType.Wed));
        //                }
        //                break;
        //                case eJoinedContentFilter.Daily_Dungeon_Thu:
        //                {
        //                    tempRow = GetDragonListByTagList(User.Instance.PrefData.GetDailyFormation(eDailyType.Thu));
        //                }
        //                break;
        //                case eJoinedContentFilter.Daily_Dungeon_Fri:
        //                {
        //                    tempRow = GetDragonListByTagList(User.Instance.PrefData.GetDailyFormation(eDailyType.Fri));
        //                }
        //                break;
        //                case eJoinedContentFilter.Daily_Dungeon_Sat:
        //                {
        //                    tempRow = GetDragonListByTagList(User.Instance.PrefData.GetDailyFormation(eDailyType.Sat));
        //                }
        //                break;
        //                case eJoinedContentFilter.Daily_Dungeon_Sun:
        //                {
        //                    tempRow = GetDragonListByTagList(User.Instance.PrefData.GetDailyFormation(eDailyType.Sun));
        //                }
        //                break;
        //            }

        //            if(tempRow.Count > 0)
        //                userFormationFilter = userFormationFilter.Concat(tempRow).ToList();
        //        }
        //    }

        //    return userFormationFilter;
        //}

        //List<UserDragon> GetDragonListByTagList(List<int> _tagList)
        //{
        //    List<UserDragon> ret = new List<UserDragon>();

        //    if (_tagList == null || _tagList.Count <= 0)
        //        return ret;

        //    ret.Clear();

        //    foreach(var tag in _tagList)
        //    {
        //        if (tag <= 0)
        //            continue;

        //        var userDragon = User.Instance.DragonData.GetDragon(tag);
        //        if (userDragon == null)
        //            continue;

        //        ret.Add(userDragon);
        //    }

        //    return ret;
        //}

        #endregion


        void SetArtBlockState()
        {
            artBlockBtn.gameObject.SetActive(false); // 서버 응답동안 보이면 안되니깐 꺼둠
            //if (User.Instance.ENABLE_P2E && GameConfigTable.IsRegistedVersion()) 
            //{
            //    artBlockBtn.gameObject.SetActive(true);
            //    artBlockBtn.GetComponent<Image>().sprite = User.Instance.UserData.ExtraStatBuff.IsArtBlockAble ? artBlockAbleImg : artBlockDisableImg;
            //}
            //else
            //{
            //    artBlockBtn.gameObject.SetActive(false);
            //}
        }
        public void OnClickArtBlock()
        {
            PopupManager.OpenPopup<ArtBlockPopup>();
            LoginManager.Instance.SetFirebaseEvent("art_block_icon_clicked");
        }
    }
}

