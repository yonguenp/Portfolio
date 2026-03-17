using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class ShopBuyObjLarge : ShopBuyObj
    {
        [SerializeField]
        private Image bgImage = null;

        [SerializeField]
        Text explainText = null;
        [SerializeField]
        TableViewLayout tableView = null;
        [SerializeField]
        RectTransform itemParent = null;

        bool isTableViewFirstInit = false;

        private void Start()
        {
            if(!isTableViewFirstInit)
            {
                tableView.OnStart();
                isTableViewFirstInit = true;
            }

            tableView.ReLoad();
        }
        public override void SetBuyLayer(int goodsNumber, ShopGoodsData data, int buyAbleCount, VoidDelegate buyCallBack)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            base.SetBuyLayer(goodsNumber, data, buyAbleCount, buyCallBack);
            //float minWidth = GetComponent<RectTransform>().sizeDelta.x - 140;
            //itemParent.GetComponent<LayoutElement>().minWidth = minWidth;

            buyLimitObj.SetActive(data.BUY_LIMIT > 0);
            buyLimitText.text = StringData.GetStringFormatByStrKey("상점 계정당 구매가능 횟수", buyAbleCount, data.BUY_LIMIT);
            if (ShopBannerData.Get(data.KEY) != null)
            {
                bgImage.sprite = ShopBannerData.Get(data.KEY).BG_SPRITE;
            }
            else
            {
                bgImage.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.UISpritePath, "bg_title_01");
            }
            var xVal = bgImage.rectTransform.sizeDelta.x;
            float sizeRatio = bgImage.sprite.bounds.size.y / bgImage.sprite.bounds.size.x;
            bgImage.rectTransform.sizeDelta = new Vector2(xVal, sizeRatio * xVal);
            var subScribeData = ShopSubscriptionData.GetByGroup(int.Parse(data.KEY));
            
            explainText.text = StringData.GetStringByStrKey(subScribeData != null ? "상점청약약관_구독" : "상점청약약관");
            //tableView.OnStart();
            var itemWidth = tableView.ItemTemplate.GetComponent<RectTransform>().sizeDelta.x;
            List<ITableData> tableViewItemList = new List<ITableData>();
            
            foreach (var itemInfo in PostRewardData.GetGroup(data.REWARD_ID))
            {
                tableViewItemList.Add(itemInfo);
            }
            //itemParent.GetComponent<ContentSizeFitter>().enabled = itemParent.GetComponent<LayoutElement>().enabled = itemParent.GetComponent<HorizontalLayoutGroup>().enabled = tableViewItemList.Count * itemWidth < minWidth;
            //tableViewItemList.Reverse();
            tableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject itemNode, ITableData item) =>
            {
                if (itemNode == null || item == null)
                {
                    return;
                }
                var packageItem = itemNode.GetComponent<ShopPackageItem>();
                if (packageItem == null)
                {
                    return;
                }
                
                packageItem.SetData(((PostRewardData)item).Reward);
            }));

            //tableView.ReLoad();

            Invoke("RefreshLayout", 0.1f);
        }

        void RefreshLayout()
        {
            var layout = itemParent.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(itemParent);

                if (itemParent.sizeDelta.x < (itemParent.parent as RectTransform).rect.width)
                {
                    itemParent.sizeDelta = new Vector2((itemParent.parent as RectTransform).rect.width, itemParent.sizeDelta.y);
                    layout.enabled = true;
                }
            }
        }

        protected override string GetDefaultPath()
        {
            return "shop_banner_big_default";
        }
    }
}


