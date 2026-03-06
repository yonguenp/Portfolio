using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SBProjectileCenterGeneric : SBProjectileCenter
    {
        [SerializeField]
        protected SpriteRenderer sprite = null;

        protected override void Awake()
        {
            base.Awake();

            if (projectileObject != null)
            {
                if (sprite == null)
                    sprite = projectileObject.GetComponent<SpriteRenderer>();
                if (sprite == null)
                    sprite = projectileObject.GetComponentInChildren<SpriteRenderer>();
            }
            else //이 아래 타면 오류 예외처리
            {
                if (sprite == null)
                {
                    var renderers = GetComponentsInChildren<SpriteRenderer>();
                    if (renderers != null)
                    {
                        for (int i = 0, count = renderers.Length; i < count; ++i)
                        {
                            if (renderers[i].name == "shadow")
                                continue;

                            sprite = renderers[i];
                            break;
                        }
                    }
                }
            }
        }
        private void Start()
        {
            SetSprite();
        }
        public override void Set(IBattleCharacterData Caster, Vector3 targetPosition, SBSkill Skill, VoidDelegate CallBack, int idx)
        {
            base.Set(Caster, targetPosition, Skill, CallBack, idx);
            SetSprite();
        }
        private void SetSprite()
        {
            if (Summon == null)
                return;

            var rData = Summon.GetArrowResource();
            if (rData == null)
                return;

            var sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ProjectileSpritePath, rData.IMAGE);
            if (this.sprite != null)
                this.sprite.sprite = sprite;
        }
    }
}