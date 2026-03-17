using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class TranscendenceResultPopup : Popup<TranscendenceResultPopupData>
    {
        [Header("Spine")]
        [SerializeField]
        private UIDragonSpine spine = null;

        [Space(10f)]
        [Header("Text")]
        [SerializeField]
        private Text successTitleText = null;
        [SerializeField]
        private Text failedTitleText = null;
        [SerializeField]
        private Text nameLabel = null;
        [SerializeField]
        private Text botText = null;

        [Space(10f)]
        [Header("Button")]
        [SerializeField]
        private Button dimdBtn = null;

        [Space(10f)]
        [Header("Effect")]
        [SerializeField]
        private GameObject bgEffect = null;
        [SerializeField]
        private GameObject backEffect = null;
        [SerializeField]
        private GameObject frontEffect = null;

        [Space(10f)]
        [Header("Anim")]
        [SerializeField]
        private Animation[] starAnims = null;
        [SerializeField]
        private Image[] starImage = null;
        [SerializeField]
        private GameObject[] starBackObjs = null;
        [SerializeField]
        private float starStartDelay = 0.4f;
        [SerializeField]
        private float starEndDelay = 2f;

        [Space(10f)]
        [Header("ETC")]
        [SerializeField]
        private StatPanel statPanel = null;

        private Coroutine starCoroutine = null;
        private int MaxStep { get; set; } = 0;
        private int Prev { get; set; } = 0;
        private int Next { get; set; } = 0;
        private bool IsSuccess { get => Data.Success; }
        private Tween statTween = null;

        #region OpenPopup
        public static TranscendenceResultPopup OpenPopup(bool success, UserDragon dragon)
        {
            return OpenPopup(new TranscendenceResultPopupData(success, dragon));
        }
        public static TranscendenceResultPopup OpenPopup(TranscendenceResultPopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<TranscendenceResultPopup>(data);
        }
        #endregion
        public override void InitUI()
        {
            if (null == Data || null == Data.Dragon)
            {
                ClosePopup();
                return;
            }
            SetVisibleDimd(false);
            InitializeData();
            InitializeSpine();
            InitializeText();
            InitializeEffect();
            InitializeAnim();
        }
        #region Initialize

        private void SetVisibleDimd(bool isDimd)
        {
            if (dimdBtn == null)
                return;

            dimdBtn.gameObject.SetActive(isDimd);
        }

        /// <summary> 해당 드래곤의 MaxStep 확보 </summary>
        private void InitializeData()
        {
            var grade = (eDragonGrade)Data.Dragon.BaseData.GRADE;
            MaxStep = CharTranscendenceData.GetStepMax(grade);

            var nextData = CharTranscendenceData.Get(grade, Data.Dragon.TranscendenceStep);
            Prev = Data.Dragon.Level;
            if (Data.Dragon.TranscendenceStep > 1)
            {
                var prevData = CharTranscendenceData.Get(grade, Data.Dragon.TranscendenceStep - 1);
                if (prevData != null)
                    Prev += prevData.ADD_STAT;
            }
            if (nextData != null)
                Next = Data.Dragon.Level + nextData.ADD_STAT;
        }
        /// <summary> 스파인 데이터 세팅 </summary>
        private void InitializeSpine()
        {
            if (null == spine)
                return;

            spine.SetData(Data.Dragon);
            spine.InitComplete = InitializeSpineAnim;
            InitializeSpineAnim(spine);
        }
        /// <summary> Text 상태 적용 </summary>
        private void InitializeText()
        {
            if (null != successTitleText)
            {
                successTitleText.text = StringData.GetStringByStrKey("초월성공");
                successTitleText.gameObject.SetActive(IsSuccess);
            }
            if (null != failedTitleText)
            {
                failedTitleText.text = StringData.GetStringByStrKey("초월실패");
                failedTitleText.gameObject.SetActive(false == IsSuccess);
            }
            if (null != nameLabel)
            {
                nameLabel.text = Data.Dragon.Name();
            }
            if (null != botText)
            {
                botText.text = StringData.GetStringByStrKey("터치닫기");
            }
        }
        /// <summary> Effect 켜고 꺼짐 </summary>
        private void InitializeEffect()
        {
            if (null != bgEffect)
                bgEffect.SetActive(IsSuccess);
            if (null != backEffect)
                backEffect.SetActive(IsSuccess);
            if (null != frontEffect)
                frontEffect.SetActive(IsSuccess);
        }
        /// <summary> 스파인 완료 시 적용 </summary>
        private void InitializeSpineAnim(UIDragonSpine data)
        {
            if (IsSuccess)
                data.SetAnimation(eSpineAnimation.WIN);
            else
                data.SetAnimation(eSpineAnimation.LOSE);
        }
        /// <summary> Star Active 상태 변경 및 연출 시작 </summary>
        private void InitializeAnim()
        {
            if (starAnims != null)
            {
                for (int i = 0, count = starAnims.Length; i < count; ++i)
                {
                    if (starAnims[i] == null)
                        continue;

                    starAnims[i].gameObject.SetActive(i < MaxStep);
                    starBackObjs[i].SetActive(i < MaxStep);
                }
            }
            if (starImage != null)
            {
                var step = IsSuccess ? Data.Dragon.TranscendenceStep - 1 : Data.Dragon.TranscendenceStep;
                for (int i = 0, count = starImage.Length; i < count; ++i)
                {
                    if (starImage[i] == null)
                        continue;

                    starImage[i].gameObject.SetActive(i < step);
                    starBackObjs[i].SetActive(i < MaxStep);
                    if (i < step)
                    {
                        starAnims[i][starAnims[i].clip.name].normalizedTime = 1f;
                        starAnims[i].Play();
                    }
                }
            }

            if (IsSuccess)
                starCoroutine = StartCoroutine(StarCoroutine(Data.Dragon.TranscendenceStep - 1));
            else
                SetVisibleDimd(true);
        }
        /// <summary> 해당 스탭에 속하는 별만 연출 </summary>
        IEnumerator StarCoroutine(int startStar)
        {
            if(0 > startStar || null == starAnims || startStar >= starAnims.Length)
            {
                starCoroutine = null;
                yield break;
            }

            yield return SBDefine.GetWaitForSeconds(starStartDelay);
            if (null != starAnims[startStar])
                starAnims[startStar].Play();

            yield return SBDefine.GetWaitForSeconds(starEndDelay);

            if(statPanel != null)
            {
                var baseData = Data.Dragon.BaseData;
                statPanel.Initialize(Data.Dragon.Status.GetTotalINF(), SBFunc.BaseCharStatus(Prev, baseData, StatFactorData.Get(baseData.FACTOR)), SBFunc.BaseCharStatus(Next, baseData, StatFactorData.Get(baseData.FACTOR)));
                var group = statPanel.GetCanvasGroup();
                if (group != null)
                {
                    group.alpha = 0f;
                    statTween = DOTween.To(() => group.alpha, (alpha) => group.alpha = alpha, 1, 0.5f);
                }
            }

            SetVisibleDimd(true);
            starCoroutine = null;

            SendSystemMessage();
        }

        void SendSystemMessage()//유니크 이상 일때 드래곤, 펫에 대한 시스템 메시지
        {
            switch(Data.Dragon.TranscendenceStep)
            {
                case 1:
                    ChatManager.Instance.SendAchieveSystemMessage(eAchieveSystemMessageType.TRANSCENDENCE_STEP1, User.Instance.UserData.UserNick, Data.Dragon.Tag);
                    break;
                case 2:
                    ChatManager.Instance.SendAchieveSystemMessage(eAchieveSystemMessageType.TRANSCENDENCE_STEP2, User.Instance.UserData.UserNick, Data.Dragon.Tag);
                    break;
                case 3:
                    ChatManager.Instance.SendAchieveSystemMessage(eAchieveSystemMessageType.TRANSCENDENCE_STEP3, User.Instance.UserData.UserNick, Data.Dragon.Tag);
                    break;
            }
        }
        #endregion

        /// <summary> 팝업 꺼질 때 초기화 </summary>
        private void OnDisable()
        {
            if(null != starCoroutine)
            {
                StopCoroutine(starCoroutine);
                starCoroutine = null;
            }

            if (starAnims != null)
            {
                for (int i = 0, count = starAnims.Length; i < count; ++i)
                {
                    if (starAnims[i] == null || false == starAnims[i].isPlaying)
                        continue;

                    starAnims[i].Stop();
                }
            }

            if (statPanel != null)
            {
                var group = statPanel.GetCanvasGroup();
                if (group != null)
                    group.alpha = 0f;
            }

            if (statTween != null)
                statTween.Kill();
            statTween = null;
        }
    }
}