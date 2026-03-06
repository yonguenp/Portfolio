using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    [CreateAssetMenu(fileName = "CameraShakeCustom", menuName = "Scriptable Object Asset/EffectCustom/CameraShake", order = 2)]
    public class CameraShakeCustom : EffectCustom
    {
        [Header("[Custom Value]")]
        public Vector3 Strength = new Vector2(10, 10);

        [Range(.02f, 3f)]
        public float Duration = .5f;

        [Range(1, 100)]
        public int Vibrato = 10;

        [Range(0f, 1f)]
        public float Randomness = .1f;

        [Range(0f, .5f)]
        public float Smoothness = .1f;

        public bool UseRandomInitialAngle = true;

        [Range(0f, 360f)]
        public float InitialAngle;

        public Vector3 Rotation;

        public bool IgnoreTimeScale;

        private void Awake()
        {
            effectCustomType = eEffectCustomType.CameraShake;
        }
    }
}
