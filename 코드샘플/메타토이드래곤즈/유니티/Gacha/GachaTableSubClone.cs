using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GachaTableSubClone : MonoBehaviour
    {
        [SerializeField] Image portraitBG = null;
        [SerializeField] Image portraitIcon = null;
        [SerializeField] protected Text nameText = null;

        [SerializeField] GameObject jobInfoLayerObject = null;
        [SerializeField] Image jobIcon = null;
        [SerializeField] Image elemIcon = null;

        [SerializeField] protected Text probabilityText = null;

        [SerializeField] Sprite[] jobIconSprite = null;

        [SerializeField] ItemFrame item = null;

        public GachaRateData currentRateData { get; private set; } = null;
        public ItemGroupData currentItemGroupData { get; protected set; } = null;

        public SkillPassiveRateData currentPassiveSkillRateData { get; protected set; } = null;

        public void InitSubClone(GachaRateData rateData)
        {
            currentRateData = rateData;
            item.gameObject.SetActive(false);
            portraitIcon.gameObject.SetActive(true);
            if (currentRateData.reward_type == "CHAR")  // 드래곤
            {
                SetDragonInfoData();
            }
            else if (currentRateData.reward_type == "PET")  // 펫
            {
                SetPetInfoData();
            }
        }
        public virtual void InitSubClone(ItemGroupData rateData)
        {
            currentItemGroupData = rateData;
            var reward = currentItemGroupData.Reward;

            portraitIcon.sprite = reward.ICON;
            portraitBG.sprite = SBFunc.GetGradeBGSprite(0);
            nameText.text = reward.GetName() + " X " + reward.Amount.ToString();

            item.SetFrameItem(reward);
            jobInfoLayerObject.SetActive(false);
            item.gameObject.SetActive(true);
            portraitIcon.gameObject.SetActive(false);
        }
        public virtual void InitSubClone(SkillPassiveRateData rateData, string titleText)
        {
            currentPassiveSkillRateData = rateData;
            jobInfoLayerObject.SetActive(false);
            item.gameObject.SetActive(false);
            portraitIcon.gameObject.SetActive(false);
            var passiveData = SkillPassiveData.Get(currentPassiveSkillRateData.RESULT_GROUP);
            var stringFormat = passiveData.EFFECT_VALUE == eStatusValueType.PERCENT ? "{0} <color=#03B027>+{1}%</color>" : "{0} <color=#03B027>+{1}</color>";
            var valueByPassiveType = passiveData.PASSIVE_EFFECT == eSkillPassiveEffect.SILENCE ? passiveData.MAX_TIME : passiveData.VALUE;

            nameText.text = string.Format(stringFormat, titleText, valueByPassiveType);


            //if (passiveData.EFFECT_VALUE == eStatusValueType.PERCENT)
            //    nameText.text = string.Format("{0} <color=#03B027>+{1}%</color>", titleText, passiveData.VALUE);
            //else
            //    nameText.text = string.Format("{0} <color=#03B027>+{1}</color>", titleText, passiveData.VALUE);
            //probabilityText.text = (currentPassiveSkillRateData.RATE / (float)SBDefine.Million).ToString("P2");
        }

        public virtual void UpdateMaxRate(float curRate, int subTotal)
        {
            if (probabilityText != null)
            {
                if (currentRateData != null)
                {
                    var rate = ((float)currentRateData.weight / subTotal) * curRate;
                    probabilityText.text = rate.ToString("P4");
                }
                else if(currentItemGroupData != null)
                {
                    var rate = ((float)currentItemGroupData.ITEM_RATE / subTotal) * curRate;
                    probabilityText.text = rate.ToString("P4");
                }
                else if (currentPassiveSkillRateData != null)
                {
                    var rate = ((float)currentPassiveSkillRateData.RATE / subTotal) * curRate;
                    rate *= SBDefine.MILLION;
                    rate = Mathf.Floor(rate * SBDefine.THOUSAND) / SBDefine.THOUSAND; // 소수점 4째짜리이하 버림
                    rate = Mathf.Ceil(rate * SBDefine.BASE_FLOAT) / SBDefine.BASE_FLOAT; // 소수점 3째짜리 올림
                    rate /= SBDefine.MILLION;
                    probabilityText.text = rate.ToString("P2");
                }
            }
        }

        void SetDragonInfoData()
        {
            CharBaseData charData = CharBaseData.Get(currentRateData.result_id);
            if (charData == null) return;

            portraitIcon.sprite = charData.GetThumbnail();
            portraitBG.sprite = SBFunc.GetGradeBGSprite(charData.GRADE);

            nameText.text = StringData.GetStringByStrKey(charData._NAME);

            jobIcon.sprite = charData.GetClassIcon();
            elemIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, SBFunc.StrBuilder("type_", SBDefine.ConvertToElementString(charData.ELEMENT)));

            jobInfoLayerObject.SetActive(true);
        }

        void SetPetInfoData()
        {
            PetBaseData petData = PetBaseData.Get(currentRateData.result_id);
            if (petData == null) return;

            portraitIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.PetIconPath, petData.THUMBNAIL);
            portraitBG.sprite = SBFunc.GetGradeBGSprite(petData.GRADE);

            nameText.text = StringData.GetStringByStrKey(petData._NAME);

            jobInfoLayerObject.SetActive(false);
        }

        public virtual void SetActive(bool enable)
        {
            gameObject.SetActive(enable);
        }
    }
}