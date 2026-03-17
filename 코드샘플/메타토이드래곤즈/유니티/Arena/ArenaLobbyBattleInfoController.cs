using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SandboxNetwork
{
    enum eArenaTabType
    {
        BATTLE = 1,
        RECORD,
        RANKING,
        SEASON_REWARD
    }
    public class ArenaLobbyBattleInfoController : MonoBehaviour
    {
        [Header("Tab Button")]
        [SerializeField]
        private Button tabButtonBattle;
        [SerializeField]
        private Button tabButtonRecord;
        [SerializeField]
        private Button tabButtonRanking;
        [SerializeField]
        private Button tabButtonSeasonReward;

        [Header("Tab Layer")]
        [SerializeField]
        private GameObject tabLayerBattle;
        [SerializeField]
        private GameObject tabLayerRecord;
        [SerializeField]
        private GameObject tabLayerRanking;
        [SerializeField]
        private GameObject tabLayerSeasonReward;


        [Header("image")]
        [SerializeField]
        private Image backImg = null;
        [SerializeField]
        private Sprite backImgDefault = null;
        [SerializeField]
        private Sprite backImgRanking = null;




        private eArenaTabType curTabType = eArenaTabType.BATTLE;

        private int recordForceIndex = -1;

        public void Init()
        {
            int tempIndex = ArenaManager.Instance.battleInfoTabIdx;
            if (tempIndex == 2)
            {
                recordForceIndex = ArenaManager.Instance.CurrentDefencePage;
            }
            OnClickTabButton(tempIndex);
        }
        public void OnClickTabButton(int customEventData)
        {
            if (tabButtonBattle == null || tabButtonRecord == null || tabButtonRanking == null || tabButtonSeasonReward == null) return;
            if (tabLayerBattle == null || tabLayerRecord == null || tabLayerRanking == null || tabLayerSeasonReward == null) return;

            tabButtonBattle.interactable = true;
            tabButtonRecord.interactable = true;
            tabButtonRanking.interactable = true;
            tabButtonSeasonReward.interactable = true;  

            tabLayerBattle.SetActive(false);
            tabLayerRecord.SetActive(false);
            tabLayerRanking.SetActive(false);
            tabLayerSeasonReward.SetActive(false);
            backImg.sprite = backImgDefault;
            switch (customEventData)
            {
                //battle
                case (int)eArenaTabType.BATTLE:
                    curTabType = eArenaTabType.BATTLE;
                    tabButtonBattle.interactable = false;
                    tabLayerBattle.SetActive(true);
                    tabLayerBattle.GetComponent<ArenaBattleTabController>().Init();
                    break;
                //record
                case (int)eArenaTabType.RECORD:
                    curTabType = eArenaTabType.RECORD;
                    tabButtonRecord.interactable = false;
                    tabLayerRecord.SetActive(true);
                    var recordComponent = tabLayerRecord.GetComponent<ArenaRecordTabController>();
                    if (recordComponent != null)
                    {
                        bool isPageChange = recordComponent.ChangeIndexFlag;
                        int requestPage = recordComponent.tempPageIndex;

                        if (!isPageChange)
                        {
                            requestPage = 0;
                        }

                        if (recordForceIndex >= 0)
                        {
                            requestPage = recordForceIndex;
                        }
                        ArenaManager.Instance.SetArenaDefenceData(() =>
                        {
                            recordComponent.Init();
                            recordForceIndex = -1;
                        }, requestPage);

                    }
                    break;
                //ranking
                case (int)eArenaTabType.RANKING:
                    curTabType = eArenaTabType.RANKING;
                    tabButtonRanking.interactable = false;
                    tabLayerRanking.SetActive(true);
                    backImg.sprite = backImgRanking;
                    ArenaManager.Instance.RefreshRankList(() =>
                    {
                        if (tabLayerRanking != null)
                        {
                            var rangkingTabComp = tabLayerRanking.GetComponent<ArenaRankingTabController>();
                            if (rangkingTabComp != null)
                            {
                                rangkingTabComp.Init();
                            }
                        }
                    });
                    break;
                // season reward
                case (int)eArenaTabType.SEASON_REWARD:
                    //ToastManager.On(100000326);
                    curTabType = eArenaTabType.SEASON_REWARD;
                    tabButtonSeasonReward.interactable = false;
                    tabLayerSeasonReward.SetActive(true);
                    tabLayerSeasonReward.GetComponent<ArenaSeasonRewardController>().Init();
                    break;
            }
        }


        public void Refresh()
        {
            OnClickTabButton((int)curTabType);
        }
    }
}