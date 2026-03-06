using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DailyResult : MonoBehaviour
    {
        private DailyBattleData Data { get; set; } = null;
        [Header("BG")]
        [SerializeField]
        private Image bgImg = null;
        [SerializeField]
        private Sprite[] bgSprites = null;

        [Space]
        [Header("Time")]
        [SerializeField]
        private Text battleTimeLabel = null;

        [Space]
        [Header("LevelLayer")]
        [SerializeField]
        private GameObject winLayerNode = null;
        [SerializeField]
        private GameObject loseLayerNode = null;

        [Space]
        [Header("Dragons")]
        [SerializeField]
        private GameObject[] arrDragonNode = null;
        [SerializeField]
        private GameObject[] arrLvBG = null;
        [SerializeField]
        private Text[] arrLvNow = null;
        [SerializeField]
        private Text[] arrExpLabel = null;
        [SerializeField]
        private Slider[] arrExpSlider = null;
        [SerializeField]
        private Animator[] expLevelAnimators = null;
        [SerializeField]
        private Color failureLvColor = Color.black;
        [SerializeField]
        private Color winnerLvColor = Color.black;

        [Space]
        [Header("Animation")]
        [SerializeField]
        private Animator resultAnimator = null;
        [SerializeField]
        private float fontStartDelay = 0f;
        [SerializeField]
        private GameObject winAnim = null;
        [SerializeField]
        private GameObject loseAnim = null;
        [SerializeField]
        private CanvasGroup buttomGroup = null;

        [Space]
        [Header("Rewards")]
        [SerializeField]
        private ScrollRect rewardScroll = null;
        [SerializeField]
        private GameObject nodeRewardContent = null;
        [SerializeField]
        private GameObject itemAniPrefab = null;

        [Space]
        [Header("Restart And Next Btn")]
        [SerializeField]
        private Button retryBtn = null;
        [SerializeField]
        private GameObject retryFreeLayer = null;
        [SerializeField]
        private GameObject retryAdvLayer = null;
        [SerializeField]
        private Text freeEnterCntText = null;
        [SerializeField]
        private Text AdvEnterCntText = null;
        [SerializeField]
        private Text advTimerText = null;
        [SerializeField]
        private Button nextStageBtn = null;


        [Space(10)]
        [SerializeField]
        private GameObject StatisticBtn = null;

        private bool isWin = false;
        private bool isAdvWaitEnd = true;
        private bool isRequest = false;
        private List<int> levelUpIndexList = null;
        private int rewardCount = 0;
        private float itemsTime = 0;
        private List<GameObject> newItems = null;
        private int freeCount { get { return GameConfigTable.GetConfigIntValue("DAY_DUNGEON_CLEAR_COUNT"); } }
        private int adCount { get { return AdvertisementData.Get(SBDefine.AD_DAILY_KEY).LIMIT; } } 

        private TimeObject timeObj = null;

        void Start()
        {
            InitializeData();
            if (Data == null)
                return;

            UIManager.Instance.InitUI(eUIType.None);

            InitializeBG();
            InitializeTime();
            InitializeLevelLayer();
            InitializeDragons();
            InitializeReward();
            InitializeBtn();
            InitializeAnimation();
        }
        private void InitializeData()
        {
            Data = DailyManager.Instance.Data;
            if (Data == null)
                return;
            
            switch (Data.State)
            {
                case eBattleState.Playing:
                case eBattleState.Win:
                    isWin = true;
                    break;
                case eBattleState.Lose:
                default:
                    isWin = false;
                    break;
            }
            isRequest = false;
        }
        private void InitializeBG()
        {
            if (bgImg == null || bgSprites == null || bgSprites.Length < 2)
                return;

            bgImg.sprite = isWin ? bgSprites[0] : bgSprites[1];
        }
        private void InitializeTime()
        {
            if (battleTimeLabel == null)
                return;

            battleTimeLabel.text = SBFunc.TimeString(Data.Time);
        }
        private void InitializeLevelLayer()
        {
            if (winLayerNode == null || loseLayerNode == null)
                return;

            winLayerNode.SetActive(isWin);
            loseLayerNode.SetActive(!isWin);
        }
        private void InitializeAnimation()
        {
            if (winAnim == null || loseAnim == null)
                return;

            winAnim.SetActive(isWin);
            loseAnim.SetActive(!isWin);
            StartCoroutine(nameof(AnimationCoroutine));
        }
        private IEnumerator AnimationCoroutine()
        {
            resultAnimator.SetBool("win", isWin);

            yield return SBDefine.GetWaitForSeconds(fontStartDelay);
            if (levelUpIndexList.Count > 0)
            {
                SoundManager.Instance.PlaySFX("FX_LEVELUP_DRAGON1");
                foreach (int index in levelUpIndexList)
                {
                    arrLvBG[index].SetActive(true);
                    expLevelAnimators[index].SetBool("levelUpOn", true);
                }
            }

            yield return ItemAnimation();

            if (buttomGroup != null)
            {
                DOTween.To(() => buttomGroup.alpha, x => buttomGroup.alpha = x, 1, 0.5f).
                    OnComplete(() => { buttomGroup.interactable = true; }).Play();
            }

            resultAnimator.SetBool("layer1End", true);
        }
        void InitializeReward()
        {
            if (newItems == null)
                newItems = new List<GameObject>();
            else
                newItems.Clear();

            if (Data.AccExp > 0)
            {
                var newItem = Instantiate(itemAniPrefab, nodeRewardContent.transform);
                newItem.GetComponent<CanvasGroup>().alpha = 0;
                newItem.GetComponent<Animator>().enabled = false;
                newItems.Add(newItem);
                var type = (int)eGoodType.ITEM;
                var no = 10000003;
                var cnt = Data.AccExp;

                newItem.GetComponent<ItemFrame>().SetFrameItem(no, cnt, type);
            }

            if(Data.Rewards != null)
            {
                foreach (var reward in Data.Rewards)
                {
                    var newItem = Instantiate(itemAniPrefab, nodeRewardContent.transform);
                    newItem.GetComponent<CanvasGroup>().alpha = 0;
                    newItem.GetComponent<Animator>().enabled = false;
                    newItems.Add(newItem);
                    var type = reward.GoodType;
                    var no = reward.ItemNo;
                    var cnt = reward.Amount;

                    newItem.GetComponent<ItemFrame>().SetFrameItem(no, cnt, (int)type);
                }
            }

            rewardCount = newItems.Count;
        }

        private void InitializeBtn()
        {
            if (StatisticBtn != null)
                StatisticBtn.SetActive(Data.Time > 0);

            int availableCount = StageManager.Instance.DailyDungeonProgressData.DailyDungeonTicketCount;
            int allCount = freeCount + adCount;
            retryBtn.SetButtonSpriteState(true);
            if( availableCount < freeCount)
            {
                retryFreeLayer.SetActive(true);
                retryAdvLayer.SetActive(false);
                freeEnterCntText.text = string.Format("{0}/{1}",freeCount - availableCount,freeCount);
                isAdvWaitEnd = true;
            }
            else if( availableCount < allCount)
            {
                retryFreeLayer.SetActive(false);
                retryAdvLayer.SetActive(true);
                AdvEnterCntText.text = string.Format("{0}/{1}", allCount- availableCount, adCount);
                SetTimeObj();
                retryBtn.SetButtonSpriteState(isAdvWaitEnd);
                retryBtn.interactable = isAdvWaitEnd;
            }
            else
            {
                retryBtn.SetButtonSpriteState(false);
                retryFreeLayer.SetActive(false);
                retryAdvLayer.SetActive(true);
                AdvEnterCntText.text = string.Format("<color=red>{0}</color>/{1}", allCount - availableCount, adCount);
                advTimerText.text = string.Empty;
            }


            bool isEnterable = isWin && isAdvWaitEnd;
            int next = Data.Stage + 1;
            if (StageManager.Instance.DailyDungeonProgressData.GetLastestStage(Data.World) < next)
                isEnterable = false;
            nextStageBtn.SetButtonSpriteState(isEnterable);
            nextStageBtn.interactable = isEnterable;
        }

        private void SetTimeObj()
        {
            timeObj = advTimerText.GetComponent<TimeObject>();
            if (timeObj == null)
                timeObj = advTimerText.gameObject.AddComponent<TimeEnable>();

            var lastestDate = ShopManager.Instance.GetAdvertiseState(SBDefine.AD_DAILY_KEY).LAST_VIEWDATE;
            var term = AdvertisementData.Get(SBDefine.AD_DAILY_KEY).TERM;
            int advWaitEndTime = TimeManager.GetTimeStamp(lastestDate) + term;
            if (TimeManager.GetTimeCompare(advWaitEndTime)>0)
            {
                isAdvWaitEnd = false;
                timeObj.Refresh = () =>
                {
                    advTimerText.text = TimeManager.GetTimeCompareString(advWaitEndTime);
                    if (TimeManager.GetTimeCompare(advWaitEndTime) <= 0)
                    {
                        advTimerText.text = string.Empty;
                        timeObj.Refresh = null;
                        isAdvWaitEnd = true;
                        InitializeBtn();
                    }
                };
                return;
            }
            isAdvWaitEnd = true;
            timeObj.Refresh = null;
            advTimerText.text = string.Empty;
        }
        private IEnumerator ItemAnimation()
        {
            if (newItems != null || newItems.Count > 0)
            {
                rewardScroll.horizontalNormalizedPosition = 0;
                float delay = (float)(0.7 / (float)rewardCount);
                delay = Mathf.Min(delay, 0.2f);
                delay = Mathf.Max(delay, 0.07f);
                for (int i = 0; i < newItems.Count; ++i)
                {
                    itemsTime += delay;
                    float maxTime = delay * rewardCount;
                    itemsTime = Mathf.Min(itemsTime, maxTime);
                    newItems[i].GetComponent<CanvasGroup>().alpha = 1;
                    newItems[i].GetComponent<Animator>().enabled = true;
                    rewardScroll.DOHorizontalNormalizedPos(i / (float)rewardCount, delay * 0.75f);
                    yield return SBDefine.GetWaitForSeconds(delay);
                }
            }
            rewardScroll.DOHorizontalNormalizedPos(1, 0.07f);
            yield break;
        }
        private void InitializeDragons()
        {
            if (arrDragonNode == null || arrLvNow == null || arrDragonNode.Length != arrLvNow.Length)
                return;

            if (levelUpIndexList == null)
                levelUpIndexList = new();

            for (int i = 0, count = arrDragonNode.Length; i < count; ++i)
            {
                arrDragonNode[i].SetActive(false);
            }

            if (Data.OffenseDic != null)
            {
                int charExp = 0;
                var curStageData = StageBaseData.GetByWorldStage(Data.World, Data.Stage);
                if (curStageData != null)
                        charExp = curStageData.REWARD_CHAR_EXP;

                int charCount = 0;

                foreach (var it in Data.OffenseDic)
                {
                    var bTag = it.Key;
                    var elem = it.Value;
                    if (elem == null)
                        continue;

                    var tag = it.Value.ID;
                    if (it.Value.BaseData is CharBaseData data)
                    {
                        arrDragonNode[charCount].SetActive(true);
                        GameObject dragonClone = Instantiate(data.GetUIPrefab(), arrDragonNode[charCount].transform);
                        dragonClone.transform.SetAsFirstSibling();
                        UIDragonSpine spine = dragonClone.GetComponent<UIDragonSpine>();
                        if (spine == null)
                            spine = dragonClone.AddComponent<UIDragonSpine>();

                        var myDragonData = User.Instance.DragonData.GetDragon(tag);
                        spine.SetData(myDragonData);

                        if (elem.Death)
                            spine.SetAnimation(eSpineAnimation.LOSE);
                        else
                            spine.SetAnimation(isWin ? eSpineAnimation.WIN : eSpineAnimation.LOSE);

                        spine.RandomAnimationFrame();

                        dragonClone.transform.localScale = new Vector3(-2, 2, 1);
                        int dragonLv = myDragonData.Level;
                        bool isMaxLevel = GameConfigTable.GetDragonLevelMax() == dragonLv;
                        bool isLvUp = false;
                        arrLvNow[charCount].text = string.Format("Lv. {0}", dragonLv);
                        if (Data.LevelUpList != null && Data.LevelUpList.Contains(tag))
                        {
                            levelUpIndexList.Add(charCount);
                            isLvUp = true;
                        }

                        arrExpLabel[charCount].text = isMaxLevel ? "LEVEL MAX" : string.Format("EXP +{0}", charExp);
                        if (isWin)
                        {
                            arrExpSlider[charCount].gameObject.SetActive(true);
                            if (isMaxLevel)
                            {
                                arrExpSlider[charCount].value = 1;
                            }
                            else
                            {
                                Sequence sequence = DOTween.Sequence();
                                sequence.SetDelay(2f);

                                var nextExp = CharExpData.GetCurrentRequireLevelExp(User.Instance.DragonData.GetDragon(tag).Grade(), dragonLv);
                                var curLvTotalExp = CharExpData.GetCurrentAccumulateGradeAndLevelExp(User.Instance.DragonData.GetDragon(tag).Grade(), dragonLv);
                                var exp = User.Instance.DragonData.GetDragon(tag).Exp - curLvTotalExp;
                                if (isLvUp)
                                {
                                    var beforeLvExp = CharExpData.GetCurrentRequireLevelExp(User.Instance.DragonData.GetDragon(tag).Grade(), dragonLv - 1);
                                    var beforeLvTotalExp = CharExpData.GetCurrentAccumulateGradeAndLevelExp(User.Instance.DragonData.GetDragon(tag).Grade(), dragonLv - 1);
                                    arrExpSlider[charCount].value = (User.Instance.DragonData.GetDragon(tag).Exp - beforeLvTotalExp - charExp) / (float)beforeLvExp;
                                    sequence.Append(arrExpSlider[charCount].DOValue(1f, 1f).SetEase(Ease.InQuart));
                                    sequence.Append(arrExpSlider[charCount].DOValue(0f, 0f));

                                }
                                else
                                {
                                    arrExpSlider[charCount].value = (exp - charExp) / (float)nextExp;
                                }
                                sequence.Append(arrExpSlider[charCount].DOValue(exp / (float)nextExp, 1f).SetEase(Ease.InOutCubic));
                            }
                        }
                        else
                        {
                            arrExpSlider[charCount].gameObject.SetActive(false);
                        }

                        if (arrDragonNode[charCount] != null)
                        {
                            Image img = arrDragonNode[charCount].GetComponent<Image>();
                            if (img != null)
                                img.color = isWin ? winnerLvColor : failureLvColor;
                        }
                        ++charCount;
                    }
                }
            }
        }

        #region ButtonEvent
        private bool isClick = false;
        public void OnClickToDungeonSelect()
        {
            if (isClick)
                return;

            isClick = true;
            LoadingManager.Instance.EffectiveSceneLoad("DailyDungeonLobby", eSceneEffectType.CloudAnimation);
        }
        public void OnClickToVillage()
        {
            if (isClick)
                return;

            isClick = true;
            LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Town));
        }
        public void OnClickRetry()
        {
            if (isClick)
                return;

            isClick = true;


            if (User.Instance.CheckInventoryForContentsEnter(eInvenSlotCheckContentType.DailyDungeon, Data.World, Data.Stage) == false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                    () => {
                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
                    },
                    () => {
                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
                    },
                    () => {
                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
                    }
                );
                isClick = false;
                return;
            }

            var allCount = freeCount + adCount;
            int availableCount = StageManager.Instance.DailyDungeonProgressData.DailyDungeonTicketCount;
            
            if(availableCount< freeCount)
            {
                BattleStart();
            }
            else if (availableCount< allCount)
            {
                AdvertiseManager.Instance.TryADWithPopup((log) => BattleStart(true, log), ()=>
                {
                    ToastManager.On(StringData.GetStringByStrKey("광고실패"));
                });
                isClick = false;
            }
            else
            {
                ToastManager.On(100002103);
                isClick = false;
            }
        }

        private void BattleStart(bool isAd = false, string log = "")
        {
            BattleLine battleLine = User.Instance.PrefData.DailyBattleLine;
            WWWForm param = new WWWForm();
            param.AddField("world", Data.World);
            param.AddField("stage", Data.Stage);
            param.AddField("diff", 1);
            param.AddField("deck", battleLine.GetJsonString());            
            param.AddField("is_ad", isAd ? 1 : 0);
            param.AddField("ad_log", log);
            param.AddField("deckinf", battleLine.GetJsonStringINF());
            NetworkManager.Send("daily/dailystart", param, (JObject jsonData) =>
            {
                if (jsonData["rs"] != null)
                {
                    switch ((eApiResCode)(int)jsonData["rs"])
                    {
                        case eApiResCode.OK:
                            DailyManager.Instance.SetStartData(Data.World, Data.Stage, jsonData);
                            if (!DailyManager.Instance.IsStartCheck())
                                return;

                            LoadingManager.Instance.EffectiveSceneLoad("DailyBattle", eSceneEffectType.CloudAnimation, DailyManager.Instance.LoadingCoroutine);
                            break;
                        case eApiResCode.DAILY_DAY_NOT_MATCHED:
                            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("요일던전 초기화 문구"),
                            ()=>LoadingManager.Instance.EffectiveSceneLoad("DailyDungeonLobby", eSceneEffectType.CloudAnimation),
                            null,
                            ()=>LoadingManager.Instance.EffectiveSceneLoad("DailyDungeonLobby", eSceneEffectType.CloudAnimation)
                        , true, false, true);

                            break;
                        case eApiResCode.ADV_INVALID_DRAGON:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000635), true, false, false);
                            break;
                    }
                }
                isClick = false;
            }, (jsonData) =>
            {
                isClick = false;
            });
        }

        public void OnClickNextStage()
        {
            if (isClick)
                return;

            isClick = true;
            var curWorld = Data.World;
            var curStage = Data.Stage;

            if (User.Instance.CheckInventoryForContentsEnter(eInvenSlotCheckContentType.Adventure, curWorld, curStage + 1) == false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                    () => {
                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
                    },
                    () => {
                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
                    },
                    () => {
                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
                    }
                );
                return;
            }
            PopupManager.OpenPopup<DailyReadyPopup>(new DailyReadyData(DailyManager.Instance.GetDaily(), curWorld, curStage + 1, 1));
            isClick = false;

            //BattleLine battleLine = UserDragonData.BattleLine;
            //WWWForm param = new WWWForm();
            //param.AddField("world", curWorld);
            //param.AddField("stage", curStage + 1);
            //param.AddField("diff", 1);
            //param.AddField("deck", battleLine.getFormationToNetworkFormat());
            //param.AddField("deckinf", battleLine.GetInfToNetworkFormat());
            //NetworkManager.Send("daily/dailystart", param, (JObject jsonData) =>
            //{
            //    if (jsonData["rs"] != null)
            //    {
            //        switch ((eApiResCode)(int)jsonData["rs"])
            //        {
            //            case eApiResCode.OK:
            //                UserDragonData.SetBattleLine(battleLine);
            //                DailyManager.Instance.SetStartData(curWorld, curStage + 1, jsonData);
            //                User.Instance.SetPrevInvenSlotCount(eInvenSlotCheckContentType.Adventure, -1);
            //                if (!DailyManager.Instance.IsStartCheck())
            //                    return;
            //                LoadingManager.Instance.EffectiveSceneLoad("DailyBattle", eSceneEffectType.CloudAnimation, AdventureManager.Instance.LoadingCoroutine);
            //                break;
            //            case eApiResCode.DAILY_DAY_NOT_MATCHED:
            //                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000618), StringData.GetStringByIndex(100002158),
            //                    () => {
            //                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation);
            //                    },
            //                    null,
            //                    () => {
            //                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation);
            //                    }
            //                );
            //                break;
            //            case eApiResCode.ADV_INVALID_DRAGON:
            //                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000635), true, false, false);
            //                break;
            //        }
            //    }
            //});
        }
        #endregion
        public void OnClickStatisticInfo()
        {
            PopupManager.OpenPopup<AdventureStatisticsPopup>(new AdventureStatisticPopupData(false, Data.Time, ""));
        }
    }
}