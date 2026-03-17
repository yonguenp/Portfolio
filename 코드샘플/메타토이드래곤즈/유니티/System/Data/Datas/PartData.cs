using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
	public class PartBaseData: TableData<DBPart_base>
	{
		static private PartTable table = null;
		static public PartBaseData Get(int key)
		{
			return Get(key.ToString());
		}
		static public PartBaseData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<PartTable>();

			return table.Get(key);
		}
		static public int GetMaxReinforceCount(string key)
		{
			if (table == null)
				table = TableManager.GetTable<PartTable>();

			return table.GetMaxReinforceCount(key);
		}
		static public int GetMaxReinforceSlotCount(int key)
		{
			if (table == null)
				table = TableManager.GetTable<PartTable>();

			return table.GetMaxReinforceSlotCount(key);
		}
		static public List<PartBaseData> GetAllForChampion()
		{
            if (table == null)
                table = TableManager.GetTable<PartTable>();

            return table.GetGradeAll((int)ePartGrade.Legend);
        }
		static public PartBaseData GetBasePartFromStatType(string type)
        {
			if (table == null)
				table = TableManager.GetTable<PartTable>();

			return table.GetBasePartFromStatType(type);
		}
        public int KEY { get { return Int(UNIQUE_KEY); } }
		public int GRADE => Data.GRADE;
		public string STAT_TYPE => Data.STAT_TYPE;
		public string VALUE_TYPE => Data.VALUE_TYPE;
		public float VALUE => Data.VALUE;
		public float VALUE_GROW => Data.VALUE_GROW;
		public List<int> SUB { get; private set; } = new List<int>();
		public List<int> SUB_STEP { get; private set; } = new List<int>();
		public int SET_GROUP => Data.SET_GROUP;
		public string UNEQUIP_COST_TYPE => Data.UNEQUIP_COST_TYPE;
		public int UNEQUIP_COST_NUM => Data.UNEQUIP_COST_NUM;
		public int INF => Data.INF;

		private int ItemID => Data.ITEM;
		private ItemBaseData item = null;
		public ItemBaseData ITEM 
		{ 
			get { 
				if (item == null && ItemID > 0) 
					item = ItemBaseData.Get(ItemID); 
				
				return item; 
			} 
		}
		public override void SetData(DBPart_base _data)
		{
			SUB.Clear();
			SUB_STEP.Clear();
			base.SetData(_data);

			SUB.Add(Data.SUB_1);
			SUB.Add(Data.SUB_2);
			SUB.Add(Data.SUB_3);
			SUB.Add(Data.SUB_4);
			SUB_STEP.Add(Data.SUB_1_STEP);
			SUB_STEP.Add(Data.SUB_2_STEP);
			SUB_STEP.Add(Data.SUB_3_STEP);
			SUB_STEP.Add(Data.SUB_4_STEP);
		}
	}

	public class PartSetData: TableData<DBPart_set>
	{
		static private PartSetTable table = null;
		static public PartSetData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<PartSetTable>();

			return table.Get(key);
		}
		static public List<PartSetData> GetAllOptionByGroup(int _group)
		{
			if (table == null)
				table = TableManager.GetTable<PartSetTable>();

			return table.GetSetOptionListByGroup(_group);
		}
		/// <summary>
		/// 그룹과 갯수로 해당 데이터 리스트 검색 - count 보다 작은 옵을 전부 찾아냄 (ex. count 가 6이면 하위 애들 전부다)
		/// </summary>
		/// <param name="_group"></param>
		/// <param name="_count"></param>
		/// <returns></returns>
		static public List<PartSetData> GetOptionByGroupAndCount(int _group, int _count)
		{
			if (table == null)
				table = TableManager.GetTable<PartSetTable>();

			return table.GetSetOptionListByGroup(_group, _count);
		}

		public string KEY { get { return UNIQUE_KEY; } }
		public int GROUP => Data.GROUP;
		public ePartSetNum NUM => GetConvertSetNum(Data.NUM);
		public string STAT_TYPE => Data.STAT_TYPE;
		public string VALUE_TYPE => Data.VALUE_TYPE;
		public float VALUE => Data.VALUE;
		private ePartSetNum GetConvertSetNum(int set)
		{
			return set switch
			{
				1 => ePartSetNum.SET_1,
				2 => ePartSetNum.SET_2,
				3 => ePartSetNum.SET_3,
				4 => ePartSetNum.SET_4,
				5 => ePartSetNum.SET_5,
				6 => ePartSetNum.SET_6,
                _ => ePartSetNum.SET_3
			};
		}
    }

	public class PartReinforceData: TableData<DBPart_reinforce>
	{
		static private PartReinforceTable table = null;
		static public PartReinforceData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<PartReinforceTable>();

			return table.Get(key);
		}

		static public PartReinforceData GetDataByGradeAndStep(int grade, int step)
		{
			if (table == null)
				table = TableManager.GetTable<PartReinforceTable>();

			return table.GetDataByGradeAndStep(grade, step);
		}

		static public int GetMaxReinforceStep(int grade)
        {
			if (table == null)
				table = TableManager.GetTable<PartReinforceTable>();

			return table.GetMaxReinforceStep(grade);
		}

		public string KEY { get { return UNIQUE_KEY; } }
		public int GRADE => Data.GRADE;
		public int STEP => Data.STEP;
		public int RATE => Data.RATE;
		public int ITEM => Data.ITEM;
		public int ITEM_NUM => Data.ITEM_NUM;
		public string COST_TYPE => Data.COST_TYPE;
		public int COST_NUM => Data.COST_NUM;
		public int DESTROY => Data.DESTROY;
		public int DESTROY_REWARD => Data.DESTROY_REWARD;

		public int ITEM2 => Data.ITEM2;
		public int ITEM_NUM2 => Data.ITEM_NUM2;
		public int RATE2 => Data.RATE2;
		public int DESTROY2 => Data.DESTROY2;
	}

	public class PartMergeBaseData: TableData<DBPart_merge_base>
	{
		static private PartMergeBaseTable table = null;
		static public PartMergeBaseData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<PartMergeBaseTable>();

			return table.Get(key);
		}
		static public List<PartMergeBaseData> GetDataByGrade(int grade)
		{
			if (table == null)
				table = TableManager.GetTable<PartMergeBaseTable>();

			return table.GetDataByGrade(grade);
		}
		public string KEY { get { return UNIQUE_KEY; } }
		public int GRADE => Data.GRADE;
		public int RATE => Data.RATE;
		public int ITEM => Data.ITEM;
		public int ITEM_NUM => Data.ITEM_NUM;
		public string COST_TYPE => Data.COST_TYPE;
		public int COST_NUM => Data.COST_NUM;
		public int BASE_NUM => Data.BASE_NUM;
		public int RESULT_SUCCESS => Data.RESULT_SUCCESS;
		public int RESULT_FAIL => Data.RESULT_FAIL;
	}

	public class PartMergeReinforceBonusData: TableData<DBPart_merge_reinforcebonus>
	{
		static private PartMergeReinforceBonusTable table = null;
		static public PartMergeReinforceBonusData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<PartMergeReinforceBonusTable>();

			return table.Get(key);
		}
		static public int GetRateByGradeAndReinforceNum(int grade, int reinforce_num)
		{
			if (table == null)
				table = TableManager.GetTable<PartMergeReinforceBonusTable>();

			return table.GetRateByGradeAndReinforceNum(grade, reinforce_num);
		}
		public string KEY { get { return UNIQUE_KEY; } }
		public int GRADE => Data.GRADE;
		public int REINFORCE_NUM => Data.REINFORCE_NUM;
		public int ADD_RATE => Data.ADD_RATE;
	}

	public class PartMergeEquipAmountBonusData: TableData<DBPart_merge_equipamountbonus>
	{
		static private PartMergeEquipAmountBonusTable table = null;
		static public PartMergeEquipAmountBonusData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<PartMergeEquipAmountBonusTable>();

			return table.Get(key);
		}
		static public int GetRateByGradeAndBonusAmountNum(int grade, int extra_num)
		{
			if (table == null)
				table = TableManager.GetTable<PartMergeEquipAmountBonusTable>();

			return table.GetRateByGradeAndBonusAmountNum(grade, extra_num);
		}
		public string KEY { get { return UNIQUE_KEY; } }
		public int GRADE => Data.GRADE;
		public int EXTRA_NUM => Data.EXTRA_NUM;
		public int ADD_RATE => Data.ADD_RATE;
	}

	public class PartDecomposeData: TableData<DBPart_decompose>
	{
		static private PartDecomposeTable table = null;
		static public PartDecomposeData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<PartDecomposeTable>();

			return table.Get(key);
		}
		static public List<int> GetTotalResultItemList()
		{
			if (table == null)
				table = TableManager.GetTable<PartDecomposeTable>();

			return table.GetTotalResultItemList();
		}
		static public PartDecomposeData GetDecomposeDataByGradeAndPartLevel(int grade, int partLevel)
		{
			if (table == null)
				table = TableManager.GetTable<PartDecomposeTable>();

			return table.GetDecomposeDataByGradeAndPartLevel(grade, partLevel);
		}

		public string KEY { get { return UNIQUE_KEY; } }
		public int GRADE => Data.GRADE;
		public int REINFORCE_NUM => Data.REINFORCE_NUM;
		public int ITEM => Data.ITEM;
		public int ITEM_NUM => Data.ITEM_NUM;
	}

	public class PartFusionData : TableData<DBFusion_option>
	{
		static private PartFusionTable table = null;
		static public PartFusionData Get(int key)
		{
			if (table == null)
				table = TableManager.GetTable<PartFusionTable>();

			return table.Get(key);
		}

		static public List<PartFusionData> GetForChampion()
		{
			if (table == null)
				table = TableManager.GetTable<PartFusionTable>();
			
			return table.GetAllList();
		}

		public string KEY { get { return UNIQUE_KEY; } }
		public string STAT => Data.STAT;
		public string VALUE_TYPE => Data.VALUE_TYPE;
		public float VALUE_MIN => Data.VALUE_MIN;
		public float VALUE_MAX => Data.VALUE_MAX;
		public float LEGEND_BONUS => Data.LEGEND_BONUS;
		public float VALUE_REINFORCE => Data.VALUE_REINFORCE;
		public string _DESC => Data._DESC;
	}
}