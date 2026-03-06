using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DragonPartButtonController : MonoBehaviour
    {
        [SerializeField]
        List<DragonPartButton> buttonList = new List<DragonPartButton>();

        public void SetVisibleButtonVisibleByType(PartListViewType _currentUIType)
        {
            if(buttonList != null  && buttonList.Count >= 0)
            {
                foreach (var button in buttonList)
                    button.SetVisibleButton(_currentUIType);
            }
        }
    }
}
