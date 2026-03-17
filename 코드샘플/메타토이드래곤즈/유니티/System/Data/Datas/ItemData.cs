using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	public class ItemBaseData : TableData<DBItem_base>
	{
		static private ItemBaseTable table = null;
		static public ItemBaseData Get(int key)
        {
			return Get(key.ToString());
        }
		static public ItemBaseData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<ItemBaseTable>();

			return table.Get(key);
		}

		static public List<ItemBaseData> GetItemListByKind(eItemKind kind)
		{
			if (table == null)
				table = TableManager.GetTable<ItemBaseTable>();

			return table.GetItemListByKind(kind);
		}

		public int KEY { get { return Int(UNIQUE_KEY); } }

		public eGoodType ASSET_TYPE { get; private set; } = eGoodType.NONE;
		public eItemKind KIND { get; private set; } = eItemKind.RESOURCE;
		private string icon = "";
		private Sprite sprite = null;
		public Sprite ICON_SPRITE {
			get
			{
				if(sprite == null)
                {
					switch (ASSET_TYPE)
                    {
						case eGoodType.GOLD:
							sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
							break;
						case eGoodType.ARENA_TICKET:
							sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "item_pvp_ticket_1");
							break;
						case eGoodType.GEMSTONE:
							sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
							break;
						case eGoodType.ACCOUNT_EXP:
							sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "acc_exp_icon");
							break;
						case eGoodType.MAGNET:
							sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "magnet");
							break;
						case eGoodType.ARENA_POINT:
							sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "icon_arena_point");
							break;
						case eGoodType.EQUIPMENT:
							sprite = ResourceManager.GetResource<Sprite>(eResourcePath.PartsIconPath, icon);
							break;
						case eGoodType.GUILD_POINT:
							sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "guild_point");
							break;
						case eGoodType.GUILD_EXP:
							sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "guild_exp");
							break;
						default:
							sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, icon);
							break;
					}
				}
					
				return sprite;
			}
		}

		public int GRADE { get; private set; } = -1;
		public string SLOT_USE { get; private set; } = "";
		string _NAME = "";
		public string NAME_KEY { get { return _NAME; } }//최대한 안써야하는걸
		public string NAME { get {
				return StringData.GetStringByStrKey(_NAME);
			} 
		}
		string _DESC = "";
		public string DESC
		{
			get
			{
				string ret = StringData.GetStringByStrKey(_DESC);

				switch(KIND)
                {
					case eItemKind.EXP:
					{
						string expString = System.Text.RegularExpressions.Regex.Replace(ret, @"\D", "");
						ret = ret.Replace(expString, ((int)(int.Parse(expString) * ServerOptionData.GetFloat("dragon_exp", 1.0f))).ToString());
					}break;
					default:
						break;
                }
				return ret;
			}
		}
		public int SORT { get; private set; } = -1;
		public int MERGE { get; private set; } = -1;
		public int SELL { get; private set; } = -1;
		public int BUY { get; private set; } = -1;//기본 구매 재화가 다이아
		public bool USE { get; private set; } = false;//0 이면 false , 1이면 true
		public int VALUE { get; private set; } = -1;
		public int PROPERTY { get; private set; } = 0;//각 아이템의 목적성 표시 (bit_flag 형식)
		public bool IS_NORMAL { get { return PROPERTY == 0; } }//일반 아이템
		public bool ENABLE_P2E { get { return (PROPERTY & 1) > 0; } }//지역별 P2E 아이템 노출 여부 구분
		public bool ENABLE_NFT { get { return (PROPERTY & 2) > 0; } }//NFT화 가능 아이템 구분
		public bool ENABLE_RATE_TABLE { get { return (PROPERTY & 4) > 0; } }//확률표 확인 필요 아이템 구분

		public override void SetData(DBItem_base _data)
		{
			Init();
			base.SetData(_data);

			KIND = GetItemKintByInt(Data.KIND);
			icon = Data.ICON;
			ASSET_TYPE = GetGoodTypeByIcon(icon);
			GRADE = Data.GRADE;
			SLOT_USE = Data.SLOT_USE;
			_NAME = Data._NAME;
			_DESC = Data._DESC;
			SORT = Data.SORT;
			MERGE = Data.MERGE;
			SELL = Data.SELL;
			BUY = Data.BUY;
			USE = Data.USE != 0;
			VALUE = Data.VALUE;
			PROPERTY = Data.PROPERTY;

			switch (KIND)
			{
				case eItemKind.EXP:
				{
					VALUE = (int)(VALUE * ServerOptionData.GetFloat("dragon_exp", 1.0f));
				}
				break;
				default:
					break;
			}
		}

		private eGoodType GetGoodTypeByIcon(string _icon) => _icon switch
		{
			"gold" => eGoodType.GOLD,
			"energy" => eGoodType.ENERGY,
			"acc_exp_icon" => eGoodType.ACCOUNT_EXP,
			"exp_icon" => eGoodType.ACCOUNT_EXP,
			"gemstone" => eGoodType.GEMSTONE,
			"magnet" => eGoodType.MAGNET,
			"guild_exp" => eGoodType.GUILD_EXP,
			"guild_point" => eGoodType.GUILD_POINT,
			string partIcon when partIcon.Contains("gem_") && KIND == eItemKind.EQUIP => eGoodType.EQUIPMENT,//장비 강화 템 ("칩셋 블록 리소스 이름이 gem_chipset이라 추가처리)
			_=> eGoodType.ITEM,
		};

		private eItemKind GetItemKintByInt(int strKind)
        {
			return strKind switch
			{
				1 => eItemKind.RESOURCE,
				2 => eItemKind.EVENT,
				3 => eItemKind.GACHA,
				4 => eItemKind.EXP,
				5 => eItemKind.RECEIPE,
				6 => eItemKind.PRODUCT,
				7 => eItemKind.ACC_TICKET,
				8 => eItemKind.SKILL_UP,
				9 => eItemKind.HIGH_RECEIPE,
				10 => eItemKind.EQUIP,
				11 => eItemKind.EQUIP_UPGRADE,
				12 => eItemKind.RECHARGE,
				13 => eItemKind.SWEEP,
				14 => eItemKind.PET_UPGRADE,
				15 => eItemKind.SHOWCASE,
				16 => eItemKind.GEM_BOOSTER,
				17 => eItemKind.GEM_FATIGUE_RECOVERY,
				18 => eItemKind.SKILL_PASSIVE_MATERIAL,
				19 => eItemKind.MINE_BOOSTER,
				_ => eItemKind.RESOURCE
			};
        }
	}

	public class ItemGroupData : TableData<DBItem_group>
	{
		static private ItemGroupTable table = null;
		static public List<ItemGroupData> Get(string key)
        {
			if(int.TryParse(key, out int iKey))
				return Get(iKey);

			return null;
        }
		static public List<ItemGroupData> Get(int key)
		{
			if (table == null)
				table = TableManager.GetTable<ItemGroupTable>();

			return table.GetByGroup(key);
		}
		public int KEY { get { return Int(UNIQUE_KEY); } }
		public int GROUP => Data.GROUP;
		public int ITEM_RATE => Data.ITEM_RATE;

		private string TYPE => Data.TYPE;
		private int VALUE => Data.VALUE;
		private int NUM => Data.NUM;


		private List<ItemGroupData> child = null;
		public List<ItemGroupData> Child { get
			{
                if (child == null)
                    child = new List<ItemGroupData>();

				if(child.Count > 0)
					return child;
                if (TYPE == "DICE_GROUP")
				{
					var childValue = Get(VALUE);
					if(childValue != null)
						child.AddRange(childValue);
                }
                return child;
			}
		}

		public Asset Reward { get; private set; } = null;
        public override void Init()
        {
            base.Init();
			Reward = null;

		}
        public override void SetData(DBItem_group _data)
		{
			base.SetData(_data);
			Reward = new Asset(SBFunc.ConvertStringToItemType(TYPE), VALUE, NUM);
		}
	}

	public class ItemGroupListData : TableData<DBItem_group_list>
	{
		static private ItemGroupListTable table = null;
		static public ItemGroupListData Get(int key)
		{
			if (table == null)
				table = TableManager.GetTable<ItemGroupListTable>();

			return table.Get(key);
		}
		public int KEY => Int(UNIQUE_KEY);
		public int DICE_TYPE => Data.DICE_TYPE;
		public int MAX_RATE => Data.MAX_RATE;
	}

	public class DefineResourceData : TableData<DBDefine_resource>
	{
		static private DefineResourceTable table = null;
		static public DefineResourceData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<DefineResourceTable>();

			return table.Get(key);
		}
		public string KEY { get { return UNIQUE_KEY; } }
		public string DEFINE => Data.DEFINE;
	}
}