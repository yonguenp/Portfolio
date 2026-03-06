using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UserProfile
{
    public enum FriendType
    {
        NONE,
        FRIEND,
        SENT,
        DECLINED,
        TAKEN,
        RECOMMAND,
        BLOCKED,
    };

    public long uno = 0;
    public string nick = "";
    public int point = 0;
    public FriendType type = FriendType.NONE;
    public uint gift_flag = 0;
    public uint last_update = 0;
    public string lastMessage = "";
    public string clan_name = "";
    public long clan_no = 0;

    public DateTime lastLogin;
    public DateTime lastLogout;

    public void UIShown(uint curTime)
    {
        string json = PlayerPrefs.GetString("UserProfile", "");
        try
        {
            JObject root;
            if (string.IsNullOrEmpty(json))
                root = new JObject();
            else
                root = JObject.Parse(json);

            if (root.ContainsKey(uno.ToString()))
            {
                root[uno.ToString()] = curTime;
            }
            else
            {
                root.Add(uno.ToString(), curTime);
            }

            PlayerPrefs.SetString("UserProfile", root.ToString(Newtonsoft.Json.Formatting.None));
        }
        catch
        {
            PlayerPrefs.SetString("UserProfile", "");
        }
    }

    public bool isAlarm
    {
        get
        {
            string json = PlayerPrefs.GetString("UserProfile", "");
            if (string.IsNullOrEmpty(json))
                return true;

            JObject root = JObject.Parse(json);
            if (root != null && root.ContainsKey(uno.ToString()))
            {
                return root[uno.ToString()].Value<uint>() < last_update;
            }
            else
            {
                return true;
            }
        }
    }

    public bool IsLoginUser()
    {
        return lastLogin > lastLogout;
    }
}

public class FriendsManager
{
    private List<UserProfile> users = new List<UserProfile>();
    private uint newFriendCount = 0;
    private uint newReciveCount = 0;

    public DuoManager DUO { get; } = new DuoManager();
    public int FRIEND_MAX_COUNT { get; set; } = 40;

    public List<UserProfile> GetFriendList()
    {
        List<UserProfile> ret = new List<UserProfile>();
        foreach (UserProfile user in users)
        {
            if (user.type == UserProfile.FriendType.FRIEND)
                ret.Add(user);
        }

        ret.Sort((x, y) =>
        {
            if (x.IsLoginUser())
                return 1;
            if (y.IsLoginUser())
                return -1;

            return 0;
        });

        return ret;
    }

    public List<UserProfile> GetSentList()
    {
        List<UserProfile> ret = new List<UserProfile>();
        foreach (UserProfile user in users)
        {
            if (user.type == UserProfile.FriendType.SENT)
                ret.Add(user);
        }

        return ret;
    }

    public List<UserProfile> GetDeclinedList()
    {
        List<UserProfile> ret = new List<UserProfile>();
        foreach (UserProfile user in users)
        {
            if (user.type == UserProfile.FriendType.DECLINED)
                ret.Add(user);
        }

        return ret;
    }

    public List<UserProfile> GetTakenList()
    {
        List<UserProfile> ret = new List<UserProfile>();
        foreach (UserProfile user in users)
        {
            if (user.type == UserProfile.FriendType.TAKEN)
                ret.Add(user);
        }

        return ret;
    }

    public List<UserProfile> GetRecommandList()
    {
        List<UserProfile> ret = new List<UserProfile>();
        foreach (UserProfile user in users)
        {
            if (user.type == UserProfile.FriendType.RECOMMAND)
                ret.Add(user);
        }

        return ret;
    }

    public UserProfile UpdateUserProfile(JObject data, UserProfile.FriendType type)
    {
        long un = data["user_no"].Value<long>();
        foreach (UserProfile user in users)
        {
            if (user.uno == un)
            {
                user.point = data["point"].Value<int>();
                user.nick = data["nick"].Value<string>();
                user.type = type;
                if (data.ContainsKey("last_login"))
                    user.lastLogin = DateTime.Parse(data["last_login"].Value<string>());
                if (data.ContainsKey("last_logout"))
                    user.lastLogout = DateTime.Parse(data["last_logout"].Value<string>());

                if (data.ContainsKey("last_update"))
                {
                    if (user.last_update < data["last_update"].Value<uint>())
                        user.last_update = data["last_update"].Value<uint>();
                }
                return user;
            }
        }

        UserProfile new_user = new UserProfile();
        new_user.uno = un;
        new_user.point = data["point"].Value<int>();
        new_user.nick = data["nick"].Value<string>();
        new_user.type = type;
        if (data.ContainsKey("last_login"))
            new_user.lastLogin = DateTime.Parse(data["last_login"].Value<string>());
        if (data.ContainsKey("last_logout"))
            new_user.lastLogout = DateTime.Parse(data["last_logout"].Value<string>());

        if (data.ContainsKey("last_update"))
            new_user.last_update = data["last_update"].Value<uint>();
        if (data.ContainsKey("clan_name"))
            new_user.clan_name = data["clan_name"].Value<string>();
        if (data.ContainsKey("clan_no"))
            new_user.clan_no = data["clan_no"].Value<int>();

        users.Add(new_user);

        return new_user;
    }
    public void AddUserProfile(UserProfile profile, UserProfile.FriendType type)
    {
        if (users.Contains(profile))
        {
            users.Find(_ => _.uno == profile.uno).type = type;
            return;
        }

        users.Add(profile);
    }

    public void DeleteUserProfile(UserProfile profile)
    {
        users.Remove(profile);
    }

    public void DeleteUserProfiles(List<UserProfile> list)
    {
        foreach (UserProfile user in list)
        {
            DeleteUserProfile(user);
        }
    }

    public void SetNewFriendCount(uint newFriend)
    {
        newFriendCount = newFriend;
    }


    public void SetNewRecivedCount(uint newRecive)
    {
        newReciveCount = newRecive;
    }

    public long GetNewFriendCount()
    {
        return newFriendCount;
    }

    public long GetNewRecivedCount()
    {
        return newReciveCount;
    }

    public UserProfile GetUserProfile(long userNo)
    {
        foreach (UserProfile user in users)
        {
            if (userNo == user.uno)
            {
                return user;
            }
        }

        return null;
    }

    public bool CheckRequestFriendNotice(long id)
    {
        string json = PlayerPrefs.GetString("UserProfile", "");
        if (string.IsNullOrEmpty(json))
            return true;

        JObject root = JObject.Parse(json);
        if (root != null && root.ContainsKey(id.ToString()))
        {
            return false;
        }

        return true;
    }

    JObject requestFriendInfo = null;

    public bool CheckRequestFriendNotice()
    {
        if (requestFriendInfo == null) return false;

        int count = requestFriendInfo.Count - 1;
        for (int i = 0; i < count; ++i)
        {
            var item = requestFriendInfo[i.ToString()];
            if (item == null) continue;
            uint id = item["un"].Value<uint>();
            int status = item["status"].Value<int>();
            if (status != 2) continue;
            if (CheckRequestFriendNotice(id))
            {
                return true;
            }
        }

        return false;
    }

    public void RequestFriendInfo(Action cb = null)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 18);

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            SetRequestFriendInfo((JObject)response);

            if (cb != null) cb.Invoke();
        });
    }

    public void SetRequestFriendInfo(JObject data)
    {
        requestFriendInfo = data;
    }

    public void SetFriendRecommend(bool isShow, Action<bool> cb = null)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 19);
        data.AddField("recommend", isShow ? 1 : 0);

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            var rs = response.Value<int>("rs");
            if (rs != 0) return;
            var recommend = int.Parse(response["recommend"].ToString());
            if (cb != null) cb.Invoke(recommend == 1);
        });
    }
}

public class DuoManager
{
    //public UserProfile Host { get { return Managers.FriendData.GetUserProfile((uint)HostNo); } }
    //public UserProfile Guest { get { return Managers.FriendData.GetUserProfile((uint)GuestNo); } }
    public SBSocketSharedLib.SCDuoInviteNotify Host { get; private set; } = null;
    public SBSocketSharedLib.SCDuoAcceptNotify Guest { get; private set; } = null;

    public bool GuestReady { get; private set; } = false;
    public void ClearDuo(bool receive = false)
    {
        if (!receive)
            Managers.Network.SendDuoClear();

        if (Host != null && Guest != null)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_duo_break"));
        }

        Host = null;
        Guest = null;
        GuestReady = false;

        var LobbyScene = Managers.Scene.CurrentScene as LobbyScene;

        var matchinfopopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP);
        if (matchinfopopup.gameObject.activeSelf)
        {
            //준형 :: 듀오 취소 시 원래 캔슬 매치를 보냈지만 현재 서버에서 담당하기로 변경함 
            //(matchinfopopup as MatchInfoPopup).OnCancelMatch();
        }

        if (LobbyScene != null)
        {
            LobbyScene.SetEnableMatch(true);
        }

        if(PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP))
        {
            (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup).DuoClear();
        }
    }

    public bool IsDuoPlaying()
    {
        return Host != null && Guest != null;
    }

    public bool IsHost()
    {
        if (!IsDuoPlaying())
            return false;

        return Host.HostUserNo == Managers.UserData.MyUserID;
    }

    public void SendDuoRequest(UserProfile friendProfile, SBSocketSharedLib.DuoType duoType)
    {
        SendDuoRequest(friendProfile.uno, duoType);
    }

    public void SendDuoRequest(long user_no, SBSocketSharedLib.DuoType duoType)
    {
        Managers.Network.SendDuoInvite(user_no, duoType);

        PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.DUO_POPUP);
        SetMatchUI(false);
    }

    public void RecvDuoResponse(SBSocketSharedLib.SCDuoInvite resPacket)
    {
        if (resPacket.ErrorCode != (byte)SBSocketSharedLib.ErrorCode.Success)
        {
            SetMatchUI(true);
            //PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.DUO_POPUP);
            PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("ui_duo_fail"));
        }
        else
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_duo_wait_acc"));

            Host = new SBSocketSharedLib.SCDuoInviteNotify
            {
                HostUserNo = Managers.UserData.MyUserID,
                NickName = Managers.UserData.MyName,
                ChaserUID = Managers.UserData.MyDefaultChaserCharacter,
                SurvivorUID = Managers.UserData.MyDefaultSurvivorCharacter,
                RankPoint = Managers.UserData.MyPoint
            };
            OnRefershDuoUI();
        }
    }

    public void OnDuoResponse(SBSocketSharedLib.SCDuoInviteNotify resPacket)
    {
        if (!GameConfig.Instance.DUO_ENABLE)
        {
            SendDuoAccept(false, resPacket);
            return;
        }

        PopupCanvas.Instance.ShowConfirmPopup(StringManager.GetString("ui_duo_request", resPacket.NickName), StringManager.GetString("ui_acc"), StringManager.GetString("ui_refusal"), () =>
        {
            SendDuoAccept(true, resPacket);
        },
        () =>
        {
            SendDuoAccept(false, resPacket);
        });
    }

    public void SendDuoAccept(bool accepted, SBSocketSharedLib.SCDuoInviteNotify hostInfo)
    {
        Managers.Network.SendDuoAccpet(accepted, hostInfo.HostUserNo, hostInfo.DuoType);

        if (accepted)
        {
            Host = hostInfo;
            Guest = new SBSocketSharedLib.SCDuoAcceptNotify
            {
                Response = 1,
                GuestUserNo = Managers.UserData.MyUserID,
                NickName = Managers.UserData.MyName,
                ChaserUID = Managers.UserData.MyDefaultChaserCharacter,
                SurvivorUID = Managers.UserData.MyDefaultSurvivorCharacter,
                RankPoint = Managers.UserData.MyPoint
            };

            SetMatchUI(false);

            VibrateManager.OnVibrate(0.5f, 100);
        }
        else
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_duo_refusal"));
            ClearDuo();
        }
    }

    public void RecvDuoAccept(SBSocketSharedLib.SCDuoAccept resPacket)
    {
        if (resPacket.ErrorCode != (byte)SBSocketSharedLib.ErrorCode.Success)
        {
            ClearDuo();
        }
        else
        {
            PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.FRIEND_POPUP);
            PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.DUO_POPUP);
            PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP);

            SetMatchUI(true);
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_duo_acc"));
        }
    }

    public void OnDuoAccept(SBSocketSharedLib.SCDuoAcceptNotify resPacket)
    {
        if (resPacket.Response == 1)//수락
        {
            Host = new SBSocketSharedLib.SCDuoInviteNotify
            {
                HostUserNo = Managers.UserData.MyUserID,
                NickName = Managers.UserData.MyName,
                ChaserUID = Managers.UserData.MyDefaultChaserCharacter,
                SurvivorUID = Managers.UserData.MyDefaultSurvivorCharacter,
                RankPoint = Managers.UserData.MyPoint
            };
            Guest = resPacket;

            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_duo_acc"));
            OnRefershDuoUI();

            PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.FRIEND_POPUP);
            PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.DUO_POPUP);
            PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP);

            VibrateManager.OnVibrate(0.5f, 100);
        }
        else
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_duo_refusal"));
            ClearDuo();
        }
    }

    public void SendDuoGuestMatch()
    {
        if (Host != null && Guest != null)
        {
            Managers.Network.SendDuoGuestMatch(Host.HostUserNo);
            SetMatchUI(false);
        }
        else
        {
            ClearDuo();
            SetMatchUI(true);
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_duo_error_fail"));
        }
    }

    public void RecvDuoGuestMatch(SBSocketSharedLib.SCDuoGuestMatch resPacket)
    {
        if (resPacket.ErrorCode != (byte)SBSocketSharedLib.ErrorCode.Success)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_match_fail"));
            SetMatchUI(true);
        }
        else
        {
            GuestReady = true;
            SetMatchUI(false);
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_host_stand"));
        }
    }

    public void OnDuoGuestMatch(SBSocketSharedLib.SCDuoGuestMatchNotify resPacket)
    {
        GuestReady = true;
        SetMatchUI(true);
        PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_gest_ready"));
    }

    public void SendDuoMatch()
    {
        MatchInfoPopup matchInfo = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP) as MatchInfoPopup;
        if (matchInfo != null)
            matchInfo.SetRankMode();

        if (Host != null && Guest != null)
        {
            Managers.Network.SendDuoMatch(Host.HostUserNo, Guest.GuestUserNo);
            SetMatchUI(false);
        }
        else
        {
            ClearDuo();
            SetMatchUI(true);
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_duo_error_fail"));
        }
    }

    public void RecvDuoMatch(SBSocketSharedLib.SCDuoMatch resPacket)
    {
        MatchInfoPopup matchInfo = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP) as MatchInfoPopup;
        if (matchInfo != null)
            matchInfo.SetRankMode();

        PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_duo_match_success"));
        SetMatchUI(false);
    }

    public void RecvDuoCharacterChange(SBSocketSharedLib.SCDuoCharacterChange resPacket)
    {
        if (IsDuoPlaying())
        {
            if (IsHost())
            {
                if (resPacket.CharacterType == 1)
                    Guest.ChaserUID = resPacket.SelectCharacterUID;
                else
                    Guest.SurvivorUID = resPacket.SelectCharacterUID;
            }
            else
            {
                if (resPacket.CharacterType == 1)
                    Host.ChaserUID = resPacket.SelectCharacterUID;
                else
                    Host.SurvivorUID = resPacket.SelectCharacterUID;
            }

            OnRefershDuoUI();
        }
    }

    public void OnGameStart()
    {
        if (IsDuoPlaying())
            GuestReady = false;
    }

    public void SetMatchUI(bool enable)
    {
        var lobby = Managers.Scene.CurrentScene as LobbyScene;
        if (lobby != null)
        {
            lobby.SetEnableMatch(enable);
        }
    }

    public void OnRefershDuoUI()
    {
        var lobby = Managers.Scene.CurrentScene as LobbyScene;
        if (lobby != null)
        {
            lobby.RefreshDuoInfo();
        }
    }
}
