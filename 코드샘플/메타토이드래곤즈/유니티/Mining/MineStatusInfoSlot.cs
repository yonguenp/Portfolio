using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class MineStatusInfoSlot : MonoBehaviour
    {
        [SerializeField] Text titleText = null;
        [SerializeField] Text valueText = null;

        public eMineStatusTextType textType = eMineStatusTextType.NONE;

        public void SetText(string _title, string _desc)
        {
            if (titleText != null)
                titleText.text = _title;
            if (valueText != null)
                valueText.text = _desc;
        }
    }
}

