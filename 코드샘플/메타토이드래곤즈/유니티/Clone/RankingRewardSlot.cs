using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 랭킹 보상 슬롯 관련 기본 클래스
    /// </summary>
    public class RankingRewardSlot : MonoBehaviour
    {
        [SerializeField]
        protected Text RankingText = null;
        [SerializeField]
        protected ScrollRect scroll = null;
        [SerializeField]
        protected GameObject ItemPrefab = null;
        [SerializeField]
        protected int scrollViewEnableCount = 3;

        protected List<GameObject> items = new List<GameObject>();

        public virtual void Init(int startRank, int endRank, int itemGroup)
        {
            SetRankingText(startRank, endRank);
            SetRewardItem(itemGroup);
        }

        public virtual void Init(int _group, int startRank, int endRank, int itemGroup)
        {
            SetRankingText(startRank, endRank);
            SetRewardItem(itemGroup);
        }

        public virtual void Init(int _group, int _index ,int startRank, int endRank, int itemGroup)
        {
            SetRankingText(startRank, endRank);
            SetRewardItem(itemGroup);
        }

        public virtual void SetRankingText(int startRank, int endRank)
        {
            RankingText.color = startRank < 4 ? Color.yellow : Color.white;
            if (startRank == endRank)
            {
                if(startRank == -1)
                    RankingText.text = StringData.GetStringByStrKey("참여보상");
                else
                    RankingText.text = SBFunc.GetRankText(startRank);
            }
            else
            {
                if (endRank == -1)
                    RankingText.text = StringData.GetStringFormatByStrKey("나머지순위", startRank);
                else
                    RankingText.text = StringData.GetStringFormatByStrKey("순위범위", startRank, endRank);
            }
        }

        public void InitRewardItem()
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    item.SetActive(false);
                }
            }
        }

        public virtual void SetRewardItem(int itemGroup, bool _init = true)
        {
            if(_init)
                InitRewardItem();

            var rewardParentTr = scroll.content.GetComponent<RectTransform>();
            var itemGroupData = ItemGroupData.Get(itemGroup);
            if (itemGroupData != null)
            {
                for (int i = 0, count = itemGroupData.Count; i < count; ++i)
                {
                    if (itemGroupData[i].Reward != null)
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
                        frame.SetFrameItem(itemGroupData[i].Reward);
                        frame.SetItemBgOff(false);

                    }
                }
                scroll.enabled = itemGroupData.Count > scrollViewEnableCount;
            }
        }
    }
}

