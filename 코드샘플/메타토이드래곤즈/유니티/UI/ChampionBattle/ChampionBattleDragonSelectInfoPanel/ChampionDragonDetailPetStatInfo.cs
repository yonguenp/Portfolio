using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class ChampionDragonDetailPetStatInfo : PetStatInfo
{
    [SerializeField]
    Text[] statEmptyText = null;

    private List<int> statList = null;
    private List<int> subOptionList = null;

    public ChampionDragonDetailPopup parentPopup { get { return PopupManager.GetPopup<ChampionDragonDetailPopup>(); } }
    public override UserPetData GetPetInfo()
    {
        return parentPopup.PetData;
    }

    public override UserDragonData GetDragonInfo()
    {
        return parentPopup.Dragons;
    }

    public override int CurPopupDragonTag
    {
        get
        {
            return parentPopup.DragonTag;
        }
    }

    public override int CurPopupPetTag
    {
        get
        {
            return parentPopup.PetTag;
        }
    }

    public override void RequestEquipPet()
    {
        var currentDragonData = parentPopup.Dragon;

        if (currentDragonData == null)
        {
            return;
        }

        if (CurPopupPetTag <= 0)
        {
            ToastManager.On(100001844);//펫을 선택해주세요.
            return;
        }

        ChampionPet pet = new ChampionPet(PetBaseData.Get(CurPopupPetTag));
        if (statList != null && statList.Count > 0)
        {
            for (int i = 0; i < statList.Count; i++)
            {
                PetStatData stat = PetStatData.Get(statList[i].ToString());

                pet.SetStat(stat, true, i);
            }
        }

        if (subOptionList != null && subOptionList.Count > 0)
        {
            for (int i = 0; i < subOptionList.Count; i++)
            {
                SubOptionData subOption = SubOptionData.Get(subOptionList[i]);

                pet.SetOption(subOption, i);
            }
        }

        currentDragonData.SetPet(pet, (data)=> {

            parentPopup.BackButton();
        });
    }

    public override void RequestUnEquipPet()
    {
        var currentDragonData = parentPopup.Dragon;
        if (currentDragonData == null)
        {
            return;
        }

        //currentDragonData.SetPet(0, (data) => {
        //    parentPopup.BackButton();
        //});
    }

    protected override void RefreshCurrentPetData()
    {
        //base.RefreshCurrentPetData();
        ClearInfoObj();

        statList = null;
        subOptionList = null;

        SetEmptySlot(CurPopupPetTag <= 0);

        var currentDragonData = parentPopup.Dragon;
        var userPetInfo = parentPopup.Pet;
        if (currentDragonData == null || userPetInfo == null) return;

        petSpine.gameObject.SetActive(true);
        petSpine.Init();
        if (userPetInfo.GetPetDesignData().SKIN != "NONE")
        {
            petSpine.SetSkin(userPetInfo.GetPetDesignData().SKIN);
        }
        var petGrade = userPetInfo.Grade();
        var maxLv = GameConfigTable.GetPetLevelMax(petGrade);
        var maxReinforceLv = PetReinforceData.GetMaxReinforceStep(petGrade);
        petLevelText.text = string.Format("<color=#FAC81E>Lv. {0}</color> <color=#ABABAB>/ {1}</color>", userPetInfo.Level, maxLv);
        petReinforceText.text = StringData.GetStringByIndex(100001128) + string.Format(" : +{0}", userPetInfo.Reinforce);
        petNameText.text = userPetInfo.Name();
        if (userPetInfo.LinkDragonTag > 0)
        {
            SetBelongedDragonPortrait(userPetInfo);
        }
        else
        {
            EquipDragonNode.SetActive(false);
        }

        petRankImgs[userPetInfo.Grade() - 1].SetActive(true);
        petElemImgs[userPetInfo.Element() - 1].SetActive(true);

        Set_SetOption(userPetInfo);

        SBFunc.RemoveAllChildrens(effectTargetParent.transform);
        var effectClone = Instantiate(petEffectPrefabList[userPetInfo.Grade() - 1], effectTargetParent.transform);//펫 등급 이펙트
        effectClone.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        if (currentDragonData.ChampionPet != null && currentDragonData.ChampionPet.ID == CurPopupPetTag) 
        {
            //펫이 있으면
            if (currentDragonData.ChampionPet.Stats != null && currentDragonData.ChampionPet.Stats.Count > 0)
            {
                var _stats = currentDragonData.ChampionPet.Stats;
                statList = new List<int>();
                for (int i = 0; i < _stats.Count; i++)
                {
                    if (_stats[i].Key == 0)
                        continue;

                    statList.Add(_stats[i].Key);
                    PetStatData stats = PetStatData.Get(_stats[i].Key.ToString());

                    StatObjs[i].SetActive(true);
                    baseStatTexts[i].gameObject.SetActive(true);
                    baseStatValueTexts[i].gameObject.SetActive(true);
                    statEmptyText[i].gameObject.SetActive(false);

                    string statKey = stats.GetKey().ToString();

                    var dataType = stats.STAT_TYPE;
                    var valueType = stats.VALUE_TYPE;
                    var typeStr = StatTypeData.GetDescStringByStatType(dataType, valueType == eStatusValueType.PERCENT);

                    float optionValue = PetStatData.GetStatValue(statKey, GameConfigTable.GetPetLevelMax((int)eDragonGrade.Legend), GameConfigTable.GetPetReinforceLevelMax((int)eDragonGrade.Legend), true);

                    optionValue = (float)Math.Round((double)optionValue, 2);
                    var str = SBFunc.StrBuilder(" +", optionValue);
                    if (valueType == eStatusValueType.PERCENT)
                    {
                        str += "%";
                    }

                    baseStatTexts[i].text = typeStr;
                    baseStatValueTexts[i].text = str;
                }
            }

            if (currentDragonData.ChampionPet.SubOptionList != null && currentDragonData.ChampionPet.SubOptionList.Count > 0)
            {
                var _subOptions = currentDragonData.ChampionPet.SubOptionList;
                subOptionList = new List<int>();
                for (int i = 0; i < _subOptions.Count; i++)
                {
                    if (_subOptions[i].Key == 0)
                        continue;

                    subOptionList.Add(_subOptions[i].Key);
                    SubOptionData subOption = SubOptionData.Get(_subOptions[i].Key);

                    //임시처리 - 텍스트만 보여줌
                    optionObjs[i].SetActive(true);
                    optionTexts[i].gameObject.SetActive(true);
                    optionValueTexts[i].gameObject.SetActive(true);
                    optionEmptyTexts[i].gameObject.SetActive(false);

                    string statKey = subOption.GetKey().ToString();

                    var dataType = subOption.STAT_TYPE;
                    var valueType = subOption.VALUE_TYPE;
                    var typeStr = StatTypeData.GetDescStringByStatType(dataType, valueType == "PERCENT");

                    float optionValue = subOption.VALUE_MAX;

                    optionValue = (float)Math.Round((double)optionValue, 2);
                    var str = SBFunc.StrBuilder(" +", optionValue);
                    if (valueType == "PERCENT")
                    {
                        str += "%";
                    }

                    optionTexts[i].text = typeStr;
                    optionValueTexts[i].text = str;

                }
            }
        }

    }
    void SetEmptySlot(bool _isEmpty)
    {
        for (var i = 0; i < optionObjs.Length; i++)
        {
            statEmptyText[i].gameObject.SetActive(!_isEmpty);
            optionEmptyTexts[i].gameObject.SetActive(!_isEmpty);
            optionLockObjs[i].SetActive(false);
            LockStatObjs[i].SetActive(false);
        }
    }

    public void OnClickSetStat()
    {
        if (CurPopupPetTag <= 0 || CurPopupDragonTag <= 0) return;

        var popup = PopupManager.OpenPopup<OptionSelectPopup>();
        popup.SetData(OptionSelectPopup.OptionType.PetStat, CurPopupDragonTag, CurPopupPetTag);
        popup.SetCallback(selectStatComplete);
    }
    public void selectStatComplete(List<int> _options)
    {
        statList = new List<int>(_options);

        for (int i = 0; i < _options.Count; i++)
        {
            var id = _options[i];

            PetStatData option = PetStatData.Get(id.ToString());

            StatObjs[i].SetActive(true);
            baseStatTexts[i].gameObject.SetActive(true);
            baseStatValueTexts[i].gameObject.SetActive(true);
            statEmptyText[i].gameObject.SetActive(false);

            string statKey = option.GetKey().ToString();

            var dataType = option.STAT_TYPE;
            var valueType = option.VALUE_TYPE;
            var typeStr = StatTypeData.GetDescStringByStatType(dataType, valueType == eStatusValueType.PERCENT);

            float optionValue = PetStatData.GetStatValue(statKey, GameConfigTable.GetPetLevelMax((int)eDragonGrade.Legend), GameConfigTable.GetPetReinforceLevelMax((int)eDragonGrade.Legend), true);

            optionValue = (float)Math.Round((double)optionValue, 2);
            var str = SBFunc.StrBuilder(" +", optionValue);
            if (valueType == eStatusValueType.PERCENT)
            {
                str += "%";
            }
            
            baseStatTexts[i].text = typeStr;
            baseStatValueTexts[i].text = str;
        }


    }

    public void OnClickSetOption()
    {
        if (CurPopupPetTag <= 0 || CurPopupDragonTag <= 0) return;

        var popup = PopupManager.OpenPopup<OptionSelectPopup>();
        popup.SetData(OptionSelectPopup.OptionType.PetOption, CurPopupDragonTag, CurPopupPetTag);
        popup.SetCallback(selectOptionComplete);
    }
    public void selectOptionComplete(List<int> _options)
    {
        subOptionList = new List<int>(_options);

        for (int i = 0; i < _options.Count; i++)
        {
            var id = (int)_options[i];

            SubOptionData option = SubOptionData.Get(id);

            //임시처리 - 텍스트만 보여줌
            optionObjs[i].SetActive(true);
            optionTexts[i].gameObject.SetActive(true);
            optionValueTexts[i].gameObject.SetActive(true);
            optionEmptyTexts[i].gameObject.SetActive(false);

            string statKey = option.GetKey().ToString();

            var dataType = option.STAT_TYPE;
            var valueType = option.VALUE_TYPE;
            var typeStr = StatTypeData.GetDescStringByStatType(dataType, valueType == "PERCENT");

            float optionValue = option.VALUE_MAX;

            optionValue = (float)Math.Round((double)optionValue, 2);
            var str = SBFunc.StrBuilder(" +", optionValue);
            if (valueType == "PERCENT")
            {
                str += "%";
            }

            optionTexts[i].text = typeStr;
            optionValueTexts[i].text = str;
      
        }
    }
}
