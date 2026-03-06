using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionBattleDragonTabTypePopupData : TabTypePopupData
    {
        public ChampionBattleDragonTabTypePopupData(int tab, int subtab = -1)
            : base(tab, subtab)
        {
            
        }
    }


    public class ChampionBattleDragonSelectPopup : Popup<ChampionBattleDragonTabTypePopupData>
    {
        [SerializeField]
        Text titlePopupLabel = null;
        [SerializeField]
        public int maxCnt = 0;
        [SerializeField]
        ChampionBattleDragonSelectTabController tabController = null;

        public int DragonTag { get { return Dragon != null ? Dragon.Tag : 0; } }
        public int PetTag { get; private set; } = 0;

        public UserDragonData UserDragonData { get; private set; } = new UserDragonData();

        public List<int> SelectedDragonList { get; private set; } = new List<int>();

        public int PartTag { get; private set; } = 0;

        public ChampionDragon Dragon { get; private set; } = null;
        
        UserPetData petData = null;

        public UserPetData PetData
        {
            get
            {
                if (petData == null)
                {
                    petData = new UserPetData();
                    foreach (var petBase in ChampionManager.GetSelectablePets())
                    {
                        petData.AddPet(new ChampionPet(petBase));
                    }
                }

                return petData;
            }
        }

        public void Clear()
        {
            Dragon = null;
            PetTag = 0;
            PartTag = 0;
        }

        public void SetPetTag(int tag)
        {
            PetTag = tag;
        }

        #region OpenPopup
        public static ChampionBattleDragonSelectPopup OpenPopup(int tab, int subTab = -1)
        {
            return OpenPopup(new ChampionBattleDragonTabTypePopupData(tab, subTab));
        }

        public static ChampionBattleDragonSelectPopup OpenDragonSetting(ChampionDragon data, UserDragonData dragons)
        {
            var popup = ChampionBattleDragonSelectPopup.OpenPopup(0, 3);
            popup.InitDragonSetting(data, dragons);

            return popup;
        }
        public static ChampionBattleDragonSelectPopup OpenPopup(ChampionBattleDragonTabTypePopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<ChampionBattleDragonSelectPopup>(data);
        }
        #endregion
        public void InitDragonSetting(ChampionDragon data, UserDragonData dragons)
        {
            SetDragonData(data);

            SetExitCallback(() => {
                if (data != null)
                {
                    switch (data)
                    {
                        case PracticeDragon practiceDragon:
                        {
                            PracticeDragonChangedEvent.Refresh();
                        }
                        break;
                        default:
                            DragonChangedEvent.Refresh();
                            break;
                    }
                }
                PopupManager.GetPopup<ChampionBattleDragonSelectPopup>()?.ClearSelectedDragonList();
                PopupManager.GetPopup<ChampionBattleDragonSelectPopup>()?.ClearDragonInfoList();
            });

            ClearDragonInfoList();

            foreach (ChampionDragon dragon in dragons.GetAllUserDragons())
            {
                UserDragonData.AddUserDragon(dragon.Tag, dragon);
            }
            
            ForceUpdate();
        }

        public void SetDragonData(ChampionDragon data)
        {
            Dragon = data;
        }

        public void SetPartTag(int tag)
        {
            PartTag = tag;
        }
        public bool isFull()
        {
            if(SelectedDragonList != null && maxCnt > 0)
            {
                if (SelectedDragonList.Count == maxCnt)
                    return true;
            }
            return false;
        }

        public void onClickCloseBtn()
        {
            ClosePopup();
        }
        public override void ForceUpdate(ChampionBattleDragonTabTypePopupData data)
        {
            base.DataRefresh(data);
            tabController.RefreshTab();
        }

        public override void InitUI()
        {
            if (tabController == null)
            {
                return;
            }

            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);

            InitTabController();
        }

        void InitTabController()
        {
            int tabIndex;
            int subIndex = 0;
            if (Data == null)
            {
                tabIndex = 0;
            }
            else
            {
                tabIndex = Data.TabIndex;
            }
            if (tabIndex < 0)
            {
                tabIndex = 0;
            }
            if (Data.SubIndex != -1)
                subIndex = Data.SubIndex;

            tabController.InitTab(tabIndex, new TabTypePopupData(tabIndex, subIndex));
        }

        public void ClearDragonInfoList()
        {
            if (UserDragonData == null)
                UserDragonData = new UserDragonData();
            UserDragonData.ClearData();
        }
        
        public void ClearSelectedDragonList()
        {
            if (SelectedDragonList == null)
                SelectedDragonList = new List<int>();
            SelectedDragonList.Clear();
            
        }

        public void moveTab(ChampionBattleDragonTabTypePopupData data)
        {
            if (data == null)
            {
                return;
            }

            int tabIndex = data.TabIndex;
            int subIndex = 0;

            if (data.SubIndex != -1)
                subIndex = data.SubIndex;

            if (tabIndex >= 0)
            {
                tabController.ChangeTab(tabIndex, new TabTypePopupData(tabIndex, subIndex));
            }
        }
        public void onClickExpectGameAlphaUpdate()
        {
            ToastManager.On(100000326);
        }

        public override void OnClickDimd()
        {
            foreach (var subLayerObj in tabController.GetCurrentTabLayer().GetSubLayerList())
            {
                if (subLayerObj.activeSelf == true)
                {
                    var subLayer = subLayerObj.GetComponent<SubLayer>();
                    if (subLayer.backBtnCall() == false) // 백 버튼 콜백이 있을 경우
                    {
                        ClosePopup();
                    }
                    break;
                }
            }
        }
        public override void BackButton()
        {
            foreach (var subLayerObj in tabController.GetCurrentTabLayer().GetSubLayerList())
            {
                if (subLayerObj.activeSelf == true)
                {
                    var subLayer = subLayerObj.GetComponent<SubLayer>();
                    if (subLayer.backBtnCall() == false) // 백 버튼 콜백이 있을 경우
                    {
                        ClosePopup();
                    }
                    break;
                }
            }
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
        }

        public void SetRandomFull()//테스트용
        {
            var dragons = ChampionManager.GetSelectableDragons();
            while(!isFull())
            {
                var ranDr = dragons[SBFunc.Random(0, dragons.Count)].KEY;
                if (!SelectedDragonList.Contains(ranDr))
                {
                    SelectedDragonList.Add(ranDr);
                }
            }
        }
    }
}

