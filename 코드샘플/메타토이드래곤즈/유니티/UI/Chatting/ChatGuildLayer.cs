using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum eChatGuildState
    {
        NONE,
        CHAT,
        MEMBER
    }
    public class ChatGuildLayer : ChatLayerBase, EventListener<ChatEvent>
    {
        [Header("Node")]
        [SerializeField] GameObject chatObj = null;
        [SerializeField] GameObject memberObj = null;

        [Header("ScrollView")]
        [SerializeField] ChatScrollview chatScrollView = null;
        [SerializeField] ScrollRect memberScrollView = null;

        [Space(10f)]
        [Header("Prefab")]
        [SerializeField] ChattingOtherNode memberNode = null;

        [Space(10f)]
        [Header("Top")]
        [SerializeField] Button listButton = null;
        [SerializeField] Button backButton = null;

        [Space(10f)]
        [Header("Bot")]
        [SerializeField] Button blockButton = null;
        [SerializeField] TimeObject timeObj = null;
        [SerializeField] Text timeText = null;

        private int chatDelayTimeStamp = 0;//유저가 마지막으로 입력한 당시(채팅 반영시간)의 시간 값을 들고있어야함 - 발송 시간 기준 + 딜레이 추가 값
        private eChatGuildState state = eChatGuildState.NONE;
        private List<ChattingOtherNode> nodes = new();

        private void OnEnable()
        {
            EventManager.AddListener(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }
        public void OnEvent(ChatEvent eventType)
        {
            switch (eventType.Event)
            {
                case ChatEvent.eChatEventEnum.SendMacro:
                    break;
                case ChatEvent.eChatEventEnum.RefreshUI:
                    RefreshUI();
                    break;
            }
        }
        public override void InitUI(TabTypePopupData datas = null)//ChangeTab
        {
            chatScrollView?.Initialize();
            base.InitUI(datas);

            state = eChatGuildState.NONE;
            InitializeStateUI(eChatGuildState.CHAT);
        }
        public override void InitChatLayer()
        {
            ReddotManager.Set(eReddotEvent.GUILD_CHAT, false);
            base.InitChatLayer();

            chatScrollView?.SetVisibleNewMsgButton(false);//일단 신규 메세지 버튼 끔
            RefreshChatDelayTime();

            chatScrollView?.RequestRefreshScrollview(chatLayerType);
            chatScrollView?.RequestScrollviewGotoBottom();
        }
        public void InitGuildMember()
        {
            if (GuildManager.Instance.IsNoneGuild)
                return;

            ScrollCreateItem(GuildManager.Instance.MyGuildInfo.GuildUserList.FindAll(info => info != null && info.UID != User.Instance.UserAccountData.UserNumber));
        }

        public override void RefreshUI()//ForceUpdate();
        {
            base.RefreshUI();
            InitializeStateUI(state);
        }
        public void OnClickBack()
        {
            InitializeStateUI(eChatGuildState.CHAT);
        }
        public void OnClickMember()
        {
            ReddotManager.Set(eReddotEvent.GUILD_WISPER, false);
            InitializeStateUI(eChatGuildState.MEMBER);
        }
        public void InitializeStateUI(eChatGuildState next)
        {
            switch (next)
            {
                case eChatGuildState.MEMBER:
                {
                    if(state != next)
                    {
                        if (chatObj != null)
                            chatObj.SetActive(false);
                        if (listButton != null)
                            listButton.gameObject.SetActive(false);

                        if (memberObj != null)
                            memberObj.SetActive(true);
                        if (backButton != null)
                            backButton.gameObject.SetActive(true);
                    }

                    InitGuildMember();
                } break;
                case eChatGuildState.CHAT:
                default:
                {
                    if (state != next)
                    {
                        if (chatObj != null)
                            chatObj.SetActive(true);
                        if (listButton != null)
                            listButton.gameObject.SetActive(true);

                        if (memberObj != null)
                            memberObj.SetActive(false);
                        if (backButton != null)
                            backButton.gameObject.SetActive(false);
                    }

                    InitChatLayer();
                } break;
            }
            state = next;
        }
        public void OnClickSendMessage()//전송 버튼 누를 때
        {
            if (string.IsNullOrEmpty(chatEditBox.text))
            {
                ToastManager.On(100002128);
                return;
            }

            OnChatSubmit(chatLayerType);

            chatScrollView?.RequestScrollviewGotoBottom(true);
            SendMsgAndSettingDelayProcess();
        }
        void SendMsgAndSettingDelayProcess()
        {
            //임시 타임값 세팅 + 시간 제어 갱신
            chatDelayTimeStamp = TimeManager.GetTime() + SBDefine.CHAT_TIME_UI_DELAY;
            RefreshChatDelayTime();
            chatScrollView?.RequestScrollviewGotoBottom(true);
        }

        void RefreshChatDelayTime()
        {
            if (chatDelayTimeStamp <= 0)
            {
                if (blockButton != null)
                    blockButton.gameObject.SetActive(false);
                return;
            }

            if (timeObj != null && chatDelayTimeStamp - TimeManager.GetTime() > 0)
            {
                if (blockButton != null)
                    blockButton.gameObject.SetActive(true);

                timeObj.Refresh = delegate
                {
                    float remain = chatDelayTimeStamp - TimeManager.GetTime();
                    timeObj.GetComponent<Text>().text = string.Format(StringData.GetStringByIndex(100002435), remain); //SBFunc.StrBuilder(remain, "초뒤 입력 가능");

                    if (remain <= 0)
                    {
                        if (blockButton != null)
                            blockButton.gameObject.SetActive(false);

                        timeObj.Refresh = null;
                        chatDelayTimeStamp = 0;
                    }
                };
            }
        }
        void ClearNode()
        {
            foreach (var node in nodes)
            {
                node.gameObject.SetActive(false);
            }
        }
        void ScrollCreateItem(List<GuildUserData> infos)
        {
            ClearNode();
            if(infos.Count > 1)
            {
                var curDic = ChatManager.Instance.OneOnOneLastChatID;
                infos.Sort((data1, data2) =>
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
            }
            for (int i = 0, count = infos.Count; i < count; ++i)
            {
                if (nodes.Count <= i)
                {
                    var obj = Instantiate(memberNode, memberScrollView.content);
                    nodes.Add(obj);
                }

                nodes[i].gameObject.SetActive(true);
                nodes[i].SetReturnTab(LayerIndex);
                nodes[i].Init(infos[i], eOtherNodeType.GUILD);
            }
        }
    }
}