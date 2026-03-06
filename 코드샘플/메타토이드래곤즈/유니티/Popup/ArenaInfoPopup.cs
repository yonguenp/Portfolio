
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ArenaInfoPopup : Popup<PopupBase>
    {
        [SerializeField]
        private ScrollRect mainScrollRect;
        [SerializeField]
        private Transform rankObjParentTr;
        [SerializeField]
        private GameObject rankResetInfoObject;

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
            rankResetInfoObject.SetActive(false);
            var rankDic= ArenaRankData.GetResetRankDic().OrderByDescending(r => ArenaRankData.Get(r.Key.ToString()).NEED_POINT);
            foreach(var obj in rankList)
            {
                obj.SetActive(false);
            }
            int i = 0;
            foreach (var rankInfo in rankDic)
            {
                if ( i >= rankList.Count)
                {
                    var obj = Instantiate(rankResetInfoObject, rankObjParentTr);
                    rankList.Add(obj);
                }
                rankList[i].SetActive(true);
                rankList[i].GetComponent<RankNextResetInfoClone>().Init(rankInfo.Key, rankInfo.Value);
                ++i;
            }
        }


    }
}

