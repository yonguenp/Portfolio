using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * todo
 * 1. request 기본 테이블 데이터 세팅하기
 * 2. request 항목 하나(일단 UI상 최대 4개)를 구성하기 위한 데이터 구조(초상화 ID, 요구 아이템 리스트, 획득 보상, 소원 상태 (갱신쿨 도는 중 인지, 열/닫)
 * 3. 기본 보상 (하루 단위) 타이머
 * 4. 보상 획득 상태
 */
namespace SandboxNetwork
{
    public class LandmarkExchange : Landmark
    {
        public LandmarkExchange()
            : base(eLandmarkType.EXCHANGE)
        {

        }

        public override void SetData(JToken jsonData)//서버쪽에서 데이터 세팅
        {
            base.SetData(jsonData);

        }
    }
}