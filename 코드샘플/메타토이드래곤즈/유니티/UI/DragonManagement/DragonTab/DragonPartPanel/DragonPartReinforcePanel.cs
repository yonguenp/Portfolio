using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonPartReinforcePanel : MonoBehaviour
    {
        int REINFORCE_FIRST_STEP = -1;//6;
        int REINFORCE_SECOND_STEP = -1;//9;
        int REINFORCE_THIRD_STEP = -1;//12;
        int REINFORCE_FOURTH_STEP = -1;//15;

        [Space(10)]
        [Header("reinforce Info")]
        [SerializeField]
        PartSlotFrame slotFrame = null;

        [SerializeField]
        Text partNameLabel = null;
        [SerializeField]
        Text statTypeLabel = null;
        [SerializeField]
        Text nextStatTypeLabel = null;
        [SerializeField]
        Text statAmountLabel = null;
        [SerializeField]
        Text currentPartLevelLabel = null;
        [SerializeField]
        Text nextPartLevelLabel = null;
        [SerializeField]
        Text nextMainPartOptionLabel = null;
        [SerializeField]
        Button reinforceButton = null;
        [SerializeField]
        GameObject[] partOptionNodeList = null;

        //normal
        [SerializeField]
        GameObject SelectedNormal = null;
        [SerializeField]
        Text success_rate_label = null;
        [SerializeField]
        ItemFrame goldItemFrame = null;
        [SerializeField]
        ItemFrame materialItemFrame = null;
        [SerializeField]
        Text destroy_rate_label = null;
        [SerializeField]
        Text max_level_label = null;
        //nanochipset
        [SerializeField]
        GameObject SelectedNanoChipset = null;
        [SerializeField]
        Text success_rate_label_nano = null;
        [SerializeField]
        ItemFrame goldItemFrame_nano = null;
        [SerializeField]
        ItemFrame materialItemFrame_nano = null;
        [SerializeField]
        Text destroy_rate_label_nano = null;
        [SerializeField]
        Text max_level_label_nano = null;

        [SerializeField]
        GameObject spineNodeParent = null;
        

        [SerializeField]
        List<GameObject> maxInvisibleObjectList = new List<GameObject>();


        [Space(10)]
        [Header("initialize Info")]
        [SerializeField]
        Sprite partIconInit = null;
        [SerializeField]
        int partNameStringIndexInit = -1;
        [SerializeField]
        string partIconLevelStringIndexInit = "";
        [SerializeField]
        string partGradeLevelStringIndexInit = "?";

        [SerializeField]
        Button backBtn = null;

        int partTag = -1;
        int partReinforce = 0;//파츠 레벨
        bool isSufficientGold = false;
        bool isSufficientMaterial = false;
        SkeletonGraphic spine = null;

        protected bool isReinforce = false;

        bool isOpen = false;
        public bool IsOpen { get { return isOpen; } }
        bool SelectedNanoChipsetMode = false;
        void OnEnable()
        {

        }

        void OnDisable()
        {
            isOpen = false;
            partTag = -1;
            TurnOffAllOpenAnim();
        }
        public void ShowReinforcePanel()
        {
            gameObject.SetActive(true);
            isOpen = true;
            Init();
        }

        public void HideReinforcePanel()
        {
            gameObject.SetActive(false);
        }

        public void Init()
        {
            SelectedNanoChipsetMode = false;
            InitStepData();
            InitCurrentPartTag();
            InitEffectSetting();
            InitPartInfoSlot();

            RefreshSelectMode();
        }

        void SuccessLevelPartData()//tableView에 세팅된 장비데이터에 신규 레벨 갱신
        {
            DragonPartEvent.SuccessReinforcePart(partTag);
        }

        void DeletePartDataInTableData(int parttag)
        {
            DragonPartEvent.DeleteReinforcePart(parttag);
        }

        public void OnClickPartFrame(int _partTag)
        {
            if (_partTag > 0)
            {
                SetCurrentPartTagData(_partTag);
                InitCurrentPartTag();
                InitPartInfoSlot();
            }
        }

        public void ForceUpdate()
        {
            Init();
        }

        void InitCurrentPartTag()
        {
            if (PopupManager.GetPopup<DragonManagePopup>().CurPartTag != 0)
            {
                var currentTag = PopupManager.GetPopup<DragonManagePopup>().CurPartTag;
                if (currentTag > 0)
                {
                    partTag = currentTag;
                }
                PopupManager.GetPopup<DragonManagePopup>().CurPartTag = 0;
            }
        }

        void InitStepData()
        {
            if (REINFORCE_FIRST_STEP < 0)
                REINFORCE_FIRST_STEP = GameConfigTable.GetPartReinforceFirstStep();
            if (REINFORCE_SECOND_STEP < 0)
                REINFORCE_SECOND_STEP = GameConfigTable.GetPartReinforceSecondStep();
            if (REINFORCE_THIRD_STEP < 0)
                REINFORCE_THIRD_STEP = GameConfigTable.GetPartReinforceThirdStep();
            if (REINFORCE_FOURTH_STEP < 0)
                REINFORCE_FOURTH_STEP = GameConfigTable.GetPartReinforceFourthStep();
        }

        void SetCurrentPartTagData(int tag)
        {
            PopupManager.GetPopup<DragonManagePopup>().CurPartTag = tag;
        }

        void InitEffectSetting()
        {
            spine = null;
            SBFunc.RemoveAllChildrens(spineNodeParent.transform);
        }

        void InitPartInfoSlot()
        {
            RefreshPartDetailUI();
        }

        void InitializePartDetailUI()
        {
            InitPartDetailUI();//초기값 라벨 및 디자인 세팅
            InitPartOptionUI();//부옵 전체 잠금
            InitPartReinforceButtonUI();//강화 버튼 잠금
            InitMaxLabelNode();//최대 강화 라벨 지우기
        }

        void InitPartDetailUI()
        {
            if (currentPartLevelLabel != null)
            {
                currentPartLevelLabel.text = partGradeLevelStringIndexInit;
            }
            if (nextPartLevelLabel != null)
            {
                nextPartLevelLabel.text = partGradeLevelStringIndexInit;
            }
            if (partNameLabel != null)
            {
                partNameLabel.text = StringData.GetStringByIndex(partNameStringIndexInit);
            }
            if (nextMainPartOptionLabel != null)
            {
                nextMainPartOptionLabel.text = partGradeLevelStringIndexInit;
            }
            if (statAmountLabel != null)
            {
                statAmountLabel.text = partGradeLevelStringIndexInit;
            }
            if (success_rate_label != null)
            {
                success_rate_label.text = "-%";
            }
            if (success_rate_label_nano != null)
            {
                success_rate_label_nano.text = "-%";
            }
            if (statTypeLabel != null)
            {
                statTypeLabel.text = "-";
            }
            if (destroy_rate_label != null)
            {
                var destoyStrFormat = StringData.GetStringByIndex(100002044);
                destroy_rate_label.text = string.Format(destoyStrFormat, "0.00");
            }
            if (destroy_rate_label_nano != null)
            {
                var destoyStrFormat = StringData.GetStringByIndex(100002044);
                destroy_rate_label_nano.text = string.Format(destoyStrFormat, "0.00");
            }
        }

        public void OnSelectNormal()
        {
            SelectedNanoChipsetMode = false;
            RefreshSelectMode();
        }

        public void OnSelectNanoChipset()
        {
            SelectedNanoChipsetMode = true;
            RefreshSelectMode();
        }

        void RefreshSelectMode()
        {
            if (SelectedNormal != null)
                SelectedNormal.SetActive(!SelectedNanoChipsetMode);
            if (SelectedNanoChipset != null)
                SelectedNanoChipset.SetActive(SelectedNanoChipsetMode);

            
            if (!SelectedNanoChipsetMode && materialItemFrame != null)
            {
                isSufficientMaterial = materialItemFrame.IsSufficientAmount;
            }

            if (SelectedNanoChipsetMode && materialItemFrame_nano != null)
            {
                isSufficientMaterial = materialItemFrame_nano.IsSufficientAmount;
            }

            RefreshPartReinforceButtonUI();
        }

        void RefreshPartDetailUI()
        {
            var partData = User.Instance.PartData.GetPart(partTag);//파츠 원본 데이터

            if (partData == null)
            {
                return;
            }
            var partItemDesignData = partData.GetItemDesignData();//파츠 아이템 테이블 데이터
            var partDesignData = partData.GetPartDesignData();//파츠 테이블 데이터

            if (partItemDesignData == null || partDesignData == null)
            {
                Debug.Log("designData is null");
                return;
            }

            partReinforce = partData.Reinforce;//강화 레벨
            var partOptionList = partData.SubOptionList;//key와 value

            var partMainoptionType_str = partDesignData.STAT_TYPE;//파츠 타입 string
            var partMainoption_Amount = partData.GetValue();
            var partMainType = partDesignData.VALUE_TYPE;//per 인지 value인지

            partNameLabel.text = partItemDesignData.NAME;

            if (slotFrame != null)
                slotFrame.SetPartSlotFrame(partTag, partReinforce, false);

            if (currentPartLevelLabel != null)
            {
                currentPartLevelLabel.text = "+" + partReinforce;
            }

            var maxReinforceCount = PartReinforceData.GetMaxReinforceStep(partDesignData.GRADE);//현재 최대 강화 단계
            var isMaxLevel = partReinforce >= maxReinforceCount;
            if (nextPartLevelLabel != null)
            {
                nextPartLevelLabel.text = isMaxLevel ? "MAX" : SBFunc.StrBuilder("+", partReinforce + 1);
            }

            statTypeLabel.text = StatTypeData.GetDescStringByStatType(partMainoptionType_str, partMainType == "PERCENT");
            nextStatTypeLabel.text = StatTypeData.GetDescStringByStatType(partMainoptionType_str, partMainType == "PERCENT");
            var mainoptionValueStr = Math.Round(partMainoption_Amount + partDesignData.VALUE_GROW, 2).ToString(GamePreference.Instance.Culture);
            float mainoptionValue = 0.0f;

            if (float.TryParse(mainoptionValueStr, System.Globalization.NumberStyles.Float, GamePreference.Instance.Culture, out float ret))
                mainoptionValue = ret;

            SetLabelByMainType(partMainType, statAmountLabel, partMainoption_Amount, false);
            SetLabelByMainType(partMainType, nextMainPartOptionLabel, mainoptionValue, isMaxLevel);

            //강화 단계별 리스트 데이터 갱신
            RefreshPartReinforceMaterial();
            RefreshPartOptionUI(partDesignData.KEY, partOptionList);
        }

        void SetLabelByMainType(string type, Text targetLabel, float text, bool isMaxLevel)
        {
            if (isMaxLevel)
            {
                targetLabel.text = "MAX";
                return;
            }
            switch (type.ToUpper())
            {
                case "PERCENT":
                {
                    targetLabel.text = SBFunc.StrBuilder("+", text,"%");
                }
                break;
                case "VALUE":
                {
                    targetLabel.text = '+' + text.ToString();
                }
                break;
            }
        }

        void RefreshPartReinforceMaterial()//현재 레벨에서 강화 요구 재료 세팅
        {
            var partData = User.Instance.PartData.GetPart(partTag);//파츠 원본 데이터
            var partGrade = partData.Grade();
            var partIndex = partData.ID;

            //현재 아이템의 등급과 강화 레벨로 현재 강화 스텝 가져오기
            var maxLevelCheck = PartReinforceData.GetMaxReinforceStep(partGrade);
            var isMaxLevel = partReinforce == maxLevelCheck;

            if (max_level_label != null)
            {
                max_level_label.gameObject.SetActive(isMaxLevel);
            }

            if (max_level_label_nano != null)
            {
                max_level_label_nano.gameObject.SetActive(isMaxLevel);
            }

            if (maxInvisibleObjectList != null && maxInvisibleObjectList.Count > 0)
            {
                foreach(var obj in maxInvisibleObjectList)
                {
                    if (obj == null)
                        continue;
                    obj.SetActive(!isMaxLevel);
                }
            }

            if (isMaxLevel)//현재 레벨이 맥스인 경우 리턴
            {
                return;
            }

            var reinforceData = PartReinforceData.GetDataByGradeAndStep(partGrade, partReinforce + 1);
            if(reinforceData == null)
            {
                return;
            }

            float success_rate = reinforceData.RATE;//100만 기준 값
            float success_rate2 = reinforceData.RATE2;//100만 기준 값

            var needitemID = reinforceData.ITEM;
            var needitemCount = reinforceData.ITEM_NUM;

            var needitemID2 = reinforceData.ITEM2;
            var needitemCount2 = reinforceData.ITEM_NUM2;

            var cost_type = reinforceData.COST_TYPE;
            var cost_num = reinforceData.COST_NUM;

            float destroy_rate = reinforceData.DESTROY;//100만 기준 값
            float destroy_rate2 = reinforceData.DESTROY2;//100만 기준 값
            var destroy_reward = reinforceData.DESTROY_REWARD;//실패시 획득 재료아이템
            //var destroy_reward_num = reinforceData.DESTROY_REWARD_NUM;//실패시 획득 재료 수량

            string calc_percent = "";
            if (success_rate_label != null)
            {
                calc_percent = ((success_rate / (float)SBDefine.MILLION * 100)).ToString();
                success_rate_label.text = SBFunc.StrBuilder(calc_percent, "%");
            }

            if (destroy_rate_label != null)
            {
                if (float.TryParse(calc_percent, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ret))
                {
                    if (ret >= 100)
                        destroy_rate = 0;
                }

                var destoyStrFormat = StringData.GetStringByIndex(100002044);
                destroy_rate_label.text = string.Format(destoyStrFormat, ((destroy_rate / (float)SBDefine.MILLION * 100)).ToString());
            }

            if (success_rate_label_nano != null)
            {
                calc_percent = ((success_rate2 / (float)SBDefine.MILLION * 100)).ToString();
                success_rate_label_nano.text = SBFunc.StrBuilder(calc_percent, "%");
            }

            if (destroy_rate_label_nano != null)
            {
                if (float.TryParse(calc_percent, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ret))
                {
                    if (ret >= 100)
                        destroy_rate2 = 0;
                }

                var destoyStrFormat = StringData.GetStringByIndex(100002044);
                destroy_rate_label_nano.text = string.Format(destoyStrFormat, ((destroy_rate2 / (float)SBDefine.MILLION * 100)).ToString());
            }

            if (goldItemFrame != null)
            {
                var type = eGoodType.NONE;

                switch (cost_type)
                {
                    case "CASH":
                        type = eGoodType.CASH;
                        break;

                    case "GOLD":
                        type = eGoodType.GOLD;
                        break;

                    case "GEMSTONE":
                        type = eGoodType.GEMSTONE;
                        break;
                }

                goldItemFrame.setFrameCashInfo((int)type, cost_num, true);
                goldItemFrame.SetItemBgOff();
            }

            if (goldItemFrame_nano != null)
            {
                var type = eGoodType.NONE;

                switch (cost_type)
                {
                    case "CASH":
                        type = eGoodType.CASH;
                        break;

                    case "GOLD":
                        type = eGoodType.GOLD;
                        break;

                    case "GEMSTONE":
                        type = eGoodType.GEMSTONE;
                        break;
                }

                goldItemFrame_nano.setFrameCashInfo((int)type, cost_num, true);
                goldItemFrame_nano.SetItemBgOff();
            }

            if (materialItemFrame != null)
            {
                materialItemFrame.setFrameRecipeInfo(needitemID, needitemCount);
                materialItemFrame.SetItemBgOff();

                if(!SelectedNanoChipsetMode)
                    isSufficientMaterial = materialItemFrame.IsSufficientAmount;
            }

            if (materialItemFrame_nano != null)
            {
                materialItemFrame_nano.setFrameRecipeInfo(needitemID2, needitemCount2);
                materialItemFrame_nano.SetItemBgOff();

                if (SelectedNanoChipsetMode)
                    isSufficientMaterial = materialItemFrame_nano.IsSufficientAmount;
            }

            var userGold = User.Instance.GOLD;
            isSufficientGold = userGold >= cost_num;
        }

        void InitPartOptionUI()//전체 잠금 처리
        {
            if (partOptionNodeList != null && partOptionNodeList.Length > 0)
            {
                var listCount = partOptionNodeList.Length;
                for (var i = 0; i < listCount; i++)//각 단계별 부옵 리스트 갱신 (강화 단계로 인한 잠금상태, 단순 미강화 잠금, 부옵 표시)
                {
                    var CheckNode = partOptionNodeList[i];
                    var optionKey = 0;
                    var optionValue = 0;
                    RefreshPartOptionUIByType("none", CheckNode, optionKey, optionValue);
                }
            }
        }

        void RefreshPartOptionUI(int ID, List<KeyValuePair<int, float>> partOptionList)
        {
            if (partOptionNodeList != null && partOptionNodeList.Length > 0)
            {
                var listCount = partOptionNodeList.Length;
                var partOptionListLength = partOptionList.Count;
                var availableSlot = PartBaseData.GetMaxReinforceSlotCount(ID);//현재 오픈 가능한 최대 슬롯 갯수
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
                    RefreshPartOptionUIByType(checkType, CheckNode, optionKey, optionValue);
                }
            }

            RefreshPartReinforceButtonUI();
        }

        void InitPartReinforceButtonUI()
        {
            if (reinforceButton != null)
            {
                reinforceButton.SetButtonSpriteState(false);
            }
        }

        void InitMaxLabelNode()
        {
            if (max_level_label != null)
            {
                max_level_label.gameObject.SetActive(false);
            }

            if (max_level_label_nano != null)
            {
                max_level_label_nano.gameObject.SetActive(false);
            }
        }

        void RefreshPartReinforceButtonUI()
        {
            if (reinforceButton != null)
            {
                reinforceButton.SetButtonSpriteState(IsAvailableReinforce());
            }
        }

        bool IsAvailableReinforce(bool _showToast = false)
        {
            var partData = User.Instance.PartData.GetPart(partTag);//파츠 원본 데이터
            var maxReinforceCount = PartReinforceData.GetMaxReinforceStep(partData.Grade());//현재 최대 강화 단계
            var isPossibleReinforce = partReinforce < maxReinforceCount;

            if(_showToast)
            {
                if (!isPossibleReinforce)
                {
                    ToastManager.On(StringData.GetStringByStrKey("gem_info_text_04"));
                }
                else if(!isSufficientMaterial || ! isSufficientGold)
                {
                    ToastManager.On(100000619);
                }
            }

            return isPossibleReinforce && isSufficientMaterial && isSufficientGold;
        }

        void RefreshPartOptionUIByType(string checkType, GameObject targetNode, int optionKey = 0, float optionValue = 0)//켜야될 노드 세팅
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

                        var LabelNode = visibleNode.GetComponentInChildren<Text>();
                        if (LabelNode != null)
                        {
                            optionValue = (float)Math.Round((double)optionValue, 2);
                            var str = SBFunc.StrBuilder(typeStr, " +", optionValue);
                            if (valueType == "PERCENT")
                            {
                                str += "%";
                            }

                            LabelNode.text = str;
                        }
                    }
                    break;
                }
            }
        }
        public void OnClickReinforcePart()//강화 버튼 클릭 시
        {
            if (partTag <= 0)
            {
                return;
            }

            if (!IsAvailableReinforce(true))
            {
                return;
            }

            if (isReinforce)
            {
                return;
            }
            isReinforce = true;

            CreateSpineNode();//스파인 노드 생성

            var param = new WWWForm();
            param.AddField("tag", partTag);
            param.AddField("reinforce", partReinforce + 1);
            param.AddField("type", SelectedNanoChipsetMode ? 2 : 1);

            NetworkManager.Send("part/reinforce", param, (NetworkManager.SuccessCallback)((jsonObj) =>
            {
                var data = jsonObj;
                if (jsonObj.ContainsKey("result"))
                {
                    var result = jsonObj["result"].Value<int>();
                    switch (result)
                    {
                        case 0://success
                        {
                            RefreshPartDetailUI();//왼쪽 UI 갱신
                            SuccessLevelPartData();

                            ChangeSpineAnimation("success", false);
                            PlayUnlockAnimation();

                            SoundManager.Instance.PlaySFX("FX_PART_REINFORCE_SUCCESS");

                            // 시스템 메시지 발송
                            UserPart currentPart = User.Instance.PartData.GetPart(partTag);
                            if (currentPart != null)
                            {
                                switch(currentPart.Reinforce)
                                {
                                    case 9:
                                        ChatManager.Instance.SendAchieveSystemMessage((eAchieveSystemMessageType)eAchieveSystemMessageType.EQUIPMENT_9_REINFORCE, User.Instance.UserData.UserNick, currentPart.GetItemDesignData().KEY);
                                        break;
                                    case 12:
                                        ChatManager.Instance.SendAchieveSystemMessage((eAchieveSystemMessageType)eAchieveSystemMessageType.EQUIPMENT_12_REINFORCE, User.Instance.UserData.UserNick, currentPart.GetItemDesignData().KEY);
                                        break;
                                    case 15:
                                        ChatManager.Instance.SendAchieveSystemMessage((eAchieveSystemMessageType)eAchieveSystemMessageType.EQUIPMENT_15_REINFORCE, User.Instance.UserData.UserNick, currentPart.GetItemDesignData().KEY);
                                        break;
                                }
                                
                            }
                        }
                        break;
                        case 1://failed
                        {
                            ChangeSpineAnimation("failed", false);

                            RefreshPartReinforceMaterial();//소모 재료 업데이트
                            SoundManager.Instance.PlaySFX("FX_PART_REINFORCE_FAIL");
                        }
                        break;
                        case 2://destroy
                        {
                            var array = (JArray)jsonObj["reward"];
                            var count = array.Count;

                            var items = new List<Asset>();
                            for (var i = 0; i < count; i++)
                            {
                                var itemData = JArray.FromObject(array[i]);
                                items.Add(new Asset(eGoodType.ITEM, itemData[1].Value<int>(), itemData[2].Value<int>()));
                            }

                            ShowDestroyPopup(items);//파괴 팝업 노출

                            var tempPartTag = partTag;//삭제할 태그값 임시저장
                            partTag = -1; //삭제된 장비 태그값 초기화
                            InitializePartDetailUI();//왼쪽 화면 초기화
                            DeletePartDataInTableData(tempPartTag);//오른쪽 터져버린 항목 삭제

                            SoundManager.Instance.PlaySFX("FX_PART_REINFORCE_DESTROY");
                        }
                        break;
                    }
                }
                else
                {
                    Debug.Log(data);
                    RefreshPartDetailUI();//왼쪽 UI 갱신
                }

                isReinforce = false;
            }));
        }
        void PlayUnlockAnimation()
        {
            var checkIndex = -1;
            if (partReinforce == REINFORCE_FIRST_STEP)
            {
                checkIndex = 0;
            }
            else if (partReinforce == REINFORCE_SECOND_STEP)
            {
                checkIndex = 1;
            }
            else if (partReinforce == REINFORCE_THIRD_STEP)
            {
                checkIndex = 2;
            }
            else if (partReinforce == REINFORCE_FOURTH_STEP)
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

        void CreateSpineNode()
        {
            if (spine == null)
            {
                var spineClonePrefab = ResourceManager.GetResource<GameObject>(eResourcePath.EffectPrefabPath, "part_reinforce_effect");
                if (spineClonePrefab != null && spineNodeParent != null)
                {
                    var clone = Instantiate(spineClonePrefab, spineNodeParent.transform);

                    spine = clone.GetComponentInChildren<SkeletonGraphic>();
                }
            }
        }

        float GetAnimationLength(string AnimName)
        {
            var duration = 0;
            if (spine == null)
            {
                return duration;
            }

            var anim = spine.SkeletonData.FindAnimation(AnimName);
            if (anim == null)
            {
                return duration;
            }
            return anim.Duration;
        }

        void ChangeSpineAnimation(string animName, bool isLoop)
        {
            if (spine == null)
            {
                CreateSpineNode();
            }

            if (spine != null)
            {
                spine.AnimationState.SetAnimation(0, animName, isLoop);
            }
        }

        void ShowDestroyPopup(List<Asset> items)//장비 파괴 시 - UI전부 끄기 처리
        {
            if (items == null || items.Count < 1)
                return;

            PopupManager.OpenPopup<PartDestroyPopup>(new PartPopupData(items));
        }

        public bool backBtnCall()
        {
            if (backBtn != null)
            {
                backBtn.onClick.Invoke();
                return true;
            }
            return false;
        }
    }
}
