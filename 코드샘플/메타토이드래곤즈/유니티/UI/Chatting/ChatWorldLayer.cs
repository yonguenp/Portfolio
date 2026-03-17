using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChatWorldLayer : ChatLayerBase, EventListener<ChatEvent>
    {
        [SerializeField] ChatScrollview scrollview = null;
        
        [SerializeField] Button blockButton = null;
        [SerializeField] TimeObject timeObj = null;

        private int worldChatDelayTimeStamp = 0;//유저가 마지막으로 입력한 당시(채팅 반영시간)의 시간 값을 들고있어야함 - 발송 시간 기준 + 딜레이 추가 값
        private bool isChatDelay = false;

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
                    var currentComment = eventType.comment;
                    if (isChatDelay)
                    {
                        ToastManager.On(100002540);
                        return;
                    }

                    SendMsgAndSettingDelayProcess();
                    break;
                case ChatEvent.eChatEventEnum.RefreshUI:
                    //to do - 신규 메세지 데이터 가져와서 스크롤 뷰 아이템 등록
                    scrollview?.RequestRefreshScrollview(chatLayerType);
                    scrollview?.CheckNewMessageButton();//스크롤이 바닥이 아닐 때 신규 메세지 확인 버튼 출력
                    break;
            }
        }
        public override void InitUI(TabTypePopupData datas = null)//ChangeTab
        {
            scrollview?.Initialize();
            base.InitUI(datas);

            InitChatLayer();
        }

        public override void InitChatLayer()
        {
            base.InitChatLayer();

            scrollview?.SetVisibleNewMsgButton(false);//일단 신규 메세지 버튼 끔
            RefreshChatDelayTime();

            scrollview?.RequestRefreshScrollview(chatLayerType);
            scrollview?.RequestScrollviewGotoBottom();
        }

        public override void RefreshUI()
        {
            base.RefreshUI();

            RefreshChatLayer();
        }

        public override void RefreshChatLayer()
        {
            base.RefreshChatLayer();

            scrollview?.RequestRefreshScrollview(chatLayerType);
        }

        public void OnClickSendMessage()//전송 버튼 누를 때
        {
            if (string.IsNullOrEmpty(chatEditBox.text))
            {
                ToastManager.On(100002128);
                return;
            }

            OnChatSubmit(chatLayerType);

            scrollview?.RequestScrollviewGotoBottom(true);
            SendMsgAndSettingDelayProcess();
        }

        void RefreshChatDelayTime()
        {
            if (worldChatDelayTimeStamp <= 0)
            {
                if (blockButton != null)
                    blockButton.gameObject.SetActive(false);
                isChatDelay = false;
                return;
            }

            if (timeObj != null && worldChatDelayTimeStamp - TimeManager.GetTime() > 0)
            {
                if (blockButton != null)
                    blockButton.gameObject.SetActive(true);

                isChatDelay = true;
                timeObj.Refresh = delegate
                {
                    float remain = worldChatDelayTimeStamp - TimeManager.GetTime();
                    timeObj.GetComponent<Text>().text = string.Format(StringData.GetStringByIndex(100002435), remain); //SBFunc.StrBuilder(remain, "초뒤 입력 가능");

                    if (remain <= 0)
                    {
                        if (blockButton != null)
                            blockButton.gameObject.SetActive(false);

                        timeObj.Refresh = null;
                        isChatDelay = false;
                        worldChatDelayTimeStamp = 0;
                    }
                };
            }
        }

        void SendMsgAndSettingDelayProcess()
        {
            //임시 타임값 세팅 + 시간 제어 갱신
            worldChatDelayTimeStamp = TimeManager.GetTime() + SBDefine.CHAT_TIME_UI_DELAY;
            RefreshChatDelayTime();
            scrollview?.RequestScrollviewGotoBottom(true);
        }

#if DEBUG
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                OnClickSendMessage();
            }
        }
#endif
    }
}