using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class PopupBeacon : MonoBehaviour
    {
        [SerializeField]
        private GameObject popupTarget = null;
        [SerializeField]
        private GameObject tutorialTarget = null;
        [SerializeField]
        private PopupTopUIObject popupTop = null;

        protected virtual void Start()
        {
            // 일반 팝업
            if(popupTarget == null)
                PopupManager.Instance.SetBeacon(gameObject);
            else
                PopupManager.Instance.SetBeacon(popupTarget);

            // 튜토리얼 팝업
            if (tutorialTarget == null)
                PopupManager.Instance.SetTutorialBeacon(gameObject);
            else
                PopupManager.Instance.SetTutorialBeacon(tutorialTarget);

            PopupManager.Instance.SetPopupTop(popupTop);
        }
    }
}
