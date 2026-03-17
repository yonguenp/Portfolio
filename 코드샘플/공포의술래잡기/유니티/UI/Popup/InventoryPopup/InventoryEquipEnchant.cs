using Coffee.UIExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryEquipEnchant : MonoBehaviour
{
    InventoryPanel parent = null;

    [SerializeField] UIbundleEquip equipSample;
    [SerializeField] GameObject upBtn;
    [SerializeField] GameObject downBtn;
    [SerializeField] Dropdown dropdown;
    [SerializeField] string[] optionKey;
    [SerializeField] Text selected_item_text;

    [Header("[장비 상세]")]
    [SerializeField] UIbundleEquip curEquip;
    [SerializeField] UIbundleEquip sacriEquip;
    [SerializeField] UIbundleEquip upEquip;

    [Header("[버 튼]")]
    [SerializeField] Image echantBtn_icon;
    [SerializeField] Text echantBtn_text;
    [SerializeField] Button initBtn;
    [SerializeField] Button echantBtn;


    int curSort = 0;
    bool isDescending = true;
    UserEquipData selectedEquip;
    List<UIbundleEquip> items = new List<UIbundleEquip>();

    private UserEquipData sacriEquipData = null;

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
        EquipEchantRefreshUI();
    }

    public void EquipEchantRefreshUI()
    {
        if (selectedEquip == null)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_equip_popup_error"));
            return;
        }

        Clear();

        Dictionary<int, UserEquipData> equipDatas = new Dictionary<int, UserEquipData>(Managers.UserData.MyEquips);

        foreach (var equip in equipDatas)
        {
            if (equip.Value.id == selectedEquip.id)
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
            //장비 등급이 다르거나 장비 렙이 낮으면 제외
            if (data.Value.equipData.grade != selectedEquip.equipData.grade)
                continue;
            if (data.Value.lv < selectedEquip.lv)
                continue;

            var equip = GameObject.Instantiate(equipSample, equipSample.transform.parent);
            equip.Init(data.Value, this);
            equip.SetEquipMark(false, false);

            items.Add(equip);
        }
        equipSample.gameObject.SetActive(false);

        SetUpEquipWindow();

        SetEchantBtnPrice();
    }

    public void SetUpEquipWindow()
    {
        //장비 창 왼쪽 번들장비 세팅
        curEquip.Init(selectedEquip);
        curEquip.SetEquipMark(false);
        foreach (var item in Managers.UserData.MyCharacters)
        {
            if (item.Value.curEquip != null)
            {
                if (item.Value.curEquip == selectedEquip)
                {
                    curEquip.SetEquipMark(true);
                    break;
                }
            }
        }
        sacriEquip.MissingEquip();
        upEquip.MissingEquip();
    }

    public void SetEchantBtnPrice()
    {
        var tableData = Managers.Data.GetData(GameDataManager.DATA_TYPE.equipment_reinforce);

        var data = tableData.Find(_ => _.GetID() == selectedEquip.equipData.GetID()) as EquipReinforce;
        if (data == null)
            return;

        bool isable = false;

        string icon_resourcePath = string.Empty;
        int param = 0;
        switch (data.price_type)
        {
            case 1:
                icon_resourcePath = "Texture/UI/Lobby/Icon_gold";

                isable = Managers.UserData.MyGold < data.price_amount;
                break;
            case 2:
                icon_resourcePath = "Texture/UI/Lobby/icon_gem";

                isable = Managers.UserData.MyDia < data.price_amount;
                break;
            case 3:
                param = data.price_param;
                echantBtn_icon.sprite = ItemGameData.GetItemData(param).sprite;

                isable = Managers.UserData.GetMyItemCount(param) < data.price_amount;
                break;
            case 4:
                param = data.price_param;
                echantBtn_icon.sprite = CharacterGameData.GetCharacterData(param).sprite_ui_resource;
                break;
            case 5:
                break;
            case 6:
                break;
            case 7:
                break;
            case 8:
                break;
            case 9:
                break;
        }

        if (icon_resourcePath != string.Empty)
            echantBtn_icon.sprite = Managers.Resource.Load<Sprite>(icon_resourcePath);
        echantBtn_text.text = data.price_amount.ToString();


        if (isable)
        {
            echantBtn_text.color = Color.red;
            echantBtn.interactable = false;
        }
        else
        {
            echantBtn_text.color = Color.white;
            echantBtn.interactable = true;
        }

    }
    public void SelectEchantEquip(UserEquipData se)
    {
        if (sacriEquipData != null && sacriEquipData != se)
        {
            var target = items.Find(_ => _.info == sacriEquipData);
            if(target != null)
                target.SetFocus(false);
        }
        if (se == null)
        {
            sacriEquipData = null;
            sacriEquip.MissingEquip();
            upEquip.MissingEquip();
            return;
        }

        sacriEquipData = se;
        sacriEquip.Init(sacriEquipData);

        var tableData = Managers.Data.GetData(GameDataManager.DATA_TYPE.equipment_reinforce);

        var data = tableData.Find(_ => _.GetID() == selectedEquip.equipData.GetID()) as EquipReinforce;
        upEquip.Init(EquipInfo.GetEquipData(data.reinforce_next_equip));
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
    public void DeScendingBtn()
    {
        isDescending = !isDescending;
        EquipEchantRefreshUI();
    }

    public void InitBtn()
    {
        SelectEchantEquip(null);
    }
    public void EchantBtn()
    {
        if (sacriEquipData == null)
            return;

        var prior = selectedEquip;
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EQUIPMENT_POPUP) as EquipmentPopup;
        popup.EquipItemEchant(selectedEquip.id, sacriEquipData.id, (res) =>
        {
            var cur = Managers.UserData.MyEquips[selectedEquip.id];
            EquipEchantRefreshUI();

            PopupCanvas.Instance.ShowEquipEchantResult(prior, cur);
            //resultUI.SetActive(true);
            //resultUI.GetComponent<InventoryEchantResult>().Init(prior, cur);
            //fx_equip_bulidup00.Play();
        });

    }
}
