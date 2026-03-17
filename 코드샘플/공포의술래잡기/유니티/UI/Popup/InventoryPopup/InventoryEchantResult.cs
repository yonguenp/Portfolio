using Coffee.UIExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryEchantResult : MonoBehaviour
{
    [SerializeField] UIbundleEquip equip;
    [SerializeField] Text equip_name;
    [SerializeField] Text equip_priorLv_text;
    [SerializeField] Text equip_curLv_text;

    [SerializeField] Text[] equipOption_type;
    [SerializeField] Text[] equipOption_value;
    [SerializeField] Text[] equipOption_addValue;

    [SerializeField] GameObject[] optionDim;
    [SerializeField] GameObject[] optionNew;

    [SerializeField] UIParticle fx_confeti00;
    [SerializeField] UIParticle fx_equip_bulidup00;


    public void Init(UserEquipData prior, UserEquipData cur)
    {
        equip.Init(cur);
        equip.SetEquipMark(false);
        foreach (var item in Managers.UserData.MyCharacters)
        {
            if (item.Value.curEquip != null)
            {
                if (item.Value.curEquip == prior || item.Value.curEquip == cur)
                {
                    equip.SetEquipMark(true);
                    break;
                }
            }
        }
        EquipOptionType(prior, cur);
        fx_confeti00.Play();
        fx_equip_bulidup00.Play();
    }

    public void EquipOptionType(UserEquipData prior, UserEquipData cur)
    {
        var tableData = Managers.Data.GetData(GameDataManager.DATA_TYPE.equipment_level);
        int maxlv = 0;

        equip_name.text = prior.equipData.itemData.GetName();
        switch (prior.equipData.grade)
        {
            case 1:
                maxlv = EquipConfig.Config["equipment_c_max_level"];
                break;
            case 2:
                maxlv = EquipConfig.Config["equipment_b_max_level"];
                break;
            case 3:
                maxlv = EquipConfig.Config["equipment_a_max_level"];
                break;
            case 4:
                maxlv = EquipConfig.Config["equipment_s_max_level"];
                break;

        }
        equip_priorLv_text.text = "LV " + maxlv.ToString();
        maxlv = 0;
        switch (cur.equipData.grade)
        {
            case 1:
                maxlv = EquipConfig.Config["equipment_c_max_level"];
                break;
            case 2:
                maxlv = EquipConfig.Config["equipment_b_max_level"];
                break;
            case 3:
                maxlv = EquipConfig.Config["equipment_a_max_level"];
                break;
            case 4:
                maxlv = EquipConfig.Config["equipment_s_max_level"];
                break;
        }
        equip_curLv_text.text = "LV " + maxlv.ToString();

        List<EquipLevel> datas = new List<EquipLevel>();
        foreach (EquipLevel item in tableData)
        {
            if (item.group_id == prior.equipData.group_id)
                datas.Add(item);
        }

        int[] optionInt = new int[4];

        foreach (EquipLevel item in datas)
        {
            if (optionInt[0] == 0)
            {
                if (item.equipLevelEffects[0].effect_type != 0)
                    optionInt[0] = item.equipLevelEffects[0].effect_type;
            }
            if (optionInt[1] == 0)
            {
                if (item.equipLevelEffects[1].effect_type != 0)
                    optionInt[1] = item.equipLevelEffects[1].effect_type;
            }
            if (optionInt[2] == 0)
            {
                if (item.equipLevelEffects[2].effect_type != 0)
                    optionInt[2] = item.equipLevelEffects[2].effect_type;
            }
            if (optionInt[3] == 0)
            {
                if (item.equipLevelEffects[3].effect_type != 0)
                    optionInt[3] = item.equipLevelEffects[3].effect_type;
            }
        }

        string opDes = string.Empty;
        int index = 0;
        foreach (int optionNum in optionInt)
        {
            switch (optionNum)
            {
                case 1:
                    opDes = "ui_equip_option_01";
                    break;
                case 2:
                    opDes = "ui_equip_option_02";
                    break;
                case 3:
                    opDes = "ui_equip_option_03";
                    break;
                case 4:
                    opDes = "ui_equip_option_04";
                    break;
                case 5:
                    opDes = "ui_equip_option_05";
                    break;
                case 6:
                    opDes = "ui_equip_option_06";
                    break;
                case 7:
                    opDes = "ui_equip_option_07";
                    break;
                case 8:
                    opDes = "ui_equip_option_08";
                    break;
                case 9:
                    opDes = "ui_equip_option_09";
                    break;
                case 10:
                    opDes = "ui_equip_option_10";
                    break;
                case 11:
                    opDes = "ui_equip_option_11";
                    break;
                case 12:
                    opDes = "ui_equip_option_12";
                    break;
                case 13:
                    opDes = "ui_equip_option_13";
                    break;
                case 14:
                    opDes = "ui_equip_option_14";
                    break;
                case 15:
                    opDes = "ui_equip_option_15";
                    break;
                case 16:
                    opDes = "ui_equip_option_16";
                    break;
                case 17:
                    opDes = "ui_equip_option_17";
                    break;
                case 18:
                    opDes = "ui_equip_option_18";
                    break;
                case 19:
                    opDes = "ui_equip_option_19";
                    break;
                case 20:
                    opDes = "ui_equip_option_20";
                    break;
                case 21:
                    opDes = "ui_equip_option_21";
                    break;
                case 22:
                    opDes = "ui_equip_option_22";
                    break;
                case 23:
                    opDes = "ui_equip_option_23";
                    break;

                default:
                    opDes = "미발견옵션";
                    break;
            }
            equipOption_type[index].text = StringManager.GetString(opDes);
            index++;
        }

        //장비 상세 옵션 딤처리
        int isDim = 0;
        foreach (var dim in optionDim)
        {
            dim.SetActive(isDim >= cur.equipData.grade);
            isDim++;
        }

        foreach (var newst in optionNew)
        {
            newst.SetActive(false);
        }
        optionNew[cur.equipData.grade - 1].SetActive(true);


        var priorD = datas.Find(_ => _.level == prior.lv);
        var curD = datas.Find(_ => _.level == cur.lv);

        for (int i = 0; i < priorD.equipLevelEffects.Count; i++)
        {
            double value = priorD.equipLevelEffects[i].effect_value;
            float f = Convert.ToSingle(value);

            string st = string.Empty;
            if (priorD.equipLevelEffects[i].effect_text_type == 2)
                st = (f * 0.1).ToString() + "%";
            else if (priorD.equipLevelEffects[i].effect_text_type == 3)
                st = (f * 0.001).ToString();
            else
                st = f.ToString();

            equipOption_value[i].text = st;
        }

        for (int i = 0; i < curD.equipLevelEffects.Count; i++)
        {
            double value = curD.equipLevelEffects[i].effect_value;
            float f = Convert.ToSingle(value);

            string st = string.Empty;
            if (curD.equipLevelEffects[i].effect_text_type == 2)
                st = (f * 0.1).ToString() + "%";
            else if (priorD.equipLevelEffects[i].effect_text_type == 3)
                st = (f * 0.001).ToString();
            else
                st = f.ToString();

            equipOption_addValue[i].text = st;
        }

    }
}
