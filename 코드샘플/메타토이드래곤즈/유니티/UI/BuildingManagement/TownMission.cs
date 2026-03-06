using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SandboxNetwork
{
	public class TownMission: MonoBehaviour
	{
		[SerializeField] Color normalBGColor;
		[SerializeField] Color completeBGColor;

		[SerializeField] Color normalToggleBoxColor;
		[SerializeField] Color completeToggleBoxColor;

		[SerializeField] Image bg;
		[SerializeField] Toggle checker;
		[SerializeField] Text missionLabel;
		[SerializeField] GameObject missionButtonNode;

		private QuestConditionData townMissionData = null;

        public void Init()
		{
			SetData(StringData.GetStringByIndex(100000326),true);
		}
		private void SetData(string text, bool isClear = false)
		{
			bg.color = isClear ? completeBGColor : normalBGColor;
			if(missionLabel != null)
				missionLabel.text = text;
			if(checker != null)
				checker.isOn = isClear;
			if(missionButtonNode != null)
				missionButtonNode?.SetActive(!isClear);
			var toggleImage = checker.GetComponent<Image>();
			if (toggleImage != null)
				toggleImage.color = isClear ? completeToggleBoxColor : normalToggleBoxColor;
		}
		public void Init(QuestConditionData _progressData)
		{
			SetData(_progressData);
			SetUI();
		}
		private void SetData(QuestConditionData _progressData)
		{
			townMissionData = _progressData;
		}
		private void SetUI()
		{
			if (townMissionData == null)
			{
				Debug.LogError("미션 데이터 누락" + townMissionData);
				return;
			}

			var desc = townMissionData.GetDesc();
			var isClear = townMissionData.IsQuestClear();

			bg.color = isClear ? completeBGColor : normalBGColor;
			if (missionLabel != null)
				missionLabel.text = desc;
			if (checker != null)
				checker.isOn = isClear;
			if (missionButtonNode != null)
            {
				missionButtonNode?.SetActive(!isClear);
				missionButtonNode.GetComponent<Button>()?.SetButtonSpriteState(IsAvailableDirect());
			}
			var toggleImage = checker.GetComponent<Image>();
			if (toggleImage != null)
				toggleImage.color = isClear ? completeToggleBoxColor : normalToggleBoxColor;
		}

		bool IsAvailableDirect()//바로가기 기능이 가능한지
        {
			return QuestManager.Instance.IsAvailableDirect(townMissionData.TriggerData);
        }
		public void OnClickDirectMission()//바로 가기 기능
        {
            if (!IsAvailableDirect())
            {
				ToastManager.On(StringData.GetStringByStrKey("퀘스트바로가기오류"));
				return;
            }

			GotoTarget();
		}
		public void GotoTarget()
		{
			if(townMissionData == null)
            {
				Debug.LogError("타운 미션 데이터 누락" + townMissionData);
				return;
            }
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            var triggerData = townMissionData.TriggerData;
			if (triggerData != null)
				QuestManager.Instance.DirectGotoTarget(triggerData);
		}
	}
}