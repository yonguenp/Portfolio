using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class UIBeacon : MonoBehaviour
    {
        [SerializeField]
        private GameObject root = null;
        public GameObject Root { get { return root; } }
        [SerializeField]
        private List<UIObject> uiObjects = null;
        public List<UIObject> UIObjects { get { return uiObjects; } }

        protected virtual void Start()
        {
            UIManager.Instance.SetBeacon(this);
        }
    }
}