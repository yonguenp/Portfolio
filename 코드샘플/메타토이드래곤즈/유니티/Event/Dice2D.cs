using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace SandboxNetwork
{
    [System.Serializable]
    public struct DicePathInfo
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
        public float jumpPower;
        public int jumpCount;
        public float jumpDuration;

        public void Init()
        {
            startPoint = Vector3.zero;
            endPoint = Vector3.zero;
            jumpPower = 250f;
            jumpCount = 3;
            jumpDuration = 0.5f;
        }
    }

    public enum eDiceMode
    {
        NONE,
        AUTO,
        MANUAL,
    }

    public class Dice2D : MonoBehaviour
    {
        [SerializeField]
        List<Transform> face;

        [SerializeField]
        List<DicePathInfo> dicePathList = new List<DicePathInfo>();

        Sequence idleSeq = null;
        Sequence dropSeq = null;
        Sequence resultSeq = null;

        VoidDelegate resultCallback = null;

        eDiceMode currentDiceMode = eDiceMode.AUTO;

        public delegate void funcInt(int _resultNum);//결과값 invoke

        public void Update()
        {
            //CustomUpdate();//WJ - 이전에 소스로 제어 했던 부분을 animation 으로 바깥에서 제어.
        }

        void CustomUpdate()
        {
            face.Sort((a, b) => { return a.transform.position.z.CompareTo(b.transform.position.z); });
            for (int i = 0; i < face.Count; i++)
            {
                face[i].SetSiblingIndex(i);
            }
        }

        [ContextMenu("Random Dice Shot")]
        public void OnRandomDiceShot()
        {
            currentDiceMode = eDiceMode.MANUAL;
            SeqAllClear();
            OnDice(SBFunc.Random(1, 7), null, true);
        }

        public void OnDice(int targetnum, TweenCallback callback, bool _endOffVisible = false /* 처리 끝나면 끌 것인지*/)
        {
            resultSeq = DOTween.Sequence();
            resultSeq.Append(transform.DOLocalRotate(new Vector3(180.0f + SBFunc.RandomValue * 180.0f, 180.0f + SBFunc.RandomValue * 180.0f, 180.0f + SBFunc.RandomValue * 180.0f), 0.3f));

            float rollTime = 0.7f;
            while (rollTime > 0.0f)
            {
                var t = rollTime > 0.2f ? 0.05f + (SBFunc.RandomValue * 0.15f) : rollTime;
                rollTime -= t;
                resultSeq.Append(transform.DOLocalRotate(new Vector3(SBFunc.RandomValue * 360.0f, SBFunc.RandomValue * 360.0f, SBFunc.RandomValue * 360.0f), t));
            }

            resultSeq.Restart();

            Sequence diceSeq = DOTween.Sequence();
            transform.localScale = Vector3.one * 0.8f;
            diceSeq.Append(transform.DOScale(Vector3.one * 1.2f, 0.3f));
            diceSeq.Append(transform.DOScale(Vector3.one * 0.8f, 0.5f).SetEase(Ease.OutBounce).OnComplete(
            () =>
            {
                transform.DOKill();
                List<Vector3> pr = new List<Vector3>();

                switch (targetnum)
                {
                    case 1:
                        //1
                        pr.Add(new Vector3(0, 180, 0));
                        pr.Add(new Vector3(0, 180, 90));
                        pr.Add(new Vector3(0, 180, 180));
                        pr.Add(new Vector3(0, 180, 270));
                        break;
                    case 2:
                        //2
                        pr.Add(new Vector3(0, 90, 180));
                        pr.Add(new Vector3(0, 270, 0));
                        pr.Add(new Vector3(270, 180, 90));
                        pr.Add(new Vector3(180, 270, 0));
                        break;
                    case 3:
                        //3
                        pr.Add(new Vector3(0, 90, 90));
                        pr.Add(new Vector3(0, 270, 270));
                        pr.Add(new Vector3(90, 0, 0));
                        pr.Add(new Vector3(180, 270, 270));
                        break;
                    case 4:
                        //4
                        pr.Add(new Vector3(0, 90, 270));
                        pr.Add(new Vector3(0, 270, 90));
                        pr.Add(new Vector3(180, 270, 90));
                        pr.Add(new Vector3(90, 90, 270));
                        break;
                    case 5:
                        //5
                        pr.Add(new Vector3(0, 90, 0));
                        pr.Add(new Vector3(0, 270, 180));
                        pr.Add(new Vector3(90, 90, 0));
                        pr.Add(new Vector3(90, 180, 90));
                        break;
                    case 6:
                        //6
                        pr.Add(new Vector3(0, 0, 0));
                        pr.Add(new Vector3(0, 0, 90));
                        pr.Add(new Vector3(0, 0, 180));
                        pr.Add(new Vector3(0, 0, 270));
                        break;
                }


                pr.Sort((a, b) => { return Vector3.Angle(a, transform.localEulerAngles).CompareTo(Vector3.Angle(b, transform.localEulerAngles)); });

                Sequence sq = DOTween.Sequence();

                sq.Append(transform.DOLocalRotate(pr[0], 0.1f));
                sq.Append(transform.DOLocalRotate(pr[SBFunc.Random(0, pr.Count)], 0.1f));

                sq.Append(transform.DOScale(Vector3.one * 0.8f, 0.25f));

                sq.Append(transform.DOScale(Vector3.one * 1.1f, 0.25f));
                sq.Append(transform.DOScale(Vector3.one * 1.0f, 0.25f));
                sq.Append(transform.DOScale(Vector3.one * 1.1f, 0.25f));
                sq.Append(transform.DOScale(Vector3.one * 1.0f, 0.25f).OnComplete(() =>
                {

                    if (_endOffVisible && currentDiceMode == eDiceMode.MANUAL)
                        Clear();

                    callback?.Invoke();
                }));
                sq.Restart();
            }));
        }

        public void Clear()
        {
            SetVisible(false);
        }

        /// <summary>
        /// 낙하 연출 포함 굴리기
        /// </summary>
        /// <param name="resultNum"></param> 결과값 외부에서 미리 들고있기
        /// <param name="_startIdleDelay"></param> 연출 길이
        [ContextMenu("Random Dice Jump")]
        public void OnRandomDiceJumpDrop(int resultNum = 0, float _startIdleDelay = 0.0f, VoidDelegate _resultCallback = null)//주사위 던지는 연출
        {
            SeqAllClear();
            SetResultCallBack(_resultCallback);
            gameObject.GetComponent<RectTransform>().anchoredPosition3D = dicePathList[0].startPoint;
            GetComponent<RectTransform>().DOJumpAnchorPos(dicePathList[0].endPoint, dicePathList[0].jumpPower, dicePathList[0].jumpCount, dicePathList[0].jumpDuration, true).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                if (currentDiceMode == eDiceMode.AUTO)
                    OnRandomCustomDiceOpen(resultNum, _startIdleDelay);//끝나면 결과값 연출
            });
        }

        public void SetResultCallBack(VoidDelegate _resultCallback)
        {
            resultCallback = _resultCallback;
        }

        /// <summary>
        /// 낙하 연출 없는 단순 굴리기
        /// </summary>
        /// <param name="resultNum"></param>
        /// <param name="idleDelay"></param>
        /// <param name="_resultCallback"></param>
        public void OnRandomCustomDiceOpen(int resultNum = 0, float idleDelay = 0.0f)
        {
            SeqAllClear();

            if (resultNum <= 0)
                resultNum = SBFunc.Random(1, 7);

            OnDice(resultNum, () =>
            {
                if (currentDiceMode == eDiceMode.AUTO)
                {
                    if (resultCallback != null)
                        resultCallback.Invoke();

                    dropSeq = DOTween.Sequence();
                    dropSeq.AppendInterval(idleDelay).AppendCallback(() => { OnRandomDiceIdle(); });
                }
            });
        }

        [ContextMenu("Random Dice Idle")]
        public void OnRandomDiceIdle(float rollTime = 1.4f)
        {
            SeqAllClear();

            idleSeq = DOTween.Sequence();

            idleSeq.Append(transform.DOLocalRotate(new Vector3(180.0f + SBFunc.RandomValue * 180.0f, 180.0f + SBFunc.RandomValue * 180.0f, 180.0f + SBFunc.RandomValue * 180.0f), 0.6f));

            while (rollTime > 0.0f)
            {
                var t = rollTime > 0.4f ? 0.1f + (SBFunc.RandomValue * 0.3f) : rollTime;
                rollTime -= t;
                idleSeq.Append(transform.DOLocalRotate(new Vector3(SBFunc.RandomValue * 360.0f, SBFunc.RandomValue * 360.0f, SBFunc.RandomValue * 360.0f), t));
            }

            idleSeq.SetLoops(-1);
            idleSeq.Restart();
        }

        void SeqAllClear()
        {
            if (resultSeq != null)
            {
                resultSeq.Kill();
                resultSeq = null;
            }

            if (dropSeq != null)
            {
                dropSeq.Kill();
                dropSeq = null;
            }

            if (idleSeq != null)
            {
                idleSeq.Kill();
                idleSeq = null;
            }

        }

        public void SetVisible(bool _isVisible)
        {
            gameObject.SetActive(_isVisible);
        }

        public bool IsIdle()
        {
            return idleSeq != null;
        }

        public void SetDiceLayer()
        {
            SBFunc.SetLayer(this.gameObject, "Dice");
        }
    }
}

