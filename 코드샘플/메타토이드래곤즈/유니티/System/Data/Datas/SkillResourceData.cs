using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class SkillResourceData : TableData<DBSkill_resource>
    {
        static private SkillResourceTable table = null;
        static public SkillResourceData Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<SkillResourceTable>();

            return table.Get(key.ToString());
        }

        public string FILE => Data.FILE;
        public string IMAGE => Data.IMAGE;
        public eSkillResourceOrderType ORDER_TYPE => SBFunc.ConvertSkillResourceOrderType(Data.ORDER_TYPE);
        public eSkillResourceLocation LOCATION => SBFunc.ConvertSkillResourceLocation(Data.LOCATION);
        public eSkillResourceFollow FOLLOW => SBFunc.ConvertSkillResourceFollow(Data.FOLLOW);
        public eSkillResourceOrder ORDER => SBFunc.ConvertSkillResourceOrder(Data.ORDER);
        public string VIBE_FILE => Data.VIBE_FILE;
        public float VIBE_DELAY => Data.VIBE_DELAY;
        public float VIBE_LIFE => Data.VIBE_LIFE;
        public bool AUTO_DIRECTION => Data.AUTO_DIRECTION == 1;
    }
}