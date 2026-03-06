using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class ProductTable : TableBase<ProductData, DBBuilding_product>
    {
        Dictionary<int, string> itemBuildingDic = null;
        Dictionary<string, Dictionary<int, ProductData>> grouplevelDic = null;
        Dictionary<string, Dictionary<int, ProductData>> groupkeyDic = null;
        public override void Init()
        {
            base.Init();
            grouplevelDic = new();
            itemBuildingDic = new();
            groupkeyDic = new();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (grouplevelDic == null)
                grouplevelDic = new();
            else
                grouplevelDic.Clear();
            if (itemBuildingDic == null)
                itemBuildingDic = new();
            else
                itemBuildingDic.Clear();
            if (groupkeyDic == null)
                groupkeyDic = new();
            else
                groupkeyDic.Clear();
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        protected override bool Add(ProductData data)
        {
            if (base.Add(data))
            {
                if (!grouplevelDic.ContainsKey(data.BUILDING_GROUP))
                    grouplevelDic.Add(data.BUILDING_GROUP, new());

                grouplevelDic[data.BUILDING_GROUP][data.BUILDING_LEVEL] = data;

                if (!groupkeyDic.ContainsKey(data.BUILDING_GROUP))
                    groupkeyDic.Add(data.BUILDING_GROUP, new());

                groupkeyDic[data.BUILDING_GROUP][data.KEY] = data;

                if (!itemBuildingDic.ContainsKey(data.ProductItemNo))
                    itemBuildingDic.Add(data.ProductItemNo, data.BUILDING_GROUP);
                return true;
            }

            return false;
        }

        public string GetBuildingGroupByProductItem(int productItem)
        {
            if (itemBuildingDic == null || itemBuildingDic.Count <= 0)
                return "";

            if (!itemBuildingDic.ContainsKey(productItem))
                return "";

            return itemBuildingDic[productItem];
        }
        public List<ProductData> GetProductListByGroup(string group)
        {
            if (groupkeyDic == null || !groupkeyDic.ContainsKey(group))
                return null;

            if (groupkeyDic[group] == null)
                return null;

            return groupkeyDic[group].Values.ToList();
        }
        public ProductData GetProductDataByGroupAndKey(string group, int key)
        {
            if (groupkeyDic == null || !groupkeyDic.ContainsKey(group) || !groupkeyDic[group].ContainsKey(key))
                return null;

            if (groupkeyDic[group] == null || groupkeyDic[group][key] == null)
                return null;

            return groupkeyDic[group][key];
        }

        public ProductData GetProductDataByGroupAndLevel(string group, int level)
        {
            if (grouplevelDic == null || !grouplevelDic.ContainsKey(group) || !grouplevelDic[group].ContainsKey(level))
                return null;

            if (grouplevelDic[group] == null || grouplevelDic[group][level] == null)
                return null;

            return grouplevelDic[group][level];
        }

        // 생산관리 한번에 채우기 관련 데이터 반환 통합 함수 (추후 추가되는 옵션에 따라 수정 필요)
        public ProductData GetProductDataForProduceOption(eProduceOptionFilter option, string group, int level)
        {
            if (grouplevelDic == null || !grouplevelDic.ContainsKey(group) || !grouplevelDic[group].ContainsKey(level))
                return null;

            if (grouplevelDic[group] == null || grouplevelDic[group][level] == null)
                return null;

            ProductData resultData = null;

            var groupList = grouplevelDic[group].Values.ToList();
            groupList = groupList.FindAll(product => product.BUILDING_LEVEL <= level);
            
            switch (option)
            {
                case eProduceOptionFilter.PRODUCT_TIME_SHORT:
                    int minValue = groupList.Min(product => product.PRODUCT_TIME);
                    resultData = groupList.Find(product => product.PRODUCT_TIME == minValue);
                    break;
                case eProduceOptionFilter.PRODUCT_TIME_LONG:
                    int maxValue = groupList.Max(product => product.PRODUCT_TIME);
                    resultData = groupList.Find(product => product.PRODUCT_TIME == maxValue);
                    break;
            }

            return resultData;
        }

        public List<ProductData> GetProductDataForProduceListByOption(eProduceOptionFilter option, string group, int level)
        {
            if (grouplevelDic == null || !grouplevelDic.ContainsKey(group) || !grouplevelDic[group].ContainsKey(level))
                return null;

            if (grouplevelDic[group] == null || grouplevelDic[group][level] == null)
                return null;

            ProductData resultData = null;

            var groupList = grouplevelDic[group].Values.ToList();
            groupList = groupList.FindAll(product => product.BUILDING_LEVEL <= level);

            switch (option)
            {
                case eProduceOptionFilter.PRODUCT_TIME_LONG:
                    groupList.Reverse();
                    break;
            }

            return groupList;
        }

        // 빌딩 Key로 해당 건물의 Product / Auto Product 타입 판별용
        public bool IsProductBuilding(string group)
        {
            return groupkeyDic != null && groupkeyDic.ContainsKey(group);
        }

        public List<ProductData> GetProductDatasByItemNo(int itemNo)
        {
            string key = GetBuildingGroupByProductItem(itemNo);
            List < ProductData > data = new List < ProductData >();
            foreach (var building in groupkeyDic[key].Values)
            {
                if(building.ProductItemNo == itemNo)
                    data.Add(building);
            }
            return data;
        }
    }

    public class ProductAutoTable : TableBase<ProductAutoData, DBBuilding_product_auto>
    {
        Dictionary<string, Dictionary<int, List<ProductAutoData>>> grouplevelDic = null;
        public override void Init()
        {
            base.Init();
            grouplevelDic = new();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (grouplevelDic == null)
                grouplevelDic = new();
            else
                grouplevelDic.Clear();
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        protected override bool Add(ProductAutoData data)
        {
            if(base.Add(data))
            {
                if (!grouplevelDic.ContainsKey(data.BUILDING_GROUP))
                    grouplevelDic.Add(data.BUILDING_GROUP, new());

                if (!grouplevelDic[data.BUILDING_GROUP].ContainsKey(data.LEVEL))
                    grouplevelDic[data.BUILDING_GROUP].Add(data.LEVEL, new());

                grouplevelDic[data.BUILDING_GROUP][data.LEVEL].Add(data);

                return true;
            }

            return false;
        }

		public List<ProductAutoData> GetListByGroupAndLevel(string group, int level)
		{
			if (grouplevelDic == null || !grouplevelDic.ContainsKey(group) || !grouplevelDic[group].ContainsKey(level))
				return null;

            var groups = grouplevelDic[group][level];
            if (groups == null)
                return null;

            return grouplevelDic[group][level];
        }
    }
}