using System;
using UnityEngine;

namespace SandboxNetwork
{
    [Serializable]
    public class SkillDimmed
    {
        [SerializeField]
        private eSkillDimmedType dimmedType = eSkillDimmedType.None;
        public eSkillDimmedType DimmedType
        {
            get { return dimmedType; }
        }
        [SerializeField]
        private GameObject dimmed = null;
        public GameObject DimmedObject
        {
            get { return dimmed; }
        }
    }
}