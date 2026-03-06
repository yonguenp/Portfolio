using UnityEngine;

namespace SandboxNetwork
{
    public enum eEffectPosType
    {
        Collider,
        Top,
        Bottom
    }
    public enum eAttachType
    {
        Map,
        Target,
    }

    public enum eSiblingType
    {
        None,
        First,
        Last,
    }
    public class SBEffectPosType : MonoBehaviour
    {
        [SerializeField]
        protected eEffectPosType effectPosType = eEffectPosType.Collider;
        public eEffectPosType EffectPosType { get { return effectPosType; } }
        [SerializeField]
        protected eAttachType attachType = eAttachType.Map;
        public eAttachType AttachType { get { return attachType; } }

        [SerializeField]
        protected eSiblingType siblingType = eSiblingType.None;
        public eSiblingType SiblingType { get { return siblingType; } }
    }
}