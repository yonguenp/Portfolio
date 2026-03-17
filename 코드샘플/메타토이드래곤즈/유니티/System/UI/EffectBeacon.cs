
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class EffectBeacon : MonoBehaviour
    {
        [SerializeField]
        private GameObject root = null;
        public GameObject Root { get { return root; } }
        [SerializeField]
        private GameObject particle = null;
        public GameObject Particle { get { return particle; } }

        private Camera effectCamera = null;
        public Camera EffectCamera
        {
            get
            {
                if (effectCamera == null)
                {
                    var camera = GameObject.FindGameObjectWithTag("UICamera");
                    if (camera != null)
                        effectCamera = camera.GetComponent<Camera>();
                }
                return effectCamera;
            }
        }

        private float touchTime = 0f;
        public float TouchTime
        {
            get { return 0.05f; }
        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && touchTime <= 0f)
            {
                TouchEffect();
                touchTime = TouchTime;
            }
            touchTime -= Time.deltaTime;
        }
        public void TouchEffect()
        {
            if (EffectCamera != null)
            {
                var lPosition = EffectCamera.ScreenToWorldPoint(Input.mousePosition);
                lPosition.z = 0f;

                if (Particle != null)
                {
                    var particle = Instantiate(Particle);
                    if (particle != null)
                    {
                        if (Root == null)
                            particle.transform.SetParent(transform);
                        else
                            particle.transform.SetParent(Root.transform);

                        particle.transform.position = lPosition;
                    }
                }
            }
        }
    }
}
