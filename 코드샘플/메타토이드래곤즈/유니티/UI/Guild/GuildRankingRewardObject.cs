using SRF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GuildRankingRewardObject : RankingRewardSlot
    {
        [SerializeField]
        Image accumFrameImg = null;
        [SerializeField]
        GameObject NoneRewardTextObj = null;
        //누적보상
        bool isInstanceFrame = false;

        public override void Init(int _group, int index, int startRank, int endRank, int itemGroup)
        {
            SetRankingText(startRank, endRank);
            NoneRewardTextObj.SetActive(false);
            
            accumFrameImg.gameObject.SetActive(false);
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

            bool isAccumGroup = _group == (int)eGuildRankType.SumRanking;

            //rewardParentTr.pivot = isAccumGroup ? new Vector2(0.5f, 0.5f) : new Vector2(1, 0.5f);
            rewardParentTr.pivot = new Vector2(0.5f, 0.5f);

            if (!isAccumGroup)
                SetRewardItem(itemGroup, false);
            else
            {
                var imageIdx = GuildRankRewardData.GetByRankGroup(startRank, (eGuildRankRewardGroup)index).ACCUMULATE_REWARD;
                accumFrameImg.gameObject.SetActive(true);
                var data = GuildResourceData.Get(imageIdx);
                if (data != null)
                {
                    accumFrameImg.color = new Color(1f,1f, 1f, 1f);
                    accumFrameImg.sprite = data.RESOURCE;
                }
                else
                {
                    accumFrameImg.color = new Color(1f, 1f, 1f, 0f);
                    NoneRewardTextObj.SetActive(true);
                }

                scroll.enabled = false;
            }
            scroll.horizontalNormalizedPosition = 0;
        }

        public override void SetRankingText(int startRank, int endRank)
        {
            if (startRank == endRank)
                RankingText.text = SBFunc.GetRankText(startRank); 
            else
            {
                if (endRank == -1)
                    RankingText.text = StringData.GetStringFormatByStrKey("나머지순위", startRank);
                else
                    RankingText.text = StringData.GetStringFormatByStrKey("순위범위", startRank, endRank);
            }
        }
    }
}

