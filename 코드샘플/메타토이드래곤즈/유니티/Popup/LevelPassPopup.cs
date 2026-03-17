using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class LevelPassPopup : Popup<PopupData>
    {
        public static int SHOP_LEVEL_PASS_GOODS_ID = 999998;

        [Header("UserInfo")]
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

        [Header("tableView")]
        [SerializeField]
        TableView passTableView = null;

        [Header("buttons")]
        [SerializeField] Button leftButton = null;
        [SerializeField] Button rightButton = null;

        [SerializeField] Text leftButtonLevelText = null;
        [SerializeField] Text rightButtonLevelText = null;

        [SerializeField] ItemFrame leftItem = null;
        [SerializeField] ItemFrame rightItem = null;

        [SerializeField] GameObject leftBubble = null;
        [SerializeField] GameObject rightBubble = null;

        [SerializeField] Button passSpecialButton = null;

        [SerializeField] Button getRewardButton = null;

        [SerializeField] Button closeButton = null;

        [Header("overlay")]
        [SerializeField] LevelPassObject overLayObj = null;
        [SerializeField] CanvasGroup overLayCanvasGroup = null;
        [SerializeField] Image overlayBgTargetImage = null;
        [SerializeField] Sprite overlayRewardedSprite = null;
        [SerializeField] Sprite overlayNoRewardSprite = null;

        [SerializeField]
        private GuildBaseInfoObject guildBaseObj;


        [Header("AccountBonusInfo")]
        [SerializeField]
        private GameObject AccountBonusInfo;
        [SerializeField]
        private GameObject AccountBonusCategoryClone;
        [SerializeField]
        private GameObject AccountBonusClone;
        [SerializeField]
        private Transform AccountBonusParent;
        [SerializeField]
        private Image AccountBonusAllBtnImage;
        [SerializeField]
        private Image AccountBonusDetailBtnImage;
        [SerializeField]
        private Sprite SelectedImage;
        [SerializeField]
        private Sprite NormalImage;

        bool isTableInit = false;
        List<AccountData> tableData = new List<AccountData>();
        List<ITableData> tableViewItemList = new List<ITableData>();
        bool initScrollPos = false;


        int leftIndex;//버튼 및 화살표 LV 표시 인덱스
        int rightIndex;

        bool isTweening = false;
        bool AccountBonusViewDetailMode = false;
        #region OpenPopup
        public static LevelPassPopup OpenPopup()
        {
            return OpenPopup(new PopupData());
        }
        static LevelPassPopup OpenPopup(PopupData data = null)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<LevelPassPopup>(data);
        }
        public static void RequestLevelPassPopup(VoidDelegate success = null, VoidDelegate fail = null)
        {
            WWWForm form = new WWWForm();
            NetworkManager.Send("pass/levelpass", form, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
                {
                    if (jsonData.ContainsKey("level_pass") && SBFunc.IsJTokenType(jsonData["level_pass"], JTokenType.Object))
                    {
                        BattlePassManager.Instance.SetLevelPassData((JObject)jsonData["level_pass"]);
                    }

                    success?.Invoke();
                }
                else
                    fail?.Invoke();
            }, (failString) =>
            {
                fail?.Invoke();
            });
        }
        #endregion
        void SetSubCamTextureOn()
        {
            Town.Instance?.SetSubCamState(true);
            UICanvas.Instance.StartBackgroundBlurEffect();
        }

        void SetSubCamTextureOff()
        {
            Town.Instance?.SetSubCamState(false);
            UICanvas.Instance.EndBackgroundBlurEffect();
        }
        public override void InitUI()
        {
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);

            initScrollPos = true;
            isTweening = false;

            SetSubCamTextureOn();
            RefreshUserInfo();

            SetTable();
            DrawScrollView();

            SetMoveScrollUserLevel();
            RefreshPassBuyButton();
            RefreshGetRewardButton();
            SetVisibleCloseButton(true);
            SetGuildInfo();

            OffAccoundBonusInfo();
        }

        void SetGuildInfo()
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

        void SetTable()
        {
            if (passTableView != null && !isTableInit)
            {
                passTableView.OnStart();
                isTableInit = true;

                SetTableData();
            }
        }

        void SetTableData()
        {
            tableData = AccountData.GetTotalRewardList().ToList();
        }

        public void DrawScrollView()
        {
            if (passTableView == null || tableData == null)
                return;

            tableViewItemList.Clear();
            if (tableData.Count > 0)
            {
                for (var i = 0; i < tableData.Count; i++)
                {
                    if (i == tableData.Count - 1)//만렙은 scrollview에서 제외
                        continue;

                    var data = tableData[i];
                    if (data == null)
                        continue;

                    tableViewItemList.Add(data);
                }
            }

            //가장 마지막 데이터 overLay처리 - WJ - 2024.02.15 overlay 만렙 보상 고정으로 변경
            if (overLayObj != null)
            {
                var lastData = GetStateByIndex(tableData.Count - 1);
                if (lastData.Count == 2)
                    overLayObj.Init(tableData[tableData.Count - 1], lastData[0], lastData[1]);
                else
                    overLayObj.Init(tableData[tableData.Count - 1], eBattlePassRewardState.LOCK, eBattlePassRewardState.LOCK);

                if (overlayBgTargetImage != null)
                    overlayBgTargetImage.sprite = lastData.Count > 0 && lastData[0] == eBattlePassRewardState.REWARDED ? overlayRewardedSprite : overlayNoRewardSprite;
            }

            passTableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) =>
            {
                if (node == null)
                    return;

                var frame = node.GetComponent<LevelPassObject>();
                if (frame == null)
                    return;

                var passData = (AccountData)item;

                var index = GetIndexByTableData(passData);
                if (index >= 0)
                {
                    var data = GetStateByIndex(index);
                    if (data.Count == 2)
                        frame.Init(passData, data[0], data[1]);
                    else
                        frame.Init(passData, eBattlePassRewardState.LOCK, eBattlePassRewardState.LOCK);
                }

                SetCurrentArrangeButtonNode();//프레임마다 계산하는게 올바를진 모르겠음.
            }));

            passTableView.ReLoad(initScrollPos);
            initScrollPos = false;
        }

        /// <summary>
        /// 첫번째 param - normal
        /// 두번째 param - special
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        List<eBattlePassRewardState> GetStateByIndex(int _index)
        {
            List<eBattlePassRewardState> ret = new List<eBattlePassRewardState>();
            var levelPassList = BattlePassManager.Instance.LevelPassRewardStates?.ToList();
            var levelSpecialPassList = BattlePassManager.Instance.LevelSpecialRewardStates?.ToList();

            if (levelPassList == null || levelPassList.Count <= 0 || levelSpecialPassList == null || levelSpecialPassList.Count <= 0 ||
                (levelPassList.Count != levelSpecialPassList.Count))//스페셜과 일반이 사이즈 다르거나, 0일 때
            {
                return ret;
            }

            if (levelPassList.Count <= _index || _index < 0)
                return ret;

            ret.Add(levelPassList[_index]);
            ret.Add(levelSpecialPassList[_index]);

            return ret;
        }


        void SetCurrentArrangeButtonNode()//버튼 양 사이드 버튼 세팅하기
        {
            var curVisible = passTableView.GetVisibleNodes();
            var objList = curVisible.Values.ToList();

            List<LevelPassObject> tempList = new List<LevelPassObject>();
            foreach (var obj in objList)
            {
                var comp = obj.GetComponent<LevelPassObject>();
                if (comp == null)
                    continue;

                tempList.Add(comp);
            }

            tempList.Sort((a, b) =>
            {
                if (a.PassLevel > b.PassLevel)
                    return 1;
                else if (a.PassLevel == b.PassLevel)
                    return 0;
                else
                    return -1;
            });

            if (tempList.Count <= 0)
                return;

            var minLevel = tempList[0].PassLevel;
            var maxLevel = tempList[tempList.Count - 1].PassLevel;

            if (minLevel < 100)
            {
                leftIndex = 10;
                rightIndex = 200;
            }
            else
            {
                var modulerCheck = minLevel % 100 == 0;
                var minDefault = minLevel / 100 * 100;
                leftIndex = modulerCheck ? minDefault - 100 : minDefault;
                if (leftIndex <= 0)
                {
                    leftIndex = 10;
                    rightIndex = 200;
                }
                else
                    rightIndex = leftIndex + 100 + 100;

                if (rightIndex >= 980)
                    rightIndex = 990;
            }

            var isMinButtonCheck = minLevel < 20;
            if (leftButton != null)
                leftButton.gameObject.SetActive(!isMinButtonCheck);
            if (leftButtonLevelText != null)
                leftButtonLevelText.text = leftIndex.ToString();
            var leftItemData = GetAccountDataByLevel(leftIndex);
            var leftDataIndex = GetIndexByTableData(leftItemData);

            var states_left = GetStateByIndex(leftDataIndex);
            var leftNormalstate = states_left.Count > 0 ? states_left[0] : eBattlePassRewardState.LOCK;
            var leftSpecialstate = states_left.Count > 1 ? states_left[1] : eBattlePassRewardState.LOCK;

            SetBubbleItem(leftBubble, leftItem, leftItemData, leftNormalstate, leftSpecialstate);

            var isMaxButtonCheck = maxLevel >= 980;
            if (rightButton != null)
                rightButton.gameObject.SetActive(!isMaxButtonCheck);
            if (rightButtonLevelText != null)
                rightButtonLevelText.text = rightIndex.ToString();
            var rightItemData = GetAccountDataByLevel(rightIndex);
            var rightDataIndex = GetIndexByTableData(rightItemData);

            var states_right = GetStateByIndex(rightDataIndex);
            var rightNormalstate = states_right.Count > 0 ? states_right[0] : eBattlePassRewardState.LOCK;
            var rightSpecialstate = states_right.Count > 1 ? states_right[1] : eBattlePassRewardState.LOCK;

            SetBubbleItem(rightBubble, rightItem, rightItemData, rightNormalstate, rightSpecialstate);
        }

        void SetBubbleItem(GameObject _target, ItemFrame _targetItem, AccountData itemData, eBattlePassRewardState _normalState, eBattlePassRewardState _specialState)
        {
            if (_target == null)
                return;

            if (_targetItem == null)
                return;

            if (itemData == null)
                return;

            _target.SetActive(_normalState != eBattlePassRewardState.REWARDED);
            _targetItem.SetFrameItem(itemData.NormalReward);

            if (_specialState == eBattlePassRewardState.LOCK || _specialState == eBattlePassRewardState.REWARD_ABLE)//스페셜 패스 추가 조건
            {
                _target.SetActive(true);
                if (_normalState == eBattlePassRewardState.REWARDED)//이미 일반 보상 받음
                    _targetItem.SetFrameItem(itemData.SpecialReward);
            }
        }

        /// <summary>
        /// SetCurrentArrangeButtonNode() 이 함수 안에 종속 조건으로 하려니, UI가 구려서 
        /// scrollrect OnValueChanged Event에 달아놓음
        /// </summary>
        /// <param name="_pos"></param>
        /// WJ - 2024.02.15 - 만렙 보상 고정뷰로 변경

        public void SetOverLayCanvasGroupAlpha(Vector2 _pos)
        {
            var XPos = _pos.x;
            var alpaDiff = XPos <= 0.96 ? 1 : (float)(1 - XPos) * 25;
            if (overLayCanvasGroup != null)
                overLayCanvasGroup.alpha = alpaDiff;
        }
        public void OnClickButtonMove(bool _isRight)
        {
            if (isTweening)
                return;

            isTweening = true;

            var passObj = GetDataByIndex(_isRight ? rightIndex : leftIndex);
            if (passObj == null)
            {
                isTweening = false;
                return;
            }

            passTableView.ScrollMoveTweenItem(passObj, eTableViewAnchor.FIRST, () =>
            {
                SetCurrentArrangeButtonNode();
                isTweening = false;
            });
        }

        public ITableData GetDataByIndex(int _index)//테이블 뷰안의 itempool index
        {
            foreach (var item in tableViewItemList)
            {
                var data = item as AccountData;
                if (data == null)
                    continue;
                if (data.LEVEL == _index)
                    return data;
            }

            return null;
        }
        public int GetIndexByTableData(AccountData _data)//데이터 테이블 상의 index
        {
            return tableViewItemList.FindIndex(element => element as AccountData == _data);
        }

        AccountData GetAccountDataByLevel(int _level)
        {
            return tableData.Find(element => element.LEVEL == _level);
        }
        /// <summary>
        /// 현재 레벨에 가장 가까운 위치 자동 이동(현재 레벨 바로 아래 위치가 가장 왼쪽으로 가게)
        /// </summary>
        void SetMoveScrollUserLevel()
        {
            var currentUserLevel = User.Instance.UserData.Level;
            AccountData currentMinData = null;
            foreach (var item in tableViewItemList)
            {
                var data = item as AccountData;
                if (data == null)
                    continue;
                if (data.LEVEL <= currentUserLevel)
                    currentMinData = data;
            }

            if (currentMinData != null)
            {
                passTableView.ScrollMoveTweenItem(currentMinData, eTableViewAnchor.FIRST, () =>
                {
                    SetCurrentArrangeButtonNode();
                });
            }
        }

        #region userInfo
        void RefreshUserInfo()
        {
            RefreshPortrait();
            RefreshNickAndLv();
            RefershExp();
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

        void RefershExp()
        {
            int myLv = User.Instance.UserData.Level;
            int myExp = User.Instance.UserData.Exp;
            int accountLevelData = AccountData.GetLevel(myLv).TOTAL_EXP;
            int devider = AccountData.GetLevel(myLv).EXP;

            var tb = AccountData.GetLevel(myLv + 1);
            int Need = 0;
            if (tb != null)
                Need = AccountData.GetLevel(myLv + 1).EXP;

            expSlider.maxValue = devider;
            expSlider.value = myExp - accountLevelData;
            string expValue = SBFunc.CommaFromNumber(Need - (myExp - accountLevelData));
            expText.text = string.Format("Next {0}", expValue);
        }
        #endregion

        /// <summary>
        /// 보상 받기 요청
        /// </summary>
        public void OnClickGetReward()
        {
            if (!IsGetRewardCondition())
            {
                ToastManager.On(StringData.GetStringByStrKey("보상획득불가"));
                return;
            }

            List<Asset> AllItems = new List<Asset>();
            var passState = BattlePassManager.Instance.LevelPassRewardStates;
            bool isVip = BattlePassManager.Instance.isUserLevelPassVIP;
            var spPassState = BattlePassManager.Instance.LevelSpecialRewardStates;
            for (int i = 0, count = tableData.Count; i < count; ++i)
            {
                var infoData = tableData[i];
                if (infoData == null)
                    continue;

                if (infoData.LEVEL > User.Instance.UserData.Level)
                    continue;

                if (passState[i] == eBattlePassRewardState.REWARD_ABLE && infoData.NormalReward != null)
                    AllItems.Add(infoData.NormalReward);
                if (spPassState[i] == eBattlePassRewardState.REWARD_ABLE)
                {
                    if (isVip && infoData.SpecialReward != null)
                        AllItems.Add(infoData.SpecialReward);
                }
            }

            if (User.Instance.CheckInventoryGetItem(AllItems))
            {
                IsFullBagAlert();
                return;
            }

            WWWForm param = new WWWForm();
            NetworkManager.Send("pass/levelpassreward", param, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                {
                    if ((int)jsonData["rs"] == (int)eApiResCode.OK)
                    {
                        BattlePassManager.Instance.ClearLevelFlag();//보상 플래그 끔
                        if (jsonData.ContainsKey("level_pass") && SBFunc.IsJTokenType(jsonData["level_pass"], JTokenType.Object))
                        {
                            BattlePassManager.Instance.SetLevelPassData((JObject)jsonData["level_pass"]);
                        }
                        if (SBFunc.IsJArray(jsonData["reward"]))
                        {
                            var assetList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["reward"]));
                            var dragonReward = assetList.Find(element => element.GoodType == eGoodType.CHARACTER);
                            bool isDragonReward = dragonReward != null;
                            SetVisibleCloseButton(false);

                            if (isDragonReward)
                            {
                                var dragonID = dragonReward.ItemNo;
                                var isSuccess = true;
                                var hasDragon = User.Instance.DragonData.IsUserDragon(dragonID);
                                DragonCompoundInfoData info = new DragonCompoundInfoData(dragonID, isSuccess, !hasDragon);
                                List<DragonCompoundInfoData> dragonList = new List<DragonCompoundInfoData>() { info };

                                DragonCompoundResultPopupData newPopupData = new DragonCompoundResultPopupData(dragonList, () =>
                                {

                                    ShowRewardList(assetList);

                                }, " ");//StringData.GetStringByIndex(100002334) //보상획득
                                PopupManager.OpenPopup<DragonCompoundResultPopup>(newPopupData);
                            }
                            else
                                ShowRewardList(assetList);
                        }
                        DrawScrollView();
                        RefreshGetRewardButton();
                    }
                }
                else if ((int)jsonData["rs"] == (int)eApiResCode.INVENTORY_FULL)
                {
                    IsFullBagAlert();
                }
            }, (string res) =>
            {
                SetVisibleCloseButton(false);
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("서버요청실패"), () =>
                {
                    ShowCloseButtonAndHideTopUI();
                }, () =>
                {
                    ShowCloseButtonAndHideTopUI();
                }, () =>
                {
                    ShowCloseButtonAndHideTopUI();
                });
            });
        }

        void ShowRewardList(List<Asset> _rewardList)
        {
            SystemRewardPopup.OpenPopup(_rewardList, () =>
            {
                ShowCloseButtonAndHideTopUI();
            }, true);
        }

        void IsFullBagAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                () =>
                {
                    //메인팝업 열기
                    PopupManager.OpenPopup<InventoryPopup>();
                    PopupManager.ClosePopup<PostListPopup>();
                }, () => { }, () => { });
        }

        /// <summary>
        /// 받을 보상이 있는 상태인지
        /// </summary>
        /// <returns></returns>
        bool IsGetRewardCondition()
        {
            return BattlePassManager.Instance.GetLevelPassReddotCondition();
        }

        void RefreshGetRewardButton()
        {
            if (getRewardButton != null)
                getRewardButton.SetButtonSpriteState(IsGetRewardCondition());
        }

        public void OnClickBuyLevelPassSpecial()
        {
            var data = ShopGoodsData.Get(SHOP_LEVEL_PASS_GOODS_ID);
            if (data != null)
            {
                var popup = PopupManager.OpenPopup<ShopBannerPopup>(new ShopBuyPopupData(data));
                popup.SetBattlePassUI(false);
                SetVisibleCloseButton(false);
                popup.SetBuyCallBack(() =>
                {
                    BattlePassManager.Instance.isUserLevelPassVIP = true;
                    BattlePassManager.Instance.RefreshSpecialRewardState();//레벨 패스 가능 데이터로 변경
                    RefreshPassBuyButton();
                    RefreshGetRewardButton();
                    DrawScrollView();
                });
                popup.SetExitCallback(() =>
                {
                    ShowCloseButtonAndHideTopUI();
                });
            }
        }

        void RefreshPassBuyButton()
        {
            if (passSpecialButton != null)
                passSpecialButton.gameObject.SetActive(!BattlePassManager.Instance.isUserLevelPassVIP);
        }

        void SetVisibleCloseButton(bool _isVisible)
        {
            if (closeButton != null)
                closeButton.gameObject.SetActive(_isVisible);
        }

        void ShowCloseButtonAndHideTopUI()
        {
            PopupTopUIRefreshEvent.Hide(true);
            SetVisibleCloseButton(true);
        }

        public override void ClosePopup()
        {
            base.ClosePopup();

            UserStatusEvent.RefreshPortrait();
            UserStatusEvent.RefreshLevel();
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            SetSubCamTextureOff();
        }

        public void OnClickForceClosePopup()
        {
            SetExitCallback(null);

            ClosePopup();
        }

        public void OnAccountBonusTotal()
        {
            AccountBonusViewDetailMode = false;
            OnAccountBonusInfo();
        }

        public void OnAccountBonusDetail()
        {
            AccountBonusViewDetailMode = true;
            OnAccountBonusInfo();
        }

        public void OnAccountBonusInfo()
        {
            AccountBonusInfo.SetActive(true);

            foreach (Transform child in AccountBonusParent)
            {
                if (child == AccountBonusClone.transform)
                    continue;

                if (child == AccountBonusCategoryClone.transform)
                    continue;

                Destroy(child.gameObject);
            }

            AccountBonusCategoryClone.SetActive(true);
            AccountBonusClone.SetActive(true);

            AccountBonusAllBtnImage.sprite = AccountBonusViewDetailMode ? NormalImage : SelectedImage;
            AccountBonusDetailBtnImage.sprite = AccountBonusViewDetailMode ? SelectedImage : NormalImage;

            if (AccountBonusViewDetailMode)
            {
                if (GuildManager.Instance.MyGuildInfo != null)
                {
                    AdditionalStatus UserBuff = GuildManager.Instance.MyGuildInfo.GetGuildBuff();
                    bool hasBuff = false;
                    for (eStatusType type_ = eStatusType.START; type_ < eStatusType.MAX; ++type_)
                    {
                        if (hasBuff)
                            break;
                        for (eStatusCategory cate_ = eStatusCategory.START; cate_ < eStatusCategory.TOTAL; cate_++)
                        {
                            float val = UserBuff.GetStatus(cate_, type_);
                            if (val > 0.0f)
                                hasBuff = true;

                            if (hasBuff)
                                break;
                        }
                    }

                    if (hasBuff)
                    {
                        var category = Instantiate(AccountBonusCategoryClone, AccountBonusParent);
                        category.transform.Find("Category").GetComponent<Text>().text = StringData.GetStringByStrKey("버프스탯콘텐츠::GUILD");

                        for (eStatusType type_ = eStatusType.START; type_ < eStatusType.MAX; ++type_)
                        {
                            for (eStatusCategory cate_ = eStatusCategory.START; cate_ < eStatusCategory.TOTAL; cate_++)
                            {
                                float val = UserBuff.GetStatus(cate_, type_);
                                if (val <= 0.0f)
                                    continue;

                                var effectClone = Instantiate(AccountBonusClone, AccountBonusParent);
                                var dataClone = effectClone.GetComponent<AccountBonusObj>();
                                if (dataClone == null)
                                {
                                    Destroy(effectClone);
                                    continue;
                                }

                                switch (cate_)
                                {
                                    case eStatusCategory.ADD:
                                    case eStatusCategory.ADD_BUFF:
                                        dataClone.InitUI(type_, eStatusValueType.ADD_VALUE, val);//complete 갱신 추가
                                        break;
                                    case eStatusCategory.RATIO:
                                    case eStatusCategory.RATIO_BUFF:
                                        dataClone.InitUI(type_, eStatusValueType.PERCENT, val);//complete 갱신 추가
                                        break;
                                    default:
                                        Debug.LogError("error");
                                        break;
                                }
                            }
                        }
                    }
                }

                var BuffByContent = User.Instance.UserData.ExtraStatBuff.BuffByContent;
                foreach (var ub in BuffByContent)
                {
                    eExtraStatContent key = ub.Key;
                    bool hasValue = ub.Value.Count > 0;
                    if (key == eExtraStatContent.ARTBLOCK && !User.Instance.ENABLE_P2E)
                        hasValue = false;

                    var buffUnits = ub.Value;
                    
                    if (!hasValue)
                        continue;

                    hasValue = false;
                    foreach (var buffUnit in buffUnits)
                    {
                        if (buffUnit.value > 0.0f)
                        {
                            hasValue = true;
                            break;
                        }
                    }

                    var category = Instantiate(AccountBonusCategoryClone, AccountBonusParent);
                    category.transform.Find("Category").GetComponent<Text>().text = StringData.GetStringByStrKey("버프스탯콘텐츠::" + key);

                    foreach (var buffUnit in buffUnits)
                    {
                        if (buffUnit.value <= 0.0f)
                            continue;

                        var effectClone = Instantiate(AccountBonusClone, AccountBonusParent);
                        var dataClone = effectClone.GetComponent<AccountBonusObj>();
                        if (dataClone == null)
                        {
                            Destroy(effectClone);
                            continue;
                        }

                        switch (buffUnit.category)
                        {
                            case eStatusCategory.ADD:
                            case eStatusCategory.ADD_BUFF:
                                dataClone.InitUI(buffUnit.type, eStatusValueType.ADD_VALUE, buffUnit.value);//complete 갱신 추가
                                break;
                            case eStatusCategory.RATIO:
                            case eStatusCategory.RATIO_BUFF:
                                dataClone.InitUI(buffUnit.type, eStatusValueType.PERCENT, buffUnit.value);//complete 갱신 추가
                                break;
                            default:
                                Debug.LogError("error");
                                break;
                        }
                    }
                }

            }
            else
            {
                var category = Instantiate(AccountBonusCategoryClone, AccountBonusParent);
                category.transform.Find("Category").GetComponent<Text>().text = StringData.GetStringByStrKey("버프스탯콘텐츠::TOTAL");
                AdditionalStatus guildBuff = null;
                AdditionalStatus BuffByContent = User.Instance.UserData.ExtraStatBuff.GetUserBuff();

                if (GuildManager.Instance.MyGuildInfo != null)
                {
                    guildBuff = GuildManager.Instance.MyGuildInfo.GetGuildBuff();
                }

                for (eStatusType type_ = eStatusType.START; type_ < eStatusType.MAX; ++type_)
                {
                    for (eStatusCategory cate_ = eStatusCategory.START; cate_ < eStatusCategory.TOTAL; cate_++)
                    {
                        float val = BuffByContent.GetStatus(cate_, type_);
                        if(guildBuff != null)
                            val += guildBuff.GetStatus(cate_, type_);

                        if (val <= 0.0f)
                            continue;

                        var effectClone = Instantiate(AccountBonusClone, AccountBonusParent);
                        var dataClone = effectClone.GetComponent<AccountBonusObj>();
                        if (dataClone == null)
                        {
                            Destroy(effectClone);
                            continue;
                        }

                        switch (cate_)
                        {
                            case eStatusCategory.ADD:
                            case eStatusCategory.ADD_BUFF:
                                dataClone.InitUI(type_, eStatusValueType.ADD_VALUE, val);//complete 갱신 추가
                                break;
                            case eStatusCategory.RATIO:
                            case eStatusCategory.RATIO_BUFF:
                                dataClone.InitUI(type_, eStatusValueType.PERCENT, val);//complete 갱신 추가
                                break;
                            default:
                                Debug.LogError("error");
                                break;
                        }
                    }
                }
            }


            AccountBonusCategoryClone.SetActive(false);
            AccountBonusClone.SetActive(false);

            AccountBonusParent.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 1.0f;
        }

        public void OffAccoundBonusInfo()
        {
            AccountBonusInfo.SetActive(false);
            AccountBonusViewDetailMode = false;
        }
    }
}

