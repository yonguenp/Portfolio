using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork { 


    public class GuildWithdrawSlot : MonoBehaviour
    {
        [SerializeField]
        Image userRankImg;
        [SerializeField]
        Text rankText;
        [SerializeField]
        UserPortraitFrame userFrame;
        [SerializeField]
        Text nickText;
        [SerializeField]
        Text positionText;

        GuildUserData data = null;
        public void Init(GuildUserData userData)
        {
            data = userData;
            userFrame.SetUserPortraitFrame(data);           
            nickText.text = data.Nick;
            positionText.text = data.GuildPosition switch
            {
                eGuildPosition.Leader => StringData.GetStringByStrKey("guild_desc:23"),
                eGuildPosition.Operator => StringData.GetStringByStrKey("guild_desc:113"),
                eGuildPosition.Normal => StringData.GetStringByStrKey("guild_desc:28"),
                _ => ""
            };

            rankText.text = SBFunc.GetRankText(data.Rank);

            int pickSpriteIdx = 0;
            if (data.Rank != 0)
                pickSpriteIdx = GuildRankRewardData.GetByRankGroup(data.Rank, eGuildRankRewardGroup.UserRank).ACCUMULATE_REWARD;
            var resourceData = GuildResourceData.Get(pickSpriteIdx);
            if (resourceData != null)
            {
                userRankImg.color = new Color(1f, 1f, 1f, 1f);
                userRankImg.sprite = resourceData.RESOURCE;
                userRankImg.gameObject.SetActive(true);
            }
            else
            {
                userRankImg.gameObject.SetActive(false);
            }
        }

        public void OnMagnetWithdraw()
        {
            GuildWidthdrawPopup.Open(GuildAssetType.MAGNET_WITHDRAW, data);
        }

        public void OnMagniteWithdraw()
        {
            GuildWidthdrawPopup.Open(GuildAssetType.MAGNITE_WITHDRAW, data);
        }
    }
}