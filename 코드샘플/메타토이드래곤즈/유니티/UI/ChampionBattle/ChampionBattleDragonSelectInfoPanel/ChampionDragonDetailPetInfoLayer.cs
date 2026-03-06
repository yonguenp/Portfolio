using Google.Impl;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChampionDragonDetailPetInfoLayer : PetInformationLayer
{
    public ChampionDragonDetailPopup parentPopup { get { return PopupManager.GetPopup<ChampionDragonDetailPopup>(); } }
    public override UserPetData GetPetInfo()
    {
        return parentPopup.PetData;
    }

    public override UserDragonData GetDragonInfo()
    {
        return parentPopup.Dragons;
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
    }

    public override void OnClickBackButton()
    {
        HidePetInfoPanel();
    }
    public void ShowPetInfoPanel()
    {
        gameObject.SetActive(true);
    }

    public void HidePetInfoPanel()
    {
        gameObject.SetActive(false);
    }

    public bool IsActive()
    {
        if (gameObject.activeInHierarchy)
        {
            return true;
        }
        return false;
    }
}
