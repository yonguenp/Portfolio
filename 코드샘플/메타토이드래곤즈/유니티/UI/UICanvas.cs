using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UICanvas : SBPersistentSingleton<UICanvas>
    {
        [SerializeField] Canvas uiMainCanvas = null;
        [SerializeField] Camera uiMainCamera = null;
        [SerializeField] WebIndecator webIndecator = null;
        [SerializeField] GameObject dim = null;
        // Background Blur Effect
        [SerializeField] UIBackgroundBlurController blurBackground = null;
        [SerializeField] ScenarioManager ScenarioManager = null;

        public WebIndecator Indecator { get { return webIndecator; } }
        public GameObject DIM { get { return dim; } }
        public bool BlurOpening
        { 
            get 
            {
                if (blurBackground != null)
                {
                    return blurBackground.gameObject.activeInHierarchy;
                }

                return false;
            } 
        }
        private Tweener tweener = null;

        protected override void Awake()
        {
            base.Awake();

            if (Indecator != null)
                Indecator.SetActive(false);

            if (DIM != null)
                DIM.SetActive(false);

            if (blurBackground != null)
            {
                blurBackground.gameObject.SetActive(false);
            }
        }

        public Camera GetCamera()
        {
            return uiMainCamera;
        }

        public RectTransform GetCanvasRectTransform()
        {
            return uiMainCanvas.GetComponent<RectTransform>();
        }

        public void StartBackgroundBlurEffect()
        {
            blurBackground.gameObject.SetActive(true);
        }

        public void EndBackgroundBlurEffect()
        {
            blurBackground.Clear();            
        }

        public ScenarioManager GetScenarioManager()
        {
            return ScenarioManager;
        }
    }
}