using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork 
{
    public class StageInfoPopup : Popup<StageInfoPopupData>
    {
        [SerializeField] private TableView tableview = null;

        [Header("StageInfo")]
        [SerializeField] private Text labelStage = null;
        [SerializeField] private Text labelStageName = null;
        [SerializeField] private Text labelBattlePoint = null;
        [SerializeField] private GameObject[] arrNodeStar = null;
        [SerializeField] private Text labelReqEnergy = null;

        private WorldData worldInfo = null;
        private StageBaseData stageInfo = null;

        public override void Init(StageInfoPopupData data)
        {
            base.Init(data);

            worldInfo = WorldData.GetByWorldNumber(Data.World);
            stageInfo = StageBaseData.GetByAdventureWorldStage(Data.World, Data.Stage);
			List<MonsterSpawnData> spawnInfo = MonsterSpawnData.GetBySpawnGroup(stageInfo.SPAWN);
			int enemyTotalBp = 0;

			for (int i = 0; i < arrNodeStar.Length; ++i)
            {
                arrNodeStar[i].SetActive(Data.Star > i);
            }

            if(spawnInfo != null)
            {
                for(int i = 0; i < spawnInfo.Count; ++i)
                {
                    if (spawnInfo[i] == null) continue;

                    enemyTotalBp += spawnInfo[i].INF;
                }
            }

            if(labelStage != null)
            {
                labelStage.text = string.Format("{0}-{1}", Data.World, Data.Stage);
            }

            if(labelStageName != null)
            {
                if(worldInfo != null)
                {
                    labelStageName.text = StringData.GetStringByIndex(worldInfo._NAME);
                }
                else
                {
                    labelStageName.text = "";
                }
            }

            if(labelBattlePoint != null)
            {
                labelBattlePoint.text = enemyTotalBp.ToString();
            }

			if (labelReqEnergy != null)
			{
				labelReqEnergy.text = string.Format("x{0}", stageInfo.COST_VALUE);
			}

			SetRewardScrollview();
        }

        void SetRewardScrollview()
        {
            List<ITableData> items = new List<ITableData>();
			for(int i = 0; i < stageInfo.GetStarRewardCount(); i++)
			{
				//못받은 별 보상이 있다면
				if (Data.Star < i + 1)
				{
                    foreach(var reward in stageInfo.GetStarRewards(i))
                    {
                        RewardItemInfo Item = new RewardItemInfo(reward.Reward.ItemNo, reward.Reward.Amount, i + 1, false);
                        items.Add(Item);
                    }
				}
			}

			//공통 리워드 추가
            ReceivedNormalReward(items, 10000003, stageInfo.REWARD_ACCOUNT_EXP);
            ReceivedNormalReward(items, 10000004, stageInfo.REWARD_CHAR_EXP);
            ReceivedNormalReward(items, 10000001, stageInfo.REWARD_GOLD);

            //전용 리워드 추가
            var rewards = stageInfo.REWARD_ITEMS;
            if (rewards.Count > 0)
            {
                for (int i = 0; i < rewards.Count; ++i)
                {
                    var elem = rewards[i];
                    if (elem == null) continue;
                    ReceivedNormalReward(items, elem.ItemNo, elem.Amount);
                }
            }

            if (tableview != null)
            {
                tableview.OnStart();
                tableview.SetDelegate(new TableViewDelegate(items, (GameObject itemNode, ITableData item) => 
				{
                    ItemFrame frame = itemNode.GetComponent<ItemFrame>();
                    RewardItemInfo reward = (RewardItemInfo)item;

                    if (reward.star > 0)
                    {
                        frame.SetFrameItem(reward.itemNo, reward.count, (int)reward.type);
                    }
                    else
                    {
                        frame.SetFrameItemInfo(reward.itemNo, reward.count);
                    }

                    frame.setRewardStar(reward.star);
                    frame.setFrameCheck(reward.check);

                }));

                tableview.ReLoad();
            }
        }

        void ReceivedNormalReward(List<ITableData> items, int itemNo, int count)
        {
            if (count > 0)
            {
                RewardItemInfo Item = new RewardItemInfo(itemNo, count, 0, false);

                items.Add(Item);
            }
        }

        public void OnClickDeploy()
        {
            StageManager.Instance.SetAll(Data.World, Data.Stage, Data.Diff);
            StageManager.Instance.SetLastEnterWorld(Data.World);
            PopupManager.ClosePopup<StageInfoPopup>();
            //LoadingManager.ImmediatelySceneLoad("AdventureReadyScene");
            LoadingManager.Instance.EffectiveSceneLoad("AdventureReadyScene", eSceneEffectType.CloudAnimation);

        }

		public override void InitUI() { }
	}
}