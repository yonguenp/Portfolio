using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public abstract class BattleCharacterData : IBattleCharacterData
    {
        #region BattleStat
        public int Position { get; protected set; } = 0;
        public object Target { get; set; } = null;
        public int ID { get; protected set; } = -1;
        public virtual int Level { get; protected set; } = 1;
        public int SkillLevel { get; protected set; } = 1;
        private int _HP = 0;
        public virtual int HP
        {
            get => _HP;
            set
            {
                if (value <= 0)
                    Alive = false;

                _HP = value;
            }
        }
        public virtual int MaxHP { get; protected set; } = 0;
        public virtual int MaxShield => MaxHP;
        public int PetID { get; protected set; } = -1;
        public int SkillRaito { get; private set; } = 0;
        protected bool Alive { get; set; } = true;
        public virtual bool Death => false == Alive || HP <= 0;
        public virtual CharacterStatus Stat { get; protected set; } = null;
        public virtual float MOVE_SPEED => BaseData.MOVE_SPEED;
        public abstract bool IsEnemy { get; }
        public abstract bool IsLeft { get; }
        public abstract eElementType Element { get; }
        public abstract float Size { get; }
        public abstract bool IsBoss { get; }
        public virtual eDirectionBit KnockBackDirection => IsEnemy ? eDirectionBit.Right : eDirectionBit.Left;
        public virtual Transform Transform { get; private set; } = null;
        Transform effectTransform = null;
        public virtual Transform EffectTransform
        {
            get
            {
                if (effectTransform == null)
                {
                    if (Transform != null)
                    {
                        var effect = Transform.GetComponent<statusEffect>();
                        if (effect != null)
                            effectTransform = effect.EffectTr;
                        else
                            effectTransform = Transform;
                    }
                }

                return effectTransform;
            }
        }
        public virtual void SetTransform(Transform transform)
        {
            Transform = transform;
        }
        public virtual TranscendenceData TranscendenceData { get; protected set; } = new();
        #endregion
        #region DesignData
        protected ICharacterBaseData baseData = null;
        public abstract ICharacterBaseData BaseData { get; set; }
        private SkillCharData normalSkill = null;
        public SkillCharData NormalSkill
        {
            get
            {
                if (normalSkill == null && BaseData != null)
                {
                    normalSkill = BaseData.NORMAL_SKILL;
                }
                return normalSkill;
            }
            set
            {
                normalSkill = value;
            }
        }
        private List<SkillEffectData> normalEffect = null;
        public List<SkillEffectData> NormalEffect
        {
            get
            {
                if (normalEffect == null && NormalSummon != null)
                {
                    normalEffect = SkillEffectData.GetGroup(NormalSummon.EFFECT_GROUP_KEY);
                }
                return normalEffect;
            }
            set
            {
                normalEffect = value;
            }
        }
        private SkillSummonData normalSummon = null;
        public SkillSummonData NormalSummon
        {
            get
            {
                if (normalSummon == null && NormalSkill != null)
                    normalSummon = SkillSummonData.Get(NormalSkill.SUMMON_KEY);

                return normalSummon;
            }
            set
            {
                normalSummon = value;
            }
        }
        private SkillCharData skill1 = null;
        public SkillCharData Skill1
        {
            get
            {
                if (skill1 == null && BaseData != null)
                {
                    skill1 = BaseData.SKILL1;
                }
                return skill1;
            }
            set
            {
                skill1 = value;
            }
        }
        private List<SkillEffectData> skill1Effect = null;
        public List<SkillEffectData> Skill1Effect
        {
            get
            {
                if (skill1Effect == null && Skill1Summon != null)
                {
                    skill1Effect = SkillEffectData.GetGroup(Skill1Summon.EFFECT_GROUP_KEY);
                }
                return skill1Effect;
            }
            set
            {
                skill1Effect = value;
            }
        }
        private SkillSummonData skill1Summon = null;
        public SkillSummonData Skill1Summon
        {
            get
            {
                if (skill1Summon == null && Skill1 != null)
                    skill1Summon = SkillSummonData.Get(Skill1.SUMMON_KEY);

                return skill1Summon;
            }
            set
            {
                skill1Summon = value;
            }
        }
        #endregion
        public abstract void SetData(int pos, JToken token);
        public virtual void Update(float dt)
        {
            InfosUpdate(dt);
            DelayUpdate(dt);
        }
        protected CircleCollider2D collider = null;
        public virtual CircleCollider2D GetCircleCollider()
        {
            if (collider == null)
            {
                if (Transform != null)
                    collider = Transform.GetComponent<CircleCollider2D>();
                else
                    return null;
            }

            return collider;
        }
        public bool IsActioning => ActionCoroutine != null;
        private Coroutine ActionCoroutine { get; set; }
        public void SetActionCoroutine(IEnumerator coroutine)
        {
            var spine = GetSpine();
            if (spine == null)
                return;

            if (IsActioning)
                spine.StopCoroutine(ActionCoroutine);
            ActionCoroutine = null;

            if (coroutine != null)
                ActionCoroutine = spine.StartCoroutine(coroutine);
        }
        public void EndActionCoroutine()
        {
            if (IsActioning)
            {
                var spine = GetSpine();
                if (spine != null)
                    spine.StopCoroutine(ActionCoroutine);
            }

            ActionCoroutine = null;
        }
        public virtual eBattleSkillType ActiveSkillType { get; protected set; } = eBattleSkillType.None;
        public void SetActiveSkilType(eBattleSkillType skillType)
        {
            ActiveSkillType = skillType;
        }
        public virtual bool Untouchable { get; protected set; } = false;
        public void SetUntouchable(bool untouchable)
        {
            Untouchable = untouchable;
        }
        public void ClearTypeInfo(eSkillEffectType type)
        {
            if (Infos == null)
                return;

            for (int i = 0, count = Infos.Count; i < count; ++i)
            {
                var info = Infos[i];
                if (info == null)
                    continue;

                if (info.EFFECT_TYPE == type)
                    info.TimeEnd();
            }

            InfosUpdate(0f);
        }
        public void ClearBuffStat()
        {
            if (Stat == null)
                return;

            Stat.ClearCategory(eStatusCategory.ADD_BUFF);
            Stat.ClearCategory(eStatusCategory.RATIO_BUFF);

            Stat.CalcStatusAll();
        }
        public virtual bool HasImmunity(params eCharacterImmunity[] immunity)
        {
            if (BaseData == null)
                return false;

            var flag = BaseData.Immunity;
            for (int i = 0, count = immunity.Length; i < count; ++i)
            {
                if (flag.HasFlag(immunity[i]))
                    return true;
            }
            return false;
        }
        #region DelayAndSkip
        public float CastingDelay { get; protected set; } = 0f;
        public void SetCastingDelay(float time)
        {
            CastingDelay = time;
        }
        public float AfterDelay { get; protected set; } = 0f;
        public void SetAfterDelay(float time)
        {
            AfterDelay = time;
        }
        public float NormalDelay { get; protected set; } = 0f;
        public void SetNormalDelay(float time)
        {
            NormalDelay = time;
        }
        public float Skill1Delay { get; protected set; } = 0f;
        public void SetSkill1Delay(float time)
        {
            if (Stat != null)
                Skill1Delay = time - (time * Stat.GetTotalStatusConvert(eStatusType.DEL_COOLTIME));
            else
                Skill1Delay = time;

            if (Skill1Delay < 0)
                Skill1Delay = 0;
        }
        public float WeakDelay { get; protected set; } = 0f;
        public void SetWeakDelay(float time)
        {
            WeakDelay = time;
        }
        protected virtual void DelayUpdate(float dt)
        {
            NormalDelay -= dt;
            if (NormalDelay < 0f)
                NormalDelay = 0f;

            Skill1Delay -= Stat.GetSkillCoolSpeed(dt);
            if (Skill1Delay < 0f)
                Skill1Delay = 0f;

            WeakDelay -= dt;
            if (WeakDelay < 0f)
                WeakDelay = 0f;

            var spine = GetSpine();
            if (spine != null)
            {
                switch (spine.Animation)
                {
                    case eSpineAnimation.A_CASTING:
                    case eSpineAnimation.ATTACK:
                        dt *= Stat.GetAttackSpeed();
                        break;
                    default:
                        break;
                }
            }

            CastingDelay -= dt;
            if (CastingDelay < 0f)
            {
                AfterDelay += CastingDelay;
                if (AfterDelay < 0f)
                    AfterDelay = 0f;
                CastingDelay = 0f;
            }
        }
        public virtual bool IsActionSkip()
        {
            if (Death)
                return true;

            if (CastingDelay > 0f)
                return true;

            var spine = GetSpine();
            if (spine != null)
            {
                switch (spine.Animation)
                {
                    case eSpineAnimation.A_CASTING:
                    case eSpineAnimation.CASTING:
                    case eSpineAnimation.ATTACK:
                    case eSpineAnimation.SKILL:
                        return true;
                    default: break;
                }
            }

            if (IsEffectInfo(eSkillEffectType.AIRBORNE,
                eSkillEffectType.STUN,
                eSkillEffectType.FROZEN,
                eSkillEffectType.KNOCK_BACK))
                return true;

            return false;
        }
        public virtual bool IsAttackSkip()
        {
            if (Death)
                return true;

            if (ActionCoroutine != null)
                return true;

            if (AfterDelay > 0f)
                return true;

            return false;
        }
        public virtual bool IsTargetSkip()
        {
            if (Death)
                return true;

            if (IsEffectInfo(eSkillEffectType.AIRBORNE))
                return true;

            return false;
        }
        public virtual bool IsSkillSkip()
        {
            if (Skill1 == null || Skill1Delay > 0f)
                return true;

            if (IsCastingSkip())
                return true;

            switch (Skill1.SKILL_CONDITION)
            {
                case eSkillCharCondition.HP_LOW:
                    switch (Skill1.CONDITION_VALUE_TYPE)
                    {
                        case eStatusValueType.PERCENT:
                            var maxHP = Stat.GetTotalStatusInt(eStatusType.HP);
                            if (maxHP * Skill1.CONDITION_VALUE * SBDefine.CONVERT_FLOAT < HP)
                                return true;
                            break;
                        case eStatusValueType.VALUE:
                        default:
                            if (Skill1.CONDITION_VALUE < HP)
                                return true;
                            break;
                    }
                    break;
                case eSkillCharCondition.FRIEND_DIE://미구현
                    break;
                case eSkillCharCondition.COOL_TIME:
                default:
                    break;
            }
            return false;
        }
        public virtual bool IsMoveSkip()
        {
            if (Death)
                return true;

            var spine = GetSpine();
            if (spine != null)
            {
                switch (spine.Animation)
                {
                    //case eSpineAnimation.A_CASTING:
                    //case eSpineAnimation.ATTACK:
                    case eSpineAnimation.CASTING:
                    case eSpineAnimation.SKILL:
                        return true;
                    default:
                        break;
                }
            }

            if (IsEffectInfo(eSkillEffectType.AIRBORNE,
                eSkillEffectType.STUN,
                eSkillEffectType.FROZEN,
                eSkillEffectType.KNOCK_BACK,
                eSkillEffectType.PULL))
                return true;

            return false;
        }
        public virtual bool IsCastingSkip()
        {
            if (Death)
                return true;

            if (IsEffectInfo(eSkillEffectType.AIRBORNE,
                eSkillEffectType.STUN,
                eSkillEffectType.FROZEN,
                eSkillEffectType.AGGRO,
                eSkillEffectType.SILENCE,
                eSkillEffectType.KNOCK_BACK))
                return true;

            return false;
        }
        public virtual bool IsWeak()
        {
            if (WeakDelay > 0f)
                return true;

            return false;
        }
        #endregion
        #region EffectInfo
        public List<EffectInfo> Infos { get; protected set; } = null;
        protected virtual void InfosUpdate(float dt)
        {
            if (Infos == null)
                return;

            for (int i = 0, count = Infos.Count; i < count; ++i)
            {
                if (Infos[i] == null)
                    continue;

                Infos[i].Update(dt);
            }

            Infos.RemoveAll((EffectInfo it) =>
            {
                if (it == null)
                    return true;

                if (it.IsActive)
                    it.Update(0f);
                return it.Time <= 0f;
            });
        }
        public virtual bool SetEffectInfo(EffectInfo info)
        {
            if (info == null)
                return false;

            bool isEffectEnd = true;
            bool isNew = true;
            for (int i = 0, count = Infos.Count; i < count; ++i)
            {
                if (Infos[i] == null)
                    continue;

                if (Infos[i].IsEquals(info))
                {
                    if (Infos[i].IsCover(info))
                    {
                        Infos[i].SetData(info);
                    }
                    isEffectEnd = Infos[i].IsEffectEnd();
                    isNew = false;
                    break;
                }
            }

            if (isNew)
                Infos.Add(info);

            return isEffectEnd;
        }
        public virtual void ReduceEffectInfo(eSkillEffectType type, float timeValue)
        {
            for (int i = 0, count = Infos.Count; i < count; ++i)
            {
                if (Infos[i] == null)
                    continue;

                if (Infos[i].EFFECT_TYPE != type)
                    continue;

                Infos[i].ReduceTime(timeValue);
            }
        }
        public virtual bool IsEffectInfo(eSkillEffectType type, int nestGroup = -1)
        {
            var isInfo = false;
            for (int i = 0, count = Infos.Count; i < count; ++i)
            {
                if (Infos[i] == null)
                    continue;

                if (Infos[i].EFFECT_TYPE == type)
                {
                    if (Infos[i].IsActive)
                    {
                        if (nestGroup < 1)
                        {
                            isInfo = true;
                            continue;
                        }

                        if (Infos[i].NEST_GROUP == nestGroup)
                            return true;
                    }
                }
            }
            return isInfo;
        }
        public virtual bool IsEffectInfo(params eSkillEffectType[] type)
        {
            var types = new List<eSkillEffectType>(type);
            for (int i = 0, count = Infos.Count; i < count; ++i)
            {
                if (Infos[i] == null)
                    continue;

                if (types.Contains(Infos[i].EFFECT_TYPE))
                {
                    if (Infos[i].IsActive)
                        return true;
                }
            }
            return false;
        }
        #endregion
        #region PriorityTarget
        protected List<KeyValuePair<IBattleCharacterData, int>> PriorityTargets { get; set; } = null;
        public IBattleCharacterData PriorityTarget
        {
            get
            {
                if (PriorityTargets == null || PriorityTargets.Count < 1)
                    return null;

                if (PriorityTargets[0].Key.Death)
                {
                    PriorityTargets.RemoveAll((KeyValuePair<IBattleCharacterData, int> e) =>
                    {
                        if (e.Key == null)
                            return true;

                        if (e.Key.Death)
                            return true;

                        return false;
                    });

                    if (PriorityTargets.Count < 1)
                        return null;
                }

                return PriorityTargets[0].Key;
            }
        }
        public virtual void AddPriorityTarget(IBattleCharacterData target, int value)
        {
            if (PriorityTargets == null)
                return;

            PriorityTargets.Add(new(target, value));
            PriorityTargets.RemoveAll((KeyValuePair<IBattleCharacterData, int> e) =>
            {
                if (e.Key == null)
                    return true;

                if (e.Key.Death)
                    return true;

                return false;
            });
            PriorityTargets.Sort(SortPriorityTarget);
        }
        public virtual void DelPriorityTarget(IBattleCharacterData target, int value)
        {
            if (PriorityTargets == null)
                return;

            PriorityTargets.RemoveAll((KeyValuePair<IBattleCharacterData, int> e) =>
            {
                if (e.Key == null)
                    return true;

                if (e.Key.Death || target == e.Key && value == e.Value)
                    return true;

                return false;
            });
        }
        private int SortPriorityTarget(KeyValuePair<IBattleCharacterData, int> d1, KeyValuePair<IBattleCharacterData, int> d2)
        {
            var a = d1.Value;
            var b = d2.Value;

            if (a > b)
                return 1;
            else if (a < b)
                return -1;
            else
                return 0;
        }
        public List<IBattleCharacterData> GetPriorityTargets(int count, List<IBattleCharacterData> targets)
        {
            if (PriorityTargets == null || PriorityTargets.Count < 1)
                return targets;

            var results = new List<IBattleCharacterData>();

            PriorityTargets.RemoveAll((KeyValuePair<IBattleCharacterData, int> e) =>
            {
                if (e.Key == null)
                    return true;

                if (e.Key.Death)
                    return true;

                return false;
            });
            for (int i = 0, pCount = PriorityTargets.Count; i < pCount; ++i)
            {
                results.Add(PriorityTargets[i].Key);
            }
            targets.RemoveAll((IBattleCharacterData d) =>
            {
                for (int i = 0, pCount = PriorityTargets.Count; i < pCount; ++i)
                {
                    if (PriorityTargets[i].Key == d)
                        return true;
                }

                return false;
            });

            results.AddRange(targets);

            return results;
        }
        #endregion
        #region Spine
        protected BattleSpine spine = null;
        public virtual void SetSpine(BattleSpine target)
        {
            spine = target;
        }
        public virtual BattleSpine GetSpine()
        {
            if (Transform == null)
                return null;

            if (spine == null)
            {
                SetSpine(Transform.GetComponent<BattleSpine>());
            }

            return spine;
        }
        #endregion
    }
}