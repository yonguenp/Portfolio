using Coffee.UIEffects;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using static SandboxNetwork.FriendManager;


namespace SandboxNetwork
{
    public enum eOtherNodeType
    {
        NONE,
        FRIEND,
        GUILD,
        FRIEND_ACCEPT,
        FRIEND_SEND,
        FRIEND_RECOMMEND,
        BLOCK_USER
    }

    public class ChattingOtherNode : MonoBehaviour, EventListener<GiftEvent>
    {
        ThumbnailUserData Data = null;
        [SerializeField] UserPortraitFrame portraitFrame;
        [SerializeField] Text userName;
        [SerializeField] Text logTime;
        [SerializeField] Button chatBtn;
        [SerializeField] Button inviteBtn;
        [SerializeField] Button acceptBtn;
        [SerializeField] Button rejectBtn;
        [SerializeField] Button cancleBtn;
        [SerializeField] Button blockCancleBtn;
        [SerializeField] GameObject receiveIcon;
        [SerializeField] GameObject sendIcon;
        // Start is called before the first frame update
        FriendRemoveCB FriendDeleteCallBack = null;
        public int SendGift { get; set; } = -1;
        public int ReceiveGift { get; set; } = -1;
        int ReturnTab { get; set; } = -1;
        eOtherNodeType CurType { get; set; } = eOtherNodeType.NONE;
        Action<ThumbnailUserData> ClickAction { get; set; } = null;
        bool IsProfileBlock { get; set; } = false;
        public void Init(ThumbnailUserData data, eOtherNodeType eType, Action<ThumbnailUserData> action = null)
        {
            Data = data;
            Init(eType, action);
        }
        public void Init(eOtherNodeType eType, Action<ThumbnailUserData> action = null)
        {
            CurType = eType;
            ClickAction = action;
            SetUI();

            if (Data == null)
                return;

            SetFrame();
        }

        private void OnEnable()
        {
            EventManager.AddListener<GiftEvent>(this);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<GiftEvent>(this);
        }
        public void SetReturnTab(int _ReturnTab)
        {
            ReturnTab = _ReturnTab;
        }
        public void SetFrame()
        {
            userName.text = Data.Nick;
            portraitFrame.SetUserPortraitFrame(Data.PortraitIcon, Data.Level, true, Data.EtcInfo);
            if (logTime != null)
            {
                logTime.gameObject.SetActive(Data.LastActiveTime > 0);
                logTime.text = Data.LastActiveTime > 0 ? StringData.GetStringFormatByStrKey("마지막접속시간", SBFunc.TimeStampDeepString(Data.LastActiveTime)) : "";
            }
        }
        private void SetUI()
        {
            if (chatBtn != null)
                chatBtn.gameObject.SetActive(eOtherNodeType.FRIEND == CurType || eOtherNodeType.GUILD == CurType);
            if (inviteBtn != null)
            {
                if(Data is FriendUserData friend)
                {
                    inviteBtn.interactable = friend.IsCanReqFriend;
                }
                inviteBtn.gameObject.SetActive(eOtherNodeType.FRIEND_RECOMMEND == CurType);
            }
            if (acceptBtn != null)
                acceptBtn.gameObject.SetActive(eOtherNodeType.FRIEND_ACCEPT == CurType);
            if (rejectBtn != null)
                rejectBtn.gameObject.SetActive(eOtherNodeType.FRIEND_ACCEPT == CurType);
            if (cancleBtn != null)
                cancleBtn.gameObject.SetActive(eOtherNodeType.FRIEND_SEND == CurType);
            if (blockCancleBtn != null)
                blockCancleBtn.gameObject.SetActive(eOtherNodeType.BLOCK_USER == CurType);

            if (logTime != null)
                logTime.gameObject.SetActive(eOtherNodeType.BLOCK_USER != CurType);

            if(receiveIcon != null && sendIcon != null && SendGift >= 0 && ReceiveGift >= 0 && CurType == eOtherNodeType.FRIEND)
            {
                receiveIcon.SetActive(true);
                sendIcon.SetActive(true);

                receiveIcon.GetComponent<UIEffect>().effectFactor = ReceiveGift > 1 ? 0 : 1;
                sendIcon.GetComponent<UIEffect>().effectFactor = SendGift > 0 ? 0 : 1; ;

            }
            else
            {
                receiveIcon.SetActive(false);
                sendIcon.SetActive(false);
            }
        }
        private void ActiveAction()
        {
            switch (CurType)
            {
                case eOtherNodeType.BLOCK_USER:
                    ClickAction?.Invoke(Data);
                    break;
                case eOtherNodeType.FRIEND:
                case eOtherNodeType.FRIEND_ACCEPT:
                case eOtherNodeType.FRIEND_SEND:
                case eOtherNodeType.FRIEND_RECOMMEND:
                case eOtherNodeType.NONE:
                default: break;
            }
        }
        public void OnClickDirectChat()
        {
            if (Data != null)
            {
                PopupManager.GetPopup<ChattingPopup>().SetDirectChat(Data, ReturnTab);
                ActiveAction();
            }
        }
        public void OnClickDetailInfo()
        {
            if (IsProfileBlock)
                return;

            IsProfileBlock = true;
            if (Data != null)
            {
                WWWForm param = new();
                param.AddField("tuno", Data.UID.ToString());
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
        public void SetRemoveCallBack(FriendRemoveCB cb)
        {
            FriendDeleteCallBack = cb;
        }
        public void OnClickInviteFriend()
        {
            if (Data == null) return;

            Instance.SendFriendInvite(Data.UID, (data) => FriendEvent.SendRegist(Data.UID), null);
            ActiveAction();
        }
        public void OnClickRemoveFriend()
        {
            if (Data != null)
            {
                Instance.DeleteFriend(Data.UID, FriendDeleteCallBack);
                ActiveAction();
            }
        }
        public void OnClickAcceptInvite()
        {
            Instance.AcceptInvite(Data.UID, (res) =>
            {
                if (res["rs"].Value<int>() == 0)
                {
                    ToastManager.On(100002562);
                    FriendEvent.SendRegist(Data.UID);
                    ActiveAction();
                }
                else
                {
                    //ToastManager.On($"서버 에러 rs::{res["rs"].Value<int>()}");
                }
            });
        }
        public void OnClickRejectInvite()
        {
            Instance.RejectInvite(Data.UID, (res) =>
            {
                if (res["rs"].Value<int>() == 0)
                {
                    ToastManager.On(100002561);
                    FriendEvent.SendRemove(Data.UID);
                    ActiveAction();
                }
                else
                {
                    //ToastManager.On($"서버 에러 rs::{res["rs"].Value<int>()}");
                }
            });
        }
        /// <summary> 아직 보낸친구 Cancle API가 없음 </summary>
        public void OnClickCancleFriend()
        {
        }
        public void OnClickCancleBlock()
        {
            ActiveAction();
        }

        public void OnEvent(GiftEvent eventType)
        {
            switch (eventType.Event)
            {
                case eGiftEventEnum.ACCEPT:
                {
                    RefreshAcceptGiftUI();
                }break;
                case eGiftEventEnum.SEND:
                {
                    RefreshSendGiftUI();
                }break;
            }
        }

        public void RefreshAcceptGiftUI(int factor = 0)
        {
            if (receiveIcon != null && ReceiveGift > 0)
            {
                receiveIcon.GetComponent<UIEffect>().effectFactor = factor;
                
            }
        }
        public void RefreshSendGiftUI(int factor = 0)
        {
            if (sendIcon != null)
            {
                sendIcon.GetComponent<UIEffect>().effectFactor = factor;
            }
        }
    }
}