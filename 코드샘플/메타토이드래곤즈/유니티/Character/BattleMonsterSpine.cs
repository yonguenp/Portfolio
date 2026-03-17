using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SandboxNetwork
{
    public class BattleMonsterSpine : BattleSpine
    {
        public BattleMonsterData CData
        {
            get { return Data as BattleMonsterData; }
            protected set { Data = value; }
        }
        protected MaterialPropertyBlock blackColor = null;
        private int blackID = 0;
        private Color defaultColor = Color.black;
        private Color hitColor = new Color(0.6f, 0.6f, 0.6f);
        private IEnumerator hitWait = null;
        private EffectSpine playAnim = null;

        public override void InitializeTypeFunc()
        {
            GetTypeToName = SBDefine.GetMonsterAnimTypeToName;
            GetTypeToLoop = SBFunc.IsTypeToLoop;
            GetNameToType = SBDefine.GetMonsterAnimNameToType;
            GetTypeToSkip = SBFunc.IsAnimSkip;
        }
        public override void Init()
        {
            base.Init();

            if (CData != null && CData.BaseData != null)
            {
                InitAnimation();
            }

            skeletonAni.AnimationState.Start += StartHandleAnimation;
            skeletonAni.AnimationState.Complete += CompleteHandleAnimation;

            if (mesh != null)
            {
                blackColor = new MaterialPropertyBlock();
                blackID = Shader.PropertyToID("_Black");
                blackColor.SetColor(blackID, defaultColor);
                mesh.SetPropertyBlock(blackColor);
            }

            if (StatusEffect == null)
            {
                StatusEffect = GetComponent<statusEffect>();
            }
            hitWait = SBDefine.GetWaitForSeconds(0.15f);
        }

        protected virtual void InitAnimation()
        {
            //SetSkin(Data.BaseData.SKIN);
            Animation = eSpineAnimation.NONE;
            InitAnimation(eSpineAnimation.IDLE);
        }
        public void DeathSkip()
        {
            skeletonAni.timeScale = GetAnimaionTime(eSpineAnimation.DEATH);
        }

        protected override IEnumerator DeathCO()
        {
            SetShadow(false);
            if (Data.IsBoss)
            {
                BossDeathEvent.Send();
                SetAnimation(eSpineAnimation.DEATH);
                ClearEffectSpine();
                yield break;
            }

            yield return base.DeathCO();
        }
        public override TrackEntry SetAnimation(eSpineAnimation anim)
        {
            return base.SetAnimation(anim);
        }
        public override void Hit()
        {
            if (Data != null)
            {
                if (Data.Death)
                    return;
            }
            StopCoroutine(nameof(HitCO));
            StartCoroutine(nameof(HitCO));
        }
        protected virtual IEnumerator HitCO()
        {
            if (mesh == null)
                yield break;

            blackColor.SetColor(blackID, hitColor);
            mesh.SetPropertyBlock(blackColor);
            yield return hitWait;
            blackColor.SetColor(blackID, defaultColor);
            mesh.SetPropertyBlock(blackColor);
            yield break;
        }
        protected virtual void StartHandleAnimation(TrackEntry trackEntry)
        {
            if (Animation == eSpineAnimation.DEATH || (Data != null && Data.Death))
            {
                skeletonAni.timeScale = defaultScale * SBGameManager.Instance.TimeScale;
                return;
            }

            switch (trackEntry.Animation.Name)
            {
                case "monster_attack":
                    skeletonAni.timeScale = GetAnimScale(eSpineAnimation.ATTACK);
                    break;
                case "monster_casting":
                    skeletonAni.timeScale = GetAnimScale(eSpineAnimation.CASTING);
                    break;
                case "monster_skill1":
                    skeletonAni.timeScale = GetAnimScale(eSpineAnimation.SKILL);
                    break;
                case "monster_walk":
                    skeletonAni.timeScale = GetAnimScale(eSpineAnimation.WALK);
                    break;
                case "monster_hit":
                case "monster_idle":
                default:
                    skeletonAni.timeScale = GetAnimScale(eSpineAnimation.IDLE);
                    break;
            }
        }

        protected virtual void CompleteHandleAnimation(TrackEntry trackEntry)
        {
            if (Animation == eSpineAnimation.DEATH || Data.Death)
            {
                switch (trackEntry.Animation.Name)
                {
                    case "monster_death":
                        isDeath = true;
                        break;
                }
                return;
            }

            switch (trackEntry.Animation.Name)
            {
                case "monster_attack":
                    Animation = eSpineAnimation.NONE;
                    SetAnimation(eSpineAnimation.IDLE);
                    break;
                case "monster_casting":
                    Animation = eSpineAnimation.NONE;
                    SetAnimation(eSpineAnimation.SKILL);
                    break;
                case "monster_skill1":
                    Animation = eSpineAnimation.NONE;
                    SetAnimation(eSpineAnimation.IDLE);
                    break;
                case "monster_hit":
                    Animation = eSpineAnimation.NONE;
                    SetAnimation(eSpineAnimation.IDLE);
                    break;
                case "monster_walk":
                    Animation = eSpineAnimation.NONE;
                    SetAnimation(eSpineAnimation.IDLE);
                    break;
            }
        }

        public override void UpdateStatus(float dt)
        {
            Data.Update(dt);
            if (Data.Death)
                return;
            //KnockBack
            KnockbackEffect();
            //
        }

        public void SetSpineAnimFrame(float _setTime)
        {
            if (skeletonAni != null)
            {
                skeletonAni.Update(_setTime);
            }
        }
        public override void ClearEffectSpine()
        {
            if (playAnim == null)
                return;

            playAnim.CallBack = null;
            Destroy(playAnim.gameObject);
            playAnim = null;
        }
    }
}