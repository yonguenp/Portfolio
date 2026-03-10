
import { _decorator, Component, Node, Label } from 'cc';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { NetworkManager } from '../NetworkManager';
import { ObjectCheck } from '../Tools/SandboxTools';
import { User } from '../User/User';
import { Popup } from './Common/Popup';
import { PopupManager } from './Common/PopupManager';
import { SystemPopup } from './SystemPopup';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DragonChangePartPopup
 * DateTime = Thu Mar 31 2022 10:54:39 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = DragonChangePartPopup.ts
 * FileBasenameNoExtension = DragonChangePartPopup
 * URL = db://assets/Scripts/UI/DragonChangePartPopup.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
/**
 * 이미 장착된 상태의 드래곤에서 현재 드래곤의 파츠로 등록할 때 띄움
 */
@ccclass('DragonChangePartPopup')
export class DragonChangePartPopup extends Popup {
    @property(Label)
    costLabel : Label = null;

    @property(Label)
    bodyLabel : Label = null;

    currentPartTag :number = -1;
    currentDragonTag : number = 0;
    currentSelectSlot : number = -1;
    currentFullSlotCheck : boolean = false;

    currentNeedCost : number = 0;
    Init(data?: any)
    {
        let tempTag =  data.tag;
        let tempDragonTag = data.did;
        let tempSlot = data.slotIndex;
        let isFullSlot = Boolean(data.isFullSlot);
        
        if(tempTag < 0 || tempDragonTag < 0 || tempSlot < 0){
            return;
        }

        this.currentPartTag = tempTag;
        this.currentDragonTag = tempDragonTag;
        this.currentSelectSlot = tempSlot;
        this.currentFullSlotCheck = isFullSlot;
        this.SetDetailData(this.currentPartTag);

        super.Init(data);
    }

    SetDetailData(partTag : number)//금액 갱신 & 본문 데이터 수정
    {
        let partData = User.Instance.partData.GetPart(partTag);
        if(partData == null){
            return;
        }

        let partDesignData = partData.GetPartDesignData();
        if(partDesignData == null){
            return;
        }
        let unEquipCostNum = partDesignData.UNEQUIP_COST_NUM;
        this.currentNeedCost = partDesignData.UNEQUIP_COST_NUM as number;

        if(this.costLabel != null){
            this.costLabel.string = unEquipCostNum.toString();
        }

        if(this.bodyLabel != null)
        {
            if(this.currentFullSlotCheck)
            {
                this.bodyLabel.string = TableManager.GetTable(StringTable.Name).Get(100000364).TEXT;
            }
            else{
                this.bodyLabel.string = TableManager.GetTable(StringTable.Name).Get(100000363).TEXT;
            }
        }
    }

    //장착 해제 눌렀을 때 서버 연동 - 현재 열린 레이어 강제 업데이트
    onClickChangeEquipButton()
    {
        let currentUserGold = User.Instance.GOLD;
        if(currentUserGold < this.currentNeedCost)
        {
            let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
            popup.setMessage(StringTable.GetString(100000248), StringTable.GetString(100000620));
            this.ClosePopup();
            return;
        }

        let params = {
            did : this.currentDragonTag,//드래곤 id
            tag : this.currentPartTag,//부속품 tag
            slot : this.currentSelectSlot,//장착 슬롯 인덱스
        }
        //이전 드래곤 귀속 상태 끊어야함
        NetworkManager.Send("dragon/equippart", params, (jsonData) => {
            if (ObjectCheck(jsonData, "rs") && jsonData.rs >= 0) {
                let destroyTag = jsonData.destroyTag;//장비 삭제 태그
                if(destroyTag > 0){
                    User.Instance.partData.DeleteUserPart(destroyTag);
                }
                
                let currentPopup = PopupManager.GetPopupByName("DragonManagePopup");
                currentPopup.ForceUpdate();
            }
            else {
                let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
                popup.setMessage(StringTable.GetString(100000248), StringTable.GetString(100000621));
            }

            this.ClosePopup();
        })
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
