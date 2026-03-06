using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class ElementRateData : TableData<DBElement_rate>
    {
        static private ElementTable table = null;
        static public ElementRateData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<ElementTable>();

            return table.Get(key);
        }
        static public ElementRateData Get(eElementType type)
        {
            if (table == null)
                table = TableManager.GetTable<ElementTable>();

            return table.Get(type);
        }
        public string KEY => Data.UNIQUE_KEY;
        public string A_ELEMENT => Data.A_ELEMENT;
        public int T_FIRE => Data.T_FIRE;
        public int T_WATER => Data.T_WATER;
        public int T_EARTH => Data.T_EARTH;
        public int T_WIND => Data.T_WIND;
        public int T_LIGHT => Data.T_LIGHT;
        public int T_DARK => Data.T_DARK;
    }
}