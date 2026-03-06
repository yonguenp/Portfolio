using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork 
{
    public class WorldStageInfo : MonoBehaviour
    {
        [SerializeField] private GameObject[] starIcon_normal = null;
        [SerializeField] private GameObject[] starIcon_hard = null;
        [SerializeField] private GameObject[] starIcon_hell = null;

        [SerializeField] private GameObject starNode = null;
        [SerializeField] private Button autoAdventureButton = null;
        [SerializeField] private Button autoAdventureWithTicketButton = null;
        [SerializeField] private Text labelEnemyBattlePoint = null;
        [SerializeField] private Text labelReqEnergy = null;
        //[SerializeField] private TableView monsterTableView = null;
        [SerializeField] private TableView rewardTableView = null;
        [SerializeField] private GameObject rewardScrollArrow =  null;

        [SerializeField] private TableView rewardTableView_additional = null;
        [SerializeField] private GameObject rewardScrollArrow_additional = null;

        [SerializeField] private Button okButton = null;
        [SerializeField] DOTweenAnimation animObj = null;

        [Header("monster view")]
        [SerializeField] private Transform monsterViewParentTr = null;
        [SerializeField] private GameObject monsterObj = null;
        private List<GameObject> monsterObjs = null;

        public bool IsLock { get; private set; } = false;

        public int WorldIndex { get; private set; } = 0;

        public int StageIndex { get; private set; } = 0;

        public bool IsDetailAlreadyOpen { get; private set; } = false;

        public delegate void CallBack();
        private CallBack BattleStartCallBack;
        private int star = -1;
        const int starMaxCount = 3;
		bool isGameOff = false;
        bool isRewardItemOver = false;
        int enemyTotalBp = 0;

        bool isTableViewFirstInit = false;
        public void SetData(int worldNum, int stageNum, int starCount, bool is_lock, bool isAlreayOpen, CallBack battleStart_cb = null)
        {
            WorldIndex = worldNum;
            StageIndex = stageNum;
            IsLock = is_lock;
            IsDetailAlreadyOpen = isAlreayOpen;
            BattleStartCallBack = battleStart_cb;
            star = starCount;

            SetVisibleState();
            SetStarCount(starCount, IsLock);

            SetEnemyScrollView();
            SetRewardScrollView();
            SetBattleStartButton();
            
            PopupManager.Instance.Top.SetStaminaTimeCallBack(SetBattleStartButton);
            UIManager.Instance.MainUI.SetStaminaTimeCallBack(SetBattleStartButton);
        }
        
        private void OnApplicationQuit()
        {
            isGameOff = true;
        }

        private void OnDestroy()
        {
            if (isGameOff) 
				return;
            PopupManager.Instance.Top.SetStaminaTimeCallBack(null);
            UIManager.Instance.MainUI.SetStaminaTimeCallBack(null);
        }

        void SetVisibleState()
        {
            starNode.SetActive(true);
        }


        void SetStarCount(int StarCount, bool is_lock)
        {
            for (int i = 0; i < starMaxCount; ++i)
            {
                starIcon_normal[i].SetActive(false);
                starIcon_hard[i].SetActive(false);
                starIcon_hell[i].SetActive(false);
            }

            GameObject[] starSpriteTargetNodeList = null;
            switch(CacheUserData.GetInt("adventure_difficult", 1))
            {
                case 2:
                    starSpriteTargetNodeList = starIcon_hard;
                    break;
                case 3:
                    starSpriteTargetNodeList = starIcon_hell;
                    break;
                case 1:
                default:
                    starSpriteTargetNodeList = starIcon_normal;
                    break;
            }
            if (!is_lock)
            {
				for (int i = 0; i < starMaxCount; ++i)
				{
					starSpriteTargetNodeList[i].SetActive(i < StarCount);
				}

				if (autoAdventureButton != null)
				{
					autoAdventureButton.SetButtonSpriteState(StarCount >= 1);
				}

                if (autoAdventureWithTicketButton != null)
                {
                    autoAdventureWithTicketButton.gameObject.SetActive(GameConfigTable.GetConfigIntValue("sweep_active", 1) > 0);
                    autoAdventureWithTicketButton.SetButtonSpriteState(StarCount >= 3);
                }
            }
        }

        void SetEnemyScrollView()
        {
            StageBaseData stageInfo = StageBaseData.GetByAdventureWorldStage(WorldIndex, StageIndex);
            List<MonsterSpawnData> spawnInfo = MonsterSpawnData.GetBySpawnGroup(stageInfo.SPAWN);
			
			enemyTotalBp = 0;

            if (monsterObjs == null)
            {
                monsterObjs = new List<GameObject>();
            }

            foreach (var elem in monsterObjs)
            {
                elem.SetActive(false);
            }

            List<MonsterBaseData> registed_monsters = new List<MonsterBaseData>();
            int index = 0;
            foreach (MonsterSpawnData elem in spawnInfo)
            {
                var baseData = MonsterBaseData.Get(elem.MONSTER.ToString());

                if (!registed_monsters.Contains(baseData))
                {
                    registed_monsters.Add(baseData);
                    if (monsterObjs.Count <= index)
                    {
                        var obj = Instantiate(monsterObj, monsterViewParentTr);
                        monsterObjs.Add(obj);
                    }

                    monsterObjs[index].SetActive(true);
                    monsterObjs[index].GetComponent<EnemyFrame>().SetEnemyFrame(int.Parse(baseData.GetKey()), baseData.ELEMENT, elem.IS_BOSS > 0);
                    ++index;
                }

                enemyTotalBp += elem.INF;
            }


            //monsterTableView.OnStart();

            //if (monsterTableView != null)
            //{
            //    monsterTableView.SetDelegate(new TableViewDelegate(monsterKeyList, (GameObject itemNode, ITableData item) =>
            //    {
            //        EnemyFrame frame = itemNode.GetComponent<EnemyFrame>();
            //        if (frame != null)
            //        {
            //            int num = int.Parse(item.GetKey());
            //            frame.SetEnemyFrame(num, element);
            //        }
            //    }));
            //}

            //monsterTableView.ReLoad();

            if (!TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Adventure))//튜토리얼이 아닐때만
            {
                float HP_rate = ServerOptionData.GetJsonValueFloat("stage_balance", "hp", 1.0f);
                float ATK_rate = ServerOptionData.GetJsonValueFloat("stage_balance", "atk", 1.0f);
                float DEF_rate = ServerOptionData.GetJsonValueFloat("stage_balance", "def", 1.0f);

                float nomalize = (HP_rate + ATK_rate + DEF_rate) / 3;
                enemyTotalBp = (int)(enemyTotalBp * nomalize);
            }

            labelEnemyBattlePoint.text = enemyTotalBp.ToString();
            labelReqEnergy.text = string.Format("-{0}", stageInfo.COST_VALUE);
        }

        void SetRewardScrollView()
        {
            var stageInfo = StageBaseData.GetByAdventureWorldStage(WorldIndex, StageIndex);
            List<ITableData> items = new List<ITableData>();
            List<ITableData> items_additional = new List<ITableData>();

            foreach(var item in stageInfo.GetStarRewards(0))
            {
                NotReceivedStarReward(items, 1, item.Reward);
            }
            foreach (var item in stageInfo.GetStarRewards(1))
            {
                NotReceivedStarReward(items, 2, item.Reward);
            }
            foreach (var item in stageInfo.GetStarRewards(2))
            {
                NotReceivedStarReward(items, 3, item.Reward);
            }
            
            ReceivedNormalReward(items, 10000003, stageInfo.REWARD_ACCOUNT_EXP);
            ReceivedNormalReward(items, 10000004, stageInfo.REWARD_CHAR_EXP);
            ReceivedNormalReward(items, 10000001, stageInfo.REWARD_GOLD);

            List<int> addtionals = new List<int>();
            var rewards = stageInfo.REWARD_ITEMS;
            if (rewards.Count > 0)
            {
                int dataCount = rewards.Count;
                for (int i = 0; i < dataCount; ++i)
                {
                    var reward = rewards[i];
                    if (reward.GoodType == eGoodType.DICE_GROUP)
                    {
                        var diceDatas = ItemGroupData.Get(reward.ItemNo);
                        for (int j = 0; j < diceDatas.Count; ++j)
                        {
                            var r = diceDatas[j].Reward;
                            if (addtionals.Contains(r.ItemNo))
                            {
                                continue;
                            }

                            addtionals.Add(r.ItemNo);
                            ReceivedAdditionalReward(items_additional, r.ItemNo, (int)(r.Amount * ServerOptionData.GetFloat("adventure_reward", 1.0f)));
                        }
                    }
                    else
                    {
                        ReceivedNormalReward(items, reward.ItemNo, reward.Amount);
                    }

                }
            }

            if(rewardTableView != null && rewardTableView_additional != null && !isTableViewFirstInit)
            {
                rewardTableView.OnStart();
                rewardTableView_additional.OnStart();
                isTableViewFirstInit = true;
            }

            isRewardItemOver = items.Count > 5;
            rewardScrollArrow.SetActive(isRewardItemOver);
            if (rewardTableView != null)
            {
                rewardTableView.SetDelegate(new TableViewDelegate(items, (GameObject itemNode, ITableData item) => 
				{
					ItemFrame frame = itemNode.GetComponent<ItemFrame>();

                    if(frame != null)
                    {
						RewardItemInfo itemData = (RewardItemInfo)item;

						if (itemData.star > 0)
						{
                            frame.SetFrameItem(itemData.itemNo, itemData.count, (int)itemData.type);
                        }
						else
						{
							frame.SetFrameItemInfo(itemData.itemNo, itemData.count);
						}

						frame.setRewardStar(itemData.star);
                        //frame.SetItemBgOff();

                    }
                }));

                rewardTableView.ReLoad();
            }

            if(rewardTableView_additional != null)
            {
                if (items_additional.Count > 0)
                {
                    rewardTableView_additional.gameObject.SetActive(true);
                    rewardTableView_additional.SetDelegate(new TableViewDelegate(items_additional, (GameObject itemNode, ITableData item) =>
                    {
                        ItemFrame frame = itemNode.GetComponent<ItemFrame>();

                        if (frame != null)
                        {
                            RewardItemInfo itemData = (RewardItemInfo)item;

                            if (itemData.star > 0)
                            {
                                frame.SetFrameItem(itemData.itemNo, itemData.count, (int)itemData.type);
                            }
                            else
                            {
                                frame.SetFrameItemInfo(itemData.itemNo, itemData.count);
                            }

                            frame.setRewardStar(itemData.star);
                        //frame.SetItemBgOff();

                    }
                    }));
                    rewardTableView_additional.ReLoad();
                }
                else
                {
                    rewardTableView_additional.gameObject.SetActive(false);
                }
            }
        }

        void ReceivedNormalReward(List<ITableData> items, int item, int count)
        {
            if (count > 0)
            {
                items.Add(new RewardItemInfo(item, count, 0, false));
            }
        }

        void ReceivedAdditionalReward(List<ITableData> items, int item, int count)
        {
            if (count > 0)
            {
                items.Add(new RewardItemInfo(item, count, 0, false));
            }
        }

        void NotReceivedStarReward(List<ITableData> items, int stars, Asset reward)
        {
            if (star < stars)
            {
                if (reward.Amount > 0)
                {
                    items.Add(new RewardItemInfo(reward, stars, false));
                }
            }
        }

        void SetBattleStartButton()
        {
            if(okButton != null)
            {
                var isSufficientCondition = IsSufficientEnergyToBattleStart();
                okButton.SetButtonSpriteState(isSufficientCondition);
                SetBubbleNodeEffect(isSufficientCondition);
            }
        }

        void SetBubbleNodeEffect(bool _isNormal)
        {
            if (animObj != null)
            {
                if (_isNormal)
                    animObj.DOPlay();
                else
                    animObj.DOPause();
            }
        }

        bool IsSufficientEnergyToBattleStart()
        {
            var currentUserEnergy = User.Instance.UserData.Energy;
            var stageInfo = StageBaseData.GetByAdventureWorldStage(WorldIndex, StageIndex);
            var needEnergy = stageInfo.COST_VALUE;

            return currentUserEnergy >= needEnergy;
        }

        public void OnClickBattleStart()
        {
            if (!IsSufficientEnergyToBattleStart())
            {
                ToastManager.On(100000134);
                return;
            }

            if (User.Instance.CheckInventoryForContentsEnter(eInvenSlotCheckContentType.Adventure, WorldIndex, StageIndex)==false)
            {
                IsFullBackAlert();
                return;
            }
            if (BattleStartCallBack != null)
            {
                BattleStartCallBack();
            }
        }

        public void OnClickAutoAdventureButton()
        {
            if(star <= 0)//현재 스테이지 클리어 X
            {
                ToastManager.On(100002566);
                return;
            }

            if (User.Instance.CheckInventoryForContentsEnter(eInvenSlotCheckContentType.Adventure, WorldIndex, StageIndex) == false)
            {
                IsFullBackAlert();
                return;
            }

            var popup = PopupManager.OpenPopup<AutoAdventurePopup>(new StagePopupData(WorldIndex,StageIndex));
            popup.SetCallBack(OnClickAutoStart);
        }

        public void OnClickAutoAdventureWithTicketButton()
        {
            if (star < 3)//현재 스테이지 클리어 X
            {
                ToastManager.On("소탕권사용제한");
                return;
            }

            if (User.Instance.CheckInventoryForContentsEnter(eInvenSlotCheckContentType.Adventure, WorldIndex, StageIndex) == false)
            {
                IsFullBackAlert();
                return;
            }

            var popup = PopupManager.OpenPopup<AutoAdventureWithTicketPopup>(new StagePopupData(WorldIndex, StageIndex));
            popup.SetCallBack(OnClickAutoWithTicketStart);
        }

        public void OnClickAutoStart(StageBaseData _currentStageData)
        {
            var countCheckData = StageManager.AccumCount;
            if (countCheckData >= 0)
            {
                StageManager.StageCompleteAccum();
            }

            var worldIndexCheck = _currentStageData.WORLD;
            var stageIndexCheck = _currentStageData.STAGE;

            if(worldIndexCheck != WorldIndex)
            {
                WorldIndex = worldIndexCheck;
            }

            if(stageIndexCheck != StageIndex)
            {
                StageIndex = stageIndexCheck;
            }

            OnClickBattleStart();
        }

        public void OnClickAutoWithTicketStart(StageBaseData _currentStageData, int repeat)
        {
            WWWForm param = new();
            param.AddField("world", _currentStageData.WORLD);
            param.AddField("diff", (int)_currentStageData.DIFFICULT);
            param.AddField("stage", _currentStageData.STAGE);
            param.AddField("deck", User.Instance.PrefData.AdventureBattleLine.GetJsonString());
            param.AddField("repeat", repeat);

            NetworkManager.Send("adventure/sweep", param, (JObject jsonData) =>
            {
                if (jsonData["rs"] != null)
                {
                    switch ((eApiResCode)(int)jsonData["rs"])
                    {
                        case eApiResCode.OK:
                            AdventureManager.Instance.SetAutoTicketData(_currentStageData.WORLD, _currentStageData.STAGE, repeat, jsonData);
                            LoadingManager.Instance.EffectiveSceneLoad("AdventureReward", eSceneEffectType.CloudAnimation);
                            break;

                        case eApiResCode.COST_SHORT:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100002249), StringData.GetStringByIndex(100002249));
                            break;
                        case eApiResCode.ADV_INVALID_DRAGON:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000635), true, false, false);
                            break;
                    }
                }
            }, (string arg) =>
            {

            });
        }

        void IsFullBackAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                () => {
                    LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(()=> 
				    {
                        PopupManager.OpenPopup<InventoryPopup>();
                    }));
                }
            );
            return;
        }

        public void RewardScrollChange(Vector2 scrollValue)
        {
            if (isRewardItemOver)
            {
                rewardScrollArrow.SetActive(scrollValue.x < 0.1f);
            }
        }

        public void UpdateMyBattlePoint(int bp)
        {
            int enemyBP = enemyTotalBp;
            float buffer = bp * 0.1f;

            if(bp - buffer > enemyBP)
            {
                labelEnemyBattlePoint.color = Color.green;
            }
            else if(bp + buffer < enemyBP)
            {
                labelEnemyBattlePoint.color = Color.red;
            }
            else
            {
                labelEnemyBattlePoint.color = Color.yellow;
            }
        }
    }
}