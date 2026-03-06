using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class WorldBossResult : MonoBehaviour
    {
        private WorldBossBattleData Data { get; set; } = null;
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
        [Header("TotalDamageText")]
        [SerializeField]
        private Text battlePointLabel = null;
        [SerializeField]
        private Text bossLevelLabel = null;

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
        [SerializeField]
        private GameObject hasnotRewardPanel = null;

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

        [Space(10)]
        [SerializeField]
        private GameObject StatisticBtn = null;

        [Space(10)]
        [SerializeField]
        private List<WorldBossResultDeckSlotInfo> deckStatisticInfoList = new List<WorldBossResultDeckSlotInfo>();

        private bool isWin = false;
        private bool isAdvWaitEnd = true;
        private bool isRequest = false;
        private List<int> levelUpIndexList = null;
        private int rewardCount = 0;
        private float itemsTime = 0;
        private List<GameObject> newItems = null;
        private int freeEnterCnt { get { return WorldBossManager.Instance.WorldBossEnterCount; } }
        private int advEnterCount { get { return AdvertisementData.Get(SBDefine.AD_RAID_BOSS_KEY).LIMIT; } }
        private int curTicketCnt { get { return WorldBossManager.Instance.WorldBossProgressData.WorldBossPlayCount; } }

        private TimeObject timeObj = null;

        [SerializeField]
        private Transform[] dragonTransform = null;
        [SerializeField]
        private GameObject StatisticUI = null;
        [SerializeField]
        private RectTransform resultLabelNodeRect = null;

        void Start()
        {
            InitializeData();
            if (Data == null)
                return;

            UIManager.Instance.InitUI(eUIType.None);

            InitializeBG();
            InitializeTime();
            InitializeLevelLayer();
            //InitializeDragons();
            InitializeReward();
            InitializeBtn();
            InitializeAnimation();
            RefreshDeck();
            RefreshLog();
        }
        private void InitializeData()
        {
            Data = WorldBossManager.Instance.Data;
            if (Data == null)
                return;

            if (Data.State != eBattleState.Abort)
                isWin = true;
            else
                isWin = false;

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

            battleTimeLabel.text = SBFunc.TimeString(Mathf.Min(Data.Time + 1.0f, 60 * 3));
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

            //yield return SBDefine.GetWaitForSeconds(fontStartDelay);
            //if (levelUpIndexList.Count > 0)
            //{
            //    SoundManager.Instance.PlaySFX("FX_LEVELUP_DRAGON1");
            //    foreach (int index in levelUpIndexList)
            //    {
            //        arrLvBG[index].SetActive(true);
            //        expLevelAnimators[index].SetBool("levelUpOn", true);
            //    }
            //}

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

            hasnotRewardPanel.SetActive(true);
            if (Data.AccExp > 0)
            {
                hasnotRewardPanel.SetActive(false);
                var newItem = Instantiate(itemAniPrefab, nodeRewardContent.transform);
                newItem.GetComponent<CanvasGroup>().alpha = 0;
                newItem.GetComponent<Animator>().enabled = false;
                newItems.Add(newItem);
                var type = (int)eGoodType.ITEM;
                var no = 10000003;
                var cnt = Data.AccExp;

                newItem.GetComponent<ItemFrame>().SetFrameItem(no, cnt, type);
            }

            if (Data.Rewards != null)
            {
                foreach (var reward in Data.Rewards)
                {
                    hasnotRewardPanel.SetActive(false);
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
            int allCount = freeEnterCnt + advEnterCount;
            retryBtn.SetButtonSpriteState(true);
            if (curTicketCnt < freeEnterCnt)
            {
                retryFreeLayer.SetActive(true);
                retryAdvLayer.SetActive(false);
                freeEnterCntText.text = string.Format("{0}/{1}", freeEnterCnt - curTicketCnt, freeEnterCnt);
                isAdvWaitEnd = true;
            }
            else if (curTicketCnt < allCount)
            {
                retryFreeLayer.SetActive(false);
                retryAdvLayer.SetActive(true);
                AdvEnterCntText.text = string.Format("{0}/{1}", allCount - curTicketCnt, advEnterCount);
                SetTimeObj();
                retryBtn.SetButtonSpriteState(isAdvWaitEnd);
                retryBtn.interactable = isAdvWaitEnd;
            }
            else
            {
                retryBtn.SetButtonSpriteState(false);
                retryFreeLayer.SetActive(false);
                retryAdvLayer.SetActive(true);
                AdvEnterCntText.text = string.Format("<color=red>{0}</color>/{1}", allCount - curTicketCnt, advEnterCount);
                advTimerText.text = string.Empty;
            }
        }

        private void SetTimeObj()
        {
            timeObj = advTimerText.GetComponent<TimeObject>();
            if (timeObj == null)
                timeObj = advTimerText.gameObject.AddComponent<TimeEnable>();

            var lastestDate = ShopManager.Instance.GetAdvertiseState(SBDefine.AD_RAID_BOSS_KEY).LAST_VIEWDATE;
            var term = AdvertisementData.Get(SBDefine.AD_RAID_BOSS_KEY).TERM;
            int advWaitEndTime = TimeManager.GetTimeStamp(lastestDate) + term;
            if (TimeManager.GetTimeCompare(advWaitEndTime) > 0)
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
            var dic = StatisticsMananger.Instance.myDamageDic;

            List<StatisticsInfo> statisticsRank = StatisticsMananger.Instance.myDamageDic.Values.ToList();
            statisticsRank.Sort((a, b) => {
                return (b.NormalAtkTotalDmg + b.SkillTotalDmg + b.RecieveDmgInShield + b.RecieveDmgReal).CompareTo(a.NormalAtkTotalDmg + a.SkillTotalDmg + a.RecieveDmgInShield + a.RecieveDmgReal);
            });

            if (Data.OffenseDic != null)
            {
                foreach (var it in Data.OffenseDic)
                {
                    var bTag = it.Key;
                    var elem = it.Value;
                    if (elem == null)
                        continue;
                    var index = statisticsRank.FindIndex((info) => { return info.ID == elem.ID; });
                    StartCoroutine(LoadDragon(bTag, elem, index));
                }
            }
        }

        private IEnumerator LoadDragon(int bTag, IBattleCharacterData elem, int index)
        {
            var tag = elem.ID;
            if (elem.BaseData is CharBaseData data)
            {
                var pt = bTag - 1;
                var attachTarget = dragonTransform[pt / WorldBossFormationData.MAX_DRAGON_COUNT].transform.GetChild(pt % WorldBossFormationData.MAX_DRAGON_COUNT);

                var dragonClone = Instantiate(data.GetUIPrefab(), attachTarget);
                UIDragonSpine spine = dragonClone.GetComponent<UIDragonSpine>();
                if (spine == null)
                    spine = dragonClone.AddComponent<UIDragonSpine>();

                dragonClone.transform.SetAsFirstSibling();

                dragonClone.transform.localScale = new Vector3(-2, 2, 1);

                if (spine != null)
                {
                    var myDragonData = User.Instance.DragonData.GetDragon(tag);
                    spine.SetData(myDragonData);

                    spine.SetAnimation(eSpineAnimation.WALK);
                    spine.transform.localPosition = new Vector3(1000, 0, 0);
                    spine.transform.DOLocalMoveX(0.0f, 1.0f);

                    yield return new WaitForSeconds(1.0f + (index * 0.1f));

                    if (!isWin)
                        spine.SetAnimation(eSpineAnimation.LOSE);
                    else
                        spine.SetAnimation(eSpineAnimation.WIN);

                    spine.RandomAnimationFrame();
                }
            }
        }

        void RefreshDeck()
        {
            if (deckStatisticInfoList == null || deckStatisticInfoList.Count <= 0)
                return;

            for(int i = 0; i< deckStatisticInfoList.Count; i++)
            {
                var deck = deckStatisticInfoList[i];
                if (deck != null)
                    deck.SetData(i);
            }

            if(battlePointLabel != null)
                battlePointLabel.text = SBFunc.CommaFromNumber(Data.BossData.SCORE);

            if (bossLevelLabel != null)
                bossLevelLabel.text = StringData.GetStringFormatByStrKey("user_info_lv_02", Data.BossData.Level);

            if (resultLabelNodeRect != null)
                RefreshContentFitter(resultLabelNodeRect);
        }

        //일단 클라에서 최고 점수 , 최고 레벨 갱신
        void RefreshLog()
        {
            WorldBossManager.Instance.WorldBossProgressData.UpdateLogData(Data.World, Data.BossData.SCORE, Data.BossData.Level);
        }
        private void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeSelf)
            {
                return;
            }

            foreach (RectTransform child in transform)
            {
                RefreshContentFitter(child);
            }

            var layoutGroup = transform.GetComponent<LayoutGroup>();
            var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();
            if (layoutGroup != null)
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            if (contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }
        #region ButtonEvent
        private bool isClick = false;
        public void OnClickToWorldBossLobby()
        {
            if (isClick)
                return;

            isClick = true;
            LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.BlackBackground);
        }
        public void OnClickToVillage()
        {
            if (isClick)
                return;

            isClick = true;
            LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.BlackBackground, UIManager.RefreshUICoroutine(eUIType.Town));
        }
        public void OnClickRetry()
        {
            WorldBossManager.Instance.WorldBossProgressData.IsToday(() => {
                RetryProcess();
            }
            ,() => { });
        }

        void RetryProcess()
        {
            if (isClick)
                return;

            isClick = true;
            //사전 획득 보상 체크로직 필요한지 알아봐야함.
            //if (User.Instance.CheckInventoryForContentsEnter(eInvenSlotCheckContentType.DailyDungeon, Data.World, Data.Stage) == false)
            //{
            //    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
            //        () => {
            //            LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
            //        },
            //        () => {
            //            LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
            //        },
            //        () => {
            //            LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
            //        }
            //    );
            //    isClick = false;
            //    return;
            //}

            var allCount = freeEnterCnt + advEnterCount;
            if (curTicketCnt < freeEnterCnt)
            {
                WorldBossManager.Instance.RequestBattleStart();
            }
            else if (curTicketCnt < allCount)
            {
                AdvertiseManager.Instance.TryADWithPopup((log) => WorldBossManager.Instance.RequestBattleStart(true, null, log), () =>
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

        #endregion
        public void OnClickStatisticInfo()
        {
            StatisticUI.SetActive(!StatisticUI.activeInHierarchy);
        }
    }
}
