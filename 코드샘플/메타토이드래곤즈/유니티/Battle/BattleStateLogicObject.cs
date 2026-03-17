using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public enum eAdventureCombatKey
    {
        None = 0,

        TARGET_UPDATE_DRAGON = 1,
        TARGET_UPDATE_MONSTER,

        ATTACK_CAST = 100,
        ATTACK_DAMAGE,
        ATTACK_DODGE,

        SKILL_CAST = 200,
        SKILL_DAMAGE,
        SKILL_DODGE,

        BUFF_APPLIED = 500,
        BUFF_DODGE,
        BUFF_UPDATE,
        BUFF_REMOVED,

        DEBUFF_TICKDMG = 600
    }
    public class SBTeleportObject : IBattleEventObject
    {
        public SBSpine<eSpineAnimation> Caster { get; protected set; } = null;
        public float Delay { get; protected set; }
        public VoidDelegate StartCallBack { get; protected set; } = null;
        public VoidDelegate EndCallBack { get; protected set; } = null;

        private bool isInit = false;
        public bool IsEnd { get { return !isInit; } }

        public void Set(SBSpine<eSpineAnimation> Caster, VoidDelegate StartCallBack, VoidDelegate EndCallBack, float Delay = 0f)
        {
            this.Caster = Caster;
            this.StartCallBack = StartCallBack;
            this.EndCallBack = EndCallBack;
            this.Delay = Delay;
            Start();
        }

        public virtual void Start()
        {
            if (Caster == null)
                return;

            isInit = true;
            StartCallBack?.Invoke();
        }

        public virtual void Update(float dt)
        {
            if (!isInit)
                return;

            if (Caster == null)
                return;

            Delay -= dt;
            if (Delay > 0f)
                return;

            isInit = false;
            EndCallBack?.Invoke();
        }

        public virtual void Translate(Vector3 pos) { }
    }
    public class SBFollowObject : IBattleEventObject
    {
        public Transform Effect { get; protected set; } = null;
        public IBattleCharacterData TargetData { get; protected set; } = default;
        public Transform Target { get; protected set; } = default;
        public string EffectName { get; protected set; }
        public float EffectTime { get; protected set; }
        public float Duration { get; protected set; }
        public VoidDelegate EffectCallBack { get; protected set; } = null;
        public Vector3 AddedPosition { get; protected set; } = Vector3.zero;
        public VoidDelegate EndCallBack { get; protected set; } = null;
        private bool isInit = false;
        private bool isTrigger = false;
        private bool isLoop = false;
        public bool IsEnd
        {
            get { return !isInit; }
        }

        public void Set(Transform Effect, Transform Target, IBattleCharacterData TargetData, string EffectName, float Duration, float EffectTime = 0f, VoidDelegate EffectCallBack = null, Vector2 AddedPosition = default, VoidDelegate EndCallBack = null)
        {
            if (Effect != null)
                this.Effect = Effect;

            this.Target = Target;
            this.TargetData = TargetData;
            this.EffectName = EffectName;
            if (Duration < 0f)
                isLoop = true;
            this.Duration = Duration;
            this.EffectTime = EffectTime;
            this.EffectCallBack = EffectCallBack;
            AddedPosition.y -= 0.01f;
            this.AddedPosition = AddedPosition;
            this.EndCallBack = EndCallBack;
            Start();
        }
        public void AddDuration(float Duration)
        {
            this.Duration += Duration;
        }
        public void SetDuration(float Duration)
        {
            this.Duration = Duration;
        }

        public virtual void Start()
        {
            if (Effect == null || Target == null)
                return;

            isTrigger = true;
            isInit = true;

            if (EffectTime <= 0f)
            {
                EffectCallBack?.Invoke();
                isTrigger = false;
            }
        }

        public virtual void Update(float dt)
        {
            if (!isInit)
                return;

            EffectTime -= dt;
            Duration -= dt;
            if (Effect == null || Target == null)
            {
                if (Effect != null)
                    GameObject.Destroy(Effect.gameObject);

                EndCallBack?.Invoke();
                isInit = false;
                return;
            }

            if (TargetData.Death)
            {
                Duration = 0f;
            }

            if (isTrigger)
            {
                if (EffectTime <= 0f)
                {
                    EffectCallBack?.Invoke();
                    isTrigger = false;
                }
            }

            Effect.transform.position = Target.position + AddedPosition;
            Effect.transform.localScale = Target.localScale;
            if (Duration > 0f || isLoop)
                return;

            GameObject.Destroy(Effect.gameObject);
            EndCallBack?.Invoke();
            isInit = false;
        }
        public virtual void Translate(Vector3 pos) { }
    }
    public class SBFieldObject : IBattleEventObject
    {
        GameObject[] prefabObject = null;

        private bool isInit = false;
        public bool IsEnd
        {
            get { return !isInit; }
        }

        public void Set(params GameObject[] obj)
        {
            prefabObject = obj;
            Start();
        }

        public virtual void Start()
        {
            isInit = true;
        }

        public virtual void Update(float dt)
        {
            if (!isInit)
                return;

            if (prefabObject == null)
            {
                isInit = false;
                return;
            }

            for(int i = 0, count = prefabObject.Length; i < count; ++i)
            {
                if (prefabObject[i] == null)
                    continue;

                return;
            }

            isInit = false;
        }
        public virtual void Translate(Vector3 pos)
        {
            if (prefabObject == null)
                return;

            for (int i = 0, count = prefabObject.Length; i < count; ++i)
            {
                if (prefabObject[i] == null)
                    continue;

                prefabObject[i].transform.Translate(pos);
            }
        }
    }
    public class SBDelayObject : IBattleEventObject
    {
        GameObject[] prefabObject = null;
        public float EndDelay { get; protected set; }
        public float EffectDelay { get; protected set; }
        public VoidDelegate EffectCallBack { get; protected set; } = null;
        public VoidDelegate EndCallBack { get; protected set; } = null;

        private bool isInit = false;
        public bool IsEnd
        {
            get { return !isInit; }
        }

        private bool isEffect = false;

        public void SetObject(params GameObject[] obj)
        {
            prefabObject = obj;
        }
        public void Set(VoidDelegate EffectCallBack, float EffectDelay, VoidDelegate EndCallBack, float EndDelay)
        {
            this.EffectCallBack = EffectCallBack;
            this.EffectDelay = EffectDelay;
            this.EndCallBack = EndCallBack;
            this.EndDelay = EndDelay;
            Start();
        }

        public virtual void Start()
        {
            isInit = true;
            isEffect = EffectCallBack == null;
            if (EffectDelay <= 0f && EndDelay <= 0f)
            {
                isInit = false;
                EndCallBack?.Invoke();
            }
        }

        public virtual void Update(float dt)
        {
            if (!isInit)
                return;

            EffectDelay -= dt;
            EndDelay -= dt;
            if (!isEffect)
            {
                if (EffectDelay > 0f)
                    return;

                isEffect = true;
                EffectCallBack?.Invoke();
                return;
            }
            else if (EndDelay > 0f)
                return;

            if(isInit)
            {
                isInit = false;
                EndCallBack?.Invoke();
            }
        }
        public virtual void Translate(Vector3 pos)
        {
            if (prefabObject == null)
                return;

            for(int i = 0, count = prefabObject.Length; i < count; ++i)
            {
                if (prefabObject[i] == null)
                    continue;
                prefabObject[i].transform.Translate(pos);
            }
        }
    }
    public class SBAttackSfxSound
    {
        private string soundName = "";
        private float delay = 0f;
        public bool IsPlaying { get; private set; } = false;

        public SBAttackSfxSound(string soundName, float delay)
        {
            this.soundName = soundName;
            this.delay = delay;
            IsPlaying = false;
        }

        public void Update(float dt)
        {
            if (IsPlaying)
                return;

            delay -= dt;
            if (delay <= 0)
            {
                delay = 0;
                Play();
            }
        }

        private void Play()
        {
            IsPlaying = true;
            SoundManager.Instance.PlaySFX(soundName);
        }
    }
    public class SBSkill
    {
        public SBSkill()
        {
            Initialize();
        }
        public SBSkill(IBattleCharacterData caster, SkillCharData skill, SkillSummonData summon, eBattleSkillType skillType)
        {
            SetCastData(caster, skill, summon, skillType);
        }
        ~SBSkill()
        {
            Initialize();
        }

        public IBattleCharacterData Caster { get; private set; } = null;
        private Vector3 tPos = default;
        public Vector3 TargetPosition 
        { 
            get
            {
                if(TargetTrasnform != null)
                    return TargetTrasnform.position;

                return tPos;
            }
            set
            {
                tPos = value;
            }
        }
        public Vector3 TargetScale
        {
            get
            {
                if (TargetTrasnform != null)
                    return TargetTrasnform.localScale;

                return Vector3.one;
            }
        }
        protected Transform TargetTrasnform { get; private set; } = null;
        public List<IBattleCharacterData> Targets { get; private set; } = null;
        public eBattleSkillType SkillType { get; private set; } = eBattleSkillType.None;
        public SkillCharData Skill { get; private set; } = null;
        public List<SkillSummonData> Summons { get; private set; } = null;
        private SkillSummonData Summon { get; set; } = null;
        public void Initialize()
        {
            Caster = null;
            Targets = null;
            SkillType = eBattleSkillType.None;
            Skill = null;
            Summon = null;
            TargetTrasnform = null;
        }
        public void SetCastData(IBattleCharacterData caster, SkillCharData skill, SkillSummonData summon, eBattleSkillType skillType, Transform targetTrasnform = null)
        {
            Caster = caster;
            Skill = skill;
            Summon = summon;
            if (Summons == null)
                Summons = new();
            else
                Summons.Clear();
            Summons.Add(summon);

            SkillType = skillType;
            
            if (targetTrasnform == null)
                targetTrasnform = Caster.Transform;

            TargetTrasnform = targetTrasnform;
        }
        
        public void SetTargetPos(Vector3 pos)
        {
            TargetTrasnform = null;
            TargetPosition = pos;
        }
        public bool NextSummon()
        {
            if(Summon.TRIGGER_TYPE == eSkillTriggerType.NEXT && Summon.NEXT_SUMMON > 0)
            {
                var summon = SkillSummonData.Get(Summon.NEXT_SUMMON);
                if (summon == null)
                    return false;

                Summon = summon;
                Summons.Add(summon);
                return true;
            }
            return false;
        }
        public bool HitSummon()
        {
            if (Summon.TRIGGER_TYPE == eSkillTriggerType.HIT && Summon.NEXT_SUMMON > 0)
            {
                var summon = SkillSummonData.Get(Summon.NEXT_SUMMON);
                if (summon == null)
                    return false;

                Summon = summon;
                Summons.Add(summon);
                return true;
            }
            return false;
        }
        public SkillSummonData GetSummon(int idx = 0)
        {
            if (Summons == null || Summons.Count <= idx || idx < 0)
                return null;

            return Summons[idx];
        }
        public List<SkillEffectData> GetEffect(int idx = 0)
        {
            var summon = GetSummon(idx);
            if (summon == null)
                return null;

            return summon.GetEffects();
        }
    }

    public class BattleSkill
    {
        public BattleSkill()
        {
            Initialze();
        }
        public IBattleCharacterData Character { get; private set; } = null;
        public eBattleSkillType SkillType { get; private set; } = eBattleSkillType.Normal;
        public eBattleSide Side { get; private set; } = eBattleSide.OffenseSide_1;
        public bool IsSkillActive { get; private set; } = false;
        public void Initialze()
        {
            Character = null;
            SkillType = eBattleSkillType.Normal;
            Side = eBattleSide.OffenseSide_1;
            IsSkillActive = false;
        }
        public void Set(IBattleCharacterData character, eBattleSkillType skillType, eBattleSide side)
        {
            Character = character;
            SkillType = skillType;
            Side = side;
            IsSkillActive = false;
        }
        public bool IsSkillSkip()
        {
            if (Character == null)
                return true;

            return Character.IsSkillSkip();
        }
        public bool SkillActive(IBattleCharacterData caster)
        {
            if (Character == null || Character.Death || IsSkillActive)
                return false;

            if (Character != caster)
                return false;

            IsSkillActive = true;
            return true;
        }
        public bool IsEmpty()
        {
            if (Character == null)
                return true;

            return false;
        }
    }
    public class BattleSkillQueue : List<BattleSkill>
    {
        public BattleSkillQueue(eBattleSide side) : base()
        {
            this.side = side;
            addedList = new List<IBattleCharacterData>();
            skillPool = new SBListPool<BattleSkill>(Reuse, Unuse);
        }
        ~BattleSkillQueue()
        {
            addedList.Clear();
            addedList = null;
            skillPool.Clear();
            skillPool = null;
            Clear();
        }
        private eBattleSide side = eBattleSide.OffenseSide_1;
        private SBListPool<BattleSkill> skillPool = null;
        private List<IBattleCharacterData> addedList = null;

        /// <summary>
        /// 밖에서 현재 스킬을 누가 쓸건 지 확보하기 위함.
        /// </summary>
        /// <returns>현재 스킬 사용자의 스킬</returns>
        public BattleSkill Get()
        {
            int index = 0;
            while (Count > index)
            {
                if (this[index].Character.Death || this[index].IsSkillSkip() || this[index].IsSkillActive)
                {
                    ++index;
                    continue;
                }

                break;
            }

            return Count > index ? this[index] : null;
        }
        /// <summary>
        /// 들어가기 전에 캐릭터 전체를 한 프레임에 계산을 한번에 하기 위함 행동 이후 실행
        /// </summary>
        /// <param name="skill">캐릭터 스킬</param>
        public void Cast(IBattleCharacterData character)
        {
            if (character.IsSkillSkip())
                return;

            if (addedList.Contains(character))
                return;

            if (Contains(character))
                return;

            addedList.Add(character);
        }
        private bool Contains(IBattleCharacterData character)
        {
            return FindIndex((BattleSkill skill) => { return skill.Character == character; }) >= 0;
        }
        /// <summary>
        /// 대기열에 들어오기 전 정리 및 대기열 적용(프레임 마지막에 호출)
        /// </summary>
        public void SortCast()
        {
            RemoveAll(RemoveCheck);
            if (addedList.Count < 1)
                return;

            addedList.Sort(SortCast);
            foreach (var item in addedList)
            {
                if (item == null)
                    continue;
                var skill = ReuseSkill();
                skill.Set(item, eBattleSkillType.Skill1, side);
                Add(skill);
            }

            addedList.Clear();
        }
        /// <summary>
        /// Position 정렬
        /// </summary>
        /// <param name="s1">타겟1</param>
        /// <param name="s2">타겟2</param>
        /// <returns>앞으로갈지 뒤로갈지</returns>
        private int SortCast(IBattleCharacterData s1, IBattleCharacterData s2)
        {
            if (s1 == null || s2 == null)
                return 0;

            var p1 = s1.Position;
            var p2 = s2.Position;
            if (p1 > p2)
                return 1;
            else if (p1 < p2)
                return -1;
            else
                return 0;
        }
        /// <summary>
        /// 죽거나 잘못된 데이터라면 제거하기
        /// </summary>
        /// <param name="skill">타겟</param>
        /// <returns>지울지말지</returns>
        private bool RemoveCheck(BattleSkill skill)
        {
            if (skill == null)
                return true;

            if (skill.Character == null || skill.Character.Death || skill.IsSkillActive)
            {
                UnuseSkill(skill);
                return true;
            }

            return false;
        }
        /// <summary>
        /// 이미 생성해두었으면 생성안하는 형태로 구현.
        /// </summary>
        /// <returns>제일 마지막 사용 스킬 or 새 스킬</returns>
        private BattleSkill ReuseSkill()
        {
            if (skillPool == null)
                return null;

            skillPool.Spawn(1);
            return skillPool.Get();
        }
        /// <summary>
        /// 다 사용한 스킬 재사용하기 위함
        /// </summary>
        /// <param name="skill">타겟</param>
        private void UnuseSkill(BattleSkill skill)
        {
            if (skillPool == null)
                return;

            skillPool.Put(skill);
        }
        /// <summary>
        /// 재사용 시 할 내용(지금은 없음)
        /// </summary>
        /// <param name="item">타겟</param>
        private void Reuse(BattleSkill item)
        {
        }
        /// <summary>
        /// 해재 시 초기화
        /// </summary>
        /// <param name="item">타겟</param>
        private void Unuse(BattleSkill item)
        {
            item.Initialze();
        }
    }
}