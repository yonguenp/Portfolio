using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonTranscendencePanel : DragonManageSubPanel
    {
        [Header("현재 정보 표기")]
        [SerializeField] GameObject currentExistAbilityLayer;
        [SerializeField] GameObject currentNoExistAbilityLayer; // 현재 별이 없어서 현재 단계 정보가 없을 때 출력
        [SerializeField] GameObject[] curEmptyStars;
        [SerializeField] GameObject[] curStars;
        [SerializeField] Text curAbilityText;

        [Header("다음 정보 표기")]
        [SerializeField] GameObject nextExistAbilityLayer;
        [SerializeField] GameObject nextNoExistAbilityLayer; // 현재 이미 max 상태라서 다음 단계가 없을 때 출력
        [SerializeField] GameObject[] nextEmptyStars;
        [SerializeField] GameObject[] nextStars;
        [SerializeField] Text nextAbilityText;

        [Header("확률 표기")]
        [SerializeField] Slider percentageSlider;
        [SerializeField] Text percentageText;

        [Header("드래곤 재료")]
        [SerializeField] TableViewGrid dragonCardTableView;
        [SerializeField] GameObject emptyCardLabel;

        [Header("button")]
        [SerializeField] Button autoSelectBtn;
        [SerializeField] Text selectedCountText;
        [SerializeField] Button TranscendenceBtn;

        Tween sliderTween = null;
        int MaterialMax = 0;
        int StepMax = 0;


        int requireGrade;
        private bool IsTranscendence { get; set; } = false;
       
        int currentStep = 0;
        UserDragon curDragonData = null;
        CharTranscendenceData CurrentCharTranscendenceData = null;
        CharTranscendenceData NextCharTranscendenceData = null;
        List<ITableData> tableViewItemList = new List<ITableData>();
        private List<UserDragonCard> selectedList = new List<UserDragonCard>();

        int MaxSlider = SBDefine.MILLION;
        bool isMaxPercentageState = false;

        int autoCount = 0;
        public override void Init()
        {
            base.Init();
            curDragonData = User.Instance.DragonData.GetDragon(dragonTag);
            Refresh();
        }
        public void Refresh()
        {
            SetInfo();
            SetAbilityLayer();
            InitPercentage();
            RefreshCardTableView();
        }

        private void OnDisable()
        {
            sliderTween?.Kill();
        }

        void SetInfo()
        {
            isMaxPercentageState = false;
            currentStep = curDragonData.TranscendenceStep;
            var grade = (eDragonGrade)curDragonData.Grade();
            StepMax = CharTranscendenceData.GetStepMax(grade);
            NextCharTranscendenceData = CharTranscendenceData.Get(grade, currentStep + 1);
            CurrentCharTranscendenceData = CharTranscendenceData.Get(grade, currentStep);
            if (currentStep < StepMax)
            {
                SetRequireGrade(NextCharTranscendenceData);   
                MaterialMax = NextCharTranscendenceData.MATERIAL_MAX;
            }
            else
            {
                SetRequireGrade(CurrentCharTranscendenceData);
                NextCharTranscendenceData = null;
            }
            
        }

        void SetRequireGrade(CharTranscendenceData data)
        {
            if (data.MATERIAL_GRADE_RATE_1 > 0)
            {
                requireGrade = 1;
                return;
            }
            if (data.MATERIAL_GRADE_RATE_2 > 0)
            {
                requireGrade = 2;
                return;
            }
            if (data.MATERIAL_GRADE_RATE_3 > 0)
            {
                requireGrade = 3;
                return;
            }
            if (data.MATERIAL_GRADE_RATE_4 > 0)
            {
                requireGrade = 4;
                return;
            }
            if (data.MATERIAL_GRADE_RATE_5 > 0)
            {
                requireGrade = 5;
                return;
            }
        }


        void SetAbilityLayer()
        {
            bool isCurrentAbilityExist = (currentStep > 0);
            bool isCurrentStepMax = (currentStep == StepMax);
            currentExistAbilityLayer.SetActive(isCurrentAbilityExist);
            currentNoExistAbilityLayer.SetActive(!isCurrentAbilityExist);
            nextExistAbilityLayer.SetActive(!isCurrentStepMax);
            nextNoExistAbilityLayer.SetActive(isCurrentStepMax);
            for (int i = 0, count = curStars.Length; i < count; ++i)
            {
                if (curStars[i] != null)
                    curStars[i].SetActive(i < currentStep);
            }
            for (int i = 0, count = nextStars.Length; i < count; ++i)
            {
                if (nextStars[i] != null)
                    nextStars[i].SetActive(i <= currentStep);
            }
            for (int i = 0, count = curEmptyStars.Length; i < count; ++i)
            {
                if (curEmptyStars[i] != null)
                    curEmptyStars[i].SetActive(i < StepMax);
                if (nextEmptyStars[i] != null)
                    nextEmptyStars[i].SetActive(i < StepMax);
            }


            int strKey = 0;
            switch ((eElementType)curDragonData.Element())
            {
                case eElementType.FIRE:
                    strKey = 100002850;
                    break;
                case eElementType.WATER:
                    strKey = 100002849;
                    break;
                case eElementType.EARTH:
                    strKey = 100002852;
                    break;
                case eElementType.WIND:
                    strKey = 100002851;
                    break;
                case eElementType.LIGHT:
                    strKey = 100002847;
                    break;
                case eElementType.DARK:
                    strKey = 100002848;
                    break;
            }
            int curPassiveSkillSlot = 0;
            if (CurrentCharTranscendenceData != null)
            {
                int curStat = CurrentCharTranscendenceData.ADD_STAT;
                curPassiveSkillSlot = CurrentCharTranscendenceData.SKILL_SLOT_MAX;
                curAbilityText.text = StringData.GetStringFormatByStrKey("TRANSCENDENCE_BONUS", curStat);
            }
            if(NextCharTranscendenceData != null)
            {
                int nextStat = NextCharTranscendenceData.ADD_STAT;
                string statText= StringData.GetStringFormatByStrKey("TRANSCENDENCE_BONUS", nextStat);
                if (curPassiveSkillSlot < NextCharTranscendenceData.SKILL_SLOT_MAX)
                {
                    statText += "\n" + StringData.GetStringByStrKey("패시브오픈");
                }
                nextAbilityText.text = statText;
            }
            //To Do ability Text
        }

        void InitPercentage()
        {
            percentageSlider.maxValue = MaxSlider;
            if (NextCharTranscendenceData != null)
            {
                percentageSlider.value = 0;
                percentageText.text = "0%";
            }
            else // 다음 초월 단계가 없다
            {
                percentageSlider.value = MaxSlider;
                percentageText.text = StringData.GetStringByStrKey("최대초월");
            }
        }

        void RefreshCardTableView()
        {
            if (selectedList == null)
                selectedList = new();
            selectedList.Clear();
            List<UserDragonCard> totalList = User.Instance.DragonCards.GetAllList();
            
            autoSelectBtn.SetButtonSpriteState(totalList.Count > 0);
            TranscendenceBtn.SetButtonSpriteState(totalList.Count > 0);
            selectedCountText.text = string.Format("{0}/{1}", selectedList.Count, MaterialMax);
            tableViewItemList.Clear();
            totalList = totalList.OrderByDescending(dragon => dragon.DragonTag).ToList();
            foreach (UserDragonCard card in totalList)
            {
                if (card.CardGrade >= requireGrade)
                    tableViewItemList.Add(card);
            }
            emptyCardLabel.gameObject.SetActive(!(tableViewItemList.Count > 0));
            RefreshInfo();
            dragonCardTableView.OnStart();
            dragonCardTableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) =>
            {
                if (node == null)
                    return;
                var frame = node.GetComponent<DragonCardFrame>();
                if (frame == null)
                    return;
                var data = (UserDragonCard)item;
                bool isSelect = selectedList.Contains(data);
                frame.InitCardFrame(data, isSelect, true);
                frame.SetSelect(isSelect);
                frame.ClickCallBack = (DragonCardFrame dragonFrame, UserDragonCard cardData) =>
                {
                    if (dragonFrame.IsSelect)
                    {
                        selectedList.Remove(cardData);
                        dragonFrame.SetSelect(false);
                        RefreshInfo();
                    }
                    else
                    {
                        if (selectedList.Count >= MaterialMax || isMaxPercentageState)
                        {
                            ToastManager.On(StringData.GetStringByStrKey("재료선택불가")); // 더이상 선택 불가
                        }
                        else
                        {
                            selectedList.Add(cardData);
                            dragonFrame.SetSelect(true);
                            RefreshInfo();
                        }
                    }
                };
            }));
            dragonCardTableView.ReLoad();
        }
        void RefreshInfo()
        {
            int selectCnt = selectedList.Count;

            selectedCountText.text = string.Format("{0}/{1}", selectCnt, MaterialMax);
            TranscendenceBtn.SetButtonSpriteState(selectCnt > 0);
            if (NextCharTranscendenceData == null)
                return;
            int currentPercentage = 0;
            foreach (var item in selectedList)
            {
                switch ((eDragonGrade)item.CardGrade)
                {
                    case eDragonGrade.Normal:
                        currentPercentage += NextCharTranscendenceData.MATERIAL_GRADE_RATE_1;
                        break;
                    case eDragonGrade.Uncommon:
                        currentPercentage += NextCharTranscendenceData.MATERIAL_GRADE_RATE_2;
                        break;
                    case eDragonGrade.Rare:
                        currentPercentage += NextCharTranscendenceData.MATERIAL_GRADE_RATE_3;
                        break;
                    case eDragonGrade.Unique:
                        currentPercentage += NextCharTranscendenceData.MATERIAL_GRADE_RATE_4;
                        break;
                    case eDragonGrade.Legend:
                        currentPercentage += NextCharTranscendenceData.MATERIAL_GRADE_RATE_5;
                        break;
                }
            }
            if(currentPercentage >= MaxSlider)
            {
                currentPercentage = MaxSlider;
                isMaxPercentageState = true;
            }
            else
            {
                isMaxPercentageState = false;
            }
            if (sliderTween != null)
                sliderTween.Kill();
            sliderTween = percentageSlider.DOValue(currentPercentage, 0.3f);
            
            percentageText.text = (currentPercentage/(float)MaxSlider).ToString("P4");
        }
        public void OnClickTranscendence()
        {
            if (curDragonData == null || 0 >= curDragonData.Tag || false == User.Instance.DragonData.IsContainsDragon(curDragonData.Tag))
            {
                ToastManager.On(StringData.GetStringByIndex(100002548));//해당 드래곤을 보유하고 있지 않습니다.
                return;
            }
            if (curDragonData.Level < GameConfigTable.GetConfigIntValue("TRANSCENDENCE_MINIMUM_LEVEL", 50) || curDragonData.SLevel < GameConfigTable.GetConfigIntValue("TRANSCENDENCE_MINIMUM_SKILL_LEVEL", 50))
            {
                ToastManager.On(StringData.GetStringByStrKey("town_floors_text_01")); // 레벨이 부족합니다.
                
                return;
            }
            if (null == selectedList || 0 >= selectedList.Count)
            {
                ToastManager.On(StringData.GetStringByStrKey("초월재료선택")); //초월에 사용할 드래곤 카드를 선택해 주세요.
                return;
            }
            if(MaterialMax < selectedList.Count)
            {
                ToastManager.On(StringData.GetStringByStrKey("재료선택불가")); //더 이상의 드래곤 카드를 선택할 수 없습니다.
                return;
            }
            if (currentStep == StepMax)
            {
                ToastManager.On(StringData.GetStringByStrKey("최대초월토스트")); // 최대 단계입니다.
                return;
            }

            if (IsTranscendence)
                return;

            IsTranscendence = true;

            var data = new WWWForm();

            JArray array = new JArray();
            for (int i = 0, count = selectedList.Count; i < count; ++i)
            {
                if (selectedList[i] == null)
                    continue;

                array.Add(selectedList[i].CardTag);
            }

            if (array.Count != selectedList.Count)
            {
                IsTranscendence = false;
                return;
            }

            //초월할 드래곤
            data.AddField("main_did", curDragonData.Tag);
            data.AddField("materials", array.ToString());

            NetworkManager.Send("dragon/transcendence", data, ResponseTranscendence, FailedTranscendence);
        }
        /// <summary> 결과값 받는 곳, 이미 Push DragonUpdate에서 초월상태 및 데이터는 갱신 완료됨. </summary>
        private void ResponseTranscendence(JObject jsonData)
        {
            if (SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
            {
                switch ((eApiResCode)jsonData["rs"].Value<int>())
                {
                    /** 강화 성공 */
                    case eApiResCode.OK:
                    {
                        Refresh();
                        successCallback();
                        TranscendenceResultPopup.OpenPopup(true, curDragonData);
                        
                        if(Town.TownDragonsDic.ContainsKey(curDragonData.Tag))
                        {
                            Town.TownDragonsDic[curDragonData.Tag].SetTranscendEffect(curDragonData.TranscendenceStep);
                        }
                    } break;
                    /** 강화 실패 */
                    case eApiResCode.DRA_TRANSCENDENCE_FAIL:
                    {
                        Refresh();
                        TranscendenceResultPopup.OpenPopup(false, curDragonData);
                    }
                    break;
                    /** 드래곤의 초월 조건 미달성 */
                    case eApiResCode.DRA_TRANSCENDENCE_UNDER_CONDITION:
                    {
                        ToastManager.On(StringData.GetStringByStrKey("town_floors_text_01")); // 레벨이 부족합니다.
                    } break;
                    /** 없는 드래곤을 초월하려고 함 */
                    case eApiResCode.DRA_NO_SUCH_DRAGON:
                    {
                        ToastManager.On(StringData.GetStringByIndex(100002548));//해당 드래곤을 보유하고 있지 않습니다.
                    } break;
                    default: break;
                }
            }

            IsTranscendence = false;
        }
        /// <summary> 네트워크 상태 등 기타의 의미로 통신 실패 </summary>
        private void FailedTranscendence(string message)
        {
            IsTranscendence = false;
        }

        public void OnClickAutoFill()
        {
            if (currentStep == StepMax)
            {
                ToastManager.On(StringData.GetStringByStrKey("최대초월토스트")); // 최대 단계입니다.
                return;
            }
            if (tableViewItemList.Count == 0)
            {
                ToastManager.On(StringData.GetStringByIndex(100002249));
                return;
            }
            autoCount = MaterialMax - selectedList.Count; // 자동 선택 가능 카운트
            dragonCardTableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) =>
            {
                if (node == null)
                    return;
                var frame = node.GetComponent<DragonCardFrame>();
                if (frame == null)
                    return;
                var data = (UserDragonCard)item;
                bool isSelect = selectedList.Contains(data);
                if(isSelect ==false && autoCount > 0 && isMaxPercentageState ==false)
                {
                    frame.InitCardFrame(data, true, true);
                    frame.SetSelect(true);
                    selectedList.Add(data);
                    --autoCount;
                    RefreshInfo();
                }
                else
                {
                    frame.InitCardFrame(data, isSelect, true);
                    frame.SetSelect(isSelect);
                }
                frame.ClickCallBack = (DragonCardFrame dragonFrame, UserDragonCard cardData) =>
                {
                    autoCount = 0;
                    if (dragonFrame.IsSelect)
                    {
                        selectedList.Remove(cardData);
                        dragonFrame.SetSelect(false);
                        RefreshInfo();
                    }
                    else
                    {
                        if (selectedList.Count >= MaterialMax || isMaxPercentageState)
                        {
                            ToastManager.On(StringData.GetStringByStrKey("재료선택불가")); // 더이상 선택 불가
                        }
                        else
                        {
                            selectedList.Add(cardData);
                            dragonFrame.SetSelect(true);
                            RefreshInfo();
                        }
                    }
                };
            }));
            dragonCardTableView.ReLoad();
            RefreshInfo();
        }

    }
}