using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG

namespace SandboxNetwork
{
    public class SimulatorPresetSavePopup : Popup<SimulatorDragonPopupData>
    {
        [SerializeField] private InputField input;
        private int dragon_tag;

        public override void InitUI()
        {
            dragon_tag = int.Parse(Data.SimulatorDragonTag);
            input.text = StringData.GetStringByStrKey(CharBaseData.Get(dragon_tag)._NAME);
        }

        public void OnClickSavePreset()
        {
            if (input.text == "")
            {
                ToastManager.On("저장할 프리셋의 이름을 입력하시오");
                return;
            }

            if (SimulatorPreset.isDuplicationPresetName(input.text, ePreset.DRAGON))//중복되는 프리셋이름이 있다
            {
                SystemPopup.OnSystemPopup("덮어쓰기", "해당 프리셋과 이름이 같은 프리셋이 존재합니다. 덮어 쓸까요?",
                    () => {
                        //ok
                        presetSaveOverWriteProcess();
                        ToastManager.On(SBFunc.StrBuilder("'", input.text, "'", " 프리셋 저장 완료!"));
                    }, 
                    () =>
                    {
                        //cancel
                        ShowAnywaySavePopup();
                    },
                    () =>
                    {
                        //x
                    }
                );
            }
            else
            {
                PresetSaveProcess();
                ToastManager.On(SBFunc.StrBuilder("'", input.text, "'", " 프리셋 저장 완료!"));
            }
        }

        void ShowAnywaySavePopup()
        {
            SystemPopup.OnSystemPopup("따로저장", "프리셋 이름이 같아도 그냥 저장할까요?",
                () =>
                {
                    //ok
                    PresetSaveProcess();
                    ToastManager.On(SBFunc.StrBuilder("'", input.text, "'", " 프리셋 저장 완료!"));
                }, 
                () =>
                {
                    //cancel
                },
                () =>
                {
                    //x
                }
            );
        }

        void presetSaveOverWriteProcess()//덮어 쓰기 기능 만들기
        {
            var duplicateID = SimulatorPreset.GetPresetID(input.text, ePreset.DRAGON);
            var dragon = makePresetDragon(duplicateID);

            SimulatorPreset.ReadMyDocument();
            SimulatorPreset.ApplyPreset(dragon);
            SimulatorPreset.SavePreset();
            ClosePopup();
        }

        void PresetSaveProcess()//저장은 생성의 역순(?)
        {
            var dragon = makePresetDragon();//데이터 기반 드래곤 프리셋 생성

            SimulatorPreset.ReadMyDocument();
            SimulatorPreset.ApplyPreset(dragon);
            SimulatorPreset.SavePreset();
            ClosePopup();
        }

        PresetDragon makePresetDragon(int _customPresetID = -1)
        {
            PresetDragon dragon = null;
            PresetPart[] parts = null;
            PresetPet pet = null;

            UserDragon uDragon = User.Instance.DragonData.GetDragon(dragon_tag);
            UserPet uPet = null;

            //유저펫 -> PresetPet 변환
            var uDragonsPetTag = uDragon.Pet;
            if (uDragonsPetTag > 0)
            {
                uPet = User.Instance.PetData.GetPet(uDragonsPetTag);

                if (uPet != null)
                {
                    int petPid = SimulatorPreset.GetPresetIDMax(ePreset.PET) + 1;
                    pet = new PresetPet(petPid, "프리셋" + petPid.ToString(), uPet.ID, uPet.Exp, uPet.SkillsID);
                }
            }

            var PartTagList = uDragon.Parts;
            UserPart[] uParts = new UserPart[PartTagList.Length];
            for (var i = 0; i < PartTagList.Length; i++)
            {
                var partTag = PartTagList[i];
                if (partTag <= 0)
                {
                    uParts[i] = null;
                    continue;
                }
                var uPartData = User.Instance.PartData.GetPart(partTag);
                uParts[i] = uPartData;
            }

            //유저 장비 -> PresetPart 변환
            if (uParts != null && uParts.Length > 0)
            {
                parts = new PresetPart[uParts.Length];
                for (var i = 0; i < uParts.Length; i++)
                {
                    var uPart = uParts[i];
                    if (uPart == null)
                    {
                        continue;
                    }

                    var partSubs = uPart.SubOptionList;
                    int[] partOptionList = new int[partSubs.Count];

                    for (var k = 0; k < partSubs.Count; k++)
                    {
                        partOptionList[k] = partSubs[k].Key;
                    }

                    int partPid = SimulatorPreset.GetPresetIDMax(ePreset.PART) + 1;
                    parts[i] = new PresetPart(partPid, "프리셋" + partPid.ToString(), uPart.ID, uPart.Reinforce, partOptionList);
                }
            }

            int pid = _customPresetID > 0 ? _customPresetID : SimulatorPreset.GetPresetIDMax(ePreset.DRAGON) + 1;
            dragon = new PresetDragon(pid, input.text, uDragon.Tag, uDragon.Exp, uDragon.SLevel, parts, pet);

            return dragon;
        }
    }

}
#endif