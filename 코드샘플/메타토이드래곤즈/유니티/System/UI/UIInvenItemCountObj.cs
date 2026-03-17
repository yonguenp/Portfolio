using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIInvenItemCountObj : UIObject
    {
        [SerializeField]
        private Image Icon = null;
        [SerializeField]
        private Text AmountLabel = null;

        int currentItem = 0;
        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            RefreshItem();
        }

        public override bool RefreshUI(eUIType targetType)
        {
            if (base.RefreshUI(targetType))
            {
                RefreshItem();
            }
            return curSceneType != targetType;
        }

        public void RefreshItem(int defaultBookNum = -1)
        {
            if (AmountLabel != null)
            {
                if (defaultBookNum > 0)
                {
                    currentItem = defaultBookNum;
                }
                var itemData = ItemBaseData.Get(currentItem);
                if (itemData == null)
                    return;
                var iconImage = itemData.ICON_SPRITE;
                int itemCount = 0;

                if (currentItem == SBDefine.ITEM_MAGNET)
                {
                    itemCount = User.Instance.UserData.Magnet;
                }
                else
                {
                    var item = User.Instance.GetItem(currentItem);
                    if (item != null)
                    {
                        itemCount = item.Amount;
                    }
                }
                if (iconImage != null && Icon != null)
                {
                    Icon.sprite = iconImage;
                }

                AmountLabel.text = SBFunc.CommaFromNumber(itemCount);
            }
        }
    }
}
