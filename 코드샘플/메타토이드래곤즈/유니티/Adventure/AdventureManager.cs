using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class AdventureManager
    {
        private static AdventureManager instance = null;
        public static AdventureManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new AdventureManager();
                }
                return instance;
            }
        }
        private AdventureBattleData data = null;
        public AdventureBattleData Data
        {
            get
            {
                if (data == null)
                {
                    data = new AdventureBattleData();
                }
                return data;
            }
        }

        public IEnumerator LoadingCoroutine { get; private set; } = null;

        public bool IsStartCheck()
        {
            if (Data == null || Data.World <= 0 || Data.Stage <= 0)
                return false;

            if (Data.BattleTag == -1 || Data.State == eBattleState.None || Data.Wave == -1)
                return false;

            if (Data.OffensePos == null || Data.DefensePos == null)
                return false;

            if (Data.OffensePos.Count <= 0 || Data.DefensePos.Count <= 0)
                return false;

            return true;
        }

        public bool IsWaveCheck()
        {
            if (Data == null)
                return false;

            if (Data.BattleTag == -1 || Data.State == eBattleState.None || Data.Wave == -1)
                return false;

            if (Data.OffensePos == null || Data.DefensePos == null)
                return false;

            if (Data.OffensePos.Count <= 0 || Data.DefensePos.Count <= 0)
                return false;

            return true;
        }

        public bool IsRewardCheck()
        {
            if (Data == null)
                return false;

            switch (Data.State)
            {
                case eBattleState.Win:
                case eBattleState.Lose:
                case eBattleState.TimeOver:
                    break;
                default:
                    return false;
            }

            return true;
        }

        public void SetSimulatorStartData(int world, int stage, JObject jsonObject)
        {
            if (Data == null || jsonObject == null)
                return;

            Data.Initialize();
            Data.SetWorld(world, stage);
            Data.SetData(jsonObject);
        }

        public void SetStartData(int world, int stage, JObject jsonObject)
        {
            if (Data == null || jsonObject == null)
                return;

            Data.Initialize();
            Data.SetWorld(world, stage);
            Data.SetData(jsonObject);
            LoadingCoroutine = StartLoadingCO();
        }

        public void SetAutoTicketData(int world, int stage, int repeat, JObject jsonObject)
        {
            if (Data == null || jsonObject == null)
                return;

            Data.Initialize();
            Data.SetWorld(world, stage);
            Data.SetAutoTicketData(jsonObject, repeat);
        }
        public void SetWaveData(JObject jsonObject)
        {
            if (Data == null || jsonObject == null)
                return;

            Data.InitializeWave();
            Data.SetData(jsonObject);
        }

        public void SetRewardData(JObject jsonObject)
        {
            if (Data == null || jsonObject == null)
                return;

            Data.InitializeReward();
            Data.SetData(jsonObject);
        }

        public IEnumerator StartLoadingCO()
        {
            var loadDatas = new Dictionary<eResourcePath, List<string>>();//로딩 리소스 파일 리스트 확보
            var stageData = StageBaseData.GetByAdventureWorldStage(Data.World, Data.Stage);
            if(stageData != null)
            {
                var spawnDatas = MonsterSpawnData.GetBySpawnGroup(stageData.SPAWN);
                if (spawnDatas != null)
                {
                    for (int i = 0, count = spawnDatas.Count; i < count; ++i)
                    {
                        var spawnData = spawnDatas[i];
                        if (spawnData == null)
                            continue;

                        var baseData = MonsterBaseData.Get(spawnData.MONSTER.ToString());
                        if (baseData == null)
                            continue;

                        SBFunc.AddedResourceKey(loadDatas, eResourcePath.MonsterClonePath, baseData.IMAGE);
                        SBFunc.SkillDataSetLoadingList(loadDatas, baseData.NORMAL_SKILL);
                        SBFunc.SkillDataSetLoadingList(loadDatas, baseData.SKILL1);
                        SBFunc.SkillDataSetLoadingList(loadDatas, baseData.SKILL2);
                    }
                }
            }

            if(Data.Characters != null)
            {
                for(int i = 0, count = Data.Characters.Count; i < count; ++i)
                {
                    if (Data.Characters[i] == null)
                        continue;
                    var curData = Data.Characters[i] as BattleDragonData;
                    if (curData == null || curData.BaseData == null)
                        continue;

                    var baseData = curData.BaseData;

                    SBFunc.AddedResourceKey(loadDatas, eResourcePath.DragonClonePath, baseData.IMAGE);
                    SBFunc.SkillDataSetLoadingList(loadDatas, baseData.NORMAL_SKILL);
                    SBFunc.SkillDataSetLoadingList(loadDatas, baseData.SKILL1);
                    SBFunc.SkillDataSetLoadingList(loadDatas, baseData.SKILL2);

                    SBFunc.PassiveDataSetLoadingList(loadDatas, curData.TranscendenceData);
                }
            }

            yield return ResourceManager.LoadAsyncPaths(loadDatas);

            LoadingCoroutine = null;
            yield break;
        }
    }
}