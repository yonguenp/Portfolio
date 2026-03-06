using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    //[Serializable]
    //public class Temp
    //{
    //    [Key(0)]
    //    public virtual int temp { get; set; }
    //}

    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class UserInfo
    {
        [Key(0)]
        public string UserId { get; set; }
        [Key(1)]
        public string UserName { get; set; }
        [Key(2)]
        public int Exp { get; set; }
        [Key(3)]
        public short Level { get; set; }
        [Key(4)]
        public int Elo { get; set; }
        [Key(5)]
        public int WinChaser { get; set; }
        [Key(6)]
        public int LoseChaser { get; set; }
        [Key(7)]
        public int WinSurvivor { get; set; }
        [Key(8)]
        public int LoseSurvivor { get; set; }
        [Key(9)]
        public int CurChaser { get; set; }
        [Key(10)]
        public int CurSurvivor { get; set; }
        [Key(11)]
        public short State { get; set; }
        [Key(12)]
        public int Gold { get; set; }
        [Key(13)]
        public int Dia { get; set; }
    }

    [MessagePackObject]
    [Serializable]
    public class SelectCharacterInfo
    {
        [Key(0)]
        public byte PlayerType { get; set; }     //1: 추격자, 2: 생존자
        [Key(1)]
        public int CharacterType { get; set; }       //캐릭터타입
        [Key(2)]
        public int Level { get; set; }     //캐릭터 레벨
        [Key(3)]
        public int Enhance_Step { get; set; }
        [Key(4)]
        public int Skill { get; set; } //캐릭터 스킬 레벨
        [Key(5)]
        public IList<int> ItemNos { get; set; } 
    }

    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class PositionInfo
    {
        [Key(0)]
        public byte Status { get; set; }
        [Key(1)]
        public byte MoveStatus { get; set; }
        [Key(2)]
        public Vec2Float MoveDir { get; set; }
        [Key(3)]
        public Vec2Float Pos { get; set; }
        //[Index(4)]
        //public int PosZ { get; set; }
    }

    
    [MessagePackObject]
    [Serializable]
    public class StatInfo
    {
        [Key(0)]
        public ushort Hp { get; set; }
        [Key(1)]
        public ushort MaxHp { get; set; }
        [Key(2)]
        public float MoveSpeed { get; set; } // 이동시간(ms) (한 칸 이동)
        [Key(3)]
        public float QuestSpeed { get; set; } // 퀘스트 속도
        [Key(4)]
        public ushort Attack { get; set; } // 공격력
        [Key(5)]
        public float ReduceAttackTime { get; set; } // 공속 쿨타임 감소 계수(ms)
        [Key(6)]
        public byte Shield_Cnt { get; set; }    //쉴드로 방어횟수
        [Key(7)]
        public byte Max_Battery { get; set; }    //맥스 배터리
        [Key(8)]
        public float ReduceSkillTime { get; set; }    //스킬 쿨타임 감소 계수(ms)
        [Key(9)]
        public float AttackDist { get; set; }    //일반 공격 거리값
        [Key(10)]
        public float SkillDist { get; set; }    //스킬 거리값
    }

#if false
    [MessagePackObject]
    [Serializable]
    public class CharacterDesignData
    {
        [Key(0)]
        public int Index { get; set; } = 0;
        [Key(1)]
        public int UID { get; set; } = 0;
        [Key(2)]
        public short Char_Type { get; set; } = 0;
        [Key(3)]
        public short Char_Grade { get; set; } = 0;
        [Key(4)]
        public int Char_Stat { get; set; } = 0;
        [Key(5)]
        public int Char_Skill { get; set; } = 0;
        [Key(6)]
        public int Char_Skill_Atk { get; set; } = 0;
    }
#endif
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SkillInfo
    {
        [Key(0)]
        public int SkillId { get; set; }
        [Key(1)]
        public int Damage { get; set; }
    }

    //[ZeroFormattable]
    //[Serializable]
    //public class MapObjectData
    //{
    //    [Key(0)]
    //    public int IndexNo { get; set; } // 오브젝트 번호
    //    [Key(1)]
    //    public string Name { get; set; } // 오브젝트 이름
    //    [Key(2)]
    //    public byte ObjectType { get; set; } // 오브젝트 타입(MapObjectType enum 참조)
    //    [Key(3)]
    //    public ushort ObjectUser { get; set; } // 해당 오브젝트를 사용할 진영(PlayerType enum 참조)
    //    [Key(4)]
    //    public ushort ObjectUsingTime { get; set; } // 해당 오브젝트를 사용하기 위한 시간(단위: ms, 0이면 즉시 사용)
    //    [Key(5)]
    //    public byte ObjectHp { get; set; } // 해당 오브젝트의 Hp
    //    [Key(6)]
    //    public ushort ObjectValue1 { get; set; } // 타입 별로 필요한 값 입력(Default: 0)
    //    [Key(7)]
    //    public byte ObjectValue2 { get; set; } // 타입 별로 필요한 값 입력(Default: 0)
    //    [Key(8)]
    //    public byte ObjectValue3 { get; set; } // 타입 별로 필요한 값 입력(Default: 0)
    //    [Key(9)] 
    //    public string ObjectResource { get; set; } // 해당 오브젝트의 리소스 ID 혹은 경로 입력
    //}

    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class PlayerObjectInfo
    {
        [Key(0)]
        public string ObjectId { get; set; }
        [Key(1)]
        public PositionInfo PosInfo { get; set; }
        [Key(2)]
        public StatInfo StatInfo { get; set; }
        [Key(3)]
        public string Name { get; set; }
    }

    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class MapObjectInfo
    {
        [Key(0)]
        public string MapObjectId { get; set; }
        [Key(1)]
        public int MapObjectIndex { get; set; }
        [Key(2)]
        public Vec2Float Position { get; set; }
        [Key(3)]
        public int PositionId { get; set; }
        [Key(4)]
        public byte Hp { get; set; }
    }

    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class RoomInfo
    {
        [Key(0)]
        public byte RoomType { get; set; }
        [Key(1)]
        public string RoomId { get; set; }
        [Key(2)]
        public int MapNo { get; set; }
        [Key(3)]
        public IList<KeyValuePair<string, RoomPlayerInfo>> ChaserList { get; set; }
        [Key(4)]
        public IList<KeyValuePair<string, RoomPlayerInfo>> SurvivorList { get; set; }
        [Key(5)]
        public IList<PlayerObjectInfo> PlayerObjectList { get; set; }
        [Key(6)]
        public IList<MapObjectInfo> MapObjectList { get; set; }
    }

    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class RoomPlayerInfo
    {
        [Key(0)]
        public long UserId { get; set; }
        [Key(1)]
        public string UserName { get; set; }
        [Key(2)]
        public SelectCharacterInfo SelectedCharacter { get; set; }
        [Key(3)]
        public int RankPoint { get; set; }
        [Key(4)]
        public long DuoKey { get; set; }
        [Key(5)]
        public string ClanName { get; set; }
    }

    [MessagePackObject]
    [Serializable]
    public class MatchPlayerInfo
    {
        [Key(0)]
        public long UserId { get; set; }
        [Key(1)]
        public string UserName { get; set; }
        [Key(2)]
        public int RankPoint { get; set; }
    }

    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class RespawnObjectInfo
    {
        [Key(0)]
        public string ObjectId { get; set; }
        [Key(1)]
        public Vec2Float Direction { get; set; }
        [Key(2)]
        public Vec2Float Position { get; set; }
    }

    [MessagePackObject]
    [Serializable]
    public class DropInfo
    {
        [Key(0)]
        public Vec2Float Pos { get; set; }
        [Key(1)]
        public int Cnt { get; set; }
        [Key(2)]
        public string GameObjectId { get; set; }
    }

    [MessagePackObject]
    [Serializable]
    public class RePlayerObjectInfo //재접속 유저 정보
    {
        [Key(0)]
        public string ObjectId { get; set; }
        [Key(1)]
        public PositionInfo PosInfo { get; set; }
        [Key(2)]
        public StatInfo StatInfo { get; set; }
        [Key(3)]
        public string Name { get; set; }
        [Key(4)]
        public int BatteryCnt { get; set; } //현재 가진 배터리 개수
        [Key(5)]
        public int PutBatteryCnt { get; set; }  //지금 까지 넣은 개수
        [Key(6)]
        public byte RePlayerType { get; set; }    //1 = 정상유저, 2= 세션이 끊긴 유저, 3=닷지된 유저 
    }

    [MessagePackObject]
    [Serializable]
    public class ReMapObjectInfo //재접속 유저 정보
    {
        [Key(0)]
        public string MapObjectId { get; set; }
        [Key(1)]
        public int MapObjectIndex { get; set; }
        [Key(2)]
        public Vec2Float Position { get; set; }
        [Key(3)]
        public int PositionId { get; set; }
        [Key(4)]
        public byte Hp { get; set; }
        [Key(5)]
	    public long RegenTime { get; set; }     //활성화 가능 시간
    }

    [MessagePackObject]
    [Serializable]
    public class ReSkillObjectInfo //재접속 유저 정보
    {
        [Key(0)]
        public string MapObjectId { get; set; }
        [Key(1)]
        public int SummonId { get; set; }
        [Key(2)]
        public Vec2Float Position { get; set; }
        [Key(3)]
        public byte Hp { get; set; }
    }

    [MessagePackObject]
    [Serializable]
    public class FriendInfo
    {
        [Key(0)]
        public long UserNo { get; set; }
        [Key(1)]
        public byte UserState { get; set; }
    }

    [MessagePackObject]
    [Serializable]
    public class SeasonRankUser
    {
        [Key(0)]
        public long UserNo { get; set; }
        [Key(1)]
        public int Point { get; set; }      //포인트
        [Key(2)]
        public int Rank { get; set; }       //등수
        [Key(3)]
        public int GradePoint { get; set; }       //등급 포인트
        [Key(4)]
        public string Nick { get; set; }
    }


    [MessagePackObject]
    [Serializable]
    public class ClanMemberInfo
    {
        [Key(0)]
        public long UserNo { get; set; }
        [Key(1)]
        public byte UserState { get; set; }
    }

    [MessagePackObject]
    [Serializable]
    public class DelGameObject
    {
        [Key(0)]
        public string ObjectId { get; set; }
        [Key(1)]
        public Vec2Float Position { get; set; }
    }

}
