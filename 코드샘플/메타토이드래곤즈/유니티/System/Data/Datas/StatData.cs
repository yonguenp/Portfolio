using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public enum UserType
    {
        None = 0,

        START,

        User = START,
        Monster,
        Boss,

        MAX,
    }

    public class StatFactorData : TableData<DBStat_factor>
    {
        static private StatTable table = null;
        static public StatFactorData Get(int key)
        {
            return Get(key.ToString());
        }
        static public StatFactorData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<StatTable>();

            return table.Get(key);
        }
        public UserType USER => (UserType)Data.USER;
        public int ATK => Data.ATK;
        public int DEF => Data.DEF;
        public int HP => Data.HP;
        public float CRI_PROC => Data.CRI_PROC;
        public int CRI_DMG => Data.CRI_DMG;
        public int LIGHT_DMG => Data.LIGHT_DMG;
        public int DARK_DMG => Data.DARK_DMG;
        public int WATER_DMG => Data.WATER_DMG;
        public int FIRE_DMG => Data.FIRE_DMG;
        public int WIND_DMG => Data.WIND_DMG;
        public int EARTH_DMG => Data.EARTH_DMG;
        public float ADD_ATKSPEED => Data.ADD_ATKSPEED;        
    }
    public class StatTypeData : TableData<DBStat_type>
    {
        static private StatTypeTable table = null;
        static public StatTypeData Get(int key)
        {
            return Get(key.ToString());
        }
        static public StatTypeData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<StatTypeTable>();

            return table.Get(key);
        }
        static public StatTypeData Get(eStatusType key)
        {
            if (table == null)
                table = TableManager.GetTable<StatTypeTable>();

            return table.Get(key);
        }
        public string KEY => Data.UNIQUE_KEY;
        public eStatusType STAT_TYPE => SBFunc.ConvertStatusType(Data.STAT_TYPE);
        public eStatusValueType VALUE_TYPE => SBFunc.ConvertValueType(Data.VALUE);
        public int STAT_MIN => Data.STAT_MIN;
        public int STAT_MAX => Data.STAT_MAX;
        public string TYPE_DESC
        {
            get
            {
                switch (VALUE_TYPE)
                {
                    case eStatusValueType.VALUE:
                        return VALUE_DESC;
                    case eStatusValueType.PERCENT:
                        return PERCENT_DESC;
                    default:
                        return "";
                }
            }
        }

        public string TYPE_STR
        {
            get
            {
                switch(STAT_TYPE)
                {
                    case eStatusType.START:
                        return "ATK";
                    default:
                        return STAT_TYPE.ToString();
                }
            }
        }

        public string STAT_DESC { get { return StringData.GetStringByStrKey(TYPE_STR); } }
        public string PERCENT_DESC { get { return StringData.GetStringByStrKey(TYPE_STR + "_PERCENT"); } }
        public string VALUE_DESC { get { return StringData.GetStringByStrKey(TYPE_STR + "_VALUE"); } }
        public int SORT_GROUP => Data.SORT_GROUP;
        public int GEM_STAT => Data.GEM_STAT;
        
        public static eStatusValueType GetDataType(eStatusType type)
        {
            var data = Get(type);
            if (data == null)
                return eStatusValueType.VALUE;

            return data.VALUE_TYPE;
        }
        public static float GetStatValue(eStatusType type, float value)
        {
            var data = Get(type);
            if (data == null)
                return value;

            var statMin = data.STAT_MIN;
            var statMax = data.STAT_MAX;
            if (statMin != -1 && value < statMin)
                value = statMin;
            else if (statMax != -1 && statMax < value)
                value = statMax;

            return value;
        }

        public static string GetDescStringByStatType(string strType)
        {
            var str = "";
            var data = Get(SBFunc.ConvertStatusType(strType));
            if (data != null)
            {
                str = StringData.GetStringByStrKey(data.TYPE_DESC);
            }
            return str;
        }

        /// <summary>
        /// 펫은 바깥쪽에서 percent Type을 체크해서 넘기는 형태라, 위의 함수를 쓰면 안됨.
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public static string GetDescStringByStatType(string strType, bool _isPercent)
        {
            var data = Get(SBFunc.ConvertStatusType(strType));
            if (data != null)
            {
                return _isPercent ? data.PERCENT_DESC : data.VALUE_DESC;
            }
            return data.TYPE_DESC;
        }
    }
}