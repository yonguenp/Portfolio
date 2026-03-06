using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class ChampionBattlePartButton : MonoBehaviour
    {
        [SerializeField]
        protected ChampionBattlePartListViewType curUIType = ChampionBattlePartListViewType.NONE;

        public void SetVisibleButton(ChampionBattlePartListViewType _type)
        {
            if (curUIType != ChampionBattlePartListViewType.NONE && curUIType.HasFlag(_type))
                gameObject.SetActive(true);
            else
                gameObject.SetActive(false);
        }
    }
}
