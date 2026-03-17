using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GiftBoxContentSlot : MonoBehaviour
    {
        [SerializeField] ItemFrame item = null;
        [SerializeField] Button openButton = null;
        [SerializeField] Text boxOpenButtonText = null;
        [SerializeField] Button buyButton = null;
        [SerializeField] Image giftBoxIcon = null;
        [SerializeField] Text giftBoxCountText = null;
        [SerializeField] Button rateGuideButton = null;
        [SerializeField] Image itemImage = null;
        [SerializeField] List<Sprite> bigSizeItemSpriteList = new List<Sprite>();


        int gachaTypeKey = 0;
        Asset itemData = null;

        public Asset Item { get { return itemData; } }

        int slotIndex = 0;

        int MaxOpenCount
        {
            get
            {
                if (slotIndex < 0)
                    return 1;
                else
                {
                    switch (slotIndex)
                    {
                        case 0:
                            return Convert.ToInt32(GameConfigData.Get("2023_HOLIDAY_BOX1_OPEN_MAX").VALUE);
                        case 1:
                            return Convert.ToInt32(GameConfigData.Get("2023_HOLIDAY_BOX2_OPEN_MAX").VALUE);
                        case 2:
                            return Convert.ToInt32(GameConfigData.Get("2023_HOLIDAY_BOX3_OPEN_MAX").VALUE);
                    }
                    return 1;
                }
            }
        }

        public void SetData(int _slotIndex, int _typeKey, InventoryItem _itemData)
        {
            if (_itemData == null)
                return;

            itemData = new Asset(_itemData.ItemNo, _itemData.Amount);
            slotIndex = _slotIndex;
            gachaTypeKey = _typeKey;

            if (item != null)
                item.SetFrameItem(itemData);

            if (itemImage != null && _slotIndex < bigSizeItemSpriteList.Count)
                itemImage.sprite = bigSizeItemSpriteList[_slotIndex];

            RefreshButton();
            RefreshIcon();
            RefreshOpenButtonText();
        }

        public int GetAvailableOpenCount(int count = 1)
        {
            return itemData.Amount >= count ? count : itemData.Amount;
        }

        public void RefreshButton()
        {
            if (itemData == null)
                return;

            var upperZeroCount = itemData.Amount > 0;

            if (buyButton != null)
                buyButton.gameObject.SetActive(!upperZeroCount);
            if (openButton != null)
                openButton.gameObject.SetActive(upperZeroCount);
            if (giftBoxCountText != null)
                giftBoxCountText.text = string.Format("x {0}", itemData.Amount);

            if(slotIndex > 0)//2,3 상자는 구매 불가 상품이라 딤드 처리
            {
                if (buyButton != null)
                    buyButton.gameObject.SetActive(false);
                if (openButton != null)
                    openButton.gameObject.SetActive(true);

                openButton.SetButtonSpriteState(upperZeroCount);
            }
            else//0번 상자
            {
                var eventData = DiceEventPopup.GetHolidayData();
                if (eventData == null)
                    return;
                bool isEventPeriod = eventData.IsEventPeriod(false);//endTime 이후로는 구매하기 못함.
                if (!upperZeroCount && buyButton != null)//구매하기 버튼 활성화 시점
                    buyButton.SetButtonSpriteState(isEventPeriod);
            }
        }

        void RefreshIcon()
        {
            if (itemData != null && itemData.BaseData != null && itemData.BaseData.ICON_SPRITE != null)
                giftBoxIcon.sprite = itemData.BaseData.ICON_SPRITE;
        }

        public void OnClickRateBoard()
        {
            if (gachaTypeKey <= 0)
                return;

            if (itemData == null)
                return;

            GachaTablePopup.OpenPopup(gachaTypeKey, StringData.GetStringByStrKey(itemData.GetName()));
        }

        void RefreshOpenButtonText()
        {
            
            if (boxOpenButtonText != null)
            {
                int count = GetAvailableOpenCount();
                if(count > 1)
                    boxOpenButtonText.text = StringData.GetStringFormatByStrKey("open_count", count);
                else
                    boxOpenButtonText.text = StringData.GetStringByStrKey("open");
            }
                
        }
    }
}

