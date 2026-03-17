using Coffee.UIEffects;
using Google.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    //[Serializable]
    //public class OptionObjectList
    //{
    //    public List<GameObject> objects = new List<GameObject>();
    //}

    public class OptionSelectPopup : Popup<PopupBase>
    {
        public enum OptionType
        { 
            None,
            SkillPassive,
            PetStat,
            PetOption,
            PartOption,
            PartFusion,
        }


        [SerializeField]
        private GameObject optionListPrefab = null;
        [SerializeField]
        private Transform optionListParent = null;

        [SerializeField]
        private GameObject viewport = null;
        [SerializeField]
        private GameObject optionObj = null;
        [SerializeField]
        private GameObject optionText = null;
        [SerializeField]
        private HorizontalLayoutGroup horizontalLayoutGroup = null;

        public delegate void func(List<int> _options);
        private func clickCallback = null;

        private float newSize = 1f;
        private int contentCount = 4;
        private float viewportSize = 624f;
        private float contentSize = 598f;

        private float layoutSpacing = -15f;

        int dragonTag = -1;
        int pTag = -1;
        int equipSlot = -1;
        OptionType curType = OptionType.None;
        List<int> SelectedOption = new List<int>();
        public void SetData(OptionType type, int dtag, int ptag = -1, int equipslot = -1)
        {
            curType = type;
            dragonTag = dtag;
            pTag = ptag;
            equipSlot = equipslot;
            SetViewPort();
            RefreshUI();
        }

        public void SetViewPort()
        {
            switch (curType)
            {
                case OptionType.PartFusion:
                {
                    newSize = 4f;
                }
                break;
                case OptionType.SkillPassive:
                {
                    newSize = 2f;
                }
                break;
                case OptionType.PetOption:
                case OptionType.PetStat:
                case OptionType.PartOption:
                {
                    newSize = 1f;
                }break;
                default:
                    break;
            }

            
            horizontalLayoutGroup.spacing = layoutSpacing * newSize;

            ReSizeWidth(viewport.GetComponent<RectTransform>(), viewportSize, newSize);
            ReSizeWidth(optionObj.GetComponent<RectTransform>(), contentSize, newSize);
            ReSizeWidth(optionText.GetComponent<RectTransform>(), contentSize, newSize);

        }

        public void ReSizeWidth(RectTransform _obj, float _defaultSize, float _newSize)
        {
            _obj.sizeDelta = new Vector2((_defaultSize * _newSize), _obj.sizeDelta.y);
        }

        public override void InitUI()
        {
            
        }

        public void RefreshUI()
        {
            SelectedOption.Clear();
            foreach (Transform child in optionListParent)
            {
                if (child == optionListPrefab.transform)
                    continue;

                Destroy(child.gameObject);
            }

            switch(curType)
            {
                case OptionType.PartOption:
                    SetPartOption();
                    break;
                case OptionType.PetOption:
                    SetPetOption();
                    break;
                case OptionType.PetStat:
                    SetPetStat();
                    break;
                case OptionType.SkillPassive:
                    SetPassive();
                    break;
                case OptionType.PartFusion:
                    SetPartFusionOption();
                    break;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(optionListParent.GetComponent<RectTransform>());
        }

        void SetPassive()
        {
            optionListPrefab.SetActive(true);
            var dragon = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;

            for (int i = 0; i < 2; ++i)
            {
                var option = ChampionManager.GetSelectablePassive(dragon.BaseData.JOB, i);
                var clone = Instantiate(optionListPrefab, optionListParent);
                var slotInfo = clone.GetComponent<ChampionOptionSlot>();

                int selected = -1;
                if (dragon.PassiveSkills.Count > i)
                {
                    selected = dragon.PassiveSkills[i];
                }

                SelectedOption.Add(selected);
                slotInfo.Init(option, i, this, selected);                
            }
            optionListPrefab.SetActive(false);
        }

        void SetPartOption()
        {
            optionListPrefab.SetActive(true);
            var dragon = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
            ChampionPart part = null;
            if (equipSlot > 0)
                part = dragon.GetPart(equipSlot);
            
            for (int i = 0; i < 4; ++i)
            {
                var option = ChampionManager.GetSelectableSubOptions(PartBaseData.Get(pTag), i);
                var clone = Instantiate(optionListPrefab, optionListParent);
                var slotInfo = clone.GetComponent<ChampionOptionSlot>();

                int selected = part != null && part.SubOptionList.Count > i ? part.SubOptionList[i].Key : 0;
                SelectedOption.Add(selected);
                slotInfo.Init(option, i, this, selected);
            }
            optionListPrefab.SetActive(false);
        }

        void SetPartFusionOption()
        {
            optionListPrefab.SetActive(true);
            var dragon = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
            ChampionPart part = null;
            if (equipSlot > 0)
                part = dragon.GetPart(equipSlot);

            var option = ChampionManager.GetSelectableFusionOptions();
            var clone = Instantiate(optionListPrefab, optionListParent);
            var slotInfo = clone.GetComponent<ChampionOptionSlot>();

            int selected = part != null && part.FusionStatKey > 0 ? part.FusionStatKey : 0;
            SelectedOption.Add(selected);
            slotInfo.Init(option, 0, this, selected);

            optionListPrefab.SetActive(false);
        }

        void SetPetOption()
        {
            optionListPrefab.SetActive(true);
            var dragon = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
            var pet = dragon.ChampionPet;

            for (int i = 0; i < 4; ++i)
            {
                var option = ChampionManager.GetSelectableSubOptions(PetBaseData.Get(pTag), i);
                var clone = Instantiate(optionListPrefab, optionListParent);
                var slotInfo = clone.GetComponent<ChampionOptionSlot>();

                int selected = pet != null && pet.SubOptionList.Count > i ? pet.SubOptionList[i].Key : 0;
                SelectedOption.Add(selected);
                slotInfo.Init(option, i, this, selected);
            }
            optionListPrefab.SetActive(false);
        }

        void SetPetStat()
        {
            optionListPrefab.SetActive(true);
            var dragon = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
            var pet = dragon.ChampionPet;

            var option = ChampionManager.GetSelectablePetStatsList();
            for (int i = 0; i < 4; ++i)
            {
                var clone = Instantiate(optionListPrefab, optionListParent);
                var slotInfo = clone.GetComponent<ChampionOptionSlot>();

                int selected = pet != null && pet.Stats.Count > i ? pet.Stats[i].Key : 0;
                SelectedOption.Add(selected);
                slotInfo.Init(option, i, this, selected);
            }
            optionListPrefab.SetActive(false);
        }

        public void OnClickOption(int index, ChampionOptionSelectSlot option)
        {
            switch(curType)
            {
                case OptionType.PartOption:
                    SelectedOption[index] = option.subOptionData.KEY;
                    break;
                case OptionType.PetOption:
                    SelectedOption[index] = option.subOptionData.KEY;
                    break;
                case OptionType.PetStat:
                    SelectedOption[index] = option.petStatData.KEY;
                    break;
                case OptionType.SkillPassive:
                    SelectedOption[index] = option.passiveData.KEY;
                    break;
                case OptionType.PartFusion:
                    SelectedOption[index] = int.Parse(option.fusionData.KEY);
                    break;
            }
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
        }

        public void OnclickOK()
        {
            if (SelectedOption.Contains(0) || SelectedOption.Contains(-1))
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("옵션선택경고"), StringData.GetStringByStrKey("확인"), "",
                () => { }
                );
                return;
            }

            clickCallback.Invoke(SelectedOption);
            base.ClosePopup();
        }

        public void SetCallback(func ok_cb)
        {
            if (ok_cb != null)
            {
                clickCallback = ok_cb;
            }
        }

    }
}

