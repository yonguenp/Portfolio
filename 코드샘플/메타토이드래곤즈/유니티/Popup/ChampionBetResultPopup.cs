using Coffee.UIEffects;
using Google.Impl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionBetResultPopup : Popup<PopupBase>
    {
        [SerializeField]
        private ChampionBetResultLayer BetLayer = null;

        public static void OpenPopup()
        {
            ChampionManager.Instance.CurChampionInfo.ReqBetLog(() => {
                if (ChampionManager.Instance.CurChampionInfo.MatchData == null || ChampionManager.Instance.CurChampionInfo.MatchData.Count < 1)
                    return;
                PopupManager.OpenPopup<ChampionBetResultPopup>();
            });
        }

        public override void InitUI()
        {
            BetLayer.Init();
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
        }
    }
}

