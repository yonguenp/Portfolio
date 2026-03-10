
import { _decorator, Component, Node, Label } from 'cc';
import { CharBaseTable, CharExpTable } from '../../../Data/CharTable';
import { StringTable } from '../../../Data/StringTable';
import { TableManager } from '../../../Data/TableManager';
import { DataManager } from '../../../Tools/DataManager';
import { StringBuilder } from '../../../Tools/SandboxTools';
import { User } from '../../../User/User';
import { ToastMessage } from '../../ToastMessage';
import { DragonReserveLayer } from '../DragonReserveLayer';
import { dragonEquipButtonSlotComponent } from './dragonEquipButtonSlotComponent';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = dragonEquipmentSlotComponent
 * DateTime = Mon Mar 21 2022 15:42:34 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = dragonEquipmentSlotComponent.ts
 * FileBasenameNoExtension = dragonEquipmentSlotComponent
 * URL = db://assets/Scripts/UI/DragonManagement/detailInfoLayer/dragonEquipmentSlotComponent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 //드래곤 현재 레벨에 따른 슬롯 열림 / 닫힌 상태 파악 및 해당 레벨에 따른 해금 노티
@ccclass('dragonEquipmentSlotComponent')
export class dragonEquipmentSlotComponent extends Component {
    
    charDataTable : CharBaseTable = null;
    charExpTable : CharExpTable = null;
    stringTable : StringTable = null;

    currentDragonLevel : number = 0;
    currentDragonTag : number = 0;

    @property(DragonReserveLayer)
    taplayer : DragonReserveLayer = null;

    @property(Node)
    slotList : Node[] = []

    @property(Node)
    dragonPartNode : Node = null;

    @property(Label)
    titleLabel : Label = null;

    init()
    {
        if(this.charDataTable == null){
            this.charDataTable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name);
        }
        if(this.charExpTable == null){
            this.charExpTable = TableManager.GetTable<CharExpTable>(CharExpTable.Name);
        }
        if(this.stringTable == null){
            this.stringTable = TableManager.GetTable<StringTable>(StringTable.Name);
        }

        this.refreshCurrentDragonData();
    }
    refreshCurrentDragonData()
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
            
            this.RefreshDragonEquipSlot(dragonTag,userDragonInfo.Level);
            this.RefreshTitleLabel(dragonTag);
        }
    }

    RefreshDragonEquipSlot(dragonTag : number, currentLevel : number)
    {
        let dragonInfo = User.Instance.DragonData.GetDragon(dragonTag);
        if(dragonInfo == null){
            return;
        }

        let dragonPartInfo = dragonInfo.GetPartsList();//고정 길이 6으로 옴. 내부에서 세팅
        
        this.currentDragonLevel = currentLevel;
        this.currentDragonTag = dragonTag;
        let currentOpenSlotCount = User.Instance.DragonData.GetDragon(this.currentDragonTag).GetCurrentSlotOpenCount();//현재 오픈된 슬롯 개수
        if(this.slotList == null || this.slotList.length <=0){
            return;
        }

        for(let i = 0 ; i< this.slotList.length ; i++){
            let buttonNode = this.slotList[i];
            if(buttonNode == null){
                continue;
            }
            let buttonComp = buttonNode.getComponent(dragonEquipButtonSlotComponent);
            if(buttonComp == null){
                continue;
            }

            let buttonIndex = i + 1;
            let partID = -1;
            let partTag = -1;

            if(dragonPartInfo[i] != null)
            {
                partID = dragonPartInfo[i].ID as number;
                partTag = dragonPartInfo[i].Tag as number;
            }
            buttonComp.refreshSlot(buttonIndex <= currentOpenSlotCount, partTag, partID);
        }
    }

    RefreshTitleLabel(dragonTag : number)
    {
        if(this.titleLabel != null)
        {
            let dragonInfo = User.Instance.DragonData.GetDragon(dragonTag);
            if(dragonInfo == null){
                return;
            }
            let dragonDesc = dragonInfo.Name;
            this.titleLabel.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000339).TEXT , dragonDesc );
        }
    }

    showAnimationSlot(openSlotCount : number)
    {
        if(this.slotList == null || this.slotList.length <=0){
            return;
        }

        for(let i = 0 ; i< this.slotList.length ; i++){
            let buttonNode = this.slotList[i];
            if(buttonNode == null){
                continue;
            }
            let buttonComp = buttonNode.getComponent(dragonEquipButtonSlotComponent);
            if(buttonComp == null){
                continue;
            }

            if(i < openSlotCount){
                buttonComp.ShowAnimArrowNode();
            }
            else{
                buttonComp.HideAnimArrowNode();
            }
        }
    }

    hideAllAnimationSlot()
    {
        let dragonInfo = User.Instance.DragonData.GetDragon(this.currentDragonTag);
        if(dragonInfo == null){
            return;
        }
        let openSlotCount = dragonInfo.GetCurrentSlotOpenCount();

        if(this.slotList == null || this.slotList.length <=0){
            return;
        }

        for(let i = 0 ; i< this.slotList.length ; i++){
            let buttonNode = this.slotList[i];
            if(buttonNode == null){
                continue;
            }
            let buttonComp = buttonNode.getComponent(dragonEquipButtonSlotComponent);
            if(buttonComp == null){
                continue;
            }

            if(i < openSlotCount){
                buttonComp.HideAnimArrowNode();
            }
        }
    }

    OnClickSlot(event, customEventData)
    {
        let checker = JSON.parse(customEventData);
        let clickSlotIndex = Number(checker.slotindex);//현재 클릭한 슬롯 가져옴

        let miniMizeLevel = this.charExpTable.GetUnLockLevelBySlotIndex(clickSlotIndex);
        
        let slotUnlockChecker = miniMizeLevel <= this.currentDragonLevel;
        if(slotUnlockChecker){
            this.MoveEquipPage();
        }
        else{
            let stringData = this.stringTable.Get(100000328);
            if(stringData == null){
                return;
            }

            let toastStr = StringBuilder(stringData.TEXT, miniMizeLevel);
            let emptyCheck = ToastMessage.isToastEmpty();
            if(emptyCheck == false){
                return;
            }
            ToastMessage.Set(toastStr, null, -52);
        }
    }

    onClickSlotDetailData(event , customEventData)
    {
        if(event != null)
        {
            let currentButtonComponent = event.currentTarget as Node;
            if(currentButtonComponent == null)
            {
                return;
            }
    
            let buttonInfo = currentButtonComponent.getComponent(dragonEquipButtonSlotComponent);
            if(buttonInfo == null){
                return;
            }

            buttonInfo.onClickSlotDetailData(this.dragonPartNode);
        }
    }

    MoveEquipPage()
    {
        if(this.taplayer != null)
        {
            this.taplayer.moveLayer({"index" : 2});
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
