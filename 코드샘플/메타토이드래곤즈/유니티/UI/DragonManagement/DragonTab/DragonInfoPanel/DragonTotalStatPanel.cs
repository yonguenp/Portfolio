using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonTotalStatPanel : DragonManageSubPanel
    {
        [SerializeField]
        Color oddColor = new Color();
        [SerializeField]
        Color evenColor = new Color();

        [SerializeField]
        Text battlePointLabel = null;
        [SerializeField]
        Text AtkLabel = null;
        [SerializeField]
        Text DefLabel = null;
        [SerializeField]
        Text HealthLabel = null;
        [SerializeField]
        Text critLabel = null;

        [SerializeField]
        TableView tableView = null;

        [SerializeField]
        GameObject statSlotObject = null;
        [SerializeField]
        GameObject statLineObject = null;
        [SerializeField]
        GameObject contentParent = null;

        private bool isTableInit = false;

        private List<ITableData> statInfolist = new List<ITableData>();
        public override void ShowPanel(VoidDelegate _successCallback = null)
        {
            base.ShowPanel();
        }

        public override void HidePanel()
        {
            base.HidePanel();
        }
        public override void Init()
        {
            base.Init();

            if (tableView != null && !isTableInit)
            {
                tableView.OnStart();
                isTableInit = true;
            }

            CharacterStatus stat = null;
            var hasDragon = User.Instance.DragonData.IsUserDragon(dragonTag);
            if (hasDragon)
            {
                var userDragon = User.Instance.DragonData.GetDragon(dragonTag);
                if (userDragon != null)
                    stat = userDragon.GetALLStatus();
            }
            else
            {
                var maxDragonLevel = GameConfigTable.GetDragonLevelMax();
                var baseData = CharBaseData.Get(dragonTag);
                if (baseData != null)
                {
                    stat = SBFunc.BaseCharStatus(maxDragonLevel, baseData, StatFactorData.Get(baseData.FACTOR));
                    stat.CalcStatusAll();

                    stat.SetSkillLevel(GameConfigTable.GetSkillLevelMax());
                }
            }

            DrawScollviewInstancePrefab(stat);
        }
        void SetDetailData(CharacterStatus _dragonStat)
        {
            statInfolist.Clear();

            if (battlePointLabel != null)
                battlePointLabel.text = SBFunc.CommaFromNumber(_dragonStat.GetTotalINF());//결과 전투력

            for (eStatusType type = eStatusType.ATK; type < eStatusType.MAX; type++)
            {
                var statData = StatTypeData.Get(type);
                if (statData == null)
                    continue;

                var totalStatByType = _dragonStat.GetTotalStatus(type);//각 타입 별 합산 총 스탯
                AddStatInfoData(totalStatByType, statData.TYPE_DESC, false);

                //그냥 토탈 하나값만 뿌리기로 결정해서 세부내용은 제외
                //if (statData.VALUE == eStatusData.VALUE)//value 면 기본 base, add, ratio 계산
                //{
                //    for (eStatusCategory cType = eStatusCategory.BASE; cType < eStatusCategory.ADD_BUFF; cType++)
                //    {
                //        var isPercent = false;
                //        var value = _dragonStat.GetStatus(cType,type);

                //        var stringFactor = "";
                //        switch(cType)
                //        {
                //            case eStatusCategory.BASE://기본
                //                stringFactor = "기본 {0}";
                //                break;
                //            case eStatusCategory.ADD://추가
                //                stringFactor = "추가 {0}";
                //                break;
                //            case eStatusCategory.RATIO://증가
                //                stringFactor = "{0} 증가";
                //                isPercent = true;
                //                break;
                //        }

                //        AddStatInfoData(value, string.Format(stringFactor, StringData.GetString(statData._DESC)), isPercent);
                //    }
                //}
            }

            DrawScrollView(statInfolist);
        }

        void AddStatInfoData(float _value, string _desc, bool _isPercent, bool _isCategory = false)
        {
            statInfo tempStat = new statInfo();
            tempStat.isPercent = _isPercent;
            tempStat.modifyStatStr = _desc;
            tempStat.statvalue = _value;
            tempStat.isCategory = _isCategory;
            statInfolist.Add(tempStat);
        }

        void DrawScollviewInstancePrefab(CharacterStatus _dragonStat)
        {
            SBFunc.RemoveAllChildrens(contentParent.transform);

            if (battlePointLabel != null)
                battlePointLabel.text = SBFunc.CommaFromNumber(_dragonStat.GetTotalINF());//결과 전투력

            List<statInfo> list = new List<statInfo>();
            list.Clear();

            var clientIndex = 0;
            for (eStatusType type = eStatusType.ATK; type < eStatusType.MAX; type++)
            {
                var statData = StatTypeData.Get(type);
                if (statData == null)
                    continue;

                switch (type)
                {
                    //기본 스텟은 리스트 포함x
                    case eStatusType.ATK or eStatusType.DEF or eStatusType.HP or eStatusType.CRI_PROC:
                    {
                        SetDefaultStat(type, _dragonStat.GetTotalStatus(type));
                        clientIndex++;
                    } continue; 
                    //
                    //치명타 데미지는 base, add , ratio 따로 뿌려달라고함
                    case eStatusType.CRI_DMG:
                    {
                        var baseValue = _dragonStat.GetStatus(eStatusCategory.BASE, type);
                        list.Add(GetStatInfo(baseValue, false, StringData.GetStringByIndex(100002843), statData.SORT_GROUP, int.Parse(statData.KEY), clientIndex));
                        clientIndex++;
                        var ratioValue = _dragonStat.GetStatus(eStatusCategory.RATIO, type);
                        list.Add(GetStatInfo(ratioValue, true, statData.PERCENT_DESC, statData.SORT_GROUP, int.Parse(statData.KEY), clientIndex));
                        clientIndex++;
                        var addValue = _dragonStat.GetStatus(eStatusCategory.ADD, type);
                        list.Add(GetStatInfo(addValue, false, statData.VALUE_DESC, statData.SORT_GROUP, int.Parse(statData.KEY), clientIndex));
                        clientIndex++;
                    }
                    break;
                    //주속성 대미지 추가
                    case eStatusType.FIRE_DMG:
                    case eStatusType.WATER_DMG:
                    case eStatusType.EARTH_DMG:
                    case eStatusType.WIND_DMG:
                    case eStatusType.DARK_DMG:
                    case eStatusType.LIGHT_DMG:
                    {
                        statInfo tempData = new statInfo();
                        tempData.statvalue = _dragonStat.GetTotalStatus(type);
                        tempData.isCategory = false;
                        tempData.modifyStatStr = statData.TYPE_DESC;
                        tempData.isPercent = statData.VALUE_TYPE == eStatusValueType.PERCENT;
                        tempData.sortOrderIndex = statData.SORT_GROUP;
                        tempData.statTypeKey = int.Parse(statData.KEY);
                        tempData.clientOrderIndex = clientIndex;

                        eElementType elem = eElementType.None;
                        switch (type)
                        {
                            case eStatusType.FIRE_DMG:
                                elem = eElementType.FIRE; break;
                            case eStatusType.WATER_DMG:
                                elem = eElementType.WATER; break;
                            case eStatusType.EARTH_DMG:
                                elem = eElementType.EARTH; break;
                            case eStatusType.WIND_DMG:
                                elem = eElementType.WIND; break;
                            case eStatusType.DARK_DMG:
                                elem = eElementType.DARK; break;
                            case eStatusType.LIGHT_DMG:
                                elem = eElementType.LIGHT; break;
                        }

                        if (dragonBase != null && dragonBase.ELEMENT_TYPE == elem)
                        {
                            tempData.statvalue += _dragonStat.GetTotalStatus(eStatusType.ADD_MAIN_ELEMENT_DMG);
                        }

                        list.Add(tempData);
                        clientIndex++;
                    }
                    break;
                    //보스 데미지는 2줄로 표현
                    case eStatusType.BOSS_DMG:
                    {
                        var baseValue = _dragonStat.GetStatus(eStatusCategory.BASE, type);
                        var addValue = _dragonStat.GetStatus(eStatusCategory.ADD, type);
                        list.Add(GetStatInfo(baseValue + addValue, false, statData.STAT_DESC, statData.SORT_GROUP, int.Parse(statData.KEY), clientIndex));
                        clientIndex++;

                        var ratioValue = _dragonStat.GetStatus(eStatusCategory.RATIO, type) + _dragonStat.GetStatus(eStatusCategory.RATIO, eStatusType.RATIO_BOSS_DMG);
                        list.Add(GetStatInfo(ratioValue, true, statData.PERCENT_DESC, statData.SORT_GROUP, int.Parse(statData.KEY), clientIndex));
                        clientIndex++;
                    }
                    break;
                    //
                    //모든 속성 대미지, 모든 속성 대미지 저항은 안보여줌.
                    case eStatusType.ALL_ELEMENT_DMG or eStatusType.ALL_ELEMENT_DMG_RESIS: continue;
                    //
                    //그 외 일반 스텟
                    default:
                    {
                        statInfo tempData = new statInfo();
                        tempData.statvalue = _dragonStat.GetTotalStatus(type);
                        tempData.isCategory = false;
                        tempData.modifyStatStr = statData.TYPE_DESC;
                        tempData.isPercent = statData.VALUE_TYPE == eStatusValueType.PERCENT;
                        tempData.sortOrderIndex = statData.SORT_GROUP;
                        tempData.statTypeKey = int.Parse(statData.KEY);
                        tempData.clientOrderIndex = clientIndex;

                        list.Add(tempData);
                        clientIndex++;
                    }
                    break;
                    //
                }
            }

            if (list == null || list.Count <= 0)
                return;

            var floorSortIndex = list[0].sortOrderIndex;
            list.Sort((statInfo a, statInfo b) =>
            {
                var sortValue = a.sortOrderIndex - b.sortOrderIndex;
                sortValue = sortValue == 0 ? a.statTypeKey - b.statTypeKey : sortValue;
                sortValue = sortValue == 0 ? a.clientOrderIndex - b.clientOrderIndex : sortValue;
                if (sortValue < 0)
                    return -1;
                else if (sortValue > 0)
                    return 1;
                else
                    return 0;
            });

            foreach (var data in list)
            {
                if (data == null)
                    continue;

                if (data.sortOrderIndex <= 0)
                    continue;

                if (floorSortIndex != data.sortOrderIndex)
                {
                    Instantiate(statLineObject, contentParent.transform);
                    floorSortIndex = data.sortOrderIndex;
                }

                SetPrefab(data);
            }
        }
        statInfo GetStatInfo(float _value, bool _isPercent, string _strDesc, int _sortOrder, int _key, int clientIndex)
        {
            statInfo tempData = new statInfo();
            tempData.statvalue = _value;
            tempData.isCategory = false;
            tempData.modifyStatStr = _strDesc;
            tempData.isPercent = _isPercent;
            tempData.sortOrderIndex = _sortOrder;
            tempData.statTypeKey = _key;
            tempData.clientOrderIndex = clientIndex;

            return tempData;
        }


        void SetDefaultStat(eStatusType _type, float _value)
        {
            switch (_type)
            {
                case eStatusType.ATK:
                    AtkLabel.text = /*StringTable.GetString(100000178) + " "+*/SBFunc.CommaFromNumber((int)_value);
                    break;
                case eStatusType.DEF:
                    DefLabel.text = /*StringTable.GetString(100000179)+ " "+*/SBFunc.CommaFromNumber((int)_value);
                    break;
                case eStatusType.HP:
                    HealthLabel.text = /*StringTable.GetString(100000180)+ " "+ */SBFunc.CommaFromNumber((int)_value);
                    break;
                case eStatusType.CRI_PROC:
                    critLabel.text = /*StringTable.GetString(100000181)+ " "+ */SBFunc.CommaFromNumber(Math.Round(_value, 2)) + "%";
                    break;
            }
        }

        void SetPrefab(statInfo _data)
        {
            if (_data == null)
                return;

            var typeValue = _data.statvalue;
            var desc = _data.modifyStatStr;
            var isPercent = _data.isPercent;
            var isCategory = _data.isCategory;
            var sortIndex = _data.sortOrderIndex;
            var clone = Instantiate(statSlotObject, contentParent.transform);
            var slotComp = clone.GetComponent<DragonPartStatSlot>();
            if (slotComp == null)
            {
                Destroy(clone);
                return;
            }

            slotComp.SetData(desc, typeValue, isPercent);
            slotComp.SetColor(sortIndex % 2 == 0 ? evenColor : oddColor);
        }
        void SetPrefab(float _value, string _desc, bool _isPercent, bool _isCategory = false)
        {
            var clone = Instantiate(statSlotObject, contentParent.transform);
            var slotComp = clone.GetComponent<DragonPartStatSlot>();
            if (slotComp == null)
            {
                Destroy(clone);
                return;
            }

            slotComp.SetData(_desc, _value, _isPercent);
        }
        void DrawScrollView(List<ITableData> statList)
        {
            tableView.SetDelegate(new TableViewDelegate(statList, (GameObject node, ITableData item) =>
            {
                if (node == null)
                {
                    return;
                }
                var frame = node.GetComponent<DragonPartStatSlot>();
                if (frame == null)
                {
                    return;
                }

                var statData = (statInfo)item;
                frame.SetData(statData.modifyStatStr, statData.statvalue, statData.isPercent);
            }));

            tableView.ReLoad();
        }
        public override void ForceUpdate()
        {
            Init();
        }
    }
}
