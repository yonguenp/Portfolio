
import { _decorator, Component, Label, Sprite, Node, Event } from 'cc';
import { ItemBaseTable } from '../../Data/ItemTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { DataManager } from '../../Tools/DataManager';
import { ObjectCheck } from '../../Tools/SandboxTools';
import { User, UserPart } from '../../User/User';
import { PopupManager } from '../Common/PopupManager';
import { SubLayer } from '../Common/SubLayer';
import { SystemPopup } from '../SystemPopup';
import { dragonEquipButtonSlotComponent } from './detailInfoLayer/dragonEquipButtonSlotComponent';
const { ccclass, property } = _decorator;
/**
 * Predefined variables
 * Name = InventoryEquipDetailComponent
 * DateTime = Thu Mar 10 2022 14:09:13 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = InventoryEquipDetailComponent.ts
 * FileBasenameNoExtension = InventoryEquipDetailComponent
 * URL = db://assets/Scripts/UI/DragonManagement/InventoryEquipDetailComponent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('InventoryEquipDetailComponent')
export class InventoryEquipDetailComponent extends Component {

    private currentClickPartTag : number;

    @property(Node)
    itemNode : Node = null;
    @property(Node)
    buttonBundleNode : Node = null;
    @property(Node)
    noItemLabelNode : Node = null;

    @property(Label)
    itemName : Label;
    @property(Label)
    itemDesc : Label;
    @property(Label)
    itemAmount : Label;
    @property(Sprite)
    itemRes : Sprite;
    @property(Node)
    enchantNode : Node = null;
    @property(Node)
    enchantDescNode : Node = null;

    @property(Node)
    dragonEquipLayer : Node = null;


    tempdragonTag : number = 0;

    init()
    {
        this.initCurrentDragonData();
        this.onClickHideEnchantButton();
        this.onClickHideEnchantDesc();

        let items = User.Instance.partData.GetAllUserParts();
        let isEmpty = items.length <= 0;
        this.initDetailItem(isEmpty);
        if(!isEmpty)
        {
            this.SetCurrentClickItem(items[0].Tag as number);
        }
    }

    forceUpdate()
    {
        //this.init()
        this.refreshDetailItem();
    }

    SetCurrentClickItem(partTag : number)
    {
        //console.log("catch itemID : "+partTag);
        
        this.currentClickPartTag = partTag;
        this.refreshDetailItem();
    }

    initDetailItem(isEmpty : boolean)
    {
        this.noItemLabelNode.active = isEmpty;
        this.itemNode.active = !isEmpty;
        this.buttonBundleNode.active = !isEmpty;
    }

    refreshDetailItem()
    {
        this.onClickHideEnchantDesc();

        if(this.currentClickPartTag <= 0)
        {
            console.log("partTag is not under 0");
            return;
        }

        let partData = User.Instance.partData.GetPart(this.currentClickPartTag) as UserPart;
        if(partData == null)
        {
            console.log("partData is null");
            return;
        }

        let partDesignData = partData.GetItemDesignData();
        if(partDesignData == null)
        {
            console.log("partDesignData is null");
            return;
        }
        
        this.itemName.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(partDesignData._NAME).TEXT
        this.itemRes.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.PARTICON_SPRITEFRAME)[partDesignData.ICON]
        this.itemAmount.string = `+${partData.Level}`
        this.itemDesc.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(partDesignData._DESC).TEXT
    }
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

    //강화 버튼 클릭시
    onClickShowEnchantButton()
    {
        if(this.enchantNode != null && this.enchantNode.activeInHierarchy == false){
            this.enchantNode.active = true;
        }
    }

    onClickHideEnchantButton()
    {
        if(this.enchantNode != null && this.enchantNode.activeInHierarchy == true){
            this.enchantNode.active = false;
        }
    }

    onClickSellButton()
    {
        this.onClickHideEnchantButton();//열려있다면 끄기
        this.onClickHideEnchantDesc();

        let popup = PopupManager.OpenPopup("SystemPopupOKCANCEL") as SystemPopup;
        popup.setMessage(StringTable.GetString(100000232, "판매"), StringTable.GetString(100000625, "장비를 판매할까요?"));
        popup.setCallback(()=>
        {
            let params = {
            // 아직은 없음
            }
            // 서버로 부터 가챠 결과 리턴
            // NetworkManager.Send("gacha/dragon", params, (jsonObj) => {
            //     if (ObjectCheck(jsonObj, "result") && jsonObj.result > 0) {
            //         console.log("check gacah -->" + jsonObj.result);
            //         let dragonID = jsonObj.result;

            //         let info = User.Instance.DragonData.GetDragon(dragonID);

            //         let gachaResult: CapsuleInfo = new CapsuleInfo();
            //         gachaResult.objectID = info.Tag as number;
            //         gachaResult.capsuleGrade = info.Grade;
            //         gachaResult.objectName = info.Name;
            
            //         this.capsuleInfoList.push(gachaResult);
            
            //         this.gachaEffectController.Init();
            //         this.gachaEffectController.node.active = true;

            //         // 가챠 결과 페이지 데이터 초기화
            //         this.gachaResultController.Init(this.capsuleInfoList);

            //         // 페이지 갱신
            //         this.RefreshDataByTabState();
            //     }
            //     else {
            //     }
            // })

        popup.ClosePopup();
        },
        () =>
        {
            popup.ClosePopup();
        });

    }

    onClickEquipButton()
    {
        this.onClickHideEnchantButton();//열려있다면 끄기
        this.onClickHideEnchantDesc();

        let popup = PopupManager.OpenPopup("SystemPopupOKCANCEL") as SystemPopup;
        popup.setMessage(StringTable.GetString(100000183, "장착"), StringTable.GetString(100000626, "장비를 장착할까요?"));
        popup.setCallback(()=>
        {
            let dragonInfo = User.Instance.DragonData.GetDragon(this.tempdragonTag);
            if(dragonInfo == null){
                let errorpopup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
                popup.setMessage(StringTable.GetString(100000621, "장착 실패"), StringTable.GetString(100000627, "잘못된 드래곤 데이터"));
                popup.ClosePopup();
                return;
            }

            let inputSlot = dragonInfo.GetEmptySlotIndex();
            if(inputSlot < 0)//장착 슬롯이 가득 차 있을 경우 - 교체 팝업 띄우기
            {

            }

            let params = {
                did : this.tempdragonTag,//드래곤 id
                tag : this.currentClickPartTag,//부속품 tag
                slot : inputSlot,//장착 슬롯 인덱스
            }
            console.log("equip adequate Slot index : " + inputSlot);
            
            //장착 msg -> item_update 및 dragon_exp_update 날라옴
            NetworkManager.Send("dragon/equippart", params, (jsonData) => {
                if (ObjectCheck(jsonData, "rs") && jsonData.rs > 0) {
                    
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

            popup.ClosePopup();
        },
        () =>
        {
            popup.ClosePopup();
        });
    }
    
    //장비 관리 탭에서 슬롯 클릭시 -> 상세 정보 갱신데이터 넘기기
    onClickSlotDetailData(event : Event , customEventData)
    {
        if(event != null)
        {
            let currentButtonComponent = event.currentTarget as Node;
            if(currentButtonComponent == null)
            {
                return;
            }
    
            let buttonInfo = currentButtonComponent.getComponent(dragonEquipButtonSlotComponent);
            console.log(buttonInfo);
            if(buttonInfo == null){
                return;
            }

            let buttonTag = buttonInfo.SlotPartTag;
            if(buttonTag > 0){
                this.SetCurrentClickItem(buttonTag);
            }
        }
    }

    onClickShowEnchantDesc()
    {
        if(this.enchantDescNode != null && !this.enchantDescNode.activeInHierarchy)
        {
            this.enchantDescNode.active = true;
        }
    }

    onClickHideEnchantDesc()
    {
        if(this.enchantDescNode != null && this.enchantDescNode.activeInHierarchy)
        {
            this.enchantDescNode.active = false;
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
