using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class WorldData : TableData<DBWorld_base>
    {
        static private WorldTable table = null;
        static public WorldData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<WorldTable>();

            return table.Get(key);
        }
        static public WorldData GetByWorldNumber(int worldNumber)
        {
            if (table == null)
                table = TableManager.GetTable<WorldTable>();

            return table.GetByWorldNumber(worldNumber);
        }

        public int NUM => Data.NUM;
        public int _NAME => Data._NAME;
        public string BACKGROUND => Data.BACKGROUND;
        public string IMAGE => Data.IMAGE;

        List<int> star = null;
        List<int> reward = null;
        public List<int> STAR
        {
            get { 
                if(star == null)
                {
                    star = new List<int>();
                    star.Add(Data.STAR_1);
                    star.Add(Data.STAR_2);
                    star.Add(Data.STAR_3);
                }

                return star;
            }
        }
        public List<int> STAR_REWARD
        {
            get
            {
                if (reward == null)
                {
                    reward = new List<int>();
                    reward.Add(Data.REWARD_STAR_1);
                    reward.Add(Data.REWARD_STAR_2);
                    reward.Add(Data.REWARD_STAR_3);
                }

                return reward;
            }
        }

    }
}