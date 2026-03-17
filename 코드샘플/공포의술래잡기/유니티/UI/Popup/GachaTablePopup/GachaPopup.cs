using Coffee.UIExtensions;
using DG.Tweening;
using SBCommonLib;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaPopup : Popup
{
    [SerializeField] Transform tabContent;

    [SerializeField] Text eventBannerText;
    [SerializeField] Text eventBannerSub_Text;
    [SerializeField] Transform bgBannerContainer;

    [SerializeField] Image priceImage_ad;
    [SerializeField] Text priceText_ad;
    [SerializeField] Text adText;
    [SerializeField] Button adButton;

    [SerializeField] Image priceImage;
    [SerializeField] Text priceText;
    [SerializeField] Text onceText;
    [SerializeField] Button onceButton;

    [SerializeField] Image priceImage_repeat;
    [SerializeField] Text priceText_repeat;
    [SerializeField] Text repeatText;
    [SerializeField] Button repeatButton;

    [SerializeField]
    Sprite GoldSprite;
    [SerializeField]
    Sprite DiaSprite;
    [SerializeField]
    Sprite AdSprite;

    [SerializeField]
    Transform MenuList;
    [SerializeField] Button listOutButton;
    [SerializeField] Button listInButton;

    [SerializeField]
    GameObject GachaAnimation;
    [SerializeField]
    SkeletonGraphic GachaAnimationSpine;
    [SerializeField]
    SkeletonGraphic SubGachaAnimationSpine;

    [SerializeField]
    GameObject CapsuleAnimation;
    [SerializeField]
    SkeletonGraphic CapsuleAnimationSpine;
    [SerializeField]
    GameObject Effect_nomal;
    [SerializeField]
    GameObject Effect_special;

    [SerializeField]
    Transform Left;
    [SerializeField]
    Transform Right;
    [SerializeField]
    Transform Top;
    [SerializeField]
    Transform Bottom;

    [SerializeField]
    UIPostersPage posterUI;
    [SerializeField]
    AudioClip gachaBGM;
    AudioSource animationAudio;

    bool isListShown = false;
    public bool bSkipAnimation = false;
    Coroutine gachaAnimation = null;
    System.Action gachaAnimDoneCallback = null;
    public GachaGameData curSelectData { get; private set; } = null;
    GameObject curBannerObject = null;
    Image adbannerRedDotImage= null;

    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);
        gachaBGM = Managers.Resource.LoadAssetsBundle<AudioClip>("AssetsBundle/Sounds/bgm/GACHA_TITLE");
        Managers.Sound.Play(gachaBGM, Sound.Bgm);
    }
    public override void Close()
    {
        base.Close();
        Managers.Scene.CurrentScene.StartBackgroundMusic(false);

        LobbyScene lobby = (Managers.Scene.CurrentScene as LobbyScene);
        if (lobby != null)
            lobby.CheckLimitedIAP();

        curSelectData = null;
    }

    public override void RefreshUI()
    {
        List<GachaGameData> ValidGacha = GachaGameData.GetValidGacha();

        posterUI.Init();
        GachaAnimation.SetActive(false);
        CapsuleAnimation.SetActive(false);

        isListShown = ValidGacha.Count <= 1;

        ValidGacha.Sort((a, b) =>
        {
            if (a.list_weight > b.list_weight)
                return -1;
            else
                return 1;
        });

        OnMenuListChange();

        Clear();

        GachaGameData defualtGachaData = null;

        foreach (GachaGameData gacha in ValidGacha)
        {
            GameObject item = gacha.GetMenuGameObject();

            if (item != null)
            {
                if (defualtGachaData == null)
                    defualtGachaData = gacha;

                if (gacha.GetID() == 3)
                {
                    if (item.transform.Find("redDot") != null)
                    {
                        if (Managers.UserData.ADEnables[3])
                            item.transform.Find("redDot").GetComponent<Image>().color = Color.white;
                        else
                            item.transform.Find("redDot").GetComponent<Image>().color = Color.clear;

                        adbannerRedDotImage = item.transform.Find("redDot").GetComponent<Image>();
                    }
                }

                item.transform.SetParent(tabContent.transform);
                item.transform.localPosition = Vector3.zero;
                item.transform.localScale = Vector3.one;

                if (item.GetComponent<Timer>() != null)
                {
                    if (item.GetComponent<Timer>().time != null)
                    {
                        var remainTime = (gacha.endTime - SBUtil.KoreanTime);
                        item.GetComponent<Timer>().InitTime(gacha.endTime, () =>
                        {
                            RefreshUI();
                        });
                    }
                }

                foreach(Coffee.UIExtensions.UIParticle up in item.GetComponentsInChildren<Coffee.UIExtensions.UIParticle>(true))
                {
                    up.RefreshParticles();
                }

                item.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnSelectGacha(gacha);
                });
            }
        }
        //#if UNITY_EDITOR
        //        foreach (CharacterGameData charData in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character))
        //        {
        //            GameObject item = Instantiate(TapMenuSample);
        //            item.transform.SetParent(TapMenuContainer.transform);
        //            item.transform.localPosition = Vector3.zero;
        //            item.transform.localScale = Vector3.one;
        //            item.GetComponentInChildren<Text>().text = charData.GetName();

        //            item.GetComponent<Button>().onClick.AddListener(() => {
        //                SBWeb.OnObtainCharacter(charData.GetID());
        //                PopupCanvas.Instance.ShowMessagePopup(charData.GetName());
        //            });
        //        }
        //#endif

        if (defualtGachaData != null)
        {
            OnSelectGacha(defualtGachaData, true);
        }
        else
        {
            Close();
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_pick_error01"));
        }

        OnUIShow();
    }

    public void OnAdvertisementRandomBox()
    {
        if (curSelectData == null)
            return;

        adButton.interactable = false;
        onceButton.interactable = false;
        repeatButton.interactable = false;

        if (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP) != null)
            PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP).CloseForce();

        //할수있는지 체크
        if (SBCommonLib.SBUtil.KoreanTime < Managers.UserData.ADSeen_GACHA.AddHours(GameConfig.Instance.GACHA_ADVERTISEMENT_TIME))
        {
            return;
        }

        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
        {
            PopupCanvas.Instance.ShowFadeText("매치대기중사용불가");
            return;
        }

        if (!Managers.ADS.IsAdvertiseReady())
        {
            PopupCanvas.Instance.ShowFadeText("광고로드실패");
            return;
        }


        Managers.ADS.TryADWithCallback(() =>
        {
            SBWeb.OnRandomBox(curSelectData.GetID(), curSelectData.adTypeID, (rewards) =>
            {
                com.adjust.sdk.AdjustEvent adjust = new com.adjust.sdk.AdjustEvent("ki9wfy");
                com.adjust.sdk.Adjust.trackEvent(adjust);

                int prevCount = Managers.UserData.GachaCount;
                int count = prevCount + 1;

                switch (count)
                {
                    case 1:
                        {
                            com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("7bkpi3");
                            com.adjust.sdk.Adjust.trackEvent(ae);
                        }
                        break;
                    case 10:
                        {
                            com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("ei2wjx");
                            com.adjust.sdk.Adjust.trackEvent(ae);
                        }
                        break;
                    case 50:
                        {
                            com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("yduee2");
                            com.adjust.sdk.Adjust.trackEvent(ae);
                        }
                        break;
                    case 100:
                        {
                            com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("d7lx0b");
                            com.adjust.sdk.Adjust.trackEvent(ae);
                        }
                        break;
                    case 200:
                        {
                            com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("f28fkb");
                            com.adjust.sdk.Adjust.trackEvent(ae);
                        }
                        break;
                }

                Managers.UserData.GachaCount = count;

                OnGachaAnimation(rewards, () =>
                {
                    PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP);
                    var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP) as GachaResultPopup;
                    popup.Init(rewards, OnRandomBox, OnUIShow);

                    adButton.interactable = true;
                    onceButton.interactable = true;
                    repeatButton.interactable = true;


                    if (adbannerRedDotImage != null)
                        adbannerRedDotImage.color = Color.clear;
                });
            });
        }, () =>
        {
            PopupCanvas.Instance.ShowFadeText("광고취소");
        });
    }

    public void OnRandomBox()
    {
        if (curSelectData == null)
            return;

        adButton.interactable = false;
        onceButton.interactable = false;
        repeatButton.interactable = false;

        if (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP) != null)
            PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP).CloseForce();

        if (EquipConfig.Config["equip_gacha_proc_group"] == curSelectData.GetID())
        {
            if (EquipConfig.Config.ContainsKey("equip_max") && Managers.UserData.MyEquips.Count > EquipConfig.Config["equip_max"])
            {
                PopupCanvas.Instance.ShowMessagePopup("소지장비갯수제한");
                return;
            }
        }

        //돈있는지 다시한번 체크
        switch (curSelectData.onceInfo.priceInfo.type)
        {
            case ASSET_TYPE.GOLD:
                if (Managers.UserData.MyGold < curSelectData.onceInfo.priceInfo.amount)
                {
                    PopupCanvas.Instance.ShowConfirmPopup("골드부족상점이동", () =>
                    {
                        PopupCanvas.Instance.ShowShopPopup(GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu, 6) as ShopMenuGameData);
                    });
                    OnUIShow();
                    return;
                }
                break;
            case ASSET_TYPE.DIA:
                if (Managers.UserData.MyDia < curSelectData.onceInfo.priceInfo.amount)
                {
                    PopupCanvas.Instance.ShowConfirmPopup("다이아부족상점이동", () =>
                    {
                        PopupCanvas.Instance.ShowShopPopup(GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu, 6) as ShopMenuGameData);
                    });
                    OnUIShow();
                    return;
                }
                break;
            case ASSET_TYPE.ITEM:
                if (Managers.UserData.GetMyItemCount(curSelectData.onceInfo.priceInfo.param) < curSelectData.onceInfo.priceInfo.amount)
                {
                    PopupCanvas.Instance.ShowFadeText("NOT_ENOUGH_FOR_COST");
                    OnUIShow();
                    return;
                }
                break;
        }

        SBWeb.OnRandomBox(curSelectData.GetID(), curSelectData.onceTypeID, (rewards) =>
        {
            switch (curSelectData.onceInfo.priceInfo.type)
            {
                case ASSET_TYPE.GOLD:
                    break;
                case ASSET_TYPE.DIA:
                    {
                        if (curSelectData.endTime == System.DateTime.MaxValue)
                        {
                            com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("o7yeab");
                            com.adjust.sdk.Adjust.trackEvent(ae);
                        }
                        else
                        {
                            com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("80gs1r");
                            com.adjust.sdk.Adjust.trackEvent(ae);
                        }
                    }
                    break;
                case ASSET_TYPE.ITEM:
                    {
                        com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("qweyj6");
                        com.adjust.sdk.Adjust.trackEvent(ae);
                    }
                    break;
            }


            int prevCount = Managers.UserData.GachaCount;
            int count = prevCount + 1;

            switch (count)
            {
                case 1:
                    {
                        com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("7bkpi3");
                        com.adjust.sdk.Adjust.trackEvent(ae);
                    }
                    break;
                case 10:
                    {
                        com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("ei2wjx");
                        com.adjust.sdk.Adjust.trackEvent(ae);
                    }
                    break;
                case 50:
                    {
                        com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("yduee2");
                        com.adjust.sdk.Adjust.trackEvent(ae);
                    }
                    break;
                case 100:
                    {
                        com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("d7lx0b");
                        com.adjust.sdk.Adjust.trackEvent(ae);
                    }
                    break;
                case 200:
                    {
                        com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("f28fkb");
                        com.adjust.sdk.Adjust.trackEvent(ae);
                    }
                    break;
            }

            Managers.UserData.GachaCount = count;

            OnGachaAnimation(rewards, () =>
            {
                PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP);
                var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP) as GachaResultPopup;
                popup.Init(rewards, OnRandomBox, OnUIShow);

                adButton.interactable = true;
                onceButton.interactable = true;
                repeatButton.interactable = true;

            });
        });
    }

    public void OnRandomBoxCount10()
    {
        if (curSelectData == null)
            return;
        adButton.interactable = false;
        onceButton.interactable = false;
        repeatButton.interactable = false;

        if (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP) != null)
            PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP).CloseForce();
        
        if(EquipConfig.Config["equip_gacha_proc_group"] == curSelectData.GetID())
        {
            if(EquipConfig.Config.ContainsKey("equip_max") && Managers.UserData.MyEquips.Count > EquipConfig.Config["equip_max"])
            {
                PopupCanvas.Instance.ShowMessagePopup("소지장비갯수제한");
                return;
            }
        }

        //돈있는지 다시한번 체크
        switch (curSelectData.repeatInfo.priceInfo.type)
        {
            case ASSET_TYPE.GOLD:
                if (Managers.UserData.MyGold < curSelectData.repeatInfo.priceInfo.amount)
                {
                    PopupCanvas.Instance.ShowConfirmPopup("골드부족상점이동", () =>
                    {
                        PopupCanvas.Instance.ShowShopPopup(GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu, 6) as ShopMenuGameData);
                    });
                    OnUIShow();
                    return;
                }
                break;
            case ASSET_TYPE.DIA:
                if (Managers.UserData.MyDia < curSelectData.repeatInfo.priceInfo.amount)
                {
                    PopupCanvas.Instance.ShowConfirmPopup("다이아부족상점이동", () =>
                    {
                        PopupCanvas.Instance.ShowShopPopup(GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu, 6) as ShopMenuGameData);
                    });
                    OnUIShow();
                    return;
                }
                break;
            case ASSET_TYPE.ITEM:
                if (Managers.UserData.GetMyItemCount(curSelectData.repeatInfo.priceInfo.param) < curSelectData.repeatInfo.priceInfo.amount)
                {
                    PopupCanvas.Instance.ShowFadeText("NOT_ENOUGH_FOR_COST");
                    OnUIShow();
                    return;
                }
                break;
        }

        SBWeb.OnRandomBox(curSelectData.GetID(), curSelectData.repeatTypeID, (rewards) =>
        {
            switch (curSelectData.onceInfo.priceInfo.type)
            {
                case ASSET_TYPE.GOLD:
                    break;
                case ASSET_TYPE.DIA:
                    {
                        if (curSelectData.endTime == System.DateTime.MaxValue)
                        {
                            com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("o7yeab");
                            com.adjust.sdk.Adjust.trackEvent(ae);
                        }
                        else
                        {
                            com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("80gs1r");
                            com.adjust.sdk.Adjust.trackEvent(ae);
                        }
                    }
                    break;
                case ASSET_TYPE.ITEM:
                    {
                        com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("qweyj6");
                        com.adjust.sdk.Adjust.trackEvent(ae);
                    }
                    break;
            }

            int prevCount = Managers.UserData.GachaCount;
            int count = prevCount + 11;

            if (prevCount == 0)
            {
                com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("7bkpi3");
                com.adjust.sdk.Adjust.trackEvent(ae);

                ae = new com.adjust.sdk.AdjustEvent("ei2wjx");
                com.adjust.sdk.Adjust.trackEvent(ae);
            }
            else if (prevCount < 50 && count >= 50)
            {
                com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("yduee2");
                com.adjust.sdk.Adjust.trackEvent(ae);
            }
            else if (prevCount < 100 && count >= 100)
            {
                com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("d7lx0b");
                com.adjust.sdk.Adjust.trackEvent(ae);
            }
            else if (prevCount < 200 && count >= 200)
            {
                com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("f28fkb");
                com.adjust.sdk.Adjust.trackEvent(ae);
            }

            Managers.UserData.GachaCount = count;

            OnGachaAnimation(rewards, () =>
            {
                PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP);
                var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP) as GachaResultPopup;
                popup.Init(rewards, OnRandomBoxCount10, OnUIShow);

                adButton.interactable = true;
                onceButton.interactable = true;
                repeatButton.interactable = true;

            });
        });
    }

    public void OnGachaAnimation(List<SBWeb.ResponseReward> rewards, System.Action cb)
    {
        GachaEquipBanner banner = bgBannerContainer.GetComponentInChildren<GachaEquipBanner>(true);
        if (banner != null)        
        {
            if (gachaAnimation != null)
            {
                StopCoroutine(gachaAnimation);
                gachaAnimation = null;
            }
            gachaAnimation = StartCoroutine(CapsuleAnim(rewards, ()=> {
                PopupCanvas.Instance.ShowRewardResult(rewards);

                adButton.interactable = true;
                onceButton.interactable = true;
                repeatButton.interactable = true;

                GachaEquipBanner banner = bgBannerContainer.GetComponentInChildren<GachaEquipBanner>(true);
                if (banner != null)
                    banner.PlayIdle();
            }));
            return;
        }

        GachaAnimationSpine.Initialize(true);
        SubGachaAnimationSpine.Initialize(true);
        bSkipAnimation = true;

        SkeletonGraphic parentAnim = bgBannerContainer.GetComponentInChildren<SkeletonGraphic>(true);
        if (parentAnim != null)
        {
            Spine.TrackEntry track = parentAnim.AnimationState.Tracks.Items[0];
            var skin = GachaAnimationSpine.Skeleton.Data.FindSkin(parentAnim.Skeleton.Skin.Name);
            if (track != null && skin != null)
            {
                GachaAnimationSpine.AnimationState.ClearTracks();
                GachaAnimationSpine.Skeleton.SetSkin(skin);

                Spine.TrackEntry subTrack = GachaAnimationSpine.AnimationState.SetAnimation(0, track.Animation, false);
                subTrack.TrackTime = track.TrackTime;
            }
        }

        if (gachaAnimation != null)
        {
            StopCoroutine(gachaAnimation);
            gachaAnimation = null;
        }
        gachaAnimation = StartCoroutine(GachaAnim(rewards, cb));
    }

    IEnumerator CapsuleAnim(List<SBWeb.ResponseReward> rewards, System.Action cb)
    {
        gachaAnimDoneCallback = cb;
        //bSkipAnimation = false;

        Effect_nomal.SetActive(false);
        Effect_special.SetActive(false);
        CapsuleAnimation.SetActive(true);
        CapsuleAnimationSpine.gameObject.SetActive(false);

        OnUIHide();

        bSkipAnimation = true;
        GachaEquipBanner banner = bgBannerContainer.GetComponentInChildren<GachaEquipBanner>(true);
        if (banner != null)
            banner.StopIdle();

        Spine.TrackEntry track = null;
        SkeletonGraphic parentAnim = bgBannerContainer.GetComponentInChildren<SkeletonGraphic>(true);
        if (parentAnim != null)
        {
            if (parentAnim.AnimationState.Data.SkeletonData.FindAnimation("f_play_0") != null)
            {
                track = parentAnim.AnimationState.SetAnimation(0, "f_play_0", false);
                yield return new WaitForSpineAnimationComplete(track);
            }
        }

        CapsuleAnimationSpine.Initialize(true);        

        animationAudio = null;
        animationAudio = Managers.Sound.Play("effect/EF_GACHA", Sound.Effect);

        if (parentAnim != null && parentAnim.AnimationState.Data.SkeletonData.FindAnimation("f_play_1") != null)
            parentAnim.AnimationState.SetAnimation(0, "f_play_1", true);

        int maxGrade = 0;
        foreach (SBWeb.ResponseReward reward in rewards)
        {
            if (reward.originReward.Type == SBWeb.ResponseReward.RandomRewardType.EQUIP ||
                reward.originReward.Type == SBWeb.ResponseReward.RandomRewardType.ITEM)
            {
                ItemGameData gameData = ItemGameData.GetItemData(reward.Id);
                if (gameData != null)
                {
                    if (gameData.grade > maxGrade)
                    {
                        maxGrade = gameData.grade;
                    }
                }
            }
        }

        switch (maxGrade)
        {
            case 3:
                CapsuleAnimationSpine.Skeleton.SetSkin("a");
                break;
            case 4:
                CapsuleAnimationSpine.Skeleton.SetSkin("s");
                break;
            default:
                CapsuleAnimationSpine.Skeleton.SetSkin("b");
                break;
        }

        CapsuleAnimationSpine.gameObject.SetActive(true);

        if (CapsuleAnimationSpine.AnimationState.Data.SkeletonData.FindAnimation("f_play_0") != null)
        {
            track = CapsuleAnimationSpine.AnimationState.SetAnimation(0, "f_play_0", false);
            yield return new WaitForSpineAnimationComplete(track);
        }

        switch (maxGrade)
        {
            case 3:
                Effect_nomal.SetActive(true);
                Effect_nomal.GetComponent<UIParticle>().Play();
                break;
            case 4:
                Effect_special.SetActive(true);
                Effect_special.GetComponent<UIParticle>().Play();
                break;
            default:
                Effect_nomal.SetActive(true);
                Effect_nomal.GetComponent<UIParticle>().Play();
                break;
        }

        if (CapsuleAnimationSpine.AnimationState.Data.SkeletonData.FindAnimation("f_play_1") != null)
        {
            track = CapsuleAnimationSpine.AnimationState.SetAnimation(0, "f_play_1", false);
            yield return new WaitForSpineAnimationComplete(track);
        }

        OnAllSkipGachaAnimation();
    }

    IEnumerator GachaAnim(List<SBWeb.ResponseReward> rewards, System.Action cb)
    {
        //if (curSelectData.GetID() != 1)
        //{
        //    posterUI.play01();
        //    yield return new WaitForSeconds(1f);
        //}

        OnUIHide();


        gachaAnimDoneCallback = cb;
        //bSkipAnimation = false;
        GachaAnimation.SetActive(true);

        Button skipButton = GachaAnimation.GetComponentInChildren<Button>(true);
        if (skipButton)
            skipButton.interactable = false;

        SubGachaAnimationSpine.AnimationState.ClearTracks();
        //가챠 사운드 삽입
        animationAudio = null;
        animationAudio = Managers.Sound.Play("effect/EF_GACHA", Sound.Effect);
        int maxGrade = 1;
        foreach (SBWeb.ResponseReward reward in rewards)
        {
            if (reward.originReward.Type == SBWeb.ResponseReward.RandomRewardType.CHARACTER)
            {
                CharacterGameData gameData = CharacterGameData.GetCharacterData(reward.Id);
                if (gameData != null)
                {
                    if (gameData.char_grade > maxGrade)
                    {
                        maxGrade = gameData.char_grade;
                    }
                }
            }
        }

        Spine.TrackEntry track = null;
        switch (maxGrade)
        {
            case 0:
            case 1:
            case 2:
                track = GachaAnimationSpine.AnimationState.SetAnimation(1, "start_gradeB", false);
                SubGachaAnimationSpine.AnimationState.SetAnimation(0, "start_gradeB", false);
                break;
            case 3:
                track = GachaAnimationSpine.AnimationState.SetAnimation(1, "start_gradeA", false);
                SubGachaAnimationSpine.AnimationState.SetAnimation(0, "start_gradeA", false);
                break;
            case 4:
                track = GachaAnimationSpine.AnimationState.SetAnimation(1, "start_gradeS", false);
                SubGachaAnimationSpine.AnimationState.SetAnimation(0, "start_gradeS", false);
                break;
        }

        track.MixDuration = 1.0f;

        yield return new WaitForSpineAnimationComplete(track);


        GachaAnimation.SetActive(false);
        posterUI.gameObject.SetActive(false);
        cb.Invoke();
        bSkipAnimation = false;
    }

    public void OnSelectMenu(int bannerID = 0)
    {
        List<GachaGameData> ValidGacha = GachaGameData.GetValidGacha();

        foreach (GachaGameData item in ValidGacha)
        {
            if (item.GetID() == bannerID)
            {
                OnSelectGacha(item, true);
                return;
            }
        }
    }
    void OnSelectGacha(GachaGameData data, bool force = false)
    {
        if (!force)
        {
            if (curSelectData == data)
                return;
        }

        curSelectData = data;

        SetBanner(data);
        SetPrice(data);
        (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHATABLE_POPUP) as GachaTablePopup).SetGachaBase(data.GetID());
    }

    void SetBanner(GachaGameData data)
    {
        if (curBannerObject != null)
            Destroy(curBannerObject);

        curBannerObject = null;
        GameObject newBanner = data.GetBannerGameObject();
        if (newBanner != null)
        {
            curBannerObject = newBanner;
            curBannerObject.transform.SetParent(bgBannerContainer.transform);
            curBannerObject.transform.localPosition = Vector3.zero;
            curBannerObject.transform.localRotation = Quaternion.identity;
            curBannerObject.transform.localScale = Vector3.one;
            curBannerObject.GetComponent<RectTransform>().sizeDelta = (transform.GetComponentInParent<Canvas>().transform as RectTransform).sizeDelta;

            PickupBanner pickup = curBannerObject.GetComponent<PickupBanner>();
            if (pickup != null)
            {
                if (curSelectData.target_char.Count > 0)
                    pickup.SetTarget(curSelectData.target_char);
            }
            eventBannerText.text = StringManager.GetString("gacha_pickup:tap:" + data.GetID());
            eventBannerSub_Text.text = StringManager.GetString("gacha_pickup:desc:" + data.GetID());
        }
        else
        {
            Close();
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_pick_error01"));
        }
    }

    void SetPrice()
    {
        CancelInvoke("SetPrice");
        SetPrice(curSelectData);
    }
    void SetPrice(GachaGameData data)
    {
        bool enable = false;

        adButton.gameObject.SetActive(false);
        if (data.adInfo != null)
        {
            string remain = data.onceInfo.priceInfo.amount.ToString();
            adButton.gameObject.SetActive(true);
            if (adbannerRedDotImage.color == Color.white)
                adButton.transform.Find("reddot").gameObject.SetActive(true);
            else
                adButton.transform.Find("reddot").gameObject.SetActive(false);

            switch (data.adInfo.priceInfo.type)
            {
                case ASSET_TYPE.GOLD:
                    priceImage_ad.sprite = GoldSprite;
                    enable = Managers.UserData.MyGold >= data.onceInfo.priceInfo.amount;
                    break;
                case ASSET_TYPE.DIA:
                    priceImage_ad.sprite = DiaSprite;
                    enable = Managers.UserData.MyDia >= data.onceInfo.priceInfo.amount;
                    break;
                case ASSET_TYPE.ITEM:
                    priceImage_ad.sprite = ItemGameData.GetItemIcon(data.onceInfo.priceInfo.param);
                    enable = Managers.UserData.GetMyItemCount(data.onceInfo.priceInfo.param) >= data.onceInfo.priceInfo.amount;
                    break;
                case ASSET_TYPE.ADVERTISEMENT:
                    priceImage_ad.sprite = AdSprite;

                    enable = Managers.UserData.ADSeen_GACHA < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > Managers.UserData.ADSeen_GACHA.AddHours(GameConfig.Instance.GACHA_ADVERTISEMENT_TIME);
                    if (enable)
                    {
                        remain = StringManager.GetString("광고시청");
                    }
                    else
                    {
                        if (Managers.UserData.ADSeen_GACHA == System.DateTime.MaxValue)
                        {
                            remain = StringManager.GetString("이용불가");
                        }
                        else
                        {
                            System.TimeSpan diff = Managers.UserData.ADSeen_GACHA.AddHours(GameConfig.Instance.GACHA_ADVERTISEMENT_TIME) - SBCommonLib.SBUtil.KoreanTime;
                            if (diff.TotalDays >= 1.0f)
                            {
                                remain = StringManager.GetString("ui_day", (int)diff.TotalDays);
                            }
                            else if (diff.TotalHours >= 1.0f)
                            {
                                remain = StringManager.GetString("ui_hour", (int)diff.TotalHours);
                                Invoke("SetPrice", (float)diff.TotalSeconds);
                            }
                            else if (diff.TotalMinutes >= 1.0f)
                            {
                                remain = StringManager.GetString("ui_min", (int)diff.TotalMinutes);
                                Invoke("SetPrice", (float)diff.TotalSeconds);
                            }
                            else if (diff.TotalSeconds >= 1.0f)
                            {
                                remain = StringManager.GetString("ui_second", (int)diff.TotalSeconds);
                                Invoke("SetPrice", (float)diff.TotalSeconds);
                            }

                            remain = StringManager.GetString("ui_left_time", remain);
                        }
                    }
                    break;
            }
            priceText_ad.text = remain;
            adButton.interactable = enable;
            priceText_ad.color = enable ? Color.white : Color.red;
            adText.text = StringManager.GetString("뽑기_" + data.onceInfo.repeats);
        }

        switch (data.onceInfo.priceInfo.type)
        {
            case ASSET_TYPE.GOLD:
                priceImage.sprite = GoldSprite;
                enable = Managers.UserData.MyGold >= data.onceInfo.priceInfo.amount;
                break;
            case ASSET_TYPE.DIA:
                priceImage.sprite = DiaSprite;
                enable = Managers.UserData.MyDia >= data.onceInfo.priceInfo.amount;
                break;
            case ASSET_TYPE.ITEM:
                priceImage.sprite = ItemGameData.GetItemIcon(data.onceInfo.priceInfo.param);
                enable = Managers.UserData.GetMyItemCount(data.onceInfo.priceInfo.param) >= data.onceInfo.priceInfo.amount;
                break;
        }
        priceText.text = data.onceInfo.priceInfo.amount.ToString();
        //onceButton.interactable = enable;
        //상점으로 보내기위해interactable true로
        onceButton.interactable = true;
        priceText.color = enable ? Color.white : Color.red;
        onceText.text = StringManager.GetString("뽑기_" + data.onceInfo.repeats);

        switch (data.repeatInfo.priceInfo.type)
        {
            case ASSET_TYPE.GOLD:
                priceImage_repeat.sprite = GoldSprite;
                enable = Managers.UserData.MyGold >= data.repeatInfo.priceInfo.amount;
                break;
            case ASSET_TYPE.DIA:
                priceImage_repeat.sprite = DiaSprite;
                enable = Managers.UserData.MyDia >= data.repeatInfo.priceInfo.amount;
                break;
            case ASSET_TYPE.ITEM:
                priceImage_repeat.sprite = ItemGameData.GetItemIcon(data.repeatInfo.priceInfo.param);
                enable = Managers.UserData.GetMyItemCount(data.repeatInfo.priceInfo.param) >= data.repeatInfo.priceInfo.amount;
                break;
        }
        priceText_repeat.text = data.repeatInfo.priceInfo.amount.ToString();
        //repeatButton.interactable = enable;
        //상점으로 보내기위해interactable true로
        repeatButton.interactable = true;
        priceText_repeat.color = enable ? Color.white : Color.red;
        repeatText.text = StringManager.GetString("뽑기_" + data.repeatInfo.repeats);
    }

    void Clear()
    {
        foreach (Transform child in tabContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ButtonBinding(int value)
    {
        switch (value)
        {
            case 1:
                PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.GACHATABLE_POPUP);
                return;
            default:
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_button_link"));
                }
                return;
        }
    }

    public void OnMenuListChange()
    {
        isListShown = !isListShown;

        listOutButton.gameObject.SetActive(false);
        listInButton.gameObject.SetActive(false);

        if (isListShown)
        {
            MenuList.DOLocalMoveX(0, 0.3f).OnComplete(() =>
            {
                listOutButton.gameObject.SetActive(isListShown);
                listInButton.gameObject.SetActive(!isListShown);
            });
        }
        else
        {
            MenuList.DOLocalMoveX(-450, 0.3f).OnComplete(() =>
            {
                listOutButton.gameObject.SetActive(isListShown);
                listInButton.gameObject.SetActive(!isListShown);
            });
        }
    }

    public void OnUIShow()
    {
        Vector2 size = (GetComponentInParent<Canvas>().transform as RectTransform).sizeDelta;

        (Left as RectTransform).DOAnchorPosX(0, 0.2f);   //* -0.5f
        (Right as RectTransform).DOAnchorPosX(0, 0.2f);  //* 0.5f
        Top.DOLocalMoveY(size.y, 0.2f);    //* 0.5f
        Bottom.DOLocalMoveY(size.y * -1, 0.2f); //* -0.5f

        SetPrice(curSelectData);
    }

    void OnUIHide()
    {
        Vector2 size = (GetComponentInParent<Canvas>().transform as RectTransform).sizeDelta;

        Left.DOLocalMoveX(-size.x, 0.2f);
        Right.DOLocalMoveX(size.x, 0.2f);
        Top.DOLocalMoveY(size.y, 0.2f);
        Bottom.DOLocalMoveY(-size.y, 0.2f);
    }

    public void OnSkipGachaAnimation()
    {
        if (GachaAnimation.activeInHierarchy)
        {
            bSkipAnimation = true;
            Button skipButton = GachaAnimation.GetComponentInChildren<Button>(true);
            if (skipButton != null)
            {
                skipButton.interactable = false;
            }
        }
    }

    public void OnAllSkipGachaAnimation()
    {
        if (gachaAnimation != null)
        {
            StopCoroutine(gachaAnimation);
            gachaAnimation = null;
        }
        if (animationAudio != null)
            animationAudio.Stop();
        gachaAnimDoneCallback?.Invoke();
        gachaAnimDoneCallback = null;

        GachaAnimation.SetActive(false);
        CapsuleAnimation.SetActive(false);

        OnUIShow();
    }
}
