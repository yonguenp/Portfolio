using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetInfoLayer : SubLayer
    {
        [SerializeField]
        PetTabLayer petTabLayer = null;

        [SerializeField]
        PetInfoPanel petDetailSlot = null;

        [SerializeField]
        PetListPanel petSubListSlot = null;

        [SerializeField]
        Button backBtn = null;

        public override void Init()
        {
            petDetailSlot?.Init();
            petSubListSlot?.Init();
        }

        public void onClickBackButton()
        {
            if (PopupManager.GetPopup<DragonManagePopup>().CurDragonTag != 0)//드래곤 태그값
            {
                var dragonManagePopup = PopupManager.GetPopup<DragonManagePopup>();
                if (dragonManagePopup != null)
                {
                    dragonManagePopup.moveTab(new DragonTabTypePopupData(0, 1));
                }
            }
            else
            {
                petTabLayer.onClickChangeLayer("0");
            }
        }

        public void OnClickPetLevelupButton()
        {
            if (petSubListSlot != null)
            {
                var tempPetTag = petSubListSlot.petTag;
                if (tempPetTag <= 0)
                {
                    return;
                }

                var petData = User.Instance.PetData.GetPet(tempPetTag);
                if (petData == null)
                {
                    return;
                }
                var petLevel = petData.Level;
                var petMaxLevel = GameConfigTable.GetPetLevelMax(petData.Grade());

                if (petLevel >= petMaxLevel)
                {
                    ShowToastMsg(100000099);
                    return;
                }
                else
                {
                    PopupManager.GetPopup<DragonManagePopup>().CurPetTag = tempPetTag;
                    petTabLayer.onClickChangeLayer("2");
                }
            }
        }

        public void OnClickPetReinforceLevelupButton()
        {
            if (petSubListSlot != null)
            {
                var tempPetTag = petSubListSlot.petTag;
                if (tempPetTag <= 0)
                {
                    return;
                }

                var petData = User.Instance.PetData.GetPet(tempPetTag);
                if (petData == null)
                {
                    return;
                }
                var petReinforceLevel = petData.Reinforce;
                var petMaxLevel = GameConfigTable.GetPetReinforceLevelMax(petData.Grade());

                if (petReinforceLevel >= petMaxLevel)
                {
                    ShowToastMsg(100000099);
                    return;
                }
                else
                {
                    PopupManager.GetPopup<DragonManagePopup>().CurPetTag = tempPetTag;
                    petTabLayer.onClickChangeLayer("3");
                }
            }
        }

        public int GetCompoundButtonCondition(UserPet petInfo)//현재 버튼의 상태 가져오기
        {
            //case 0 : 합성 가능
            //case 1 : 장착함
            //case 2 : 만렙, 만강이 아님

            var isBelonged = petInfo.LinkDragonTag > 0;
            if (isBelonged)
            {
                return 1;
            }
            else
            {
                var level = petInfo.Level;
                var reinforce = petInfo.Reinforce;

                if (level < GameConfigTable.GetPetLevelMax(petInfo.Grade()) || reinforce < GameConfigTable.GetPetReinforceLevelMax(petInfo.Grade()))
                {
                    return 2;
                }
                else
                {
                    return 0;
                }
            }
        }

        public void OnClickCompoundButton()
        {
            if (petSubListSlot == null)
            {
                return;
            }

            var tempPetTag = petSubListSlot.petTag;
            if (tempPetTag <= 0)
            {
                return;
            }

            var petInfo = User.Instance.PetData.GetPet(tempPetTag);
            if (petInfo == null)
            {
                return;
            }

            var compoundCheck = GetCompoundButtonCondition(petInfo);
            switch (compoundCheck)
            {
                case 0:
                    PopupManager.GetPopup<DragonManagePopup>().CurPetTag = tempPetTag;
                    petTabLayer.onClickChangeLayer("4");
                    break;
                case 1:
                    ShowToastMsg(100002245);
                    break;
                case 2:
                    ShowToastMsg(100002246);
                    break;
                default:
                    break;
            }
        }

        public void ShowToastMsg(int strindex)
        {
            ToastManager.On(strindex);
            Debug.Log(StringData.GetStringByIndex(strindex));
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
