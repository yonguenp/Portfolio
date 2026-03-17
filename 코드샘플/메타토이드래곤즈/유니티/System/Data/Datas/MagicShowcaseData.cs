using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	public enum eShowcaseGroupType
    {
		NONE,
		MENU_TYPE_ATK,
		MENU_TYPE_DEF,
		MENU_TYPE_ELEMENT,
		MENU_TYPE_CRI,
		MENU_TYPE_PVP,
		MENU_TYPE_BOSS_DMG,
		MAX,
    }

    public class MagicShowcaseData : TableData<DBMagic_showcase_info>
    {
		static private MagicShowcaseTable table = null;
		static public MagicShowcaseData Get(int key)
		{
			return Get(key.ToString());
		}
		static public MagicShowcaseData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<MagicShowcaseTable>();

			return table.Get(key);
		}
		static public List<MagicShowcaseData> GetDataByGroup(eShowcaseGroupType _group)
		{
			if (table == null)
				table = TableManager.GetTable<MagicShowcaseTable>();

			return table.GetDataByGroup((int)_group);
		}
		static public MagicShowcaseData GetDataByGroupAndLevel(eShowcaseGroupType _group , int _level)
		{
			if (table == null)
				table = TableManager.GetTable<MagicShowcaseTable>();

			return table.GetDataByGroupAndLevel((int)_group, _level);
		}
		static public List<MagicShowcaseData> GetAccumulateDataByLevel(eShowcaseGroupType _group, int _level, bool _isInclude = true)
		{
			if (table == null)
				table = TableManager.GetTable<MagicShowcaseTable>();

			return table.GetAccumulateDataByLevel((int)_group, _level, _isInclude);
		}
		static public List<MagicShowcaseData> GetNextTotalDataByLevel(eShowcaseGroupType _group, int _level, bool _isInclude = false)
		{
			if (table == null)
				table = TableManager.GetTable<MagicShowcaseTable>();

			return table.GetNextTotalDataByLevel((int)_group, _level, _isInclude);
		}
		static public int GetMaxLevelByGroup(eShowcaseGroupType _group)
		{
			if (table == null)
				table = TableManager.GetTable<MagicShowcaseTable>();

			return table.GetMaxLevelByGroup((int)_group);
		}

		public string KEY { get { return UNIQUE_KEY; } }

		public eShowcaseGroupType GROUP => (eShowcaseGroupType)Data.GROUP;
		public int LEVEL => Data.LEVEL;
		public List<Asset> MATERIAL_LIST { get; private set; } = new List<Asset>();
		
		private int stat_type_key => Data.STAT_TYPE_KEY;
		public eStatusType STAT_TYPE { 
			get 
			{
				var statData = StatTypeData.Get(stat_type_key);
				if (statData != null)
					return statData.STAT_TYPE;
				else
					return eStatusType.NONE;
			} 
		}
		public eStatusValueType STAT_VALUE_TYPE => (eStatusValueType)Data.STAT_VALUE_TYPE;
		public float STAT_VALUE => Data.STAT_VALUE;

		public override void SetData(DBMagic_showcase_info _data)
		{
			base.SetData(_data);

			SetMaterialList("CORE1_AMOUNT", Data.CORE1_AMOUNT);
			SetMaterialList("CORE2_AMOUNT", Data.CORE2_AMOUNT);
			SetMaterialList("CORE3_AMOUNT", Data.CORE3_AMOUNT);
		}
		void SetMaterialList(string _key , int value)
        {
			int itemNo = _key switch{
				"CORE1_AMOUNT" => 150000001,
				"CORE2_AMOUNT" => 150000002,
				"CORE3_AMOUNT" => 150000003,
				_=>0,
			};

			if(value > 0 && itemNo > 0)
				MATERIAL_LIST.Add(new Asset(itemNo, value));
		}
	}
}
