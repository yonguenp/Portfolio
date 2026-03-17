using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ProducesBuilding : ITableData
    {
        public int Tag { get; private set; } = -1;
        public int Slot { get; private set; } = -1;
        public List<ProducesRecipe> Items { get; private set; } = null;

        private BuildingOpenData openData = null;
        public BuildingOpenData OpenData { 
            get { 
                if(openData == null)
                {
                    openData = BuildingOpenData.GetWithTag(Tag);
                }

                return openData; 
            } 
        }

        public ProducesBuilding(int tag, int slot, List<ProducesRecipe> items)
        {
            SetData(tag, slot, items);
        }

        public void SetData(int tag, int slot, List<ProducesRecipe> items)
        {
            Tag = tag;
            Slot = slot;
            Items = items;
        }

        public void Init()
        {
        }

        // 해당 생산건물에서 실제 생산가능한 슬롯 갯수를 반환
        public int GetRemainProductQueueSize()
        {
            int result = 0;

            BuildingOpenData openData = BuildingOpenData.GetWithTag(Tag);

            if (openData != null)
            {
                int totalSlotCount = Slot;
                int produceItemCount = 0;

                int localTime = 0;
                for (int i = 0; i < Items.Count; ++i)
                {
                    ProductData itemInfo = ProductData.GetProductDataByGroupAndKey(openData.BUILDING, Items[i].RecipeID);

                    switch (Items[i].State)
                    {
                        case eProducesState.Idle:
                            localTime += itemInfo.PRODUCT_TIME;
                            break;
                        case eProducesState.Ing:
                            localTime = Items[i].ProductionExp;
                            break;
                        default:
                            break;
                    }

                    if (localTime > TimeManager.GetTime())
                    {
                        produceItemCount++;
                    }
                }

                return totalSlotCount - produceItemCount;
            }

            return result;
        }

        public string GetKey()
        {
            return Tag.ToString();
        }
    }
}