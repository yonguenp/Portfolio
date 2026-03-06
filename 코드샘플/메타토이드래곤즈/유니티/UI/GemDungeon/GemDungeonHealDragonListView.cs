using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GemDungeonHealDragonListView : BattleDragonListView
    {
        [SerializeField] Image healItemIcon;
        [SerializeField] Text healItemCountText;
        [SerializeField] GameObject noneDragonAlert;
        [SerializeField] Transform contentTransform;

        
        public override void Init()
        {
            userDragons = new();
            var curDragon = LandmarkGemDungeon.Get().DragonDatas;
            int maxFatigue = GameConfigTable.GetConfigIntValue("CHAR_STAMINA_MAX");
            foreach ( var dragon in curDragon.Values)
            {
                if (Mathf.CeilToInt(dragon.ExpectedFatigue / (float)maxFatigue * SBDefine.BASE_FLOAT) < SBDefine.BASE_FLOAT)
                {
                    var dragonDat = User.Instance.DragonData.GetDragon(dragon.DragonNo);
                    userDragons.Add(dragonDat);
                }
            }

            if (tableViewGrid != null && !isInitFirst)
            {
                tableViewGrid.OnStart();
                isInitFirst = true;
            }

            InitDragonInfoData();
            InitCustomSort();
        }
        protected override void SetListCustomSort(int sortIndex)
        {
            if (userDragons == null)
            {
                userDragons = new();
                var curDragon = LandmarkGemDungeon.Get().DragonDatas;
                int maxFatigue = GameConfigTable.GetConfigIntValue("CHAR_STAMINA_MAX");
                foreach (var dragon in curDragon.Values)
                {
                    if (dragon.ExpectedFatigue < maxFatigue)
                    {
                        var dragonDat = User.Instance.DragonData.GetDragon(dragon.DragonNo);
                        userDragons.Add(dragonDat);
                    }
                }

            }

            base.SetListCustomSort(sortIndex);
        }

        public override void DrawScrollView(bool _initPos = true)
        {
            var healItem = ItemBaseData.GetItemListByKind(eItemKind.GEM_FATIGUE_RECOVERY);
            int healItemCount = 0;
            foreach (var item in healItem)
            {
                healItemCount += User.Instance.GetItemCount(item.KEY);
            }
            healItemIcon.sprite = healItem[0].ICON_SPRITE;
            healItemCountText.text = healItemCount.ToString();

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

            noneDragonAlert.SetActive(tableViewItemList.Count == 0);
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
                var gemDungeonDragonData = dat.GetDragonData(dragonData.Tag);

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
                                    if (floorData.IsFullReward)
                                        fatigue = gemDungeonDragonData.Fatigue;
                                }
                                break;
                                default: fatigue = gemDungeonDragonData.Fatigue; break;
                            }
                        }
                    }
                }
                frame.Init(dragonData, isRegist, fatigue, maxFatigue, IsInTeamDragon(dragonData.Tag), (param) =>
                {
                    bool check = IsInTeamDragon(int.Parse(param));
                    if (check)
                    {
                        if (clickReleaseCallback != null)
                        {
                            frame.SetSelectedState(false);
                            clickReleaseCallback(dragonData.Tag.ToString());
                        }
                    }
                    else
                    {
                        if (clickRegistCallBack != null)
                        {
                            frame.SetSelectedState(true);
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
        protected virtual int Sort8(UserDragon a, UserDragon b)
        {
            var checker = SortFatigueDescend(a, b);
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
        protected virtual int Sort9(UserDragon a, UserDragon b)
        {
            var checker = SortFatigueAscend(a, b);
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

        public void SetFatigueHealEffect(List<int> curEffectDragons,VoidDelegate effectEndCallBack)
        {
            SoundManager.Instance.PlaySFX("sfx_part_reinforcesuccess");
            if (curEffectDragons == null || curEffectDragons.Count == 0)
            {
                effectEndCallBack?.Invoke();
                return;
            }
            var slots = contentTransform.GetComponentsInChildren<GemDungeonSelectDragonSlot>();
            foreach (var slot in slots)
            {
                if (slot == null)
                    continue;
                if (curEffectDragons.Contains(slot.DragonTag))
                {
                    slot.ShowHeal();
                }
            }
            StartCoroutine(EffectEndCor(effectEndCallBack));
        }

        IEnumerator EffectEndCor(VoidDelegate callBack )
        {
            yield return new WaitForSeconds(0.7f);
            callBack?.Invoke();
        }
    }
}

