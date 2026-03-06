using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public enum GuildAssetType
    {
        MAGNET_DEPOSIT,
        MAGNET_WITHDRAW,
        MAGNITE_DEPOSIT,
        MAGNITE_WITHDRAW,
    }
    public struct GuildAssetEvent
    {
        private static GuildAssetEvent e = default;
        
        public static void Send()
        {
            EventManager.TriggerEvent(e);
        }
    }
    public class GuildWalletLayer : TabLayer, EventListener<GuildAssetEvent>
    {
        [SerializeField]
        Button DepositButton;
        [SerializeField]
        Button WithdrawButton;
        [SerializeField]
        Button LogButton;

        [SerializeField]
        Text Magnet;
        [SerializeField]
        Text Magnite;

        [SerializeField]
        GameObject logPanel;
        [SerializeField]
        GameObject withdrawPanel;

        [SerializeField]
        TableView logTableView;
        [SerializeField]
        TableView withdrawTableView;

        [SerializeField]
        GameObject logEmpty;
        [SerializeField]
        GameObject withdrawEmpty;

        bool withdrawOn = false;
        bool isInit = false;

        void OnEnable()
        {
            EventManager.AddListener(this);
        }

        void OnDisable()
        {
            EventManager.RemoveListener(this);
        }
        public override void InitUI(TabTypePopupData datas = null)
        {
            base.InitUI(datas);

            if (isInit == false)
            {
                logTableView.OnStart();
                withdrawTableView.OnStart();
                isInit = true;
            }

            withdrawOn = false;
            RefreshUI();
        }

        public void OnDepositMagnite()
        {
            GuildWidthdrawPopup.Open(GuildAssetType.MAGNITE_DEPOSIT, null);
        }
        public void OnDepositMagnet()
        {
            GuildWidthdrawPopup.Open(GuildAssetType.MAGNET_DEPOSIT, null);
        }

        public void OnWithdraw()
        {
            withdrawOn = true;
            RefreshUI();
        }

        public void OnAssetLog()
        {
            withdrawOn = false;
            RefreshUI();
        }

        public override void RefreshUI()
        {
            base.RefreshUI();

            WithdrawButton.interactable = GuildManager.Instance.IsWalletWithdraw;
            Magnet.text = SBFunc.CommaFromNumber(GuildManager.Instance.MyGuildInfo.GuildMagnet);
            Magnite.text = SBFunc.CommaFromNumber(GuildManager.Instance.MyGuildInfo.GuildMagnite);

            if (withdrawOn)
            {
                logPanel.SetActive(false);
                withdrawPanel.SetActive(true);
                LogButton.interactable = true;

                DrawWithdrawScrollView();
            }
            else
            {
                logPanel.SetActive(true);
                withdrawPanel.SetActive(false);
                LogButton.interactable = false;

                GuildManager.Instance.NetworkSend("guild/accounttxhistory", new WWWForm(), (jObject) => {
                    DrawLogScrollView(jObject);
                });
            }
        }

        void DrawWithdrawScrollView()
        {
            var userList = GuildManager.Instance.MyGuildInfo.GuildUserList;            
            userList = userList.OrderByDescending(user => {
                if (user.Rank > 0)
                    return -user.Rank;
                return int.MinValue;
            }).ThenBy(user => user.SumContribution).ToList();

            if (withdrawTableView == null || userList.Count <= 0) return;

            List<ITableData> list = new List<ITableData>();
            for (var i = 0; i < userList.Count; i++)
            {
                if (userList[i] == null)
                {
                    continue;
                }
                list.Add(userList[i]);
            }

            withdrawEmpty.SetActive(userList.Count == 0);
            withdrawTableView.SetDelegate(
                new TableViewDelegate(list, (GameObject node, ITableData item) => {
                    if (node == null) return;
                    var manageObj = node.GetComponentInChildren<GuildWithdrawSlot>();
                    if (manageObj == null) return;
                    manageObj.Init((GuildUserData)item);
                    node.SetActive(true);
                })); 
            withdrawTableView.ReLoad();
        }


        void DrawLogScrollView(JObject jObject)
        {
            List<ITableData> list = new List<ITableData>();
            if(jObject.ContainsKey("history") && jObject["history"].Type == JTokenType.Array)
            {
                int index = 1;
                foreach(var log in jObject["history"])
                {
                    list.Add(new GuildAssetLogSlot.GuildAssetLog(index++, (JObject)log));
                }
            }
            logEmpty.SetActive(list.Count == 0);
            logTableView.SetDelegate(
                new TableViewDelegate(list, (GameObject node, ITableData item) => {
                    if (node == null) return;
                    var manageObj = node.GetComponent<GuildAssetLogSlot>();
                    if (manageObj == null) return;
                    manageObj.Init((GuildAssetLogSlot.GuildAssetLog)item);
                    node.SetActive(true);
                }));
            logTableView.ReLoad();
        }

        public void OnEvent(GuildAssetEvent eventType)
        {
            RefreshUI();
        }
    }

}

