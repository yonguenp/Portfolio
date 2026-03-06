using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace SandboxNetwork
{
	public class QuestIcon: MonoBehaviour
	{
		[SerializeField] private Image reddot;
		[SerializeField] private Image icon;
		[SerializeField] private Text labelProgress;

		[SerializeField] private Sprite newSprite;
		[SerializeField] private Sprite CompleteSprite;
		[SerializeField] private Image progressFillImg=null;
		
		private int _qID;

		public void Start()
		{
			
		}

		public void Init(int qID)
		{
			_qID = qID;
			QuestData qData = QuestData.Get(qID.ToString());
			Quest quest = QuestManager.Instance.GetQuest(qID);
			int require, value;
			bool isClear = false;

			icon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.QuestIconPath, 
				SBFunc.StrBuilder(quest.Type == eQuestType.MAIN ? "main_" : "sub_", qData.ICON, "_icon"));

			if(!quest.IsSigleCondition)//todo 완료 목표가 다수 일 때의 노티 방식
			{
				require = quest.Conditions.Count;
				value = quest.GetQuestConditionClearCount();//완료한 목표 수
			}
			else
			{
				QuestConditionData cond = quest.GetSingleConditionData();
				require = cond.CompleteValue;
				value = cond.CurrentValue;
				isClear = cond.IsQuestClear();
			}

			labelProgress.text = SBFunc.StrBuilder(value, " / ", require);
            progressFillImg.fillAmount = value / (float)require;

            

			if(quest.State == eQuestState.PROCEEDING && !isClear)
			{
				reddot.gameObject.SetActive(false);
			}
			else
			{
				reddot.gameObject.SetActive(true);
				reddot.sprite = quest.IsNewQuest() ? newSprite : CompleteSprite;
			}
		}

		public void OpenQuestPopup()
		{
			OnClickQuestIcon(true);
		}

		void SetStartEvent()
		{
			QuestEvent.Event(QuestEvent.eEvent.QUEST_START, _qID);
		}

		public void OnClickQuestIcon(bool autoPopup = false)
		{
			CacheUserData.SetInt("LastOpenedMainQuest", _qID);

			PopupManager.OpenPopup<QuestPopup>(new QuestPopupData(_qID)/*, false*/);
			var questData = QuestManager.Instance.GetQuest(_qID);
			if(questData != null && questData.IsNewQuest())
            {
				if (autoPopup)
				{
					Invoke("SetStartEvent", 0.1f);
				}

				QuestManager.Instance.ReadReddot(_qID);
				Init(_qID);
				QuestEvent.Event(QuestEvent.eEvent.QUEST_UPDATE);
			}

			ScenarioManager.Instance.OnCloseGuide();
		}
	}
}
