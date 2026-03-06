using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DiceBoardTable : TableBase<DiceBoardData, DBDice_board>
    {
        Dictionary<int, List<DiceBoardData>> boards = new Dictionary<int, List<DiceBoardData>>();
        public override void DataClear()
        {
            base.DataClear();
            boards.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();

            foreach (var board in boards)
            {
                board.Value.Sort((x, y) => { return x.BOARD_ID.CompareTo(y.BOARD_ID); });
            }
        }
        protected override bool Add(DiceBoardData data)
        {
            if (base.Add(data))
            {
                if (data.EVENT_ID > 0)
                {
                    if (!boards.ContainsKey(data.EVENT_ID))
                        boards[data.EVENT_ID] = new List<DiceBoardData>();

                    boards[data.EVENT_ID].Add(data);
                }
                return true;
            }
            return false;
        }

        public List<DiceBoardData> GetBoard(int event_id)
        {
            if (boards.ContainsKey(event_id))
                return boards[event_id];

            return new List<DiceBoardData>();
        }
    }

    public class DiceBoardData : TableData<DBDice_board>
    {
        static private DiceBoardTable table = null;
        static public List<DiceBoardData> GetBoards(int event_id)
        {
            if (table == null)
                table = TableManager.GetTable<DiceBoardTable>();

            return table.GetBoard(event_id);
        }

        public int EVENT_ID => Data.EVENT_ID;
        public int BOARD_ID => Data.BOARD_ID;
        public int REWARD_ID => Data.REWARD_ID;
        public int BOARD_TYPE => Data.BOARD_TYPE;
        public int RARITY => Data.RARITY;
        public int RATE => Data.RATE;
    }
}
