using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork 
{ 
    public class GuildMissionObject : MissionUIObject
    {
        public override void OnClickGetReward()
        {
            if (currentQuest.IsAlreadyGetRewards())
            {
                ToastManager.On(StringData.GetStringByStrKey("일일보상오류"));
                return;
            }

            QuestManager.Instance.RequestAcceptableRewardQuest(currentQuest, () =>
            {
                QuestManager.Instance.RequestQuestComplete(currentQuest.ID, () => {
                    if (getRewardDelegate != null)
                        getRewardDelegate();
                });
            }
            , () =>
            {
                if (getRewardDelegate != null)//UI 갱신
                    getRewardDelegate();
            });
        }
    }
}