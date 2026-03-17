using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SystemMessageBeacon : MonoBehaviour
    {
        [SerializeField]
        private GameObject root = null;
        public GameObject Root { get { return root; } }

        [SerializeField]
        private GameObject toastMessage = null;
        [SerializeField]
        private GameObject caMessage = null;
        [SerializeField]
        private SystemMessage systemMessage = null;

        private void Start()
        {
            ToastManager.Instance.Init(this, root.transform, toastMessage);
            CAMessageManager.Instance.Init(this, root.transform, caMessage);
            if (systemMessage != null)
                systemMessage.Init();
        }
    }
}
