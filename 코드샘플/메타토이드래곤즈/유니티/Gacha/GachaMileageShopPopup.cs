using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GachaMileageShopPopup : Popup<ShopPopupData>
    {
        const int SCROLL_LIMIT_COUNT = 10;

        [SerializeField] ScrollRect scrollView = null;
        [SerializeField] GameObject shopItemPrefab = null;

        List<GachaMileageItemClone> mileageItemCloneList = new();

        private void OnEnable()
        {
            PopupManager.Instance.Top.SetMileageUI(true);
        }

        private void OnDisable()
        {
            PopupManager.Instance.Top.SetMileageUI(false);
        }

        public override void InitUI()
        {
            Clear();

            SetGachaMileageItem();
        }

        public override void ForceUpdate(ShopPopupData data)
        {
            base.DataRefresh(data);

            RefreshItemClone();
        }

        void RefreshItemClone()
        {
            if (mileageItemCloneList.Count <= 0) return;

            mileageItemCloneList.ForEach(clone => clone.RefreshItemClone());
        }

        void SetGachaMileageItem()
        {
            var goodsList = ShopGoodsData.GetByMenuID((int)eShopMenuType.MILEAGE_SHOP);

            goodsList = goodsList.OrderBy(goods => goods.SORT).ToList();

            foreach (ShopGoodsData goodsData in goodsList)
            {
                GameObject newShopItem = Instantiate(shopItemPrefab, scrollView.content);
                GachaMileageItemClone mileageItem = newShopItem.GetComponent<GachaMileageItemClone>();
                mileageItem?.InitItemClone(goodsData);

                mileageItemCloneList.Add(mileageItem);
            }

            ResizeScrollViewContent(goodsList.Count);

            scrollView.horizontalNormalizedPosition = 0;
            scrollView.horizontal = goodsList.Count > SCROLL_LIMIT_COUNT;
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

        void Clear()
        {
            mileageItemCloneList.Clear();

            SBFunc.RemoveAllChildrens(scrollView.content);

            scrollView.content.offsetMax = Vector2.zero;
            scrollView.horizontalNormalizedPosition = 0;
        }
    }
}