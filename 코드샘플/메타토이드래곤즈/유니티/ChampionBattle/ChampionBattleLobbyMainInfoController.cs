using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
namespace SandboxNetwork
{
    public class ChampionBattleLobbyMainInfoController : MonoBehaviour
    {
        [SerializeField]
        ChampionBattlePlayerInfo ChampionBattleInfo = null;

        public void Init()
        {
            SetTeamInfoPopup();
            RefreshMainInfoLayer();
        }
        public void RefreshMainInfoLayer()
        {
            RefreshTeamInfoPopup();
        }

        void SetTeamInfoPopup()
        {
            if (ChampionBattleInfo != null)
            {
                ChampionBattleInfo.init();
            }
        }

        public void RefreshTeamInfoPopup()
        {
            if (ChampionBattleInfo != null)
            {
                ChampionBattleInfo.RefreshData();
            }
        }
    }
}