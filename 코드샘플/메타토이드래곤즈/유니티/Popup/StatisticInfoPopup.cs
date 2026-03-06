using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class StatisticInfoPopup : Popup<StatisticPopupData>
    {
        const int SCROLL_ENABLE_SIZE = 5;

        [SerializeField]
        private ScrollRect scroll = null;

        [SerializeField]
        private GameObject myTeamObj;
        [SerializeField]
        private List<StatisticInfoClone>  statisticInfoObjects = new List<StatisticInfoClone>();
        [SerializeField]
        RectTransform bodyRect;
        [SerializeField]
        private GameObject testBtn;
        [SerializeField]
        private Text testBtnText;

        [SerializeField]
        private Text[] RowTexts;

        bool isShowMyTeam = true;
        public override void InitUI()
        {
            testBtn.SetActive(false);
            testBtnText.text = string.Empty;
            isShowMyTeam = true;
            RefreshMyTeamInfo();

            if (Data.IsArena)
            {
#if UNITY_EDITOR
                testBtn.SetActive(true);
                testBtnText.text = "상대팀 보기";
                //RefreshEnemyTeamInfo();
#endif
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(bodyRect);
        }

        void RefreshMyTeamInfo()
        {
            foreach (var obj in statisticInfoObjects)
            {
                obj.gameObject.SetActive(false);
            }
            var dic = StatisticsMananger.Instance.myDamageDic;
            var bestStatistic = GetBestStatistic(dic.Values.ToList());
            int i = 0;
            foreach (int dragonNo in dic.Keys)
            {
                if (statisticInfoObjects.Count <= i)
                {
                    var obj = Instantiate(statisticInfoObjects[0], statisticInfoObjects[0].transform.parent);
                    statisticInfoObjects.Add(obj);
                }

                statisticInfoObjects[i].gameObject.SetActive(true);
                statisticInfoObjects[i].Init(dragonNo, dic[dragonNo], bestStatistic);
                ++i;
            }

            if (scroll != null)
                scroll.enabled = dic.Keys.Count > SCROLL_ENABLE_SIZE;
        }

        void RefreshEnemyTeamInfo()
        {
            foreach (var obj in statisticInfoObjects)
            {
                obj.gameObject.SetActive(false);
            }
            var dic = StatisticsMananger.Instance.enemyDamageDic;
            var bestStatistic = GetBestStatistic(dic.Values.ToList());
            int i = 0;
            foreach (int dragonNo in dic.Keys)
            {
                statisticInfoObjects[i].gameObject.SetActive(true);
                statisticInfoObjects[i].Init(dragonNo, dic[dragonNo], bestStatistic);
                ++i;
            }
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
            }
            return BestStatsitic;
        }

        public void OnClickChangeMode()
        {
#if UNITY_EDITOR
            if (Data.IsArena)
            {
                if (isShowMyTeam)
                {
                    RefreshEnemyTeamInfo();
                    testBtnText.text = "내 팀 보기";
                }
                else
                {
                    RefreshMyTeamInfo();
                    testBtnText.text = "상대팀 보기";
                }
                isShowMyTeam = !isShowMyTeam;
            }
#endif
        }

    }

}
