using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DragonLevelupLabel : MonoBehaviour
    {
        [SerializeField]
        DragonLevelUpPopup popup = null;

        public void ChangeLevelText()//애니메이션에 의해 호출하면 증감된 숫자로 변경하기
        {
            if(popup != null)
            {
                popup.ChangeLevelText();
            }
        }
    }
}
