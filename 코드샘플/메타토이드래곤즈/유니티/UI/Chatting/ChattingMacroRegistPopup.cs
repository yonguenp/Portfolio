using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChattingMacroRegistPopup : Popup<ChattingMacroData>
    {
        [SerializeField] InputField inputField = null;
        [SerializeField] Button sendButton = null;

        private int clickIndex = 0;
        public void OnClickCloseBtn()
        {
            ClosePopup();
        }
        public override void InitUI()//수정전(기존 등록된) 매크로 string 값 가져오기
        {
            clickIndex = Data.macroIndex;
        }

        public void OnClickRegistMacro()
        {
            var originText = inputField.text;
            var trimText = originText.Trim();

            if(trimText == "")
            {
                ToastManager.On(100002501);
                return;
            }

            //필터링(금칙어) 체크 로직 필요함


            //to do 서버 데이터 요청 성공 시(팝업 끄고) chatmanager 매크로 리스트 데이터 갱신 및 UI 이벤트 갱신


        }
    }
}
