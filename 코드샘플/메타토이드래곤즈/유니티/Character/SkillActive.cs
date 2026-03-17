using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public abstract class SkillActive : MonoBehaviour
    {
        [SerializeField]
        protected List<GameObject> activeTargets = null;

        protected IBattleCharacterData casterData = null;
        protected IBattleCharacterData targetData = null;

        public virtual void Set(IBattleCharacterData casterData, IBattleCharacterData targetData)
        {
            this.casterData = casterData;
            this.targetData = targetData;
        }
        public abstract void Active();
    }
}