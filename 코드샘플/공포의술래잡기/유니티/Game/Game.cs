using DG.Tweening;
using SBCommonLib;
using SBSocketSharedLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public partial class Game : SBSingleton<Game>
{
    public class RecivedTempPacket
    {
        public TcpServerSession serverSession_;
        public SBSocketPacketLib.SBPacket packet_;

        public RecivedTempPacket(TcpServerSession s, SBSocketPacketLib.SBPacket p)
        {
            serverSession_ = s;
            packet_ = p;
        }
    }

    List<RecivedTempPacket> recivedPacket = new List<RecivedTempPacket>();

    public void AddTempPacket(TcpServerSession s, SBSocketPacketLib.SBPacket p)
    {
        recivedPacket.Add(new RecivedTempPacket(s, p));
    }

    public UIGame UIGame { get; private set; }
    public GameRoom GameRoom { get; private set; }
    public PlayerController PlayerController { get; private set; }
    public bool IsReady { get; private set; }  //플레이어 확인
    public bool IsPlay { get; private set; }
    public HudNode HudNode { get; private set; }
    public UIPortrait UIPortrait { get; private set; }

    private Map curMap = null;
    public bool IsOpenDoor { get; private set; }

    public Game()
    {
        IsReady = false;
        IsPlay = false;

        SBDebug.Log("new game");
    }

    ~Game()
    {
        SBDebug.Log("end game");
    }

    public void SetGameRoomData(GameRoom roomData)
    {
        GameRoom = roomData;
    }

    public void ClearUIGame()
    {
        UIGame = null;
    }

    public void InitGame()
    {
        SBDebug.Log("Game Start() Set Gameroom");

        UIGame = FindObjectOfType<UIGame>();
        if (UIGame == null)
        {
            SBDebug.LogError("UIGame is NULL");//InitGame시점이 씬로드보다 빨르다.
        }

        HudNode = FindObjectOfType<HudNode>();
        if (HudNode == null)
        {
            SBDebug.LogError("HudNode is NULL");//InitGame시점이 씬로드보다 빨르다.
        }

        UIPortrait = FindObjectOfType<UIPortrait>();
        if (UIPortrait == null)
        {
            SBDebug.LogError("UIPortrait is NULL");//InitGame시점이 씬로드보다 빨르다.
        }

        GameRoom.AddInitStateEvent(GameRoom.eState.Ready, InitReadyProcessor);
        GameRoom.AddInitStateEvent(GameRoom.eState.Play, InitPlayProcessor);
        GameRoom.AddInitStateEvent(GameRoom.eState.GameOver, InitGameOverProcessor);
        GameRoom.AddStateEvent(GameRoom.eState.Ready, ReadyProcessor);
        GameRoom.AddStateEvent(GameRoom.eState.Play, PlayProcessor);

        UIGame.SetVisibleInput(true); // Input Layer가 disable 되어있으면 FindObjectOfType에서 찾지 못하여 게임이 터진다. 만약 씬에서 비활성화하고 저장했더라도 터지지 않게 하기 위해 넣음

        // 맵 동적 로드 함수
        curMap = LoadMap(Managers.PlayData.GameRoomInfo.MapId);

        var playerList = GameRoom.RoomInfo.PlayerObjectList;
        for (int i = 0; i < playerList.Count; i++)
        {
            Managers.Object.AddPlayer(GameRoom.RoomInfo.PlayerObjectList[i]);
        }

        int count = GameRoom.RoomInfo.MapObjectList.Count;
        for (int i = 0; i < count; i++)
        {
            SBSocketSharedLib.MapObjectInfo objData = GameRoom.RoomInfo.MapObjectList[i];
            if (objData != null)
            {
                CreateObject(objData);
            }
        }

        ////cctv test
        //var cctv = GameObject.FindObjectOfType<CCTV>();
        //if (cctv)
        //{
        //    cctv.Show(false);
        //}

        IsReady = true;

        SendPing();
    }

    long latestPingTime = 0;
    Coroutine pingCoroutine = null;
    public long Latency { get; set; }
    public void RecvPong(long time)
    {
        Instance.UIGame.CancelInvoke("ShowNetworkAlert");
        Latency = (SBUtil.GetCurrentMilliSecTimestamp() - latestPingTime) / 2;

        //todo Server Busy..
        Instance.UIGame.ShowNetworkAlert(Latency > 1000);

        CancelInvoke("SendPing");
        Invoke("SendPing", 10.0f);
    }

    public void SendPing()
    {
        CancelInvoke("SendPing");

        if (UIGame != null)
        {
            bool init = latestPingTime == 0;
            Managers.GameServer.SendPing(ref latestPingTime);
            UIGame.Invoke("ShowNetworkAlert", init ? 5.0f : 0.5f);
        }
    }

    public Map LoadMap(int mapId)
    {
        MapTypeGameData typeData = MapTypeGameData.GetMapTypeData(mapId);
        if (typeData == null)
        {
            SBDebug.LogError($"MapTypeGameData is Null");
            return null;
        }

        SBDebug.Log($"LoadMap : Map ID {mapId}");
        GameObject map = typeData.GetMapObject();
        if (map == null)
        {
            SBDebug.LogError($"No Map Prefab : {typeData.map_resource}");
            throw new System.Exception();
        }

        var mapComp = map.GetComponent<Map>();
        if (mapComp == null)
        {
            SBDebug.LogError($"mapCompnent is Null");
            return null;
        }

        mapComp.MapData = typeData.GetMapData();

        return map.GetComponent<Map>();
    }

    void InitReadyProcessor(GameRoom.eState state, long time)
    {
        UIGame.SetVisibleInput(false);
        UIGame.ShowReadyCount(true, Managers.PlayData.AmIChaser());
        var cctv = GameObject.FindObjectOfType<CCTV>();
        if (cctv)
        {
            //cctv.Show(_characterController.Character.Chaser);
            cctv.Show(false);
        }

        UIGame.SetGuideText("");

        if (!Managers.PlayData.IsResume)
        {
            if (curMap != null)
            {
                Light2D light = Camera.main.transform.GetComponentInChildren<Light2D>();

                Sequence sequence = DOTween.Sequence();
                bool firstCamPos = true;
                bool zoomOut = false;
                if (curMap.CenterPosition != Vector3.zero)
                {
                    Camera.main.DOKill();

                    Camera.main.transform.position = curMap.CenterPosition;
                    Camera.main.orthographicSize = 20.0f;

                    light.lightType = Light2D.LightType.Global;

                    //sequence.AppendInterval(1.0f);

                    firstCamPos = false;
                    zoomOut = true;
                }

                if (curMap.EscapePosition != Vector3.zero)
                {
                    if (firstCamPos)
                    {
                        Camera.main.transform.position = curMap.EscapePosition;
                        UIGame.SetGuideText(StringManager.GetString("탈출구"));
                        firstCamPos = false;
                    }
                    else
                    {
                        sequence.Append(Camera.main.transform.DOMove(curMap.EscapePosition, 1.0f).SetEase(Ease.OutCirc).OnComplete(() =>
                        {
                            UIGame.SetGuideText(StringManager.GetString("탈출구"));
                        }));
                    }

                    sequence.AppendInterval(0.8f);
                    sequence.AppendCallback(() =>
                    {
                        UIGame.SetGuideText("");
                    });
                }

                if (curMap.ChargePosition != Vector3.zero)
                {
                    if (firstCamPos)
                    {
                        Camera.main.transform.position = curMap.EscapePosition;
                        UIGame.SetGuideText(StringManager.GetString("충전기"));
                        firstCamPos = false;
                    }
                    else
                    {
                        sequence.Append(Camera.main.transform.DOMove(curMap.ChargePosition, 1.0f).SetEase(Ease.OutCirc).OnComplete(() =>
                        {
                            UIGame.SetGuideText(StringManager.GetString("충전기"));
                        }));
                    }

                    sequence.AppendInterval(0.8f);
                    sequence.AppendCallback(() =>
                    {
                        UIGame.SetGuideText("");
                    });
                }

                Vector3 playerPos = new Vector3(PlayerController.Character.transform.position.x, PlayerController.Character.transform.position.y, -10);
                if (firstCamPos)
                    Camera.main.transform.position = playerPos;

                sequence.AppendCallback(UIGame.ShowQuestGuide);
                sequence.Append(Camera.main.transform.DOMove(playerPos, 0.9f).SetEase(Ease.OutCirc));
                sequence.Append(Camera.main.DOOrthoSize(6.5f, 0.5f));

                sequence.AppendCallback(() =>
                {
                    light.lightType = Light2D.LightType.Point;
                    light.shadowIntensity = 0.0f;
                });

                sequence.Append(DOTween.To(() => light.shadowIntensity, x => light.shadowIntensity = x, 0.5f, 0.5f));
            }
        }
        Managers.Object.InitBlockObjects();
    }

    void InitPlayProcessor(GameRoom.eState state, long time)
    {
        UIGame.ShowReadyCount(false);
        UIGame.ShowScore(true);
        UIGame.ShowEscapeScore(false);
        UIGame.ShowBatteryGeneratorProgress(true);
        UIGame.StartBatteryGeneratorTime();
        UIGame.SetVisibleInput(true);

        Managers.Scene.CurrentScene.StartBackgroundMusic();

        if (Managers.PlayData.IsResume)
        {
            var resPacket = Managers.PlayData.ResumeGameDataPacket;

            foreach (var mapObject in resPacket.MapObjectInfos)
            {
                var prop = Managers.Object.FindBaseObjectById(mapObject.MapObjectId) as PropController;
                if (prop != null)
                {
                    var regenTime = Game.Instance.GameRoom.GameTime.GetClientTimestamp((long)SBUtil.ConvertToUnixTimestamp(new DateTime(mapObject.RegenTime)).TotalMilliseconds);
                    prop.SetRespawnTimeOnReconnect(regenTime);
                }
            }

            foreach (var player in resPacket.PlayerInfos)
            {
                var co = Managers.Object.FindCharacterById(player.ObjectId);
                if (co != null && !co.IsChaser)
                {
                    co.InitBatteryChargeCount(player.PutBatteryCnt);
                    co.SetBattery(player.BatteryCnt, false, true);
                    GameRoom.SetChargeBatteryCount(player.ObjectId, player.PutBatteryCnt, true);
                }
            }
            foreach (var pair in resPacket.GeneraterBatteryInfos)
            {
                var bo = Managers.Object.FindBaseObjectById(pair.Key);
                if (bo != null)
                {
                    var bg = bo.GetComponent<BatteryGenerator>();
                    if (bg != null)
                    {
                        bg.OnSetBattery(pair.Value, false);
                    }
                }
            }
        }

        IsPlay = true;

        if (Managers.PlayData.IsResume)
        {
            foreach (var rp in recivedPacket)
            {
                PacketManager.Instance.HandlePacket(rp.serverSession_, rp.packet_);
            }

            if (Managers.PlayData.ResumeGameDataPacket != null && Managers.PlayData.ResumeGameDataPacket.EscapeDoorOpenTime > 0)
            {
                foreach (var obj in Managers.Object.Objects)
                {
                    var go = obj.Value;
                    if (go == null)
                        continue;
                    var key = go.GetComponent<EscapeKey>();
                    if (key)
                    {
                        key.OpenDoor();
                    }
                }

                UIGame.ShowEscapeIcon();
                UIGame.ShowBatteryGeneratorProgress(false);
                UIGame.SetEscapeUI(0);
                GameRoom.SetOpenDoor(Managers.PlayData.ResumeGameDataPacket.EscapeDoorOpenTime);
            }
        }
    }

    void InitGameOverProcessor(GameRoom.eState state, long time)
    {
        UIGame.StopBatteryGeneratorTime();
        var generator = Managers.Object.GetBatteryGenerator();
        generator.OnDeactivateGenerator();
        UIGame.ClearTime();
    }

    void ReadyProcessor(GameRoom.eState state, long time)
    {
        UIGame.OnReadyCount((int)time);
    }

    void PlayProcessor(GameRoom.eState state, long time)
    {
        UIGame.RefreshTime((int)time);

        if (GameRoom.IsAnyDoorOpened()) return;

        var coolTimeSeconds = (int)(Managers.PlayData.GameRoomInfo.BatteryGeneratorCoolTime * 0.001f);
        var activeSeconds = (int)(Managers.PlayData.GameRoomInfo.BatteryGeneratorActiveTime * 0.001f);
        var periodSeconds = coolTimeSeconds + activeSeconds;
        var curRound = ((int)((Managers.PlayData.GameRoomInfo.PlayTime * 0.001f) - time)) / periodSeconds + 1;
        var value = time % periodSeconds;
        var generator = Managers.Object.GetBatteryGenerator();
        if (value == 0)
            value = periodSeconds;
        if (value > activeSeconds)
        {
            // 충전 불가
            UIGame.RefreshBatteryGeneratorTime((int)value - activeSeconds, curRound, true);
            generator.OnDeactivateGenerator();
        }
        else
        {
            // 충전 가능
            UIGame.RefreshBatteryGeneratorTime((int)value, curRound, false);
            generator.OnActiveGenerator();
        }
    }

    public void GameResult(SCBcGameResult result_)
    {
        Managers.PlayData.OnGameResult(result_);
        IsPlay = false;

        bool isChaser = Managers.PlayData.AmIChaser();
        bool isWin = false;
        //PlayerResult myResult = null;

        // myResult = result_.PlayerResults.ToList().Find(x => x.ObjectId == Managers.UserData.MyUserID);
        // Managers.UserData.SetMyGameResult(myResult);
        Managers.UserData.SetGameResult(result_);

        {
            var objs = Managers.Object.Objects;
            foreach (var obj in objs)
            {
                if (obj.Value == null) continue;
                var co = obj.Value.GetComponent<CharacterObject>();
                if (co != null)
                {
                    co.SetState(CreatureStatus.Idle);
                }
            }
        }

        if (isChaser)
        {
            if (result_.GameResult == 1)
            {
                isWin = true;
            }
        }
        else
        {
            if (result_.GameResult == 2)
            {
                isWin = true;
            }
        }

        UIGame.SetResult();
        if (isWin)
        {
            SBDebug.Log("GameResult Win");
        }
        else
        {
            SBDebug.Log("GameResult Lose");
        }

        GameRoom.SetStateWithCallback(GameRoom.eState.GameOver);
    }

    public void EnterGame(SCBcEnterGame resPacket)
    {
        SBDebug.Log($"=====================EnterGame] {resPacket.OwnerId}");
        //GameRoom.GameTime.SetBaseTime(resPacket.Timestamp, SBUtil.GetCurrentMilliSecTimestamp());
        //StartCoroutine(EnterGameCoroutine(resPacket));

        CharacterObject charObj = Managers.Object.FindCharacterById(resPacket.OwnerId.ToString());
        if (charObj)
        {
            charObj.SetBaseData(resPacket.OwnerId.ToString(), resPacket.ObjectInfo.PosInfo, resPacket.ObjectInfo.StatInfo);
        }
    }

    public void SetGameStartTimeStamp(long stamp)
    {
        GameRoom.SetGameStartTimeStamp(stamp);
    }

    public void SpawnPlayer(RespawnObjectInfo info)
    {
        //GameObject go = Managers.Object.FindById(info.ObjectId);
        var character = Managers.Object.FindCharacterById(info.ObjectId);
        if (character != null)
            character.OnRespawn(info);
    }

    public void ObjectDamage(string objectId)
    {
        var obj = Managers.Object.GetPropObject(objectId);
        if (obj)
        {
            if (PlayerController.Target == obj)
            {
                if (PlayerController.Character.State == CreatureStatus.Hiding)
                {
                    PlayerController.Character.OnHide(false);
                }
            }
            obj.OnDamage();
        }
    }

    public void OnStateChanged(SCBcStateChanged stateChange)
    {
        if (!IsPlay) return;

        var character = Managers.Object.FindCharacterById(stateChange.ObjectInfo.ObjectId);
        var state = stateChange.ChangedState;
        bool isMy = Managers.UserData.MyUserID.Equals(stateChange.ObjectInfo.ObjectId);
        if ((ChangedStat)state == ChangedStat.CreatureStatus)
        {
            if (stateChange.ObjectInfo.PosInfo.Status == (byte)CreatureStatus.Hiding)
            {
                character.ApplyPos(stateChange.ObjectInfo.PosInfo.Pos.X, stateChange.ObjectInfo.PosInfo.Pos.Y);
                character.OnHide(true);


                HideObjectController hideObject = Managers.Object.GetHideObjectByPos(stateChange.ObjectInfo.PosInfo.Pos);
                if (hideObject != null)
                {
                    hideObject.SetInTheCharacter(true);
                }
            }
            else if (stateChange.ObjectInfo.PosInfo.Status == (byte)CreatureStatus.Idle)
            {
                if (character.State == CreatureStatus.Hiding)
                {
                    character.OnHide(false);
                    character.ApplyPos(stateChange.ObjectInfo.PosInfo.Pos.X, stateChange.ObjectInfo.PosInfo.Pos.Y);
                }
                else
                {
                    character.SetState(CreatureStatus.Idle);
                }
            }
            else if (stateChange.ObjectInfo.PosInfo.Status == (byte)CreatureStatus.Groggy)
            {
                if (character.State == CreatureStatus.Hiding)
                {
                    character.OnHide(false);
                }

                character.OnGroggy();
                character.SetState(CreatureStatus.Groggy);
            }
            else
            {
                var stats = (CreatureStatus)stateChange.ObjectInfo.PosInfo.Status;
                character.SetState(stats);
            }
        }
    }

    private void Update()
    {
        if (GameRoom != null) GameRoom.Update();

        if (!Managers.Network.GameServer.IsAlive() && !PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MESSAGE_POPUP) && (Managers.Scene.CurrentScene is GameScene))
            DisconnectPopup();
    }

    public void NotifyElectricBox(string id, int value)
    {
        var obj = Managers.Object.GetPropObject(id);
        if (obj == null)
        {
            SBDebug.Log($"Cant find Object[{id}]");
            return;
        }

        SetEscapeKey(id, value);

        var keyData = Managers.Data.GetData(GameDataManager.DATA_TYPE.object_key, obj.ObjData.sub_obj_uid) as ObjectKeyGameData;

        var ret = (float)value / keyData.activation_time;
        HudNode.CreateHudPosElectricBox(obj.gameObject, new Vector2(obj.transform.position.x, obj.transform.position.y), ret, 5.0f);
    }

    public void NotifyEscapeDoor(string id, int value)
    {
        var obj = Managers.Object.GetPropObject(id);
        if (obj == null)
        {
            SBDebug.Log($"Cant find Object[{id}]");
            return;
        }

        SetEscapeOpenDoor(id, value);

        var keyData = Managers.Data.GetData(GameDataManager.DATA_TYPE.object_key, obj.ObjData.sub_obj_uid) as ObjectKeyGameData;

        var ret = (float)value / keyData.activation_time;
        if (ret < 1.0f)//만땅은 다른처리하는듯?
            HudNode.CreateHudPosEscapeDoor(obj.gameObject, new Vector2(obj.transform.position.x, obj.transform.position.y), ret, 5.0f);
    }

    public void EndProccessor()
    {
        PlayerController.Release();
    }

    public void SetPlayerController(PlayerController controller)
    {
        PlayerController = controller;
    }

    public bool CheckVisibleObject(GameObject targetObject)
    {
        // 아직 PlayerController가 세팅되어있지 않으면 시야 체크를 할 수 없으므로 무조건 볼 수 있다고 판단
        if (PlayerController == null) return true;
        return PlayerController.IsVisibleObject(targetObject);
    }

    public byte GetTileInfo(int x, int y)
    {
        var value = curMap.MapData.TileInfo[y, x];
        return value;
    }

    public void ProcessGamePoint(string id, IList<KeyValuePair<int, int>> infos)
    {
        bool isMe = false;
        if (long.Parse(id) == Managers.UserData.MyUserID)
            isMe = true;

        foreach (var info in infos)
        {
            SBDebug.Log($"SCBcGamePoint {info.Key} / {info.Value}");
            var data = Managers.Data.GetData(GameDataManager.DATA_TYPE.reward_point, info.Key) as RewardGameData;
            if (isMe && HudNode != null)
            {
                CharacterObject charObj = Managers.Object.FindCharacterById(id);
                HudNode.OnPoint(charObj, data.Point);
            }
            if (data.CenterAlarm && UIGame != null)
            {
                UIGame.ShowCenterMessage(info.Key, id);
            }
        }
    }

    public void DisconnectPopup()
    {
        PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("서버연결종료"), () =>
        {
            Managers.Network.Disconnect();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
        });
    }
}
