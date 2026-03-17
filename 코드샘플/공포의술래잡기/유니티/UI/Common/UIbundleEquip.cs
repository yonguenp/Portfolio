using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIbundleEquip : MonoBehaviour
{
    [SerializeField] Image equip_Icon;
    [SerializeField] Image equip_grade;
    [SerializeField] Text equip_lv;
    [SerializeField] GameObject equip_max;
    [SerializeField] GameObject equip_set;
    [SerializeField] GameObject equip_lock;
    [SerializeField] GameObject equip_dim;
    [SerializeField] GameObject focus;
    [SerializeField] GameObject checked_mark;

    public bool isSelected { get; private set; }
    public UserEquipData info { get; private set; }

    InventoryEquip Inventory = null;
    InventoryEquipLevelUp equipLevelUp = null;
    InventoryEquipEnchant equipEchant = null;
    //EquipmentPopup equipPopup = null;

    public void Init(UserEquipData data, InventoryEquip inven)
    {
        Inventory = inven;
        if (Inventory != null)
        {
            isSelected = Inventory.curSelectedEquipData == data;
        }

        Init(data);

    }
    public void Init(UserEquipData data, InventoryEquipLevelUp levelUp)
    {
        equipLevelUp = levelUp;

        Init(data);
    }
    public void Init(UserEquipData data, InventoryEquipEnchant enchant)
    {
        equipEchant = enchant;

        Init(data);
    }


    public void Init(UserEquipData data)
    {
        var ConfigDic = EquipConfig.GetConfigDic();
        info = data;

        equip_Icon.sprite = data.equipData.itemData.sprite;
        equip_lv.text = "LV." + data.lv.ToString();

        //MAX 처리
        string key = string.Empty;
        string resourcePath = string.Empty;
        switch (data.equipData.grade)
        {
            case 1:
                key = "equipment_c_max_level";
                resourcePath = "Texture/UI/equip/bg_equipment_c";
                break;
            case 2:
                key = "equipment_b_max_level";
                resourcePath = "Texture/UI/equip/bg_equipment_b";
                break;
            case 3:
                key = "equipment_a_max_level";
                resourcePath = "Texture/UI/equip/bg_equipment_a";
                break;
            case 4:
                key = "equipment_s_max_level";
                resourcePath = "Texture/UI/equip/bg_equipment_s";
                break;

        }
        equip_grade.sprite = Managers.Resource.Load<Sprite>(resourcePath);
        bool isMax = ConfigDic[key] <= data.lv;
        equip_max.SetActive(isMax);

        equip_lock.SetActive(CacheUserData.GetBoolean("Equip_Lock_" + info.id.ToString(), false));

        SetFocus(isSelected);
    }

    public void Init(EquipInfo info)
    {
        var ConfigDic = EquipConfig.GetConfigDic();

        equip_Icon.sprite = info.itemData.sprite;
        equip_lv.text = "";

        string resourcePath = string.Empty;
        switch (info.grade)
        {
            case 1:
                resourcePath = "Texture/UI/equip/bg_equipment_c";
                break;
            case 2:
                resourcePath = "Texture/UI/equip/bg_equipment_b";
                break;
            case 3:
                resourcePath = "Texture/UI/equip/bg_equipment_a";
                break;
            case 4:
                resourcePath = "Texture/UI/equip/bg_equipment_s";
                break;

        }
        equip_grade.sprite = Managers.Resource.Load<Sprite>(resourcePath);
        equip_max.SetActive(false);
        equip_lock.SetActive(false);
        SetEquipMark(false);

        SetFocus(isSelected);
    }
    public void MissingEquip()
    {
        equip_Icon.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/icon_plus_05");
        equip_grade.sprite = Managers.Resource.Load<Sprite>("Texture/UI/equip/bg_equipment_c");
        equip_lv.text = "";
        equip_max.SetActive(false);
        equip_lock.SetActive(false);
        equip_dim.SetActive(false);
        equip_set.SetActive(false);
        checked_mark.SetActive(false);
        SetFocus(false);
    }

    public void SetEquipMark(bool show)
    {
        if (equip_set != null)
            equip_set.SetActive(show);
    }

    public void SetCheckMark(bool show)
    {
        if (checked_mark != null)
            checked_mark.SetActive(show);

        if (equipLevelUp != null || equipEchant != null)
        {
            if (equip_dim != null)
                equip_dim.SetActive(show);
        }
    }
    public void SetEquipMark(bool show, bool dim)
    {
        if (equip_set != null)
            equip_set.SetActive(show);
        if (equip_dim != null)
            equip_dim.SetActive(dim);
    }

    public void SetLock(bool isLock)
    {
        CacheUserData.SetBoolean("Equip_Lock_" + info.id.ToString(), isLock);
        equip_lock.SetActive(CacheUserData.GetBoolean("Equip_Lock_" + info.id.ToString(), false));
    }

    public void SetFocus(bool bFocus)
    {
        isSelected = bFocus;
        focus.SetActive(isSelected);
        equip_dim.SetActive(isSelected);

        if (Inventory != null || equipLevelUp != null || equipEchant != null)
        {
            SetCheckMark(bFocus);
        }
    }


    public void OnButton()
    {
        if (info == null)
            return;

        SetFocus(!isSelected);

        if (Inventory != null)
        {
            if (isSelected)
                Inventory.SetSelectedEquipInfo(info);
            else
                Inventory.SetSelectedEquipInfo(null);
        }

        if (equipLevelUp != null)
        {
            if (equip_lock.activeSelf)
            {
                PopupCanvas.Instance.ShowFadeText("잠금아이템누름");
                SetFocus(false);
                return;
            }
            if (equip_set.activeSelf)
            {
                PopupCanvas.Instance.ShowFadeText("누군가착용중인아이템");
                SetFocus(false);
                return;
            }

            if (isSelected && !equipLevelUp.UseExpItem())
                SetFocus(false);
            else
                equipLevelUp.SelectedEquipList(info);
        }
        if (equipEchant != null)
        {
            if (equip_lock.activeSelf)
            {
                PopupCanvas.Instance.ShowFadeText("잠금아이템누름");
                SetFocus(false);
                return;
            }
            if (equip_set.activeSelf)
            {
                PopupCanvas.Instance.ShowFadeText("누군가착용중인아이템");
                SetFocus(false);
                return;
            }

            if (isSelected)
                equipEchant.SelectEchantEquip(info);
            else
                equipEchant.SelectEchantEquip(null);
        }

        //교환소 선택의 경우
        if (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP).IsOpening())
        {
            var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;

            if (isSelected)
                popup.exchange_equipList.Add(this);
            else
                popup.exchange_equipList.Remove(this);

            foreach (var item in popup.exchange_equipList)
            {
                item.SetFocus(true);
            }
        }
    }
    public bool OnPointerUpSelect()
    {
        if (info == null)
            return false;

        SetFocus(!isSelected);

        if (equipLevelUp != null)
        {
            if (isSelected && !equipLevelUp.UseExpItem())
            {
                SetFocus(false);
                return (false);
            }
            else
                equipLevelUp.SelectedEquipList(info);
        }
        return true;
    }

    public List<KeyValuePair<string, string>> EquipInfoTypeValue(int group_id, int level = 1)
    {
        var tableData = Managers.Data.GetData(GameDataManager.DATA_TYPE.equipment_level);

        List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
        List<EquipLevel> datas = new List<EquipLevel>();

        foreach (EquipLevel item in tableData)
        {
            if (item.group_id == group_id)
                datas.Add(item);
        }

        string[] optionValue = new string[4];
        var curLevelData = datas.Find(_ => _.level == level);
        for (int i = 0; i < curLevelData.equipLevelEffects.Count; i++)
        {
            double value = curLevelData.equipLevelEffects[i].effect_value;
            float f = Convert.ToSingle(value);

            string st = string.Empty;
            if (curLevelData.equipLevelEffects[i].effect_text_type == 2)
                st = (f * 0.1).ToString() + "%";
            else if (curLevelData.equipLevelEffects[i].effect_text_type == 3)
                st = (f * 0.001).ToString();
            else
                st = f.ToString();

            optionValue[i] = st;
        }

        int[] optionType = new int[4];
        foreach (EquipLevel item in datas)
        {
            if (optionType[0] == 0)
            {
                if (item.equipLevelEffects[0].effect_type != 0)
                    optionType[0] = item.equipLevelEffects[0].effect_type;
            }
            if (optionType[1] == 0)
            {
                if (item.equipLevelEffects[1].effect_type != 0)
                    optionType[1] = item.equipLevelEffects[1].effect_type;
            }
            if (optionType[2] == 0)
            {
                if (item.equipLevelEffects[2].effect_type != 0)
                    optionType[2] = item.equipLevelEffects[2].effect_type;
            }
            if (optionType[3] == 0)
            {
                if (item.equipLevelEffects[3].effect_type != 0)
                    optionType[3] = item.equipLevelEffects[3].effect_type;
            }
        }

        string opDes = string.Empty;
        int index = 0;
        foreach (int op in optionType)
        {
            switch (op)
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
            list.Add(new KeyValuePair<string, string>(opDes, optionValue[index]));
            index++;
        }

        return list;
    }

    public bool GetEquipSet()
    {
        return equip_set.activeSelf;
    }
    public bool GetEquipLock()
    {
        return equip_lock.activeSelf;
    }
}
