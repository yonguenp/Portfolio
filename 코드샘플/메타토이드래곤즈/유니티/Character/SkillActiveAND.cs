using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SkillActiveAND : SkillActive
    {
        public override void Active()
        {
            if (activeTargets == null || casterData == null || targetData == null)
                return;

            for (int i = 0, count = activeTargets.Count; i < count; ++i)
            {
                if (activeTargets[i] == null)
                    continue;

                activeTargets[i].SetActive(casterData == targetData);
            }
        }
    }
}