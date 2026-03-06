using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GuildLvRewardClone : MonoBehaviour
    {
        [SerializeField]
        Text lvText;
        [SerializeField]
        Text rewardText;
        [SerializeField]
        Image panel;

        public void Init(int requireLv, string rewardStr, int myGuildLv)
        {
            lvText.text = StringData.GetStringFormatByStrKey("user_info_lv_02", requireLv);
            rewardText.text = rewardStr;
            panel. color = requireLv > myGuildLv ? Color.gray : Color.white;
        }
    }

}
