using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	public class SubOptionData : TableData<DBSub_option>
	{
		static private SubOptionTable table = null;
		static public SubOptionData Get(int key)
		{
			if (table == null)
				table = TableManager.GetTable<SubOptionTable>();

			return table.Get(key);
		}

		static public List<SubOptionData> GetForChampion(PetBaseData petBase, int slot)
		{
			if (table == null)
				table = TableManager.GetTable<SubOptionTable>();

			if(petBase != null && petBase.SUB.Count > slot)
            {
				return table.GetOptionByGroup(petBase.SUB[slot]);
            }

			return new List<SubOptionData>();
		}
		static public List<SubOptionData> GetForChampion(PartBaseData partBase, int slot)
		{
			if (table == null)
				table = TableManager.GetTable<SubOptionTable>();

			if (partBase != null && partBase.SUB.Count > slot)
			{
				return table.GetOptionByGroup(partBase.SUB[slot]);
			}

			return new List<SubOptionData>();
		}

		public int KEY => Int(Data.UNIQUE_KEY);
		public int GROUP => Data.GROUP;
		public string STAT_TYPE => Data.STAT_TYPE;
		public string VALUE_TYPE => Data.VALUE_TYPE;
		public float VALUE_MIN => Data.VALUE_MIN;
		public float VALUE_MAX => Data.VALUE_MAX;
		public float VALUE_STEP => Data.VALUE_STEP;
		public int WEIGHT => Data.WEIGHT;
		public int RATE => Data.RATE;
	}
}