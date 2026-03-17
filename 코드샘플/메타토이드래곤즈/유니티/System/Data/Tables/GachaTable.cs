using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SandboxNetwork
{
	//미사용으로 보임
	public class GachaShopTable: TableBase<GachaShopData, DBGacha_shop>
	{
	}
	//미사용으로 보임
	public class GachaListTable : TableBase<GachaListData, DBGacha_list>
	{
		private Dictionary<int, List<GachaListData>> groupDic = null;
		public override void Init()
		{
			base.Init();
			groupDic = new();
		}
		public override void Preload()
		{
			base.Preload();
			LoadAll();
		}
		protected override bool Add(GachaListData data)
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
        public override void DataClear()
		{
			base.DataClear();
			if (groupDic == null)
				groupDic = new();
			else
				groupDic.Clear();
		}
		public List<GachaListData> GetByGroup(int group)
		{
			if (groupDic == null || !groupDic.ContainsKey(group))
				return null;

			return groupDic[group];
        }
    }

    public class GachaGroup : TableBase<GachaGroupData, DBGacha_group>
    {
		public override void Preload()
		{
			base.Preload();
			LoadAll();
		}
	}
	public class GachaMenu : TableBase<GachaMenuData, DBGacha_menu>
	{
		public override void Preload()
		{
			base.Preload();
			LoadAll();
		}
	}
	public class GachaType : TableBase<GachaTypeData, DBGacha_type>
	{
		public override void Preload()
		{
			base.Preload();
			LoadAll();
		}

		public List<GachaTypeData> GetByPriceItem(int itemNo)
        {
			List<GachaTypeData> ret = new List<GachaTypeData>();
			foreach(var data in datas.Values)
            {
				if (data.price_type == eGoodType.ITEM)
                {
					if(data.price_uid == itemNo)
                    {
						ret.Add(data);
                    }
                }
            }

			return ret;
        }
	}
	public class GachaRate : TableBase<GachaRateData, DBGacha_rate>
	{
		private Dictionary<int, List<GachaRateData>> dicGroup = new Dictionary<int, List<GachaRateData>>();
        public override void Init()
        {
            base.Init();
			dicGroup.Clear();
		}
        public override void DataClear()
        {
            base.DataClear();
			dicGroup.Clear();
		}
		public override void Preload()
		{
			base.Preload();
			LoadAll();

            SetGroup();
		}
		protected override bool Add(GachaRateData data)
		{
			if (base.Add(data))
			{
				if (!dicGroup.ContainsKey(data.group_id))
				{
					var list = new List<GachaRateData>();
					dicGroup.Add(data.group_id, list);
				}

				dicGroup[data.group_id].Add(data);
				return true;
			}
			return false;
		}

		void SetGroup()
        {
			foreach(var data in datas.Values)
            {
				if(data.result_type > 0)
                {
					if(ContainsKey(data.result_type))
					{
						var parent = Get(data.result_type);
						if (parent != null)
						{
							parent.AddChild(data);
						}
					}
                }
			}
        }

		public List<GachaRateData> GetGroup(int group)
		{
			if (dicGroup.ContainsKey(group))
				return dicGroup[group];

			return null;
		}
	}

	public class RateTableUrlTable : TableBase<RateTableUrlData, DBRate_table_url>
	{
		
	}
}