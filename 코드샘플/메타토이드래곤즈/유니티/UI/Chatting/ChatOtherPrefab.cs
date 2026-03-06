using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChatOtherPrefab : ChatPrefab
    {
        [SerializeField]
        UserPortraitFrame otherPortrait;
        [SerializeField]
        RectTransform portraitRect;
        [SerializeField]
        RectTransform textRect;
        bool IsProfileBlock { get; set; } = false;
        public override void Init(ChatUIItem _chatTotalData)
        {
            base.Init(_chatTotalData);

            var pType = _chatTotalData.chatInfo.PortraitType;
            var pValue = _chatTotalData.chatInfo.PortraitValue;
            var pData = new PortraitEtcInfoData((ePortraitEtcType)pType, pValue);

            otherPortrait.SetUserPortraitFrame(chatData.chatInfo.SendIcon, 0, true, pData);
            //otherPortrait.SetCustomPotraitFrame();
        }

        /*
         * 프로필 팝업에 뿌려줄 유저 데이터 필요함.(프로필 데이터, 친구 상태, pvp 랭크 , 신고 상태 등등... init 함수 파라미터 통 데이터로 받아서 그대로 popup에 넘겨야할듯.
         */
        public void OnClickChatObject()//상대방 프로필 팝업 연동
        {
            if (IsProfileBlock)
                return;

            IsProfileBlock = true;
            if (chatData != null && chatData.chatInfo != null)
            {
                WWWForm param = new();
                param.AddField("tuno", chatData.chatInfo.SendUID.ToString());
                NetworkManager.Send("user/profile", param, (jsonData) =>
                {
                    if (SBFunc.IsJArray(jsonData["profile"]))
                    {
                        var array = (JArray)jsonData["profile"];
                        for (int i = 0, count = array.Count; i < count; ++i)
                        {
                            if (false == SBFunc.IsJObject(array[i]))
                                continue;

                            ChattingProfilePopup.OpenPopup(new ChattingPopupData(new(array[i])));
                            IsProfileBlock = false;
                            return;
                        }
                    }
                    //실패 토스트
                    ToastManager.Instance.Set(StringData.GetStringByIndex(100002264));
                    IsProfileBlock = false;

                }, (error) =>
                {
                    //실패 토스트
                    ToastManager.Instance.Set(StringData.GetStringByIndex(100002264));
                    IsProfileBlock = false;
                });
                return;
            }
            //실패 토스트
            ToastManager.Instance.Set(StringData.GetStringByIndex(100002264));
            IsProfileBlock = false;
        }

        public void OnClickDetailInfo()
        {
        }

        public void OnClickUserBlock()
        {
            var popup = PopupManager.OpenPopup<ChattingBlockAlarmPopup>();
            popup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002426));
            popup.SetUserNickName(chatData.chatInfo.SendNickname);
            popup.SetCallBack(() =>
            {
                ChatManager.Instance.AddUserBlockList(chatData.chatInfo);
            },
            () =>
            {//cancle
            },
            () =>
            {//exit
            });
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
                if (rectTransform != null)
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, sizeY + diffY + 40f);
            }
            else
            {
                base.RebuildLayout();
            }
        }
    }
}
