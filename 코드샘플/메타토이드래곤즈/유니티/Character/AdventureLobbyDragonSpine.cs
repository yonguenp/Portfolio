using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace  SandboxNetwork{
    public class AdventureLobbyDragonSpine : UIDragonSpine
    {
        Transform targetTransform = null;
        Transform parentTr = null;
        private Coroutine coStateUpdate = null;
        private int dragonOrder = 0; // 드래곤의 도착 위치 순서를 정하기 위한 것
        private int dragonMaxCnt = 0;
        float originYpos = 0f;
        float targetPosX = 0f;
        Vector3 LeftScale;
        Vector3 RightScale;

        public bool isDragonWaitScrolling { get; private set; } = false;
        public void SetTargetTransform(Transform transform)
        {
            targetTransform = transform;
            Stop();
            coStateUpdate = StartCoroutine(StateUpdate());
        }

        public void TeleportTargetPos(Transform targettransform)
        {
            targetTransform = targettransform;
            SetTargetPos();
            Stop();
            skeletonAni.AnimationState.SetAnimation(0, SBDefine.GetDragonAnimTypeToName(eSpineAnimation.IDLE), true);
            transform.SetAsFirstSibling();
            transform.localScale = RightScale;
            transform.position = targetPosX * Vector2.right;
            transform.localPosition = new Vector3(transform.localPosition.x, originYpos);
        }

        public void SetDragonWaitScrolling(bool state)
        {
            isDragonWaitScrolling = state;
        }

        public void SetParentTr(Transform transform)
        {
            parentTr = transform;
        }

        public void SetTranscendParent(Coffee.UIExtensions.UIParticle transcendParent)
        {
            TranscendParent = transcendParent;
        }

        protected override void Start()
        {
            base.Start();
        }
        public override void Init()
        {
            base.Init();
            originYpos = transform.localPosition.y;
            RightScale = transform.localScale;
            LeftScale = new Vector3(-RightScale.x, RightScale.y);
        }

        public void SetOrder(int order, int dragonNum)
        {
            if (order < 0) return;
            if (dragonNum <=0) return;
            dragonOrder = order;
            dragonMaxCnt = dragonNum;
        }
        private void OnDisable()
        {
            Stop();
        }

        public void Stop()
        {
            if (coStateUpdate != null)
            {
                StopCoroutine(coStateUpdate);
                coStateUpdate = null;
            }

            skeletonAni.AnimationState.SetAnimation(0, SBDefine.GetDragonAnimTypeToName(eSpineAnimation.IDLE), true);
        }

        public bool IsMoving()
        {
            return coStateUpdate != null;
        }

        IEnumerator StateUpdate()  // 타운 드래곤 stateMachine 처럼 하려 했으나 이 연출에는 그만큼 많은 기능이 필요가 없을 거 같아서 간단하게 함
        {
            yield return SBDefine.GetWaitForEndOfFrame();
            if (isDragonWaitScrolling)
            {
                yield return SBDefine.GetWaitForEndOfFrame();
                yield return SBDefine.GetWaitForSeconds(SBDefine.AdventureLobbyScrollTime);
                isDragonWaitScrolling = false;
            }
            if (parentTr != null)
            {
                transform.SetParent(parentTr, true);
                transform.SetAsFirstSibling();
            }
            yield return SBDefine.GetWaitForSeconds(SBFunc.Random(0f, 0.5f)); //제각각의 출발 딜레이

            SetTargetPos();
            skeletonAni.AnimationState.SetAnimation(0, SBDefine.GetDragonAnimTypeToName(eSpineAnimation.WALK), true);
            while (Mathf.Abs(targetPosX - transform.position.x) > 0.1f) {
                
                switch (SBFunc.Random(0, 1000))
                {
                    case 0:
                        yield return FallDown(); // 길가다가 넘어짐
                        yield return NormalGo();   // 우연의 일치로 두번 이상 넘어지면 이상하니깐 넣어둠
                        break;
                    default:
                        yield return NormalGo();
                        break;

                }
                SetTargetPos();  // 이동 이후 타겟 위치 재 계산 : 스크롤링으로 인해서 타겟 포지션 변경될 수 있음
            }
            skeletonAni.AnimationState.SetAnimation(0, SBDefine.GetDragonAnimTypeToName(eSpineAnimation.IDLE), true);

            coStateUpdate = null;
            yield break;
        }

        void SetTargetPos()
        {
            if (dragonMaxCnt % 2 == 1) // 드래곤 수가 홀수인 경우 정렬 용도
            {
                int standardValue = dragonMaxCnt / 2; //의도적으로 내림 처리
                targetPosX = targetTransform.position.x + ((dragonOrder - standardValue) * 0.5f); // 타겟 기준으로 중앙에 중간 드래곤이 오도록 설정하기 위한 용도
            }
            else // 드래곤 수가 짝수인 경우
            {
                float startPoint = -0.25f - (0.5f * ((dragonMaxCnt / 2f) - 1));
                targetPosX = targetTransform.position.x + startPoint + (dragonOrder * 0.5f); // 타겟 기준으로 양 옆으로 동일한 간격으로 드래곤이 오도록 설정하기 위한 용도
            }
        }

        IEnumerator NormalGo()
        {
            var dist = Mathf.Abs(targetPosX - transform.position.x);
            var speed = (5 + (dist * 0.5f)) * Time.deltaTime;
            if (targetPosX > transform.position.x)
            {
                transform.position += Vector3.right * speed;
                transform.localScale = RightScale;
            }
            else
            {
                transform.position += Vector3.left * speed;
                transform.localScale = LeftScale;
            }
            yield break;
        }
        IEnumerator FallDown()
        {
            MixAnim(SBDefine.GetDragonAnimTypeToName(eSpineAnimation.IDLE), SBDefine.GetDragonAnimTypeToName(eSpineAnimation.LOSE), 0.25f, 0.5f, 0.2f);
            yield return SBDefine.GetWaitForSeconds(0.2f);
            MixAnim(SBDefine.GetDragonAnimTypeToName(eSpineAnimation.LOSE), SBDefine.GetDragonAnimTypeToName(eSpineAnimation.IDLE), 0.25f, 0.5f, 0.2f);
            yield return SBDefine.GetWaitForSeconds(0.2f);
            skeletonAni.AnimationState.SetAnimation(0, SBDefine.GetDragonAnimTypeToName(eSpineAnimation.WALK), true);
            yield break;
        }
        
    }
}

