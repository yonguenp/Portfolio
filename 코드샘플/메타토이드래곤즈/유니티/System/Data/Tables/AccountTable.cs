using Newtonsoft.Json.Linq;
using SandboxNetwork.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class AccountTable : TableBase<AccountData, DBAccount_exp> 
    {
        public Dictionary<int, AccountData> levelDatas = new Dictionary<int, AccountData>();

        public override void DataClear()
        {
            base.DataClear();
            levelDatas.Clear();
        }

        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        protected override bool Add(AccountData data)
        {
            if (base.Add(data))
            {
                AddLevel(data);
                return true;
            }
            return false;
        }
        protected bool AddLevel(AccountData data)
        {
            if (levelDatas.ContainsKey(data.LEVEL))
            {
                UnityEngine.Debug.LogError("SBTableBase Error : 중복 키 => " + data.LEVEL);
                return false;
            }

            levelDatas.Add(data.LEVEL, data);
            return true;
        }
        public AccountData GetByLevel(int key)
        {
            if (levelDatas.ContainsKey(key))
            {
                return levelDatas[key];
            }
            return null;
        }

        public int GetLevelByExp(int exp)
        {
            foreach(var ld in levelDatas.Values)
            {
                if (ld.EXP > exp)
                    return ld.LEVEL;
            }

            return levelDatas.Count;
        }
        /// <summary>
        /// NORMAL_REWARD 와 SPECIAL_REWARD 가 둘다 0이 아닌 로우를 가져옴
        /// </summary>
        /// <returns></returns>
        public List<AccountData> GetTotalRewardList()
        {
            List<AccountData> ret = new List<AccountData>();

            foreach(var data in levelDatas)
            {
                var value = data.Value;

                if (value.NORMAL_REWARD > 0 && value.SPECIAL_REWARD > 0)
                    ret.Add(value);
            }

            ret.Sort((a,b) => {
                if (a.LEVEL > b.LEVEL)
                    return 1;
                else if (a.LEVEL == b.LEVEL)
                    return 0;
                else
                    return -1;
            });

            return ret;
        }
    }
}
