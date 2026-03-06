using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class GemDungeonDragonListView : BattleDragonListView
    {
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

            var dat = LandmarkGemDungeon.Get();
            int maxFatigue = GameConfigTable.GetConfigIntValue("CHAR_STAMINA_MAX");
            tableViewGrid.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject itemNode, ITableData item) => {
                if (itemNode == null || item == null)
                {
                    return;
                }
                itemNode.SetActive(true);
                var frame = itemNode.GetComponent<GemDungeonSelectDragonSlot>();
                if (frame == null)
                {
                    return;
                }
                var dragonData = (UserDragon)item;
                var dragonTableData = CharBaseData.Get(dragonData.Tag);
                if (dragonTableData == null)
                    return;
                bool isRegist = IsInTeamDragon(dragonData.Tag);
                var gemDungeonDragonData =dat.GetDragonData(dragonData.Tag);

                int fatigue = maxFatigue;
                if (gemDungeonDragonData != null)
                {
                    fatigue = gemDungeonDragonData.ExpectedFatigue;
                    if (gemDungeonDragonData.Floor != 0)
                    {
                        var floorData = dat.GetFloorData(gemDungeonDragonData.Floor);
                        if (floorData != null)
                        {
                            switch (floorData.State)
                            {
                                case eGemDungeonState.BATTLE: break;
                                case eGemDungeonState.IDLE: break;
                                case eGemDungeonState.END:
                                {
                                    if(floorData.IsFullReward)
                                        fatigue = gemDungeonDragonData.Fatigue;
                                } break;
                                default: fatigue = gemDungeonDragonData.Fatigue; break;
                            }
                        }
                    }
                }
                frame.Init(dragonData, isRegist, fatigue, maxFatigue, (param) =>
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

        protected override System.Comparison<UserDragon> Sort(int index)
        {
            switch (index)
            {
                case 0: return Sort0;
                case 1: return Sort1;
                case 2: return Sort2;
                case 3: return Sort3;
                case 4: return Sort4;
                case 5: return Sort5;
                case 6: return Sort6;
                case 7: return Sort7;
                case 8: return Sort8;
                case 9: return Sort9;
            }

            return null;
        }

        protected virtual int SortRegistedFirstBurnOutLast(int A_dragonNo,int B_dragonNo) // 등록된 드래곤 먼저 표기, 피로도 전부 소모 드래곤 아래 표기
        {
            var dat = LandmarkGemDungeon.Get();
            bool containB =dat.DragonDatas.ContainsKey(B_dragonNo);
            bool containA = dat.DragonDatas.ContainsKey(A_dragonNo);
            if (containB && containA == false)
            {
                if (dat.DragonDatas[B_dragonNo].Floor != 0)
                    return 1;
                else
                {
                    if (dat.DragonDatas[B_dragonNo].ExpectedFatigue == 0)
                        return -1;
                }
            }
            else if (containA && containB == false)
            {
                if (dat.DragonDatas[A_dragonNo].Floor != 0)
                    return -1;
                else
                {
                    if (dat.DragonDatas[A_dragonNo].ExpectedFatigue == 0)
                        return 1;
                }
            }
            else if (containA && containB)
            {
                if (!(dat.DragonDatas[B_dragonNo].Floor == 0 && dat.DragonDatas[A_dragonNo].Floor == 0)) 
                {
                    return dat.DragonDatas[A_dragonNo].Floor - dat.DragonDatas[B_dragonNo].Floor;
                }
                else  // 둘다 휴식 상태
                {
                    if (dat.DragonDatas[B_dragonNo].ExpectedFatigue == 0 || dat.DragonDatas[A_dragonNo].ExpectedFatigue == 0)
                        return dat.DragonDatas[B_dragonNo].ExpectedFatigue - dat.DragonDatas[A_dragonNo].ExpectedFatigue;
                }
            }

            return 0;
        }

        protected virtual int SortRegistedFirst(int A_dragonNo, int B_dragonNo) // 등록된 드래곤 먼저 표기
        {
            var dat = LandmarkGemDungeon.Get();
            bool containB = dat.DragonDatas.ContainsKey(B_dragonNo);
            bool containA = dat.DragonDatas.ContainsKey(A_dragonNo);
            if (containB && containA == false)
            {
                if (dat.DragonDatas[B_dragonNo].Floor != 0)
                    return 1;
            }
            else if (containA && containB == false)
            {
                if (dat.DragonDatas[A_dragonNo].Floor != 0)
                    return -1;
            }
            else if (containA && containB)
            {
                if (!(dat.DragonDatas[B_dragonNo].Floor == 0 && dat.DragonDatas[A_dragonNo].Floor == 0))
                {
                    return dat.DragonDatas[A_dragonNo].Floor - dat.DragonDatas[B_dragonNo].Floor;
                }
            }
            return 0;
        }
        protected override int Sort0(UserDragon a, UserDragon b)
        {
            int val = SortRegistedFirstBurnOutLast(a.Tag,b.Tag);
            if (val != 0)
                return val;

            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortGradeDescend(a, b);
                if (checker == 0)
                {
                    checker = SortLevelDescend(a, b);
                    if (checker == 0)
                    {
                        checker = SortBattlePointDescend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeDescend(a, b);
                            if (checker == 0)
                                return SortTagDescend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        protected override int Sort1(UserDragon a, UserDragon b)
        {
            int val = SortRegistedFirstBurnOutLast(a.Tag, b.Tag);
            if (val != 0)
                return val;

            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortGradeAscend(a, b);
                if (checker == 0)
                {
                    checker = SortLevelAscend(a, b);
                    if (checker == 0)
                    {
                        checker = SortBattlePointAscend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeAscend(a, b);
                            if (checker == 0)
                                return SortTagAscend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        protected override int Sort2(UserDragon a, UserDragon b)
        {
            int val = SortRegistedFirstBurnOutLast(a.Tag, b.Tag);
            if (val != 0)
                return val;
            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortLevelDescend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeDescend(a, b);
                    if (checker == 0)
                    {
                        checker = SortBattlePointDescend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeDescend(a, b);
                            if (checker == 0)
                                return SortTagDescend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        protected override int Sort3(UserDragon a, UserDragon b)
        {
            int val = SortRegistedFirstBurnOutLast(a.Tag, b.Tag);
            if (val != 0)
                return val;
            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortLevelAscend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeAscend(a, b);
                    if (checker == 0)
                    {
                        checker = SortBattlePointAscend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeAscend(a, b);
                            if (checker == 0)
                                return SortTagAscend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        protected override int Sort4(UserDragon a, UserDragon b)
        {
            int val = SortRegistedFirstBurnOutLast(a.Tag, b.Tag);
            if (val != 0)
                return val;

            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortBattlePointDescend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeDescend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelDescend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeDescend(a, b);
                            if (checker == 0)
                                return SortTagDescend(a, b);
                        }
                    }
                }
            }
            return checker;
        }

        protected override int Sort5(UserDragon a, UserDragon b)
        {
            int val = SortRegistedFirstBurnOutLast(a.Tag, b.Tag);
            if (val != 0)
                return val;
            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortBattlePointAscend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeAscend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelAscend(a, b);
                        if (checker == 0)
                        {
                            checker = SortObtainTimeAscend(a, b);
                            if (checker == 0)
                                return SortTagAscend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        protected override int Sort6(UserDragon a, UserDragon b)
        {
            int val = SortRegistedFirstBurnOutLast(a.Tag, b.Tag);
            if (val != 0)
                return val;

            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortObtainTimeDescend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeDescend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelDescend(a, b);
                        if (checker == 0)
                        {
                            checker = SortBattlePointDescend(a, b);
                            if (checker == 0)
                                return SortTagDescend(a, b);
                        }
                    }
                }
            }

            return checker;
        }

        protected override int Sort7(UserDragon a, UserDragon b)
        {
            int val = SortRegistedFirstBurnOutLast(a.Tag, b.Tag);
            if (val != 0)
                return val;
            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortObtainTimeAscend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeAscend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelAscend(a, b);
                        if (checker == 0)
                        {
                            checker = SortBattlePointAscend(a, b);
                            if (checker == 0)
                                return SortTagAscend(a, b);
                        }
                    }
                }
            }

            return checker;
        }
        protected virtual int Sort8(UserDragon a, UserDragon b)
        {
            int val = SortRegistedFirst(a.Tag, b.Tag);
            if (val != 0)
                return val;
            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortFatigueDescend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeDescend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelDescend(a, b);
                        if (checker == 0)
                        {
                            checker = SortBattlePointDescend(a, b);
                            if (checker == 0)
                            {
                                checker = SortObtainTimeDescend(a, b);
                                if (checker == 0)
                                    return SortTagDescend(a, b);
                            }
                        }
                    }
                }
            }


            return checker;
        }
        protected virtual int Sort9(UserDragon a, UserDragon b)
        {
            int val = SortRegistedFirst(a.Tag, b.Tag);
            if (val != 0)
                return val;
            var checker = SortFavorite(a, b);
            if (checker == 0)
            {
                checker = SortFatigueAscend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeAscend(a, b);
                    if (checker == 0)
                    {
                        checker = SortLevelAscend(a, b);
                        if (checker == 0)
                        {
                            checker = SortBattlePointAscend(a, b);
                            if (checker == 0)
                            {
                                checker = SortObtainTimeAscend(a, b);
                                if (checker == 0)
                                    return SortTagAscend(a, b);
                            }
                        }
                    }
                }
            }

            return checker;
        }

        protected int SortFatigueDescend(UserDragon a, UserDragon b)
        {
            var aFatigue = GetFatigue(a.Tag);
            var bFatigue = GetFatigue(b.Tag);
            return bFatigue - aFatigue;
        }
        //등급 오름차순
        protected int SortFatigueAscend(UserDragon a, UserDragon b)
        {
            var aFatigue = GetFatigue(a.Tag);
            var bFatigue = GetFatigue(b.Tag);
            return aFatigue - bFatigue;
        }

        int GetFatigue(int dragonTag)
        {
            var dragonDat = LandmarkGemDungeon.Get().GetDragonData(dragonTag);
            if (dragonDat == null)
                return GameConfigTable.GetConfigIntValue("CHAR_STAMINA_MAX");
            return dragonDat.ExpectedFatigue;
        }
    }

}
