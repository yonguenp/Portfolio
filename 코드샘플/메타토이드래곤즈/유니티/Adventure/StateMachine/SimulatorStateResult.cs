using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
#if DEBUG
    public class SimulatorStateResult : AdventureStateResult
    {
        public bool OnStateEnter()
        {
            if (IsEnter == true) return false;
            IsEnter = true;
            return true;
        }

        public bool OnBaseEnter()
        {
            if (OnStateEnter() && IsData)
            {
                EventManager.AddListener(this);
                if (events == null)
                    events = new();

                return true;
            }
            return false;
        }

        public override bool OnEnter()
        {
            if (OnBaseEnter())
            {
                var lives = 0;
                for (int i = 0, count = Data.Characters.Count; i < count; ++i)
                {
                    if (Data.Characters[i] == null || Data.Characters[i].Death)
                        continue;

                    lives++;
                }

                var result = Data.State;
                if (result == eBattleState.None || result == eBattleState.Playing)
                    result = lives > 0 ? eBattleState.Playing : eBattleState.Lose;

                // 이펙트 커스텀 작동 정지
                EffectReceiverClearEvent.Send();

                //현재 배틀 상태
                var currentState = (int)result;
                JObject totalData = new JObject();

                if (SimulatorLoger.Wave > 0)
                {
                    var currentDataWave = SimulatorLoger.Wave;
                    if (currentDataWave == 3)//웨이브 종료
                    {
                        currentState = (int)result == 3 ? 3 : 2;
                        totalData.Add("state", currentState);
                    }
                    else
                    {
                        var currentWorld = SimulatorLoger.World;
                        var currentStage = SimulatorLoger.Stage;

                        var enemyCustomData = MakeMonsterJobject(currentWorld, currentStage, currentDataWave + 1);

                        totalData.Add("enemy", enemyCustomData);
                        totalData.Add("state", currentState);
                        totalData.Add("wave", currentDataWave + 1);

                        SimulatorLoger.Wave = currentDataWave + 1;
                    }
                }

                switch ((eBattleState)currentState)
                {
                    case eBattleState.Playing:
                        Data.InitializeWave();
                        Data.SetData(totalData);
                        BossAlert();
                        break;
                    case eBattleState.Win:
                    case eBattleState.Lose:
                    case eBattleState.TimeOver:
                        Data.InitializeReward();
                        Data.SetData(totalData);
                        break;
                    case eBattleState.None:
                    default:
                        break;
                }

                switch (result)
                {
                    case eBattleState.Win:
                    case eBattleState.Playing:
                    {
                        Stage.PrevSpines.Clear();

                        var it = Stage.DefenseSpines.GetEnumerator();
                        while (it.MoveNext())
                        {
                            List<BattleSpine> prevEnemys = new List<BattleSpine>();

                            var itit = it.Current.GetEnumerator();
                            while (itit.MoveNext())
                            {
                                //UnityEngine.Object.Destroy(itit.Current.gameObject);
                                prevEnemys.Add(itit.Current);
                            }

                            Stage.PrevSpines.Add(prevEnemys);
                        }
                        Stage.DefenseSpines.Clear();
                    }
                    break;
                }
                return true;
            }
            return false;
        }
        public override void BossAlert()
        {
            base.BossAlert();
        }
        public override bool Update(float dt)
        {
            return base.Update(dt);
        }

        JToken MakeMonsterJobject(int worldIndex, int stageIndex, int waveIndex)
        {
            var stageInfo = StageBaseData.GetByWorldStage(worldIndex, stageIndex);
            var stageSpawnKey = stageInfo.SPAWN;//몬스터 스폰키

            var monsterSpawnData = MonsterSpawnData.GetBySpawnGroup(stageSpawnKey);//현재 스폰 데이터

            List<MonsterSpawnData> waveList = monsterSpawnData.FindAll(data => (data.WAVE == waveIndex));//같은 웨이브 데이터만 뽑아냄
            if (waveList == null || waveList.Count <= 0)
                return null;

            JArray rawData = new JArray();
            for (int i = 0; i < 3; i++)
            {
                JArray colData = new JArray();
                JObject tagData = null;

                List<MonsterSpawnData> rawDataList = waveList.FindAll(data => (data.POSITION == i + 1));//같은 포지션 데이터만 뽑아냄

                if (rawDataList == null || rawDataList.Count <= 0)
                {
                    rawData.Add(colData);
                    continue;
                }

                for (int k = 0; k < rawDataList.Count; k++)
                {
                    tagData = new JObject();

                    var rawDataInfo = rawDataList[k];

                    var position = rawDataInfo.POSITION;
                    var grp = rawDataInfo.GROUP;
                    var id = int.Parse(rawDataInfo.KEY.ToString());
                    var btag = grp + 50;
                    var type = 15;

                    tagData.Add("btag", btag);
                    tagData.Add("type", type);
                    tagData.Add("id", id);
                    tagData.Add("grp", grp);

                    colData.Add(tagData);
                }

                rawData.Add(colData);
            }

            return rawData;
        }
    }
#endif
}