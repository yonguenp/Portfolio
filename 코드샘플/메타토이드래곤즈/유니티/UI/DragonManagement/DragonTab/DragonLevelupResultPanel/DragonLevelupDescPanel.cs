using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//드래곤 레벨업 팝업에서 드래곤 스탯 박는 용도
namespace SandboxNetwork
{
    public class DragonLevelupDescPanel : MonoBehaviour
    {
        //CharBaseTable charDataTable = null;
        Text dragonLevelLabel = null;

        public Text battleLabel = null;
        [SerializeField]
        Text AtkLabel = null;
        [SerializeField]
        Text AtkAftLabel = null;
        [SerializeField]
        Text DefLabel = null;
        [SerializeField]
        Text DefAftLabel = null;
        [SerializeField]
        Text HealthLabel = null;
        [SerializeField]
        Text HealthAftLabel = null;
        [SerializeField]
        Text critLabel = null;
        [SerializeField]
        Text critAftLabel = null;

        CharacterStatus prevStat = null;
        public CharacterStatus PrevStat { get { return prevStat; } }
        CharacterStatus currentStat = null;
        public CharacterStatus CurrentStat { get { return currentStat; } }


        public void Init(int dragonTag, CharacterStatus prevStat, int prevLevel, bool isNextLevel)
        {
            var dragonData = User.Instance.DragonData;
            if (dragonData == null)
                return;

            var userDragonInfo = dragonData.GetDragon(dragonTag);
            if (userDragonInfo == null)
                return;

            RefreshDragonStat(userDragonInfo, prevStat, prevLevel, isNextLevel);
        }
        void RefreshDragonStat(UserDragon dragonData, CharacterStatus prevStat ,int prevLevel ,bool isNextLevel)
        {
            if (dragonData == null)
                return;

            var userDragonStat = dragonData.Status;//현재 올라가버린 드래곤 스탯
            if (userDragonStat == null)
                return;

            var currentINF = userDragonStat.GetTotalINF();
            var currentAtk = userDragonStat.GetTotalStatus(eStatusType.ATK);
            var currentDef = userDragonStat.GetTotalStatus(eStatusType.DEF);
            var currentHp = userDragonStat.GetTotalStatus(eStatusType.HP);
            var currentCri = userDragonStat.GetTotalStatus(eStatusType.CRI_PROC);

            currentStat = userDragonStat;

            //이전 상태

            this.prevStat = prevStat;

            var prevBP = prevStat.GetTotalINF();
            var prevAtk = prevStat.GetTotalStatus(eStatusType.ATK);
            var prevDef = prevStat.GetTotalStatus(eStatusType.DEF);
            var prevHp = prevStat.GetTotalStatus(eStatusType.HP);
            var prevCri = prevStat.GetTotalStatus(eStatusType.CRI_PROC);

            //이전 상승치
            var modAtk = currentAtk - prevAtk;
            var modDef = currentDef - prevDef;
            var modHp = currentHp - prevHp;
            var modCri = currentCri - prevCri;

            if (isNextLevel)
            {
                battleLabel.text = Mathf.FloorToInt(currentINF).ToString();

                AtkLabel.text = Mathf.FloorToInt(currentAtk).ToString();
                AtkAftLabel.text = modAtk > 0 ? SBFunc.StrBuilder("+", (int)modAtk) : "";

                DefLabel.text = Mathf.FloorToInt(currentDef).ToString();
                DefAftLabel.text = modDef > 0 ? SBFunc.StrBuilder("+", (int)modDef) : "";

                HealthLabel.text = Mathf.FloorToInt(currentHp).ToString();
                HealthAftLabel.text = modHp > 0 ? SBFunc.StrBuilder("+", (int)modHp) : "";

                critLabel.text = Math.Round(currentCri, 2).ToString() + (modCri > 0 ? "" : "%");
                critAftLabel.text = modCri > 0 ? SBFunc.StrBuilder("+", Math.Round(modCri, 2), "%") : "";
            }
            else
            {
                dragonLevelLabel.text = string.Format(StringData.GetStringByIndex(100000056), prevLevel);
                battleLabel.text = Mathf.FloorToInt(prevBP).ToString();
                AtkLabel.text = Mathf.FloorToInt(prevAtk).ToString();
                DefLabel.text = Mathf.FloorToInt(prevDef).ToString();
                HealthLabel.text = Mathf.FloorToInt(prevHp).ToString();
                critLabel.text = Math.Round(prevCri,2) .ToString() + "%";
            }
        }
    }
}
