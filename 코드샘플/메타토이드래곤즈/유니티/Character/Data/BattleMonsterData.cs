using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class BattleMonsterData : MonsterData
    {
        public BattleMonsterData(eStageType type)
        {
            stageType = type;
        }

        protected eStageType stageType = eStageType.UNKNOWN;
        public override eElementType Element => BaseData.ELEMENT_TYPE;
        public override float Size => BaseData.SIZE;
        public override void SetData(int pos, JToken token)
        {
            if (token == null)
                return;

            Alive = true;
            Position = pos;
            ID = token.Value<int>();

            Level = SpawnData.LEVEL;
            SkillLevel = 1;
            PetID = -1;
            Stat = SBFunc.BaseMonsterStatus(SpawnData.LEVEL, BaseData, FactorData, stageType);

            MaxHP = Stat.GetTotalStatusInt(eStatusType.HP);
            HP = MaxHP;

            if (Skill1 != null)
                SetSkill1Delay(Skill1.START_COOL_TIME - (Skill1.START_COOL_TIME * Stat.GetTotalStatusConvert(eStatusType.DEL_START_COOLTIME)));

            if (Infos == null)
                Infos = new();
            if (PriorityTargets == null)
                PriorityTargets = new();
        }
    }
    public class BattleWorldBossData : BattleMonsterData
    {
        public BattleWorldBossData() 
            : base(eStageType.WORLD_BOSS)
        {

        }

        public BattleWorldBossSpine WorldbossSpine { get; private set; } = null;
        public List<BattleWorldBossPartsData> ActiveParts => WorldbossSpine != null ? WorldbossSpine.ActiveParts : null;
        public override bool Death => false;
        public long SCORE { get; protected set; } = 0;
        public int SCORE_INT => SCORE > int.MaxValue ? int.MaxValue : (int)SCORE;
        public BattleWorldBossPartsSpine LastHitPart { get; private set; } = null;
        public override Transform Transform => LastHitPart ? LastHitPart.DamageTransform : WorldbossSpine.transform;
        public override int MaxShield => Stat.GetStatusInt(eStatusCategory.BASE, eStatusType.SHIELD_POINT);
        private int PREV
        {
            get
            {
                if (CurLevelData != null)
                    return CurLevelData.NEED_DMG;

                return SCORE_INT;
            }
        }
        public override int HP
        {
            get
            {
                if (NextLevelData != null)
                    return NextLevelData.NEED_DMG - SCORE_INT;
                else
                    return SCORE_INT;
            }
        }
        public override int MaxHP
        {
            get
            {
                if (CurLevelData != null)
                {
                    if (NextLevelData != null)
                        return NextLevelData.NEED_DMG - CurLevelData.NEED_DMG;
                }
                else
                {
                    if (NextLevelData != null)
                        return CurLevelData.NEED_DMG;
                }

                return HP + 1;// zero divide 방지
            }
        }
        private WorldBossLevelData CurLevelData { get { return WorldBossLevelData.Get(MonsterKey, Level); } }
        private WorldBossLevelData NextLevelData { get { return WorldBossLevelData.Get(MonsterKey, Level + 1); } }
        public int MonsterKey { get; protected set; } = -1;
        public override void SetData(int pos, JToken token)
        {
            if (token == null)
                return;

            Alive = true;
            Position = pos;
            ID = token.Value<int>();

            Level = 1;
            SkillLevel = 1;
            PetID = -1;
            baseData = MonsterBaseData.Get(ID.ToString());
            Stat = SBFunc.BaseBossCharStatus(BaseData, FactorData, stageType);
            MonsterKey = BaseData.KEY;
            MaxHP = Stat.GetTotalStatusInt(eStatusType.HP);
            HP = MaxHP;

            if (NormalSkill != null)
                SetNormalDelay(NormalSkill.START_COOL_TIME);

            if (Skill1 != null)
                SetSkill1Delay(Skill1.START_COOL_TIME - (Skill1.START_COOL_TIME * Stat.GetTotalStatusConvert(eStatusType.DEL_START_COOLTIME)));


            if (Infos == null)
                Infos = new();
            if (PriorityTargets == null)
                PriorityTargets = new();
        }
        public void OnDamage(int value, IBattleCharacterData caster)
        {
            if (Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT) > 0)
            {
                for (int i = 0, count = Infos.Count; i < count; ++i)
                {
                    if (Infos[i] == null)
                        continue;

                    value = Infos[i].SetDamage(value);
                    if (value == 0)
                        break;
                }
            }

            SCORE += value;
            StatisticsMananger.Instance.AddBossDamage(caster.ID, value, caster.IsEnemy);
            if (NextLevelData != null && NextLevelData.NEED_DMG < SCORE)
            {
                Level++;
                //ToastManager.On("보스레벨업");

                if (WorldbossSpine != null)
                {
                    WorldbossSpine.OnLevelUp(Level);
                }
            }

            if (WorldbossSpine != null)
            {
                LastHitPart = WorldbossSpine.GetHitObject(caster as WorldBossBattleDragonData);
            }
        }
        public void SetSpine(BattleWorldBossSpine spine)
        {
            WorldbossSpine = spine;
        }
        public override bool IsActionSkip()
        {
            return false;//본체는 껍다구
        }
    }
    public class BattleWorldBossPartsData : BattleMonsterData
    {
        public BattleWorldBossPartsData()
            : base(eStageType.WORLD_BOSS)
        {

        }
        public delegate void SpecialDelegate(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, SBSkill skill, float exValue);
        BattleWorldBossData BossData = null;
        public BattleWorldBossPartsSpine PartsSpine { get; private set; } = null;
        protected virtual SBSkill SpecialSkill { get; set; } = null;
        protected virtual SpecialDelegate SpecialFunc { get; set; } = null;
        protected virtual List<BattleSpine> SpecialTargets { get; set; } = null;
        /// <summary> Parts는 BossData의 IS_BOSS를 따라감 </summary>
        public override bool IsBoss => BossData.SpawnData.IS_BOSS > 0;
        protected bool isLeft = false;
        public override bool IsLeft => isLeft;
        public override Transform Transform => PartsSpine != null ? PartsSpine.DamageTransform : null;
        public static BattleWorldBossPartsData Create(BattleWorldBossData bossData, WorldBossPartData partsData)
        {
            BattleWorldBossPartsData ret = null;
            int type = 0;
            if (partsData.KEY > 0)
                type = partsData.KEY % 100;
            switch (type)
            {
                case 8:
                case 9:
                    ret = new BattleWorldBossLaserCannonPartsData(0);
                    break;
                case 10:
                case 11:
                    ret = new BattleWorldBossLaserCannonPartsData(1);
                    break;
                default:
                    ret = new BattleWorldBossPartsData();
                    break;
            }
            ret.SetData(bossData, partsData);

            return ret;
        }
        public override void SetSpine(BattleSpine spine)
        {
            base.SetSpine(spine);

            PartsSpine = (BattleWorldBossPartsSpine)spine;
            SetTransform(PartsSpine.DamageTransform);
        }
        public override int Level => BossData.Level;
        public int MonsterKey { get; protected set; } = -1;
        public override Transform EffectTransform => BossData.EffectTransform;
        public void SetData(BattleWorldBossData bossData, WorldBossPartData partsData)
        {
            BossData = bossData;
            SetData(partsData.ACTIVE_LEVEL, partsData.KEY);
        }
        public override bool IsCastingSkip()
        {
            if (WorldBossStage.Instance.IsSummoningParts(this))
                return true;

            return base.IsCastingSkip();
        }
        public void SetData(int level, int id)
        {
            Alive = true;
            Position = level;
            ID = id;

            Level = 1;
            SkillLevel = 1;
            PetID = -1;
            baseData = MonsterBaseData.Get(ID.ToString());
            Stat = SBFunc.BaseMonsterStatus(Level, BaseData, FactorData, stageType);
            MonsterKey = BaseData.KEY;
            MaxHP = Stat.GetTotalStatusInt(eStatusType.HP);
            if (BossData != null && BossData.Stat is BossCharacterStatus bossStat)
                bossStat.AddPartsStatus(Stat);

            HP = MaxHP;

            if (Skill1 != null)
                SetSkill1Delay(Skill1.START_COOL_TIME - (Skill1.START_COOL_TIME * Stat.GetTotalStatusConvert(eStatusType.DEL_START_COOLTIME)));

            if (Infos == null)
                Infos = new();
            if (PriorityTargets == null)
                PriorityTargets = new();
        }
        public override CircleCollider2D GetCircleCollider()
        {
            if (collider == null)
            {
                if (Transform != null)
                    collider = PartsSpine.GetComponent<CircleCollider2D>();
                if (collider == null)
                {
                    if (BossData != null)
                        collider = BossData.GetCircleCollider();
                }
            }

            return collider;
        }
        public override bool HasImmunity(params eCharacterImmunity[] immunity)
        {
            return BossData.HasImmunity(immunity);
        }
        public virtual void OnSpecialSkill(List<BattleSpine> targets, SBSkill skill, SpecialDelegate specialFunc)
        {
            SpecialSkill = skill;
            SpecialTargets = targets;
            SpecialFunc = specialFunc;
        }
        public override bool SetEffectInfo(EffectInfo info)
        {
            if (BossData == null || info == null)
                return false;

            switch (info.Data.TYPE)
            {
                case eSkillEffectType.TICK_DMG:
                case eSkillEffectType.POISON:
                case eSkillEffectType.DOT:
                case eSkillEffectType.D_DOT:
                    return base.SetEffectInfo(info);
                default:
                    break;
            }

            if (info.Passive != null)
                info.SetPassiveData(info.Caster, BossData, info.Passive, info.Data, 1);
            else
                info.SetEffectData(info.Caster, BossData, info.Data, 1);
            return BossData.SetEffectInfo(info);
        }
        public override void ReduceEffectInfo(eSkillEffectType type, float timeValue)
        {
            if (BossData == null)
                return;

            switch (type)
            {
                case eSkillEffectType.TICK_DMG:
                case eSkillEffectType.POISON:
                case eSkillEffectType.DOT:
                case eSkillEffectType.D_DOT:
                    base.ReduceEffectInfo(type, timeValue);
                    return;
                default:
                    break;
            }

            BossData.ReduceEffectInfo(type, timeValue);
        }
        public override bool IsEffectInfo(eSkillEffectType type, int nestGroup = -1)
        {
            if (BossData == null)
                return false;

            switch (type)
            {
                case eSkillEffectType.TICK_DMG:
                case eSkillEffectType.POISON:
                case eSkillEffectType.DOT:
                case eSkillEffectType.D_DOT:
                    return base.IsEffectInfo(type, nestGroup);
                default:
                    break;
            }

            return BossData.IsEffectInfo(type, nestGroup);
        }
        public override bool IsEffectInfo(params eSkillEffectType[] type)
        {
            if (BossData == null)
                return false;

            return BossData.IsEffectInfo(type);
        }
        public void SetPartsDirection(bool isLeft)
        {
            this.isLeft = isLeft;
        }
    }

    public class BattleWorldBossLaserCannonPartsData : BattleWorldBossPartsData
    {
        Coroutine curCoroutine = null;
        MonoBehaviour coroutineObject = null;
        int reinforcement = -1;

        Transform landEffect = null;
        public BattleWorldBossLaserCannonPartsData(int reinforce)
        {
            reinforcement = reinforce;
        }
        public override void SetSpine(BattleSpine spine)
        {
            base.SetSpine(spine);

            landEffect = spine.transform.Find("fx_wboss_laser");
            if (landEffect != null)
                landEffect.gameObject.SetActive(false);
        }
        /// <summary>
        /// 기획서 참고해본 결과
        /// 탱커라인 2,3 부터 때리며 가까운 탱커부터 먼 탱커로 타격
        /// 이후 0,1 라인에서 먼거리부터 가까운쪽으로 타격
        /// 대미지 감소는 1타격 당 NEST_GROUP의 숫자만큼 최초 수치부터 %대미지 감소
        /// 최대 중첩은 NEST_COUNT 숫자만큼 중첩됨
        /// </summary>
        public override void OnSpecialSkill(List<BattleSpine> targets, SBSkill skill, SpecialDelegate specialFunc)
        {
            base.OnSpecialSkill(targets, skill, specialFunc);
            SpecialTargets.Sort(SortTargets);
            if (Transform != null)
            {
                coroutineObject = Transform.GetComponent<MonoBehaviour>();
                if (coroutineObject != null)
                {
                    if (curCoroutine != null)
                    {
                        coroutineObject.StopCoroutine(curCoroutine);
                        curCoroutine = null;
                    }
                    curCoroutine = coroutineObject.StartCoroutine(SkillCoroutine());
                    coroutineObject.StartCoroutine(EffectCoroutine());
                }
            }
        }
        protected IEnumerator EffectCoroutine()
        {
            const float delayTime = 0.1f;
            const float animTime = 2.667f;
            yield return SBDefine.GetWaitForSeconds(delayTime);

            if (landEffect != null)
                landEffect.gameObject.SetActive(true);

            yield return SBDefine.GetWaitForSeconds(animTime - delayTime);

            if (landEffect != null)
                landEffect.gameObject.SetActive(false);
        }
        public int SortTargets(BattleSpine t1, BattleSpine t2)
        {
            var target1 = t1.Data as WorldBossBattleDragonData;
            var target2 = t2.Data as WorldBossBattleDragonData;
            if (target1 == null || target2 == null)
                return 0;

            if (target1.PartyIndex < target2.PartyIndex)
                return 1;
            else if (target1.PartyIndex > target2.PartyIndex)
                return -1;
            else//index가 같음
            {
                var distance1 = Mathf.Abs(spine.transform.position.x - t1.transform.position.x);
                var distance2 = Mathf.Abs(spine.transform.position.x - t2.transform.position.x);
                if (distance1 > distance2)
                    return target1.PartyIndex >= 2 ? 1/** 전방 */ : -1/** 후방 */;
                else if (distance1 < distance2)
                    return target1.PartyIndex >= 2 ? -1/** 전방 */ : 1/** 후방 */;
            }

            return 0;
        }
        protected IEnumerator SkillCoroutine()
        {
            const float laserLineDelay = 1.3333f;
            const float laserParam = 0.15f;
            BattleSpine prevTarget = null;
            var curTarget = 0;
            var curTime = 0f;
            while (curTarget < SpecialTargets.Count)
            {
                if (SpecialTargets[curTarget] != null && SpecialTargets[curTarget].Data.Death == false)
                {
                    var curData = SpecialTargets[curTarget].Data as WorldBossBattleDragonData;
                    if (curData.PartyIndex < 2 && curTime < laserLineDelay)
                    {
                        yield return SBDefine.GetWaitForSeconds(laserLineDelay - curTime);
                        prevTarget = null;
                        curTime = laserLineDelay;
                    }
                    float absX;
                    if (prevTarget == null)
                        absX = Mathf.Abs(SpecialTargets[curTarget].transform.position.x);
                    else
                        absX = Mathf.Abs(SpecialTargets[curTarget].transform.position.x - prevTarget.transform.position.x);
                    absX = curData.PartyIndex < 2 && prevTarget == null ? laserLineDelay - absX * laserParam : absX * laserParam;
                    if (absX > 0f)
                        yield return SBDefine.GetWaitForSeconds(absX);

                    curTime += absX;

                    var effect = SpecialSkill.GetEffect();
                    if (effect != null)
                    {
                        for (int j = 0, jCount = effect.Count; j < jCount; ++j)
                        {
                            if (effect[j] == null)
                                continue;

                            var nestCount = curTarget > effect[j].NEST_COUNT ? effect[j].NEST_COUNT : curTarget;
                            var exValue = (100f - (effect[j].NEST_GROUP * nestCount)) * SBDefine.CONVERT_FLOAT;
                            exValue = exValue < 0f ? 0f : exValue;
                            SpecialFunc?.Invoke(this, SpecialTargets[curTarget].Data, effect[j], SpecialSkill, exValue);
                        }
                    }

                    prevTarget = SpecialTargets[curTarget];
                }
                curTarget++;
            }
            SpecialTargets = null;
            SpecialSkill = null;
            SpecialFunc = null;
            yield break;
        }
    }
    public class BattleWorldBossSummonMonsterData : BattleMonsterData
    {
        protected bool isLeft = false;
        public override bool IsLeft => isLeft;

        List<BattleSpine> targetPartyDragons = null;
        public BattleWorldBossSummonMonsterData(int level)
            : base(eStageType.WORLD_BOSS)
        {
            Level = level;
        }
        public override void SetData(int pos, JToken token)
        {
            if (token == null)
                return;

            Alive = true;
            Position = pos;
            ID = token.Value<int>();

            SkillLevel = 1;
            PetID = -1;
            Stat = SBFunc.BaseMonsterStatus(SpawnData.LEVEL, BaseData, FactorData, stageType);

            MaxHP = Stat.GetTotalStatusInt(eStatusType.HP);
            HP = MaxHP;

            if (Skill1 != null)
                SetSkill1Delay(Skill1.START_COOL_TIME - (Skill1.START_COOL_TIME * Stat.GetTotalStatusConvert(eStatusType.DEL_START_COOLTIME)));

            if (Infos == null)
                Infos = new();
            if (PriorityTargets == null)
                PriorityTargets = new();
        }
        public void SetSummonInfo(bool isLeft, List<BattleSpine> targets)
        {
            this.isLeft = isLeft;

            targetPartyDragons = targets;
        }
        public void SetDirection(IBattleCharacterData data)
        {
            var spine = GetSpine();
            if (spine == null || spine.SpineTransform == null)
                return;

            spine.SpineTransform.localScale = new Vector3(Mathf.Abs(spine.SpineTransform.localScale.x) * (data.IsLeft ? -1 : 1) * (IsLeft ? -1 : 1), spine.SpineTransform.localScale.y, spine.SpineTransform.localScale.z);
        }
        public override void Update(float dt)
        {
            base.Update(dt);

            if (targetPartyDragons != null)
            {
                foreach (var target in targetPartyDragons)
                {
                    if (target != null && target.Data != null && target.Data.Death == false)
                    {
                        return;
                    }
                }

                //target이 하나도 없음.
                WorldBossStage.Instance.DestroySummonMonster(this);
            }
        }
    }
}