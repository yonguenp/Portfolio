using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChampionPetTabLayer : PetTabLayer
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
            parentPopup.SetPetTag(value);
        }
    }
}
