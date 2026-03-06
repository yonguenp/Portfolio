using Google.Impl;
using Newtonsoft.Json.Linq;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class PetStatInfo : MonoBehaviour
    {
        protected const int MAX_OPTION_SLOT_COUNT = 4;

        [SerializeField]
        GameObject petExistNode = null;//현재 클릭한 펫이 없으면 꺼야하는 노드

        [SerializeField]
        GameObject NoneSelectedPetUI = null;

        [SerializeField]
        PetListPanel petList = null;

        [Space(5)]
        [Header("PetBaseInfo")]
        [SerializeField]
        protected UIPetSpine petSpine = null;
        [SerializeField]
        protected Text petNameText = null;
        [SerializeField]
        protected Text petLevelText = null;
        [SerializeField]
        protected Text petReinforceText = null;
        [SerializeField]
        protected List<GameObject> petEffectPrefabList = new List<GameObject>();
        [SerializeField]
        protected GameObject effectTargetParent = null;

        [Header("PetBaseStat")]
        [SerializeField]
        protected GameObject[] LockStatObjs = null;
        [SerializeField]
        protected GameObject[] StatObjs = null;
        [SerializeField]
        protected Text[] baseStatTexts = null;
        [SerializeField]
        protected Text[] baseStatValueTexts = null;

        [Header("pet elementBuff")]
        [SerializeField] protected GameObject buffFrame = null;//동일 속성 아닐 때
        [SerializeField] protected GameObject buffOFFNode = null;//동일 속성 아닐 때
        [SerializeField] protected GameObject buffOnNode = null;//동일 속성 일 때
        [SerializeField] protected Text buffAmountLabel = null;
        [SerializeField] protected Color defaultColor = new Color();//미등록 시
        [SerializeField] protected Color buffColor = new Color();//버프 발동 시

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

        [Header("Belong Dragon")]
        [SerializeField]
        protected GameObject EquipDragonNode = null;
        [SerializeField]
        protected GameObject[] petRankImgs = null;
        [SerializeField]
        protected GameObject[] petElemImgs = null;
        [SerializeField]
        protected DragonPortraitFrame dragonFrame = null;

        [Header("Button")]
        [SerializeField]
        Button mergeBtn = null;
        [SerializeField]
        Button decomposeBtn = null;
        [SerializeField]
        Button reinforceBtn = null;
        [SerializeField]
        protected Button levelupBtn = null;
        [SerializeField]
        protected Button equipBtn = null;
        [SerializeField]
        protected GameObject unequipBtnObj = null;
        [SerializeField]
        Button petSubOptionReroll = null;

        [SerializeField]
        Toggle lockToggle = null;
        [SerializeField]
        GameObject unlockObj = null;

        [SerializeField]
        Color disabledColor = new Color();


        private bool isNetworkState = false;

        public virtual UserPetData GetPetInfo()
        {
            return User.Instance.PetData;
        }
        public virtual UserDragonData GetDragonInfo()
        {
            return User.Instance.DragonData;
        }

        public virtual int CurPopupPetTag
        {
            get
            {
                return PopupManager.GetPopup<DragonManagePopup>().CurPetTag;
            }
            set
            {
                PopupManager.GetPopup<DragonManagePopup>().CurPetTag = value;
            }
        }

        public virtual int CurPopupDragonTag
        {
            get
            {
                return PopupManager.GetPopup<DragonManagePopup>().CurDragonTag;
            }
            set
            {
                PopupManager.GetPopup<DragonManagePopup>().CurDragonTag = value;
            }
        }

        protected virtual void RefreshCurrentPetData()
        {
            isNetworkState = false;
            ClearInfoObj();
            if (CurPopupPetTag != 0)//펫 태그값
            {
                var petTagValue = CurPopupPetTag;
                var petData = GetPetInfo();
                if (petData == null)
                {
                    Debug.Log("user's pet Data is null");
                    return;
                }
                CurPopupPetTag = petTagValue;
                var userPetInfo = petData.GetPet(CurPopupPetTag);
                int petReinforce = userPetInfo.Reinforce;
                var petLv = userPetInfo.Level;
                if (userPetInfo == null)
                {
                    Debug.Log("user pet is null");
                    return;
                }

                if (userPetInfo.Stats != null)
                {
                    var statCount = userPetInfo.Stats.Count;

                    for (int i = 0; i < statCount; ++i)
                    {
                        var stat = userPetInfo.Stats[i];
                        if (stat == null)
                            continue;

                        string statKey = stat.Key.ToString();
                        PetStatData data = PetStatData.Get(statKey);
                        bool isPercent = data.VALUE_TYPE == eStatusValueType.PERCENT;

                        var statTypeData = StatTypeData.Get(SBFunc.ConvertStatusType(data.STAT_TYPE));
                        var optionString = isPercent ? statTypeData.PERCENT_DESC : statTypeData.VALUE_DESC;
                        SetBaseStatInfo(i, optionString, PetStatData.GetStatValue(statKey, petLv, petReinforce, stat.IsStatus1), isPercent);  // 동진 바꿀 것 - data.STAT_TYPE 을 _DESC 를 사용해서 스트링 테이블 값 가져오도록
                    }
                }

                var petSubStat = userPetInfo.SubOptionList;
                var maxReinLimit = GameConfigTable.GetPetReinforceLevelMax(userPetInfo.Grade());
                int subOptCount = 0;
                foreach (int limit in GetPetInfo().NewOptNeedReinforceVal)
                {
                    if (limit <= maxReinLimit)
                    {
                        ++subOptCount;
                    }
                }


                if (petSubOptionReroll != null)
                {
                    petSubOptionReroll.gameObject.SetActive(true);                    
                }

                for (var i = 0; i < optionObjs.Length; i++)
                {
                    optionEmptyTexts[i].gameObject.SetActive(false);
                    optionLockObjs[i].SetActive(true);
                }

                if (petSubStat != null)
                {
                    if (petSubOptionReroll != null)
                        petSubOptionReroll.interactable = petSubStat.Count > GameConfigTable.GetConfigIntValue("min_need_reroll_pet_option_count", 0);

                    for (var i = 0; i < subOptCount; i++)
                    {
                        if (petSubStat.Count <= i)
                        {
                            SetOptionText(i, "", 0, true);
                            continue;
                        }
                        int statKey = petSubStat[i].Key;
                        SubOptionData data = SubOptionData.Get(statKey);
                        bool isPercent = data.VALUE_TYPE == "PERCENT";
                        bool isLock = (petReinforce < GetPetInfo().NewOptNeedReinforceVal[i]);
                        var value = petSubStat[i].Value;
                        SetOptionText(i, StatTypeData.GetDescStringByStatType(data.STAT_TYPE, isPercent), value, isLock, isPercent);
                    }
                }

                bool btnState = userPetInfo.Grade() != (int)eDragonGrade.Legend && userPetInfo.LinkDragonTag < 0 && GameConfigTable.GetPetLevelMax(userPetInfo.Grade()) <= userPetInfo.Level && GameConfigTable.GetPetReinforceLevelMax(userPetInfo.Grade()) <= userPetInfo.Reinforce;
                
                mergeBtn.SetButtonSpriteState(btnState);
                decomposeBtn.SetButtonSpriteState(userPetInfo.LinkDragonTag < 0);

                petSpine.gameObject.SetActive(true);
                petSpine.Init();
                if (userPetInfo.GetPetDesignData().SKIN != "NONE")
                {
                    petSpine.SetSkin(userPetInfo.GetPetDesignData().SKIN);
                }
                //petFrame.gameObject.SetActive(true);
                //petFrame.SetPetPortraitFrame(userPetInfo);
                var petGrade = userPetInfo.Grade();
                var maxLv = GameConfigTable.GetPetLevelMax(petGrade);
                var maxReinforceLv = PetReinforceData.GetMaxReinforceStep(petGrade);
                petLevelText.text = string.Format("<color=#FAC81E>Lv. {0}</color> <color=#ABABAB>/ {1}</color>", userPetInfo.Level, maxLv);
                petReinforceText.text = StringData.GetStringByIndex(100001128) + string.Format(" : +{0}", userPetInfo.Reinforce);
                levelupBtn.SetButtonSpriteState(userPetInfo.Level < maxLv);
                reinforceBtn.SetButtonSpriteState(userPetInfo.Reinforce < maxReinforceLv);
                equipBtn.SetButtonSpriteState(true);
                petNameText.text = userPetInfo.Name();
                if (userPetInfo.LinkDragonTag > 0)
                {
                    unequipBtnObj.gameObject.SetActive(true);
                    SetBelongedDragonPortrait(userPetInfo);
                }
                else
                {
                    unequipBtnObj.gameObject.SetActive(false);
                    EquipDragonNode.SetActive(false);
                }
                petRankImgs[userPetInfo.Grade() - 1].SetActive(true);
                petElemImgs[userPetInfo.Element() - 1].SetActive(true);

                Set_SetOption(userPetInfo);

                SBFunc.RemoveAllChildrens(effectTargetParent.transform);
                var effectClone = Instantiate(petEffectPrefabList[userPetInfo.Grade() - 1], effectTargetParent.transform);//펫 등급 이펙트
                effectClone.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                lockToggle.gameObject.SetActive(true);
                bool lockState = User.Instance.Lock.IsLockPet(CurPopupPetTag);
                lockToggle.isOn = lockState;
                unlockObj.SetActive(!lockState);
            }
            else//빈 값일 때 처리 (초기화)
            {
                SetEmptySlot();
            }
        }

        public void OnLockToggle()
        {
            lockToggle.isOn = !lockToggle.isOn;
            if (lockToggle.isOn)
            {
                User.Instance.Lock.SetPetLock(CurPopupPetTag, () => {
                    //RefreshCurrentPetData();
                    unlockObj.SetActive(false);
                });

            }
            else
            {
                User.Instance.Lock.SetPetUnlock(CurPopupPetTag, () => {
                    //RefreshCurrentPetData();
                    unlockObj.SetActive(true);
                });

            }


        }

        void SetEmptySlot()
        {
            petReinforceText.text = petLevelText.text = petNameText.text = "";
            petSpine.gameObject.SetActive(false);
            lockToggle.gameObject.SetActive(false);
            unlockObj.gameObject.SetActive(false);
            SBFunc.RemoveAllChildrens(effectTargetParent.transform);

            unequipBtnObj.gameObject.SetActive(false);
            EquipDragonNode.SetActive(false);

            if (petSubOptionReroll != null)
                petSubOptionReroll.gameObject.SetActive(false);
        }

        protected void ClearInfoObj()
        {
            foreach (var obj in LockStatObjs)
            {
                if (obj == null) return;
                obj.SetActive(true);
            }
            foreach (var obj in StatObjs)
            {
                if (obj == null) return;
                obj.SetActive(false);
            }

            for (int i = 0; i < MAX_OPTION_SLOT_COUNT; i++)
            {
                optionObjs[i].gameObject.SetActive(true);
                optionTexts[i].gameObject.SetActive(false);
                optionEmptyTexts[i].gameObject.SetActive(false);
                optionValueTexts[i].gameObject.SetActive(false);
                optionLockObjs[i].gameObject.SetActive(true);
            }

            foreach (var obj in petRankImgs)
            {
                if (obj == null) return;
                obj.gameObject.SetActive(false);
            }
            foreach (var obj in petElemImgs)
            {
                if (obj == null) return;
                obj.gameObject.SetActive(false);
            }
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

        protected void SetBaseStatInfo(int index, string optionString, float optionValue, bool isOptionValuePercent = false)
        {
            if (StatObjs.Length > index && index >= 0)
            {
                StatObjs[index].SetActive(true);
                LockStatObjs[index].SetActive(false);
                baseStatTexts[index].text = optionString;
                string optionValueString = "+" + optionValue.ToString("F2");
                if (isOptionValuePercent) optionValueString += "%";
                baseStatValueTexts[index].text = optionValueString;

            }
        }

        protected void SetBelongedDragonPortrait(UserPet pet)
        {
            var isBelonged = pet.LinkDragonTag > 0;

            if (!isBelonged)
            {
                EquipDragonNode.SetActive(false);
            }
            else
            {
                var dragonData = GetDragonInfo().GetDragon(pet.LinkDragonTag);
                if (dragonData == null)
                {
                    EquipDragonNode.SetActive(false);
                    return;
                }
                EquipDragonNode.SetActive(true);


                dragonFrame.SetDragonPortraitFrame(dragonData);

            }
        }

        protected void Set_SetOption(UserPet _petInfo)
        {
            eStatusType element_buffType = PetBaseData.Get(_petInfo.ID).ELEMENT_BUFF_TYPE;
            int petGrade = _petInfo.Grade();
            int petReinforce = _petInfo.Reinforce;
            int belongTag = _petInfo.LinkDragonTag;
            bool isSameElement = false;
            if (belongTag <= 0)
            {
                SetBuffDefault();
            }
            else
            {
                var dragonData = GetDragonInfo().GetDragon(belongTag);
                var currentDragonElement = dragonData.Element();
                isSameElement = _petInfo.Element() == currentDragonElement;
                if (isSameElement)
                    SetElementBuff();
                else
                    SetDimmedBuff();
            }

            SetBuffAmountLabel(isSameElement, element_buffType, petGrade, petReinforce);//라벨 세팅
        }

        void SetBuffDefault()//미장착 상태일 때는 프레임 켜기
        {
            buffFrame.SetActive(true);
            buffOFFNode.SetActive(false);
            buffOnNode.SetActive(false);
        }

        void SetDimmedBuff()
        {
            buffFrame.SetActive(true);
            buffOFFNode.SetActive(true);
            buffOnNode.SetActive(false);
        }
        void SetElementBuff()
        {
            buffFrame.SetActive(false);
            buffOFFNode.SetActive(false);
            buffOnNode.SetActive(true);
        }

        void SetBuffAmountLabel(bool isSameElement, eStatusType element_buffType, int petGrade, int petReinforce)
        {
            var data = PetReinforceData.GetDataByGradeAndStep(petGrade, petReinforce);
            string str = "0";
            int strIndex = 0;
            bool isData = data != null;
            if (isData)
                str = data.ELEMENT_BUFF.ToString() + "%";

            switch (element_buffType)
            {
                case eStatusType.ATK:
                    strIndex = 100001332;
                    break;
                case eStatusType.DEF:
                    strIndex = 100001334;
                    break;
                case eStatusType.HP:
                    strIndex = 100001336;
                    break;
            }

            buffAmountLabel.color = isData && isSameElement ? buffColor : defaultColor;
            buffAmountLabel.text = isData ? StringData.GetStringFormatByIndex(strIndex, str) : "--";
        }

        public virtual void RequestEquipPet()
        {
            var currentDragonData = GetDragonInfo().GetDragon(CurPopupDragonTag);
            if (currentDragonData == null)
            {
                return;
            }
            if (CurPopupPetTag <= 0)
            {
                ToastManager.On(100001844);//펫을 선택해주세요.
                return;
            }

            var currentBelongingPet = currentDragonData.Pet;
            var isPrevOtherPet = currentBelongingPet > 0;

            if (isPrevOtherPet)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100001764), StringData.GetStringByIndex(100001357),
                    () => {
                        //ok
                        SendMsgEquipPet(true, currentBelongingPet);
                    },
                    () => {
                        //cancle
                    },
                    () => {
                        //x
                    }
                );
            }
            else
            {
                SendMsgEquipPet(false);
            }
        }

        public virtual void RequestUnEquipPet()
        {
            var petData = GetPetInfo().GetPet(CurPopupPetTag);
            if (petData == null)
            {
                return;
            }

            var belongedTag = petData.LinkDragonTag;
            if (belongedTag <= 0)
            {
                return;
            }

            if (isNetworkState)
            {
                return;
            }

            var param = new WWWForm();
            param.AddField("did", belongedTag);

            Debug.Log("unequip pet : " + CurPopupPetTag + "  Target Dragon : " + belongedTag);

            isNetworkState = true;
            NetworkManager.Send("pet/unequip", param, (NetworkManager.SuccessCallback)((jsonData) =>
            {
                isNetworkState = false;
                if (jsonData.ContainsKey("rs") && (eApiResCode)jsonData["rs"].Value<int>() == eApiResCode.OK)
                {

                    var curPetTag = jsonData["ptag"].Value<int>();
                    var petData = GetPetInfo().GetPet(curPetTag);
                    if (petData != null)
                    {
                        petData.SetLinkDragonTag(-1);
                    }

                    this.CurPopupPetTag = this.CurPopupPetTag;
                    RefreshCurrentPetData();
                    petList.ViewDirty = true;
                    petList.Init();
                }
                else
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000621), StringData.GetStringByIndex(100000614));
                }
            }), (string arg) =>
            {
                isNetworkState = false;
            });
        }
        void SendMsgEquipPet(bool isPrevOtherPet, int currentBelongingPet = -1)
        {
            if (isNetworkState)
            {
                return;
            }
            var param = new WWWForm();
            param.AddField("did", CurPopupDragonTag);
            param.AddField("ptag", CurPopupPetTag);

            Debug.Log("equip pet : " + CurPopupPetTag + "  Target Dragon : " + CurPopupDragonTag);
            isNetworkState = true;
            NetworkManager.Send("pet/equip", param, (NetworkManager.SuccessCallback)((jsonData) =>
            {
                isNetworkState = false;
                //push로 dragon_update로 펫 세팅 해줌
                if (isPrevOtherPet)//이전에 장착한 드래곤이 있다면
                {
                    var petData = GetPetInfo().GetPet(currentBelongingPet);
                    if (petData != null)
                    {
                        petData.SetLinkDragonTag(-1);
                    }
                }

                if (jsonData.ContainsKey("rs") && (eApiResCode)jsonData["rs"].Value<int>() == eApiResCode.OK)
                {
                    this.CurPopupPetTag = this.CurPopupPetTag;
                    RefreshCurrentPetData();
                    petList.ViewDirty = true;
                    petList.Init();
                }
                else
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000621), StringData.GetStringByIndex(100000614));
                }
            }), (string arg) =>
            {
                isNetworkState = false;
            });
        }
         
        public void Init()
        {
            InitAllButtons();
            NoneSelectedPetUI.SetActive(CurPopupPetTag <= 0);
            petExistNode.SetActive(CurPopupPetTag > 0);
            EquipDragonNode.SetActive(CurPopupPetTag > 0);
            RefreshCurrentPetData();
        }

        void InitAllButtons()
        {
            if (mergeBtn != null)
                mergeBtn.SetButtonSpriteState(false);
            if (reinforceBtn != null)
                reinforceBtn.SetButtonSpriteState(false);
            if (decomposeBtn != null)
                decomposeBtn.SetButtonSpriteState(false);
            if (levelupBtn != null)
                levelupBtn.SetButtonSpriteState(false);
            if (equipBtn != null)
                equipBtn.SetButtonSpriteState(false);
        }


        public void OnSubOptionReroll()
        {
            var petData = GetPetInfo();
            if (petData == null)
            {
                Debug.Log("user's pet Data is null");
                return;
            }
            var userPetInfo = petData.GetPet(CurPopupPetTag);

            PetSubOptionRerollPopup.Open(userPetInfo, RefreshCurrentPetData);
        }
    }

}