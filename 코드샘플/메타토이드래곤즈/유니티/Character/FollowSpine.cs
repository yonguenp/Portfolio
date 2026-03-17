using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

namespace SandboxNetwork
{
    public class FollowSpine : MonoBehaviour
    {
        readonly Vector3 DefaultFollowPos = new Vector3(0f, 0f, 0f);
        readonly Vector4 MoveCubic = new Vector4(.3f, 0f, .3f, 1f);
        const float DefaultMoveTime = 2f;
        private Transform target = null;
        public Transform Target
        {
            get { return target; }
            private set { target = value; }
        }
        private Transform parent = null;
        public Transform Parent
        {
            get { return parent; }
            private set { parent = value; }
        }
        private IBattleCharacterData targetData = null;
        public IBattleCharacterData TargetData
        {
            get { return targetData; }
            private set { targetData = value; }
        }

        private SkeletonAnimation spine = null;
        public PetSpine Spine { get; protected set; } = null;
        private Vector3 pos = Vector3.zero;
        private Vector3 scale = Vector3.one;

        private float curTime = 0f;
        private float maxTime = 0f;
        private bool isLeft = false;
        private bool isDeath = false;

        private Vector3 targetPosition = Vector3.zero;
        private bool isTeleport = false;


        void Awake()
        {
            spine = GetComponentInChildren<SkeletonAnimation>();
            Spine = GetComponent<PetSpine>();
            if (Spine == null)
                Spine = GetComponentInChildren<PetSpine>();
        }
        void Start()
        {
            scale = new Vector3(transform.localScale.x * GetDir(), transform.localScale.y, transform.localScale.z);
            //transform.localScale = scale;

            spine.AnimationState.Complete += CompleteHandleAnimation;

            Refresh();
        }

        void LateUpdate()
        {
            if (target == null)
                return;

            Follow(SBGameManager.Instance.DTime);
        }

        public void Set(Transform target, Transform parent, IBattleCharacterData targetData, Vector3 pos, bool isLeft)
        {
            this.target = target;
            this.parent = parent;
            this.targetData = targetData;
            this.pos = pos;
            this.isLeft = isLeft;

            Refresh();
        }

        public void Refresh()
        {

            if (spine != null && spine.timeScale != SBGameManager.Instance.TimeScale)
            {
                spine.timeScale = SBGameManager.Instance.TimeScale;
            }

            if (target != null)
            {
                transform.position = target.position;
            }
        }

        private float GetDir()
        {
            if (parent == null || target == null)
                return 1f;

            if (TargetData != null && !TargetData.IsLeft)
                return parent.localScale.x * target.localScale.x < 0 ? 1 : -1;
            else
                return parent.localScale.x * target.localScale.x < 0 ? -1 : 1;
        }

        private void Follow(float dt)
        {
            if (isDeath)
                return;

            if (targetData == null)
                return;

            if (!isDeath && targetData.Death)
            {
                spine.AnimationState.SetAnimation(0, "death", false);
                //gameObject.SetActive(false);
                isDeath = true;
                return;
            }

            var targetX = target.position.x + pos.x * GetDir();
            var targetY = target.position.y + pos.y;
            if (!spine.gameObject.activeSelf && isTeleport)
            {
                curTime = 0;
                maxTime = DefaultMoveTime;
                transform.position = new Vector3(targetX, targetY, 0);
                transform.localScale = new Vector3(Mathf.Abs(scale.x) * GetDir(), scale.y, scale.z);
                isTeleport = false;
                spine.gameObject.SetActive(true);
            }
            else if (!target.gameObject.activeSelf)
            {
                isTeleport = true;
                spine.gameObject.SetActive(false);
                return;
            }

            var curPosision = transform.position;
            if (targetX == curPosision.x && targetY == curPosision.y)
            {
                curTime = 0;
                maxTime = DefaultMoveTime;
                transform.localScale = new Vector3(Mathf.Abs(scale.x) * GetDir(), scale.y, scale.z);
                return;
            }
            else if ((curTime + 1.2) > maxTime)
            {
                transform.localScale = new Vector3(Mathf.Abs(scale.x) * GetDir(), scale.y, scale.z);
            }

            if (targetPosition == null || (targetPosition.x == target.position.x && targetPosition.y == target.position.y))
            {
                curTime += dt;
            }
            else
            {
                var ing = curTime == 0 ? 1 : 1 - curTime / maxTime;
                curTime += dt * ing;
                maxTime += dt;
            }

            targetPosition = new Vector3(target.position.x, target.position.y, target.position.z);

            if (curTime >= maxTime)
            {
                curTime = 0;
                maxTime = DefaultMoveTime;
                transform.position = new Vector3(targetX, targetY, 0);
                transform.localScale = new Vector3(Mathf.Abs(scale.x) * GetDir(), scale.y, scale.z);
                return;
            }

            var curX = SBFunc.BezierCurveSpeed(curPosision.x, targetX, curTime, maxTime, MoveCubic);
            var curY = SBFunc.BezierCurveSpeed(curPosision.y, targetY, curTime, maxTime, MoveCubic);

            transform.position = new Vector3(curX, curY, 0);
            transform.localScale = new Vector3(Mathf.Abs(scale.x) * GetDir(), scale.y, scale.z);
        }

        public void FollowImmediate()
        {
            curTime = maxTime;
            Follow(0.0f);
        }

        protected TrackEntry SetAnimation(eSpineAnimation anim)
        {
            if (Spine == null)
                return null;

            return Spine.SetAnimation(anim);
        }

        protected virtual void CompleteHandleAnimation(TrackEntry trackEntry)
        {
            switch (trackEntry.Animation.Name)
            {
                case "death":
                    gameObject.SetActive(false);
                    Destroy(gameObject);
                    break;
            }
        }
    }
}