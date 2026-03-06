using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork { 
    public class DungeonSelectPopup : Popup<PopupData>
    {
        [SerializeField] private ScrollRect scrollRect = null;
        [SerializeField] private RectTransform content = null;
        [Space(10)]
        [Header("CountLabel")]
        [SerializeField] private Text adventureCountLabel = null;
        [SerializeField] private Text arenaCountLabel = null;
        [SerializeField] private Text dailyDungeonCountLabel = null;
        [SerializeField] private Text worldBossCountLabel = null;
        [Space(10)]
        [Header("TimeLabel")]
        [SerializeField] private Text arenaTimeLabel = null;
        [SerializeField] private Text eventTimeLabel = null;
        [SerializeField] private Text dailyDungeonTimeLabel = null;
        [SerializeField] private Text worldBossTimeLabel = null;
        [SerializeField] private Text championBattleTimeLabel = null;

        [Space(10)]
        [Header("Arena Rank")]
        [SerializeField] private Image arenaRankImg = null;
        [SerializeField] private Text arenaRankText = null;
        [SerializeField] private Image arenaLoading = null;
        [SerializeField] private GameObject arenaPreseason = null;

        [Space(10)]
        [Header("World Boss")]
        [SerializeField] private GameObject worldBossButtonCover = null;
        [SerializeField] private List<GameObject> worldBossConditionObjectList = null;
        [SerializeField] private Text worldBossConditionText = null;

        [Header("HOT TIME ICON")]
        [SerializeField] private HotTimeController adventureHotTimeIcon = null;
        [SerializeField] private HotTimeController worldBossHotTimeIcon = null;
        [SerializeField] private HotTimeController dailyDungeonHotTimeIcon = null;

        [Header("CHAMPION")]
        [SerializeField] private Image championLoading = null;
        [SerializeField] private ChampionSelectPanel championPanel = null;

        private TimeEnable arenaTimeEnable = null;
        private TimeEnable eventTimeEnable = null;
        private TimeEnable dailyTimeEnable = null;
        private TimeEnable worldBossTimeEnable = null;

        bool isDailyDungeonClicked = false;
        bool isArenaData = false;
        bool isDailyData = false;

        public delegate void CallBack();

        private void Awake()
        {
            GameObject championParent = championPanel.transform.parent.gameObject;
            championParent.SetActive(GameConfigTable.IsChampionActive());
        }
        private void Start()
        {
            if (arenaTimeLabel != null)
            {
                arenaTimeEnable = arenaTimeLabel.GetComponent<TimeEnable>();
                if (arenaTimeEnable == null)
                    arenaTimeEnable = arenaTimeLabel.gameObject.AddComponent<TimeEnable>();
            }

            if (eventTimeLabel != null)
            {
                eventTimeEnable = eventTimeLabel.GetComponent<TimeEnable>();
                if (eventTimeEnable == null)
                    eventTimeEnable = eventTimeLabel.gameObject.AddComponent<TimeEnable>();
            }

            if (dailyDungeonTimeLabel != null)
            {
                dailyTimeEnable = dailyDungeonTimeLabel.GetComponent<TimeEnable>();
                if (dailyTimeEnable == null)
                    dailyTimeEnable = dailyDungeonTimeLabel.gameObject.AddComponent<TimeEnable>();
            }

            if (worldBossTimeLabel != null)
            {
                worldBossTimeEnable = worldBossTimeLabel.GetComponent<TimeEnable>();
                if (worldBossTimeEnable == null)
                    worldBossTimeEnable = worldBossTimeLabel.gameObject.AddComponent<TimeEnable>();
            }

            SetArenaRankInfo();

            InitTimeEnable();
            StageManager.ClearAccumData();

            if (GameConfigTable.IsChampionActive())
            {
                championPanel.gameObject.SetActive(false);
                championLoading.gameObject.SetActive(true);
                
                SetLoadingIconAnim(championLoading);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);

            scrollRect.horizontalNormalizedPosition = 0.0f;
        }

        public void SetLoadingIconAnim(Image target)
        {
            target.gameObject.SetActive(true);
            target.DOKill();

            target.color = new Color(Random.value, Random.value, Random.value, 0.0f);

            var seq = DOTween.Sequence();
            seq.Join(target.transform.DORotate(new Vector3(0, 0, 360), 0.75f * 4, RotateMode.FastBeyond360).SetEase(Ease.Linear));

            var coloring = DOTween.Sequence();
            coloring.Append(target.DOColor(new Color(Random.value, Random.value, Random.value), 0.75f));
            coloring.Append(target.DOColor(new Color(Random.value, Random.value, Random.value), 0.75f));
            coloring.Append(target.DOColor(new Color(Random.value, Random.value, Random.value), 0.75f));
            coloring.Append(target.DOColor(new Color(Random.value, Random.value, Random.value), 0.75f));
            seq.Join(coloring);
            seq.SetLoops(-1);
        }

        public override void ClosePopup()
        {
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            isDailyDungeonClicked = false;
            base.ClosePopup();
            championPanel.OnClose();
        }

        public override void InitUI()
        {
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);
            RequestDungeonTicketCount();
            InitWorldBossButtonUI();
            RequestDungeonData();
            RefreshCountLabelUI();
            RefreshHotTimeIcon();
        }

        void InitTimeEnable()
        {
            if (arenaTimeEnable != null)
                arenaTimeEnable.Refresh = RefreshArenaTime;
            if (eventTimeEnable != null)
                eventTimeEnable.Refresh = RefreshEventTime;
            if (dailyTimeEnable != null)
                dailyTimeEnable.Refresh = RefreshDailyTime;
            if (worldBossTimeEnable != null)
                worldBossTimeEnable.Refresh = RefreshWorldBossTime;
        }

        void RefreshCountLabelUI()
        {
            if (adventureCountLabel != null)
                adventureCountLabel.text = StaminaText();
            if (arenaCountLabel != null)
                arenaCountLabel.text = ArenaTicketText();
            if (dailyDungeonCountLabel != null)
                dailyDungeonCountLabel.text = DailyTicketText();
            if (worldBossCountLabel != null)
                worldBossCountLabel.text = WorldBossTicketText();

            if (arenaRankImg != null)
            {
                SetArenaRankInfo();
            }
            bool isPreSeason = ArenaManager.Instance.UserArenaData.season_type == ArenaBaseData.SeasonType.PreSeason;
            if (arenaPreseason != null)
            {
                arenaPreseason.SetActive(isPreSeason);
            }
            if (isPreSeason)
                arenaRankText.text = string.Empty;
        }

        void RefreshHotTimeIcon()
        {
            if (adventureHotTimeIcon != null)
                adventureHotTimeIcon.Refresh();
            if (worldBossHotTimeIcon != null)
                worldBossHotTimeIcon.Refresh();
            if (dailyDungeonHotTimeIcon != null)
                dailyDungeonHotTimeIcon.Refresh();
        }
        void RequestDungeonTicketCount()
        {
            if (isDailyData)
                return;

            RefreshCountLabelUI();
        }
        void RequestDungeonData()
        {
            NetworkManager.Send("user/dungeonstate", null, (jsonData) => {
                ArenaManager.Instance.SetArenaData(jsonData);
                arenaLoading.gameObject.SetActive(false);

                SetDailyInfo(jsonData);
                WorldBossManager.Instance.SetWorldBossProgress(jsonData);

                if (GameConfigTable.IsChampionActive())
                {
                    ChampionManager.Instance.SetChampionData(jsonData, true);
                }

                if (GameConfigTable.IsChampionActive() && ChampionManager.Instance.CurChampionInfo.CurSeason > 0)
                {
                    championLoading.gameObject.SetActive(false);
                    championPanel.gameObject.SetActive(true);
                    championPanel.RefreshUI();
                }
                else
                {
                    GameObject championParent = championPanel.transform.parent.gameObject;
                    championParent.SetActive(false);
                }

                RefreshCountLabelUI();
                //RefreshWorldBossButtonUI();
            },
            (json)=> {
                //기존버전 대응
                //ArenaManager.Instance.ReqArenaData(RefreshCountLabelUI);
            });
        }

        public void InitWorldBossButtonUI()
        {
            var worldBossEnterCondition = WorldBossManager.Instance.IsAvailEnterCondition();

            if (worldBossConditionText != null)
                worldBossConditionText.gameObject.SetActive(!worldBossEnterCondition);

            if (worldBossButtonCover != null)
                worldBossButtonCover.SetActive(!worldBossEnterCondition);

            if (worldBossConditionObjectList != null && worldBossConditionObjectList.Count >= 0)
            {
                foreach(var obj in worldBossConditionObjectList)
                    if(obj != null)
                        obj.SetActive(worldBossEnterCondition);
            }
        }

        public void OnClickAdventureButton()
        {
            LoadingManager.Instance.EffectiveSceneLoad("AdventureStageSelect", eSceneEffectType.CloudAnimation);
            ReleaseTexture();
            PopupManager.ClosePopup<DungeonSelectPopup>();
        }
        public void OnClickArenaButton()
        {
            LoadingManager.Instance.EffectiveSceneLoad("ArenaLobby", eSceneEffectType.CloudAnimation);
            ReleaseTexture();
            PopupManager.ClosePopup<DungeonSelectPopup>();
        }
        public void OnClickDailyDungeonButton()
        {
            if (isDailyDungeonClicked) return;

            isDailyDungeonClicked = true;

            RequestDailyDungeonData(() =>
            {
                LoadingManager.Instance.EffectiveSceneLoad("DailyDungeonLobby", eSceneEffectType.CloudAnimation);
                ReleaseTexture();
                PopupManager.ClosePopup<DungeonSelectPopup>();
            }, () =>
             {
                 isDailyDungeonClicked = false;
             });
        }

        void RequestDailyDungeonData(CallBack SuccessCallBack, CallBack failResponseCallBack = null)
        {
            //데이터 검증 이후 Success 되도록 변경 필요

            NetworkManager.Send("daily/dailyinfo", null, (JObject jsonData) =>
            {
                if (jsonData["err"] != null && jsonData["rs"] != null && (int)jsonData["err"] == 0 && (int)jsonData["rs"] == 0)
                {
                    SetDailyInfo(jsonData);
                    SuccessCallBack?.Invoke();
                }
                else
                {
                    failResponseCallBack?.Invoke();
                }
            }, (string err) =>
            {
                Debug.Log(err);
                failResponseCallBack?.Invoke();
            });
        }

        public void SetDailyInfo(JObject jsonData)
        {
            if(jsonData.ContainsKey("daily_info"))
            {
                var infoData = jsonData["daily_info"];
                if (infoData != null)
                    StageManager.Instance.DailyDungeonProgressData.SetDailyInfoData((JObject)infoData);
            }
            
            if(jsonData.ContainsKey("daily_log"))
            {
                var logData = jsonData["daily_log"];
                if (logData != null && SBFunc.IsJArray(logData))
                    StageManager.Instance.SetDailyDungeonProgress((JArray)logData);
            }

            RefreshCountLabelUI();
            isDailyData = true;
        }

        public void OnClickRaidButton()
        {
            
            if(!WorldBossManager.Instance.IsAvailEnterCondition())
                return;

            //임시
            LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.CloudAnimation);
            ReleaseTexture();
            PopupManager.ClosePopup<DungeonSelectPopup>();
        }

        // Start is called before the first frame update
        private string StaminaText()
        {
            int energyMax;
            var data = AccountData.GetLevel(User.Instance.UserData.Level);
            if (data == null)
                energyMax = 15;
            else
                energyMax = data.MAX_STAMINA;

            var energy = User.Instance.ENERGY;
            if(energy == 0)
                return string.Format("<color=#666666>{0}</color>/{1}", energy, energyMax);

            return string.Format("<color=#FFFF00>{0}</color>/{1}", energy, energyMax);
        }
        private string ArenaTicketText()
        {
            var ticketMax = GameConfigTable.GetArenaUserMaxTicketCount();

            var ticket = ArenaManager.Instance.UserArenaData.Arena_Ticket;
            if (ticket == 0)
                return string.Format("<color=#666666>{0}</color>/{1}", ticket, ticketMax);

            return string.Format("<color=#FFFF00>{0}</color>/{1}", ticket, ticketMax);
        }

        private void SetArenaRankInfo()
        {
            if (ArenaManager.Instance.UserArenaData.season_type == ArenaBaseData.SeasonType.Unkwown)
            {
                arenaRankImg.gameObject.SetActive(false);
                arenaPreseason.SetActive(false);
                arenaRankText.text = string.Empty;
                arenaLoading.gameObject.SetActive(true);
                SetLoadingIconAnim(arenaLoading);
            }
            else
            {
                var currentUserGrade = ArenaManager.Instance.UserArenaData.SeasonGrade;
                var currentRankData = ArenaRankData.GetFirstInGroup((int)currentUserGrade);
                ArenaManager.Instance.battleInfoTabIdx = 1;
                if (currentRankData == null)
                    return;
                string nameKey = currentRankData._NAME;
                arenaRankImg.gameObject.SetActive(true);
                arenaRankImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, currentRankData.ICON);
                if (arenaRankImg.sprite == null)
                {
                    arenaRankImg.gameObject.SetActive(false);
                }
                arenaRankText.text = StringData.GetStringByStrKey(nameKey);
            }
        }
        private string DailyTicketText()
        {
            int stringIndex = 100002624;
            int maxCount = GameConfigTable.GetConfigIntValue("DAY_DUNGEON_CLEAR_COUNT");
            int count = maxCount - StageManager.Instance.DailyDungeonProgressData.DailyDungeonTicketCount;
            if(count <= 0)//광고 입장 시
            {
                stringIndex = 100002624; 
                var adInfo = AdvertisementData.Get(SBDefine.AD_DAILY_KEY);
                if (adInfo != null)
                    maxCount = adInfo.LIMIT;
                else
                    maxCount = GameConfigTable.GetConfigIntValue("DAY_DUNGEON_CHARGE_COUNT");
                count = maxCount + count;
            }

            if (count <= 0)
                return SBFunc.StrBuilder(StringData.GetStringByIndex(stringIndex), string.Format(" <color=#666666>{0}/{1}</color>", 0, maxCount));

            return SBFunc.StrBuilder(StringData.GetStringByIndex(stringIndex), string.Format(" <color=#FFFF00>{0}/{1}</color>", count, maxCount));
        }

        private string WorldBossTicketText()
        {
            int stringIndex = 100002624;
            int maxCount = WorldBossManager.Instance.WorldBossEnterCount;
            int count = maxCount - WorldBossManager.Instance.WorldBossProgressData.WorldBossPlayCount;
            if (count <= 0)//광고 입장 시
            {
                stringIndex = 100002624;
                var adInfo = AdvertisementData.Get(SBDefine.AD_RAID_BOSS_KEY);
                if (adInfo != null)
                    maxCount = adInfo.LIMIT;
                count = maxCount + count;
            }

            if (count <= 0)
                return SBFunc.StrBuilder(StringData.GetStringByIndex(stringIndex), string.Format(" <color=#666666>{0}/{1}</color>", 0, maxCount));

            return SBFunc.StrBuilder(StringData.GetStringByIndex(stringIndex), string.Format(" <color=#FFFF00>{0}/{1}</color>", count, maxCount));
        }

        private void RefreshArenaTime()
        {
            if (arenaTimeLabel == null)
                return;

            int time = TimeManager.GetTimeCompare(ArenaManager.Instance.UserArenaData.season_remain_time);
            if (time > 0)
                arenaTimeLabel.text = TimeText(time);
            else
                arenaTimeLabel.text = TimeText(0);
        }
        private void RefreshEventTime()
        {
            if (eventTimeLabel == null)
                return;

            int time = 0;
            if (time > 0)
                eventTimeLabel.text = TimeText(time);
            else
                eventTimeLabel.text = TimeText(0);
        }
        private void RefreshDailyTime()
        {
            if (dailyDungeonTimeLabel == null)
                return;

            int time = TimeManager.GetContentResetTime();
            if (time > 0)
                dailyDungeonTimeLabel.text = TimeText(time);
            else
                dailyDungeonTimeLabel.text = TimeText(0);
        }
        private void RefreshWorldBossTime()
        {
            if (worldBossTimeLabel == null)
                return;

            int time = TimeManager.GetContentResetTime();
            if (time > 0)
                worldBossTimeLabel.text = TimeText(time);
            else
                worldBossTimeLabel.text = TimeText(0);
        }

        private string TimeText(int time)
        {
            return SBFunc.TimeString(time);
        }
        protected override IEnumerator OpenAnimation()
        {
            InitialzeTexture();
            yield return base.OpenAnimation();
        }
        protected override IEnumerator CloseAnimation()
        {
            if (popupActionAnim == null)
            {
                popupActionAnim = GetComponent<Animator>();
            }

            if (popupActionAnim != null)
            {
                popupActionAnim.Play("PopupClose", 0);
                yield return new WaitUntil(() => popupActionAnim.GetCurrentAnimatorStateInfo(0).IsName("PopupClose"));
                yield return new WaitUntil(() => popupActionAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
            }
            ReleaseTexture();
            SetActive(false);
            yield break;
        }
        private void InitialzeTexture()
        {
            UICanvas.Instance.StartBackgroundBlurEffect();
        }
        private void ReleaseTexture()
        {
            UICanvas.Instance.EndBackgroundBlurEffect();
        }
    }
}