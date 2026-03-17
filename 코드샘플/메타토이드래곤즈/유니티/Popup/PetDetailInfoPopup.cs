using System;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    [Serializable]
    public class PetStatOptions
    {
        [SerializeField]
        Text emptyStatText = null;
        [SerializeField]
        Text emptyOptionText = null;
        [SerializeField]
        Text petNameText = null;

        [SerializeField]
        PetPortraitFrame petPortraitFrame = null;
        [SerializeField]
        Text[] petStatTexts = null;
        [SerializeField]
        Text[] petStatValueText = null;
        [SerializeField]
        Text[] petOptionTexts = null;
        [SerializeField]
        Text[] petOptionValueText = null;
        [SerializeField]
        GameObject[] optionLockObjs = null;


        public void SetPetInfo(UserPet userPet)
        {
            if (userPet == null || userPet.Tag <= 0)
            {
                OffPetFrame();
                return;
            }
            petPortraitFrame.gameObject.SetActive(true);
            petPortraitFrame.SetPetPortraitFrame(userPet);

            petNameText.text = userPet.Name();

            for (int i = 0; i < petStatTexts.Length; i++)
            {
                var text = petStatTexts[i];
                text.gameObject.SetActive(true);
                text.alignment = TextAnchor.MiddleRight;
                text.text = "---";

                petStatValueText[i].gameObject.SetActive(true);
                petStatValueText[i].text = "";
            }

            var petSubStat = userPet.SubOptionList;
            var petReinforce = userPet.Reinforce;
            if (userPet.Stats != null && userPet.Stats.Count != 0)
            {
                emptyStatText.gameObject.SetActive(false);
                var statCount = userPet.Stats.Count;

                for (int i = 0; i < statCount; ++i)
                {
                    var stat = userPet.Stats[i];
                    if (stat == null)
                        continue;

                    string statKey = stat.Key.ToString();
                    PetStatData data = PetStatData.Get(statKey);
                    if (data == null)
                        continue;
                    bool isPercent = data.VALUE_TYPE == eStatusValueType.PERCENT;

                    SetBaseStatInfo(i, data.STAT_TYPE, PetStatData.GetStatValue(statKey, userPet.Level, petReinforce, stat.IsStatus1), isPercent);
                }
            }

            for (int i = 0; i < petOptionTexts.Length; i++)
            {
                var text = petOptionTexts[i];
                text.gameObject.SetActive(true);
                text.alignment = TextAnchor.MiddleRight;
                text.text = "---";

                petOptionValueText[i].gameObject.SetActive(true);
                petOptionValueText[i].text = "";
            }

            if (petSubStat != null)
            {
                emptyOptionText.gameObject.SetActive(false);
                for (var i = 0; i < petSubStat.Count; i++)
                {
                    int statKey = petSubStat[i].Key;
                    var data = SubOptionData.Get(statKey);
                    var value = petSubStat[i].Value;
                    bool isPercent = data.VALUE_TYPE == "PERCENT";
                    SetOptionText(i, data.STAT_TYPE, value, (petReinforce < User.Instance.PetData.NewOptNeedReinforceVal[i]), isPercent);  // 동진 바꿀 것 - data.STAT_TYPE 을 _DESC 를 사용해서 스트링 테이블 값 가져오도록
                }
            }
        }

        public void CustomPetInfo(UserPet userPet)
        {
            petPortraitFrame.gameObject.SetActive(false);
            petNameText.gameObject.SetActive(false);

            if (userPet == null || userPet.Tag <= 0)
            {
                OffPetFrame();
                return;
            }

            for (int i = 0; i < petStatTexts.Length; i++)
            {
                var text = petStatTexts[i];
                text.gameObject.SetActive(true);
                text.alignment = TextAnchor.MiddleCenter;
                text.text = "---";
                text.GetComponent<RectTransform>().sizeDelta = new Vector2(895, 50);
                petStatValueText[i].gameObject.SetActive(true);
                petStatValueText[i].text = "";
            }

            var petSubStat = userPet.SubOptionList;
            var petReinforce = userPet.Reinforce;
            if (userPet.Stats != null && userPet.Stats.Count != 0)
            {
                emptyStatText.gameObject.SetActive(false);
                var statCount = userPet.Stats.Count;

                for (int i = 0; i < statCount; ++i)
                {
                    var stat = userPet.Stats[i];
                    if (stat == null)
                        continue;

                    string statKey = stat.Key.ToString();
                    PetStatData data = PetStatData.Get(statKey);
                    if (data == null)
                        continue;
                    bool isPercent = data.VALUE_TYPE == eStatusValueType.PERCENT;

                    SetBaseStatInfo(i, data.STAT_TYPE, PetStatData.GetStatValue(statKey, userPet.Level, petReinforce, stat.IsStatus1), isPercent,true);
                }
            }

            for (int i = 0; i < petOptionTexts.Length; i++)
            {
                var text = petOptionTexts[i];
                text.gameObject.SetActive(true);
                text.alignment = TextAnchor.MiddleCenter;
                text.text = "---";
                text.GetComponent<RectTransform>().sizeDelta = new Vector2(895, 50);

                petOptionValueText[i].gameObject.SetActive(true);
                petOptionValueText[i].text = "";
            }

            if (petSubStat != null)
            {
                emptyOptionText.gameObject.SetActive(false);
                for (var i = 0; i < petSubStat.Count; i++)
                {
                    int statKey = petSubStat[i].Key;
                    var data = SubOptionData.Get(statKey);
                    var value = petSubStat[i].Value;
                    bool isPercent = data.VALUE_TYPE == "PERCENT";
                    SetOptionText(i, data.STAT_TYPE, value, (petReinforce < User.Instance.PetData.NewOptNeedReinforceVal[i]), isPercent, true);  // 동진 바꿀 것 - data.STAT_TYPE 을 _DESC 를 사용해서 스트링 테이블 값 가져오도록
                }
            }
        }


        public void OffPetFrame()
        {
            petPortraitFrame.gameObject.SetActive(false);
            foreach (var text in petStatTexts)
            {
                text.gameObject.SetActive(false);
            }
            foreach (var text in petOptionTexts)
            {
                text.gameObject.SetActive(false);
            }

            emptyStatText.gameObject.SetActive(true);
            emptyOptionText.gameObject.SetActive(true);
        }

        void SetBaseStatInfo(int index, string statType, float optionValue, bool isOptionValuePercent = false, bool cutomSize = false)
        {
            if (petStatTexts.Length > index && index >= 0)
            {
                string optionValueString = "+" + optionValue.ToString("F2");
                if (isOptionValuePercent) optionValueString += "%";
                petStatTexts[index].text = StatTypeData.GetDescStringByStatType(statType, isOptionValuePercent);
                petStatValueText[index].text = optionValueString;
                petStatTexts[index].alignment = TextAnchor.MiddleLeft;

                if (cutomSize)
                    petStatTexts[index].GetComponent<RectTransform>().sizeDelta = new Vector2(560, 50);
            }
        }

        void SetOptionText(int index, string statType, float optionValue, bool isLock, bool isOptionValuePercent = false, bool cutomSize = false)
        {
            if (petOptionTexts.Length > index && index >= 0)
            {
                string optionValueString = "+" + optionValue.ToString("F2");
                if (isOptionValuePercent) optionValueString += "%";
                petOptionTexts[index].text = isLock ? "" : StatTypeData.GetDescStringByStatType(statType, isOptionValuePercent);
                petOptionValueText[index].text = isLock ? "" : optionValueString;
                petOptionTexts[index].alignment = TextAnchor.MiddleLeft;

                if (cutomSize)
                    petOptionTexts[index].GetComponent<RectTransform>().sizeDelta = new Vector2(560, 50);
                //optionLockObjs[index].SetActive(isLock);
            }
        }
    }

    public class PetDetailInfoPopup : Popup<PetDetailInfoPopupData>
    {
        [SerializeField] GameObject leftPetInfoObject = null;
        [SerializeField] GameObject rightPetInfoObject = null;
        [SerializeField] PetStatOptions leftPetStatOptions = null;
        [SerializeField] PetStatOptions rightPetStatOptions = null;

        public override void InitUI()
        {
            InitPetDetailInfo();
        }

        void InitPetDetailInfo()
        {
            if (Data == null) return;

            if (Data.leftPetData != null)
            {
                leftPetStatOptions.SetPetInfo(Data.leftPetData);
            }
            
            if (Data.rightPetData != null)
            {
                rightPetStatOptions.SetPetInfo(Data.rightPetData);
            }

            leftPetInfoObject.SetActive(Data.leftPetData != null);
            rightPetInfoObject.SetActive(Data.rightPetData != null);
        }
    }
}