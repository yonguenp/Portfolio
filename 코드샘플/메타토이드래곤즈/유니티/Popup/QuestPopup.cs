using Com.LuisPedroFonseca.ProCamera2D;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SandboxNetwork
{
	public class QuestPopup: Popup<QuestPopupData>
	{
		const int LAYOUT_ENABLE_COUNT = 3;
		readonly private int SPACING_BETWEEN_ITEMFRAME = 6;
		readonly private int ITEMFRAME_WIDTH = 80;
		readonly private int MAX_SCROLLVIEW_WIDTH = 370;

		[SerializeField] private TableView tableView = null;
		[SerializeField] private ScrollRect tableRect = null;
		[SerializeField] private Text questProceedText;
		[SerializeField] private Text questSubjectText;
		[SerializeField] private Text questBodyText;
		[SerializeField] private Image questIcon;
		[SerializeField] private GameObject itemClone;
		[SerializeField] private Button rewardBtn;
		[SerializeField] private Text rewardBtnText;
		[SerializeField] private Animation stamp;
		[SerializeField] private Button closeBtn;
		[SerializeField] private HorizontalLayoutGroup contentLayoutGroup = null;


		private ScrollRect scrollView;
		private QuestData questTableData;
		private Quest quest;

		bool isClicked = false;

		bool isTableInit = false;

		bool isTutorialing101 = false;
		bool isTutorialing102 = false;
		bool isTutorialing105 = false;

		public override void InitUI()
		{
			isClicked = false;
			quest = Data.questData;
			questTableData = QuestData.Get(quest.GetKey());

			questSubjectText.text = StringData.GetStringByStrKey(questTableData._SUBJECT);
			questBodyText.text = quest.GetQuestDesc(false);
			if(quest.IsSigleCondition)
            {
				var conditionData = quest.GetSingleConditionData();
				questProceedText.text = SBFunc.StrBuilder(conditionData.CurrentValue, "/", conditionData.CompleteValue);
			}
				
			questIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.QuestIconPath, 
				SBFunc.StrBuilder(quest.Type == eQuestType.MAIN ? "main_" : "sub_", questTableData.ICON, "_icon"));

			//if(!isTableInit && tableView != null)
			//{
			//	tableView.OnStart();
			//	isTableInit = true;
			//}

			//DrawScrollView();
			DrawUnityScrollView();

			if (quest.IsQuestClear())
			{
				closeBtn.gameObject.SetActive(false);
				rewardBtnText.text = StringData.GetStringByIndex(100000413);
				stamp.gameObject.SetActive(true);
				stamp.Play();
			}
			else
			{
				closeBtn.gameObject.SetActive(true);
				stamp.gameObject.SetActive(false);
				rewardBtnText.text = StringData.GetStringByIndex(100000414);
			}
			isTutorialing101 = TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Construct);
            isTutorialing102 = TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Product);
			isTutorialing105 = TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.DragonGacha);
            if (isTutorialing101 || isTutorialing105)
                TutorialManager.tutorialManagement.NextTutorialStart();
            rewardBtn.SetButtonSpriteState(!isTutorialing101);
            
        }

		void DrawUnityScrollView()
        {
			scrollView = GetComponentInChildren<ScrollRect>();
			SBFunc.RemoveAllChildrens(scrollView.content);

			List<Asset> rewards = questTableData.REWARDS;
			for (int i = 0; i < rewards.Count; i++)
			{
				GameObject clone = Instantiate(itemClone, scrollView.content);
				clone.GetComponent<ItemFrame>().SetFrameItemInfo(rewards[i].ItemNo, rewards[i].Amount);
			}

			var isLayoutEnable = rewards.Count <= LAYOUT_ENABLE_COUNT;

			if (isLayoutEnable)//pivot 센터
			{
				scrollView.content.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
				scrollView.content.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
				scrollView.content.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
				scrollView.content.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
			}
			else
			{
				scrollView.content.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
				scrollView.content.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
				scrollView.content.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
				scrollView.content.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

				if (scrollView != null)
					scrollView.horizontalNormalizedPosition = 0f;
			}
		}

		void DrawScrollView()
        {
			List<ITableData> tableViewItemList = new List<ITableData>();
			tableViewItemList.Clear();
			List<Asset> rewards = questTableData.REWARDS;
			if (rewards != null && rewards.Count > 0)
			{
				for (var i = 0; i < rewards.Count; i++)
				{
					var data = rewards[i];
					if (data == null)
						continue;

					tableViewItemList.Add(data);
				}
			}

			var isLayoutEnable = rewards.Count <= LAYOUT_ENABLE_COUNT;
			contentLayoutGroup.enabled = isLayoutEnable;

			tableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
				if (node == null)
					return;

				var frame = node.GetComponent<ItemFrame>();
				if (frame == null)
					return;

				var rewardData = (Asset)item;
				frame.GetComponent<ItemFrame>().SetFrameItemInfo(rewardData.ItemNo, rewardData.Amount);
			}));

			tableView.ReLoad();

			if(isLayoutEnable) // 아이템이 3개인 경우에만 중앙에 배치, 나머지의 경우 왼쪽부터 순차적으로 배치
				tableRect.content.sizeDelta = tableRect.viewport.sizeDelta;
		}

		public void OnClickBtn()
		{
			if (isTutorialing101 || isTutorialing105)
                return;
			if (quest.IsQuestClear() && !isClicked)
			{
				//WJ - 2023.09.12 인벤토리가 다 찼을 경우에는 서버쪽에서 우편으로 보내줌.
				//var expectReward = quest.GetReward().ToList();
				//if (User.Instance.CheckInventoryGetItem(expectReward))
				//{
				//	SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
				//		() => {
				//			PopupManager.ClosePopup<QuestPopup>();
				//			PopupManager.OpenPopup<InventoryPopup>();
				//		}
				//	);
				//	return;
				//}
				//else
				//{
				//	isClicked = true;
				//	QuestManager.Instance.RequestQuestComplete(quest.ID, () => {
				//		isClicked = false;
				//	});
				//}
				isClicked = true;
				QuestManager.Instance.RequestQuestComplete(quest.ID, () => {
					isClicked = false;
				});
			}
			else
			{
				var triggerData = quest.GetSingleTriggerData();
				if (triggerData != null)
					QuestManager.Instance.DirectGotoTarget(triggerData);
			}
			PopupManager.ClosePopup<QuestPopup>();
		}
		

		public override void BackButton()
		{
			if(quest.IsQuestClear())
				return;
            base.BackButton();
		}
		public override void OnClickDimd()
		{
			if (quest.IsQuestClear())
				return;
			base.OnClickDimd();
		}
        public override void ClosePopup()
        {
            base.ClosePopup();
        }
    }
}
