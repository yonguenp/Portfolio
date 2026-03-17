using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class PassiveMainElementBuff : PassiveBuffEffect
    {
        public override eStatusType STAT_TYPE => Target.Element.StatDMG();
    }
}