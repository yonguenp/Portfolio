using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class UserDragonCard : ITableData
    {
        public int CardTag { get; private set; } = -1;
        public int DragonTag { get; private set; } = -1;
        public int CardGrade { get; private set; } = -1;

        public bool Set(JToken jsonData)
        {
            var jarrayData = JArray.FromObject(jsonData);

            if (jarrayData.Type == JTokenType.Array)
            {
                if (jarrayData.Count == 2)
                {
                    SetData(jarrayData[0].Value<int>(), jarrayData[1].Value<int>());
                    return true;
                }
            }

            return false;
        }

        protected void SetData(int cTag, int dTag)
        {
            CardTag = cTag;
            DragonTag = dTag;

            var charData = CharBaseData.Get(DragonTag.ToString());
            CardGrade = charData == null ? -1 : charData.GRADE;
        }

        public void Init() { }
        public string GetKey() { return CardTag.ToString(); }
    }
}