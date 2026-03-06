using SBSocketSharedLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DuoPopup : Popup
{
    [SerializeField]
    DuoList DuoEnableListUI;
    [SerializeField]
    Button RefreshButton;
    private void OnEnable()
    {
        DuoEnableListUI.RefreshUI();

        RefreshButton.interactable = false;
        Invoke("OnRefreshButtonActive", 3.0f);
    }

    private void OnDisable()
    {

    }

    public void OnToggleMenu()
    {

    }


    public void OnExitButton()
    {
        Close();
    }

    public void OnRefresh()
    {
        DuoEnableListUI.RefreshUI();

        RefreshButton.interactable = false;

        Invoke("OnRefreshButtonActive", 3.0f);
    }

    void OnRefreshButtonActive()
    {
        CancelInvoke("OnRefreshButtonActive");

        RefreshButton.interactable = true;
    }

    public void SetCandidateDuo(IList<FriendInfo> friendInfos)
    {
        DuoEnableListUI.SetCandidateDuo(friendInfos);
    }
}
