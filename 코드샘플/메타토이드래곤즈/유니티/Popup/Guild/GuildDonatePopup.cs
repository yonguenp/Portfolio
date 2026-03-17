using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GuildDonatePopup : Popup<PopupData>
    {
        [SerializeField]
        Text remainDonateCntText = null;
        [SerializeField]
        GuildDonationClone[] guildDonationClones = null;

        List<GuildDonationData> data = new List<GuildDonationData>();
        public override void InitUI()
        {
            remainDonateCntText.text = StringData.GetStringFormatByStrKey("guild_desc:43", GuildManager.Instance.GuildRemainDonationCount);
            
            data = GuildDonationData.GetAll();
            if (data == null || data.Count == 0)
                return;

            for(int i = 0; i < guildDonationClones.Length; ++i)
            {
                if(data.Count > i)
                {
                    int index = i;

                    guildDonationClones[index].Init(data[index]);
                    guildDonationClones[index].SetButtonListener(()=> {
                        RefreshUI();
                    });
                }
            }
        }

        public void RefreshUI()
        {
            remainDonateCntText.text = StringData.GetStringFormatByStrKey("guild_desc:43", GuildManager.Instance.GuildRemainDonationCount);
           
            for (int i = 0; i < guildDonationClones.Length; ++i)
            {
                if (data.Count > i)
                    guildDonationClones[i].RefreshButtonState();
            }
        }
    }
}
