using System;
using System.Collections.Generic;
using System.Text;

using SBCommonLib;
using SBSocketPacketLib;
using SBSocketPacketLib.Serializer;

namespace SBSocketSharedLib
{
    public enum PacketId : ushort
    {
        // pair
        CSInit,
        SCInit,

        CSEnterLobby,
        SCBcEnterLobby,

        CSLeaveLobby,
        SCBcLeaveLobby,

        CSChat,
        SCBcChat,

        CSEnterGame,
        SCBcEnterGame,

        CSLeaveGame,
        SCBcLeaveGame,
        
        CSMove,
        SCMove,

        CSMoveStart,
        SCBcMoveStart,

        CSMoveEnd,
        SCBcMoveEnd,

        CSSkill,
        SCSkill,

        SCBcUnlockKey,

        CSUnlockEscape,
        SCBcUnlockEscape,

        SCBcEscapeRoom,

        CSUseVehicle,
        SCBcUseVehicle,

        CSGetOffVehicle,
        SCBcGetOffVehicle,

        CSOneOnOneMessage,
        SCOneOnOneMessage,

        CSUseVent,

        // only use client
        CSHiding,

        // only use server
        SCObject,
        SCObjectList,
        SCDuplicateLogin,

        SCBcSpawn,
        SCBcDespawn,
        SCBcProjectileSpawn,
        SCBcProjectileDespawn,
        SCBcDamaged,
        SCBcGameResult,
        SCBcStateChanged,
        //SCBcDead,
        SCBcMoveUpdate,
        SCBcBroken,
        SCBcSkill,
        SCBcMatchInfo, // ToDo: 임시로 사용할 매치 정보 전달용 패킷
        SCBCGameStart,

        CSPing,
        SCPong,

        CSKeyStart,         //배전함 수리 시작
        SCBcKeyStart,       //가능여부및 주위 브로드

        SCBcKeyEnd,         //배전한 상태 브로드

        SCBcKeyNotify,      //배전함 알림

        CSEscapeStart,
        SCBcEscapeStart,

        SCBcEscapeEnd,

        SCBcEscapeNotify,



        CSSkillCasting,             //스킬 캐스팅
        SCSkillCasting,
        SCBcSkillCasting,


        SCBcCreateBuffs,            //버프 생성
        SCBcDeleteBuffs,            //버프 삭제
        SCBcStatusSync,               //스텟 변경시 동기화 할 패킷

        // for Dummy Test
        CSMatchWithDummy,
        CSEnterGameWithDummy,
        CSLeaveGameWithDummy,
        ////////////////////////////////////////

        CSBatteryCreater,           //배터리 생성기 액션
        SCBatteryCreater,           //배터리 생성기 액션 응답

        SCBcBatteryCreate,          //배터리 생성(아이템화는 테스트 이후에)
        SCBcBatteryDrop,            //배터리 드랍(아이템화는 테스트 이후에)

        SCBcBatteryPickUp,          //배터리 픽업(아이템화는 테스트 이후에)

        SCBcDeleteDropObjects,       //삭제되는 드랍 오브젝트들

        CSBatteryGenerater,         //배터리 충천
        SCBcBatteryGenerater,		//배터리 충전 결과

        SCNotifyReconnect,          //재접속
        CSReconnectRoom,


        SCBcDisconnect,
        SCGameRoomReconnect,        //재접속한 유저에게 게임룸 재접속
        SCBcGameRoomReconnect,      //게임방에 있는 유저들에게 새로온 접속한 유저 정보 전달

        SCBcGamePoint,              //서버에서 계산한 게임점수

        //매칭 로직 변경에 따른 새로운 패킷들 
        CSReqMatch,             //매칭 하기
        SCResMatch,

        CSReqMatchCancel,       //매칭 취소
        SCResMatchCancel,

        SCBcCreateGameRoom,     //게임룸생성

        //연습 매칭 관련
        CSReqExerciseRoomCreate,    //연습매칭 방 만들기
        SCResExerciseRoomCreate,    //연습매칭 방 만들기 응답

        CSReqExerciseMatch,     //연습매칭 요청
        SCResExerciseMatch,     //연습매칭 요청 응답

        CSReqExerciseRoomPassword,  //방 비번 입력
        SCResExerciseRoomPassword,	//방 비번 입력 응답

        CSEmoticon,                 //이모티콘 보내기
        SCBcEmoticon,               //이모티콘 응답


        LCGameServerEnter,           //접속할 게임서버 정보를 보내준다.

        CGGameServerEnter,          //게임서버 접속하기
        GCGameServerEnter,          //게임서버 접속 응답



        //듀오
        CSDuoInvite,                //듀오 초대
        SCDuoInvite,                //듀오 초대 응답
        SCDuoInviteNotify,          //듀오 초대 알림(guest에게) 

        CSDuoAccept,                //듀오 초대 결과
        SCDuoAccept,                //듀오 초대 결과 응답
        SCDuoAcceptNotify,          //듀오 초대 결과 알림 (host에게)

        CSDuoGuestMatch,            //듀오 게스트 랭크 매칭
        SCDuoGuestMatch,            //듀오 게스트 랭크 매칭 응답
        SCDuoGuestMatchNotify,      //듀오 게스트 랭크 매칭 알림 (host에게)

        CSDuoMatch,                 //듀오 매칭
        SCDuoMatch,

        CSDuoCancel,                //듀오 해제
        SCDuoCancel,

        CSFriendList,               //친구 리스트 요청
        SCFriendList,

        CSAlivePingNotify,          //살아 있다고 통보

        SCSystemMessageNotify,      //시스템 메세지 통보

        CSDuoCharacterChange,       //듀오 캐릭터 변경
        SCDuoCharacterChange,

        SCDuoClearNotify,           //듀오 해제

        //시즌 랭크
        CSSeasonRank,               //시즌 랭크 정보 요청
        SCSeasonRank,               //시즌 랭크 정보 응답

        //
        SCBridgeServerNotConnectNotify,     //브릿지 서버랑 연결이 끊어져 있을때 클라에 통보
        SCMatchServerCrashNotify,           //매칭서버 크래쉬 통보

        //
        SCLoobyDisconnectNotify,            //로비서버에서 연결 종료 하라고 클라에 통보


        //클랜
        CSClanInfoUpdateNotify,         //클랜 정보 최신화 요청

        CSClanChat,                    //클랜 채팅
        SCClanChat,

        CSClanMemberList,              //클랜원 정보
        SCClanMemberList,

        //테스트용 패킷
        CSTestInit,             //테스트용 접속 정보


        TypeMax,
    }


    public enum LoginResult : byte
    {
        Successed = 0,
        Failed = 1,
        WrongParam = 2,
        TokenNotMatch = 3,
        NotFindUserData = 4,
        WrongClientVersion = 5,
        Reconnect = 6,
    }

    public enum MoveResult : byte
    {
        Successed = 0,
        Failed = 1,
    }

    //public enum MoveFailReason : byte
    //{
    //    None = 0,
    //    CannotMoveYet = 1,
    //    InvalidPosition = 2,
    //}

    public enum MatchOpCode : byte
    {
        Request = 0,
        Cancel = 1,
    }

    public enum MatchResult : byte
    {
        Successed = 0,
        Failed = 1,
        RequestTimeout = 2,
        Cancelled = 3,
        Proceeding = 4,
        MatchRoomRelease = 5,
        NotFindUserData = 6,
        NotFindCharacterData = 7,
        NeedPassword = 8,
        DisUserPlayState = 9,
        DuoPlayer = 10,             //듀오 플레이어
    }

    public enum DespawnReason : byte
    {
        Dead = 0,
        Broken = 1,
        Timeout = 2,
    }

    public enum AttackResult : byte
    {
        Successed = 0,
        Failed = 1,
    }

    public enum FailReason : sbyte
    {
        None = 0,
        NotExistsTarget = 1,        //타겟이 없다
        NotReusableYet,             //시간 부족
        NotSkill,                   //스킬이 아니다
        NotStatus,                  //사용 가능한 상태가 아니다
        NotPosition,                //사용가능한 위치가 아니다
        NotCasting,                 //캐스팅이 아직 완료가 안되었다.
    }

    public enum GameResult : byte
    {
        None = 0,
        ChaserWin = 1,
        SurvivorWin = 2,
    }

    public enum UnlockStatus : byte
    {
        None = 0,
        OnGoing = 1,
        Unlocked = 2,
    }
}
