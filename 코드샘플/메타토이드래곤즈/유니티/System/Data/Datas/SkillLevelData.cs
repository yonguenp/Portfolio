using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class SkillLevelData : TableData<DBSkill_level>
    {
        static private SkillLevelTable table = null;
        static public SkillLevelData Get(int key)
        {
            return Get(key.ToString());
        }

        static public SkillLevelData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<SkillLevelTable>();

            return table.Get(key);
        }

        public static SkillLevelData GetDataByJobAndLevel(eJobType job, int level)
        {
            if (table == null)
                table = TableManager.GetTable<SkillLevelTable>();

            return table.GetDataByJobAndLevel(job, level);
        }

        public static List<SkillLevelData> GetDatasByJob(eJobType job)
        {
            if (table == null)
                table = TableManager.GetTable<SkillLevelTable>();

            return table.GetDatasByJob(job);
        }

        static public int CalculateMaxLevel(UserDragon dragon)
        {
            if (table == null)
                table = TableManager.GetTable<SkillLevelTable>();

            return table.CalculateMaxLevel(dragon);
        }
        public eJobType JOB => (eJobType)Data.JOB;
        public int LEVEL => Data.SKILL_LEVEL;
        public int ITEM => Data.ITEM;
        public int ITEM_NUM => Data.ITEM_NUM;
    }
}