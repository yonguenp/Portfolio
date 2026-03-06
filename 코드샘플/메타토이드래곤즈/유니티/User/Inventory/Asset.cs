using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class Asset : Item, ITableData
    {
        // 리워드 아이템 타입
        public eGoodType GoodType { get; private set; } = eGoodType.NONE;
        public Sprite ICON
        {
            get
            {
                return SBFunc.GetGoodTypeIcon(GoodType, ItemNo);    
            }
        }
        public Asset(eGoodType type, int id, int count)
        {
            SetData(type, id, count);
        }

        public Asset(Item item)
        {
            SetData(eGoodType.ITEM, item.ItemNo, item.Amount);
        }
        public Asset(int no)
        {
            SetData(eGoodType.ITEM, no, 1);
        }
        public Asset(int no, int count)
        {
            SetData(eGoodType.ITEM, no, count);
        }
        public Asset(int no, int count, int type)
        {
            SetData((eGoodType)type, no, count);
        }
        
        public Asset(JToken data)
        {
            SetData((eGoodType)data[0].Value<int>(), data[1].Value<int>(), data[2].Value<int>());
        }

        public void SetData(eGoodType type, int id, int count)
        {
            SetData(id, count);
            GoodType = type;
        }

        public void Init() { }
        public string GetKey() { return ItemNo.ToString(); }

        public string GetName()
        {
            switch (GoodType)
            {
                case eGoodType.ITEM:
                case eGoodType.GEMSTONE:
                    return ItemBaseData.Get(ItemNo).NAME;
                case eGoodType.ARENA_TICKET:
                    return ItemBaseData.Get(10000007).NAME;
                case eGoodType.ENERGY:
                    return ItemBaseData.Get(10000002).NAME;
                case eGoodType.GOLD:
                    return ItemBaseData.Get(10000001).NAME;
            }
            return "";
        }

        public string GetDesc()
        {
            switch (GoodType)
            {
                case eGoodType.ITEM:
                    return ItemBaseData.Get(ItemNo).DESC;
                    break;
            }
            return "";
        }
    }

    public class ProductReward : Asset
    {
        public bool isNew { get; private set; } = false;
        public ProductReward(Item item)
            : base(item)
        {
            
        }
        public ProductReward(eGoodType type, int id, int count, bool bNew = false)
            : base(type, id, count)
        {
            isNew = bNew;
        }

        public void Checked(bool check = true)
        {
            isNew = !check;
        }
    }
}