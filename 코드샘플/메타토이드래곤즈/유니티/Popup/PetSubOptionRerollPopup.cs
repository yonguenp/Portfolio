using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetSubOptionRerollPopup : Popup<PopupBase>
    {
        protected const int MAX_OPTION_SLOT_COUNT = 4;
        const string REROLL_CONSTRAINT_GOAL_TIME = "option_reroll_time";//오늘 하루 안보기를 누르면 켜야될 시간을 미리 계산해서 넘김

        UserPet userPetInfo = null;
        Action callback = null;

        [SerializeField]
        protected UIPetSpine petSpine = null;
        [SerializeField]
        protected List<GameObject> petEffectPrefabList = new List<GameObject>();
        [SerializeField]
        protected GameObject effectTargetParent = null;

        [Header("PetOption")]
        [SerializeField]
        protected GameObject[] optionObjs = null;
        [SerializeField]
        protected Text[] optionTexts = null;
        [SerializeField]
        protected Text[] optionEmptyTexts = null;
        [SerializeField]
        protected Text[] optionValueTexts = null;
        [SerializeField]
        protected GameObject[] optionLockObjs = null;
        [SerializeField]
        protected GameObject[] optionEffects = null;
        [SerializeField]
        Text costValue = null;
        [SerializeField]
        Button rerollBtn = null;
        public static PetSubOptionRerollPopup Open(UserPet petData, Action cb)
        {
            PetSubOptionRerollPopup popup = PopupManager.OpenPopup<PetSubOptionRerollPopup>();
            popup.SetData(petData, cb);

            return popup;
        }
        public virtual UserPetData GetPetInfo()
        {
            return User.Instance.PetData;
        }
        protected void ClearInfoObj()
        {
            for (int i = 0; i < MAX_OPTION_SLOT_COUNT; i++)
            {
                optionObjs[i].gameObject.SetActive(true);
                optionTexts[i].gameObject.SetActive(false);
                optionEmptyTexts[i].gameObject.SetActive(false);
                optionValueTexts[i].gameObject.SetActive(false);
                optionLockObjs[i].gameObject.SetActive(true);
            }
        }

        public void SetData(UserPet petData, Action cb)
        {
            SetExitCallback(cb);
            userPetInfo = petData;
            
            RefreshUI();
        }
        public override void InitUI() 
        {
            costValue.text = SBFunc.CommaFromNumber(GameConfigTable.GetConfigIntValue("pet_option_reroll_cost_dia", 150));
            ClearInfoObj();
        }

        void RefreshUI(bool withEffect = false)
        {
            ClearInfoObj();

            petSpine.Init();
            if (userPetInfo.GetPetDesignData().SKIN != "NONE")
            {
                petSpine.SetSkin(userPetInfo.GetPetDesignData().SKIN);
            }

            SBFunc.RemoveAllChildrens(effectTargetParent.transform);
            var effectClone = Instantiate(petEffectPrefabList[userPetInfo.Grade() - 1], effectTargetParent.transform);//펫 등급 이펙트
            effectClone.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            var maxReinLimit = GameConfigTable.GetPetReinforceLevelMax(userPetInfo.Grade());
            int subOptCount = 0;
            foreach (int limit in GetPetInfo().NewOptNeedReinforceVal)
            {
                if (limit <= maxReinLimit)
                {
                    ++subOptCount;
                }
            }

            int petReinforce = userPetInfo.Reinforce;
            List<KeyValuePair<int, float>> petSubStat = userPetInfo.SubOptionList;
            if (petSubStat != null)
            {
                for (var i = 0; i < optionObjs.Length; i++)
                {
                    optionEmptyTexts[i].gameObject.SetActive(false);
                    optionLockObjs[i].SetActive(true);
                }

                for (var i = 0; i < subOptCount; i++)
                {
                    SetOptionText(i, "", 0, true);
                    if (petSubStat.Count <= i)
                    {                        
                        continue;
                    }

                    StartCoroutine(EffectCoroutine(i, petReinforce, petSubStat, withEffect));
                }

            }

            rerollBtn.interactable = User.Instance.GEMSTONE > GameConfigTable.GetConfigIntValue("pet_option_reroll_cost_dia", 150);
        }

        IEnumerator EffectCoroutine(int i, int petReinforce, List<KeyValuePair<int, float>> petSubStat, bool withEffect)
        {
            optionEffects[i].SetActive(false);
            if (withEffect)
            {                
                yield return new WaitForSeconds(SBFunc.Random(0.01f, 0.1f));
                optionEffects[i].SetActive(true);
            }

            int statKey = petSubStat[i].Key;
            SubOptionData data = SubOptionData.Get(statKey);
            bool isPercent = data.VALUE_TYPE == "PERCENT";
            bool isLock = (petReinforce < GetPetInfo().NewOptNeedReinforceVal[i]);
            var value = petSubStat[i].Value;

            SetOptionText(i, StatTypeData.GetDescStringByStatType(data.STAT_TYPE, isPercent), value, isLock, isPercent);
        }

        protected void SetOptionText(int index, string optionString, float optionValue, bool isLock, bool isOptionValuePercent = false)
        {
            if (optionObjs.Length > index && index >= 0)
            {
                optionObjs[index].SetActive(true);
                string optionValueString = "+" + optionValue.ToString("F2");
                if (isOptionValuePercent) optionValueString += "%";
                optionValueTexts[index].text = isLock ? "" : optionValueString;
                optionTexts[index].text = isLock ? "" : optionString;
                optionValueTexts[index].gameObject.SetActive(!isLock);
                optionTexts[index].gameObject.SetActive(!isLock);
                optionEmptyTexts[index].gameObject.SetActive(isLock);
                optionLockObjs[index].SetActive(false);
            }
        }

        public void OnReroll()
        {
            var valueCheck = SBFunc.HasTimeValue(REROLL_CONSTRAINT_GOAL_TIME);
            if (valueCheck)
            {
                ReqReroll();
                return;//하루동안 보이지 않기 on
            }

            var popup = PopupManager.OpenPopup<PetOptionRerollConstraintPopup>();
            popup.setCallback(() =>
            {
                //예 - 토글 상태 (하루동안 보이지않기) 체크는 예 일때만 판단 (기획)
                //쿠키 세팅
                var checkValue = popup.toggle.isOn;
                if (checkValue)
                {
                    SBFunc.SetTimeValue(REROLL_CONSTRAINT_GOAL_TIME);
                }

                ReqReroll();

                popup.ClosePopup();
            }, () =>
            {
                popup.ClosePopup();
            }, () =>
            {
                popup.ClosePopup();
            });
        }

        void ReqReroll()
        {
            WWWForm param = new WWWForm();
            param.AddField("tag", userPetInfo.Tag);

            NetworkManager.Send("pet/subsreroll", param, (jsonData) =>
            {
                RefreshUI(true);
            });
        }
    }
}
