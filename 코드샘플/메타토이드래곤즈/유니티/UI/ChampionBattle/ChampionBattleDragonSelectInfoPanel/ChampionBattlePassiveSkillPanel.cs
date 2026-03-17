using Google.Impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class ChampionBattlePassiveSkillPanel : ChampionBattleDragonSettingSubPanel
    {
        [Header("skill 1")]
        [SerializeField] GameObject skill1Node;
        [SerializeField] GameObject skill1SkillLayer;
        [SerializeField] Text skill1Title;
        [SerializeField] Text skill1Text;

        [Header("skill 2")]
        [SerializeField] GameObject skill2Node;
        [SerializeField] GameObject skill2SkillLayer;
        [SerializeField] Text skill2Title;
        [SerializeField] Text skill2Text;

        [Space(10)]
        [SerializeField] Button skillGetBtn;

        [SerializeField]
        GameObject[] PassiveSkillNodeList = null;

        [SerializeField]
        ChampionBattleDragonSettingLayer parentLayer = null;

        private SkillPassiveData option_1 = null;
        private SkillPassiveData option_2 = null;
        private List<int> optionList = null;


        UserDragon dragonData = null;
        int currentSlot = 0;
        int minSkillGetLv = 0;
        int skillSlotMax = 0;
        public override void Init()
        {
            base.Init();

            skill1Text.text = "";
            skill2Text.text = "";

            RefreshUI();
        }

        void RefreshUI()
        {
            skill1SkillLayer.SetActive(true);
            skill2SkillLayer.SetActive(true);

            dragonData = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;

            skill1Title.text = StringData.GetStringByStrKey("스킬1");
            skill2Title.text = StringData.GetStringByStrKey("스킬2");
            skill1Text.text = StringData.GetStringByStrKey("스킬미획득");
            skill2Text.text = StringData.GetStringByStrKey("스킬미획득");

            //패시브스킬 장착되어 있음.
            if (dragonData.PassiveSkills.Count > 0)
            {
                option_1 = SkillPassiveData.Get(dragonData.PassiveSkills[0]);
                if (option_1 != null)
                {
                    skill1Title.text = SkillPassiveRateData.GetSkillGroupName(dragonData.PassiveSkills[0]);
                    skill1Text.text = option_1.STRING;
                }

                option_2 = SkillPassiveData.Get(dragonData.PassiveSkills[1]);
                if (option_1 != null)
                {
                    skill2Title.text = SkillPassiveRateData.GetSkillGroupName(dragonData.PassiveSkills[1]);
                    skill2Text.text = option_2.STRING;
                }
            }

            skill1Text.alignment = TextAnchor.MiddleCenter;
            skill2Text.alignment = TextAnchor.MiddleCenter;
        }

        public void OnClickGetPassiveSkill()
        {
            if (currentSlot > 0)
            {
                var popup = PopupManager.OpenPopup<PassiveSkillPopup>(new PassiveSkillPopupData(dragonTag, skillSlotMax));
                popup.SetSkillGetCallBack(
                    () =>
                    {
                        Init();
                        successCallback();
                    });
            }
            else
                ToastManager.On(StringData.GetStringFormatByStrKey("스킬슬롯잠김", minSkillGetLv));
        }
        public void OnClicKReqPassiveSkill()
        {
            if (optionList == null || optionList.Count <= 0) return;

            var dragonInfo = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
            if (dragonInfo == null)
            {
                Debug.Log("dragonData is null");
                return;
            }
            dragonInfo.AddPassive(dragonTag, optionList, (data) => {
                RefreshUI();
                parentLayer.OnClickHideSubPopup(5);

            });
        }

        public void OnClickSetPassiveSkill()
        {
            var popup = PopupManager.OpenPopup<OptionSelectPopup>();
            popup.SetData(OptionSelectPopup.OptionType.SkillPassive, dragonTag);
            popup.SetCallback(selectComplete);
        }
        public void selectComplete(List<int> _options)
        {
            var id_1 = _options[0];
            option_1 = SkillPassiveData.Get(id_1);
            skill1Text.text = option_1.STRING;
            skill1Title.text = SkillPassiveRateData.GetSkillGroupName(id_1);

            var id_2 = _options[1];
            option_2 = SkillPassiveData.Get(id_2);
            skill2Text.text = option_2.STRING;
            skill2Title.text = SkillPassiveRateData.GetSkillGroupName(id_2);

            optionList = _options;
        }
    }
}