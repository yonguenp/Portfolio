using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChampionLeagueTable;

namespace SandboxNetwork
{
    public interface IChampionBattleDragonSelectSubPanel
    {
        void ShowPanel(VoidDelegate _successCallback = null);
        void HidePanel();
        void Init();

        void SetData();
    }

    public abstract class ChampionBattleDragonSelectSubPanel : MonoBehaviour, IChampionBattleDragonSelectSubPanel
    {
        protected VoidDelegate successCallback = null;
        protected bool isSubPopupOpen = false;
        public bool IsSubPopupOpen { get { return isSubPopupOpen; }}

        protected int dragonTag = -1;

        protected CharBaseData dragonBase = null;

        ChampionBattleDragonSelectPopup ParentPopup { get { return PopupManager.GetPopup<ChampionBattleDragonSelectPopup>(); } }

        public virtual void HidePanel()
        {
            gameObject.SetActive(false);
        }

        public virtual void ShowPanel(VoidDelegate _successCallback = null)
        {
            if (successCallback == null)
                successCallback = _successCallback;

            Init();
            gameObject.SetActive(true);
        }

        public virtual void Init()
        {
            SetData();
        }
        public void SetData()
        {
            dragonTag = ParentPopup.DragonTag;
            dragonBase = CharBaseData.Get(dragonTag);
        }

        public virtual void ForceUpdate()
        {
            Init();
        }

        public void SetPopupOpenFlag(bool _isOpen)
        {
            isSubPopupOpen = _isOpen;
        }
    }

    public enum eChampionBattleDragonSelectInfoSubPopupType
    {
        STORY,
        SKILLDESC,
        MAX,
    }
    public class ChampionBattleDragonSelectInfoLayer : SubLayer
    {
        [SerializeField]
        private ChampionBattleDragonSelectTabLayer DragonTap = null;

        [SerializeField]
        List<ChampionBattleDragonSelectSubPanel> subPopupList = new List<ChampionBattleDragonSelectSubPanel>();

        [SerializeField]
        ChampionBattleDragonSelectStatPanel dragonStat = null;

        [SerializeField]
        ChampionBattleDragonSelectDescPanel dragonDesc = null;

        [SerializeField]
        ChampionBattleDragonSelectSkillIconPanel dragonSkillDesc = null;

        [SerializeField]
        Button dragonAcquisitionButton = null;//미획득 드래곤 획득하러가기 버튼

        [SerializeField]
        Button backBtn = null;

        [SerializeField] GameObject arrowNode = null;
        ChampionBattleDragonSelectPopup ParentPopup { get { return PopupManager.GetPopup<ChampionBattleDragonSelectPopup>(); } }
        public eChampionBattleDragonSelectInfoSubPopupType CurrentSubPopupType { get; private set; }

        private void OnDisable()
        {
            ResetAllSubPopupOpenFlag();
        }

        public override void Init()
        {
            InitAllSubPopup();
            RefreshDragonStat();
            RefreshDragonDesc();
            RefreshDragonSkillDesc();
        }
        public override void ForceUpdate()
        {
            Init();
        }
        void RefreshDragonStat()//드래곤 스탯 UI (여기 레벨업 버튼 들어가있음)
        {
            if (dragonStat != null)
            {
                dragonStat.Init();
            }
        }
        void RefreshDragonDesc()//레벨라벨과 캐릭터 이미지(스파인) 등등 드래곤 속성 세팅 UI
        {
            if (dragonDesc != null)
            {
                dragonDesc.Init();
            }
        }
        void RefreshDragonSkillDesc()//이부분 팝업 화 될 예정
        {
            if (dragonSkillDesc != null)
            {
                dragonSkillDesc.Init();
            }
        }
        void InitAllSubPopup()
        {
            if (subPopupList == null || subPopupList.Count <= 0)
                return;

            for (int i = 0; i < subPopupList.Count; i++)
            {
                if (IsOpenSubPopup(i))
                {
                    OnClickShowSubPopup(i);
                    continue;
                }

                SetHideSubPopup(i);
            }
            SetShowSubPopup((int)eChampionBattleDragonSelectInfoSubPopupType.SKILLDESC, () => { });
        }

        void InitSpecificSubPopup(int _index)
        {
            if (IsOpenSubPopup(_index))
            {
                OnClickShowSubPopup(_index);
                return;
            }

            SetHideSubPopup(_index);
        }

        public void OnClickShowSubPopup(int _index)
        {
            AllHideSubPopup();

            var curDragonTag = ParentPopup.DragonTag;

            if (curDragonTag <= 0)
                return;
            SetVisibleArrowNode(true);
            CurrentSubPopupType = (eChampionBattleDragonSelectInfoSubPopupType)_index;
            switch(CurrentSubPopupType)
            {
                case eChampionBattleDragonSelectInfoSubPopupType.STORY:

                    var designData = CharBaseData.Get(curDragonTag);
                    if (designData == null)
                        return;

                    if (designData._DESC == "0")
                    {
                        ToastManager.On(StringData.GetStringByStrKey("스토리없는드래곤"));
                        return;
                    }
                    break;
            }
            SetShowSubPopup(_index, () => { });
            
        }

        public void SetVisibleArrowNode(bool _isVisible)
        {
            var popup = ParentPopup;
            if (popup == null)
                return;

            var dragonmanageList = popup.UserDragonData;
            if (dragonmanageList == null)
                return;

            bool isSingleList = dragonmanageList.GetAllUserDragonCount() <= 1;

            if (_isVisible)
                arrowNode.SetActive(isSingleList ? false : true);
            else
                arrowNode.SetActive(false);
        }

        public void OnClickHideSubPopup(int _index)
        {
            SetHideSubPopup(_index);
        }

        void ResetAllSubPopupOpenFlag()
        {
            foreach(var subPopup in subPopupList)
            {
                if (subPopup == null)
                    continue;
                subPopup.SetPopupOpenFlag(false);
            }
        }

        bool IsOpenSubPopup(int _panelIndex)
        {
            if (subPopupList.Count <= _panelIndex)
                return false;

            var listData = subPopupList[_panelIndex];
            if (listData != null)
                return listData.IsSubPopupOpen;
            else
                return false;
        }

        void AllHideSubPopup()
        {
            foreach(var subPopup in subPopupList)
            {
                if (subPopup == null)
                    continue;

                subPopup.SetPopupOpenFlag(false);
                subPopup.HidePanel();
            }
            SetShowSubPopup((int)eChampionBattleDragonSelectInfoSubPopupType.SKILLDESC, () => { });
        }

        void SetHideSubPopup(int _panelIndex)
        {
            SetSubPopupOpenFlag(_panelIndex, false);
            subPopupList[_panelIndex]?.HidePanel();
        }
        void SetShowSubPopup(int _panelIndex, VoidDelegate _SuccessCallback)
        {
            SetSubPopupOpenFlag(_panelIndex, true);
            subPopupList[_panelIndex]?.ShowPanel(_SuccessCallback);
        }

        void SetSubPopupOpenFlag(int _panelIndex, bool _isOpen)
        {
            subPopupList[_panelIndex]?.SetPopupOpenFlag(_isOpen);
        }

        public void OnClickAddBtn()
        {
            var curDragonTag = ParentPopup.DragonTag;
            if (!ParentPopup.SelectedDragonList.Contains(curDragonTag) &&
                !ParentPopup.isFull())
            {
                ParentPopup.SelectedDragonList.Add(curDragonTag);
                //ParentPopup.SelectedDragonList.Remove(curDragonTag);
            }
            backBtnCall();
        }
        
        public void OnClickRemoveBtn()
        {
            var curDragonTag = ParentPopup.DragonTag;
            if (ParentPopup.SelectedDragonList.Contains(curDragonTag))
            {
                ParentPopup.SelectedDragonList.Remove(curDragonTag);
            }
            else
            {
                //ParentPopup.SelectedDragonList.Add(curDragonTag);
            }
            backBtnCall();
        }




        public override bool backBtnCall()
        {
            DragonTap.moveLayer(0);
            return true;
        }

        int GetCurrentOpenPopup()
        {
            if (subPopupList == null || subPopupList.Count <= 0)
                return -1;

            for (int i = 0; i < subPopupList.Count; i++)
            {
                if (IsOpenSubPopup(i))
                    return i;
            }
            return -1;
        }

    }
}
