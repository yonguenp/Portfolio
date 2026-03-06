using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class BattlePassMisionObj : MonoBehaviour
    {
        [SerializeField]
        Text missionText;
        [SerializeField]
        Text missonCountText;
        [SerializeField]    
        Text missionPointText;
        [SerializeField]
        Button shortCutBtn;
        [SerializeField]
        Sprite shortCutAbleSprtie;
        [SerializeField]
        Sprite shortCutDisableSprite;

        [SerializeField]
        GameObject moveAbleLayer;
        [SerializeField]
        GameObject moveDisableLayer;
        [SerializeField]
        GameObject clearLayer;

        [SerializeField]
        Image bgImg = null;
        [SerializeField]
        Color selectedColor = Color.white;
        [SerializeField]
        Color noneSelectedColor = Color.white;

        [SerializeField]
        Slider MissionProgressingSlider = null;


        [SerializeField]
        GameObject dimLayer = null;


        private QuestTriggerData triggerData = null;
        public void Init(Quest quest)
        {
            bool isRewardAble = IsGetRewardCondition(quest);
            bool isClear = quest.IsQuestClear() || quest.State == eQuestState.TERMINATE;
            triggerData = quest.GetSingleTriggerData();
            var condition = quest.GetSingleConditionData();
            missionText.text = StringData.GetStringByStrKey(triggerData._NOTIE);
            missonCountText.text = string.Format("{0}/{1}", isClear ? condition.CompleteValue : condition.CurrentValue,condition.CompleteValue);
            MissionProgressingSlider.maxValue = condition.CompleteValue;
            MissionProgressingSlider.value = condition.CurrentValue;
            //MissionProgressBarImg.sprite = condition.CompleteValue > condition.CurrentValue ? missionProgressingSprite : missionDoneSprite;
            missionPointText.text = quest.GetReward()[0].Amount.ToString();
            SetClearState(isClear, isRewardAble);
        }
        bool IsGetRewardCondition(Quest _quest)//보상을 받을 수 있는 상태인가?
        {
            if (_quest == null)
                return false;

            if (_quest.State == eQuestState.TERMINATE || _quest.State == eQuestState.PROCESS_DONE)//이미 완료한 퀘스트
                return false;

            if (_quest.IsQuestClear())//진행 중인 퀘스트만 체크
                return true;

            return false;
        }

        public void OnClickDirectShortCut()
        {
            if(triggerData != null)
            {
                if (QuestManager.Instance.IsAvailableDirect(triggerData))
                {
                    PopupManager.ClosePopup<BattlePassPopup>();
                    PopupManager.ClosePopup<HolderPassPopup>();
                    QuestManager.Instance.DirectGotoTarget(triggerData);
                }
            }
        }

        void SetClearState(bool isClear,bool isRewardAble)
        {
            moveAbleLayer.SetActive(false);
            moveDisableLayer.SetActive(false);
            clearLayer.SetActive(false);
            dimLayer.SetActive(false);
            bgImg.color = noneSelectedColor;
            if (isClear && !isRewardAble) // 클리어 및 패스 경험치 수령 완료
            {
                shortCutBtn.interactable = false;
                shortCutBtn.SetButtonSpriteState(false);
                clearLayer.SetActive(true);
                //bgImg.color = selectedColor;
                MissionProgressingSlider.value = MissionProgressingSlider.maxValue =1;
               // MissionProgressBarImg.sprite = missionDoneSprite;
                dimLayer.SetActive(true);
            }
            else if(isClear && isRewardAble)  // 보상 수령 가능
            {
                shortCutBtn.interactable = false;
                shortCutBtn.SetButtonSpriteState(true);
                clearLayer.SetActive(true);
                bgImg.color = selectedColor;
            }
            else
            {
                shortCutBtn.interactable = true;
                bool isMoveAble = QuestManager.Instance.IsAvailableDirect(triggerData);
                
                if (isMoveAble)
                {
                    moveAbleLayer.SetActive(true);
                    shortCutBtn.GetComponent<Image>().sprite = shortCutAbleSprtie;
                }
                else
                {
                    shortCutBtn.GetComponent<Image>().sprite = shortCutDisableSprite;
                    moveDisableLayer.SetActive(true);
                }                    
            }
        }
    }

}
