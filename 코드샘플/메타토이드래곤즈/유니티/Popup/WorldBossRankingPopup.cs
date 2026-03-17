using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class WorldBossRankingPopup : Popup<PopupData>
    {
        [SerializeField] WorldBossRankingTabController controller = null;

        public override void InitUI()
        {
            if (controller != null)
                controller.Init();
        }
    }
}
