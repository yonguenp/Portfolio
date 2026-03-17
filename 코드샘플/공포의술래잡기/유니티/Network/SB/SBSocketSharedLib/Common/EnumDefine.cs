using System;
using System.Collections.Generic;
using System.Text;

namespace SBSocketSharedLib
{
    public enum GameObjectType : byte
    {
        None = 0,
        Player = 1,
        MapObject = 2,
        Projectile = 3,
        Block = 4,
        //Monster = 3,
        //Projectile = 4,
        //FieldObject = 5,
    }

    //public enum CreatureType : byte
    //{
    //    None = 0,
    //    Player = 1,
    //    Npc = 2,
    //    Monster = 3,
    //}

    public enum PlayerType : byte
    {
        None = 0,
        Chaser = 1,
        Survivor = 2,
        AllPlayer = 3,  //전체 플레이어
    }

    public enum TargetType : byte
    {
        None = 0x0,
        Chaser = 0x1,
        Survivor = 0x2,
        Object = 0x4,
        ChaserSurvivor = Chaser | Survivor,
        ChaserObject = Chaser | Object,
        SurvivorObject= Survivor | Object,
        All = Chaser | Survivor | Object,
    }

    public enum MapObjectType : byte
    {
        None = 0,
        Hide = 1,
        Key = 2,
        ExitDoor = 3,
        //Escape = 4,
        Obstruct = 4,
        Vehicle = 5,
        Vent = 6,
        BatteryCreater = 7,
        BatteryGenerater = 8,
        Block=9,                //방해물 오브젝트
    }

    public enum KeyObjectType : byte
    {
        None = 0,
        DistributionBox = 1,
        Escape = 2,
    }

    public enum DoorType : byte
    {
        None = 0,
        Unlocked = 1,
        locked = 2,
    }

    public enum ObstacleSightType : byte
    {
        None = 0,
        See = 1,
        NotSee = 2,
    }

    public enum VehicleType : byte
    {
        None = 0,
        Ringer = 1,
        Barrel = 2,
        Cart = 3,
    }

    //public enum Inanimate : byte
    //{
    //    None = 0,
    //}

    public enum CreatureStatus : byte
    {
        None = 0,
        Idle = 1,
        Moving,
        Hiding,
        Groggy,
        Dead,
        Repairing,  //수리
        Escape,     //탈출
    }

    public enum MoveStatus : byte
    {
        None = 0,
        Walk = 1,
        Run = 2,
        Jump = 3,
        Chase = 4,
        Pull = 5,
        Push = 6,
        Knockback = 7,
        Teleport = 8,
        Riding = 9,
        Vent = 10,
        Pluck = 11,
    }

    //public enum Stat : byte
    //{
    //    None = 0x0,
    //    Hp = 0x1,
    //    Attack = 0x2,
    //    MoveSpeed = 0x4,
    //}

    public enum Direction : byte
    {
        None = 0x0,
        Up = 0x1,
        Down = 0x2,
        Left = 0x4,
        Right = 0x8,
        UpLeft = Up | Left,
        UpRight = Up | Right,
        DownLeft = Down | Left,
        DownRight = Down | Right,
    }

    public enum SkillEffectType : byte
    {
        None = 0,
        Stun = 1,
        StaminaControl = 2,
        FovControl = 3,
        Attack = 4,
        Teleport = 5,
        Search = 6,
        SpeedControl = 7,
    }

    public enum RangeType : byte
    {
        None = 0,
        Front = 1,
        Square = 2,
    }

    public enum RoomType : byte
    {
        None = 0,
        Lobby = 1,
        Game = 2,
        Chat = 3,
        Match = 4,
    }

    public enum ChangedStat : byte
    {
        None = 0,
        CreatureStatus = 1,
        MoveDir = 2,
        Position = 3,
        Hp = 4,
        Stamina = 5,
        MoveSpeed = 6,
        AttackCooltime = 7,
    }

    public enum DamageType : byte
    {
        None = 0,
        Attack,         //공격
        Heal,           //회복
        Miss,           //회피
        Critical,       //크리티컬
        DotAttack,      //도트 공격
    }

    public enum RePlayerType : byte
    {
        None = 0,
        NormalPlayer,   //일반 플레이어
        OutPlayer,      //세션이 끊긴 플레이어
        DodgePlayer,    //닷지된 플레이어
    }

    public enum GameRoomType : byte
    {
        RankGame = 1,       //랭크게임
        ExerciseGame,       //연습게임
    }

    public enum DisconnectReason : byte
    {
        None = 0,
        SessionExpire,
        LostUserInstance,
        LostGame,
        InvalidGameRoom,
        Dodge,
        ServerCrash,		//서버 다운
        Cancel,             //클라에서 보내는 접속 취소
        TimeOut,            //일정시간동안 ping인안되서 연결 종료
        GamePlayEnd,        //게임방 플레이가 끝났다.
    }


    public enum UserPlayState : int
    {
        None = 0,        //비접속
        Lobby = 1 << 0,   //로비
        MatchReq = 1 << 1,   //매칭 요청
        Match = 1 << 2,   //매칭중
        GameCreate = 1 << 3,	//게임 생성중
        GameReq = 1 << 4,   //게임서버 접속중
        Game = 1 << 5,   //게임중
    }

    public enum DuoState : byte
    {
        None = 0,
        DuoInvite,              //듀오중
        DuoMatchPossible,       //듀오매칭중
        Gaming,					//게임중
    }

    public enum DuoType : byte
    {
        None = 0,
        NormalDuo,          //일반 듀오
        ClanDuo,            //클랜 듀오
    }

    public enum MatchReason : byte
    {
        None = 0,
        UserisNull,         //유저가 없다.
        MatchRoomisNull,    //매칭중이 룸이 없다.
        CreateGameRoom,     //게임룸 생성중
        NotMatchState,		//매치중인 상태가 아니다.
    }
}
