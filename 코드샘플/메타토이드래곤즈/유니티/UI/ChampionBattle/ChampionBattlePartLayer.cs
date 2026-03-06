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
    public enum ChampionBattlePartListViewType
    {
        NONE = 0,
        DEFAULT = 1,    //평소리스트(장착 드래곤 포함) - 표시 초록 라운드
        INFO = 2,       //평소리스트(장착 드래곤 포함) - 표시 초록 라운드
        REINFORCE = 4,  //평소리스트(장착 드래곤 포함) - 표시 초록 라운드
        DECOMPOSE = 8, //장착한 상태에서는 리스트 제외 - 체크 표시
        COMPOUND = 16,   //장착한 상태에서는 리스트 제외 - 체크 표시
    }


    class ChampionBattlePartInfo : ITableData
    {
        public ChampionPart userPart = null;
        public SubLayer layer = null;

        public void Init() { }
        public string GetKey() { return ""; }
    }

    public class ChampionBattlePartLayer : SubLayer, EventListener<DragonPartEvent>
    {
        [SerializeField]
        ChampionBattlePartListPanel dragonPartListPanel = null;

        [SerializeField]
        ChampionBattleDragonSelectDescPanel dragonCharacter = null;
    
        [SerializeField]
        ChampionBattlePartIconPanel dragonEquipSlot = null;

        [SerializeField]
        ChampionBattleDragonPartTotalStatPanel dragonStat = null;

        [SerializeField]
        ChampionBattleDragonPartInfoPanel dragonPartInfo = null;

        [SerializeField]
        ChampionBattleDragonPartReinforcePanel dragonPartReinforce = null;

        [SerializeField]
        DragonPartCompoundPanel dragonPartCompound = null;

        [SerializeField]
        DragonPartDecomposePanel dragonPartDecompose = null;

        [SerializeField]
        ChampionBattleDragonPartSetOptionPanel dragonSetInfo = null;
        
        [Header("LeftNodeList")]
        [SerializeField]
        List<GameObject> leftNodeObejctList = new List<GameObject>();

        [SerializeField]
        Button backBtn = null;
        
        bool isLeftNodeVisibleOn = false;

        int partTag = -1;
        public int PartTag { get { return partTag; } set { partTag = value; } }

        private ChampionBattlePartListViewType currentViewType = ChampionBattlePartListViewType.DEFAULT;
        //무조건 DragonPartListPanel의 DragonPartEvent.DragonPartEventEnum.SetListType 로만 set함
        public ChampionBattlePartListViewType CurrentViewType { set { currentViewType = value; } }
        public void OnEnable()
        {
            EventManager.AddListener(this);
            currentViewType = ChampionBattlePartListViewType.DEFAULT;
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
                    var partID = eventType.partTag;
                    
                    var isRight = eventType.isRight;
                    var buttonInfo = eventType.obj?.GetComponent<DragonPartIconSlot>();
                    if (dragonPartInfo != null && partID > 0)
                    {
                        if (buttonInfo != null)
                        {
                            buttonInfo.ShowClickNode();
                        }
                        else
                        {
                            buttonInfo = null;
                        }

                        partTag = partID;

                        isLeftNodeVisibleOn = isRight;//정보창이 오른쪽에 보여지면 켜야되고, false 면 꺼야함

                        var dragonTag = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().DragonTag;
                        
                        var CurDragonData = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
                        if (CurDragonData == null)
                        {
                            Debug.Log("user Dragon is null");
                            return;
                        }

                        if (CurDragonData != null)
                        {
                            if (eventType.equipIndex < 0)
                            {
                                var equipIndex = CurDragonData.ChampionPart.Count;
                                for (int i = 0; i < 6; i++)
                                {
                                    if (CurDragonData.GetPart(i) == null)
                                    {
                                        equipIndex = i;
                                        break;
                                    }
                                }

                                if (equipIndex >= CharExpData.GetSlotCountByDragonLevel((int)eDragonGrade.Legend, GameConfigTable.GetDragonLevelMax()))
                                {
                                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("젬블럭공간부족"));
                                    return;
                                }

                                eventType.equipIndex = equipIndex;
                            }

                            SetVisibleLeftNodeList(isLeftNodeVisibleOn);

                            dragonPartInfo.ShowDetailInfo(partID, eventType.equipIndex, isRight, () =>
                            {
                                if (buttonInfo != null)
                                    buttonInfo.HideClickNode();
                            });
                        }
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
                        var eaterComp = node.GetComponent<ChampionBattleDragonPartChangeEaterPanel>();
                        if (eaterComp != null)
                        {
                            eaterComp.ShowTradePopup(partTag);
                        }
                    }
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.ShowReinforcePanel://강화 패널 오픈 요청
                {

                }
                break;
                case DragonPartEvent.DragonPartEventEnum.HideReinforcePanel://강화 패널 닫으면 -> 정보 창 떠야함. (강화 실패시 처리 따로해야함)
                {

                }
                break;
                case DragonPartEvent.DragonPartEventEnum.ReinforcePartSlotSelect://강화 패널에서 슬롯 클릭 시 
                {

                }
                break;
                case DragonPartEvent.DragonPartEventEnum.SuccessReinforce://강화 성공
                {

                }
                break;
                case DragonPartEvent.DragonPartEventEnum.DeleteReinforce://강화 실패해서 터짐 -> 장비 정보 화면으로 이동 시켜달라고함.
                {

                }
                break;
                case DragonPartEvent.DragonPartEventEnum.ShowCompoundPanel://합성 패널 오픈 요청 - 정보창 열려있으면 닫기
                {

                }
                break;
                case DragonPartEvent.DragonPartEventEnum.HideCompoundPanel://합성 패널 닫기 -> 전부 닫기 -> 일반 모드로 변경
                {

                }
                break;
                case DragonPartEvent.DragonPartEventEnum.CompoundAutoSet://자동 합성 요청
                {
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.ShowDecomposePanel://분해 패널 오픈
                {
                    
                }
                break;
                case DragonPartEvent.DragonPartEventEnum.HideDecomposePanel://분해 패널 닫기 -> 전부 닫기 -> 일반 모드로 변경
                {

                }
                break;
                case DragonPartEvent.DragonPartEventEnum.DecomposeAllSelect://분해 패널 전체 선택
                {

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
                DragonPartEvent.SetListType((int)ChampionBattlePartListViewType.DEFAULT);
                dragonPartListPanel.Init(this);
            }    
        }

        public void OnClickCloseDetailInfo()
        {
            dragonPartInfo.HideDetailInfo();
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
                //else if (dragonPartInfo.IsOpen)   // 23.07.31 - 드래곤 장비 세부정보 패널 상태에서 우상단 X버튼 누를 시 드래곤 인포로 돌아가도록 처리
                //    dragonPartInfo.HideDetailInfo();
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
    }
}
