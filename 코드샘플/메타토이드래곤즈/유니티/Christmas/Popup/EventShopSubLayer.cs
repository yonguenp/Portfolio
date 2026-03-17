using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 이벤트 팝업에서 상점과 같은 형태의 구성을 함에 따라서 같은 레이아웃 돌려 쓰는 용도.
    /// </summary>
    public class EventShopSubLayer : SubLayer
    {
        [Header("SmallLayer")]
        [SerializeField]
        private Transform SLayerItemParentTr = null;
        [SerializeField]
        private GameObject smallItem = null;
        [SerializeField]
        Text emptyItemText = null;

        List<ShopBuyObj> smallItems = new List<ShopBuyObj>();

        ShopMenuData giftData = null;

        public override void ForceUpdate() { }
        public override bool backBtnCall() { return base.backBtnCall(); } //백 버튼 콜백이 없으면 false 를 출력
        public override void Init()
        {
            var shopData = ShopMenuData.Get((int)eShopMenuType.EVENT_SHOP);
            if (shopData == null)
                return;

            giftData = shopData;
            SetData(shopData);
        }

        protected virtual void SetData(ShopMenuData data)
        {
            foreach (var item in smallItems)
            {
                item.gameObject.SetActive(false);
            }

            var nothingItemCheck = false;

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
                    smallItems[i].SetBuyLayer(i, goods, goodsStateData.RemainGoodsCount, () => {
                        BuyCallbackProcess();
                    });

                    nothingItemCheck = true;
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

            if (emptyItemText != null)
                emptyItemText.gameObject.SetActive(!nothingItemCheck);
        }

        /// <summary>
        /// 이벤트 아이템 구매 이후 프로세스
        /// </summary>
        protected virtual void BuyCallbackProcess()
        {
            SetData(giftData);
        }
    }
}
