using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class EventRewardTable : TableBase<EventRewardData, DBEvent_reward>
    {
        private Dictionary<int, List<EventRewardData>> dic = null;//key : group , value list
        public override void Init()
        {
            base.Init();
            if (dic == null)
                dic = new Dictionary<int, List<EventRewardData>>();
            else
                dic.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (dic == null)
                dic = new Dictionary<int, List<EventRewardData>>();
            else
                dic.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();

            foreach (var cur in dic.Values)
            {
                cur.Sort(SortSeq);
            }
        }
        protected override bool Add(EventRewardData data)
        {
            if (base.Add(data))
            {
                if (false == dic.ContainsKey(data.GROUP_ID))
                    dic.Add(data.GROUP_ID, new());

                dic[data.GROUP_ID].Add(data);
                return true;
            }
            return false;
        }
        private int SortSeq(EventRewardData a, EventRewardData b)
        {
            if (a.SEQ > b.SEQ)
                return 1;
            else if (a.SEQ == b.SEQ)
                return 0;
            else
                return -1;
        }
        public List<EventRewardData> GetGroup(int groupID)
        {
            if (dic is null)
                return null;

            if (false == dic.ContainsKey(groupID))
                return null;

            return dic[groupID];
        }
    }
}