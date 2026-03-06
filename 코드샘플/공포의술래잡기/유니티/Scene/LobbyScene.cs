using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBSocketSharedLib;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using SandboxPlatform.SAMANDA;
using Newtonsoft.Json.Linq;
using DG.Tweening;
using SBCommonLib;
using System;

public class LobbyScene : BaseScene, EventListener<NotifyEvent>
{
    [SerializeField] Canvas canvas;
    [Header("탑")]
    [SerializeField]
    UITop topUI;
    //todo 시간있을때 클래스화 극히 필요

    [Header("생존자")]
    [SerializeField]
    Text DefaultSurvivorCharacterName;
    [SerializeField]
    Text DefaultSurvivorCharacterDesc;
    [SerializeField]
    SelectedCharacter DefaultSurvivorCharacter;
    [SerializeField]
    UIGrade SurvivorGrade;
    [SerializeField]
    UIEnchant SurvivorEnchant;
    [SerializeField]
    Text SurvivorLevel;
    [SerializeField]
    Transform SurvivorShadow;
    [SerializeField]
    GameObject SurvivorEquipIcon;

    [Header("추격자")]
    [SerializeField]
    Text DefaultChaserCharacterName;
    [SerializeField]
    Text DefaultChaserCharacterDesc;
    [SerializeField]
    SelectedCharacter DefaultChaserCharacter;
    [SerializeField]
    UIGrade ChaserGrade;
    [SerializeField]
    UIEnchant ChaserEnchant;
    [SerializeField]
    Text ChaserLevel;
    [SerializeField]
    Transform ChaserShadow;
    [SerializeField]
    GameObject ChaserEquipIcon;

    [Header("매치 버튼")]
    [SerializeField]
    Button[] MatchButtons;
    [SerializeField]
    RectTransform matchBtnBG;
    [SerializeField]
    RectTransform matchExpendArrow;

    [Header("듀오")]
    [SerializeField] DuoUI DuoUI;

    [SerializeField] UISubMenu SubBtnMenu;

    [Header("[레드닷 버튼]")]
    public Button[] lobbyBtns;

    [Header("좌 메뉴")]
    [SerializeField] BattlePassIcon BattlePassIcon;

    [SerializeField] GameObject HottimeEffect;
    [SerializeField] List<Button> EventButtonList = new List<Button>();

    [SerializeField] GameObject GoldHottime;
    [SerializeField] GameObject ItemHottime;


    [SerializeField]
    Image SurveyButton;
    public Lobby Lobby { get; private set; }

    private void OnEnable()
    {
        this.EventStartListening();
    }

    private void OnDisable()
    {
        this.EventStopListening();
    }

    public override void StartBackgroundMusic(bool clearPopup = true)
    {
        BGM = Managers.Resource.LoadAssetsBundle<AudioClip>("AssetsBundle/Sounds/bgm/BGM_LOBBY");

        base.StartBackgroundMusic(clearPopup);
    }


    public void ReceiveMessage(sChatData chatData)
    {
        if (chatData.Type == eChatType.Receive)
        {
            OnRedDot(lobbyBtns[4].transform, true);
            SBDebug.Log($"{chatData.TargetNick}]{chatData.Message}");
        }
    }

    public void OnEvent(NotifyEvent eventType)
    {
        switch (eventType.Message)
        {
            case NotifyEvent.NotifyEventMessage.ON_USER_INFO:
                {
                    RefreshUI();
                }
                break;
        }
    }
    void Start()
    {
        ClearMatchBtn();

        Managers.Chat.SetAddMessageCallback(typeof(LobbyScene), ReceiveMessage);

        ClaerGameData();

        Lobby = new Lobby();

        Managers.Network.SendEnterLobby(EnterLobbyComplete);

        ShowUiMatching(false);

        if (Managers.PlayData.AutomaticallyEnterGame)
        {
            Managers.PlayData.ClearAutoEnterGame();
            OnEnterGame();
        }

        if (Managers.Network.IsAlive())
            RefreshUI();
        else
            OnNetworkError();

        foreach (var item in lobbyBtns)
        {
            OnRedDot(item.transform);
        }

        RefreshBattlePassIcon();

        SBWeb.GetLobbyData(CheckNotificationSeq);

        //gameserver init
        Managers.GameServer.Disconnect();

        EventTimeCheck();
        HotTimeCheck();
    }

    private void HotTimeCheck()
    {
        int hottimeFlag = GameConfig.Instance.GetHotTimeFlag();

        HottimeEffect.SetActive(hottimeFlag > 0);

        bool isGoldHotTime = (hottimeFlag & 1) > 0;
        bool isItemHotTime = (hottimeFlag & 2) > 0;

        GoldHottime.SetActive(isGoldHotTime);
        ItemHottime.SetActive(isItemHotTime);

        int goldRate = 100;
        int itemRate = 100;

        foreach (ConfigGameData cd in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.config))
        {
            switch (cd.key)
            {
                case "hot_time_rate":
                    if (isGoldHotTime)
                    {
                        goldRate = int.Parse(cd.value);
                        if (goldRate <= 0)
                            goldRate = 100;
                    }
                    break;
                case "hot_time_item_rate":
                    if (isItemHotTime)
                    {
                        itemRate = int.Parse(cd.value);
                        if (itemRate <= 0)
                            itemRate = 100;
                    }
                    break;
            }
        }



        GoldHottime.transform.Find("num").GetComponent<Text>().text = goldRate.ToString() + "%";
        ItemHottime.transform.Find("num").GetComponent<Text>().text = itemRate.ToString() + "%";
    }

    private void CheckNotificationSeq()
    {
#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
        if (EasyMobile.Advertising.IsInitialized())
            Managers.ADS.IsAdvertiseReady();
#endif
        if (!Managers.UserData.ShownNotice)
        {
            SAMANDA.Instance.UI.SetMainCloseCallback(CheckNotificationSeq);
            SAMANDA.Instance.UI.SetUIState(LOGIN_STATE.MAIN_OPEN);
            Managers.UserData.ShowFirstNotice();
            return;
        }

        if (!Managers.UserData.ShownTutorial)
        {
            PopupCanvas.Instance.ShowHelpPopup(TutorialPopup.HelpTapType.RULE, CheckNotificationSeq);
            return;
        }

        if (!CheckRankseasonID())
        {
            (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.RANK_POPUP) as RankPopup).bundleType = false;
            PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.RANK_POPUP, CheckNotificationSeq);
            return;
        }

        if (Managers.UserData.IsLobbyShown == false)
        {
            (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.RANKREWARD_POPUP) as RankRewardPopup).SyncData();

            SBWeb.CheckDodge(() =>
            {
                PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("닷지안내"), CheckNotificationSeq);
            }, CheckNotificationSeq);

            Managers.UserData.SetLobbyVisit();
            return;
        }

        if (Managers.UserData.IsRankChanged())
        {
            var value1 = CacheUserData.GetInt("saved_rank", Managers.UserData.MyRank.GetID());
            var value2 = Managers.UserData.MyRank.GetID();
            PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.ADVANCEMENT_POPUP, CheckNotificationSeq);
            bool check = value1 < value2;
            var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.ADVANCEMENT_POPUP) as AdvancementPopup;
            popup.Init(check);

            return;
        }

        //SurveyButton.color = Managers.UserData.EnableSurvay ? Color.white : Color.gray;
        if (!Managers.UserData.IsAttendanceChecked(1))
        {
            PopupCanvas.Instance.ShowAttandancePopup(1, CheckNotificationSeq);
            return;
        }

        foreach (EventScheduleData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.event_schedule))
        {
            if (data.event_type != 3)
            {
                continue;
            }

            if (data.use <= 0)
                continue;

            if (Managers.UserData.IsAttendanceChecked(data.GetID()))
            {
                continue;
            }

            if (data.IsEventEnable())
            {
                data.ShowAttendance(CheckNotificationSeq);
                return;
            }
        }

        CheckLimitedIAP();

        if (Managers.FriendData.DUO.IsDuoPlaying())
        {
            if (Managers.FriendData.DUO.IsHost())
            {
                SetEnableMatch(Managers.FriendData.DUO.GuestReady);
            }
            else
            {
                SetEnableMatch(!Managers.FriendData.DUO.GuestReady);
            }
        }
        else
        {
            SetEnableMatch(true);
        }

        if (PlayerPrefs.GetInt("Store_rate", 0) <= 1)
        {
            if (Managers.UserData.RankPlayCount >= 3)
            {
                PlayerPrefs.SetInt("Store_rate", 2);

                PopupCanvas.Instance.ShowConfirmPopup(StringManager.GetString("ui_review"), StringManager.GetString("button_check"), StringManager.GetString("button_cancel"),
                    () =>
                    {
                        //예 선택 시 마켓 주소로
                        Application.OpenURL(
#if UNITY_IOS
                        GameConfig.Instance.APPLE_STORE_URL
#else
                        GameConfig.Instance.GOOGLE_STORE_URL
#endif
                        );
                    },
                    () =>
                    {
                        //취소 선택 시 
                        PopupCanvas.Instance.ShowConfirmPopup(StringManager.GetString("ui_review_no"), StringManager.GetString("button_check"), StringManager.GetString("button_cancel"), () =>
                            {
                                string url = GameConfig.Instance.SUPPORT_URL;
                                string paramString = "";

                                long[] tfID =
                                {
                        900011725766, //닉네임 / 닉네임
                        5354068699161, //회원 번호 / 유저넘버
                            };

                                for (int i = 0; i < tfID.Length; i++)
                                {
                                    if (i == 0)
                                    {
                                        if (url.Contains("?"))
                                            paramString += "&";
                                        else
                                            paramString += "?";
                                    }
                                    else
                                        paramString += "&";

                                    paramString += "tf_" + tfID[i].ToString() + "=";
                                    switch (i)
                                    {
                                        case 0:
                                            paramString += Managers.UserData.MyName;
                                            break;
                                        case 1:
                                            paramString += Managers.UserData.MyUserID;
                                            break;
                                    }
                                }

                                Application.OpenURL(url + paramString);
                            }, CheckNotificationSeq);
                    });
                return;
            }
        }

    }

    public void RefreshUI()
    {
        if (Managers.UserData == null)
        {
            PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("ui_data_error"));

            return;
        }

        topUI.Refresh();
        RefreshDuoInfo();

        int chaser = Managers.UserData.MyDefaultChaserCharacter;
        int survivor = Managers.UserData.MyDefaultSurvivorCharacter;

        DefaultSurvivorCharacterName.SetText(StringManager.Instance.GetName(GameDataManager.DATA_TYPE.character, survivor), SHelper.TEXT_TYPE.SURVIVOR_CHARACTER);
        DefaultSurvivorCharacterDesc.text = StringManager.Instance.GetDesc(GameDataManager.DATA_TYPE.character, survivor);
        //DefaultSurvivorCharacterName.color = new Color(0.137f, 0.980f, 0.0f, 1.0f);

        UserCharacterData survivorUserData = Managers.UserData.GetMyCharacterInfo(survivor);
        DefaultSurvivorCharacter.SetCharacter(survivor, survivorUserData.curEquip);
        SurvivorGrade.SetGrade(survivorUserData.characterData.char_grade);
        SurvivorEnchant.SetEnchant(survivorUserData.enchant);
        SurvivorLevel.text = "Lv." + survivorUserData.lv.ToString();
        SurvivorEquipIcon.SetActive(survivorUserData.curEquip != null);
        SurvivorShadow.localScale = Vector3.one * survivorUserData.characterData.scale_ratio;


        DefaultChaserCharacterName.SetText(StringManager.Instance.GetName(GameDataManager.DATA_TYPE.character, chaser), SHelper.TEXT_TYPE.CHASER_CHARACTER);
        DefaultChaserCharacterDesc.text = StringManager.Instance.GetDesc(GameDataManager.DATA_TYPE.character, chaser);
        //DefaultChaserCharacterName.color = new Color(0.933f, 0.372f, 0.047f, 1.0f);

        UserCharacterData chaserUserData = Managers.UserData.GetMyCharacterInfo(chaser);
        DefaultChaserCharacter.SetCharacter(chaser, chaserUserData.curEquip);
        ChaserGrade.SetGrade(chaserUserData.characterData.char_grade);
        ChaserEnchant.SetEnchant(chaserUserData.enchant);
        ChaserLevel.text = "Lv." + chaserUserData.lv.ToString();
        ChaserEquipIcon.SetActive(chaserUserData.curEquip != null);
        ChaserShadow.localScale = Vector3.one * chaserUserData.characterData.scale_ratio;

        LayoutRebuilder.ForceRebuildLayoutImmediate((topUI.transform.parent as RectTransform));

        //OnCompleteSurvivorAnimation(null);
        //OnCompleteChaserAnimation(null);
    }

    bool CheckRankseasonID()
    {
        var rank_uid = Managers.UserData.MyRankingSeasonID;
        if (rank_uid > Managers.UserData.RANK_SEANSON_ID)
        {
            Managers.UserData.RANK_SEANSON_ID = rank_uid;
            return false;
        }

        return true;
    }

    void OnNetworkError()
    {
        PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("네트워크오류"), () =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
        });
    }
    void ClaerGameData()
    {
        Game.ClearInstance();
        Managers.PlayData.Clear();
    }

    void EnterLobbyComplete()
    {
        //uiConnect.SetActive(false);
        //uiEnterGame.SetActive(true);
    }

    public void OnEnterGame()
    {
        if (Managers.FriendData.DUO.IsDuoPlaying())
        {
            if (Managers.FriendData.DUO.IsHost())
            {
                Managers.FriendData.DUO.SendDuoMatch();
            }
            else
            {
                Managers.FriendData.DUO.SendDuoGuestMatch();
            }
            return;
        }
        //Managers.GameServerNetwork.Clear
        ReqMatch();
        ShowUiMatching(true, true);
    }

    void ReqMatch()
    {
        MatchInfoPopup matchInfo = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP) as MatchInfoPopup;
        matchInfo.SetCount(0, 0);

        Managers.Network.SendGameMatch();

        SetEnableMatch(false);
    }

    public void OnCreatePracticeGame(string password, int mapNo)
    {
        Managers.Network.SendCreatePracticeMatch(password, mapNo);

        ShowUiMatching(true);

        SetEnableMatch(false);
    }

    public void OnJoinPracticeGame(string nick)
    {
        Managers.Network.SendJoinPracticeMatch(nick);

        ShowUiMatching(true);

        SetEnableMatch(false);
    }

    public void OnPasswordPracticeGame(string nick, string password)
    {
        Managers.Network.SendPasswordPracticeMatch(nick, password);

        ShowUiMatching(true);

        SetEnableMatch(false);
    }

    public void OnMatchSuccess()
    {
        Managers.Network.SendLeaveLobby();
    }

    public void OnMatchFail(int result)
    {
        if (result == 2 || result == 3)
        {
            ShowUiMatching(false);
        }
    }

    public void ShowUiMatching(bool isShow, bool rankMode = false)
    {
        if (isShow)
        {
            PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP);
            SetEnableMatch(false);

            if (Array.Find(lobbyBtns, _ => _.name == "btnParty") != null)
            {
                var btn = Array.Find(lobbyBtns, _ => _.name == "btnParty");
                foreach (Transform item in btn.transform)
                {
                    if (item.GetComponent<MaskableGraphic>() != null)
                    {
                        item.GetComponent<MaskableGraphic>().color = Color.gray;
                    }
                }
                btn.interactable = false;
            }
        }
        else
        {
            PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP);
            SetEnableMatch(true);

            if (Array.Find(lobbyBtns, _ => _.name == "btnParty") != null)
            {
                var btn = Array.Find(lobbyBtns, _ => _.name == "btnParty");
                foreach (Transform item in btn.transform)
                {
                    if (item.GetComponent<MaskableGraphic>() != null)
                    {
                        item.GetComponent<MaskableGraphic>().color = Color.white;
                    }
                }
                btn.interactable = true;
            }
        }

        MatchInfoPopup matchInfo = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP) as MatchInfoPopup;
        if (matchInfo)
        {
            if (rankMode)
            {
                matchInfo.SetRankMode();
            }
            else
            {
                matchInfo.SetPracticeMode();
            }
        }
    }

    public void LoadRoom()
    {
        Managers.Scene.LoadScene(SceneType.Room);
    }

    public void LoadGame()
    {
        Managers.Scene.LoadScene(SceneType.Game);
    }

    public override void Clear()
    {
        Managers.Chat.RemoveAddMessageCallback(typeof(LobbyScene));
    }

    public void OnUserProfile()
    {
        SAMANDA.Instance.OnAccountUI();
    }

    public void ShowReadyNotice()
    {
        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.DEVLOPING_NOTICE_POPUP);
    }

    public void SelectMenuButton(int type)
    {
        SubBtnMenu.Clear();

        switch (type)
        {
            case 0:
                PopupCanvas.Instance.ShowShopPopup();
                return;
            case -2:
                SAMANDA.Instance.UI.SetUIState(LOGIN_STATE.MAIN_OPEN);
                return;
            case 2:
                PopupCanvas.Instance.ShowInventoryPopup();
                return;
            case 6:
#if !UNITY_EDITOR
                PopupCanvas.Instance.ShowFadeText("개발중컨텐츠");
                return;
#endif
                break;
            case 30:
                {
                    CharacterPopup charPopup = (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP) as CharacterPopup);
                    charPopup.OnSuvivorUI();
                    charPopup.OnSelectCharacter(CharacterGameData.GetCharacterData(Managers.UserData.MyDefaultSurvivorCharacter));
                    PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP);
                }
                return;
            case 31:
                {
                    CharacterPopup charPopup = (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP) as CharacterPopup);
                    charPopup.OnChaserUI();
                    charPopup.OnSelectCharacter(CharacterGameData.GetCharacterData(Managers.UserData.MyDefaultChaserCharacter));
                    PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP);
                }
                return;
            case 16:
                if (!GameConfig.Instance.USE_CUSTOM_MATCH_SERVICE)
                {
                    PopupCanvas.Instance.ShowMessagePopup("msg_develop");
                    return;
                }

                SetEnableMatch(false);
                OnMatchBtn();
                PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.PRACTICEMATCHING_POPUP);
                return;
            case 17:
                SetEnableMatch(false);
                PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.PRACTICEMATCHING_POPUP);
                (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.PRACTICEMATCHING_POPUP) as PracticeMatchingPopup).JoinButton(3);
                return;
            //case 20:
            //SBWeb.GetGameResult("421d1cae-64b3-4f1a-af3c-15d4d5b19040");
            //break;
            case 22:
                PopupCanvas.Instance.ShowQuestPopup();
                return;

            case 25:
                {
                    if (!GameConfig.Instance.USE_DUO_MATCH_SERVICE)
                    {
                        PopupCanvas.Instance.ShowMessagePopup("msg_develop");
                        return;
                    }
                }
                break;

            default:
                break;
        }
        PopupCanvas.Instance.ShowPopup((PopupCanvas.POPUP_TYPE)type);
    }

    public void SetEnableMatch(bool enable)
    {
        if (Managers.FriendData.DUO.Host != null && Managers.FriendData.DUO.Guest == null)
            enable = false;

        foreach (Button btn in MatchButtons)
        {
            btn.interactable = enable;

            Text text = btn.GetComponentInChildren<Text>();
            if (text != null)
            {
                if (enable == false)
                    text.color = btn.colors.disabledColor;
                else
                    text.color = btn.colors.normalColor;
            }

            var symbol = btn.transform.Find("Image");
            if (symbol != null)
            {
                if (enable == false)
                    symbol.GetComponent<Image>().color = btn.colors.disabledColor;
                else
                    symbol.GetComponent<Image>().color = btn.colors.normalColor;

            }
        }

        RefreshDuoInfo();

        HotTimeCheck();
    }

    public bool SetEnableSendDuo()
    {
        if (Managers.FriendData.DUO.Host != null || Managers.FriendData.DUO.Guest != null)
            return false;

        if (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP).IsOpening())
            return false;
        return true;
    }

    public void RefreshDuoInfo()
    {
        DuoUI.OnRefresh();
        if (DuoUI.gameObject.activeSelf || PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP).IsOpening())
        {
            ClearMatchBtn();

            if (Array.Find(lobbyBtns, _ => _.name == "btnParty") != null)
            {
                var btn = Array.Find(lobbyBtns, _ => _.name == "btnParty");
                foreach (Transform item in btn.transform)
                {
                    if (item.GetComponent<MaskableGraphic>() != null)
                    {
                        item.GetComponent<MaskableGraphic>().color = Color.gray;
                    }
                }
                btn.interactable = false;
            }
        }
        else
        {
            if (Array.Find(lobbyBtns, _ => _.name == "btnParty") != null)
            {
                var btn = Array.Find(lobbyBtns, _ => _.name == "btnParty");
                foreach (Transform item in btn.transform)
                {
                    if (item.GetComponent<MaskableGraphic>() != null)
                    {
                        item.GetComponent<MaskableGraphic>().color = Color.white;
                    }
                }
                btn.interactable = true;
            }
        }
    }

    public void ClearMatchBtn()
    {
        matchExpendArrow.rotation.Set(0, 0, 1, 0);
        (MatchButtons.SingleOrDefault(_ => _.name == "btnPlay")).GetComponent<Button>().interactable = false;
        matchBtnBG.sizeDelta = new Vector2(358, 214);
        (MatchButtons.SingleOrDefault(_ => _.name == "btnPlay").transform as RectTransform).anchoredPosition = new Vector2((MatchButtons.SingleOrDefault(_ => _.name == "btnPlay").transform as RectTransform).anchoredPosition.x, 0);
    }
    public void OnMatchBtn()
    {
        if (Managers.FriendData.DUO.Host != null)
            return;

        if (matchExpendArrow.rotation.z == 0)
        {
            var sizeY = (MatchButtons.SingleOrDefault(_ => _.name == "btnPlay").transform as RectTransform).sizeDelta.y;
            (MatchButtons.SingleOrDefault(_ => _.name == "btnPlay")).GetComponent<Button>().interactable = false;
            var deltaSize = matchBtnBG.sizeDelta - new Vector2(0, sizeY);
            (MatchButtons.SingleOrDefault(_ => _.name == "btnPlay").transform as RectTransform).DOAnchorPosY(0, 0.2f);
            matchBtnBG.GetComponent<Button>().interactable = false;
            matchBtnBG.DOSizeDelta(deltaSize, 0.2f).OnComplete(() =>
            {
                matchExpendArrow.DORotate(new Vector3(0, 0, 180), 0f);
                matchBtnBG.GetComponent<Button>().interactable = true;
            });
        }
        else if (matchExpendArrow.rotation.z == 1)
        {
            var sizeY = (MatchButtons.SingleOrDefault(_ => _.name == "btnPlay").transform as RectTransform).sizeDelta.y;
            (MatchButtons.SingleOrDefault(_ => _.name == "btnPlay")).GetComponent<Button>().interactable = true;

            var deltaSize = matchBtnBG.sizeDelta + new Vector2(0, sizeY);
            (MatchButtons.SingleOrDefault(_ => _.name == "btnPlay").transform as RectTransform).DOAnchorPosY(149, 0.2f);
            matchBtnBG.GetComponent<Button>().interactable = false;
            matchBtnBG.DOSizeDelta(deltaSize, 0.2f).OnComplete(() =>
            {
                matchExpendArrow.DORotate(Vector3.zero, 0f);
                matchBtnBG.GetComponent<Button>().interactable = true;
            });
        }

        var matchinfo = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP) as MatchInfoPopup;
        if (matchinfo.IsOpening())
            SetEnableMatch(false);
    }

    /// <summary>
    /// </summary>
    /// <param name="button">
    ///  버튼 순서
    ///  0 -> 캐릭터 버튼, 1 -> 상점 버튼, 2 -> 가챠 버튼, 3 -> 퀘스트 버튼, 4 -> 친구 버튼, 5 -> 듀오파티 버튼
    /// </param>
    /// <param name="enable"></param>
    public void OnRedDot(Transform button, bool enable = false)
    {
        var dot = button.Find("RedDot");
        if (dot == null)
            return;

        dot.gameObject.SetActive(enable);
    }
    public void CheckQuestRedDot(JToken res)
    {
        Managers.UserData.SetMyQuestDBData(res);

        CheckQuestRedDot();
    }

    public void CheckShopRedDot()
    {
        bool redAble = true;
        if (!Managers.UserData.ADEnables[1] && !Managers.UserData.ADEnables[2])
            redAble = false;
        else
            redAble = true;

        if (!redAble)
        {
            JObject limitInfo = (JObject)Managers.UserData.LimitedIAP;
            if (limitInfo != null)
            {
                if (limitInfo.ContainsKey("new") && limitInfo["new"].Type == JTokenType.Array)
                {
                    JArray newLimitIAP = (JArray)limitInfo["new"];

                    foreach (JToken notiTarget in newLimitIAP)
                    {
                        int targetID = notiTarget.Value<int>();
                        ShopItemGameData data = ShopItemGameData.GetShopData(targetID);
                        if (data.IsBuyLimitValid())
                        {
                            redAble = true;
                            break;
                        }
                    }
                }
            }
        }

        OnRedDot(lobbyBtns[1].transform, redAble);
    }
    public void CheckQuestRedDot()
    {
        foreach (var dic in Managers.UserData.userQuestDic)
        {
            var data = QuestData.GetQuestData(dic.Key);

            if (data.quest_type == 5)
                continue;

            if (!LimitedQuestTimeCheck(data))
                continue;
            else
            {
                if (data.clear_count <= dic.Value)
                {
                    bool enableQuest = true;

                    foreach (int prev in QuestData.GetPrevQuests(dic.Key))
                    {
                        enableQuest = Managers.UserData.IsContainClearQuest(prev);

                        if (!enableQuest)
                            break;
                    }

                    if (!enableQuest)
                        continue;

                    bool rewarded = Managers.UserData.IsContainClearQuest(dic.Key);

                    if (!rewarded)
                    {
                        OnRedDot(lobbyBtns[3].transform, true);
                        return;
                    }
                }
            }
        }
        OnRedDot(lobbyBtns[3].transform, false);
    }
    public bool LimitedQuestTimeCheck(QuestData data)
    {
        var limitedList = Managers.Data.GetData(GameDataManager.DATA_TYPE.limited_quest_info);
        foreach (LimitedQuestInfoData list in limitedList)
        {
            if (list.quest_group_uid == data.quest_type)
            {
                if (SBUtil.KoreanTime <= DateTime.Parse(list.start_day) || SBUtil.KoreanTime > DateTime.Parse(list.end_day))
                    return false;
                else
                    return true;

            }
        }
        return true;
    }

    public void OnLeftScreenTouch()
    {
        //CancelInvoke("OnLeftScreenTouch");

        string animName = "";
        switch (UnityEngine.Random.Range(0, 4))
        {
            case 0:
                animName = "f_action_0";
                break;
            case 1:
                animName = "f_victory_0";
                break;
            case 2:
                animName = "f_failure_0";
                break;
            case 3:
                animName = "f_interecting_0";
                break;
        }

        var target = DefaultSurvivorCharacter.GetSkeletonGraphic();
        if (target == null || target.AnimationState == null)
            return;

        target.AnimationState.SetAnimation(0, animName, false);
        target.AnimationState.Complete += OnCompleteSurvivorAnimation;
    }

    public void OnCompleteSurvivorAnimation(Spine.TrackEntry entry)
    {
        var target = DefaultSurvivorCharacter.GetSkeletonGraphic();

        target.AnimationState.Complete -= OnCompleteSurvivorAnimation;
        target.AnimationState.SetAnimation(0, "f_idle_0", true);

        //Invoke("OnLeftScreenTouch", 5.0f + (UnityEngine.Random.value * 5.0f));
    }

    public void OnRightScreenTouch()
    {
        //CancelInvoke("OnRightScreenTouch");

        string animName = "";
        switch (UnityEngine.Random.Range(0, 4))
        {
            case 0:
                animName = "f_action_0";
                break;
            case 1:
                animName = "f_victory_0";
                break;
            case 2:
                animName = "f_failure_0";
                break;
            case 3:
                animName = "f_action_1";
                break;
        }

        var target = DefaultChaserCharacter.GetSkeletonGraphic();
        target.AnimationState.SetAnimation(0, animName, false);
        target.AnimationState.Complete += OnCompleteChaserAnimation;
    }

    public void OnCompleteChaserAnimation(Spine.TrackEntry entry)
    {
        var target = DefaultChaserCharacter.GetSkeletonGraphic();
        target.AnimationState.Complete -= OnCompleteChaserAnimation;
        target.AnimationState.SetAnimation(0, "f_idle_0", true);

        //Invoke("OnRightScreenTouch", 5.0f + (UnityEngine.Random.value * 5.0f));
    }

    public void RefreshBattlePassIcon()
    {
        BattlePassIcon.Refresh();
    }

    public void CheckLimitedIAP()
    {
        JObject limitInfo = (JObject)Managers.UserData.LimitedIAP;
        if (limitInfo != null)
        {
            if (limitInfo.ContainsKey("new") && limitInfo["new"].Type == JTokenType.Array)
            {
                JArray newLimitIAP = (JArray)limitInfo["new"];
                foreach (JToken notiTarget in newLimitIAP)
                {
                    int targetID = notiTarget.Value<int>();
                    if (!CacheUserData.GetBoolean("IAP_NOTI_" + targetID.ToString()))
                    {
                        CacheUserData.SetBoolean("IAP_NOTI_" + targetID.ToString(), true);

                        ShopItemGameData data = ShopItemGameData.GetShopData(targetID);
                        if (data != null)
                        {
                            PopupCanvas.Instance.ShowBuyPopup(data, (cnt) =>
                            {
                                Managers.IAP.TryPurchase((uint)data.GetID(), ShopPackageGameData.GetIAPConstants(data.GetID()), (responseArr) =>
                                {
                                    PopupCanvas.Instance.ShowFadeText("결제성공");
                                    Managers.UserData.UpdateMyShopInfo(data.GetID(), 1);
                                    CheckLimitedIAP();
                                }, (responseArr) =>
                                {
                                    PopupCanvas.Instance.ShowFadeText("결제실패");
                                    CheckLimitedIAP();
                                });
                            }, CheckLimitedIAP);
                            newLimitIAP.Remove(notiTarget);
                            return;
                        }
                    }
                }
            }
        }

        RankType curRankType = Managers.UserData.MyRank;
        int[] rank_iaps = { 2, 5, 8, 11 };

        foreach (int id in rank_iaps)
        {
            if (id <= curRankType.GetID())
            {
                int pid = 0;
                switch (id)
                {
                    case 2:
                        pid = 90006; break;
                    case 5:
                        pid = 90007; break;
                    case 8:
                        pid = 90008; break;
                    case 11:
                        pid = 90009; break;
                }

                if (pid == 0)
                    continue;

                if (Managers.UserData.GetMyShopHistory(pid) > 0)
                    continue;

                if (!CacheUserData.GetBoolean("IAP_NOTI_" + id.ToString()))
                {
                    CacheUserData.SetBoolean("IAP_NOTI_" + id.ToString(), true);
                    ShopItemGameData data = ShopItemGameData.GetShopData(pid);

                    if (data != null)
                    {
                        PopupCanvas.Instance.ShowBuyPopup(data, (cnt) =>
                        {
                            Managers.IAP.TryPurchase((uint)data.GetID(), ShopPackageGameData.GetIAPConstants(data.GetID()), (responseArr) =>
                            {
                                PopupCanvas.Instance.ShowFadeText("결제성공");
                                Managers.UserData.UpdateMyShopInfo(data.GetID(), 1);
                                CheckLimitedIAP();
                            }, (responseArr) =>
                            {
                                PopupCanvas.Instance.ShowFadeText("결제실패");
                                CheckLimitedIAP();
                            });
                        }, CheckLimitedIAP);
                        return;
                    }
                }
            }
        }
    }

    public void EventTimeCheck()
    {
        var tableData = Managers.Data.GetData(GameDataManager.DATA_TYPE.event_schedule);

        int[] index_array = {
            1001,
            1002,
            2003,
        };
        var index = 0;
        foreach (Button button in EventButtonList)
        {
            if (index_array.Length <= index)
                return;

            EventScheduleData data = Managers.Data.GetData(GameDataManager.DATA_TYPE.event_schedule, index_array[index], true) as EventScheduleData;
            if (data == null)
                continue;

            bool useLobbyIcon = data.use > 0;

            if (useLobbyIcon && EventTimeState(data.start_time, data.ui_duration))
            {
                SBDebug.Log($"{data.GetDesc()} event :: activity");
                button.gameObject.name = data.GetDesc();
                button.gameObject.SetActive(true);
                button.image.sprite = data.event_icon;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    switch (data.uid)
                    {

                        case 1001:
                            {
                                //할로윈
                                PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.HALLOWEENEVENT_POPUP);
                            }
                            break;
                        case 1002:
                            {
                                //크리스마스
                                PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.CHRISTMAS_POPUP);
                            }
                            break;
                        case 2003:
                            {
                                //설날
                                PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.KOREANEWYEAR_POPUP);
                            }
                            break;
                        default:
                            break;
                    }
                });

                EventIcon component = EventButtonList[index].GetComponent<EventIcon>();
                if (component)
                {
                    component.SetData(data);
                }
            }
            else
            {
                button.gameObject.SetActive(false);
            }
            index++;
        }

        foreach (var data in Managers.Data.GetData(GameDataManager.DATA_TYPE.event_schedule))
        {
            //배경 설정
            GameObject bg = null;
            
            LobbyBackGround lobbyBG = canvas.GetComponentInChildren<LobbyBackGround>();

            if (lobbyBG != null && lobbyBG.type == LobbyBackGround.BackType.Default)
            {
                lobbyBG.gameObject.SetActive(false);

                if ((data as EventScheduleData).use_background == 1)
                {
                    switch ((data as EventScheduleData).uid)
                    {
                        case 1001://할로윈 배경
                            {
                                bg = GameObject.Instantiate(Managers.Resource.Load<GameObject>("Prefabs/UI/UI_Common/bg_halloween"), canvas.transform);
                            }
                            break;
                        case 1002://크리스마스
                            {
                                bg = GameObject.Instantiate(Managers.Resource.Load<GameObject>("Prefabs/UI/UI_Common/bg_christmas"), canvas.transform);
                            }
                            break;
                        case 2004: //새해 겨울 배경
                            bg = GameObject.Instantiate(Managers.Resource.Load<GameObject>("Prefabs/UI/UI_Common/bg_winter"), canvas.transform);
                            break;
                    }
                }
                if (bg == null)
                    lobbyBG.gameObject.SetActive(true);
            }

            if (bg != null)
                bg.transform.SetAsFirstSibling();
        }

    }
    public bool EventTimeState(DateTime start, DateTime end)
    {
        if (start <= SBUtil.KoreanTime && SBUtil.KoreanTime <= end)
            return true;
        return false;
    }


    [ContextMenu("TestButton")]
    public void TestButton()
    {
        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.ADVANCEMENT_POPUP);
        bool check = CacheUserData.GetInt("saved_rank", Managers.UserData.MyRank.GetID()) < Managers.UserData.MyRank.GetID();
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.ADVANCEMENT_POPUP) as AdvancementPopup;
        popup.Init(true);
    }
}
