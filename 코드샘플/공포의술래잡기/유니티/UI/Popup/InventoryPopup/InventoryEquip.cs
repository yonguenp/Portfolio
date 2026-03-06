using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryEquip : InventoryPanel
{
    public enum sortType
    {
        GRADE,
        LEVEL,
        GAIN,
    }
    [SerializeField] GameObject equipInventoryUI;

    [SerializeField] UIbundleEquip equipSample;
    [SerializeField] Dropdown dropdown;
    [SerializeField] string[] optionKey;
    [SerializeField] Text myItemList_text;
    [SerializeField] GameObject upBtn;
    [SerializeField] GameObject downBtn;

    [Header("[장비 상세정보]")]
    [SerializeField] UIbundleEquip Detail_equip;
    [SerializeField] Text Detail_equipName;
    [SerializeField] Text[] Detail_equipOption;
    [SerializeField] Text[] equoipOption_value;

    [SerializeField] GameObject notSelected;
    [SerializeField] GameObject detail;
    [SerializeField] GameObject[] optionDim;

    [Header("[장비 서브팝업]")]
    [SerializeField] InventoryEquipLevelUp levelupMenu;
    [SerializeField] InventoryEquipEnchant enchantMenu;


    [Header("[장비팝업 그룹]")]
    [SerializeField] Button equip_Btn;
    [SerializeField] Button equip_Lock;
    [SerializeField] Button equip_sell_btn;
    [SerializeField] Button equip_all_sell_c;
    [SerializeField] Button equip_all_sell_b;
    [SerializeField] Button equip_all_sell_a;
    [SerializeField] GameObject equip_all_sell_btn;



    int curSort = 0;
    bool isDescending = true;
    List<UIbundleEquip> items = new List<UIbundleEquip>();

    bool isOverrided = false;
    public UserEquipData curSelectedEquipData { get; private set; } = null;
    public bool isSell = false;
    private void Start()
    {
        dropdown.options.Clear();
        foreach (var item in optionKey)
        {
            dropdown.options.Add(new Dropdown.OptionData(StringManager.GetString(item), Managers.Resource.Load<Sprite>("Texture/UI/friend/btn_sub_ivory_01")));
        }


        levelupMenu.Init(this);
        enchantMenu.Init(this);
    }
    public override void RefreshUI()
    {
        CancelInvoke("RefreshUI");

        Clear();

        var equipDatas = Managers.UserData.MyEquips;
        if (curSelectedEquipData != null)
        {
            if (equipDatas.ContainsKey(curSelectedEquipData.id))
            {
                curSelectedEquipData = equipDatas[curSelectedEquipData.id];
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

        bool hasCurSelect = false;

        foreach (var data in equipDatas)
        {
            equipSample.gameObject.SetActive(true);
            var equip = GameObject.Instantiate(equipSample, equipSample.transform.parent);
            equip.Init(data.Value, this);
            equip.SetEquipMark(false);

            items.Add(equip);

            if (curSelectedEquipData == data.Value)
                hasCurSelect = true;
        }
        equipSample.gameObject.SetActive(false);

        if (Managers.UserData.MyCharacters != null)
        {
            int charID = 0;
            //캐릭터 창에서 장비 열때
            if (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP).IsOpening())
            {
                charID = (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP) as CharacterPopup).GetUI().GetSelectedCharacterID();
            }

            foreach (UIbundleEquip item in items)
            {
                foreach (var userCharacters in Managers.UserData.MyCharacters)
                {
                    if (userCharacters.Value.curEquip != null)
                    {
                        if (item.info == userCharacters.Value.curEquip)
                        {
                            item.SetEquipMark(true);
                            break;
                        }
                    }
                }
            }
        }
        if (!hasCurSelect)
            curSelectedEquipData = null;
        SetSelectedEquipInfo(curSelectedEquipData);

        //장비 판매 버튼 
        if (equip_sell_btn != null)
            equip_sell_btn.gameObject.SetActive(PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP).IsOpening());
        if (equip_all_sell_c != null)
            equip_all_sell_c.gameObject.SetActive(PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP).IsOpening());
        if (equip_all_sell_b != null)
            equip_all_sell_b.gameObject.SetActive(PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP).IsOpening());
        if (equip_all_sell_a != null)
            equip_all_sell_a.gameObject.SetActive(PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP).IsOpening());
        if(equip_all_sell_btn != null)
            equip_all_sell_btn.SetActive(PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP).IsOpening());


        //장비 삭제 
        List<UIbundleEquip> removeEquipList = new List<UIbundleEquip>();
        if (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP).IsOpening())
        {
            foreach (var item in items)
            {
                //장비 잠금 // 착용   
                if (item.GetEquipLock() || item.GetEquipSet())
                {
                    removeEquipList.Add(item);
                }
            }

            foreach (var item in removeEquipList)
            {
                Destroy(item.gameObject);
                items.Remove(item);
            }
        }

        myItemList_text.text = StringManager.GetString("보유 장비 리스트") + $"({items.Count} / {EquipConfig.Config["equip_max"]})";
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
        levelupMenu.Close();
        enchantMenu.Close();
    }

    public void SetSelectedEquipInfo(UserEquipData equipInfo)
    {
        curSelectedEquipData = equipInfo;

        notSelected.SetActive(equipInfo == null);
        Detail_equip.gameObject.SetActive(equipInfo != null);
        detail.SetActive(equipInfo != null);
        isOverrided = false;

        foreach (UIbundleEquip item in items)
        {
            item.SetFocus(equipInfo == item.info);
        }
        if (equipInfo != null)
        {
            Detail_equip.Init(equipInfo);
            Detail_equip.SetEquipMark(false);
            //장비 착용관련
            if (equip_Btn != null)
            {
                equip_Btn.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("Texture/UI/equip/btn_sub_blue_04");
                equip_Btn.GetComponentInChildren<Text>().text = StringManager.GetString("장비장착");
            }
            //캐릭터 창에서 장비 열때
            if (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP).IsOpening())
            {
                var equipPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EQUIPMENT_POPUP).GetComponent<EquipmentPopup>();
                int characterNo = equipPopup.GetSelectedCharacterNo();

                if (Managers.UserData.MyCharacters[characterNo].curEquip == equipInfo)
                {
                    Detail_equip.SetEquipMark(true);
                    if (equip_Btn != null)
                    {
                        equip_Btn.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("Texture/UI/ch_info/btn_sub_pink_01");
                        equip_Btn.GetComponentInChildren<Text>().text = StringManager.GetString("장비해제");
                    }
                }

                foreach (var userCharacters in Managers.UserData.MyCharacters)
                {
                    if (userCharacters.Value.curEquip != null)
                    {
                        if (equipInfo == userCharacters.Value.curEquip && userCharacters.Key != characterNo)
                        {
                            Detail_equip.SetEquipMark(true);
                            //장비 착용관련
                            if (equip_Btn != null)
                            {
                                equip_Btn.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("Texture/UI/equip/btn_sub_blue_04");
                                equip_Btn.GetComponentInChildren<Text>().text = StringManager.GetString("장비장착");
                                isOverrided = true;
                            }
                            break;
                        }
                    }
                }
            }
            //인벤토리에서의 경우
            else if (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.COMBINEINVENTORY_POPUP).IsOpening())
            {
                foreach (var userCharacters in Managers.UserData.MyCharacters)
                {
                    if (userCharacters.Value.curEquip != null)
                    {
                        if (equipInfo == userCharacters.Value.curEquip)
                        {
                            Detail_equip.SetEquipMark(true);
                            //장비 착용관련
                            if (equip_Btn != null)
                            {
                                equip_Btn.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("Texture/UI/equip/btn_sub_blue_04");
                                equip_Btn.GetComponentInChildren<Text>().text = StringManager.GetString("장비장착");
                                isOverrided = true;
                            }
                            break;
                        }
                    }
                }

            }

            //장비 잠금관련 UI
            if (equip_Lock != null)
            {
                if (CacheUserData.GetBoolean("Equip_Lock_" + curSelectedEquipData.id.ToString()))
                {
                    equip_Lock.GetComponentInChildren<Text>().text = StringManager.GetString("잠금해제");
                }
                else
                {
                    equip_Lock.GetComponentInChildren<Text>().text = StringManager.GetString("잠금");
                }
            }

            //장비 상세 옵션 딤처리
            int isDim = 0;
            foreach (var dim in optionDim)
            {
                dim.SetActive(isDim >= equipInfo.equipData.grade);
                isDim++;
            }

            Detail_equipName.text = equipInfo.equipData.itemData.GetName();

            string opDes = string.Empty;
            int index = 0;
            foreach (int optionNum in GetOptionData(equipInfo.equipData.group_id))
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


            var tableData = Managers.Data.GetData(GameDataManager.DATA_TYPE.equipment_level);
            List<EquipLevel> datas = new List<EquipLevel>();
            foreach (EquipLevel item in tableData)
            {
                if (item.group_id == equipInfo.equipData.group_id)
                    datas.Add(item);
            }
            var curExpData = datas.Find(_ => _.level == equipInfo.lv);
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

                equoipOption_value[i].text = st;
            }
        }
    }

    public int[] GetOptionData(int group_id)
    {
        var tableData = Managers.Data.GetData(GameDataManager.DATA_TYPE.equipment_level);

        List<EquipLevel> datas = new List<EquipLevel>();
        foreach (EquipLevel item in tableData)
        {
            if (item.group_id == group_id)
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
        return optionInt;
    }

    public void SortList()
    {
        RefreshUI();
    }

    public void DeScendingBtn()
    {
        isDescending = !isDescending;

        RefreshUI();
    }

    public void CloseSubPopup()
    {
        equipInventoryUI.SetActive(true);
        RefreshUI();
    }

    public bool UseState()
    {
        string key = string.Empty;
        switch (curSelectedEquipData.equipData.grade)
        {
            case 1:
                key = "equipment_c_max_level";
                break;
            case 2:
                key = "equipment_b_max_level";
                break;
            case 3:
                key = "equipment_a_max_level";
                break;
            case 4:
                key = "equipment_s_max_level";
                break;

        }
        bool isMax = EquipConfig.GetConfigDic()[key] <= curSelectedEquipData.lv;

        return isMax;
    }

    public void OnLevelUpMenu()
    {
        if (UseState())
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_equip_max_lv"));
            return;
        }

        Clear();
        equipInventoryUI.SetActive(false);
        levelupMenu.Show(curSelectedEquipData);

        if (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EQUIPMENT_POPUP).IsOpening())
        {
            var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EQUIPMENT_POPUP) as EquipmentPopup;
            popup.curType = EquipmentPopup.EquipPopupType.LevelUp;
            popup.SetSubPopupFlag(true);
        }

    }

    public void OnEnchantMenu()
    {
        if (!UseState() || curSelectedEquipData.equipData.grade >= 4)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_equip_max_reinforce"));
            return;
        }

        Clear();
        equipInventoryUI.SetActive(false);
        enchantMenu.Show(curSelectedEquipData);

        if (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EQUIPMENT_POPUP).IsOpening())
        {
            var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EQUIPMENT_POPUP) as EquipmentPopup;
            popup.curType = EquipmentPopup.EquipPopupType.Echant;
            popup.SetSubPopupFlag(true);
        }
    }

    public void OnLockButton()
    {
        if (curSelectedEquipData == null)
            return;

        Detail_equip.SetLock(!CacheUserData.GetBoolean("Equip_Lock_" + curSelectedEquipData.id.ToString(), false));
        RefreshUI();
    }

    public bool IsSubMenuOpening()
    {
        return levelupMenu.gameObject.activeInHierarchy || enchantMenu.gameObject.activeInHierarchy;
    }

    //장비팝업 버튼 함수
    public void SetEquipBtn()
    {
        if (isOverrided)
        {
            PopupCanvas.Instance.ShowConfirmPopup(StringManager.GetString("msg_equip_change"), StringManager.GetString("ui_equip_use"), StringManager.GetString("button_cancel"), () =>
            {
                //장착 ok 버튼 시 
                var equipPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EQUIPMENT_POPUP).GetComponent<EquipmentPopup>();
                int characterNo = equipPopup.GetSelectedCharacterNo();

                int equipNo = curSelectedEquipData.id;

                if (Managers.UserData.MyCharacters[characterNo].curEquip != null)
                {
                    if (curSelectedEquipData == Managers.UserData.MyCharacters[characterNo].curEquip)
                    {
                        equipNo = 0;
                    }
                }

                equipPopup.SetEquipItem(characterNo, equipNo, (res) =>
                {
                    if (equipNo != 0)
                        PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_equip_use"));
                    else
                        PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_equip_cancel"));
                    RefreshUI();
                }); ;
            }, null);
            return;
        }
        var equipPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EQUIPMENT_POPUP).GetComponent<EquipmentPopup>();
        int characterNo = equipPopup.GetSelectedCharacterNo();

        int equipNo = curSelectedEquipData.id;

        if (Managers.UserData.MyCharacters[characterNo].curEquip != null)
        {
            if (curSelectedEquipData == Managers.UserData.MyCharacters[characterNo].curEquip)
            {
                equipNo = 0;
            }
        }

        equipPopup.SetEquipItem(characterNo, equipNo, (res) =>
        {
            if (equipNo != 0)
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_equip_use"));
            else
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_equip_cancel"));
            RefreshUI();
        }); ;
    }

    public void SelectSellEquipBtn()
    {
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;

        if (popup.exchange_equipList.Count <= 0)
            return;
        isSell = true;
        PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.COMBINEINVENTORY_POPUP).Close();
    }

    public void AllSellSelectedGrade(int grade)
    {
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;

        popup.exchange_equipList.Clear();
        foreach (var item in items)
        {
            if (item.gameObject != null)
            {
                if (item.info.equipData.grade == grade)
                {
                    popup.exchange_equipList.Add(item);
                    //item.OnButton();
                }
            }
        }

        if (popup.exchange_equipList.Count <= 0)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("exchange_no_selected_equip"));
            return;
        }
        PopupCanvas.Instance.ShowFadeText(StringManager.GetString("exchange_selected_equip", popup.exchange_equipList.Count));

        isSell = true;
        PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.COMBINEINVENTORY_POPUP).Close();
    }

    public void SetCurSelectItem(UserEquipData data)
    {
        curSelectedEquipData = data;
    }
}
