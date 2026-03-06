using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SandboxNetwork
{
    public class ProfileUIObject : UIObject, EventListener<UserStatusEvent>, EventListener<UIObjectEvent>, EventListener<GuildEvent>
    {
        [SerializeField]
        private portraitObject portrait;
        [SerializeField]
        private Text lvText;
        [SerializeField]
        private Text nickNameText;
        [SerializeField]
        private Slider expSlider;
        [SerializeField]
        private Text expText;
        [SerializeField]
        private GameObject reddot;
        [SerializeField]
        private GuildBaseInfoObject guildBaseObj;

        [SerializeField] private Animator pAnimator = null;
        public override void Init()
        {
            base.Init();
            EventManager.AddListener<UserStatusEvent>(this);
            EventManager.AddListener<UIObjectEvent>(this);
            EventManager.AddListener<GuildEvent>(this);
            //RefreshNickAndLv();
            //RefershExp();
        }
        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            if (curSceneType > eUIType.None && curUIType.HasFlag(curSceneType))
            {
                RefreshNickAndLv();
                RefershExp();
                RefreshPortrait();
                RefreshLevelPassReddot();
                RefreshGuild();
            }
        }
        public override bool RefreshUI(eUIType targetType)
        {
            if (base.RefreshUI(targetType)) // 씬타입이 같아서 갱신부
            {
                RefreshNickAndLv();
                RefershExp();
                RefreshPortrait();
                RefreshLevelPassReddot();
                RefreshGuild();
            }
            return curSceneType != targetType;
        }
        void RefreshPortrait()
        {
            portrait.SetProfile();
        }
        void RefreshNickAndLv()
        {
            nickNameText.text = User.Instance.UserData.UserNick;
            lvText.text = string.Format("Lv. {0}", User.Instance.UserData.Level);
        }

        void RefreshLevel()
        {
            lvText.text = string.Format("Lv. {0}", User.Instance.UserData.Level);
        }

        void RefershExp()
        {
            //AccountTable table = TableManager.GetTable<AccountTable>();
            //if (table != null)
            //{
            int myLv = User.Instance.UserData.Level;
            int myExp = User.Instance.UserData.Exp;
            int accountLevelData = AccountData.GetLevel(myLv).TOTAL_EXP; //table.GetByLevel(myLv).TOTAL_EXP;
            int devider = AccountData.GetLevel(myLv).EXP;//table.GetByLevel(myLv).EXP;

            var tb = AccountData.GetLevel(myLv + 1);
            int Need = 0;
            if (tb != null)
                Need = AccountData.GetLevel(myLv + 1).EXP;//table.GetByLevel(myLv + 1).EXP;
            
            expSlider.maxValue = devider;
            expSlider.value = myExp - accountLevelData;
            string expValue = SBFunc.CommaFromNumber(Need - (myExp - accountLevelData));
            expText.text = string.Format("Next {0}", expValue);
            //}
        }

        void RefreshGuild()
        {
            if (GuildManager.Instance.IsNoneGuild)
            {
                guildBaseObj.gameObject.SetActive(false);
            }
            else
            {
                guildBaseObj.gameObject.SetActive(true);
                guildBaseObj.Init(GuildManager.Instance.MyBaseData);
            }
            
        }
        /// <summary>
        /// 레벨 패스 기준으로 보상 컨디션이면 켜기
        /// </summary>
        void RefreshLevelPassReddot()
        {
            if (reddot != null)
                reddot.gameObject.SetActive(BattlePassManager.Instance.IsReddot());
        }

        public void OnClickProfile()
        {
            LevelPassPopup.RequestLevelPassPopup(() =>
            {
                LevelPassPopup.OpenPopup();
            }, () =>
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("서버데이터호출실패"));
            });
        }

        public void OnEvent(UserStatusEvent eventType)
        {
            switch (eventType.Event)
            {
                case UserStatusEvent.eUserStatusEventEnum.EXP:
                    RefershExp();
                    RefreshLevel();
                    break;
                case UserStatusEvent.eUserStatusEventEnum.PORTRAIT:
                    RefreshPortrait();
                    break;
                case UserStatusEvent.eUserStatusEventEnum.LEVEL:
                    RefreshLevelPassReddot();
                    break;
            }
        }
        public void OnEvent(UIObjectEvent eventType)
        {
            if ((eventType.t & UIObjectEvent.eUITarget.LT) != UIObjectEvent.eUITarget.NONE)
            {
                switch (eventType.e)
                {
                    case UIObjectEvent.eEvent.EVENT_SHOW:
                        if (pAnimator != null)
                        {
                            pAnimator.SetBool("FOLD", false);
                            pAnimator.SetTrigger("SHOW");
                        }
                        break;
                    case UIObjectEvent.eEvent.EVENT_HIDE:
                        if (pAnimator != null)
                        {
                            pAnimator.SetBool("SHOW", false);
                            pAnimator.SetTrigger("FOLD");
                        }
                        break;
                }
            }
        }

        public void OnEvent(GuildEvent eventType)
        {
            switch (eventType.Event)
            {
                case GuildEvent.eGuildEventType.GuildRefresh:
                    RefreshGuild();
                    break;
                case GuildEvent.eGuildEventType.LostGuild:
                    guildBaseObj.gameObject.SetActive(false);
                    break;
            }
            
        }
    }
}