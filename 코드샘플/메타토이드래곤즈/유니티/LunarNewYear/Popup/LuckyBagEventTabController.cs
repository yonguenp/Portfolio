using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class LuckyBagEventTabController : DiceEventTabController
    {
        protected override void SetCustomReddot(DiceEventTabItem _target, int _index)
        {
            _target.SetReddotState(false);

            switch (_index)
            {
                case 0:
                    _target.SetReddotState(LuckyBagEventPopup.GetEventItemReddotCondition());
                    break;
                case 1:
                    _target.SetReddotState(LuckyBagEventPopup.GetEventQuestReddotCondition());
                    break;
            }
        }
    }
}
