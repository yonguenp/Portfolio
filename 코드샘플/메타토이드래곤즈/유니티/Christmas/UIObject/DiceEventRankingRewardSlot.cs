using SRF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DiceEventRankingRewardSlot : MonoBehaviour
    {
        [SerializeField]
        Text RankingText = null;
        [SerializeField]
        ScrollRect scroll = null;
        [SerializeField]
        GameObject ItemPrefab = null;

        private List<GameObject> items = new List<GameObject>();


        public void Init(int key,int startRank, int endRank, int itemGroup)
        {
            var itemGroupDat = ItemGroupData.Get(itemGroup);
            RankingText.color = startRank < 4 ? Color.yellow : Color.white;
            //임시 -- 데이터 나오면 필히 바꿀 것
            if (startRank == endRank)
                RankingText.text = SBFunc.GetRankText(startRank);
            else
            {
                if(endRank == -1)
                    RankingText.text = StringData.GetStringFormatByStrKey("나머지순위", startRank);
                else
                {
                    RankingText.text = StringData.GetStringFormatByStrKey("순위범위", startRank,endRank);
                }
            }

            var rewardParentTr = scroll.content.GetComponent<RectTransform>();
            if (rewardParentTr.childCount > 0&& items.Count ==0)
            {
                foreach(var child in rewardParentTr.GetChildren())
                {
                    items.Add(child.gameObject);
                }
            }
            foreach (var item in items)
            {
                item.SetActive(false);
            }
            if(itemGroupDat != null)
            {
                for (int i = 0, count = itemGroupDat.Count; i < count; ++i)
                {
                    if(itemGroupDat[i].Reward != null)
                    {
                        if (itemGroupDat[i].Reward.BaseData != null && itemGroupDat[i].Reward.BaseData.ENABLE_P2E && !User.Instance.ENABLE_P2E)
                            continue;

                        if (itemGroupDat[i].Reward.Amount > 0)
                        {
                            if (i >= items.Count)
                            {
                                var item = Instantiate(ItemPrefab, rewardParentTr);
                                item.GetComponent<ItemFrame>().SetAmountTextSizeMultiful(1.7f);
                                item.GetComponent<ItemFrame>().SetTextAlignment(TextAnchor.LowerCenter);
                                items.Add(item);
                            }
                            items[i].SetActive(true);
                            items[i].transform.SetAsLastSibling();
                            var frame = items[i].GetComponent<ItemFrame>();
                            frame.SetFrameItem(itemGroupDat[i].Reward);
                            frame.SetItemBgOff(false);
                        }
                        
                    }
                }
                scroll.enabled = itemGroupDat.Count > 2;
            }
            scroll.horizontalNormalizedPosition = 0;
        }
    }

}