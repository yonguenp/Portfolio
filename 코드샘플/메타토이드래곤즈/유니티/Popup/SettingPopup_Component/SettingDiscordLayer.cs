using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SettingDiscordLayer : MonoBehaviour
    {
        [SerializeField]
        private string url = "";

        public void onClickDiscordButton()
        {
            if (url != "")
            {
                Application.OpenURL(url);
            }
        }
    }
}
