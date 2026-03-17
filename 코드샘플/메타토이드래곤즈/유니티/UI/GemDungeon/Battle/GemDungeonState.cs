using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class GemDungeonState : BattleStateLogic
    {
        protected float movingTime = 1f;
        int floor = 0;
        protected int Floor
        {
            get
            {
                if (floor == 0)
                {
                    floor = Stage.GetComponent<GemDungeonStage>().CurFloor;
                }
                return floor;
            }
        }
        private LandmarkGemDungeonFloor floorData = null;
        protected LandmarkGemDungeonFloor FloorData
        {
            get
            {
                if (floorData == null)
                    floorData = LandmarkGemDungeon.Get().GetFloorData(Floor);
                return floorData;
            }
        }
        readonly float moveMaxTime = 2f;
        readonly float maxXScale = TownMap.Width * 3.2f;
        readonly float dragonStartYpos = -0.1f;
        readonly float maxPosY = 0.2f;
        readonly float minPosY = -0.7f;
        readonly float BuffDamage = 1.5f;
        protected bool moving = false;
        protected List<int> burnOutDragonList = new();
        protected List<GameObject> projectileObjs = new();
        protected List<BattleSpine> burnOutDragons = new();
        protected int maxMonsterCount = GameConfigTable.GetConfigIntValue("GEMDUNGEON_MONSTER_COUNT_MAX");

        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                //EventManager.AddListener(this);
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                floorData = null;
                return true;
            }
            return false;
        }

        #region 캐릭터 세팅
        protected void DragonSet(eGemDungeonState state)
        {
            var dics = Data.OffenseDic;
            var itrDragon = Data.OffensePos.GetEnumerator();

            burnOutDragonList.Clear();
            burnOutDragons.Clear();
            foreach (var dragonData in LandmarkGemDungeon.Get().DragonDatas.Values)
            {
                if (dragonData.ExpectedFatigue == 0)
                {
                    burnOutDragonList.Add(dragonData.DragonNo);
                }
            }

            int currentSpineCount = Stage.OffenseSpines.Count;

            if (currentSpineCount < dics.Count)  // 신규 생성이 필요한 드래곤 생성
            {
                for (int i = 0; i < currentSpineCount; ++i)
                {
                    itrDragon.MoveNext();
                }
                while (itrDragon.MoveNext())
                {
                    var x = itrDragon.Current.Key;
                    var list = itrDragon.Current.Value;
                    var spineList = new List<BattleSpine>();
                    for (int y = 0, count = list.Count; y < count; ++y)
                    {
                        var curPos = list[y];

                        if (!dics.ContainsKey(curPos.BattleTag) || y >= 2)
                            continue;
                        var dragonSpine = Stage.GetOffenseSpine(dics[curPos.BattleTag]);
                        dragonSpine.transform.localScale = new Vector3((x + 1) % 2 == 0 ? 1.2f : -1.2f, 1.2f);
                        dragonSpine.transform.localPosition = GetDragonPosition(x);
                        spineList.Add(dragonSpine);
                    }
                    Stage.OffenseSpines.Add(spineList);
                }
            }

            // 드래곤 상태 변경
            var dragonItr = Data.OffensePos.GetEnumerator();
            for (int i = 0, count = Stage.OffenseSpines.Count; i < count; ++i)
            {
                var itr = dragonItr.MoveNext();
                var x = dragonItr.Current.Key;
                var posList = dragonItr.Current.Value;
                var dragonSpines = Stage.OffenseSpines[i];
                if (dragonSpines == null)
                    continue;
                if (itr == false)
                {
                    foreach (var dragon in dragonSpines)
                    {
                        dragon.gameObject.SetActive(false);
                    }
                    continue;
                }
                for (int y = 0, yCount = dragonSpines.Count; y < yCount; ++y)
                {
                    var curPos = posList[y];
                    var curDragon = dragonSpines[y];
                    curDragon.GetComponent<Collider2D>().enabled = true;
                    curDragon.Controller.StopCO();
                    curDragon.Data.SetActiveSkilType(eBattleSkillType.None);
                    curDragon.SetDefaultSpeed(1.5f);
                    curDragon.transform.localPosition = GetDragonPosition(curPos.BattleTag);
                    bool isBurnOut = burnOutDragonList.Contains(dics[curPos.BattleTag].ID);
                    switch (state)
                    {
                        case eGemDungeonState.IDLE:
                            curDragon.gameObject.SetActive(false);
                            offenses.Add(curDragon);
                            break;
                        case eGemDungeonState.BATTLE:
                            if (isBurnOut) // 오펜스에 추가하면 배틀로직 돌아버리니깐 따로 보관
                            {
                                burnOutDragons.Add(curDragon);
                                curDragon.SetAnimation(eSpineAnimation.LOSE);
                            }
                            else
                            {
                                offenses.Add(curDragon);
                                curDragon.SetAnimation(eSpineAnimation.IDLE);
                            }

                            break;
                        case eGemDungeonState.END:
                            curDragon.gameObject.SetActive(true);
                            curDragon.SetAnimation(eSpineAnimation.LOSE);
                            offenses.Add(curDragon);
                            burnOutDragons.Add(curDragon);
                            break;
                    }
                    curDragon.SkeletonAni.Skeleton.SetColor(Color.white);
                }

            }


            Stage.StartCoroutine(SetBurnOutDragonCor());
        }
        protected void SetBurnOutDragon()
        {
            if (burnOutDragons == null)
                return;
            foreach (var curDragon in burnOutDragons)
            {
                curDragon.SkeletonAni.Skeleton.SetColor(Color.grey);
                curDragon.SetAnimation(eSpineAnimation.LOSE);
            }
                
        }

        IEnumerator SetBurnOutDragonCor() // 한 프레임 뒤에( 각각의 오브젝트들이 start 가 호출되고 난 뒤) 해야 됨
        {
            yield return null;
            SetBurnOutDragon();
        }
        protected void MonsterSet()
        {
            // 몬스터 세팅
            if (Stage.DefenseSpines.Count == 0)  // 세팅한 몬스터가 없으면
            {
                var monsterItr = Data.DefensePos.GetEnumerator();
                var monsterDic = Data.DefenseDic;
                while (monsterItr.MoveNext())
                {
                    var x = monsterItr.Current.Key;
                    var list = monsterItr.Current.Value;

                    var spineList = new List<BattleSpine>();
                    for (int y = 0, count = list.Count; y < count; ++y)
                    {
                        var curPos = list[y];
                        if (!monsterDic.ContainsKey(curPos.BattleTag))
                            continue;
                        var enemySpine = Stage.GetDefenseSpine(monsterDic[curPos.BattleTag]) as BattleMonsterSpine;
                        enemySpine.transform.localScale = new Vector3(curPos.BattleTag <= 0 ? -1.2f : 1.2f, 1.2f);
                        //enemySpine.Controller.IsRight = curPos.BattleTag > 0;
                        enemySpine.transform.localPosition = curPos.Type != 1 ? GetMonstePosition(x) : GetBossPosition(x);

                        enemySpine.SetDefaultSpeed(1.5f);
                        enemySpine.GetComponent<Collider2D>().enabled = true;
                        enemySpine.SetRigidbodySimulated(false);
                        enemySpine.SetAnimation(eSpineAnimation.IDLE);
                        spineList.Add(enemySpine);
                        defenses.Add(enemySpine);
                        //enemySpine.CData = 
                    }
                    Stage.DefenseSpines.Add(spineList);
                }
            }
            else // 세팅한 몬스터가 있으면
            {
                var monsterItr = Data.DefensePos.GetEnumerator();
                for (int i = 0, count = Stage.DefenseSpines.Count; i < count; ++i)
                {
                    var itr = monsterItr.MoveNext();
                    var x = monsterItr.Current.Key;
                    var posList = monsterItr.Current.Value;
                    if (itr == false)
                        continue;
                    var list = Stage.DefenseSpines[i];
                    if (list == null)
                        continue;
                    for (int y = 0, yCount = list.Count; y < yCount; ++y)//몬스터 정위치 이동
                    {
                        var curPos = posList[y];
                        var curMonster = list[y];
                        //curMonster.Controller.IsRight = curPos.BattleTag > 0;
                        curMonster.transform.localPosition = curPos.Type != 1 ? GetMonstePosition(x) : GetBossPosition(x);
                        curMonster.gameObject.SetActive(true);
                        curMonster.GetComponent<Collider2D>().enabled = true;
                        curMonster.SetRigidbodySimulated(false);
                        curMonster.SetAnimation(eSpineAnimation.IDLE);
                        defenses.Add(curMonster);
                        curMonster.Controller.StopCO();
                    }
                }
            }
        }
        protected void MonsterAdd()
        {
            var monsterItr = Data.DefensePos.GetEnumerator();
            var monsterDic = Data.DefenseDic;
            //Debug.Log("monster count : " + Data.DefenseDic.Count);
            while (monsterItr.MoveNext())
            {
                var x = monsterItr.Current.Key;
                var list = monsterItr.Current.Value;

                var spineList = new List<BattleSpine>();
                for (int y = 0, count = list.Count; y < count; ++y)
                {
                    var curPos = list[y];
                    if (!monsterDic.ContainsKey(curPos.BattleTag))
                        continue;
                    bool isExist = false;
                    for (int i = 0, countD = defenses.Count; i < countD; ++i)
                    {
                        if (defenses[i].Data.ID == curPos.SpawnID)
                        {
                            isExist = true;
                            break;
                        }
                    }
                    if (isExist)
                        continue;
                    var enemySpine = Stage.GetDefenseSpine(monsterDic[curPos.BattleTag]) as BattleMonsterSpine;
                    enemySpine.transform.localScale = new Vector3(curPos.BattleTag <= 0 ? -1 : 1, 1);
                    //enemySpine.transform.localPosition = GetMonstePosition(x);
                    enemySpine.transform.localPosition = curPos.Type != 1 ? GetMonstePosition(x) : GetBossPosition(x);
                    enemySpine.GetComponent<Collider2D>().enabled = true;
                    enemySpine.SetRigidbodySimulated(false);
                    enemySpine.SetAnimation(eSpineAnimation.IDLE);
                    spineList.Add(enemySpine);
                    defenses.Add(enemySpine);
                }
                if (spineList.Count > 0)
                {
                    Stage.DefenseSpines.Add(spineList);
                    Stage.DefenseSpines.RemoveAll(x => x.Count == 0);
                }

            }
        }


        #endregion

        #region 캐릭터 기본 행동
        protected void MonsterMove(float dt)
        {
            movingTime -= dt;
            for (int x = 0, count = Stage.DefenseSpines.Count; x < count; ++x)
            {
                var monsters = Stage.DefenseSpines[x];
                if (monsters == null)
                    continue;
                foreach (var monster in monsters)
                {
                    if (movingTime < 0 && moving)
                    {
                        monster.Controller.StopCO();
                        monster.SetAnimation(eSpineAnimation.IDLE);
                    }
                    else if (movingTime > 0 && moving == false && SBFunc.RandomValue > 0.5f)
                    {
                        monster.Controller.StopCO();
                        monster.SetAnimation(eSpineAnimation.WALK);
                        float monsterSpeed = SBFunc.Random(monster.Data.MOVE_SPEED, monster.Data.MOVE_SPEED * 2);
                        var targetVec = new Vector3(maxXScale * (SBFunc.RandomValue - 0.5f), SBFunc.Random(minPosY, maxPosY));
                        monster.Controller.MoveLocalTarget(targetVec, 0.01f, true, 100f * monsterSpeed);
                        var rand = SBFunc.Random(0, 4);
                        if (rand == 0)
                        {
                            monster.SetAnimation(eSpineAnimation.IDLE);
                            monster.SetSpeed(0);
                        }
                        else
                        {
                            monster.SetSpeed(monsterSpeed);
                        }
                        //UpdateSinusoidalCharacter(dt, monster.Data);
                    }
                }


            }
            if (movingTime < 0)
            {
                movingTime = moveMaxTime;
                moving = false;
            }
            else
            {
                moving = true;
            }
        }
        protected override void CharacterAction(IBattleCharacterData aData, float dt)
        {
            if (aData == null)
                return;
            var spine = aData.GetSpine();
            if (spine == null)
                return;
            if (aData.Death)
                return;
            if (aData.IsActionSkip())
            {
                if (aData.IsEffectInfo(eSkillEffectType.KNOCK_BACK))
                {
                    if (WallCheck(aData, dt))
                    {
                        for (int i = 0, count = aData.Infos.Count; i < count; ++i)
                        {
                            if (SBDefine.TYPE_KNOCKBACK != aData.Infos[i].GetType())
                                continue;

                            KnockbackEffect eff = (KnockbackEffect)aData.Infos[i];
                            if (eff.IsPlaying)
                            {
                                eff.SetStopBack();
                            }
                        }
                    }
                }
                return;
            }
            var skill = CheckSkill(aData);
            if (skill == null)
                return;

            IBattleCharacterData moveTarget = FindTarget(aData, skill);
            if (moveTarget != null)//이동 할 곳이 있음
            {
                if (aData.IsActioning)
                {
                    spine.ClearEffectSpine();
                    spine.ClearAnimation();
                    aData.SetActiveSkilType(eBattleSkillType.None);
                    aData.SetActionCoroutine(null);
                }

                var goal = GetMoveDestinationPosition(aData, moveTarget);
                var direction = (goal - spine.Controller.transform.position);
                var x = direction.x;
                if (x < 0)
                    spine.Controller.transform.localScale = new Vector3(spine.Controller.IsRight ? Mathf.Abs(spine.Controller.transform.localScale.x) : -Mathf.Abs(spine.Controller.transform.localScale.x), spine.Controller.transform.localScale.y, spine.Controller.transform.localScale.z);
                else if (x > 0)
                    spine.Controller.transform.localScale = new Vector3(spine.Controller.IsRight ? -Mathf.Abs(spine.Controller.transform.localScale.x) : Mathf.Abs(spine.Controller.transform.localScale.x), spine.Controller.transform.localScale.y, spine.Controller.transform.localScale.z);

                goal = direction.normalized * SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED;
                goal += spine.Controller.transform.position;

                spine.Controller.MoveWorldTargetUpdate(dt, goal, false, SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED);

                spine.SetAnimation(eSpineAnimation.WALK);
            }
            else//공격 대상 탐색
            {
                var list = GetSkillCharList(aData, skill);
                if (skill.SkillType is eBattleSkillType.Skill1)
                {
                    if (aData.IsActioning)
                    {
                        spine.ClearEffectSpine();
                        spine.ClearAnimation();
                        aData.SetActiveSkilType(eBattleSkillType.None);
                        aData.SetActionCoroutine(null);
                    }

                    aData.SetActionCoroutine(AttackCoroutine(aData, skill));
                }
                else if (list.Count > 0 && aData.IsAttackSkip() is false)
                {
                    aData.SetActionCoroutine(AttackCoroutine(aData, skill));
                }
                else//정지 => 자기 포지션을 찾아 이동 해당 타겟이 바뀐다면 멈춤
                {
                    spine.SetAnimation(eSpineAnimation.IDLE);

                    var spaceOffset = GetSpaceOffsetVector(aData, true);
                    if (spaceOffset != Vector3.zero)
                    {
                        spaceOffset = spaceOffset.normalized * SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED * 0.5f;
                        spaceOffset += aData.Transform.position;
                        spine.Controller.MoveWorldTargetUpdate(dt, spaceOffset, false, (SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED * 0.5f));
                    }
                }
            }

            WallCheck(aData, dt);
        }

        #endregion

        #region WallCheck
        protected override bool WallCheck(IBattleCharacterData aData, float dt)
        {
            var enemyCenter = GetEnemyOffsetVector(aData);
            var wallCenter = GetWallOffsetVector(aData);
            if (wallCenter != Vector3.zero)
            {
                aData.GetSpine().Controller.MoveLocalTargetUpdate(dt, wallCenter + enemyCenter, false, (SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED));
                return true;
            }
            else if (enemyCenter != Vector3.zero)
            {
                aData.GetSpine().Controller.MoveLocalTargetUpdate(dt, enemyCenter, false, (SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED));
            }
            return false;
        }
        #endregion

        #region 스킬 평타 코루틴 - 스킬, 평타 사용시의 방향성 때문에 상속형태, 에어본과 넉백으로 인해 끼임현상 발생하니깐 빼버림
        protected override IEnumerator NormalCoroutine(IBattleCharacterData casterData, SBSkill skill)
        {
            if (casterData == null || skill == null)
            {
                casterData.EndActionCoroutine();
                yield break;
            }

            var caster = casterData.GetSpine();
            var summonDat = CheckSummon(casterData, skill.GetSummon(0));
            if (summonDat != null && summonDat.Count > 0 && summonDat[0] != null)
            {
                var enemyXpos = summonDat[0].transform.position.x;
                var myXpos = caster.transform.position.x;
                if (caster != null)
                {
                    caster.Controller.MoveWorldTargetUpdate(0f, casterData.Transform.position + (enemyXpos < myXpos ? Vector3.left : Vector3.right));
                }
            }



            float casterSpeed = 1 / casterData.Stat.GetAttackSpeed();

            casterData.SetActiveSkilType(eBattleSkillType.Normal);
            casterData.SetWeakDelay(skill.Skill.WEAK_TIME);
            casterData.SetCastingDelay(skill.Skill.CASTING_TIME);

            var afterDelay = skill.Skill.AFTER_DELAY;
            if (afterDelay < 0.05f)
                afterDelay = 0.05f;
            casterData.SetAfterDelay(afterDelay);


            if (casterData.CastingDelay > 0)
            {
                caster.SetAnimation(eSpineAnimation.A_CASTING);
                while (casterData.CastingDelay > 0)
                {
                    yield return SBDefine.GetWaitForEndOfFrame();
                }
            }

            casterData.SetNormalDelay(casterData.NormalSkill.COOL_TIME);//공속에 관련 있는 노말 스킬만 적용.
            casterData.SetAfterDelay(afterDelay);
            caster.SetAnimation(eSpineAnimation.ATTACK);

            yield return ActiveSkill(casterData, skill);
            casterData.EndActionCoroutine();
            yield break;
        }
        protected override IEnumerator SkillCoroutine(IBattleCharacterData casterData, SBSkill skill)
        {
            if (casterData == null || skill == null)
            {
                casterData.EndActionCoroutine();
                yield break;
            }

            var caster = casterData.GetSpine();
            var summonDat = CheckSummon(casterData, skill.GetSummon(0));
            if (summonDat != null && summonDat.Count > 0 && summonDat[0] != null)
            {
                var enemyXpos = summonDat[0].transform.position.x;
                var myXpos = caster.transform.position.x;
                if (caster != null)
                {
                    caster.Controller.MoveWorldTargetUpdate(0f, casterData.Transform.position + (enemyXpos < myXpos ? Vector3.left : Vector3.right));
                }
            }
            eBattleSide side = GetSide(casterData);

            var queueSkill = Data.GetSkill(casterData, side);
            if (queueSkill.SkillActive(casterData) is false)
            {
                casterData.EndActionCoroutine();
                yield break;
            }

            casterData.SetActiveSkilType(skill.SkillType);
            casterData.SetCastingDelay(skill.Skill.CASTING_TIME);
            casterData.SetWeakDelay(skill.Skill.WEAK_TIME);
            casterData.SetSkill1Delay(skill.Skill.COOL_TIME);
            casterData.SetAfterDelay(skill.Skill.AFTER_DELAY);
            Data.SetGlobalDelay(side, skill.Skill.GLOBAL_COOL_TIME);




            if (casterData.CastingDelay > 0)
            {
                caster.SetAnimation(eSpineAnimation.CASTING);
                while (casterData.CastingDelay > 0)
                {
                    yield return SBDefine.GetWaitForEndOfFrame();
                }
            }

            casterData.SetAfterDelay(skill.Skill.AFTER_DELAY);
            caster.SetAnimation(eSpineAnimation.SKILL);

            yield return ActiveSkill(casterData, skill);
            casterData.SetActionCoroutine(null);
            yield break;
        }
        protected override void SetEffectInfo(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, eBattleSkillType skillType, float exValue, Dictionary<eSkillPassiveStartType, List<SkillPassiveData>> passives, bool isEffect = true)
        {
            if (caster == null || target == null || effect == null)
                return;

            var skillLevel = skillType switch
            {
                eBattleSkillType.Skill1 => caster.SkillLevel,
                _ => 1
            };

            if (IsEffectTargetSkip(effect.TYPE, target))
                return;

            EffectInfo effectInfo;
            switch (effect.TYPE)
            {
                case eSkillEffectType.KNOCK_BACK:
                case eSkillEffectType.AIRBORNE:
                case eSkillEffectType.DEBUFF:
                case eSkillEffectType.D_BUFF:
                case eSkillEffectType.BUFF:
                case eSkillEffectType.BUFF_MAIN_ELEMENT:
                case eSkillEffectType.STUN:
                case eSkillEffectType.FROZEN:
                    return;

                case eSkillEffectType.NONE:
                case eSkillEffectType.DMG:
                case eSkillEffectType.NORMAL_DMG:
                case eSkillEffectType.SKILL_DMG:
                case eSkillEffectType.SKILL_CRI_DMG:
                case eSkillEffectType.SKILL_ELEMENT_DMG:
                case eSkillEffectType.PULL:
#if DEBUG
                    Debug.LogError(SBFunc.StrBuilder("SetEffectInfo => 들어오면 안되는 타입이 들어옴 ->", effect.TYPE.ToString()));
#endif
                    return;
                case eSkillEffectType.DOT:
                case eSkillEffectType.POISON:
                case eSkillEffectType.TICK_DMG:
                    effectInfo = new DotEffect((effectInfo) => DotSkillDamage(caster, target, effectInfo, eBattleSkillType.Skill1, exValue));
                    break;
                case eSkillEffectType.IMMUNE_DMG:
                    effectInfo = new InvincibilityEffect();
                    break;
                case eSkillEffectType.SILENCE:
                    if (target.HasImmunity(eCharacterImmunity.SILENCE) || target.IsEffectInfo(eSkillEffectType.IMN_CC))
                        return;

                    effectInfo = new SilenceEffect();
                    break;
                case eSkillEffectType.SHIELD:
                    effectInfo = new ShieldEffect();
                    break;
                case eSkillEffectType.HEAL:
                    effectInfo = new HealEffect();
                    break;
                case eSkillEffectType.AGGRO:
                    if (target.HasImmunity(eCharacterImmunity.AGGRO) || target.IsEffectInfo(eSkillEffectType.IMN_AGGRO, eSkillEffectType.IMN_CC))
                        return;

                    effectInfo = new AggroEffect();
                    break;
                case eSkillEffectType.AGGRO_R:
                    effectInfo = new ReverseAggroEffect();
                    break;
                case eSkillEffectType.STAT:
                    effectInfo = new StatEffect();
                    break;
                case eSkillEffectType.D_ABUFF:
                    effectInfo = new AllCancleBuffEffect();
                    break;
                case eSkillEffectType.D_DEBUFF:
                    effectInfo = new CancleDebuffEffect();
                    break;
                case eSkillEffectType.D_ADEBUFF:
                    effectInfo = new AllCancleDebuffEffect();
                    break;
                case eSkillEffectType.D_DOT:
                    effectInfo = new CancleDotEffect();
                    break;
                case eSkillEffectType.D_SHIELD:
                    effectInfo = new AllCancleShieldEffect();
                    break;
                case eSkillEffectType.D_STUN:
                    effectInfo = new CancleStunEffect();
                    break;
                case eSkillEffectType.D_AGGRO:
                    effectInfo = new CancleAggroEffect();
                    break;
                case eSkillEffectType.ENV_BUFF:
                    effectInfo = new EnvBuffEffect();
                    break;
                case eSkillEffectType.IMN_STUN:
                    effectInfo = new IMStunEffect();
                    break;
                case eSkillEffectType.IMN_AGGRO:
                    effectInfo = new IMAggroEffect();
                    break;
                case eSkillEffectType.IMN_AIR:
                    effectInfo = new IMAirborneEffect();
                    break;
                case eSkillEffectType.IMN_PULL:
                    effectInfo = new IMPullEffect();
                    break;
                case eSkillEffectType.IMN_KNOCK:
                    effectInfo = new IMKnockbackEffect();
                    break;
                case eSkillEffectType.IMN_CC:
                    effectInfo = new IMCCEffect();
                    break;
                default:
#if DEBUG
                    Debug.LogError("SetEffectInfo => 정의되지 않은 타입이 들어옴");
#endif
                    return;
            }

            if (effectInfo == null)
                return;

            effectInfo.SetEffectData(caster, target, effect, skillLevel);
            if (target.SetEffectInfo(effectInfo))
                effectInfo.SetFollowEffect(CreateFollowEffect(SkillResourceData.Get(effect.TARGET_EFFECT_RSC_KEY), effect, caster, target, effectInfo.MaxTime));
        }
        protected override IBattleCharacterData CheckCastingTarget(IBattleCharacterData caster, SBSkill skill)
        {
            var target = base.CheckCastingTarget(caster, skill);
            if (target == null)
                return target;

            if (caster.PriorityTarget == null)
            {
                caster.AddPriorityTarget(target, 100);
            }
            return target;
        }
        protected override void SetEffectInfo(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, SBSkill skill, float exValue, Dictionary<eSkillPassiveStartType, List<SkillPassiveData>> passives, bool isEffect = true)  // 장판 스킬 pull 타입을 하지 않기 위해 오버라이드 함
        {
            if (caster == null || target == null || effect == null)
                return;
            switch (effect.TYPE)
            {
                case eSkillEffectType.PULL:
                    return;
                default:
                    SetEffectInfo(caster, target, effect, skill.SkillType, exValue, passives, isEffect);
                    return;
            }
        }
        protected override List<BattleSpine> GetSkillSummonList(IBattleCharacterData caster, SkillSummonData summon)
        {
            if (summon == null)
                return null;

            var list = GetTargetList(caster, summon.TARGET_TYPE);
            if (caster.Transform == null)
                return list;
            SortTargetList(list, caster, caster.Transform.position, summon.TARGET_SORT);
            //도발 걸리면 우선순위 업 폭발에는 적용하면 안됨
            list.Sort((d1, d2) => SortPriority(caster.GetSpine(), d1, d2));

            return list;
        }
        protected override Vector3 GetMoveDestinationPosition(IBattleCharacterData mine, IBattleCharacterData target)
        {
            if (mine == null || target == null)
                return Vector3.zero;
            if (target.Transform == null)
                return mine.Transform.position;

            Vector3 position = (GetCircleColliderPos(mine.GetCircleCollider())
                - GetCircleColliderPos(target.GetCircleCollider())).normalized
                * GetCircleColliderRadius(mine.GetCircleCollider());

            return target.Transform.position + position;
        }
        protected override void ArrowTrigger(IBattleCharacterData casterData, SBSkill skill, int idx)
        {
            var summon = skill.GetSummon(idx);
            if (summon == null)
                return;

            var targets = CheckSummon(casterData, summon);
            if (targets == null)
                return;

            switch (summon.TARGET)
            {
                case eSkillTarget.CENTER:
                {
                    var center = Vector3.zero;
                    var posX = 0f;
                    var posY = 0f;
                    int count = targets.Count;
                    if (count < 1)
                        return;

                    for (int i = 0; i < count; ++i)
                    {
                        var target = targets[i];
                        if (target == null)
                            continue;

                        posX += target.transform.position.x + 10000f;
                        posY += target.transform.position.y + 10000f;
                    }
                    center.x = posX / count - 10000f;
                    center.y = posY / count - 10000f;

                    var arrowResourceData = summon.GetArrowResource();
                    var curArrow = CreateFieldEffect(arrowResourceData, casterData, casterData, Vector3.zero, casterData.Transform.localScale);
                    if (curArrow != null)
                    {
                        var sbProjectile = curArrow.GetComponent<SBProjectileCenter>();
                        if (sbProjectile == null)
                        {
                            if (arrowResourceData != null && arrowResourceData.IMAGE != "NONE")
                                sbProjectile = curArrow.AddComponent<SBProjectileCenterGeneric>();
                            else
                                sbProjectile = curArrow.AddComponent<SBProjectileCenter>();
                        }

                        sbProjectile.SetAutoDirection(arrowResourceData);
                        sbProjectile.Set(casterData, center, skill, () =>
                        {
                            if (summon.SKILL_EFFECT_RSC_KEY > 0)
                            {
                                var skillEffect = CreateFieldEffect(summon.GetEffectResource(), casterData, skill.TargetPosition, Vector3.zero, skill.TargetScale);
                                if (skillEffect != null)
                                    skillEffect.transform.position = center;
                            }
                            var effects = summon.GetEffects();
                            if (effects == null)
                            {
                                if (skill.HitSummon())
                                {
                                    var curSummon = skill.GetSummon(idx + 1);
                                    if (curSummon == null)
                                        return;

                                    var curSkill = new SBSkill();
                                    curSkill.SetCastData(casterData, skill.Skill, curSummon, skill.SkillType);
                                    curSkill.SetTargetPos(center);
                                    Stage.StartCoroutine(ActiveSkill(casterData, curSkill));
                                }
                                return;
                            }

                            for (int i = 0, count = effects.Count; i < count; ++i)
                            {
                                var effect = effects[i];
                                if (effect == null)
                                    continue;

                                EffectTriggerExplosion(casterData, center, effect, skill);
                            }

                            if (skill.HitSummon())
                            {
                                var curSummon = skill.GetSummon(idx + 1);
                                if (curSummon == null)
                                    return;

                                var curSkill = new SBSkill();
                                curSkill.SetCastData(casterData, skill.Skill, curSummon, skill.SkillType);
                                curSkill.SetTargetPos(center);
                                Stage.StartCoroutine(ActiveSkill(casterData, curSkill));
                            }
                        }, idx);
                    }
                }
                break;
                case eSkillTarget.NONE:
                default:
                {
                    for (int i = 0, count = targets.Count; i < count; ++i)
                    {
                        var targetSpine = targets[i];
                        var arrowResourceData = summon.GetArrowResource();
                        var curArrow = CreateFieldEffect(arrowResourceData, casterData, skill.TargetPosition, GetAddedPosition(casterData, summon), casterData.Transform.localScale);
                        if (curArrow != null)
                        {
                            var sbProjectile = curArrow.GetComponent<SBProjectileTarget>();
                            if (sbProjectile == null)
                            {
                                if (arrowResourceData != null && arrowResourceData.IMAGE != "NONE")
                                    sbProjectile = curArrow.AddComponent<SBProjectileTargetGeneric>();
                                else
                                    sbProjectile = curArrow.AddComponent<SBProjectileTarget>();
                            }

                            VoidDelegate func = () =>
                            {
                                var effectPos = targetSpine.transform.position;
                                if (summon.SKILL_EFFECT_RSC_KEY > 0)
                                    CreateFieldEffect(summon.GetEffectResource(), casterData, targetSpine.Data, Vector2.zero, casterData.Transform.localScale);

                                var effects = summon.GetEffects();
                                if (effects == null)
                                {
                                    if (skill.HitSummon())
                                    {
                                        var curSummon = skill.GetSummon(idx + 1);
                                        if (curSummon == null)
                                            return;

                                        var curSkill = new SBSkill();
                                        curSkill.SetCastData(casterData, skill.Skill, curSummon, skill.SkillType, targetSpine.transform);
                                        Stage.StartCoroutine(ActiveSkill(casterData, curSkill));
                                    }
                                    return;
                                }

                                for (int i = 0, count = effects.Count; i < count; ++i)
                                {
                                    if (effects[i] == null)
                                        continue;
                                    EffectTrigger(casterData, targetSpine.Data, effects[i], skill);
                                }

                                if (skill.HitSummon())
                                {
                                    var curSummon = skill.GetSummon(idx + 1);
                                    if (curSummon == null)
                                        return;

                                    var curSkill = new SBSkill();
                                    curSkill.SetCastData(casterData, skill.Skill, curSummon, skill.SkillType, targetSpine.transform);
                                    Stage.StartCoroutine(ActiveSkill(casterData, curSkill));
                                }
                            };
                            sbProjectile.SetAutoDirection(arrowResourceData);
                            sbProjectile.Set(casterData, targetSpine.Data, skill, func, idx);
                            projectileObjs.Add(sbProjectile.gameObject);
                        }
                    }
                }
                break;
            }
        }
        protected void RemoveAllProjectile()
        {
            foreach (var obj in projectileObjs)
            {
                if (obj != null)
                    Object.Destroy(obj);
            }
            projectileObjs.Clear();
        }

        protected override int CriCalc(IBattleCharacterData caster, IBattleCharacterData target, SkillLevelStat skillStat, eBattleSkillType type)
        {
            if (target.IsEffectInfo(eSkillEffectType.IMMUNE_DMG))
                return 0;

            if (skillStat.CRI < 1)
                return 0;

            var resis = 1f - target.Stat.GetTotalStatusConvert(eStatusType.CRI_DMG_RESIS);
            if (resis < 0f)
                resis = 0f;

            float DMG = caster.Stat.GetStatus(eStatusCategory.BASE, eStatusType.CRI_DMG);
            float ratio = 1.0f + (caster.Stat.GetStatus(eStatusCategory.RATIO, eStatusType.CRI_DMG) + caster.Stat.GetStatus(eStatusCategory.RATIO_BUFF, eStatusType.CRI_DMG)) * SBDefine.CONVERT_FLOAT;
            float added = caster.Stat.GetStatus(eStatusCategory.ADD, eStatusType.CRI_DMG) + caster.Stat.GetStatus(eStatusCategory.ADD_BUFF, eStatusType.CRI_DMG);
            if (Data.BattleType == eBattleType.ARENA || Data.BattleType == eBattleType.ChampionBattle)
            {
                ratio += caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PVP_CRI_DMG);
                added += caster.Stat.GetTotalStatus(eStatusType.ADD_PVP_CRI_DMG);
            }
            var C_DMG = DMG * ratio + added;
            C_DMG = SBFunc.CalcRatio(C_DMG, skillStat.CRI) * resis;
            if (C_DMG < 1)
                C_DMG = 1;

            return Mathf.FloorToInt(C_DMG);
        }
        #endregion

        #region 데미지 -  UI에 데미지 수치를 표기하지 않기 위해서 따로 둠
        protected override void Damage(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, eBattleSkillType type, float exValue = 1, Dictionary<eSkillPassiveStartType, List<SkillPassiveData>> passives = null, bool isEffect = true)
        {
            if (target == null || target.Death || target.Untouchable)
                return;

            var spine = target.GetSpine();
            var isINVINCIBILITY = target.IsEffectInfo(eSkillEffectType.IMMUNE_DMG);
            if (!isINVINCIBILITY)
            {
                if (spine != null)
                {
                    spine.Hit();
                    spine.KnockBackHit();
                }
            }
            if (target.IsEnemy == false)
                return;

            if (effect != null)
            {
                int skillLevel = type == eBattleSkillType.Skill1 ? caster.SkillLevel : 1;
                var skilLevelStat = effect.GetEffectStat(skillLevel);
                var BD = BaseCalc(caster, target, skilLevelStat, type);
                var ED = ElementCalc(caster, target, skilLevelStat, type);

                var rnd = SBFunc.Random(0, 100);
                if (skilLevelStat.CRI <= 0)
                    rnd = int.MaxValue;

                var element = eDamageType.ELEMENT_NORMAL;
                var eType = rnd < (caster.Stat.GetTotalStatus(eStatusType.CRI_PROC) - target.Stat.GetTotalStatusConvert(eStatusType.CRI_RESIS)) ? eDamageType.CRITICAL : element;

                if (eType == eDamageType.CRITICAL)
                {
                    var criDMG = CriCalc(caster, target, skilLevelStat, type);
                    if (criDMG < 1)
                        eType = element;
                    else
                        BD += criDMG;
                }

                var DMG = Mathf.FloorToInt((BD + ED) * exValue * skilLevelStat.VALUE * SBDefine.CONVERT_FLOAT);
                if (DMG < 1)
                    DMG = 1;

                if (effect.TARGET_EFFECT_RSC_KEY > 0)
                    CreateFieldEffect(SkillResourceData.Get(effect.TARGET_EFFECT_RSC_KEY), caster, target, Vector2.zero, target.IsEnemy ? Vector3.one : Vector3.one * 0.5f);

                if (FloorData.IsBuffState)
                {
                    SetDamage(caster, target, Mathf.FloorToInt(DMG * BuffDamage), type == eBattleSkillType.Skill1);
                }
                else
                {
                    SetDamage(caster, target, DMG, type == eBattleSkillType.Skill1);
                }
            }
        }
        protected override void DotSkillDamage(IBattleCharacterData caster, IBattleCharacterData target, EffectInfo effect, eBattleSkillType type, float exValue = 1f)
        {
            if (target == null || target.Death || target.Untouchable)
                return;
            if (target.IsEnemy == false)
                return;
            if (effect != null)
            {
                int skillLevel = type == eBattleSkillType.Skill1 ? caster.SkillLevel : 1;
                var skilLevelStat = effect.Data.GetEffectStat(skillLevel);
                var BD = BaseCalc(caster, target, skilLevelStat, type);
                var ED = ElementCalc(caster, target, skilLevelStat, type);

                var rnd = SBFunc.Random(0, 100);
                if (skilLevelStat.CRI <= 0)
                    rnd = int.MaxValue;

                var element = eDamageType.ELEMENT_NORMAL;
                var eType = rnd < (caster.Stat.GetTotalStatus(eStatusType.CRI_PROC) - target.Stat.GetTotalStatusConvert(eStatusType.CRI_RESIS)) ? eDamageType.CRITICAL : element;

                if (eType == eDamageType.CRITICAL)
                {
                    var criDMG = CriCalc(caster, target, skilLevelStat, type);
                    if (criDMG < 1)
                        eType = element;
                    else
                        BD += criDMG;
                }

                var DMG = Mathf.FloorToInt((BD + ED) / effect.Data.HIT_COUNT * exValue * skilLevelStat.VALUE * SBDefine.CONVERT_FLOAT);
                if (DMG < 1)
                    DMG = 1;
                SetDamage(caster, target, DMG, type == eBattleSkillType.Skill1);
            }
        }
        #endregion

        #region 드래곤, 몬스터 시작 위치
        protected Vector3 GetDragonPosition(int x) // 드래곤의 배틀 테그는 0,1,2,3,4,5 이렇게 주어지므로 오른쪽으로 쏠릴수 있으니
        {                                           // -3~3 범위로 설정
            float distanceIndex = (x + 1) % 2 == 0 ? x / 2f : -(x + 1) / 2f;
            return new Vector3((Stage.DragonSpancingX * distanceIndex), dragonStartYpos, 0f);
        }
        protected Vector3 GetMonstePosition(int x)
        {
            var townMapLimitSize = (x > 0) ? TownMap.Width * 1.6f : -TownMap.Width * 1.6f;
            return new Vector3(townMapLimitSize - Stage.MonsterSpancingX * x, SBFunc.Random(minPosY, maxPosY), 0f);
        }
        protected Vector3 GetBossPosition(int x) // 보스몬스터는 생각보다 너무 커서 좀더 안쪽에서 소환될 필요가 있음
        {
            bool isRight = (x > 0);
            var townMapLimitSize = isRight ? TownMap.Width * 1.6f : -TownMap.Width * 1.6f;
            return new Vector3(townMapLimitSize - Stage.MonsterSpancingX * (x + (isRight ? 1.5f : -1.5f)), SBFunc.Random(minPosY, maxPosY), 0f);
        }
        #endregion

        #region ETC
        protected override void UpdateSounds(float dt) { } // 젬던전에서는 사운드가 실행되면 안됨
        #endregion

        #region SoundDisable
        protected override GameObject CreatePrefab(SkillResourceData data)
        {
            return SoundDisable(base.CreatePrefab(data));
        }
        protected override GameObject CreatePrefabEffect(SkillResourceData data, IBattleCharacterData casterData, IBattleCharacterData targetData, Vector2 pos, Vector3 scale)
        {
            return SoundDisable(base.CreatePrefabEffect(data, casterData, targetData, pos, scale));
        }
        protected override GameObject CreatePrefabEffect(SkillResourceData data, IBattleCharacterData casterData, Vector3 position, Vector3 addedPos, Vector3 scale)
        {
            return SoundDisable(base.CreatePrefabEffect(data, casterData, position, addedPos, scale));
        }
        protected override GameObject CreateFieldEffect(SkillResourceData data, IBattleCharacterData casterData, Vector3 position, Vector2 addedPos, Vector3 scale)
        {
            return SoundDisable(base.CreateFieldEffect(data, casterData, position, addedPos, scale));
        }
        protected override GameObject CreateFieldEffect(SkillResourceData data, IBattleCharacterData casterData, IBattleCharacterData targetData, Vector2 addedPos, Vector3 scale)
        {
            return SoundDisable(base.CreateFieldEffect(data, casterData, targetData, addedPos, scale));
        }
        protected override SBFollowObject CreateFollowEffect(SkillResourceData data, SkillEffectData effect, IBattleCharacterData caster, IBattleCharacterData target, float duration)
        {
            var effectObj = base.CreateFollowEffect(data, effect, caster, target, duration);
            if (effectObj != null)
                SoundDisable(effectObj.Effect.gameObject);
            return effectObj;
        }
        protected GameObject SoundDisable(GameObject obj)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj.GetComponent<SFXPlayer>());
            }
            return obj;
        }
        #endregion
    }

}

