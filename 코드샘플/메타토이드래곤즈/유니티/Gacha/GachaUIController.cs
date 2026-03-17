using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    #region GachaDatas

    public class GachaResult
    {
        public int ID { get; protected set; }

        public GachaResult(int _id)
        {
            ID = _id;
        }
    }
    public class GachaResultDragonAndPet : GachaResult
    {
        public bool IsNew { get; protected set; }
        public int GRADE { get; protected set; }
        public string NAME { get; protected set; }
        protected GameObject clone;

        public GachaResultDragonAndPet(int _id, GameObject _clone) : base(_id)
        {
            clone = _clone;
        }

        public GameObject GetPrefab() { return clone; }
    }
    public class GachaResultSpine : GachaResultDragonAndPet
    {
        protected string prefab;

        /// <summary>
        /// 직접 사용은 불가능, 하위 클래스인 GachaResultDragonSpine 또는 GachaResultPetSpine을 사용하여 접근
        /// </summary>
        /// <param name="_id">Tag ID</param>
        protected GachaResultSpine(int _id, GameObject _clone) : base(_id, _clone)
        {

        }

        public string SKIN { get; protected set; }

    }

    public class GachaResultDragonSpine : GachaResultSpine
    {
        public GachaResultDragonSpine(int _id, GameObject _clone, bool _isNew) : base(_id, _clone)
        {
            CharBaseData cData = CharBaseData.Get(ID.ToString());
            NAME = StringData.GetStringByStrKey(cData._NAME);
            prefab = cData.IMAGE;
            GRADE = cData.GRADE;
            SKIN = cData.SKIN;
            IsNew = _isNew;
        }
    }

    public class GachaResultPetSpine : GachaResultSpine
    {
        public GachaResultPetSpine(int _id, GameObject _clone) : base(_id, _clone)
        {
            PetBaseData pData = PetBaseData.Get(ID);
            NAME = StringData.GetStringByStrKey(pData._NAME);
            prefab = pData.IMAGE;
            GRADE = pData.GRADE;
            SKIN = pData.SKIN;
        }
    }

    public class GachaResultPortrait : GachaResultDragonAndPet
    {
        public Sprite THUMBNAIL { get; protected set; }
        public string BACKGROUND { get; protected set; }
        public int ELEMENT { get; protected set; }

        public GachaResultPortrait(int _id, GameObject _clone) : base(_id, _clone)
        {
            CharBaseData cData = CharBaseData.Get(ID.ToString());
            NAME = StringData.GetStringByStrKey(cData._NAME);
            BACKGROUND = cData.BACKGROUND;
            THUMBNAIL = cData.GetThumbnail();
            ELEMENT = cData.ELEMENT;
            GRADE = cData.GRADE;
        }
    }
    public enum eGachaType
    {
        NONE = -1,
        DRAGON = 100000001,
        PET = 100000002,
    }
    #endregion GachaDatas

    public class GachaUIController : MonoBehaviour, EventListener<GachaEvent>, EventListener<ShopBuyPopupEvent>
    {
        enum eRefreshType
        {
            ALL,
            MENU,
            BUTTON,
        }

        public eGachaGroupMenu defaultTab = eGachaGroupMenu.CLASS;
        [SerializeField] Text gachaTitleText = null;
        [SerializeField] GameObject[] gachaUIOfftargets;

        [Space(10)]
        [SerializeField] AudioSource[] bgmSources;
        BGMData[] bgmDatas;

        [Header("[Gacha BG]")]
        [SerializeField] Image bgImage = null;
        [SerializeField] Sprite[] bgSprites = null;

        [Header("[Gacha Tab]")]
        [SerializeField] ScrollRect gachaTabScrollRect = null;
        [SerializeField] GameObject gachaTabMenuPrefab = null;
        
        [Header("[Gacha Menu Item]")]
        [SerializeField] GameObject commonGachaLayerObject = null;
        [SerializeField] ScrollRect gachaMenuScrollRect = null;
        [SerializeField] GameObject gachaMenuItemPrefab = null;

        [Header("[Gacha Button]")]
        [SerializeField] GameObject buttonLayoutObject = null;
        [SerializeField] GameObject gachaButtonPrefab = null;
        [Header("[Pickup Prefabs]")]
        [SerializeField] GameObject ArrowPanel = null;
        [SerializeField] GameObject Pickup_FIRE = null;
        [SerializeField] GameObject Pickup_WATER = null;
        [SerializeField] GameObject Pickup_EARTH = null;
        [SerializeField] GameObject Pickup_WIND = null;
        [SerializeField] GameObject Pickup_LIGHT = null;
        [SerializeField] GameObject Pickup_DARK = null;

        GachaGroupData currentGroupData = null;             // 현재 선택한 가챠 그룹 데이터
        GachaMenuData currentMenuData = null;             // 현재 선택한 가챠 메뉴 데이터

        List<GachaTabMenuItem> gachaTabMenuList = new();
        List<GachaMenuItem> gachaMenuItemList = new();
        List<GachaMenuButton> gachaMenuButtonList = new();

        List<GachaGroupData> gachaGroupDataList = new();

        bool isGachaTypeChanged = false;        // Gacha Spine 갱신 판별용

        int currentSelectedMenuIndex = 0;
        int currentSelectedTabIndex = 0;

        bool isTutorialing = false;

        private void OnEnable()
        {
            EventManager.AddListener<GachaEvent>(this);
            EventManager.AddListener<ShopBuyPopupEvent>(this);

            // 바로가기 기능으로 인하여 OnEnable에서 Init실행
            gachaGroupDataList = GachaGroupData.GetAll();
            InitUI();
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<GachaEvent>(this);
            EventManager.RemoveListener<ShopBuyPopupEvent>(this);
            UIGachaIconObject.CheckButtonStates();
        }

        public void OnEvent(GachaEvent eventType)
        {
            switch (eventType.Event)
            {
                case GachaEvent.GachaEventEnum.SetUIVisible:
                    if (eventType.isUIVisible)
                        DrawEndUISetting();
                    else
                        DrawUISetting();
                    break;
                case GachaEvent.GachaEventEnum.EndProduction:
                    SoundManager.Instance.PushBGM(bgmDatas[0]);
                    SoundManager.Instance.PlayBGM();
                    DrawEndUISetting();
                    break;
                case GachaEvent.GachaEventEnum.SetGachaTab:
                    SetTargetTabState(eventType.groupMenuType);
                    break;
            }
        }

        public void OnEvent(ShopBuyPopupEvent eventType)
        {
            switch (eventType.EventType)
            {
                case ShopBuyPopupEvent.eEventType.Buy:
                    ReloadAll();
                    break;
                default:
                    break;
            }
        }

        void InitUI()
        {
            UIManager.Instance.InitUI(eUIType.Gacha);
            isTutorialing = TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.DragonGacha);
#if DEBUG
            Debug.LogFormat("tutorial state : {0}", isTutorialing);
#endif
            var groups = GachaGroupData.GetAll();
            groups.Sort((data1, data2) => {
                if (data1.weight > data2.weight)
                    return 1;
                else if (data1.weight < data2.weight)
                    return -1;
                else
                    return 0;
            });
            //currentGroupData = GachaGroupData.Get((int)defaultTab);
            if (groups.Count > 0)
            {
                currentGroupData = groups[0];
                defaultTab = (eGachaGroupMenu)currentGroupData.key;
            }
            else
                currentGroupData = GachaGroupData.Get((int)defaultTab);

            if (currentGroupData.GetGachaMenus().Count == 0)
            {
                foreach(var group in GachaGroupData.GetAll())
                {
                    if(group.GetGachaMenus().Count > 0)
                    {
                        currentGroupData = group;
                    }
                }                
            }
            // bgm set
            bgmDatas = new BGMData[bgmSources.Length];

            for (int i = 0; i < bgmSources.Length; i++)
            {
                bgmDatas[i] = new BGMData();
                bgmDatas[i].isSmoothChange = true;
                bgmDatas[i].BGMAudioSource = bgmSources[i];
            }

            SoundManager.Instance.PushBGM(bgmDatas[0]);

            isGachaTypeChanged = true;

            InitTabMenu();
            ReloadAll();
        }

        private void Start()
        {
            if (isTutorialing)
            {
#if DEBUG
                Debug.Log("tutorial Start Next");
#endif
                TutorialManager.tutorialManagement.NextTutorialStart();
            }
        }

        void InitTabMenu()
        {
            if (gachaGroupDataList == null || gachaGroupDataList.Count <= 0) return;

            gachaGroupDataList = gachaGroupDataList.OrderBy(group => group.weight).ToList();

            for (int i = 0; i < gachaGroupDataList.Count; ++i)
            {
                GameObject newTab = Instantiate(gachaTabMenuPrefab, gachaTabScrollRect.content);
                GachaTabMenuItem tabMenuItem = newTab.GetComponent<GachaTabMenuItem>();
                tabMenuItem.InitTabMenu(gachaGroupDataList[i], this);
                // 튜토리얼 가챠 메뉴 세팅 --
                if (isTutorialing)
                {
                    if(gachaGroupDataList[i].key == TutorialManager.tutorialManagement.GetCurTutoPrivateKey())
                    {
                        TutorialManager.Instance.SetRecordObject(500101, tabMenuItem.GetComponent<RectTransform>());
                    }
                }
                // --
                gachaTabMenuList.Add(tabMenuItem);
            }

            currentSelectedTabIndex = 0;

            gachaTabScrollRect.verticalNormalizedPosition = 1;
            LayoutRebuilder.ForceRebuildLayoutImmediate(gachaTabScrollRect.content.GetComponent<RectTransform>());
        }

        void SetTargetTabState(eGachaGroupMenu type)
        {
            if (gachaTabMenuList == null || gachaTabMenuList.Count <= 0) return;

            GachaTabMenuItem targetTabMenu = gachaTabMenuList.Find(tabmenu => tabmenu.CurrentGroupData.key == (int)type);
            if (targetTabMenu != null)
            {
                UpdateGachaGroupTab(targetTabMenu.CurrentGroupData);
            }
        }

        void SetGachaMenuItem()
        {
            List<GachaMenuData> menuList = currentGroupData.GetGachaMenus();

            ArrowPanel.SetActive(false);
            bool isFirstMenuSelected = currentMenuData == null;
            // 튜토리얼 가챠 리스트 세팅 --
            if (isTutorialing && currentGroupData.key == TutorialManager.tutorialManagement.GetCurTutoPrivateKey())
            {
                int tutorialMenu = TutorialManager.tutorialManagement.GetCurTutoPrivateKey(1);
                for (int i = 0; i < menuList.Count; ++i)
                {
                    if (tutorialMenu > 0 && tutorialMenu != menuList[i].key)
                        continue;
                    GameObject newMenu = Instantiate(gachaMenuItemPrefab, gachaMenuScrollRect.content);
                    GachaMenuItem menuItem = newMenu.GetComponent<GachaMenuItem>();
                    TutorialManager.Instance.SetRecordObject(500102, newMenu.GetComponent<RectTransform>());
                    menuItem.InitMenuItem(menuList[i], this);
                    gachaMenuItemList.Add(menuItem);
                    menuItem.SetSelectedState(false);
                }
            }
            // --
            else
            {
                bool pickup_added = false;
                for (int i = 0; i < menuList.Count; ++i)
                {
                    GameObject newMenu = null;
                    if (menuList[i].resource_type == 4)
                    {
                        if(pickup_added)
                            ArrowPanel.SetActive(true);
                        if (!pickup_added)
                            pickup_added = true;

                        var charData = CharBaseData.Get(menuList[i].resource);
                        switch (charData.ELEMENT_TYPE)
                        {
                            case eElementType.FIRE:
                                newMenu = Instantiate(Pickup_FIRE, gachaMenuScrollRect.content);
                                break;
                            case eElementType.WATER:
                                newMenu = Instantiate(Pickup_WATER, gachaMenuScrollRect.content);
                                break;
                            case eElementType.EARTH:
                                newMenu = Instantiate(Pickup_EARTH, gachaMenuScrollRect.content);
                                break;
                            case eElementType.WIND:
                                newMenu = Instantiate(Pickup_WIND, gachaMenuScrollRect.content);
                                break;
                            case eElementType.LIGHT:
                                newMenu = Instantiate(Pickup_LIGHT, gachaMenuScrollRect.content);
                                break;
                            case eElementType.DARK:
                                newMenu = Instantiate(Pickup_DARK, gachaMenuScrollRect.content);
                                break;
                        }
                    }
                    else
                    {
                        newMenu = Instantiate(gachaMenuItemPrefab, gachaMenuScrollRect.content);                        
                    }

                    GachaMenuItem menuItem = newMenu.GetComponent<GachaMenuItem>();
                    menuItem.InitMenuItem(menuList[i], this);
                    gachaMenuItemList.Add(menuItem);

                    // 선택된 메뉴 처리
                    if (isFirstMenuSelected == false)
                    {
                        menuItem.SetSelectedState(currentMenuData.key == menuList[i].key);
                    }
                }
            }



            SortGachaMenuItem();

            // 선택된 메뉴 처리
            if (isFirstMenuSelected)
            {
                if (gachaMenuItemList != null && gachaMenuItemList.Count > 0 && gachaMenuItemList[0] != null)
                {
                    foreach (var menu in gachaMenuItemList)
                    {
                        menu.SetSelectedState(false);
                    }

                    currentMenuData = gachaMenuItemList[0].CurrentMenuData;
                    gachaMenuItemList[0].SetSelectedState(true);
                    gachaMenuScrollRect.horizontalNormalizedPosition = 0;
                }
            }

            RefreshLayout();
        }

        void UpdateGachaMenuItem()
        {
            for (int i = 0; i < gachaMenuItemList.Count; ++i)
            {
                // 중앙 포커싱을 위한 sibling index 저장
                if (currentMenuData == null)
                {
                    currentSelectedMenuIndex = 0;
                }
                else
                {
                    if (currentMenuData.key == gachaMenuItemList[i].CurrentMenuData.key)
                    {
                        currentSelectedMenuIndex = i;
                    }

                    gachaMenuItemList[i].SetSelectedState(currentMenuData.key == gachaMenuItemList[i].CurrentMenuData.key);
                }
            }
        }

        void SortGachaMenuItem()
        {
            gachaMenuItemList = gachaMenuItemList.OrderByDescending(menu => menu.IsAvailGacha)
                .ThenByDescending(menu => menu.IsAvailPeriod)
                .ThenByDescending(menu => menu.IsAvailCondition)
                .ThenBy(menu => menu.CurrentMenuData.weight).ToList();

            for (int i = 0; i < gachaMenuItemList.Count; ++i)
            {
                gachaMenuItemList[i].gameObject.transform.SetSiblingIndex(i);

                // 중앙 포커싱을 위한 sibling index 저장
                if (currentMenuData == null)
                {
                    currentSelectedMenuIndex = 0;
                }
                else
                {
                    if (currentMenuData.key == gachaMenuItemList[i].CurrentMenuData.key)
                    {
                        currentSelectedMenuIndex = i;
                    }
                }
            }
        }

        void SetGachaButton()
        {
            // -- 튜토리얼 가챠 버튼 세팅
            if(isTutorialing && currentMenuData.key == TutorialManager.tutorialManagement.GetCurTutoPrivateKey(1))
            {
                var gachaType = currentMenuData.typeDatas.Find(dat => dat.key == TutorialManager.tutorialManagement.GetCurTutoPrivateKey(2));
                GameObject newButton = Instantiate(gachaButtonPrefab, buttonLayoutObject.transform);
                GachaMenuButton menuButton = newButton.GetComponent<GachaMenuButton>();
                menuButton.InitButton(gachaType, SendGachaProcess, CheckMenuReddot);
                gachaMenuButtonList.Add(menuButton);
                TutorialManager.Instance.SetRecordObject(500103, menuButton.GetComponent<RectTransform>());
                return;
            }
            // --

            foreach (GachaTypeData gachaType in currentMenuData.typeDatas)
            {
                GameObject newButton = Instantiate(gachaButtonPrefab, buttonLayoutObject.transform);
                GachaMenuButton menuButton = newButton.GetComponent<GachaMenuButton>();
                menuButton.InitButton(gachaType, SendGachaProcess, CheckMenuReddot);
                gachaMenuButtonList.Add(menuButton);
            }
        }

        void CheckMenuReddot()
        {
            foreach(var menu in gachaTabMenuList)
            {
                menu.CheckReddot();
            }
        }

        void SetGachaBG()
        {
            if (bgImage != null && currentGroupData != null && bgSprites != null && (bgSprites.Length < currentGroupData.key - 1))
            {
                bgImage.sprite = bgSprites[currentGroupData.key - 1];
            }
        }

        public void ReloadAll()
        {
            ClearUI(eRefreshType.ALL);

            SetGachaBG();
            SetGachaMenuItem();
            SetGachaButton();

            gachaTitleText.text = currentMenuData.Name;

            UpdateTabButtonState();

            RefreshLayout();

            if (isGachaTypeChanged)
            {
                isGachaTypeChanged = false;
                RefreshInitProductionSpineEvent();
            }
        }

        public void ReloadGachaMenu()
        {
            ClearUI(eRefreshType.BUTTON);

            UpdateGachaMenuItem();

            SetGachaButton();

            gachaTitleText.text = currentMenuData.Name;

            RefreshLayout();

            if (isGachaTypeChanged)
            {
                isGachaTypeChanged = false;
                RefreshInitProductionSpineEvent();
            }
        }

        public void RefreshGachaMenu()
        {
            for (int i = 0; i < gachaTabMenuList.Count; ++i)
            {
                gachaTabMenuList[i].CheckReddot();
            }

            if (gachaMenuItemList == null || gachaMenuItemList.Count <= 0) return;

            gachaMenuItemList.ForEach(menu => menu.RefreshMenuItem());
        }

        public void RefreshGachaMenuButton()
        {
            if (gachaMenuButtonList == null || gachaMenuButtonList.Count <= 0) return;

            gachaMenuButtonList.ForEach(menuButton => menuButton.RefreshMenuButton());
        }

        void RefreshInitProductionSpineEvent()
        {
            var currentMenuType = (eGachaMenuType)currentMenuData.menu_type;
            eGachaType setGachaType = eGachaType.PET;
            switch (currentMenuType)
            {
                case eGachaMenuType.DRAGON:
                case eGachaMenuType.PICKUP:
                    setGachaType = eGachaType.DRAGON;
                    break;
            }

            GachaEvent.InitProduction(setGachaType);
        }

        public void RefreshLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(gachaMenuScrollRect.content.GetComponent<RectTransform>());
        }

        public void UpdateGachaGroupTab(GachaGroupData groupData)
        {
            if (currentGroupData.key == groupData.key) return;

            currentGroupData = groupData;
            currentMenuData = null;

            isGachaTypeChanged = true;

            // 스크롤 중앙정렬을 위한 현재 선택된 탭 리스트 인덱스 갱신
            for (int i = 0; i < gachaTabMenuList.Count; ++i)
            {
                if (gachaTabMenuList[i].CurrentGroupData.key == currentGroupData.key)
                {
                    currentSelectedTabIndex = i;
                    break;
                }
            }

            ReloadAll();

            FocusCenterSelectedGachaTabMenu();
        }

        public void UpdateGachaMenuItem(GachaMenuData menuData)
        {
            if (currentMenuData.key == menuData.key)
            {
                FocusCenterSelectedGachaMenu();
                return;
            }

            isGachaTypeChanged = currentMenuData.menu_type != menuData.menu_type;   // 가챠타입이 달라지면 갱신 요청 on

            currentMenuData = menuData;

            ReloadGachaMenu();

            FocusCenterSelectedGachaMenu();
        }

        public void OnClickMileageShopButton()
        {
            PopupManager.OpenPopup<GachaMileageShopPopup>();
        }
        int GachaItemCnt(GachaRateData gachaDat)
        {
            int cnt = 0;
            if (gachaDat.reward_type == "DICE_GROUP")
            {
                var itemDiceGroup = ItemGroupData.Get(gachaDat.result_id);
                foreach (var itemGroup in itemDiceGroup)
                {
                    bool isItemExist = false;
                    foreach (var items in itemGroup.Child)
                    {
                        if (items.Reward.GoodType == eGoodType.ITEM)
                            isItemExist = true;
                    }
                    if (isItemExist)
                        ++cnt;
                }
            }
            return cnt;
        }
        // 단일 뽑기
        void SendGachaProcess(GachaTypeData gachaData, string log = "")
        {
            var data = GachaMenuData.Get(gachaData.menu_id);
            if (data != null)
            {
                UpdateGachaMenuItem(data);
            }

            var gachaRateDat = gachaData.Rate;
            int requireInvenSpace = 0;
            foreach (var dat in gachaRateDat)
            {
                if (dat.reward_type == "GACHA_GROUP")
                {
                    var gachaDats = GachaRateData.GetGroup(dat.result_id);
                    foreach (var gachaDat in gachaDats)
                    {
                        requireInvenSpace += GachaItemCnt(gachaDat);
                    }
                }
                else if (dat.reward_type == "DICE_GROUP")
                {
                    requireInvenSpace += GachaItemCnt(dat);
                }
            }
            if (User.Instance.Inventory.GetEmptySlotCount() < requireInvenSpace)
            {
                IsFullBagAlert();
                return;
            }

            AppsFlyerSDK.AppsFlyer.sendEvent("gacha", new Dictionary<string, string>() {
                { "key", gachaData.key.ToString() },
                { "price_type", gachaData.price_type.ToString() },
                { "price_param", gachaData.price_uid.ToString() },
                { "price_value", gachaData.price_value.ToString() },
                { "repeat", gachaData.repeats.ToString() }, 
            });

            WWWForm param = new WWWForm();
            param.AddField("type", gachaData.GetKey());
            if (data != null)
            {
                param.AddField("menu_type", data.menu_type);
            }
            param.AddField("ad_log", log);

            NetworkManager.Send("gacha/gacha", param, (jsonData) =>
            {
                if (jsonData.ContainsKey("results") && jsonData["results"].Type == JTokenType.Array)
                {
                    Debug.Log("GACHA SUCCESS !");

                    JArray res = (JArray)jsonData["results"];

                    RefreshGachaMenu();
                    RefreshGachaMenuButton();

                    GachaEvent.SendGachaTypeDataResult(gachaData, res);
                }
                else
                {
                    ReloadAll();
                }

                if(jsonData.ContainsKey("rs") && (int)jsonData["rs"] == (int) eApiResCode.INVENTORY_FULL)
                {
                    IsFullBagAlert();
                }

                if (jsonData.ContainsKey("force_weight") && jsonData["force_weight"].Type == JTokenType.Integer)
                {
                    PlayerPrefs.SetInt("tmp_force_weight_" + User.Instance.UserAccountData.AccessToken, jsonData["force_weight"].Value<int>());
                }

                if (jsonData.ContainsKey("portrait"))
                {
                    User.Instance.UserData.UpdatePortrait(jsonData["portrait"].Value<string>());
                }
            });
        }


        void IsFullBagAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                () => {
                    PopupManager.OpenPopup<InventoryPopup>();
                }, () => { }, () => { });
        }

        void ClearUI(eRefreshType type)
        {
            switch (type)
            {
                case eRefreshType.ALL:
                    //SBFunc.RemoveAllChildrens(gachaTabScrollRect.content);
                    gachaMenuItemList.Clear();
                    gachaMenuButtonList.Clear();

                    SBFunc.RemoveAllChildrens(gachaMenuScrollRect.content);
                    SBFunc.RemoveAllChildrens(buttonLayoutObject.transform);
                    break;
                case eRefreshType.MENU:
                    gachaMenuItemList.Clear();
                    gachaMenuButtonList.Clear();

                    SBFunc.RemoveAllChildrens(gachaMenuScrollRect.content);
                    SBFunc.RemoveAllChildrens(buttonLayoutObject.transform);
                    break;
                case eRefreshType.BUTTON:
                    gachaMenuButtonList.Clear();

                    SBFunc.RemoveAllChildrens(buttonLayoutObject.transform);
                    break;
            }
        }

        void DrawUISetting()
        {
            for (int i = 0; i < gachaUIOfftargets.Length; i++)
            {
                gachaUIOfftargets[i].SetActive(false);
            }
        }

        void DrawEndUISetting()
        {
            for (int i = 0; i < gachaUIOfftargets.Length; i++)
            {
                gachaUIOfftargets[i].SetActive(true);
            }
            ReloadAll();
        }

        void UpdateTabButtonState()
        {
            if (gachaTabMenuList == null || gachaTabMenuList.Count <= 0) return;

            gachaTabMenuList.ForEach(button =>
            {
                button.SetSelectedState(currentGroupData);
            });
        }

        void FocusCenterSelectedGachaTabMenu()
        {
            gachaTabScrollRect.FocusOnItem(gachaTabMenuList[currentSelectedTabIndex].GetComponent<RectTransform>(), 0.2f);
        }

        void FocusCenterSelectedGachaMenu()
        {
            gachaMenuScrollRect.FocusOnItem(gachaMenuItemList[currentSelectedMenuIndex].GetComponent<RectTransform>(), 0.2f);
        }

        public void OnPrevMenu()
        {
            currentSelectedMenuIndex -= 1;
            if (currentSelectedMenuIndex < 0)
            {
                currentSelectedMenuIndex = gachaMenuItemList.Count - 1;
            }

            UpdateGachaMenuItem(gachaMenuItemList[currentSelectedMenuIndex].CurrentMenuData);
        }

        public void OnNextMenu()
        {
            currentSelectedMenuIndex += 1;
            if (currentSelectedMenuIndex > gachaMenuItemList.Count - 1)
            {
                currentSelectedMenuIndex = 0;
            }

            UpdateGachaMenuItem(gachaMenuItemList[currentSelectedMenuIndex].CurrentMenuData);
        }
    }
}
