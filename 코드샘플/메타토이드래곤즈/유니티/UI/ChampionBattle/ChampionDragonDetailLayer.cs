using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SandboxNetwork.UITrainStateEvent;

namespace SandboxNetwork
{
    public interface IChampionDragonDetailSubPanel
    {
        void ShowPanel(VoidDelegate _successCallback = null);
        void HidePanel();
        void Init();

        void SetData();
    }

    public abstract class ChampionDragonDetailSubPanel : MonoBehaviour, IChampionDragonDetailSubPanel
    {
        protected VoidDelegate successCallback = null;
        protected bool isSubPopupOpen = false;
        public bool IsSubPopupOpen { get { return isSubPopupOpen; }}

        protected int dragonTag = -1;

        protected CharBaseData dragonBase = null;
        protected ChampionDragonDetailPopup ParentPopup { get { return PopupManager.GetPopup<ChampionDragonDetailPopup>(); } }
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

    public enum eChampionDragonDetailSubPopupType
    {
        STATUS,
        STORY,
        SKILLDESC,
        PASSIVE_SKILL,
        PARTINFO,
    }
    public class ChampionDragonDetailLayer : MonoBehaviour
    {
        [SerializeField]
        private ChampionBattleDragonSelectTabLayer DragonTap = null;

        [SerializeField]
        List<ChampionDragonDetailSubPanel> subPopupList = new List<ChampionDragonDetailSubPanel>();

        [SerializeField]
        ChampionDragonDetailStatPanel dragonStat = null;

        [SerializeField]
        ChampionDragonDetailDescPanel dragonDesc = null;

        [SerializeField]
        ChampionDragonDetailSkillIconPanel dragonSkillDesc = null;

        [SerializeField]
        ChampionDragonDetailPassivePanel dragonPassiveSlot = null;

        [SerializeField]
        ChampionDragonDetailPartIconPanel dragonEquipSlot = null;

        [SerializeField]
        ChampionDragonDetailPetIconPanel dragonPetIconSlot = null;

        [SerializeField]
        ChampionDragonDetailPartInfoPanel dragonPartInfoSlot = null;

        [SerializeField]
        ChampionDragonDetailPetInfoLayer dragonPetInfoSlot = null;

        [SerializeField]
        Button dragonAcquisitionButton = null;//미획득 드래곤 획득하러가기 버튼

        [SerializeField]
        Button backBtn = null;

        [SerializeField] GameObject arrowNode = null;
        ChampionDragonDetailPopup ParentPopup { get { return PopupManager.GetPopup<ChampionDragonDetailPopup>(); } }
        public eChampionDragonDetailSubPopupType CurrentSubPopupType { get; private set; }

        private void OnDisable()
        {
            ResetAllSubPopupOpenFlag();
        }

        public void Init()
        {
            InitAllSubPopup();
            RefreshDragonStat();
            RefreshDragonDesc();
            RefreshDragonPartSlot();
            RefreshDragonSkillDesc();
            RefreshDragonPetIcon();
            RefreshDragonPartInfo();
            RefreshDragonPetInfo();
            RefreshDragonPassiveSlot();
        }
        public void ForceUpdate()
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
                dragonDesc.ShowDescPanel();
            }
        }
        void RefreshDragonSkillDesc()//이부분 팝업 화 될 예정
        {
            if (dragonSkillDesc != null)
            {
                dragonSkillDesc.Init();
            }
        }
        void RefreshDragonPetIcon()
        {
            if (dragonPetIconSlot != null)
            {
                dragonPetIconSlot.Init();
            }
        }

        void RefreshDragonPartInfo()
        {
            if (dragonPartInfoSlot != null)
            {
                dragonPartInfoSlot.HideDetailInfo();
            }
        }
        
        void RefreshDragonPetInfo()
        {
            if (dragonPetInfoSlot != null)
            {
                dragonPetInfoSlot.Init();
                dragonPetInfoSlot.HidePetInfoPanel();
            }
        }

        public void OnClickPetInfo()
        {
            if (dragonPetInfoSlot != null) {
                dragonPetInfoSlot.ShowPetInfoPanel();
                dragonDesc.HideDescPanel();
            }
        }

        public void OnClickExitPetInfo()
        {
            if (dragonPetInfoSlot != null)
            {
                dragonPetInfoSlot.HidePetInfoPanel();
                dragonDesc.ShowDescPanel();
            }
        }

        /// <summary>
        /// 드래곤 획득 상태면 장비세팅 보이고, 아니면 획득 버튼 보이기
        /// </summary>
        void RefreshCheckDragonAcquisition()
        {
            //var curDragonTag = PopupManager.GetPopup<DragonManagePopup>().DragonTagInfo;
            //var designData = CharBaseData.Get(curDragonTag);
            //if (designData == null)
            //    return;

            //var hasDragon = User.Instance.DragonData.IsUserDragon(curDragonTag);//보유 드래곤
            //var isIncomeLevelPass = PopupManager.GetPopup<DragonManagePopup>().IncomeLevelPassFlag;
            //var isLevelPassDragon = designData.IS_LEVEL_PASS_REWARD;

            //if(isIncomeLevelPass)
            //    dragonAcquisitionButton.gameObject.SetActive(!isLevelPassDragon);
            //else
            //    dragonAcquisitionButton.gameObject.SetActive(!hasDragon);
                    
            dragonEquipSlot.gameObject.SetActive(true);
        }

        void RefreshDragonPassiveSlot()
        {
            if (dragonPassiveSlot != null)
            {
                dragonPassiveSlot.Init();
            }
        }

        void RefreshDragonPartSlot()
        {
            if (dragonEquipSlot != null)
            {
                dragonEquipSlot.Init();
            }
        }
        #region 레벨업, 스킬 팝업 제어
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

            if(GetCurrentOpenPopup() >= 0)
                SetPartIconSlotDimmed(true);
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

        public bool IsPetInfoPanel()
        {
            if (dragonPetInfoSlot.IsActive())
                return true;

            return false;
        }

        public void ShowPartInfoPanel(int _index)
        {
            if (!ParentPopup.Dragon.ChampionPart.ContainsKey(_index) || ParentPopup.Dragon.ChampionPart[_index] == null) return;
            
            var partID = ParentPopup.Dragon.ChampionPart[_index].ID;
            var tag = ParentPopup.Dragon.ChampionPart[_index].Tag;
            dragonPartInfoSlot.ShowDetailInfo(partID, tag);
        }

        public void OnClickShowSubPopup(int _index)
        {
            AllHideSubPopup();
            OnClickExitPetInfo();

            var curDragonTag = ParentPopup.DragonTag;
            //var hasDragon = User.Instance.DragonData.IsUserDragon(curDragonTag);
            var hasDragon = true;
            if (curDragonTag <= 0)
                return;
            SetVisibleArrowNode(true);
            CurrentSubPopupType = (eChampionDragonDetailSubPopupType)_index;
            switch(CurrentSubPopupType)
            {
                case eChampionDragonDetailSubPopupType.STORY:

                    var designData = CharBaseData.Get(curDragonTag);
                    if (designData == null)
                        return;

                    if (designData._DESC == "0")
                    {
                        ToastManager.On(StringData.GetStringByStrKey("스토리없는드래곤"));
                        return;
                    }
                    break;
                case eChampionDragonDetailSubPopupType.STATUS:
                    if (!hasDragon)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("미획득드래곤알림"));
                        return;
                    }
                    break;
                case eChampionDragonDetailSubPopupType.PASSIVE_SKILL:
                    if (!hasDragon)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("미획득드래곤알림"));
                        return;
                    }
                    SetVisibleArrowNode(false);
                    var dragon = ChampionManager.Instance.MyInfo.ChampionDragons.GetDragon(curDragonTag);
                    int minimumStep = CharTranscendenceData.GetNewSkillSlotMinimumStep((eDragonGrade)dragon.Grade());
                    if (dragon.IsTranscendenceAble()==false)
                    {
                        ToastManager.On(StringData.GetStringFormatByStrKey("스킬슬롯잠김", minimumStep));
                        return;
                    }
                    break;
            }

            SetShowSubPopup(_index, () =>
            {
                RefreshDragonStat();
                RefreshDragonDesc();

            });
        }

        public void SetVisibleArrowNode(bool _isVisible)
        {
            var popup = ParentPopup;
            if (popup == null)
                return;

            var dragonmanageList = popup.DragonInfoList;
            if (dragonmanageList == null)
                return;

            bool isSingleList = dragonmanageList.Count <= 1;

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
        }

        void SetHideSubPopup(int _panelIndex)
        {
            SetSubPopupOpenFlag(_panelIndex, false);
            subPopupList[_panelIndex]?.HidePanel();
            SetPartIconSlotDimmed(false);
            switch ((eChampionDragonDetailSubPopupType)_panelIndex)
            {
                case eChampionDragonDetailSubPopupType.PASSIVE_SKILL:
                {
                    RefreshDragonPassiveSlot();
                    SetVisibleArrowNode(true);
                }
                break;
            }
        }
        void SetShowSubPopup(int _panelIndex, VoidDelegate _SuccessCallback)
        {
            SetSubPopupOpenFlag(_panelIndex, true);
            subPopupList[_panelIndex]?.ShowPanel(_SuccessCallback);
            SetPartIconSlotDimmed(true);
        }

        void SetSubPopupOpenFlag(int _panelIndex, bool _isOpen)
        {
            subPopupList[_panelIndex]?.SetPopupOpenFlag(_isOpen);
        }

        void SetPartIconSlotDimmed(bool _isDimmed)
        {
            if (dragonEquipSlot != null)
                dragonEquipSlot.SetALLPartIconDimmed(_isDimmed);
        }

        #endregion
        public bool backBtnCall()
        {
            PopupManager.ClosePopup<ChampionDragonDetailPopup>();
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

        public void OnClickDragonAcquisition()
        {
            var curDragonTag = ParentPopup.DragonTag;
            if (curDragonTag <= 0)
                return;

            var designData = CharBaseData.Get(curDragonTag);
            if (designData == null)
                return;

            //var hasDragon = User.Instance.DragonData.IsUserDragon(curDragonTag);
            var hasDragon = true;
            if (designData.IS_SCENARIO)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("미보유시나리오드래곤"));
                return;
            }

            if (designData.IS_CASH)
            {

                // char base에 로우를 늘리자니 겨우 한마리를 위해서 로우를 추가하는 건 미친 짓이고
                // 사실 현재 방법도 미친 짓이지만 딱히 떠오르는 대안이 없음
                int groupID = ShopGoodsData.GetKeyBySpecificAsset(eGoodType.CHARACTER, designData.KEY);

                var popup = PopupManager.OpenPopup<ConditionalBuyPopup>(new ConditionBuyData(groupID));
                popup.SetRewardCallBack(() =>
                {
                    PopupManager.GetPopup<ShopPopup>().RefreshCurrentMenu();
                    popup.ClosePopup();
                });
                return;
            }

            if(designData.IS_LEVEL_PASS_REWARD)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("레벨패스드래곤획득문구"), () => {
                    PopupManager.AllClosePopup();
                    LevelPassPopup.OpenPopup();
                },
                   () =>
                   {
                    //나가기
                },
                   () =>
                   {
                    //나가기
                }
               );
                return;
            }

            if (designData.IS_GUILD_SHOP_REWARD)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("드래곤리스트미습득2"), () => {

                    var isNoGuild = GuildManager.Instance.IsNoneGuild;
                    if(isNoGuild)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("조합드래곤획득토스트문구"));
                    }
                    else
                    {
                        PopupManager.AllClosePopup();
                        GuildManager.Instance.OpenGuild();
                    }
                },
                   () =>
                   {
                       //나가기
                   },
                   () =>
                   {
                       //나가기
                   }
               );
                return;
            }

            if (designData.GRADE >= (int)eDragonGrade.Legend)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("미보유드래곤_합성"), () => {
                    DragonTap?.moveLayer(3);
                },
                () =>
                {
                        //나가기
                    },
                () =>
                {
                        //나가기
                    }
            );
                return;
            }

            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("미보유드래곤"),
                    () =>
                    {
                        PopupManager.AllClosePopup();//가챠 씬이동
                                SBFunc.MoveGachaScene();
                    },
                    () => { },
                    () => { }
                );
        }
    }
}
