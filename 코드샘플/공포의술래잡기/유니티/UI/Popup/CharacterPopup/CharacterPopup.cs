using SBSocketSharedLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPopup : Popup, EventListener<NotifyEvent>
{
    [SerializeField]
    CharacterStatUI StatUI;
    [SerializeField]
    GameObject ParticleEffect;
    enum CHARACTER_UI
    {
        NONE,
        CHARACTER_STAT,
        CHARACTER_CHANGE
    };

    CHARACTER_UI curUIType = CHARACTER_UI.NONE;

    private void OnEnable()
    {
        HideParticle();
        this.EventStartListening();
    }

    private void OnDisable()
    {
        this.EventStopListening();
    }

    public void OnEvent(NotifyEvent eventType)
    {
        switch (eventType.Message)
        {
            case NotifyEvent.NotifyEventMessage.ON_USER_INFO:
                {
                    RefreshUI();
                }
                break;
            case NotifyEvent.NotifyEventMessage.ON_CHARACTER_UPDATE:
                {
                    RefreshUI();
                }
                break;
        }
    }

    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);
        var curScene = Managers.Scene.CurrentScene;
        curUIType = CHARACTER_UI.CHARACTER_STAT;

        StatUI.SubPopupSetActive(false);
        var lobby = Managers.Scene.CurrentScene as LobbyScene;
        if (lobby)
            lobby.OnRedDot(lobby.lobbyBtns[0].transform, false);


        RefreshUI();
    }
    public override void Close()
    {
        base.Close();
        curUIType = CHARACTER_UI.NONE;
    }
    public override void RefreshUI()
    {
        StatUI.SetActive(true);
        StatUI.RefreshUI();
    }

    public void OnSelectCharacter(CharacterGameData data)
    {
        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("매치대기캐릭터변경불가"));
            return;
        }

        if (data.IsChaserCharacter())
        {
            if (Managers.UserData.MyDefaultChaserCharacter != data.GetID())
                SBWeb.SetDefaultChaserCharacter(data.GetID(), () =>
                {
                    StatUI.OnSelectCharacter(data);
                    if (Managers.FriendData.DUO.IsDuoPlaying())
                    {
                        Managers.Network.SendDuoCharacterChange(data.char_type, data.GetID());
                        if (Managers.FriendData.DUO.IsHost())
                        {
                            Managers.FriendData.DUO.Host.ChaserUID = data.GetID();
                        }
                        else
                        {
                            Managers.FriendData.DUO.Guest.ChaserUID = data.GetID();
                        }

                        Managers.FriendData.DUO.OnRefershDuoUI();
                    }
                    else if (Managers.FriendData.DUO.IsHost())
                    {
                        Managers.FriendData.DUO.Host.ChaserUID = data.GetID();
                        Managers.FriendData.DUO.OnRefershDuoUI();
                    }
                });
            else
                StatUI.OnSelectCharacter(data);
        }
        else
        {
            if (Managers.UserData.MyDefaultSurvivorCharacter != data.GetID())
                SBWeb.SetDefaultSuvivorCharacter(data.GetID(), () =>
                {
                    StatUI.OnSelectCharacter(data);
                    if (Managers.FriendData.DUO.IsDuoPlaying())
                    {
                        Managers.Network.SendDuoCharacterChange(data.char_type, data.GetID());
                        if (Managers.FriendData.DUO.IsHost())
                        {
                            Managers.FriendData.DUO.Host.SurvivorUID = data.GetID();
                        }
                        else
                        {
                            Managers.FriendData.DUO.Guest.SurvivorUID = data.GetID();
                        }

                        Managers.FriendData.DUO.OnRefershDuoUI();
                    }
                    else if (Managers.FriendData.DUO.IsHost())
                    {
                        Managers.FriendData.DUO.Host.SurvivorUID = data.GetID();
                        Managers.FriendData.DUO.OnRefershDuoUI();
                    }

                });
            else
                StatUI.OnSelectCharacter(data);
        }
    }

    public void OnCharacterStatsMenu()
    {
        curUIType = CHARACTER_UI.CHARACTER_STAT;
        RefreshUI();
    }

    public void OnChaserUI()
    {
        StatUI.OnChaserUI();
    }

    public void OnSuvivorUI()
    {
        StatUI.OnSuvivorUI();
    }

    public void OnParticleEffect(Transform posTarget, float time)
    {
        CancelInvoke("ShowParticle");
        HideParticle();

        ParticleEffect.transform.position = posTarget.position;

        Invoke("ShowParticle", time);
    }

    void ShowParticle()
    {
        CancelInvoke("HideParticle");
        ParticleEffect.SetActive(true);

        Invoke("HideParticle", 2.0f);
    }

    public void HideParticle()
    {
        CancelInvoke("ShowParticle");
        ParticleEffect.SetActive(false);
    }
    public CharacterStatUI GetUI()
    {
        return StatUI;
    }

    public void OpenEquipmentPopup()
    {
        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("매치대기캐릭터변경불가"));
            return;
        }

        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.EQUIPMENT_POPUP);
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EQUIPMENT_POPUP) as EquipmentPopup;
        popup.SetSelectedCharacterNo(GetUI().GetSelectedCharacterID());
    }
}
