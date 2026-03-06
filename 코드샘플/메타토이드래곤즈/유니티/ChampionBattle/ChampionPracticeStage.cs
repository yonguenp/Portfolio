using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChampionPracticeStage : ChampionBattleStage
    {
        protected override void InitStateMachine()
        {
            if (StateMachine == null)
            {
                StateMachine = new ChampionParacticeColosseumMachine(this, ChampionManager.Instance.PracticeBattleData);
                StateMachine.SetState();
            }

            StateMachine.ChangeState<ChampionPracticeColosseumStart>();
        }

        protected override void UpdateState()
        {
            if (!StateMachine.Update(SBGameManager.Instance.DTime))
            {
                if (StateMachine.IsState<ChampionPracticeColosseumStart>() && StateMachine.ChangeState<ChampionPracticeColosseumBattle>())
                {
                    return;
                }
                else if (StateMachine.IsState<ChampionPracticeColosseumBattle>() && StateMachine.ChangeState<ChampionPracticeColosseumEnd>())
                {
                    RoundWinnerEffect();
                    return;
                }
                else if (StateMachine.IsState<ChampionPracticeColosseumEnd>())
                {
                    BattleEnd();
                    StateMachine = null;
                }
            }
        }
        protected override void BattleEnd()
        {
            PopupManager.OpenPopup<ChampionBattleStatisticPopup>(new ChampionBattleStatisticPopupData(ChampionManager.Instance.ChampionData, () => {
                LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleSetting", eSceneEffectType.CloudAnimation);
            }));        
        }
    }
}