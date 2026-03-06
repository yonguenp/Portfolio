using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct EffectCustomData
    {
        public EffectCustomData(ICharacterBaseData Data, IBattleCharacterData cData)
        {
            Key = Data.KEY;
            SCRIPT_OBJECT_KEY = Data.SCRIPT_OBJECT_KEY;
            if (cData != null)
                SPEED = cData.Stat.GetAttackSpeed();
            else
                SPEED = 1 + Data.ADD_ATKSPEED;
        }

        public int Key;
        public string SCRIPT_OBJECT_KEY;
        public float SPEED;
    }
    public class EffectCustomSender : MonoBehaviour
    {
        [SerializeField]
        private bool isMonster = false;

        bool isArena = false;

        eSpineAnimation currentSpineAnimType = eSpineAnimation.NONE;
        EffectCustomData customData = default;

        // 이펙트 커스텀 
        public EffectCustomSet EffectCustomSet { get; private set; } = null;
        public IBattleCharacterData BattleCharcterData { get; private set; } = null;
        public SBSpine<eSpineAnimation> Spine { get; private set; } = null;

        public void SetArena(bool value)
        {
            isArena = value;
        }
        public void SetCaster(SBSpine<eSpineAnimation> obj, IBattleCharacterData data)
        {
            Spine = obj;
            BattleCharcterData = data;
            customData = new EffectCustomData(data.BaseData, data);

            EffectCustomSet = ResourceManager.GetResource<EffectCustomSet>(eResourcePath.EffectCustomPath, customData.SCRIPT_OBJECT_KEY);
        }

        public void Send(eSpineAnimation anim)
        {
            if (currentSpineAnimType == anim)
                return;

            currentSpineAnimType = anim;
            if (EffectCustomSet != null)
                PlayCustomEvent(EffectCustomSet.GetEffectCustomByAnimType(anim));
        }

        public void PlayCustomEvent(List<EffectCustom> list)
        {
            if (list == null || list.Count < 1)
                return;

            for(int i = 0, count = list.Count; i < count; ++i)
            {
                if (list[i] == null)
                    continue;

                EffectReceiverEvent.Send(customData, Spine, list[i]);
            }
        }
    }
}