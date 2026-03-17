using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SandboxNetwork
{
    public abstract class SBProjectile : MonoBehaviour
    {
        /// <summary> 접근 정밀도 </summary>
        protected const float distance = 0.02f;
        /// <summary> 날아가는 방향으로 자동 방향전환 할것인가 </summary>
        [SerializeField]
        protected bool autoDirection = false;
        /// <summary> SortGroup 세팅 (안해도 무방) </summary>
        [SerializeField]
        protected SortingGroup sortingGroup = null;
        /// <summary> 자동 방향전환을 위한 투사체 Object </summary>
        [SerializeField]
        protected GameObject projectileObject = null;
        /// <summary> 자동 방향전환을 위한 그림자 Object </summary>
        [SerializeField]
        protected GameObject shadowObject = null;
        /// <summary> 시전자 </summary>
        public IBattleCharacterData Caster { get; protected set; } = null;
        /// <summary> 목적지 </summary>
        public abstract Vector3 TargetPos { get; protected set; }
        /// <summary> 발동 스킬 Effect연출 </summary>
        public SBSkill Skill { get; protected set; } = null;
        /// <summary> 발동 스킬 Index </summary>
        protected int SkillIndex { get; set; } = 0;
        /// <summary> 발동 스킬 Summon </summary>
        public SkillSummonData Summon
        {
            get
            {
                if (Skill == null)
                    return null;

                return Skill.GetSummon(SkillIndex);
            }
        }
        /// <summary> 발동 스킬 Effect </summary>
        public List<SkillEffectData> Effects
        {
            get
            {
                if (Skill == null)
                    return null;

                return Skill.GetEffect(SkillIndex);
            }
        }
        /// <summary> 발동 CallBack </summary>
        public VoidDelegate CallBack { get; protected set; } = null;
        /// <summary> 출발 여부 </summary>
        protected bool isInit = false;
        /// <summary> 도달 여부(밖에서 관리할 경우 삭제 용도 및 도달 확인) </summary>
        public virtual bool IsEnd { get => !isInit; }
        /// <summary> 투사체 속도 </summary>
        protected float Speed { get => ((Summon == null) ? 500f : Summon.ARROW_SPD) * SBDefine.CONVERT_FLOAT; }
        /// <summary> 투사체 시작 거리 </summary>
        protected float startDistance = 0f;
        /// <summary> 투사체 남은 거리 </summary>
        protected float curDistance = 0f;
        /// <summary> 시작시 몸통 세팅(일부는 등록해서 사용할 수 있음) </summary>
        protected virtual void Awake()
        {
            if(projectileObject == null)
            {
                var obj = transform.Find("projectile");
                if (obj != null)
                    projectileObject = obj.gameObject;
            }
            if (projectileObject == null)
            {
                var obj = transform.Find("body");
                if (obj != null)
                    projectileObject = obj.gameObject;
            }
            if (shadowObject == null)
            {
                var obj = transform.Find("shadow");
                if (obj != null)
                    shadowObject = obj.gameObject;
            }
            if (projectileObject == null)
            {
                var obj = transform.Find(name);
                if (obj != null)
                    projectileObject = obj.gameObject;
            }
        }
        /// <summary>  특정 조건이 달린 경우에 사용 </summary>
        /// <returns> 콜백 동작 여부 </returns>
        protected virtual bool IsCallback()
        {
            return CallBack != null;
        }
        /// <summary> 날아갈 수 있는 상태인가 </summary>
        protected virtual bool IsMove()
        {
            return isInit;
        }
        /// <summary> 출발 시 변경사항 들(기본 세팅) </summary>
        protected virtual void Launch()
        {
            startDistance = Vector2.Distance(transform.position, TargetPos);
            if (autoDirection)
            {
                var scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
            isInit = true;

        }
        /// <summary> 데이터로 된 방향전환 적용 </summary>
        /// <param name="data">리소스 데이터</param>
        public virtual void SetAutoDirection(SkillResourceData data)
        {
            if (data != null)
                autoDirection = data.AUTO_DIRECTION;
            else
                autoDirection = false;
        }
        /// <summary> 특수 구현을 제외하고 변경하지 말 것 </summary>
        protected virtual void FixedUpdate()
        {
            if (false == UpdateTile(Time.fixedDeltaTime))
                return;

            if (IsCallback())
                CallBack.Invoke();

            isInit = false;
            Destroy(gameObject);
        }
        /// <summary> Update or 시간 가는경우 이동로직 구현 </summary>
        /// <param name="dt">시간</param>
        /// <returns>목적지 도달 여부 : true => 도착, false => 아직 이동중</returns>
        protected virtual bool UpdateTile(float dt)
        {
            var diff = TargetPos - transform.position;
            var normal = diff.normalized;
            if (autoDirection)
                AutoDirection(normal);

            curDistance = Vector2.Distance(transform.position, TargetPos);
            if (curDistance > distance * Speed)
            {
                float magnitude = diff.magnitude;

                transform.position = Vector3.MoveTowards(transform.position, transform.position + (normal * magnitude), dt * Speed);
                return false;
            }
            return true;
        }
        /// <summary> 자동 방향 제어를 위함 </summary>
        /// <param name="normal">날아가는 위치 Vecter</param>
        protected virtual void AutoDirection(Vector3 normal)
        {
            var rotate = Mathf.Rad2Deg * (Mathf.Atan2(normal.y, normal.x));
            var rotateOffset = transform.localScale.x > 0f ? 0f : 180f;
            if (projectileObject == null)
                transform.localRotation = Quaternion.Euler(0f, 0f, rotate + rotateOffset);
            else
                projectileObject.transform.localRotation = Quaternion.Euler(0f, 0f, rotate + rotateOffset);

            if (shadowObject != null)
                shadowObject.transform.localRotation = Quaternion.Euler(0f, 0f, rotate + rotateOffset);
        }
    }
}