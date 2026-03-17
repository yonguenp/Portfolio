using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public abstract class MonsterData : BattleCharacterData
    {
        private MonsterSpawnData spawnData = null;
        public MonsterSpawnData SpawnData
        {
            get
            {
                if (spawnData == null)
                {
                    spawnData = MonsterSpawnData.GetKey(ID);
                }
                return spawnData;
            }
            set
            {
                spawnData = value;
            }
        }
        public override ICharacterBaseData BaseData
        {
            get
            {
                if (baseData == null)
                {
                    baseData = MonsterBaseData.Get(SpawnData.MONSTER.ToString());
                }
                return baseData;
            }
            set
            {
                baseData = value;
            }
        }
        protected StatFactorData factorData = null;
        public virtual StatFactorData FactorData
        {
            get
            {
                if (factorData == null)
                    factorData = StatFactorData.Get(BaseData.FACTOR.ToString());

                return factorData;
            }
            set
            {
                factorData = value;
            }
        }
        protected CharGradeData gradeData = null;
        public CharGradeData GradeData
        {
            get
            {
                if (gradeData == null)
                    gradeData = CharGradeData.Get(BaseData.GRADE.ToString());

                return gradeData;
            }
            set
            {
                gradeData = value;
            }
        }
        public override bool IsEnemy { get => true; }
        public override bool IsLeft { get => false; }
        public override bool IsBoss { get => SpawnData != null && SpawnData.IS_BOSS > 0; }
    }
}