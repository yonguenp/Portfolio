using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionDragonDetailStatPanel : MonoBehaviour
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
        ChampionDragonDetailPopup ParentPopup { get { return PopupManager.GetPopup<ChampionDragonDetailPopup>(); } }
        public void Init()
        {
            RefreshCurrentDragonData();
        }
        void RefreshCurrentDragonData()
        {
            if (ParentPopup.Dragon != null)
            {
                RefreshButton();
                RefreshDragonStat();
            }

        }

        void RefreshButton()
        {
            var dragon = ParentPopup.Dragon;

            bool transcendCondition = false;
            bool transcendShowCondition = false;
            bool isTranscendMax = false;
            bool isPassiveExist = false;
            if (dragon != null)
            {
                isTranscendMax = CharTranscendenceData.GetStepMax((eDragonGrade)dragon.Grade()) == dragon.TranscendenceStep;
                int requireLv = GameConfigTable.GetConfigIntValue("TRANSCENDENCE_MINIMUM_LEVEL", 50);
                int requireSkillLv = GameConfigTable.GetConfigIntValue("TRANSCENDENCE_MINIMUM_SKILL_LEVEL", 50);
                int requireGrade = GameConfigTable.GetConfigIntValue("TRANSCENDENCE_MINIMUM_GRADE", 4);
                
                if (dragon.Level >= requireLv  && dragon.Grade() >= requireGrade)
                {
                    transcendShowCondition = true;
                    isPassiveExist = dragon.PassiveSkillSlot > 0;
                    transcendCondition = dragon.SLevel >= requireSkillLv;
                }
            }

            if (showDetailStatButton != null)
                showDetailStatButton.SetButtonSpriteState(dragon != null);
        }


        void RefreshDragonStat()
        {
            int dragonBattlePoint = 0;
            float dragonAtk = 0;
            float dragonDef = 0;
            float dragonHealth = 0;
            float dragonCri = 0;

            CharacterStatus dragonStat = null;

            var dragon = ParentPopup.Dragon;
            if(dragon != null)
            {
                tempDragonLevel = dragon.Level;
                tempDragonTag = dragon.Tag;

                dragon.RefreshALLStatus();//스탯 계산 한번 더
                dragonStat = dragon.Status;//스킬 반영 전투력

                dragonBattlePoint = dragonStat.GetTotalINF();
                dragonAtk = dragonStat.GetTotalStatus(eStatusType.ATK);//총 공격
                                                                       //var baseAtk = dragonStat.GetStatus(eStatusCategory.BASE, eStatusType.ATK);//기본 공격
                dragonDef = dragonStat.GetTotalStatus(eStatusType.DEF);
                //var baseDef = dragonStat.GetStatus(eStatusCategory.BASE, eStatusType.DEF);//기본 방어
                dragonHealth = dragonStat.GetTotalStatus(eStatusType.HP);
                //var baseHP = dragonStat.GetStatus(eStatusCategory.BASE, eStatusType.HP);//기본 체력
                dragonCri = dragonStat.GetTotalStatus(eStatusType.CRI_PROC);
                //var baseCri = dragonStat.GetStatus(eStatusCategory.BASE, eStatusType.CRI_PROC);//기본 크리
            }
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

            battleLabel.alignment = dragon != null ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
            AtkLabel.alignment = dragon != null ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
            DefLabel.alignment = dragon != null ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
            HealthLabel.alignment = dragon != null ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
            critLabel.alignment = dragon != null ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;

            battleLabel.text = dragon != null ? SBFunc.CommaFromNumber(dragonBattlePoint) : "-";
            AtkLabel.text = dragon != null ? SBFunc.CommaFromNumber((int)dragonAtk) : "-";
            DefLabel.text = dragon != null ? SBFunc.CommaFromNumber((int)dragonDef) : "-";
            HealthLabel.text = dragon != null ? SBFunc.CommaFromNumber((int)dragonHealth) : "-";
            critLabel.text = dragon != null ? SBFunc.CommaFromNumber(Math.Round(dragonCri,2)) + "%" : "-";
        }

        public void CustomRefreshDragonStat(int dragonTag,int customLevel,int customSkillLevel ,List<UserPart> equipedParts, UserPet petData)
        {
            var dragonInfo = ChampionManager.Instance.MyInfo.ChampionDragons.GetDragon(dragonTag);
            if (dragonInfo == null)
            {
                return;
            }

            var partEffectList = dragonInfo.GetPartEffectOption(equipedParts.ToArray());

            tempDragonLevel = customLevel;
            tempDragonTag = dragonTag;

            var dragonStat = dragonInfo.GetDragonCustomStat(customLevel, customSkillLevel, equipedParts, partEffectList, petData);//스킬 반영 전투력
            var dragonBattlePoint = dragonStat.GetTotalINF();
            var dragonAtk = dragonStat.GetTotalStatus(eStatusType.ATK);
            var dragonDef = dragonStat.GetTotalStatus(eStatusType.DEF);
            var dragonHealth = dragonStat.GetTotalStatus(eStatusType.HP);
            var dragonCri = dragonStat.GetTotalStatus(eStatusType.CRI_PROC);

            battleLabel.text = /*StringTable.GetString(100000177) + " "+*/SBFunc.CommaFromNumber((int)dragonBattlePoint);
            AtkLabel.text = /*StringTable.GetString(100000178) + " "+*/SBFunc.CommaFromNumber((int)dragonAtk);
            DefLabel.text = /*StringTable.GetString(100000179)+ " "+*/SBFunc.CommaFromNumber((int)dragonDef);
            HealthLabel.text = /*StringTable.GetString(100000180)+ " "+ */SBFunc.CommaFromNumber((int)dragonHealth);
            critLabel.text = /*StringTable.GetString(100000181)+ " "+ */SBFunc.CommaFromNumber(Math.Round(dragonCri, 2)) + "(%)";

            RefreshElementIcon(dragonInfo.Element());
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

