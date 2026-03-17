using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class ArenaPointShopPopup : Popup<ShopPopupData>
    {
        const int SCROLL_LIMIT_COUNT = 10;

        [SerializeField] protected ScrollRect scrollView = null;
        [SerializeField] protected GameObject shopItemPrefab = null;

        [Header("[Time]")]
        [SerializeField] protected Text timeText = null;
        [SerializeField] protected TimeObject timeObject = null;
        [SerializeField] Button refreshButton = null;

        protected List<ArenaPointItemClone> itemCloneList = new();

        protected int lastUpdateTime { get { return ShopManager.Instance.ArenaRandomGoodsUpdateTime; } }
        
        protected int refreshTime 
        { 
            get {
                return GameConfigTable.GetConfigIntValue("ARENA_POINT_SHOP_RESET_TIME");
            }
        }

        protected bool isAvailRefresh = false;

        private void OnEnable()
        {
            OnEnableProcess();
        }

        private void OnDisable()
        {
            OnDisableProcess();
        }

        protected virtual void OnEnableProcess()
        {
            PopupManager.Instance.Top.SetArenaPointUI(true);
        }

        protected virtual void OnDisableProcess()
        {
            PopupManager.Instance.Top.SetArenaPointUI(false);
            UIManager.Instance.MainUI.RefreshArenaPoint();
        }

        public override void InitUI()
        {
            ReloadItem();
        }

        public override void ForceUpdate(ShopPopupData data)
        {
            base.DataRefresh(data);

            RefreshAllItemClone();
        }

        public virtual void OnClickRefreshButton()
        {
            // todo.. 리프레쉬 버튼 동작 구현
            if (isAvailRefresh)
            {
                ReloadItem();
            }
            else
            {
                string title = StringData.GetStringByStrKey("shop_item_refresh_title");
                string content = string.Format(StringData.GetStringByStrKey("아레나갱신"));

                PricePopup.OpenPopup(title, "", content, GameConfigTable.GetConfigIntValue("ARENA_POINT_SHOP_REFRESH_COST_NUM"),
                    ePriceDataFlag.ContentBG | ePriceDataFlag.CancelBtn | ePriceDataFlag.GemStone, () =>
                    {
                        PopupManager.ClosePopup<PricePopup>();
                    });
            }
        }

        void RefreshAllItemClone()
        {
            if (itemCloneList.Count <= 0) return;

            itemCloneList.ForEach(clone => clone.RefreshItemClone());
        }

        void RefreshPopupState()
        {
            SetScrollview(GetShopGoods());
        }

        protected virtual List<ShopGoodsState> GetShopGoods()
        {
            return ShopManager.Instance.GetArenaRandomGoods();
        }

        protected virtual void SetClone(List<ShopGoodsState> _shopGoodsList)
        {
            if (_shopGoodsList == null || _shopGoodsList.Count <= 0)
                return;

            foreach (ShopGoodsState goodsStateData in _shopGoodsList)
            {
                GameObject newShopItem = Instantiate(shopItemPrefab, scrollView.content);
                ArenaPointItemClone arenaPointItem = newShopItem.GetComponent<ArenaPointItemClone>();
                arenaPointItem?.InitItemClone(goodsStateData.BaseData);

                itemCloneList.Add(arenaPointItem);
            }
        }

        void SetScrollview(List<ShopGoodsState> _shopGoodsList)
        {
            SetClone(_shopGoodsList);

            ResizeScrollViewContent(_shopGoodsList.Count);

            scrollView.horizontalNormalizedPosition = 0;
            scrollView.horizontal = _shopGoodsList.Count > SCROLL_LIMIT_COUNT;
        }

        protected virtual void SetItem()
        {
            int refreshEndTime = lastUpdateTime + refreshTime;
            int resultTime = TimeManager.GetTimeCompare(refreshEndTime);

            if (resultTime < 0)
            {
                // 아이템 리스트 호출하는 api 
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("shop", (int)eShopMenuType.ARENA_POINT_SHOP); //메뉴번호 

                NetworkManager.Send("shop/getrandomlist", wwwForm, (jsonData) =>
                {
                    if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                        return;

                    switch ((eApiResCode)jsonData["rs"].Value<int>())
                    {
                        case eApiResCode.OK:
                        {
                            if (jsonData.ContainsKey("list"))
                            {
                                // 아레나 상점 데이터 갱신
                                ShopManager.Instance.SetArenaRandomGoods(jsonData["list"]);

                                UpdateUIData();
                            }
                        }
                        break;
                        case eApiResCode.PARAM_ERROR:
                        {

                        }
                        break;
                    }
                });
            }
            else
            {
                UpdateUIData();
            }
        }

        protected void UpdateUIData()
        {
            RefreshPopupState();
            UpdateRemainTime();
        }


        // 아레나 포인트 상점 아이템 품목을 새로 가져옴
        protected void ReloadItem()
        {
            Clear();

            SetItem();

            //UpdateRemainTime();
        }

        // 스크롤 content의 사이즈 보정
        void ResizeScrollViewContent(int listCount)
        {
            if (listCount > SCROLL_LIMIT_COUNT)
            {
                GridLayoutGroup gridLayout = scrollView.content.GetComponent<GridLayoutGroup>();
                if (gridLayout != null)
                {
                    int result = ((listCount + 1) - SCROLL_LIMIT_COUNT) / gridLayout.constraintCount;
                    float resultX = (result * gridLayout.cellSize.x) + (result * gridLayout.spacing.x);
                    scrollView.content.offsetMax = new Vector2(resultX, 0);
                }
            }
        }

        protected virtual void UpdateRemainTime()
        {
            if (timeObject != null)
            {
                timeObject.Refresh = () =>
                {
                    int refreshEndTime = lastUpdateTime + refreshTime;
                    int resultTime = TimeManager.GetTimeCompare(refreshEndTime);

                    if (resultTime >= 0)
                    {
                        isAvailRefresh = false;
                        timeText.text = SBFunc.TimeString(resultTime);
                    }
                    else
                    {
                        isAvailRefresh = true;
                        timeText.text = SBFunc.TimeString(0);
                        timeObject.Refresh = null;

                        ReloadItem();
                    }
                };
            }
        }

        void Clear()
        {
            isAvailRefresh = false;

            itemCloneList.Clear();

            SBFunc.RemoveAllChildrens(scrollView.content);

            scrollView.content.offsetMax = Vector2.zero;
            scrollView.horizontalNormalizedPosition = 0;

            PopupManager.ClosePopup<ShopBuyPopup>();
        }
    }
}