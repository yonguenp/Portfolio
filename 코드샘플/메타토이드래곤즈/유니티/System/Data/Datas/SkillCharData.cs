using UnityEngine;
using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class SkillCharData : TableData<DBSkill_char>
    {
        static private SkillCharTable table = null;
        private Sprite SprIcon { get; set; } = null;

        static public SkillCharData Get(int key)
        {
            return Get(key.ToString());
        }

        static public SkillCharData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<SkillCharTable>();

            return table.Get(key);
        }

        static public SkillCharData GetBySkillID(long key)
        {
            if (table == null)
                table = TableManager.GetTable<SkillCharTable>();

            return table.Get(key);
        }

        public int KEY => int.Parse(UNIQUE_KEY);
        public int ANI => Mathf.Max(1, Data.ANI);
        public float WEAK_TIME => Data.WEAK_TIME;
        public float CASTING_TIME => Data.CASTING_TIME;
        public float AFTER_DELAY => Data.AFTER_DELAY;
        public float GLOBAL_COOL_TIME => Data.GLOBAL_COOL_TIME;
        public float START_COOL_TIME => Data.START_COOL_TIME;
        public float COOL_TIME => Data.COOL_TIME;
        public eSkillTargetType TARGET_TYPE => SBFunc.ConvertSkillTargetType(Data.TARGET_TYPE);
        public eSkillTargetSort TARGET_SORT => SBFunc.ConvertSkillTargetSort(Data.TARGET_SORT);
        public float R_X => Data.RANGE_X;
        public float R_Y => Data.RANGE_Y;
        public float RANGE_X { get => R_X * SBDefine.BattleTileX; }
        public float RANGE_Y { get => R_Y * SBDefine.BattleTileY; }
        public string ICON => Data.ICON;
        public string NAME => Data.NAME;
        public string DESC => Data.DESC;
        public int SUMMON_KEY => Data.SUMMON_KEY;
        public int CASTING_EFFECT_RSC_KEY => Data.CASTING_EFFECT_RSC_KEY;
        public float CASTING_EFFECT_RSC_DELAY => Data.CASTING_EFFECT_RSC_DELAY;
        public string CASTING_SOUND => Data.CASTING_SOUND;
        public float CASTING_SOUND_DELAY => Data.CASTING_SOUND_DELAY;
        public eSkillCharCondition SKILL_CONDITION => SBFunc.ConvertSkillCondition(Data.SKILL_CONDITION);
        public eStatusValueType CONDITION_VALUE_TYPE => SBFunc.ConvertValueType(Data.CONDITION_VALUE_TYPE);
        public float CONDITION_VALUE => Data.CONDITION_VALUE;
        public int INF => Data.INF;
                
        public Sprite GetIcon()
        {
            if (SprIcon == null)
            {
                SprIcon = ResourceManager.GetResource<Sprite>(eResourcePath.SkillIconPath, ICON);
                if (SprIcon == null)
                    SprIcon = SkillCharTable.DefaultSkillIcon;
            }

            return SprIcon;
        }
    }
}