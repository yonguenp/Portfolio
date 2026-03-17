using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class GemDungeonBattleData : BattleData
    {
        #region IBattleData
        public override float Speed { get; set; } = 1f;
        public override float Time { get; set; } = 0;
        public override eBattleType BattleType { get => eBattleType.GEM_DUNGEON; }
        public override int Wave { get; protected set; } = 1;
        public override int MaxWave { get; protected set; } = 1;
        #endregion

        public override bool CheckTimeOver()
        {
            return false;
        }

        public void SetData(List<int> dragonTag,List<int> monsterSpawnList)
        {
            OffensePos.Clear();
            OffenseDic.Clear();
            Characters.Clear();
            for (int i = 0, count = dragonTag.Count; i < count; ++i)
            {
                OffensePos.Add(i, new());
                var playerData = new BattleDragonData();
                playerData.SetData(i, dragonTag[i]);
                OffensePos[i].Add(new(i, dragonTag[i]));
                OffenseDic.Add(i, playerData);
                Characters.Add(playerData);
            }
            int index = 0;
            DefensePos.Clear();
            DefenseDic.Clear();
            foreach(var key in monsterSpawnList)
            {
                var spawn = MonsterSpawnData.GetKey(key);

                if (spawn != null)
                {
                    int bTag = index %2==0 ? -index:index;
                    DefensePos.Add(bTag, new());
                    int monsterTag = spawn.MONSTER;
                    DefensePos[bTag].Add(new(bTag, spawn.IS_BOSS, spawn.SPAWN_GROUP, key));
                    var enemyData = new GemDungeonMonsterData();
                    enemyData.SetData(bTag, key);
                    DefenseDic.Add(bTag, enemyData);
                    ++index;
                }
            }


            //MaxWave = 0;
            //for (int worldIndex = curWorld; worldIndex < maxWorld; ++worldIndex)
            //{
            //    int maxStageIndex = StageManager.Instance.AdventureProgressData.GetWorldInfoData(worldIndex).GetLastestClearStage();
            //    for (int stageIndex = (worldIndex==curWorld) ? curStage:1; stageIndex < maxStageIndex; ++stageIndex)
            //    {
            //        var BaseData = StageBaseData.GetByWorldStage(worldIndex, stageIndex);
            //        if (BaseData != null)
            //        {
            //            var spawn = MonsterSpawnData.GetBySpawnGroup(BaseData.SPAWN);
            //            if (spawn != null)
            //            {
            //                for (int i = 0, count = spawn.Count; i < count; ++i)
            //                {
            //                    if (spawn[i] == null)
            //                        continue;
            //                    ++MaxWave;
            //                }
            //            }
            //        }
            //    }

            //}
        }


        public void AddMonsterData(List<int> addTag)
        {
            int index = 0;
            foreach (var key in addTag)
            {
                var spawn = MonsterSpawnData.GetKey(key);

                if (spawn != null)
                {
                    int bTag = index % 2 == 0 ? -index : index;
                    while(DefensePos.ContainsKey(bTag))
                    {
                        ++index;
                        bTag = index % 2 == 0 ? -index : index;
                    }
                    DefensePos.Add(bTag, new());
                    int monsterTag = spawn.MONSTER;
                    DefensePos[bTag].Add(new(bTag, spawn.IS_BOSS, spawn.SPAWN_GROUP, key));
                    var enemyData = new GemDungeonMonsterData();
                    enemyData.SetData(bTag, key);
                    DefenseDic.Add(bTag, enemyData);
                    ++index;
                }
            }
        }
        public override void SetData(JObject jsonData) 
        {
            
        }
        public override void Update(float dt)
        {
            UpdateGlobalDelay(dt);
        }
    }
}


