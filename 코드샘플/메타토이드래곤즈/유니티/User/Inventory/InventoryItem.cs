using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class InventoryItem : Item, ITableData
    {
        public InventoryItem() : base() { }
        public InventoryItem(int no, int count) : base(no, count) { }

        public void Init() { }
        public string GetKey() { return ItemNo.ToString(); }
    }

    public class batteryItem : InventoryItem
    {
        public batteryItem(int _itemID, int _itemCount) : base(_itemID, _itemCount) { }

        public int GetEXP()
        {
            if (BaseData != null)
                return BaseData.VALUE;
            else
            {
                var itemData = ItemBaseData.Get(ItemNo);
                return itemData != null ? itemData.VALUE : 0;
            }
        }
    }
}