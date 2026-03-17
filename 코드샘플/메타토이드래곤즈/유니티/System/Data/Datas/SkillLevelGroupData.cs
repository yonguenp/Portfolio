using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class SkillLevelStat //스킬 레벨 계산용
    {
        public SkillLevelStat(SkillEffectData data)
        {
            InitializeData(data);
        }
        public float EFFECT_RATE { get; private set; } = 0f;
        public float VALUE { get; private set; } = 0f;
        public float HP { get; private set; } = 0f;
        public float DEF { get; private set; } = 0f;
        public float ATK { get; private set; } = 0f;
        public float LIGHT { get; private set; } = 0f;
        public float DARK { get; private set; } = 0f;
        public float WATER { get; private set; } = 0f;
        public float FIRE { get; private set; } = 0f;
        public float WIND { get; private set; } = 0f;
        public float EARTH { get; private set; } = 0f;
        public float CRI { get; private set; } = 0f;
        public float PVP { get; private set; } = 0f;
        public float BOSS { get; private set; } = 0f;
        public float MAX_TIME { get; private set; } = 0f;
        public float INF { get; private set; } = 0f;

        public void InitializeData(SkillEffectData data)
        {
            EFFECT_RATE += data.RATE;
            VALUE += data.VALUE;
            HP += data.HP;
            DEF += data.DEF;
            ATK += data.ATK;
            LIGHT += data.LIGHT;
            DARK += data.DARK;
            WATER += data.WATER;
            FIRE += data.FIRE;
            WIND += data.WIND;
            EARTH += data.EARTH;
            CRI += data.CRI;
            PVP += data.PVP;
            BOSS += data.BOSS;
            MAX_TIME += data.MAX_TIME;
        }

        public void AddedData(SkillLevelGroupData data, int level)
        {
            EFFECT_RATE += data.EFFECT_RATE * level;
            VALUE += data.VALUE * level;
            HP += data.HP * level;
            DEF += data.DEF * level;
            ATK += data.ATK * level;
            LIGHT += data.LIGHT * level;
            DARK += data.DARK * level;
            WATER += data.WATER * level;
            FIRE += data.FIRE * level;
            WIND += data.WIND * level;
            EARTH += data.EARTH * level;
            CRI += data.CRI * level;
            PVP += data.PVP * level;
            BOSS += data.BOSS * level;
            MAX_TIME += data.MAX_TIME * level;
            INF += data.INF * level;
        }

        public float GetStatByStr(string _statName) => _statName.ToUpper() switch
        {
            "EFFECT_RATE" => EFFECT_RATE,
            "VALUE" => VALUE,
            "HP" => HP,
            "DEF" => DEF,
            "ATK" => ATK,
            "LIGHT" => LIGHT,
            "DARK" => DARK,
            "WATER" => WATER,
            "FIRE" => FIRE,
            "WIND" => WIND,
            "EARTH" => EARTH,
            "CRI" => CRI,
            "PVP" => PVP,
            "BOSS" => BOSS,
            "MAX_TIME" => MAX_TIME,
            "INF"=> INF,
            _ => 0f,
        };
    }
    public class SkillLevelGroupData : TableData<DBSkill_level_group>
    {
        static private SkillLevelGroupTable table = null;
        static public List<SkillLevelGroupData> GetGroup(int group)
        {
            if (table == null)
                table = TableManager.GetTable<SkillLevelGroupTable>();

            return table.GetGroup(group);
        }

        public int GROW_GROUP_KEY => Data.GROW_GROUP_KEY;
        public int GROW_LEVEL => Data.GROW_LEVEL;
        public float EFFECT_RATE => Data.EFFECT_RATE;
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
        public float MAX_TIME => Data.MAX_TIME;
        public int INF => Data.INF;
    }
}