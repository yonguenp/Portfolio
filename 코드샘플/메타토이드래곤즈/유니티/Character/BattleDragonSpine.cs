using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;

namespace SandboxNetwork
{
    public class BattleDragonSpine : BattleSpine
    {
        public BattleDragonData CData
        {
            get { return Data as BattleDragonData; }
            protected set { Data = value; }
        }

        protected MaterialPropertyBlock blackColor = null;
        protected int blackID = 0;
        protected Color defaultColor = Color.black;
        protected Color hitColor = new Color(0.6f, 0.6f, 0.6f);
        private IEnumerator hitWait = null;
        private EffectSpine playAnim = null;

        public string GetAnimName(eSpineAnimation anim)
        {
            switch (anim)
            {
                case eSpineAnimation.A_CASTING:
                case eSpineAnimation.ATTACK:
                    return SBDefine.GetDragonAnimTypeToName(anim, 1);
                case eSpineAnimation.CASTING:
                case eSpineAnimation.SKILL:
                    return SBDefine.GetDragonAnimTypeToName(anim, Data.Skill1.ANI);
                default:
                    return SBDefine.GetDragonAnimTypeToName(anim, 1);
            }
        }

        public override void InitializeTypeFunc()
        {
            GetTypeToName = GetAnimName;
            GetTypeToLoop = SBFunc.IsTypeToLoop;
            GetNameToType = SBDefine.GetDragonAnimNameToType;
            GetTypeToSkip = SBFunc.IsAnimSkip;
        }

        public override void Init()
        {
            base.Init();
            if (StatusEffect == null)
                StatusEffect = GetComponent<statusEffect>();

            if (CData != null)
            {
                SetSkin(CData.BaseData.SKIN);
                SetAnimation(eSpineAnimation.IDLE);
                SetTranscendEffect(CData.TranscendenceData.Step);
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

            hitWait = SBDefine.GetWaitForSeconds(0.15f);

            if (outlineRenderer != null)
            {
                outlineRenderer.Sync();
                SetOutline(false);
            }
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
        protected virtual void CompleteHandleAnimation(TrackEntry trackEntry)
        {
            if (Animation == eSpineAnimation.DEATH)
                return;

            switch (trackEntry.Animation.Name)
            {
                case "acasting_ani1":
                case "acasting_ani2":
                case "acasting_ani3":
                case "acasting_ani4":
                    SetAnimation(eSpineAnimation.ATTACK);
                    break;
                case "atk_ani1":
                case "atk_ani2":
                    Animation = eSpineAnimation.NONE;
                    SetAnimation(eSpineAnimation.IDLE);
                    break;
                case "scasting_ani1":
                case "scasting_ani2":
                case "scasting_ani3":
                case "scasting_ani4":
                    SetAnimation(eSpineAnimation.SKILL);
                    break;
                case "skill_ani1":
                case "skill_ani2":
                case "skill_ani3":
                case "skill_ani4":
                    Animation = eSpineAnimation.NONE;
                    SetAnimation(eSpineAnimation.IDLE);
                    break;
            }
        }

        protected virtual void StartHandleAnimation(TrackEntry trackEntry)
        {
            if (Animation == eSpineAnimation.DEATH || Data.Death)
            {
                skeletonAni.timeScale = SBGameManager.Instance.TimeScale;
                return;
            }

            switch (trackEntry.Animation.Name)
            {
                case "acasting_ani1":
                case "acasting_ani2":
                case "acasting_ani3":
                case "acasting_ani4":
                    trackEntry.MixDuration = 0f;
                    skeletonAni.timeScale = GetAnimScale(eSpineAnimation.A_CASTING);
                    break;
                case "atk_ani1":
                case "atk_ani2":
                    trackEntry.MixDuration = 0f;
                    skeletonAni.timeScale = GetAnimScale(eSpineAnimation.ATTACK);
                    break;
                case "skill_ani1":
                case "skill_ani2":
                case "skill_ani3":
                case "skill_ani4":
                    trackEntry.MixDuration = 0f;
                    skeletonAni.timeScale = GetAnimScale(eSpineAnimation.SKILL);
                    break;
                case "scasting_ani1":
                case "scasting_ani2":
                case "scasting_ani3":
                case "scasting_ani4":
                    trackEntry.MixDuration = 0f;
                    skeletonAni.timeScale = GetAnimScale(eSpineAnimation.CASTING);
                    break;
                case "move_ani1":
                    if (Data == null || Data.IsEffectInfo(eSkillEffectType.STUN))
                    {
                        skeletonAni.timeScale = GetAnimScale(eSpineAnimation.WALK);
                    }
                    break;
                case "idle_ani1":
                default:
                    skeletonAni.timeScale = GetAnimScale(eSpineAnimation.IDLE);
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
        public override void ClearEffectSpine()
        {
            if (playAnim == null)
                return;

            playAnim.CallBack = null;
            Destroy(playAnim.gameObject);
            playAnim = null;
        }
        public override TrackEntry SetAnimation(eSpineAnimation anim)
        {
            if (anim == eSpineAnimation.HIT)
                return null;

            return base.SetAnimation(anim);
        }
        public override float GetAnimaionTime(eSpineAnimation anim)
        {
            if (anim == eSpineAnimation.HIT)
                return 0f;

            return base.GetAnimaionTime(anim);
        }
        public override void SetData(IBattleCharacterData data)
        {
            base.SetData(data);
            CData = data as BattleDragonData;
        }
        public override Vector3 GetAddDeathPos(Vector3 lastPos, float pos)
        {
            Vector3 ret = new Vector3(lastPos.x, lastPos.y + SBDefine.DeathY1, lastPos.z);
            ret.x -= Data.ConvertPos(pos);

            return ret;
        }
        public virtual void SetTranscendEffect(int transcendStep) // 임시 초월 캐릭터 이펙트로 쓰기 위해 준비 중인 코드
        {
            if (transcendStep <= 0)
                return;

            if (StatusEffect == null)
                return;

            var TranscendParent = StatusEffect.TranscendTr;
            TranscendParent.gameObject.SetActive(true);
            if (TranscendParent.childCount > 0)
                SBFunc.RemoveAllChildrens(TranscendParent);

            //GameObject obj = ResourceManager.GetResource<GameObject>(eResourcePath.EffectPrefabPath, SBDefine.TRANSCENDENCE_EFFECT_NAME);//HEAD
            GameObject obj = ResourceManager.GetResource<GameObject>(eResourcePath.EffectPrefabPath, SBDefine.GetTranscendEffectName(transcendStep));

            if (obj != null)
                Instantiate(obj, TranscendParent);
        }

        public override void Death()
        {
            base.Death();
            GetComponent<statusEffect>().TranscendTr.gameObject.SetActive(false);
            //SBFunc.RemoveAllChildrens(GetComponent<statusEffect>().TranscendTr);
        }
    }
}