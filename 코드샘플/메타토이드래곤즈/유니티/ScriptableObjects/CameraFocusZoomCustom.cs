using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    [CreateAssetMenu(fileName = "CameraFocusZoomCustom", menuName = "Scriptable Object Asset/EffectCustom/FocusZoom", order = 2)]
    public class CameraFocusZoomCustom : EffectCustom
    {
        [Header("[Custom Value]")]
        public eEffectCustomFocusObjectType focusObjectType = eEffectCustomFocusObjectType.None;
        public Vector3 focusPostion = Vector3.zero;

        [Space(10)]
        public float ZoomDuration = 1;
        public float ZoomOutBorder = 1;
        public float ZoomInBorder = 1;
        public float ZoomInSmoothness = 0.5f;
        public float ZoomOutSmoothness = 1.0f;

        public float MaxZoomInAmount = 2;
        public float MaxZoomOutAmount = 3;

        public bool DisableWhenOneTarget = false;

        public bool CompensateForCameraPosition = false;

        private void Awake()
        {
            effectCustomType = eEffectCustomType.CameraFocusZoom;
        }
    }
}
