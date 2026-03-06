using SBSocketSharedLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendPopup : Popup
{
    public enum FriendsUI
    {
        NONE = -1,
        FRIENDS_LIST,
        FRIENDS_SEARCH,
        FRIENDS_CALL,
        BLOCK_USER,
        DUO_ENABLE_LIST,
    };

    public Toggle[] ToggleMenu;
    public FriendsUIObject[] UIPanel;
    public FriendsUI curUI = FriendsUI.NONE;
    public GameObject[] RedDot;

    [SerializeField]
    PrivateChat ChatUI = null;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }
    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);
        var scene = Managers.Scene.CurrentScene as LobbyScene;
        if(scene != null)
        {
            if ((scene as LobbyScene).lobbyBtns[4].transform.Find("RedDot") != null)
                (scene as LobbyScene).lobbyBtns[4].transform.Find("RedDot").gameObject.SetActive(false);
        }    
    }
    public override void RefreshUI()
    {
        Managers.Chat.SetAddMessageCallback(typeof(FriendPopup), ReceiveMessage);

        curUI = FriendsUI.NONE;
        Toggle target = ToggleMenu[(int)FriendsUI.FRIENDS_LIST];
        foreach (Toggle toggle in ToggleMenu)
        {
            toggle.isOn = toggle == target;
        }

        OnToggleMenu();
        OnFriendAlarmCheck();
    }

    public void OnToggleMenu()
    {
        int index = 0;
        for (int i = 0; i < ToggleMenu.Length; i++)
        {
            Toggle toggle = ToggleMenu[i];
            if (toggle.isOn)
            {
                index = i;
                if (RedDot[i] != null)
                {
                    RedDot[i].SetActive(false);
                    UIPanel[i].NewFlagDone();
                }

                var txtObj = toggle.transform.Find("Text");
                if(txtObj != null)
                {
                    var txtName = txtObj.GetComponent<Text>();
                    if(txtName != null)
                    {
                        txtName.color = Color.black;
                    }
                }
            }
            else
            {
                var txtObj = toggle.transform.Find("Text");
                if(txtObj != null)
                {
                    var txtName = txtObj.GetComponent<Text>();
                    if(txtName != null)
                    {
                        txtName.color = Color.white;
                    }
                }
            }
        }

        OnMenuChange((FriendsUI)index);
    }

    public void OnMenuChange(FriendsUI ui)
    {
        if (ui == curUI)
            return;

        curUI = ui;
        for (int i = 0; i < UIPanel.Length; i++)
        {
            UIPanel[i].OnFriendsStateChange(curUI);
        }

        //WWWForm data = new WWWForm();
        //data.AddField("api", "friend");
        //data.AddField("op", 12);

        //SBWeb.SendPost("friend/friend", data, (response) =>
        //{
        //    Invoke("OnFriendAlarmCheck", 0.1f);
        //});
    }

    public override void Close()
    {
        Managers.Chat.RemoveAddMessageCallback(typeof(FriendPopup));

        for (int i = 0; i < UIPanel.Length; i++)
        {
            UIPanel[i].HideHI();
        }

        var scene = Managers.Scene.CurrentScene as LobbyScene;
        if (scene != null)
        {
            if ((scene as LobbyScene).lobbyBtns[4].transform.Find("RedDot") != null)
                (scene as LobbyScene).lobbyBtns[4].transform.Find("RedDot").gameObject.SetActive(false);
        }

        base.Close();
    }

    public void OnFriendAlarmCheck()
    {
        for (int i = 0; i < RedDot.Length; i++)
        {
            if (RedDot[i] != null)
                RedDot[i].SetActive(UIPanel[i].IsNewFlag());
        }
    }

    public void ShowErrorMessage(int rs)
    {
        switch (rs)
        {
            case 0:
                break;
            case 1:
                PopupCanvas.Instance.ShowFadeText("ui_fr_search_fail");
                break;
            case 2:
                PopupCanvas.Instance.ShowFadeText("already_friend");
                break;
            case 3:
                PopupCanvas.Instance.ShowFadeText("already_sent");
                break;
            case 4:
                PopupCanvas.Instance.ShowFadeText("no_request_sent");
                break;
            case 5:
                PopupCanvas.Instance.ShowFadeText("no_request_taken");
                break;
            case 6:
                PopupCanvas.Instance.ShowFadeText("friend_list_full");
                break;
            case 7:
                PopupCanvas.Instance.ShowFadeText("not_a_friend");
                break;
            case 8:
                PopupCanvas.Instance.ShowFadeText("already_received");
                break;
            case 9:
                PopupCanvas.Instance.ShowFadeText("cannot_send_yet");
                break;
            case 10:
                PopupCanvas.Instance.ShowFadeText("nothing_to_receive");
                break;
            case 11:
                PopupCanvas.Instance.ShowFadeText("gift_daily_limited");
                break;
            case 12:
                break;
            case 13:
                PopupCanvas.Instance.ShowFadeText("recommend_negative");
                break;
            default:
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_fr_error"));
                break;
        }
    }

    public void ReceiveMessage(sChatData chatData)
    {
        if(ChatUI.IsChatTarget(chatData.ChatId))
            return;
        if(chatData.Type == eChatType.Receive)
            OnFriendAlarmCheck();
    }

    public void SetCandidateDuo(IList<FriendInfo> friendInfos)
    {
       (UIPanel[0] as FriendsList).SetCandidateDuo(friendInfos);
    }

}
