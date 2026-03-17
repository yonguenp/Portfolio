using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetCompoundListPanel : MonoBehaviour
    {
        const int PET_COMPOUND_MAX_SLOT_NUM = 2;//펫 합성 최대 재료 갯수

        [SerializeField]
        TableViewGrid tableViewGrid = null;

        [Space(10)]
        [Header("DropDown")]
        [SerializeField]
        GameObject sortDropdown = null;
        [SerializeField]
        Text sortButtonLabel = null;
        [SerializeField]
        protected Button[] elementButtonList = null;
        
        [SerializeField]
        GameObject[] buttonNodeList = null;
        [SerializeField]
        Text invenCheckLabel = null;

        [Space(10)]
        [Header("InfoIcon")]

        List<UserPet> userPets = null;
        List<UserPet> viewPets = null;
        bool viewDirty = true;

        public bool ViewDirty { set { viewDirty = value; } }

        int currentCustomSortIndex = 0;
        int currentElementSortIndex = -1;

        bool tableViewResetFlag = true;

        int petTag = -1;

        List<int> compoundPetTagList = null;//합성 요청할 재료 팻 태그 리스트

        public delegate void func();
        public delegate void funcStr(string CustomEventData);
        public delegate void funcList(List<int> CustomEventData);

        private funcStr clickRegistCallback = null;
        public funcStr ClickRegistCallback { set { if (value != null) { clickRegistCallback = value; } } }

        private funcStr clickReleaseCallback = null;
        public funcStr ClickReleaseCallback { set { if (value != null) { clickReleaseCallback = value; } } }

        private funcList clickAutoRegistCallback = null;
        public funcList ClickAutoRegistCallback { set { if (value != null) { clickAutoRegistCallback = value; } } }


        bool isTableInit = false;

        void OnEnable()
        {
            currentElementSortIndex = -1;
        }

        public void Init()
        {
            userPets = User.Instance.PetData.GetAllUserPets();
            InitTableView();
            InitCompoundList();
            InitDropDown();
            InitPetInfoData();//레이어 교체 이전 선택된 펫 태그 가져오기
            InitCustomSort();
        }

        void InitTableView()
        {
            if(tableViewGrid != null && !isTableInit)
            {
                tableViewGrid.OnStart();
                isTableInit = true;
            }
        }

        void InitCompoundList()
        {
            if (compoundPetTagList == null)
            {
                compoundPetTagList = new List<int>();
            }
            compoundPetTagList.Clear();
        }

        void InitPetInfoData()
        {
            if (PopupManager.GetPopup<DragonManagePopup>().CurPetTag != 0)
            {
                petTag = PopupManager.GetPopup<DragonManagePopup>().CurPetTag;

                //외부에서 클릭해서 들어온 petTag가 존재할 경우 미리 넣어버림
                if (petTag > 0)
                {
                    pushMaterialList(petTag);
                }
            }
        }

        void InitDropDown()
        {
            if (sortDropdown.activeInHierarchy)
            {
                sortDropdown.SetActive(false);
            }
        }

        public void InitCustomSort()
        {
            onClickCustomSort(currentCustomSortIndex.ToString());
            RefreshButtonLabelForce(currentCustomSortIndex);
        }

        void SetCurrentClickSortIndex(int sortIndex)
        {
            currentCustomSortIndex = sortIndex;
        }

        public void ForceUpdate()
        {
            DrawScrollView();
        }

        public void onClickChangeSort()
        {
            sortDropdown.SetActive(!sortDropdown.activeInHierarchy);
        }

        void DrawScrollView()
        {
            if (!viewDirty || tableViewGrid == null || viewPets == null)
            {
                return;
            }

            var isEmpty = viewPets.Count <= 0;

            if (invenCheckLabel != null)
            {
                invenCheckLabel.gameObject.SetActive(isEmpty);

                if (isEmpty)
                {
                    invenCheckLabel.text = StringData.GetStringByIndex(-1);//기본 인벤에 장비가 없을 때 처리
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
                frame.SetVisibleClickNode(false);
                frame.SetCallback((param) => {
                    var isSelected = frame.IsSelected;//이미 선택이 되었는지
                    if (isSelected)
                    {
                        popMaterialList(petData.Tag);

                        if (clickReleaseCallback != null)
                        {
                            clickReleaseCallback(param);
                        }
                    }
                    else
                    {
                        var toastCheck = toastMaxCountCheck();
                        if (toastCheck)
                        {
                            return;
                        }

                        pushAndClickMaterialProcess(petData.Tag.ToString());
                    }
                    frame.SetVisibleSelectedNode(!isSelected);//버튼 UI 켜기

                    //materialList 기준으로 다시 그리기
                    SetScrollRefreshByCompoundListLength(isSelected);
                    InitCustomSort();
                    SetTableViewFlag(true);
                });

                var index = compoundPetTagList.IndexOf(petData.Tag);
                if (index > -1)
                {
                    frame.SetVisibleSelectedNode(true);//파츠가 이미 등록된 상태에서 다시 그릴경우, 현재 켜져있으면 켜기
                }
            }));

            tableViewGrid.ReLoad(tableViewResetFlag);
            viewDirty = false;
        }

        void SetScrollRefreshByCompoundListLength(bool isSelected)
        {
            var currentMaterialCount = compoundPetTagList.Count;
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

        public void SetTableViewFlag(bool flag)
        {
            tableViewResetFlag = flag;
        }

        public void popMaterialList(int tag)
        {
            var index = compoundPetTagList.IndexOf(tag);
            if (index > -1)
            {
                compoundPetTagList.RemoveAt(index);
            }
        }

        void pushMaterialList(int tag)
        {
            var index = compoundPetTagList.IndexOf(tag);
            if (index < 0)
            {
                compoundPetTagList.Add(tag);
            }
        }

        void pushAndClickMaterialProcess(string tag)
        {
            pushMaterialList(int.Parse(tag));
            if (clickRegistCallback != null)
            {
                clickRegistCallback(tag.ToString());
            }
        }

        bool isFullMaterialList()
        {
            if(compoundPetTagList == null || compoundPetTagList.Count <= 0)
            {
                return false;
            }

            var count = 0;
            for(var i = 0 ; i<compoundPetTagList.Count; i++)
            {
                var tag = compoundPetTagList[i];
                if (tag > 0)
                {
                    count++;
                }
            }

            return count == PET_COMPOUND_MAX_SLOT_NUM;
        }

        int GetGradeByMaterialList()
        {
            var grade = 0;
            if (compoundPetTagList == null || compoundPetTagList.Count <= 0)
            {
                return grade;
            }

            for (var i = 0; i < compoundPetTagList.Count; i++)
            {
                var tag = compoundPetTagList[i];
                var petData = User.Instance.PetData.GetPet(tag);
                if (tag > 0 && petData != null)
                {
                    grade = petData.Grade();
                    return grade;
                }
            }
            return grade;
        }

        bool toastMaxCountCheck()
        {
            var isFullCheck = isFullMaterialList();//재료칸 전부 찼는 지
            if (isFullCheck)
            {
                ToastManager.On(100001132);
                Debug.Log(StringData.GetStringByIndex(100001132));
                return true;
            }
            return false;
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
        public void onClickElementSort(string customEventData)
        {
            var checker = int.Parse(customEventData);

            currentElementSortIndex = checker;
            GetListCustomSort(currentCustomSortIndex);

            if (checker != 0)
            {
                viewPets = viewPets.FindAll(Element => Element.Element() == checker);
            }
            RefreshElementButtonInteration();

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
        public void onClickCustomSort(string customEventData)
        {
            var checker = int.Parse(customEventData);

            currentCustomSortIndex = checker;
            RefreshButtonLabelForce(currentCustomSortIndex);
            SetCurrentClickSortIndex(currentCustomSortIndex);//현재 클릭한 정렬인덱스 글로벌 저장
            GetListCustomSort(currentCustomSortIndex);//클릭인덱스 기준 정렬 완료 데이터 받아오기
            RefreshElementButtonInteration();
            SetCustomElementList();
            ForceUpdate();
            InitDropDown();//일단 임시로 끄기
        }

        //드래곤에 장착된 펫은 뒤로, 아닌것은 앞으로
        void GetListCustomSort(int sortIndex)
        {
            if (userPets == null)
            {
                userPets = User.Instance.PetData.GetAllUserPets();
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
            userPets.ForEach((Element) => {
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

                var isbelonged = (Element.LinkDragonTag > 0);//귀속 드래곤은 제외
                if (!isbelonged)
                {
                    petList.Add(Element);
                }
            });

            petList.Sort(sortFunc);
            viewPets = petList.ToList();
            viewDirty = true;
        }

        void SetCustomElementList()
        {
            if (currentElementSortIndex != 0 && currentElementSortIndex >= 0)
            {
                viewPets = viewPets.FindAll(Element => Element.Element() == currentElementSortIndex);
            }
        }

        void RefreshButtonLabelForce(int index)
        {
            if (buttonNodeList == null || buttonNodeList.Length <= 0)
            {
                if (sortButtonLabel != null)
                {
                    sortButtonLabel.text = StringData.GetStringByStrKey("희귀도내림차");
                }
            }

            var length = buttonNodeList.Length;
            if (length > index)
            {
                var childbuttonLabel = buttonNodeList[index].GetComponentInChildren<Text>();
                if (sortButtonLabel != null)
                {
                    sortButtonLabel.text = childbuttonLabel.text;
                }
            }
        }

        void RefreshElementButtonInteration()//비활성 시킬 인덱스
        {
            if (elementButtonList == null || elementButtonList.Length <= 0)
            {
                return;
            }

            if (currentElementSortIndex < 0)
            {
                currentElementSortIndex = 0;
            }

            for (var i = 0; i < elementButtonList.Length; i++)
            {
                var button = elementButtonList[i];
                if (button == null)
                {
                    continue;
                }

                button.SetInteractable(!(currentElementSortIndex == i));
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
                            return SortObtainTimeDescend(a, b);
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
                            return SortObtainTimeAscend(a, b);
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
                            return SortObtainTimeDescend(a, b);
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
                            return SortObtainTimeAscend(a, b);
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
                            return SortObtainTimeDescend(a, b);
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
                            return SortObtainTimeAscend(a, b);
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
                            return SortBattlePointDescend(a, b);
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
                            return SortBattlePointAscend(a, b);
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
                        return SortObtainTimeDescend(a, b);
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
                        return SortObtainTimeAscend(a, b);
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

        public void onClickInfoIcon()
        {
            //var popup = PopupManager.OpenPopup("tooltip", true) as ItemTooltip;
            //var btnWorldpos = infoButton.node.worldPosition;

            //var parent = PopupManager.GetInstance.CurBeacon;
            //var beaconScale = parent.node.scale;

            //btnWorldpos = new Vec3(btnWorldpos.x - 255 * beaconScale.x, btnWorldpos.y + 15 * beaconScale.y);
            //popup.setMessage(StringTable.GetString(-1, "펫 합성 조건"), StringTable.GetString(-1, "MAX레벨 (LV. 60)\nMAX강화 레벨 (LV. 10)"));
            //popup.setTooltipPosition(btnWorldpos);
            //popup.setTooltipReverse();
        }
    }
}
