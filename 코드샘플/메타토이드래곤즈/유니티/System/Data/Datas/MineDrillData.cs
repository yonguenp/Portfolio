using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class MineDrillData : TableData<DBMine_drill>
    {
        static private MineDrillTable table = null;
        static public MineDrillData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<MineDrillTable>();

            return table.Get(key);
        }
        static public MineDrillData GetMineDrillDataByLevel(int _level)
        {
            if (table == null)
                table = TableManager.GetTable<MineDrillTable>();

            return table.GetMineDrillDataByLevel(_level);
        }
        static public int GetMineDrillMaxLevel()
        {
            if (table == null)
                table = TableManager.GetTable<MineDrillTable>();

            return table.GetMineDrillMaxLevel();
        }

        public int KEY { get { return Int(UNIQUE_KEY); } }
        public int LEVEL => Data.LEVEL;
        public int MINE_DURABILITY => Data.MINE_DURABILITY;//드릴 내구도
        public string REPAIR_COST_TYPE => Data.REPAIR_COST_TYPE;//수리 타입
        public int REPAIR_COST_VALUE => Data.REPAIR_COST_VALUE;//수리용도로 쓸 아이템 넘버
        public int REPAIR_COST_NUM => Data.REPAIR_COST_NUM;//수리용도 사용 아이템 갯수
        public Asset REPAIR_COST_ITEM { get; private set; } = null;//수리 용도로 사용할 아이템 데이터

        public override void SetData(DBMine_drill _data)
        {
            base.SetData(_data);

            //각 레벨별 수리 아이템 데이터 세팅
            REPAIR_COST_ITEM = new Asset(SBFunc.ConvertStringToItemType(REPAIR_COST_TYPE.ToUpper()), REPAIR_COST_VALUE, REPAIR_COST_NUM);
        }
    }
}

