using UnityEngine;

namespace SandboxNetwork
{
    public class ChatMyPrefab : ChatPrefab
    {
        [SerializeField]
        UserPortraitFrame myPortrait;
        [SerializeField]
        RectTransform portraitRect;
        [SerializeField]
        RectTransform textRect;

        public override void Init(ChatUIItem _chatTotalData)
        {
            base.Init(_chatTotalData);

            var pType = _chatTotalData.chatInfo.PortraitType;
            var pValue = _chatTotalData.chatInfo.PortraitValue;
            var pData = new PortraitEtcInfoData((ePortraitEtcType)pType , pValue);


            myPortrait.SetUserPortraitFrame(User.Instance.UserData.UserPortrait, 0, true, pData);
        }
        public override void RebuildLayout()
        {
            if (textRect != null)
            {
                RefreshContentFitters();

                var sizeY = textRect.sizeDelta.y;
                if (portraitRect != null)
                    portraitRect.sizeDelta = new Vector2(portraitRect.sizeDelta.x, sizeY);

                var rectTransform = GetComponent<RectTransform>();
                if(rectTransform != null)
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, sizeY + diffY + 40f);
            }
            else
            {
                base.RebuildLayout();
            }
        }
    }
}

