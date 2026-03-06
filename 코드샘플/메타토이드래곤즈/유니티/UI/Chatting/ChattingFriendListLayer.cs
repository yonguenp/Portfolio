using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum eFriendListType
    {
        NONE,
        FRIEND,
        SUGGESTION,
        ADDED,
    }
    public enum eGiftEventEnum
    {
        SEND,
        ACCEPT,
    }

    public struct GiftEvent
    {
        public static GiftEvent e = default;
        public eGiftEventEnum Event { get; set; }

        public static void SendGift()
        {
            e.Event = eGiftEventEnum.SEND;
            EventManager.TriggerEvent(e);
        }
        public static void AcceptGift()
        {
            e.Event = eGiftEventEnum.ACCEPT;
            EventManager.TriggerEvent(e);
        }
    }

    public class ChattingFriendListLayer : TabLayer, EventListener<FriendEvent>
    {
        [Header("Default")]
        [SerializeField]
        ChattingOtherNode friendNode = null;
        [SerializeField]
        Text titleText = null;
        [SerializeField]
        Text titleMainText = null;
        [SerializeField]
        GameObject noFriendAlertNode = null;
        [Header("MainScroll")]
        [SerializeField]
        RectTransform mainScroll = null;
        [SerializeField]
        Transform mainScrollParent = null;
        [SerializeField]
        Text mainScrollText = null;
        [Header("SubScroll")]
        [SerializeField]
        GameObject subScrollNode = null;
        [SerializeField]
        Transform subScrollParent = null;
        [SerializeField]
        Text subScrollText = null;

        [Header("TopButton")]
        [SerializeField]
        Button backBtn = null;
        [SerializeField]
        Button suggestionListBtn = null;
        [SerializeField]
        Button addedListBtn = null;

        [Header("Friend")]
        [SerializeField]
        Button friendlySendAllBtn = null;
        [SerializeField]
        Button friendlyRecvAllBtn = null;
        [SerializeField]
        Button friendlyShopBtn = null;

        [Header("Search")]
        [SerializeField]
        InputField searchEditBox = null;
        [SerializeField]
        Button searchBtn = null;

        eFriendListType CurType { get; set; } = eFriendListType.NONE;
        bool networkEnable = false;
        string lastSearch = "";
        List<FriendUserData> searchInfos = new List<FriendUserData>();
        List<FriendUserData> recommendInfos = new List<FriendUserData>();
        List<FriendUserData> receiveInfos = new List<FriendUserData>();
        List<ChattingOtherNode> mainScrollNodes = new List<ChattingOtherNode>();
        List<ChattingOtherNode> subScrollNodes = new List<ChattingOtherNode>();
        private void OnEnable()
        {
            this.EventStart();
        }
        private void OnDisable()
        {
            this.EventStop();
            PopupManager.Instance.Top.SetFriendPointUI(false);
        }
        public override void InitUI(TabTypePopupData datas = null)
        {
            PopupManager.Instance.Top.SetFriendPointUI(true);
            CurType = eFriendListType.NONE;
            OnClickFriendList();
        }
        void ClearNode()
        {
            foreach (var node in mainScrollNodes)
            {
                node.gameObject.SetActive(false);
            }
            foreach (var node in subScrollNodes)
            {
                node.gameObject.SetActive(false);
            }
        }
        void RemoveItem(long userNo)
        {
            searchInfos = searchInfos.FindAll(data => data.UID != userNo);

            SetScrollName();
            SetFriends();
        }

        public void OnEvent(FriendEvent eventType)
        {
            switch (eventType.eType)
            {
                case eFriendEventType.REFRESH:
                {
                    switch (CurType)
                    {
                        case eFriendListType.SUGGESTION:
                            OnClickSuggestion(false);
                            break;
                        case eFriendListType.ADDED:
                            OnClickAddedList();
                            break;
                        case eFriendListType.NONE:
                        case eFriendListType.FRIEND:
                        default:
                            OnClickFriendList();
                            break;
                    }
                }
                break;
                case eFriendEventType.REMOVE:
                {
                    switch (CurType)
                    {
                        case eFriendListType.ADDED:
                            SetScrollName();
                            receiveInfos = receiveInfos.FindAll(data => data.UID != eventType.UserUID);
                            if (mainScrollText != null)
                                mainScrollText.text = string.Format(mainScrollText.text + " ({0}/{1})", receiveInfos.Count, GameConfigTable.GetConfigIntValue("FRIEND_MAX_NUM", 30));
                            MainScrollCreateItem(receiveInfos, eOtherNodeType.FRIEND_ACCEPT);
                            break;
                        case eFriendListType.FRIEND:
                            RemoveItem(eventType.UserUID);
                            break;
                        case eFriendListType.NONE:
                        case eFriendListType.SUGGESTION:
                        default:
                            break;
                    }
                }
                break;
                case eFriendEventType.REGIST:
                {
                    switch (CurType)
                    {
                        case eFriendListType.SUGGESTION:
                            for(int i = 0, count = recommendInfos.Count; i < count; ++i)
                            {
                                if (recommendInfos[i] == null)
                                    continue;

                                if (recommendInfos[i].UID != eventType.UserUID)
                                    continue;

                                recommendInfos[i].IsCanReqFriend = false;
                                break;
                            }

                            OnClickSuggestion(false);
                            break;
                        case eFriendListType.ADDED:
                            SetScrollName();
                            receiveInfos = receiveInfos.FindAll(data => data.UID != eventType.UserUID);
                            if (mainScrollText != null)
                                mainScrollText.text = string.Format(mainScrollText.text + " ({0}/{1})", receiveInfos.Count, GameConfigTable.GetConfigIntValue("FRIEND_MAX_NUM", 30));
                            MainScrollCreateItem(receiveInfos, eOtherNodeType.FRIEND_ACCEPT);
                            break;
                        case eFriendListType.NONE:
                        case eFriendListType.FRIEND:
                        default:
                            break;
                    }
                }
                break;
                default: break;
            }
        }
        public void SetTopButtonEnable(bool isback, bool isSuggestion, bool isAdded)
        {
            if (backBtn != null)
                backBtn.gameObject.SetActive(isback);
            if (suggestionListBtn != null)
                suggestionListBtn.gameObject.SetActive(isSuggestion);
            if (addedListBtn != null)
                addedListBtn.gameObject.SetActive(isAdded);
        }
        public void SetBotButtonEnable(bool isFriend, bool isSearch)
        {
            if (friendlySendAllBtn != null)
                friendlySendAllBtn.gameObject.SetActive(isFriend);
            if (friendlyRecvAllBtn != null)
                friendlyRecvAllBtn.gameObject.SetActive(isFriend);
            if (friendlyShopBtn != null)
                friendlyShopBtn.gameObject.SetActive(isFriend);
            if (searchEditBox != null)
                searchEditBox.gameObject.SetActive(isSearch);
            if (searchBtn != null)
                searchBtn.gameObject.SetActive(isSearch);
        }
        public void OnClickFriendList()
        {
            if (CurType == eFriendListType.FRIEND)
                return;
            if (networkEnable)
                return;

            CurType = eFriendListType.FRIEND;

            SetTopButtonEnable(false, true, true);
            SetBotButtonEnable(true, false);
            RefreshFriendlyHeartBtn();
            networkEnable = true;
            friendNode.gameObject.SetActive(false);
            searchInfos = FriendManager.Instance.GetFriendDataToList();
            SetScrollName();
            SetSubScroll(false);
            SetFriends();
            ReddotManager.Set(eReddotEvent.FRIEND, false);

            FriendManager.Instance.FriendList((callBack) =>
            {
                networkEnable = false;
            }, (body) => networkEnable = false);
        }
        public void OnClickAddedList()
        {
            noFriendAlertNode.SetActive(false);

            if (CurType == eFriendListType.ADDED)
                return;
            if (networkEnable)
                return;

            CurType = eFriendListType.ADDED;

            SetTopButtonEnable(true, false, false);
            SetBotButtonEnable(false, false);
            networkEnable = true;
            ClearNode();
            //SetSubScroll(true);
            //2024-03-25 일단 메인리스트에 받은 신청내역만 나오도록 적용.
            SetSubScroll(false);
            SetScrollName();
            ReddotManager.Set(eReddotEvent.FRIEND, false);

            FriendManager.Instance.FriendReceiveList((callback) =>
            {
                networkEnable = false;
                int count = 0;
                if (callback == null || callback.Count < 1)
                    ToastManager.On(100000774);
                else
                    count = callback.Count;
                //콜백이 두개던가 다른방식으로 동작 구현해야함
                //if (mainScrollText != null)
                //    mainScrollText.text = string.Format(mainScrollText.text + " ({0}/{1})", 0, GameConfigTable.GetConfigIntValue("FRIEND_MAX_NUM", 30));
                //if (subScrollText != null)
                //    subScrollText.text = string.Format(subScrollText.text + " ({0}/{1})", count, GameConfigTable.GetConfigIntValue("FRIEND_MAX_NUM", 30));
                //친구 신청 보낸 리스트
                //MainScrollCreateItem(callback, false, false, true);
                //친구 신청 받은 리스트
                //SubScrollCreateItem(callback, false, true, false);
                //2024-03-25 일단 메인리스트에 받은 신청내역만 나오도록 적용.
                receiveInfos = callback;
                if (mainScrollText != null)
                    mainScrollText.text = string.Format(mainScrollText.text + " ({0}/{1})", count, GameConfigTable.GetConfigIntValue("FRIEND_MAX_NUM", 30));
                if (count > 0)
                    MainScrollCreateItem(receiveInfos, eOtherNodeType.FRIEND_ACCEPT);
            }, (body) => networkEnable = false);
        }
        public void OnClickSuggestion(bool isRefresh)
        {
            noFriendAlertNode.SetActive(false);

            if (networkEnable)
                return;

            if (CurType != eFriendListType.SUGGESTION)
            {
                CurType = eFriendListType.SUGGESTION;

                InitInputField();
                SetTopButtonEnable(true, false, false);
                SetBotButtonEnable(false, true);
                networkEnable = true;
                lastSearch = "";
                SetScrollName();
                SetSubScroll(false);
            }
            ReddotManager.Set(eReddotEvent.FRIEND, false);

            if (isRefresh)
            {
                FriendManager.Instance.FriendReFreshRecommendList((callback) =>
                {
                    networkEnable = false;
                    if (callback == null || callback.Count < 1)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("errorcode_1417"));
                        return;
                    }
                    recommendInfos = callback;
                    MainScrollCreateItem(recommendInfos, eOtherNodeType.FRIEND_RECOMMEND);
                }, (body) => networkEnable = false);
            }
            else
            {
                if(recommendInfos.Count < 1)
                {
                    FriendManager.Instance.FriendRecommendList((callback) =>
                    {
                        networkEnable = false;
                        if (callback == null || callback.Count < 1)
                        {
                            ToastManager.On(StringData.GetStringByStrKey("errorcode_1417"));
                            return;
                        }
                        recommendInfos = callback;
                        MainScrollCreateItem(recommendInfos, eOtherNodeType.FRIEND_RECOMMEND);
                    }, (body) => networkEnable = false);
                }
                else
                {
                    MainScrollCreateItem(recommendInfos, eOtherNodeType.FRIEND_RECOMMEND);
                    networkEnable = false;
                }
            }
        }
        /// <summary> 하트 전체 받기 & 하트 전체 보내기 </summary>
        /// <param name="op_code">1 => 보내기, 2 => 받기</param>
        public void OnClickFriendlyPoint(int op_code)
        {
            networkEnable = true;
            FriendManager.SendFriendlyPoint(op_code, (res) =>
            {
                if (op_code == 1)
                {
                    ToastManager.On(100002560);
                    GiftEvent.SendGift();
                }
                else
                {
                    ToastManager.On(100002559);
                    GiftEvent.AcceptGift();
                }
                    

                networkEnable = false;
                RefreshFriendlyHeartBtn();
            }, (body) => networkEnable = false);
        }
        public void OnClickFriendShopPopup()//친구 상점 오픈 요청
        {
            PopupManager.OpenPopup<FriendPointShopPopup>();
        }
        void RefreshFriendlyHeartBtn()
        {
            if (friendlySendAllBtn != null)
            {
                var sendState = IsSendFriendButtonCondition();
                friendlySendAllBtn.SetButtonSpriteState(sendState);
                friendlySendAllBtn.interactable = sendState;
            }

            if (friendlyRecvAllBtn != null)
            {
                var recvState = IsReceiveFriendButtonCondition();
                friendlyRecvAllBtn.SetButtonSpriteState(recvState);
                friendlyRecvAllBtn.interactable = recvState;
            }
        }
        bool IsSendFriendButtonCondition()//보낼 건수가 하나라도 있으면 true
        {
            var tempSearchInfos = FriendManager.FriendIdList.Values;
            if (tempSearchInfos == null || tempSearchInfos.Count <= 0)
                return false;

            var check = false;
            foreach (var infoData in tempSearchInfos)
            {
                if (infoData.SendGiftPoint == 0)
                {
                    check = true;
                    break;
                }
            }

            var todayCount = FriendManager.Instance.TodaySendCount;//오늘 하루 보낸 수량
            var availableToday = todayCount < GameConfigTable.GetSendFriendDailyGiftMax();//30 이상이면 불가능

            return check && availableToday;
        }
        bool IsReceiveFriendButtonCondition()//유저의 포인트 점수 값 0=> 기본 , 1 보냄 2 받음
        {
            var tempSearchInfos = FriendManager.FriendIdList.Values;
            if (tempSearchInfos == null || tempSearchInfos.Count <= 0)
                return false;

            var check = false;
            foreach (var infoData in tempSearchInfos)
            {
                if (infoData.ReceiveGiftPoint == 1)
                {
                    check = true;
                    break;
                }
            }
            return check;
        }
        public void OnClickSuggestionSearch()
        {
            if (networkEnable)
                return;

            networkEnable = true;
            OnSearch(searchEditBox.text);
        }

        void OnSearch(string _text)
        {
            _text = _text.Trim();//공란 체크
            if (string.IsNullOrEmpty(_text))
            {
                InitInputField();
                ToastManager.On(100002128);
                networkEnable = false;
                return;
            }

            if (_text == User.Instance.UserData.UserNick)
            {
                InitInputField();
                ToastManager.On(100010007);
                networkEnable = false;
                return;
            }

            if (lastSearch == string.Empty || lastSearch != _text)
                lastSearch = _text;
            else
            {
                networkEnable = false;
                return;
            }

            InitInputField();
            FriendManager.Instance.FriendSearch(_text, (resultList) =>
            {
                networkEnable = false;
                // 검색 할 수 없는 유저 처리
                if (resultList == null || resultList.Count <= 0)
                {
                    ToastManager.On(100000766);
                    return;
                }
                SubScrollCreateItem(resultList, eOtherNodeType.FRIEND_RECOMMEND);
            }, (body) => {
                ToastManager.On(100000766);
                networkEnable = false; 
            });
        }

        void InitInputField()//인풋박스 초기화
        {
            searchEditBox.text = "";//입력 데이터 초기화
            searchEditBox.placeholder.GetComponent<Text>().enabled = true;
        }
        void SetFriends()
        {
            if (searchInfos == null || searchInfos.Count == 0)
            {
                ClearNode();
                noFriendAlertNode.gameObject.SetActive(true);
                return;
            }
            noFriendAlertNode.gameObject.SetActive(false);
            // 검색 할 수 없는 유저 처리
            if (searchInfos == null || searchInfos.Count <= 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("errorcode_1415"));
                return;
            }

            var curDic = ChatManager.Instance.OneOnOneLastChatID;
            searchInfos.Sort((data1, data2) =>
            {
                long sort1 = 0;
                long sort2 = 0;
                if (curDic == null || (false == curDic.TryGetValue(data1.UID, out sort1) && false == curDic.TryGetValue(data2.UID, out sort2)))
                {
                    if (data1.LastActiveTime > data2.LastActiveTime)
                        return -1;
                    else if (data1.LastActiveTime < data2.LastActiveTime)
                        return 1;
                    else
                        return 0;
                }
                else
                {
                    if (sort1 > sort2)
                        return -1;
                    else if (sort1 < sort2)
                        return 1;
                    else
                    {
                        if (data1.LastActiveTime > data2.LastActiveTime)
                            return -1;
                        else if (data1.LastActiveTime < data2.LastActiveTime)
                            return 1;
                        else
                            return 0;
                    }
                }
            });

            MainScrollCreateItem(searchInfos, eOtherNodeType.FRIEND);
            RefreshFriendlyHeartBtn();
        }
        void MainScrollCreateItem(List<FriendUserData> infos, eOtherNodeType type)
        {
            ClearNode();
            SetSubScroll(false);
            //friendNodes
            for (int i = 0, count = infos.Count; i < count; ++i)
            {
                if (mainScrollNodes.Count <= i)
                {
                    var obj = Instantiate(friendNode, mainScrollParent);
                    mainScrollNodes.Add(obj);
                }

                mainScrollNodes[i].gameObject.SetActive(true);
                mainScrollNodes[i].SendGift = infos[i].SendGiftPoint;
                mainScrollNodes[i].ReceiveGift = infos[i].ReceiveGiftPoint;
                mainScrollNodes[i].Init(infos[i], type);
                mainScrollNodes[i].SetReturnTab(LayerIndex);
                mainScrollNodes[i].SetRemoveCallBack(RemoveItem);
            }
        }
        void SubScrollCreateItem(List<FriendUserData> infos, eOtherNodeType type)
        {
            SetSubScroll(true);
            //friendNodes
            for (int i = 0, count = infos.Count; i < count; ++i)
            {
                if (subScrollNodes.Count <= i)
                {
                    var obj = Instantiate(friendNode, subScrollParent);
                    subScrollNodes.Add(obj);
                }

                subScrollNodes[i].gameObject.SetActive(true);
                subScrollNodes[i].Init(infos[i], type);
                subScrollNodes[i].SetReturnTab(LayerIndex);
                subScrollNodes[i].SetRemoveCallBack(RemoveSubScroll);
            }
        }
        void RemoveSubScroll(long UID)
        {
            SetSubScroll(false);
        }
        void SetSubScroll(bool isActive)
        {
            if (subScrollNode != null)
                subScrollNode.SetActive(isActive);
        }
        void SetScrollName()
        {
            switch (CurType)
            {
                case eFriendListType.SUGGESTION:
                    if (mainScrollText != null)
                        mainScrollText.text = StringData.GetStringByIndex(100000479);
                    if (subScrollText != null)
                        subScrollText.text = StringData.GetStringByStrKey("chat_friend_search_result");

                    SetTitleText(StringData.GetStringByIndex(100000479));
                    break;
                case eFriendListType.ADDED:
                    //if (mainScrollText != null)
                    //    mainScrollText.text = StringData.GetStringByIndex(100000478);
                    if (subScrollText != null)
                        subScrollText.text = StringData.GetStringByIndex(100000477);
                    //2024-03-25 일단 메인리스트에 받은 신청내역만 나오도록 적용.
                    if (mainScrollText != null)
                        mainScrollText.text = StringData.GetStringByIndex(100000477);

                    SetTitleText(StringData.GetStringByStrKey("guild_desc:11"));
                    break;
                case eFriendListType.NONE:
                case eFriendListType.FRIEND:
                default:
                    if (mainScrollText != null)
                        mainScrollText.text = StringData.GetStringByIndex(100000476);
                    if (subScrollText != null)
                        subScrollText.text = "";

                    if (searchInfos == null || searchInfos.Count < 1)
                        SetTitleText(StringData.GetStringFormatByStrKey("친구사람수", 0, GameConfigTable.GetConfigIntValue("FRIEND_MAX_NUM")));
                    else
                        SetTitleText(StringData.GetStringFormatByStrKey("친구사람수", searchInfos.Count, GameConfigTable.GetConfigIntValue("FRIEND_MAX_NUM")));
                    break;
            }
        }
        void SetTitleText(string leftText)
        {
            if (titleText != null)
                titleText.text = leftText;
        }
    }
}