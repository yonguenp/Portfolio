using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionBattleStatisticPopup : Popup<ChampionBattleStatisticPopupData>
    {
        [SerializeField]
        Button[] tabs = null;
        [SerializeField]
        RectTransform Back = null;
        [SerializeField]
        Text battleTimeText = null;
        [SerializeField]
        ChampionBattleStatisticClone[] myTeamInfos = null;
        [SerializeField]
        UserArenaPortraitFrame myPortrait = null;
        [SerializeField]
        Text myNickText = null;
        [SerializeField]
        GameObject myWinIcon = null;
        [SerializeField]
        GameObject myLoseIcon = null;
        [SerializeField]
        ChampionBattleStatisticClone[] enemyTeamInfos = null;
        [SerializeField]
        UserArenaPortraitFrame enemyPortrait = null;
        [SerializeField]
        Text enemyNickText = null;
        [SerializeField]
        GameObject enemyWinIcon = null;
        [SerializeField]
        GameObject enemyLoseIcon = null;
        [SerializeField]
        ServerFlagUI luser_server = null;
        [SerializeField]
        ServerFlagUI ruser_server = null;

        [Space(10)]
        [Header("centerText")]
        [SerializeField]
        Text[] texts1 = null;
        [SerializeField]
        Text[] texts2 = null;
        [SerializeField]
        Text[] texts3 = null;
        [SerializeField]
        Text[] texts4 = null;
        [SerializeField]
        Text[] texts5 = null;

        StatisticsInfo MaxData = null;

        Coroutine gradientCor = null;

        float BattleTime
        {
            get
            {
                return Data.BattleData.BattleTime;
            }
        }

        eChampionWinType WinType
        {
            get
            {
                return Data.BattleData.WinType;
            }
        }

        Dictionary<int, StatisticsInfo> SideA
        {
            get
            {
                return Data.BattleData.GetStatisticsInfo(true);
            }
        }

        Dictionary<int, StatisticsInfo> SideB
        {
            get
            {
                return Data.BattleData.GetStatisticsInfo(false);
            }
        }

        ChampionUserInfo SideAUser
        {
            get
            {
                return Data.BattleData.UserA;
            }
        }

        ChampionUserInfo SideBUser
        {
            get
            {
                return Data.BattleData.UserB;
            }
        }

        string SideANick
        {
            get
            {
                return Data.BattleData.ASideNick;
            }
        }

        string SideBNick
        {
            get
            {
                return Data.BattleData.BSideNick;
            }
        }



        public override void InitUI()
        {
            SetIconsOff();
            SetWinIcon();
            battleTimeText.text = SBFunc.TimeStringMinute(BattleTime);
            SetMaxData();
            SetOffenceTeamData();
            SetDefenceTeamData();
            OnClickMenu(0);

            SetExitCallback(Data.CloseCB);
        }

        void SetIconsOff()
        {
            myWinIcon.SetActive(false);
            enemyWinIcon.SetActive(false);
            myLoseIcon.SetActive(false);
            enemyLoseIcon.SetActive(false);
        }

        void SetWinIcon()
        {
            if (WinType == eChampionWinType.SIDE_A_WIN)
            {
                myWinIcon.SetActive(true);
                enemyLoseIcon.SetActive(true);
            }
            else
            {
                enemyWinIcon.SetActive(true);
                myLoseIcon.SetActive(true);
            }
        }

        void SetGradientEffect()
        {
            Back.localScale = new Vector3(WinType == eChampionWinType.SIDE_A_WIN ? 1 : -1, 1f, 1f);
            gradientCor = StartCoroutine(GradientCor(0.5f,-0.163f,true));
        }

        IEnumerator GradientCor(float maxTime, float offset,bool blackToWhite)
        {
            Back.GetComponent<UIGradient>().offset = blackToWhite ? -1f : 1f;
            float timer = 0;
            while (maxTime > timer)
            {
                Back.GetComponent<UIGradient>().offset = blackToWhite ? Mathf.Lerp(-1f, offset, timer / maxTime) : Mathf.Lerp(offset, 1f, timer / maxTime);
                timer += Time.deltaTime;
                yield return null;
            }
            SetWinIcon();
        }

        public override void ClosePopup()
        {
            if(gradientCor != null) 
                StopCoroutine(gradientCor);
            base.ClosePopup();
        }


        /// <summary>
        /// 내팀, 상대팀 전부를 합친 최대값 데이터 세팅
        /// </summary>
        void SetMaxData()
        {
            var allData = SideA.Values.ToList();
            allData.AddRange(SideB.Values.ToList());
            MaxData = MakeMaxData(allData);
        }

        void SetOffenceTeamData()
        {
            foreach(var item in myTeamInfos)
            {
                item.gameObject.SetActive(false);
            }

            var list = SideA.Values.ToList();
            var maxData = MakeMaxData(list);

            if (SideAUser == null)
            {
                myPortrait.SetUserPortraitFrame("");
                if (luser_server != null)
                    luser_server.SetFlag(0);
            }
            else
            {
                myPortrait.SetUserPortraitFrame(new ArenaUserData(SideAUser));
                if (luser_server != null)
                    luser_server.SetFlag(SideAUser.Server);
            }
            myNickText.text = SideANick;
            for (int i =0, count = list.Count; i < count; ++i)
            {
                myTeamInfos[i].SetData(list[i], MaxData, 0, 0);
                myTeamInfos[i].gameObject.SetActive(true);
            }
        }

        void SetDefenceTeamData()
        {
            foreach (var item in enemyTeamInfos)
            {
                item.gameObject.SetActive(false);
            }
            var list = SideB.Values.ToList();
            var maxData = MakeMaxData(list);

            if (SideBUser == null)
            {
                enemyPortrait.SetUserPortraitFrame("");
                if (ruser_server != null)
                    ruser_server.SetFlag(0);
            }
            else
            {
                enemyPortrait.SetUserPortraitFrame(new ArenaUserData(SideBUser));
                if (ruser_server != null)
                    ruser_server.SetFlag(SideBUser.Server);
            } 

            enemyNickText.text = SideBNick;
            for (int i = 0, count = list.Count; i < count; ++i)
            {
                enemyTeamInfos[i].SetData(list[i], MaxData, 0, 0);
                enemyTeamInfos[i].gameObject.SetActive(true);
            }
        }


        /// <summary>
        /// 데이터 중 최대값 구하는 파트, 세부 사항은 주석 참조
        /// </summary>
        /// [스킬 총 데미지]와 [평타 총 데미지]의 최대값은 {전체 총데미지}으로 설정
        /// [스킬 사용횟수]와 [평타 사용횟수]의 최대값은 {둘 중 가장 높은 값}으로 설정 
        /// [보호막에 막힌 스킬 피해량]과 [보호막에 막힌 평타 피해량]의 최대값은 {둘 중 가장 높은 값}으로 설정
        /// [최고 스킬 데미지]와 [최고 평타 데미지]의 최대값은 {둘 중 가장 높은 값}으로 설정
        /// [보호막이 흡수한 데미지]와 [실제 입은 데미지]의 최대값은 {둘 중 가장 높은 값}으로 설정
        StatisticsInfo MakeMaxData(List<StatisticsInfo> infos)
        {
            StatisticsInfo MaxData = new(0);
            foreach (var obj in infos)
            {
                if (obj.RecieveDmgInShield > MaxData.RecieveDmgInShield)
                    MaxData.RecieveDmgInShield = obj.RecieveDmgInShield;
                if (obj.RecieveDmgReal > MaxData.RecieveDmgReal)
                    MaxData.RecieveDmgReal = obj.RecieveDmgReal;
                if (obj.SkillCount > MaxData.SkillCount)
                    MaxData.SkillCount = obj.SkillCount;
                if (obj.NormalAtkCount > MaxData.NormalAtkCount)
                    MaxData.NormalAtkCount = obj.NormalAtkCount;
                if (obj.TotalDmg > MaxData.TotalDmg)
                    MaxData.TotalDmg = obj.TotalDmg;
                if (obj.SkillTotalDmg > MaxData.SkillTotalDmg)
                    MaxData.SkillTotalDmg = obj.SkillTotalDmg;
                if (obj.SkillTotalDmgToShield > MaxData.SkillTotalDmgToShield)
                    MaxData.SkillTotalDmgToShield = obj.SkillTotalDmgToShield;
                if (obj.NormalAtkTotalDmg > MaxData.NormalAtkTotalDmg)
                    MaxData.NormalAtkTotalDmg = obj.NormalAtkTotalDmg;
                if (obj.NormalTotalDmgToShield > MaxData.NormalTotalDmgToShield)
                    MaxData.NormalTotalDmgToShield = obj.NormalTotalDmgToShield;
                if (obj.BestSkillDmg > MaxData.BestSkillDmg)
                    MaxData.BestSkillDmg = obj.BestSkillDmg;
                if (obj.BestNormalAtkDmg > MaxData.BestNormalAtkDmg)
                    MaxData.BestNormalAtkDmg = obj.BestNormalAtkDmg;

                MaxData.AliveTime = (int)BattleTime;
            }
            MaxData.SkillTotalDmg = MaxData.TotalDmg;
            MaxData.NormalAtkTotalDmg = MaxData.TotalDmg;
            
            if(MaxData.SkillCount > MaxData.NormalAtkCount)
                MaxData.NormalAtkCount = MaxData.SkillCount;
            else
                MaxData.SkillCount = MaxData.NormalAtkCount;
            
            if (MaxData.SkillTotalDmgToShield > MaxData.NormalTotalDmgToShield)
                MaxData.NormalTotalDmgToShield = MaxData.SkillTotalDmgToShield;
            else
                MaxData.SkillTotalDmgToShield = MaxData.NormalTotalDmgToShield;

            if (MaxData.BestSkillDmg > MaxData.BestNormalAtkDmg)
                MaxData.BestNormalAtkDmg = MaxData.BestSkillDmg;
            else
                MaxData.BestSkillDmg = MaxData.BestNormalAtkDmg;

            if (MaxData.RecieveDmgInShield > MaxData.RecieveDmgReal)
                MaxData.RecieveDmgReal = MaxData.RecieveDmgInShield;
            else
                MaxData.RecieveDmgInShield = MaxData.RecieveDmgReal;

            return MaxData;
        }

        void SetTeamInfoMode(params eDamageStatisticsType[] type)
        {
            foreach(var team in myTeamInfos)
            {
                team.SetShowData(type);
            }
            foreach (var team in enemyTeamInfos)
            {
                team.SetShowData(type);
            }
        }

        public void OnClickMenu(int index)
        {
            foreach (var tab in tabs)
            {
                tab.interactable = true;
            }
            tabs[index].interactable = false;
            switch ((eArenaStatisticType)index)
            {
                case eArenaStatisticType.Default:
                    SetTeamInfoMode(eDamageStatisticsType.AllDmg, eDamageStatisticsType.RealRecvDmg, eDamageStatisticsType.AliveTime);
                    SetText(eDamageStatisticsType.AllDmg, eDamageStatisticsType.RealRecvDmg, eDamageStatisticsType.AliveTime);
                    break;
                case eArenaStatisticType.AllDmg:
                    SetTeamInfoMode(eDamageStatisticsType.AllDmg, eDamageStatisticsType.SkillDmg, eDamageStatisticsType.NormalDmg);
                    SetText(eDamageStatisticsType.AllDmg, eDamageStatisticsType.SkillDmg, eDamageStatisticsType.NormalDmg);
                    break;
                case eArenaStatisticType.Count:
                    SetTeamInfoMode(eDamageStatisticsType.SkillUseCnt, eDamageStatisticsType.NormalUseCnt);
                    SetText(eDamageStatisticsType.SkillUseCnt, eDamageStatisticsType.NormalUseCnt);
                    break;
                case eArenaStatisticType.BlockDmg:
                    SetTeamInfoMode(eDamageStatisticsType.BlockedSkillDmg, eDamageStatisticsType.BlockedNormalDmg);
                    SetText(eDamageStatisticsType.BlockedSkillDmg, eDamageStatisticsType.BlockedNormalDmg);
                    break;
                case eArenaStatisticType.BestDmg:
                    SetTeamInfoMode(eDamageStatisticsType.BestSkillDmg, eDamageStatisticsType.BestNormalDmg);
                    SetText(eDamageStatisticsType.BestSkillDmg, eDamageStatisticsType.BestNormalDmg);
                    break;
                case eArenaStatisticType.Etc:
                    SetTeamInfoMode(eDamageStatisticsType.AbsorbedDmg, eDamageStatisticsType.RealRecvDmg, eDamageStatisticsType.AliveTime);
                    SetText(eDamageStatisticsType.AbsorbedDmg, eDamageStatisticsType.RealRecvDmg, eDamageStatisticsType.AliveTime);
                    break;
            }
        }

        void SetText(params eDamageStatisticsType[] type)
        {
            for(int i =0, count = texts1.Length; i < count; ++i)
            {
                texts1[i].gameObject.SetActive(false);
                texts2[i].gameObject.SetActive(false);
                texts3[i].gameObject.SetActive(false);
                texts4[i].gameObject.SetActive(false);
                texts5[i].gameObject.SetActive(false);
            }
            for (int i = 0, count = type.Length; i < count; ++i)
            {
                string infoText = StringData.GetStringByStrKey( type[i] switch
                {
                    eDamageStatisticsType.AllDmg => "battle_end_statistic_total_dmg",
                    eDamageStatisticsType.SkillDmg => "battle_end_statistic_skill_dmg",
                    eDamageStatisticsType.NormalDmg => "battle_end_statistic_normal_dmg",
                    eDamageStatisticsType.SkillUseCnt => "battle_end_statistic_skill_count",
                    eDamageStatisticsType.NormalUseCnt => "battle_end_statistic_normal_count",
                    eDamageStatisticsType.BlockedSkillDmg => "battle_end_statistic_skill_shield_dmg",
                    eDamageStatisticsType.BlockedNormalDmg => "battle_end_statistic_normal_shield_dmg",
                    eDamageStatisticsType.BestSkillDmg => "battle_end_statistic_skill_highest_dmg",
                    eDamageStatisticsType.BestNormalDmg => "battle_end_statistic_normal_highest_dmg",
                    eDamageStatisticsType.AbsorbedDmg => "battle_end_statistic_shield_blocked_dmg",
                    eDamageStatisticsType.RealRecvDmg => "battle_end_statistic_hit_dmg",
                    eDamageStatisticsType.AliveTime => "battle_end_statistic_life_time",

                    _ => ""
                });

                texts1[i].gameObject.SetActive(true);
                texts1[i].text = infoText;
                texts2[i].gameObject.SetActive(true);
                texts2[i].text = infoText;
                texts3[i].gameObject.SetActive(true);
                texts3[i].text = infoText;
                texts4[i].gameObject.SetActive(true);
                texts4[i].text = infoText;
                texts5[i].gameObject.SetActive(true);
                texts5[i].text = infoText;
            }
        }
    }
}

