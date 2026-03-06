using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class LuckyBagReinforceRewardSlot : MonoBehaviour
    {
        [SerializeField] ItemFrame item = null;
        [SerializeField] Text rewardCountText = null;
        [SerializeField] Text rateText = null;

        ItemGroupData data = null;
        public void SetData(ItemGroupData _data)
        {
            if (_data == null)
                return;

            data = _data;

            var curItem = data.Reward;
            if (curItem == null)
                return;

            item.SetFrameItem(curItem);

            if (rewardCountText != null)
                rewardCountText.text = curItem.Amount.ToString();

            if (rateText != null)
            {
                var successPercent = (float)(data.ITEM_RATE / (float)SBDefine.MILLION * 100);
                rateText.text = SBFunc.StrBuilder(Math.Round(successPercent, 2), "%");
            }
        }
    }
}