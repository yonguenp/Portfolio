
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
	public class ItemBaseTable: TableBase<ItemBaseData, DBItem_base>
	{
		public List<ItemBaseData> GetItemListByKind(eItemKind kind)
        {
			LoadAll();

			List<ItemBaseData> resultList = new List<ItemBaseData>();
			foreach (KeyValuePair<string, ItemBaseData> element in datas)
			{
				if (element.Value.KIND == kind)
				{
					resultList.Add(element.Value);
				}
			}

			return resultList;
		}
	}

	public class ItemGroupTable: TableBase<ItemGroupData, DBItem_group>
	{
		Dictionary<int, List<ItemGroupData>> groupDic = null;
		public override void Init()
		{
			base.Init();
			if (groupDic == null)
				groupDic = new();
			else
				groupDic.Clear();
		}
		public override void DataClear()
		{
			base.DataClear();
			if (groupDic == null)
				groupDic = new();
			else
				groupDic.Clear();
		}
		public override void Preload()
		{
			base.Preload();
			LoadAll();
		}
		protected override bool Add(ItemGroupData data)
		{
			if (base.Add(data))
			{
				if (!groupDic.ContainsKey(data.GROUP))
					groupDic.Add(data.GROUP, new());

				groupDic[data.GROUP].Add(data);
				return true;
			}
			return false;
		}
		public List<ItemGroupData> GetByGroup(int group)
		{
			if (groupDic == null || !groupDic.ContainsKey(group))
				return null;

			return groupDic[group];
        }
    }

    public class ItemGroupListTable : TableBase<ItemGroupListData, DBItem_group_list>
	{
		public override void Preload()
		{
			base.Preload();
			LoadAll();
		}
	}


    public class DefineResourceTable: TableBase<DefineResourceData, DBDefine_resource>
	{
		public override void Preload()
		{
			base.Preload();
			LoadAll();
		}
	}
}