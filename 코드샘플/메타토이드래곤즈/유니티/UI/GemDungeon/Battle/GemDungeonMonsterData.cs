using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class GemDungeonMonsterData : BattleMonsterData
    {
        public GemDungeonMonsterData()
            :base(eStageType.UNKNOWN)
        {

        }

        public override void SetData(int pos, JToken token)
        {
            Alive = true;
        }

        public void SetData(int pos, int monsterID)
        {
            Alive = true;
            Position = pos;
            Level = 1;
            SkillLevel = 1;
            PetID = -1;
            ID = monsterID;
            Stat = SBFunc.BaseMonsterStatus(SpawnData.LEVEL, BaseData, FactorData, stageType);
            MaxHP = Stat.GetTotalStatusInt(eStatusType.HP);
            HP = MaxHP;

            if (Skill1 != null)
                SetSkill1Delay(Skill1.START_COOL_TIME - (Skill1.START_COOL_TIME * Stat.GetTotalStatusConvert(eStatusType.DEL_START_COOLTIME)));

            if (Infos == null)
                Infos = new();
            if (PriorityTargets == null)
                PriorityTargets = new();
        }
    }

}
