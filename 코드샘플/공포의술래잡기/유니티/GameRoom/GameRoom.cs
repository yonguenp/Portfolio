using SBCommonLib;
using SBSocketSharedLib;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class GameRoomEvent : UnityEvent<GameRoom.eState, long> { }

public class GameRoom
{
    public enum eState
    {
        None,
        Lobby,
        //상태
        Ready,
        Play,
        GameOver,
    }

    public eState State { get; private set; } = eState.None;


    Dictionary<GameRoom.eState, GameRoomEvent> stateEvent = new Dictionary<eState, GameRoomEvent>();
    Dictionary<GameRoom.eState, GameRoomEvent> initStateEvent = new Dictionary<eState, GameRoomEvent>();

    public TimeManager GameTime { get { return Managers.TimeManager; } }
    private long gameStartTimestamp;
    private long gameEndTimestamp;
    private long gameOpenDoorTimestamp;

    public long GameStartTimestamp { get { return gameStartTimestamp; } }

    int leftTime = int.MaxValue;

    RoomInfo roomInfo = null;
    public RoomInfo RoomInfo
    {
        get { return roomInfo; }
        set { roomInfo = value; }
    }

    public float CurrentBatteryProgress { get; private set; } = 0f;
    public int CurrentBatteryCount { get; private set; } = 0;

    private Dictionary<string, int> chaserKillCount = new Dictionary<string, int>();
    private Dictionary<string, int> chaserVehicleKillCount = new Dictionary<string, int>();
    private Dictionary<string, int> survivorBatteryScore = new Dictionary<string, int>();

    public int CurrentEscapedSurvivorCount { get; private set; } = 0;

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void SetStateWithCallback(eState state)
    {
        State = state;
        InvokeInitStateEvent(state, -1);
    }

    public void SetPlayerCount(byte playerCount) { }

    //서버가 보내준 타임스탬프로 내 타임스탬프를 구한다.
    public void SetGameStartTimeStamp(long serverTimeStamp)
    {
        gameStartTimestamp = serverTimeStamp;
        gameEndTimestamp = gameStartTimestamp + Managers.PlayData.GameRoomInfo.PlayTime;

        State = eState.Ready;
        leftTime = int.MaxValue;

        LoadingUI.ClearLoadingUI();
    }

    //탈출 문이 열리면 고정된 시간(openDoorTimeStamp)으로 변경
    public void SetOpenDoor(long openDoorTimeStamp)
    {
        if (gameOpenDoorTimestamp != 0) return;
        gameOpenDoorTimestamp = GameTime.GetClientTimestamp(openDoorTimeStamp);
        //GameOpenDoorTimeStamp = openDoorTimeStamp;
        gameEndTimestamp = gameOpenDoorTimestamp + Managers.PlayData.GameRoomInfo.EscapeTime;
        leftTime = Managers.PlayData.GameRoomInfo.EscapeTime / 1000;
    }

    public bool IsAnyDoorOpened()
    {
        return gameOpenDoorTimestamp > 0;
    }

    //entergame으로 준비된 플레이어 카운팅
    //public void ReadyPlayer()
    //{
    //    readyPlayerCount++;
    //    if (readyPlayerCount == playerCount)
    //    {
    //        ReadyComplete();
    //    }
    //    else if (readyPlayerCount > playerCount)
    //    {
    //        SBDebug.LogError("Over PlayerCount ");
    //    }
    //}

    public void ReadyComplete()
    {
        InvokeInitStateEvent(eState.Ready, 0);
    }

    public void AddStateEvent(eState addState, UnityAction<eState, long> unityAction)
    {
        try
        {
            if (stateEvent.ContainsKey(addState) == true)
            {
                stateEvent[addState].AddListener(unityAction);
            }
            else
            {
                var ev = new GameRoomEvent();
                ev.AddListener(unityAction);
                stateEvent.Add(addState, ev);
            }
        }
        catch (Exception e)
        {
            SBDebug.LogException(e);
        }
    }

    public void RemoveGameRoomEvent(GameRoom.eState addState, UnityAction<GameRoom.eState, long> unityAction)
    {
        if (stateEvent.ContainsKey(addState) == true)
        {
            stateEvent[addState].RemoveListener(unityAction);
        }
        else
        {
            SBDebug.LogError($"Cant Find State[{addState.ToString()}]");
        }
    }

    public void AddInitStateEvent(GameRoom.eState addState, UnityAction<GameRoom.eState, long> unityAction)
    {
        try
        {
            if (initStateEvent.ContainsKey(addState) == true)
            {
                initStateEvent[addState].AddListener(unityAction);
            }
            else
            {
                var ev = new GameRoomEvent();
                ev.AddListener(unityAction);
                initStateEvent.Add(addState, ev);
            }
        }
        catch (Exception e)
        {
            SBDebug.LogException(e);
        }
    }

    public void RemoveStateInitEvent(GameRoom.eState addState, UnityAction<GameRoom.eState, long> unityAction)
    {
        if (initStateEvent.ContainsKey(addState) == true)
        {
            initStateEvent[addState].RemoveListener(unityAction);
        }
        else
        {
            SBDebug.LogError($"Cant Find State[{addState.ToString()}]");
        }
    }

    public long PassTimeFromGameStartTime()
    {
        var localTime = GameTime.GetClientTimestamp();
        return gameStartTimestamp - localTime;
    }

    public long PassTimeFromGameEndTime()
    {
        var localTime = GameTime.GetClientTimestamp();
        return gameEndTimestamp - localTime;
    }

    public void InvokeStateEvent(eState state, long value)
    {
        GameRoomEvent re = null;
        stateEvent.TryGetValue(state, out re);
        if (re != null)
            re.Invoke(state, value);
    }

    public void InvokeInitStateEvent(eState state, long value)
    {
        GameRoomEvent re = null;
        if (initStateEvent.TryGetValue(state, out re))
        {
            re?.Invoke(state, value);
        }
    }

    //update를 통해 타이머 카운팅을 한다.
    public void Update()
    {
        //var passTime = PassTimeFromServerTime();
        //SBDebug.Log($"time [{passTime}]");

        if (State == eState.Ready)
            UpdateReady();
        else if (State == eState.Play)
            UpdatePlay();
    }

    void UpdateReady()
    {
        long value = PassTimeFromGameStartTime();

        if (value < 0)
        {
            leftTime = Mathf.CeilToInt(Managers.PlayData.GameRoomInfo.PlayTime * 0.001f) + 1;
            State = eState.Play;
            InvokeInitStateEvent(eState.Play, 0);
            return;
        }


        // 밀리초 단위는 올림한다
        int sec = Mathf.CeilToInt(value * 0.001f);
        if (leftTime > sec)
        {
            leftTime = sec;
            InvokeStateEvent(eState.Ready, sec);
        }
    }

    void UpdatePlay()
    {
        var value = PassTimeFromGameEndTime();
        if (value <= 0)
        {
            leftTime = int.MaxValue;
            State = eState.GameOver;
            InvokeInitStateEvent(eState.GameOver, 0);
            return;
        }

        // 밀리초 단위는 올림한다
        int sec = Mathf.CeilToInt(value * 0.001f);
        if (leftTime > sec)
        {
            leftTime = sec;
            InvokeStateEvent(eState.Play, sec);
        }
    }

    //탈출 문을 열수 있는 조건 체크
    public bool CanOpenDoor()
    {
        //return escapeScore >= UnlockedObjectCount;
        return CurrentBatteryProgress >= 1f;
    }

    public void OnEscape(string id)
    {
        var isMe = Game.Instance.PlayerController.Character.Id == id;
        ++CurrentEscapedSurvivorCount;
    }

    public void SetBatteryProgress(float value)
    {
        CurrentBatteryProgress = value;
    }

    public void SetChargeBatteryCount(string id, int value, bool isResume = false)
    {
        var isMe = false;
        if (Game.Instance.PlayerController != null && Game.Instance.PlayerController.Character != null)            
            isMe = Game.Instance.PlayerController.Character.IsMe && (Game.Instance.PlayerController.Character.Id == id);

        var batteryCount = value;
        if (survivorBatteryScore.ContainsKey(id))
            survivorBatteryScore[id] += batteryCount;
        else
            survivorBatteryScore[id] = batteryCount;

        CurrentBatteryCount = value;
    }

    public Dictionary<string, int> GetBatteryScore()
    {
        var sortedDict = from entry in survivorBatteryScore orderby entry.Value ascending select entry;
        return sortedDict.ToDictionary(x => x.Key, x => x.Value);
    }

    public void IncreaseKillCount(string id)
    {
        var isMe = Game.Instance.PlayerController.Character.IsMe && (Game.Instance.PlayerController.Character.Id == id);

        if (chaserKillCount.ContainsKey(id))
            ++chaserKillCount[id];
        else
            chaserKillCount[id] = 1;
    }

    public void IncreaseVehicleKillCount(string id)
    {
        var isMe = Game.Instance.PlayerController.Character.IsMe && (Game.Instance.PlayerController.Character.Id == id);

        if (chaserVehicleKillCount.ContainsKey(id)) ++chaserVehicleKillCount[id];
        else chaserVehicleKillCount[id] = 1;
    }

    public int GetKillCount(long id)
    {
        var result = Managers.UserData.GameResult;

        if (result != null)
        {
            // foreach (var playerResult in result.PlayerResults)
            // {
            //     if (playerResult.ObjectId == id)
            //     {
            //         if (playerResult.ResultInfos.Count != 0)
            //             return playerResult.ResultInfos.FirstOrDefault(x => x.Key == 12).Value;
            //     }
            // }
        }

        return 0;
    }

    public int GetChargeCount(long id)
    {
        var result = Managers.UserData.GameResult;

        if (result != null)
        {
            // foreach (var playerResult in result.PlayerResults)
            // {
            //     if (playerResult.ObjectId == id)
            //     {
            //         if (playerResult.ResultInfos.Count != 0)
            //             return playerResult.ResultInfos.FirstOrDefault(x => x.Key == 1).Value;
            //     }
            // }
        }

        return 0;
    }
}
