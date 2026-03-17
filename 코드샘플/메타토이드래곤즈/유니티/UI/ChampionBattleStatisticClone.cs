using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class ChampionBattleStatisticClone : MonoBehaviour
    {
        [Header("color")]
        [SerializeField]
        Color allDmgColor = Color.white;
        [SerializeField]
        Color skillDmgColor = Color.white;
        [SerializeField]
        Color normalDmgColor = Color.white;
        [SerializeField]
        Color skillCntColor = Color.white;
        [SerializeField]
        Color normalCntColor = Color.white;
        [SerializeField]
        Color blockSkillDmgColor = Color.white;
        [SerializeField]
        Color blockNormalDmgColor = Color.white;
        [SerializeField]
        Color bestSkillDmgColor = Color.white;
        [SerializeField]
        Color bestNormalDmgColor = Color.white;
        [SerializeField]
        Color absorbedDmgColor = Color.white;
        [SerializeField]
        Color realRecvDmgColor = Color.white;
        [SerializeField]
        Color aliveTimeColor = Color.white;

        [Header("dragon")]
        [SerializeField]
        DragonPortraitFrame dragonFrame = null;
        [SerializeField]
        Text dragonNameText = null;

        [Header("graph")]
        [SerializeField]
        ChampionBattleGraphClone[] ChampionBattleGraphClones = null;


        StatisticsInfo CurrentStatistic = null;
        StatisticsInfo MaxStatistic = null;
        /// <summary>
        /// 최초 1회 데이터 세팅용 (필요한 데이터 전부 세팅)
        /// </summary>
        /// <param name="statistics">현재 드래곤의 통계 값</param>
        /// <param name="maxStatistics">통계 값 별 최대치 데이터</param>
        public void SetData(StatisticsInfo statistics, StatisticsInfo maxStatistics , int dragonLv, int dragonTranscend = 0) 
        {
            CurrentStatistic = statistics;
            MaxStatistic = maxStatistics;
            dragonNameText.text = StringData.GetStringByStrKey(CharBaseData.Get(statistics.ID)._NAME);
            dragonFrame.SetCustomPotraitFrame(statistics.ID, dragonLv, dragonTranscend);
            dragonFrame.SetDragonDimmed(statistics.AliveTime>=0);
        }


        /// <summary>
        /// 탭 바꿀때마다 가지고 있는 데이터에서 전환시켜주는 용도
        /// </summary>
        public void SetShowData(params eDamageStatisticsType[] type)
        {
            foreach(var clone in ChampionBattleGraphClones)
            {
                clone.gameObject.SetActive(false);
            }
            if(CurrentStatistic == null)
            {
                gameObject.SetActive(false);
                return;
            }
            if (type == null)
                return;
            for(int i = 0; i < ChampionBattleGraphClones.Length; i++)
            {
                if(type.Length <= i)
                {
                    ChampionBattleGraphClones[i].gameObject.SetActive(false);
                    continue;
                }
                switch (type[i])
                {
                    case eDamageStatisticsType.AllDmg:
                        ChampionBattleGraphClones[i].SetData( CurrentStatistic.TotalDmg, MaxStatistic.TotalDmg, allDmgColor);
                        break;
                    case eDamageStatisticsType.SkillDmg:
                        ChampionBattleGraphClones[i].SetData(CurrentStatistic.SkillTotalDmg, MaxStatistic.SkillTotalDmg, skillDmgColor);
                        break;
                    case eDamageStatisticsType.NormalDmg:
                        ChampionBattleGraphClones[i].SetData(CurrentStatistic.NormalAtkTotalDmg, MaxStatistic.NormalAtkTotalDmg, normalDmgColor);
                        break;
                    case eDamageStatisticsType.SkillUseCnt:
                        ChampionBattleGraphClones[i].SetData(CurrentStatistic.SkillCount, MaxStatistic.SkillCount, skillCntColor);
                        break;
                    case eDamageStatisticsType.NormalUseCnt:
                        ChampionBattleGraphClones[i].SetData(CurrentStatistic.NormalAtkCount, MaxStatistic.NormalAtkCount, normalCntColor);
                        break;
                    case eDamageStatisticsType.BlockedSkillDmg:
                        ChampionBattleGraphClones[i].SetData(CurrentStatistic.SkillTotalDmgToShield, MaxStatistic.SkillTotalDmgToShield, blockSkillDmgColor);
                        break;
                    case eDamageStatisticsType.BlockedNormalDmg:
                        ChampionBattleGraphClones[i].SetData(CurrentStatistic.NormalTotalDmgToShield, MaxStatistic.NormalTotalDmgToShield, blockNormalDmgColor);
                        break;
                    case eDamageStatisticsType.BestSkillDmg:
                        ChampionBattleGraphClones[i].SetData(CurrentStatistic.BestSkillDmg, MaxStatistic.BestSkillDmg, bestSkillDmgColor);
                        break;
                    case eDamageStatisticsType.BestNormalDmg:
                        ChampionBattleGraphClones[i].SetData(CurrentStatistic.BestNormalAtkDmg, MaxStatistic.BestNormalAtkDmg, bestNormalDmgColor);
                        break;
                    case eDamageStatisticsType.AbsorbedDmg:
                        ChampionBattleGraphClones[i].SetData(CurrentStatistic.RecieveDmgInShield, MaxStatistic.RecieveDmgInShield, absorbedDmgColor);
                        break;
                    case eDamageStatisticsType.RealRecvDmg:
                        ChampionBattleGraphClones[i].SetData(CurrentStatistic.RecieveDmgReal, MaxStatistic.RecieveDmgReal, realRecvDmgColor);
                        break;
                    case eDamageStatisticsType.AliveTime:
                        ChampionBattleGraphClones[i].SetData(CurrentStatistic.AliveTime, MaxStatistic.AliveTime, aliveTimeColor);
                        break;
                    default:
                        ChampionBattleGraphClones[i].gameObject.SetActive(false);
                        break;
                }
            }
            
        }


        private void OnDisable()
        {
            CurrentStatistic = null;
            MaxStatistic = null;
        }
    }
}

