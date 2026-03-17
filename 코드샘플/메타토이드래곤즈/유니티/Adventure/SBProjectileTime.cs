using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public delegate void SBProjectileTimeEvent(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, SBSkill skill);
    public delegate eDirectionBit SBProjectileDirectionEvent(IBattleCharacterData caster);
    public class SBProjectileTime : SBProjectileTarget
    {
        [SerializeField]
        float duration = 3f;
        float time = 3f;
        eDirectionBit dir = eDirectionBit.None;
        List<IBattleCharacterData> isExist = new List<IBattleCharacterData>();
        SBProjectileTimeEvent triggerEvent = null;
        IBattleData battleData = null;
        SBRect rect = null;
        BoxCollider2D boxCollider2D = null;
        public override void Set(IBattleCharacterData Caster, IBattleCharacterData Target, SBSkill Skill, VoidDelegate CallBack, int idx)
        {
            if(IsMove())
            {
                base.Set(Caster, Target, Skill, CallBack, idx);
                var worldTargetPos = Target.Transform.position;
                var diff = transform.position - worldTargetPos;
                if (diff.x > 0)
                    dir = eDirectionBit.Left;
                else
                    dir = eDirectionBit.Right;
            }
            else
            {
                base.Set(Caster, Caster, Skill, CallBack, idx);
                if (Caster.IsEnemy)
                    dir = eDirectionBit.Left;
                else
                    dir = eDirectionBit.Right;
            }

            var scale = transform.localScale;
            scale.x = dir == eDirectionBit.Left ? 1 : -1;
            transform.localScale = scale;
            time = duration;

            boxCollider2D = GetComponent<BoxCollider2D>();
            if (boxCollider2D == null)
                boxCollider2D = gameObject.AddComponent<BoxCollider2D>();

            if (Summon != null)
            {
                if(Summon.VALUE1 > 0)
                    time = Summon.VALUE1;

                if(Summon.RANGE_X > 0 && Summon.RANGE_Y > 0)
                {
                    boxCollider2D.size = new(Summon.RANGE_X, Summon.RANGE_Y);
                }
            }
        }
        public void SetDicrection(SBProjectileDirectionEvent direction)
        {
            if (direction == null)
                return;

            dir = direction.Invoke(Caster);
            var scale = transform.localScale;
            scale.x = dir == eDirectionBit.Left ? 1 : -1;
            transform.localScale = scale;
        }
        public void SetTriggerData(IBattleData battleData, SBProjectileTimeEvent triggerEvent)
        {
            this.battleData = battleData;
            this.triggerEvent = triggerEvent;
        }
        protected override bool UpdateTile(float dt)
        {
            time -= dt;
            if (time > 0f)
            {
                transform.position = Vector3.MoveTowards(transform.position, 
                    transform.position + (dir == eDirectionBit.Left ? Vector3.left : Vector3.right), dt * Speed);

                TargetCheckEvent();
                return false;
            }
            return true;
        }

        protected void TargetCheckEvent()
        {
            var rect = GetRect();
            if (rect == null || battleData == null)
                return;

            foreach(var info in battleData.OffenseDic)
            {
                var characterData = info.Value;
                if (characterData == null || characterData.Death || characterData.Transform == null)
                    continue;

                if (Summon.TARGET_TYPE.IsTarget(Caster, characterData) && rect.IsContain(characterData.Transform.position))
                    TargetEvent(characterData);
            }

            foreach (var info in battleData.DefenseDic)
            {
                var characterData = info.Value;
                if (characterData == null || characterData.Death || characterData.Transform == null)
                    continue;

                if (Summon.TARGET_TYPE.IsTarget(Caster, characterData) && rect.IsContain(characterData.Transform.position))
                    TargetEvent(characterData);
            }
        }

        protected void TargetEvent(IBattleCharacterData target)
        {
            if (target != null)
            {
                if (isExist.Contains(target))
                    return;
                isExist.Add(target);

                if (Effects != null)
                {
                    for (int i = 0, count = Effects.Count; i < count; ++i)
                    {
                        if (Effects[i] == null)
                            continue;

                        triggerEvent?.Invoke(Caster, target, Effects[i], Skill);
                    }
                }
            }
        }
        protected SBRect GetRect()
        {
            var box = GetCollider();
            if (box == null)
                return null;

            if (rect == null)
                rect = new SBRect(transform.position + new Vector3(box.offset.x, box.offset.y), box.bounds.size.x, box.bounds.size.y);
            else
                rect.SetPosition(transform.position + new Vector3(box.offset.x, box.offset.y));

            return rect;
        }

        protected BoxCollider2D GetCollider()
        {
            return boxCollider2D;
        }
    }
}