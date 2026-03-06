
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class WorldBossInfoPopup : Popup<PopupBase>
    {
        [SerializeField]
        private ScrollRect mainScrollRect;
        [SerializeField]
        private Transform rankObjParentTr;

        [SerializeField]
        private GameObject textLayerObj;
        [SerializeField]
        private GameObject rankLayerObj;


        private List<GameObject> rankList = new List<GameObject>();
        public override void InitUI()
        {
            textLayerObj.SetActive(true);
            rankLayerObj.SetActive(true);
            mainScrollRect.verticalNormalizedPosition = 1;
        }
    }
}

