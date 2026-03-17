using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class SkillSummonData : TableData<DBSkill_summon>
    {
        static private SkillSummonTable table = null;
        static public SkillSummonData Get(int key)
        {
            return Get(key.ToString());
        }
        static public SkillSummonData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<SkillSummonTable>();

            return table.Get(key);
        }
        public int KEY => int.Parse(UNIQUE_KEY);
        public float DELAY => Data.DELAY;
        public eSkillSummonType TYPE => SBFunc.ConvertSkillSummonType(Data.TYPE);
        public eSkillTarget TARGET => SBFunc.ConvertSkillTarget(Data.TARGET);
        public eSkillTargetType TARGET_TYPE => SBFunc.ConvertSkillTargetType(Data.TARGET_TYPE);
        public eSkillTargetSort TARGET_SORT => SBFunc.ConvertSkillTargetSort(Data.TARGET_SORT);
        public int TARGET_COUNT => Data.TARGET_COUNT;
        public eSkillRangeType RANGE_TYPE => SBFunc.ConvertSkillRangeType(Data.RANGE_TYPE);
        public float R_X => Data.RANGE_X;
        public float R_Y => Data.RANGE_Y;
        public float RANGE_X { get => R_X * SBDefine.BattleTileX; }
        public float RANGE_Y { get => R_Y * SBDefine.BattleTileY; }
        public float GROUND_X => Data.GROUND_X;
        public float GROUND_Y => Data.GROUND_Y;
        public float GROUND_MIN => Data.GROUND_MIN;
        public long EFFECT_GROUP_KEY => Data.EFFECT_GROUP_KEY;
        public int ARROW_SPD => Data.ARROW_SPD;
        /// <summary> 데이터로 들어온 POS_X </summary>
        public int POS_X => Data.POS_X;
        /// <summary> 데이터로 들어온 POS_Y </summary>
        public int POS_Y => Data.POS_Y;
        /// <summary> POS_X를 TilePos로 변경 </summary>
        public float POSITION_X { get => POS_X * SBDefine.BattleTileX; }
        /// <summary> POS_Y를 TilePos로 변경 </summary>
        public float POSITION_Y { get => POS_Y * SBDefine.BattleTileY; }
        public int ARROW_RSC_KEY => Data.ARROW_RSC_KEY;
        public float VALUE1 => Data.VALUE1;
        public float VALUE2 => Data.VALUE2;
        public int SKILL_EFFECT_RSC_KEY => Data.SKILL_EFFECT_RSC_KEY;
        public string SKILL_SOUND => Data.SKILL_SOUND;
        public float SKILL_SOUND_DELAY => Data.SKILL_SOUND_DELAY;
        public eSkillTriggerType TRIGGER_TYPE => SBFunc.ConvertSkillTriggerType(Data.TRIGGER_TYPE);
        public float TRIGGER_VALUE => Data.TRIGGER_VALUE;
        public int NEXT_SUMMON => Data.NEXT_SUMMON;
        private List<SkillEffectData> Effects { get; set; } = null;

        public List<SkillEffectData> GetEffects()
        {
            if (Effects == null)
                Effects = SkillEffectData.GetGroup(EFFECT_GROUP_KEY);

            return Effects;
        }
    }
}