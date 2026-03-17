using SBCommonLib;
using SBSocketSharedLib;
using System;
using System.Collections.Generic;

public partial class NetworkManager
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// 아웃게임 (UI 등)
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    public bool SendInit(long userNo, string token)
    {
        SendMessage(PacketId.CSInit,
            new CSInit
            {
                UserNo = userNo,
                SessionToken = token,
                Version = GameConfig.Instance.VERSION
            });
        return true;
    }

    public bool SendEnterLobby(Action callback)
    {
        SendMessage(PacketId.CSEnterLobby, new CSEnterLobby(), (int)PacketId.SCBcEnterLobby, callback);
        return true;
    }

    public bool SendEnterGame()
    {
#if UNITY_EDITOR || SB_TEST
        if (GameConfig.Instance.USE_DUMMY && !Managers.PlayData.IsResume)
        {
            SendMessage(PacketId.CSEnterGameWithDummy, new CSEnterGame());
        }
        else
#endif
        {
            SendMessage(PacketId.CSEnterGame, new CSEnterGame());
        }
        return true;
    }

    public void SendGameMatch()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || SB_TEST
        if (GameConfig.Instance.USE_DUMMY)
        {
            SendMessage(PacketId.CSMatchWithDummy,
                new CSReqMatchWithDummy
                {
                    IsChaser = GameConfig.Instance.DUMMY_PLAYCHASER,
                });
        }
        else
#endif
        {
            SendMessage(PacketId.CSReqMatch, new CSReqMatch());
        }
    }

    public void SendCreatePracticeMatch(string password, int mapNo)
    {
        SendMessage(PacketId.CSReqExerciseRoomCreate, new CSReqExerciseRoomCreate
        {
            NickName = Managers.UserData.MyName,
            Password = password,
            MapNo = mapNo
        });
    }

    public void SendJoinPracticeMatch(string nick)
    {
        SendMessage(PacketId.CSReqExerciseMatch, new CSReqExerciseMatch
        {
            NickName = nick
        });
    }
    public void SendPasswordPracticeMatch(string nick, string password)
    {
        SendMessage(PacketId.CSReqExerciseRoomPassword, new CSReqExerciseRoomPassword
        {
            NickName = nick,
            Password = password
        });
    }


    public void SendGameMatchCancel()
    {
        SendMessage(PacketId.CSReqMatchCancel, new CSReqMatchCancel());
    }

    public bool SendLeaveLobby()
    {
        SendMessage(PacketId.CSLeaveLobby, new CSLeaveLobby());
        return true;
    }

    //     public bool SendGameReady()
    //     {
    // #if UNITY_EDITOR
    //         if (GameConfig.Instance.USE_DUMMY)
    //         {
    //             SendMessage(PacketId.CSGameReadyWithDummy, new CSGameReady());
    //         }
    //         else
    // #endif
    //         {
    //             SendMessage(PacketId.CSGameReady, new CSGameReady());
    //         }

    //         return true;
    //     }

    //     public bool SendSelectCharacter(int id)
    //     {
    //         SendMessage(PacketId.CSMatchSelectCharacter,
    //             new CSMatchSelectCharacter
    //             {
    //                 CharacterUID = id,
    //             });
    //         return true;
    //     }

    public bool SendDuoList(List<long> users)
    {   
        SendMessage(PacketId.CSFriendList, new CSFriendList
        {
            FriendInfos = users
        });

        return true;
    }

    public bool SendClanList(List<long> users)
    {
        SendMessage(PacketId.CSClanMemberList, new CSClanMemberList
        {
            MemberInfos = users
        });

        return true;
    }

    public bool SendDuoInvite(long geustNo, DuoType duoType)
    {
        SendMessage(PacketId.CSDuoInvite, new CSDuoInvite
        {
            GuestUserNo = geustNo,
            DuoType = (byte)duoType
        });
        return true;
    }

    public bool SendDuoAccpet(bool accpeted, long hostNo, byte duoType)
    {
        SendMessage(PacketId.CSDuoAccept, new CSDuoAccept
        {
            Response = (byte)(accpeted ? 1 : 0),
            HostUserNo = hostNo,
            DuoType = (byte)duoType
        });
        return true;
    }

    public bool SendDuoGuestMatch(long hostNo)
    {
        SendMessage(PacketId.CSDuoGuestMatch, new CSDuoGuestMatch
        {   
            HostUserNo = hostNo
        });
        return true;
    }

    public bool SendDuoMatch(long hostNo, long guestNo)
    {
        List<long> userNos = new List<long>();
        userNos.Add(hostNo);
        userNos.Add(guestNo);

        SendMessage(PacketId.CSDuoMatch, new CSDuoMatch
        {
            UserNos = userNos
        });

        return true;
    }

    public bool SendDuoCharacterChange(int charType, int selectCharacterUID)
    {
        SendMessage(PacketId.CSDuoCharacterChange, new CSDuoCharacterChange
        {
            CharacterType = (byte)charType,
            SelectCharacterUID = selectCharacterUID
        });

        return true;
    }

    public bool SendDuoClear()
    {
        SendMessage(PacketId.CSDuoCancel, new CSDuoCancel());

        return true;
    }


    public bool SendChatMessage(long to_id, string to_nick, string message)
    {
        var packet = new CSOneOnOneMessage()
        {
            ToUserId = to_id,
            ToUserName = to_nick,
            FromUserId = Managers.UserData.MyUserID,
            FromUserName= Managers.UserData.MyName,
            Message = message
        };

        SendMessage(PacketId.CSOneOnOneMessage,packet);

        return true;
    }


    public bool SendAlivePingNotify()
    {
        SendMessage(PacketId.CSAlivePingNotify, new CSAlivePingNotify());

        return true;
    }

    public bool SendSeasonRank()
    {
        SendMessage(PacketId.CSSeasonRank, new CSSeasonRank());

        return true;
    }

    public bool SendClanChatMessage(string msg)
    {
        var packet = new CSClanChat()
        {
            Message = msg
        };
        SendMessage(PacketId.CSClanChat, packet);
        return true;
    }

    public bool SendCSClanInfoUpdateNotify()
    {
        SendMessage(PacketId.CSClanInfoUpdateNotify, new CSClanInfoUpdateNotify());

        return true;
    }


}
