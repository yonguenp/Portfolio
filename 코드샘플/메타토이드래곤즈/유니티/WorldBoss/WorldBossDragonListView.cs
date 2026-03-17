using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class WorldBossDragonListView : BattleDragonListView
    {
        const int MAX_PARTY_COUNT = 4;
        /// <summary>
        /// 현재 슬롯을 제외한 나머지 덱 인덱스 가져오기 추가
        /// </summary>
        protected override void InitDragonTagList(int[] dragonTagArr)
        {
            dragonTagList = new List<int>();
            for (int i = 0; i < dragonTagArr.Length; ++i)
            {
                if (dragonTagArr[i] != 0)
                {
                    dragonTagList.Add(dragonTagArr[i]);
                }
            }

            var currentIndex = WorldBossManager.Instance.UIDeckIndex;

            for(int i = 0; i< MAX_PARTY_COUNT; i++)
            {
                if (i == currentIndex)
                    continue;

                var targetList = User.Instance.PrefData.WorldBossFormationData.GetTemporaryFormation(i)?.ToList();
                if(targetList != null && targetList.Count > 0)
                    dragonTagList.AddRange(targetList);
            }

            dragonTagList = dragonTagList.Distinct().ToList();
        }
        /// <summary>
        /// 변경 점 -> 기존 선택된 드래곤만 표시할 것이 아니라, 덱 전체의 선택 상태를 알아야함.
        /// </summary>
        /// <param name="_initPos"></param>
        public override void DrawScrollView(bool _initPos = true)
        {
            if (!viewDirty || tableViewGrid == null || viewDragons == null)
            {
                return;
            }

            if (invenCheckLabel != null)
            {
                var isEmpty = viewDragons.Count <= 0;
                invenCheckLabel.gameObject.SetActive(isEmpty);
            }

            List<ITableData> tableViewItemList = new List<ITableData>();
            tableViewItemList.Clear();
            if (viewDragons != null && viewDragons.Count > 0)
            {
                for (var i = 0; i < viewDragons.Count; i++)
                {
                    var data = viewDragons[i];
                    if (data == null)
                    {
                        continue;
                    }

                    tableViewItemList.Add(data);
                }
            }


            tableViewGrid.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject itemNode, ITableData item) => {
                if (itemNode == null || item == null)
                {
                    return;
                }
                var frame = itemNode.GetComponent<WorldBossDragonPortraitFrame>();
                if (frame == null)
                {
                    return;
                }
                var dragonData = (UserDragon)item;
                var dragonTableData = CharBaseData.Get(dragonData.Tag);
                if (dragonTableData == null)
                    return;
                bool isRegist = IsInTeamDragon(dragonData.Tag);

                frame.SetDragonPortraitFrame(dragonData, isRegist);
                if (User.Instance.DragonData.IsFavorite(dragonData.Tag))
                {
                    frame.SetFrameColor(new Color(0.0f, 0.8f, 0.0f));
                }

                frame.setCallback((param) =>
                {
                    bool check = IsInTeamDragon(int.Parse(param));
                    if (check)
                    {
                        if (clickReleaseCallback != null)
                        {
                            clickReleaseCallback(dragonData.Tag.ToString());
                        }
                    }
                    else
                    {
                        if (clickRegistCallBack != null)
                        {
                            clickRegistCallBack(dragonData.Tag.ToString());
                        }
                    }
                });
            }));

            tableViewGrid.ReLoad(_initPos);
            viewDirty = false;
        }
    }
}

