using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum ChampionStatTypePriority
    {
        ATK,
        ATK_PER,
        DEF,
        DEF_PER,
        HP,
        HP_PER,
        CRITICAL,
    }

    public class ChampionStatInfo : ITableData
    {
        public int statTypeKey;
        public float statvalue;
        public string modifyStatStr;
        public bool isPercent;
        public bool isCategory;
        public int sortOrderIndex;
        public int clientOrderIndex;

        public void Init() { }
        public string GetKey() { return ""; }
    }

    public class ChampionBattleDragonPartTotalStatPanel : MonoBehaviour
    {
        [SerializeField]
        Color oddColor = new Color();

        [SerializeField]
        List<DragonPartStatSlot> customSlotList = new List<DragonPartStatSlot>();

        public void CustomInit()
        {
            var dragonTag = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().DragonTag;
            if (dragonTag <= 0)
                return;

            ChampionDragon dragonData = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
            if (dragonData == null)
            {
                return;
            }

            var defaultDragonStat = GetDragonBaseStatus(dragonData);//드래곤 테이블 기준 기본 스탯치
            var partStat = GetPartStatus(dragonData);//드래곤 데이터 테이블 기준으로 장비를 착용했을 때 가산 치를 구함
            if (partStat == null)
                return;

            List<ChampionStatInfo> list = new List<ChampionStatInfo>();
            list.Clear();

            for (eStatusType type = eStatusType.ATK; type < eStatusType.MAX; type++)
            {
                var statData = StatTypeData.Get(type);
                if (statData == null)
                    continue;

                var totalStatByTypeValue = partStat.GetTotalStatus(type);//각 타입 별 합산 총 스탯

                if (statData.GEM_STAT == 0)//0이면 노티 제외
                    continue;

                var tempStatByDragonStat = defaultDragonStat.GetTotalStatus(type);//테이블로 기본 드래곤 스탯
                totalStatByTypeValue -= tempStatByDragonStat;//상승분만 구하려면 기존값 - 테이블값
                if (totalStatByTypeValue <= 0)
                    totalStatByTypeValue = 0;

                ChampionStatInfo tempData = new ChampionStatInfo();
                tempData.statvalue = totalStatByTypeValue;
                tempData.isCategory = false;
                tempData.modifyStatStr = statData.TYPE_DESC;
                tempData.isPercent = statData.VALUE_TYPE == eStatusValueType.PERCENT;
                tempData.sortOrderIndex = statData.SORT_GROUP;

                if(type == eStatusType.ATK || type == eStatusType.DEF || type == eStatusType.HP || type == eStatusType.BOSS_DMG)//공방체(+보뎀)만 %로 변경 표기 요청
                {
                    tempData.isPercent = true;
                    tempData.statvalue = partStat.GetStatus(eStatusCategory.RATIO, type);

                    if(type == eStatusType.BOSS_DMG)
                    {
                        tempData.statvalue += partStat.GetStatus(eStatusCategory.RATIO, eStatusType.RATIO_BOSS_DMG);
                    }
                }

                list.Add(tempData);
            }

            if (list == null || list.Count <= 0)
                return;

            var floorSortIndex = list[0].sortOrderIndex;
            list.Sort((ChampionStatInfo a, ChampionStatInfo b) => {
                var sortValue = a.sortOrderIndex - b.sortOrderIndex;
                if (sortValue == 0)
                    return a.statTypeKey - b.statTypeKey;
                else
                    return sortValue;
            });

            InitPrefab();

            for (int i = 0; i< list.Count; i++)
            {
                var data = list[i];
                if (data == null)
                    continue;

                if (data.sortOrderIndex <= 0)
                    continue;

                SetPrefab(data, i);
            }
        }
        void SetPrefab(ChampionStatInfo _data, int _index)
        {
            if (_data == null)
                return;

            var typeValue = _data.statvalue;
            var desc = _data.modifyStatStr;
            var isPercent = _data.isPercent;
            var isCategory = _data.isCategory;
            var sortIndex = _data.sortOrderIndex;
            customSlotList[_index].SetData(desc, typeValue, isPercent);
            var remain = _index / 2;
            if (remain % 2 == 1)
                customSlotList[_index].SetBG(oddColor);
            else
                customSlotList[_index].SetBG(new Color(0,0,0,0));

            customSlotList[_index].gameObject.SetActive(true);
        }

        void InitPrefab()
        {
            foreach(var slot in customSlotList)
            {
                if (slot == null)
                    continue;
                slot.gameObject.SetActive(false);
            }
        }

        CharacterStatus GetPartStatus(ChampionDragon _dragonData)
        {
            var addedStatus = new List<UnitStatus>();

            var currentDragonTag = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().DragonTag;
            if (currentDragonTag <= 0)
                return null;

            var dragonData = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
            if(dragonData == null)
                return null;

            List<UserPart> equipedParts = new List<UserPart>();
            foreach (var part in _dragonData.ChampionPart.Values)
            {
                equipedParts.Add(part);
            }

            equipedParts.ForEach((element) =>
            {
                if (element == null)
                    return;

                addedStatus.Add(element.GetALLStat());
            });

            //장착 세트 효과 계산
            if (dragonData.PartsSetList != null && dragonData.PartsSetList.Count() > 0)
                addedStatus.Add(SBFunc.GetPartSetEffectOption(dragonData.PartsSetList));

            CharacterStatus status = _dragonData.GetDragonBaseStatus(_dragonData.Tag, _dragonData.Level);//현재 드래곤 데이터(테이블) 기준으로 스텟 추가치를 구함
            for (int i = 0, count = addedStatus.Count; i < count; ++i)
            {
                if (addedStatus[i] == null)
                    continue;

                status.IncreaseStatus(addedStatus[i]);
            }
            status.CalcStatusAll();

            return status;
        }

        CharacterStatus GetDragonBaseStatus(UserDragon _dragonData)
        {
            var tag = _dragonData.Tag;
            var level = _dragonData.Level;

            return _dragonData.GetDragonBaseStatus(tag, level);
        }
    }
}
