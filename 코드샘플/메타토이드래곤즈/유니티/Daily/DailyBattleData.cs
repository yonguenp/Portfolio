using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DailyBattleData : BattleData
    {
        #region WorldData
        public int World { get; private set; } = -1;
        public int Stage { get; private set; } = -1;
        public int Diff { get; private set; } = 1;
        public override float Speed { get; set; } = 1f;
        public override bool IsAuto { get; set; } = true;
        public override float Time { get; set; } = 0;
        public float MaxTime { get; private set; } = 180f;
        #endregion

        #region BattleData
        public override eBattleType BattleType { get => eBattleType.DAILY; }
        public override int Wave { get; protected set; }
        protected int maxWave = -1;
        public override int MaxWave
        {
            get
            {
                if (maxWave < 0)
                {
                    var stageData = StageBaseData.GetByWorldStage(World, Stage);
                    if (stageData != null)
                    {
                        var spawn = MonsterSpawnData.GetBySpawnGroup(stageData.SPAWN);
                        if (spawn != null)
                        {
                            maxWave = -1;
                            for (int i = 0, count = spawn.Count; i < count; ++i)
                            {
                                if (spawn[i] == null)
                                    continue;
                                if (maxWave < spawn[i].WAVE)
                                {
                                    maxWave = spawn[i].WAVE;
                                }
                            }
                        }
                    }
                }
                return maxWave;
            }
            protected set { maxWave = value; }
        }
        public bool IsPlayerSkill { get; private set; } = false;
        #endregion

        #region RewardData
        public int AccExp { get; private set; }
        public List<Asset> Rewards { get; private set; }
        public List<int> LevelUpList { get; private set; }
        #endregion
        public override void Initialize()
        {
            base.Initialize();

            IsAuto = PlayerPrefs.GetInt("DailyAuto", 1) == 1;
            var cur = PlayerPrefs.GetInt("DailySpeedIndex", 0);
            int max = User.Instance.BATTLE_SPPED_BOOST ? 5 : 4;
            cur = cur % max;

            var speed = 1.0f;
            switch (cur)
            {
                case 1:
                    speed = 1.2f;
                    break;
                case 2:
                    speed = 1.5f;
                    break;
                case 3:
                    speed = 2f;
                    break;
                case 4:
                    speed = 2.5f;
                    break;
                case 0:
                default:
                    speed = 1.0f;
                    break;
            }
            PlayerPrefs.SetFloat("DailySpeed", speed);;
            Speed = speed;

            World = -1;
            Stage = -1;
            IsPlayerSkill = false;
            Time = 0;

            AccExp = -1;
        }

        public override void InitializeWave()
        {
            State = eBattleState.None;
            Wave = -1;

            if (DefensePos == null)
                DefensePos = new();
            else
                DefensePos.Clear();

            if (DefenseDic == null)
                DefenseDic = new();
            else
                DefenseDic.Clear();
        }

        public override void InitializeReward()
        {
            State = eBattleState.None;
            AccExp = -1;

            if (Rewards == null)
                Rewards = new();
            else
                Rewards.Clear();

            if (DefensePos == null)
                DefensePos = new();
            else
                DefensePos.Clear();

            if (DefenseDic == null)
                DefenseDic = new();
            else
                DefenseDic.Clear();
        }

        public void SetWorld(int world, int stage)
        {
            World = world;
            Stage = stage;
            Diff = 1;
        }

        public override bool CheckTimeOver()
        {
            if (Time > MaxTime)
            {
                State = eBattleState.TimeOver;
                return true;
            }
            return false;
        }

        public override void SetData(JObject jsonData)
        {
            if (jsonData == null)
                return;

            if (SBFunc.IsJTokenType(jsonData["maxTime"], JTokenType.Float))
                MaxTime = (float)jsonData["maxTime"];
            else
                MaxTime = GameConfigTable.GetArenaBattleTime();

            if (SBFunc.IsJTokenType(jsonData["tag"], JTokenType.Integer))
            {
                BattleTag = jsonData["tag"].Value<int>();
            }

            if (SBFunc.IsJTokenType(jsonData["state"], JTokenType.Integer))
            {
                State = (eBattleState)jsonData["state"].Value<int>();
            }

            if (SBFunc.IsJTokenType(jsonData["wave"], JTokenType.Integer))
            {
                Wave = jsonData["wave"].Value<int>();
            }

            if (SBFunc.IsJArray(jsonData["player"]))
            {
                var datas = jsonData["player"].ToObject<JArray>();
                if (datas == null)
                    return;

                for (int i = 0, count = datas.Count; i < count; ++i)
                {
                    if (datas[i] == null)
                        continue;

                    if (!SBFunc.IsJArray(datas[i]))
                        continue;

                    var curDatas = datas[i].ToObject<JArray>();
                    if (curDatas == null || curDatas.Count == 0)
                        continue;

                    OffensePos.Add(i, new());
                    for (int j = 0, jCount = curDatas.Count; j < jCount; ++j)
                    {
                        if (curDatas[j] == null)
                            continue;

                        if (!SBFunc.IsJObject(curDatas[j]))
                            continue;

                        var data = curDatas[j].ToObject<JObject>();
                        if (data == null)
                            continue;

                        int btag = -1;
                        if (SBFunc.IsJTokenType(data["btag"], JTokenType.Integer))
                        {
                            btag = data["btag"].Value<int>();
                        }

                        int dtag = -1;
                        if (SBFunc.IsJTokenType(data["dtag"], JTokenType.Integer))
                        {
                            dtag = data["dtag"].Value<int>();
                        }

                        if (btag == -1 || dtag == -1)
                            continue;

                        OffensePos[i].Add(new(btag, dtag));

                        var playerData = new BattleDragonData();
                        playerData.SetData(btag, data["dtag"]);

                        OffenseDic.Add(btag, playerData);
                        Characters.Add(playerData);
                    }
                }
            }

            if (SBFunc.IsJArray(jsonData["enemy"]))
            {
                var datas = jsonData["enemy"].ToObject<JArray>();
                if (datas == null)
                    return;

                for (int i = 0, count = datas.Count; i < count; ++i)
                {
                    if (datas[i] == null)
                        continue;

                    if (!SBFunc.IsJArray(datas[i]))
                        continue;

                    var curDatas = datas[i].ToObject<JArray>();
                    if (curDatas == null || curDatas.Count == 0)
                        continue;

                    DefensePos.Add(i, new());
                    for (int j = 0, jCount = curDatas.Count; j < jCount; ++j)
                    {
                        if (curDatas[j] == null)
                            continue;

                        if (!SBFunc.IsJObject(curDatas[j]))
                            continue;

                        var data = curDatas[j].ToObject<JObject>();
                        if (data == null)
                            continue;

                        int btag = -1;
                        if (SBFunc.IsJTokenType(data["btag"], JTokenType.Integer))
                        {
                            btag = data["btag"].Value<int>();
                        }

                        int type = -1;
                        if (SBFunc.IsJTokenType(data["type"], JTokenType.Integer))
                        {
                            type = data["type"].Value<int>();
                        }

                        int group = -1;
                        if (SBFunc.IsJTokenType(data["grp"], JTokenType.Integer))
                        {
                            group = data["grp"].Value<int>();
                        }

                        int mtag = -1;
                        if (SBFunc.IsJTokenType(data["id"], JTokenType.Integer))
                        {
                            mtag = data["id"].Value<int>();
                        }

                        if (btag == -1 || type == -1 || group == -1 || mtag == -1)
                            continue;

                        DefensePos[i].Add(new(btag, type, group, mtag));

                        var enemyData = new BattleMonsterData(eStageType.DAILY_DUNGEON);
                        enemyData.SetData(btag, data["id"]);

                        DefenseDic.Add(enemyData.Position, enemyData);
                    }
                }
            }

            if (SBFunc.IsJTokenType(jsonData["accExp"], JTokenType.Integer))
            {
                AccExp = jsonData["accExp"].Value<int>();
            }

            if (SBFunc.IsJArray(jsonData["rewards"]))
            {
                var datas = jsonData["rewards"].ToObject<JArray>();
                if (datas == null)
                    return;

                for (int i = 0, count = datas.Count; i < count; ++i)
                {
                    if (datas[i] == null)
                        continue;

                    if (!SBFunc.IsJArray(datas[i]))
                        continue;

                    var curDatas = datas[i].ToObject<JArray>();
                    if (curDatas == null || curDatas.Count != 3)
                        continue;

                    int itemType = -1;
                    if (SBFunc.IsJTokenType(curDatas[0], JTokenType.Integer))
                    {
                        itemType = curDatas[0].Value<int>();
                    }

                    int itemNo = -1;
                    if (SBFunc.IsJTokenType(curDatas[1], JTokenType.Integer))
                    {
                        itemNo = curDatas[1].Value<int>();
                    }

                    int itemCount = -1;
                    if (SBFunc.IsJTokenType(curDatas[2], JTokenType.Integer))
                    {
                        itemCount = curDatas[2].Value<int>();
                    }

                    if (itemNo == -1 || itemCount <= 0 || itemType == -1)
                        continue;

                    Rewards.Add(new Asset(itemNo, itemCount, itemType));
                }
            }

            if (jsonData["levelup"] != null && SBFunc.IsJArray(jsonData["levelup"]))
            {
                LevelUpList = jsonData["levelup"].ToObject<List<int>>();
            }
            else
            {
                LevelUpList = new List<int>();
            }
        }

        public void SetAutoTicketData(JObject jsonData)
        {
            if (jsonData == null)
                return;

            InitializeReward();
            MaxTime = GameConfigTable.GetArenaBattleTime();
            BattleTag = 0;
            State = eBattleState.Win;
            Wave = 0;

            if (SBFunc.IsJArray(jsonData["player"]))
            {
                var datas = jsonData["player"].ToObject<JArray>();
                if (datas == null)
                    return;

                for (int i = 0, count = datas.Count; i < count; ++i)
                {
                    if (datas[i] == null)
                        continue;

                    if (!SBFunc.IsJArray(datas[i]))
                        continue;

                    var curDatas = datas[i].ToObject<JArray>();
                    if (curDatas == null || curDatas.Count == 0)
                        continue;

                    OffensePos.Add(i, new());
                    for (int j = 0, jCount = curDatas.Count; j < jCount; ++j)
                    {
                        if (curDatas[j] == null)
                            continue;

                        if (!SBFunc.IsJObject(curDatas[j]))
                            continue;

                        var data = curDatas[j].ToObject<JObject>();
                        if (data == null)
                            continue;

                        int btag = -1;
                        if (SBFunc.IsJTokenType(data["btag"], JTokenType.Integer))
                        {
                            btag = data["btag"].Value<int>();
                        }

                        int dtag = -1;
                        if (SBFunc.IsJTokenType(data["dtag"], JTokenType.Integer))
                        {
                            dtag = data["dtag"].Value<int>();
                        }

                        if (btag == -1 || dtag == -1)
                            continue;

                        OffensePos[i].Add(new(btag, dtag));

                        var playerData = new BattleDragonData();
                        playerData.SetData(btag, data["dtag"]);

                        OffenseDic.Add(btag, playerData);
                        Characters.Add(playerData);
                    }
                }
            }

            if (SBFunc.IsJTokenType(jsonData["accExp"], JTokenType.Integer))
            {
                AccExp = jsonData["accExp"].Value<int>();
            }

            if (SBFunc.IsJArray(jsonData["rewards"]))
            {
                var datas = jsonData["rewards"].ToObject<JArray>();
                if (datas == null)
                    return;

                for (int i = 0, count = datas.Count; i < count; ++i)
                {
                    if (datas[i] == null)
                        continue;

                    if (!SBFunc.IsJArray(datas[i]))
                        continue;

                    var curDatas = datas[i].ToObject<JArray>();
                    if (curDatas == null || curDatas.Count != 3)
                        continue;

                    int itemType = -1;
                    if (SBFunc.IsJTokenType(curDatas[0], JTokenType.Integer))
                    {
                        itemType = curDatas[0].Value<int>();
                    }

                    int itemNo = -1;
                    if (SBFunc.IsJTokenType(curDatas[1], JTokenType.Integer))
                    {
                        itemNo = curDatas[1].Value<int>();
                    }

                    int itemCount = -1;
                    if (SBFunc.IsJTokenType(curDatas[2], JTokenType.Integer))
                    {
                        itemCount = curDatas[2].Value<int>();
                    }

                    if (itemNo == -1 || itemCount <= 0 || itemType == -1)
                        continue;

                    Rewards.Add(new Asset(itemNo, itemCount, itemType));
                }
            }

            if (jsonData["levelup"] != null && SBFunc.IsJArray(jsonData["levelup"]))
            {
                LevelUpList = jsonData["levelup"].ToObject<List<int>>();
            }
            else
            {
                LevelUpList = new List<int>();
            }
        }
        public override void Update(float dt)
        {
            UpdateGlobalDelay(dt);
        }
    }
}