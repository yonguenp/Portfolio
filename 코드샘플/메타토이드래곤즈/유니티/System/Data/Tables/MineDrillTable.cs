using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class MineDrillTable : TableBase<MineDrillData, DBMine_drill>
	{
		Dictionary<int, MineDrillData> dicGroup = new Dictionary<int, MineDrillData>();//key : level , value : data
		public override void DataClear()
		{
			base.DataClear();
			if (dicGroup == null)
				dicGroup = new();
			else
				dicGroup.Clear();
		}
		public override void Init()
		{
			base.Init();
			if (dicGroup == null)
				dicGroup = new();
			else
				dicGroup.Clear();
		}
		public override void Preload()
		{
			Init();
			base.Preload();
			LoadAll();
		}
		protected override bool Add(MineDrillData data)
		{
			if (base.Add(data))
			{
				if (!dicGroup.ContainsKey(data.LEVEL))
					dicGroup.Add(data.LEVEL, data);
				else
					dicGroup[data.LEVEL] = data;
				return true;
			}
			return false;
		}
		public MineDrillData GetMineDrillDataByLevel(int _level)
		{
			if (dicGroup.ContainsKey(_level))
			{
				return dicGroup[_level];
			}
			else
				return null;
		}
		public int GetMineDrillMaxLevel()
		{
			var keys = dicGroup.Keys.ToList();
			if (keys == null || keys.Count <= 0)
				return 0;

			keys.Sort();
			return keys[keys.Count - 1];
		}

	}
}

