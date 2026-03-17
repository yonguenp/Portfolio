using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class HamburgerUIController : UIObject, EventListener<UIObjectEvent>
    {
        [SerializeField]
        private GameObject hamburgerUiObj;

        [SerializeField]
        private Animator animController;

        [SerializeField]
        private GameObject topmenuUIObj;

        [SerializeField]
        private GameObject rankUIObj = null;
        [SerializeField]
        private GameObject NaverLoungeButton = null;
        public override void Init()
        {
            base.Init();
            EventManager.AddListener(this);
            hamburgerUiObj.SetActive(false);
            topmenuUIObj.SetActive(false);
        }
        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            if (curSceneType > eUIType.None && curUIType.HasFlag(curSceneType))
            {
                if (User.Instance.PrefData == null || User.Instance.PrefData.ArenaFormationData == null)
                {
                    rankUIObj.SetActive(false);
                    return;
                }

                rankUIObj.SetActive(User.Instance.PrefData.ArenaFormationData.IsRegistDragonTeam());
            }
        }
        public void OnEvent(UIObjectEvent eventType)
        {
            if ((eventType.t & UIObjectEvent.eUITarget.HAMBURGER) != UIObjectEvent.eUITarget.NONE)
            {
                switch (eventType.e)
                {
                    case UIObjectEvent.eEvent.EVENT_SHOW:
                        OnShowHamburger();
                        break;

                    case UIObjectEvent.eEvent.EVENT_HIDE:
                        OnHideHamburger();
                        break;
                }
            }
        }
        void OnShowHamburger()
        {   
            if(NaverLoungeButton != null)
            {
                NaverLoungeButton.SetActive(GamePreference.Instance.GameLanguage == SystemLanguage.Korean);
            }

            animController.Play("HamburgerShow");            
            topmenuUIObj.SetActive(false);
            Invoke("ShowTopMenu", 0.5f);

            UICanvas.Instance.StartBackgroundBlurEffect();
        }
        void ShowTopMenu()
        {
            topmenuUIObj.SetActive(true);
        }

        public void OnHideHamburger()
        {
            animController.Play("HamburgerHide");
            topmenuUIObj.SetActive(false);

            UICanvas.Instance.EndBackgroundBlurEffect();
        }

        public void OnClickNotification()
        {
            OnHideHamburger();
            PopupManager.OpenPopup<AnnouncePopup>();            
        }

        public void OnClickChatting()
        {
            OnHideHamburger();
            PopupManager.OpenPopup<ChattingPopup>();
        }

        public void OnClickPost()
        {
            OnHideHamburger();
            PopupManager.OpenPopup<PostListPopup>();            
        }

        public void OnClickRank()
        {
            if (User.Instance.DragonData.GetAllUserDragons().Count <= 0)
            {
                ToastManager.On(100000623);
                return;
            }
            ArenaManager.Instance.ReqArenaData(null, null);
            OnHideHamburger();
            LoadingManager.Instance.EffectiveSceneLoad("ArenaLobby", eSceneEffectType.CloudAnimation);
            var isArenaDefEmpty = isArenaDefDeckEmpty();
            if (!isArenaDefEmpty)
            {
                LoadingManager.Instance.SceneCallback = () =>
                {
                    ArenaManager.Instance.battleInfoTabIdx = 3;
                };
            }
        }

        public void OnClickAchieve()
        {
            OnHideHamburger();
            PopupManager.OpenPopup<CollectionAchievementPopup>(new TabTypePopupData(1, 0));
        }

        public void OnClickAttendance()
        {
            OnHideHamburger();
            AttendancePopup.OpenPopup();
        }
        public void OnClickInventory()
        {
            OnHideHamburger();
            PopupManager.OpenPopup<InventoryPopup>();
        }

        public void OnClickSetting()
        {
            OnHideHamburger();
            PopupManager.OpenPopup<SettingPopup>();
        }
        public void OnClickNaverLounge()
        {
            OnHideHamburger();
            Application.OpenURL(GameConfigTable.GetNaverLoungeURL());
        }
        bool isArenaDefDeckEmpty()
        {
            if (User.Instance.PrefData.ArenaFormationData.TeamFormationDEF == null || User.Instance.PrefData.ArenaFormationData.TeamFormationDEF.Count <= 0)
            {
                return true;
            }

            var arenaFirstpreset = User.Instance.PrefData.ArenaFormationData.TeamFormationDEF[0];
            if (arenaFirstpreset == null || arenaFirstpreset.Count <= 0)
            {
                return true;
            }

            var arenaFirstpresetCount = arenaFirstpreset.Count;
            var emptyCheckIndex = 0;
            var checkValue = 0;
            for (var i = 0; i < arenaFirstpresetCount; i++)
            {
                var arenaValue = arenaFirstpreset[i];

                if (checkValue == arenaValue)
                {
                    emptyCheckIndex++;
                }
            }

            return emptyCheckIndex == arenaFirstpresetCount;
        }

        public void OnClickDAppLink()
        {
            OnHideHamburger();
            bool active = GameConfigTable.WEB3_MENU_OPEN_ON_KOREAN || User.Instance.ENABLE_P2E;
            if (!active)
            {
                ToastManager.On(StringData.GetStringByIndex(100000636));
                return;
            }

            DAppManager.Instance.OpenDAppWebView();            
        }
    }
} 

