using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SandboxNetwork
{
    public enum eProductType
    {
        NONE = -1,

        NORMAL = 0,
        AUTO = 1,

        TYPE_MAX
    }
    public interface IProductFormData
    {
        public eProductType Type { get; }
        public int KEY { get; }
        public string BUILDING_GROUP { get; }
        public Item ProductItem { get; }
    }

    public class ProductData : TableData<DBBuilding_product>, IProductFormData
    {
        static private ProductTable table = null;
        static public ProductData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<ProductTable>();

            return table.Get(key);
        }
        static public List<ProductData> GetProductListByGroup(string group)
        {
            if (table == null)
                table = TableManager.GetTable<ProductTable>();

            return table.GetProductListByGroup(group);
        }
        
        static public ProductData GetProductDataByGroupAndLevel(string group, int level)
        {
            if (table == null)
                table = TableManager.GetTable<ProductTable>();

            return table.GetProductDataByGroupAndLevel(group, level);
        }

        static public ProductData GetProductDataByGroupAndKey(string group, int key)
        {
            if (table == null)
                table = TableManager.GetTable<ProductTable>();

            return table.GetProductDataByGroupAndKey(group, key);
        }


        static public string GetBuildingGroupByProductItem(int productItem)
        {
            if (table == null)
                table = TableManager.GetTable<ProductTable>();

            return table.GetBuildingGroupByProductItem(productItem);
        }

        static public ProductData GetProductDataForProduceByOption(eProduceOptionFilter option, string group, int level)
        {
            if (table == null)
                table = TableManager.GetTable<ProductTable>();

            return table.GetProductDataForProduceOption(option, group, level);
        }

        static public List<ProductData> GetProductDataForProduceListByOption(eProduceOptionFilter option, string group, int level)
        {
            if (table == null)
                table = TableManager.GetTable<ProductTable>();

            return table.GetProductDataForProduceListByOption(option, group, level);
        }

        static public bool IsProductBuilding(string group)
        {
            if (table == null)
                table = TableManager.GetTable<ProductTable>();

            return table.IsProductBuilding(group);
        }

        static public List<ProductData> GetProductDatasByItemNo(int itemNo)
        {
            if (table == null)
                table = TableManager.GetTable<ProductTable>();
            return table.GetProductDatasByItemNo(itemNo);
        }
        public eProductType Type { get; protected set; } = eProductType.NONE;
        public int KEY { get { return Int(UNIQUE_KEY); } }
        public string BUILDING_GROUP => Data.BUILDING_GROUP;

        protected Item productItem = null;
        protected int PRODUCT_ITEM_NO => Data.PRODUCT_ITEM;
        protected int PRODUCT_ITEM_AMOUNT => Data.PRODUCT_NUM;
        public Item ProductItem
        {
            get
            {
                if (productItem == null)
                    productItem = new Item(PRODUCT_ITEM_NO, PRODUCT_ITEM_AMOUNT);

                return productItem;
            }
        }
        public int BUILDING_LEVEL => Data.BUILDING_LEVEL;
        public string ICON => Data.ICON;
        private Sprite sprite = null;
        public Sprite ICON_SPRITE
        {
            get
            {
                if (sprite == null)
                    sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, ICON);
                return sprite;
            }
        }
        public int PRODUCT_TIME => Data.PRODUCT_TIME;
        public int needitemLength { get; private set; } = -1;
        public List<Asset> NEED_ITEM { get; private set; } = new List<Asset>();
        public int NEED_GOLD => Data.NEED_GOLD;


        public int ProductItemNo { get { return PRODUCT_ITEM_NO; } }
        
        public ProductData()
        {
            Type = eProductType.NORMAL;
        }

        public override void SetData(DBBuilding_product _data)
        {
            NEED_ITEM.Clear();
            base.SetData(_data);
            needitemLength = 0;
            if (Data.NEED_ITEM_1 > 0 && Data.NEED_ITEM_1_NUM > 0)
            {
                NEED_ITEM.Add(new Asset(Data.NEED_ITEM_1, Data.NEED_ITEM_1_NUM));
                needitemLength++;
            }
            if (Data.NEED_ITEM_2 > 0 && Data.NEED_ITEM_2_NUM > 0)
            {
                NEED_ITEM.Add(new Asset(Data.NEED_ITEM_2, Data.NEED_ITEM_2_NUM));
                needitemLength++;
            }
            if (Data.NEED_ITEM_3 > 0 && Data.NEED_ITEM_3_NUM > 0)
            {
                NEED_ITEM.Add(new Asset(Data.NEED_ITEM_3, Data.NEED_ITEM_3_NUM));
                needitemLength++;
            }
        }

        public bool CheckRecipeNeedItem()
        {
            int sufficientCount = 0;
            if (needitemLength > 0) // 재료 아이템이 있는 경우
            {
                foreach (Asset needItem in NEED_ITEM)
                {
                    if (User.Instance.GetItemCount(needItem.ItemNo) >= needItem.Amount)
                    {
                        sufficientCount++;
                    }
                }
            }

            return sufficientCount == needitemLength;
        }
    }
    public class ProductAutoData : TableData<DBBuilding_product_auto>, IProductFormData
    {
        static private ProductAutoTable table = null;
        static public List<ProductAutoData> GetListByGroupAndLevel(string group, int level)
        {
            if (table == null)
                table = TableManager.GetTable<ProductAutoTable>();

            return table.GetListByGroupAndLevel(group, level);
        }

        static public ProductAutoData GetProductDataByGropuAndLevel(string group, int level)
        {
            var list = GetListByGroupAndLevel(group, level);
            if(list != null)
            {
                return list[0];
            }

            return null;
        }
        public eProductType Type { get; protected set; } = eProductType.NONE;
        public int KEY { get { return Int(UNIQUE_KEY); } }
        public string BUILDING_GROUP => Data.BUILDING_GROUP;

        protected Item productItem = null;
        protected int PRODUCT_ITEM_NO => Data.VALUE;
        protected int PRODUCT_ITEM_AMOUNT => Data.NUM;
        public Item ProductItem
        {
            get
            {
                if (productItem == null)
                    productItem = new Item(PRODUCT_ITEM_NO, PRODUCT_ITEM_AMOUNT);

                return productItem;
            }
        }
        public int LEVEL => Data.LEVEL;
        public string TYPE => Data.TYPE;

        public int TERM => Data.TERM;
        public int MAX_TIME => Data.MAX_TIME;

        public ProductAutoData()
        {
            Type = eProductType.AUTO;
        }
    }
}