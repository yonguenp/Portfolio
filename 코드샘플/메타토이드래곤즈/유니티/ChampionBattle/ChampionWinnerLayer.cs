using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;
using Google.Impl;

namespace SandboxNetwork
{    
    public class ChampionWinnerLayer : MonoBehaviour
    {
        [SerializeField]
        private GameObject winnerSlotPrefab = null;
        [SerializeField]
        private Transform winnerParent = null;

        [SerializeField]
        private Text noWinnerText = null; // 명예의 전달에 등록된 유저가 없습니다 텍스트

        const int MaxDragonDeckSeatCount = 6;//드래곤 자리 카운트
        private bool isFirst = false;

        [SerializeField]
        private GameObject[] noDataUI;
        [SerializeField]
        private GameObject[] hasDataUI;

        [SerializeField]
        protected TableView tableView = null;

        private List<ITableData> tableViewItemList = new List<ITableData>();

        void Clear()
        {
            tableViewItemList.Clear();
        }
        public void Init(List<ChampionWinUserInfo> winners)
        {
            Clear();

            if (winners == null || winners.Count <= 0)
            {
                foreach (var ui in noDataUI)
                {
                    ui.SetActive(true);
                }
                foreach (var ui in hasDataUI)
                {
                    ui.SetActive(false);
                }
                return;
            }

            if (tableView != null)
            {
                tableView.OnStart();
            }

            foreach (var ui in noDataUI)
            {
                ui.SetActive(false);
            }
            foreach (var ui in hasDataUI)
            {
                ui.SetActive(true);
            }

            foreach (var winner in winners)
            {
                tableViewItemList.Add(winner);
            }

            DrawScrollView();
        }

        public void DrawScrollView()
        {
            if (tableView == null || winnerSlotPrefab == null) return;
            tableView.SetDelegate(
                new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
                    if (node == null) return;
                    var slotInfo = node.GetComponent<ChampionWinnerInfoSlot>();
                    if (slotInfo == null) return;
                    ChampionWinUserInfo data = (ChampionWinUserInfo)item;
                    slotInfo.Init(data);
                }));
            tableView.ReLoad();
        }
    }
}