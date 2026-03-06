using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChampionPetInformationLayer : PetInformationLayer
{
    public ChampionBattleDragonSelectPopup parentPopup { get { return PopupManager.GetPopup<ChampionBattleDragonSelectPopup>(); } }
    public override UserPetData GetPetInfo()
    {
        return parentPopup.PetData;
    }

    public override UserDragonData GetDragonInfo()
    {
        return ChampionManager.Instance.MyInfo.ChampionDragons;
    }

    public override int DragonTagInfo
    {
        get
        {
            return parentPopup.DragonTag;
        }
    }

    public override int PetTagInfo
    {
        get
        {
            return parentPopup.PetTag;
        }
        set
        {
            parentPopup.SetPartTag(value);
        }
    }

    public override void OnClickBackButton()
    {
        if (DragonTagInfo != 0)//드래곤 태그값
        {
            var dragonManagePopup = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>();
            if (dragonManagePopup != null)
            {
                dragonManagePopup.moveTab(new ChampionBattleDragonTabTypePopupData(0, 3));
            }
        }
        else
        {
            PopupManager.ClosePopup<ChampionBattleDragonSelectPopup>();
        }
    }
}
