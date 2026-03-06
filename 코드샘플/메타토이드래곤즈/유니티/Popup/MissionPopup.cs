using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork { 
    public class MissionPopup : Popup<TabTypePopupData>
    {
        public enum eQuestTabType
        {
            DAILYTAB,
            WEEKLYTAB,
            EVENTTAB,
        }

        [SerializeField] 
        private TabLayer[] TabLayers = null;
        [SerializeField]
        private Button[] TabButtons = null;

        [SerializeField]
        private GameObject[] Reddots = null;

        private int curTabNum = -1;
        void SetSubCamTextureOn()
        {
            Town.Instance?.SetSubCamState(true);
            UICanvas.Instance.StartBackgroundBlurEffect();
        }

        void SetSubCamTextureOff()
        {
            Town.Instance?.SetSubCamState(false);
            UICanvas.Instance.EndBackgroundBlurEffect();
        }
        public override void ForceUpdate(TabTypePopupData data = null)
        {
            base.ForceUpdate(data);
            if (curTabNum == Data.TabIndex) return;
            curTabNum = Data.TabIndex;
            ClearTab();
            if (TabLayers != null)
            {
                if(TabLayers.Count() > Data.TabIndex && TabLayers[Data.TabIndex] != null)
                {
                    TabLayers[curTabNum].gameObject.SetActive(true);
                    TabLayers[curTabNum].RefreshUI();
                    TabButtons[curTabNum].interactable = false;
                    RefreshReddot();
                }
            }
        }

        public void OnClickTab(int layerNumber)
        {
            ForceUpdate(new TabTypePopupData(layerNumber,0));
        }

        public override void InitUI()
        {
            ClearTab();
            curTabNum = Data.TabIndex;
            TabLayers[curTabNum].gameObject.SetActive(true);
            TabLayers[curTabNum].RefreshUI();
            TabButtons[curTabNum].interactable = false;
            RefreshReddot();
            
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);

            SetSubCamTextureOn();
        }

        void ClearTab()
        {
            foreach (var layer in TabLayers)
            {
                if (layer != null)
                {
                    layer.gameObject.SetActive(false);
                }
            }
            foreach (var button in TabButtons)
            {
                button.interactable = true;
            }
        }

        void RefreshReddot()//현재의 퀘스트 진행 상태를 보고 보상 받을 수 있는 상태체크
        {
            SetAllVisibleReddot(false);

            var currentQuestTab = (eQuestTabType)curTabNum;
            eQuestType currentType = eQuestType.NONE;
            switch (currentQuestTab)
            {
                case eQuestTabType.DAILYTAB:
                    currentType = eQuestType.DAILY;
                    break;
                case eQuestTabType.WEEKLYTAB:
                    currentType = eQuestType.WEEKLY;
                    break;
                case eQuestTabType.EVENTTAB:
                    currentType = eQuestType.EVENT;
                    break;
            }

            var questList = QuestManager.Instance.GetProceedUIData(currentType, eQuestGroup.Normal);
            if (questList == null || questList.Count <= 0)
                return;

            bool isClearQuest = false;
            foreach (var questData in questList)//보상 받을 수 있는 상태(퀘 클)하나라도 있으면
            {
                if (questData.IsQuestClear())
                {
                    isClearQuest = true;
                    break;
                }
            }

            SetVisibleSpecificReddot(curTabNum, isClearQuest);
        }

        void SetAllVisibleReddot(bool _isVisible)
        {
            if (Reddots == null || Reddots.Length <= 0)
                return;

            foreach(var reddot in Reddots)
            {
                if (reddot == null)
                    continue;
                reddot.SetActive(_isVisible);
            }
        }

        void SetVisibleSpecificReddot(int _index, bool _isVisible)
        {
            if (Reddots == null || Reddots.Length <= 0 || Reddots.Length <= _index)
                return;

            Reddots[_index].SetActive(_isVisible);
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);

            SetSubCamTextureOff();
        }
    }
}