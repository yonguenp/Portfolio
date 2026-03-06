using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{

    [Serializable]
    public class TutorialSeqData
    {
        [Header("[Common Info]")]
        public int tutorialSeq = 0;
        public float delay = 0;
        public float waitTime = 0;
        [SerializeField]
        private eObjectPos useWorldCanvas = eObjectPos.UICanvas;
        public eObjectPos UseWorldCanvas
        {
            get { return useWorldCanvas; }
        } 

        [SerializeField]
        protected eTutorialOption option = eTutorialOption.None;
        [SerializeField]
        eFocusTarget focusTarget = eFocusTarget.TargetTransform;
        public eTutorialOption Option { get { return option; } }
        public eFocusTarget FocusTarget { get { return focusTarget; } }

        [Header("[Tutorial Target]")]
        public RectTransform targetTransform = null;        // 지정된 타겟이 있을경우
        [SerializeField] int targetValue = 0;               // 타겟에 관한 값 FocusTarget이 건물이면 건물 tag,
        public int TargetValue { get { return targetValue; } }

        public Vector2 targetPos = Vector2.zero;            // targetPos는 targetTransfrom을 우선함
        public Vector2 targetSize = Vector2.zero;
        public VoidDelegate StartEvent = null;                // 버튼 눌렀을 때 버튼 이벤트 보다 더 빠르게 시작되는 이벤트

        [Header("[Message Box]")]
        public Vector2 messageBoxPos = Vector2.zero;
        [Header("[Arrow Pos]")]
        public Vector2 arrowPos = Vector2.zero;


        [Header("[Deco]")]
        [Range(0, 1)]
        public float dimmedAlpha = 0.58f;
        public eTutorialGuideArrowDir arrowType = eTutorialGuideArrowDir.NONE;

        [Header("[BoxEffect]")]
        public float tweenScale = 1.2f;
        public float tweenTimer = 1f;
    }

    public class TutorialSeqController : MonoBehaviour, EventListener<TutorialEvent>
    {
        [SerializeField]
        int tutorialGroup = 0;
        [SerializeField]
        List<TutorialSeqData> seqDataList = new List<TutorialSeqData>();

        private void OnEnable()
        {
            EventManager.AddListener(this);

            InitController();
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener(this);
        }

        public void OnEvent(TutorialEvent eventType)
        {
            return;
        }

        void InitController()
        {
            foreach (TutorialSeqData seqData in seqDataList)
            {
                if (seqData == null) 
                    continue;

                TutorialManager.Instance.SetSeqDataDic(tutorialGroup, seqData.tutorialSeq, seqData);
            }


            // OLD_VER
            //ClearController();
            
            //tutorialTable = TableManager.GetTable<TutorialTable>();
        }
    }
}