using UnityEngine;

namespace SandboxNetwork
{
    public class ChatSystemLayer : ChatLayerBase, EventListener<ChatEvent>
    {
        [SerializeField] ChatScrollview scrollview = null;

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
                    ToastManager.On(100002539);
                    break;
                case ChatEvent.eChatEventEnum.RefreshUI:
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

            scrollview?.RequestRefreshScrollview(chatLayerType);
            scrollview?.RequestScrollviewGotoBottom();
        }

        public override void RefreshUI()//ForceUpdate();
        {
            base.RefreshUI();

            RefreshChatLayer();
        }

        public override void RefreshChatLayer()
        {
            base.RefreshChatLayer();

            scrollview?.RequestRefreshScrollview(chatLayerType);
        }
    }
}
