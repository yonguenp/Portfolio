using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SandboxNetwork
{
    public class ExchangeData : TableData<DBExchange_base>
    {
        public int KEY => Int(Data.UNIQUE_KEY);
        public int TYPE => Data.TYPE;
        public int NEED_PRODUCT_COUNT => Data.NEED_PRODUCT_COUNT;
        
        private int need_item => Data.NEED_ITEM;
        private ItemBaseData needItemData = null;
        public int MIN_NUM => Data.MIN_NUM;
        public int MAX_NUM => Data.MAX_NUM;
        //public int BONUS_COUNT => Data.BONUS_COUNT;
        public ItemBaseData NEED_ITEM 
        { 
            get {
                if (needItemData == null)
                {
                    needItemData = ItemBaseData.Get(need_item);
                }

                return needItemData;
            }
        }
	}
    public class ExchangeGroupData : TableData<DBExchange_group>
    {
        public string KEY => Data.UNIQUE_KEY;
        public int GROUP => Data.GROUP;
        public int REQUIRED_NUMBER => Data.REQUIRED_NUMBER;
        public int REWORD_GOLD => Data.REWORD_GOLD;
        public int REWORD_EXP => Data.REWORD_EXP;
        public int RATE => Data.RATE;
    }
}