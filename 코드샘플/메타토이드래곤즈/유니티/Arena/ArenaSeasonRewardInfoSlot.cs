using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ArenaSeasonRewardInfoSlot : MonoBehaviour
    {
        [SerializeField]
        Image bg = null;
        //[SerializeField]
        //Sprite normalBg;
        //[SerializeField]
        //Sprite selectBg;

        [SerializeField]
        Image rankingIconSprite = null;
        [SerializeField]
        Text rankingIconName = null;

        [SerializeField]
        Sprite defaultRankIconSprite = null;

        [Header("Season Reward Layer")]
        [SerializeField]
        Text TrophyCount = null;
        [SerializeField]
        RectTransform seasonRewardLayer = null;
        [SerializeField]
        GameObject seasonEmpty = null;

        List<GameObject> seasonRewardList = new List<GameObject>();

        [Header("Rank Reward Layer")]
        [SerializeField]
        RectTransform rankRewardLayer = null;
        [SerializeField]
        GameObject rankEmpty = null;

        List<GameObject> rankRewardList = new List<GameObject>();

        [SerializeField]
        Image arenaPointImage = null;
        [SerializeField]
        Text arenaPointLabel = null;

        ArenaRankData currentSeasonRewardData = null;

        int seasonID = 0;

        public void Init(ArenaRankData rewardData)
        {
            currentSeasonRewardData = rewardData;
            SetRewardSlotData();
        }

        void SetRewardSlotData()
        {
            if (currentSeasonRewardData == null) 
                return;
            seasonID = ArenaManager.Instance.UserArenaData.GetRewardSeasonID();

            //if (bg != null)
            //{
            //    // 내가 받을 보상
            //    bg.sprite = currentSeasonRewardData.GROUP == (int)ArenaManager.Instance.UserArenaData.SeasonGrade ? selectBg : normalBg;
            //}

            // 랭크 아이콘 관련 세팅
            Sprite icon;
            icon = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, currentSeasonRewardData.ICON);
            if (icon == null)
                icon = defaultRankIconSprite;
            rankingIconSprite.sprite = icon;

            if(rankingIconName != null)
                rankingIconName.text = StringData.GetStringByStrKey(currentSeasonRewardData._NAME);
            //

            ResetRewardSlot();

            //if (User.Instance.IS_HOLDER)
            //{
            //    SetItemGroup(currentSeasonRewardData.HOLDER_FIRST_REWARD_GROUP, rankRewardList, rankRewardLayer, rankEmpty, true);
            //    SetItemGroup(currentSeasonRewardData.HOLDER_SEASON_REWARD_GROUP, seasonRewardList, seasonRewardLayer, seasonEmpty);
            //}
            //else
            {
                SetItemGroup(currentSeasonRewardData.FIRST_REWARD_GROUP, rankRewardList, rankRewardLayer, rankEmpty, true);
                SetItemGroup(currentSeasonRewardData.SEASON_REWARD_GROUP, seasonRewardList, seasonRewardLayer, seasonEmpty);
            }

            if(currentSeasonRewardData.GROUP >= (int)eArenaRankGrade.MASTER4)
            {
                TrophyCount.gameObject.SetActive(true);
                TrophyCount.text = SBFunc.CommaFromNumber(currentSeasonRewardData.NEED_POINT);
            }
        }

        private void SetItemGroup(int itemGroup, List<GameObject> rewardList, RectTransform parent, GameObject empty, bool isRank = false)
        {
            var rewards = ItemGroupData.Get(itemGroup);
            var isEmpty = rewards == null;
            if (empty != null)
                empty.SetActive(isEmpty);
            if (isEmpty)
                return;

            for (int i = 0, count = rewards.Count; i < count; ++i)
            {
                var curReward = rewards[i];
                if (curReward == null || curReward.Reward == null)
                    continue;

                if(curReward.Reward.GoodType == eGoodType.MAGNET)
                {
                    if (false == User.Instance.IS_HOLDER || (isRank && seasonID > 1))
                        continue;
                }

                var newItemSlot = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), parent);
                ItemFrame itemframe = newItemSlot.GetComponent<ItemFrame>();
                if (itemframe != null)
                {
                    itemframe.SetTextAlignment(TextAnchor.LowerCenter);
                    itemframe.SetAmountTextSizeMultiful(1.4f);
                    itemframe.SetFrameItem(curReward.Reward);
                    itemframe.SetMediumSizeArenaPointIcon();
                    itemframe.SetItemBgOff(false);
                }


                rewardList.Add(newItemSlot);
            }

            var rewardCount = rewardList.Count;
            for (int i = 0, count = rewardCount; i < count; ++i)
            {
                if (rewardList[i] == null)
                    continue;

                rewardList[i].transform.localScale = Vector3.one * 0.7f;
            }

            var layout = parent.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
                layout.spacing = rewardCount < 3 ? 100 : 60 + 20 * (3 - rewardCount);

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
        }

        void ResetRewardSlot()
        {
            SBFunc.RemoveAllChildrens(seasonRewardLayer);
            SBFunc.RemoveAllChildrens(rankRewardLayer);

            seasonRewardList.Clear();
            rankRewardList.Clear();
            TrophyCount.gameObject.SetActive(false);
        }
    }
}
