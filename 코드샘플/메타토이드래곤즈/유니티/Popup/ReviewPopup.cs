using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    /*
     * WJ - 2023.09.21
     * systemPopup을 연속으로 두번 띄워야 하는 이슈가 생김 - 즐거웠나요 질문 팝업 -> 리뷰 남기기 팝업 또는 고객센터 이동팝업
     * 구조적으로 yes 를 누르던, no를 누르던 SystemPopup이 또 떠야되서, 할 수없이 자식 클래스 생성
     */
    public class ReviewPopup : SystemPopup
    {
        bool _isEqualParentDimmed = false;
        public static ReviewPopup OnReviewPopup(string title, string body, string yes, string no, Action okCallBack = null, Action cancelCallBack = null, Action exitCallBack = null, bool _isEqualParentDimmed = true)
        {
            var popup = PopupManager.OpenPopup<ReviewPopup>();

            popup.SetMessage(title, body, yes, no);
            popup.SetCallBack(okCallBack, cancelCallBack, exitCallBack);
            popup.SetDimmedFlag(_isEqualParentDimmed);
            return popup;
        }

        public void SetDimmedFlag(bool _isEqual)
        {
            _isEqualParentDimmed = _isEqual;
        }

        public override void OnClickDimd()
        {
            if (_isEqualParentDimmed)
                base.OnClickDimd();
            else
            {
                exitCall?.Invoke();
                ClosePopup();
            }
        }
    }
}
