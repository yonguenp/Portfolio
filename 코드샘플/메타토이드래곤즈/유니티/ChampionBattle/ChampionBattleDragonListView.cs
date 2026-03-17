using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace SandboxNetwork
{
    public class ChampionBattleDragonListView : MonoBehaviour
    {
        [Header("on BG mode ")]
        [SerializeField]
        bool isBgOn = false;
        [SerializeField]
        GameObject bgObject = null;


        [Header("Scroll")]
        [SerializeField]
        protected TableViewGrid tableViewGrid = null;

        [Header("DropDown")]
        [SerializeField]
        protected DropDownUIController dropdownController = null;


        [SerializeField]
        protected Text invenCheckLabel = null;

        [SerializeField]
        protected Text dragonCountLabel = null;

        [SerializeField] Button saveButton = null;
        [SerializeField] Text saveButtonText = null;

        protected List<ChampionDragon> viewDragons = null;
        protected bool viewDirty = true;

        protected List<int> dragonTagList = new List<int>();
        public delegate void ClickCallBack(int tag);
        protected ClickCallBack clickRegistCallBack = null;
        protected ClickCallBack clickReleaseCallback = null;

        int currentSortIndex = 0;
        FilterPopupData filterData = null;
        protected bool isInitFirst = false;

        ChampionBattleSetting parent = null;

        public void SetRegistCallBack(ClickCallBack ok_cb)
        {
            if (ok_cb != null)
            {
                clickRegistCallBack = ok_cb;
            }
        }
        public void SetReleaseCallback(ClickCallBack ok_cb)
        {
            if (ok_cb != null)
            {
                clickReleaseCallback = ok_cb;
            }
        }

        public void OnShowList()
        {
            if (gameObject.activeInHierarchy == false)
            {
                gameObject.SetActive(true);
            }

            if (isBgOn && bgObject != null)
            {
                bgObject.SetActive(true);
            }
        }
        public void OnHideList()
        {
            if (gameObject.activeInHierarchy == true)
            {
                gameObject.SetActive(false);
            }

            if (isBgOn && bgObject != null)
            {
                bgObject.SetActive(false);
            }
        }
        public bool IsShowList()
        {
            return gameObject.activeInHierarchy;
        }
        public void Init(int[] dragonTagList, ChampionBattleSetting p)
        {
            parent = p;

            Init(dragonTagList);
        }

        public void Init(int[] dragonTagList)
        {
            Init();
            InitDragonTagList(dragonTagList);
            DrawScrollView();
        }

        protected virtual void InitDragonTagList(int[] dragonTagArr)
        {
            dragonTagList = new List<int>();
            for (int i = 0; i < dragonTagArr.Length; ++i)
            {
                if (dragonTagArr[i] != 0)
                {
                    dragonTagList.Add(dragonTagArr[i]);
                }
            }
        }

        public virtual void RefreshSaveButtonState(bool _state)
        {
            if (saveButton != null)
            {
                saveButton.SetButtonSpriteState(_state);
                saveButton.interactable = _state;
            }

            if (_state)
            {
                saveButtonText.text = StringData.GetStringByStrKey("팀세팅저장");
            }
            else
            {
                saveButtonText.text = StringData.GetStringByStrKey("팀세팅마감");
            }
        }

        public virtual void RefreshList(int[] dragonTagList)
        {
            dropdownController.InitDropDown();
            InitDragonTagList(dragonTagList);

            //이미 필터링된 스크롤뷰에 드래곤 태그 리스트 기반으로 갱신하는 거면 scrollRefresh만 하면되지않을까 해서 수정함
            //수정 이유 -> 151줄 원소 인덱스값 같으면 갱신안하는 코드 때문에, 원소 필터 누른 상태에서 드래곤 넣기, 빼기 하면 UI 갱신 안되는 현상 발생

            viewDirty = true;
            DrawScrollView(false);
        }

        public void RefreshSort()
        {
            SetListCustomSort(currentSortIndex);
        }

        protected virtual List<ChampionDragon> GetSelectableDragons()
        {
            return ChampionManager.Instance.MyInfo.GetAllChampionDragons();
        }
        protected virtual void SetListCustomSort(int sortIndex)
        {
            var dragonList = GetSelectableDragons();

            //소팅 구성 데이터 map 세팅 - 소팅 하기전 기본 map 형태 // init에서 맵 구성 완료.
            var sortFunc = Sort(sortIndex);
            currentSortIndex = sortIndex;
            if (sortFunc == null)
            {
                return;
            }
            viewDragons = dragonList.ToList(); // 깊은 복사

            FilteringDragon();//필터 데이터 세팅 -> viewDragon의 필터 적용

            viewDragons = dragonList.ConvertAll(s => s);//viewdragons로 복사

            viewDirty = true;
        }

        protected virtual bool IsInTeamDragon(int tag)
        {
            var teamType = ChampionManager.Instance.MyInfo.GetForamtionType(tag);
            if (teamType != ParticipantData.eTournamentTeamType.NONE)
            {
                if (teamType == parent.CurTeamType)
                {
                    return parent.battleLine.IsContainDragon(tag);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return parent.battleLine.IsContainDragon(tag);
            }
        }

        public virtual void ScrollDelegate(GameObject itemNode, ITableData item)
        {
            if (itemNode == null || item == null)
            {
                return;
            }
            var frame = itemNode.GetComponent<ChampionBattleDragonPortraitFrame>();
            if (frame == null)
            {
                return;
            }
            var dragonData = (UserDragon)item;
            var dragonTableData = CharBaseData.Get(dragonData.Tag);
            if (dragonTableData == null)
                return;

            var teamType = ChampionManager.Instance.MyInfo.GetForamtionType(dragonData.Tag);
            if (teamType != ParticipantData.eTournamentTeamType.NONE)
            {
                if (teamType == parent.CurTeamType)
                {
                    if (!parent.battleLine.IsContainDragon(dragonData.Tag))
                    {
                        teamType = ParticipantData.eTournamentTeamType.NONE;
                    }
                }
            }
            else
            {
                if (parent.battleLine.IsContainDragon(dragonData.Tag))
                {
                    teamType = parent.CurTeamType;
                }
            }

            frame.SetDragonPortraitFrame(dragonData, teamType);
            frame.setCallback((param) =>
            {
                if (parent != null)
                {
                    if (!parent.IsCurTeamSettableTime())
                    {
                        string strTeam = "";
                        switch(parent.CurTeamType)
                        {
                            case ParticipantData.eTournamentTeamType.ATTACK:
                                strTeam = StringData.GetStringByStrKey("공격팀");
                                break;
                            case ParticipantData.eTournamentTeamType.DEFFENCE:
                                strTeam = StringData.GetStringByStrKey("방어팀");
                                break;
                            case ParticipantData.eTournamentTeamType.HIDDEN:
                                strTeam = StringData.GetStringByStrKey("히든팀");
                                break;
                        }
                        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("챔피언세팅시간오류", strTeam));
                        return;
                    }
                }

                int tag = int.Parse(param);
                bool check = IsInTeamDragon(tag);
                if (check)
                {
                    if (ChampionManager.Instance.MyInfo.GetForamtionType(tag) == ParticipantData.eTournamentTeamType.NONE)
                    {
                        if (clickReleaseCallback != null)
                        {
                            clickReleaseCallback(dragonData.Tag);
                        }
                        return;
                    }

                    if (ChampionManager.Instance.MyInfo.GetForamtionType(tag) != parent.CurTeamType)
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("다른덱에사용중인드래곤"));
                        return;
                    }

                    if (clickReleaseCallback != null)
                    {
                        clickReleaseCallback(dragonData.Tag);
                    }
                }
                else
                {
                    if (clickRegistCallBack != null)
                    {
                        clickRegistCallBack(tag);
                    }
                }
            });
        }
        public virtual void DrawScrollView(bool _initPos = true)
        {
            if (!viewDirty || tableViewGrid == null || viewDragons == null)
            {
                return;
            }

            if (invenCheckLabel != null)
            {
                var isEmpty = viewDragons.Count <= 0;
                invenCheckLabel.gameObject.SetActive(isEmpty);
            }

            List<ITableData> tableViewItemList = new List<ITableData>();
            tableViewItemList.Clear();
            if (viewDragons != null && viewDragons.Count > 0)
            {
                for (var i = 0; i < viewDragons.Count; i++)
                {
                    var data = viewDragons[i];
                    if (data == null)
                    {
                        continue;
                    }

                    tableViewItemList.Add(data);
                }
            }


            tableViewGrid.SetDelegate(new TableViewDelegate(tableViewItemList, ScrollDelegate));

            tableViewGrid.ReLoad(_initPos);
            viewDirty = false;
        }
        public void OnClickCustomSort(string customEventData)
        {
            var checker = int.Parse(customEventData);

            SetListCustomSort(checker);//클릭인덱스 기준 정렬 완료 데이터 받아오기

            dropdownController.SetDropdownIndex(eDropDownType.DEFAULT, checker);
            dropdownController.RefreshAllFilterLabel();
            dropdownController.InitDropDown();

            ForceUpdate();
        }

        public void ForceUpdate()
        {
            DrawScrollView();
        }
        public virtual void Init()
        {
            if (tableViewGrid != null && !isInitFirst)
            {
                tableViewGrid.OnStart();
                isInitFirst = true;
            }

            InitDragonInfoData();
            InitCustomSort();
        }
        protected void InitDragonInfoData()
        {
            if (PopupManager.GetPopup<DragonManagePopup>() != null)
            {
                PopupManager.GetPopup<DragonManagePopup>().CurDragonTag = 0;
            }
        }

        protected void InitCustomSort()
        {
            dropdownController.RefreshAllFilterLabel();
            dropdownController.InitDropDown();
            OnClickCustomSort(dropdownController.GetDropdownIndex(eDropDownType.DEFAULT).ToString());
        }

        public void RefreshDragonCountLabel(int count, int TotalCount)
        {
            if (dragonCountLabel != null)
                dragonCountLabel.text = string.Format("{0}/{1}", count, TotalCount);
        }
        protected virtual System.Comparison<UserDragon> Sort(int index)
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

        protected virtual int Sort0(UserDragon a, UserDragon b)
        {
            var checker = SortGradeDescend(a, b);

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

            return checker;
        }

        protected virtual int Sort1(UserDragon a, UserDragon b)
        {
            var checker = SortGradeAscend(a, b);

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

            return checker;
        }

        protected virtual int Sort2(UserDragon a, UserDragon b)
        {
            var checker = SortLevelDescend(a, b);

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

            return checker;
        }

        protected virtual int Sort3(UserDragon a, UserDragon b)
        {
            var checker = SortLevelAscend(a, b);

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

            return checker;
        }

        protected virtual int Sort4(UserDragon a, UserDragon b)
        {
            var checker = SortBattlePointDescend(a, b);
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

            return checker;
        }

        protected virtual int Sort5(UserDragon a, UserDragon b)
        {
            var checker = SortBattlePointAscend(a, b);

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

            return checker;
        }

        protected virtual int Sort6(UserDragon a, UserDragon b)
        {
            var checker = SortObtainTimeDescend(a, b);
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

            return checker;
        }

        protected virtual int Sort7(UserDragon a, UserDragon b)
        {
            var checker = SortObtainTimeAscend(a, b);

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
                            return SortTagAscend(a, b);
                    }
                }
            }

            return checker;
        }
        protected int SortGradeDescend(UserDragon a, UserDragon b)
        {
            var aGrade = a.Grade();
            var bGrade = b.Grade();
            return bGrade - aGrade;
        }
        //등급 오름차순
        protected int SortGradeAscend(UserDragon a, UserDragon b)
        {
            var aGrade = a.Grade();
            var bGrade = b.Grade();
            return aGrade - bGrade;
        }
        //레벨 내림차순
        protected int SortLevelDescend(UserDragon a, UserDragon b)
        {
            var aLevel = a.Level;
            var bLevel = b.Level;
            return bLevel - aLevel;
        }
        //레벨 오름차순
        protected int SortLevelAscend(UserDragon a, UserDragon b)
        {
            var aLevel = a.Level;
            var bLevel = b.Level;
            return aLevel - bLevel;
        }
        //전투력 내림차순
        protected int SortBattlePointDescend(UserDragon a, UserDragon b)
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
        //전투력 오름차순
        protected int SortBattlePointAscend(UserDragon a, UserDragon b)
        {
            var aInf = a.Status.GetTotalINFFloat();
            var bInf = b.Status.GetTotalINFFloat();

            if (aInf > bInf)
                return 1;
            else if (aInf == bInf)
                return 0;
            else
                return -1;
        }
        //최신 획득 내림 차순
        protected int SortObtainTimeDescend(UserDragon a, UserDragon b)
        {
            var aObtainTime = a.Obtain;
            var bObtaionTime = b.Obtain;
            return bObtaionTime - aObtainTime;
        }
        //최신 획득 오름 차순
        protected int SortObtainTimeAscend(UserDragon a, UserDragon b)
        {
            var aObtainTime = a.Obtain;
            var bObtaionTime = b.Obtain;
            return aObtainTime - bObtaionTime;
        }
        //태그(키 값) 내림 차순
        protected int SortTagDescend(UserDragon a, UserDragon b)
        {
            var aTag = a.Tag;
            var bTag = b.Tag;
            return bTag - aTag;
        }
        //태그(키 값) 오름 차순
        protected int SortTagAscend(UserDragon a, UserDragon b)
        {
            var aTag = a.Tag;
            var bTag = b.Tag;
            return aTag - bTag;
        }

        void FilteringDragon()
        {
            if (filterData == null)
            {
                filterData = new FilterPopupData();
                filterData.Init();
            }


            if (filterData.gradeFilter != eGradeFilter.ALL)
            {
                viewDragons.RemoveAll(dragon =>
                {
                    var grade = SBFunc.GetGradeFilterType(dragon.Grade());
                    return filterData.gradeFilter.HasFlag(grade) == false;
                });
            }
            if (filterData.elementFilter != eElementFilter.ALL)
            {
                viewDragons.RemoveAll(dragon =>
                {
                    var elem = SBFunc.GetElemFilterType(dragon.Element());
                    return filterData.elementFilter.HasFlag(elem) == false;
                });
            }
            if (filterData.jobFilter != eJobFilter.ALL)
            {
                viewDragons.RemoveAll(dragon =>
                {
                    var job = SBFunc.GetJobFilterType(dragon.JOB());
                    return filterData.jobFilter.HasFlag(job) == false;
                });
            }

        }

        #region 필터 데이터
        public void OnClickFilterPopup()//현재 컴포넌트 같이 넘기기
        {
            var popup = PopupManager.OpenPopup<DragonTeamFilterPopup>(filterData);
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

            OnClickCustomSort(dropdownController.GetDropdownIndex(eDropDownType.DEFAULT).ToString());
        }

        #endregion





    }
}