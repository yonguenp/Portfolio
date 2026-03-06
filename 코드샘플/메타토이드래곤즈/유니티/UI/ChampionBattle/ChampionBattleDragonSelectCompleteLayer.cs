using Google.Impl;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionBattleDragonSelectCompleteLayer : SubLayer
    {
        [SerializeField]
        private ChampionBattleDragonSelectTabLayer DragonTap = null;

        [SerializeField]
        protected TableViewGrid tableViewGrid = null;
        private List<CharBaseData> viewDragons = null;

        [SerializeField]
        Button backBtn = null;

        [SerializeField]
        private BattleDragonListView stageDragonList = null;

        ChampionBattleDragonSelectPopup ParentPopup { get { return PopupManager.GetPopup<ChampionBattleDragonSelectPopup>(); } }
        public override void Init()
        {
            if (tableViewGrid != null)
            {
                tableViewGrid.OnStart();
            }
            ForceUpdate();
        }
        public override void ForceUpdate()
        {
            DrawScrollView();
        }
        public void DrawScrollView(bool _initPos = true)
        {
            if (tableViewGrid == null)
                return;

            var selectedList = ParentPopup.SelectedDragonList;

            List<ITableData> tableViewItemList = new List<ITableData>();
            tableViewItemList.Clear();
            if (selectedList != null && selectedList.Count > 0)
            {
                for (var i = 0; i < selectedList.Count; i++)
                {
                    var data = CharBaseData.Get(selectedList[i]);
                    if (data == null)
                        continue;

                    tableViewItemList.Add(data);
                }
            }

            tableViewGrid.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) =>
            {
                if (node == null)
                    return;

                var frame = node.GetComponent<DragonPortraitFrame>();
                if (frame == null)
                    return;

                var dragonData = (CharBaseData)item;

                frame.SetCustomPotraitFrame(dragonData.KEY, 0);
            }));

            tableViewGrid.ReLoad(_initPos);
        }

        public override bool backBtnCall()
        {
            PopupManager.ClosePopup<ChampionBattleDragonSelectPopup>();
            return true;
        }

        public void OnClickOK()
        {
            ChampionManager.Instance.CurChampionInfo.ReqSelectDragons(ParentPopup.SelectedDragonList.ToArray(), (jsonData) => {
                if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                    return;

                switch ((eApiResCode)jsonData["rs"].Value<int>())
                {
                    case eApiResCode.OK:
                    {
                        LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleLobby", eSceneEffectType.CloudAnimation);
                        return;
                    }
                    break;
                    default:
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언드래곤등록실패"));
                        return;
                    }
                    break;
                }                
            });
        }

    }
}
