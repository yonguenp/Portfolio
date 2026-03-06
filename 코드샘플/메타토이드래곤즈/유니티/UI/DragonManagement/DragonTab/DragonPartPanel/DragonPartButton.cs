using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class DragonPartButton : MonoBehaviour
    {
        [SerializeField]
        protected PartListViewType curUIType = PartListViewType.NONE;

        public void SetVisibleButton(PartListViewType _type)
        {
            if (curUIType != PartListViewType.NONE && curUIType.HasFlag(_type))
                gameObject.SetActive(true);
            else
                gameObject.SetActive(false);
        }
    }
}
