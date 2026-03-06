using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetInformationLayer : SubLayer
    {
        [SerializeField]
        PetTabLayer petTabLayer = null;



        [SerializeField]
        PetListPanel petSubListSlot = null;

        [SerializeField]
        Button backBtn = null;

        [SerializeField]
        PetStatInfo petDetailSlot = null;

        public virtual UserPetData GetPetInfo()
        {
            return User.Instance.PetData;
        }
        public virtual UserDragonData GetDragonInfo()
        {
            return User.Instance.DragonData;
        }

        public virtual int PetTagInfo
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

        public virtual int DragonTagInfo
        {
            get
            {
                return PopupManager.GetPopup<DragonManagePopup>().CurDragonTag;
            }
            set
            {
                PopupManager.GetPopup<DragonManagePopup>().CurDragonTag = value;
            }
        }
                
        public override void Init()
        {
            petDetailSlot?.Init();
            petSubListSlot?.Init();            
        }

        public virtual void OnClickBackButton()
        {
            if (DragonTagInfo != 0)//드래곤 태그값
            {
                var dragonManagePopup = PopupManager.GetPopup<DragonManagePopup>();
                if (dragonManagePopup != null)
                {
                    dragonManagePopup.moveTab(new DragonTabTypePopupData(0, 1));
                }
            }
            else
            {
                PopupManager.ClosePopup<DragonManagePopup>();
            }
        }

        public void OnClickPetLevelupButton()
        {
            if (petSubListSlot != null)
            {
                var tempPetTag = petSubListSlot.petTag;
                if (tempPetTag <= 0)
                {
                    ToastManager.On(StringData.GetStringByStrKey("사용할펫을선택해주세요"));//펫을 선택해주세요.
                    return;
                }

                var petData = GetPetInfo().GetPet(tempPetTag);
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
                    PetTagInfo = tempPetTag;
                    petTabLayer.onClickChangeLayer("5");
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
                    ToastManager.On(StringData.GetStringByStrKey("사용할펫을선택해주세요"));//펫을 선택해주세요.
                    return;
                }

                var petData = GetPetInfo().GetPet(tempPetTag);
                if (petData == null)
                {
                    return;
                }
                var petReinforceLevel = petData.Reinforce;
                var petReinforceMaxLevel = GameConfigTable.GetPetReinforceLevelMax(petData.Grade());

                if (petReinforceLevel >= petReinforceMaxLevel)
                {
                    ShowToastMsg(100000099);
                    return;
                }
                else
                {
                    PetTagInfo = tempPetTag;
                    petTabLayer.onClickChangeLayer("2");
                }
            }
        }

        public int GetCompoundButtonCondition(UserPet petInfo)//현재 버튼의 상태 가져오기
        {
            //case 0 : 합성 가능
            //case 1 : 장착함
            //case 2 : 만렙, 만강이 아님
            //case 3 : 레전더리인 경우 불가

            if (petInfo.Grade() == (int)eDragonGrade.Legend)
            {
                return 3;
            }

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
                ToastManager.On(StringData.GetStringByStrKey("사용할펫을선택해주세요"));//펫을 선택해주세요.
                return;
            }

            var petInfo = GetPetInfo().GetPet(tempPetTag);
            if (petInfo == null)
            {
                return;
            }

            var compoundCheck = GetCompoundButtonCondition(petInfo);
            switch (compoundCheck)
            {
                case 0:
                    PetTagInfo = tempPetTag;
                    petTabLayer.onClickChangeLayer("3");
                    break;
                case 1:
                    ShowToastMsg(100002245);
                    break;
                case 2:
                    ShowToastMsg(100002246);
                    break;
                case 3:
                    ShowToastMsg(100009869);
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

        public void OnClickDecompose()
        {
            if (petSubListSlot == null)
            {
                return;
            }

            var tempPetTag = petSubListSlot.petTag;
            if (tempPetTag <= 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("사용할펫을선택해주세요"));//펫을 선택해주세요.
                return;
            }

            var petInfo = GetPetInfo().GetPet(tempPetTag);
            if (petInfo == null)
            {
                return;
            }
            if(petInfo.LinkDragonTag > 0)
            {
                return;
            }
            PetTagInfo = tempPetTag;
            petTabLayer.onClickChangeLayer("4");
        }

        public void OnSubOptionReroll()
        {

        }
    }
}
