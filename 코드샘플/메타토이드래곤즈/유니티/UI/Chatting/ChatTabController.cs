using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChatTabController : TabController, EventListener<GuildEvent>
    {
        [SerializeField] ChattingController chattingController = null;
        [SerializeField] GameObject guildBtn = null;
        [SerializeField] GameObject blockBtn = null;

        public ChatUserLayer chatLayer = null;
        private int returnTab = -1;

        private void OnEnable()
        {
            EventManager.AddListener<GuildEvent>(this);
            if (guildBtn != null)
                guildBtn.SetActive(false == GuildManager.Instance.IsNoneGuild);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<GuildEvent>(this);
            CurTab = -1;
            returnTab = -1;
        }

        public void InitController()
        {
            InitUI();
        }

        private bool Check(int index)
        {
            if (CurTab == index)
                return true;

            if (chattingController != null)
            {
                chattingController.InitBlockUI();
                chattingController.InitMacroUI();
            }

            //if (index == 2 )
            //{
            //    ToastManager.On(100002672);
            //    return true;
            //}

            chatLayer.gameObject.SetActive(false);

            return false;
        }

        public void OnClickBackToChatRoom()
        {
            chatLayer.gameObject.SetActive(false);
            if (returnTab < 0)
            {
                InitTab(defaultTab);
            }
            else
            {
                InitTab(returnTab);
                returnTab = -1;
            }
        }

        public override void InitTab(int index, TabTypePopupData datas = null)
        {
            if (!Check(index))
            {
                base.InitTab(index, datas);
            }
        }
        public override void ChangeTab(int index, TabTypePopupData datas = null)
        {
            if (!Check(index))
            {
                base.ChangeTab(index, datas);
            }
        }
        public void SetReturnTab(int returnTab)
        {
            this.returnTab = returnTab;
        }
        public void RefreshTitle()
        {
            SetTitleText();
        }

        public void OnEvent(GuildEvent eventType)
        {
            switch (eventType.Event)
            {
                case GuildEvent.eGuildEventType.GuildRefresh:
                    break;
                case GuildEvent.eGuildEventType.LostGuild:
                    if (guildBtn != null)
                        guildBtn.SetActive(false == GuildManager.Instance.IsNoneGuild);
                    break;
            }

        }

    }
}
