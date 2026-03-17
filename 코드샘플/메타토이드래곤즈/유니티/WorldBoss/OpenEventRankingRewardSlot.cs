using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SRF;

namespace SandboxNetwork
{
    public class OpenEventRankingRewardSlot : RankingRewardSlot
    {        
        public override void Init(int _group, int startRank, int endRank, int itemGroup)
        {
            base.SetRankingText(startRank, endRank);

            //var rewardParentTr = scroll.content.GetComponent<RectTransform>();
            //if (rewardParentTr.childCount > 0)
            //{
            //    var itemEmpty  = items.Count == 0;
            //    foreach (var child in rewardParentTr.GetChildren())
            //    {
            //        var itemFrameCheck = child.GetComponent<ItemFrame>();
            //        if (itemFrameCheck != null && itemEmpty)
            //            items.Add(child.gameObject);

            //        if (itemFrameCheck == null)
            //            child.gameObject.SetActive(false);
            //    }
            //}

            //InitRewardItem();

            //rewardParentTr.pivot = new Vector2(1, 0.5f);

            //SetRewardItem(itemGroup, false);

            //scroll.horizontalNormalizedPosition = 0;
        }

        public void Init(int _group, int startRank, int endRank, List<Asset> rewards)
        {
            base.SetRankingText(startRank, endRank);

            switch (startRank)
            {
                case 1:
                {
                    if (ColorUtility.TryParseHtmlString("#fff600", out Color resultColor))
                    {
                        RankingText.color = resultColor;
                    }
                }
                break;
                case 2:
                {
                    if (ColorUtility.TryParseHtmlString("#b4b4b4", out Color resultColor))
                    {
                        RankingText.color = resultColor;
                    }
                }
                break;
                case 3:
                {
                    if (ColorUtility.TryParseHtmlString("#bca579", out Color resultColor))
                    {
                        RankingText.color = resultColor;
                    }
                }
                break;
            }

            var rewardParentTr = scroll.content.GetComponent<RectTransform>();
            if (rewardParentTr.childCount > 0)
            {
                var itemEmpty = items.Count == 0;
                foreach (var child in rewardParentTr.GetChildren())
                {
                    var itemFrameCheck = child.GetComponent<ItemFrame>();
                    if (itemFrameCheck != null && itemEmpty)
                        items.Add(child.gameObject);

                    if (itemFrameCheck == null)
                        child.gameObject.SetActive(false);
                }
            }

            InitRewardItem();

            rewardParentTr.pivot = new Vector2(1, 0.5f);

            SetRewardItem(rewards, true);

            scroll.horizontalNormalizedPosition = 0;
        }
        public override void SetRewardItem(int itemGroup, bool _init = true)
        {

        }
        public void SetRewardItem(List<Asset> reward, bool _init = true)
        {
            if (_init)
                InitRewardItem();

            var rewardParentTr = scroll.content.GetComponent<RectTransform>();
            for (int i = 0, count = reward.Count; i < count; ++i)
            {
                if (reward[i] != null)
                {
                    if (i >= items.Count)
                    {
                        var item = Instantiate(ItemPrefab, rewardParentTr);
                        item.GetComponent<ItemFrame>().SetAmountTextSizeMultiful(1.65f);
                        item.GetComponent<ItemFrame>().SetTextAlignment(TextAnchor.LowerCenter);
                        items.Add(item);
                    }
                    items[i].SetActive(true);
                    items[i].transform.SetAsLastSibling();
                    var frame = items[i].GetComponent<ItemFrame>();
                    frame.SetFrameItem(reward[i]);
                    frame.SetItemBgOff(false);

                }
            }
            scroll.enabled = reward.Count > scrollViewEnableCount;
        }
    }
}
