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
    public class ChampionBattleDragonPartReinforcePanel : MonoBehaviour
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
        [SerializeField]
        Text success_rate_label = null;
        [SerializeField]
        ItemFrame goldItemFrame = null;
        [SerializeField]
        ItemFrame materialItemFrame = null;
        [SerializeField]
        GameObject spineNodeParent = null;
        [SerializeField]
        Text destroy_rate_label = null;
        [SerializeField]
        Text max_level_label = null;

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

        bool isSufficientMaterial = false;
        SkeletonGraphic spine = null;

        protected bool isReinforce = false;

        bool isOpen = false;
        public bool IsOpen { get { return isOpen; } }
        ChampionBattleDragonSelectPopup ParentPopup { get { return PopupManager.GetPopup<ChampionBattleDragonSelectPopup>(); } }
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
            InitStepData();
            InitCurrentPartTag();
            InitEffectSetting();
            InitPartInfoSlot();
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
            if (ParentPopup.PartTag != 0)
            {
                var currentTag = ParentPopup.PartTag;
                if (currentTag > 0)
                {
                    partTag = currentTag;
                }
                ParentPopup.SetPartTag(0);
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
            ParentPopup.SetPartTag(tag);
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
            if (statTypeLabel != null)
            {
                statTypeLabel.text = "-";
            }
            if (destroy_rate_label != null)
            {
                var destoyStrFormat = StringData.GetStringByIndex(100002044);
                destroy_rate_label.text = string.Format(destoyStrFormat, "0.00");
            }
        }

        void RefreshPartDetailUI()
        {
            var partData = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon.GetPart(partTag);//파츠 원본 데이터

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
            //강화안됨
            
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
            //강화안됨
            var partData = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon.GetPart(partTag);//파츠 원본 데이터


            var maxReinforceCount = PartReinforceData.GetMaxReinforceStep(partData.Grade());//현재 최대 강화 단계
            var isPossibleReinforce = partReinforce < maxReinforceCount;

            if(_showToast)
            {
                if (!isPossibleReinforce)
                {
                    ToastManager.On(StringData.GetStringByStrKey("gem_info_text_04"));
                }
                else if(!isSufficientMaterial)
                {
                    ToastManager.On(100000619);
                }
            }

            return isPossibleReinforce && isSufficientMaterial;
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
            //강화안됨
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
