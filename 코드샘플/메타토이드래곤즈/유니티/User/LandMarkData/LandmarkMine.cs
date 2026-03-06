using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * todo
 * miningManager 쪽에서 통합 관리 하려고함.
 */
namespace SandboxNetwork
{
    public class LandmarkMine : Landmark
    {
        public LandmarkMine()
            : base(eLandmarkType.MINE)
        {

        }

        public override void SetData(JToken jsonData)//서버쪽에서 데이터 세팅
        {
            base.SetData(jsonData);

            //miningManager 연결
            MiningManager.Instance.UpdateUserMiningData((JObject)jsonData);
        }
    }
}