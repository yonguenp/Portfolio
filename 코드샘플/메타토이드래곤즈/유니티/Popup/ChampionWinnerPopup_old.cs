using Coffee.UIEffects;
using Google.Impl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionWinnerPopup_old : Popup<PopupBase>
    {
        [SerializeField]
        private ChampionWinnerInfo winnerInfo = null;

        [SerializeField]
        private ChampionWinnerLayer winnerLayer = null;

        public override void InitUI()
        {
            //winnerInfo.Init(ChampionManager.Instance.UserA);
            //winnerLayer.Init(ChampionManager.Instance.UserB);
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
        }
    }
}

