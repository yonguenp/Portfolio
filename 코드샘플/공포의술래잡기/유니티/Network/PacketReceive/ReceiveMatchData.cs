using SBSocketPacketLib;
using SBSocketSharedLib;

public partial class PacketManager
{
    public void ReceiveSCResMatch(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCResMatch resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");

            var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP);
            popup?.Close();

            var lobby = Managers.Scene.CurrentScene as LobbyScene;
            if (lobby != null)
            {
                lobby.SetEnableMatch(true);
            }
            return;
        }
    }

    public void ReceiveSCResMatchCancel(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCResMatchCancel resPacket);
        if (resPacket == null)
            return;

        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP);

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            PopupCanvas.Instance.ShowFadeText("매치취소실패");
            popup.CancelInvoke("NetworkError");
            return;
        }

        
        popup?.Close();

        //if (Managers.FriendData.DUO.IsDuoPlaying())
        //{
        //    Managers.FriendData.DUO.ClearDuo();
        //}

        var lobby = Managers.Scene.CurrentScene as LobbyScene;
        if (lobby != null)
        {
            if (Managers.FriendData.DUO.IsDuoPlaying())
            {
                if (Managers.FriendData.DUO.IsHost())
                    lobby.SetEnableMatch(false);
                else
                    lobby.SetEnableMatch(true);
            }
            else
                lobby.SetEnableMatch(true);


        }
    }

    private void ReceiveSCBcEnterLobby(TcpServerSession serverSession_, SBPacket packet_)
    {
        //SCBroadcastEnterGame
        SCBcEnterLobby resPacket = resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        //LobbyPlayerManager.Instance.EnterGame(resPacket);
    }

    private void ReceiveSCBcEnterGame(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcEnterGame resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        Game.Instance.EnterGame(resPacket);
    }

    private void ReceiveSCBcGameStart(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcGameStart resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        //RoomScene room = Managers.Scene.CurrentScene as RoomScene;
        //if (room != null)
        //{
        //    Game.Instance.SetGameStartTimeStamp(resPacket.Timestamp);
        //    room.OnStartGame();
        //}

        Game.Instance.SetGameStartTimeStamp(resPacket.Timestamp);
    }

    private void ReceiveSCBcLeaveLobby(TcpServerSession serverSession_, SBPacket packet_)
    {
        //SCBroadcastLeaveLobby resPacket = SBMessagePack.Deserialize<SCBroadcastLeaveLobby>(serverSession_.CryptKey, serverSession_.CryptIV, packet_.Data);
        SCBcLeaveGame resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        //LobbyPlayerManager.Instance.LeaveGame(resPacket);
    }

    private void ReceiveSCBcMatchInfo(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcMatchInfo resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        // if (resPacket.ErrorCode != (int)ErrorCode.Success)
        // {
        //     SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
        //     //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
        //     return;
        // }

        SBDebug.Log($"ProccessSCBcMatchInfo Result[{resPacket.Result}] / MatchLimit[{resPacket.MatchLimit}] /Count[{resPacket.MatchPlayerInfos.Count}]");

        Managers.PlayData.SetRoomPlayers(resPacket.MatchPlayerInfos);

        switch ((MatchResult)resPacket.Result)
        {
            case MatchResult.Proceeding:
                {
                    if (!PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
                    {
                        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP);

                        if (resPacket.GameRoomType == 1)
                        {
                            (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP) as MatchInfoPopup).SetRankMode();
                        }
                        else
                        {
                            (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP) as MatchInfoPopup).SetPracticeMode();
                        }
                    }

                    MatchInfoPopup matchInfo = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP) as MatchInfoPopup;
                    matchInfo.SetCount(resPacket.MatchPlayerInfos.Count, resPacket.MatchLimit);
                    matchInfo.SetRoomPlayer(resPacket.MatchPlayerInfos);
                }
                break;
            case MatchResult.Successed:
                {
                    Managers.FriendData.DUO.OnGameStart();

                    var LobbyScene = Managers.Scene.CurrentScene as LobbyScene;
                    if (LobbyScene != null)
                    {
                        if (resPacket.Result != 0)
                        {
                            SBDebug.Log($"match fail {resPacket.Result}");
                            LobbyScene.OnMatchFail(resPacket.Result);
                            return;
                        }

                        LobbyScene.OnMatchSuccess();
                    }
                    //Managers.PlayData.SetForceStartTime(resPacket.Timestamp);
                    Managers.Scene.LoadScene(SceneType.Room);
                }
                break;
            case MatchResult.MatchRoomRelease:
                //Managers.Network.SendGameMatch(1);
                //Managers.FriendData.DUO.ClearDuo();

                Managers.Network.SendGameMatchCancel();

                var RoomScene = Managers.Scene.CurrentScene as RoomScene;
                if (RoomScene == null)
                    return;

                // RoomScene.StopTimer();

                PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("게임취소안내"), () =>
                {
                    var RoomScene = Managers.Scene.CurrentScene as RoomScene;
                    if (RoomScene == null)
                        return;

                    Managers.Scene.LoadScene(SceneType.Lobby);
                });
                break;
            case MatchResult.Failed:
                // 매치 failed 왔을때 처리 필요 
                break;
            default:
                SBDebug.Log("MatchResult : " + (((MatchResult)resPacket.Result).ToString()));
                break;
        }
    }

    private void ReceiveSCInit(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCInit resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        Managers.PlayData.IsResume = false;
        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (resPacket.Result == (sbyte)LoginResult.WrongClientVersion)
        {
            GameConfig.Instance.SetWrongClientVersion();
            return;
        }

        if (resPacket.Result == (sbyte)LoginResult.Reconnect)
        {
            Managers.PlayData.IsResume = true;
        }



        // 서버에서 전달 받은 세션 아이디로 세팅
        serverSession_.SetSessionID(packet_.SessionId);
        Managers.UserData.SetMySocketSessionID(packet_.SessionId);
        //GameManager.Instance.SetPlayerID(resPacket.UserInfo.UserId);
        //Managers.UserData.SetMyUserInfo_notuse(resPacket.UserInfo);

        //SBLog.PrintInfo($"[ProcessSCInit] RecvMessage: SessionId - {serverSession_.SessionID}, Index - {resPacket.Index}, ErrorCode - {resPacket.ErrorCode},");
        //NetworkManager.Instance.OnConnectComplete();

        //Managers.Instance.SendPing();
        Managers.Instance.RunAlivePingNotify();
    }

    private void ReceiveSCPong(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCPong resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        Game.Instance.RecvPong(resPacket.Time);
    }

    private void ReceiveSCBcCreateGameRoom(TcpServerSession serverSession_, SBPacket packet_)
    {
        SBDebug.Log($"ReceiveSCBcCreateGameRoom");
        Deserialize(serverSession_, packet_, out SCBcCreateGameRoom resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        
        Managers.TimeManager.SetBaseTime(resPacket.Timestamp, SBCommonLib.SBUtil.GetCurrentMilliSecTimestamp());

        Managers.FriendData.DUO.OnGameStart();

        Managers.Object.Clear();

        Managers.PlayData.ResetGameRoomInfo();
        Managers.PlayData.SetGameRoomInfo(resPacket);

        var roomScene = Managers.Scene.CurrentScene as RoomScene;
        if (roomScene == null)
        {            
            Managers.Scene.LoadScene(SceneType.Room);
            Managers.PlayData.CreateGameRoom();
            return;
        }

        roomScene.OnAllReady(resPacket.Timestamp);
    }

    //private void ReceiveGCGameRoomInfo(TcpServerSession serverSession_, SBPacket packet_)
    //{
    //    SBDebug.Log($"ReceiveSCBcCreateGameRoom");
    //    Deserialize(serverSession_, packet_, out GCGameRoomInfo resPacket);
    //    if (resPacket == null)
    //        return;

    //    if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
    //    {
    //        SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
    //        //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
    //        return;
    //    }

    //    Managers.Object.Clear();

    //    Managers.PlayData.ResetGameRoomInfo();
    //    Managers.PlayData.SetGameRoomInfo(resPacket);

    //    //Managers.GameServerNetwork.ReqGamePlayerInfo();
    //}

    //private void ReceiveSCResGameRoomPlayerList(TcpServerSession serverSession_, SBPacket packet_)
    //{
    //    SBDebug.Log($"ReceiveSCResGameRoomPlayerList");
    //    Deserialize(serverSession_, packet_, out GCBcGamePlayerInfo resPacket);
    //    if (resPacket == null)
    //        return;

    //    Managers.PlayData.SetGameRoomPlayerList(resPacket);
    //    Managers.FriendData.DUO.OnGameStart();

    //    var roomScene = Managers.Scene.CurrentScene as RoomScene;
    //    if (roomScene == null)
    //    {
    //        //Managers.PlayData.IsResume = true;
    //        Managers.Scene.LoadScene(SceneType.Room);
    //        Managers.PlayData.CreateGameRoom();
    //        return;
    //    }

    //    roomScene.OnAllReady(resPacket.Timestamp);
    //}

    private void ReceiveSCResExerciseRoomCreate(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCResExerciseRoomCreate resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");

            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_troom_fail"));

            var LobbyScene = Managers.Scene.CurrentScene as LobbyScene;
            if (LobbyScene != null)
            {
                LobbyScene.OnMatchFail(2);
            }

            return;
        }

        PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_troom_succ"));
    }

    private void ReceiveSCResExerciseMatch(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCResExerciseMatch resPacket);
        if (resPacket == null)
            return;

        switch ((MatchResult)resPacket.Result)
        {
            case MatchResult.Successed:
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_troom_request_succ"));
                break;
            case MatchResult.NeedPassword:
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_troom_password"));
                    var LobbyScene = Managers.Scene.CurrentScene as LobbyScene;
                    if (LobbyScene != null)
                    {
                        LobbyScene.SelectMenuButton(17);
                    }
                }
                break;
            default:
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_troom_request_fail"));
                    var LobbyScene = Managers.Scene.CurrentScene as LobbyScene;
                    if (LobbyScene != null)
                    {
                        LobbyScene.OnMatchFail(2);
                    }
                }
                break;
        }
    }

    private void ReceiveSCResExerciseRoomPassword(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCResExerciseRoomPassword resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_sroom_request_fail"));
            var LobbyScene = Managers.Scene.CurrentScene as LobbyScene;
            if (LobbyScene != null)
            {
                LobbyScene.OnMatchFail(2);
            }
            return;
        }
        PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_sroom_request_succ"));
    }

    private void ReceiveSCResLCGameServerEnter(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out LCGameServerEnter resPacket);
        if (resPacket == null)
            return;

        SBDebug.Log($"Ip [{resPacket.GameServerIP}], Port [{resPacket.GameServerPort}]");
        Managers.GameServer.OnConnectGameServer(resPacket.GameServerIP, resPacket.GameServerPort, OnConnnectedGameServer);
    }

    private void ReceiveSCResGameServerEnter(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out GCGameServerEnter resPacket);
        if (resPacket == null)
            return;

        Managers.Instance.CancelInvoke("FailGameServerConnect");
        SBDebug.Log($"result [{resPacket.Result}]");
    }

    #region FOR DUO

    private void ReceiveSCDuoCancel(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCDuoCancel resPacket);
        if (resPacket == null)
            return;

        Managers.FriendData.DUO.ClearDuo(true);
    }
    private void ReceiveSCDuoInvite(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCDuoInvite resPacket);
        if (resPacket == null)
            return;

        Managers.FriendData.DUO.RecvDuoResponse(resPacket);
    }

    private void ReceiveSCDuoInviteNotify(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCDuoInviteNotify resPacket);
        if (resPacket == null)
            return;

        Managers.FriendData.DUO.OnDuoResponse(resPacket);
    }

    private void ReceiveSCFriendList(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCFriendList resPacket);
        if (resPacket == null)
            return;

        if(PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.FRIEND_POPUP))
            (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.FRIEND_POPUP) as FriendPopup).SetCandidateDuo(resPacket.FriendInfos);        
    }

    private void ReceiveSCClanMemberList(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCClanMemberList resPacket);
        if (resPacket == null)
            return;

        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP))
            (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup).SetCandidateDuo(resPacket.GuildMemberInfos);
    }

    private void ReceiveSCDuoAccept(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCDuoAccept resPacket);
        if (resPacket == null)
            return;

        Managers.FriendData.DUO.RecvDuoAccept(resPacket);
    }
    private void ReceiveSCDuoAcceptNotify(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCDuoAcceptNotify resPacket);
        if (resPacket == null)
            return;

        Managers.FriendData.DUO.OnDuoAccept(resPacket);
    }

    private void ReceiveSCDuoGuestMatch(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCDuoGuestMatch resPacket);
        if (resPacket == null)
            return;

        Managers.FriendData.DUO.RecvDuoGuestMatch(resPacket);
    }

    private void ReceiveSCDuoGuestMatchNotify(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCDuoGuestMatchNotify resPacket);
        if (resPacket == null)
            return;

        Managers.FriendData.DUO.OnDuoGuestMatch(resPacket);
    }

    private void ReceiveSCDuoMatch(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCDuoMatch resPacket);
        if (resPacket == null)
            return;

        Managers.FriendData.DUO.RecvDuoMatch(resPacket);
    }

    private void ReceiveSCDuoCharacterChange(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCDuoCharacterChange resPacket);
        if (resPacket == null)
            return;

        Managers.FriendData.DUO.RecvDuoCharacterChange(resPacket);
    }

    private void ReceiveSCDuoClearNotify(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCDuoClearNotify resPacket);
        if (resPacket == null)
            return;

        Managers.FriendData.DUO.ClearDuo(true);
    }
    #endregion // FOR DUO

    private void ReceiveSCOneOnOneMessage(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCOneOnOneMessage resPacket);
        if (resPacket == null)
            return;

        Managers.Chat.AddMessage(resPacket.FromUserId, resPacket.FromUserName, resPacket.ToUserId, resPacket.ToUserName, resPacket.Message);
    }
    private void ReceiveSCMatchServerCrashNotify(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCMatchServerCrashNotify resPacket);
        if (resPacket == null)
            return;

        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP);
        if (popup.IsOpening())
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_msever_error"));

            (popup as MatchInfoPopup).OnCancelMatch();
        }
    }

    private void ReceiveSCClanChatMessage(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCClanChat resPacket);
        if (resPacket == null)
            return;

        Managers.ClanCaht.AddMessage(resPacket.UserNo,resPacket.Nick, resPacket.RankPoint, resPacket.Message, resPacket.Time);
    }

    void OnConnnectedGameServer()
    {
        Managers.GameServer.GameServerEnter();
    }
}
