using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class GuildShopPopup : Popup<ShopPopupData>
    {
        [SerializeField]
        Button[] tabs;
        [SerializeField]
        GuildPointShopItem[] guildPointShopItemObj;

        private readonly int maxShowItemCnt = 10;

        int curIndex = 0;
        int tabCount = 0;
        List<ShopGoodsData> goodsList = new List<ShopGoodsData>();
        public override void InitUI()
        {
            //PopupManager.Instance.Top.SetGuildPointUI(true);
            // 길드 상점 판매 목록 리스트를 받아야 됨 // 테이블 데이터
            goodsList = ShopGoodsData.GetByMenuID((int)eShopMenuType.GUILD_POINT_SHOP);

            SetTabs();
            OnClickTab(0);
        }
        public override void ClosePopup()
        {
            //PopupManager.Instance.Top.SetGuildPointUI(false);
            base.ClosePopup();
        }

        void SetTabs()
        {
            foreach (var tab in tabs)
            {
                tab.gameObject.SetActive(false);
            }
            tabCount = (goodsList.Count / maxShowItemCnt) + (goodsList.Count % maxShowItemCnt > 0 ? 1: 0);
            for (int i =0; i < tabCount; ++i)
            {
                tabs[i].gameObject.SetActive(true);
            }
        }

        public void OnClickTab(int index)
        {
            foreach (var tab in tabs)
            {
                tab.interactable = true;
            }
            tabs[index].interactable = false;
            curIndex = index;
            int guildLv = GuildManager.Instance.MyGuildInfo.GetGuildLevel();
            int showCount = goodsList.Count % maxShowItemCnt; // 마지막 탭 대응
            for (int i = 0; i < guildPointShopItemObj.Length; ++i)
            {
                if ((index < tabCount - 1)|| (index == tabCount-1 && (showCount==0 || i < showCount) )) //마지막 탭이 아니거나 마지막 탭의 남은 아이템 갯수를 충족하거나
                {
                    int idx = i + (curIndex * maxShowItemCnt);
                    guildPointShopItemObj[i].gameObject.SetActive(true);
                    guildPointShopItemObj[i].InitItemClone(goodsList[idx], guildLv);
                }
                else
                    guildPointShopItemObj[i].gameObject.SetActive(false);
            }
        }
        public override void ForceUpdate(ShopPopupData data)
        {
            base.DataRefresh(data);
            OnClickTab(curIndex);
        }

    }
}