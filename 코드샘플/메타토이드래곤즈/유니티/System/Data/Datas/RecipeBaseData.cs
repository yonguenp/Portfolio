using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	public class RecipeBaseData : TableData<DBRecipe_core>
	{
		static private RecipeBaseTable table = null;
		static public RecipeBaseData Get(int key)
		{
			return Get(key.ToString());
		}
		static public RecipeBaseData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<RecipeBaseTable>();

			return table.Get(key);
		}
		static public bool IsRecipeMaterial(int _itemNo)
		{
			if (table == null)
				table = TableManager.GetTable<RecipeBaseTable>();

			var list = table.GetAllList();
			return list.FindAll(element => element.REWARD_ITEM_LIST != null && element.REWARD_ITEM_LIST.Count == 1 &&
			element.REWARD_ITEM_LIST[0].ItemNo == _itemNo).Count > 0;
		}
		static public RecipeBaseData GetRecipeData(int _itemNo)
		{
			if (table == null)
				table = TableManager.GetTable<RecipeBaseTable>();

			var list = table.GetAllList();
			var result = list.FindAll(element => element.REWARD_ITEM_LIST != null && element.REWARD_ITEM_LIST.Count == 1 &&
			element.REWARD_ITEM_LIST[0].ItemNo == _itemNo);

			if (result == null || result.Count <= 0)
				return null;

			return result[0];
		}

		public int REWARD => Data.REWARD;//item_group의 그룹값
		public int FAIL => Data.FAIL;//item_group의 그룹값
		public int RATE => Data.RATE;//확률
		public List<Asset> REWARD_ITEM_LIST { get { return GetItemList(REWARD); } }
		public List<Asset> FAIL_ITEM_LIST { get { return GetItemList(FAIL); } }
		public List<Asset> GetItemList(int index)
        {
			List<Asset> tempList = new List<Asset>();
			var list = ItemGroupData.Get(index);
			if (list == null || list.Count <= 0)
				return tempList;
			foreach (var groupData in list)
				if (groupData != null)
					tempList.Add(groupData.Reward);

			return tempList;
		}
	}

	public class RecipeMaterialData : TableData<DBRecipe_material>
	{
		static private RecipeMaterialTable table = null;
		static public RecipeMaterialData Get(int key)
		{
			return Get(key.ToString());
		}
		static public RecipeMaterialData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<RecipeMaterialTable>();

			return table.Get(key);
		}
		static public List<RecipeMaterialData> GetDataByGroup(int _group)
		{
			if (table == null)
				table = TableManager.GetTable<RecipeMaterialTable>();

			return table.GetDataByGroup(_group);
		}

		public int RECIPE_ID => Data.RECIPE_ID;//레시피 그룹ID
		public eGoodType TYPE => (eGoodType)Data.TYPE;
		public int PARAM => Data.PARAM;//itemID
		public int VALUE => Data.VALUE;//amount
		public Asset REWARD { get; private set; } = null;

		public override void SetData(DBRecipe_material _data)
		{
			base.SetData(_data);
			REWARD = new Asset(TYPE, PARAM, VALUE);
		}
	}
}
