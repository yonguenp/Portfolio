using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class FriendPointShopPopup : ArenaPointShopPopup
    {
        protected new int lastUpdateTime { get { return ShopManager.Instance.FriendPointRandomGoodsUpdateTime; } }
        protected new int refreshTime
        {
            get
            {
                return GameConfigTable.GetConfigIntValue("FRIEND_POINT_SHOP_RESET_TIME");
            }
        }

        protected override void OnEnableProcess()
        {
            
        }

        protected override void OnDisableProcess()
        {
            
        }
        protected override List<ShopGoodsState> GetShopGoods()
        {
            return ShopManager.Instance.GetFriendRandomGoods();
        }
        protected override void SetClone(List<ShopGoodsState> _shopGoodsList)
        {
            if (_shopGoodsList == null || _shopGoodsList.Count <= 0)
                return;

            foreach (ShopGoodsState goodsStateData in _shopGoodsList)
            {
                GameObject newShopItem = Instantiate(shopItemPrefab, scrollView.content);
                FriendPointItemClone item = newShopItem.GetComponent<FriendPointItemClone>();
                item?.InitItemClone(goodsStateData.BaseData);

                itemCloneList.Add(item);
            }
        }

        /// <summary>
        /// 시간값을 빼버리고 UI 입장 할 때마다 요청할지는 정해야함.
        /// </summary>
        protected override void SetItem()//우정포인트 아이템 호출 api 요청
        {
            int refreshEndTime = lastUpdateTime + refreshTime;
            int resultTime = TimeManager.GetTimeCompare(refreshEndTime);

            if (resultTime < 0)
            {
                // 아이템 리스트 호출하는 api 
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("shop", (int)eShopMenuType.FRIEND_POINT_SHOP); //메뉴번호 

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
                                ShopManager.Instance.SetFriendRandomGoods(jsonData["list"]);

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

        protected override void UpdateRemainTime()
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
    }
}
