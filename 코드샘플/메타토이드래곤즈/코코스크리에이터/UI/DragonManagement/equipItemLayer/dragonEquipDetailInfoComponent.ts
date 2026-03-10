
import { _decorator, Component, Node, Sprite, Label } from 'cc';
import { StringTable } from '../../../Data/StringTable';
import { TableManager } from '../../../Data/TableManager';
import { GameManager } from '../../../GameManager';
import { NetworkManager } from '../../../NetworkManager';
import { ResourceManager, ResourcesType } from '../../../ResourceManager';
import { DataManager } from '../../../Tools/DataManager';
import { ObjectCheck } from '../../../Tools/SandboxTools';
import { TutorialManager } from '../../../Tutorial/TutorialManager';
import { User } from '../../../User/User';
import { PopupManager } from '../../Common/PopupManager';
import { SubLayer } from '../../Common/SubLayer';
import { DragonChangePartPopup } from '../../DragonChangePartPopup';
import { DragonUnequipPopup } from '../../DragonUnequipPopup';
import { SystemPopup } from '../../SystemPopup';
import { ToastMessage } from '../../ToastMessage';
import { dragonEquipmentSlotComponent } from '../detailInfoLayer/dragonEquipmentSlotComponent';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = dragonEquipDetailInfoComponent
 * DateTime = Mon Mar 28 2022 15:30:59 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = dragonEquipDetailInfoComponent.ts
 * FileBasenameNoExtension = dragonEquipDetailInfoComponent
 * URL = db://assets/Scripts/UI/DragonManagement/equipItemLayer/dragonEquipDetailInfoComponent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 /**
  * 드래곤 파츠 슬롯 클릭시 보이는 파츠 정보 UI(장착, 해제, 강화 등을 표시하는 UI)
  */
@ccclass('dragonEquipDetailInfoComponent')
export class dragonEquipDetailInfoComponent extends Component {
    
    @property(Node)
    dragonEquipLayer : Node = null;//장비 레이어
    @property(Sprite)
    partIcon : Sprite;
    @property(Label)
    partNameLabel : Label;
    @property(Label)
    statTypeLabel : Label;
    @property(Label)
    statAmountLabel : Label;

    @property(Node)
    equipButtonNode : Node = null;
    @property(Node)
    unequipButtonNode : Node = null;

    @property(Node)
    partSlotEaterNode : Node = null;

    @property(Node)
    partSlotNode : Node = null;//화살표 애니메이션 실행용

    partTag : number = 0; //파츠 태그 정보
    tempdragonTag : number = 0;//클릭 할 때의 드래곤 태그
    isPartChangeState : boolean = false;//파츠를 갈아껴야되는 상황에서의 플래그
    stringTable : StringTable = null;

    initCurrentDragonData()
    {
        if(DataManager.GetData("DragonInfo") != null)//드래곤 태그값
        {
            let dragonTag = DataManager.GetData("DragonInfo") as number;
            let dragonData = User.Instance.DragonData;
            if(dragonData == null){
                console.log("user's dragon Data is null");
                return;         
            }

            let userDragonInfo = dragonData.GetDragon(dragonTag);
            if(userDragonInfo == null){
                console.log("user Dragon is null");
                return;
            }
            
            this.tempdragonTag = dragonTag;
        }
    }

    /**
     * @param param //파츠 태그가 param으로 들어옴
     */
    ShowDetailInfo(param : number)
    {
        if(this.stringTable == null){
            this.stringTable =  TableManager.GetTable<StringTable>(StringTable.Name);
        }

        if(param < 0){
            return;
        }

        if(this.isPartChangeState)//교체 요청 상태일 때는 교체 팝업 요청
        {
            let partLink = User.Instance.partData.GetPartLink(this.partTag);

            //현재 세팅된 드래곤 태그 값으로 파츠 슬롯 넘버 가져오기
            let partList = User.Instance.DragonData.GetDragon(this.tempdragonTag).GetPartsList();
            if(partList == null || partList.length <= 0){
                return;
            }

            let clickSlot = -1;
            partList.forEach((Element, index)=>{
                if(Element == null){
                    return;
                }
                let tag = Element.Tag;
                if(tag == param){
                    clickSlot = index;
                }
            });

            if(clickSlot < 0){
                return;
            }

            if(partLink <= 0)//다른 드래곤 착용상태 아님
            {
                this.showTradePartPopup(clickSlot);
            }
            else//코스트 팝업 출력
            {
                let params ={
                    did : this.tempdragonTag,//드래곤 id
                    tag : this.partTag,//부속품 tag
                    slotIndex : clickSlot,
                    isFullSlot : true,
                    isUnequipPopup : false,
                }
                PopupManager.OpenPopup("DragonUnequipPopup", true, params) as DragonUnequipPopup;
            }
        }
        else
        {
            if(this.node.activeInHierarchy == false)
            {
                this.node.active = true;
            }

            this.initCurrentDragonData();
            this.partTag = param;

            this.RefreshButtonUI(param);
            this.RefreshUI();

            // 튜토리얼 진행
            TutorialManager.GetInstance.OnTutorialEvent(114, 11);
        }
    }

    HideDetailInfo()
    {
        if(this.node.activeInHierarchy == true)
        {
            this.node.active = false;
        }

        this.onHidePartSlotEaterNode();//장비 슬롯 등록 버튼UI 도 끄기        
    }

    //장비 태그를 기준으로 링크 상태 판단, -1은 연결되지 않음
    RefreshButtonUI(partTag :number)
    {
        let partLink = User.Instance.partData.GetPartLink(partTag);
        let isPartLink = (partLink > 0);

        //귀속이 됨
        if(isPartLink)
        {
            let isSamebelonging = (this.tempdragonTag == partLink);//클릭한 파츠의 귀속 드래곤이 현재 같은지
            this.unequipButtonNode.active = isSamebelonging;
            this.equipButtonNode.active = !isSamebelonging;
        }
        else//귀속 안됨
        {
            this.unequipButtonNode.active = isPartLink;
            this.equipButtonNode.active = !isPartLink;
        }
    }

    RefreshUI()
    {
        if(this.partTag <= 0){
            return;
        }

        let partData = User.Instance.partData.GetPart(this.partTag);//파츠 원본 데이터

        if(partData == null){
            return;
        }
        let partItemDesignData = partData.GetItemDesignData();//파츠 아이템 테이블 데이터
        let partDesignData = partData.GetPartDesignData();//파츠 테이블 데이터

        if(partItemDesignData == null || partDesignData == null){
            console.log('designData is null');
            return;
        }

        let partIconStr = partData.Image;//파츠 아이콘 호출용 str
        let partName = partItemDesignData._NAME;//파츠(장비) 이름
        let partLevel = partData.Level;//강화 레벨

        let partMainoptionType_str = partDesignData.STAT_TYPE;//파츠 타입 string
        let partMainoption_Amount = partData.GetValue();

        //console.log(partIconStr);
        this.partIcon.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.PARTICON_SPRITEFRAME)[partIconStr]
        this.partNameLabel.string = (this.stringTable.Get(partName).TEXT +" +" +partLevel);
        this.statTypeLabel.string = this.GetStringByType(partMainoptionType_str);
        this.statAmountLabel.string = partMainoption_Amount.toString() + '(%)';

        //강화 단계별 리스트 데이터 갱신

        

    }
    GetStringByType(statTypeStr : string) : string
    {
        let tempStr = statTypeStr;
        if(this.stringTable == null)
        {
            return tempStr;
        }

        switch(statTypeStr)
        {
            case ('ATK' || 'atk'):
                tempStr = this.stringTable.Get(100000178).TEXT
                break;
            case ('DEF' || 'def'):
                tempStr = this.stringTable.Get(100000179).TEXT
                break;
            case ('HP' || 'hp'):
                tempStr = this.stringTable.Get(100000180).TEXT
                break;
            case ('CRI' || 'cri'):
                tempStr = this.stringTable.Get(100000181).TEXT
                break;
            default:
                break;
        }
        return tempStr;
    }

    /**
     * 1. 장착 비용 없음
     * 2. 해제 비용 있음 (장비 테이블 unequip_cost_num)
     * 3. 기존 장비에 또다른 장비를 씌울때 비용 없음(기존 장비 삭제 , 신규 장비 등록)
     * 4. 현재 드래곤에서(빈슬롯O) 다른 드래곤이 끼고 있는것 착용시 (다른 드래곤 장비 해제 비용 발생)
     * 5. 현재 드래곤에서 (풀슬롯) 다른 드래곤이 끼고 있는것 착용시 (2가지 중 뭐가 맞는지)
     * =>3번 조건 실행, 다른 드래곤 장비 해제 비용 발생)
     */
    onClickEquipButton()
    {
        let partLink = User.Instance.partData.GetPartLink(this.partTag);
        let isPartLink = (partLink > 0);
        
        let dragonInfo = User.Instance.DragonData.GetDragon(this.tempdragonTag);
        if(dragonInfo == null){
            console.log('dragonData is null');
            return;
        }
        let inputSlot = dragonInfo.GetEmptySlotIndex();

        if(isPartLink)//귀속이 됨
        {
            let isSamebelonging = (this.tempdragonTag == partLink);//클릭한 파츠의 귀속 드래곤 체크 - 같으면 여기로 아예 안옴
            if(!isSamebelonging)//교체 코스트 발생
            {
                if(inputSlot < 0)//장착 슬롯이 가득 차 있을 경우
                {
                    let slotOpenTotalCount = dragonInfo.GetCurrentSlotOpenCount();
                    this.fullSlotState(slotOpenTotalCount);
                }
                else//풀슬롯이 아니면 바로 팝업 띄우기
                {
                    let params ={
                        did : this.tempdragonTag,//드래곤 id
                        tag : this.partTag,//부속품 tag
                        slotIndex : inputSlot,
                        isFullSlot : false,
                        isUnequipPopup : false,
                    }
                    PopupManager.OpenPopup("DragonUnequipPopup", true, params) as DragonUnequipPopup;
                }
            }
        }
        else//귀속 안됨
        {
            if(inputSlot < 0)//장착 슬롯이 가득 차 있을 경우 - 교체 팝업 띄우기
            {
                let slotOpenTotalCount = dragonInfo.GetCurrentSlotOpenCount();
                this.fullSlotState(slotOpenTotalCount);
            }
            else
            {
                this.remainSlotState(inputSlot);
            }
        }
    }
    remainSlotState(inputSlot : number)
    {
        let params = {
            did : this.tempdragonTag,//드래곤 id
            tag : this.partTag,//부속품 tag
            slot : inputSlot,//장착 슬롯 인덱스
        }
        console.log("equip adequate Slot index : " + inputSlot);
        
        //장착 msg -> item_update 및 dragon_exp_update 날라옴
        NetworkManager.Send("dragon/equippart", params, (jsonData) => {
            if (ObjectCheck(jsonData, "rs") && jsonData.rs >= 0) {
                
                if(this.dragonEquipLayer != null){

                    let equipLayer = this.dragonEquipLayer.getComponent(SubLayer);
                    if(equipLayer != null)
                    {
                        equipLayer.ForceUpdate();
                    }
                }
            }
            else {
                let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
                popup.setMessage(StringTable.GetString(100000621, "장착 실패"), StringTable.GetString(100000614, "ERROR"));
            }
        })
    }

    //장착 슬롯이 가득 찼을 경우 유저에게 슬롯 갈아 끼울거냐고 제공
    fullSlotState(totalSlotCount : number)
    {
        if(this.partSlotNode == null){
            return;
        }
        //가득 찼을 때의 최대 슬롯 갯수 가져오기 - 화살표 전체 출력
        let equipSlotComponent = this.partSlotNode.getComponent(dragonEquipmentSlotComponent);
        if(equipSlotComponent == null){
            return;
        }

        this.onShowPartSlotEaterNode();//파츠 교체시 표시 UI
        equipSlotComponent.showAnimationSlot(totalSlotCount);
    }

    showTradePartPopup(slotIndex : number)
    {
        let popup = PopupManager.OpenPopup("SystemPopupOKCANCEL") as SystemPopup;
        popup.setMessage(StringTable.GetString(100000248, "장착"), StringTable.GetString(100000345, "선택한 장비를 착용 하시겠습니까? \n기존의 장착된 장비는 삭제됩니다."));

        popup.setCallback(()=>
        {
            let dragonInfo = User.Instance.DragonData.GetDragon(this.tempdragonTag);
            if(dragonInfo == null){
                let errorpopup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
                errorpopup.setMessage(StringTable.GetString(100000621, "장착 실패"), StringTable.GetString(100000627, "잘못된 드래곤 데이터"));
                popup.ClosePopup();
                return;
            }

            let params = {
                did : this.tempdragonTag,//드래곤 id
                tag : this.partTag,//부속품 tag
                slot : slotIndex,//장착 슬롯 인덱스
            }
            console.log("equip adequate Slot index : " + slotIndex);
            
            //장착 msg -> item_update 및 dragon_exp_update 날라옴
            NetworkManager.Send("dragon/equippart", params, (jsonData) => {
                if (ObjectCheck(jsonData, "rs") && jsonData.rs >= 0) {
                    let destroyTag = jsonData.destroyTag;//장비 삭제 태그
                    if(destroyTag > 0){
                        User.Instance.partData.DeleteUserPart(destroyTag);
                    }
                    if(this.dragonEquipLayer != null){

                        let equipLayer = this.dragonEquipLayer.getComponent(SubLayer);
                        if(equipLayer != null)
                        {
                            this.onHidePartSlotEaterNode();
                            equipLayer.ForceUpdate();
                        }
                    }
                }
                else {
                    let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
                    popup.setMessage(StringTable.GetString(100000621, "장착 실패"), StringTable.GetString(100000614, "에러"));
                }
            })

            popup.ClosePopup();
        },
        () =>
        {
            popup.ClosePopup();
        });
    }

    //해제 버튼
    onClickUnEquipButton()
    {
        let params = { 
            partTag : this.partTag, 
            dragonTag : this.tempdragonTag,
            isUnequipPopup : true,
        }

        PopupManager.OpenPopup("DragonUnequipPopup", true, params) as DragonUnequipPopup;
    }

    //파츠 교체시 표시 UI
    onShowPartSlotEaterNode()
    {
        if(this.partSlotEaterNode != null && this.partSlotEaterNode.activeInHierarchy == false){
            this.partSlotEaterNode.active = true;
            this.isPartChangeState = true;//교체 UI 켜짐 상태 체크
        }
    }

    onHidePartSlotEaterNode()
    {
        let equipSlotComponent = this.partSlotNode.getComponent(dragonEquipmentSlotComponent);
        if(equipSlotComponent == null){
            return;
        }
        equipSlotComponent.hideAllAnimationSlot();

        if(this.partSlotEaterNode != null && this.partSlotEaterNode.activeInHierarchy == true){
            this.partSlotEaterNode.active = false;
            this.isPartChangeState = false;
        }
    }

    /**
     * //업데이트 기대해달라는 문구
     */
    onClickExpectGameAlphaUpdate()
    {
        let emptyCheck = ToastMessage.isToastEmpty();
        if(emptyCheck == false){
            return;
        }
         
        let textData = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000326);
        if(textData != null)
        {
            ToastMessage.Set(textData.TEXT, null, -52);
        }
    }

    
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.4/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.4/manual/en/scripting/decorator.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.4/manual/en/scripting/life-cycle-callbacks.html
 */
