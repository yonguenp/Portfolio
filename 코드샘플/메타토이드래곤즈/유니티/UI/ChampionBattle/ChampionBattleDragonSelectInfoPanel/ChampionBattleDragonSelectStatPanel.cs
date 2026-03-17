using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionBattleDragonSelectStatPanel : MonoBehaviour
    {
        [SerializeField]
        Text battleLabel = null;
        [SerializeField]
        Text AtkLabel = null;
        [SerializeField]
        Text DefLabel = null;
        [SerializeField]
        Text HealthLabel = null;
        [SerializeField]
        Text critLabel = null;

        [SerializeField]
        ChampionBattleDragonSelectTabLayer tablayer = null;
        
        [SerializeField]
        Image elementTarget = null;

        [SerializeField]
        Sprite[] elementList = null;

        [SerializeField]
        Button showDetailStatButton = null;

        int tempDragonTag = 0;
        int tempDragonLevel = 0;
        ChampionBattleDragonSelectPopup ParentPopup { get { return PopupManager.GetPopup<ChampionBattleDragonSelectPopup>(); } }
        public void Init()
        {
            RefreshCurrentDragonData();
        }
        void RefreshCurrentDragonData()
        {
            if (ParentPopup.DragonTag != 0)//드래곤 태그값
            {
                RefreshButton(ParentPopup.Dragon);
                RefreshDragonStat(ParentPopup.Dragon);
            }
        }

        void RefreshButton(ChampionDragon dragon)
        {
            bool transcendCondition = false;
            bool transcendShowCondition = false;
            bool isTranscendMax = false;
            bool isPassiveExist = false;

            isTranscendMax = CharTranscendenceData.GetStepMax((eDragonGrade)dragon.Grade()) == dragon.TranscendenceStep;
            int requireLv = GameConfigTable.GetConfigIntValue("TRANSCENDENCE_MINIMUM_LEVEL", 50);
            int requireSkillLv = GameConfigTable.GetConfigIntValue("TRANSCENDENCE_MINIMUM_SKILL_LEVEL", 50);
            int requireGrade = GameConfigTable.GetConfigIntValue("TRANSCENDENCE_MINIMUM_GRADE", 4);

            if (dragon.Level >= requireLv && dragon.Grade() >= requireGrade)
            {
                transcendShowCondition = true;
                isPassiveExist = dragon.PassiveSkillSlot > 0;
                transcendCondition = dragon.SLevel >= requireSkillLv;
            }

            if (showDetailStatButton != null)
                showDetailStatButton.SetButtonSpriteState(true);
        }


        void RefreshDragonStat(ChampionDragon dragonInfo)
        {
            bool hasDragon = true;

            int dragonBattlePoint = 0;
            float dragonAtk = 0;
            float dragonDef = 0;
            float dragonHealth = 0;
            float dragonCri = 0;

            if (dragonInfo == null)
                return;

            tempDragonLevel = dragonInfo.Level;
            tempDragonTag = dragonInfo.Tag;

            dragonInfo.RefreshALLStatus();//스탯 계산 한번 더
            var dragonStat = dragonInfo.Status;//스킬 반영 전투력

            dragonBattlePoint = dragonStat.GetTotalINF();
            dragonAtk = dragonStat.GetTotalStatus(eStatusType.ATK);//총 공격
                                                                   //var baseAtk = dragonStat.GetStatus(eStatusCategory.BASE, eStatusType.ATK);//기본 공격
            dragonDef = dragonStat.GetTotalStatus(eStatusType.DEF);
            //var baseDef = dragonStat.GetStatus(eStatusCategory.BASE, eStatusType.DEF);//기본 방어
            dragonHealth = dragonStat.GetTotalStatus(eStatusType.HP);
            //var baseHP = dragonStat.GetStatus(eStatusCategory.BASE, eStatusType.HP);//기본 체력
            dragonCri = dragonStat.GetTotalStatus(eStatusType.CRI_PROC);
            //var baseCri = dragonStat.GetStatus(eStatusCategory.BASE, eStatusType.CRI_PROC);//기본 크리
            //else//비소유 일 때는 강제 맥렙 처리
            //{
            //    var maxDragonLevel = GameConfigTable.GetDragonLevelMax();
            //    var baseData = CharBaseData.Get(dragonTag);
            //    if (baseData != null)
            //    {
            //        dragonStat = SBFunc.BaseCharStatus(maxDragonLevel, baseData, StatFactorData.Get(baseData.FACTOR));
            //        dragonStat.CalcStatusAll();

            //        dragonStat.SetSkillLevel(GameConfigTable.GetSkillLevelMax());
            //        dragonStat.CalcINF();
            //    }
            //}

            battleLabel.alignment = hasDragon ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
            AtkLabel.alignment = hasDragon ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
            DefLabel.alignment = hasDragon ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
            HealthLabel.alignment = hasDragon ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
            critLabel.alignment = hasDragon ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;

            battleLabel.text = hasDragon ? SBFunc.CommaFromNumber(dragonBattlePoint) : "-";
            AtkLabel.text = hasDragon ? SBFunc.CommaFromNumber((int)dragonAtk) : "-";
            DefLabel.text = hasDragon ? SBFunc.CommaFromNumber((int)dragonDef) : "-";
            HealthLabel.text = hasDragon ? SBFunc.CommaFromNumber((int)dragonHealth) : "-";
            critLabel.text = hasDragon ? SBFunc.CommaFromNumber(Math.Round(dragonCri,2)) + "%" : "-";
        }

        

        void RefreshElementIcon(int _element)
        {
            if (elementTarget != null) 
            {
                elementTarget.sprite = elementList[_element - 1];
            }
        }
    }
}

