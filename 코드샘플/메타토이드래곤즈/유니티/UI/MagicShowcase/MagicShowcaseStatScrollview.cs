using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 현재 레벨에 따른 누적 스탯 데이터 스크롤 뷰 및 다음 레벨에 대한 정보 노티 스크롤뷰 (2개 관리)
/// 디자인은 따로 안들어가서 일단 유니티 스크롤뷰로만 구성 - 프레임 드랍 심하면 테이블뷰로 전환
/// </summary>
namespace SandboxNetwork
{
    public class MagicShowcaseStatScrollview : MagicShowcaseComponent
    {
        [SerializeField] Text levelTextLabel = null;//현재 레벨단계 표시

        [SerializeField] GameObject statClone = null;//현재 스탯 클론
        [SerializeField] GameObject nextStatClone = null;//다음 단계 보상 클론

        [SerializeField] ScrollRect statScroll = null;
        [SerializeField] ScrollRect nextLevelScroll = null;
        [SerializeField] Text maxLevelLabel = null;
        [SerializeField] Text emptyLevelLabel = null;

        List<MagicShowcaseData> nextLevelBaseDataList = new List<MagicShowcaseData>();//다음 레벨 기준 테이블 데이터

        public override void InitUI(int _currentIndex)
        {
            base.InitUI(_currentIndex);
            SetData();
            SetLevelLabel();
            DrawScrollview();
        }

        void SetData()//현재 해당 타입에 대한 레벨을 가져와야함.
        {
            nextLevelBaseDataList = MagicShowcaseManager.Instance.GetNextLevelTableDataByType(type);
        }

        void SetLevelLabel()//레벨 표시
        {
            if (levelTextLabel != null)
                levelTextLabel.text = StringData.GetStringFormatByStrKey("업그레이드단계", infoData == null ? 0 : infoData.LEVEL);

            if (emptyLevelLabel != null)
                emptyLevelLabel.gameObject.SetActive(infoData == null ? false : infoData.LEVEL <= 0);
        }

        void DrawScrollview()
        {
            SBFunc.RemoveAllChildrens(statScroll.content.transform);
            SBFunc.RemoveAllChildrens(nextLevelScroll.content.transform);

            DrawCurrentLevelStat();
            DrawNextLevelStat();
        }

        void DrawCurrentLevelStat()//현재 획득한 스탯 정보
        {
            var statDic = MagicShowcaseManager.Instance.GetStatTypeListByType(type);
            if (statDic == null || statDic.Count <= 0)
                return;

            foreach (var typeData in statDic)
            {
                var type = typeData.Key;
                var valueTypeDic = typeData.Value;
                var statData = StatTypeData.Get(type);
                if (statData != null && valueTypeDic != null && valueTypeDic.Count > 0)
                {
                    foreach(var valueTypeKeyPair in valueTypeDic)
                    {
                        var valueType = valueTypeKeyPair.Key;
                        var value = valueTypeKeyPair.Value;

                        var clone = Instantiate(statClone, statScroll.content.transform);
                        var statComp = clone.GetComponent<DragonPartStatSlot>();
                        if (statComp == null)
                        {
                            Destroy(clone);
                            continue;
                        }

                        var statStr = valueType == eStatusValueType.PERCENT ? statData.PERCENT_DESC : statData.VALUE_DESC;
                        statComp.SetData(statStr , value, valueType == eStatusValueType.PERCENT,false,true);
                    }
                }
            }
        }

        void DrawNextLevelStat()//다음 레벨 보상 스탯 정보
        {
            // to do 다음 레벨 정보 없으면 (맥렙 - maxLevelLabel 켜기)
            var isMaxLevel = MagicShowcaseData.GetMaxLevelByGroup(type) <= infoData.LEVEL;
            if (maxLevelLabel != null)
                maxLevelLabel.gameObject.SetActive(isMaxLevel);

            if (nextLevelBaseDataList == null || nextLevelBaseDataList.Count <= 0)
                return;

            foreach(var tableData in nextLevelBaseDataList)
            {
                if (tableData == null)
                    continue;

                var rewardLevel = tableData.LEVEL;
                var stat_type = tableData.STAT_TYPE;
                var statData = StatTypeData.Get(stat_type);
                var stat_value_type = tableData.STAT_VALUE_TYPE;
                var stat_value = tableData.STAT_VALUE;
                var statStr = stat_value_type == eStatusValueType.PERCENT ? statData.PERCENT_DESC : statData.VALUE_DESC;


                var clone = Instantiate(nextStatClone, nextLevelScroll.content.transform);
                var statComp = clone.GetComponent<DragonPartStatSlot>();
                if (statComp == null)
                {
                    Destroy(clone);
                    continue;
                }
                statComp.SetData(statStr, stat_value, stat_value_type == eStatusValueType.PERCENT, false, true);
                statComp.SetTitle(StringData.GetStringFormatByStrKey("보상단계", rewardLevel));
            }
        }
    }
}