using DG.Tweening;
using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBadgeObject : MonoBehaviour, EventListener<UIObjectEvent>, EventListener<SettingEvent>
{
    [SerializeField] GameObject EventBadgeSample = null;
    [SerializeField] GameObject BattlePass = null;
    [SerializeField] GameObject HolderPass = null;
    
    [SerializeField] GameObject Sample = null;
    [SerializeField] Transform Container = null;

    [SerializeField] Sprite BattlePassIcon = null;
    [SerializeField] Sprite HolderPassIcon = null;
    [SerializeField] Sprite EventAttendanceIcon = null;
    [SerializeField] Sprite EventHolidayIcon = null;
    [SerializeField] Sprite EventHotTimeIcon = null;

    [SerializeField] ScrollRect scroll;
    [SerializeField] Image leftArrow = null;
    [SerializeField] Image rightArrow = null;
    GameObject HottimeObject = null;
    public void Init()
    {
        SetBadges(true);
        EventManager.AddListener<UIObjectEvent>(this);
        EventManager.AddListener<SettingEvent>(this);
    }

    private void Start()
    {
        
    }
    private void OnDestroy()
    {
        EventManager.RemoveListener<UIObjectEvent>(this);
        EventManager.RemoveListener<SettingEvent>(this);
    }

    private void Clear()
    {
        BattlePass.SetActive(false);
        if (HolderPass != null)
            HolderPass.SetActive(false);

        EventBadgeSample.SetActive(false);

        if(HottimeObject != null)
        {
            Destroy(HottimeObject);
            HottimeObject = null;
        }

        foreach (Transform item in Container)
        {
            if (item == Sample.transform ||
                item == BattlePass.transform ||
                item == HolderPass.transform ||  
                item == EventBadgeSample.transform
                )
                continue;

            Destroy(item.gameObject);
        }
    }
    public void SetBadges(bool init = false)
    {
        Clear();

        if (User.Instance.UserAccountData.UserNumber == 0)
            return;

        int badgeCount = 0;
        int hotTimeLongRemainTime = 0;
        EventScheduleData hotTimeData = null;

        List<EventScheduleData> events = User.Instance.EventData.GetActiveEvents(true);

        foreach (var data in events)
        {
            int remain = User.Instance.EventData.GetRemainTime(data);
            if (remain > 0)
            {
                if (data.TYPE == eActionType.EVENT_ATTENDANCE)
                {
                    EventAttendanceData attendanceData = (EventAttendanceData)data.EventBaseData;
                    if (attendanceData != null)
                    {
                        var rewards = EventRewardData.GetGroup(int.Parse(data.KEY));
                        if (rewards != null)
                        {
                            if (rewards.Count <= attendanceData.AttendanceDay)
                                continue;
                        }

                        GameObject clone = Instantiate(EventBadgeSample, Container);
                        clone.SetActive(true);
                        var eventIcon = data.SPRITE;
                        if (eventIcon == null)
                            eventIcon = EventAttendanceIcon;

                        clone.GetComponent<UIBadgeItem>().InitEventIcon(eventIcon, attendanceData.GetEventButtonString(), User.Instance.EventData.GetRemainTime(data, false), true, false, () =>
                        {
                            EventAttendancePopup.OpenPopup(data);
                        });

                        clone.transform.SetAsFirstSibling();
                        ++badgeCount;
                    }
                }
                else if(data.TYPE == eActionType.EVENT_DICE)
                {
                    if (data.EventBaseData == null)
                        continue;

                    GameObject clone = Instantiate(EventBadgeSample, Container);
                    clone.SetActive(true);
                    var eventIcon = data.SPRITE;
                    if (eventIcon == null)
                        eventIcon = EventHolidayIcon;

                    var reddotCondition = DiceEventPopup.GetTotalReddotCondition();
                    clone.GetComponent<UIBadgeItem>().InitEventIcon(eventIcon, data.EventBaseData.GetEventButtonString(), User.Instance.EventData.GetRemainTime(data, false), true, reddotCondition, () =>
                    {
                        DiceEventPopup.RequestEventHolidayData(()=> {
                            DiceEventPopup.OpenPopup(0);
                        },()=> {
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("서버데이터호출실패"));
                        });
                    });

                    clone.transform.SetAsFirstSibling();
                    ++badgeCount;
                }
                else if (data.TYPE == eActionType.EVENT_LUCKY_BAG)
                {
                    if (data.EventBaseData == null)
                        continue;

                    GameObject clone = Instantiate(EventBadgeSample, Container);
                    clone.SetActive(true);
                    var eventIcon = data.SPRITE;
                    if (eventIcon == null)
                        eventIcon = EventHolidayIcon;

                    var reddotCondition = LuckyBagEventPopup.GetTotalReddotCondition();
                    clone.GetComponent<UIBadgeItem>().InitEventIcon(eventIcon, data.EventBaseData.GetEventButtonString(), User.Instance.EventData.GetRemainTime(data, false), true, reddotCondition, () =>
                    {
                        LuckyBagEventPopup.RequestEventData(() => {
                            LuckyBagEventPopup.OpenPopup(0);
                        }, () => {
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("서버데이터호출실패"));
                        });
                    });

                    clone.transform.SetAsFirstSibling();
                    ++badgeCount;
                }
                else if (data.TYPE == eActionType.RESTRICTED_AREA_EVENT)
                {
                    if (data.USE)
                    {
                        GameObject clone = Instantiate(EventBadgeSample, Container);
                        clone.SetActive(true);
                        var eventIcon = data.SPRITE;
                        if (eventIcon == null)
                            eventIcon = EventHolidayIcon;

                        clone.GetComponent<UIBadgeItem>().InitEventIcon(eventIcon, StringData.GetStringByStrKey("event:name:" + data.KEY), User.Instance.EventData.GetRemainTime(data, false), true, false, () =>
                        {
                            PopupManager.OpenPopup<RestrictedAreaEventPopup>(new EventRankingPopupData(int.Parse(data.KEY)));
                        });

                        clone.transform.SetAsFirstSibling();
                        ++badgeCount;
                    }
                }
                else if (data.TYPE == eActionType.LUNASERVER_OPEN_EVENT)
                {
                    if (data.USE)
                    {
                        GameObject clone = Instantiate(EventBadgeSample, Container);
                        clone.SetActive(true);
                        var eventIcon = data.SPRITE;
                        if (eventIcon == null)
                            eventIcon = EventHolidayIcon;

                        bool reddot = true;
                        reddot = LunaServerEventPopup.GetLunaQuestReddotCondition(-1);
                        clone.GetComponent<UIBadgeItem>().InitEventIcon(eventIcon, StringData.GetStringByStrKey("event:name:" + data.KEY), User.Instance.EventData.GetRemainTime(data, false), true, reddot, () =>
                        {
                            LunaServerEventPopup.OpenPopup(new TabTypePopupData(0, 0));
                        });

                        clone.transform.SetAsFirstSibling();
                        ++badgeCount;
                    }
                }
                else if (data.TYPE == eActionType.UNIONRAID_RANKING)
                {
                    if (data.USE)
                    {
                        GameObject clone = Instantiate(EventBadgeSample, Container);
                        clone.SetActive(true);
                        var eventIcon = data.SPRITE;
                        if (eventIcon == null)
                            eventIcon = EventHolidayIcon;

                        clone.GetComponent<UIBadgeItem>().InitEventIcon(eventIcon, StringData.GetStringByStrKey("event:name:" + data.KEY), User.Instance.EventData.GetRemainTime(data, false), true, false, () =>
                        {
                            if (User.Instance.ENABLE_P2E)
                            {
                                DAppManager.Instance.OpenDAppEventPage(int.Parse(data.KEY));
                            }
                            else
                            {
                                PopupManager.OpenPopup<UnionRaidEventPopup>(new EventRankingPopupData(int.Parse(data.KEY)));
                            }
                        });

                        clone.transform.SetAsFirstSibling();
                        ++badgeCount;
                    }
                }
                else if (data.TYPE == eActionType.CHAMPIONEVENT_RANKING)
                {
                    if (data.USE)
                    {
                        GameObject clone = Instantiate(EventBadgeSample, Container);
                        clone.SetActive(true);
                        var eventIcon = data.SPRITE;
                        if (eventIcon == null)
                            eventIcon = EventHolidayIcon;

                        clone.GetComponent<UIBadgeItem>().InitEventIcon(eventIcon, StringData.GetStringByStrKey("event:name:" + data.KEY), User.Instance.EventData.GetRemainTime(data, false), true, false, () =>
                        {
                            if (User.Instance.ENABLE_P2E)
                            {
                                DAppManager.Instance.OpenDAppEventPage(int.Parse(data.KEY));
                            }
                            else
                            {
                                PopupManager.OpenPopup<ChampionEventRankingPopup>(new EventRankingPopupData(int.Parse(data.KEY)));
                            }
                        });

                        clone.transform.SetAsFirstSibling();
                        ++badgeCount;
                    }
                }
                else if (data.TYPE == eActionType.ANNOUNCE_OPEN)
                {   
                    GameObject clone = Instantiate(EventBadgeSample, Container);
                    clone.SetActive(true);
                    var eventIcon = data.SPRITE;
                    if (eventIcon == null)
                        eventIcon = EventHolidayIcon;

                    clone.GetComponent<UIBadgeItem>().InitEventIcon(eventIcon, StringData.GetStringByStrKey("event:name:" + data.KEY), User.Instance.EventData.GetRemainTime(data, false), true, false, () =>
                    {
                        if (User.Instance.ENABLE_P2E)
                        {
                            DAppManager.Instance.OpenDAppNoticePage(int.Parse(data.KEY));
                        }
                        else
                        {
                            AnnouncePopup popup = PopupManager.OpenPopup<AnnouncePopup>();
                            popup.SetDefaultMenuId(int.Parse(data.KEY));
                        }
                    });

                    clone.transform.SetAsFirstSibling();
                    ++badgeCount;
                }
                else if(data.TYPE == eActionType.EVENT_HOT_TIME_ADVENTURE || data.TYPE == eActionType.EVENT_HOT_TIME_DAILYDUNGEON 
                    || data.TYPE == eActionType.EVENT_HOT_TIME_WORLDBOSS|| data.TYPE == eActionType.EVENT_HOT_TIME_GEMDUNGEON)
                {
                    if(remain > hotTimeLongRemainTime)
                    {
                        hotTimeLongRemainTime = remain;
                        hotTimeData = data;
                    }
                }
            }
        }

        //hot time long remain Time Setting - 핫타임은 타입은 여러개지만, 하나의 팝업으로 처리 하기 때문에, 뱃지는 종료 시각이 제일 뒷 시각으로 표시
        if (HottimeObject == null && hotTimeLongRemainTime > 0)
        {
            HottimeObject = Instantiate(EventBadgeSample, Container);
            HottimeObject.SetActive(true);
            var eventIcon = hotTimeData.SPRITE;
            if (eventIcon == null)
                eventIcon = EventHotTimeIcon;

            HottimeObject.GetComponent<UIBadgeItem>().InitEventIcon(eventIcon, StringData.GetStringByStrKey("핫타임아이콘"), hotTimeLongRemainTime, true, false, () =>
            {
                PopupManager.OpenPopup<HotTimeEventDescPopup>();
            });

            HottimeObject.transform.SetAsFirstSibling();
            ++badgeCount;
        }

        //var curPassData = PassInfoData.GetCurPass(eBattlePassType.BATTLE);
        //if (curPassData != null)
        //{
        //    int remain = (int)(curPassData.END_TIME - TimeManager.GetDateTime()).TotalSeconds;
        //    if(remain> 0)
        //    {
        //        BattlePass.SetActive(true);
        //        BattlePass.GetComponent<UIBadgeItem>().InitEventIcon(BattlePassIcon, StringData.GetStringByStrKey("battle_pass"), remain, true, false, () => {
        //            PopupManager.OpenPopup<BattlePassPopup>();
        //        });
        //        ++badgeCount;
        //    }   
        //}        
        //if (PassInfoData.IsAvailablePassDataExist(eBattlePassType.HOLDER)) // 이용가능한 패스가 있고 홀더 패스 데이터 세팅했는지 체크!
        //{
        //    int remain = BattlePassManager.Instance.HolderPassRemainTime - TimeManager.GetTime();
        //    if (remain > 0)
        //    {
        //        HolderPass.SetActive(true);
        //        HolderPass.GetComponent<UIBadgeItem>().InitEventIcon(HolderPassIcon, StringData.GetStringByStrKey("holder_pass"), remain, true, false, () =>
        //        {
        //            PopupManager.OpenPopup<HolderPassPopup>();
        //        });
        //        ++badgeCount;
        //    }
        //    else
        //    {
        //        BattlePassManager.Instance.RefreshHolderPassData();
        //    }
        //}

        if (init)
            gameObject.SetActive(true);

        Sample.SetActive(true);
        
#region 첫구매 팝업 띄우는 뱃지 조건 처리
        List<ShopBannerData> banners  = ShopBannerData.GetByType(BANNER_TYPE.SMALL);
        foreach (ShopBannerData banner in banners)
        {
            int goods = int.Parse(banner.KEY);
            var state = ShopManager.Instance.GetGoodsState(goods);

            if (state.RemainGoodsCount > 0) // 상품 있음
            {
                ++badgeCount;
                GameObject clone = Instantiate(Sample, Container);
                int remain = int.MaxValue;
                bool reddot = DateTime.Today.Day != CacheUserData.GetInt("check_badge_" + goods, 0);

                clone.GetComponent<UIBadgeItem>().InitShopIcon(state.BaseData.BANNER.ICON_RESOURCE, PersonalGoodsData.Get(goods).NAME_STRING, remain, false, reddot, () =>
                {
                    CacheUserData.SetInt("check_badge_" + goods, DateTime.Today.Day);
                    var popup = PopupManager.OpenPopup<ConditionalBuyPopup>(new ConditionBuyData(goods));
                    popup.SetRewardCallBack(() => {
                        PopupManager.GetPopup<ShopPopup>().RefreshCurrentMenu();
                        popup.ClosePopup();
                    });
                });
            }
        }
        #endregion

        #region 첫구매 팝업을 제외한 나머지 조건 처리
        #region 노티가 필요한 그룹아이템 처리
        if (ShopGroupBanner.IsValid)
        {
            var groupData = ShopGroupBanner.GroupMenuData;
            GameObject clone = Instantiate(Sample, Container);
            int remain = (int)(groupData.END_TIME - TimeManager.GetDateTime()).TotalSeconds;
            bool reddot = DateTime.Today.Day != CacheUserData.GetInt("check_badge_group", 0);
            clone.GetComponent<UIBadgeItem>().InitEventIcon(groupData.ICON, groupData.NAME, remain, false, reddot, () =>
            {
                CacheUserData.SetInt("check_badge_group", DateTime.Today.Day);
                PopupManager.OpenPopup<ShopGroupBanner>();
            });
        }
        #endregion

        foreach (var goods in ShopManager.Instance.PrivateGoods.Keys)
        {
            var shopBannerData = ShopBannerData.Get(goods.ToString());
            if(shopBannerData == null)
            {
                Debug.LogError(">>>>shopBannerData null key : " + goods);
                continue;
            }

            if (shopBannerData.TYPE == BANNER_TYPE.SMALL)
                continue;
            var state = ShopManager.Instance.GetGoodsState(goods);
            if (state.IS_VALIDE && state.BaseData != null && state.BaseData.BANNER != null && !string.IsNullOrEmpty(state.BaseData.BANNER.ICON_RESOURCE) && state.RemainGoodsCount>0)
            {
                badgeCount++;
                GameObject clone = Instantiate(Sample, Container);
                int remain = (int)(ShopManager.Instance.PrivateGoods[goods] - TimeManager.GetDateTime()).TotalSeconds;
                bool reddot = DateTime.Today.Day != CacheUserData.GetInt("check_badge_" + goods, 0);
                clone.GetComponent<UIBadgeItem>().InitShopIcon(state.BaseData.BANNER.ICON_RESOURCE, PersonalGoodsData.Get(goods).NAME_STRING, remain, false, reddot, () =>
                {
                    CacheUserData.SetInt("check_badge_" + goods, DateTime.Today.Day);
                    var popupData = ShopGoodsData.Get(goods);
                    if (popupData != null)
                    {
                        var bannerPopup = PopupManager.OpenPopup<ShopBannerPopup>(new ShopBuyPopupData(popupData));
                    }
                });
            }

        }
        #endregion

        Sample.SetActive(false);

        
        LayoutRebuilder.ForceRebuildLayoutImmediate(Container.GetComponent<RectTransform>());
        Invoke("CheckArrow", 0.1f);
    }

    public void OnScroll()
    {
        Invoke("CheckArrow", 0.1f);
    }

    public void CheckArrow()
    {
        CancelInvoke("CheckArrow");

        bool l = false;
        bool r = false;

        if((scroll.transform as RectTransform).sizeDelta.x < (Container as RectTransform).sizeDelta.x)
        {
            const float cap = 0.01f;
            if (scroll.horizontalNormalizedPosition > cap)
                l = true;
            if (scroll.horizontalNormalizedPosition < 1f - cap)
                r = true;

            leftArrow.gameObject.SetActive(true);
            rightArrow.gameObject.SetActive(true);
        }
        else
        {
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
            return;
        }

        Color startClr = new Color(0.9f, 1f, 0.9f, 0.3f);
        Color endClr = new Color(1f, 1f, 0.9f, 0.5f);
        Color disableClr = new Color(1f, 1f, 1f, 0.0f);
        if (l)
        {
            leftArrow.DOKill();
            leftArrow.DOColor(startClr, 0.2f).OnComplete(() => {
                leftArrow.DOColor(endClr, 0.3f);
            });
        }
        else
        {
            leftArrow.DOKill();
            leftArrow.DOColor(disableClr, 0.5f);
        }

        if (r)
        {
            rightArrow.DOKill();
            rightArrow.DOColor(startClr, 0.2f).OnComplete(() => {
                rightArrow.DOColor(endClr, 0.3f);
            });
        }
        else
        {
            rightArrow.DOKill();
            rightArrow.DOColor(disableClr, 0.5f);
        }

    }
    public void SetActive(bool active)
    {
        switch (active)
        {
            case true:
                // 데이터 세팅 등 무언가의 작업
                SetBadges(true);
                break;

            case false:
                gameObject.SetActive(false);
                break;
        }
    }
    public void OnEvent(UIObjectEvent eventType)
    {
        if ((eventType.t & UIObjectEvent.eUITarget.LT) != UIObjectEvent.eUITarget.NONE)
        {
            switch (eventType.e)
            {                
                case UIObjectEvent.eEvent.REFRESH_BADGE:
                    SetBadges();
                    break;
            }
        }
    }
    public void OnEvent(SettingEvent eventType)
    {
        SetBadges();
    }

    private void OnDisable()
    {
        rightArrow.DOKill();
        leftArrow.DOKill();
    }
}
