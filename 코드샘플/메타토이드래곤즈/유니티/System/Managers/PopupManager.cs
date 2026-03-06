using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public partial class PopupManager : IManagerBase
    {
        private static PopupManager instance = null;
        public static PopupManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new PopupManager();

                return instance;
            }
        }

        public Transform Beacon { get; private set; } = null;
        public PopupTopUIObject Top { get; private set; } = null;
        public Transform TutorialBeacon { get; private set; } = null;
        private Dictionary<Type, PopupBase> popupDic = null;
        private List<PopupBase> openPopupList = null;

        public List<Type> UsingExitButtonPopups { get; private set; } = new List<Type>();
        public List<Type> NoTopUIPopups { get; private set; } = new List<Type>();

        public static int OpenPopupCount
        {
            get
            {
                if (instance == null || instance.openPopupList == null)
                    return 0;

                return instance.openPopupList.Count;
            }
        }


        public void Initialize()
        {
            if (popupDic == null)
                popupDic = new Dictionary<Type, PopupBase>();
            else
                popupDic.Clear();

            if (openPopupList == null)
                openPopupList = new List<PopupBase>();
            else
                openPopupList.Clear();
        }

        public void SetBeacon(GameObject beacon)
        {
            if (beacon == null)
                return;

            Beacon = beacon.transform;
        }

        public void SetTutorialBeacon(GameObject beacon)
        {
            if (beacon == null)
                return;

            TutorialBeacon = beacon.transform;
        }

        public void SetPopupTop(PopupTopUIObject top)
        {
            UsingExitButtonPopups = new List<Type> {
                typeof(TownManagePopup),
                typeof(BuildingConstructListPopup),
                typeof(ProductPopup),
                typeof(LandMarkPopup),
                typeof(DungeonSelectPopup),
                typeof(ProductManagePopup),
                typeof(DragonManagePopup),
                typeof(AdventureReadyPopup),
                typeof(GemDungeonPopup),
                typeof(ShopPopup),
                typeof(DailyReadyPopup),
                typeof(RestrictedAreaReadyPopup),
                typeof(CollectionAchievementPopup),                
                typeof(MiningMainPopup),
                typeof(MissionPopup),
                typeof(BattlePassPopup),
                typeof(HolderPassPopup),
                typeof(MagicShowcaseEnterPopup),
                typeof(MagicShowcasePopup),
                typeof(DiceEventPopup),
                typeof(LuckyBagEventPopup),
                typeof(ProductTutorialPopup),
                typeof(ProductManageTutorialPopup),
                typeof(GuildStartPopup),
                typeof(GuildSelectPopup),
                typeof(GuildJoinPopup),
                typeof(GuildInfoPopup),
                typeof(ChampionBattleDragonSelectPopup),
                typeof(ChampionWinnerPopup_old),
                typeof(ChampionWinnerPopup),
                typeof(ChampionBetResultPopup),
                typeof(ChampionDragonDetailPopup),
                typeof(LunaServerEventPopup),
                typeof(ChampionSurpportPopup),
            };
            NoTopUIPopups = new List<Type>
            {
                typeof(SettingPopup),
                typeof(ToolTip),
                typeof(ItemToolTip),
                typeof(SimpleToolTip),
                typeof(AnnouncePopup),
                typeof(IntroPopup),
                typeof(ElemBuffInfoPopup),
                typeof(LevelPassPopup),
                typeof(PortraitPopup),
                typeof(DiceEventGiftOpenPopup),
                typeof(WebViewPopup),
                typeof(DAppWebBrowserPopup),
                typeof(GuildEmblemChangePopup),
                typeof(CoinBetPopup),
                typeof(OptionSelectPopup),
                typeof(ChampionBattleStatisticPopup),
                typeof(OpenEventRankingPopup),
                typeof(UnionRaidEventPopup),
                typeof(ChampionEventRankingPopup),
                typeof(GuildWidthdrawPopup),
                typeof(RestrictedAreaEventPopup),
                typeof(ChampionSurpportServerPopup),
                typeof(NameTagToolTip),
            };

            Top = top;
        }

        public void Update(float dt) { }



        public static T GetPopup<T>() where T : Component, IPopup
        {
            if (Instance.popupDic == null)
                return null;

            var t = typeof(T);
            if (!Instance.popupDic.ContainsKey(t))
            {
                var popup = LoadPopup(t);
                if (popup == null)
                    return null;
            }

            return Instance.popupDic[t] as T;
        }
        /// <summary>
        /// GetPopup은 Load 과정도 있기 때문에 현재 팝업이 당장 존재하는지 체크용
        /// </summary>
        public static bool IsExistPopup(Type popup)
        {
            if (Instance.popupDic == null)
                return false;
            if (!Instance.popupDic.ContainsKey(popup))
            {
                return false;
            }
            return true;
        }

        public static PopupBase LoadPopup(Type t)
        {
            if (PopupPathInfo.ContainsKey(t))
            {
                GameObject popup = ResourceManager.GetResource<GameObject>(eResourcePath.PopupPrefabPath, PopupPathInfo[t]);
                if (popup != null)
                {
                    return CreatePopup(popup);
                }
            }
            return null;
        }
        public static PopupBase CreatePopup(GameObject popup)
        {
            if (Instance.Beacon == null || Instance.popupDic == null || popup == null)
                return null;

            var target = UnityEngine.Object.Instantiate(popup, Instance.Beacon);
            if (target == null)
                return null;

            SBFunc.SetLayer(target, "UI");
            target.name = popup.name;

            PopupBase result = target.GetComponent<PopupBase>();
            if (result == null || Instance.popupDic.ContainsKey(result.GetType()))
            {
                UnityEngine.Object.Destroy(target);
                return null;
            }

            Instance.popupDic.Add(result.GetType(), result);
            result.SetActive(false);
            return result;
        }


        public static T OpenPopup<T>(PopupData data = null) where T : PopupBase
        {
            Type curType = typeof(T);

            T popupObj;
            if (Instance.popupDic.ContainsKey(curType) == false)
                popupObj = LoadPopup(curType) as T;
            else   // 이미 로드한 팝업을 건드는 경우  
                popupObj = Instance.popupDic[curType] as T;

            if (popupObj == null)
                return null;
            if (Instance.UsingExitButtonPopups.Contains(curType))
            {
                foreach (var closeTarget in Instance.UsingExitButtonPopups)
                {
                    if (curType == closeTarget)
                        continue;

                    if (Instance.popupDic.ContainsKey(closeTarget) &&IsPopupOpening(Instance.popupDic[closeTarget]) && Instance.popupDic[closeTarget].IsModeless())
                    {
                        Instance.popupDic[closeTarget].ClosePopup();
                    }
                }
            }

            if (Instance.openPopupList.Contains(popupObj))
                Instance.openPopupList.Remove(popupObj);

            popupObj.SetActive(true);
            popupObj.transform.SetAsLastSibling();
            popupObj.Init(data);

            Instance.openPopupList.Add(popupObj);
            if (Instance.NoTopUIPopups.Contains(popupObj.GetType()) ==false)
            {
                //if (Instance.openPopupList.Count == 1)
                    PopupTopUIRefreshEvent.Show(); // 우편 팝업의 Top UI에서 다이아를 클릭하여 상점 팝업으로 갔을때 상점 Top UI에 x버튼 없어서 처리
            }

            PopupEvent.Show();

            return popupObj;
        }

        public static void RemovePopupList(PopupBase target)
        {
            if (Instance.openPopupList.Contains(target))
                Instance.openPopupList.Remove(target);

            if (Instance.openPopupList.Count == 0)
            {
                PopupTopUIRefreshEvent.Hide();  
                if(Town.Instance != null && !target.HasExitCallback())
                {
                    target.SetExitCallback(NotificationManager.Instance.RefreshNotifications);
                }

                PopupEvent.Close();
            }
            else // 닫고 난 다음 남은 팝업 중에 최신 팝업에 맞추어 UI 갱신
            {
                // 탐험 준비 팝업 예외처리 (팝업이면서 탐험 씬과 동일한처리가 필요한 특이 케이스라 해당 팝업만 예외처리함)
                var curPopup = GetFirstPopup();
                if (OpenPopupCount > 0 && curPopup.name == "AdventureReadyPopup")
                {
                    var currentUIType = UIManager.Instance.CurrentUIType;
                    Instance.Top.InitUI(currentUIType);
                    Instance.Top.SetTopUI();
                    return;
                }

                if (OpenPopupCount <= 0)
                {
                    var currentUIType = UIManager.Instance.CurrentUIType;
                    Instance.Top.InitUI(currentUIType);
                    Instance.Top.SetTopUI();
                }
                else
                {
                    if (Instance.NoTopUIPopups.Contains(curPopup.GetType()) == false)
                        PopupTopUIRefreshEvent.Show(); // 우편 팝업의 Top UI에서 다이아를 클릭하여 상점 팝업으로 갔을때 상점 Top UI에 x버튼 없어서 처리
                }
            }
        }

        public static bool ClosePopup<T>() where T : PopupBase
        {
            return ClosePopup(GetPopup<T>());
        }

        protected static bool ClosePopup(PopupBase target)
        {
            if (target == null)
                return false;

            target.ClosePopup();
            return true;
        }

        public static void ForceUpdate<POPUP_TYPE>(PopupData data = null) where POPUP_TYPE : PopupBase
        {
            var popup = GetPopup<POPUP_TYPE>();
            if (popup != null)
                popup.ForceUpdate(data);
        }

        public static bool AllClosePopup()
        {
            if (Instance.openPopupList == null || Instance.openPopupList.Count < 1)
                return false;

            List<PopupBase> cloneList = new List<PopupBase>(Instance.openPopupList);
            foreach (PopupBase popup in cloneList)
            {
                popup.ClosePopup();
                popup.gameObject.SetActive(false);
            }

            Instance.openPopupList.Clear();

            PopupTopUIRefreshEvent.Hide();
            
            NotificationManager.Instance.RefreshNotifications();

            return true;
        }

        public void RefreshOrder()
        {
            if (Instance.openPopupList == null)
                return;

            Instance.openPopupList.Sort(OrderSort);
            var count = Instance.openPopupList.Count;
            //for(var i = 0; i < count; ++i)
            //{
            //    (Instance.openPopupList[i])?.transform.SetAsLastSibling();
            //}
        }

        private int OrderSort(IPopup p1, IPopup p2)
        {
            if (p1.GetOrder() > p2.GetOrder()) return -1;
            else if (p1.GetOrder() < p2.GetOrder()) return 1;
            return 0;
        }

		public static PopupBase GetFirstPopup()
		{
			int count = instance.openPopupList.Count;
			if (count == 0)
                return null;

            instance.RefreshOrder();
			return instance.openPopupList[count - 1];
		}

        public static bool IsPopupOpening()
        {
            if (Instance.openPopupList == null)
                return false;

            return instance.openPopupList.Count > 0;
        }

        public static bool IsPopupOpening(PopupBase popup)
        {
            if (Instance.openPopupList == null)
                return false;

            foreach(var p in instance.openPopupList)
            {
                if (p == popup)
                    return true;
            }

            return false;
        }

       
    }
}
