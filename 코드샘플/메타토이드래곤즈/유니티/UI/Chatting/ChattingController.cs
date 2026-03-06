using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 내용
 * 각 탭 별 스크롤 데이터기반 UI 갱신(월드, 시스템 , 1:1 등등)
 * 각 탭 별 텍스트 입력 타이머 갱신(타임오브젝트 갱신할 각 타입별 딜레이값 달리 들고있어야할듯.
 * 입력 필드 및 딜레이 버튼 제어 (onSubmitEvent)
 * 대화 데이터 생성 및 적용(소켓 방식 이전 임시 소스)
 * 탭 컨트롤러에서 물고 있고 세팅해줌
 * 
 */
namespace SandboxNetwork
{
    public class ChattingController : MonoBehaviour
    {
        [SerializeField] ChatBlockController blockController = null;
        [SerializeField] ChatMacroController macroController = null;

        //-------- 차단 관리 버튼-----------
        public void InitBlockUI()
        {
            OnHideBlockUI();
        }
        public void OnClickBlockUI()
        {
            OnHideMacroUI();
            if (blockController != null)
                blockController.ToggleUI();
        }
        void OnHideBlockUI()
        {
            if (blockController != null)
                blockController.OnHideUI();
        }
        //-------- 매크로 관리 버튼-----------
        public void InitMacroUI()
        {
            OnHideMacroUI();
        }
        public void OnClickMacroUI()
        {
            OnHideBlockUI();
            if (macroController != null)
                macroController.ToggleUI();
        }
        void OnHideMacroUI()
        {
            if (macroController != null)
                macroController.OnHideUI();
        }
    }
}
