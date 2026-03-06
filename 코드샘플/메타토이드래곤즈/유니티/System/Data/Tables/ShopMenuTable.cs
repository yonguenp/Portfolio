using EasyMobile;
using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopMenuTable : TableBase<ShopMenuData, DBShop_menu>
{

}

public class ShopGoodsTable : TableBase<ShopGoodsData, DBShop_goods>
{
	Dictionary<int, List<ShopGoodsData>> dicMenu = new Dictionary<int, List<ShopGoodsData>>();
    public override void DataClear()
    {
        base.DataClear();
		dicMenu.Clear();
	}
    public override void Preload()
    {
		base.Preload();

		LoadAll();

		foreach (var data in datas.Values)
		{
			data.Init();

			if (!dicMenu.ContainsKey(data.MENU))
				dicMenu.Add(data.MENU, new List<ShopGoodsData>());

			dicMenu[data.MENU].Add(data);
		}
	}

	public List<ShopGoodsData> GetByMenuID(int id)
	{
		if (dicMenu.ContainsKey(id))
			return dicMenu[id];

		return new List<ShopGoodsData>();
	}

	public int GetKeyBySpecificAsset(eGoodType type, int itemNo)
	{
		List<int> checkedRewards = new List<int> ();
        foreach (var dat in datas.Values)
        {
			if (checkedRewards.Contains(dat.REWARD_ID))
				continue;
            foreach(var item in dat.REWARDS)
			{
				if (item.GoodType == type && item.ItemNo == itemNo)
					return int.Parse(dat.KEY);
			}
			checkedRewards.Add(dat.REWARD_ID);
        }

        return -1;
	}
}

public class ShopSubscriptionTable : TableBase<ShopSubscriptionData, DBSubscription_item>
{
	Dictionary<int, ShopSubscriptionData> dicGroup = new Dictionary<int, ShopSubscriptionData>();
	public override void DataClear()
	{
		base.DataClear();
		dicGroup.Clear();
	}
    public override void Preload()
    {
		base.Preload();

		LoadAll();

		foreach (var data in datas.Values)
		{
			dicGroup[data.GROUP_ID] = data;
        }
	}

	public ShopSubscriptionData GetSubscription(int group)
    {
		if (dicGroup.ContainsKey(group))
		{
			return dicGroup[group];
		}

		return null;
	}
}

public class PostRewardTable : TableBase<PostRewardData, DBPost_reward>
{
	Dictionary<int, List<PostRewardData>> dicGroup = new Dictionary<int, List<PostRewardData>>();
	public override void DataClear()
	{
		base.DataClear();
		dicGroup.Clear();
	}
    public override void Preload()
    {
        base.Preload();
		LoadAll();

		foreach(var data in datas.Values)
        {
			if (!dicGroup.ContainsKey(data.GROUP_ID))
				dicGroup[data.GROUP_ID] = new List<PostRewardData>();

			dicGroup[data.GROUP_ID].Add(data);
			data.Init();
        }

		var itr = dicGroup.GetEnumerator();
		while( itr.MoveNext() )
		{
			if (itr.Current.Value == null)
				continue;
			itr.Current.Value.Sort(SortOrderList);
        }
	}

	int SortOrderList(PostRewardData a, PostRewardData b)
	{
		if (a.Order < b.Order)
			return -1;
		else if (a.Order == b.Order)
			return 0;
		else
			return 1;
	}

    public List<PostRewardData> GetGroup(int group_id)
    {
		if (dicGroup.ContainsKey(group_id))
			return dicGroup[group_id];

		return null;
	}
}

public class PersonalGoodsTable : TableBase<PersonalGoodsData, DBPersonal_goods>
{

}

public class ShopBannerTable : TableBase<ShopBannerData, DBShop_banner>
{
    public override void Preload()
    {
        base.Preload();
		LoadAll();

		foreach(var data in datas.Values)
        {
			data.Init();
        }
    }
	public List<ShopBannerData> GetByType(BANNER_TYPE type)
	{
		List<ShopBannerData> list = new List<ShopBannerData>();
		foreach(var dat in datas.Values)
		{
			if(dat.TYPE == type)
			{
				list.Add(dat);
			}
		}
		return list;
	}
}

public class ShopSKUTable : TableBase<ShopSKUData, DBShop_sku>
{
    public override void Preload()
    {
        base.Preload();
		LoadAll();

		bool changed = false;
		foreach (var data in datas.Values)
        {
			data.Init();

			IAPProduct.StoreSpecificId[] ids = new IAPProduct.StoreSpecificId[3];
			ids[0] = new IAPProduct.StoreSpecificId(IAPStore.GooglePlay, data.SKU);
			ids[1] = new IAPProduct.StoreSpecificId(IAPStore.AppleAppStore, data.SKU);
			ids[2] = new IAPProduct.StoreSpecificId(IAPStore.OneStore, data.SKU);

			IAPProduct[] products_array = EM_Settings.InAppPurchasing.Products;
			List<IAPProduct> products = new List<IAPProduct>(products_array);
			
			bool contains = false;
			foreach (IAPProduct pro in products)
			{
				if (pro.Id == data.SKU)
				{
					contains = true;
					break;
				}
			}

			if (!contains)
			{
				if (data.ShopGoods == null)
				{
					Debug.LogError("뭔가 알수없는 패키지");
					continue;
				}

				changed = true;
				products.Add(new IAPProduct(data.SKU, ids, data.SKU, IAPProductType.Consumable, data.ShopGoods.PRICE.Amount.ToString() + "KRW"));
				EM_Settings.InAppPurchasing.Products = products.ToArray();
			}
		}


#if ONESTORE
		OneStoreManager.InitializeIAP();
#else
		if (changed)
		{
			InAppPurchasing.InitializePurchasing(true);
		}
#endif
	}
}

public class AdvertisementTable : TableBase<AdvertisementData, DBAdv_limit>
{ 

}

public class ShopRandomTable : TableBase<ShopRandomData, DBShop_random>
{
   
}

public class EventBannerTable : TableBase<EventBannerData, DBEvent_banner>
{
	public override void Preload()
	{
		base.Preload();
		LoadAll();

		foreach (var data in datas.Values)
		{
			data.Init();
		}
	}
}

public class EventScheduleTable : TableBase<EventScheduleData, DBEvent_schedule>
{
	public override void Preload()
	{
		base.Preload();
		LoadAll();

		foreach (var data in datas.Values)
		{
			data.Init();
		}
	}
}

public class EventAttendanceResourceTable : TableBase<EventAttendanceResourceData, DBEvent_attendance_resource>
{
	public override void Preload()
	{
		base.Preload();
		LoadAll();

		foreach (var data in datas.Values)
		{
			data.Init();
		}
	}
}
