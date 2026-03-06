using DG.Tweening;
using Google.Impl;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class AdventureBattleReward : MonoBehaviour
    {
        [SerializeField] Text labelBattleTime = null;
        [SerializeField] Button nodeBtnRetry = null;
        [SerializeField] Text labelReqEnergy = null;
        [SerializeField] ScrollRect rewardScroll = null;
        [SerializeField] GameObject nodeRewardContent = null;
        [SerializeField] GameObject nodeReward = null;
        [SerializeField] protected Button nodeBtnNext = null;
        [SerializeField] Image[] arrStarNode;
        [SerializeField] Sprite[] starSprite;
        [SerializeField] GameObject[] arrDragonNode;
        [SerializeField] GameObject[] arrLvBG;
        [SerializeField] Text[] arrLvNow;
        [SerializeField] Text[] arrExpLabel;
        [SerializeField] Slider[] arrExpSlider;
        [SerializeField] GameObject characterNode = null;
        [SerializeField] protected GameObject autoButtonNode = null;
        [SerializeField] protected Text autoButtonLabel = null;
        [SerializeField] protected Text autoCountCompLabel = null;
        [SerializeField] int defaultAutoCount = 3;
        [SerializeField] Animation[] starAnims;
        [SerializeField] Animator rewardIdleAnim;
        [SerializeField] Animator[] expLevelAnimators;
        [SerializeField] GameObject winAnimNode = null;
        [SerializeField] GameObject loseAnimNode = null;
        [SerializeField] Color failureLvColor;
        [SerializeField] Color winnerLvColor;
        [SerializeField] GameObject itemAniPrefab = null;
        [SerializeField] Image backImg = null;
        [SerializeField] Sprite[] backImgSprites = null;

        [SerializeField] Image Ribbon = null;
        [SerializeField] Sprite[] RibbonSprite = null;
        [SerializeField] private Text difficultText = null;
        [SerializeField] private Color[] textColors = null;
        [SerializeField] Image Victory = null;
        [SerializeField] Sprite[] VictorySprite = null;

        [SerializeField] GameObject winAnim =null;
        [SerializeField] GameObject[] fireworkObjs = null;
        [SerializeField] GameObject loseAnim = null; 

        [Header("anim Delay")]
        [SerializeField] float starStartDelay = 0.4f;
        [SerializeField] float starShowDelay = 0.5f;
        [SerializeField] float dragonShowDelay=0.3f;
        [SerializeField] float rewardShowDelay=0.3f;
        [SerializeField] float dragonExpDelay = 0.4f;
        [SerializeField] float dragonLevelUpDelay = 0.3f;

        [SerializeField] DOTweenAnimation animObj = null;
        [SerializeField] CanvasGroup buttomGroup = null;
        [SerializeField] Text stageNameText = null;
        [SerializeField] GameObject statisticBtn = null;

        int currentAutoCount = 0;
        Dictionary<int, Dictionary<int, Asset>> autoAdventureReward = new Dictionary<int, Dictionary<int, Asset>>();
        private List<GameObject> newItems;
        private int rewardCount = 0;
        private float itemsTime = 0;
        protected float AutoPlayingTimer = 3f;
        protected bool isAuto = false;
        AdventureBattleData stageData = null;
        private List<int> levelUpIndexList = new List<int>();

        protected bool isSufficientRetry = false;
        protected bool isStageClear = false;//별이 1개 이상인가
        protected bool isAvailableStage = false;//클리어 실패시 나의 전투기록에 다음 스테이지를 열었는가
        protected bool isItemAnim = false;//아이템연출 완료인가?
        protected bool isBtnAnim = false;//별 연출 완료인가?

        private bool isNetworkState = false;
        protected virtual void Start()
        {
            UIManager.Instance.InitUI(eUIType.None);
            stageData = AdventureManager.Instance.Data;
            isNetworkState = false;
            var curStageData = StageBaseData.GetByAdventureWorldStage(stageData.World, stageData.Stage);
            if (stageNameText != null)
                stageNameText.text = string.Format("{0} {1}-{2}", StringData.GetStringByIndex(WorldData.GetByWorldNumber(stageData.World)._NAME), stageData.World, stageData.Stage);

            var isWin = stageData.State == eBattleState.Win;
            SoundManager.Instance.PushBGM(isWin ? "BGM_BATTLE_VICTORY" : "BGM_BATTLE_DEFEAT", true, false);

            foreach(var star in arrStarNode)
            {
                switch(curStageData.DIFFICULT)
                {
                    case StageDifficult.HARD:
                        star.sprite = starSprite[2];
                        break;
                    case StageDifficult.HELL:
                        star.sprite = starSprite[3];
                        break;
                    case StageDifficult.NORMAL:
                    default:
                        star.sprite = starSprite[1];
                        break;
                }
            }

            int difficult = CacheUserData.GetInt("adventure_difficult", 1);
            int checkIndex = difficult - 1;

            if (backImg != null)
                backImg.sprite = isWin ? backImgSprites[difficult] : backImgSprites[0];
            if(Ribbon != null && RibbonSprite.Length > checkIndex)
                Ribbon.sprite = RibbonSprite[checkIndex];

            if (difficultText != null && textColors.Length > checkIndex)
            {
                difficultText.color = textColors[checkIndex];
                switch ((StageDifficult)difficult)
                {
                    case StageDifficult.HARD:
                        difficultText.text = StringData.GetStringByStrKey("어려움난이도");
                        break;
                    case StageDifficult.HELL:
                        difficultText.text = StringData.GetStringByStrKey("지옥난이도");
                        break;
                    case StageDifficult.NORMAL:
                    default:
                        difficultText.text = StringData.GetStringByStrKey("보통난이도");
                        break;
                }
            }

            if (Victory != null && VictorySprite.Length > checkIndex)
            {
                Victory.sprite = VictorySprite[checkIndex];
            }

            levelUpIndexList = new List<int>();
            labelBattleTime.text = SBFunc.TimeStringMinute(stageData.Time);
            labelReqEnergy.text = string.Format("-{0}", curStageData.COST_VALUE);
            autoAdventureReward = new Dictionary<int, Dictionary<int, Asset>>();
            int stars = stageData.Star;
            for (int i = 0; i < arrDragonNode.Length; ++i)
            {
                arrDragonNode[i].SetActive(false);
                arrLvBG[i].SetActive(false);
                arrExpLabel[i].text = "";
            }
            if (winAnim != null)
                winAnim.SetActive(isWin);

            if (loseAnim != null)
                loseAnim.SetActive(!isWin);
            isStageClear = stars > 0;
            if (isStageClear)
            {
                nodeReward.SetActive(true);
            }
            else
            {
                if (IsAutoAdventured())//자동 사냥 실패 시 잔여 횟수 0으로 만듬
                {
                    ReleaseAutoAdventure();
                }
                
                isAvailableStage = IsOriginStageClear();//현재 스테이지가 과거에 클리어 했으면 활성화

                if (nodeBtnNext != null)
                {
                    nodeBtnNext.SetButtonSpriteState(isAvailableStage);
                }
            }

            if (winAnimNode != null) 
                winAnimNode.SetActive(isWin);
            if (loseAnimNode != null) 
                loseAnimNode.SetActive(!isWin);

            if (TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Adventure))
                TutorialManager.tutorialManagement.NextTutorialStart();

            if (isWin)
            {
                RewardSetting();
                StartCoroutine(WinAnimCoroutine(stars));
                StartCoroutine(FireWorkCor());
            }
            else
            {
                StartCoroutine(LoseAnimCoroutine());
            }

            if (IsLastStage())
            {
                if (nodeBtnNext != null)
                {
                    nodeBtnNext.SetButtonSpriteState(false);
                }
            }
            InitAutoButton(stars > 0);


            isSufficientRetry = User.Instance.ENERGY >= curStageData.COST_VALUE;

            if (nodeBtnRetry != null)
            {
                nodeBtnRetry.SetButtonSpriteState(isSufficientRetry);
                SetBubbleNodeEffect(isSufficientRetry);
            }
            StartCoroutine(nameof(ShowBtn));

            if (statisticBtn != null)
                statisticBtn.SetActive(stageData.Time > 0);
        }

        protected virtual void OnDestroy()
        {
            StopCoroutine(nameof(WinAnimCoroutine));
            StopCoroutine(nameof(FireWorkCor));
            StopCoroutine(nameof(LoseAnimCoroutine));
            StopCoroutine(nameof(ItemAnimation));
            StopCoroutine(nameof(AutoScheduler));
            StopCoroutine(nameof(ShowBtn));
        }

        void DragonShow()
        {
            var charData = stageData.Characters;
            if(charData != null) { 
                int charExp = 0;
                var curStageData = StageBaseData.GetByAdventureWorldStage(stageData.World, stageData.Stage);
                if (stageData != null && curStageData != null)
                {
                    charExp = curStageData.REWARD_CHAR_EXP;
                }
                int charCount = 0;
                var isWin = stageData.State == eBattleState.Win; 

                
                foreach (var elem in charData)
                {
                    int tag = elem.ID;
                    CharBaseData data = CharBaseData.Get(tag.ToString());
                    if (data != null)
                    {
                        GameObject dragonClone = Instantiate(data.GetUIPrefab(), arrDragonNode[charCount].transform);
                        dragonClone.transform.SetAsFirstSibling();
                        UIDragonSpine spine = dragonClone.GetComponent<UIDragonSpine>();
                        if (spine == null)
                            spine = dragonClone.AddComponent<UIDragonSpine>();

                        var myDragonData = User.Instance.DragonData.GetDragon(tag);
                        spine.SetData(myDragonData);
                        spine.InitComplete = SpineInitCallback;

                        if (elem.Death)
                            spine.SetAnimation(eSpineAnimation.LOSE);
                        else
                            spine.SetAnimation(isWin ? eSpineAnimation.WIN : eSpineAnimation.LOSE);

                        spine.RandomAnimationFrame();

                        dragonClone.transform.localScale = new Vector3(-2, 2, 1);
                        int dragonLv = myDragonData.Level;
                        bool isMaxLevel = GameConfigTable.GetDragonLevelMax() == dragonLv;
                        bool isLvUp = false;
                        
                        if (stageData.LevelUpList != null && stageData.LevelUpList.Contains(tag))
                        {
                            levelUpIndexList.Add(charCount);
                            isLvUp = true;
                            arrLvNow[charCount].text = string.Format("Lv. {0}", dragonLv -1);
                        }
                        else
                        {
                            arrLvNow[charCount].text = string.Format("Lv. {0}", dragonLv);
                        }

                        arrDragonNode[charCount].SetActive(true);
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
                                
                                var nextExp = CharExpData.GetCurrentRequireLevelExp(User.Instance.DragonData.GetDragon(tag).Grade(),dragonLv);
                                var curLvTotalExp = CharExpData.GetCurrentAccumulateGradeAndLevelExp(User.Instance.DragonData.GetDragon(tag).Grade(), dragonLv);
                                var exp = User.Instance.DragonData.GetDragon(tag).Exp - curLvTotalExp;
                                if (isLvUp)
                                {
                                    var beforeLvExp = CharExpData.GetCurrentRequireLevelExp(User.Instance.DragonData.GetDragon(tag).Grade(), dragonLv-1);
                                    var beforeLvTotalExp = CharExpData.GetCurrentAccumulateGradeAndLevelExp(User.Instance.DragonData.GetDragon(tag).Grade(), dragonLv-1);
                                    arrExpSlider[charCount].value = (User.Instance.DragonData.GetDragon(tag).Exp- beforeLvTotalExp - charExp) / (float)beforeLvExp;
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

        void SpineInitCallback(UIDragonSpine spineData)
        {
            spineData.SetShadow(true);
        }

        void RewardSetting()
        {
            newItems = new List<GameObject>();
            if(stageData.StarRewards != null) 
            {
                var starRewards = stageData.StarRewards;

                foreach(var reward in starRewards)
                {
                    foreach (var rew in reward.Value)
                    {
                        var newItem = Instantiate(itemAniPrefab, nodeRewardContent.transform);
                        newItem.GetComponent<CanvasGroup>().alpha = 0;
                        newItem.GetComponent<Animator>().enabled = false;
                        newItems.Add(newItem);

                        switch (rew.GoodType)
                        {
                            case eGoodType.GOLD:
                            case eGoodType.GEMSTONE:
                            case eGoodType.ARENA_TICKET:
                                newItem.GetComponent<ItemFrame>().setFrameCashInfo((int)rew.GoodType, rew.Amount);
                                break;
                            case eGoodType.ENERGY:
                                newItem.GetComponent<ItemFrame>().setFrameEnergyInfo(rew.Amount);
                                break;
                            case eGoodType.ITEM:
                                newItem.GetComponent<ItemFrame>().SetFrameItemInfo(rew.ItemNo, rew.Amount);
                                break;
                            case eGoodType.CHARACTER:
                                newItem.GetComponent<ItemFrame>().SetFrameItem(rew.ItemNo, rew.Amount, (int)rew.GoodType);
                                break;
                        }

                        newItem.GetComponent<ItemFrame>().setRewardStar(reward.Key);
                    }
                }
            }

            if (stageData.AccExp > 0)
            {
                var newItem = Instantiate(itemAniPrefab, nodeRewardContent.transform);
                newItem.GetComponent<CanvasGroup>().alpha = 0;
                newItem.GetComponent<Animator>().enabled = false;
                newItems.Add(newItem);
                var type = (int)eGoodType.ITEM;
                var no = 10000003;
                var cnt = stageData.AccExp;

                newItem.GetComponent<ItemFrame>().SetFrameItem(no, cnt, type);
                if (IsAutoAdventured()) SetTotalAutoAdventureReward(type, no, cnt);
            }

            foreach (var reward in stageData.Rewards)
            {
                var newItem = Instantiate(itemAniPrefab, nodeRewardContent.transform);
                newItem.GetComponent<CanvasGroup>().alpha = 0;
                newItem.GetComponent<Animator>().enabled = false;
                newItems.Add(newItem);
                var type = reward.GoodType;
                var no = reward.ItemNo;
                var cnt = reward.Amount;
                
                newItem.GetComponent<ItemFrame>().SetFrameItem(no, cnt, (int)type);
                if (IsAutoAdventured()) SetTotalAutoAdventureReward((int)type, no, cnt);
            }

            if (IsAutoAdventured())
            {
                SaveTotalAutoAdventureRewardData();
            }
            rewardCount = newItems.Count;
           
        }

        IEnumerator FireWorkCor()
        {
            foreach (var fire in fireworkObjs)
            {
                fire.SetActive(false);
                yield return SBDefine.GetWaitForSeconds(SBFunc.Random(0.5f, 1.5f));
                fire.SetActive(true);
            }
        }
        IEnumerator WinAnimCoroutine(int starCount)
        {
            var isWin = stageData.State == eBattleState.Win;
            DragonShow();
            yield return SBDefine.GetWaitForSeconds(starStartDelay);
            for (int i =0; i < starCount; ++i)
            {
                starAnims[i].Play();
                yield return SBDefine.GetWaitForSeconds(starShowDelay);
            }

            if (levelUpIndexList.Count > 0)
            {
                SoundManager.Instance.PlaySFX("FX_LEVELUP_DRAGON1");
                yield return SBDefine.GetWaitForSeconds(dragonLevelUpDelay);
                foreach (int index in levelUpIndexList)
                {
                    arrLvBG[index].SetActive(true);
                    expLevelAnimators[index].SetBool("levelUpOn", true);
                }
            }
            yield return SBDefine.GetWaitForSeconds(dragonExpDelay);
            foreach (Animator anim in expLevelAnimators)
            {
                anim.SetBool("expOn", true);
            }
            
            yield return SBDefine.GetWaitForSeconds(rewardShowDelay);
            for (int index = 0; index < levelUpIndexList.Count; ++index)
            {
                int lv = User.Instance.DragonData.GetDragon(stageData.LevelUpList[index]).Level;
                arrLvNow[levelUpIndexList[index]].text = string.Format("Lv. {0}", lv);
            }

            if (characterNode != null)
            {
                var anim = characterNode.GetComponent<Animation>();
                if (anim != null)
                {
                    anim.Play();
                }
            }

            //리본 및 별 Idle 재생시작시키기
            if (rewardIdleAnim != null)
                rewardIdleAnim.enabled = true;
            //

            StartCoroutine(ItemAnimation());
        }
        IEnumerator LoseAnimCoroutine()
        {
            DragonShow();
            yield return SBDefine.GetWaitForSeconds(starStartDelay);

            //리본 및 별 Idle 재생시작시키기
            if (rewardIdleAnim != null)
                rewardIdleAnim.enabled = true;
            //

            yield return SBDefine.GetWaitForSeconds(dragonShowDelay);
            isItemAnim = true;

        }

        bool IsOriginStageClear()//이전에 클리어한 상태인가
        {
            return StageManager.Instance.AdventureProgressData.IsClearedStage(stageData.World, stageData.Stage, stageData.Diff);
        }

        bool IsLastStage()
        {
            int isLastStage = StageTable.GetWorldStageCount(stageData.World, (StageDifficult)CacheUserData.GetInt("adventure_difficult", 1));
            return isLastStage == stageData.Stage;
        }

        void InitAutoButton(bool isSuccess)
        {
            currentAutoCount = defaultAutoCount;

            if(IsAutoAdventured())
            {
                ShowAutoButton();
                StartCoroutine(AutoScheduler());
            }
            else
            {
                HideAutoButton();
                ShowTotalRewardPopup();

                if (!isSuccess && StageManager.AccumRewards.Count > 0)
                {
                    ShowAdventureFailPopup();
                }

                StageManager.ClearAccumData();
            }
        }
        void ShowAutoButton()
        {
            if (autoButtonNode.activeInHierarchy == false)
            {
                autoButtonNode.SetActive(true);
            }

            var autoCount = StageManager.AccumCount;
            var totalCount = StageManager.AccumTotalCount;
            autoCountCompLabel.text = string.Format("{0}/{1}", totalCount - autoCount, totalCount);
        }

        IEnumerator ItemAnimation()
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
                    rewardScroll.DOHorizontalNormalizedPos(i/ (float)rewardCount, delay*0.75f);
                    yield return SBDefine.GetWaitForSeconds(delay);
                }
            }
            rewardScroll.DOHorizontalNormalizedPos(1,0.07f);
            isItemAnim = true;


            yield break;
        }
        IEnumerator ShowBtn()
        {
            var wait = SBDefine.GetWaitForSeconds(0.1f);
            while (!isItemAnim)
                yield return wait;

            //DOTween.
            if (buttomGroup != null)
                DOTween.To(() => buttomGroup.alpha, x => buttomGroup.alpha = x, 1, 0.5f).
                    OnComplete(() => { buttomGroup.interactable = true; }).Play();

            yield break;
        }

        public void OnClickToWorld()
        {
            if (IsAutoAdventured())
            {
                OpenAutoAdventurePopup();
                return;
            }
            StageManager.Instance.SetWorld(stageData.World);
            LoadingManager.Instance.EffectiveSceneLoad("AdventureStageSelect", eSceneEffectType.CloudAnimation, 
                SBFunc.CallBackCoroutine(() => characterDestory()));
        }
        public void OnClickToVillage()
        {
            if (IsAutoAdventured())
            {
                OpenAutoAdventurePopup();
                return;
            }
            LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation,
                SBFunc.CallBackCoroutine(() => characterDestory()), UIManager.RefreshUICoroutine(eUIType.Town));
        }

        void characterDestory()
        {
            if (arrDragonNode != null)
            {
                foreach (GameObject dragonNode in arrDragonNode)
                {
                    if (arrDragonNode == null || dragonNode == null) continue;
                    SBFunc.RemoveAllChildrens(dragonNode.transform);
                }
            }
        }

        bool IsAutoAdventured()
        {
            return StageManager.AccumCount > 0;
        }
        void MinusAutoCount()
        {
            StageManager.StageCompleteAccum();
        }

        void SaveTotalAutoAdventureRewardData()
        {
            foreach (var elem in autoAdventureReward)
            {
                if (!StageManager.AccumRewards.ContainsKey(elem.Key))
                {
                    StageManager.AccumRewards.Add(elem.Key, new Dictionary<int, Asset>());
                }

                foreach (var elem2 in elem.Value)
                {
                    if (StageManager.AccumRewards[elem.Key].ContainsKey(elem2.Key))
                    {
                        StageManager.AccumRewards[elem.Key][elem2.Key].AddCount(elem2.Value.Amount);
                    }
                    else
                        StageManager.AccumRewards[elem.Key].Add(elem2.Key, new Asset(elem2.Value.ItemNo, elem2.Value.Amount, (int)elem2.Value.GoodType));
                }
            }
        }

        void ShowTotalRewardPopup()
        {
            if (StageManager.AccumRewards.Count <= 0)
            {
                StageManager.AccumRewards.Clear();
                return;
            }

            List<Asset> list = new List<Asset>();
            foreach(var elem in StageManager.AccumRewards.Values)
            {
                list.AddRange(elem.Values.ToList());
            }

            SystemRewardPopup.OpenPopup(list);
            StageManager.AccumRewards.Clear();
        }
        
        public virtual void OpenAutoAdventurePopup()//자동 전투 중 끊을때
        {
            isAuto = false;
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000781),
                () => {
                    HideAutoButton();
                    ShowTotalRewardPopup();
                    StageManager.ClearAccumData();
                },
                () => {
                    StartCoroutine(AutoScheduler());//카운터 코루틴 재시작
                },
                () => {
                    StartCoroutine(AutoScheduler());//카운터 코루틴 재시작
                }
            );
        }

        public void OnClickRetry()
        {
            if (IsAutoAdventured())
            {
                OpenAutoAdventurePopup();
                return;
            }

            if (!isSufficientRetry)
            {
                ToastManager.On(100000134);
                return;
            }

            if (User.Instance.CheckInventoryForContentsEnter(eInvenSlotCheckContentType.Adventure, stageData.World, stageData.Stage) == false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                    () => {
                        HideAutoButton();
                        StageManager.ClearAccumData();
                        StageManager.AccumRewards.Clear();
                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
                        //씬 이동 후 가방 바로 열기 
                    },
                    () => {
                        HideAutoButton();
                        ShowTotalRewardPopup();
                        StageManager.ClearAccumData();
                    },
                    () => {
                        HideAutoButton();
                        ShowTotalRewardPopup();
                        StageManager.ClearAccumData();
                    }
                );
                return;
            }

            var battleLine = User.Instance.PrefData.AdventureBattleLine;
            WWWForm param = new WWWForm();
            param.AddField("world", stageData.World);
            param.AddField("diff", stageData.Diff);
            param.AddField("stage", stageData.Stage);
            param.AddField("deck", battleLine.GetJsonString());
            param.AddField("deckinf", battleLine.GetJsonStringINF());

            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("adventure/start", param, (JObject jsonData)=>
            {
                isNetworkState=false;
                if (jsonData["rs"] != null)
                {
                    switch ((eApiResCode)(int)jsonData["rs"])
                    {
                        case eApiResCode.OK:
                            AdventureManager.Instance.SetStartData(stageData.World, stageData.Stage, jsonData);
                            if (AdventureManager.Instance.IsStartCheck())
                            {
                                //characterDestory();
                                LoadingManager.Instance.EffectiveSceneLoad("AdventureBattle", eSceneEffectType.CloudAnimation, 
                                    SBFunc.CallBackCoroutine(() => characterDestory()), AdventureManager.Instance.LoadingCoroutine);
                            }
                            break;
                        case eApiResCode.COST_SHORT:
                            var costType = StageBaseData.GetByAdventureWorldStage(stageData.World, stageData.Stage).COST_TYPE;
                            string needItemName = "";
                            switch (costType)
                            {
                                case  "ENERGY":
                                    needItemName = StringData.GetStringByStrKey("item_base:name:10000002");
                                    break;
                                case "DAILY_TICKET":
                                    needItemName = string.Format(StringData.GetStringByIndex(100002096),"");
                                    break;
                            }

                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), string.Format(StringData.GetStringByIndex(100000224), needItemName));
                            break;
                        case eApiResCode.ADV_INVALID_DRAGON:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000635), true, false, false);
                            break;
                    }
                }
                //실패 해서 마을로 돌리던 다른 무언가
            },
            (string arg) =>
            {
                isNetworkState = false;
            });
        }

        bool IsAvailableNextStage()
        {
            if (IsLastStage())
            {
                ToastManager.On(100002532);
                return false;
            }

            if(!isStageClear && !isAvailableStage)//스테이지 실패 시 && 다음 스테이지 클리어 기록 없음
            {
                ToastManager.On(100002533);
                return false;
            }

            return true;
        }

        public void OnClickNextStage()//다음 스테이지 이동
        {
            if (!IsAvailableNextStage())
                return;

            if (IsAutoAdventured())
            {
                OpenAutoAdventurePopup();
                return;
            }

            StageManager.ClearAccumData();

            int world = IsLastStage() ? stageData.World + 1 : stageData.World;
            int stage = IsLastStage() ? 1 : stageData.Stage + 1;
            int diff = CacheUserData.GetInt("adventure_difficult", 1);
            var stageListData = StageManager.Instance.AdventureProgressData.GetWorldStages(world, diff);
            int starCount = 0;
            if (stageListData != null && stageListData.Count > 0)
            {
                starCount = stageListData[stage-1];
            }
            StageInfoPopupData newPopupData = new StageInfoPopupData(world, stage, diff, starCount);
            StageManager.Instance.SetAll(IsLastStage() ? stageData.World + 1 : stageData.World, IsLastStage() ? 1 : stageData.Stage + 1, 1);
            UIManager.Instance.InitUI(eUIType.Adventure);
            PopupManager.OpenPopup<AdventureReadyPopup>(newPopupData);
        }

        protected virtual IEnumerator AutoScheduler()
        {
            isAuto = true;
            while (AutoPlayingTimer > 0 && isAuto)
            {
                AutoPlayingTimer -= Time.deltaTime;
                autoButtonLabel.text = ((int)AutoPlayingTimer).ToString();
                yield return null;
            }
            if (AutoPlayingTimer <= 0 && isAuto)
            {
                AutoRetry();
                isAuto = false;
                autoButtonLabel.text = "0";
            }
        }
        public void onClickAutoClick()
        {
            isAuto = false;
            AutoRetry();
        }
        void AutoRetry()
        {
            if (User.Instance.CheckInventoryForContentsEnter(eInvenSlotCheckContentType.Adventure, stageData.World, stageData.Stage) == false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                    () => {
                        HideAutoButton();
                        StageManager.ClearAccumData();
                        StageManager.AccumRewards.Clear();
                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
                        //MainPopup.OpenPopup(eMainPopupTabType.Inventory, -1);// 가방 UI로 이동
                    },
                    () => {
                        HideAutoButton();
                        ShowTotalRewardPopup();
                        StageManager.ClearAccumData();
                    },
                    () => {
                        HideAutoButton();
                        ShowTotalRewardPopup();
                        StageManager.ClearAccumData();
                    }
                );
                return;
            }
            if (IsAutoAdventured())
            {
                MinusAutoCount();
            }
            BattleLine battleLine = User.Instance.PrefData.AdventureBattleLine;
            WWWForm param = new WWWForm();
            param.AddField("world", stageData.World);
            param.AddField("diff", stageData.Diff);
            param.AddField("stage", stageData.Stage);
            param.AddField("deck", battleLine.GetJsonString());
            param.AddField("deckinf", battleLine.GetJsonStringINF());

            NetworkManager.Send("adventure/start", param, (JObject jsonData) => {
                if (jsonData["rs"] != null)
                {
                    switch ((eApiResCode)(int)jsonData["rs"])
                    {
                        case eApiResCode.OK:
                            AdventureManager.Instance.SetStartData(stageData.World, stageData.Stage, jsonData);
                            if (AdventureManager.Instance.IsStartCheck())
                            {
                                LoadingManager.Instance.EffectiveSceneLoad("AdventureBattle", eSceneEffectType.CloudAnimation, AdventureManager.Instance.LoadingCoroutine);
                            }
                            break;
                        case eApiResCode.COST_SHORT:
                            var costType = StageBaseData.GetByAdventureWorldStage(stageData.World, stageData.Stage).COST_TYPE;
                            string needItemName = "";
                            switch (costType)
                            {
                                case "ENERGY":
                                    needItemName = StringData.GetStringByStrKey("item_base:name:10000002");
                                    break;
                                case "DAILY_TICKET":
                                    needItemName = string.Format(StringData.GetStringByIndex(100002096), "");
                                    break;
                            }
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), string.Format(StringData.GetStringByIndex(100000224), needItemName));
                            break;
                        case eApiResCode.ADV_INVALID_DRAGON:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000635), true, false, false);
                            break;
                    }
                }
            });
        }
        //실패시 자동전투 해제
        void ReleaseAutoAdventure()
        {
            StageManager.StageStartAccum();
        }

        void ShowAdventureFailPopup()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000782), true, false, true);
        }
        void HideAutoButton()
        {
            if (autoButtonNode.activeInHierarchy)
            {
                autoButtonNode.SetActive(false);
            }
        }
        void SetTotalAutoAdventureReward(int itemType, int itemNo, int itemCount) //누적 보상 데이터 쌓기
        {
            if (!autoAdventureReward.ContainsKey(itemType))
            {
                autoAdventureReward.Add(itemType, new Dictionary<int, Asset>());
            }

            if (autoAdventureReward[itemType].ContainsKey(itemNo))
            {
                autoAdventureReward[itemType][itemNo].AddCount(itemCount);
            }
            else
            {
                autoAdventureReward[itemType].Add(itemNo, new Asset(itemNo, itemCount, itemType));
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

        public void OnClickStatisticInfo()
        {
            PopupManager.OpenPopup<AdventureStatisticsPopup>(new AdventureStatisticPopupData(false, stageData.Time, stageNameText.text));
        }
    }
}

        
        
