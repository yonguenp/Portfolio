using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DEBUG

namespace SandboxNetwork
{
    public class SimulatorDragonEditPopup : Popup<SimulatorDragonPopupData>
    {
        [SerializeField] private SimulatorDragonEditPartController partController = null;
        [SerializeField] private SimulatorDragonEditStatController statController = null;
        [SerializeField] private SimulatorDragonEditSkillController skillController = null;
        [SerializeField] private SimulatorDragonEditPetController petController = null;

        int dragonTag = 0;

        public void onClickClosePopup()
        {
            revertSettingProcess();
            ClosePopup();
        }

        public override void ForceUpdate(SimulatorDragonPopupData data)//변경점 있으면 업데이트
        {
            base.DataRefresh(data);
            RefreshDragonStatPanel();
        }

        void RefreshDragonStatPanel()
        {
            if (statController != null && partController != null && petController != null)
            {
                //userPart, userPet, skill_level 세팅
                var partList = partController.GetUserPartList();
                var petData = petController.PetData;
                var skillLevel = skillController.TempSLevel;

                statController.SetPart(partList);
                statController.SetPet(petData);
                statController.SetSLevel(skillLevel);
                statController.RefreshDragonStatPanel();
            }
        }

        public override void InitUI()
        {

            dragonTag = int.Parse(Data.SimulatorDragonTag);

            var index = Data.SimulatorDragonIndex;
            if (partController != null)
            {
                partController.init(dragonTag, index);
            }

            if (skillController != null)
            {
                skillController.init(dragonTag);//드래곤 스킬 레벨 갱신
            }

            if (petController != null)//펫태그 어떻게 생성하지??? - 일단 임시로 드래곤 태그 값 넣음
            {
                petController.init(dragonTag);
            }

            if (statController != null)
            {
                statController.init(dragonTag);
                RefreshDragonStatPanel();//드래곤 스탯 갱신
            }
        }


        public void onClickApplyPopup()//현재 튜닝된 상태를 저장할지 물어보기
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100002666), "현재 편집한 내용을 저장하고 돌아갈까요?",
                () => {
                    //ok
                    SaveDataProcess();
                    ToastManager.On("저장완료!");
                    ClosePopup();

                    //드래곤 리로드 이벤트
                    SBSimulatorEvent.refreshDragonData();

                }, 
                () => {
                    //cancel
                },
                () => {
                    //x
                }
            );
        }

        void SaveDataProcess()//현재 상태 저장
        {
            var dragonLevel = statController.TempLevel;
            var dragonSLevel = skillController.TempSLevel;
            var partList = partController.GetUserPartTagList();//partTagList
            var petData = petController.PetData;//petData

            var getdragonTempData = User.Instance.DragonData.GetDragon(dragonTag);
            var userdragonTempData = new UserDragon();
            if (getdragonTempData != null)
                userdragonTempData = getdragonTempData;
            userdragonTempData.SetBaseData(dragonTag, eDragonState.Normal, CharExpData.GetCurrentAccumulateGradeAndLevelExp(userdragonTempData.BaseData.GRADE, dragonLevel), dragonLevel, dragonSLevel, -1);
            userdragonTempData.SetPartData(partList);
            //parts 기준으로 link 세팅
            Array.ForEach(userdragonTempData.Parts, (element) =>
            {
                var partTag = element;
                User.Instance.PartData.SetPartLink(partTag, dragonTag);
            });

            userdragonTempData.SetPartSetEffectOption();//부옵 계산

            var petTag = petData != null ? petData.Tag : -1;

            userdragonTempData.SetPetTag(petTag);
            User.Instance.PetData.SetPetLink(petTag, dragonTag);

            User.Instance.DragonData.AddUserDragon(dragonTag, userdragonTempData);
        }

        void revertSettingProcess()//되돌리기 기능
        {
            var partList = partController.GetDumpAllPrevPart();//기존 장비로 이전 장비 씌우기
            petController.dumpPrevPet();//이전 펫 세팅
            var petData = petController.PrevPet;//이전 펫 데이터

            var getdragonTempData = User.Instance.DragonData.GetDragon(dragonTag);
            var userdragonTempData = new UserDragon();
            if (getdragonTempData != null)
            {
                userdragonTempData = getdragonTempData;
            }

            var dragonLevel = getdragonTempData.Level;
            var dragonSLevel = getdragonTempData.SLevel;

            userdragonTempData.SetBaseData(dragonTag, eDragonState.Normal, CharExpData.GetCurrentAccumulateGradeAndLevelExp(userdragonTempData.BaseData.GRADE, dragonLevel), dragonLevel, dragonSLevel, -1);
            userdragonTempData.SetPartData(partList);
            //parts 기준으로 link 세팅
            Array.ForEach(userdragonTempData.Parts, (element) =>
            {
                var partTag = element;
                User.Instance.PartData.SetPartLink(partTag, dragonTag);
            });

            userdragonTempData.SetPartSetEffectOption();//부옵 계산

            var petTag = petData != null ? petData.Tag : -1;

            userdragonTempData.SetPetTag(petTag);
            User.Instance.PetData.SetPetLink(petTag, dragonTag);

            petController.initPrevPet();//이전 펫 데이터 초기화

            User.Instance.DragonData.AddUserDragon(dragonTag, userdragonTempData);
        }
    }
}

#endif