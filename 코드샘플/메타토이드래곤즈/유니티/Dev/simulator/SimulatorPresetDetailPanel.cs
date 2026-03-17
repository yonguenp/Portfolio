using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG
namespace SandboxNetwork
{
    public class SimulatorPresetDetailPanel : MonoBehaviour
    {
        [SerializeField] private List<PresetPartSlot> partPresetList = new List<PresetPartSlot>();
        [SerializeField] private PresetPetSlot petPreset = null;
        [SerializeField] private DragonPortraitFrame dragonFrame = null;
        [SerializeField] private GameObject infoPanel = null;
        [SerializeField] private Text petEmptyLabel = null;

        void SetVisiblePanel(bool _isVisible)
        {
            if(infoPanel != null)
            {
                infoPanel.SetActive(_isVisible);
            }
        }

        public void onHidePanel()
        {
            SetVisiblePanel(false);
        }

        public void onShowPanel(PresetDragon _data)
        {
            if(_data == null)
            {
                Debug.Log("프리셋 데이터가 없음");
                return;
            }

            RefreshDragonFrame(_data);
            RefreshPartPreset(_data.PartsArray);//장비 프리셋 UI 갱신
            RefreshPetPreset(_data.Pet);
            SetVisiblePanel(true);
        }

        void RefreshPartPreset(PresetPart[] _presetData)
        {
            var dataCount = _presetData.Length;

            for(var i = 0; i< partPresetList.Count; i++)
            {
                PresetPart tempPart = null;
                if(dataCount > i)
                {
                    var preset = _presetData[i];
                    if (preset.ToString() != "{}")
                    {
                        tempPart = preset;
                    }
                }

                partPresetList[i].SetData(tempPart);
            }
        }

        void RefreshPetPreset(PresetPet _presetData)
        {
            var isEmptyCheck = _presetData.ToString() == "{}";
            petEmptyLabel.gameObject.SetActive(isEmptyCheck);
            petPreset.gameObject.SetActive(!isEmptyCheck);

            if (!isEmptyCheck)
            {
                petPreset.SetData(_presetData);
            }
        }

        void RefreshDragonFrame(PresetDragon _data)
        {
            var exp = _data.Exp;
            var tag = _data.DragonID;
            var data = CharBaseData.Get(_data.DragonID);

            var levelExpData = TableManager.GetTable<CharExpTable>().GetLevelAndExpByGradeAndTotalExp(data.GRADE, exp);
            if(levelExpData == null)
            {
                return;
            }

            if (dragonFrame != null)
            {
                dragonFrame.SetCustomPotraitFrame(tag, levelExpData.FinalLevel);
            }
        }
    }
}
#endif
