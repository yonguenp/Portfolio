using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    /// <summary>
    /// 이벤트 출석 체크 레이어 - 새 레이어가 아니라, 탭 누르면 새 팝업 (기존에 만든 출첵팝업)이 뜨는 형태
    /// </summary>
    public class DiceEventAttendanceLayer : TabLayer
    {
        public override void InitUI(TabTypePopupData datas = null)
        {
            base.InitUI(datas);
        }

        public override void RefreshUI()//데이터 유지 갱신
        {
        }
    }
}
