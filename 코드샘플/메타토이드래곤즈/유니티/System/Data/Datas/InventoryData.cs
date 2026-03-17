using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class InventoryData : TableData<DBInventory>
    {
        static private InventoryTable table = null;
        static public InventoryData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<InventoryTable>();

            return table.Get(key);
        }
        public int KEY { get { return Int(UNIQUE_KEY); } }
        public int STEP => Data.STEP;
        public int SLOT => Data.SLOT;
        public List<Asset> NEED_ITEM { get; private set; } = new List<Asset>();
        public string COST_TYPE => Data.COST_TYPE;
        public int COST_NUM => Data.COST_NUM;
        public override void SetData(DBInventory _data)
        {
            Init();
            base.SetData(_data);
            if (Data.NEED_ITEM_1 > 0 && Data.NEED_ITEM_1_NUM > 0)
            {
                NEED_ITEM.Add(new Asset(Data.NEED_ITEM_1, Data.NEED_ITEM_1_NUM));
            }
            if (Data.NEED_ITEM_2 > 0 && Data.NEED_ITEM_2_NUM > 0)
            {
                NEED_ITEM.Add(new Asset(Data.NEED_ITEM_2, Data.NEED_ITEM_2_NUM));
            }
            if (Data.NEED_ITEM_3 > 0 && Data.NEED_ITEM_3_NUM > 0)
            {
                NEED_ITEM.Add(new Asset(Data.NEED_ITEM_3, Data.NEED_ITEM_3_NUM));
            }
        }
    }

    public class SlotCostData : TableData<DBSlot_cost>
    {
        static private SlotCostTable table = null;
        static public SlotCostData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<SlotCostTable>();

            return table.Get(key);
        }

        public string KEY { get { return UNIQUE_KEY; } }
        public int BUY_SLOT_COUNT => Data.BUY_SLOT_COUNT;
        public string COST_TYPE => Data.COST_TYPE;
        public int COST_NUM => Data.COST_NUM;

        public eSlotCostInfoType SLOT_COST_TYPE => (eSlotCostInfoType)Data.TYPE;

        public static SlotCostData GetByType(eSlotCostInfoType type, int buySlotCount)
        {
            if (table == null)
                table = TableManager.GetTable<SlotCostTable>();
            return table.GetByType(type, buySlotCount);
        }
    }
}