using Google.Impl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionBattleDragonSelectListLayer : SubLayer
    {
        [SerializeField]
        private ChampionBattleDragonSelectTabLayer DragonTap = null;
        [SerializeField]
        private Text titleLabel = null;
        [SerializeField]
        public Text countLabel = null;

        [Header("Scroll")]
        [SerializeField]
        protected TableViewGrid tableViewGrid = null;
        [SerializeField]
        protected TableViewGrid tableViewGridBottom = null;

        [Space(10)]
        private List<CharBaseData> viewDragons = null;

        private bool viewDirty = true;

        ChampionBattleDragonSelectPopup ParentPopup { get { return PopupManager.GetPopup<ChampionBattleDragonSelectPopup>(); } }
        
        public override void Init()
        {
            if (tableViewGrid != null && tableViewGridBottom != null)
            {
                tableViewGrid.OnStart();
                tableViewGridBottom.OnStart();
            }

            viewDragons = ChampionManager.GetSelectableDragons();
            if (viewDragons != null && viewDragons.Count > 0)
            {
                var length = viewDragons.Count;
                for (var i = 0; i < length; i++)
                {
                    var dragonData = viewDragons[i];
                    if (dragonData == null)
                        continue;

                    if (dragonData.KEY <= 0)
                        continue;


                    if (ParentPopup != null && ParentPopup.UserDragonData != null)
                    {
                        ParentPopup.UserDragonData.AddUserDragon(dragonData.KEY, new ChampionDragon(dragonData.KEY, null));
                    }

                }
            }
            viewDirty = true;
            SetTitleLabel(ParentPopup.SelectedDragonList.Count);
            DrawScrollView();
            DrawBottomView();
            //ForceUpdate();
        }


        public override void ForceUpdate()
        {
            SetTitleLabel(ParentPopup.SelectedDragonList.Count);
            DrawScrollView();
            DrawBottomView();
        }

        public void DrawScrollView(bool _initPos = true)
        {
            if (!viewDirty || tableViewGrid == null || viewDragons == null)
                return;

            List<ITableData> tableViewItemList = new List<ITableData>();
            tableViewItemList.Clear();
            if (viewDragons != null && viewDragons.Count > 0)
            {
                for (var i = 0; i < viewDragons.Count; i++)
                {
                    var data = viewDragons[i];
                    if (data == null)
                        continue;

                    tableViewItemList.Add(data);
                }
            }

            tableViewGrid.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) =>
            {
                if (node == null)
                    return;

                var frame = node.GetComponent<ChampionBattleDragonPortraitFrame>();
                if (frame == null)
                    return;

                var dragonData = (CharBaseData)item;
                if (ParentPopup.isFull() &&
                    !ParentPopup.SelectedDragonList.Contains(dragonData.KEY))
                {
                    frame.SetCustomPotraitFrameForSelect(dragonData.KEY, false);
                    frame.setCallback(ShowFullPopup);
                    frame.setDetailClickCallback(OnClickSelectDetails);
                }
                else
                {
                    frame.SetCustomPotraitFrameForSelect(dragonData.KEY, ParentPopup.SelectedDragonList.Contains(dragonData.KEY));
                    frame.setCallback(OnSelectDragon);
                    frame.setDetailClickCallback(OnClickSelectDetails);
                }

                frame.ClearStatusUI();
            }));

            tableViewGrid.ReLoad(_initPos);
            viewDirty = false;
        }

        public void DrawBottomView(bool _initPos = true)
        {
            if (tableViewGridBottom == null)
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

            tableViewItemList.Reverse();

            tableViewGridBottom.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) =>
            {
                if (node == null)
                    return;

                var frame = node.GetComponent<DragonPortraitFrame>();
                if (frame == null)
                    return;

                var dragonData = (CharBaseData)item;

                frame.SetCustomPotraitFrame(dragonData.KEY, 0);
            }));

            tableViewGridBottom.ReLoad(_initPos);
        }
        public void ShowFullPopup(string dragonData)
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언텍스트9"), StringData.GetStringByStrKey("확인"), "",
            () => { }
            );
        }

        public void OnSelectDragon(string dragonData)
        {
            if (ParentPopup.SelectedDragonList.Contains(int.Parse(dragonData)))
            {
                ParentPopup.SelectedDragonList.Remove(int.Parse(dragonData));
            }
            else
            {
                ParentPopup.SelectedDragonList.Add(int.Parse(dragonData));
            }
            

            viewDirty = true;
            DrawScrollView(false);

            DrawBottomView();
            SetTitleLabel(ParentPopup.SelectedDragonList.Count);

        }

        public void OnClickSelectDetails(string dragonData)
        {
            ParentPopup.SetDragonData(ParentPopup.UserDragonData.GetDragon(int.Parse(dragonData)) as ChampionDragon);
            DragonTap.moveLayer(1);
        }

        public void OnClickSelectComplete()
        {
#if UNITY_EDITOR
            //귀찮아서 넣음
            ParentPopup.SetRandomFull();
#endif

            if (ParentPopup.isFull())
                DragonTap.moveLayer(2);
            else
            {
                ShowNotEnoughPopup();
            }
        }

        public void SetTitleLabel(int cnt)
        {
            if (cnt < 0 || countLabel == null)
                return;

            countLabel.text = cnt + "/" + ParentPopup.maxCnt;            
        }

        public void ShowNotEnoughPopup()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언텍스트8"), StringData.GetStringByStrKey("확인"), "",
                () => { }
                );
        }
    }
}

