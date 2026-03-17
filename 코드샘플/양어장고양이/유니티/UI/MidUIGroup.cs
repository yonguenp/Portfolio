using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MidUIGroup : UIGroup
{
    public enum MID_UI { CHAT, ALBUM, GACHA, SHOP };

    //chat control
    public Button chatButton;
    //album control
    public Button albumButton;
    //picture control
    public Button photoButton;
    //shop control
    public Button shopButton;

    public void Awake()
    {
        chatButton.onClick.AddListener(OnChatButton);
        albumButton.onClick.AddListener(OnAlbumButton);
        photoButton.onClick.AddListener(OnPhotoButton);
        shopButton.onClick.AddListener(OnShopButton);
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

    public void OnChatButton()
    {
        FarmUIPanel.FarmCanvas.GameManager.PopupControl.OnPopupMessage("준비중");
    }
    public void OnAlbumButton()
    {
        FarmUIPanel.FarmCanvas.GameManager.SetState(GameMain.HahahaState.HAHAHA_CARD);
    }
    public void OnPhotoButton()
    {
        FarmUIPanel.FarmCanvas.GameManager.SetState(GameMain.HahahaState.HAHAHA_PHOTO);
    }
    public void OnShopButton()
    {
        FarmUIPanel.FarmCanvas.GameManager.PopupControl.OnPopupMessage("준비중");        
    }
}
