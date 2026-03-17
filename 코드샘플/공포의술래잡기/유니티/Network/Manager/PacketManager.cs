using System;
using System.Collections.Generic;
using SBCommonLib;
using SBSocketClientLib;
using SBSocketPacketLib;
using SBSocketPacketLib.Serializer;
using SBSocketSharedLib;

public partial class PacketManager : SBCommonLib.SBSingleton<PacketManager>
{
    public delegate void ApiStandardCallback(TcpServerSession serverSession_, SBPacket packet_);
    private Dictionary<PacketId, ApiStandardCallback> apiHandlers;

    public PacketManager()
    {
        apiHandlers = new Dictionary<PacketId, ApiStandardCallback>()
        {
            { PacketId.SCInit, ReceiveSCInit },
            { PacketId.SCBcChat, ReceiveSCBcChat },
            { PacketId.SCBcEnterLobby, ReceiveSCBcEnterLobby },
            { PacketId.SCBcLeaveLobby, ReceiveSCBcLeaveLobby },
            { PacketId.SCBcEnterGame, ReceiveSCBcEnterGame },
            { PacketId.SCBcLeaveGame, ReceiveSCBcLeaveGame },
            { PacketId.SCBcMoveStart, ReceiveSCBcMoveStart },
            { PacketId.SCObject, ReceiveSCObject },
            { PacketId.SCObjectList, ReceiveSCObjectList },
            // { PacketId.SCBcMatch, ReceiveSCBcMatch },
            { PacketId.SCBcSpawn, ReceiveSCBcSpawn },
            { PacketId.SCBcDespawn, ReceiveSCBcDespawn },
            { PacketId.SCBcDamaged, ReceiveSCBcDamaged },
            { PacketId.SCBcGameResult, ReceiveSCBcGameResult },
            { PacketId.SCBcStateChanged, ReceiveSCBcStateChanged },
            { PacketId.SCBcMoveEnd, ReceiveSCBcMoveEnd },
            { PacketId.SCBcMoveUpdate, ReceiveSCBcMoveUpdate },
            { PacketId.SCBcUnlockKey, ReceiveSCBcUnlockKey },
            { PacketId.SCSkill, ReceiveSCSkill },
            { PacketId.SCBcSkill, ReceiveSCBcSkill },
            { PacketId.SCSkillCasting,ReceiveSCSkillCasting },
            { PacketId.SCBcSkillCasting,ReceiveSCBcSkillCasting },
            { PacketId.SCBcBroken, ReceiveSCBcBroken },
            { PacketId.SCBcProjectileSpawn, ReceiveSCBcProjectileSpawn },
            { PacketId.SCBcProjectileDespawn, ReceiveSCBcProjectileDespawn },
            { PacketId.SCBcUnlockEscape, ReceiveSCBcUnlockEscape },
            { PacketId.SCBcEscapeRoom, ReceiveSCBcEscapeRoom },
            { PacketId.SCBcUseVehicle, ReceiveSCBcUseVehicle },
            { PacketId.SCBcGetOffVehicle, ReceiveSCBcGetOffVehicle },
            { PacketId.SCBcMatchInfo, ReceiveSCBcMatchInfo },
            { PacketId.SCBcKeyStart, ReceiveBcSCKeyStart },
            { PacketId.SCBcKeyEnd, ReceiveSCBcKeyEnd },
            { PacketId.SCBcKeyNotify, ReceiveSCBcKeyNotify },
            { PacketId.SCBcEscapeStart, ReceiveSCBcEscapeStart },
            { PacketId.SCBcEscapeEnd, ReceiveSCBcEscapeEnd },
            { PacketId.SCBcEscapeNotify, ReceiveSCBcEscapeNotify },
            { PacketId.SCPong, ReceiveSCPong },
            { PacketId.SCBCGameStart, ReceiveSCBcGameStart },
            // { PacketId.SCBcGameReady, ReceiveSCBcGameReady },
            // { PacketId.SCBcMatchSelectCharacter, ReceiveSCBcMatchSelectCharacter },
            { PacketId.SCBcCreateBuffs, ReceiveSCBcCreateBuffs },
            { PacketId.SCBcDeleteBuffs, ReceiveSCBcDeleteBuffs },
            { PacketId.SCBcStatusSync, ReceiveSCBcStatusSync },
            { PacketId.SCBatteryCreater, ReceiveSCBatteryCreater },
            { PacketId.SCBcBatteryCreate, ReceiveSCBcBatteryCreate },
            { PacketId.SCBcBatteryDrop, ReceiveSCBcBatteryDrop },
            { PacketId.SCBcBatteryPickUp, ReceiveSCBcBatteryPickUp },
            { PacketId.SCBcBatteryGenerater, ReceiveSCBcBatteryGenerater },
            { PacketId.SCBcDeleteDropObjects, ReceiveScBcDeleteDropObjects },
            { PacketId.SCDuplicateLogin, ReceiveScDuplicateLogin },
            { PacketId.SCNotifyReconnect, ReceiveSCNotifyReconnect },
            //{ PacketId.SCReconnectRoom, ReceiveSCReconnectRoom },
            { PacketId.SCGameRoomReconnect, ReceiveSCGameRoomReconnect },
            { PacketId.SCBcGameRoomReconnect, ReceiveSCBcGameRoomReconnect },
            { PacketId.SCBcDisconnect, ReceiveSCBcDisconnect },
            { PacketId.SCBcGamePoint, ReceiveScBcGamePoint },
            { PacketId.SCBcEmoticon, ReceiveSCBcEmoticon },

            { PacketId.SCResMatch, ReceiveSCResMatch },
            { PacketId.SCResMatchCancel, ReceiveSCResMatchCancel },
            { PacketId.SCBcCreateGameRoom, ReceiveSCBcCreateGameRoom },
            { PacketId.SCResExerciseRoomCreate, ReceiveSCResExerciseRoomCreate },
            { PacketId.SCResExerciseMatch, ReceiveSCResExerciseMatch },
            { PacketId.SCResExerciseRoomPassword, ReceiveSCResExerciseRoomPassword },
            
            //서버 분리 작업
            { PacketId.LCGameServerEnter, ReceiveSCResLCGameServerEnter },
            { PacketId.GCGameServerEnter, ReceiveSCResGameServerEnter },
            //안쓰는 예전 패킷이라 정리
            //{ PacketId.GCBcGamePlayerInfo, ReceiveSCResGameRoomPlayerList },
            //{ PacketId.GCGameRoomInfo, ReceiveGCGameRoomInfo },

            //for DUO
            { PacketId.SCFriendList, ReceiveSCFriendList },
            { PacketId.SCDuoInvite, ReceiveSCDuoInvite },
            { PacketId.SCDuoInviteNotify, ReceiveSCDuoInviteNotify },
            { PacketId.SCDuoAccept, ReceiveSCDuoAccept },
            { PacketId.SCDuoAcceptNotify, ReceiveSCDuoAcceptNotify },
            { PacketId.SCDuoGuestMatch, ReceiveSCDuoGuestMatch },
            { PacketId.SCDuoGuestMatchNotify, ReceiveSCDuoGuestMatchNotify },
            { PacketId.SCDuoMatch, ReceiveSCDuoMatch },
            { PacketId.SCDuoCancel, ReceiveSCDuoCancel },
            { PacketId.SCDuoCharacterChange, ReceiveSCDuoCharacterChange },
            { PacketId.SCDuoClearNotify, ReceiveSCDuoClearNotify },

            { PacketId.SCOneOnOneMessage, ReceiveSCOneOnOneMessage },
            { PacketId.SCSystemMessageNotify, ReceiveSCSystemMessageNotify },
            { PacketId.SCSeasonRank, ReceiveSCSeasonRank },
            { PacketId.SCBridgeServerNotConnectNotify, ReceiveSCBridgeServerNotConnectNotify},
            { PacketId.SCMatchServerCrashNotify, ReceiveSCMatchServerCrashNotify},
            { PacketId.SCLoobyDisconnectNotify, ReceiveSCLoobyDisconnectNotify},
            
            //CLAN
            { PacketId.SCClanChat, ReceiveSCClanChatMessage },
            { PacketId.SCClanMemberList, ReceiveSCClanMemberList },
        };
    }

    public Action<TcpServerSession, SBPacket, ushort> CustomHandler { get; set; }

    public void OnRecvPacket(Session session_, ArraySegment<byte> buffer_, Action<Session, SBPacket> onRecvCallback = null)
    {
        SBPacket recvPacket = new SBPacket(buffer_);
        // SBDebug.Log($"[PacketManager] OnRecvPacket - packetId: {(PacketId)recvPacket.PacketId}, segment - ( offset: {buffer_.Offset}, count: {buffer_.Count})");
        if (onRecvCallback != null)
            onRecvCallback.Invoke(session_, recvPacket);
        else
            HandlePacket(session_, recvPacket);
#if UNITY_EDITOR
        Managers.GameServer.RecievedPacket();
#endif
    }

    public void HandlePacket(Session session_, SBPacket packet_)
    {
        TcpServerSession serverSession = session_ as TcpServerSession;

        // handler
        PacketId packetId = (PacketId)packet_.PacketId;
        if (packetId >= PacketId.TypeMax)
        {
            SBDebug.LogError($"[PacketManager] HandlePacket - 패킷 타입 (PacketType: {packetId}, {(ushort)packetId})");
            SBLog.PrintError($"[PacketManager] PacketType: {packetId} ({(ushort)packetId})");
            return;
        }

        if (apiHandlers.ContainsKey(packetId))
        {
            apiHandlers[packetId].Invoke(serverSession, packet_);
        }
    }

    private void Deserialize<T>(TcpServerSession serverSession_, SBPacket packet_, out T resPacket)
    {
        resPacket = SBMessagePack.Deserialize<T>(serverSession_.CryptKey, serverSession_.CryptIV, packet_.Data);
        if (null == resPacket)
        {
            SBDebug.LogError("!!!!!!!!!!!!!!!!!!!! Packet Deserialize Fail. !!!!!!!!!!!!!!!!!!!!");
        }
    }
}
