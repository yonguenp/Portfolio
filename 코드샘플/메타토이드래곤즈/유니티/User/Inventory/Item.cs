using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class Item
    {
        public int ItemNo { get; private set; } = -1;
        public int Amount { get; private set; } = -1;
        private ItemBaseData baseData = null;
        public ItemBaseData BaseData
        {
            get
            {
                if (baseData == null)
                    baseData = ItemBaseData.Get(ItemNo);

                return baseData;
            }
        }

        public Item()
        {

        }

        public Item(int no, int count)
        {
            SetData(no, count);
        }

        public virtual void SetData(int no, int count)
        {
            ItemNo = no;
            Amount = count;
        }

        public virtual void AddCount(int count)
        {
            Amount += count;
        }
    }
}