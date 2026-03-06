using DG.Tweening;
using Google.Impl;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public struct UIBattleObjectStartEvent
    {
        private static UIBattleObjectStartEvent obj;

        public static void Send()
        {
            EventManager.TriggerEvent(obj);
        }
    }
    public struct UIBattleObjectEndEvent
    {
        private static UIBattleObjectEndEvent obj;

        public static void Send()
        {
            EventManager.TriggerEvent(obj);
        }
    }
    public struct UIBattleBossStartEvent
    {
        private static UIBattleBossStartEvent obj;
        public IBattleCharacterData targetBoss { get; private set; }
        public static void Send(IBattleCharacterData boss)
        {
            obj.targetBoss = boss;

            EventManager.TriggerEvent(obj);
        }
    }
    public struct UIBattleBossEndEvent
    {
        private static UIBattleBossEndEvent obj;

        public static void Send()
        {
            EventManager.TriggerEvent(obj);
        }
    }
    public struct UIBattleStateEndEvent
    {
        private static UIBattleStateEndEvent obj;

        public static void Send()
        {
            EventManager.TriggerEvent(obj);
        }
    }
    public class UIBattleObject : UIObject, EventListener<UIBattleObjectStartEvent>, EventListener<AdventureStateEvent>, EventListener<UIBattleBossStartEvent>, EventListener<UIBattleStateEndEvent>, EventListener<UIStatisticEvent>
    {
        [Header("Common")]
        [SerializeField]

        protected GameObject pauseBtn = null;
        [SerializeField]
        protected SkillSlotGroup skillSlotGroup = null;
        [SerializeField]
        protected Text speedText = null;
        [SerializeField]
        protected Animator autoAni = null;


        [Space(10f)]
        [Header("PVP")]
        [SerializeField]
        private Text pvpTimeText = null;
        [SerializeField]
        private Slider pvpOffHpbar = null;
        [SerializeField]
        private Text pvpOffNameText = null;
        [SerializeField]
        private Slider pvpDefHpbar = null;
        [SerializeField]
        private Text pvpDefNameText = null;
        [SerializeField]
        private Text pvpOffHpText = null;
        [SerializeField]
        private Text pvpDefHpText = null;
        [SerializeField]
        private RectTransform lostOffHp = null;
        [SerializeField]
        private RectTransform lostDefHp = null;
        [SerializeField]
        private Animator pvpAnim = null;
        [SerializeField]
        private ElemBuffInfoUI elemBuffUI = null;

        [Space(10f)]
        [Header("ChampionBattle")]
        [SerializeField]
        private GameObject championRoundUI = null;
        [SerializeField]
        private Text championRoundText = null;
        [SerializeField]
        GameObject[] ARoundWin;
        [SerializeField]
        GameObject[] BRoundWin;
        [SerializeField]
        private Animator championAnim = null;
        [SerializeField]
        private Text championMatchText = null;
        [SerializeField]
        private Text championMatchRound = null;
        [SerializeField]
        private GameObject championResult = null;
        [SerializeField]
        private Text championResultNick = null;
        [SerializeField]
        private Text championRoundResult = null;
        [SerializeField]
        ServerFlagUI luserFlag = null;
        [SerializeField]
        ServerFlagUI ruserFlag = null;
        [SerializeField]
        ChampionSurpportUI surpportinfo = null;
        [SerializeField]
        GameObject roundPlayer = null;
        [SerializeField]
        Button[] roundPlayBtns = null;

        [Space(10f)]
        [Header("Adventure")]
        [SerializeField]
        protected Transform stageUI = null;
        [SerializeField]
        protected Text advWorldNameText = null;
        [SerializeField]
        protected GameObject advWavePanel = null;
        [SerializeField]
        protected Text advWaveText = null;
        [SerializeField]
        protected Text advTimeText = null;
        [SerializeField]
        protected Animator advAnim = null;
        [SerializeField]
        protected Animator advBossAnim = null;
        [SerializeField]
        protected Animator worldBossAnim = null;
        [SerializeField]
        protected Text advAnimText = null;
        [SerializeField]
        protected GameObject advAutoObj = null;
        [SerializeField]
        protected Text advAutoText = null;
        [SerializeField]
        protected Animator advAutoAnim = null;
        [SerializeField]
        protected BossHpUIObject advBossHpObj = null;
        [SerializeField]
        protected WorldBossHpUI worldBossHpObj = null;

        [SerializeField]
        protected float bossAlertAnimWaitTime = 1f;
        [SerializeField]
        protected float bossAlertAnimSpeed = 1f;

        [SerializeField]
        protected GameObject noticeObject = null;



        [Header("Statistics")]

        [SerializeField]
        GameObject statisticLeftParent = null;
        [SerializeField]
        GameObject statisticRightParent = null;
        [SerializeField]
        StatisticClone[] myStatistics = null;
        [SerializeField]
        StatisticClone[] enemyStatistics = null;
        [SerializeField]
        GameObject myStatisticLayer = null;
        [SerializeField]
        GameObject enemyStatisticLayer = null;

        [SerializeField]
        Text statisticHideText = null;
        [SerializeField]
        Button detailDmgBtnObj = null;
        [SerializeField]
        Button simpleDmgBtnObj = null;
        [SerializeField]
        Button recieveDmgBtnObj = null;

        [SerializeField]
        Text menu1Text = null;
        [SerializeField]
        Text menu2Text = null;
        [SerializeField]
        Text menu1RightText = null;
        [SerializeField]
        Text menu2RightText = null;
        [SerializeField]
        GameObject openBtnImgObj = null;
        [SerializeField]
        GameObject closeBtnImgObj = null;

        bool isStatisticHide = false;
        private Dictionary<int, StatisticClone> enemyTeam = new Dictionary<int, StatisticClone>();
        private Dictionary<int, StatisticClone> myTeam = new Dictionary<int, StatisticClone>();


        protected bool Pause = false;

        float lastHpOff = 1;
        float lastHpDef = 1;

        Sequence offSequence;
        Sequence deffSequence;


        public override void Init()
        {
            base.Init();
        }

        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            advAutoObj.SetActive(false);
            championRoundUI.SetActive(false);
            championResult.SetActive(false);
            ClearStatistic();
            if (targetType.HasFlag(eUIType.Battle_ChampionBattle))
            {
                elemBuffUI.Init();

                ChampionRoundInit();

                if (LoadingManager.Instance.currentScene == "ChampionPracticeColosseum")
                {
                    championRoundText.text = StringData.GetStringByStrKey("연습모드");
                    if(luserFlag != null)
                        luserFlag.SetFlag(0);
                    if(ruserFlag != null)
                        ruserFlag.SetFlag(0);

                    if(surpportinfo != null)
                        surpportinfo.SetActive(false);
                    if(roundPlayer != null)
                        roundPlayer.SetActive(false);
                }
                else
                {
                    championRoundText.text = StringData.GetStringFormatByStrKey("경기라운드", ChampionManager.Instance.CurRoundIndex + 1);
                    var cData = ChampionManager.Instance.ChampionData;
                    if (luserFlag != null)
                        luserFlag.SetFlag(cData.UserA.Server);
                    if (ruserFlag != null)
                        ruserFlag.SetFlag(cData.UserB.Server);
                    if (surpportinfo != null)
                        surpportinfo.SetActive(true);
                    if (roundPlayer != null)
                    {
                        roundPlayer.SetActive(false);
                        if (roundPlayBtns != null)
                        {
                            //if (roundPlayBtns.Length == 3 && ChampionManager.Instance.CurMatchData != null && ChampionManager.Instance.CurMatchData.Detail != null)
                            //{
                            //    roundPlayer.SetActive(true);
                            //    bool[] results = new bool[] { 
                            //        ChampionManager.Instance.CurMatchData.Detail.Round1Result == eChampionWinType.SIDE_A_WIN || ChampionManager.Instance.CurMatchData.Detail.Round1Result == eChampionWinType.SIDE_B_WIN
                            //        , ChampionManager.Instance.CurMatchData.Detail.Round2Result == eChampionWinType.SIDE_A_WIN || ChampionManager.Instance.CurMatchData.Detail.Round2Result == eChampionWinType.SIDE_B_WIN
                            //        , ChampionManager.Instance.CurMatchData.Detail.Round3Result == eChampionWinType.SIDE_A_WIN || ChampionManager.Instance.CurMatchData.Detail.Round3Result == eChampionWinType.SIDE_B_WIN 
                            //    };
                            //    for (int i = 0; i < 3; i++)
                            //    {
                            //        roundPlayBtns[i].interactable = results[i];
                            //    }
                            //}
                        }
                    }
                }

                SetStatisticContentType(eUIStatisticsContentType.ChampionBattle);
                if (statisticLeftParent != null)
                {
                    statisticLeftParent.SetActive(true);
                    statisticRightParent.SetActive(true);
                }
                StatisticsMananger.Instance.InitStatistic(eUIStatisticsContentType.ChampionBattle);
                OnClickDefaultStatistics();
                Time.timeScale = ChampionManager.Instance.ChampionData.Speed;
                StopCoroutine(nameof(UpdateChampionBattle));
                StartCoroutine(nameof(UpdateChampionBattle));
            }
            else if (targetType.HasFlag(eUIType.Battle_Arena))
            {
                elemBuffUI.Init();
                SetStatisticContentType(eUIStatisticsContentType.Arena);
                if (statisticLeftParent != null)
                {
                    statisticLeftParent.SetActive(true);
                    statisticRightParent.SetActive(true);
                }

                StatisticsMananger.Instance.InitStatistic(eUIStatisticsContentType.Arena);
                OnClickDefaultStatistics();
                Time.timeScale = ArenaManager.Instance.ColosseumData.Speed;
                StopCoroutine(nameof(UpdateArena));
                StartCoroutine(nameof(UpdateArena));
            }
            else if (targetType.HasFlag(eUIType.Battle_Adventure))
            {
                SetStatisticContentType(eUIStatisticsContentType.Adventure);
#if UNITY_EDITOR
                if (statisticLeftParent != null)
                {
                    statisticLeftParent.SetActive(true);
                    statisticRightParent.SetActive(false);
                }
                OnClickDefaultStatistics();
#endif
                StatisticsMananger.Instance.InitStatistic(eUIStatisticsContentType.Adventure);
                Time.timeScale = AdventureManager.Instance.Data.Speed;
                StopCoroutine(nameof(UpdateAdventure));
                StartCoroutine(nameof(UpdateAdventure));
                setAutoAdvObj();

                if (noticeObject != null)//속성 팝업 열려있으면 끄기
                    noticeObject.SetActive(false);
            }
            else if (targetType.HasFlag(eUIType.Battle_Daily))
            {
                SetStatisticContentType(eUIStatisticsContentType.DailyDungeon);
#if UNITY_EDITOR
                if (statisticLeftParent != null)
                {
                    statisticLeftParent.SetActive(true);
                    statisticRightParent.SetActive(false);
                }
                OnClickDefaultStatistics();
#endif
                StatisticsMananger.Instance.InitStatistic(eUIStatisticsContentType.DailyDungeon);
                Time.timeScale = DailyManager.Instance.Data.Speed;
                StopCoroutine(nameof(UpdateDaily));
                StartCoroutine(nameof(UpdateDaily));
                setAutoAdvObj();

                if (noticeObject != null)//속성 팝업 열려있으면 끄기
                    noticeObject.SetActive(false);
            }
            else if (targetType.HasFlag(eUIType.Battle_WorldBoss))
            {
                SetStatisticContentType(eUIStatisticsContentType.WorldBoss);
                if (statisticLeftParent != null)
                {
                    statisticLeftParent.SetActive(false);
#if UNITY_EDITOR
                    statisticRightParent.SetActive(false);
#else
                    statisticRightParent.SetActive(false);
#endif
                }
                StatisticsMananger.Instance.InitStatistic(eUIStatisticsContentType.WorldBoss);
                Time.timeScale = WorldBossManager.Instance.Data.Speed;
                StopCoroutine(nameof(UpdateWorldBoss));
                StartCoroutine(nameof(UpdateWorldBoss));
                setAutoAdvObj();

                if (noticeObject != null)//속성 팝업 열려있으면 끄기
                    noticeObject.SetActive(false);
            }
            else
            {
                SetStatisticContentType(eUIStatisticsContentType.NONE);
                StopAllCoroutines();
            }


            if (skillSlotGroup != null)
                skillSlotGroup.SetContentUIType(targetType);
        }

        void Start()
        {
            EventManager.AddListener<AdventureStateEvent>(this);
            if (TutorialManager.tutorialManagement.IsOtherContentsBlock())
            {
                if (pauseBtn != null)
                    pauseBtn.gameObject.SetActive(false);
            }
        }
        void OnEnable()
        {

            EventManager.AddListener<UIStatisticEvent>(this);
            EventManager.AddListener<UIBattleObjectStartEvent>(this);
            EventManager.AddListener<UIBattleBossStartEvent>(this);
            EventManager.AddListener<UIBattleStateEndEvent>(this);
        }
        void OnDisable()
        {
            Time.timeScale = 1.0f;
            EventManager.RemoveListener<UIStatisticEvent>(this);
            EventManager.RemoveListener<UIBattleObjectStartEvent>(this);
            EventManager.RemoveListener<UIBattleBossStartEvent>(this);
            EventManager.RemoveListener<UIBattleStateEndEvent>(this);
        }
        void OnDestroy()
        {
            EventManager.RemoveListener<AdventureStateEvent>(this);
        }

        IEnumerator UpdateChampionBattle()
        {
            var cData = ChampionManager.Instance.ChampionData;
            if (pvpOffNameText != null)
                pvpOffNameText.text = cData.ASideNick;
            if (pvpDefNameText != null)
                pvpDefNameText.text = cData.BSideNick;

            if (advBossAnim != null)
                advBossAnim.gameObject.SetActive(false);
            if (advBossHpObj != null)
                advBossHpObj.Clear();
            if (worldBossHpObj != null)
                worldBossHpObj.Clear();

            List<IBattleCharacterData> list = new List<IBattleCharacterData>();
            var dragonsDic = cData.OffenseDic;
            if (dragonsDic != null)
            {
                foreach (var it in dragonsDic)
                {
                    if (it.Value == null)
                        continue;

                    list.Add(it.Value);
                }
            }

            if (skillSlotGroup != null)
                skillSlotGroup.SetSlot(cData, list);


            if (statisticLeftParent != null)
            {
                statisticLeftParent.SetActive(true);
                statisticRightParent.SetActive(true);
                for (int i = 0, count = list.Count; i < count; ++i)
                {
                    myTeam[list[i].ID] = myStatistics[i];
                    myStatistics[i].SetImage(list[i].ID);
                }
                int index = 0;
                foreach (var it in cData.DefenseDic)
                {
                    if (it.Value == null)
                        continue;
                    int id = it.Value.ID;
                    enemyTeam[id] = enemyStatistics[index];
                    enemyStatistics[index++].SetImage(id);
                }
            }

            if (pvpTimeText != null)
            {
                var timeEnable = pvpTimeText.GetComponent<TimeEnable>();
                timeEnable.Refresh = delegate
                {
                    pvpTimeText.text = SBFunc.TimeStringMinute(cData.MaxTime - cData.Time);
                };
                timeEnable.Refresh.Invoke();
            }
            lostOffHp.sizeDelta = Vector2.zero;
            lostDefHp.sizeDelta = Vector2.zero;
            offSequence = DOTween.Sequence();
            deffSequence = DOTween.Sequence();
            lastHpOff = 1;
            lastHpDef = 1;
            while (true)
            {
                if (!Pause)
                {
                    if (pvpOffHpbar != null && pvpOffHpText != null)
                    {
                        float hpPercent = cData.OffHPRate();
                        pvpOffHpbar.value = hpPercent;
                        if (hpPercent >= 0.01f)
                            pvpOffHpText.text = string.Format("{0:P0}", hpPercent);
                        else
                            pvpOffHpText.text = string.Format("{0:P2}", hpPercent);
                    }

                    if (pvpDefHpbar != null && pvpDefHpText != null)
                    {
                        float hpPercent = cData.DefHPRate();
                        pvpDefHpbar.value = hpPercent;
                        if (hpPercent >= 0.01f)
                            pvpDefHpText.text = string.Format("{0:P0}", hpPercent);
                        else
                            pvpDefHpText.text = string.Format("{0:P2}", hpPercent);
                    }
                }

                yield return null;
            }
        }

        IEnumerator UpdateArena()
        {
            var cData = ArenaManager.Instance.ColosseumData;
            if (pvpOffNameText != null)
                pvpOffNameText.text = User.Instance.UserData.UserNick;
            if (pvpDefNameText != null)
                pvpDefNameText.text = cData.EnemyNick;

            if (advBossAnim != null)
                advBossAnim.gameObject.SetActive(false);
            if (advBossHpObj != null)
                advBossHpObj.Clear();
            if (worldBossHpObj != null)
                worldBossHpObj.Clear();

            List<IBattleCharacterData> list = new List<IBattleCharacterData>();
            var dragonsDic = cData.OffenseDic;
            if (dragonsDic != null)
            {
                foreach (var it in dragonsDic)
                {
                    if (it.Value == null)
                        continue;

                    list.Add(it.Value);
                }
            }

            if (skillSlotGroup != null)
                skillSlotGroup.SetSlot(cData, list);




            if (statisticLeftParent != null)
            {
                statisticLeftParent.SetActive(true);
                statisticRightParent.SetActive(true);
                for (int i = 0, count = list.Count; i < count; ++i)
                {
                    myTeam[list[i].ID] = myStatistics[i];
                    myStatistics[i].SetImage(list[i].ID);
                }
                int index = 0;
                foreach (var it in cData.DefenseDic)
                {
                    if (it.Value == null)
                        continue;
                    int id = it.Value.ID;
                    enemyTeam[id] = enemyStatistics[index];
                    enemyStatistics[index++].SetImage(id);
                }
            }

            if (speedText != null)
            {
                speedText.text = SBFunc.StrBuilder("x", cData.Speed.ToString("F1"));
            }
            if (autoAni != null)
            {
                autoAni.Play("auto_a");
            }

            if (pvpTimeText != null)
            {
                var timeEnable = pvpTimeText.GetComponent<TimeEnable>();
                timeEnable.Refresh = delegate
                {
                    pvpTimeText.text = SBFunc.TimeStringMinute(cData.MaxTime - cData.Time);
                };
                timeEnable.Refresh.Invoke();
            }
            lostDefHp.sizeDelta = Vector2.zero;
            lostOffHp.sizeDelta = Vector2.zero;
            offSequence = DOTween.Sequence();
            deffSequence = DOTween.Sequence();
            lastHpOff = 1;
            lastHpDef = 1;
            while (true)
            {
                if (!Pause)
                {
                    if (pvpOffHpbar != null && pvpOffHpText != null)
                    {
                        float hpPercent = cData.OffHPRate();
                        pvpOffHpbar.value = hpPercent;
                        if (hpPercent >= 0.01f)
                            pvpOffHpText.text = string.Format("{0:P0}", hpPercent);
                        else
                            pvpOffHpText.text = string.Format("{0:P2}", hpPercent);
                    }

                    if (pvpDefHpbar != null && pvpDefHpText != null)
                    {
                        float hpPercent = cData.DefHPRate();
                        pvpDefHpbar.value = hpPercent;
                        if (hpPercent >= 0.01f)
                            pvpDefHpText.text = string.Format("{0:P0}", hpPercent);
                        else
                            pvpDefHpText.text = string.Format("{0:P2}", hpPercent);
                    }
                }

                yield return null;
            }
        }

        public void OffHpBarChange()
        {
            offSequence.Kill();
            lostOffHp.sizeDelta += new Vector2(220 * (lastHpOff - pvpOffHpbar.value), 0);
            offSequence.Append(lostOffHp.DOSizeDelta(Vector2.zero, .5f));
            lastHpOff = pvpOffHpbar.value;
        }
        public void DeffHpBarChange()
        {
            deffSequence.Kill();
            lostDefHp.sizeDelta += new Vector2(220 * (lastHpDef - pvpDefHpbar.value), 0);
            deffSequence.Append(lostDefHp.DOSizeDelta(Vector2.zero, 1.0f));
            lastHpDef = pvpDefHpbar.value;
        }

        protected virtual IEnumerator UpdateAdventure()
        {
            var aData = AdventureManager.Instance.Data;
            var list = new List<IBattleCharacterData>();
            var dragonsList = aData.Characters;
            if (dragonsList != null)
            {
                for (int i = 0, count = dragonsList.Count; i < count; ++i)
                {
                    if (dragonsList[i] == null)
                        continue;

                    list.Add(dragonsList[i]);
                }
            }

            if (stageUI != null)
                stageUI.gameObject.SetActive(true);

            advBossAnim.gameObject.SetActive(false);
            advBossHpObj.Clear();
            if (worldBossHpObj != null)
                worldBossHpObj.Clear();

            if (skillSlotGroup != null)
                skillSlotGroup.SetSlot(aData, list);

            if (advWorldNameText != null)
            {
                var world = WorldData.GetByWorldNumber(aData.World);
                if (world != null)
                    advWorldNameText.text = StringData.GetStringByIndex(world._NAME) + string.Format(" {0}-{1}", aData.World, aData.Stage);
            }

#if UNITY_EDITOR
            if (statisticLeftParent != null)
            {
                statisticLeftParent.SetActive(true);
                for (int i = 0, count = list.Count; i < count; ++i)
                {
                    myTeam[list[i].ID] = myStatistics[i];
                    myStatistics[i].SetImage(list[i].ID);
                }
            }

#endif

            LayoutRebuilder.ForceRebuildLayoutImmediate(advWorldNameText.GetComponent<RectTransform>());
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(stageUI.GetComponent<RectTransform>());

            if (advWaveText != null)
            {
                if (advWavePanel != null)
                    advWavePanel.SetActive(true);
                advWaveText.text = SBFunc.StrBuilder(aData.Wave, "/", aData.MaxWave);
            }

            if (speedText != null)
            {
                speedText.text = SBFunc.StrBuilder("x", aData.Speed.ToString("F1"));
            }

            if (autoAni != null)
            {
                autoAni.Play(aData.IsAuto ? "auto_a" : "auto_off");
            }

            if (advAnimText != null)
            {
                advAnimText.text = SBFunc.StrBuilder(aData.World, "-", aData.Stage);
            }

            if (advTimeText != null)
            {
                var timeEnable = advTimeText.GetComponent<TimeEnable>();
                if (timeEnable == null)
                    timeEnable = advTimeText.gameObject.AddComponent<TimeEnable>();
                timeEnable.Refresh = delegate
                {
                    advTimeText.text = SBFunc.TimeStringMinute(aData.MaxTime - aData.Time);
                };
            }
            while (aData.State == eBattleState.Playing)
            {
                if (!Pause)
                {
                    if (advWaveText != null)
                        advWaveText.text = SBFunc.StrBuilder(aData.Wave, "/", aData.MaxWave);

                    if (advBossHpObj.IsBoss())
                        advBossHpObj.RefreshBossHpBar();
                }
                yield return null;
            }
            yield break;
        }
        protected virtual IEnumerator UpdateWorldBoss()
        {
            var aData = WorldBossManager.Instance.Data;
            var list = new List<IBattleCharacterData>();
            var dragonsList = aData.Characters;
            if (dragonsList != null)
            {
                for (int i = 0, count = dragonsList.Count; i < count; ++i)
                {
                    if (dragonsList[i] == null)
                        continue;

                    list.Add(dragonsList[i]);
                }
            }

            if (stageUI != null)
                stageUI.gameObject.SetActive(true);

            advBossAnim.gameObject.SetActive(false);
            advBossHpObj.Clear();
            if (worldBossHpObj != null)
                worldBossHpObj.InitWorldBoss();

            if (advWorldNameText != null && aData.BossData != null && aData.BossData.BaseData != null)
            {
                advWorldNameText.text = StringData.GetStringByStrKey(aData.BossData.BaseData._NAME);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(advWorldNameText.GetComponent<RectTransform>());
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(stageUI.GetComponent<RectTransform>());

            if (advWavePanel != null)
                advWavePanel.SetActive(false);

            if (speedText != null)
            {
                speedText.text = SBFunc.StrBuilder("x", aData.Speed.ToString("F1"));
            }

            if (autoAni != null)
            {
                autoAni.Play(aData.IsAuto ? "auto_a" : "auto_off");
            }

            if (advTimeText != null)
            {
                var timeEnable = advTimeText.GetComponent<TimeEnable>();
                if (timeEnable == null)
                    timeEnable = advTimeText.gameObject.AddComponent<TimeEnable>();
                timeEnable.Refresh = delegate
                {
                    advTimeText.text = SBFunc.TimeStringMinute(aData.MaxTime - aData.Time);
                };
            }

            while (aData.State == eBattleState.Playing)
            {
                if (!Pause)
                {
                    worldBossHpObj.RefreshBossHpBar();
                }
                yield return null;
            }
            worldBossHpObj.RefreshBossHpBar();
            yield break;
        }
        protected virtual IEnumerator UpdateDaily()
        {
            var aData = DailyManager.Instance.Data;
            var list = new List<IBattleCharacterData>();
            var dragonsList = aData.Characters;
            if (dragonsList != null)
            {
                for (int i = 0, count = dragonsList.Count; i < count; ++i)
                {
                    if (dragonsList[i] == null)
                        continue;

                    list.Add(dragonsList[i]);
                }
            }

            if (stageUI != null)
                stageUI.gameObject.SetActive(true);

            advBossAnim.gameObject.SetActive(false);
            advBossHpObj.Clear();
            if (worldBossHpObj != null)
                worldBossHpObj.Clear();

            if (skillSlotGroup != null)
                skillSlotGroup.SetSlot(aData, list);

            if (advWorldNameText != null)
            {
                var world = WorldData.GetByWorldNumber(aData.World);
                if (world != null)
                    advWorldNameText.text = SBFunc.StrBuilder(StringData.GetStringByIndex(world._NAME), string.Format(" Lv.{0}", aData.Stage));
            }

#if UNITY_EDITOR
            statisticLeftParent.SetActive(true);
            for (int i = 0, count = list.Count; i < count; ++i)
            {
                myTeam[list[i].ID] = myStatistics[i];
                myStatistics[i].SetImage(list[i].ID);
            }
#endif

            LayoutRebuilder.ForceRebuildLayoutImmediate(advWorldNameText.GetComponent<RectTransform>());
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(stageUI.GetComponent<RectTransform>());

            if (advWaveText != null)
            {
                if (advWavePanel != null)
                    advWavePanel.SetActive(true);
                advWaveText.text = SBFunc.StrBuilder(aData.Wave, "/", aData.MaxWave);
            }

            if (speedText != null)
            {
                speedText.text = SBFunc.StrBuilder("x", aData.Speed.ToString("F1"));
            }

            if (autoAni != null)
            {
                autoAni.Play(aData.IsAuto ? "auto_a" : "auto_off");
            }

            if (advAnimText != null)
            {
                advAnimText.text = SBFunc.StrBuilder(StringData.GetStringFormatByIndex(100000056, aData.Stage));
            }

            if (advTimeText != null)
            {
                var timeEnable = advTimeText.GetComponent<TimeEnable>();
                if (timeEnable == null)
                    timeEnable = advTimeText.gameObject.AddComponent<TimeEnable>();
                timeEnable.Refresh = delegate
                {
                    advTimeText.text = SBFunc.TimeStringMinute(aData.MaxTime - aData.Time);
                };
            }
            while (aData.State == eBattleState.Playing)
            {
                if (!Pause)
                {
                    if (advWaveText != null)
                        advWaveText.text = SBFunc.StrBuilder(aData.Wave, "/", aData.MaxWave);

                    if (advBossHpObj.IsBoss())
                        advBossHpObj.RefreshBossHpBar();
                }
                yield return null;
            }
            yield break;
        }
        public virtual void OnClickExit()
        {
            if (LoadingManager.Instance.currentScene == "ChampionPracticeColosseum")
            {
                LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleSetting", eSceneEffectType.CloudAnimation);
            }
            else
            {
                LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleLobby", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() =>
                {
                    ChampionManager.Instance.LogRelease();
                    MatchInfoPopup.OpenPopup();
                }));
            }


        }

        public virtual void OnClickPause()
        {
            if (curSceneType.HasFlag(eUIType.Battle_Arena))
            {
                PausePopup.OpenArenaPopup();
            }
            else if (curSceneType.HasFlag(eUIType.Battle_Adventure))
            {
                if (isAutoAdvMode)
                {
                    Time.timeScale = 0f;
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000780), StringData.GetStringByIndex(100000778), StringData.GetStringByIndex(100000779),
                        () =>
                        {
                            Time.timeScale = AdventureManager.Instance.Data.Speed;
                        },
                        () =>
                        {
                            Time.timeScale = AdventureManager.Instance.Data.Speed;
                            OnClickAutoAdventure();
                        },
                        () =>
                        {
                            Time.timeScale = AdventureManager.Instance.Data.Speed;
                        }
                    );
                    return;
                }
                AdventurePauseProcess();
            }
            else if (curSceneType.HasFlag(eUIType.Battle_Daily))
            {
                if (isAutoAdvMode)
                {
                    Time.timeScale = 0f;
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000780), StringData.GetStringByIndex(100000778), StringData.GetStringByIndex(100000779),
                        () =>
                        {
                            Time.timeScale = DailyManager.Instance.Data.Speed;
                        },
                        () =>
                        {
                            Time.timeScale = DailyManager.Instance.Data.Speed;
                            OnClickAutoAdventure();
                        },
                        () =>
                        {
                            Time.timeScale = DailyManager.Instance.Data.Speed;
                        }
                    );
                    return;
                }
                DailyPauseProcess();
            }
            else if (curSceneType.HasFlag(eUIType.Battle_WorldBoss))
            {
                if (WorldBossStage.Instance.IsBattleState())
                    PausePopup.OpenWorldBossPopup();
            }
        }

        void AdventurePauseProcess()
        {
            PausePopup.OpenAdventurePopup();
        }

        void DailyPauseProcess()
        {
            PausePopup.OpenDailyPopup();
        }

        public virtual void OnClickSpeed()
        {
            if (curSceneType.HasFlag(eUIType.Battle_ChampionBattle))
            {
                var cur = PlayerPrefs.GetInt("ChampionBattleSpeedIndex", 0);
                cur++;
                cur = cur % 4;
                var speed = 1.0f;
                switch (cur)
                {
                    case 1:
                        speed = 1.2f;
                        break;
                    case 2:
                        speed = 1.5f;
                        break;
                    case 3:
                        speed = 2f;
                        break;                    
                    case 0:
                    default:
                        speed = 1.0f;
                        break;
                }
                PlayerPrefs.SetInt("ChampionBattleSpeedIndex", cur);
                PlayerPrefs.SetFloat("ChampionBattleSpeed", speed);
                ArenaManager.Instance.ColosseumData.Speed = speed;
                Time.timeScale = speed;

                if (speedText != null)
                {
                    speedText.text = SBFunc.StrBuilder("x", speed.ToString("F1"));
                }
            }
            else if (curSceneType.HasFlag(eUIType.Battle_Arena))
            {
                var cur = PlayerPrefs.GetInt("ArenaSpeedIndex", 0);
                cur++;
                int max = User.Instance.BATTLE_SPPED_BOOST ? 5 : 4;
                cur = cur % max;

                var speed = 1.0f;
                switch (cur)
                {
                    case 1:
                        speed = 1.2f;
                        break;
                    case 2:
                        speed = 1.5f;
                        break;
                    case 3:
                        speed = 2f;
                        break;
                    case 4:
                        speed = 2.5f;
                        break;
                    case 0:
                    default:
                        speed = 1.0f;
                        break;
                }
                PlayerPrefs.SetInt("ArenaSpeedIndex", cur);
                PlayerPrefs.SetFloat("ArenaSpeed", speed);
                ArenaManager.Instance.ColosseumData.Speed = speed;
                Time.timeScale = speed;

                if (speedText != null)
                {
                    speedText.text = SBFunc.StrBuilder("x", speed.ToString("F1"));
                }
            }
            else if (curSceneType.HasFlag(eUIType.Battle_Adventure))
            {
                var cur = PlayerPrefs.GetInt("AdventureSpeedIndex", 0);
                cur++;
                int max = User.Instance.BATTLE_SPPED_BOOST ? 5 : 4;
                cur = cur % max;

                var speed = 1.0f;
                switch (cur)
                {
                    case 1:
                        speed = 1.2f;
                        break;
                    case 2:
                        speed = 1.5f;
                        break;
                    case 3:
                        speed = 2f;
                        break;
                    case 4:
                        speed = 2.5f;
                        break;
                    case 0:
                    default:
                        speed = 1.0f;
                        break;
                }
                PlayerPrefs.SetInt("AdventureSpeedIndex", cur);
                PlayerPrefs.SetFloat("AdventureSpeed", speed);
                AdventureManager.Instance.Data.Speed = speed;
                Time.timeScale = speed;

                if (speedText != null)
                {
                    speedText.text = SBFunc.StrBuilder("x", speed.ToString("F1"));
                }
            }
            else if (curSceneType.HasFlag(eUIType.Battle_Daily))
            {
                var cur = PlayerPrefs.GetInt("DailySpeedIndex", 0);
                cur++;
                int max = User.Instance.BATTLE_SPPED_BOOST ? 5 : 4;
                cur = cur % max;

                var speed = 1.0f;
                switch (cur)
                {
                    case 1:
                        speed = 1.2f;
                        break;
                    case 2:
                        speed = 1.5f;
                        break;
                    case 3:
                        speed = 2f;
                        break;
                    case 4:
                        speed = 2.5f;
                        break;
                    case 0:
                    default:
                        speed = 1.0f;
                        break;
                }

                PlayerPrefs.SetInt("DailySpeedIndex", cur);
                PlayerPrefs.SetFloat("DailySpeed", speed);
                DailyManager.Instance.Data.Speed = speed;
                Time.timeScale = speed;

                if (speedText != null)
                {
                    speedText.text = SBFunc.StrBuilder("x", speed.ToString("F1"));
                }
            }
            else if (curSceneType.HasFlag(eUIType.Battle_WorldBoss))
            {
                var cur = PlayerPrefs.GetInt("WorldBossSpeedIndex", 0);
                cur++;
                int max = User.Instance.BATTLE_SPPED_BOOST ? 5 : 4;
                cur = cur % max;

                var speed = 1.0f;
                switch (cur)
                {
                    case 1:
                        speed = 1.2f;
                        break;
                    case 2:
                        speed = 1.5f;
                        break;
                    case 3:
                        speed = 2f;
                        break;
                    case 4:
                        speed = 2.5f;
                        break;
                    case 0:
                    default:
                        speed = 1.0f;
                        break;
                }

                PlayerPrefs.SetInt("WorldBossSpeedIndex", cur);
                PlayerPrefs.SetFloat("WorldBossSpeed", speed);
                WorldBossManager.Instance.Data.Speed = speed;
                Time.timeScale = speed;

                if (speedText != null)
                {
                    speedText.text = SBFunc.StrBuilder("x", speed.ToString("F1"));
                }
            }
        }

        public virtual void OnClickAuto()
        {
            if (curSceneType.HasFlag(eUIType.Battle_Adventure))
            {
                var cur = PlayerPrefs.GetInt("AdventureAuto", 1);
                cur++;
                cur = cur % 2;

                PlayerPrefs.SetInt("AdventureAuto", cur);
                AdventureManager.Instance.Data.IsAuto = cur == 1;
                if (!AdventureManager.Instance.Data.IsAuto)
                    AdventureManager.Instance.Data.SkillQueueClear(eBattleSide.OffenseSide_1);

                if (autoAni != null)
                    autoAni.Play(AdventureManager.Instance.Data.IsAuto ? "auto_a" : "auto_off");
            }
            else if (curSceneType.HasFlag(eUIType.Battle_Daily))
            {
                var cur = PlayerPrefs.GetInt("DailyAuto", 1);
                cur++;
                cur = cur % 2;

                PlayerPrefs.SetInt("DailyAuto", cur);
                DailyManager.Instance.Data.IsAuto = cur == 1;
                if (!DailyManager.Instance.Data.IsAuto)
                    DailyManager.Instance.Data.SkillQueueClear(eBattleSide.OffenseSide_1);

                if (autoAni != null)
                    autoAni.Play(DailyManager.Instance.Data.IsAuto ? "auto_a" : "auto_off");
            }
            else if (curSceneType.HasFlag(eUIType.WorldBoss))
            {
                if (autoAni != null)
                    autoAni.Play("auto_a");
            }
        }

        public virtual void OnEvent(UIBattleObjectStartEvent eventType)
        {
            if (advAnim != null)
            {
                advAnim.gameObject.SetActive(false);
            }
            if (pvpAnim != null)
            {
                pvpAnim.gameObject.SetActive(false);
            }
            if (worldBossAnim != null)
            {
                worldBossAnim.gameObject.SetActive(false);
            }
            if (championAnim != null)
            {
                championAnim.gameObject.SetActive(false);
            }

            if (curSceneType.HasFlag(eUIType.Battle_Adventure) | curSceneType.HasFlag(eUIType.Battle_Daily))
            {
                if (advAnim != null)
                {
                    advAnim.gameObject.SetActive(true);
                    advAnim.Play("AdventureStart");
                }
            }
            else if (curSceneType.HasFlag(eUIType.Battle_Arena))
            {
                if (pvpAnim != null)
                {
                    pvpAnim.gameObject.SetActive(true);
                    pvpAnim.Play("Arenastart");
                }
            }
            else if (curSceneType.HasFlag(eUIType.Battle_ChampionBattle))
            {
                if (championAnim != null)
                {
                    string indexString = StringData.GetStringFormatByStrKey("경기라운드", ChampionManager.Instance.CurRoundIndex + 1);
                    if (LoadingManager.Instance.currentScene == "ChampionPracticeColosseum")
                    {
                        championMatchText.text = StringData.GetStringByStrKey("연습모드");

                        int count = CacheUserData.GetInt("champion_practice_" + ChampionManager.Instance.CurChampionInfo.CurSeason, 0) + 1;
                        CacheUserData.SetInt("champion_practice_" + ChampionManager.Instance.CurChampionInfo.CurSeason, count);
                        indexString = StringData.GetStringFormatByStrKey("경기라운드", count);
                    }
                    else
                    {
                        switch (ChampionManager.Instance.CurMatchData.UIROUND_INDEX)
                        {
                            case ChampionLeagueTable.ROUND_INDEX.ROUND16_A:
                                indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 1)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.ROUND16_B:
                                indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 2)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.ROUND16_C:
                                indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 3)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.ROUND16_D:
                                indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 4)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.ROUND16_E:
                                indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 5)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.ROUND16_F:
                                indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 6)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.ROUND16_G:
                                indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 7)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.ROUND16_H:
                                indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 8)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.ROUND8_A:
                                indexString = StringData.GetStringFormatByStrKey("8강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 1)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.ROUND8_B:
                                indexString = StringData.GetStringFormatByStrKey("8강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 2)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.ROUND8_C:
                                indexString = StringData.GetStringFormatByStrKey("8강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 3)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.ROUND8_D:
                                indexString = StringData.GetStringFormatByStrKey("8강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 4)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A:
                                indexString = StringData.GetStringFormatByStrKey("4강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 1)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B:
                                indexString = StringData.GetStringFormatByStrKey("4강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 2)) + " " + indexString;
                                break;
                            case ChampionLeagueTable.ROUND_INDEX.FINAL:
                                indexString = StringData.GetStringByStrKey("결승전") + " " + indexString;
                                break;
                        }

                        championMatchText.text = StringData.GetStringFormatByStrKey("챔피언대전이름", ChampionManager.Instance.CurChampionInfo.CurSeason);
                    }

                    championMatchRound.text = indexString;
                    championAnim.gameObject.SetActive(true);
                    championAnim.Play("ChampionStart");
                }
            }
            else if (curSceneType.HasFlag(eUIType.Battle_WorldBoss))
            {
                if (worldBossAnim != null)
                {
                    worldBossAnim.gameObject.SetActive(true);
                    worldBossAnim.Play("WorldBossStart");
                }
            }

        }
        public virtual void OnEvent(UIBattleBossStartEvent eventType)
        {
            if (curSceneType.HasFlag(eUIType.Battle_Adventure) | curSceneType.HasFlag(eUIType.Battle_Daily))
            {
                if (advBossAnim != null)
                {
                    StartCoroutine(bossAlert());
                    UIBattleBossEnemyAnim ui = advBossAnim.GetComponent<UIBattleBossEnemyAnim>();
                    if (ui != null)
                    {
                        if (stageUI != null)
                            stageUI.gameObject.SetActive(false);

                        ui.SetCallBack(() =>
                        {
                            UIBattleBossEndEvent.Send();
                            advBossHpObj.SetTargetBoss(eventType.targetBoss);

                            if (stageUI != null)
                                stageUI.gameObject.SetActive(true);

                            advBossAnim.gameObject.SetActive(false);
                        });
                    }
                }
            }
            else if (curSceneType.HasFlag(eUIType.Battle_WorldBoss))
            {
                if (advBossAnim != null)
                {
                    worldBossHpObj.SetTargetBoss(eventType.targetBoss);
                    StartCoroutine(bossAlert());
                    UIBattleBossEnemyAnim ui = advBossAnim.GetComponent<UIBattleBossEnemyAnim>();
                    if (ui != null)
                    {
                        ui.SetCallBack(() =>
                        {
                            UIBattleBossEndEvent.Send();
                            advBossAnim.gameObject.SetActive(false);
                        });
                    }
                }
            }
        }
        public virtual void OnEvent(UIBattleStateEndEvent eventType)
        {
            if (curSceneType.HasFlag(eUIType.Battle_Adventure) | curSceneType.HasFlag(eUIType.Battle_Daily) | curSceneType.HasFlag(eUIType.Battle_WorldBoss))
            {
                if (pauseBtn != null)
                    pauseBtn.SetActive(false);
            }
            else if (curSceneType.HasFlag(eUIType.Battle_Arena))
            {
                if (pauseBtn != null)
                    pauseBtn.SetActive(false);
            }
            else if (curSceneType.HasFlag(eUIType.Battle_ChampionBattle))
            {
                if (pauseBtn != null)
                    pauseBtn.SetActive(false);

                ChampionRoundInit(true);
            }
        }
        protected virtual IEnumerator bossAlert()
        {
            yield return SBDefine.GetWaitForSeconds(bossAlertAnimWaitTime / AdventureManager.Instance.Data.Speed);
            //yield return SBDefine.GetWaitForSeconds(4f/ AdventureManager.Instance.Data.Speed);
            advBossAnim.gameObject.SetActive(true);
            advBossAnim.speed = bossAlertAnimSpeed * AdventureManager.Instance.Data.Speed;
            advBossAnim.Play("AdventureBoss");
        }

        public virtual void StartAnimEnd()
        {
            if (curSceneType.HasFlag(eUIType.Battle_Adventure) | curSceneType.HasFlag(eUIType.Battle_Daily))
            {
                if (advAnim != null)
                {
                    advAnim.gameObject.SetActive(false);
                    UIBattleObjectEndEvent.Send();
                }
            }
            else if (curSceneType.HasFlag(eUIType.Battle_Arena))
            {
                if (pvpAnim != null)
                {
                    pvpAnim.gameObject.SetActive(false);
                    UIBattleObjectEndEvent.Send();
                }
            }
            else if (curSceneType.HasFlag(eUIType.Battle_WorldBoss))
            {
                if (worldBossAnim != null)
                {
                    worldBossAnim.gameObject.SetActive(false);
                    UIBattleObjectEndEvent.Send();
                }
            }
            else if (curSceneType.HasFlag(eUIType.Battle_ChampionBattle))
            {
                if (pvpAnim != null)
                {
                    championAnim.gameObject.SetActive(false);
                    UIBattleObjectEndEvent.Send();
                }
            }
        }

        public virtual void BossAnimEnd()
        {
            if (curSceneType.HasFlag(eUIType.Battle_Adventure) | curSceneType.HasFlag(eUIType.Battle_Daily) | curSceneType.HasFlag(eUIType.Battle_WorldBoss))
            {
                if (advBossAnim != null)
                {
                    advBossAnim.gameObject.SetActive(false);
                    UIBattleObjectEndEvent.Send();
                }
            }
        }

        public virtual void OnEvent(AdventureStateEvent eventType)
        {
            Pause = eventType.Pause;
        }

        protected virtual void setAutoAdvObj()
        {
            if (StageManager.AccumCount >= 0 && StageManager.AccumTotalCount >= 0)
            {
                advAutoObj.SetActive(true);
                isAutoAdvMode = true;
                AutoAdvRemainCount = StageManager.AccumCount;
                AutoAdvTotalCount = StageManager.AccumTotalCount;
                advAutoText.text = string.Format("{0}/{1}", AutoAdvTotalCount - AutoAdvRemainCount, AutoAdvTotalCount);
                advAutoAnim.Play("auto_a");
            }
            else
            {
                isAutoAdvMode = false;
                advAutoObj.SetActive(false);
            }
        }

        int AutoAdvRemainCount = -1;
        int AutoAdvTotalCount = -1;
        protected bool isAutoAdvMode = false;
        public void OnClickAutoAdventure()
        {
            if (isAutoAdvMode)
            {   // 자동전투 끄기
                StageManager.ClearAccumData();
                advAutoAnim.Play("auto_off");
                isAutoAdvMode = false;
            }
            else  //자동전투 켜기
            {
                StageManager.InitAccumData(AutoAdvRemainCount, AutoAdvTotalCount);
                advAutoAnim.Play("auto_a");
                isAutoAdvMode = true;
            }
        }
        void ClearStatistic()
        {
            if (statisticLeftParent != null)
            {
                statisticLeftParent.SetActive(false);
                statisticRightParent.SetActive(false);
                foreach (var clone in myStatistics)
                {
                    if (clone != null)
                    {
                        clone.gameObject.SetActive(false);
                        clone.ClearDamage();
                    }
                }
                foreach (var clone in enemyStatistics)
                {
                    if (clone != null)
                    {
                        clone.gameObject.SetActive(false);
                        clone.ClearDamage();
                    }
                }

                myTeam.Clear();
                enemyTeam.Clear();
            }
        }
        void SetStatisticContentType(eUIStatisticsContentType type)
        {
            if (statisticLeftParent != null)
            {
                foreach (var clone in myStatistics)
                {
                    if (clone != null)
                        clone.SetContentType(type);
                }
                foreach (var clone in enemyStatistics)
                {
                    if (clone != null)
                        clone.SetContentType(type);
                }
            }

        }

        public void OnEvent(UIStatisticEvent eventType)
        {
            int id = eventType.CasterID;

            if (eventType.IsCasterEnemy)
            {
                if (enemyTeam.ContainsKey(id))
                {
                    switch (eventType.EventType)
                    {
                        case eUIStatisticsEventType.DamageRecord:
                            enemyTeam[eventType.CasterID].AddDamage(eventType.Damage, eventType.IsSkill);
                            break;
                        case eUIStatisticsEventType.SkillCount:
                            enemyTeam[eventType.CasterID].AddCount(eventType.IsSkill);
                            break;
                        case eUIStatisticsEventType.RecieveRecord:
                            enemyTeam[eventType.CasterID].AddRecieveDmg(eventType.recieveDmg, eventType.isShieldDmg);
                            break;
                        case eUIStatisticsEventType.DeathRecord:
                            enemyTeam[eventType.CasterID].SetDeath(eventType.LifeTime);
                            break;
                    }
                }
            }
            else
            {
                if (myTeam.ContainsKey(id))
                {
                    switch (eventType.EventType)
                    {
                        case eUIStatisticsEventType.DamageRecord:
                            myTeam[eventType.CasterID].AddDamage(eventType.Damage, eventType.IsSkill);
                            break;
                        case eUIStatisticsEventType.SkillCount:
                            myTeam[eventType.CasterID].AddCount(eventType.IsSkill);
                            break;
                        case eUIStatisticsEventType.RecieveRecord:
                            myTeam[eventType.CasterID].AddRecieveDmg(eventType.recieveDmg, eventType.isShieldDmg);
                            break;
                        case eUIStatisticsEventType.DeathRecord:
                            myTeam[eventType.CasterID].SetDeath(eventType.LifeTime);
                            break;
                    }

                }
            }
        }

        public void OnClickDetailStatistics()
        {
            SetAllStatisticBtnOn();
            detailDmgBtnObj.SetButtonSpriteState(false);
            menu1Text.text = StringData.GetStringByStrKey("battle_difficulty_normal");
            menu2Text.text = StringData.GetStringByStrKey("dragon_skill");
            menu1RightText.text = StringData.GetStringByStrKey("battle_difficulty_normal");
            menu2RightText.text = StringData.GetStringByStrKey("dragon_skill");
            foreach (var item in myStatistics)
            {
                item.SetDetailLayer();
            }
            foreach (var item in enemyStatistics)
            {
                item.SetDetailLayer();
            }
            isStatisticHide = true;
        }
        public void OnClickDefaultStatistics()
        {
            SetAllStatisticBtnOn();
            simpleDmgBtnObj.SetButtonSpriteState(false);
            menu1Text.text = StringData.GetStringByStrKey("총공격량");
            menu2Text.text = StringData.GetStringByStrKey("초당피해");
            menu1RightText.text = StringData.GetStringByStrKey("총공격량");
            menu2RightText.text = StringData.GetStringByStrKey("초당피해");
            foreach (var item in myStatistics)
            {
                item.SetDefaultLayer();
            }
            foreach (var item in enemyStatistics)
            {
                item.SetDefaultLayer();
            }
            isStatisticHide = true;
        }

        public void OnClickRecieveStatistics()
        {
            SetAllStatisticBtnOn();
            recieveDmgBtnObj.SetButtonSpriteState(false);
            menu1Text.text = StringData.GetStringByStrKey("battle_statistic_ui_hit_dmg_dmg");
            menu2Text.text = StringData.GetStringByStrKey("battle_statistic_ui_hit_dmg_shield_blocked");
            menu1RightText.text = StringData.GetStringByStrKey("battle_statistic_ui_hit_dmg_dmg");
            menu2RightText.text = StringData.GetStringByStrKey("battle_statistic_ui_hit_dmg_shield_blocked");
            foreach (var item in myStatistics)
            {
                item.SetLifeInfoLayer();
            }
            foreach (var item in enemyStatistics)
            {
                item.SetLifeInfoLayer();
            }
            isStatisticHide = true;
        }
        void SetAllStatisticBtnOn()
        {
            detailDmgBtnObj.SetButtonSpriteState(true);
            simpleDmgBtnObj.SetButtonSpriteState(true);
            recieveDmgBtnObj.SetButtonSpriteState(true);
        }

        public void OnClickHideBtn()
        {
            if (isStatisticHide)
            {
                statisticHideText.text = StringData.GetStringByStrKey("battle_statistic_ui_open");
                detailDmgBtnObj.gameObject.SetActive(false);
                simpleDmgBtnObj.gameObject.SetActive(false);
                recieveDmgBtnObj.gameObject.SetActive(false);
                myStatisticLayer.SetActive(false);
                enemyStatisticLayer.SetActive(false);
                openBtnImgObj.SetActive(false);
                closeBtnImgObj.SetActive(true);
            }
            else
            {
                statisticHideText.text = StringData.GetStringByStrKey("battle_statistic_ui_close");
                detailDmgBtnObj.gameObject.SetActive(true);
                simpleDmgBtnObj.gameObject.SetActive(true);
                recieveDmgBtnObj.gameObject.SetActive(true);
                myStatisticLayer.SetActive(true);
                enemyStatisticLayer.SetActive(true);
                openBtnImgObj.SetActive(true);
                closeBtnImgObj.SetActive(false);
            }
            isStatisticHide = !isStatisticHide;
        }

        void ChampionRoundInit(bool round_end = false)
        {
            championRoundUI.SetActive(true);
            bool practice = LoadingManager.Instance.currentScene == "ChampionPracticeColosseum";

            foreach (var round in ARoundWin)
            {
                round.transform.parent.gameObject.SetActive(!practice);
                round.SetActive(false);
            }
            foreach (var round in BRoundWin)
            {
                round.transform.parent.gameObject.SetActive(!practice);
                round.SetActive(false);
            }

            if (!practice && ChampionManager.Instance.CurMatchData.Detail != null)
            {
                int last_round = ChampionManager.Instance.CurRoundIndex;
                if (round_end)
                    last_round += 1;

                for (int i = 0; i < last_round; i++)
                {
                    eChampionWinType result = eChampionWinType.None;
                    switch (i)
                    {
                        case 0:
                            result = ChampionManager.Instance.CurMatchData.Detail.Round1Result;
                            break;
                        case 1:
                            result = ChampionManager.Instance.CurMatchData.Detail.Round2Result;
                            break;
                        case 2:
                            result = ChampionManager.Instance.CurMatchData.Detail.Round3Result;
                            break;
                    }
                    if (ARoundWin.Length > i && ARoundWin[i] != null)
                    {
                        ARoundWin[i].SetActive(result == eChampionWinType.SIDE_A_WIN);
                    }
                    if (BRoundWin.Length > i && BRoundWin[i] != null)
                    {
                        BRoundWin[i].SetActive(result == eChampionWinType.SIDE_B_WIN);
                    }
                }
            }

            championResult.SetActive(round_end);
            if (round_end)
            {
                int last_round = ChampionManager.Instance.CurRoundIndex;
                string nick = "";
                string text = "";
                if (!practice)
                {
                    eChampionWinType result = eChampionWinType.None;
                    switch (last_round)
                    {
                        case 0:
                            result = ChampionManager.Instance.CurMatchData.Detail.Round1Result;
                            break;
                        case 1:
                            result = ChampionManager.Instance.CurMatchData.Detail.Round2Result;
                            break;
                        case 2:
                            result = ChampionManager.Instance.CurMatchData.Detail.Round3Result;
                            break;
                    }

                    
                    switch (result)
                    {
                        case eChampionWinType.SIDE_A_WIN:
                        case eChampionWinType.UNEARNED_WIN_A:
                            nick = ChampionManager.Instance.CurMatchData.A_SIDE.NICK;
                            break;
                        case eChampionWinType.SIDE_B_WIN:
                        case eChampionWinType.UNEARNED_WIN_B:
                            nick = ChampionManager.Instance.CurMatchData.B_SIDE.NICK;
                            break;
                    }

                    text = StringData.GetStringFormatByStrKey("라운드승리", last_round + 1);
                }
                else
                {
                    switch(ChampionManager.Instance.ChampionData.WinType)
                    {
                        case eChampionWinType.SIDE_A_WIN:
                        case eChampionWinType.UNEARNED_WIN_A:
                            nick = StringData.GetStringByStrKey("left_side");
                            break;
                        case eChampionWinType.SIDE_B_WIN:
                        case eChampionWinType.UNEARNED_WIN_B:
                            nick = StringData.GetStringByStrKey("right_side");
                            break;
                        default:
                            nick = StringData.GetStringByStrKey("무승부");
                            break;
                    }

                    text = StringData.GetStringByStrKey("연습모드종료");
                }

                championResultNick.text = nick;
                championRoundResult.text = text;
            }
        }


        public void OnChampionReplay(int round)
        {
            if (ChampionManager.Instance.Playing)
            {
                ChampionManager.Instance.SetPlay(false);

                PopupManager.OpenPopup<ChampionBattleStatisticPopup>(new ChampionBattleStatisticPopupData(ChampionManager.Instance.ChampionData, () =>
                {
                    ChampionManager.Instance.LogRelease();
                    ChampionManager.Instance.OnRoundStart(round);
                }));
            }
        }
    }

}