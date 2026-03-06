using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	//프로 카메라 PanAndZoom 기능 중 수정사항이 필요한 부분수정을 위한 코드
	//프로 카메라 업데이트시 ProCamera2DPanAndZoomCustomEditor도 최신화 필요

	public class ProCamera2DPanAndZoomCustom: ProCamera2DPanAndZoom, IPreMover
	{
		new public void PreMove(float deltaTime)
		{
			if (PopupManager.OpenPopupCount > 0) { return; }

			// Detect if the user is pointing over an UI element
			bool noneUITarget = _eventSystem.currentSelectedGameObject == null;
			
			if (UseTouchInput)
			{
				_skip = DisableOverUGUI && _eventSystem && !noneUITarget;				
				if (_skip)
				{
					if(Input.touchCount > 0)
						_prevTouchPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, Mathf.Abs(Vector3D(ProCamera2D.LocalPosition)));
					CancelZoom();
				}
			}

			if (UseMouseInput)
			{
				_skip = DisableOverUGUI && _eventSystem && !noneUITarget;
				if (_skip)
				{
					_prevMousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Vector3D(ProCamera2D.LocalPosition)));
					CancelZoom();
				}
			}

			IsZooming = false;

			if (enabled && AllowPan && !_skip && noneUITarget)
				Pan(deltaTime);

			if (enabled && AllowZoom && !_skip && noneUITarget)
				Zoom(deltaTime);
		}
	}
}
