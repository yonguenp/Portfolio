using SandboxPlatform.SAMANDA;
using SBCommonLib;
using SBSocketPacketLib;
using SBSocketSharedLib;
using System;
using UnityEngine;

public partial class PacketManager
{
    private void ReceiveSCBcChat(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcChat resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        ++serverSession_.CheckMsgPacket;
        //if (serverSession_.SessionNo == 1)
        //    SBLog.PrintInfo($"[ProcessSCChat] SessionId: {serverSession_.SessionID} RecvMessage: Sender - {resPacket.Nick}, Message - {resPacket.Message}");
    }

    private void ReceiveScDuplicateLogin(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCDuplicateLogin resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;
        if ((Managers.Scene.CurrentScene as StartScene) == null)
        {
            if (resPacket.UserId == Managers.UserData.MyUserID)
            {
                PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("중복로그인"), () =>
                {
#if !UNITY_EDITOR
                Application.Quit();
#elif UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
            }
        }
    }

    private void ReceiveSCNotifyReconnect(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCNotifyReconnect resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.UserId == Managers.UserData.MyUserID)
        {
            var startScene = Managers.Scene.CurrentScene as StartScene;
            if (startScene == null)
            {
                return;
            }

#if UNITY_EDITOR
            //PopupCanvas.Instance.ShowConfirmPopup("진행중게임", "재접속", "취소", () =>
            //{
            //    Managers.GameServer.OnConnectGameServer(resPacket.GameServerIP, resPacket.GameServerPort, () => {
            //        Managers.GameServer.SendResume(true);
            //    });
            //}, () =>
            //{
            //    Managers.GameServer.OnConnectGameServer(resPacket.GameServerIP, resPacket.GameServerPort, () => {
            //        Managers.GameServer.SendResume(false);
            //    });

            //    var lobby = Managers.Scene.CurrentScene as LobbyScene;
            //    if (lobby == null)
            //    {
            //        Managers.Scene.LoadScene(SceneType.Lobby);
            //    }
            //});

            Managers.GameServer.OnConnectGameServer(resPacket.GameServerIP, resPacket.GameServerPort, () => {
                Managers.GameServer.SendResume(true);

            });
#else
            Managers.GameServer.OnConnectGameServer(resPacket.GameServerIP, resPacket.GameServerPort, () => {
                Managers.GameServer.SendResume(true);
            });
#endif
        }
    }

    //private void ReceiveSCReconnectRoom(TcpServerSession serverSession_, SBPacket packet_)
    //{
    //    //현재는 할게없는 빈패킷
    //    SCReconnectRoom resPacket = null;
    //    Deserialize(serverSession_, packet_, out resPacket);
    //    if (resPacket == null)
    //        return;

    //    if (resPacket.UserId == Managers.UserData.MyUserID)
    //    {
    //        if (resPacket.Reason == (byte)ReconnetReason.Cancel)
    //        {   
    //            PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("게임취소안내"));
    //        }
    //        if (resPacket.Reason == (byte)ReconnetReason.EnterFailed)
    //        {
    //            PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("ui_error_reconnection"));
    //        }
    //    }
    //}

    private void ReceiveSCGameRoomReconnect(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCGameRoomReconnect resPacket);
        if (resPacket == null)
            return;

        Managers.PlayData.ResumeGameDataPacket = resPacket;
    }

    private void ReceiveSCBcGameRoomReconnect(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcGameRoomReconnect resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;


        if (resPacket.PlayerInfo.ObjectId == Managers.UserData.MyUserID.ToString())
        {
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        CharacterObject charObj = Managers.Object.AddPlayer(resPacket.PlayerInfo);
        if (charObj != null)
            charObj.StopInvisible();
    }

    private void ReceiveSCBcDisconnect(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcDisconnect resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.UserId == Managers.UserData.MyUserID)
        {
            if(resPacket.Reason == (byte)DisconnectReason.Dodge)
            {
                Managers.Instance.Dodge();
            }
            else
            {
                string msg = StringManager.GetString("서버연결종료");
                switch (resPacket.Reason)
                {
                    case 1:
                        msg += "\n" + StringManager.GetString("SessionExpire");
                        break;
                    case 2:
                        msg += "\n" + StringManager.GetString("LostUserInstance");
                        break;
                    case 3:
                        msg += "\n" + StringManager.GetString("LostGame");
                        break;
                    case 4:
                        msg += "\n" + StringManager.GetString("InvalidGameRoom");
                        break;
                    case 5:
                        msg += "\n" + StringManager.GetString("Dodge");
                        break;
                    case 6:
                        msg += "\n" + StringManager.GetString("ServerCrash");
                        break;
                    case 7:
                        msg += "\n" + StringManager.GetString("Cancel");
                        break;
                    case 8:
                        msg += "\n" + StringManager.GetString("TimeOut");
                        break;
                    case 9:
                        msg += "\n" + StringManager.GetString("GamePlayEnd");
                        break;
                    case 0:
                    default:
                        break;
                }

                PopupCanvas.Instance.ShowMessagePopup(msg, () =>
                {
                    Managers.Network.Disconnect();
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
                });
            }
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        CharacterObject charObj = Managers.Object.FindCharacterById(resPacket.UserId.ToString());
        if (charObj != null)
            charObj.DisconnectCharacter();
    }

    private void ReceiveSCSystemMessageNotify(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCSystemMessageNotify resPacket);
        if (resPacket == null)
            return;

        PopupCanvas.Instance.ShowServerNotifyText(resPacket.Message);        
    }
    private void ReceiveSCSeasonRank(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCSeasonRank resPacket);
        if (resPacket == null)
            return;

        (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.RANK_POPUP) as RankPopup).GetRanDataInit(resPacket);
    }

    private void ReceiveSCBridgeServerNotConnectNotify(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCBcEmoticon resPacket);
        if (resPacket == null)
            return;

        SBDebug.Log("SCBridgeServerNotConnectNotify");

        PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("게임서버오류"), ()=>
        {
            Managers.Network.Disconnect();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
        });
    }

    private void ReceiveSCLoobyDisconnectNotify(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCLoobyDisconnectNotify resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;
        string msg = StringManager.GetString("서버연결종료");
        switch (resPacket.Reason)
        {
            case 1:
                msg += "\n" + StringManager.GetString("SessionExpire");
                break;
            case 2:
                msg += "\n" + StringManager.GetString("LostUserInstance");
                break;
            case 3:
                msg += "\n" + StringManager.GetString("LostGame");
                break;
            case 4:
                msg += "\n" + StringManager.GetString("InvalidGameRoom");
                break;
            case 5:
                msg += "\n" + StringManager.GetString("Dodge");
                break;
            case 6:
                msg += "\n" + StringManager.GetString("ServerCrash");
                break;
            case 7:
                msg += "\n" + StringManager.GetString("Cancel");
                break;
            case 8:
                msg += "\n" + StringManager.GetString("TimeOut");
                break;
            case 9:
                msg += "\n" + StringManager.GetString("GamePlayEnd");
                break;
            case 0:
            default:
                break;
        }

        PopupCanvas.Instance.ShowMessagePopup(msg, () =>
        {
            Managers.Network.Disconnect();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
        });
    }
    
}
