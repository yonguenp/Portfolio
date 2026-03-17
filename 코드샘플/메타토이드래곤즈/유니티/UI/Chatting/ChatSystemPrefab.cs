using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChatSystemPrefab : ChatPrefab
    {
        [Header("[Chat System Prefab]")]
        public Image nodeBGImage = null;
        public Color normalSystemBGColor = Color.white;
        public Color globalSystemBGColor = Color.white;

        public override void Init(ChatUIItem chatData)
        {
            base.Init(chatData);

            chatUIType = chatData.chatUIType;

            if (nodeBGImage != null)
            {
                nodeBGImage.color = chatData.chatInfo.SendUserGuildUID > 0 ? globalSystemBGColor : normalSystemBGColor;
            }
        }
    }
}