using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetReinforceLayer : SubLayer
    {
        [SerializeField]
        PetTabLayer petTabLayer = null;

        [SerializeField]
        PetListPanel petSubListSlot = null;

        [Space(10)]
        [Header("Detail Info")]
        [SerializeField]
        Button reinforceButton = null;

        [SerializeField]
        Text petNameLabel = null;
        [SerializeField]
        int initPetNameIndex = -1;

        [SerializeField]
        Text petCurrentReinforceLevelLabel = null;
        [SerializeField]
        Text petNextReinforceLevelLabel = null;
        [SerializeField]
        string initPetReinforceLabel = "?";

        [SerializeField]
        Text success_rate_label = null;
        [SerializeField]
        ItemFrame materialNode = null;
        [SerializeField]
        Text itemNameLabel = null;

        [SerializeField]
        Button backBtn = null;

        [SerializeField]
        GameObject addtionOptAlertBox = null;

        [Space(5)]
        [Header("PetUpgradeResultPopup")]
        [SerializeField]
        GameObject upgradeResultObj = null;
        [SerializeField]
        PetPortraitFrame resultPetFrame = null;
        [SerializeField]
        Text beforeReinforceText = null;
        [SerializeField]
        Text afterReinforceText = null;
        [SerializeField]
        Text[] OptionalStatValueTexts = null;
        [SerializeField]
        Text NewOptionValueText = null;

        [Header("Pet info")]
        [SerializeField]
        Text currentElementBuffLabel = null;
        [SerializeField]
        Text nextElementBuffLabel = null;
        [SerializeField]
        Text elementStatBuffLabel = null;
        [SerializeField]
        GameObject[] partOptionNodeList = null;
        [SerializeField]
        GameObject[] petStatNodeList = null;

        [SerializeField]
        GameObject spineNodeParent = null;
        [SerializeField]
        List<GameObject> maxInvisibleObjectList = new List<GameObject>();
        [SerializeField]
        Button maxDisableButton = null;
        [SerializeField]
        Text goldCostLabel = null;


        bool isSufficientMaterial = false;
        SkeletonGraphic spine = null;

        int petTag = -1;
        int petReinforce = -1;
        int petGrade = -1;
        string petNameStr = "";

        protected bool isReinforce = false;
        bool isInitFlag = false;

        private void OnDisable()
        {
            TurnOffAllOpenAnim();
        }
        void OnEnable()
        {
            isInitFlag = false;
        }

        public override void Init()
        {
            SetPetTag();//petinfo 태그값 먼저 받아서 선체크 
            SetDetailUIByTag();//태그값 기준으로 UI 세팅
            petSubListSlot?.Init(ePetPopupState.Reinforce);//petsubList에서 petinfo를 받아서 지워버림
            OnClickUpgradeResultPopupOff();
        }

        void InitPetOptionUI()//전체 잠금 처리
        {
            if (partOptionNodeList != null && partOptionNodeList.Length > 0)
            {
                var listCount = partOptionNodeList.Length;
                for (var i = 0; i < listCount; i++)//각 단계별 부옵 리스트 갱신 (강화 단계로 인한 잠금상태, 단순 미강화 잠금, 부옵 표시)
                {
                    var CheckNode = partOptionNodeList[i];
                    var optionKey = 0;
                    var optionValue = 0;
                    RefreshPetOptionUIByType("none", CheckNode, optionKey, optionValue);
                }
            }
        }
        void RefreshPetOptionUIByType(string checkType, GameObject targetNode, int optionKey = 0, float optionValue = 0)//켜야될 노드 세팅
        {
            var childNodeList = SBFunc.GetChildren(SBFunc.GetChildrensByName(targetNode.transform, new string[] { "child" }));
            if (childNodeList == null || childNodeList.Length <= 0)
            {
                return;
            }

            GameObject visibleNode = null;
            for (var i = 0; i < childNodeList.Length; i++)
            {
                var node = childNodeList[i];
                var isSameName = node.name == checkType;
                if (isSameName)
                {
                    visibleNode = node.gameObject;
                }
                node.gameObject.SetActive(isSameName);
            }

            if (visibleNode != null)//타입에 따라 세부 내용 세팅
            {
                switch (checkType)
                {
                    case "open":
                    {
                        if (optionKey <= 0 || optionValue <= 0)
                        {
                            return;
                        }
                        var data = SubOptionData.Get(optionKey);
                        var dataType = data.STAT_TYPE;
                        var valueType = data.VALUE_TYPE;
                        var typeStr = StatTypeData.GetDescStringByStatType(dataType, valueType == "PERCENT");

                        var LabelNode = visibleNode.GetComponentsInChildren<Text>();
                        if (LabelNode != null)
                        {
                            optionValue = (float)Math.Round((double)optionValue, 2);
                            var str = SBFunc.StrBuilder("+", optionValue);
                            if (valueType == "PERCENT")
                            {
                                str += "%";
                            }

                            LabelNode[0].text = typeStr;
                            LabelNode[1].text = str;
                        }
                    }
                    break;
                }
            }
        }

        int GetMaxReinforceSlotCount(int _petGrade)
        {
            var maxStep = PetReinforceData.GetMaxReinforceStep(_petGrade);//현재 등급 맥강 수치
            var reinforceList = User.Instance.PetData.NewOptNeedReinforceVal.ToList();

            var slotCount = 0;
            foreach(var step in reinforceList)
            {
                if (maxStep >= step)
                    slotCount++;
            }
            return slotCount;
        }

        void RefreshPetOptionUI(int _grade, List<KeyValuePair<int, float>> partOptionList)
        {
            if (partOptionNodeList != null && partOptionNodeList.Length > 0)
            {
                var listCount = partOptionNodeList.Length;
                var partOptionListLength = partOptionList.Count;
                var availableSlot = GetMaxReinforceSlotCount(_grade);//현재 오픈 가능한 최대 슬롯 갯수
                var checkType = "";
                for (var i = 0; i < listCount; i++)//각 단계별 부옵 리스트 갱신 (강화 단계로 인한 잠금상태, 단순 미강화 잠금, 부옵 표시)
                {
                    var CheckNode = partOptionNodeList[i];
                    var optionKey = 0;
                    float optionValue = 0;
                    if (partOptionListLength > 0 && partOptionListLength > i)//부옵이 존재함.
                    {
                        optionKey = partOptionList[i].Key;
                        optionValue = partOptionList[i].Value;
                        checkType = "open";
                    }
                    else
                    {
                        if (availableSlot > i)//단순 미강화 잠금
                        {
                            checkType = "lock";
                        }
                        else//최대 강화 수치(ex. 저급은 옵션 1개만 뚫리고 나머지 2개 슬롯 처리)보다 벗어난 잠금
                        {
                            checkType = "none";
                        }
                    }
                    RefreshPetOptionUIByType(checkType, CheckNode, optionKey, optionValue);
                }
            }

            RefreshPartReinforceButtonUI();
        }
        void CreateSpineNode()
        {
            if (spine == null)
            {
                var spineClonePrefab = ResourceManager.GetResource<GameObject>(eResourcePath.EffectPrefabPath, "pet_reinforce_effect");
                if (spineClonePrefab != null && spineNodeParent != null)
                {
                    var clone = Instantiate(spineClonePrefab, spineNodeParent.transform);

                    spine = clone.GetComponentInChildren<SkeletonGraphic>();
                }
            }
        }
        void PlayUnlockAnimation()
        {
            var checkIndex = -1;
            var stepList = User.Instance.PetData.NewOptNeedReinforceVal.ToList();
            if (petReinforce == stepList[0])
            {
                checkIndex = 0;
            }
            else if (petReinforce == stepList[1])
            {
                checkIndex = 1;
            }
            else if (petReinforce == stepList[2])
            {
                checkIndex = 2;
            }
            else if (petReinforce == stepList[3])
            {
                checkIndex = 3;
            }

            if (checkIndex < 0)
            {
                return;
            }

            if (partOptionNodeList == null || partOptionNodeList.Length <= 0)
            {
                return;
            }

            if (partOptionNodeList.Length <= checkIndex)
            {
                return;
            }

            var checkNode = partOptionNodeList[checkIndex];
            var openAnimNode = SBFunc.GetChildrensByName(checkNode.transform, new string[] { "open_anim" }).gameObject;
            if (openAnimNode == null)
            {
                return;
            }

            openAnimNode.SetActive(true);

            var animationComp = openAnimNode.GetComponentInChildren<Animator>();

            if (animationComp == null)
            {
                openAnimNode.SetActive(false);
                return;
            }

            var controller = animationComp.runtimeAnimatorController;
            if (controller == null)
            {
                openAnimNode.SetActive(false);
                return;
            }

            var animationClips = controller.animationClips;
            if (animationClips.Length <= 0)
            {
                openAnimNode.SetActive(false);
                return;
            }

            var duration = animationClips[0].length;

            var seq = DOTween.Sequence();
            seq.OnStart(() =>
            {
                SoundManager.Instance.PlaySFX("FX_PART_REINFORCE_OPEN_OPTION");
                animationComp.Play("lock_open");
            }).AppendInterval(duration).AppendCallback(() =>
            {
                openAnimNode.SetActive(false);
            });

            seq.Play();
        }

        void TurnOffAllOpenAnim()
        {
            if (partOptionNodeList == null || partOptionNodeList.Length <= 0)
            {
                return;
            }

            for (var i = 0; i < partOptionNodeList.Length; i++)
            {
                var openAnimNode = SBFunc.GetChildrensByName(partOptionNodeList[i].transform, new string[] { "open_anim" }).gameObject;
                openAnimNode.SetActive(false);
            }
        }

        void InitPartReinforceButtonUI()
        {
            if (reinforceButton != null)
            {
                reinforceButton.SetButtonSpriteState(false);
            }
        }

        void SetPetTag()
        {
            petTag = -1;

            if (PopupManager.GetPopup<DragonManagePopup>().CurPetTag != 0)
            {
                petTag = PopupManager.GetPopup<DragonManagePopup>().CurPetTag;
            }
        }

        void SetDetailUIByTag()
        {
            if (isSelectTag())
            {
                RefreshPetDetailUI();
            }
            else
            {
                isInitFlag = true;
                InitPetDetailUI();
                InitReinforceMaterialList();
            }
        }

        bool isSelectTag()
        {
            return petTag > 0;
        }

        void InitPetDetailUI()
        {
            if (petCurrentReinforceLevelLabel != null)
            {
                petCurrentReinforceLevelLabel.text = initPetReinforceLabel;
            }
            if (petNextReinforceLevelLabel != null)
            {
                petNextReinforceLevelLabel.text = initPetReinforceLabel;
            }
            if (petNameLabel != null)
            {
                petNameLabel.text = StringData.GetStringByIndex(initPetNameIndex);
            }
            if (success_rate_label != null)
            {
                success_rate_label.text = "100.00%";
            }
        }

        void InitReinforceMaterialList()
        {
            if (materialNode != null)
            {
                materialNode.gameObject.SetActive(false);   
            }
            if(itemNameLabel != null)
            {
                itemNameLabel.gameObject.SetActive(false);
            }
        }

        void RefreshPetDetailUI()//펫 정보 불러오기 성공 시 데이터 세팅
        {
            InitReinforceMaterialList();
            var petData = User.Instance.PetData.GetPet(petTag);
            if (petData == null)
            {
                InitPetDetailUI();
                return;
            }

            petReinforce = petData.Reinforce;
            petGrade = petData.Grade();
            petNameStr = petData.Name();
            var petOptionList = petData.SubOptionList;

            if (resultPetFrame != null)
            {
                resultPetFrame.gameObject.SetActive(true);
                resultPetFrame.SetPetPortraitFrame(petData, false, false, true);
            }
            SetBuffTypeLabel(PetBaseData.Get(petData.ID).ELEMENT_BUFF_TYPE);
            Set_SetOption(currentElementBuffLabel, petData.Grade(), petReinforce);
            Set_SetOption(nextElementBuffLabel, petData.Grade(), petReinforce + 1);
            RefreshPetDetailLabel();
            RefreshPetReinforceMaterial();
            RefreshPartReinforceButtonUI();
            RefreshPetOptionUI(petGrade, petOptionList);
            RefreshPetStat(petData);
        }
        void Set_SetOption(Text targetText, int petGrade, int petReinforce)
        {
            var data = PetReinforceData.GetDataByGradeAndStep(petGrade, petReinforce);
            bool isData = data != null;
            if (isData)
                targetText.text = SBFunc.StrBuilder("+", data.ELEMENT_BUFF.ToString(), "%");
            else
                targetText.text = petReinforce > 0 ? "MAX" : "--";
        }

        void SetBuffTypeLabel(eStatusType element_buffType)
        {
            if(elementStatBuffLabel != null)
                elementStatBuffLabel.text = StatTypeData.Get(element_buffType).TYPE_DESC;
        }

        void RefreshPetDetailLabel()
        {
            if (petNameLabel != null)
            {
                petNameLabel.text = petNameStr;
            }

            if (petCurrentReinforceLevelLabel != null)
            {
                petCurrentReinforceLevelLabel.text = "+" + petReinforce;
            }

            var maxReinforceCount = GameConfigTable.GetPetReinforceLevelMax(petGrade);//강화 최대 단계
            var isMaxLevel = petReinforce >= maxReinforceCount;
            if (petNextReinforceLevelLabel != null)
            {
                if (isMaxLevel)
                {
                    petNextReinforceLevelLabel.text = "MAX";
                }
                else
                {
                    petNextReinforceLevelLabel.text = SBFunc.StrBuilder("+ ", petReinforce + 1);
                }
            }
        }

        bool IsCurrentMaxReinforceLevel()
        {
            var petData = User.Instance.PetData.GetPet(petTag);//파츠 원본 데이터
            var petGrade = petData.Grade();

            //현재 아이템의 등급과 강화 레벨로 현재 강화 스텝 가져오기
            var maxLevelCheck = GameConfigTable.GetPetReinforceLevelMax(petGrade);//강화 최대 단계
            return petReinforce == maxLevelCheck;
        }

        void RefreshPetReinforceMaterial()//현재 레벨에서 강화 요구 재료 세팅
        {
            var petData = User.Instance.PetData.GetPet(petTag);//파츠 원본 데이터
            var isMaxLevel = IsCurrentMaxReinforceLevel();

            maxDisableButton.gameObject.SetActive(isMaxLevel);//맥렙일 때 불가능 버튼 추가
            resultPetFrame.gameObject.GetComponent<RectTransform>().anchoredPosition = isMaxLevel ? new Vector2(0, 0) : new Vector2(-150, 0);
            petNameLabel.gameObject.GetComponent<RectTransform>().anchoredPosition = isMaxLevel ? new Vector2(0, -120) : new Vector2(-150, -120);

            if (maxInvisibleObjectList != null && maxInvisibleObjectList.Count > 0)
            {
                foreach (var obj in maxInvisibleObjectList)
                {
                    if (obj == null)
                        continue;
                    obj.SetActive(!isMaxLevel);
                }
            }

            if (isMaxLevel)//현재 레벨이 맥스인 경우 리턴
            {
                InitReinforceMaterialList();
                success_rate_label.text = "--%";
                return;
            }

            var reinforceData = PetReinforceData.GetDataByGradeAndStep(petGrade, petReinforce + 1);

            float success_rate = reinforceData.RATE;//100만 기준 값

            var needitemID = reinforceData.ITEM;
            var needitemCount = reinforceData.ITEM_NUM;
            needitemCount += reinforceData.RAISE_ITEM_NUM * petData.ReinforceFalseCount;
            needitemCount = Mathf.Min(needitemCount, reinforceData.MAX_ITEM_NUM);
            var cost_type = reinforceData.COST_TYPE;
            var cost_num = reinforceData.COST_NUM;

            string calc_percent = "";
            if (success_rate_label != null)
            {
                calc_percent = ((success_rate / (float)SBDefine.MILLION * 100)).ToString();
                success_rate_label.text = SBFunc.StrBuilder(calc_percent, "%");
            }

            if (goldCostLabel != null)
                goldCostLabel.text = SBFunc.CommaFromNumber(cost_num);

            bool itemSufficient = false;
            if (materialNode != null)
            {
                materialNode.gameObject.SetActive(true);
                materialNode.setFrameRecipeInfo(needitemID, needitemCount);
                itemSufficient = materialNode.IsSufficientAmount;

                if (itemNameLabel != null)
                {
                    itemNameLabel.gameObject.SetActive(true);
                    itemNameLabel.text = StringData.GetStringByStrKey(ItemBaseData.Get(materialNode.GetItemID()).NAME);
                }    
            }
            

            var userGold = User.Instance.GOLD;
            var goldSufficient = userGold >= cost_num;

            isSufficientMaterial = (goldSufficient && itemSufficient);
        }

        void RefreshPartReinforceButtonUI()
        {
            var maxReinforceCount = GameConfigTable.GetPetReinforceLevelMax(petGrade);  //강화 최대 단계
            var isPossibleReinforce = petReinforce < maxReinforceCount;

            if (reinforceButton != null)
            {
                bool isInteract = (isPossibleReinforce && isSufficientMaterial);
                reinforceButton.interactable = isInteract;
                reinforceButton.SetButtonSpriteState(isInteract);
            }
        }

        public void OnClickReinforcePet()//강화 버튼 클릭 시
        {
            if (petTag <= 0)
            {
                ToastManager.On(100001844);
                return;
            }

            if(IsCurrentMaxReinforceLevel())
            {
                ToastManager.On(100000099);
                return;
            }

            if (isReinforce)
            {
                return;
            }
            isReinforce = true;

            CreateSpineNode();//스파인 노드 생성

            var param = new WWWForm();
            param.AddField("tag", petTag);
            param.AddField("reinforce", petReinforce + 1);
            
            NetworkManager.Send("pet/reinforce", param, (jsonObj) =>
            {
                if (jsonObj.ContainsKey("result")==false)
                {
                    //ToastManager.On(100002233);
                    //Debug.Log("강화 실패");

                    ChangeSpineAnimation("failed", false);
                    RefreshPetDetailUI();
                    SoundManager.Instance.PlaySFX("FX_PART_REINFORCE_FAIL");
                    isReinforce = false;
                    return;
                }
                var result = jsonObj["result"].Value<int>();
                switch (result)
                {
                    case 0://success
                    {
                        RefreshPetDetailUI();//왼쪽 UI 갱신

                        petSubListSlot?.RefreshViewPetsData(petTag);//리스트 UI 갱신
                        petSubListSlot.TableViewResetFlag = false;
                        petSubListSlot?.ForceUpdate();
                        petSubListSlot.TableViewResetFlag = true;

                        ChangeSpineAnimation("success", false);
                        PlayUnlockAnimation();

                        SoundManager.Instance.PlaySFX("FX_PART_REINFORCE_SUCCESS");

                        var petData = User.Instance.PetData.GetPet(petTag);
                        if (petData != null && petData.Reinforce == 15)
                        {
                            eAchieveSystemMessageType messageType = eAchieveSystemMessageType.PET_MAX_REINFORCE;
                            ChatManager.Instance.SendAchieveSystemMessage(messageType, User.Instance.UserData.UserNick, petData.ID);
                        }
                        //var param = new JObject();
                        //param.Add("tag", petTag);
                        //param.Add("currentLevel", petLevel - 1);
                        //param.Add("nextLevel", petLevel);
                        //SetUpgradeResultInfo(petReinforce - 1,petReinforce,User.Instance.PetData.GetPet(petTag));
                        //펫 강화 완료 팝업 연결
                        //PopupManager.OpenPopup<PetReinforceSuccessPopup>("PetReinforceSuccessPopup", param, true);
                    }
                    break;
                    case 1://failed
                    {
                        //ToastManager.On(100002233);
                        //Debug.Log("강화 실패");

                        ChangeSpineAnimation("failed", false);

                        //실패시 UI 업데이트 필요한가?
                        RefreshPetDetailUI();//소모 재료 업데이트
                        SoundManager.Instance.PlaySFX("FX_PART_REINFORCE_FAIL");
                    }
                    break;
                }
                isReinforce = false;
            });
        }



        void ChangeSpineAnimation(string animName, bool isLoop)
        {
            if (spine != null)
            {
                spine.AnimationState.SetAnimation(0, animName, isLoop);
            }
        }

        public void onClickBackButton()
        {
            if (isInitFlag)
            {
                petTabLayer.onClickChangeLayer("0");
            }
            else
            {
                PopupManager.GetPopup<DragonManagePopup>().CurPetTag = petTag;

                petTabLayer.onClickChangeLayer("1");
            }
        }
        public override bool backBtnCall()
        {
            if (backBtn != null)
            {
                backBtn.onClick.Invoke();
                return true;
            }
            return false;
        }
        public void OnClickUpgradeResultPopupOff()
        {
            upgradeResultObj.SetActive(false);
        }

        void SetUpgradeResultInfo(int beforeLv, int afterLv, UserPet petInfo) 
        {
            upgradeResultObj.SetActive(true);
            resultPetFrame.SetPetPortraitFrame(petInfo,false,false,true);
            beforeReinforceText.text = string.Format("+{0}", beforeLv);
            afterReinforceText.text = string.Format("+{0}", afterLv);
            var SubOptionList = petInfo.SubOptionList;
            for(int i = 0;i< OptionalStatValueTexts.Length; ++i)
            {
                OptionalStatValueTexts[i].text = "";
                OptionalStatValueTexts[i].gameObject.SetActive(false);
            }
            NewOptionValueText.text = ""; 
            int uniqueTextIndex = 0;
            if(SubOptionList != null) { 
                if(afterLv>= User.Instance.PetData.NewOptNeedReinforceVal[0])
                {
                    for (var i = 0; i < SubOptionList.Count; i++)
                    {
                        int statKey = SubOptionList[i].Key;
                        SubOptionData data = SubOptionData.Get(statKey); 
                        var value = SubOptionList[i].Value;
                        bool isPercent = data.VALUE_TYPE == "PERCENT";
                        if (afterLv > User.Instance.PetData.NewOptNeedReinforceVal[i])
                        {
                            OptionalStatValueTexts[uniqueTextIndex].gameObject.SetActive(true);
                            OptionalStatValueTexts[uniqueTextIndex].text = GetOptionText(data.STAT_TYPE, value, isPercent);
                            ++uniqueTextIndex;
                        }
                        if( afterLv == User.Instance.PetData.NewOptNeedReinforceVal[i]) {
                            NewOptionValueText.text = GetOptionText(data.STAT_TYPE, value, isPercent);
                            break;
                        }
                    }
                }
            }
        }
        string GetOptionText( string statType, float optionValue, bool isOptionValuePercent = false)
        {
            string optionValueString = optionValue.ToString("F2");
            if (isOptionValuePercent) optionValueString += "%";
            //string optionStr = string.Format(optionString, optionValueString);  // DESC 를 사용하면 이걸로 바꿀 것
            string optionStr = string.Format( "{0} +{1}", StatTypeData.GetDescStringByStatType(statType, isOptionValuePercent), optionValueString); // 임시
            return optionStr;
        }

        void RefreshPetStat(UserPet _petData)
        {
            SetAllLockStat();

            if (_petData.Stats != null)
            {
                var statCount = _petData.Stats.Count;

                for (int i = 0; i < statCount; ++i)
                {
                    var stat = _petData.Stats[i];
                    if (stat == null)
                        continue;

                    string statKey = stat.Key.ToString();
                    PetStatData data = PetStatData.Get(statKey);
                    bool isPercent = data.VALUE_TYPE == eStatusValueType.PERCENT;

                    var statTypeData = StatTypeData.Get(SBFunc.ConvertStatusType(data.STAT_TYPE));
                    var optionString = isPercent ? statTypeData.PERCENT_DESC : statTypeData.VALUE_DESC;
                    SetBaseStatInfo(i, optionString, PetStatData.GetStatValue(statKey, _petData.Level, petReinforce, stat.IsStatus1), isPercent);
                }
            }
        }

        void SetAllLockStat()
        {
            for(int i = 0; i< petStatNodeList.Length; i++)
            {
                var CheckNode = petStatNodeList[i];
                var optionKey = 0;
                var optionValue = 0;
                RefreshPetOptionUIByType("lock", CheckNode, optionKey, optionValue);
            }
        }

        void SetBaseStatInfo(int index, string optionString, float optionValue, bool isOptionValuePercent = false)
        {
            var childNodeList = SBFunc.GetChildren(SBFunc.GetChildrensByName(petStatNodeList[index].transform, new string[] { "child" }));
            if (childNodeList == null || childNodeList.Length <= 0)
            {
                return;
            }
            GameObject visibleNode = null;
            for (var i = 0; i < childNodeList.Length; i++)
            {
                var node = childNodeList[i];
                var isSameName = node.name == "open";
                if (isSameName)
                {
                    visibleNode = node.gameObject;
                }
                node.gameObject.SetActive(isSameName);
            }

            if (visibleNode != null)//타입에 따라 세부 내용 세팅
            {
                if (optionString == "" || optionValue <= 0)
                    return;

                var LabelNode = visibleNode.GetComponentsInChildren<Text>();
                if (LabelNode != null)
                {
                    optionValue = (float)Math.Round(optionValue, 2);
                    string optionValueString = "+" + optionValue.ToString();
                    if (isOptionValuePercent) optionValueString += "%";

                    LabelNode[0].text = optionString;
                    LabelNode[1].text = optionValueString;
                }
            }
        }
    }
}

