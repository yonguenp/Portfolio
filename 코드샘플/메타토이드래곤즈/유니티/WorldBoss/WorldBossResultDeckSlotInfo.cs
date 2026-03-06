using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class WorldBossResultDeckSlotInfo : MonoBehaviour
    {
        [SerializeField] List<WorldBossResultDragonSlotClone> slotList = new List<WorldBossResultDragonSlotClone>();
        [SerializeField] Text emptyLabelText = null;

        int deckIndex = -1;
        List<int> deckIDList = null;
        public void SetData(int _deckIndex)
        {
            deckIndex = _deckIndex;
            RefreshMyTeamInfo();
        }

        void RefreshMyTeamInfo()
        {
            deckIDList = User.Instance.PrefData.WorldBossFormationData.GetFormationData(deckIndex);
            if(deckIDList == null || deckIDList.Count <= 0)
            {
                foreach (var obj in slotList)
                    if (obj != null)
                        obj.gameObject.SetActive(false);

                if (emptyLabelText != null)
                    emptyLabelText.gameObject.SetActive(true);
                return;
            }

            foreach (var obj in slotList)
                if (obj != null)
                    obj.gameObject.SetActive(false);

            var list = StatisticsMananger.Instance.myDamageDic.Values.ToList();
            if((list == null || list.Count <= 0) && emptyLabelText != null)
            {
                emptyLabelText.gameObject.SetActive(true);
                return;
            }

            //var bestStatistic = GetBestStatistic(list);
            list = list.FindAll(FindStaticsList);
            list.Sort(SortStaticsList);
            for (int i = 0, count = slotList.Count; i < count; ++i)
            {
                if(list.Count <= i)
                {
                    if (slotList[i] != null)
                        slotList[i].gameObject.SetActive(false);
                    continue;
                }
                var data = list[i];
                if (data == null)
                    continue;

                slotList[i].gameObject.SetActive(true);
                slotList[i].Init(i == 0, data.ID, data/*, bestStatistic*/);//통계 비교 안씀(각 덱별로 정렬 첫 번째를 강조하는 것으로 변경)
            }

            var isEmptyCheckCount = 0;
            foreach (var id in deckIDList)
                if (id <= 0)
                    isEmptyCheckCount++;

            if (emptyLabelText != null)
                emptyLabelText.gameObject.SetActive(isEmptyCheckCount == deckIDList.Count || deckIDList.Count <= 0);
        }
        private bool FindStaticsList(StatisticsInfo info)
        {
            if (deckIDList == null)
                return false;

            return deckIDList.Contains(info.ID);
        }
        private int SortStaticsList(StatisticsInfo info1, StatisticsInfo info2)
        {
            if (info1.BossDmg > info2.BossDmg)
                return -1;
            else if (info1.BossDmg < info2.BossDmg)
                return 1;
            else
                return 0;
        }
        StatisticsInfo GetBestStatistic(List<StatisticsInfo> statistics)
        {
            StatisticsInfo BestStatsitic = new(0);
            foreach (var obj in statistics)
            {
                if (obj.RecieveDmgInShield > BestStatsitic.RecieveDmgInShield)
                    BestStatsitic.RecieveDmgInShield = obj.RecieveDmgInShield;
                if (obj.RecieveDmgReal > BestStatsitic.RecieveDmgReal)
                    BestStatsitic.RecieveDmgReal = obj.RecieveDmgReal;
                if (obj.SkillCount > BestStatsitic.SkillCount)
                    BestStatsitic.SkillCount = obj.SkillCount;
                if (obj.NormalAtkCount > BestStatsitic.NormalAtkCount)
                    BestStatsitic.NormalAtkCount = obj.NormalAtkCount;
                if (obj.TotalDmg > BestStatsitic.TotalDmg)
                    BestStatsitic.TotalDmg = obj.TotalDmg;
                if (obj.SkillTotalDmg > BestStatsitic.SkillTotalDmg)
                    BestStatsitic.SkillTotalDmg = obj.SkillTotalDmg;
                if (obj.SkillTotalDmgToShield > BestStatsitic.SkillTotalDmgToShield)
                    BestStatsitic.SkillTotalDmgToShield = obj.SkillTotalDmgToShield;
                if (obj.NormalAtkTotalDmg > BestStatsitic.NormalAtkTotalDmg)
                    BestStatsitic.NormalAtkTotalDmg = obj.NormalAtkTotalDmg;
                if (obj.NormalTotalDmgToShield > BestStatsitic.NormalTotalDmgToShield)
                    BestStatsitic.NormalTotalDmgToShield = obj.NormalTotalDmgToShield;
                if (obj.BestSkillDmg > BestStatsitic.BestSkillDmg)
                    BestStatsitic.BestSkillDmg = obj.BestSkillDmg;
                if (obj.BestNormalAtkDmg > BestStatsitic.BestNormalAtkDmg)
                    BestStatsitic.BestNormalAtkDmg = obj.BestNormalAtkDmg;
                if (obj.BossDmg > BestStatsitic.BossDmg)
                    BestStatsitic.BossDmg = obj.BossDmg;
            }
            return BestStatsitic;
        }
    }
}
