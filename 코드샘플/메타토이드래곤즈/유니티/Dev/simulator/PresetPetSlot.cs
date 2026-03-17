using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG
namespace SandboxNetwork
{
    public class PresetPetSlot : MonoBehaviour
    {
        [SerializeField] private List<SimulatorDragonEditPetSkillSlot> petSkillSlot = null;
        [SerializeField] private GameObject clickNode = null;
        [SerializeField] private PetPortraitFrame petSlot = null;

        bool isClicked = false;

        PresetPet presetData = null;

        public delegate void func(PresetPet CustomEventData);

        private func clickCallback = null;

        public void SetData(PresetPet _data)
        {
            if (_data == null)
            {
                return;
            }
            presetData = _data;

            RefreshPetSkillSlot();
            RefreshPetFrame();
        }

        void RefreshPetSkillSlot()
        {
            if (petSkillSlot == null || petSkillSlot.Count <= 0)
            {
                return;
            }

            if (presetData == null)
            {
                for (var i = 0; i < petSkillSlot.Count; i++)
                {
                    petSkillSlot[i].SetVisibleSkillSlot(false);
                }
                return;
            }

            var petID = presetData.PetID;
            var petExp = presetData.Exp;
            var petGrade = PetBaseData.Get(petID.ToString()).GRADE;
            var expLevelData = PetExpData.GetLevelAndExpByTotalExp(petExp, petGrade);
            var petLevel = 1;
            if(expLevelData != null && expLevelData.ContainsKey("finallevel"))
            {
                petLevel = expLevelData["finallevel"];
            }
            
            var SkillSlotCount = GetNormalSkillSize(petID);
            var petSkillData = presetData.Passives;
            var petCurrentSkillCount = petSkillData.Count;

            for (var i = 0; i < petSkillSlot.Count; i++)
            {
                if (SkillSlotCount > i)
                {
                    petSkillSlot[i].SetVisibleSkillSlot(true);
                    petSkillSlot[i].initSkillSlot();
                    if (petCurrentSkillCount > i)
                    {
                        petSkillSlot[i].RefreshPetSkillIcon(petLevel, petSkillData[i]);
                    }
                }
                else
                {
                    petSkillSlot[i].SetVisibleSkillSlot(false);
                }
            }
        }

        void RefreshPetFrame()
        {
            if(petSlot == null)
            {
                return;
            }

            var petID = presetData.PetID;
            var petExp = presetData.Exp;
            var petGrade = PetBaseData.Get(petID.ToString()).GRADE;
            var expLevelData = PetExpData.GetLevelAndExpByTotalExp(petExp, petGrade);
            var petLevel = 1;
            if (expLevelData != null && expLevelData.ContainsKey("finallevel"))
            {
                petLevel = expLevelData["finallevel"];
            }

            petSlot.SetPetPortraitFrame(new UserPet(-1, petID, petLevel, petExp, 0), false, false);
        }

        int GetNormalSkillSize(int petID)
        {
            var slotCount = 0;

            var petBaseData = PetBaseData.Get(petID.ToString());
            if (petBaseData == null)
            {
                return slotCount;
            }

            slotCount = PetGradeData.Get(petBaseData.GRADE.ToString()).START_STAT_NUM;

            return slotCount;
        }

        public void setCallback(func ok_cb)
        {
            //this.eFuncType = FrameFunctioal.CallBack;

            if (ok_cb != null)
            {
                clickCallback = ok_cb;
            }
        }

        public void onClickSlot()
        {
            if (clickCallback != null)
            {
                clickCallback(presetData);
            }
        }
        public void SetVisibleClickNode(bool isVisible)
        {
            if (clickNode != null)
            {
                clickNode.SetActive(isVisible);
            }
            isClicked = isVisible;
        }
    }
}

#endif