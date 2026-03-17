using UnityEngine;
using EasyMobile;
using UnityEngine.Purchasing;
using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections.Generic;
using System;
using System.Linq;

public class NotificationManager : SBPersistentSingleton<NotificationManager>
{
    public enum NotificationType 
    {
        SAMPLE,

        ENERGY_FULL,
        ARENA_TICKET_FULL,
        TOWN_LEVELUP,
        BUILDING_CONSTRUCT,
        PRODUCE_DONE,
        BATTERY_FULL,
        DOZER_FULL,
        SUBWAY_DONE,
        TRAVEL_DONE,
        NOGAME_LONGTIME,
        NOGAME_VERYLONGTIME,
        NOGAME_WEEK,
    };

    private bool AnnounceNeedShow = true;
    private List<EventScheduleData> ProcessedEventDatas = new List<EventScheduleData>();
    string deeplinkURL = "";
    bool notificationProcessing = false;
    bool isIntroProcessing = false;
    bool townScrolled = false;
    public bool QuestHighlight { get; private set; } = false;
    enum TutorialState
    {
        Unkown = 0,
        Run = 1,
        Skip = 2,
    }

    TutorialState tutorialState = TutorialState.Unkown;

    protected override void Awake()
    {
        base.Awake();

        Application.deepLinkActivated += OnDeppLinkActivated;
    }

    private void OnDestroy()
    {
        Application.deepLinkActivated -= OnDeppLinkActivated;
    }

    void Start()
    {
#if !UNITY_EDITOR
            try
            {
                if (Notifications.DataPrivacyConsent == ConsentStatus.Unknown)
			    {
				    Notifications.GrantDataPrivacyConsent();
				    Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
			    }   
            }
            catch
            {
                Debug.Log("Notifications.DataPrivacyConsent Failed");
            }			     
#endif


        //Notifications.Init();
    }

    public void Clear()
    {
        AnnounceNeedShow = true;
        ProcessedEventDatas = new List<EventScheduleData>();
        deeplinkURL = "";
        notificationProcessing = false;
        isIntroProcessing = false;
        townScrolled = false;
    }

    public bool IsAlram(NotificationType type)
    {
        int defaultValue = 1;
        switch(type)
        {
            case NotificationType.ENERGY_FULL:
            case NotificationType.ARENA_TICKET_FULL:
            case NotificationType.TOWN_LEVELUP:
            case NotificationType.BUILDING_CONSTRUCT:
            case NotificationType.PRODUCE_DONE:
            case NotificationType.BATTERY_FULL:
            case NotificationType.DOZER_FULL:
            case NotificationType.SUBWAY_DONE:
            case NotificationType.TRAVEL_DONE:
            case NotificationType.NOGAME_LONGTIME:
            case NotificationType.NOGAME_VERYLONGTIME:
            case NotificationType.NOGAME_WEEK:
            case NotificationType.SAMPLE:
                defaultValue = 1;
                break;
        }

        return PlayerPrefs.GetInt("Notification_" + type.ToString(), defaultValue) > 0;
    }
    public void SetAlram(NotificationType type, bool on)
    {
        PlayerPrefs.SetInt("Notification_" + type.ToString(), on ? 1 : 0);
    }

    NotificationContent GetNotificationContent(NotificationType type)
    {
        NotificationContent content = new NotificationContent();
        content.categoryId = GetNotificationCategoryID(type);

        content.title = StringData.GetStringByStrKey("push:title:" + type.ToString());
        content.subtitle = StringData.GetStringByStrKey("push:title:" + type.ToString());
        content.body = StringData.GetStringByStrKey("push:title:" + type.ToString());

        return content;
    }

    string GetNotificationCategoryID(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.ENERGY_FULL:
                return EM_NotificationsConstants.UserCategory_ENERGY_FULL;
            case NotificationType.ARENA_TICKET_FULL:
                return EM_NotificationsConstants.UserCategory_ARENA_TICKET_FULL;
            case NotificationType.TOWN_LEVELUP:
                return EM_NotificationsConstants.UserCategory_TOWN_LEVELUP;
            case NotificationType.BUILDING_CONSTRUCT:
                return EM_NotificationsConstants.UserCategory_BUILDING_CONSTRUCT;
            case NotificationType.PRODUCE_DONE:
                return EM_NotificationsConstants.UserCategory_PRODUCE_DONE;
            case NotificationType.BATTERY_FULL:
                return EM_NotificationsConstants.UserCategory_BATTERY_FULL;
            case NotificationType.DOZER_FULL:
                return EM_NotificationsConstants.UserCategory_DOZER_FULL;
            case NotificationType.SUBWAY_DONE:
                return EM_NotificationsConstants.UserCategory_SUBWAY_DONE;
            case NotificationType.TRAVEL_DONE:
                return EM_NotificationsConstants.UserCategory_TRAVEL_DONE;
            case NotificationType.NOGAME_LONGTIME:
                return EM_NotificationsConstants.UserCategory_NOGAME_LONGTIME;
            case NotificationType.NOGAME_VERYLONGTIME:
                return EM_NotificationsConstants.UserCategory_NOGAME_VERYLONGTIME;
            case NotificationType.NOGAME_WEEK:
                return EM_NotificationsConstants.UserCategory_NOGAME_WEEK;
            default:
                return EM_NotificationsConstants.UserCategory_notification_category_sample;
        }


    }
    public void RegistNotification(NotificationType type, DateTime time)
    {
        NotificationContent content = GetNotificationContent(type);

        if (content == null)
            return;

        TimeSpan delay = time - TimeManager.GetDateTime();

        //Notifications.ScheduleLocalNotification(delay, content, NotificationRepeat.None);
    }

    public void ClearNotification(NotificationType type)
    {
        //Notifications.CancelPendingLocalNotification(GetNotificationCategoryID(type));
    }
    public void ClearNotifications()
    {
        //Notifications.CancelAllPendingLocalNotifications();
    }

    public void RefreshNotifications()
    {
        ClearNotifications();

        //CheckEnergyFullNotification();
        //CheckArenaTicketFullNotification();
        //CheckTownLevelUpDoneNotification();
        //CheckBuildingConstructNotification();
        //CheckBuildingProduceDoneNotification();
        //CheckGoldDozerNotification();
        //CheckSubwayNotification();
        //CheckTravelNotification();
        //CheckLastLoginNotification();

        CheckNotificationPopup();
    }

    /// <summary>
    /// 알림팝업 순차적으로 나올 수 있도록 된 팝업
    /// 끄면 다음 팝업 호출 될 수 있도록 구현
    /// 겁나 절차지향적임
    /// </summary>
    /// 
    private void CheckNotificationPopup()
    {
        if (Town.Instance == null)
            return;

        if (ScenarioManager.Instance.IsPlaying)
        {
            return;
        }

        if (PopupManager.IsPopupOpening())
            return;

        notificationProcessing = true;


        if (ScenarioManager.Instance.OnEventCheckFirstIntroStart(() => {
            CacheUserData.SetInt("LastOpenedMainQuest", 0);
            //타운 한번 훓어주고 CheckNotificationPopup();

            Town.Instance.SetCamPanMoving(Town.Instance.head.transform.position + new Vector3(0, 2, 0), new Vector3(0, 2, 0), 1.8f, true, new Vector3(0, 2, 0), () =>
            {
                // 모든 UI 켜줘야 함
                Town.Instance.SetIntroModeState(false);
                UIManager.Instance.InitUI(eUIType.Town);
                isIntroProcessing = false;

                CheckNotificationPopup();
            });

            townScrolled = true;
        }, () => {
            Town.Instance.OnBGMPlay();
            Town.Instance.SetCamZoomTargetPos(new Vector3(0, 9, 0), false, false);
            Town.Instance.SetIntroModeState(true);
        }))
        {
            //모든 UI 꺼줘야 함
            UIManager.Instance.InitUI(eUIType.None);
            isIntroProcessing = true;
            return;
        }
        else
        {
            if (!townScrolled)
            {
                CacheUserData.SetInt("LastOpenedMainQuest", 0);
                // 모든 UI 켜줘야 함
                UIManager.Instance.InitUI(eUIType.None);
                isIntroProcessing = true;

                Town.Instance.OnBGMPlay();
                Town.Instance.ZoomBackToTarget(new Vector3(0, 9, 0), 0.2f);
                Town.Instance.SetIntroModeState(true);
                                           
                Town.Instance.SetCamPanMoving(Town.Instance.head.transform.position + new Vector3(0, 2, 0), new Vector3(0, 2, 0), 1.8f, true, new Vector3(0, 2, 0), () =>
                {
                    // 모든 UI 켜줘야 함
                    Town.Instance.SetIntroModeState(false);
                    UIManager.Instance.InitUI(eUIType.Town);
                    isIntroProcessing = false;

                    CheckNotificationPopup();
                }, 1.0f);

                townScrolled = true;
                return;
            }
        }

        if (isIntroProcessing)
            return;

        if (ScenarioManager.Instance.OnEventCheckFirstStart(() => {
            var beacon = UIManager.Instance.Beacon;
            if (beacon != null && beacon.UIObjects != null && beacon.UIObjects.Count > 4)
            {
                QuestUIObject questUI = beacon.UIObjects[4] as QuestUIObject;
                if (questUI)
                {
                    questUI.OnHighlightImmediate();
                }

                QuestHighlight = true;                
            }
        }))
        {
            return;
        }


        if (TutorialManager.tutorialManagement != null && TutorialManager.tutorialManagement.IsRemainMainTutorial())
        {
            if (tutorialState == TutorialState.Unkown)
            {                
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("튜토리얼진행확인"), StringData.GetStringByStrKey("튜토리얼진행"), StringData.GetStringByStrKey("튜토리얼스킵"), () =>
                {
                    AppsFlyerSDK.AppsFlyer.sendEvent("tutorial_start", new Dictionary<string, string>());
                    tutorialState = TutorialState.Run;
                }
                , () =>
                {
                    tutorialState = TutorialState.Skip;

                    Dictionary<string, string> param = new Dictionary<string, string>();
                    param.Add("skip", "1");

                    AppsFlyerSDK.AppsFlyer.sendEvent("tutorial_skip", param);

                    WWWForm paramData = new WWWForm();
                    paramData.AddField("tuto_type", 1);
                    paramData.AddField("tuto_id", 80001);

                    NetworkManager.Send("tutorial/advance", paramData, (jsonData) =>
                    {
                        if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                        {
                            switch (jsonData["rs"].Value<int>())
                            {
                                case (int)eApiResCode.OK:
                                    if (SBFunc.IsJTokenCheck(jsonData["reward"]))
                                    {
                                        var reward = (JArray)jsonData["reward"];
                                        if (reward != null && reward.Count > 0)
                                        {
                                            var rewardPopup = SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(reward)));
                                            rewardPopup.SetText(StringData.GetStringByStrKey("튜토리얼보상"));
                                            rewardPopup.SetDimmedClickAction(() => { }); // 딤드 눌러도 안꺼지고 아무것도 안하게 세팅
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }, (string str) =>
                    {
                        CheckNotificationPopup();
                    });

                });

                return;
            }
            else if (tutorialState == TutorialState.Run)
            {
                foreach (var tuto in TutorialTriggerData.GetByTriggerType(ScriptTriggerType.TUTORIAL_START))
                {
                    TutorialManager.tutorialManagement.StartTutorial(tuto, () =>
                    {
                        UIManager.Instance.BannerGroup.gameObject.SetActive(true);
                    });
                }
                if (TutorialManager.tutorialManagement.IsOtherContentsBlock())
                {
                    UIManager.Instance.BannerGroup.gameObject.SetActive(false);
                    return;
                }
            }
        }

        if (SBFunc.HasTimeValue("Announcement") == false && AnnounceNeedShow)
        {
            AnnounceNeedShow = false;
            if (User.Instance.ENABLE_P2E)
                DAppManager.Instance.OpenDAppNoticePage(0, CheckNotificationPopup);
            else
                PopupManager.OpenPopup<AnnouncePopup>();
            return;
        }

        if (AttendancePopup.CheckAttendance())//출석 팝업
        {
            return;
        }

        if (EventAttendancePopup.CheckEventAttendance())//이벤트 출석 팝업
        {
            return;
        }

        if (CacheUserData.GetBoolean("group_goods_19", false))
        {
            CacheUserData.SetBoolean("group_goods_19", true);
            if (ShopGroupBanner.IsValid)
            {
                PopupManager.OpenPopup<ShopGroupBanner>();
                return;
            }
        }

        int goodsID = ShopManager.Instance.GetNeedShowPrivateGoods();
        if (goodsID > 0)
        {
            ShopManager.Instance.SetShownPrivateGoods(goodsID);

            var dat = ShopBannerData.Get(goodsID.ToString());
            var personaData = PersonalGoodsData.Get(goodsID);
            if (dat != null)
            {
                switch (dat.TYPE)
                {
                    case BANNER_TYPE.SMALL:
                    {
                        var popup = PopupManager.OpenPopup<ConditionalBuyPopup>(new ConditionBuyData(goodsID));
                        popup.SetRewardCallBack(() =>
                        {
                            PopupManager.GetPopup<ShopPopup>().RefreshCurrentMenu();
                            popup.ClosePopup();
                        });
                        return;
                    }

                    case BANNER_TYPE.FULLSCREEN:
                    default:
                    {
                        ShopGoodsData data = ShopGoodsData.Get(goodsID);
                        if (data.BANNER == null)
                        {
                            CheckNotificationPopup();
                            return;
                        }

                        var popup = PopupManager.OpenPopup<ShopBannerPopup>(new ShopBuyPopupData(data));
                        return;
                    }
                    break;
                }
            }
        }

        foreach (var data in User.Instance.EventData.GetActiveEvents(false))
        {
            if (ProcessedEventDatas.Contains(data))
                continue;

            ProcessedEventDatas.Add(data);

            switch (data.TYPE)
            {
                case eActionType.EVENT_DICE:
                {
                    if (DateTime.Today.Day == CacheUserData.GetInt("EventDiceFirstOpen", 0))
                        continue;

                    WWWForm form = new WWWForm();
                    form.AddField("event_id", data.KEY);

                    NetworkManager.Send("event/dice", form, (JObject jsonData) =>
                    {
                        if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
                        {
                            User.Instance.EventData.SetData(data, jsonData);

                            DiceEventPopup popup = DiceEventPopup.OpenPopup(0);
                            if (popup != null)
                            {
                                popup.SetExitCallback(CheckNotificationPopup);
                            }
                        }
                        else
                            CheckNotificationPopup();
                    }, (failString) =>
                    {
                        CheckNotificationPopup();
                    });

                    CacheUserData.SetInt("EventDiceFirstOpen", DateTime.Today.Day);
                    return;
                }
                case eActionType.EVENT_LUCKY_BAG:
                {
                    if (DateTime.Today.Day == CacheUserData.GetInt("EventLuckybagFirstOpen", 0))
                        continue;

                    WWWForm form = new WWWForm();
                    form.AddField("event_id", data.KEY);
                    form.AddField("op", (int)eLuckyBagEventState.REQUEST_INFO);
                    NetworkManager.Send("event/newyear2024", form, (JObject jsonData) =>
                    {
                        if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
                        {
                            User.Instance.EventData.SetData(data, jsonData);

                            LuckyBagEventPopup popup = LuckyBagEventPopup.OpenPopup(0);
                            if (popup != null)
                            {
                                popup.SetExitCallback(CheckNotificationPopup);
                            }
                        }
                        else
                            CheckNotificationPopup();
                    }, (failString) =>
                    {
                        CheckNotificationPopup();
                    });

                    CacheUserData.SetInt("EventLuckybagFirstOpen", DateTime.Today.Day);
                    return;
                }
            }
        }

        if (CheckDeepLink())
            return;

        CheckCollectionAchievementNotification();

#if UNITY_ANDROID && !ONESTORE
        CheckPlayPointPurchase();
#endif

        notificationProcessing = false;
    }

    bool CheckDeepLink()
    {
        if (string.IsNullOrEmpty(deeplinkURL))
            return false;

        string[] url = deeplinkURL.Split('?');
        deeplinkURL = "";

        if (url.Length > 2)
        {
            var param = url[1];
            foreach (var p in param.Split('&'))
            {
                switch (p)
                {
                    case "holiday23":
                        List<EventScheduleData> events = User.Instance.EventData.GetActiveEvents(false);
                        foreach (var data in events)
                        {
                            int remain = User.Instance.EventData.GetRemainTime(data);
                            if (remain > 0)
                            {
                                if (data.TYPE == eActionType.EVENT_DICE)
                                {
                                    DiceEventPopup.RequestEventHolidayData(() =>
                                    {
                                        DiceEventPopup popup = DiceEventPopup.OpenPopup(0);
                                        if (popup != null)
                                        {
                                            popup.SetExitCallback(CheckNotificationPopup);
                                        }
                                    }, () =>
                                    {
                                        //SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("서버데이터호출실패"));
                                    });

                                    return true;
                                }
                            }
                        }
                        break;
                }
            }
        }

        return false;
    }
    public void CheckCollectionAchievementNotification()
    {
        CheckCompleteByType(eCollectionAchievementType.COLLECTION);
        CheckCompleteByType(eCollectionAchievementType.ACHIEVEMENT);
    }

    public void CheckPlayPointPurchase()
    {
        IAPManager.Instance.CheckPlayPointProduct();
    }

    private void CheckCompleteByType(eCollectionAchievementType _type)
    {
        string info = CollectionAchievementManager.Instance.GetLocalAccomplishDataByType(_type);
        if (string.IsNullOrEmpty(info))
            return;
        CollectionAchievementManager.Instance.ClearLocalCollectionAccomplishDataByType(_type);
        
        List<string> prevs = info.Split(",").ToList();
        foreach(var prev in prevs)
        {
            if (string.IsNullOrEmpty(prev))
                continue;

            var isCollection = _type == eCollectionAchievementType.COLLECTION;
            var detail = isCollection ? CollectionData.Get(prev)?.TOAST : AchievementBaseData.Get(prev).TOAST;
            var title = isCollection ? StringData.GetStringByStrKey("collection_info_title") : StringData.GetStringByStrKey("achievements_info_title");
            ShowToastCollectionAchievementComplete(title,detail);
        }
    }
    public void ShowToastCollectionAchievementComplete(string _title, string _detail)
    {
        CAMessageManager.OnCAComplete(_title, _detail);
    }

    public void CheckEnergyFullNotification()
    {
        NotificationType type = NotificationType.ENERGY_FULL;
        if (!IsAlram(type))
            return;

        var EnergyInfo = User.Instance.GetNextEnergyExpire();
        if (EnergyInfo != null)
        {
            var maxStamina = AccountData.GetLevel(User.Instance.UserData.Level).MAX_STAMINA;
            if (EnergyInfo.energy < maxStamina)
            {
                var maxTick = EnergyInfo.exp + ((maxStamina - User.Instance.ENERGY) * 300);
                RegistNotification(type, TimeManager.GetDateTime(0, 0, 0, TimeManager.GetTimeCompare(maxTick)));
            }
        }
    }

    public void CheckArenaTicketFullNotification()
    {
        NotificationType type = NotificationType.ARENA_TICKET_FULL;
        if (!IsAlram(type))
            return;

        var ArenaInfo = ArenaManager.Instance.GetNextArenaEnergyExpire();
        if (ArenaInfo != null)
        {
            var maxCount = GameConfigTable.GetArenaUserMaxTicketCount();
            if (ArenaInfo.Arena_Ticket < maxCount)
            {
                var maxTick = ArenaInfo.Arena_Ticket_Exp + ((maxCount - ArenaInfo.Arena_Ticket) * GameConfigTable.GetArenaOneTicketRechargeTime());
                RegistNotification(type, TimeManager.GetDateTime(0, 0, 0, TimeManager.GetTimeCompare(maxTick)));
            }
        }
    }

    public void CheckTownLevelUpDoneNotification()
    {
        NotificationType type = NotificationType.TOWN_LEVELUP;
        if (!IsAlram(type))
            return;

        var curExteriorData = User.Instance.ExteriorData;
        if (curExteriorData != null)
        {
            switch (curExteriorData.ExteriorState)
            {
                case eBuildingState.CONSTRUCTING:
                    RegistNotification(type, TimeManager.GetDateTime(0, 0, 0, TimeManager.GetTimeCompare(curExteriorData.ExteriorTime)));
                    break;
                default:
                    return;
            }
        }
    }

    public void CheckBuildingConstructNotification()
    {
        NotificationType type = NotificationType.BUILDING_CONSTRUCT;
        if (!IsAlram(type))
            return;

        foreach (var buildinfo in User.Instance.GetUserBuildingList())
        {
            if(buildinfo.State == eBuildingState.CONSTRUCTING)
            {
                RegistNotification(type, TimeManager.GetDateTime(0, 0, 0, TimeManager.GetTimeCompare(buildinfo.ActiveTime)));
                break;
            }
        }        
    }

    public void CheckBuildingProduceDoneNotification()
    {
        if (!IsAlram(NotificationType.PRODUCE_DONE) && !IsAlram(NotificationType.BATTERY_FULL))
            return;

        List<ProducesBuilding> buildingList = User.Instance.GetAllProducesList(true);

        int batteryDone = 0;
        int produceDone = 0;
        // 수령 가능한 생산품목 리스트 확인
        foreach (ProducesBuilding building in buildingList)
        {
            if (building.Items == null || building.Items.Count <= 0) continue;

            string buildingGroup = BuildingOpenData.GetWithTag(building.Tag).BUILDING;

            if (building.OpenData.BaseData.TYPE == 2)
            {
                if (!IsAlram(NotificationType.BATTERY_FULL))
                    continue;

                int count = 0;
                
                ProducesRecipe curItem = null;
                foreach (var item in building.Items)
                {
                    count++;
                    if (item.State != eProducesState.Complete)
                    {
                        curItem = item;
                        break;
                    }
                }

                if (curItem != null)
                {
                    int curExp = 0;
                    if (curItem.ProductionExp > 0)
                    {
                        curExp = curItem.ProductionExp;
                    }

                    int remainTime = 0;
                    int remainCount = building.Slot - count;
                    var buildinfo = User.Instance.GetUserBuildingInfoByTag(building.Tag);
                    if(buildinfo != null)
                    {
                        var datas = ProductAutoData.GetListByGroupAndLevel(buildingGroup, buildinfo.Level);
                        if (datas != null)
                        {
                            var autoProductData = datas[0];
                            if (autoProductData != null)
                            {
                                remainTime = (remainCount * autoProductData.MAX_TIME) - curExp;
                                batteryDone = Mathf.Max(batteryDone, remainTime);
                            }
                        }
                    }
                }
            }
            else
            {
                if (!IsAlram(NotificationType.PRODUCE_DONE))
                    continue;

                int remain = 0;
                foreach (ProducesRecipe productItem in building.Items)
                {
                    if (productItem.State == eProducesState.Complete)
                        continue;

                    if (productItem.State == eProducesState.Ing)
                    {
                        produceDone = productItem.ProductionExp - TimeManager.GetTime();
                        continue;
                    }

                    ProductData itemReceipe = ProductData.GetProductDataByGroupAndKey(buildingGroup, productItem.RecipeID);
                    if (itemReceipe == null)
                        continue;

                    remain += itemReceipe.PRODUCT_TIME;
                }

                produceDone = Mathf.Max(produceDone, remain);
            }
        }

        if (batteryDone > 0 && IsAlram(NotificationType.BATTERY_FULL))
        {
            RegistNotification(NotificationType.BATTERY_FULL, TimeManager.GetDateTime(0, 0, 0, batteryDone));
        }

        if(produceDone > 0 && IsAlram(NotificationType.PRODUCE_DONE))
        {
            RegistNotification(NotificationType.PRODUCE_DONE, TimeManager.GetDateTime(0, 0, 0, produceDone));
        }
    }

    public void CheckGoldDozerNotification()
    {
        NotificationType type = NotificationType.DOZER_FULL;
        if (!IsAlram(type))
            return;

        LandmarkDozer dozer = User.Instance.GetLandmarkData<LandmarkDozer>();
        if (dozer != null)
        {
            if (TimeManager.GetTimeCompare(dozer.ExpireTime) > 0)
                RegistNotification(type, TimeManager.GetDateTime(0, 0, 0, TimeManager.GetTimeCompare(dozer.ExpireTime)));
        }
    }

    public void CheckSubwayNotification()
    {
        NotificationType type = NotificationType.SUBWAY_DONE;
        if (!IsAlram(type))
            return;

        LandmarkSubway subway = User.Instance.GetLandmarkData<LandmarkSubway>();
        if (subway != null)
        {
            int remain = 0;
            foreach(var platform in subway.GetActivatePlatform())
            {
                remain = Mathf.Max(platform.Expire, remain);                
            }

            if (remain > 0)
                RegistNotification(type, TimeManager.GetDateTime(0, 0, 0, TimeManager.GetTimeCompare(remain)));
        }
    }

    public void CheckTravelNotification()
    {
        NotificationType type = NotificationType.TRAVEL_DONE;
        if (!IsAlram(type))
            return;

        LandmarkTravel travel = User.Instance.GetLandmarkData<LandmarkTravel>();
        if (travel != null)
        {
            if(travel.TravelState == eTravelState.Travel)
            {
                RegistNotification(type, TimeManager.GetDateTime(0, 0, 0, TimeManager.GetTimeCompare(travel.TravelTime)));
            }    
        }
    }

    public void CheckLastLoginNotification()
    {
        RegistNotification(NotificationType.NOGAME_LONGTIME, TimeManager.GetDateTime(1, 0, 0, 0));
        RegistNotification(NotificationType.NOGAME_VERYLONGTIME, TimeManager.GetDateTime(3, 0, 0, 0));
        RegistNotification(NotificationType.NOGAME_WEEK, TimeManager.GetDateTime(7, 0, 0, 0));
    }

    public void OnDeppLinkActivated(string url)
    {
        deeplinkURL = url;

        if (!notificationProcessing)
            CheckNotificationPopup();
    }
}
