using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
//드래곤 현재 레벨에 따른 슬롯 열림 / 닫힌 상태 파악 및 해당 레벨에 따른 해금 노티
namespace SandboxNetwork
{
    public class ChampionBattlePassivePanel : MonoBehaviour
    {
        [Header("skill 1")]
        [SerializeField] Text skill1Text;

        [Header("skill 2")]
        [SerializeField] Text skill2Text;

        [SerializeField] List<Text> skillList = new List<Text>();


        public void Init()
        {
            RefreshUI();
        }

        void RefreshUI()
        {
            if (skillList == null) return;

            for(int i = 0; i < skillList.Count; i++)
            {
                skillList[i].text = StringData.GetStringByStrKey("스킬미획득");
            }

            if (PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().DragonTag != 0)//드래곤 태그값
            {
                var dragonTag = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().DragonTag;
                var userDragonData = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
                if (userDragonData == null)
                    return;

                for (int i = 0; i < skillList.Count; i++)
                {
                    if (userDragonData.PassiveSkills.Count > i && userDragonData.PassiveSkills[i] > 0)
                    {
                        skillList[i].text = SkillPassiveRateData.GetSkillGroupName(userDragonData.PassiveSkills[i]);
                    }
                }

                skill1Text.alignment = TextAnchor.MiddleCenter;
                skill2Text.alignment = TextAnchor.MiddleCenter;
            }
        }
    }
}
