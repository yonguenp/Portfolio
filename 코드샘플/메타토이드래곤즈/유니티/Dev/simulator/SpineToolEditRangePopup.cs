using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class SpineToolEditRangePopup : Popup<PopupData>//스킬 조정 범위를 직접 입력 하고 싶다고 해서 세팅하는 팝업
	{
		[SerializeField] private InputField input;

		private SBSpineController controller = null;

		bool isRangeX = false;
		public override void InitUI()
		{

		}

		public void SetController(SBSpineController _controller, bool rangeState)
        {
			if(controller == null)
            {
				controller = _controller;
            }

			isRangeX = rangeState;
		}

		public void OnClickSaveRange()
		{
			if (input.text == "")
			{
				ToastManager.On("조정할 범위를 입력해주세요");
				return;
			}

			if(float.TryParse(input.text,out float result))
            {
				if(controller != null)
                {
					controller.SetModifyRange(isRangeX, result);
                }

				ClosePopup();
			}
            else
            {
				ToastManager.On("숫자(소수점 가능)로 입력해주세요");
			}
		}
	}
}
