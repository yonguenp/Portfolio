using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotUIGroup : UIGroup
{
    public enum BOT_UI { COOK, FISHING, CRAFT };

    //cook control
    public Button cookButton;
    public CookListUI CookListUI;
    //fising control
    public Button fisingButton;
    //craft control
    public Button craftButton;

    public void Awake()
    {
        cookButton.onClick.AddListener(OnCookButton);
        fisingButton.onClick.AddListener(OnFisingButton);
        craftButton.onClick.AddListener(OnCraftButton);
    }

    public override void SetUI(bool enable)
    {
        foreach (GameObject ui in UI)
        {
            ui.SetActive(enable);
        }
    }

    public override void Refresh()
    {

    }

    public void OnCookButton()
    {
        CookListUI.ShowCookList();
    }

    public void OnFisingButton()
    {
        FarmUIPanel.FarmCanvas.GameManager.PopupControl.OnPopupMessage("준비중");
    }

    public void OnCraftButton()
    {
        FarmUIPanel.FarmCanvas.GameManager.PopupControl.OnPopupMessage("준비중");
    }
}
