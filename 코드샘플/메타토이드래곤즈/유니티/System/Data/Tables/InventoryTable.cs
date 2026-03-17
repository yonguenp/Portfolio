using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class InventoryTable : TableBase<InventoryData, DBInventory>
    {
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
    }

    public class SlotCostTable : TableBase<SlotCostData, DBSlot_cost>
    {
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }

        public SlotCostData GetByType(eSlotCostInfoType type, int slotCount)
        {
            SlotCostData ret = null;
            foreach (var dat in datas.Values)
            {
                if(dat.SLOT_COST_TYPE == type && dat.BUY_SLOT_COUNT == slotCount)
                {
                    ret = dat;
                    break;
                }
            }
            return ret;
        }
    }
}