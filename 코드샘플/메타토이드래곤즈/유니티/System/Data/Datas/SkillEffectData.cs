using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class SkillEffectData : TableData<DBSkill_effect>
    {
        static private SkillEffectTable table = null;
        static public List<SkillEffectData> GetGroup(long group)
        {
            if (table == null)
                table = TableManager.GetTable<SkillEffectTable>();

            return table.GetGroup(group);
        }
        static public SkillEffectData Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<SkillEffectTable>();

            return table.Get(key);
        }

        public long GROUP_KEY => Data.GROUP_KEY;
        public float DELAY => Data.DELAY;
        public float RATE => Data.RATE;
        public eSkillEffectType TYPE => SBFunc.ConvertSkillEffectType(Data.TYPE);
        public eStatusType STAT_TYPE => SBFunc.ConvertStatusType(Data.STAT_TYPE);
        public eSkillTargetType EX_TARGET_TYPE => SBFunc.ConvertSkillTargetType(Data.EX_TARGET_TYPE);
        public eSkillTargetSort EX_TARGET_SORT => SBFunc.ConvertSkillTargetSort(Data.EX_TARGET_SORT);
        public eSkillRangeType EX_RANGE_TYPE => SBFunc.ConvertSkillRangeType(Data.EX_RANGE_TYPE);
        protected float EX_X => Data.EX_X;
        protected float EX_Y => Data.EX_Y;
        public float EXPLOSION_X { get => EX_X * SBDefine.BattleTileX; }
        public float EXPLOSION_Y { get => EX_Y * SBDefine.BattleTileX; }
        protected float EX_GROUND_X => Data.EX_GROUND_X;
        protected float EX_GROUND_Y => Data.EX_GROUND_Y;
        public float EXPLOSION_GROUND_X { get => EX_GROUND_X * SBDefine.BattleTileX; }
        public float EXPLOSION_GROUND_Y { get => EX_GROUND_Y * SBDefine.BattleTileX; }
        public float EX_GROUND_MIN => Data.EX_GROUND_MIN;
        public float EX_GROUND_MAX => Data.EX_GROUND_MAX;
        public int EX_TARGET_COUNT => Data.EX_TARGET_COUNT;
        public eStatusValueType VALUE_TYPE => SBFunc.ConvertValueType(Data.VALUE_TYPE);
        public float VALUE => Data.VALUE;
        public float HP => Data.HP;
        public float DEF => Data.DEF;
        public float ATK => Data.ATK;
        public float LIGHT => Data.LIGHT;
        public float DARK => Data.DARK;
        public float WATER => Data.WATER;
        public float FIRE => Data.FIRE;
        public float WIND => Data.WIND;
        public float EARTH => Data.EARTH;
        public float CRI => Data.CRI;
        public float PVP => Data.PVP;
        public float BOSS => Data.BOSS;
        public int GROW_GROUP_KEY => Data.GROW_GROUP_KEY;
        public int TARGET_EFFECT_RSC_KEY => Data.TARGET_EFFECT_RSC_KEY;
        public int NEST_GROUP => Data.NEST_GROUP;
        public int NEST_COUNT => Data.NEST_COUNT;
        public int HIT_COUNT => Data.HIT_COUNT;
        public float MAX_TIME => Data.MAX_TIME;
        public eSkillTriggerType TRIGGER_TYPE => SBFunc.ConvertSkillTriggerType(Data.TRIGGER_TYPE);
        public float TRIGGER_VALUE => Data.TRIGGER_VALUE;
        public int NEXT_EFFECT => Data.NEXT_EFFECT;        
    }
}