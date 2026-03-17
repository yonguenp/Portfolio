using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetListLayer : SubLayer
    {
        [SerializeField]
        private PetTabLayer petTap = null;

        [Header("Scroll")]
        [SerializeField]
        protected TableViewGrid tableViewGrid = null;

        [Space(10)]
        [Header("DropDown")]
        [SerializeField]
        private GameObject sortDropdown = null;
        [SerializeField]
        private Text sortButtonLabel = null;
        [SerializeField]
        private Button[] elementButtonList = null;
        
        [SerializeField]
        private GameObject[] buttonNodeList = null;
        [SerializeField]
        private Text invenCheckLabel = null;

        private List<UserPet> userPets = null;
        private List<UserPet> viewPets = null;
        private bool viewDirty = true;

        private int currentCustomSortIndex = 0;
        private int currentElementSortIndex = 0;

        private bool isTableInit = false;

        public override void Init()
        {
            if(tableViewGrid != null && !isTableInit)
            {
                tableViewGrid.OnStart();
                isTableInit = true;
            }

            userPets = User.Instance.PetData.GetAllUserPets();

            InitPetInfoData();
            InitDropDown();
            InitCustomSort();
        }
        void InitPetInfoData()
        {
            PopupManager.GetPopup<DragonManagePopup>().CurPetTag = 0;
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

        void setCurrentClickSortIndex(int sortIndex)
        {
            currentCustomSortIndex = sortIndex;
        }

        public override void ForceUpdate()
        {
            DrawScrollView();
        }

        public void onClickChangeSort()
        {
            sortDropdown.gameObject.SetActive(!sortDropdown.activeInHierarchy);
        }

        public void DrawScrollView()
        {
            if (!viewDirty || tableViewGrid == null || viewPets == null)
            {
                return;
            }

            var isEmpty = viewPets.Count <= 0;
            invenCheckLabel.gameObject.SetActive(isEmpty);
            if (isEmpty)
            {
                invenCheckLabel.text = StringData.GetStringByIndex(-1);//기본 인벤에 장비가 없을 때 처리
            }

            if(tableViewGrid != null)
            {
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

                    var dragonData = (UserPet)item;

                    frame.SetPetPortraitFrame(dragonData);
                    frame.SetCallback(OnClickFrame);
                }));

                tableViewGrid.ReLoad();
                viewDirty = false;
            }
        }

        void OnClickFrame(string customEventData)
        {
            if (int.TryParse(customEventData, out int customValue) == false)
            {
                return;
            }

            PopupManager.GetPopup<DragonManagePopup>().CurPetTag = customValue;
            petTap.moveLayer(1);
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
            setCurrentClickSortIndex(currentCustomSortIndex);//현재 클릭한 정렬인덱스 글로벌 저장
            GetListCustomSort(currentCustomSortIndex);//클릭인덱스 기준 정렬 완료 데이터 받아오기
            RefreshElementButtonInteration();
            SetCustomElementList();
            ForceUpdate();
            InitDropDown();//일단 임시로 끄기
        }

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

                var isbelonged = (Element.LinkDragonTag > 0);//-1또는 0이면 귀속
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
            if (currentElementSortIndex != 0 && currentElementSortIndex >= 0)
            {
                viewPets = viewPets.FindAll(Element => Element.Element() == currentElementSortIndex);
            }
        }

        private void RefreshButtonLabelForce(int index)
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
        private int SortGradeDescend(UserPet a, UserPet b)
        {
            var aGrade = a.Grade();
            var bGrade = b.Grade();
            return bGrade - aGrade;
        }
        //등급 오름차순
        private int SortGradeAscend(UserPet a, UserPet b)
        {
            var aGrade = a.Grade();
            var bGrade = b.Grade();
            return aGrade - bGrade;
        }
        //레벨 내림차순
        private int SortLevelDescend(UserPet a, UserPet b)
        {
            var aLevel = a.Level;
            var bLevel = b.Level;
            return bLevel - aLevel;
        }
        //레벨 오름차순
        private int SortLevelAscend(UserPet a, UserPet b)
        {
            var aLevel = a.Level;
            var bLevel = b.Level;
            return aLevel - bLevel;
        }
        //강화 레벨 내림차순
        private int SortReinforceLevelDescend(UserPet a, UserPet b)
        {
            var aLevel = a.Reinforce;
            var bLevel = b.Reinforce;

            return bLevel - aLevel;
        }
        //강화 레벨 오름차순
        private int SortReinforceLevelAscend(UserPet a, UserPet b)
        {
            var aLevel = a.Reinforce;
            var bLevel = b.Reinforce;

            return aLevel - bLevel;
        }
        //전투력 내림차순
        private int SortBattlePointDescend(UserPet a, UserPet b)
        {
            return 0;
        }
        //전투력 오름차순
        private int SortBattlePointAscend(UserPet a, UserPet b)
        {
            return 0;
        }
        //최신 획득 내림 차순
        private int SortObtainTimeDescend(UserPet a, UserPet b)
        {
            var aObtainTime = a.Obtain;
            var bObtaionTime = b.Obtain;
            return bObtaionTime - aObtainTime;
        }
        //최신 획득 오름 차순
        private int SortObtainTimeAscend(UserPet a, UserPet b)
        {
            var aObtainTime = a.Obtain;
            var bObtaionTime = b.Obtain;
            return aObtainTime - bObtaionTime;
        }
        /**
         * //업데이트 기대해달라는 문구
*/
        public void onClickExpectGameAlphaUpdate()
        {
            ToastManager.On(100000326);
        }

        public void onClickPetReinforceLevelupButton()
        {
            PopupManager.GetPopup<DragonManagePopup>().CurPetTag = 0;

            petTap.onClickChangeLayer("3");
        }

        public void onClickPetCompoundButton()
        {
            PopupManager.GetPopup<DragonManagePopup>().CurPetTag = 0;

            petTap.onClickChangeLayer("4");
        }
    }
}
