using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendsPanel : MonoBehaviour
{
    public enum FriendsUI
    {
        NONE = -1,
        FRIENDS_LIST,
        FRIENDS_SEARCH,
        FRIENDS_CALL,
        BLOCK_USER,
    };

    public Toggle[] ToggleMenu;
    public FriendsUIObject[] UIPanel;
    public FriendsUI curUI = FriendsUI.NONE;
    public GameObject[] RedDot;
    public ChatUI chatUI;

    private void OnEnable()
    {
        curUI = FriendsUI.NONE;
        Toggle target = ToggleMenu[(int)FriendsUI.FRIENDS_LIST];
        foreach(Toggle toggle in ToggleMenu)
        {
            toggle.isOn = toggle == target;
        }

        for (int i = 0; i < RedDot.Length; i++)
        {
            if (RedDot[i] != null)
                RedDot[i].SetActive(UIPanel[i].IsNewFlag());
        }

        OnToggleMenu();
    }

    private void OnDisable()
    {

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

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 12);

        NetworkManager.GetInstance().SendApiRequest("friend", 12, data, (response) =>
        {
            Invoke("OnFriendAlarmCheck", 0.1f);
        }, null, false);
    }

    public void OnExitButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.FRIEND_INFO_POPUP);
        chatUI.FriendAlarm.SetActive(false);
    }

    public void OnFriendAlarmCheck()
    {
        for (int i = 0; i < RedDot.Length; i++)
        {
            if(RedDot[i] != null)
                RedDot[i].SetActive(UIPanel[i].IsNewFlag());
        }
    }
}
