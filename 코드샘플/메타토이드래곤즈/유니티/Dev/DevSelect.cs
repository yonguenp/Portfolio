using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DevSelect : MonoBehaviour
    {
        private void Start()
        {
            UIManager.Instance.InitUI(eUIType.None);
        }
        public void OnDevLoginClick() //삽건웅
        {
            LoadingManager.Instance.EffectiveSceneLoad("DevLogin");
            //LoadingManager.ImmediatelySceneLoad("DevLogin");
        }
        public void OnLoginClick() //아직 안됨 ~
        {

		}
    }
}
