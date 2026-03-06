using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GuildJoinLayer : TabLayer
    {
        [SerializeField]
        Button[] tabs = null;
        [SerializeField]
        SubLayer[] subLayers = null;
        [SerializeField]
        GameObject rankingInfoBtnObj = null;

        int curTabNo = 0;

        public override void InitUI(TabTypePopupData datas = null)
        {
            base.InitUI(datas);
            SetTabLayer();
        }

        public override void RefreshUI()
        {
            base.RefreshUI();
            SetTabLayer();
        }

        public void OnClickInfoBtn()
        {
            PopupManager.OpenPopup<GuildRankingRewardPopup>(new GuildRankRewardPopupData(eGuildRankRewardGroup.GuildRank));
        }

        void SetTabLayer()
        {
            if(curTabNo >= tabs.Length || curTabNo >= subLayers.Length)
            {
                Debug.Log("index out of Range!!!");
                return;
            }

            foreach(var tab in tabs)
            {
                tab.interactable = true;
            }
            tabs[curTabNo].interactable = false;

            foreach(var layer in subLayers)
            {
                layer.gameObject.SetActive(false);
            }
            rankingInfoBtnObj.SetActive(curTabNo == (int)eGuildJoinLayerType.RankingLayer);
            subLayers[curTabNo].gameObject.SetActive(true);
            subLayers[curTabNo].Init();
        }

        public void OnClickTab(int index)
        {
            curTabNo = index;
            SetTabLayer();
        }

    }
}

