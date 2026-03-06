using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class UserBuffData
    {
        public class BuffUnit
        {
            public eStatusCategory category { get; private set; } = eStatusCategory.NONE;
            public eStatusType type { get; private set; } = eStatusType.NONE;
            public float value { get; private set; } = 0;

            public BuffUnit(eStatusCategory c, eStatusType s, float v) 
            {
                category = c;
                type = s;
                value = v;
            }
        }
        AdditionalStatus UserBuff = new AdditionalStatus();
        public Dictionary<eExtraStatContent, List<BuffUnit>> BuffByContent { get; private set; } = new Dictionary<eExtraStatContent, List<BuffUnit>>();
        /// <summary>
        /// 콜렉션 & 업적 포함 버프
        /// </summary>
        /// <returns></returns>
        public AdditionalStatus GetUserBuff()
        {
            return UserBuff;
        }

        void InitData()
        {
            BuffByContent.Clear();
            UserBuff.Initialze();
        }

        public void SetUserBuffData(JToken _jsonData)
        {
            InitData();

            if (SBFunc.IsJTokenType(_jsonData, JTokenType.Object))
                SetBuff((JObject)_jsonData);
        }

        void SetBuff(JObject _jobect)
        {
            BuffByContent.Clear();

            foreach (var val in _jobect.Properties().ToArray())
            {
                var key = int.Parse(val.Name);//eStatusType

                if(val.Value.Type == JTokenType.Object)
                {
                    foreach (var con in ((JObject)val.Value).Properties().ToArray())
                    {
                        var contents = (eExtraStatContent)int.Parse(con.Name);
                        var value = con.Value.Value<float>();
                        eStatusCategory category;
                        eStatusType type;
                        if ((int)eStatusType.MAX <= key)
                        {
                            category = eStatusCategory.RATIO;
                            type = key - eStatusType.PERC_BASE;
                        }
                        else
                        {
                            category = eStatusCategory.ADD;
                            type = (eStatusType)key;
                        }

                        if (type <= eStatusType.NONE)
                            continue;

                        UserBuff.IncreaseStatus(category, type, value);
                        if (!BuffByContent.ContainsKey(contents))
                            BuffByContent.Add(contents, new List<BuffUnit>());

                        BuffByContent[contents].Add(new BuffUnit(category, type, value));
                    }
                }
                else//과거의 유산
                {
                    var value = val.Value.Value<float>();//value

                    eStatusCategory category;
                    eStatusType type;
                    if ((int)eStatusType.MAX <= key)
                    {
                        category = eStatusCategory.RATIO;
                        type = key - eStatusType.PERC_BASE;
                    }
                    else
                    {
                        category = eStatusCategory.ADD;
                        type = (eStatusType)key;
                    }

                    if (type <= eStatusType.NONE)
                        continue;

                    UserBuff.IncreaseStatus(category, type, value);
                }
            }
        }

        /// <summary>
        /// push API : extra_stat_update 로 '결과치'로 들어오기 때문에 덮어쓰면 됩니다.
        /// </summary>
        public void UpdateUserBuffData(JObject _jsonData)
        {
            if(_jsonData.ContainsKey("data") && SBFunc.IsJTokenType(_jsonData["data"], JTokenType.Object))
            {
                StatusClear();
                SetBuff((JObject)_jsonData["data"]);
            }
        }

        public void StatusClear()
        {
            UserBuff.SetStatus();
        }

        public bool IsArtBlockAble 
        { 
            get {
                if(BuffByContent.ContainsKey(eExtraStatContent.ARTBLOCK))
                {
                    return BuffByContent[eExtraStatContent.ARTBLOCK].Count > 0;
                }

                return false;
            } 
        }
    }
}

