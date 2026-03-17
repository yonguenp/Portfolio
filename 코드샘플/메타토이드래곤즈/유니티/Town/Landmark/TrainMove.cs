using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct UITrainStateEvent  // 기차 상태에 따라 역무원 말풍선 효과 이벤트
    {
        public enum EventType { 
            TrainStateUpdate,
            PopupClose,
        }

        static UITrainStateEvent e;
        public EventType eventType;
        public static bool deliveryStart = false;

        static public void Event(bool deliver = false)
        {
            if(deliver)
                deliveryStart = true;

            e.eventType = EventType.TrainStateUpdate;
            EventManager.TriggerEvent(e);
        }

        static public void PopupCloseEvent()
        {
            e.eventType = EventType.PopupClose;
            EventManager.TriggerEvent(e);
        }


    }
    public enum eTrainState
    {
        none,
        In,
        InDelay,
        DoorOpen,
        DoorOpenIdle,
        DoorClose,
        OutDelay,
        Out,
    }


    public enum eTrainActionState   // 해당 스테이트까지 대기임
    {
        NONE,

        STOP,
        GONE,

        POPUP_HOLD, // 팝업 오픈 시 강제 멈춤
    }

    enum eTrainType
    {
        None = -1,
        Green,
        //Red,
        //Blue,
        //Yellow,
        Max
    }
    public enum eTrainStateShowType
    {
        NONE = 0,
        SEND_ABLE = 1,
        DELIVERY_FINISH = 2,
    }
    public class TrainMove : LandmarkBuilding, EventListener<UIObjectEvent>, EventListener<UITrainStateEvent>
    {
        [Header("[State Bubble]")]
        [SerializeField]
        private GameObject bubbleObj = null;
        [SerializeField]
        private GameObject stateFinishObj = null;
        [SerializeField]
        private GameObject stateItemSendAbleObj = null;


        [Header("[Train]")]
        [SerializeField]
        private Transform maskTransform = null;
        [SerializeField]
        private Transform trainTransform = null;
        [SerializeField]
        private float maxTime = 60f;
        [SerializeField]
        private float timeIn = 3f;
        [SerializeField]
        private float inDelay = 1.5f;
        [SerializeField]
        private float doorOpen = 1.5f;
        [SerializeField]
        private float doorOpenIdle = 2f;
        [SerializeField]
        private float doorClose = 1.5f;
        [SerializeField]
        private float outDelay = 1.5f;
        [SerializeField]
        private float timeOut = 3f;
        [SerializeField]
        private List<GameObject> doorRight = null;
        [SerializeField]
        private List<GameObject> doorLeft = null;
        [SerializeField]
        private List<GameObject> wheelFront = null;
        [SerializeField]
        private List<GameObject> wheelBack = null;
        [SerializeField]
        private List<GameObject> doorFrameB1 = null;
        [SerializeField]
        private List<GameObject> doorFrameB2 = null;
        [SerializeField]
        private List<GameObject> doorFrameB3 = null;
        [SerializeField]
        private List<GameObject> headFirst = null;
        [SerializeField]
        private List<GameObject> doorFrame1 = null;
        [SerializeField]
        private List<GameObject> doorFrame2 = null;
        [SerializeField]
        private List<GameObject> doorFrame3 = null;
        [SerializeField]
        private List<GameObject> window1 = null;
        [SerializeField]
        private List<GameObject> window2 = null;
        [SerializeField]
        private List<GameObject> headBack = null;
        [SerializeField]
        private List<SpriteRenderer> TrainLight = null;

        private bool isInit = false;
        private float time = 0f;
        private float delayTime = 0f;
        private float delay = 0f;
        private Vector3 defaultPosition = new Vector3(0f, 1.32f, 0f);
        private Vector3 exitPosition = new Vector3(0f, 1.32f, 0f);
        private eTrainState curTrainState = eTrainState.In;
        private Vector3 defaultLeftCloseDoorPos = new Vector3(-0.5f, 0.22f, 0f);
        private Vector3 defaultRightCloseDoorPos = new Vector3(0.5f, 0.22f, 0f);
        private Vector3 defaultLeftOpenDoorPos = new Vector3(-0.16f, 0.22f, 0f);
        private Vector3 defaultRightOpenDoorPos = new Vector3(0.165f, 0.22f, 0f);

        private Vector3 position = Vector3.zero;
        private Vector4 inBezier = new Vector4(0f, 0.8f, 0.5f, 1f);
        private Vector4 doorOpenBezier = new Vector4(0.8f, 0f, 0.25f, 1f);
        private Vector4 doorCloseBezier = new Vector4(0.8f, 0f, 0.25f, 1f);
        private float lastWheelX = 0f;
        private float lastRattle = 0f;
        private Vector4 outBezier = new Vector4(1f, 0f, 1f, 0.5f);
       
        private eTrainType currentTrain = eTrainType.None;
        bool forceStart = false;
        bool leaveTrain = false;
        bool popupOpened = false;

        int widthSize = -1;

        private eTrainStateShowType trainStateShowType = eTrainStateShowType.NONE;
        private LandmarkSubway subway = null;
        public LandmarkSubway Subway
        {
            get
            {
                if (subway == null)
                    subway = User.Instance.GetLandmarkData<LandmarkSubway>();

                return subway;
            }
        }

        protected override void BuildingAction()
        {
            if (Subway != null && Subway.PlatsData != null)
            {
                
            }
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

            Init();
        }

        void Init()
        {
            if (isInit)
                return;

            isInit = true;



            MaskResize();
            position = defaultPosition;
            lastWheelX = defaultPosition.x;
            trainTransform.localPosition = position;
            //delayTime = maxTime - timeIn - inDelay - doorOpen - doorOpenIdle - doorClose - outDelay - timeOut;
            RandomTrain();
            UpdateOn();
            EventManager.AddListener<UITrainStateEvent>(this);
            EventManager.AddListener<UIObjectEvent>(this);

            SetTrainStateCheck();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventManager.RemoveListener<UITrainStateEvent>(this);
            EventManager.RemoveListener<UIObjectEvent>(this);
        }

        void SetTrainStateCheck()
        {
            trainStateShowType = eTrainStateShowType.NONE;
            if (Subway.BuildInfo !=null && Subway.BuildInfo.State == eBuildingState.NORMAL)
            {
                var platForms = subway.GetActivatePlatform();
                if( platForms.Count > 0)
                {
                    foreach (var platform in platForms)
                    {
                        if (platform.State == LandmarkSubwayPlantState.DELIVER_COMPLETE)
                        {
                            trainStateShowType = eTrainStateShowType.DELIVERY_FINISH;
                            break;
                        }
                        else if(platform.State == LandmarkSubwayPlantState.READY)
                        {
                            int requireItemCnt = 0;
                            foreach(var slot in platform.Slots)
                            {
                                int itemNo = slot[0];
                                int needItemCount = slot[1] - slot[2];
                                int curItemCount = User.Instance.GetItemCount(itemNo);
                                if(needItemCount <= curItemCount)
                                {
                                    ++requireItemCnt;
                                }
                            }

                            if(requireItemCnt == platform.Slots.Count)
                            {
                                trainStateShowType = eTrainStateShowType.SEND_ABLE;
                            }
                        }
                    }
                }
            }

            switch (trainStateShowType)
            {
                case eTrainStateShowType.NONE:
                    bubbleObj.SetActive(false);
                    break;
                case eTrainStateShowType.SEND_ABLE:
                    bubbleObj.SetActive(true);
                    stateFinishObj.SetActive(false);
                    stateItemSendAbleObj.SetActive(true);
                    break;
                case eTrainStateShowType.DELIVERY_FINISH:
                    bubbleObj.SetActive(true);
                    stateFinishObj.SetActive(true);
                    stateItemSendAbleObj.SetActive(true);
                    break;
            }
        }

        public void MaskResize()
        {
            if (maskTransform != null)
            {
                var size = TownMap.Width;
                var scale = maskTransform.localScale;
                scale.x = (size - 2) * 324 + 2 * 348 - 72;
                maskTransform.localScale = scale;
                defaultPosition = new Vector3(3.2f + maskTransform.localScale.x * 0.005f, 1.32f, 0);
                exitPosition = new Vector3(-3.2f - maskTransform.localScale.x * 0.005f, 1.32f, 0);
                position = defaultPosition;
                if (widthSize != size)
                    trainTransform.localPosition = position;
                widthSize = size;

                //curTrainState = eTrainState.In;
                //switch (curTrainState)
                //{
                //    case eTrainState.In:
                //    {
                //        lastWheelX = defaultPosition.x;
                //    }
                //    break;
                //    default:
                //    {
                //    }
                //    break;
                //}
                lastWheelX = defaultPosition.x;
            }
        }
        void RandomTrain()
        {
            SetTrain(SBFunc.Random((int)eTrainType.Green, (int)eTrainType.Max));
        }

        public void SetTrain(int eTrainType)
        {
            if (((int)currentTrain) == eTrainType)
                return;

            currentTrain = (eTrainType)eTrainType;
            if (doorFrameB1 != null)
            {
                for (int i = 0, count = doorFrameB1.Count; i < count; ++i)
                {
                    var obj = doorFrameB1[i];
                    if (obj == null)
                        continue;

                    obj.SetActive(eTrainType == i);
                }
            }

            if (doorFrameB2 != null)
            {
                for (int i = 0, count = doorFrameB2.Count; i < count; ++i)
                {
                    var obj = doorFrameB2[i];
                    if (obj == null)
                        continue;

                    obj.SetActive(eTrainType == i);
                }
            }

            if (doorFrameB3 != null)
            {
                for (int i = 0, count = doorFrameB3.Count; i < count; ++i)
                {
                    var obj = doorFrameB3[i];
                    if (obj == null)
                        continue;

                    obj.SetActive(eTrainType == i);
                }
            }

            if (headFirst != null)
            {
                for (int i = 0, count = headFirst.Count; i < count; ++i)
                {
                    var obj = headFirst[i];
                    if (obj == null)
                        continue;

                    obj.SetActive(eTrainType == i);
                }
            }

            if (doorFrame1 != null)
            {
                for (int i = 0, count = doorFrame1.Count; i < count; ++i)
                {
                    var obj = doorFrame1[i];
                    if (obj == null)
                        continue;

                    obj.SetActive(eTrainType == i);
                }
            }

            if (doorFrame2 != null)
            {
                for (int i = 0, count = doorFrame2.Count; i < count; ++i)
                {
                    var obj = doorFrame2[i];
                    if (obj == null)
                        continue;

                    obj.SetActive(eTrainType == i);
                }
            }

            if (doorFrame3 != null)
            {
                for (int i = 0, count = doorFrame3.Count; i < count; ++i)
                {
                    var obj = doorFrame3[i];
                    if (obj == null)
                        continue;

                    obj.SetActive(eTrainType == i);
                }
            }

            if (window1 != null)
            {
                for (int i = 0, count = window1.Count; i < count; ++i)
                {
                    var obj = window1[i];
                    if (obj == null)
                        continue;

                    obj.SetActive(eTrainType == i);
                }
            }

            if (window2 != null)
            {
                for (int i = 0, count = window2.Count; i < count; ++i)
                {
                    var obj = window2[i];
                    if (obj == null)
                        continue;

                    obj.SetActive(eTrainType == i);
                }
            }

            if (headBack != null)
            {
                for (int i = 0, count = headBack.Count; i < count; ++i)
                {
                    var obj = headBack[i];
                    if (obj == null)
                        continue;

                    obj.SetActive(eTrainType == i);
                }
            }
        }


        void InitTrainState()
        {
            if (Subway != null && Subway.PlatsData != null)
            {
                foreach (var platform in Subway.PlatsData)
                {
                    if (platform.State == LandmarkSubwayPlantState.READY || platform.State == LandmarkSubwayPlantState.DELIVER_COMPLETE)
                    {
                        if (curTrainState == eTrainState.none)
                        {
                            curTrainState = eTrainState.In;
                        }

                        return;
                    }

                    curTrainState = eTrainState.none;
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            float dt = Time.deltaTime;

            if (!isInit || Subway.PlatsData == null)
                return;
            
            if (delay > 0f)
            {
                delay -= dt;
                position.x = defaultPosition.x;
                return;
            }

            time += dt;
            
            switch (curTrainState)
            {
                case eTrainState.In:
                {
                    bool go = false;
                    foreach (var platform in Subway.PlatsData)
                    {
                        if (platform.State == LandmarkSubwayPlantState.READY || platform.State == LandmarkSubwayPlantState.DELIVER_COMPLETE)
                        {
                            go = true;
                            break;
                        }
                    }

                    if (!go && !forceStart)
                    {
                        time = 0;
                        return;
                    }

                    if (time > timeIn)
                    {
                        curTrainState = eTrainState.InDelay;
                        var x1 = SBFunc.BezierCurveSpeed(defaultPosition.x, 0f, timeIn, timeIn, inBezier);
                        WheelUpdate(x1);
                        position.x = x1;
                        trainTransform.localPosition = position;
                        time = 0f;
                        return;
                    }

                    var x = SBFunc.BezierCurveSpeed(defaultPosition.x, 0f, time, timeIn, inBezier);
                    WheelUpdate(x);
                    position.x = x;
                    trainTransform.localPosition = position;
                }
                break;
                case eTrainState.InDelay:
                {
                    if (time > inDelay)
                    {
                        curTrainState = eTrainState.DoorOpen;
                        time = 0f;
                        return;
                    }
                }
                break;
                case eTrainState.DoorOpen:
                {
                    if (time > doorOpen)
                    {
                        curTrainState = eTrainState.DoorOpenIdle;
                        time = 0f;

                        var doorLCount1 = doorLeft.Count;
                        var xL1 = SBFunc.BezierCurveSpeed(defaultLeftOpenDoorPos.x, defaultLeftCloseDoorPos.x, doorOpen, doorOpen, doorOpenBezier);
                        for (int i = 0; i < doorLCount1; i++)
                        {
                            if (doorLeft[i] == null)
                                continue;

                            doorLeft[i].transform.localPosition = new Vector3(xL1, defaultLeftOpenDoorPos.y, defaultLeftOpenDoorPos.z);
                        }

                        var doorRCount1 = doorRight.Count;
                        var xR1 = SBFunc.BezierCurveSpeed(defaultRightOpenDoorPos.x, defaultRightCloseDoorPos.x, doorOpen, doorOpen, doorOpenBezier);
                        for (int i = 0; i < doorRCount1; i++)
                        {
                            if (doorRight[i] == null)
                                continue;

                            doorRight[i].transform.localPosition = new Vector3(xR1, defaultRightOpenDoorPos.y, defaultRightOpenDoorPos.z);
                        }

                        foreach(var door in doorFrameB1)
                        {
                            if(door.activeInHierarchy)
                            {
                                var sp = door.GetComponent<SpriteRenderer>();
                                if (sp != null)
                                {
                                    sp.DOColor(Color.gray, 0.5f);
                                }
                            }
                        }
                        foreach (var door in doorFrameB2)
                        {
                            if (door.activeInHierarchy)
                            {
                                var sp = door.GetComponent<SpriteRenderer>();
                                if (sp != null)
                                {
                                    sp.DOColor(Color.gray, 0.5f);
                                }
                            }
                        }
                        foreach (var door in doorFrameB3)
                        {
                            if (door.activeInHierarchy)
                            {
                                var sp = door.GetComponent<SpriteRenderer>();
                                if (sp != null)
                                {
                                    sp.DOColor(Color.gray, 0.5f);
                                }
                            }
                        }
                        foreach (var light in TrainLight)
                        {
                            light.DOColor(Color.clear, 1.0f).SetEase(Ease.InBounce);
                        }
                        return;
                    }

                    if (doorLeft == null || doorRight == null)
                        return;


                    var doorLCount = doorLeft.Count;
                    var xL = SBFunc.BezierCurveSpeed(defaultLeftOpenDoorPos.x, defaultLeftCloseDoorPos.x, time, doorOpen, doorOpenBezier);
                    for (int i = 0; i < doorLCount; i++)
                    {
                        if (doorLeft[i] == null)
                            continue;

                        doorLeft[i].transform.localPosition = new Vector3(xL, defaultLeftOpenDoorPos.y, defaultLeftOpenDoorPos.z);
                    }

                    var doorRCount = doorRight.Count;
                    var xR = SBFunc.BezierCurveSpeed(defaultRightOpenDoorPos.x, defaultRightCloseDoorPos.x, time, doorOpen, doorOpenBezier);
                    for (int i = 0; i < doorRCount; i++)
                    {
                        if (doorRight[i] == null)
                            continue;

                        doorRight[i].transform.localPosition = new Vector3(xR, defaultRightOpenDoorPos.y, defaultRightOpenDoorPos.z);
                    }
                }
                break;
                case eTrainState.DoorOpenIdle:
                {
                    if (time > doorOpenIdle)
                    {
                        bool go = true;
                        foreach (var platform in Subway.PlatsData)
                        {
                            if (platform.State == LandmarkSubwayPlantState.READY || platform.State == LandmarkSubwayPlantState.DELIVER_COMPLETE)
                            {
                                go = false;
                                break;
                            }
                        }

                        if (go || forceStart)
                        {
                            forceStart = false;
                            curTrainState = eTrainState.DoorClose;
                            time = 0f;
                        }
                    }
                }
                break;
                case eTrainState.DoorClose:
                {
                    if (time > doorClose)
                    {
                        curTrainState = eTrainState.OutDelay;
                        time = 0f;

                        var doorLCount1 = doorLeft.Count;
                        var xL1 = SBFunc.BezierCurveSpeed(defaultLeftCloseDoorPos.x, defaultLeftOpenDoorPos.x, doorClose, doorClose, doorCloseBezier);
                        for (int i = 0; i < doorLCount1; i++)
                        {
                            if (doorLeft[i] == null)
                                continue;

                            doorLeft[i].transform.localPosition = new Vector3(xL1, defaultLeftCloseDoorPos.y, defaultLeftCloseDoorPos.z);
                        }

                        var doorRCount1 = doorRight.Count;
                        var xR1 = SBFunc.BezierCurveSpeed(defaultRightCloseDoorPos.x, defaultRightOpenDoorPos.x, doorClose, doorClose, doorCloseBezier);
                        for (int i = 0; i < doorRCount1; i++)
                        {
                            if (doorRight[i] == null)
                                continue;

                            doorRight[i].transform.localPosition = new Vector3(xR1, defaultRightCloseDoorPos.y, defaultRightCloseDoorPos.z);
                        }

                        foreach (var door in doorFrameB1)
                        {
                            if (door.activeInHierarchy)
                            {
                                var sp = door.GetComponent<SpriteRenderer>();
                                if (sp != null)
                                {
                                    sp.DOColor(Color.white, 0.5f);
                                }
                            }
                        }
                        foreach (var door in doorFrameB2)
                        {
                            if (door.activeInHierarchy)
                            {
                                var sp = door.GetComponent<SpriteRenderer>(); 
                                if (sp != null)
                                {
                                    sp.DOColor(Color.white, 0.5f);
                                }
                            }
                        }
                        foreach (var door in doorFrameB3)
                        {
                            if (door.activeInHierarchy)
                            {
                                var sp = door.GetComponent<SpriteRenderer>();
                                if (sp != null)
                                {
                                    sp.DOColor(Color.white, 0.5f);
                                }
                            }
                        }
                        foreach (var light in TrainLight)
                        {
                            light.DOColor(Color.white, 1.0f).SetEase(Ease.InBounce);
                        }
                        return;
                    }

                    if (doorLeft == null || doorRight == null)
                        return;

                    var doorLCount = doorLeft.Count;
                    var xL = SBFunc.BezierCurveSpeed(defaultLeftCloseDoorPos.x, defaultLeftOpenDoorPos.x, time, doorClose, doorCloseBezier);
                    for (int i = 0; i < doorLCount; i++)
                    {
                        if (doorLeft[i] == null)
                            continue;

                        doorLeft[i].transform.localPosition = new Vector3(xL, defaultLeftCloseDoorPos.y, defaultLeftCloseDoorPos.z);
                    }

                    var doorRCount = doorRight.Count;
                    var xR = SBFunc.BezierCurveSpeed(defaultRightCloseDoorPos.x, defaultRightOpenDoorPos.x, time, doorClose, doorCloseBezier);
                    for (int i = 0; i < doorRCount; i++)
                    {
                        if (doorRight[i] == null)
                            continue;

                        doorRight[i].transform.localPosition = new Vector3(xR, defaultRightCloseDoorPos.y, defaultRightCloseDoorPos.z);
                    }
                }
                break;
                case eTrainState.OutDelay:
                {
                    if (time > outDelay)
                    {
                        curTrainState = eTrainState.Out;
                        time = 0f;
                        return;
                    }
                }
                break;
                case eTrainState.Out:
                {
                    if (time > timeOut)
                    {
                        bool go = false;
                        
                        foreach (var platform in Subway.PlatsData)
                        {   
                            if (platform.State == LandmarkSubwayPlantState.READY || platform.State == LandmarkSubwayPlantState.DELIVER_COMPLETE)
                            {
                                go = true;
                                break;
                            }
                        }

                        if (go || forceStart)
                        {
                            curTrainState = eTrainState.In;
                            ResetTrain();
                            time = 0f;
                        }
                        
                        return;
                    }

                    var x = SBFunc.BezierCurveSpeed(0f, exitPosition.x, time, timeOut, outBezier);
                    WheelUpdate(x);
                    position.x = x;
                    trainTransform.localPosition = position;
                }
                break;

            }
        }

        void ResetTrain()
        {
            var x1 = SBFunc.BezierCurveSpeed(0f, exitPosition.x, timeOut, timeOut, outBezier);
            WheelUpdate(x1);
            position.x = x1;
            trainTransform.localPosition = position;
            time = 0f;
            //delay = delayTime;
            delay = 0;
            RandomTrain();
        }

        void WheelUpdate(float x)
        {
            var diff = x - lastWheelX;

            lastRattle += Mathf.Abs(diff);
            if (lastRattle > 1.0f)
            {
                SetRattle();
            }

            if (wheelFront != null)
            {
                var wheelCount = wheelFront.Count;

                for (int i = 0; i < wheelCount; ++i)
                {
                    var wheel = wheelFront[i];
                    if (wheel == null)
                        continue;

                    wheel.transform.localEulerAngles -= new Vector3(0f, 0f, diff * 150);
                }
            }

            if (wheelBack != null)
            {
                var wheelCount = wheelBack.Count;

                for (int i = 0; i < wheelCount; ++i)
                {
                    var wheel = wheelBack[i];
                    if (wheel == null)
                        continue;

                    wheel.transform.localEulerAngles -= new Vector3(0f, 0f, diff * 150);
                }
            }

            lastWheelX = x;                        
        }

        void SetRattle()
        {
            lastRattle = 0;

            position.y = defaultPosition.y + 0.01f;            
            Invoke("SetNormal", 0.1f);
        }

        void SetNormal()
        {
            position.y = defaultPosition.y;
        }

        protected override void SetLockIcon(eBuildingState state)
        {
            if (curBuildingState == state)
                return;
            
            Town.Instance.SetUndergroundState(state == eBuildingState.LOCKED || state == eBuildingState.NOT_BUILT);

            base.SetLockIcon(state);
        }

        public void OnEvent(UITrainStateEvent eventType)
        {
            if (eventType.eventType == UITrainStateEvent.EventType.PopupClose)
            {
                if (UITrainStateEvent.deliveryStart)
                    forceStart = true;

                UITrainStateEvent.deliveryStart = false;
            }

            SetTrainStateCheck();
        }



        public void OnEvent(UIObjectEvent eventType)
        {
            if(eventType.e == UIObjectEvent.eEvent.ITEM_GET || eventType.e == UIObjectEvent.eEvent.ITEM_USE)
                SetTrainStateCheck();
        }

        public override void OnTouchAction()
        {
            if (curBuildingState == eBuildingState.LOCKED)
            {
                return;
            }

            Town.Instance.OnFoorTouchAction(-1);

            foreach (var render in trainTransform.GetComponentsInChildren<SpriteRenderer>())
            {
                if (render.sortingOrder >= 5)
                    continue;

                if (DOTween.IsTweening(render))
                    continue;

                Color origin = render.color;
                render.color = Color.gray;
                render.DOColor(origin, 1.0f);
            }
        }
    }
}