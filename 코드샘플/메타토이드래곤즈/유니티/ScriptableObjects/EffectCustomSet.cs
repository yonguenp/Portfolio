using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    [CreateAssetMenu(fileName = "EffectCustomSet", menuName = "Scriptable Object Asset/EffectCustom/EffectCustomSet", order = 0)]
    public class EffectCustomSet : ScriptableObject
    {
        [SerializeField]
        List<EffectCustom> effectCustomList = new();

        public List<EffectCustom> GetEffectCustomByAnimType(eSpineAnimation animType)
        {
            return effectCustomList.FindAll(element => element.animState == animType);
        }

        public List<EffectCustom> GetAllEffectCustomList()
        {
            return effectCustomList;
        } 
    }
}