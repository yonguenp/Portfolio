using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/*
 * 매크로 리스트
 * chatManager에서 매크로 목록 데이터 송/수신 만들어야함
 * 
 */
namespace SandboxNetwork
{
    public class ChatMacroController : MonoBehaviour
    {
        [SerializeField] GameObject chatMacroObject = null;
        [SerializeField] List<InputField> macroInputFieldList = new List<InputField>();

        private bool isShow = false;
        public void OnShowUI()
        {
            if (chatMacroObject != null)
                chatMacroObject.SetActive(true);
            isShow = true;

            RefreshMacroScrollData();
        }

        public void OnHideUI()
        {
            if (chatMacroObject != null)
                chatMacroObject.SetActive(false);
            isShow = false;
        }

        public void ToggleUI()
        {
            if (isShow)
                OnHideUI();
            else
                OnShowUI();
        }

        void RefreshMacroScrollData()//chatManager 매크로 리스트 갱신해서 다시그리기(일단 임시 데이터)
        {
            var macroStringIndexList = ChatManager.Instance.GetMacroStringIndexArray();
            if (macroStringIndexList == null || macroStringIndexList.Length <= 0)
                return;

            for(int i = 0; i < macroStringIndexList.Length; i++)
            {
                var index = macroStringIndexList[i];
                var stringData = StringData.GetStringByIndex(index);

                if (macroInputFieldList.Count > i)
                    macroInputFieldList[i].text = stringData;
            }
        }

        public void OnClickInputFieldFlag(int _slotIndex)//발송 버튼에 인덱스 하나씩 달아서 리스트 인덱스 플래그 넘기고, 세팅된 인풋값으로 보내기
        {
            if (macroInputFieldList == null || macroInputFieldList.Count <= 0 || macroInputFieldList.Count <= _slotIndex)
                return;

            var currentText = macroInputFieldList[_slotIndex].text;
            var trimCheck = currentText.Trim();

            if(trimCheck == "")
            {
                ToastManager.On(100002538);
                return;
            }

            ChatEvent.SendChatMacro(trimCheck);
        }

        public void OnClickMacroRegistPopup(int _slotIndex)
        {
            if (macroInputFieldList == null || macroInputFieldList.Count <= 0 || macroInputFieldList.Count <= _slotIndex)
                return;

            PopupManager.OpenPopup<ChattingMacroRegistPopup>(new ChattingMacroData("", _slotIndex));
        }
    }
}
