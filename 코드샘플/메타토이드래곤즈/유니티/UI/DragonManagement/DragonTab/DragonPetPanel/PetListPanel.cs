using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{

    public enum ePetPopupState
    {
        Info,
        LevelUp,
        Reinforce,
        Compound,
        Decompose
    }
    public class PetListPanel : MonoBehaviour
    {
        [SerializeField]
        SubLayer[] petTapLayer = null;

        [SerializeField]
        TableViewGrid tableViewGrid = null;
        [SerializeField]
        GameObject tableViewContent = null;

        [Space(10)]
        [Header("scroll Info")]
        [SerializeField]
        Text invenCheckLabel = null;

        [SerializeField]
        GameObject registCntObj = null;
        [SerializeField]
        Text registCntText = null;

        [Header("DropDown")]
        [SerializeField]
        private DropDownUIController dropdownController = null;

        [SerializeField]
        GameObject[] ListBotButtonParents = null;

        [SerializeField]
        RectTransform levelupIconRect = null;
        [SerializeField]
        RectTransform decompoundIconRect = null;

        [SerializeField]
        PetSingleDetailInfoPanel materialDetailPanel = null;

        [SerializeField]
        GameObject arrowNode = null;

        List<UserPet> userPets = null;

        List<UserPet> viewPets = null;
        bool viewDirty = true;
        public bool ViewDirty { set { viewDirty = value; } }

        private bool tableViewResetFlag = true;

        ePetPopupState currentPanelState = ePetPopupState.Info;
        int slotLimit = 99;

        public bool TableViewResetFlag
        {
            get { return tableViewResetFlag; }
            set { tableViewResetFlag = value; }
        }

        PetFilterData filterData = null;

        public int petTag = -1;

        private bool isTableInit = false;
        public virtual UserPetData GetPetInfo()
        {
            return User.Instance.PetData;
        }
        public virtual UserDragonData GetDragonInfo()
        {
            return User.Instance.DragonData;
        }

        public virtual int CurPopupPetTag
        {
            get
            {
                return PopupManager.GetPopup<DragonManagePopup>().CurPetTag;
            }
            set
            {
                PopupManager.GetPopup<DragonManagePopup>().CurPetTag = value;
            }
        }

        // 레벨업, 합성 파트

        const int PET_COMPOUND_MAX_SLOT_NUM = 2;//펫 합성 최대 재료 갯수
        const string PET_POPUP_CHECK_MANUAL = "PET_POPUP_CHECK_MANUAL";
        const string PET_POPUP_CHECK_AUTO = "PET_POPUP_CHECK_AUTO";
        const int CONSTRAINT_PET_LEVEL = 30;
        const int PET_LVUP_CONSUME_MAX_SLOT_NUM = 99;//펫 레벨 업 시 소모되는 슬롯 최대 개수
        const int PET_DECOMPOSE_MAX_SLOT = 99;// 한번에 분해할 수 있는 최대 수량
        List<int> petTagList = new List<int>();//레벨업, 합성 요청할 재료 팻 태그 리스트
        public delegate void func();
        public delegate void funcStr(string CustomEventData);
        public delegate void funcList(List<int> CustomEventData);

        private funcStr clickRegistCallback = null;
        public funcStr ClickRegistCallback { set { if (value != null) { clickRegistCallback = value; } } }

        private funcStr clickReleaseCallback = null;
        public funcStr ClickReleaseCallback { set { if (value != null) { clickReleaseCallback = value; } } }

        private funcList clickAutoRegistCallback = null;
        public funcList ClickAutoRegistCallback { set { if (value != null) { clickAutoRegistCallback = value; } } }

        private VoidDelegate clickReleasAlleCallback = null;
        public VoidDelegate ClickReleaseAllCallback { set { if (value != null) { clickReleasAlleCallback = value; } } }


        public void Init(ePetPopupState state = ePetPopupState.Info)
        {
            if(tableViewGrid != null && !isTableInit)
            {
                tableViewGrid.OnStart();
                isTableInit = true;
            }
            currentPanelState = state;
            userPets = GetPetInfo().GetAllUserPets();

            InitPetInfoData();
            InitCustomSort();
            InitCheckElementSort();//이전 필터를 세팅한 상태에서 갱신 요청 시 돌리기
            InitDetailPanel();

            SetPopupState();
        }

        void SetPopupState()
        {
            foreach(var btns in ListBotButtonParents)
            {
                btns.SetActive(false);
            }

            switch (currentPanelState)
            {
                case ePetPopupState.Info:
                    ListBotButtonParents[0].SetActive(true);
                    registCntObj.SetActive(false);
                    break;
                case ePetPopupState.LevelUp:
                    ListBotButtonParents[1].SetActive(true);
                    slotLimit = PET_LVUP_CONSUME_MAX_SLOT_NUM;
                    registCntObj.SetActive(true);
                    registCntText.text = "0/" + PET_LVUP_CONSUME_MAX_SLOT_NUM.ToString();
                    InitTagsList();
                    break;
                case ePetPopupState.Reinforce:
                    ListBotButtonParents[2].SetActive(true);
                    registCntObj.SetActive(false);
                    break;
                case ePetPopupState.Compound:
                    ListBotButtonParents[3].SetActive(false);
                    slotLimit = PET_COMPOUND_MAX_SLOT_NUM;
                    InitTagsList();
                    if (petTag > 0)
                        PushMaterialList(petTag);

                    OnClickCustomSort(dropdownController.GetDropdownIndex(eDropDownType.DEFAULT).ToString());
                    break;
                case ePetPopupState.Decompose:
                    slotLimit = PET_DECOMPOSE_MAX_SLOT;
                    InitTagsList();
                    registCntObj.SetActive(true);
                    registCntText.text = "0/" + PET_LVUP_CONSUME_MAX_SLOT_NUM.ToString();
                    ListBotButtonParents[4].SetActive(true);
                    break;
            }
        }

        void InitPetInfoData()
        {
            if (CurPopupPetTag != 0)
                petTag = CurPopupPetTag;
            else
                petTag = -1;
        }

        public void InitCustomSort()
        {
            dropdownController.RefreshAllFilterLabel();
            dropdownController.InitDropDown();
            OnClickCustomSort(dropdownController.GetDropdownIndex(eDropDownType.DEFAULT).ToString());
        }


        public void ForceUpdate()
        {
            DrawScrollView();
        }

        void DrawScrollView()
        {
            if (!viewDirty || tableViewGrid == null || viewPets == null)
            {
                return;
            }

            var isEmpty = viewPets.Count <= 0;

            if(invenCheckLabel != null)
            {
                invenCheckLabel.gameObject.SetActive(isEmpty);

                if (isEmpty)
                {
                    switch(currentPanelState)
                    {
                        case ePetPopupState.Info:
                        case ePetPopupState.Reinforce:
                        case ePetPopupState.LevelUp:
                            invenCheckLabel.text = StringData.GetStringByIndex(100002234);//보유한 펫이 없습니다.
                            break;
                        case ePetPopupState.Compound:
                            invenCheckLabel.text = StringData.GetStringByIndex(100002247);//펫 합성 조건에 충족되는 펫이 없습니다.
                            break;
                        case ePetPopupState.Decompose:
                            invenCheckLabel.text = StringData.GetStringByIndex(100002234);//보유한 펫이 없습니다.
                            break;
                    }    
                }
            }

            List<ITableData> tableViewItemList = new List<ITableData>();
            tableViewItemList.Clear();
            if (viewPets != null && viewPets.Count > 0)
            {
                for (var i = 0; i < viewPets.Count; i++)
                {
                    var data = viewPets[i];
                    if (data == null)
                    {
                        continue;
                    }

                    tableViewItemList.Add(data);
                }
            }

            tableViewGrid.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
                if (node == null)
                {
                    return;
                }
                var frame = node.GetComponent<PetPortraitFrame>();
                if (frame == null)
                {
                    return;
                }

                var petData = (UserPet)item;

                frame.SetPetPortraitFrame(petData);
                frame.SetVisibleSelectedNode(false);

                switch (currentPanelState)
                {
                    case ePetPopupState.Info:
                    case ePetPopupState.Reinforce:
                        frame.SetVisibleClickNode(petData.Tag == petTag);
                        break;
                    case ePetPopupState.LevelUp:
                    case ePetPopupState.Compound:
                    case ePetPopupState.Decompose:
                        var index = -1;
                        if (petTagList != null && petTagList.Count > 0)
                            index = petTagList.IndexOf(petData.Tag);

                        frame.SetVisibleClickNode(false);
                        if (index > -1)
                            frame.SetVisibleSelectedNode(true);//파츠가 이미 등록된 상태에서 다시 그릴경우, 현재 켜져있으면 켜기


                        if(currentPanelState != ePetPopupState.Compound)
                        {
                            var lastindex = GetLastPetIndex();
                            var isShow = materialDetailPanel.IsShowPanel;
                            if(lastindex >= 0 && lastindex == petData.Tag && isShow)
                            {
                                frame.SetVisibleClickNode(true);
                            }
                        }
                        break;
                }
                
                frame.SetCallback((param)=> {
                    ClickPetPortraitFrame(param, frame, petData);
                });
            }));

            tableViewGrid.ReLoad(tableViewResetFlag);
            viewDirty = false;
        }

        void ClickPetPortraitFrame(string _param, PetPortraitFrame _frame , UserPet _petData)
        {
            var isSelected = _frame.IsSelected;//이미 선택이 되었는지
            switch (currentPanelState)
            {
                case ePetPopupState.Info:
                case ePetPopupState.Reinforce:
                    OnClickFrame(_param);
                    break;
                case ePetPopupState.LevelUp:
                case ePetPopupState.Decompose:
                case ePetPopupState.Compound:
                    if (isSelected)
                    {
                        PopMaterialList(_petData.Tag);
                        if (clickReleaseCallback != null)
                            clickReleaseCallback(_param);

                        if (currentPanelState != ePetPopupState.Compound)
                        {
                            var isShow = materialDetailPanel.IsShowPanel;
                            if (isShow)
                                SetDetailButtonState(true);
                            else
                                PetDataEvent.FocusFrame(_petData.Tag, false);
                        }
                    }
                    else
                    {
                        var toastCheck = ToastMaxCountCheck();
                        if (toastCheck)
                            return;

                        var constraintCheck = false;
                        if (currentPanelState != ePetPopupState.Compound)
                            constraintCheck = IsAlertCondition(_petData.Tag);

                        if (constraintCheck)
                        {
                            ShowAlertPopup(PET_POPUP_CHECK_MANUAL, _petData.Tag.ToString(), PushAndClickMaterialProcess, () => {
                                _frame.SetVisibleSelectedNode(false);//취소시 선택된 프레임 끄기            
                            });
                        }
                        else
                            PushAndClickMaterialProcess(_petData.Tag.ToString());
                    }
                    _frame.SetVisibleSelectedNode(!isSelected);//버튼 UI 켜기

                    if(currentPanelState == ePetPopupState.Compound)
                    {
                        //materialList 기준으로 다시 그리기
                        SetScrollRefreshByCompoundListLength(isSelected);
                        InitCustomSort();
                        SetTableViewFlag(true);
                    }
                    break;
            }
        }

        //펫 재료 상세 패널 세팅하기
        void InitDetailPanel()
        {
            SetDetailButtonState(false);
        }

        public void OnClickDetailPanel()
        {
            SetDetailButtonState(!materialDetailPanel.IsShowPanel, true);
        }

        public void SetDetailButtonState(bool _state, bool _isShowToast = false)
        {
            if (materialDetailPanel == null)
                return;

            var isDetailShowCondition = currentPanelState == ePetPopupState.LevelUp || currentPanelState == ePetPopupState.Decompose;
            if (!isDetailShowCondition)
            {
                materialDetailPanel.SetVisible(false);
                return;
            }

            var targetPetIndex = GetLastPetIndex();
            if (targetPetIndex <= 0)
            {
                materialDetailPanel.SetVisible(false);
                SetDetailIcon(false);

                PetDataEvent.FocusFrame(-1, false);

                if (_isShowToast)
                    ToastManager.On(StringData.GetStringByStrKey("펫선택버튼"));
                return;
            }

            var petData = viewPets.Find(element => element.Tag == targetPetIndex);
            if(petData == null)
            {
                materialDetailPanel.SetVisible(false);
                SetDetailIcon(false);

                PetDataEvent.FocusFrame(-1, false);

                if (_isShowToast)
                    ToastManager.On(StringData.GetStringByStrKey("펫선택버튼"));
                return;
            }

            if (_state)
            {
                materialDetailPanel.InitUI(petData);
                arrowNode.SetActive(GetCurrentSelectTagList().Count > 1);

                var nodeIndex = tableViewGrid.GetDataIndex(petData);
                if(nodeIndex >= 0)
                {
                    var isVisibleIndex = tableViewGrid.IsVisibleNode(nodeIndex);//현재 보여주는 노드에 인덱스가 포함인가
                    var currentState = materialDetailPanel.IsShowPanel;
                    if (currentState)
                    {
                        if(isVisibleIndex)
                            PetDataEvent.FocusFrame(petData.Tag, true);
                        else
                        {
                            tableViewGrid.ScrollMoveTweenItem(petData, eTableViewAnchor.FIRST, () =>
                            {
                                PetDataEvent.FocusFrame(petData.Tag, true);
                            });
                        }
                    }
                    else
                    {
                        tableViewGrid.ScrollMoveTweenItem(petData, eTableViewAnchor.FIRST, () =>
                        {
                            PetDataEvent.FocusFrame(petData.Tag, true);
                        });
                    }

                }
                else//인덱스가 없는 경우?
                {

                }
            }
            else
            {
                PetDataEvent.FocusFrame(petData.Tag, false);
            }

            materialDetailPanel.SetVisible(_state);
            SetDetailIcon(materialDetailPanel.IsShowPanel);
        }

        void SetDetailIcon(bool _isShow)
        {
            if (currentPanelState == ePetPopupState.LevelUp)
                levelupIconRect.localScale = _isShow ? new Vector3(1, -1, 1) : new Vector3(1, 1, 1);
            else if (currentPanelState == ePetPopupState.Decompose)
                decompoundIconRect.localScale = _isShow ? new Vector3(1, -1, 1) : new Vector3(1, 1, 1);
        }

        public void OnClickArrow(bool _isRight)
        {
            var currentSelectTagList = GetCurrentSelectTagList();
            if (currentSelectTagList == null || currentSelectTagList.Count <= 0)
                return;

            var nextIndex = GetPetNextIndexByTagList(_isRight);
            if (nextIndex < 0)
                return;

            var targetTag = currentSelectTagList[nextIndex];
            var petData = viewPets.Find(element => element.Tag == targetTag);

            var isShow = materialDetailPanel.IsShowPanel;
            if(isShow)
            {
                materialDetailPanel.InitUI(petData);
                materialDetailPanel.SetVisible(true);
                tableViewGrid.ScrollMoveTweenItem(petData, eTableViewAnchor.FIRST, ()=> {
                    PetDataEvent.FocusFrame(petData.Tag, true);
                });
            }
        }

        int GetPetNextIndexByTagList(bool _isRight)//재료 리스트에서 찾기
        {
            var currentSelectTagList = GetCurrentSelectTagList();

            var currentIndex = materialDetailPanel.PetTag;

            if (currentIndex <= 0)
                return currentSelectTagList.Count - 1;

            var currentListIndex = currentSelectTagList.IndexOf(currentIndex);

            if (currentListIndex < 0)
                return -1;

            int retIndex = -1;
            if(_isRight)
            {
                if (currentListIndex + 1 >= currentSelectTagList.Count)
                    retIndex = 0;
                else
                    retIndex = currentListIndex + 1;
            }
            else
            {
                if (currentListIndex - 1 < 0)
                    retIndex = currentSelectTagList.Count - 1;
                else
                    retIndex = currentListIndex - 1;
            }
            return retIndex;
        }

        int GetLastPetIndex()
        {
            var currentPetTagList = GetCurrentSelectTagList();
            if (currentPetTagList == null || currentPetTagList.Count <= 0)
                return -1;

            else
                return currentPetTagList[currentPetTagList.Count - 1];
        }

        /// <summary>
        /// 필터나 정렬이 들어가면, petTagList를 정제해서 사용해야함.
        /// </summary>
        /// <returns></returns>
        List<int> GetCurrentSelectTagList()//petTagList -> 전체 태그 리스트 // viewPets -> 현재 보여지는 펫
        {
            List<int> sortPetTagList = new List<int>();

            foreach(var petTag in petTagList)
            {
                if (petTag <= 0)
                    continue;

                var isPetData = viewPets.Find(element => element.Tag == petTag);
                if (isPetData != null)
                    sortPetTagList.Add(petTag);
            }

            return sortPetTagList;
        }

        public void RefreshViewPetsData(int petTag = -1)//지정 펫 이면 단일, 없으면 전체 데이터 갱신
        {
            if (viewPets == null || viewPets.Count <= 0)
            {
                return;
            }

            if (petTag < 0)
            {
                for (var i = 0; i < viewPets.Count; i++)
                {
                    var viewPetData = viewPets[i];
                    if (viewPetData == null)
                        continue;

                    var viewPetTag = viewPetData.Tag;
                    var petData = GetPetInfo().GetPet(viewPetTag);
                    if (petData == null)
                        continue;

                    viewPetData.SetReinforce(petData.Reinforce);
                }
            }
            else
            {
                var petData = GetPetInfo().GetPet(petTag);
                if (petData == null)
                    return;

                for (var i = 0; i < viewPets.Count; i++)
                {
                    var viewPetData = viewPets[i];
                    if (viewPetData == null)
                        continue;

                    var viewPetTag = viewPetData.Tag;
                    if (viewPetTag == petTag)
                    {
                        viewPetData.SetReinforce(petData.Reinforce);
                        break;
                    }
                }
            }

            viewDirty = true;
        }


        void InitPartslotClickNode()
        {
            var children = SBFunc.GetChildren(tableViewContent.transform);
            if (children == null || children.Length <= 0)
            {
                return;
            }

            for (var i = 0; i < children.Length; i++)
            {
                var node = children[i];
                if (node == null)
                {
                    continue;
                }

                var partComp = node.GetComponent<PetPortraitFrame>();
                if (partComp == null)
                {
                    continue;
                }
                partComp.SetVisibleClickNode(false);
            }
        }

        void SetVisibleClickNode(int petTag)
        {
            var children = SBFunc.GetChildren(tableViewContent.transform);
            if (children == null || children.Length <= 0)
            {
                return;
            }

            for (var i = 0; i < children.Length; i++)
            {
                var node = children[i];
                if (node == null)
                {
                    continue;
                }

                var partComp = node.GetComponent<PetPortraitFrame>();
                if (partComp == null)
                {
                    continue;
                }

                var tempPetTag = partComp.PetTag;
                partComp.SetVisibleClickNode(petTag == tempPetTag);
            }
        }

        public void OnClickFrame(string customEventData)
        {
            if (int.TryParse(customEventData, out int customValue) && customValue == petTag)
            {
                return;
            }

            CurPopupPetTag = customValue;
            tableViewResetFlag = false;

            var tempSortIndex = 0;
            var currentElementSortIndex = dropdownController.GetDropdownIndex(eDropDownType.ELEMENT);
            if (currentElementSortIndex >= 0)
            {
                tempSortIndex = currentElementSortIndex;
            }

            petTapLayer[(int)currentPanelState].Init();

            dropdownController.SetDropdownIndex(eDropDownType.ELEMENT, 0);
            OnClickElementSort(tempSortIndex.ToString());

            tableViewResetFlag = true;
        }

        void InitCheckElementSort()
        {
            var currentElementSortIndex = dropdownController.GetDropdownIndex(eDropDownType.ELEMENT);
            if (currentElementSortIndex >= 0)
            {
                var tempSortIndex = currentElementSortIndex;
                dropdownController.SetDropdownIndex(eDropDownType.ELEMENT, 0);
                OnClickElementSort(tempSortIndex.ToString());
            }
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

            var customSortIndex = dropdownController.GetDropdownIndex(eDropDownType.DEFAULT);
            switch (currentPanelState)
            {
                case ePetPopupState.Compound:
                    GetListCustomSortCompound(customSortIndex);
                    break;
                case ePetPopupState.Decompose:
                    GetListCustomSortExceptLinked(customSortIndex);
                    SetDetailButtonState(false);
                    break;
                case ePetPopupState.LevelUp:
                    GetListCustomSortExceptLinked(customSortIndex, true);
                    SetDetailButtonState(false);
                    break;
                default:
                    GetListCustomSort(customSortIndex);
                    break;
            }
            if (checker != 0)
            {
                viewPets = viewPets.FindAll(Element => Element.Element() == checker);
            }

            dropdownController.SetDropdownIndex(eDropDownType.ELEMENT, checker);
            dropdownController.RefreshSpecificFilterLabel(eDropDownType.ELEMENT);
            dropdownController.InitDropDown();

            viewDirty = true;
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
            FilteringPet();
            switch (currentPanelState)
            {
                case ePetPopupState.Compound:
                    GetListCustomSortCompound(checker);
                    break;
                case ePetPopupState.Decompose:
                    GetListCustomSortExceptLinked(checker);
                    SetDetailButtonState(false);
                    break;
                case ePetPopupState.LevelUp:
                    GetListCustomSortExceptLinked(checker, true);
                    SetDetailButtonState(false);
                    break;
                default:
                    GetListCustomSort(checker);
                    break;
            }

            dropdownController.SetDropdownIndex(eDropDownType.DEFAULT, checker);
            dropdownController.RefreshAllFilterLabel();
            dropdownController.InitDropDown();
            //SetListCustomSort();
            
            //SetCustomElementList();
            ForceUpdate();
        }

        public void ClearScrollViewCheck()
        {
            clickReleasAlleCallback?.Invoke();
            SetDetailButtonState(false);
            tableViewResetFlag = true;
            viewDirty = true;
            SetPopupState();
            DrawScrollView();
        }

        //드래곤에 장착된 펫은 뒤로, 아닌것은 앞으로
        void GetListCustomSort(int sortIndex)
        {
            if (viewPets == null)
            {
                viewPets = GetPetInfo().GetAllUserPets();
            }

            //소팅 구성 데이터 map 세팅 - 소팅 하기전 기본 map 형태 // init에서 맵 구성 완료.
            var sortFunc = Sort(sortIndex);
            if (sortFunc == null)
            {
                return;
            }

            //유저리스트, 빈 장비 리스트 분리 - 기본 빈 장비가 우선
            List<UserPet> dragonPartList = new List<UserPet>();
            List<UserPet> emptyPartList = new List<UserPet>();

            dragonPartList.Clear();
            emptyPartList.Clear();

            viewPets.ForEach((Element) => {
                if (Element == null)
                {
                    return;
                }

                var isbelonged = Element.LinkDragonTag > 0;//-1또는 0이면 귀속
                if (isbelonged)
                {
                    dragonPartList.Add(Element);
                }
                else
                {
                    emptyPartList.Add(Element);
                }
            });

            dragonPartList.Sort(sortFunc);
            emptyPartList.Sort(sortFunc);
            viewPets = emptyPartList.Concat(dragonPartList).ToList();

            //viewPets = userPets.sort(sortFunc);
            viewDirty = true;
        }

        void SetCustomElementList()
        {
            var currentElementSortIndex = dropdownController.GetDropdownIndex(eDropDownType.ELEMENT);
            if (currentElementSortIndex > 0)
            {
                viewPets = viewPets.FindAll(Element => Element.Element() == currentElementSortIndex);
            }
        }

        public void OnClickFilter()
        {
            var popup = PopupManager.OpenPopup<PetListFilterPopup>(filterData);
            popup.ApplyCallback = SetFilter;
        }
        public void SetFilter(PetFilterData data)//필터 체크 후 받아와야하는 값
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

        void FilteringPet()
        {
            if (filterData == null)
            {
                filterData = new PetFilterData();
                filterData.Init();
            }
            viewPets = GetPetInfo().GetAllUserPets();
            if (filterData.isShowOnlyLockState)
            {
                viewPets.RemoveAll(pet => User.Instance.Lock.IsLockPet(pet.Tag)==false);
            }
            if(filterData.gradeFilter != eGradeFilter.ALL)
            {
                viewPets.RemoveAll(pet =>
                {
                    var grade = SBFunc.GetGradeFilterType(pet.Grade());
                    return filterData.gradeFilter.HasFlag(grade) ==false;
                });
            }
            if (filterData.elementFilter != eElementFilter.ALL)
            {
                viewPets.RemoveAll(pet =>
                {
                    var elem = SBFunc.GetElemFilterType(pet.Element());
                    return filterData.elementFilter.HasFlag(elem) == false;
                });
            }
            if (filterData.statFilter != ePetStatFilter.ALL)
            {
                viewPets.RemoveAll(pet =>
                {
                    var stat = SBFunc.GetPetStatFilterType(pet);
                    return stat == ePetStatFilter.None || (filterData.statFilter & stat) == 0;
                });
            }
            if (filterData.optionFilter != ePetOptionFilter.ALL)
            {
                viewPets.RemoveAll(pet =>
                {
                    var option = SBFunc.GetPetOptionFilterType(pet);
                    return option == ePetOptionFilter.None || (filterData.optionFilter & option) == 0;
                });
            }
        }

        private Comparison<UserPet> Sort(int index)
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
                case 8: return Sort8;
                case 9: return Sort9;
            }

            return null;
        }

        private int Sort0(UserPet a, UserPet b)
        {
            var checker = SortGradeDescend(a, b);
            if (checker == 0)
            {
                checker = SortReinforceLevelDescend(a, b);
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
                                return SortPetTagDescend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort1(UserPet a, UserPet b)
        {
            var checker = SortGradeAscend(a, b);
            if (checker == 0)
            {
                checker = SortReinforceLevelAscend(a, b);
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
                                return SortPetTagAscend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort2(UserPet a, UserPet b)
        {
            var checker = SortLevelDescend(a, b);

            if (checker == 0)
            {
                checker = SortGradeDescend(a, b);
                if (checker == 0)
                {
                    checker = SortReinforceLevelDescend(a, b);
                    if (checker == 0)
                    {
                        checker = SortBattlePointDescend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeDescend(a, b);
                            if (checker == 0)
                                return SortPetTagDescend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort3(UserPet a, UserPet b)
        {
            var checker = SortLevelAscend(a, b);

            if (checker == 0)
            {
                checker = SortGradeAscend(a, b);
                if (checker == 0)
                {
                    checker = SortReinforceLevelAscend(a, b);
                    if (checker == 0)
                    {
                        checker = SortBattlePointAscend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeAscend(a, b);
                            if (checker == 0)
                                return SortPetTagAscend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort4(UserPet a, UserPet b)
        {
            var checker = SortBattlePointDescend(a, b);
            if (checker == 0)
            {
                checker = SortGradeDescend(a, b);
                if (checker == 0)
                {
                    checker = SortReinforceLevelDescend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelDescend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeDescend(a, b);
                            if (checker == 0)
                                return SortPetTagDescend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort5(UserPet a, UserPet b)
        {
            var checker = SortBattlePointAscend(a, b);

            if (checker == 0)
            {
                checker = SortGradeAscend(a, b);

                if (checker == 0)
                {
                    checker = SortReinforceLevelAscend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelAscend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeAscend(a, b);
                            if (checker == 0)
                                return SortPetTagAscend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort6(UserPet a, UserPet b)
        {
            var checker = SortObtainTimeDescend(a, b);
            if (checker == 0)
            {
                checker = SortGradeDescend(a, b);
                if (checker == 0)
                {
                    checker = SortReinforceLevelDescend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelDescend(a, b);
                        if (checker == 0)
                        {
                            checker = SortBattlePointDescend(a, b);
                            if (checker == 0)
                                return SortPetTagDescend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort7(UserPet a, UserPet b)
        {
            var checker = SortObtainTimeAscend(a, b);

            if (checker == 0)
            {
                checker = SortGradeAscend(a, b);
                if (checker == 0)
                {
                    checker = SortReinforceLevelAscend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelAscend(a, b);
                        if (checker == 0)
                        {
                            checker = SortBattlePointAscend(a, b);
                            if (checker == 0)
                                return SortPetTagAscend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        private int Sort8(UserPet a, UserPet b)
        {
            var checker = SortReinforceLevelDescend(a, b);
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
                            return SortPetTagDescend(a, b);
                    }
                }
            }

            return checker;
        }

        private int Sort9(UserPet a, UserPet b)
        {
            var checker = SortReinforceLevelAscend(a, b);

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
                            return SortPetTagAscend(a, b);
                    }
                }
            }

            return checker;
        }

        //등급 내림차순
        private int SortGradeDescend(UserPet param_a, UserPet param_b)
        {
            var aGrade = param_a.Grade();
            var bGrade = param_b.Grade();
            return bGrade - aGrade;
        }
        //등급 오름차순
        private int SortGradeAscend(UserPet param_a, UserPet param_b)
        {
            var aGrade = param_a.Grade();
            var bGrade = param_b.Grade();
            return aGrade - bGrade;
        }
        //레벨 내림차순
        private int SortLevelDescend(UserPet param_a, UserPet param_b)
        {
            var aLevel = param_a.Level;
            var bLevel = param_b.Level;
            return bLevel - aLevel;
        }
        //레벨 오름차순
        private int SortLevelAscend(UserPet param_a, UserPet param_b)
        {
            var aLevel = param_a.Level;
            var bLevel = param_b.Level;
            return aLevel - bLevel;
        }
        //강화 레벨 내림차순
        private int SortReinforceLevelDescend(UserPet param_a, UserPet param_b)
        {
            var aLevel = param_a.Reinforce;
            var bLevel = param_b.Reinforce;
            return bLevel - aLevel;
        }
        //강화 레벨 오름차순
        private int SortReinforceLevelAscend(UserPet param_a, UserPet param_b)
        {
            var aLevel = param_a.Reinforce;
            var bLevel = param_b.Reinforce;
            return aLevel - bLevel;
        }
        //전투력 내림차순
        private int SortBattlePointDescend(UserPet param_a, UserPet param_bt)
        {
            return 0;
        }
        //전투력 오름차순
        private int SortBattlePointAscend(UserPet param_a, UserPet param_b)
        {
            return 0;
        }
        //최신 획득 내림 차순
        private int SortObtainTimeDescend(UserPet param_a, UserPet param_b)
        {
            var aObtainTime = param_a.Obtain;
            var bObtaionTime = param_b.Obtain;
            return bObtaionTime - aObtainTime;
        }
        //최신 획득 오름 차순
        private int SortObtainTimeAscend(UserPet param_a, UserPet param_b)
        {
            var aObtainTime = param_a.Obtain;
            var bObtaionTime = param_b.Obtain;
            return aObtainTime - bObtaionTime;
        }

        //최신 획득 내림 차순
        private int SortPetTagDescend(UserPet param_a, UserPet param_b)
        {
            var aTag = param_a.Tag;
            var bTag = param_b.Tag;
            return bTag - aTag;
        }
        //최신 획득 오름 차순
        private int SortPetTagAscend(UserPet param_a, UserPet param_b)
        {
            var aTag = param_a.Tag;
            var bTag = param_b.Tag;
            return aTag - bTag;
        }

        #region 펫 레벨업
        public void PopMaterialList(int tag)
        {
            var index = petTagList.IndexOf(tag);
            if (index > -1)
            {
                petTagList.RemoveAt(index);
            }
            registCntText.text = string.Format("{0}/{1}", petTagList.Count, slotLimit);
        }
        void InitTagsList()
        {
            if (petTagList == null)
            {
                petTagList = new List<int>();
            }

            petTagList.Clear();
        }
        void PushMaterialList(int tag)
        {
            var index = petTagList.IndexOf(tag);
            if (index < 0)
            {
                petTagList.Add(tag);
            }
            registCntText.text = string.Format("{0}/{1}", petTagList.Count, slotLimit);
        }

        void PushAndClickMaterialProcess(string tag)
        {
            PushMaterialList(int.Parse(tag));
            if (clickRegistCallback != null)
            {
                clickRegistCallback(tag.ToString());
            }
        }
        bool IsFullMaterialList()
        {
            if (petTagList == null || petTagList.Count <= 0)
            {
                return false;
            }

            var count = 0;
            for (var i = 0; i < petTagList.Count; i++)
            {
                var tag = petTagList[i];
                if (tag > 0)
                {
                    count++;
                }
            }

            return count == slotLimit;
        }
        bool ToastMaxCountCheck()
        {
            var isFullCheck = IsFullMaterialList();//재료칸 전부 찼는 지
            if (isFullCheck)
            {
                ToastManager.On(100001132);
                return true;
            }
            return false;
        }
        public void OnClickAutoRegist()//일괄 등록 버튼
        {
            if(currentPanelState == ePetPopupState.LevelUp)
            {
                var toastCountCheck = ToastMaxCountCheck();
                if (toastCountCheck)
                {
                    return;
                }
            }
            //등록 사이즈 먼저 체크
            var availableCount = GetCountRemainMaterial();//등록 가능 수량
            if (viewPets == null || viewPets.Count <= 0)
            {
                return;
            }

            var checkCount = 0;
            List<int> remainAddList = new List<int>();
            remainAddList.Clear();
            List<int> constraintList = new List<int>();
            constraintList.Clear();


            var dataPetList = viewPets.ToList();
            dataPetList.Sort(Sort(1));//희귀도 오름차순 (합성 및 레벨업 시에는 낮은 등급 부터 들어감
            for (var i = 0; i < dataPetList.Count; i++)
            {
                if (checkCount == availableCount)
                {
                    break;
                }

                var petData = dataPetList[i];
                if (petData == null)
                {
                    continue;
                }

                var tag = petData.Tag;
                var index = petTagList.IndexOf(tag);
                if (index >= 0)//이미 등록되있음
                {
                    continue;
                }
                else
                {
                    var constraintCheck = IsAlertCondition(tag);
                    if (constraintCheck)
                    {
                        constraintList.Add(tag);
                    }

                    petTagList.Add(tag);//데이터 등록
                    remainAddList.Add(tag);
                    checkCount++;
                }
            }

            var hasConstraint = constraintList.Count > 0;
            if (hasConstraint)
            {
                ShowAlertPopup(PET_POPUP_CHECK_AUTO, "0",
                (param) => {
                    //확인
                    AutoRegistProcess(remainAddList);
                },
                () => {
                    //취소
                    for (var i = 0; i < constraintList.Count; i++)
                    {
                        var tag = constraintList[i];
                        if (tag <= 0)
                        {
                            continue;
                        }

                        var remainIndex = remainAddList.IndexOf(tag);
                        if (remainIndex > -1)
                        {
                            remainAddList.RemoveAt(remainIndex);
                        }

                        var levelupIndex = petTagList.IndexOf(tag);
                        if (levelupIndex > -1)
                        {
                            petTagList.RemoveAt(levelupIndex);
                        }
                    }

                    AutoRegistProcess(remainAddList);
                });
            }
            else
            {
                AutoRegistProcess(remainAddList);
            }

            registCntText.text = string.Format("{0}/{1}", petTagList.Count, slotLimit);
        }

        void AutoRegistProcess(List<int> remainAddList)
        {
            if (clickAutoRegistCallback != null)
            {
                clickAutoRegistCallback(remainAddList);
            }

            viewDirty = true;
            SetTableViewFlag(false);
            ForceUpdate();//다시 그리기 요청
            SetTableViewFlag(true);

            SetDetailButtonState(true);
        }
        public void SetTableViewFlag(bool flag)
        {
            tableViewResetFlag = flag;
        }
        int GetCountRemainMaterial()// 등록된 재료 기준으로 남은 재료 등록 수량
        {
            if (petTagList == null || petTagList.Count <= 0)
            {
                return slotLimit;
            }

            var currentRegist = 0;

            for (var i = 0; i < petTagList.Count; i++)
            {
                var tag = petTagList[i];
                if (tag > 0)
                {
                    currentRegist++;
                }
            }

            return slotLimit - currentRegist;
        }
        /// <summary>
        /// 기획 변경 - 현재 조건 (자신 보다 높은 등급 & 30레벨 이상)
        /// 변경 조건 - 등급 상관 없이 강화 단계 및 레벨 1이상
        /// </summary>
        /// <param name="clickPetTag"></param>
        /// <returns></returns>
        bool IsAlertCondition(int clickPetTag)
        {
            //기준 펫 데이터
            if (petTag <= 0)
            {
                return false;
            }

            var petData = GetPetInfo().GetPet(petTag);
            if (petData == null)
            {
                return false;
            }

            if (clickPetTag <= 0)
            {
                return false;
            }

            var clickPetData = GetPetInfo().GetPet(clickPetTag);
            if (clickPetData == null)
            {
                return false;
            }

            var petGrade = petData.Grade();
            var clickPetGrade = clickPetData.Grade();

            var clickPetLevel = clickPetData.Level;
            var clickReinforce = clickPetData.Reinforce;

            var isHighLevel = clickPetLevel > 1;
            var isHighReinforceLevel = clickReinforce > 0;
            var isHighGrade = petGrade < clickPetGrade;

            return (isHighReinforceLevel || isHighLevel || isHighGrade);
        }

        void ShowAlertPopup(string checkFlag, string petTag, funcStr ok_cb, func cancel_cb)//재료 펫레벨 30이상 이거나 선택한 펫이 레벨업 대상 펫보다 등급 높을 때
        {
            var valueCheck = SBFunc.HasTimeValue(checkFlag);
            if (valueCheck)
            {
                if (ok_cb != null)
                {
                    ok_cb(petTag);
                }
                return;//하루동안 보이지 않기 on
            }

            var popup = PopupManager.OpenPopup<PetLevelUpConstraintPopup>();
            popup.setMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("펫레벨업경고문구"));
            popup.setCallback(() => {
                //예 - 토글 상태 (하루동안 보이지않기) 체크는 예 일때만 판단 (기획)
                //쿠키 세팅
                var checkValue = popup.toggle.isOn;
                if (checkValue)
                {
                    SBFunc.SetTimeValue(checkFlag);
                }

                if (ok_cb != null)
                {
                    ok_cb(petTag);
                }
            },
            () => {
                //취소
                if (cancel_cb != null)
                {
                    cancel_cb();
                }
                popup.ClosePopup();
            },
            () => {
                //x
                if (cancel_cb != null)
                {
                    cancel_cb();
                }
                popup.ClosePopup();
            });
        }

        #endregion

        #region 펫 합성
        void SetScrollRefreshByCompoundListLength(bool isSelected)
        {
            var currentMaterialCount = petTagList.Count;
            if (isSelected)//해제 시
            {
                switch (currentMaterialCount)
                {
                    case 0:
                        SetTableViewFlag(true);
                        break;
                    case 1:
                        SetTableViewFlag(false);
                        break;
                }
            }
            else
            {//선택 시
                switch (currentMaterialCount)
                {
                    case 1:
                        SetTableViewFlag(true);
                        break;
                    case 2:
                        SetTableViewFlag(false);
                        break;
                }
            }
        }
        void GetListCustomSortCompound(int sortIndex)
        {
            if (viewPets == null)
            {
                viewPets = GetPetInfo().GetAllUserPets();
            }

            //소팅 구성 데이터 map 세팅 - 소팅 하기전 기본 map 형태 // init에서 맵 구성 완료.
            var sortFunc = Sort(sortIndex);
            if (sortFunc == null)
            {
                return;
            }

            //현재 재료 리스트에 하나라도 있으면 해당 동일 등급만 필터링
            var tempGrade = GetGradeByMaterialList();

            //유저리스트, 빈 장비 리스트 분리 - 기본 빈 장비가 우선
            List<UserPet> petList = new List<UserPet>();
            petList.Clear();
            viewPets.ForEach((Element) => {
                if (Element == null)
                {
                    return;
                }

                if (Element.Level < GameConfigTable.GetPetLevelMax(Element.Grade()))
                {//만렙 아니면 리턴
                    return;
                }

                if (Element.Reinforce < GameConfigTable.GetPetReinforceLevelMax(Element.Grade()))
                {//만강 아니면 리턴
                    return;
                }

                if (tempGrade > 0 && Element.Grade() != tempGrade)
                {//재료가 하나라도 있으면 동일 등급만
                    return;
                }

                var isbelonged = (Element.LinkDragonTag > 0) || User.Instance.Lock.IsLockPet(Element.Tag); //귀속 드래곤은 제외
                if (!isbelonged)
                {
                    petList.Add(Element);
                }
            });

            petList.Sort(sortFunc);
            viewPets = petList.ToList();
            viewDirty = true;
        }
        int GetGradeByMaterialList()
        {
            var grade = 0;
            if (petTagList == null || petTagList.Count <= 0)
            {
                return grade;
            }

            for (var i = 0; i < petTagList.Count; i++)
            {
                var tag = petTagList[i];
                var petData = GetPetInfo().GetPet(tag);
                if (tag > 0 && petData != null)
                {
                    grade = petData.Grade();
                    return grade;
                }
            }
            return grade;
        }
        #endregion

        #region 펫 분해
        void GetListCustomSortExceptLinked(int sortIndex, bool isExceptCurPetTag = false)
        {
            if (viewPets == null)
            {
                viewPets = GetPetInfo().GetAllUserPets();
            }
            if (isExceptCurPetTag &&petTag>0)
            {
                viewPets.Remove(GetPetInfo().GetPet(petTag));
            }
            //소팅 구성 데이터 map 세팅 - 소팅 하기전 기본 map 형태 // init에서 맵 구성 완료.
            var sortFunc = Sort(sortIndex);
            if (sortFunc == null)
            {
                return;
            }

            List<UserPet> petList = new List<UserPet>();
            petList.Clear();
            viewPets.ForEach((Element) => {
                if (Element == null)
                {
                    return;
                }

                var isbelonged = (Element.LinkDragonTag > 0) || User.Instance.Lock.IsLockPet(Element.Tag);//귀속 드래곤은 제외
                if (!isbelonged)
                {
                    petList.Add(Element);
                }
            });

            petList.Sort(sortFunc);
            viewPets = petList.ToList();
            viewDirty = true;
        }
        #endregion

    }
}
