using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentPopup : Popup
{
    public delegate void SuccessCallback(JToken body);

    public enum EquipPopupType
    {
        List,
        LevelUp,
        Echant,
    }
    public EquipPopupType curType = EquipPopupType.List;
    [SerializeField] InventoryEquip popup_List;
    [SerializeField] InventoryEquipLevelUp popup_Level;
    [SerializeField] InventoryEquipEnchant popup_Echant;

    private int CharacterNo;
    private bool subPopupOn = false;

    public override void Open(CloseCallback cb = null)
    {
        curType = EquipPopupType.List;

        base.Open(cb);

        subPopupOn = false;

        popup_Level.Init(popup_List);
        popup_Echant.Init(popup_List);
    }
    public override void Close()
    {
        if (subPopupOn)
        {
            curType = EquipPopupType.List;
            RefreshUI();
            subPopupOn = false;
            return;
        }

        var LobbyScene = Managers.Scene.CurrentScene as LobbyScene;

        if (LobbyScene != null)
        {
            LobbyScene.RefreshUI();
        }

        base.Close();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        popup_List.SetActive(false);
        popup_Level.Close();
        popup_Echant.Close();
        switch (curType)
        {
            case EquipPopupType.List:
                popup_List.SetActive(true);
                popup_List.RefreshUI();
                break;
            case EquipPopupType.LevelUp:
                popup_Level.Show();
                break;
            case EquipPopupType.Echant:
                popup_Echant.Show();
                break;
            default:
                break;
        }
    }
    public void SetSubPopupFlag(bool isOn)
    {
        subPopupOn = isOn;
    }

    public void SetSelectedCharacterNo(int no)
    {
        CharacterNo = no;
        UserEquipData equipData = Managers.UserData.MyCharacters[CharacterNo].curEquip;
        popup_List.SetSelectedEquipInfo(equipData);
    }
    public int GetSelectedCharacterNo()
    {
        return CharacterNo;
    }

    public void SetEquipItem(int character_no, int equip_no, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("character_no", character_no);
        param.AddField("equip_no", equip_no);

        SBWeb.SendPost("equipment/equip_character", param, (response) =>
        {
            if (SBWeb.IsResultOK(response))
            {
                JToken res = SBWeb.GetResultData(response);
                Managers.UserData.UpdateMyCharacter(res["characters"]);
                Managers.UserData.SetMyEquipItems(res["equipment"]);

                cb?.Invoke(response);
            }
        });
    }

    public void EquipItemLevelUp(int equip_no, List<int> resources, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("equip_no", equip_no);
        param.AddField("resources", string.Join(",", resources));

        SBWeb.SendPost("equipment/equip_exp", param, (response) =>
        {
            if (SBWeb.IsResultOK(response))
            {
                JToken res = SBWeb.GetResultData(response);
                Managers.UserData.SetMyUserInfo(res["user"]);
                Managers.UserData.SetMyEquipItems(res["equipment"]);
                Managers.UserData.UpdateMyCharacter(res["characters"]);
                cb?.Invoke(response);
            }
        });

    }

    public void EquipItemEchant(int equip_no, int resource, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("equip_no", equip_no);
        param.AddField("resource", resource);

        SBWeb.SendPost("equipment/equip_upgrade", param, (response) =>
        {
            if (SBWeb.IsResultOK(response))
            {
                JToken res = SBWeb.GetResultData(response);

                Managers.UserData.SetMyUserInfo(res["user"]);
                Managers.UserData.SetMyEquipItems(res["equipment"]);
                Managers.UserData.UpdateMyCharacter(res["characters"]);
                
                cb?.Invoke(response);
            }
        });

    }
}
