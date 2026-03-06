using UnityEngine;
using Spine.Unity;
using System.Collections.Generic;
using Spine;

namespace SandboxNetwork
{
    public class BattleWorldBossPartsSpine : BattleMonsterSpine
    {
        [SerializeField]
        private bool isLeft = false;
        [SerializeField]
        private string StartAnimation = "";
        [SerializeField]
        private string ShowAnimation = "";
        [SerializeField]
        private string IdleAnimation = "";
        [SerializeField]
        private string CastingAnimation = "";
        [SerializeField]
        private string AttackAnimation = "";
        [SerializeField]
        private string DeathAnimation = "";
        [SerializeField]
        private bool formChange = false;
        [SerializeField]
        private bool levelUpChange = false;

        public override SBController Controller
        {
            get { return BossData.GetSpine().Controller; }
        }

        public BattleWorldBossPartsData BattleData { get; private set; } = null;
        public BattleWorldBossData BossData { get; private set; } = null;
        public WorldBossPartData PartData { get; private set; } = null;

        public List<BattleWorldBossPartsSpine> childSpines = new List<BattleWorldBossPartsSpine>();

        public Transform DamageTransform { get; private set; } = null;

        bool loaded = false;
        
        public void OnDisable()
        {
            foreach (var child in childSpines)
            {
                child.gameObject.SetActive(false);
            }
        }

        public bool SetReadyAnimation()
        {
            if (loaded == false)
            {
                if (SkeletonAni != null && SkeletonAni.skeletonDataAsset != null)
                {
                    var animation = SkeletonAni.skeletonDataAsset.GetSkeletonData(true).FindAnimation("ready");
                    if (animation != null)
                    {
                        base.Init();

                        SetAnimation(0, "ready", true);
                        loaded = true;

                        return true;
                    }
                }
            }

            return false;
        }
        public override void Init()
        {
            if (loaded == false)
            {
                base.Init();

                if (OnStartAnimation() == null)
                    OnShowAnimation();
            }

            loaded = true;

            foreach (var child in childSpines)
            {
                child.gameObject.SetActive(true);
            }

            DamageTransform = transform;
            var collider = GetComponent<CircleCollider2D>();
            if (collider != null)
            {
                DamageTransform = new GameObject("DamageTransform").transform;
                DamageTransform.SetParent(transform);

                DamageTransform.localScale = Vector3.one;
                DamageTransform.position = new Vector3(transform.position.x + (collider.offset.x * transform.lossyScale.x), transform.position.y + (collider.offset.y * transform.lossyScale.y), 0);

                StatusEffect.SetEffectTransform(DamageTransform);
            }
        }
        public bool SetData(BattleWorldBossData bossData, WorldBossPartData partsData)
        {
            BossData = bossData;
            PartData = partsData;
            BattleData = BattleWorldBossPartsData.Create(BossData, PartData);
            SetData(BattleData);
            BattleData.SetSpine(this);
            BattleData.SetPartsDirection(isLeft);
            
            //if(IsTargetingPart())
            //{
            //	WorldBossStage.Instance.SetRedHpBar(BattleData);
            //}

            return Data != null;
        }

        public override void InitializeComponent()
        {
            if (spineObj == null)
                spineObj = this.gameObject;

            if (skeletonAni == null && !spineObj.TryGetComponent(out skeletonAni))
                skeletonAni = spineObj.AddComponent<SkeletonAnimation>();

            if (outlineRenderer == null)
                outlineRenderer = GetComponentInChildren<OutlineRenderer>();
        }

        public bool IsScarecrow()
        {
            if (PartData != null)
            {
                if (PartData.PARTS_TYPE == 1 /*&& PartData.ACTIVE_LEVEL == 1*/)
                {
                    return true;
                }
            }

            return false;
        }

        public override void SetDamage(int value, IBattleCharacterData caster)
        {
            if (BossData == null)
            {
                Death();
                return;
            }

            if (BossData.Stat.GetTotalStatusInt(eStatusType.SHIELD_POINT) > 0)
            {
                for (int i = 0, count = BossData.Infos.Count; i < count; ++i)
                {
                    if (BossData.Infos[i] == null)
                        continue;

                    value = BossData.Infos[i].SetDamage(value);
                    if (value == 0)
                        break;
                }
            }

            BossData.OnDamage(value, caster);
        }

        public override void KnockBackHit()
        {
            return;
        }
        public List<int> GetAttackPartyIndexs()
        {
            if (PartData != null)
            {
                //같은 로직이 있어서 통합
                return PartData.GetAttackPartyIndexs();
            }
            return new();
        }

        public virtual TrackEntry OnStartAnimation()
        {
            if (string.IsNullOrEmpty(StartAnimation))
                return null;

            OnIdleAnimation();

            Debug.Log("OnStartAnimation : " + gameObject.name);
            var track = SetAnimation(0, StartAnimation, false);

            foreach (var child in childSpines)
            {
                child.OnStartAnimation();
            }

            return track;
        }

        public virtual TrackEntry OnIdleAnimation()
        {
            if (string.IsNullOrEmpty(IdleAnimation))
                return null;

            formChangeCheck();

            return SetAnimation(0, IdleAnimation, true);
        }

        
        public virtual TrackEntry OnShowAnimation()
        {
            if (string.IsNullOrEmpty(ShowAnimation))
                return OnIdleAnimation();

            Debug.Log("OnShowAnimation : " + gameObject.name);
            var track = SetAnimation(0, ShowAnimation, false);

            return track;
        }

        public virtual TrackEntry OnCastingAnimation()
        {
            if (string.IsNullOrEmpty(CastingAnimation))
                return null;

            formChangeCheck();

            var track = SetAnimation(0, CastingAnimation, false);

            return track;
        }

        public virtual TrackEntry OnAttackAnimation()
        {
            if (string.IsNullOrEmpty(AttackAnimation))
                return null;

            formChangeCheck();

            var track = SetAnimation(0, AttackAnimation, false);

            return track;
        }

        public virtual TrackEntry OnDeathAnimation()
        {
            if (string.IsNullOrEmpty(DeathAnimation))
                return null;

            var track = SetAnimation(0, DeathAnimation, false);

            return track;
        }



        public override TrackEntry SetAnimation(eSpineAnimation anim)
        {
            if (loaded == false || (
                CurrentTrack != null  && CurrentTrack.Animation != null && (
                    CurrentTrack.Animation.Name == StartAnimation
                    || CurrentTrack.Animation.Name == ShowAnimation
                    || CurrentTrack.Animation.Name == AttackAnimation)))
            {
                return null;                
            }

            switch (anim)
            {
                case eSpineAnimation.IDLE:
                    return OnIdleAnimation();
                case eSpineAnimation.CASTING:
                case eSpineAnimation.A_CASTING:
                    return OnCastingAnimation();
                case eSpineAnimation.SKILL:
                case eSpineAnimation.ATTACK:
                    return OnAttackAnimation();
                case eSpineAnimation.DEATH:
                    return OnDeathAnimation();
            }
            return null;
        }

        public void formChangeCheck()
        {
            if(formChange)
            {
                bool change = true;

                if (change)
                {                    
                    if (!isLeft)
                    {
                        int startIndex = eWorldBoss.POS_BOTTOM_LEFT * WorldBossFormationData.MAX_DRAGON_COUNT;
                        for (int i = startIndex; i < startIndex + WorldBossFormationData.MAX_DRAGON_COUNT; i++)
                        {
                            int index = i + 1;
                            if (!WorldBossManager.Instance.Data.OffenseDic.ContainsKey(index))
                            {
                                continue;
                            }
                            var target = WorldBossManager.Instance.Data.OffenseDic[index];
                            if (target != null && !target.Death)
                            {
                                change = false;
                                break;
                            }
                        }
                    }
                }

                if (change)
                {
                    if (isLeft)
                    {
                        int startIndex = eWorldBoss.POS_BOTTOM_RIGHT * WorldBossFormationData.MAX_DRAGON_COUNT;
                        for (int i = startIndex; i < startIndex + WorldBossFormationData.MAX_DRAGON_COUNT; i++)
                        {
                            int index = i + 1;
                            if (!WorldBossManager.Instance.Data.OffenseDic.ContainsKey(index))
                            {
                                continue;
                            }
                            var target = WorldBossManager.Instance.Data.OffenseDic[index];
                            if (target != null && !target.Death)
                            {
                                change = false;
                                break;
                            }
                        }
                    }
                }

                if (!change)
                    return;

                StartAnimation = StartAnimation.Replace("_b", "_t");
                ShowAnimation = ShowAnimation.Replace("_b", "_t");
                IdleAnimation = IdleAnimation.Replace("_b", "_t");
                CastingAnimation = CastingAnimation.Replace("_b", "_t");
                AttackAnimation = AttackAnimation.Replace("_b", "_t");
                DeathAnimation = DeathAnimation.Replace("_b", "_t");

                formChange = false;
            }
        }

        public void OnLevelUp()
        {
            if (!levelUpChange)
                return;

            if (BossData.Level < 5)
                return;

            IdleAnimation = "idle2";

            OnShowAnimation();

            levelUpChange = false;
        }

        protected override void CompleteHandleAnimation(TrackEntry trackEntry)
        {
            if(!trackEntry.Loop)
            {
                OnIdleAnimation();
            }
        }
    }
}