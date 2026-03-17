using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SandboxNetwork;
using UnityEngine.UI;
using System;

public class ChampionOptionSelectSlot : MonoBehaviour
{
    [SerializeField]
    Text desc;

    [SerializeField]
    Image image;

    public PetStatData petStatData { get; private set; } = null;
    public SubOptionData subOptionData { get; private set; } = null;
    public SkillPassiveData passiveData { get; private set; } = null;
    public PartFusionData fusionData { get; private set; } = null;
    ChampionOptionSlot parent = null;
    void Clear()
    {
        petStatData = null;
        subOptionData = null;
        passiveData = null;
        fusionData = null;
    }

    public void SetData(PetStatData data, ChampionOptionSlot p)
    {
        Clear();

        parent = p;

        string statKey = data.GetKey().ToString();

        var dataType = data.STAT_TYPE;
        var valueType = data.VALUE_TYPE;
        var typeStr = StatTypeData.GetDescStringByStatType(dataType, valueType == eStatusValueType.PERCENT);

        float optionValue = PetStatData.GetStatValue(statKey, GameConfigTable.GetPetLevelMax((int)eDragonGrade.Legend), GameConfigTable.GetPetReinforceLevelMax((int)eDragonGrade.Legend), true);

        optionValue = (float)Math.Round((double)optionValue, 2);
        var str = SBFunc.StrBuilder(typeStr, " +", optionValue);
        if (valueType == eStatusValueType.PERCENT)
        {
            str += "%";
        }

        desc.text = str;

        petStatData = data;
    }

    public void SetData(SubOptionData data, ChampionOptionSlot p)
    {
        Clear();

        parent = p;

        var dataType = data.STAT_TYPE;
        var valueType = data.VALUE_TYPE;
        var typeStr = StatTypeData.GetDescStringByStatType(dataType, valueType == "PERCENT");
        if (dataType == "ATK_DMG_RESIS" && valueType == "PERCENT")
        {
            typeStr = StringData.GetStringByStrKey("BASE_DMG_RESIS_PERCENT");
        }

        float optionValue = data.VALUE_MAX;

        optionValue = (float)Math.Round((double)optionValue, 2);
        var str = SBFunc.StrBuilder(typeStr, " +", optionValue);
        if (valueType == "PERCENT")
        {
            str += "%";
        }

        desc.text = str;

        subOptionData = data;
    }

    public void SetData(PartFusionData data, ChampionOptionSlot p)
    {
        Clear();

        parent = p;

        desc.text = StringData.GetStringFormatByStrKey(data._DESC, data.VALUE_MAX + data.LEGEND_BONUS + (data.VALUE_REINFORCE * 3));

        fusionData = data;
    }

    public void SetData(SkillPassiveData data, ChampionOptionSlot p)
    {
        Clear();

        parent = p;

        //desc.text = data.STRING;
        desc.text = SkillPassiveRateData.GetSkillGroupName(int.Parse(data.GetKey()));

        passiveData = data;
    }

    public void OnClick()
    {
        parent.OnClickOption(this);
    }

    public void SetColor(Color color, Color textColor)
    {
        image.color = color;
        desc.color = textColor;
    }
}
