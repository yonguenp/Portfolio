using Coffee.UIExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryEquipLevelUp : MonoBehaviour
{
    InventoryPanel parent = null;

    [SerializeField] UIbundleEquip equipSample;
    [SerializeField] GameObject upBtn;
    [SerializeField] GameObject downBtn;
    [SerializeField] Dropdown dropdown;
    [SerializeField] string[] optionKey;
    [SerializeField] Text selected_item_text;

    [Header("[장비 상세정보]")]
    [SerializeField] UIbundleEquip Detail_equip;
    [SerializeField] Text Detail_equipName;
    [SerializeField] Text Detail_EquipLV;
    [SerializeField] Text[] Detail_equipOption;
    [SerializeField] Text[] Detail_equipOption_value;
    [SerializeField] Text[] Detail_equipOption_addValue;
    [SerializeField] Slider slider;
    [SerializeField] Text slider_text;
    [SerializeField] GameObject[] optionDim;

    [Header("[버 튼]")]
    [SerializeField] Text levelUpBtn_text;
    [SerializeField] Button initBtn;
    [SerializeField] Button autoSelectBtn;
    [SerializeField] Button levelUpBtn;

    [Header("[이펙트]")]
    [SerializeField] UIParticle fx_equip_front00;
    [SerializeField] UIParticle fx_equip_light00;
    int curSort = 0;
    bool isDescending = true;
    int SelectedEquipNo = 0;

    int needGold = 0;
    UserEquipData selectedEquip
    {
        get
        {
            if (Managers.UserData.MyEquips.ContainsKey(SelectedEquipNo))
                return Managers.UserData.MyEquips[SelectedEquipNo];
            else
                return null;
        }
        set
        {
            SelectedEquipNo = value.id;
        }
    }
    List<UIbundleEquip> items = new List<UIbundleEquip>();
    List<UserEquipData> selectedLists = new List<UserEquipData>();

    private void Start()
    {
        dropdown.options.Clear();
        foreach (var item in optionKey)
        {
            dropdown.options.Add(new Dropdown.OptionData(StringManager.GetString(item), Managers.Resource.Load<Sprite>("Texture/UI/friend/btn_sub_ivory_01")));
        }
    }

    public void Init(InventoryPanel equip)
    {
        parent = equip;
    }

    public void Close()
    {
        if (gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);

            if (parent != null)
                parent.GetComponent<InventoryEquip>().CloseSubPopup();
        }
    }

    public void Show(UserEquipData selectEquip = null)
    {
        gameObject.SetActive(true);
        selectedEquip = selectEquip;
        EquipLevelRefreshUI();
    }

    public bool IsEquipCharacter(UserEquipData equip)
    {
        if (Managers.UserData.MyCharacters != null)
        {
            foreach (var userCharacters in Managers.UserData.MyCharacters)
            {
                if (userCharacters.Value.curEquip != null)
                {
                    if (equip == userCharacters.Value.curEquip)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public void EquipLevelRefreshUI()
    {
        if (selectedEquip == null)
        {
            PopupCanvas.Instance.ShowFadeText("not selected error!!");
            return;
        }

        Clear();

        Dictionary<int, UserEquipData> equipDatas = new Dictionary<int, UserEquipData>(Managers.UserData.MyEquips);

        foreach (var equip in equipDatas)
        {
            if (equip.Value == selectedEquip)
            {
                selectedEquip = equip.Value;
                equipDatas.Remove(equip.Key);
                break;
            }
        }

        switch (curSort)
        {
            case 1:
                if (isDescending)
                    equipDatas = equipDatas.OrderByDescending(_ => _.Value.lv).ToDictionary(_ => _.Key, _ => _.Value);
                else
                    equipDatas = equipDatas.OrderBy(_ => _.Value.lv).ToDictionary(_ => _.Key, _ => _.Value);
                break;
            case 2:
                if (isDescending)
                    equipDatas = equipDatas.OrderByDescending(_ => _.Value.update_time).ToDictionary(_ => _.Key, _ => _.Value);
                else
                    equipDatas = equipDatas.OrderBy(_ => _.Value.update_time).ToDictionary(_ => _.Key, _ => _.Value);
                break;
            default:
                if (isDescending)
                    equipDatas = equipDatas.OrderByDescending(_ => _.Value.equipData.grade).ToDictionary(_ => _.Key, _ => _.Value);
                else
                    equipDatas = equipDatas.OrderBy(_ => _.Value.equipData.grade).ToDictionary(_ => _.Key, _ => _.Value);
                break;
        }

        downBtn.SetActive(isDescending);
        upBtn.SetActive(!isDescending);

        foreach (var data in equipDatas)
        {
            equipSample.gameObject.SetActive(true);

            //장비가 락 or 누군가 착용중
            if (CacheUserData.GetBoolean("Equip_Lock_" + data.Value.id.ToString(), false) || IsEquipCharacter(data.Value))
                continue;

            var equip = GameObject.Instantiate(equipSample, equipSample.transform.parent);
            equip.Init(data.Value, this);
            equip.SetEquipMark(false, false);

            items.Add(equip);
        }
        equipSample.gameObject.SetActive(false);


        UpdateEquipOptionUI();
    }

    void UpdateEquipOptionUI()
    {
        if (selectedEquip == null)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_equip_popup_error"));
            return;
        }

        Detail_equip.Init(selectedEquip);
        Detail_equip.SetEquipMark(false);
        foreach (var userCharacters in Managers.UserData.MyCharacters)
        {
            if (userCharacters.Value.curEquip != null)
            {
                if (selectedEquip == userCharacters.Value.curEquip)
                {
                    Detail_equip.SetEquipMark(true);
                    break;
                }
            }
        }
        Detail_EquipLV.text = "LV." + selectedEquip.lv.ToString();
        Detail_EquipLV.color = Color.white;
        Detail_equipName.text = selectedEquip.equipData.itemData.GetName();
        Detail_equipName.color = Color.white;
        string opDes = string.Empty;
        int index = 0;
        foreach (int optionNum in parent.GetComponent<InventoryEquip>().GetOptionData(selectedEquip.equipData.group_id))
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
            Detail_equipOption[index].text = StringManager.GetString(opDes);
            index++;
        }

        //장비 상세 옵션 딤처리
        int isDim = 0;
        foreach (var dim in optionDim)
        {
            dim.SetActive(isDim >= selectedEquip.equipData.grade);
            isDim++;
        }
        UpdateEquipLevelData();
    }

    public int CalSelectedExp(UserEquipData addEquip = null)
    {
        int addExp = 0;
        //선택된 장비들 경험치 추출
        List<UserEquipData> simulList = new List<UserEquipData>();
        foreach (var item in selectedLists)
        {
            simulList.Add(item);
        }
        if (addEquip != null)
            simulList.Add(addEquip);

        var applyExp = 0;
        if (simulList != null && simulList.Count != 0)
        {
            var equip_configData = EquipConfig.GetConfigDic();
            foreach (var item in simulList)
            {
                int configVal = 0;
                switch (item.equipData.grade)
                {
                    //C등급 -> 1
                    case 1:
                        configVal = equip_configData["base_grade_exp_c"];
                        break;
                    case 2:
                        configVal = equip_configData["base_grade_exp_b"];
                        break;
                    case 3:
                        configVal = equip_configData["base_grade_exp_a"];
                        break;
                    case 4:
                        configVal = equip_configData["base_grade_exp_s"];
                        break;
                }
                var bous = selectedEquip.equipData.group_id == item.equipData.group_id ? equip_configData["bonus_exp"] * 0.001 : 1;
                addExp += (int)((double)configVal * bous);

                addExp += item.exp;

            }
            var max_level = 0;
            switch (selectedEquip.equipData.grade)
            {
                case 1:
                    max_level = equip_configData["equipment_c_max_level"];
                    break;
                case 2:
                    max_level = equip_configData["equipment_b_max_level"];
                    break;
                case 3:
                    max_level = equip_configData["equipment_a_max_level"];
                    break;
                case 4:
                    max_level = equip_configData["equipment_s_max_level"];
                    break;
            }
            var tableData = Managers.Data.GetData(GameDataManager.DATA_TYPE.equipment_level);
            List<EquipLevel> datas = new List<EquipLevel>();
            foreach (EquipLevel item in tableData)
            {
                if (item.group_id == selectedEquip.equipData.group_id)
                    datas.Add(item);
            }
            var me = datas.Find(_ => _.level == (max_level - 1)).max_exp;
            if (addExp + selectedEquip.exp > me)
            {
                applyExp = me - selectedEquip.exp;
            }
            else
            {
                applyExp = addExp;
            }
        }
        selected_item_text.text = $"{StringManager.GetString("msg_equip_up")} ({simulList.Count}/{items.Count})";
        levelUpBtn_text.text = $"{EquipConfig.GetConfigDic()["exp_price"] * applyExp}";
        needGold = applyExp;
        return addExp;
    }
    //장비 경험치 슬라이더바
    void UpdateEquipLevelData()
    {
        int addExp = CalSelectedExp();

        var tableData = Managers.Data.GetData(GameDataManager.DATA_TYPE.equipment_level);
        List<EquipLevel> datas = new List<EquipLevel>();
        foreach (EquipLevel item in tableData)
        {
            if (item.group_id == selectedEquip.equipData.group_id)
                datas.Add(item);
        }
        var curExpData = datas.Find(_ => _.level == selectedEquip.lv);
        if (addExp == 0)
        {
            for (int i = 0; i < curExpData.equipLevelEffects.Count; i++)
            {
                double value = curExpData.equipLevelEffects[i].effect_value;
                float f = Convert.ToSingle(value);

                string st = string.Empty;
                if (curExpData.equipLevelEffects[i].effect_text_type == 2)
                    st = (f * 0.1).ToString() + "%";
                else if (curExpData.equipLevelEffects[i].effect_text_type == 3)
                    st = (f * 0.001).ToString();
                else
                    st = f.ToString();

                Detail_equipOption_value[i].text = st;
            }
        }

        EquipLevel preExpData = null;
        EquipLevel maxData = null;
        foreach (var item in datas)
        {
            if (maxData == null)
                maxData = item;
            if (maxData.level < item.level)
                maxData = item;

            if (item.max_exp <= selectedEquip.exp + addExp)
                continue;
            else if (item.max_exp > selectedEquip.exp + addExp)
            {
                preExpData = item;
                break;
            }
        }

        if (preExpData == null && addExp > 0)//만렙
        {
            preExpData = maxData;
        }

        int startExp = 0;
        EquipLevel target = curExpData;
        if (preExpData != null && preExpData != curExpData)
        {
            target = preExpData;
        }

        EquipLevel pivot = datas.Find(_ => _.level == (target.level - 1));
        if (pivot != null)
        {
            startExp = pivot.max_exp;
        }


        slider.maxValue = target.max_exp - startExp;
        slider.value = (selectedEquip.exp + addExp) - startExp;
        slider_text.text = $"{(int)slider.value} / {(int)slider.maxValue}";

        var maxLv = 0;
        switch (selectedEquip.equipData.grade)
        {
            case 1:
                maxLv = EquipConfig.Config["equipment_c_max_level"];
                break;
            case 2:
                maxLv = EquipConfig.Config["equipment_b_max_level"];
                break;
            case 3:
                maxLv = EquipConfig.Config["equipment_a_max_level"];
                break;
            case 4:
                maxLv = EquipConfig.Config["equipment_s_max_level"];
                break;
        }

        if (target.level >= maxLv)
        {
            slider.maxValue = 1.0f;
            slider.value = 1.0f;
            slider_text.text = "MAX";

            preExpData = datas.Find(_ => _.level == maxLv);
        }

        UpdateEquipOptionValue(curExpData, preExpData);
    }

    void UpdateEquipOptionValue(EquipLevel cur, EquipLevel prev)
    {
        if (prev == null && cur.max_exp == 0)
        {
            foreach (var item in Detail_equipOption_addValue)
            {
                item.text = "MAX";
            }

            Detail_EquipLV.text = "LV." + cur.level.ToString();
            Detail_EquipLV.color = Color.white;
            Detail_equipName.color = Color.white;
        }
        else if (prev == null)
        {
            foreach (var item in Detail_equipOption_addValue)
            {
                item.text = "0";
            }

            Detail_EquipLV.text = "LV." + cur.level.ToString();
            Detail_EquipLV.color = Color.white;
            Detail_equipName.color = Color.white;
        }
        else
        {
            double[] diff = new double[4];

            for (int i = 0; i < prev.equipLevelEffects.Count; i++)
            {
                if (selectedEquip.equipData.grade <= i)
                {
                    diff[i] = 0.0f;
                }
                else
                {
                    diff[i] = prev.equipLevelEffects[i].effect_value;// - cur.equipLevelEffects[i].effect_value;
                }
                if (prev.equipLevelEffects[i].effect_text_type == 2)
                    diff[i] = diff[i] * 0.1f;
                else if (prev.equipLevelEffects[i].effect_text_type == 3)
                    diff[i] = diff[i] * 0.001;

                Detail_equipOption_addValue[i].text = Convert.ToSingle(diff[i]).ToString();

                var unitText = string.Empty;
                if (prev.equipLevelEffects[i].effect_text_type == 2)
                    unitText = "%";

                Detail_equipOption_addValue[i].text += unitText;
            }

            Detail_EquipLV.text = "LV." + prev.level.ToString();
            Detail_EquipLV.color = Color.green;
            Detail_equipName.color = Color.green;
        }
    }

    void Clear()
    {
        foreach (Transform tr in equipSample.transform.parent)
        {
            if (equipSample.transform == tr)
                continue;

            Destroy(tr.gameObject);
        }
        items.Clear();
        selectedLists.Clear();
    }

    public bool UseExpItem(UserEquipData addEquip = null)
    {
        var tableData = Managers.Data.GetData(GameDataManager.DATA_TYPE.equipment_level);
        List<EquipLevel> datas = new List<EquipLevel>();
        foreach (EquipLevel item in tableData)
        {
            if (item.group_id == selectedEquip.equipData.group_id)
                datas.Add(item);
        }

        var maxLv = 0;
        switch (selectedEquip.equipData.grade)
        {
            case 1:
                maxLv = EquipConfig.Config["equipment_c_max_level"];
                break;
            case 2:
                maxLv = EquipConfig.Config["equipment_b_max_level"];
                break;
            case 3:
                maxLv = EquipConfig.Config["equipment_a_max_level"];
                break;
            case 4:
                maxLv = EquipConfig.Config["equipment_s_max_level"];
                break;
        }

        var maxleveldata = datas.Find(_ => _.level == maxLv);
        //내 선택한 장비가 재료 투입 불가능
        if (selectedEquip.lv >= maxleveldata.level)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_equip_max_lv"));
            return false;
        }

        maxleveldata = datas.Find(_ => _.level == (maxLv - 1));
        if (maxleveldata.max_exp <= (selectedEquip.exp + CalSelectedExp(addEquip)))
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_equip_max_exp"));
            return false;
        }

        return true;
    }

    public void DeScendingBtn()
    {
        isDescending = !isDescending;

        EquipLevelRefreshUI();
    }

    public void SelectedEquipList(UserEquipData info)
    {
        if (!selectedLists.Contains(info))
            selectedLists.Add(info);
        else
            selectedLists.Remove(info);

        UpdateEquipLevelData();
    }

    public void LevelUpBtn()
    {
        if (selectedLists.Count <= 0)
            return;

        CalSelectedExp();
        int gold = needGold;// * EquipConfig.Config["exp_price"];
        if (gold > Managers.UserData.MyGold)
        {
            PopupCanvas.Instance.ShowFadeText("골드부족");
            return;
        }

        PopupCanvas.Instance.ShowConfirmPopup("msg_equip_lvup", () =>
        {
            var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EQUIPMENT_POPUP) as EquipmentPopup;
            List<int> list = new List<int>();
            foreach (var item in selectedLists)
            {
                list.Add(item.id);
            }

            popup.EquipItemLevelUp(selectedEquip.id, list, (res) =>
            {
                EquipLevelRefreshUI();
                fx_equip_front00.Play();
                fx_equip_light00.Play();
            });
        });
    }

    public void SelectedListClearBtn()
    {
        foreach (var item in items)
        {
            item.SetFocus(false);
        }
        selectedLists.Clear();
        UpdateEquipLevelData();
    }

    public void AutoLevelUpBtn()
    {
        StartCoroutine(coAutoLevelUpBtn());
    }
    IEnumerator coAutoLevelUpBtn()
    {
        SelectedListClearBtn();

        var candidateItems = new List<UIbundleEquip>(items);
        candidateItems.Sort((x, y) =>
        {
            int xe = x.info.exp;
            int ye = y.info.exp;
            if (xe != ye)
            {
                return ye.CompareTo(xe);
            }
            return y.info.equipData.grade.CompareTo(x.info.equipData.grade);
        });

        var SelectedItem = new List<UIbundleEquip>();
        foreach (var item in candidateItems)
        {
            if (item.isSelected)
                continue;

            if (!item.OnPointerUpSelect())
                break;

            SelectedItem.Add(item);
        }

        if (SelectedItem.Count > 0)
        {
            var lastItem = SelectedItem.Last();
            if (lastItem.isSelected)
            {
                SelectedItem.Remove(lastItem);
                lastItem.OnPointerUpSelect();
            }
        }

        candidateItems.Reverse();
        var revItems = new List<UIbundleEquip>();
        foreach (var item in candidateItems)
        {
            if (SelectedItem.Contains(item))
                continue;

            if (!item.OnPointerUpSelect())
                break;

            SelectedItem.Add(item);
            revItems.Add(item);
        }

        var tableData = Managers.Data.GetData(GameDataManager.DATA_TYPE.equipment_level);
        List<EquipLevel> datas = new List<EquipLevel>();
        foreach (EquipLevel item in tableData)
        {
            if (item.group_id == selectedEquip.equipData.group_id)
                datas.Add(item);
        }

        var maxLv = 0;
        switch (selectedEquip.equipData.grade)
        {
            case 1:
                maxLv = EquipConfig.Config["equipment_c_max_level"];
                break;
            case 2:
                maxLv = EquipConfig.Config["equipment_b_max_level"];
                break;
            case 3:
                maxLv = EquipConfig.Config["equipment_a_max_level"];
                break;
            case 4:
                maxLv = EquipConfig.Config["equipment_s_max_level"];
                break;
        }

        var maxleveldata = datas.Find(_ => _.level == (maxLv - 1));
        //최대한 합리적으로 아이템 입력
        if (maxleveldata.max_exp < (selectedEquip.exp + CalSelectedExp()) && revItems.Count > 0)
        {
            UIbundleEquip delItem = null;

            while (maxleveldata.max_exp < (selectedEquip.exp + CalSelectedExp()))
            {
                var firstItem = revItems.First();
                if (firstItem.isSelected)
                {
                    SelectedItem.Remove(firstItem);
                    revItems.Remove(firstItem);

                    firstItem.OnPointerUpSelect();
                    delItem = firstItem;
                }
            }

            if (delItem != null)
            {
                if (!delItem.isSelected)
                {
                    delItem.OnPointerUpSelect();
                    SelectedItem.Add(delItem);
                }
            }
        }

        yield return null;
    }
}
