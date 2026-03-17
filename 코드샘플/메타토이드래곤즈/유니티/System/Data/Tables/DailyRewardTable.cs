using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DailyRewardTable : TableBase<DailyRewardData, DBDaily_reward>
    {
        private Dictionary<int, List<DailyRewardData>> dic = null;
        public override void Init()
        {
            base.Init();
            if (dic == null)
                dic = new Dictionary<int, List<DailyRewardData>>();
            else
                dic.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (dic == null)
                dic = new Dictionary<int, List<DailyRewardData>>();
            else
                dic.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();

            if(dic.ContainsKey(SBDefine.DEFAULT_ATTENDANCE_REWARD_GROUP))
            {
                ServerOptionData attendance_option = ServerOptionData.Get("attendance");
                if(attendance_option != null)
                {
                    dic[SBDefine.DEFAULT_ATTENDANCE_REWARD_GROUP].Clear();

                    foreach(var prop in attendance_option.JSON_VALUE.Properties())
                    {
                        dic[SBDefine.DEFAULT_ATTENDANCE_REWARD_GROUP].Add(new ServerOptionDailyRewardData(int.Parse(prop.Name), SBDefine.DEFAULT_ATTENDANCE_REWARD_GROUP, (JObject)prop.Value));
                    }
                }
            }

            foreach (var cur in dic.Values)
            {
                cur.Sort(SortDay);
            }
        }
        protected override bool Add(DailyRewardData data)
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
        private int SortDay(DailyRewardData a, DailyRewardData b)
        {
            if (a.DAY > b.DAY)
                return 1;
            else if (a.DAY == b.DAY)
                return 0;
            else
                return -1;
        }
        public List<DailyRewardData> GetGroup(int groupID)
        {
            if (dic is null)
                return null;

            if (false == dic.ContainsKey(groupID))
                return null;

            return dic[groupID];
        }
    }
}