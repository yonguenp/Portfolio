using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SandboxNetwork
{
    public enum eChampionLayerType
    {
        NORMAL = 1,
        PLAYER,
    }
    public class ChampionLobbyLayerController : MonoBehaviour
    {
        [Header("Tab Button")]
        [SerializeField]
        private Button tabButtonNORMAL;
        [SerializeField]
        private Button tabButtonPLAYER;

        [Header("Tab Layer")]
        [SerializeField]
        private GameObject normalLayer;
        [SerializeField]
        private GameObject playerLayer;

        public eChampionLayerType curTabType = eChampionLayerType.NORMAL;


        private void Start()
        {
            Init();    
        }

        void Init()
        {
            //OnClickTabButton(ChampionManager.Instance.CurChampionInfo.AmIParticipant ? eChampionLayerType.PLAYER : eChampionLayerType.NORMAL);
        }
        public void OnClickTabButton(eChampionLayerType tempIndex)
        {
            if (tabButtonNORMAL == null || tabButtonPLAYER == null || normalLayer == null || playerLayer == null) return;

            tabButtonNORMAL.interactable = true;
            tabButtonPLAYER.interactable = true;

            normalLayer.SetActive(false);
            playerLayer.SetActive(false);
            switch (tempIndex)
            {
                case eChampionLayerType.NORMAL:
                    curTabType = eChampionLayerType.NORMAL;
                    tabButtonNORMAL.interactable = false;
                    normalLayer.SetActive(true);
                    break;
                case eChampionLayerType.PLAYER:
                    curTabType = eChampionLayerType.PLAYER;
                    tabButtonPLAYER.interactable = false;
                    playerLayer.SetActive(true);
                    break;
            }
        }


        public void Refresh()
        {
            OnClickTabButton(curTabType);
        }
    }
}