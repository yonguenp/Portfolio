using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanInfoMenu : MonoBehaviour
{
    public enum CLAN_MENU
    {
        None,
        Clan_CrewList,
        CLAN_MISSION,
        CLAN_SHOP,
        CLAN__MEMBERSHIPAUTH,
    }

    [SerializeField] ClanInfo claninfo;
    [SerializeField] GameObject page;
    [SerializeField] GameObject ClanItem;

    [Header("[가입 승인]")]
    [SerializeField] GameObject noWaitPanel;
    [SerializeField] GameObject Scroll;

    [Header("[미션]")]
    [SerializeField] ClanMissionItem[] clanMission;

    public CLAN_MENU menuType;
    List<ClanInfo_CrewItem> loginUserUI = null;

    public void RefreshPage()
    {
        if (claninfo.cur_menu != menuType)
        {
            page.SetActive(false);
            return;
        }

        switch (menuType)
        {
            case CLAN_MENU.Clan_CrewList:
                if (claninfo.ClanPeopleData == null)
                {
                    PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP).Close();
                    return;
                }

                Clear();

                List<JObject> loginUsers = new List<JObject>();
                List<JObject> logoutUsers = new List<JObject>();

                foreach (JObject people in claninfo.ClanPeopleData)
                {
                    DateTime lastlogin = DateTime.Now;
                    DateTime lastlogout = DateTime.Now;

                    if (people.ContainsKey("last_login"))
                        lastlogin = DateTime.Parse(people["last_login"].Value<string>());

                    if (people.ContainsKey("last_logout"))
                        lastlogout = DateTime.Parse(people["last_logout"].Value<string>());

                    if (lastlogin <= lastlogout)
                    {
                        logoutUsers.Add(people);
                    }
                    else
                    {
                        loginUsers.Add(people);
                    }
                }

                if (loginUserUI == null)
                    loginUserUI = new List<ClanInfo_CrewItem>();

                loginUserUI.Clear();
                foreach (JObject people in loginUsers)
                {
                    ClanItem.SetActive(true);
                    var obj = GameObject.Instantiate(ClanItem, ClanItem.transform.parent);
                    ClanInfo_CrewItem ui = obj.GetComponentInChildren<ClanInfo_CrewItem>();
                    ui.Init(people);

                    loginUserUI.Add(ui);
                }

                foreach (JObject people in logoutUsers)
                {
                    ClanItem.SetActive(true);
                    var obj = GameObject.Instantiate(ClanItem, ClanItem.transform.parent);
                    obj.GetComponentInChildren<ClanInfo_CrewItem>().Init(people);
                }

                ClanItem.SetActive(false);

                RefreshMemberstate();
                break;
            case CLAN_MENU.CLAN_MISSION:
                RefreshClanMission();
                break;
            case CLAN_MENU.CLAN_SHOP:
                return;
            case CLAN_MENU.CLAN__MEMBERSHIPAUTH:
                RefreshAcceptClanPanel();
                break;
        }

        page.SetActive(true);
    }

    public void RefreshMemberstate()
    {
        if (loginUserUI == null)
            return;

        List<long> arrayUsers = new List<long>();
        foreach (var ui in loginUserUI)
        {
            if (ui.userData.ContainsKey("user_no"))
            {
                long user_no = ui.userData["user_no"].Value<long>();
                if (user_no != Managers.UserData.MyUserID)
                    arrayUsers.Add(user_no);
            }
        }

        if (arrayUsers.Count == 0)
            return;

        Managers.Network.SendClanList(arrayUsers);
    }

    public void SetCandidateDuo(IList<SBSocketSharedLib.ClanMemberInfo> infos)
    {
        var scene = Managers.Scene.CurrentScene as LobbyScene;
        if (loginUserUI != null)
        {
            foreach (var ui in loginUserUI)
            {
                if (ui == null)
                    continue;

                if (ui.userData == null)
                    continue;

                byte state = (byte)SBSocketSharedLib.UserPlayState.None;
                JObject data = ui.userData;
                if (data.ContainsKey("user_no") && infos != null)
                {
                    long user_no = data["user_no"].Value<long>();
                    foreach (SBSocketSharedLib.ClanMemberInfo info in infos)
                    {
                        if (info.UserNo == user_no)
                        {
                            state = info.UserState;
                        }
                    }
                }

                if (!scene.SetEnableSendDuo())
                    ui.SetDuoEnable(false);
                else
                    ui.SetDuoEnable(state == (byte)SBSocketSharedLib.UserPlayState.Lobby);
            }
        }
    }

    public void Clear()
    {
        foreach (Transform item in ClanItem.transform.parent)
        {
            if (item == ClanItem.transform)
                continue;
            Destroy(item.gameObject);
        }
    }

    //가입 요청 목록 생성
    public void RefreshAcceptClanPanel()
    {
        var clanPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;
        int no = clanPopup.ClanInfo["my"]["clan_no"].Value<int>();
        clanPopup.ClanRequestUserList(no, (res) =>
         {
             JToken result = res["result"];
             JArray users = (JArray)result["list"];
             //SBDebug.Log(users);

             if (noWaitPanel != null)
                 noWaitPanel.SetActive(users == null || users.Count == 0);
             if (Scroll != null)
                 Scroll.SetActive(users != null && users.Count > 0);

             Clear();
             ClanItem.SetActive(true);
             foreach (JObject user in users)
             {
                 var obj = GameObject.Instantiate(ClanItem, Scroll.GetComponent<ScrollRect>().content.transform);
                 obj.GetComponent<ClanInfo_CrewItem>().Init(user);
             }
             ClanItem.SetActive(false);
         });
    }

    public void RefreshClanMission()
    {
        if (clanMission.Length <= 0)
            return;
        var clanPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;
        //SBDebug.Log(clanPopup.ClanInfo["my"]);
        JObject jo = (JObject)clanPopup.ClanInfo["my"];

        int idx = 1;
        foreach (ClanMissionItem item in clanMission)
        {
            item.Init(idx, jo);
            idx++;
        }
    }
}
