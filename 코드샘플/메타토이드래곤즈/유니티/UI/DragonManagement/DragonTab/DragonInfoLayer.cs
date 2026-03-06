using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public interface IDragonManageSubPanel
    {
        void ShowPanel(VoidDelegate _successCallback = null);
        void HidePanel();
        void Init();

        void SetData();
    }

    public abstract class DragonManageSubPanel : MonoBehaviour, IDragonManageSubPanel
    {
        protected VoidDelegate successCallback = null;
        protected bool isSubPopupOpen = false;
        public bool IsSubPopupOpen { get { return isSubPopupOpen; } }

        protected int dragonTag = -1;

        protected CharBaseData dragonBase = null;
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
            dragonTag = PopupManager.GetPopup<DragonManagePopup>().CurDragonTag;
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

    public enum eDragonInfoSubPopupType
    {
        STATUS,
        LEVELUP,
        SKILLUP,
        STORY,
        SKILLDESC,
        PASSIVE_SKILL,
        TRANSCENDENCE,
        COLLECTION,
        MAX,
    }
    public class DragonInfoLayer : SubLayer
    {
        [SerializeField]
        private DragonTabLayer DragonTap = null;

        [SerializeField]
        List<DragonManageSubPanel> subPopupList = new List<DragonManageSubPanel>();

        [SerializeField]
        DragonStatPanel dragonStat = null;

        [SerializeField]
        DragonDescPanel dragonDesc = null;

        [SerializeField]
        DragonSkillIconPanel dragonSkillDesc = null;

        [SerializeField]
        DragonPartIconPanel dragonEquipSlot = null;

        [SerializeField]
        DragonPetIconPanel dragonPetIconSlot = null;

        [SerializeField]
        Button dragonAcquisitionButton = null;//미획득 드래곤 획득하러가기 버튼

        [SerializeField]
        GameObject CollectionButton = null;

        [SerializeField]
        Button backBtn = null;

        [SerializeField] GameObject arrowNode = null;

        public eDragonInfoSubPopupType CurrentSubPopupType { get; private set; }

        private void OnDisable()
        {
            ResetAllSubPopupOpenFlag();
        }

        private void OnEnable()
        {
            if (CollectionButton != null)
            {
                CollectionButton.SetActive(true);
            }
        }

        public override void Init()
        {
            InitAllSubPopup();
            RefreshDragonStat();
            RefreshDragonDesc();
            RefreshCheckDragonAcquisition();
            RefreshDragonPartSlot();
            RefreshDragonSkillDesc();
            RefreshDragonPetIcon();
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
        void RefreshDragonPetIcon()
        {
            if (dragonPetIconSlot != null)
            {
                dragonPetIconSlot.Init();
            }
        }

        /// <summary>
        /// 드래곤 획득 상태면 장비세팅 보이고, 아니면 획득 버튼 보이기
        /// </summary>
        void RefreshCheckDragonAcquisition()
        {
            var curDragonTag = PopupManager.GetPopup<DragonManagePopup>().CurDragonTag;
            var hasDragon = User.Instance.DragonData.IsUserDragon(curDragonTag);//보유 드래곤

            dragonAcquisitionButton.gameObject.SetActive(!hasDragon);
            dragonEquipSlot.gameObject.SetActive(hasDragon);

            if (!hasDragon)
            {
                dragonAcquisitionButton.interactable = true;

                var designData = CharBaseData.Get(curDragonTag);
                if (designData == null)
                    return;

                if (designData.IS_GUILD_SHOP_REWARD && PopupManager.IsPopupOpening(PopupManager.GetPopup<GuildShopPopup>()))
                {
                    dragonAcquisitionButton.interactable = false;
                }
                if (designData.IS_LEVEL_PASS_REWARD && PopupManager.IsPopupOpening(PopupManager.GetPopup<LevelPassPopup>()))
                {
                    dragonAcquisitionButton.interactable = false;
                }
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

            if (GetCurrentOpenPopup() >= 0)
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

        public void OnClickShowSubPopup(int _index)
        {
            AllHideSubPopup();

            var curDragonTag = PopupManager.GetPopup<DragonManagePopup>().CurDragonTag;
            var hasDragon = User.Instance.DragonData.IsUserDragon(curDragonTag);

            if (curDragonTag <= 0)
                return;
            SetVisibleArrowNode(true);
            CurrentSubPopupType = (eDragonInfoSubPopupType)_index;
            switch (CurrentSubPopupType)
            {
                case eDragonInfoSubPopupType.STORY:

                    var designData = CharBaseData.Get(curDragonTag);
                    if (designData == null)
                        return;

                    if (designData._DESC == "0")
                    {
                        ToastManager.On(StringData.GetStringByStrKey("스토리없는드래곤"));
                        return;
                    }
                    break;
                case eDragonInfoSubPopupType.SKILLUP:
                    if (!hasDragon)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("미획득드래곤알림"));
                        return;
                    }
                    if (User.Instance.DragonData.GetDragon(curDragonTag).SLevel == GameConfigTable.GetSkillLevelMax())
                    {
                        ToastManager.On(StringData.GetStringByStrKey("스킬레벨업불가"));
                        return;
                    }
                    break;
                case eDragonInfoSubPopupType.LEVELUP:
                case eDragonInfoSubPopupType.STATUS:
                    if (!hasDragon)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("미획득드래곤알림"));
                        return;
                    }
                    break;
                case eDragonInfoSubPopupType.PASSIVE_SKILL:
                    if (!hasDragon)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("미획득드래곤알림"));
                        return;
                    }
                    SetVisibleArrowNode(false);
                    var dragon = User.Instance.DragonData.GetDragon(curDragonTag);
                    int minimumStep = CharTranscendenceData.GetNewSkillSlotMinimumStep((eDragonGrade)dragon.Grade());
                    if (dragon.IsTranscendenceAble() == false)
                    {
                        ToastManager.On(StringData.GetStringFormatByStrKey("스킬슬롯잠김", minimumStep));
                        return;
                    }
                    break;
                case eDragonInfoSubPopupType.TRANSCENDENCE:
                    if (!hasDragon)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("미획득드래곤알림"));
                        return;
                    }
                    var dragonDat = User.Instance.DragonData.GetDragon(curDragonTag);
                    int requireSkillLv = GameConfigTable.GetConfigIntValue("TRANSCENDENCE_MINIMUM_SKILL_LEVEL", 50);
                    if (dragonDat.SLevel < requireSkillLv)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("초월활성조건"));
                        return;
                    }

                    if (dragonDat.TranscendenceStep == CharTranscendenceData.GetStepMax((eDragonGrade)dragonDat.Grade()))
                    {
                        ToastManager.On(StringData.GetStringByStrKey("최대초월토스트"));
                        return;
                    }
                    SetVisibleArrowNode(false);
                    break;
                case eDragonInfoSubPopupType.COLLECTION:
                    //List<Collection> dragon_collection = new List<Collection>();
                    //var collections = CollectionAchievementManager.Instance.GetCompleteDataByType(eCollectionAchievementType.COLLECTION);

                    //foreach(Collection col in collections)
                    //{
                    //    if(col.CollectionBaseData.CollectionIDList.Contains(curDragonTag))
                    //    {
                    //        dragon_collection.Add(col);
                    //    }
                    //}

                    //if (dragon_collection.Count == 0)
                    //{
                    //    ToastManager.On(StringData.GetStringByStrKey("콜렉션없는드래곤"));
                    //    return;
                    //}
                    break;
            }

            SetShowSubPopup(_index, () =>
            {
                RefreshDragonStat();
                RefreshDragonDesc();

                switch (CurrentSubPopupType)
                {
                    case eDragonInfoSubPopupType.LEVELUP:
                        RefreshDragonPartSlot();
                        break;
                    case eDragonInfoSubPopupType.SKILLUP:
                        RefreshDragonSkillDesc();
                        break;
                }
            });
        }

        public void SetVisibleArrowNode(bool _isVisible)
        {
            var popup = PopupManager.GetPopup<DragonManagePopup>();
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
            foreach (var subPopup in subPopupList)
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
            foreach (var subPopup in subPopupList)
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
            switch ((eDragonInfoSubPopupType)_panelIndex)
            {
                case eDragonInfoSubPopupType.TRANSCENDENCE:
                case eDragonInfoSubPopupType.PASSIVE_SKILL:
                    SetVisibleArrowNode(true);
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
        public override bool backBtnCall()
        {
            var forceClose = PopupManager.GetPopup<DragonManagePopup>().ForceCloseFlag;

            if (Town.Instance == null || forceClose)
            {
                PopupManager.ClosePopup<DragonManagePopup>();
                PopupManager.GetPopup<DragonManagePopup>().ForceCloseFlag = false;

                return true;
            }

            if (backBtn != null)
            {
                var type = GetCurrentOpenPopup();
                if (type >= 0)
                {
                    SetSubPopupOpenFlag(type, false);
                    InitSpecificSubPopup(type);
                }
                else
                    backBtn.onClick.Invoke();

                return true;
            }
            return false;
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
            var curDragonTag = PopupManager.GetPopup<DragonManagePopup>().CurDragonTag;
            if (curDragonTag <= 0)
                return;

            var designData = CharBaseData.Get(curDragonTag);
            if (designData == null)
                return;

            var hasDragon = User.Instance.DragonData.IsUserDragon(curDragonTag);
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
                if (groupID < 0)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("상점이동유도문구"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"), () =>
                    {
                        PopupManager.ClosePopup<ConditionalBuyPopup>();
                        PopupManager.ClosePopup<SystemPopup>();
                        PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData());
                    },
                    () =>
                    {
                        PopupManager.ClosePopup<SystemPopup>();
                    },
                    () =>
                    {
                        PopupManager.ClosePopup<SystemPopup>();
                    });

                    return;
                }

                var popup = PopupManager.OpenPopup<ConditionalBuyPopup>(new ConditionBuyData(groupID));
                popup.SetRewardCallBack(() =>
                {
                    PopupManager.GetPopup<ShopPopup>().RefreshCurrentMenu();
                    popup.ClosePopup();
                });
                return;
            }

            if (designData.IS_LEVEL_PASS_REWARD)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("레벨패스드래곤획득문구"), () =>
                {
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
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("드래곤리스트미습득2"), () =>
                {

                    var isNoGuild = GuildManager.Instance.IsNoneGuild;
                    if (isNoGuild)
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

            if (designData.IS_LIMITED)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("미보유한정드래곤"));
                return;
            }

            if (designData.GRADE >= (int)eDragonGrade.Legend)
            {
                //뽑기등장 레전드 그룹
                var gacha_dragons = GachaRateData.GetGroup(10102);
                foreach (var rate in gacha_dragons)
                {
                    if (rate.result_id == curDragonTag)
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("미보유드래곤"),
                            () =>
                            {
                                PopupManager.AllClosePopup();//가챠 씬이동
                                SBFunc.MoveGachaScene();
                            },
                            () => { },
                            () => { }
                        );
                        return;
                    }
                }
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("미보유드래곤_합성"), () =>
                {
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
