using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class StatisticInfoClone : MonoBehaviour
    {
        [SerializeField]
        private Color BestColor = Color.white;
        [SerializeField]
        private Color NormalColor = Color.white;

        [SerializeField]
        private Image dragonImg;
        [SerializeField]
        private Image dragonImgBack;
        [SerializeField]
        private Text dragonNameText;
        [SerializeField]
        private Text totalDmgText;
        [SerializeField]
        private Text skillDmgText;
        [SerializeField]
        private Text normalDmgText;
        [SerializeField]
        private Text skillUseCountText;
        [SerializeField]
        private Text normalAtkCountText;
        [SerializeField]
        private Text skillAtkToShield;
        [SerializeField]
        private Text normalAtkToShield;
        [SerializeField]
        private Text bestSkillAtkText;
        [SerializeField]
        private Text bestNormalAtkText;
        [SerializeField]
        private Text shieldedDmgText;
        [SerializeField]
        private Text realRecieveDmgText;
        [SerializeField]
        private Text LifeTimeText;

        public void Init(int dragonNo, StatisticsInfo info, StatisticsInfo bestStatistic)
        {
            var dragonDat = CharBaseData.Get(dragonNo);
            dragonImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath, dragonDat.THUMBNAIL);
            dragonNameText.text = StringData.GetStringByStrKey(dragonDat._NAME);
            var resourceString = MakeStringByGradeAndElement(dragonDat.GRADE);
            var icon = ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, resourceString);
            if(icon != null)
                dragonImgBack.sprite = icon;

            // 탈주로 인해 0이 많아지는 경우를 고려해 0은 아예 강조조차 하지 않도록 함
            totalDmgText.text = info.TotalDmg.ToString();
            SetTextState(totalDmgText, bestStatistic.TotalDmg == info.TotalDmg && info.TotalDmg !=0);
            skillDmgText.text = info.SkillTotalDmg.ToString();
            SetTextState(skillDmgText, bestStatistic.SkillTotalDmg == info.SkillTotalDmg &&info.SkillTotalDmg != 0);
            normalDmgText.text = info.NormalAtkTotalDmg.ToString();
            SetTextState(normalDmgText, bestStatistic.NormalAtkTotalDmg == info.NormalAtkTotalDmg && info.NormalAtkTotalDmg != 0);
            skillUseCountText.text = info.SkillCount.ToString();
            SetTextState(skillUseCountText, bestStatistic.SkillCount == info.SkillCount && info.SkillCount != 0);
            normalAtkCountText.text = info.NormalAtkCount.ToString();
            SetTextState(normalAtkCountText, bestStatistic.NormalAtkCount == info.NormalAtkCount && info.NormalAtkCount != 0);
            skillAtkToShield.text = info.SkillTotalDmgToShield.ToString();
            SetTextState(skillAtkToShield, bestStatistic.SkillTotalDmgToShield == info.SkillTotalDmgToShield && info.SkillTotalDmgToShield != 0);
            normalAtkToShield.text = info.NormalTotalDmgToShield.ToString();
            SetTextState(normalAtkToShield, bestStatistic.NormalTotalDmgToShield == info.NormalTotalDmgToShield && info.NormalTotalDmgToShield != 0);
            bestSkillAtkText.text = info.BestSkillDmg.ToString();
            SetTextState(bestSkillAtkText, bestStatistic.BestSkillDmg == info.BestSkillDmg && info.BestSkillDmg != 0);
            bestNormalAtkText.text = info.BestNormalAtkDmg.ToString();
            SetTextState(bestNormalAtkText, bestStatistic.BestNormalAtkDmg == info.BestNormalAtkDmg && info.BestNormalAtkDmg != 0);
            shieldedDmgText.text = info.RecieveDmgInShield.ToString();
            SetTextState(shieldedDmgText, bestStatistic.RecieveDmgInShield == info.RecieveDmgInShield && info.RecieveDmgInShield != 0);
            realRecieveDmgText.text = info.RecieveDmgReal.ToString();
            SetTextState(realRecieveDmgText, bestStatistic.RecieveDmgReal == info.RecieveDmgReal && info.RecieveDmgReal != 0);

            if (info.AliveTime == -1)
            {
                LifeTimeText.text = "-";
                dragonImgBack.color = dragonImg.color = Color.white;
            }
            else
            {
                LifeTimeText.text = info.AliveTime.ToString();
                dragonImgBack.color = dragonImg.color = Color.gray;
            }
            
        }
        void SetTextState(Text text,bool isBest)
        {
            text.color = (isBest) ? BestColor : NormalColor;
            text.fontStyle = (isBest) ? FontStyle.Bold : FontStyle.Normal;
        }
        string MakeStringByGradeAndElement(int grade)
        {
            var gradeString = GetGradeConvertString(grade);
            return SBFunc.StrBuilder("bggrade_", gradeString);
        }
        string GetGradeConvertString(int grade)
        {
            var gradeNameStrIndex = CharGradeData.Get(grade.ToString())._NAME;
            var gradeString = StringData.GetStringByIndex(gradeNameStrIndex).ToLower();
            return gradeString;
        }
    }
}
