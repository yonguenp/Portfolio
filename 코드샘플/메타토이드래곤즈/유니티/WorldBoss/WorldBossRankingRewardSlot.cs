using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SRF;

namespace SandboxNetwork
{
    public class WorldBossRankingRewardSlot : RankingRewardSlot
    {
        const int WORLD_BOSS_FRAME_REWARD_GROUP = 3;
        
        //누적보상 임시
        [SerializeField] List<GameObject> frameRewardList = new List<GameObject>();
        bool isInstanceFrame = false;
        
        public override void Init(int _group, int startRank, int endRank, int itemGroup)
        {
            base.SetRankingText(startRank, endRank);

            var rewardParentTr = scroll.content.GetComponent<RectTransform>();
            if (rewardParentTr.childCount > 0)
            {
                var itemEmpty  = items.Count == 0;
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

            bool isWorldBossFrame = _group >= WORLD_BOSS_FRAME_REWARD_GROUP;

            rewardParentTr.pivot = isWorldBossFrame ? new Vector2(0.5f, 0.5f) : new Vector2(1, 0.5f);

            if (!isWorldBossFrame)
                SetRewardItem(itemGroup, false);
            else//프레임 보상 부분 - 누적 랭킹 - 일단 하드코딩
            {
                var modifyIndex = itemGroup - 1;

                if (!isInstanceFrame)
                {

                    for(int i = 0; i <  frameRewardList.Count; i++)
                    {
                        var prefab = frameRewardList[i];
                        var item = Instantiate(prefab, rewardParentTr);
                        item.name = (i + 1).ToString();

                        var isTargetObj = modifyIndex == i;
                        item.SetActive(isTargetObj);
                    }

                    isInstanceFrame = true;
                }
                else
                {
                    var child = SBFunc.GetChildrensByName(rewardParentTr, new string[] { itemGroup.ToString() });
                    if (child != null)
                        child.gameObject.SetActive(true);
                }
            }
            scroll.horizontalNormalizedPosition = 0;
        }
    }
}
