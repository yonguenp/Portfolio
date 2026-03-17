using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetLevelupListPanel : MonoBehaviour
    {
        const string PET_POPUP_CHECK_MANUAL = "PET_POPUP_CHECK_MANUAL";
        const string PET_POPUP_CHECK_AUTO = "PET_POPUP_CHECK_AUTO";
        const int CONSTRAINT_PET_LEVEL = 30;
        const int PET_CONSUME_MAX_SLOT_NUM = 20;//펫 레벨 업 시 소모되는 슬롯 최대 개수

        [SerializeField]
        TableViewGrid tableViewGrid = null;

        [Space(10)]
        [Header("DropDown")]
        [SerializeField]
        GameObject sortDropdown = null;
        [SerializeField]
        Text sortButtonLabel = null;
        [SerializeField]
        Button[] elementButtonList = null;
        [SerializeField]
        string sortinitLabelStrIndex = "등급 ▼";
        [SerializeField]
        GameObject[] buttonNodeList = null;

        List<UserPet> userPets = null;
        List<UserPet> viewPets = null;
        bool viewDirty = true;
        public bool ViewDirty { set { viewDirty = value; } }

        int currentCustomSortIndex = 0;
        int currentElementSortIndex = 0;

        private bool tableViewResetFlag = true;

        public bool TableViewResetFlag
        {
            get { return tableViewResetFlag; }
            set { tableViewResetFlag = value; }
        }

        int petTag = -1;
        public int PetTag
        {
            get { return petTag; }
        }

        List<int> levelUpPetTagList = new List<int>();//레벨업 요청할 재료 팻 태그 리스트
        
        private bool isTableInit = false;

        public delegate void func();
        public delegate void funcStr(string CustomEventData);
        public delegate void funcList(List<int> CustomEventData);

        private funcStr clickRegistCallback = null;
        public funcStr ClickRegistCallback { set { if (value != null) { clickRegistCallback = value; } } }

        private funcStr clickReleaseCallback = null;
        public funcStr ClickReleaseCallback { set { if (value != null) { clickReleaseCallback = value; } } }

        private funcList clickAutoRegistCallback = null;
        public funcList ClickAutoRegistCallback { set { if (value != null) { clickAutoRegistCallback = value; } } }


        void OnEnable()
        {
            currentElementSortIndex = -1;
        }

        public void Init()
        {
            userPets = User.Instance.PetData.GetAllUserPets();
            InitTableView();
            InitPetInfoData();//레이어 교체 이전 선택된 펫 태그 가져오기
            InitDropDown();
            InitCustomSort();
            InitLevelUpList();

            ForceUpdate();
        }

        void InitTableView()
        {
            if(tableViewGrid != null && !isTableInit)
            {
                tableViewGrid.OnStart();
                isTableInit = true;
            }
        }

        void InitLevelUpList()
        {
            if(levelUpPetTagList == null)
            {
                levelUpPetTagList = new List<int>();
            }

            levelUpPetTagList.Clear();
        }

        void InitPetInfoData()
        {
            if (PopupManager.GetPopup<DragonManagePopup>().CurPetTag != 0)
            {
                petTag = PopupManager.GetPopup<DragonManagePopup>().CurPetTag;
            }
        }

        void InitDropDown()
        {
            if (sortDropdown.activeInHierarchy)
            {
                sortDropdown.SetActive(false);
            }
        }

        void InitCustomSort()
        {

            onClickCustomSort(currentCustomSortIndex.ToString());
            RefreshButtonLabelForce(currentCustomSortIndex);

            ForceUpdate();
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

                        var constraintCheck = isAlertCondition(petData.Tag);
                        if (constraintCheck)
                        {
                            ShowAlertPopup(PET_POPUP_CHECK_MANUAL, petData.Tag.ToString(), pushAndClickMaterialProcess, () => {
                                frame.SetVisibleSelectedNode(false);//취소시 선택된 프레임 끄기            
                            });
                        }
                        else
                        {
                            pushAndClickMaterialProcess(petData.Tag.ToString());
                        }
                    }
                    frame.SetVisibleSelectedNode(!isSelected);//버튼 UI 켜기
                });
                var index = levelUpPetTagList.IndexOf(petData.Tag);
                if (index > -1)
                {
                    frame.SetVisibleSelectedNode(true);//파츠가 이미 등록된 상태에서 다시 그릴경우, 현재 켜져있으면 켜기
                }

            }));


            tableViewGrid.ReLoad(tableViewResetFlag);
            viewDirty = false;
        }

        public void SetTableViewFlag(bool flag)
        {
            tableViewResetFlag = flag;
        }

        public void popMaterialList(int tag)
        {
            var index = levelUpPetTagList.IndexOf(tag);
            if (index > -1)
            {
                levelUpPetTagList.RemoveAt(index);
            }
        }

        void pushMaterialList(int tag)
        {
            var index = levelUpPetTagList.IndexOf(tag);
            if (index < 0)
            {
                levelUpPetTagList.Add(tag);
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
            if(levelUpPetTagList == null || levelUpPetTagList.Count <= 0)
            {
                return false;
            }

            var count = 0;
            for(var i = 0 ; i < levelUpPetTagList.Count; i++)
            {
                var tag = levelUpPetTagList[i];
                if (tag > 0)
                {
                    count++;
                }
            }

            return count == PET_CONSUME_MAX_SLOT_NUM;
        }

        bool toastMaxCountCheck()
        {
            var isFullCheck = isFullMaterialList();//재료칸 전부 찼는 지
            if(isFullCheck)
            {
                ToastManager.On(100001132);
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

            if (currentElementSortIndex == checker)
            {
                return;
            }
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

            //유저리스트, 빈 장비 리스트 분리 - 기본 빈 장비가 우선
            List<UserPet> dragonPartList = new List<UserPet>();
            List<UserPet> emptyPartList = new List<UserPet>();

            userPets.ForEach((Element) => {
                if (Element == null)
                {
                    return;
                }

                if (Element.Tag == petTag)//선택된 펫은 제외
                {
                    return;
                }

                var isbelonged = (Element.LinkDragonTag > 0);//귀속 드래곤은 제외
                if (!isbelonged)
                {
                    emptyPartList.Add(Element);
                }
            });

            emptyPartList.Sort(sortFunc);
            viewPets = emptyPartList.ToList();
            viewDirty = true;
        }

        void SetCustomElementList()
        {
            if (currentElementSortIndex != 0 && currentElementSortIndex >= 0)
            {
                viewPets = viewPets.FindAll(Element => Element.Element() == currentElementSortIndex);
            }
        }

        protected void RefreshButtonLabelForce(int index)
        {
            if (buttonNodeList == null || buttonNodeList.Length <= 0)
            {
                if (sortButtonLabel != null)
                {
                    sortButtonLabel.text = sortinitLabelStrIndex;
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

        public void onClickAutoRegist()//일괄 등록 버튼
        {
            var toastCountCheck = toastMaxCountCheck();
            if (toastCountCheck)
            {
                return;
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

            for (var i = 0; i < viewPets.Count; i++)
            {
                if (checkCount == availableCount)
                {
                    break;
                }

                var petData = viewPets[i];
                if (petData == null)
                {
                    continue;
                }

                var tag = petData.Tag;
                var index = levelUpPetTagList.IndexOf(tag);
                if (index >= 0)//이미 등록되있음
                {
                    continue;
                }
                else
                {
                    var constraintCheck = isAlertCondition(tag);
                    if (constraintCheck)
                    {
                        constraintList.Add(tag);
                    }

                    levelUpPetTagList.Add(tag);//데이터 등록
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

                        var levelupIndex = levelUpPetTagList.IndexOf(tag);
                        if (levelupIndex > -1)
                        {
                            levelUpPetTagList.RemoveAt(levelupIndex);
                        }
                    }

                    AutoRegistProcess(remainAddList);
                });
            }
            else
            {
                AutoRegistProcess(remainAddList);
            }
        }

        void AutoRegistProcess(List<int> remainAddList)
        {
            if (clickAutoRegistCallback != null)
            {
                clickAutoRegistCallback(remainAddList);
            }

            viewDirty = true;
            SetTableViewFlag(false);
            DrawScrollView();//다시 그리기 요청
            SetTableViewFlag(true);
        }

        int GetCountRemainMaterial()// 등록된 재료 기준으로 남은 재료 등록 수량
        {
            if (levelUpPetTagList == null || levelUpPetTagList.Count <= 0)
            {
                return PET_CONSUME_MAX_SLOT_NUM;
            }

            var currentRegist = 0;

            for (var i = 0; i < levelUpPetTagList.Count; i++)
            {
                var tag = levelUpPetTagList[i];
                if (tag > 0)
                {
                    currentRegist++;
                }
            }

            return PET_CONSUME_MAX_SLOT_NUM - currentRegist;
        }
        /// <summary>
        /// 기획 변경 - 현재 조건 (자신 보다 높은 등급 & 30레벨 이상)
        /// 변경 조건 - 등급 상관 없이 강화 단계 및 레벨 1이상
        /// </summary>
        /// <param name="clickPetTag"></param>
        /// <returns></returns>
        bool isAlertCondition(int clickPetTag)
        {
            //기준 펫 데이터
            if (petTag <= 0)
            {
                return false;
            }

            var petData = User.Instance.PetData.GetPet(petTag);
            if (petData == null)
            {
                return false;
            }

            if (clickPetTag <= 0)
            {
                return false;
            }

            var clickPetData = User.Instance.PetData.GetPet(clickPetTag);
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

        void ShowAlertPopup(string checkFlag,string petTag, funcStr ok_cb, func cancel_cb)//재료 펫레벨 30이상 이거나 선택한 펫이 레벨업 대상 펫보다 등급 높을 때
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
            },
            () => {
                    //x
                if (cancel_cb != null)
                {
                    cancel_cb();
                }
            });
        }
    }
}
