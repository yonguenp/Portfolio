using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace SandboxNetwork {
    public class ShopPopup : Popup<MainShopPopupData>, EventListener<PopupTopUIRefreshEvent>
    {
        [SerializeField]
        private Image BgImage = null;
        [SerializeField]
        private Text termsText=  null;

        [Space]
        [Header("shop Layer")]
        [SerializeField]
        private GameObject largeSizeLayer;
        [SerializeField]
        private GameObject mediumSizeLayer;
        [SerializeField]
        private GameObject smallSizeLayer;
        [SerializeField]
        private TimeObject subscribeRefreshTimeObj = null;

        [Space]
        [Header("Tab Layer")]
        [SerializeField]
        private GameObject shopTabObj = null;
        [SerializeField]
        private ScrollRect showTabScrollRect = null;
        [SerializeField]
        private Transform shopTabParent = null;
        List<ShopTabItem> shopTabItems = new List<ShopTabItem>();
        List<ShopMenuData> shopTabDatas = new List<ShopMenuData>();

        [Space]
        [Header("SmallLayer")]
        [SerializeField]
        private Transform SLayerItemParentTr = null;
        [SerializeField]
        private GameObject smallItem = null;
        List<ShopBuyObj> smallItems = new List<ShopBuyObj>();

        [Space]
        [Header("MediumLayer")]
        [SerializeField]
        private Transform MLayerItemParentTr = null;
        [SerializeField]
        private GameObject mediumItem = null;
        List<ShopBuyObjMedium> mediumItems = new List<ShopBuyObjMedium>();
        [SerializeField]
        private LayoutElement mediumLayout = null;

        [Space]
        [Header("LargeLayer")]
        [SerializeField]
        private Transform LLayerItemParentTr = null;
        [SerializeField]
        private UIPageViewController LLayerPageView = null;
        [SerializeField]
        private GameObject ArrowLayerObj =  null;
        [SerializeField]
        private GameObject largeItem = null;
        List<ShopBuyObjLarge> largeItems = new List<ShopBuyObjLarge>();
        [SerializeField]
        private GridLayoutGroup largeLayoutGroup = null;


        [Space]
        [Header("BadgeLayer")]
        [SerializeField]
        private GameObject badgeLayer = null;

        [SerializeField]
        private Transform badgeParentTr = null;

        [SerializeField]
        private GameObject badgeObj = null;

        List<ShopBadgeObject> badgeObjs = new List<ShopBadgeObject>();


        private int curLargeIndex = -1;
        private int curLargeCount = 0;

        private int curTabIndex = 0;

        Tween bgTween = null;

        Coroutine LargeLayerCoroutine = null;
        public override void InitUI()
        {
            curTabIndex = 0;
            ShopManager.Instance.CheckRefreshGoodsByDayChange();
            ShopManager.Instance.RefreshSubscribe();
            if (Data == null)
                SetShopTab(true);
            else
                SetShopTab(true, Data.menuNum);
            SetSubScribeRefreshTimeObj();
            InittUISetting();

            AppsFlyerSDK.AppsFlyer.sendEvent("visit_shop", new Dictionary<string, string>());
            //bgTween?.Kill();
            //bgTween = BgImage.transform.DOLocalMove(Vector2.one * -500, 30f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
            //BgImage.transform.localPosition = Vector2.zero;
        }

        private void OnEnable()
        {
            EventManager.AddListener<PopupTopUIRefreshEvent>(this);
            //bgMoveAnim.DORestart();
            
        }

        private void OnDisable()
        {
            //bgTween?.Kill();
            EventManager.RemoveListener<PopupTopUIRefreshEvent>(this);

            //var currentUIType = UIManager.Instance.CurrentUIType;
            //PopupManager.Instance.Top.InitUI(currentUIType);
            //PopupManager.Instance.Top.SetTopUI();
            UserStatusEvent.RefreshFriendPoint();
            UIShopObject.CheckReddot();
            LargeLayerMoveCorOff();
        }
        public override void ClosePopup()
        {
            PopupManager.Instance.Top.SetMagnetUI(false);
            PopupManager.Instance.Top.SetStaminaUI(false);
            PopupManager.Instance.Top.SetArenaTicketUI(false);
            base.ClosePopup();
        }

        public override void OnClickDimd()
        {
            PopupManager.Instance.Top.SetMagnetUI(false);
            PopupManager.Instance.Top.SetStaminaUI(false);
            PopupManager.Instance.Top.SetArenaTicketUI(false);
            base.OnClickDimd();
        }

        void InitBG(Sprite sprite)
        {
            BgImage.sprite = sprite;
        }
        void InittUISetting()
        {
            LLayerPageView.InitPageView();
            LLayerPageView.SetPageChangedCallback(delegate
            {
                int pageIndex = LLayerPageView.GetCurrentPageIndex();
                var data = shopTabDatas[curTabIndex].ChildGoods[pageIndex];
                if (ShopBannerData.Get(data.KEY) != null)
                {
                    InitBG(ShopBannerData.Get(data.KEY).BG_SPRITE);
                }
                else
                {
                    InitBG(shopTabDatas[curTabIndex].BG_IMAGE);
                }


            });
            showTabScrollRect.verticalNormalizedPosition = 1;
            //Vector2 canvasSize = UICanvas.Instance.GetCanvasRectTransform().sizeDelta;
            //var safeXsize = Screen.safeArea.size.x;
            //var xSize = safeXsize - 480 + (canvasSize.x - safeXsize)/2f - Screen.safeArea.x;
            //float sizeRatio = 1440 / 2080f;
            //largeLayoutGroup.cellSize = canvasSize - Vector2.right * (Screen.safeArea.x);
            var rect = LLayerPageView.GetComponent<RectTransform>();
            //rect.rect
            //float left = rect.offsetMin.x;
            //float bottom = rect.offsetMin.y;
            //float right = rect.offsetMax.x;
            //float top = rect.offsetMax.y;
            largeLayoutGroup.cellSize = rect.rect.size;
                
                //new Vector2(canvasSize.x - 480 -Screen.safeArea.x, canvasSize.y);
                
                //canvasSize + Vector2.left * 480;
            largeLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
            LayoutRebuilder.ForceRebuildLayoutImmediate(largeLayoutGroup.GetComponent<RectTransform>());
        }

        void SetSubScribeRefreshTimeObj()
        {
            int remainTime = ShopManager.Instance.GetRefreshRemainTimeSubScribe();
            if (remainTime <= 0) return;
           
            subscribeRefreshTimeObj.Refresh = () =>
            {
                int time = TimeManager.GetTimeCompare(remainTime);
                if (time <= 0)
                {
                    ShopManager.Instance.RefreshSubscribe();
                    SetShopTab(true);
                    subscribeRefreshTimeObj.Refresh = null;
                }
            };
        }

        public List<ShopMenuData> GetShopTab()
        {
            var tabs = ShopMenuData.GetShopMenus(eStoreType.SHOP);
            tabs.AddRange(ShopMenuData.GetShopMenus(eStoreType.ASSET_STORE));
            tabs.AddRange(ShopMenuData.GetShopMenus(eStoreType.TICKET_STORE));

            //if (User.Instance.ENABLE_P2E)
            //    tabs.AddRange(ShopMenuData.GetShopMenus(eStoreType.MAGNET_STORE));

            tabs = tabs.OrderByDescending(item => !ShopManager.Instance.IsSoldOutMenu(item.KEY)).ThenBy(item => item.SORT).ToList();
            return tabs;
        }

        void SetShopTab(bool isForceMoveTab =false, int targetMenuNum = 0)
        {
            if(shopTabItems == null ) {
                shopTabItems = new List<ShopTabItem>();
            }
            foreach(var item in shopTabItems)
            {
                item.gameObject.SetActive(false);
            }

            shopTabDatas = GetShopTab();
            if(shopTabDatas.Count <= 0)
            {                
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        ToastManager.On(StringData.GetStringByStrKey("윈도우결제미지원"));
#endif
                ClosePopup();
                return;
            }

            for (int i =0, dataCount = shopTabDatas.Count; i < dataCount; ++i)
            {
                if (shopTabItems.Count <= i)
                {
                    var shopTab = Instantiate(shopTabObj, shopTabParent).GetComponent<ShopTabItem>();
                    shopTabItems.Add(shopTab);
                }
                shopTabItems[i].gameObject.SetActive(true);

                if (shopTabDatas[i].KEY == targetMenuNum)
                {
                    curTabIndex = i;
                }

                shopTabItems[i].Init(shopTabDatas[i], i, (int index) =>
                {
                    curTabIndex = index;
                    showTabScrollRect.FocusOnItem(shopTabItems[curTabIndex].GetComponent<RectTransform>(), 0.2f);
                    SetShopLayer(shopTabDatas[curTabIndex]);
                    SetShopTabState();
                    LayoutRebuilder.MarkLayoutForRebuild(shopTabParent as RectTransform);                    
                }, false);

                shopTabItems[i].SetReddotState(shopTabDatas[i].IsMenuReddot());
            }
            

            if ( shopTabItems.Count > 0 && isForceMoveTab)
            {
                SetShopLayer(shopTabDatas[curTabIndex]);
                SetShopTabState(curTabIndex);
            }
        }


        void SetShopLayer(ShopMenuData data)
        {
            InitBG(data.BG_IMAGE);
            largeSizeLayer.SetActive(false);
            mediumSizeLayer.SetActive(false);
            smallSizeLayer.SetActive(false);
            badgeLayer.SetActive(true);
            LargeLayerMoveCorOff();
            SetTopUI();
            SetBadgeGoods();
            termsText.text = StringData.GetStringByStrKey(data.PAGE_TYPE == eShopPageType.SUBSCRIBE ? "상점청약약관_구독" : "상점청약약관");

            //bool terms = false;
            //foreach (var good in data.ChildGoods)
            //{
            //    if (good.PRICE.GoodType == eGoodType.CASH)
            //    {
            //        terms = true;
            //        break;
            //    }
            //}
            //termsText.gameObject.SetActive(terms);

            int remainGoodsCount = 0;
            switch (data.PAGE_TYPE)
            {
                case eShopPageType.MIDEUM:
                case eShopPageType.SUBSCRIBE:
                {
                    mediumSizeLayer.SetActive(true);
                    foreach (var item in mediumItems)
                    {
                        item.gameObject.SetActive(false);
                    }
                    for (int i = 0, count = data.ChildGoods.Count; i < count; ++i)
                    {
                        if (i >= mediumItems.Count)
                        {
                            var obj = Instantiate(mediumItem, MLayerItemParentTr).GetComponent<ShopBuyObjMedium>();
                            mediumItems.Add(obj);
                        }
                        var goods = data.ChildGoods[i];
                        var goodsStateData = ShopManager.Instance.GetGoodsState(int.Parse(goods.KEY));
                        if (goodsStateData.BaseData.USE)
                        {
                            mediumItems[i].gameObject.SetActive(true);
                            mediumItems[i].SetBuyLayer(i, goods, goodsStateData.RemainGoodsCount, RefreshCurrentMenu, null, goodsStateData.SubscribeState, goodsStateData.SubscribeDay); // 현재 상태 데이터 세팅해줘야 됨
                            remainGoodsCount += remainGoodsCount;
                        }
                        else
                        {
                            mediumItem.gameObject.SetActive(false);
                        }
                    }

                    mediumItems = mediumItems.OrderByDescending(item => item.SortByBuy).ThenBy(item => item.Sort).ToList();

                    foreach (var item in mediumItems)
                    {
                        item.transform.SetAsLastSibling();
                    }

                    var layout = MLayerItemParentTr.GetComponent<LayoutElement>();
                    if (layout != null)
                    {
                        layout.minWidth = 0.0f;
                        LayoutRebuilder.ForceRebuildLayoutImmediate(MLayerItemParentTr.GetComponent<RectTransform>());

                        if ((MLayerItemParentTr as RectTransform).sizeDelta.x < (MLayerItemParentTr.parent as RectTransform).rect.width)
                        {
                            layout.minWidth = (MLayerItemParentTr.parent as RectTransform).rect.width;
                        }
                        else
                        {
                            layout.minWidth = 0;
                        }
                    }
                }
                break;
                case eShopPageType.SMALL:
                {
                    smallSizeLayer.SetActive(true);
                    foreach (var item in smallItems)
                    {
                        item.gameObject.SetActive(false);
                    }
                    for (int i = 0, count = data.ChildGoods.Count; i < count; ++i)
                    {
                        if (i >= smallItems.Count)
                        {
                            var obj = Instantiate(smallItem, SLayerItemParentTr).GetComponent<ShopBuyObj>();
                            smallItems.Add(obj);
                        }
                        var goods = data.ChildGoods[i];
                        var goodsStateData = ShopManager.Instance.GetGoodsState(int.Parse(goods.KEY));
                        if (goodsStateData.BaseData.USE)
                        {
                            smallItems[i].gameObject.SetActive(true);
                            smallItems[i].SetBuyLayer(i, goods, goodsStateData.RemainGoodsCount, RefreshCurrentMenu); // 현재 상태 데이터 세팅해줘야 됨 - 임시
                            remainGoodsCount += remainGoodsCount;
                        }
                        else
                        {
                            smallItem.gameObject.SetActive(false);
                        }
                    }

                    smallItems = smallItems.OrderByDescending(item => item.SortByBuy).ThenBy(item => item.Sort).ToList();

                    foreach (var item in smallItems)
                    {
                        item.transform.SetAsLastSibling();
                    }

                    var layout = SLayerItemParentTr.GetComponent<LayoutElement>();
                    if (layout != null)
                    {
                        layout.minWidth = 0.0f;
                        LayoutRebuilder.ForceRebuildLayoutImmediate(SLayerItemParentTr.GetComponent<RectTransform>());

                        if ((SLayerItemParentTr as RectTransform).sizeDelta.x < (SLayerItemParentTr.parent as RectTransform).rect.width)
                        {
                            layout.minWidth = (SLayerItemParentTr.parent as RectTransform).rect.width;
                        }
                        else
                        {
                            layout.minWidth = 0;
                        }
                    }
                }
                break;
                case eShopPageType.LARGE:
                    curLargeCount = data.ChildGoods.Count;
                    ArrowLayerObj.SetActive(curLargeCount > 1);
                    largeSizeLayer.SetActive(true);
                    curLargeIndex = 0;
                    termsText.text = string.Empty;
                    foreach (var item in largeItems)
                    {
                        item.gameObject.SetActive(false);
                    }
                    
                    for (int i = 0; i < curLargeCount; ++i)
                    {
                        if (i >= largeItems.Count)
                        {
                            var obj = Instantiate(largeItem, LLayerItemParentTr).GetComponent<ShopBuyObjLarge>();
                            largeItems.Add(obj);
                        }
                        var goods = data.ChildGoods[i];
                        var goodsStateData = ShopManager.Instance.GetGoodsState(int.Parse(goods.KEY));
                        if (goodsStateData.BaseData.USE)
                        {
                            largeItems[i].gameObject.SetActive(true);
                            largeItems[i].SetBuyLayer(i, goods, goodsStateData.RemainGoodsCount, RefreshCurrentMenu); // 현재 상태 데이터 세팅해줘야 됨 - 임시
                            remainGoodsCount += remainGoodsCount;
                        }
                        else
                        {
                            largeItem.gameObject.SetActive(false);
                        }
                    }
                    if(curLargeCount <= 1)
                    {
                        LLayerPageView.pageToggleGroup.gameObject.SetActive(false);
                    }
                    else
                    {
                        LLayerPageView.pageToggleGroup.gameObject.SetActive(true);
                        if (LargeLayerCoroutine == null)
                        {
                            LargeLayerCoroutine = StartCoroutine(LargeLayerMoveCor());
                        }
                    }

                    largeItems = largeItems.OrderByDescending(item => item.SortByBuy).ThenBy(item => item.Sort).ToList();

                    foreach (var item in largeItems)
                    {
                        item.transform.SetAsLastSibling();
                    }
                    
                    var curData = data.ChildGoods[0];
                    if (ShopBannerData.Get(curData.KEY) != null)
                    {
                        InitBG(ShopBannerData.Get(curData.KEY).BG_SPRITE);
                    }

                    break;
            }

            if(remainGoodsCount == 0)
            {
                shopTabItems[curTabIndex].SetReddotState(false);
            }
        }

        void SetTopUI()
        {
            if (shopTabDatas.Count <= curTabIndex || curTabIndex < 0)
                return;

            var data = shopTabDatas[curTabIndex];
            if (data != null)
            {
                //PopupManager.Instance.Top.InitUI(eUIType.Town);   // 상점의 접속 가능한 경로가 Town으로 한정되지 않기때문에 주석처리
                PopupManager.Instance.Top.SetDiaUI(data.USE_DIA);
                PopupManager.Instance.Top.SetGoldUI(data.USE_GOLD);
                PopupManager.Instance.Top.SetMagnetUI(data.USE_MAGNET);
                PopupManager.Instance.Top.SetStaminaUI(data.USE_STAMINA);
                PopupManager.Instance.Top.SetArenaTicketUI(data.USE_PVP_TICKET);
                PopupManager.Instance.Top.SetMileageUI(false);
                PopupManager.Instance.Top.SetArenaPointUI(false);
                PopupManager.Instance.Top.SetFriendPointUI(false);
            }
        }
        public void RefreshCurrentMenu()
        {
            if (shopTabDatas.Count <= 0)
                return;

            Debug.Log(string.Format("{0}th menu tab is refreshed",curTabIndex));
            SetShopLayer(shopTabDatas[curTabIndex]);
            
            shopTabItems[curTabIndex].SetReddotState(shopTabDatas[curTabIndex].IsMenuReddot());
            if (ShopManager.Instance.IsSoldOutMenu(shopTabDatas[curTabIndex].KEY))
            {
                SetShopTab();
            }
        }


        private IEnumerator LargeLayerMoveCor()
        {
            yield return null;
            int pageCount = LLayerPageView.pageControl.PageCount;
            while (true)
            {
                yield return SBDefine.GetWaitForSeconds(10f);
                if (PopupManager.IsPopupOpening(PopupManager.GetPopup<ItemToolTip>()) == false)
                {
                    int curIndex = LLayerPageView.CurrentPageIndex;
                    LLayerPageView.MovePageByNumer((curIndex + 1) % pageCount);
                }
            }
        }

        private void LargeLayerMoveCorOff()
        {
            if (LargeLayerCoroutine != null)
            {
                StopCoroutine(LargeLayerCoroutine);
                LargeLayerCoroutine = null;
            }
        }

        public void SetShopTabState(int activateIndex= -1)
        {
            foreach(var tab in shopTabItems)
            {
                tab.SetSelectState(false);
                ShopMenuData data = ShopMenuData.Get(tab.ID);
                if (data != null)
                    tab.SetReddotState(data.IsMenuReddot());
            }
            if(activateIndex > -1 && activateIndex < shopTabItems.Count)
            {
                shopTabItems[activateIndex].SetSelectState(true);
            }
        }

        public void OnEvent(PopupTopUIRefreshEvent eventType)
        {
            if(eventType.bOn)
                SetTopUI();
        }


        void SetBadgeGoods()
        {
            foreach(var obj in badgeObjs)
            {
                obj.gameObject.SetActive(false);
            }
            var goodsDatas = ShopBannerData.GetByType(BANNER_TYPE.SMALL);
            
            int badgeCount = 0;
            for (int i=0, count = goodsDatas.Count; i< count;++i)
            {
                int key = int.Parse(goodsDatas[i].KEY);
                if (ShopManager.Instance.GetGoodsState(key)== null)
                    continue;
                var bannerData = ShopBannerData.Get(key.ToString());
                if(bannerData.TYPE == BANNER_TYPE.SMALL)
                {
                    if(badgeObjs.Count<= badgeCount)
                    {
                        var obj = Instantiate(badgeObj, badgeParentTr).GetComponent<ShopBadgeObject>();
                        badgeObjs.Add(obj);
                    }
                    badgeObjs[badgeCount].gameObject.SetActive(ShopManager.Instance.GetGoodsState(key).RemainGoodsCount>0);
                    badgeObjs[badgeCount].Init(key);
                    ++badgeCount;
                }
            
            }
        }


    }
}