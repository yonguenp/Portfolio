using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChampionBattlePartButtonController : MonoBehaviour
    {
        [SerializeField]
        List<ChampionBattlePartButton> buttonList = new List<ChampionBattlePartButton>();

        public void SetVisibleButtonVisibleByType(ChampionBattlePartListViewType _currentUIType)
        {
            if(buttonList != null  && buttonList.Count >= 0)
            {
                foreach (var button in buttonList)
                    button.SetVisibleButton(_currentUIType);
            }
        }
    }
}
