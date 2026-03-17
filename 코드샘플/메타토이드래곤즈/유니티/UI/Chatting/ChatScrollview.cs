using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    [System.Serializable]
    public enum eChatUIType
    {
        None,
        Me,
        Other,
        Day,
        System,
        Warning,
    }

    public class ChatUIItem
    {
        public eChatUIType chatUIType = eChatUIType.None;
        public ChatDataInfo chatInfo = null;
        public ChatPrefab ui = null;
        public long time = 0;
        public void SetData(eChatUIType ui_type, ChatDataInfo info)
        {
            chatUIType = ui_type;
            chatInfo = info;
        }

        public void SetData(long time)
        {
            chatUIType = eChatUIType.Day;
            this.time = time;
        }

        /// <summary>
        /// 채팅 데이터에 따른 eChatUIType 정리
        /// 1. 데이터의 send == 내 UNO 와 같으면 내 채팅
        /// 2. 보낸 snedUID == 월드 채팅 비트타입과 같으면 시스템 채팅
        /// 3. sendUID 와 UNO가 다르면 다른 사람의 채팅
        /// </summary>
        /// <param name="info"></param>
        public void SetData(ChatDataInfo info)
        {
            if (info.CommentType == eChatCommentType.SystemMsg)
                chatUIType = eChatUIType.System;
            else if (info.SendUID == User.Instance.UserAccountData.UserNumber)
                chatUIType = eChatUIType.Me;
            else
                chatUIType = eChatUIType.Other;

            chatInfo = info;
            time = chatInfo.Time;
        }

        public void SetUI(ChatPrefab ui)
        {
            this.ui = ui;
        }
        public void Clear()
        {
            chatUIType = eChatUIType.None;
            chatInfo = null;
            ui = null;
            time = 0;
        }
    }

    public class ChatScrollview : MonoBehaviour
    {
        [SerializeField] protected GameObject myChatPrefab = null;
        [SerializeField] protected GameObject otherChatPrefab = null;
        [SerializeField] protected GameObject dayChatPrefab = null;
        [SerializeField] protected GameObject systemChatPrefab = null;
        [SerializeField] protected GameObject warningChatPrefab = null;
        [SerializeField] protected GameObject scrollviewContent = null;
        [SerializeField] protected Button newMsgButton = null;

        //scroll Data List
        [SerializeField] protected List<ChatUIItem> ChatDataList = new List<ChatUIItem>();
        [SerializeField] protected SBListPool<ChatUIItem> ChatUIItemPool = new SBListPool<ChatUIItem>();

        protected eChatCommentType curType = eChatCommentType.None;
        protected long targetUID = -1;
        protected ScrollRect targetScroll = null;
        protected Queue<ChatDataInfo> CurQueue
        {
            get
            {
                switch (curType)
                {
                    case eChatCommentType.SystemMsg:
                        return ChatManager.Instance.ChatQueueSystem;
                    case eChatCommentType.Whisper:
                    {
                        if(ChatManager.Instance.OneOnOneChatDataDic.TryGetValue(targetUID, out var list))
                        {
                            return list;
                        }
                        return null;
                    }
                    case eChatCommentType.Guild:
                        return ChatManager.Instance.ChatQueueGuild;
                    default:
                        return ChatManager.Instance.ChatQueueWorld;
                }
            }
        }
        private bool isInit = false;
        private bool isFirst = false;
        protected SBTypePool<eChatUIType, GameObject> TypePool = null;

        public float NewMessageCheckValue { get; } = 0.03f; // 스크롤 최하단으로 포커싱 하는 기준값

        public virtual void Initialize()
        {
            if (isInit)
                return;

            isInit = true;
            isFirst = true;
            TypePool = new((obj) =>
            {
                obj.SetActive(true);
                obj.transform.SetAsLastSibling();
            }, (obj) => obj.SetActive(false));
            TypePool.InitializeTransform(scrollviewContent.transform);
            TypePool.InitializeType(eChatUIType.Me, myChatPrefab);
            TypePool.InitializeType(eChatUIType.Other, otherChatPrefab);
            TypePool.InitializeType(eChatUIType.Day, dayChatPrefab);
            TypePool.InitializeType(eChatUIType.System, systemChatPrefab);
            TypePool.InitializeType(eChatUIType.Warning, warningChatPrefab);
        }

        public void RefreshScrollview(eChatCommentType type)
        {
            if (targetScroll == null)
                targetScroll = GetComponent<ScrollRect>();

            if (scrollviewContent != null)
            {
                DataRefresh(type);
            }
        }

        public virtual void DataRefresh(eChatCommentType type)
        {
            curType = type;

            var isBot = isFirst || GetCurrentScrollPosition() < NewMessageCheckValue;
            ClearScrollView();

            switch (type)
            {
                case eChatCommentType.World:
                case eChatCommentType.Guild:
                case eChatCommentType.Whisper:
                    var warningItem = MakeFirstAddWarningItem();
                    warningItem.SetUI(AddChatScrollItem(warningItem));
                    ChatDataList.Add(warningItem);
                    break;
                default:
                    break;
            }

            if (CurQueue.Count > 0)
            {
                var lastDay = -1;
                foreach (var item in CurQueue)
                {
                    if (ChatManager.Instance.GetBlockUserData(item.SendUID) != null)
                        continue;

                    var date = TimeManager.GetCustomDateTime(item.Time);
                    if (lastDay < 0 || lastDay != date.Day)
                    {
                        var timeItem = MakeFirstAddDateItem(item.Time);
                        timeItem.SetUI(AddChatScrollItem(timeItem));
                        ChatDataList.Add(timeItem);
                        lastDay = date.Day;
                    }

                    ChatUIItemPool.Spawn(1);
                    ChatUIItem chatUI = ChatUIItemPool.Get();
                    chatUI.SetData(item);
                    ChatDataList.Add(chatUI);
                    chatUI.SetUI(AddChatScrollItem(chatUI));
                }
            }
            else
            {
                var timeItem = MakeFirstAddDateItem();
                timeItem.SetUI(AddChatScrollItem(timeItem));
                ChatDataList.Add(timeItem);
            }

            if (ChatDataList == null || ChatDataList.Count <= 0)
                return;

            if (ChatDataList.Count > ChatManager.CHAT_QUEUE_MAX_SIZE)
            {
                int delCount = ChatDataList.Count - ChatManager.CHAT_QUEUE_MAX_SIZE;
                int curIndex = 0;
                while (delCount > 0)
                {
                    if (ChatDataList[curIndex] != null && ChatDataList[curIndex].chatUIType != eChatUIType.Warning)
                    {
                        if (false == TypePool.Put(ChatDataList[curIndex].chatUIType, ChatDataList[curIndex].ui.gameObject))
                            Destroy(ChatDataList[curIndex].ui.gameObject);

                        delCount--;
                        ChatDataList.RemoveRange(curIndex, 1);
                    }
                    else
                        curIndex++;
                }
            }
            var rect = scrollviewContent.GetComponent<RectTransform>();
            if (rect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                
                if (isBot)
                {
                    GotoScrollBottom();
                    isFirst = false;
                }
            }
        }

        protected ChatUIItem MakeFirstAddDateItem(long timeStamp = -1)//월드 채팅 같은 경우(새로 클라켜진 시점으로) 첫 시간은 항상 오늘 날짜로 새로 찍힘
        {
            ChatUIItemPool.Spawn(1);

            ChatUIItem tempInfo = ChatUIItemPool.Get();
            if (timeStamp < 0)
                timeStamp = TimeManager.GetTime();
            tempInfo.SetData(timeStamp);

            return tempInfo;
        }
        protected ChatUIItem MakeFirstAddWarningItem()//채팅 권고 문구 추가
        {
            ChatUIItemPool.Spawn(1);

            ChatUIItem tempInfo = ChatUIItemPool.Get();
            tempInfo.SetData(eChatUIType.Warning, null);

            return tempInfo;
        }

        public ChatPrefab AddChatScrollItem(ChatUIItem _itemData)
        {
            var chatUIType = _itemData.chatUIType;
            var prefabData = GetChatPrefabByType(chatUIType);
            if (prefabData == null)
                return null;

            var go = prefabData;
            go.SetActive(true);
            var chatInfoGo = go.GetComponent<ChatPrefab>();
            if (chatInfoGo == null)
            {
                Destroy(go);
                return null;
            }

            switch (chatUIType)
            {
                case eChatUIType.Me:
                case eChatUIType.Other:
                    chatInfoGo.Init(_itemData);
                    break;
                case eChatUIType.Day:
                    chatInfoGo.Init(chatUIType, _itemData.time.ToString());
                    break;
                case eChatUIType.System:
                    chatInfoGo.Init(_itemData);
                    break;
                case eChatUIType.Warning:
                    chatInfoGo.Init(chatUIType, StringData.GetStringByStrKey("채팅권고문구"));
                    break;

            }

            return chatInfoGo;
        }

        public void ClearScrollView()
        {
            foreach(var item in ChatDataList)
            {
                if (item == null)
                    continue;

                TypePool.Put(item.chatUIType, item.ui.gameObject);
                item.Clear();
                ChatUIItemPool.Put(item);
            }
            ChatDataList.Clear();
        }

        GameObject GetChatPrefabByType(eChatUIType _type)
        {
            return TypePool.Get(_type);
        }

        public void GotoScrollBottom(Action action = null)
        {
            if (targetScroll != null)
                //targetScroll.DOVerticalNormalizedPos(0, productionTime);
                targetScroll.verticalNormalizedPosition = 0;

            action?.Invoke();
        }

        public float GetCurrentScrollPosition()
        {
            if (targetScroll == null)
                return -1f;

            return targetScroll.verticalNormalizedPosition;
        }

        public void OnChatScrollBarMove()
        {
            if (newMsgButton != null && newMsgButton.gameObject.activeSelf)
            {
                var currentVerticalPos = GetCurrentScrollPosition();
                SetVisibleNewMsgButton(currentVerticalPos > NewMessageCheckValue);
            }
        }

        public void OnClickCheckNewMsg()//최근 메세지 확인 - 스크롤 바닥으로 이동
        {
            GotoScrollBottom(() =>
            {
                SetVisibleNewMsgButton(false);
            });
        }

        public void RequestRefreshScrollview(eChatCommentType chatType)
        {
            RefreshScrollview(chatType);
        }

        public void RequestScrollviewGotoBottom(bool isForceMoveToBottom = false)
        {
            // 스크롤이 어느정도 하단영역에 위치해야만 자동으로 최하단 포커싱
            if (isForceMoveToBottom || (GetCurrentScrollPosition() < NewMessageCheckValue))
            {
                GotoScrollBottom();
            }
        }

        public void SetVisibleNewMsgButton(bool _isVisible)
        {
            if (newMsgButton == null) return;

            newMsgButton.gameObject.SetActive(_isVisible);
        }

        public void CheckNewMessageButton()//현재 스크롤링한 포지션이 바닥이 아닐때 체크
        {
            var currentVerticalPos = GetCurrentScrollPosition();
            if (currentVerticalPos > 0)
            {
                SetVisibleNewMsgButton(currentVerticalPos > NewMessageCheckValue);
            }
        }
    }
}
