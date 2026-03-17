using Coffee.UIEffects;
using Google.Impl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SandboxNetwork
{
    public class MatchInfoPopup : Popup<PopupBase>
    {
        [SerializeField]
        private ChampionBattleInfoSlot left = null;
        [SerializeField]
        private ChampionBattleInfoSlot right = null;

        [Header("TopBar")]
        [SerializeField]
        private Text titleLabel = null;
        [SerializeField]
        private GameObject expectedDividends = null;
        [SerializeField]
        private Text expectedDividendsLabel = null;

        [Header("Slider")]
        [SerializeField]
        private GameObject progressBar = null;
        [SerializeField]
        private Text totalBetLabel = null;
        [SerializeField]
        private Slider slider = null;
        [SerializeField]
        private Image fillA = null;   
        [SerializeField]
        private Image fillB = null;
        [SerializeField]
        private Text ratioA = null;
        [SerializeField]
        private Text ratioB = null;

        [SerializeField]
        private GameObject domesticReward = null;
        [SerializeField]
        private Text domesticRewardAmount = null;
        [SerializeField]
        private GameObject domesticRewardItem = null;
        [SerializeField] 
        private GameObject vsImage = null;

        [Header("Cheer Button")]
        [SerializeField]
        Sprite activeSprite;
        [SerializeField]
        Sprite inactiveSprite;

        [SerializeField]
        private List<Button> voteBtns = new List<Button>();
        [SerializeField]
        private List<Text> voteBtnsLabels = new List<Text>();
        [SerializeField]
        private GameObject replayBtn = null;
        [SerializeField]
        private GameObject resultBtn = null;
        [SerializeField]
        private Text bottomNotice = null;

        [SerializeField]
        TimeObject timeObject = null;

        [Header("Battle Anim")]
        [SerializeField] private RectTransform portraitA = null;
        [SerializeField] private RectTransform portraitB = null;
        [SerializeField] private RectTransform dimdA = null;
        [SerializeField] private RectTransform dimdB = null;
        [SerializeField] private RectTransform winA = null;
        [SerializeField] private RectTransform winB = null;


        [SerializeField]
        private Text bottomNotice_inter = null;
        [SerializeField]
        private Text bottomNotice_dome = null;

        private Sequence battleSequence;

        private Vector3 originalPos1;
        private Vector3 originalPos2;
        private int originalSiblingIndex1;
        private int originalSiblingIndex2;
        private float moveDuration = 0.3f;  // 중앙으로 이동 속도
        private float scaleFactor = 1.5f;   // 최대 확대 비율
        private float pushBackFactor = 0.15f; // PushBack 비율
        private float impactDuration = 0.4f; // 충돌 지속 시간 
        private float pushBackScale = 1.2f; // PushBack 이후 중앙으로 이동할 때 커지는 크기
        private float shakeStrength = 30f; // 충돌 시 진동 강도 
        private int shakeVibrato = 15; //충돌 시 진동 빈도 

        private Vector3 originalVsScale;
        private Vector3 originalWinIconScale;

        private RectTransform winIcon;
        private RectTransform dimdBG;

        private string[] RewardDataArr = null;

        private bool originalPosSaved = false;

        public delegate bool TimerCallback();
        List<TimerCallback> TimeCallback = new List<TimerCallback>();
        private bool IsMatchOver 
        {
            get
            {
                if (curData != null)
                    return curData.WINNER != null || curData.round < ChampionManager.Instance.CurChampionInfo.CurState;

                return false;
            }
        }

        ChampionLeagueTable.ROUND_INDEX curIndex = ChampionLeagueTable.ROUND_INDEX.NONE;
        ChampionMatchData curData { get { return ChampionManager.Instance.CurChampionInfo.GetMatchData(curIndex); } }

        void Start()
        {
            //originalPos1 = portraitA.position;
            //originalPos2 = portraitB.position;
            //originalVsScale = vsImage.transform.localScale;
            //originalWinIconScale = winA.localScale;
            ////vsImage.transform.localScale *= 0f;

            //originalSiblingIndex1 = portraitA.GetSiblingIndex();
            //originalSiblingIndex2 = portraitB.GetSiblingIndex();
        }

        public void PlayBattleAnim()
        {
            if (ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].MatchResult == eChampionWinType.UNEARNED_WIN_A 
                || ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].MatchResult == eChampionWinType.SIDE_A_WIN)
            {
                winIcon = winA;
                dimdBG = dimdB;
            }
            else if (ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].MatchResult == eChampionWinType.UNEARNED_WIN_B
                || ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].MatchResult == eChampionWinType.SIDE_B_WIN)
            {
                winIcon = winB;
                dimdBG = dimdA;
            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("챔피언결과지연"));
                return;
            }

            winIcon.gameObject.SetActive(true);
            winIcon.localScale = Vector3.zero;

            vsImage.SetActive(true);
            vsImage.transform.localScale = Vector3.zero;

            float midX = (originalPos1.x + originalPos2.x) / 2; 
            float pushBackDistance1 = (originalPos1.x) * pushBackFactor;
            float pushBackDistance2 = (originalPos2.x) * pushBackFactor;

            portraitA.SetAsLastSibling();
            portraitB.SetAsLastSibling();

            if (battleSequence != null && battleSequence.IsActive())
            {
                battleSequence.Kill(true);
            }

            battleSequence = DOTween.Sequence();
            battleSequence.Append(vsImage.transform.DOScale(originalVsScale * 2f, 0.15f).SetEase(Ease.OutBack)) // 빠르게 커짐
            .Append(vsImage.transform.DOScale(originalVsScale, 0.1f).SetEase(Ease.InBack)) // 작아짐
            .Join(vsImage.transform.DOShakePosition(0.15f, strength: new Vector3(10f, 10f, 0), vibrato: 15)); // 살짝 흔들림

            battleSequence.Append(portraitA.DOScale(pushBackScale, 0.2f));
            battleSequence.Join(portraitB.DOScale(pushBackScale, 0.2f));

            battleSequence.Append(portraitA.DOLocalMoveX(originalPos1.x + pushBackDistance1, 0.2f).SetEase(Ease.OutQuad));
            battleSequence.Join(portraitB.DOLocalMoveX(originalPos2.x + pushBackDistance2, 0.2f).SetEase(Ease.OutQuad));

            battleSequence.Append(portraitA.DOLocalMoveX(originalPos1.x * (-1.1f), moveDuration).SetEase(Ease.InExpo));
            battleSequence.Join(portraitB.DOLocalMoveX(originalPos2.x * (-1.1f), moveDuration).SetEase(Ease.InExpo));
            battleSequence.Join(portraitA.DOScale(scaleFactor, moveDuration));  //이동하면서 커짐
            battleSequence.Join(portraitB.DOScale(scaleFactor, moveDuration));


            battleSequence.Append(portraitA.DOShakePosition(impactDuration, strength: new Vector3(shakeStrength, 0, 0), vibrato: shakeVibrato));
            battleSequence.Join(portraitB.DOShakePosition(impactDuration, strength: new Vector3(shakeStrength, 0, 0), vibrato: shakeVibrato));

            battleSequence.Append(portraitA.DOLocalMove(originalPos1, moveDuration).SetEase(Ease.OutQuad));
            battleSequence.Join(portraitB.DOLocalMove(originalPos2, moveDuration).SetEase(Ease.OutQuad));
            battleSequence.Join(portraitA.DOScale(1.2f, moveDuration));  // 
            battleSequence.Join(portraitB.DOScale(1.2f, moveDuration));


            battleSequence.AppendCallback(() =>
            {
                portraitA.SetSiblingIndex(originalSiblingIndex1);
                portraitB.SetSiblingIndex(originalSiblingIndex2);
            });


            battleSequence.Append(winIcon.gameObject.transform.DOScale(originalWinIconScale * 2f, 0.15f).SetEase(Ease.OutBack)) //  커짐
            .Append(winIcon.gameObject.transform.DOScale(originalWinIconScale, 0.1f).SetEase(Ease.InBack)) // 작아짐
            .Join(winIcon.gameObject.transform.DOShakePosition(0.15f, strength: new Vector3(10f, 10f, 0), vibrato: 15)); // 살짝 흔들림


            battleSequence.OnComplete(() =>
            {
                curData.ShowResult();
                RefreshUI();
            });

        }

        public static void OpenPopup(ChampionLeagueTable.ROUND_INDEX roundindex)
        {
            ChampionInfo.ROUND_STATE round = ChampionInfo.ROUND_STATE.NONE;
            int slot = -1;
            switch (roundindex)
            {
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_A:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_B:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_C:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_D:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_E:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_F:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_G:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_H:
                    round = ChampionInfo.ROUND_STATE.ROUND_OF_16;
                    slot = (int)(roundindex - ChampionLeagueTable.ROUND_INDEX.ROUND16_START);
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_A:
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_B:
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_C:
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_D:
                    round = ChampionInfo.ROUND_STATE.QUARTER_FINALS;
                    slot = (int)(roundindex - ChampionLeagueTable.ROUND_INDEX.ROUND8_START);
                    break;
                case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A:
                case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B:
                    round = ChampionInfo.ROUND_STATE.SEMI_FINALS;
                    slot = (int)(roundindex - ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_START);
                    break;
                case ChampionLeagueTable.ROUND_INDEX.FINAL:
                    round = ChampionInfo.ROUND_STATE.FINAL;
                    slot = (int)(roundindex - ChampionLeagueTable.ROUND_INDEX.FINAL_START);
                    break;
            }

            OpenPopup(round, slot);
        }

        public static void OpenPopup(ChampionInfo.ROUND_STATE round, int slot)
        {
            ChampionManager.Instance.CurChampionInfo.ReqMatchInfo(round, slot, () => {
                var popup = PopupManager.OpenPopup<MatchInfoPopup>();
                var round_index = ChampionManager.Instance.CurChampionInfo.GetRoundIndex(round, slot);
                popup.SetData(round_index);
            });
        }

        public static void OpenPopup()
        {
            var popup = PopupManager.OpenPopup<MatchInfoPopup>();
            popup.RefreshUI();
        }

        public override void InitUI()
        {
            //ReFreshUI();
        }

        public void SetData(ChampionLeagueTable.ROUND_INDEX index)
        {
            curIndex = index;
            string rewardData = GameConfigTable.GetCheerRewardWithRound(GetCurRound());
            RewardDataArr = rewardData.Split(',');
            RefreshUI();
        }

        public void AddTimer(TimerCallback cb)
        {
            TimeCallback.Add(cb);
        }

        public void RefreshUI()
        {
            SetTimer();

            if (left != null)
            {
                left.Init(true, curData, this);
            }

            if (right != null)
            { 
                right.Init(false, curData, this);
            }

            if (vsImage != null)
            {
                vsImage.SetActive(IsMatchOver);
            }
            SetPortrait();
            SetTitle();
            SetBotUI();
            SetBtnState();
        }

        public void SetPortrait()
        {
            if (portraitA == null || portraitB == null) return;

            if (!originalPosSaved)
            {
                originalPos1 = portraitA.localPosition;
                originalPos2 = portraitB.localPosition;
                originalVsScale = vsImage.transform.localScale;
                originalWinIconScale = winA.localScale;
                //vsImage.transform.localScale *= 0f;

                originalSiblingIndex1 = portraitA.GetSiblingIndex();
                originalSiblingIndex2 = portraitB.GetSiblingIndex();

                originalPosSaved = true;
            }

            portraitA.localPosition = originalPos1;
            portraitB.localPosition = originalPos2;
            vsImage.transform.localScale = originalVsScale;
            winA.localScale = originalWinIconScale;

            portraitA.SetSiblingIndex(originalSiblingIndex1);
            portraitB.SetSiblingIndex(originalSiblingIndex2);
        }

        public void SetTimer()
        {
            TimeCallback.Clear();

            timeObject.Refresh = () => {
                bool refresh = false;

                foreach(var cb in TimeCallback)
                {
                    if (cb.Invoke())
                        refresh = true;
                }

                if (refresh)
                {
                    TimeCallback.Clear();

                    ChampionInfo.ROUND_STATE round = ChampionInfo.ROUND_STATE.NONE;
                    int slot = -1;
                    switch (curIndex)
                    {
                        case ChampionLeagueTable.ROUND_INDEX.ROUND16_A:
                        case ChampionLeagueTable.ROUND_INDEX.ROUND16_B:
                        case ChampionLeagueTable.ROUND_INDEX.ROUND16_C:
                        case ChampionLeagueTable.ROUND_INDEX.ROUND16_D:
                        case ChampionLeagueTable.ROUND_INDEX.ROUND16_E:
                        case ChampionLeagueTable.ROUND_INDEX.ROUND16_F:
                        case ChampionLeagueTable.ROUND_INDEX.ROUND16_G:
                        case ChampionLeagueTable.ROUND_INDEX.ROUND16_H:
                            round = ChampionInfo.ROUND_STATE.ROUND_OF_16;
                            slot = (int)(curIndex - ChampionLeagueTable.ROUND_INDEX.ROUND16_START);
                            break;
                        case ChampionLeagueTable.ROUND_INDEX.ROUND8_A:
                        case ChampionLeagueTable.ROUND_INDEX.ROUND8_B:
                        case ChampionLeagueTable.ROUND_INDEX.ROUND8_C:
                        case ChampionLeagueTable.ROUND_INDEX.ROUND8_D:
                            round = ChampionInfo.ROUND_STATE.QUARTER_FINALS;
                            slot = (int)(curIndex - ChampionLeagueTable.ROUND_INDEX.ROUND8_START);
                            break;
                        case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A:
                        case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B:
                            round = ChampionInfo.ROUND_STATE.SEMI_FINALS;
                            slot = (int)(curIndex - ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_START);
                            break;
                        case ChampionLeagueTable.ROUND_INDEX.FINAL:
                            round = ChampionInfo.ROUND_STATE.FINAL;
                            slot = (int)(curIndex - ChampionLeagueTable.ROUND_INDEX.FINAL_START);
                            break;
                    }

                    ChampionManager.Instance.CurChampionInfo.ReqMatchInfo(round, slot, () => {
                        SetData(curIndex);
                    });
                }
            };
        }

        void SetTitle()
        {
            if (titleLabel != null)
            {
                if (!IsMatchOver)
                {
                    titleLabel.text = StringData.GetStringFormatByStrKey("경기시작전", GetIndexString());
                }
                else if(!ChampionMatchData.IsShowResult(curIndex))
                {
                    titleLabel.text = StringData.GetStringFormatByStrKey("경기결과확인", GetIndexString());
                }
                else
                {
                    titleLabel.text = StringData.GetStringFormatByStrKey("경기결과", GetIndexString());
                }
            }
        }

        string GetIndexString()
        {
            string indexString = "";
            switch (curIndex)
            {
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_A:
                    indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 1));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_B:
                    indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 2));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_C:
                    indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 3));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_D:
                    indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 4));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_E:
                    indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 5));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_F:
                    indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 6));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_G:
                    indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 7));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_H:
                    indexString = StringData.GetStringFormatByStrKey("16강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 8));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_A:
                    indexString = StringData.GetStringFormatByStrKey("8강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 1));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_B:
                    indexString = StringData.GetStringFormatByStrKey("8강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 2));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_C:
                    indexString = StringData.GetStringFormatByStrKey("8강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 3));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_D:
                    indexString = StringData.GetStringFormatByStrKey("8강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 4));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A:
                    indexString = StringData.GetStringFormatByStrKey("4강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 1));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B:
                    indexString = StringData.GetStringFormatByStrKey("4강인덱스", StringData.GetStringFormatByStrKey("경기인덱스", 2));
                    break;
                case ChampionLeagueTable.ROUND_INDEX.FINAL:
                    indexString = StringData.GetStringByStrKey("결승전");
                    break;
            }

            return indexString;
        }

        string GetCurRound()
        {
            string indexString = "";
            switch (curIndex)
            {
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_A:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_B:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_C:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_D:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_E:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_F:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_G:
                case ChampionLeagueTable.ROUND_INDEX.ROUND16_H:
                    indexString = ((int)ChampionLeagueTable.ROUND_INDEX.ROUND16_START).ToString();
                    break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_A:
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_B:
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_C:
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_D:
                    indexString = ((int)ChampionLeagueTable.ROUND_INDEX.ROUND8_START).ToString();
                    break;
                case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A:
                case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B:
                    indexString = ((int)ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_START).ToString();
                    break;
                case ChampionLeagueTable.ROUND_INDEX.FINAL:
                    indexString = ((int)ChampionLeagueTable.ROUND_INDEX.FINAL_START).ToString();
                    break;
            }
            return indexString;
        }

        public void SetBotUI()
        {
            if (bottomNotice_inter != null && bottomNotice_dome != null)
            {
                bottomNotice_inter.gameObject.SetActive(User.Instance.ENABLE_P2E);
                bottomNotice_dome.gameObject.SetActive(!User.Instance.ENABLE_P2E);
            }
                

            if (IsMatchOver)
            {
                if (progressBar != null && domesticReward != null)
                {
                    progressBar.SetActive(false);
                    domesticReward.SetActive(false);
                }

                if (curData.Detail.EXPECTED_DIVIDEND <= 0 || curData.Detail.BET_TYPE != curData.MatchResult || curData.Result_Type == eChampionWinType.UNEARNED_WIN_A || curData.Result_Type == eChampionWinType.UNEARNED_WIN_B || !ChampionMatchData.IsShowResult(curIndex))
                {
                    expectedDividends.SetActive(false);
                    return;
                }

                expectedDividends.SetActive(User.Instance.ENABLE_P2E);
                if (User.Instance.ENABLE_P2E && expectedDividendsLabel != null)
                {
                    expectedDividendsLabel.text = (Math.Floor(curData.Detail.EXPECTED_DIVIDEND * 100) / 100).ToString("N2");
                }
            }
            else
            {
                if (progressBar != null && domesticReward != null)
                {
                    expectedDividends.SetActive(false);
                    progressBar.SetActive(User.Instance.ENABLE_P2E);
                    domesticReward.SetActive(!User.Instance.ENABLE_P2E);
                    domesticRewardAmount.text = ""+ RewardDataArr[2];
                    domesticRewardItem.GetComponent<Image>().sprite = ItemBaseData.Get(RewardDataArr[1]).ICON_SPRITE;
                }
                if (slider != null && fillA != null && fillB != null && ratioA != null && ratioB != null && totalBetLabel != null)
                {
                    int totalBet = curData.Detail.TOTAL_BET;
                    totalBetLabel.text = SBFunc.CommaFromNumber(totalBet);

                    float SideABet = curData.Detail.SIDE_A_BET;
                    float SideBBet = curData.Detail.SIDE_B_BET;

                    if (totalBet <= 0)
                    {
                        slider.value = 0.5f;
                        
                        fillA.fillAmount = slider.value;
                        fillB.fillAmount = 1 - slider.value;

                        fillB.rectTransform.anchorMin = new Vector2(slider.value, 0);
                        fillB.rectTransform.anchorMax = new Vector2(1, 1);

                        ratioA.text = 0 + "%";
                        ratioB.text = 0 + "%";
                    }
                    else
                    {
                        const float min_limit = 0.01f;
                        const float max_limit = 0.99f;

                        slider.value = Mathf.Max(Mathf.Min(SideABet / totalBet, max_limit), min_limit);

                        fillA.fillAmount = slider.value;
                        fillB.fillAmount = 1 - slider.value;

                        fillB.rectTransform.anchorMin = new Vector2(slider.value, 0);
                        fillB.rectTransform.anchorMax = new Vector2(1, 1);

                        ratioA.text = Math.Round(fillA.fillAmount * 100) + "%";
                        ratioB.text = Math.Round(fillB.fillAmount * 100) + "%";
                    }
                }
            }
        }

        public void SetBtnState()
        {
            bool IsInvalid = false;
            bool IsForfeit = false;
            bool IsNotyetBet = ChampionManager.Instance.CurChampionInfo.CurState <= ChampionInfo.ROUND_STATE.PREPARATION;
            if (voteBtns != null && replayBtn != null && bottomNotice != null)
            {
                bottomNotice.gameObject.SetActive(false);
                bottomNotice.text = "";
                if (ChampionManager.Instance.CurChampionInfo.MatchData.ContainsKey(curIndex))
                {
                    if (ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].MatchResult == eChampionWinType.INVALIDITY)
                    {
                        //양쪽에 유저가 있지만 둘다 세팅을 안함.
                        IsInvalid = true;
                        IsForfeit = true;

                        
                        if (IsMatchOver && ChampionMatchData.IsShowResult(curIndex))
                        {
                            bottomNotice.gameObject.SetActive(true);
                            if (!User.Instance.ENABLE_P2E)
                                bottomNotice.text = StringData.GetStringFormatByStrKey("덱세팅미완료VS덱세팅미완료_국내");
                            else
                                bottomNotice.text = StringData.GetStringFormatByStrKey("덱세팅미완료VS덱세팅미완료");
                        }
                    }
                    else if (ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].A_SIDE == null || ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].B_SIDE == null)
                    {
                        //유저가 어느 한쪽이라도 없는 경우
                        IsInvalid = true;
                        if (ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].A_SIDE != null || ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].B_SIDE != null)
                        {
                            if (IsMatchOver && ChampionMatchData.IsShowResult(curIndex))
                            {
                                bottomNotice.gameObject.SetActive(true);
                                bottomNotice.text = StringData.GetStringFormatByStrKey("덱세팅완료VS유저없음"); //"덱세팅미완료VS유저없음" 도 동일한 string
                            }
                        }
                    }
                    else if (ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].Result_Type >= eChampionWinType.INVALIDITY)
                    {
                        //양쪽에 유저가 있지만 어느한쪽이 세팅이 안되었음.

                        IsInvalid = true;

                        if (IsMatchOver && ChampionMatchData.IsShowResult(curIndex))
                        {
                            bottomNotice.gameObject.SetActive(true);
                            string WinnerNick = "";
                            string LoserNick = "";
                            if (ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].Result_Type == eChampionWinType.UNEARNED_WIN_A)
                            {
                                WinnerNick = ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].A_SIDE.NICK;
                                LoserNick = ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].B_SIDE.NICK;

                            }
                            else if (ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].Result_Type == eChampionWinType.UNEARNED_WIN_B)
                            {
                                WinnerNick = ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].B_SIDE.NICK;
                                LoserNick = ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].A_SIDE.NICK;
                            }
                            bottomNotice.text = StringData.GetStringFormatByStrKey("덱세팅완료VS덱세팅미완료", LoserNick, WinnerNick);
                        }

                    }
                }

                replayBtn.SetActive(IsMatchOver && !bottomNotice.gameObject.activeInHierarchy && !IsInvalid);

                bool resultOpenBtn = IsMatchOver && !IsForfeit;
                if (resultOpenBtn)
                {
                    if(!ChampionMatchData.IsShowResult(curIndex))
                        resultOpenBtn = true;
                    else
                        resultOpenBtn = false;
                }

                resultBtn.SetActive(resultOpenBtn);
            }

            foreach (var element in voteBtns)
            {
                element.gameObject.SetActive(!IsMatchOver && !IsNotyetBet);
            }

            if (voteBtnsLabels != null)
            {
                foreach (var element in voteBtnsLabels)
                {
                    element.text = User.Instance.ENABLE_P2E ? StringData.GetStringByStrKey("응원하기") : StringData.GetStringByStrKey("승부예측");
                }
            }

            if (!IsMatchOver)
            {
                bool myMatch = false;
                long curUserNo = User.Instance.UserAccountData.UserNumber;
                if (curData != null)
                {
                    myMatch = curData.luser_no == curUserNo || curData.ruser_no == curUserNo;
                    
                    if (!IsInvalid)
                    {
                        if(!curData.IsBetTime())
                        {
                            IsInvalid = true;
                        }
                    }
                }

                if (IsInvalid)
                {
                    voteBtns[0].interactable = false;
                    voteBtns[1].interactable = false;
                    SetVoteImage();
                    return;
                }


                switch (curData.Detail.BET_TYPE)
                {
                    case eChampionWinType.SIDE_A_WIN:
                        voteBtns[0].interactable = User.Instance.ENABLE_P2E && !myMatch;
                        voteBtns[1].interactable = false;
                        break;
                    case eChampionWinType.SIDE_B_WIN:
                        voteBtns[0].interactable = false;
                        voteBtns[1].interactable = User.Instance.ENABLE_P2E && !myMatch;
                        break;
                    default:
                        voteBtns[0].interactable = !myMatch;
                        voteBtns[1].interactable = !myMatch;
                        break;
                }

                SetVoteImage();
            }            
        }
        void SetVoteImage()
        {
            foreach(var btn in voteBtns)
            {
                var image = btn.GetComponent<Image>();
                if(image)
                {
                    image.sprite = btn.interactable ? activeSprite : inactiveSprite;
                }
            }
        }
        public void OnItemTooltip()
        {
            if(RewardDataArr != null && RewardDataArr.Length > 0 && domesticRewardItem != null)
                ItemToolTip.OnItemToolTip(int.Parse(RewardDataArr[1]), domesticRewardItem);
        }

        public override void ClosePopup()
        {
            if (battleSequence != null && battleSequence.IsActive())
            {
                battleSequence.Kill(true);
            }
            base.ClosePopup();
        }
        public void OnClickCheer(int index)
        {
            ParticipantData data = null;
            switch (index)
            {
                case 0:
                    data = curData.A_SIDE;
                    break;
                case 1:
                    data = curData.B_SIDE;
                    break;
            }

            if (data == null)
                return;

            if (!User.Instance.ENABLE_P2E)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("승부예측투표_국내", data.NICK), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"),
                () => {
                    ChampionManager.Instance.CurChampionInfo.ReqBet(curData, data, 0,
                    () => {
                        var popup = PopupManager.GetPopup<MatchInfoPopup>();
                        if (popup != null)
                        {
                            popup.RefreshUI();
                        }
                    });
                },
                () => { }
                );
            }
            else
            {
                CoinBetPopup.OpenPopup(curData, data);
            }
        }

        public void OnClickReplayBtn()
        {
            if (ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].Result_Type >= eChampionWinType.INVALIDITY)
            {
                PlayBattleAnim();
                return;
            }

            //특정 경기 index로 플레이 시키기
            ChampionManager.Instance.OnReplayStart(curIndex);
        }

        public void OnClickResultOpen()
        {
            PlayBattleAnim();            
        }

    }
}

