using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {

    public class RewardListShowPopup : Popup<RewardPopupData>
    {
        [SerializeField] private Text titleText = null;
        [SerializeField] private TableView rewardSubTableView = null;
        [SerializeField] private RectTransform rewardContent = null;
        [SerializeField] private RectTransform rewardViewPort = null;

        bool isTableViewInit= false;
        public override void InitUI()
        {
            if (isTableViewInit == false)
            {
                rewardSubTableView.OnStart();
                isTableViewInit = true;
            }
            List<ITableData> tableViewItemList = new List<ITableData>();
            if (Data.Rewards != null && Data.Rewards.Count > 0)
            {
                foreach (var rewardItem in Data.Rewards)
                {
                    tableViewItemList.Add(rewardItem);
                }
            }
            rewardSubTableView.SetDelegate(
                new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
                    if (node == null || item == null)
                    {
                        return;
                    }
                    Asset reward = (Asset)item;
                    node.GetComponent<ItemFrame>().SetFrameItem(reward.ItemNo, reward.Amount, (int)reward.GoodType);
                }));

            rewardSubTableView.ReLoad();
            if (tableViewItemList.Count < 5) //특정 갯수보다 적을땐 중앙 정렬
            {
                rewardContent.sizeDelta = rewardViewPort.sizeDelta;
            }
            rewardContent.GetComponent<HorizontalLayoutGroup>().enabled = tableViewItemList.Count <= 5;
        }

        public void SetTitleText(string text)
        {
            titleText.text = text;
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
            isTableViewInit= false;
        }
    }
}