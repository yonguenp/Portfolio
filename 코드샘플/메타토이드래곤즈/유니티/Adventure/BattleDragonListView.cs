using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace SandboxNetwork { 
    public class BattleDragonListView : MonoBehaviour
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

        protected List<UserDragon> userDragons = null;

        protected List<UserDragon> viewDragons = null;
        protected bool viewDirty = true;

        protected List<int> dragonTagList = new List<int>();
        public delegate void clickCallBack(string CustomEventData);
        protected clickCallBack clickRegistCallBack = null;
        protected clickCallBack clickReleaseCallback = null;

        int currentSortIndex = 0;

        FilterPopupData filterData = null;

        protected bool isInitFirst = false;

        public void SetRegistCallBack (clickCallBack ok_cb)
        {
            if(ok_cb != null)
            {
                clickRegistCallBack = ok_cb;
            }
        }
        public void SetReleaseCallback(clickCallBack ok_cb)
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

            if(isBgOn && bgObject != null)
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
        public void Init(int[] dragonTagList)
        {
            Init();
            InitDragonTagList(dragonTagList);
            DrawScrollView();

        }
        protected virtual void InitDragonTagList(int[] dragonTagArr)
        {
            dragonTagList = new List<int>();
            for(int i =0; i < dragonTagArr.Length; ++i)
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
                saveButton.SetButtonSpriteState(_state);
        }

        public virtual List<UserDragon> GetAbleDragons()
        {
            return User.Instance.DragonData.GetAllUserDragons();
        }

        public void InitSuggest(int[] dragonTagList)
        {
            userDragons = GetAbleDragons();
            InitDragonTagList(dragonTagList);
        }
        public virtual void RefreshList(int[] dragonTagList)
        {
            dropdownController.InitDropDown();
            InitDragonTagList(dragonTagList);

            viewDirty = true;
            DrawScrollView(false);
        }

        public void RefreshSort()
        {
            SetListCustomSort(currentSortIndex);
        }
        public void OnClickElementSort(string customEventData)
        {
            var checker = int.Parse(customEventData);

            if (dropdownController.GetDropdownIndex(eDropDownType.ELEMENT) == checker)
            {
                //SetDropDownVisible(eDropDownType.ELEMENT, false);
                return;
            }

            
            SetListCustomSort(currentSortIndex);

            if (checker != 0)
            {
                viewDragons = viewDragons.FindAll(Element => Element.Element() == checker);
            }
            dropdownController.SetDropdownIndex(eDropDownType.ELEMENT, checker);
            dropdownController.RefreshSpecificFilterLabel(eDropDownType.ELEMENT);
            dropdownController.InitDropDown();

            viewDirty = true;
            ForceUpdate();
        }

        protected virtual void SetListCustomSort(int sortIndex)
        {
            if (userDragons == null)
            {
                userDragons = GetAbleDragons();
            }

            //소팅 구성 데이터 map 세팅 - 소팅 하기전 기본 map 형태 // init에서 맵 구성 완료.
            var sortFunc = Sort(sortIndex);
            currentSortIndex = sortIndex;
            if (sortFunc == null)
            {
                return;
            }
            viewDragons = userDragons.ToList(); // 깊은 복사

            FilteringDragon();//필터 데이터 세팅 -> viewDragon의 필터 적용

            //userDragons.Sort(sortFunc);
            //viewDragons = userDragons.ConvertAll(s => s);//viewdragons로 복사
            viewDragons.Sort(sortFunc);
            viewDirty = true;

            //현재 드래곤 리스트상태 값 저장 (드래곤 레벨업, 스킬, 장비 넘기기 시 해당 기준 소팅 리스트로 사용)
            if (viewDragons != null && viewDragons.Count > 0)
            {
                var length = viewDragons.Count;
                for (var i = 0; i < length; i++)
                {
                    var dragonData = viewDragons[i];
                    if (dragonData == null)
                    {
                        continue;
                    }

                    if (dragonData.Tag <= 0)
                    {
                        continue;
                    }

                    PopupManager.GetPopup<DragonManagePopup>().DragonInfoList.Add(dragonData.Tag);
                }
            }
        }

        protected bool IsInTeamDragon(int tag)
        {
            if(dragonTagList==null || dragonTagList.Count<=0)
            {
                return false;
            }

            for(int i =0;i< dragonTagList.Count; ++i)
            {
                if (dragonTagList[i] == tag) return true;
            }
            return false;
        }
        public virtual void DrawScrollView(bool _initPos = true)
        {
            if (!viewDirty || tableViewGrid == null || viewDragons == null)
            {
                return;
            }

            if(invenCheckLabel != null)
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


            tableViewGrid.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject itemNode, ITableData item) => {
                if (itemNode == null || item == null)
                {
                    return;
                }
                var frame = itemNode.GetComponent<DragonPortraitFrame>();
                if (frame == null)
                {
                    return;
                }
                var dragonData = (UserDragon)item;
                var dragonTableData = CharBaseData.Get(dragonData.Tag);
                if (dragonTableData == null) 
                    return;
                bool isRegist = IsInTeamDragon(dragonData.Tag);

                frame.SetDragonPortraitFrame(dragonData,isRegist);
                if (User.Instance.DragonData.IsFavorite(dragonData.Tag))
                {
                    frame.SetFrameColor(new Color(0.0f,0.8f,0.0f));
                }

                frame.setCallback((param)=>
                {
                    bool check = IsInTeamDragon(int.Parse(param));
                    if (check)
                    {
                        if(clickReleaseCallback != null)
                        {
                            clickReleaseCallback(dragonData.Tag.ToString());
                        }
                    }
                    else
                    {
                        if (clickRegistCallBack != null)
                        {
                            clickRegistCallBack(dragonData.Tag.ToString());
                        }
                    }
                });
            }));

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

            //SetCustomElementList();
            ForceUpdate();
        }
        //void SetCustomElementList()
        //{
        //    var currentElementSortIndex = dropdownController.GetDropdownIndex(eDropDownType.ELEMENT);
        //    if (currentElementSortIndex != 0 && currentElementSortIndex >= 0)
        //    {
        //        viewDragons = viewDragons.FindAll(Element => Element.Element() == currentElementSortIndex);
        //    }
        //}
        public void ForceUpdate()
        {
            DrawScrollView();
        }
        public virtual void Init()
        {
            userDragons = GetAbleDragons();

            if(tableViewGrid != null && !isInitFirst)
            {
                tableViewGrid.OnStart();
                isInitFirst = true;
            }

            InitDragonInfoData();
            InitCustomSort();
        }
        protected void InitDragonInfoData()
        {
            if(PopupManager.GetPopup<DragonManagePopup>() != null)
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

        public void RefreshDragonCountLabel(int count , int TotalCount)
        {
            if(dragonCountLabel != null)
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

        protected virtual int Sort1(UserDragon a, UserDragon b)
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

        protected virtual int Sort2(UserDragon a, UserDragon b)
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

        protected virtual int Sort3(UserDragon a, UserDragon b)
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

        protected virtual int Sort4(UserDragon a, UserDragon b)
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

        protected virtual int Sort5(UserDragon a, UserDragon b)
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

        protected virtual int Sort6(UserDragon a, UserDragon b)
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

        protected virtual int Sort7(UserDragon a, UserDragon b)
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
        protected int SortFavorite(UserDragon a, UserDragon b)
        {
            return User.Instance.DragonData.IsFavorite(b.Tag).CompareTo(User.Instance.DragonData.IsFavorite(a.Tag));
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
            if(filterData == null)
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
        

        List<UserDragon> GetDragonListByTagList(List<int> _tagList)
        {
            List<UserDragon> ret = new List<UserDragon>();

            if (_tagList == null || _tagList.Count <= 0)
                return ret;

            ret.Clear();

            foreach (var tag in _tagList)
            {
                if (tag <= 0)
                    continue;

                var userDragon = User.Instance.DragonData.GetDragon(tag);
                if (userDragon == null)
                    continue;

                ret.Add(userDragon);
            }

            return ret;
        }
        #endregion





    }
}