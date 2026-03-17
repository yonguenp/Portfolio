using SBCommonLib;
using SBSocketSharedLib;
using System.Collections.Generic;
using UnityEngine;

public class PlayDataManager
{
    public enum GAME_RESULT
    {
        UNKNOWN,
        WIN,
        LOSE,
        ESCAPED,
    };

    public GAME_RESULT GameResult { get; private set; } = GAME_RESULT.UNKNOWN;
    public GameRoomInfo GameRoomInfo { get; private set; }
    public bool AutomaticallyEnterGame { get; private set; }
    public IList<RoomPlayerInfo> RoomPlayers { get; private set; }
    public Dictionary<string, SkillGameData> RoomPlayerActiveSkillData { get; private set; }        // 추격자, 생존자 모두 있는 액티브 스킬의 기본 정보 저장
    public Dictionary<string, SkillGameData> RoomChaserSkillData { get; private set; }              // 추격자만 보유하고 있는 평타 스킬의 기본 정보 저장

    public bool IsResume { get; set; } = false;
    public SCGameRoomReconnect ResumeGameDataPacket = null;
    public bool GameRank_Warning { get; set; } // 랭크 게임 경고

    public void SetRoomPlayers(IList<MatchPlayerInfo> players)
    {
        SBDebug.Log("PlayDataManager SetRoomPlayers");

        if (RoomPlayers != null)
            RoomPlayers.Clear();

        RoomPlayers = new List<RoomPlayerInfo>();

        foreach (var player in players)
        {
            SBDebug.Log($"[UserID]{player.UserId}, [UserName]{player.UserName}");
            var roomPlayer = new RoomPlayerInfo();
            roomPlayer.UserId = player.UserId;
            roomPlayer.UserName = player.UserName;
            roomPlayer.RankPoint = player.RankPoint;
            RoomPlayers.Add(roomPlayer);
        }
        SBDebug.Log("==============================");
    }

    public void SetRoomPlayers(RoomInfo room)
    {
        List<RoomPlayerInfo> players = new List<RoomPlayerInfo>(new RoomPlayerInfo[room.ChaserList.Count + room.SurvivorList.Count]);
        IList<RoomPlayerInfo> notFoundSlotPlayers = new List<RoomPlayerInfo>();
        foreach (KeyValuePair<string, RoomPlayerInfo> it in room.ChaserList)
        {
            bool found = false;
            if (RoomPlayers != null)
            {
                long id = long.Parse(it.Key);
                for (int i = 0; i < RoomPlayers.Count; i++)
                {
                    if (RoomPlayers[i] != null && RoomPlayers[i].UserId == id)
                    {
                        players[i] = it.Value;
                        found = true;
                        break;
                    }
                }
            }

            if (found == false)
            {
                notFoundSlotPlayers.Add(it.Value);
            }
        }
        foreach (KeyValuePair<string, RoomPlayerInfo> it in room.SurvivorList)
        {
            bool found = false;
            if (RoomPlayers != null)
            {
                long id = long.Parse(it.Key);
                for (int i = 0; i < RoomPlayers.Count; i++)
                {
                    if (RoomPlayers[i] != null && RoomPlayers[i].UserId == id)
                    {
                        players[i] = it.Value;
                        found = true;
                        break;
                    }
                }
            }

            if (found == false)
            {
                notFoundSlotPlayers.Add(it.Value);
            }
        }

        foreach (var it in notFoundSlotPlayers)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == null)
                {
                    players[i] = it;
                    break;
                }
            }
        }

        RoomPlayers = players;

        // var RoomScene = Managers.Scene.CurrentScene as RoomScene;
        // if (RoomScene == null)
        //     return;

        // RoomScene.RoomRefresh();
    }

    public void ClearPlayerSkillData()
    {
        // if (RoomPlayerActiveSkills == null) { RoomPlayerActiveSkills = new Dictionary<string, SkillBaseGameData[]>(); }
        // else { RoomPlayerActiveSkills.Clear(); }

        // if (RoomChaserSkills == null) { RoomChaserSkills = new Dictionary<string, SkillBaseGameData[]>(); }
        // else { RoomChaserSkills.Clear(); }

        if (RoomPlayerActiveSkillData == null) { RoomPlayerActiveSkillData = new Dictionary<string, SkillGameData>(); }
        else { RoomPlayerActiveSkillData.Clear(); }

        if (RoomChaserSkillData == null) { RoomChaserSkillData = new Dictionary<string, SkillGameData>(); }
        else { RoomChaserSkillData.Clear(); }
    }

    public void AddPlayerActiveSkillData(long userId, int characterId)
    {
        SBDebug.Log($"AddPlayerActiveSkillData [UserId]{userId}, [characterId]{characterId}");
        var characterData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character, characterId) as CharacterGameData;

        // RoomPlayerActiveSkills.Add(userId, characterData.skillBaseData);
        RoomPlayerActiveSkillData.Add(userId.ToString(), characterData.GetSkillData());

        if (CharacterGameData.IsChaserCharacter(characterId))
        {
            // RoomChaserSkills.Add(userId, characterData.atkSkillBaseData);
            RoomChaserSkillData.Add(userId.ToString(), characterData.GetAtkSkillData());
        }
    }

    public void SetRoomPlayer(RoomPlayerInfo player)
    {
        if (RoomPlayers == null)
            return;

        for (int i = 0; i < RoomPlayers.Count; i++)
        {
            RoomPlayerInfo info = RoomPlayers[i];
            if (info.UserId == player.UserId)
            {
                //if (RoomPlayers[i].SlotNo != player.SlotNo)
                //    SBDebug.LogError($"OriginSlotIndex : {RoomPlayers[i].SlotNo}, CurSlotIndex : {player.SlotNo}");
                RoomPlayers[i] = player;
            }
        }

        // var roomScene = Managers.Scene.CurrentScene as RoomScene;
        // if (roomScene == null)
        //     return;

        // roomScene.RoomRefresh();
    }

    public void Clear()
    {
        if (RoomPlayers != null)
            RoomPlayers.Clear();
        IsResume = false;
    }

    public void SetGameRoomInfo(SCBcCreateGameRoom matchInfo)
    {
        GameRoomInfo = new GameRoomInfo();
        SetRoomPlayers(matchInfo.RoomInfo);

        GameRoomInfo.RoomInfo = matchInfo.RoomInfo;
        GameRoomInfo.MapId = matchInfo.RoomInfo.MapNo;
        GameRoomInfo.PlayerCount = matchInfo.RoomInfo.PlayerObjectList.Count;
        GameRoomInfo.PlayTime = matchInfo.GameTimeLimit;
        GameRoomInfo.EscapeTime = matchInfo.EscapeTimeLimit;
        GameRoomInfo.TargetEscapeCount = 3; // TODO : 임시

        int generator_uid = 0;
        var MapTypeData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.map_type, matchInfo.RoomInfo.MapNo);
        if (MapTypeData != null)
        {
            var objectList = (MapTypeData as MapTypeGameData).GetMapData();

            if (objectList != null)
            {
                foreach (ObjectData item in objectList.Objects)
                {
                    if (item.Type == ObjectGameData.MapObjectType.BatteryGenerater)
                    {
                        var objectID = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.@object, item.ObjectId) as ObjectGameData;
                        generator_uid = objectID.sub_obj_uid;
                    }
                }

            }
        }
        if (generator_uid == 0)
            SBDebug.LogError("게임 데이터 오류로 오브젝트 ID를 찾아올 수 없는 문제가 있습니다.  발전기 번호가 0번일 수 없음");          //여기로 들어오면 안됌

        ObjectBatteryGeneratorGameData batteryGeneratorData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.object_battery_generator, generator_uid) as ObjectBatteryGeneratorGameData;
        GameRoomInfo.BatteryGeneratorCoolTime = (int)(batteryGeneratorData.cooltime_time * 1000);
        GameRoomInfo.BatteryGeneratorActiveTime = (int)(batteryGeneratorData.active_time * 1000);

        GameResult = GAME_RESULT.UNKNOWN;
        Managers.UserData.ResetMyGameResult();
        Managers.UserData.ResetGameResult();

        SBDebug.Log($"GameHelper SetGameRoomInfo");
        for (int i = 0; i < matchInfo.RoomInfo.ChaserList.Count; ++i)
        {
            SBDebug.Log($"ChaserId {matchInfo.RoomInfo.ChaserList[i].Key}");
        }
    }

    /// 예전 패킷으로 인해 현재 안씀
    //public void SetGameRoomInfo(GCGameRoomInfo matchInfo)
    //{
    //    GameRoomInfo = new GameRoomInfo();
    //    SetRoomPlayers(matchInfo.RoomInfo);

    //    GameRoomInfo.RoomInfo = matchInfo.RoomInfo;
    //    GameRoomInfo.MapId = matchInfo.RoomInfo.MapNo;
    //    GameRoomInfo.PlayerCount = matchInfo.RoomInfo.PlayerObjectList.Count;
    //    GameRoomInfo.PlayTime = matchInfo.GameTimeLimit;
    //    GameRoomInfo.EscapeTime = matchInfo.EscapeTimeLimit;
    //    GameRoomInfo.TargetEscapeCount = 3; // TODO : 임시

    //    ObjectBatteryGeneratorGameData batteryGeneratorData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.object_battery_generator, 80001) as ObjectBatteryGeneratorGameData;
    //    GameRoomInfo.BatteryGeneratorCoolTime = (int)(batteryGeneratorData.cooltime_time * 1000);
    //    GameRoomInfo.BatteryGeneratorActiveTime = (int)(batteryGeneratorData.active_time * 1000);

    //    GameResult = GAME_RESULT.UNKNOWN;
    //    Managers.UserData.ResetMyGameResult();
    //    Managers.UserData.ResetGameResult();

    //    SBDebug.Log($"GameHelper SetGameRoomInfo");
    //    for (int i = 0; i < matchInfo.RoomInfo.ChaserList.Count; ++i)
    //    {
    //        SBDebug.Log($"ChaserId {matchInfo.RoomInfo.ChaserList[i].Key}");
    //    }
    //}

    //public void SetGameRoomPlayerList(GCBcGamePlayerInfo playerInfo)
    //{
    //    GameRoomInfo.RoomInfo.PlayerObjectList = playerInfo.PlayerObjectList;
    //}

    public void ResetGameRoomInfo()
    {
        GameRoomInfo = null;
    }

    public GameRoom CreateGameRoom()
    {
        if (GameRoomInfo == null)
        {
            //assert
            return null;
        }

        SBDebug.Log("SetGameRoomInfo");

        var gameRoom = new GameRoom();

        gameRoom.RoomInfo = GameRoomInfo.RoomInfo;
        gameRoom.SetPlayerCount((byte)GameRoomInfo.RoomInfo.PlayerObjectList.Count);
        SBDebug.Log("==============SetPlayerCount");

        for (int i = 0; i < gameRoom.RoomInfo.ChaserList.Count; ++i)
        {
            SBDebug.Log($"ChaserId {gameRoom.RoomInfo.ChaserList[i].Key}");
        }

        SBDebug.Log("==============Set Complete");

        Game.Instance.SetGameRoomData(gameRoom);

        return gameRoom;
    }

    public void ClearAutoEnterGame()
    {
        AutomaticallyEnterGame = false;
    }

    public void SetAutoEnterGame()
    {
        AutomaticallyEnterGame = true;
    }

    public bool AmIChaser()
    {
        return IsChaserPlayer(Managers.UserData.MyUserID);
    }
    public bool IsChaserPlayer(long id)
    {
        return IsChaserPlayer(id.ToString());
    }
    public bool IsChaserPlayer(string id)
    {
        if (Managers.PlayData == null || Managers.PlayData.GameRoomInfo == null || Managers.PlayData.GameRoomInfo == null)
            return false;

        foreach (var chaser in Managers.PlayData.GameRoomInfo.RoomInfo.ChaserList)
        {
            if (chaser.Key == id)
            {
                return true;
            }
        }

        return false;
    }

    public RoomPlayerInfo GetMyRoomPlayerInfo()
    {
        return GetRoomPlayer(Managers.UserData.MyUserID);
    }

    public RoomPlayerInfo GetRoomPlayer(long id)
    {
        if (Managers.PlayData.RoomPlayers != null)
        {
            foreach (RoomPlayerInfo roomPlayer in RoomPlayers)
            {
                if (roomPlayer.UserId == id)
                {
                    return roomPlayer;
                }
            }
        }

        return null;
    }

    public RoomPlayerInfo GetRoomPlayer(string strID)
    {
        if (long.TryParse(strID, out long id))
            return GetRoomPlayer(id);
        else
            return null;
    }

    public int GetSlotIndex(long id)
    {
        bool isChaser = IsChaserPlayer(id);
        int index = 0;
        if (Managers.PlayData.RoomPlayers != null)
        {
            foreach (RoomPlayerInfo roomPlayer in RoomPlayers)
            {
                if (roomPlayer.UserId == id)
                {
                    return index;
                }

                if (isChaser == IsChaserPlayer(roomPlayer.UserId))
                    index++;
            }
        }

        return -1;
    }
    public int GetSlotIndex(string id)
    {
        if (long.TryParse(id, out long result))
        {
            return GetSlotIndex(result);
        }

        return -1;
    }

    public void OnGameResult(SCBcGameResult result)
    {
        GameResult = GAME_RESULT.LOSE;

        if (AmIChaser())
        {
            if (result.GameResult == 1)
            {
                GameResult = GAME_RESULT.WIN;
            }
        }
        else
        {
            if (result.GameResult == 2)
            {
                GameResult = GAME_RESULT.WIN;
            }
            else
            {
                CharacterObject charObj = Managers.Object.FindCharacterById(Managers.UserData.MyUserID);
                if (charObj != null)
                {
                    if (charObj.Escaped)
                        GameResult = GAME_RESULT.ESCAPED;
                }
            }
        }
    }
}
