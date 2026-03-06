using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonTabTypePopupData : TabTypePopupData
    {
        public DragonTabTypePopupData(int tab, int subtab = -1)
            : base(tab, subtab)
        {
            
        }
    }

    public struct DragonChangedEvent
    {
        private static DragonChangedEvent e = default;

        public enum TYPE {             
            REFRESH,
            MOVE_START,
            MOVE_DONE,
        }
        public TYPE type;
        public int tag;
        public Vector3 targetSlotPos;
        
        public static void Refresh()
        {
            e.type = TYPE.REFRESH;
            e.tag = -1;
            e.targetSlotPos = Vector3.zero;
            EventManager.TriggerEvent(e);
        }

        public static void MoveStart(int tag)
        {
            e.type = TYPE.MOVE_START;
            e.tag = tag;
            e.targetSlotPos = Vector3.zero;
            EventManager.TriggerEvent(e);
        }

        public static void MoveDone(int tag, Vector3 pos)
        {
            e.type = TYPE.MOVE_DONE;
            e.tag = tag;
            e.targetSlotPos = pos;
            EventManager.TriggerEvent(e);
        }
    }

    public class DragonManagePopup : Popup<DragonTabTypePopupData>
    {
        [SerializeField]
        Text titlePopupLabel = null;

        [SerializeField]
        DragonManageTabController tabController = null;

        //드래곤 인포 관련 저장하는곳
        public int CurDragonTag = 0;
        public List<int> DragonInfoList { get; private set; } = new List<int>();

        //드래곤 장비 관련 저장값
        public int CurPartTag = 0;

        //드래곤 펫정보 관련 저장
        public int CurPetTag = 0;

        bool collectionOpenFlag = false;
        public bool ForceCloseFlag = false;


        #region OpenPopup
        public static DragonManagePopup OpenPopup(int tab, int subTab = -1)
        {
            return OpenPopup(new DragonTabTypePopupData(tab, subTab));
        }
        public static DragonManagePopup OpenPopup(DragonTabTypePopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<DragonManagePopup>(data);
        }
        #endregion


        public void onClickCloseBtn()
        {
            ClosePopup();
        }
        public override void ForceUpdate(DragonTabTypePopupData data)
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

            if(!collectionOpenFlag)
                UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);

            collectionOpenFlag = false;
            InitTabController();

            SetSubCamTextureOn();
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

        void RefreshTitleLabel(int labelIndex)
        {
            if (labelIndex <= 0)
            {
                return;
            }

            if (titlePopupLabel != null)
            {
                titlePopupLabel.text = StringData.GetStringByIndex(labelIndex);
            }
        }
        public void ClearDragonInfoList()
        {
            if (DragonInfoList == null)
                DragonInfoList = new List<int>();
            DragonInfoList.Clear();
        }

        public void moveTab(DragonTabTypePopupData data)
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
            SetSubCamTextureOff();
            collectionOpenFlag = false;

            //드래곤 합성 & 장비 강화 / 획득 업적 -> 꺼질 때 알림 갱신
            NotificationManager.Instance.CheckCollectionAchievementNotification();
        }

        public void OpenCollectionForceClose()
        {
            collectionOpenFlag = true;
            base.ClosePopup();
            SetSubCamTextureOff();
        }

        public override bool IsModeless()
        {
            return false;
        }
    }
}

