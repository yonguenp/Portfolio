using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class BattleDimmed
    {
        public static List<SkillDimmed> DimmedList { get; private set; } = null;
        public static void Initiailze() { }
        public static void SetDimmed(List<SkillDimmed> dimmeds)
        {
            DimmedList = dimmeds;
        }
        public static SkillDimmed GetSkillDimmed(eSkillDimmedType type)
        {
            if (DimmedList == null)
                return null;
            return DimmedList.Find((element) => element.DimmedType == type);
        }
    }
}