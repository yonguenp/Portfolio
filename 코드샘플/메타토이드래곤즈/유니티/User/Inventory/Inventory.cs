using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class Inventory
    {
        public int InvenStep { get; private set; } = 0;
        private Dictionary<int, InventoryItem> items = new Dictionary<int, InventoryItem>();
        private List<InventoryItem> GetAllItem()
        {
            return items.Values.ToList();
        }
        /// <summary> 인벤토리에서 보여줄 ItemData </summary>
        public List<ITableData> GetViewItems(INVENSORT sort = INVENSORT.ALL)
        {
            List<ITableData> viewItems = new();

            var it = items.GetEnumerator();
            while (it.MoveNext())
            {
                var baseData = ItemBaseData.Get(it.Current.Key);
                if (baseData == null || baseData.SLOT_USE == "NO")
                    continue;

                if (false == sort.HasFlag((INVENSORT)(1 << (int)baseData.KIND)))
                    continue;

                var remainCount = it.Current.Value.Amount;
                while (remainCount != 0)
                {
                    var current = new InventoryItem();

                    if (remainCount > baseData.MERGE)
                    {
                        current.SetData(it.Current.Key, baseData.MERGE);
                        remainCount -= baseData.MERGE;
                    }
                    else
                    {
                        current.SetData(it.Current.Key, remainCount);
                        remainCount = 0;
                    }

                    viewItems.Add(current);
                }
            }

            return viewItems;
        }
        /// <summary> 해당 ItemKind의 아이템 리스트 반환 </summary>
        public List<InventoryItem> GetKindItems(eItemKind kind)
        {
            return GetAllItem().FindAll(x => x.BaseData.KIND == kind);
        }
        public void Clear()
        {
            InvenStep = 0;
            items.Clear();
        }
        public void UpdateItem(Item item)
        {
            UpdateItem(item.ItemNo, item.Amount);
        }

        public void UpdateItem(int no, int amount)
        {
            if (false == items.ContainsKey(no))
                items.Add(no, new InventoryItem(no, amount));
            else
                items[no].SetData(no, amount);
        }

        public InventoryItem GetItem(int no)
        {
            if (items.ContainsKey(no))
                return items[no];
            else
                return new InventoryItem(no, 0);
        }

        /// <summary> 해당 아이템 리스트를 획득 할 수 있는가 </summary>
        /// <returns> true 획득 가능, false 획득 불가능 </returns>
        public bool CanItems(List<Asset> itemArr)
        {
            int addedSlot = 0;
            Dictionary<int, int> modCount = new();
            itemArr.ForEach(targetItem =>
            {
                var baseData = ItemBaseData.Get(targetItem.ItemNo);
                if (baseData == null || baseData.SLOT_USE == "NO")
                    return;

                if (false == modCount.TryGetValue(targetItem.ItemNo, out int curCount))
                {
                    if (items.TryGetValue(targetItem.ItemNo, out var item))
                        curCount = item.Amount % baseData.MERGE;
                    else
                        curCount = 0;

                    modCount.Add(targetItem.ItemNo, curCount);
                }

                curCount += targetItem.Amount;
                addedSlot += curCount / baseData.MERGE;
                curCount = curCount % baseData.MERGE;

                modCount[targetItem.ItemNo] = curCount;
            });

            return addedSlot == 0 || GetEmptySlotCount() >= addedSlot;
        }
        /// <summary> 인벤토리의 총 슬롯 수량 </summary>
        /// <returns> 현재 Step의 슬롯 수량 </returns>
        public int GetSlot()
        {
            return InventoryData.Get(InvenStep.ToString()).SLOT;
        }
        public void SetStep(int step)
        {
            InvenStep = step;
        }
        /// <summary> 남은 아이템 인벤토리 슬롯 갯수 반환 </summary>
        public int GetEmptySlotCount()
        {
            var invenCount = GetSlot();
            var it = items.GetEnumerator();
            while (it.MoveNext())
            {
                ItemBaseData itemInfo = ItemBaseData.Get(it.Current.Key);
                if (itemInfo == null || itemInfo.SLOT_USE == "NO") { continue; }

                invenCount -= it.Current.Value.Amount / itemInfo.MERGE;
                invenCount -= ((it.Current.Value.Amount % itemInfo.MERGE) > 0 ? 1 : 0);
            }

            return invenCount;
        }
    }
}