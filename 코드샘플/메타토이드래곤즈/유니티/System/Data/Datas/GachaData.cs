using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	public enum eGachaAmount
	{
		_1TIME_COST = 1,
		_10TIME_COST = 10
	}
	public class GachaShopData : TableData<DBGacha_shop>
    {
        static private GachaShopTable table = null;
        static public GachaShopData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<GachaShopTable>();

            return table.Get(key);
        }

		public string KEY => Data.UNIQUE_KEY;
        public int NAME => Data._NAME;
		public int SORT => Data.SORT;
		public int GROUP1 => Data.GROUP1;
		public int GROUP1_RATE => Data.GROUP1_RATE;
		public int GROUP2 => Data.GROUP2;
		public int GROUP2_RATE => Data.GROUP2_RATE;
		public string COST_TYPE => Data.COST_TYPE;
		public Item GEM_COST { get; private set; } = null;
		public Item TICKET_COST { get; private set; } = null;

        public override void SetData(DBGacha_shop datas)
        {
            base.SetData(datas);

			GEM_COST = new Item((int)eGoodType.GEMSTONE, Data.COST_NUM);
			TICKET_COST = new Item(Data.TICKET, (int)eGachaAmount._1TIME_COST);
        }
    }

    public class GachaListData : TableData<DBGacha_list>
    {
        static private GachaListTable table = null;
        static public List<GachaListData> Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<GachaListTable>();

            return table.GetByGroup(key);
        }

        public string KEY => Data.UNIQUE_KEY;
		public int GROUP => Data.GROUP;
		public int TYPE => Data.TYPE;
		public int VALUE => Data.VALUE;
		public int NUM => Data.NUM;
		public int RATE => Data.RATE;
		public string FBX => Data.FBX;
		public int GRADE => Data.GRADE;
		public float SHOW_RATE => Data.SHOW_RATE;
	}

    public class GachaGroupData : TableData<DBGacha_group>
	{
		static private GachaGroup table = null;
		static public GachaGroupData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<GachaGroup>();

			return table.Get(key);
		}

		static public GachaGroupData Get(int key)
		{
			return Get(key.ToString());
		}

		static public List<GachaGroupData> GetAll()
		{
			if (table == null)
				table = TableManager.GetTable<GachaGroup>();

			List<GachaGroupData> ret = new List<GachaGroupData>();
			List<GachaGroupData> list = table.GetAllList();
			foreach(var item in list)
            {
				if (item.IsValid())
					ret.Add(item);
            }

			return ret;
		}

		public int key => Int(Data.UNIQUE_KEY);
		public string resource => Data.resource;
		public Sprite sprite { get { return CDNManager.LoadBanner("gacha/" + resource); } }
		public int weight => Data.list_weight;

		public bool isLimit => Data.time_limit > 0;
		public DateTime start
        {
			get
            {
				if (isLimit)
					return Date(Data.start_time);
				else
					return DateTime.MinValue;
			}
        }
		public DateTime end
		{
			get
			{
				if (isLimit)
					return Date(Data.end_time);
				else
					return DateTime.MinValue;
			}
		}

		public string Name { get { return StringData.GetStringByStrKey("gacha_group:" + key.ToString()); } }

		private List<GachaMenuData> menus = new List<GachaMenuData>();

		public override void SetData(DBGacha_group data)
		{
			menus.Clear();
			base.SetData(data);
			if (!string.IsNullOrEmpty(resource))
				CDNManager.AddCDNResourceQueue("gacha/" + resource);
		}

		public bool IsValid()
        {
			var curMenus = GetGachaMenus();
			if (curMenus.Count <= 0)
				return false;

			if(isLimit)
            {
				return (start <= TimeManager.GetDateTime() && end > TimeManager.GetDateTime());
            }

			return true;
        }

		// true : 실제 가챠 가능한 메뉴만 리턴 / false : 가챠 가능한 메뉴 포함, 남은 기간이 있는 메뉴까지 리턴
		public List<GachaMenuData> GetGachaMenus(bool valid_only = true)
        {
			List<GachaMenuData> ret = new List<GachaMenuData>();
			
			foreach (var menu in menus)
			{
				if (!menu.IsEnable())
					continue;

				if (menu.IsValid() && valid_only)
                {
					ret.Add(menu);
				}
				else if (menu.IsShowUI() && !valid_only)
                {
					ret.Add(menu);
				}
			}
			
			return ret;
        }

		public void AddMenu(GachaMenuData menu)
        {
			if (menu == null)
				return;

			menus.Add(menu);
        }
	}
	public class GachaMenuData : TableData<DBGacha_menu>
	{
		static private GachaMenu table = null;
		static public GachaMenuData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<GachaMenu>();

			if (table.ContainsKey(key))
				return table.Get(key);
			else
				return null;
		}

		static public GachaMenuData Get(int key)
		{
			return Get(key.ToString());
		}

		public int key => Int(Data.UNIQUE_KEY);
		public int group => Data.group;
		public int menu_type => Data.menu_type;
		public string bg_type => Data.bg_type;
		public int resource_type => Data.resource_type;
		public string resource => Data.resource_path;
		public Sprite sprite { get { return CDNManager.LoadBanner("gacha/"+resource); } }
		public int weight => Data.list_weight;

		public bool isLimit => Data.time_limit > 0;
		public DateTime start
		{
			get
			{
				if (isLimit)
					return Date(Data.start_time);
				else
					return DateTime.MinValue;
			}
		}
		public DateTime end
		{
			get
			{
				if (isLimit)
					return Date(Data.end_time);
				else
					return DateTime.MinValue;
			}
		}
		public List<GachaTypeData> typeDatas { get; private set; } = new List<GachaTypeData>();
		public string Name { get { return StringData.GetStringByStrKey("gacha_menu:" + key.ToString()); } }
		public override void SetData(DBGacha_menu data)
		{
			base.SetData(data);

			var groupData = GachaGroupData.Get(group);
			if (groupData != null)
				groupData.AddMenu(this);

			if (!string.IsNullOrEmpty(resource) && data.resource_type != 4)
				CDNManager.AddCDNResourceQueue("gacha/" + resource);
		}
		public bool IsEnable()
        {
			//if (key == 302)
			//{
			//	return !User.Instance.ENABLE_P2E;
			//}

			return true;
        }
		public bool IsValid()
		{
			if (!IsEnable())
				return false;

			if (isLimit)
			{
				return (start < TimeManager.GetDateTime() && end > TimeManager.GetDateTime());
			}

			return true;
		}

		public bool IsShowUI()
        {
			if (!IsEnable())
				return false;

			if (isLimit)
            {
				return start > TimeManager.GetDateTime() || end > TimeManager.GetDateTime();
			}

			return true;
        }

		public void AddTypeData(GachaTypeData data)
        {
			typeDatas.Add(data);
        }

		public bool IsEvent()
        {
			if(typeDatas.Count > 0)
            {
				JArray data = null;
				switch (typeDatas[0].proc_group)
                {
					case 102:
						data = JArray.Parse(GameConfigTable.GetConfigValue("DEFAULT_GACHA_RATE_" + typeDatas[0].proc_group, "[4260,4000,1340,350,50]"));						
						break;
					case 103:
						data = JArray.Parse(GameConfigTable.GetConfigValue("DEFAULT_GACHA_RATE_" + typeDatas[0].proc_group, "[7420,2000,500,80]"));
						break;
					case 104:
					case 105:
					case 106:
					case 107:
					case 108:
					case 109:
						data = JArray.Parse(GameConfigTable.GetConfigValue("DEFAULT_GACHA_RATE_" + typeDatas[0].proc_group, "[3625,4000,2000,350,25]"));
						break;
					case 114:
						data = JArray.Parse(GameConfigTable.GetConfigValue("DEFAULT_GACHA_RATE_" + typeDatas[0].proc_group, "[4020,4000,1580,350,50]"));
						break;
					default: 
						if(typeDatas[0].proc_group > 700 && typeDatas[0].proc_group < 800)
                        {
							data = JArray.Parse(GameConfigTable.GetConfigValue("DEFAULT_GACHA_RATE_" + typeDatas[0].proc_group, "[7400,2000,500,100]"));
						}
						break;
				}
				if (data != null)
				{
					if (typeDatas[0].Rate.Count == data.Count)
					{
						for (int i = 0; i < data.Count; i++)
						{
							if (typeDatas[0].Rate[i].weight > data[i].Value<int>())
								return true;
						}
					}
				}
			}

			return false;
		}
	}
	public class GachaTypeData : TableData<DBGacha_type>
	{
		public int key => Int(Data.UNIQUE_KEY);
		public int menu_id => Data.menu_id;
		public eGoodType price_type => (eGoodType)Data.price_type;
		public int price_uid => Data.price_uid;
		public int price_value => Data.price_value;
		public int repeats => Data.repeats;
		public int proc_group => Data.proc_group;
		public List<GachaRateData> Rate { get { return GachaRateData.GetGroup(proc_group); } }

		static private GachaType table = null;
		static public GachaTypeData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<GachaType>();

			return table.Get(key);
		}

		static public GachaTypeData Get(int key)
		{
			return Get(key.ToString());
		}

		static public List<GachaTypeData> GetByPriceItem(int itemNo)
        {
			if (table == null)
				table = TableManager.GetTable<GachaType>();

			return table.GetByPriceItem(itemNo);
		}

		public override void SetData(DBGacha_type datas)
		{
			base.SetData(datas);
			var menu = GachaMenuData.Get(menu_id);
			if (menu != null)
				menu.AddTypeData(this);
		}
	}
	public class GachaRateData : TableData<DBGacha_rate>
	{
		static private GachaRate table = null;
		static public GachaRateData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<GachaRate>();

			return table.Get(key);
		}
		static public GachaRateData Get(int key)
		{
			return Get(key.ToString());
		}

		static public List<GachaRateData> GetGroup(int group)
        {
			if (table == null)
				table = TableManager.GetTable<GachaRate>();

			return table.GetGroup(group);
		}


		public int key => Int(Data.UNIQUE_KEY);
		public int group_id => Data.group_id;
		public int grade => Data.grade;
		public int weight => Data.weight;
		public string reward_type => Data.reward_type;
		public int result_id => Data.result_id;
		public int result_type => Data.result_type;
		public int sort => Data.sort;
		public string Name { get { return StringData.GetStringByStrKey("gacha_rate:" + key.ToString()); } }
		public Color Color 
		{ 
			get 
			{ 
				string strColor = StringData.GetStringByStrKey("gacha_rate:color:" + key.ToString());
				
				if(ColorUtility.TryParseHtmlString(strColor, out Color color))
                {
					return color;
                }
				else
                {
					return new Color(0.2117647f, 0.2666667f, 0.4470588f);
				}
			} 
		}
		public List<GachaRateData> Child { get; private set; } = new List<GachaRateData>();

		public override void SetData(DBGacha_rate data)
		{
			Child.Clear();
			base.SetData(data);
		}

		public void AddChild(GachaRateData child)
        {
			Child.Add(child);
        }


		//enum GACHA_SUBGROUP
  //      {
		//	DRAGON_COMMON = 10201,
		//	DRAGON_UNCOMMON = 10202,
		//	DRAGON_RARE = 10203,
		//	DRAGON_UNIQUE = 10204,
		//	DRAGON_LEGEND = 10101,

		//	PET_COMMON = 30101,
		//	PET_UNCOMMON = 30102,
		//	PET_RARE = 30103,
		//	PET_UNIQUE = 30104,
		//	PET_LEGEND = 30105,

		//	TANKER_COMMON = 10223,
		//	TANKER_UNCOMMON = 10205,
		//	TANKER_RARE = 10206,
		//	TANKER_UNIQUE = 10207,

		//	SUPPORTER_COMMON = 10224,
		//	SUPPORTER_UNCOMMON = 10208,
		//	SUPPORTER_RARE = 10209,
		//	SUPPORTER_UNIQUE = 10210,

		//	WARRIOR_COMMON = 10225,
		//	WARRIOR_UNCOMMON = 10211,
		//	WARRIOR_RARE = 10212,
		//	WARRIOR_UNIQUE = 10213,

		//	SNIPER_COMMON = 10226,
		//	SNIPER_UNCOMMON = 10214,
		//	SNIPER_RARE = 10215,
		//	SNIPER_UNIQUE = 10216,

		//	ASSASIN_COMMON = 10227,
		//	ASSASIN_UNCOMMON = 10217,
		//	ASSASIN_RARE = 10218,
		//	ASSASIN_UNIQUE = 10219,

		//	BOMBER_COMMON = 10228,
		//	BOMBER_UNCOMMON = 10220,
		//	BOMBER_RARE = 10221,
		//	BOMBER_UNIQUE = 10222,
		//}

  //      public string GetServerOptionGachaKey()
  //      {
		//	switch ((GACHA_SUBGROUP)result_id)
		//	{
		//		case GACHA_SUBGROUP.DRAGON_COMMON:
		//		case GACHA_SUBGROUP.TANKER_COMMON:
		//		case GACHA_SUBGROUP.SUPPORTER_COMMON:
		//		case GACHA_SUBGROUP.WARRIOR_COMMON:
		//		case GACHA_SUBGROUP.SNIPER_COMMON:
		//		case GACHA_SUBGROUP.ASSASIN_COMMON:
		//		case GACHA_SUBGROUP.BOMBER_COMMON:
		//		case GACHA_SUBGROUP.PET_COMMON:
		//			return "common";
		//		case GACHA_SUBGROUP.DRAGON_UNCOMMON:
		//		case GACHA_SUBGROUP.TANKER_UNCOMMON:
		//		case GACHA_SUBGROUP.SUPPORTER_UNCOMMON:
		//		case GACHA_SUBGROUP.WARRIOR_UNCOMMON:
		//		case GACHA_SUBGROUP.SNIPER_UNCOMMON:
		//		case GACHA_SUBGROUP.ASSASIN_UNCOMMON:
		//		case GACHA_SUBGROUP.BOMBER_UNCOMMON:
		//		case GACHA_SUBGROUP.PET_UNCOMMON:
		//			return "uncommon";
		//		case GACHA_SUBGROUP.DRAGON_RARE:
		//		case GACHA_SUBGROUP.TANKER_RARE:
		//		case GACHA_SUBGROUP.SUPPORTER_RARE:
		//		case GACHA_SUBGROUP.WARRIOR_RARE:
		//		case GACHA_SUBGROUP.SNIPER_RARE:
		//		case GACHA_SUBGROUP.ASSASIN_RARE:
		//		case GACHA_SUBGROUP.BOMBER_RARE:
		//		case GACHA_SUBGROUP.PET_RARE:
		//			return "rare";
		//		case GACHA_SUBGROUP.DRAGON_UNIQUE:
		//		case GACHA_SUBGROUP.TANKER_UNIQUE:
		//		case GACHA_SUBGROUP.SUPPORTER_UNIQUE:
		//		case GACHA_SUBGROUP.WARRIOR_UNIQUE:
		//		case GACHA_SUBGROUP.SNIPER_UNIQUE:
		//		case GACHA_SUBGROUP.ASSASIN_UNIQUE:
		//		case GACHA_SUBGROUP.BOMBER_UNIQUE:
		//		case GACHA_SUBGROUP.PET_UNIQUE:
		//			return "unique";
		//		case GACHA_SUBGROUP.PET_LEGEND:
		//			return "legend";
		//		default:
		//			return string.Empty;
		//	}
		//}

		public string GetServerOptionGachaKey()
        {
			switch(grade)
            {
				case 1: return "common";
				case 2: return "uncommon";
				case 3: return "rare";
				case 4: return "unique";
				case 5: return "legend";
				default:
					return string.Empty;
            }
        }
	}

	public class RateTableUrlData : TableData<DBRate_table_url>
	{
		static private RateTableUrlTable table = null;
		static public string Get(eRateBoardType type)
		{
			if (table == null)
				table = TableManager.GetTable<RateTableUrlTable>();

			int key = (int)type;

			var data = table.Get(key);
			if (data == null)
				return string.Empty;

			switch (GamePreference.Instance.GameLanguage)
			{
				case SystemLanguage.Korean:
					return data.Data.KOR;
				case SystemLanguage.English:
					return data.Data.ENG;
				case SystemLanguage.Japanese:
					return data.Data.JPN;
				case SystemLanguage.Portuguese:
					return data.Data.PRT;
				case SystemLanguage.ChineseSimplified:
					return data.Data.CHS;
				case SystemLanguage.ChineseTraditional:
					return data.Data.CHT;
				default:
					return data.Data.ENG;
			}
		}
	}
}