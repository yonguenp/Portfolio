using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//드래곤 장비 착용 및 해제 기능 레이어
namespace SandboxNetwork
{
    [Flags]
    public enum PartListViewType
    {
        NONE = 0,
        DEFAULT = 1,    //평소리스트(장착 드래곤 포함) - 표시 초록 라운드
        INFO = 2,       //평소리스트(장착 드래곤 포함) - 표시 초록 라운드
        REINFORCE = 4,  //평소리스트(장착 드래곤 포함) - 표시 초록 라운드
        DECOMPOSE = 8, //장착한 상태에서는 리스트 제외 - 체크 표시
        COMPOUND = 16,   //장착한 상태에서는 리스트 제외 - 체크 표시
    }


    class PartInfo : ITableData
    {
        public UserPart userPart = null;
        public SubLayer layer = null;

        public void Init() { }
        public string GetKey() { return ""; }
    }

    public class DragonPartLayer : SubLayer, EventListener<DragonPartEvent>
    {
        [SerializeField]
        DragonPartListPanel dragonPartListPanel = null;

        [SerializeField]
        DragonDescPanel dragonCharacter = null;
    
        [SerializeField]
        DragonPartIconPanel dragonEquipSlot = null;

        [SerializeField]
        DragonPartTotalStatPanel dragonStat = null;

        [SerializeField]
        DragonPartInfoPanel dragonPartInfo = null;

        [SerializeField]
        DragonPartReinforcePanel dragonPartReinforce = null;

        [SerializeField]
        DragonPartCompoundPanel dragonPartCompound = null;

        [SerializeField]
        DragonPartDecomposePanel dragonPartDecompose = null;

        [SerializeField]
        PartFusionPanel partFusion;

        [SerializeField]
        DragonPartSetOptionPanel dragonSetInfo = null;
        
        [Header("LeftNodeList")]
        [SerializeField]
        List<GameObject> leftNodeObejctList = new List<GameObject>();

        [SerializeField]
        Button backBtn = null;
        
        bool isLeftNodeVisibleOn = false;

        int partTag = -1;
        public int PartTag { get { return partTag; } set { partTag = value; } }

        private PartListViewType currentViewType = PartListViewType.DEFAULT;
        //무조건 DragonPartListPanel의 DragonPartEvent.DragonPartEventEnum.SetListType 로만 set함
        public PartListViewType CurrentViewType { set { currentViewType = value; } }
        public void OnEnable()
        {
            EventManager.AddListener(this);
            currentViewType = PartListViewType.DEFAULT;
        }
        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }

        public void OnEvent(DragonPartEvent eventType) 
        {
            switch (eventType.Event)
            {
                case DragonPartEvent.DragonPartEventEnum.RefreshList:
                {
                    if (dragonPartListPanel)
                        dragonPartListPanel.RefreshList(false);
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.RefreshLayer:
                {
                    ForceUpdate();
                } break;
                case DragonPartEvent.DragonPartEventEnum.RefreshInfoPanel://강화 , 분해 , 합성 UI 쪽에서 현재 UI 업데이트 해야될 시
                {
                    var param = eventType.partTag;
                    if (currentViewType == PartListViewType.REINFORCE)//현재 강화 리스트 타입(강화 UI 열린 상태)
                    {
                        partTag = param;
                        PopupManager.GetPopup<DragonManagePopup>().CurPartTag = partTag;
                        DragonPartEvent.ShowReinforcePanel();
                        return;
                    }
                    
                    var isRight = eventType.isRight;
                    var buttonInfo = eventType.obj?.GetComponent<DragonPartIconSlot>();
                    if (dragonPartInfo != null && param > 0)
                    {
                        if (buttonInfo != null)
                        {
                            buttonInfo.ShowClickNode();
                        }
                        else
                        {
                            buttonInfo = null;
                        }

                        partTag = param;

                        isLeftNodeVisibleOn = isRight;//정보창이 오른쪽에 보여지면 켜야되고, false 면 꺼야함

                        SetVisibleLeftNodeList(isLeftNodeVisibleOn);

                        dragonPartInfo.ShowDetailInfo(param, isRight, () => {
                            if (buttonInfo != null)
                                buttonInfo.HideClickNode();
                        });
                    }
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.HideInfoPanel://상세 정보보기 끌 때 클릭한 노드 끄기
                {
                    dragonPartListPanel.InitPartslotClickNode();

                    if (!isLeftNodeVisibleOn)//장비 정보가 왼쪽에서 꺼야하는 상태면 끌 때 left node 켜야함
                        SetVisibleLeftNodeList(true);
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.ShowEaterNode://eater 노드 컴포넌트 여기서 켜게함(순환구조 막는 용)
                {
                    var node = eventType.obj;
                    var partTag = eventType.partTag;
                    if (node != null)
                    {
                        var eaterComp = node.GetComponent<DragonPartChangeEaterPanel>();
                        if (eaterComp != null)
                        {
                            eaterComp.ShowTradePopup(partTag);
                        }
                    }
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.ShowReinforcePanel://강화 패널 오픈 요청
                {
                    DragonPartEvent.SetListType((int)PartListViewType.REINFORCE);

                    if (dragonPartInfo != null)
                        dragonPartInfo.HideDetailInfo();
                    
                    if (dragonEquipSlot != null)
                        dragonEquipSlot.TryShowClickNode(PartTag);

                    if (dragonPartReinforce != null)
                        dragonPartReinforce.ShowReinforcePanel();

                    if (dragonPartListPanel != null)
                        dragonPartListPanel.DrawTableView(false);
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.HideReinforcePanel://강화 패널 닫으면 -> 정보 창 떠야함. (강화 실패시 처리 따로해야함)
                {
                    DragonPartEvent.SetListType((int)PartListViewType.DEFAULT);

                    var buttonInfo = dragonEquipSlot.GetPartSlot(partTag);
                    bool isRight = buttonInfo != null;

                    DragonPartEvent.RefreshInfoPanel(partTag, isRight, buttonInfo);//장비 정보 오픈 이벤트

                    if (dragonPartReinforce != null)
                        dragonPartReinforce.HideReinforcePanel();
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.ReinforcePartSlotSelect://강화 패널에서 슬롯 클릭 시 
                {
                    if (dragonPartReinforce != null)
                    {
                        dragonPartReinforce.OnClickPartFrame(eventType.partTag);
                        partTag = eventType.partTag;
                    }
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.SuccessReinforce://강화 성공
                {
                    if (dragonPartListPanel != null)
                    {
                        dragonPartListPanel.SuccessReinforcePartData(eventType.partTag);
                        partTag = eventType.partTag;
                    }

                    InitPanel();

                    if (dragonEquipSlot != null)
                        dragonEquipSlot.TryShowClickNode(PartTag);
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.DeleteReinforce://강화 실패해서 터짐 -> 장비 정보 화면으로 이동 시켜달라고함.
                {
                    if (dragonPartListPanel != null)
                        dragonPartListPanel.DeletePartDataInTableData(eventType.partTag);

                    if (dragonPartReinforce != null)
                        dragonPartReinforce.HideReinforcePanel();//장비 강화창 끄기

                    DragonPartEvent.SetListType((int)PartListViewType.DEFAULT);//리스트 기본 타입

                    InitPartTag();
                    InitPanel();

                    DragonPartEvent.RefreshList();//장비 리스트 갱신
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.ShowCompoundPanel://합성 패널 오픈 요청 - 정보창 열려있으면 닫기
                {
                    DragonPartEvent.SetListType((int)PartListViewType.COMPOUND);

                    InitPartTag();

                    if (dragonPartCompound != null)
                        dragonPartCompound.ShowCompoundPanel();

                    if (dragonPartInfo != null)
                        dragonPartInfo.HideDetailInfo();//정보창 닫기

                    SetVisibleLeftNodeList(false);

                    DragonPartEvent.RefreshList();//합성 리스트 갱신 요청
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.HideCompoundPanel://합성 패널 닫기 -> 전부 닫기 -> 일반 모드로 변경
                {
                    DragonPartEvent.SetListType((int)PartListViewType.DEFAULT);

                    if (dragonPartCompound != null)
                        dragonPartCompound.HideCompoundPanel();

                    SetVisibleLeftNodeList(true);

                    DragonPartEvent.RefreshList();//일반 리스트 갱신 요청
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.CompoundAutoSet://자동 합성 요청
                {
                    if (dragonPartListPanel != null)
                        dragonPartListPanel.AutoSetPart();
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.ShowDecomposePanel://분해 패널 오픈
                {
                    DragonPartEvent.SetListType((int)PartListViewType.DECOMPOSE);

                    InitPartTag();

                    if (dragonPartInfo != null)
                        dragonPartInfo.HideDetailInfo();//정보창 닫기

                    SetVisibleLeftNodeList(false);

                    DragonPartEvent.RefreshList();//합성 리스트 갱신 요청

                    if (dragonPartDecompose != null)
                        dragonPartDecompose.ShowDecomposePanel();
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.HideDecomposePanel://분해 패널 닫기 -> 전부 닫기 -> 일반 모드로 변경
                {
                    DragonPartEvent.SetListType((int)PartListViewType.DEFAULT);

                    if (dragonPartDecompose != null)
                        dragonPartDecompose.HideDecomposePanel();

                    SetVisibleLeftNodeList(true);

                    DragonPartEvent.RefreshList();//일반 리스트 갱신 요청
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.DecomposeAllSelect://분해 패널 전체 선택
                {
                    if (dragonPartListPanel)
                        dragonPartListPanel.DecomposeAllSelect();
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.PlayEquipPartAnim://장비 장착 연출
                {
                    if (dragonEquipSlot != null)
                        dragonEquipSlot.PartEquipAnim(eventType.partTag);
                }
                break;
            }
        }

        public override void Init()
        {
            InitPartTag();
            InitPartListPanel();
            InitPanel();
        }

        void InitPanel()
        {
            if (dragonCharacter != null)
            {
                dragonCharacter.Init();
            }
            if (dragonEquipSlot != null)
            {
                dragonEquipSlot.Init();
                dragonEquipSlot.InitEquipAnimation();
            }
            if (dragonStat != null)
            {
                dragonStat.CustomInit();
            }
            if (dragonPartInfo != null)
            {
                dragonPartInfo.HideDetailInfo();
            }
            if (dragonSetInfo != null)
            {
                dragonSetInfo.Init();
            }

            partFusion.Hide();
        }

        public override void ForceUpdate()
        {
            Init();
        }

        void InitPartTag()
        {
            partTag = -1;
        }

        void InitPartListPanel()
        {
            if(dragonPartListPanel != null)
            {
                DragonPartEvent.SetListType((int)PartListViewType.DEFAULT);
                dragonPartListPanel.Init(this, dragonPartCompound, dragonPartDecompose);
            }    
        }

        public void OnClickCloseDetailInfo()
        {
            dragonPartInfo.HideDetailInfo();
        }

        public void OnClickDecompose()//일괄 분해 버튼
        {
            DragonPartEvent.ShowDecomposePanel();
        }

        public void OnClickCompound()//합성 버튼
        {
            PopupManager.GetPopup<DragonManagePopup>().CurPartTag = 0;

            DragonPartEvent.ShowCompoundPanel();
        }

        public void OnClickCloseReinforce()
        {
            DragonPartEvent.HideReinforcePanel();
        }

        public void OnClickCloseCompound()
        {
            DragonPartEvent.HideCompoundPanel();
        }

        public void OnClickCloseDecompose()
        {
            DragonPartEvent.HideDecomposePanel();
        }

        /**
         * //업데이트 기대해달라는 문구
         */
        public void OnClickExpectGameAlphaUpdate()
        {
            ToastManager.On(100000326);
        }

        public override bool backBtnCall()
        {
            if (backBtn != null)
            {
                if (dragonPartReinforce.IsOpen)
                    DragonPartEvent.HideReinforcePanel();
                else if (dragonPartCompound.IsOpen)
                    DragonPartEvent.HideCompoundPanel();
                else if (dragonPartDecompose.IsOpen)
                    DragonPartEvent.HideDecomposePanel();
                //else if (dragonPartInfo.IsOpen)   // 23.07.31 - 드래곤 장비 세부정보 패널 상태에서 우상단 X버튼 누를 시 드래곤 인포로 돌아가도록 처리
                //    dragonPartInfo.HideDetailInfo();
                else if (partFusion.IsOpen)
                    DoneFusion();
                else
                    backBtn.onClick.Invoke();
                return true;
            }
            return false;
        }

        void SetVisibleLeftNodeList(bool _setVisible)
        {
            foreach(var go in leftNodeObejctList)
            {
                if (go == null)
                    continue;
                go.SetActive(_setVisible);
            }
        }

        public void OnFusion()
        {
            if (dragonPartInfo != null)
                dragonPartInfo.HideDetailInfo();

            SetVisibleLeftNodeList(false);
            partFusion.Show();
        }

        public void DoneFusion()
        {
            Init();
        }
    }
}
