using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SandboxNetwork
{
    /// <summary>
    /// 서버에서 준 worldIndex 를 가지고 오늘의 요일 인덱스 DailyManager.Instance.GetDaily()를 stage 삼아서 데이터 세팅함.
    /// </summary>
    public class WorldBossStageDataInfo
    {
        public int StagebaseKey { get; private set; } = 1;
        public int World { get; private set; } = 1;
        public int Stage { get; private set; } = 1;

        public MonsterBaseData RaidBossData = null;

        public int RaidBossKey { get 
            {
                if (RaidBossData == null)
                    return -1;
                else
                {
                    return RaidBossData.KEY;
                }
            } 
        }

        public WorldBossStageDataInfo(int _world, int _stage)
        {
            var stageData = StageBaseData.GetByWorldStage(_world,_stage);
            if(stageData != null)
            {
                World = _world;
                Stage = _stage;
                StagebaseKey = stageData.KEY;
                SetRaidBossData(stageData);
            }
        }

        public WorldBossStageDataInfo(StageBaseData _stageData)
        {
            if(_stageData != null)
            {
                World = _stageData.WORLD;
                Stage = _stageData.STAGE;
                StagebaseKey = _stageData.KEY;
                SetRaidBossData(_stageData);
            }
        }

        void SetRaidBossData(StageBaseData _data)
        {
            if(_data != null)
            {
                var spawnGroup = _data.SPAWN;
                var dataList = MonsterSpawnData.GetBySpawnGroup(spawnGroup);
                if (dataList == null || dataList.Count <= 0)
                    RaidBossData = null;
                else
                {
                    var bossData = dataList.Find(element => element.IS_BOSS == 1);
                    if (bossData != null)
                        RaidBossData = MonsterBaseData.Get(bossData.MONSTER);
                    else
                        RaidBossData = null;
                }
            }
        }
    }

    /// <summary>
    /// 오늘의 raid_log 데이터. 해당 worldIndex와 stageIndex, difficulty, high Level(최대도달 레벨), high Score(최대 도달 스코어)세팅
    /// </summary>
    public class WorldBossLogDataInfo
    {
        public int World { get; private set; } = 1;
        public int Stage { get; private set; } = 1;
        public int Difficulty { get; private set; } = 1;
        public int High_Level { get; private set; } = 1;
        public string High_Score { get; private set; } = "0";//long 이나 int로 쓸 필요는 없을 것 같음.


        public WorldBossLogDataInfo(int _world, int _stage, int _highLevel, string _score, int _difficulty = 1)
        {
            World = _world;
            Stage = _stage;
            High_Level = _highLevel;
            High_Score = _score;
            Difficulty = _difficulty;
        }
    }

    public class WorldBossProgressData
    {
        //오늘의 레이드 보스 데이터
        public Dictionary<int, WorldBossStageDataInfo> TodayBossStageDataInfo = new Dictionary<int, WorldBossStageDataInfo>();//key : worldIndex
        //오늘의 레이드에 관한 로그 데이터
        public Dictionary<int, WorldBossLogDataInfo> TodayBossLogDataInfo = new Dictionary<int, WorldBossLogDataInfo>();//key : worldIndex

        public int WorldBossPlayCount { get; private set; } = 0;

        public WorldBossStageDataInfo CurSelectBossData { get; private set; } = null;//입장 전 선택한 데이터

        public MonsterBaseData TodayBossMonsterData//선택한 월드 보스 데이터
        {
            get
            {
                if (CurSelectBossData == null)
                    return null;
                return CurSelectBossData.RaidBossData;
            }
        }

        public void Init()
        {
            WorldBossPlayCount = 0;
            CurSelectBossData = null;
            if (TodayBossStageDataInfo == null)
                TodayBossStageDataInfo = new Dictionary<int, WorldBossStageDataInfo>();
            TodayBossStageDataInfo.Clear();
            if (TodayBossLogDataInfo == null)
                TodayBossLogDataInfo = new Dictionary<int, WorldBossLogDataInfo>();
            TodayBossLogDataInfo.Clear();

        }

        public void SetCurStageData(WorldBossStageDataInfo _info)
        {
            CurSelectBossData = _info;
        }
        public WorldBossStageDataInfo GetStageDataByMonsterKey(int _bossKey)
        {
            if (_bossKey < 0)
                return null;

            if (TodayBossStageDataInfo == null || TodayBossStageDataInfo.Count <= 0)
                return null;
            var dataList = TodayBossStageDataInfo.Values.ToList();
            if (dataList == null || dataList.Count <= 0)
                return null;

            var data = dataList.Find(element => element.RaidBossKey == _bossKey);

            return new WorldBossStageDataInfo(data.World,data.Stage);
        }

        /// <summary>
        /// 로비 입장 시 기본 포맷 세팅 - 로그인일 때 주는 데이터로 일원화 시켜야함.
        /// </summary>
        /// <param name="_jsonData"></param>
        public void SetData(JObject jsonData)
        {
            JObject worldBossData = (JObject)jsonData["raid"];
            if (worldBossData != null)//raid_info , raid_log
            {
                SetRaidBossInfoData(worldBossData);
                SetWorldBossLogData(worldBossData);
            }
        }

        public void SetWorldBossTicketCount(JToken count)
        {
            if (SBFunc.IsJTokenType(count, JTokenType.Integer))
                WorldBossPlayCount = count.Value<int>();
        }

        //"{\r\n  \"raid_info\": {\r\n    \"world\": [\r\n301\r\n],\r\n    \"battle_count\": 0\r\n  },\r\n  \"raid_log\": 0\r\n}"
        //stage 는 서버에서 따로 안주고 월~일 클라에서 정의된 1~7인덱스를 사용함.
        public void SetRaidBossInfoData(JObject jsonData)
        {
            if(jsonData.ContainsKey("raid_info"))
            {
                var raidInfoData = (JObject)jsonData["raid_info"];
                if(raidInfoData.ContainsKey("world"))
                {
                    var today = (int)DailyManager.Instance.GetDaily();//state == 요일 인덱스
                    var worldJarray = JArray.FromObject(raidInfoData["world"]);
                    if(worldJarray != null && worldJarray.Count > 0)
                    {
                        var worldList = worldJarray.ToObject<List<int>>();
                        foreach(var worldIndex in worldList)
                        {
                            var stagebaseData = StageBaseData.GetByWorldStage(worldIndex, today);
                            if (stagebaseData == null)
                                continue;

                            if (!TodayBossStageDataInfo.ContainsKey(worldIndex))
                                TodayBossStageDataInfo.Add(worldIndex, new WorldBossStageDataInfo(stagebaseData));
                            else
                                TodayBossStageDataInfo[worldIndex] = new WorldBossStageDataInfo(stagebaseData);
                        }
                    }    
                }

                if(raidInfoData.ContainsKey("battle_count"))
                {
                    SetWorldBossTicketCount(raidInfoData["battle_count"]);
                }
            }
        }
        public void UpdateLogData(int _worldIndex, long _Highscore, int _highLevel)
        {
            if (!TodayBossLogDataInfo.ContainsKey(_worldIndex))
                return;

            var curData = TodayBossLogDataInfo[_worldIndex];
            var curHighLevel = curData.High_Level;
            var curHighScore = curData.High_Score;
            
            long resultHighScore = 0;
            if(long.TryParse(curHighScore , out long changeHighScore))
                resultHighScore = changeHighScore < _Highscore ? _Highscore : changeHighScore;

            var changeLevel = curHighLevel < _highLevel ? _highLevel : curHighLevel;
            var chageHighScoreString = resultHighScore != 0 ? resultHighScore.ToString() : curHighScore;

            TodayBossLogDataInfo[_worldIndex] = new WorldBossLogDataInfo(curData.World, curData.Stage, changeLevel, chageHighScoreString);
        }

        //"raid_log":[{"w":301,"s":3,"d":1,"lv":16,"hs":1540919}]
        public void SetWorldBossLogData(JObject jsonData)
        {
            if(jsonData.ContainsKey("raid_log") && SBFunc.IsJArray(jsonData["raid_log"]))
            {
                JArray logJarray = (JArray)jsonData["raid_log"];
                foreach(var info in logJarray)
                {
                    if (info != null && SBFunc.IsJObject(info))
                    {
                        var obj = (JObject)info;
                        var worldIndex = 0;
                        var stageIndex = 0;
                        var level = 0;
                        string hs = "0";

                        if (obj.ContainsKey("w"))
                            worldIndex = obj["w"].Value<int>();
                        if (obj.ContainsKey("s"))
                            stageIndex = obj["s"].Value<int>();
                        if (obj.ContainsKey("lv"))
                            level = obj["lv"].Value<int>();
                        if (obj.ContainsKey("hs"))
                            hs = obj["hs"].Value<string>();

                        if (worldIndex <= 0 || stageIndex <= 0)
                            continue;

                        if (!TodayBossLogDataInfo.ContainsKey(worldIndex))
                            TodayBossLogDataInfo.Add(worldIndex, new WorldBossLogDataInfo(worldIndex, stageIndex, level, hs));
                        else
                            TodayBossLogDataInfo[worldIndex] = new WorldBossLogDataInfo(worldIndex, stageIndex, level, hs);
                    }
                }
            }
        }

        public void IsToday(VoidDelegate matchDayCallBack, VoidDelegate dismatchDayCallBack)
        {
            var day = (int)DailyManager.Instance.GetDaily(); // 서버로부터 받은 날짜
            if (day == CurSelectBossData.Stage)
                matchDayCallBack?.Invoke();
            else
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("boss_raid_go_refresh"),() =>
                    {
                        NetworkManager.Send("user/dungeonstate", null, (JObject jsonData) =>
                        {
                            if (jsonData["err"] != null && jsonData["rs"] != null && (int)jsonData["err"] == 0 && (int)jsonData["rs"] == 0)
                            {
                                if (!jsonData.ContainsKey("world_boss"))
                                    return;

                                var worldBossData = jsonData["world_boss"];

                                SetRaidBossInfoData((JObject)worldBossData);
                                SetWorldBossLogData((JObject)worldBossData);

                                //일단 강제로 lobbyScene으로 보냄.
                                WorldBossManager.Instance.UISelectBossKey = -1;
                                WorldBossManager.Instance.UIDeckIndex = -1;
                                LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.BlackBackground, SBFunc.CallBackCoroutine(() => {
                                    ToastManager.On(StringData.GetStringByStrKey("boss_raid_complete_refresh"));
                                }));
                            }
                            dismatchDayCallBack?.Invoke();
                        }, (string log) =>
                        {
                            UnityEngine.Debug.Log(log);
                            dismatchDayCallBack?.Invoke();
                        });
                    }
                );
            }
        }
    }
}

