using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace SandboxNetwork
{
    public class trainMoveUI : MonoBehaviour
    {
        [SerializeField]
        private RectTransform trainTransform = null;
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
        private List<RectTransform> doorRight = null;
        [SerializeField]
        private List<RectTransform> doorLeft = null;

        private float time = 0f;
        private Vector3 beforeInPosition = new Vector3(1320f, -60f, 0f);
        private Vector3 CenterPosition = new Vector3(-970f, -60f, 0f);
        private Vector3 exitPosition = new Vector3(-2200f, -60f, 0f);
        public eTrainState curTrainState { get; set; }
        private Vector3 defaultLeftCloseDoorPos = new Vector3(-32, -36f, 0f);
        private Vector3 defaultRightCloseDoorPos = new Vector3(32, -36f, 0f);
        private Vector3 defaultLeftOpenDoorPos = new Vector3(-100, -36f, 0f);
        private Vector3 defaultRightOpenDoorPos = new Vector3(100, -36f, 0f);

        private Vector3 position = Vector3.zero;
        private Vector4 inBezier = new Vector4(0f, 0.8f, 0.5f, 1f);
        private Vector4 doorOpenBezier = new Vector4(0.8f, 0f, 0.25f, 1f);
        private Vector4 doorCloseBezier = new Vector4(0.8f, 0f, 0.25f, 1f);
        private Vector4 outBezier = new Vector4(1f, 0f, 1f, 0.5f);
        private bool isTrainMove = false;

        private Queue<eTrainState> trainQueue = new Queue<eTrainState>();

        private bool isInit = false;
        private IEnumerator trainCoroutine;
        // Start is called before the first frame update
        void init()
        {
            if (isInit == false)
            {
                isTrainMove = false;
                curTrainState = eTrainState.Out;
                position = beforeInPosition;
                trainTransform.localPosition = position;
                trainCoroutine = TrainMove();
                isInit = true;
            }
        }
        public void trainSetPos(int type) //0 - 전 정류장 1 - 현재 정류장 2 - 다음 정류장
        {
            switch (type)
            {
                case 0:
                    position = beforeInPosition;
                    break;
                case 1:
                    position = CenterPosition;
                    break;
                case 2:
                    position = exitPosition;
                    break;
            }

            trainTransform.localPosition = position;
        }

        public void MoveToSpecific(eTrainState startState, eTrainState endState)
        {
            init();
            if (/*trainQueue.Count == 0 && */endState == curTrainState) return;
            if (startState == endState) return;

            if (startState < endState)
            {
                for (eTrainState state = startState; state <= endState; ++state)
                {
                    trainQueue.Enqueue(state);
                }
            }
            else
            {
                if (startState != eTrainState.Out)
                {
                    for (eTrainState state = startState + 1; state <= eTrainState.Out; ++state)
                    {
                        trainQueue.Enqueue(state);
                    }
                }
                for (eTrainState state = eTrainState.In; state <= endState; ++state)
                {
                    trainQueue.Enqueue(state);
                }
            }
            if (gameObject.activeInHierarchy == false) return;
            if (isTrainMove == false)
            {
                trainCoroutine = TrainMove();
                StartCoroutine(trainCoroutine);
            }
        }

        private void OnDisable()
        {
            if (curTrainState == eTrainState.In || curTrainState == eTrainState.Out)
            {
                SetDoorOff();
            }

            isTrainMove = false;
            trainQueue.Clear();
        }

        void SetDoorOff()
        {
            for (int i = 0; i < doorLeft.Count; i++)
            {
                if (doorLeft[i] == null)
                    continue;

                doorLeft[i].localPosition = defaultLeftCloseDoorPos;
            }
            for (int i = 0; i < doorRight.Count; i++)
            {
                if (doorRight[i] == null)
                    continue;

                doorRight[i].localPosition = defaultRightCloseDoorPos;
            }
        }

        private IEnumerator TrainMove()
        {
            if (gameObject.activeInHierarchy == false)
                yield break;

            isTrainMove = true;
            while (trainQueue.TryDequeue(out eTrainState curState))
            {
                curTrainState = curState;
                switch (curState)
                {
                    case eTrainState.In:
                        yield return StartCoroutine(trainComeIn());
                        break;
                    case eTrainState.InDelay:
                        yield return StartCoroutine(InDelayFunc());
                        break;
                    case eTrainState.DoorOpen:
                        yield return StartCoroutine(doorOpenFuc());
                        break;
                    case eTrainState.DoorOpenIdle:
                        yield return StartCoroutine(doorOpenIdleFunc());
                        break;
                    case eTrainState.DoorClose:
                        yield return StartCoroutine(doorCloseFuc());
                        break;
                    case eTrainState.OutDelay:
                        yield return StartCoroutine(OutDelayFunc());
                        break;
                    case eTrainState.Out:
                        yield return StartCoroutine(trainComeOut());
                        break;
                }
            }

            isTrainMove = false;
            yield break;
        }



        IEnumerator trainComeIn()
        {
            trainTransform.localPosition = beforeInPosition;
            time = 0;
            while (time < timeIn)
            {
                time += SBGameManager.Instance.DTime;
                var x1 = SBFunc.BezierCurveSpeed(beforeInPosition.x, CenterPosition.x, time, timeIn, inBezier);
                position.x = x1;
                trainTransform.localPosition = position;
                yield return null;
            }
            trainTransform.localPosition = CenterPosition;
            yield break;
        }
        IEnumerator trainComeOut()
        {
            trainTransform.localPosition = CenterPosition;
            time = 0;
            while (time < timeOut)
            {
                time += SBGameManager.Instance.DTime;
                var x1 = SBFunc.BezierCurveSpeed(CenterPosition.x, exitPosition.x, time, timeOut, outBezier);
                position.x = x1;
                trainTransform.localPosition = position;
                yield return null;
            }
            trainTransform.localPosition = exitPosition;

            yield break;
        }
        IEnumerator doorCloseFuc()
        {
            time = 0f;
            trainTransform.localPosition = CenterPosition;
            if (doorLeft == null || doorRight == null)
                yield break;

            while (time < doorClose)
            {
                time += SBGameManager.Instance.DTime;
                var doorLCount = doorLeft.Count;
                var xL = SBFunc.BezierCurveSpeed(defaultLeftOpenDoorPos.x, defaultLeftCloseDoorPos.x, time, doorOpen, doorOpenBezier);
                for (int i = 0; i < doorLCount; i++)
                {
                    if (doorLeft[i] == null)
                        continue;

                    doorLeft[i].localPosition = new Vector3(xL, defaultLeftOpenDoorPos.y, defaultLeftOpenDoorPos.z);
                }

                var doorRCount = doorRight.Count;
                var xR = SBFunc.BezierCurveSpeed(defaultRightOpenDoorPos.x, defaultRightCloseDoorPos.x, time, doorOpen, doorOpenBezier);
                for (int i = 0; i < doorRCount; i++)
                {
                    if (doorRight[i] == null)
                        continue;

                    doorRight[i].localPosition = new Vector3(xR, defaultRightOpenDoorPos.y, defaultRightOpenDoorPos.z);
                }
                yield return null;
            }

            var doorLCount1 = doorLeft.Count;
            var xL1 = SBFunc.BezierCurveSpeed(defaultLeftOpenDoorPos.x, defaultLeftCloseDoorPos.x, doorOpen, doorOpen, doorOpenBezier);
            for (int i = 0; i < doorLCount1; i++)
            {
                if (doorLeft[i] == null)
                    continue;

                doorLeft[i].localPosition = new Vector3(xL1, defaultLeftOpenDoorPos.y, defaultLeftOpenDoorPos.z);
            }

            var doorRCount1 = doorRight.Count;
            var xR1 = SBFunc.BezierCurveSpeed(defaultRightOpenDoorPos.x, defaultRightCloseDoorPos.x, doorOpen, doorOpen, doorOpenBezier);
            for (int i = 0; i < doorRCount1; i++)
            {
                if (doorRight[i] == null)
                    continue;

                doorRight[i].localPosition = new Vector3(xR1, defaultRightOpenDoorPos.y, defaultRightOpenDoorPos.z);
            }
            yield break;

        }
        IEnumerator doorOpenFuc()
        {
            time = 0f;
            trainTransform.localPosition = CenterPosition;
            if (doorLeft == null || doorRight == null) yield break;
            while (time < doorOpen)
            {
                time += SBGameManager.Instance.DTime;
                var doorLCount = doorLeft.Count;
                var xL = SBFunc.BezierCurveSpeed(defaultLeftCloseDoorPos.x, defaultLeftOpenDoorPos.x, time, doorClose, doorCloseBezier);
                for (int i = 0; i < doorLCount; i++)
                {
                    if (doorLeft[i] == null)
                        continue;

                    doorLeft[i].localPosition = new Vector3(xL, defaultLeftCloseDoorPos.y, defaultLeftCloseDoorPos.z);
                }

                var doorRCount = doorRight.Count;
                var xR = SBFunc.BezierCurveSpeed(defaultRightCloseDoorPos.x, defaultRightOpenDoorPos.x, time, doorClose, doorCloseBezier);
                for (int i = 0; i < doorRCount; i++)
                {
                    if (doorRight[i] == null)
                        continue;

                    doorRight[i].localPosition = new Vector3(xR, defaultRightCloseDoorPos.y, defaultRightCloseDoorPos.z);
                }
                yield return null;
            }

            var doorLCount1 = doorLeft.Count;
            var xL1 = SBFunc.BezierCurveSpeed(defaultLeftCloseDoorPos.x, defaultLeftOpenDoorPos.x, doorClose, doorClose, doorCloseBezier);
            for (int i = 0; i < doorLCount1; i++)
            {
                if (doorLeft[i] == null)
                    continue;

                doorLeft[i].localPosition = new Vector3(xL1, defaultLeftCloseDoorPos.y, defaultLeftCloseDoorPos.z);
            }

            var doorRCount1 = doorRight.Count;
            var xR1 = SBFunc.BezierCurveSpeed(defaultRightCloseDoorPos.x, defaultRightOpenDoorPos.x, doorClose, doorClose, doorCloseBezier);
            for (int i = 0; i < doorRCount1; i++)
            {
                if (doorRight[i] == null)
                    continue;

                doorRight[i].localPosition = new Vector3(xR1, defaultRightCloseDoorPos.y, defaultRightCloseDoorPos.z);
            }
            yield break;
        }

        IEnumerator doorOpenIdleFunc()
        {
            time = 0;
            trainTransform.localPosition = CenterPosition;
            while (time < doorOpen)
            {
                time += SBGameManager.Instance.DTime;
                yield return null;
            }
            yield break;
        }
        IEnumerator InDelayFunc()
        {
            time = 0;
            trainTransform.localPosition = CenterPosition;
            while (time < inDelay)
            {
                time += SBGameManager.Instance.DTime;
                yield return null;
            }
            yield break;
        }

        IEnumerator OutDelayFunc()
        {
            time = 0;
            trainTransform.localPosition = CenterPosition;
            while (time < outDelay)
            {
                time += SBGameManager.Instance.DTime;
                yield return null;
            }
            yield break;
        }
    }
}